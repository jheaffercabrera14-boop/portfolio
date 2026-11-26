using Npgsql;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Repositories;

/// <summary>
/// All SQL for portfolio.project_image.
/// Methods that take (conn, tx) run INSIDE a transaction owned by ProjectRepository.
/// Methods without them open their own connection (used by the single-image Editor screens).
/// </summary>
public class ProjectImageRepository
{
    private readonly NpgsqlDataSource _db;
    public ProjectImageRepository(NpgsqlDataSource db) => _db = db;

    private static ProjectImage Map(NpgsqlDataReader r) => new()
    {
        ImageId = r.GetInt32(0), ProjectId = r.GetInt32(1), ImageUrl = r.GetString(2), SortOrder = r.GetInt32(3)
    };

    public async Task<List<ProjectImage>> GetByProjectAsync(int projectId)
    {
        await using var cmd = _db.CreateCommand(@"
            SELECT image_id, project_id, image_url, sort_order
            FROM portfolio.project_image WHERE project_id = @pid ORDER BY sort_order, image_id");
        cmd.AddInt("pid", projectId);
        await using var r = await cmd.ExecuteReaderAsync();
        var list = new List<ProjectImage>();
        while (await r.ReadAsync()) list.Add(Map(r));
        return list;
    }

    public async Task<ProjectImage?> GetByIdAsync(int imageId)
    {
        await using var cmd = _db.CreateCommand(@"
            SELECT image_id, project_id, image_url, sort_order
            FROM portfolio.project_image WHERE image_id = @id");
        cmd.AddInt("id", imageId);
        await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? Map(r) : null;
    }

    // ---- used INSIDE a transaction (connection + transaction come from ProjectRepository) ----

    public async Task InsertAsync(NpgsqlConnection conn, NpgsqlTransaction tx, int projectId, string url, int sortOrder)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO portfolio.project_image (project_id, image_url, sort_order)
            VALUES (@pid, @url, @ord)", conn, tx);
        cmd.AddInt("pid", projectId);
        cmd.AddText("url", url);
        cmd.AddInt("ord", sortOrder);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteByProjectAsync(NpgsqlConnection conn, NpgsqlTransaction tx, int projectId)
    {
        await using var cmd = new NpgsqlCommand("DELETE FROM portfolio.project_image WHERE project_id = @pid", conn, tx);
        cmd.AddInt("pid", projectId);
        await cmd.ExecuteNonQueryAsync();
    }

    // ---- single-image Editor screens (own connection) ----

    /// <summary>Adds one image at the end of the project's list. May throw 23505 / 23514 / 23503.</summary>
    public async Task<int> AddAsync(int projectId, string url)
    {
        await using var cmd = _db.CreateCommand(@"
            INSERT INTO portfolio.project_image (project_id, image_url, sort_order)
            VALUES (@pid, @url, (SELECT COALESCE(MAX(sort_order), 0) + 1 FROM portfolio.project_image WHERE project_id = @pid))
            RETURNING image_id");
        cmd.AddInt("pid", projectId);
        cmd.AddText("url", url);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<bool> UpdateUrlAsync(int imageId, string url)
    {
        await using var cmd = _db.CreateCommand("UPDATE portfolio.project_image SET image_url = @url WHERE image_id = @id");
        cmd.AddInt("id", imageId);
        cmd.AddText("url", url);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int imageId)
    {
        await using var cmd = _db.CreateCommand("DELETE FROM portfolio.project_image WHERE image_id = @id");
        cmd.AddInt("id", imageId);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
