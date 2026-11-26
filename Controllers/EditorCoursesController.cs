using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Portfolio.Data;
using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.Validation;
using Portfolio.ViewModels;

namespace Portfolio.Controllers;

/// <summary>EDITOR: add, edit and delete courses.</summary>
[Route("Editor/Courses")]
public class EditorCoursesController : EditorBaseController
{
    private readonly CourseRepository _courses;
    public EditorCoursesController(CourseRepository courses) => _courses = courses;

    [HttpGet("Create")]
    public IActionResult Create() => View("Form", new CourseFormViewModel());

    [HttpPost("Create")]
    public async Task<IActionResult> Create(CourseFormViewModel vm)
    {
        FormRules.CheckCourse(vm, ModelState);                  // regex rules (server side)
        if (!ModelState.IsValid) return View("Form", vm);

        try
        {
            await _courses.InsertAsync(ToCourse(vm));
            TempData["Success"] = $"Course {vm.CourseCode} added.";
            return RedirectToAction("Index", "Editor");
        }
        catch (PostgresException ex)                            // 23505 duplicate code, 23514 check...
        {
            ModelState.AddModelError(string.Empty, PgErrors.ToMessage(ex));
            return View("Form", vm);
        }
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var c = await _courses.GetByIdAsync(id);
        if (c == null) return NotFound();
        return View("Form", new CourseFormViewModel
        {
            CourseId = c.CourseId, CourseCode = c.CourseCode, CourseTitle = c.CourseTitle,
            Description = c.Description, Units = c.Units, AcademicTerm = c.AcademicTerm, ImageUrl = c.ImageUrl
        });
    }

    [HttpPost("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CourseFormViewModel vm)
    {
        vm.CourseId = id;
        FormRules.CheckCourse(vm, ModelState);
        if (!ModelState.IsValid) return View("Form", vm);

        try
        {
            if (!await _courses.UpdateAsync(ToCourse(vm))) return NotFound();
            TempData["Success"] = $"Course {vm.CourseCode} updated.";
            return RedirectToAction("Index", "Editor");
        }
        catch (PostgresException ex)
        {
            ModelState.AddModelError(string.Empty, PgErrors.ToMessage(ex));
            return View("Form", vm);
        }
    }

    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _courses.GetByIdAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost("Delete/{id:int}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var c = await _courses.GetByIdAsync(id);
        if (c == null) return NotFound();
        try
        {
            await _courses.DeleteAsync(id);
            TempData["Success"] = $"Course {c.CourseCode} deleted.";
            return RedirectToAction("Index", "Editor");
        }
        catch (PostgresException ex)                            // 23503: course still has projects
        {
            ModelState.AddModelError(string.Empty, PgErrors.ToMessage(ex));
            return View("Delete", c);
        }
    }

    private static Course ToCourse(CourseFormViewModel vm) => new()
    {
        CourseId = vm.CourseId,
        CourseCode = vm.CourseCode!,
        CourseTitle = vm.CourseTitle!,
        Description = vm.Description,
        Units = vm.Units ?? 0,
        AcademicTerm = vm.AcademicTerm,
        ImageUrl = vm.ImageUrl
    };
}
