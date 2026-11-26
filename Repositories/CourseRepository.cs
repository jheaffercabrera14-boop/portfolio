using Npgsql;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Repositories;

/// <summary>All SQL for the portfolio.course table. Controllers never see SQL.</summary>
public class CourseRepository
{
    private readonly NpgsqlDataSource _db;
    public CourseRepository(NpgsqlDataSource db) => _db = db;

    private const string SelectSql = @"
        SELECT c.course_id, c.course_code, c.course_title, c.description, c.units, c.academic_term, c.image_url,
               (SELECT COUNT(*) FROM portfolio.project p WHERE p.course_id = c.course_id)::int AS project_count
        FROM portfolio.course c";

    private static Course Map(NpgsqlDataReader r) => new()
    {
        CourseId = r.GetInt32(0),
        CourseCode = r.GetString(1),
        CourseTitle = r.GetString(2),
        Description = r.GetStringOrNull(3),
        Units = r.GetInt32(4),
        AcademicTerm = r.GetStringOrNull(5),
        ImageUrl = r.GetStringOrNull(6),
        ProjectCount = r.GetInt32(7)
    };

    public async Task<List<Course>> GetAllAsync()
    {
        await using var cmd = _db.CreateCommand(SelectSql + " ORDER BY c.course_code");
        await using var r = await cmd.ExecuteReaderAsync();
        var list = new List<Course>();
        while (await r.ReadAsync()) list.Add(Map(r));
        return list;
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        await using var cmd = _db.CreateCommand(SelectSql + " WHERE c.course_id = @id");
        cmd.AddInt("id", id);
        await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? Map(r) : null;
    }

    /// <summary>INSERT. May throw PostgresException 23505 (duplicate code) or 23514 (check).</summary>
    public async Task<int> InsertAsync(Course c)
    {
        await using var cmd = _db.CreateCommand(@"
            INSERT INTO portfolio.course (course_code, course_title, description, units, academic_term, image_url)
            VALUES (@code, @title, @desc, @units, @term, @img)
            RETURNING course_id");
        cmd.AddText("code", c.CourseCode);
        cmd.AddText("title", c.CourseTitle);
        cmd.AddText("desc", c.Description);
        cmd.AddInt("units", c.Units);
        cmd.AddText("term", c.AcademicTerm);
        cmd.AddText("img", c.ImageUrl);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    /// <summary>UPDATE. Returns false when the id does not exist (the controller then shows a 404).</summary>
    public async Task<bool> UpdateAsync(Course c)
    {
        await using var cmd = _db.CreateCommand(@"
            UPDATE portfolio.course
               SET course_code = @code, course_title = @title, description = @desc,
                   units = @units, academic_term = @term, image_url = @img
             WHERE course_id = @id");
        cmd.AddInt("id", c.CourseId);
        cmd.AddText("code", c.CourseCode);
        cmd.AddText("title", c.CourseTitle);
        cmd.AddText("desc", c.Description);
        cmd.AddInt("units", c.Units);
        cmd.AddText("term", c.AcademicTerm);
        cmd.AddText("img", c.ImageUrl);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    /// <summary>DELETE. Throws PostgresException 23503 if the course still has projects (ON DELETE RESTRICT).</summary>
    public async Task<bool> DeleteAsync(int id)
    {
        await using var cmd = _db.CreateCommand("DELETE FROM portfolio.course WHERE course_id = @id");
        cmd.AddInt("id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
