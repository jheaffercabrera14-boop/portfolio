using Microsoft.AspNetCore.Mvc;
using Portfolio.Repositories;
using Portfolio.ViewModels;

namespace Portfolio.Controllers;

/// <summary>VIEWER: one project with its images, links and embedded YouTube video.</summary>
public class ProjectsController : Controller
{
    private readonly ProjectRepository _projects;
    private readonly ProjectImageRepository _images;

    public ProjectsController(ProjectRepository projects, ProjectImageRepository images)
    {
        _projects = projects;
        _images = images;
    }

    public async Task<IActionResult> Details(int id)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project == null) return NotFound();

        return View(new ProjectDetailsViewModel
        {
            Project = project,
            Images = await _images.GetByProjectAsync(id)
        });
    }
}
