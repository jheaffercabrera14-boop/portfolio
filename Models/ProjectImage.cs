namespace Portfolio.Models;

/// <summary>One row of portfolio.project_image.</summary>
public class ProjectImage
{
    public int ImageId { get; set; }
    public int ProjectId { get; set; }
    public string ImageUrl { get; set; } = "";
    public int SortOrder { get; set; }
}
