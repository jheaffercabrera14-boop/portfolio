namespace Portfolio.Models;

/// <summary>One row of portfolio.project (+ joined course info and image summary for display).</summary>
public class Project
{
    public int ProjectId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? TechStack { get; set; }
    public int CompletedYear { get; set; }
    public string? ProjectUrl { get; set; }
    public string? VideoUrl { get; set; }

    // Filled by the JOIN / sub-selects in ProjectRepository (not columns of project)
    public string CourseCode { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public string? CoverImageUrl { get; set; }
    public int ImageCount { get; set; }
}
