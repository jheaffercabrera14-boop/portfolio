using Microsoft.AspNetCore.Mvc;
using Portfolio.Repositories;
using Portfolio.ViewModels;

namespace Portfolio.Controllers;

/// <summary>EDITOR dashboard: lists every course and project with Add / Edit / Delete links.</summary>
[Route("Editor")]
public class EditorController : EditorBaseController
{
    private readonly CourseRepository _courses;
    private readonly ProjectRepository _projects;

    public EditorController(CourseRepository courses, ProjectRepository projects)
    {
        _courses = courses;
        _projects = projects;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index() =>
        View(new EditorDashboardViewModel
        {
            Courses = await _courses.GetAllAsync(),
            Projects = await _projects.GetAllAsync()
        });
}
