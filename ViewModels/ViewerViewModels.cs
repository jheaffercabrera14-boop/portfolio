using Portfolio.Models;

namespace Portfolio.ViewModels;

public class HomeViewModel
{
    public List<Course> Courses { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
}

public class CourseDetailsViewModel
{
    public Course Course { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
}

public class ProjectDetailsViewModel
{
    public Project Project { get; set; } = new();
    public List<ProjectImage> Images { get; set; } = new();
}

public class EditorDashboardViewModel
{
    public List<Course> Courses { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
}

public class LoginViewModel
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ReturnUrl { get; set; }
}
