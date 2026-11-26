using Npgsql;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Repositories;

/// <summary>
/// All SQL for portfolio.project, including the TRANSACTIONS that save a project together with its images.
/// </summary>
public class ProjectRepository
{
    private readonly NpgsqlDataSource _db;
    private readonly ProjectImageRepository _images;

    public ProjectRepository(NpgsqlDataSource db, ProjectImageRepository images)
    {
        _db = db;
        _images = images;
    }

    private const string SelectSql = @"
        SELECT p.project_id, p.course_id, p.title, p.description, p.tech_stack, p.completed_year, p.project_url, p.video_url,
               c.course_code, c.course_title,
               (SELECT i.image_url FROM portfolio.project_image i
                 WHERE i.project_id = p.project_id ORDER BY i.sort_order, i.image_id LIMIT 1) AS cover_image_url,
               (SELECT COUNT(*) FROM portfolio.project_image i WHERE i.project_id = p.project_id)::int AS image_count
        FROM portfolio.project p
        JOIN portfolio.course c ON c.course_id = p.course_id";

    private const string OrderSql = " ORDER BY c.course_code, p.completed_year DESC, p.title";

    private static Project Map(NpgsqlDataReader r) => new()
    {
        ProjectId = r.GetInt32(0),
        CourseId = r.GetInt32(1),
        Title = r.GetString(2),
        Description = r.GetStringOrNull(3),
        TechStack = r.GetStringOrNull(4),
        CompletedYear = r.GetInt32(5),
        ProjectUrl = r.GetStringOrNull(6),
        VideoUrl = r.GetStringOrNull(7),
        CourseCode = r.GetString(8),
        CourseTitle = r.GetString(9),
        CoverImageUrl = r.GetStringOrNull(10),
        ImageCount = r.GetInt32(11)
    };

    public async Task<List<Project>> GetAllAsync()
    {
        await using var cmd = _db.CreateCommand(SelectSql + OrderSql);
        return await ReadListAsync(cmd);
    }

    public async Task<List<Project>> GetByCourseAsync(int courseId)
    {
        await using var cmd = _db.CreateCommand(SelectSql + " WHERE p.course_id = @cid" + OrderSql);
        cmd.AddInt("cid", courseId);
        return await ReadListAsync(cmd);
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        await using var cmd = _db.CreateCommand(SelectSql + " WHERE p.project_id = @id");
        cmd.AddInt("id", id);
        await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? Map(r) : null;
    }

    private static async Task<List<Project>> ReadListAsync(NpgsqlCommand cmd)
    {
        await using var r = await cmd.ExecuteReaderAsync();
        var list = new List<Project>();
        while (await r.ReadAsync()) list.Add(Map(r));
        return list;
    }

    /// <summary>
    /// ONE TRANSACTION: insert the project, then insert every image.
    /// If ANY step fails (duplicate URL = 23505, bad URL = 23514, ...) the whole transaction is rolled back,
    /// so neither the project nor any image is saved. Same pattern as BorrowAsync (Activity 11).
    /// </summary>
    public async Task<int> CreateWithImagesAsync(Project p, IReadOnlyList<(int Line, string Url)> images)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();          // BEGIN
        try
        {
            int projectId;
            await using (var cmd = new NpgsqlCommand(@"
                INSERT INTO portfolio.project (course_id, title, description, tech_stack, completed_year, project_url, video_url)
                VALUES (@cid, @title, @desc, @tech, @year, @purl, @vurl)
                RETURNING project_id", conn, tx))
            {
                AddProjectParams(cmd, p);
                projectId = (int)(await cmd.ExecuteScalarAsync())!;
            }

            await InsertImagesAsync(conn, tx, projectId, images);
            await tx.CommitAsync();                                       // COMMIT
            return projectId;
        }
        catch
        {
            await tx.RollbackAsync();                                     // ROLLBACK: nothing is saved
            throw;
        }
    }

    /// <summary>
    /// ONE TRANSACTION: update the project, delete its old images, insert the new list.
    /// Any failure rolls everything back, so the OLD images stay exactly as they were.
    /// Returns false (and rolls back) when the project id does not exist.
    /// </summary>
    public async Task<bool> UpdateWithImagesAsync(Project p, IReadOnlyList<(int Line, string Url)> images)
    {
        await using var conn = await _db.OpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();          // BEGIN
        try
        {
            int rows;
            await using (var cmd = new NpgsqlCommand(@"
                UPDATE portfolio.project
                   SET course_id = @cid, title = @title, description = @desc, tech_stack = @tech,
                       completed_year = @year, project_url = @purl, video_url = @vurl
                 WHERE project_id = @id", conn, tx))
            {
                AddProjectParams(cmd, p);
                cmd.AddInt("id", p.ProjectId);
                rows = await cmd.ExecuteNonQueryAsync();
            }
            if (rows == 0)
            {
                await tx.RollbackAsync();
                return false;
            }

            await _images.DeleteByProjectAsync(conn, tx, p.ProjectId);
            await InsertImagesAsync(conn, tx, p.ProjectId, images);
            await tx.CommitAsync();                                       // COMMIT
            return true;
        }
        catch
        {
            await tx.RollbackAsync();                                     // ROLLBACK
            throw;
        }
    }

    private async Task InsertImagesAsync(NpgsqlConnection conn, NpgsqlTransaction tx, int projectId,
                                         IReadOnlyList<(int Line, string Url)> images)
    {
        int order = 1;
        foreach (var (line, url) in images)
        {
            try { await _images.InsertAsync(conn, tx, projectId, url, order++); }
            catch (PostgresException ex) { throw new ImageSaveException(line, url, ex); }
        }
    }

    private static void AddProjectParams(NpgsqlCommand cmd, Project p)
    {
        cmd.AddInt("cid", p.CourseId);
        cmd.AddText("title", p.Title);
        cmd.AddText("desc", p.Description);
        cmd.AddText("tech", p.TechStack);
        cmd.AddInt("year", p.CompletedYear);
        cmd.AddText("purl", p.ProjectUrl);
        cmd.AddText("vurl", p.VideoUrl);
    }

    /// <summary>DELETE the project; its images go with it (ON DELETE CASCADE on fk_project_image_project).</summary>
    public async Task<bool> DeleteAsync(int id)
    {
        await using var cmd = _db.CreateCommand("DELETE FROM portfolio.project WHERE project_id = @id");
        cmd.AddInt("id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
