using Microsoft.AspNetCore.Mvc;
using Portfolio.Repositories;
using Portfolio.ViewModels;

namespace Portfolio.Controllers;

/// <summary>VIEWER side (read-only, public). No SQL here: the repositories do the work.</summary>
public class HomeController : Controller
{
    private readonly CourseRepository _courses;
    private readonly ProjectRepository _projects;

    public HomeController(CourseRepository courses, ProjectRepository projects)
    {
        _courses = courses;
        _projects = projects;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel
        {
            Courses = await _courses.GetAllAsync(),
            Projects = await _projects.GetAllAsync()
        };
        return View(vm);
    }
}
