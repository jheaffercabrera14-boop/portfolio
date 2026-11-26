namespace Portfolio.Models;

/// <summary>One row of portfolio.course. ProjectCount is calculated by the query, not stored.</summary>
public class Course
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public string? Description { get; set; }
    public int Units { get; set; }
    public string? AcademicTerm { get; set; }
    public string? ImageUrl { get; set; }
    public int ProjectCount { get; set; }
}
