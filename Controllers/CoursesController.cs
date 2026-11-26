using Microsoft.AspNetCore.Mvc;
using Portfolio.Repositories;
using Portfolio.ViewModels;

namespace Portfolio.Controllers;

/// <summary>VIEWER: one course and its projects.</summary>
public class CoursesController : Controller
{
    private readonly CourseRepository _courses;
    private readonly ProjectRepository _projects;

    public CoursesController(CourseRepository courses, ProjectRepository projects)
    {
        _courses = courses;
        _projects = projects;
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course == null) return NotFound();               // -> friendly 404 page

        return View(new CourseDetailsViewModel
        {
            Course = course,
            Projects = await _projects.GetByCourseAsync(id)
        });
    }
}
