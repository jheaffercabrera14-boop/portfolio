using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using Portfolio.Data;
using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.Validation;
using Portfolio.ViewModels;

namespace Portfolio.Controllers;

/// <summary>EDITOR: add, edit and delete projects. Image URLs (one per line) are saved in the SAME transaction.</summary>
[Route("Editor/Projects")]
public class EditorProjectsController : EditorBaseController
{
    private readonly ProjectRepository _projects;
    private readonly CourseRepository _courses;
    private readonly ProjectImageRepository _images;

    public EditorProjectsController(ProjectRepository projects, CourseRepository courses, ProjectImageRepository images)
    {
        _projects = projects;
        _courses = courses;
        _images = images;
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(int? courseId)
    {
        var vm = new ProjectFormViewModel { CourseId = courseId, CompletedYear = DateTime.Now.Year };
        await FillCoursesAsync(vm);
        return View("Form", vm);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(ProjectFormViewModel vm)
    {
        await FillCoursesAsync(vm);
        var images = FormRules.ParseImageLines(vm.ImageUrlsText);
        FormRules.CheckProject(vm, ModelState, images);          // 3 regexes: image lines, video, (project URL)
        if (!ModelState.IsValid) return View("Form", vm);

        try
        {
            await _projects.CreateWithImagesAsync(ToProject(vm), images);   // ONE transaction
            TempData["Success"] = $"Project “{vm.Title}” saved with {images.Count} image(s).";
            return RedirectToAction("Index", "Editor");
        }
        catch (ImageSaveException ex)                             // a bad image: everything was rolled back
        {
            ModelState.AddModelError(nameof(vm.ImageUrlsText),
                $"Line {ex.Line} ({ex.Url}): {PgErrors.ToMessage(ex.Pg)} Nothing was saved.");
            return View("Form", vm);
        }
        catch (PostgresException ex)                              // the project row itself was refused
        {
            ModelState.AddModelError(string.Empty, PgErrors.ToMessage(ex) + " Nothing was saved.");
            return View("Form", vm);
        }
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _projects.GetByIdAsync(id);
        if (p == null) return NotFound();

        var images = await _images.GetByProjectAsync(id);
        var vm = new ProjectFormViewModel
        {
            ProjectId = p.ProjectId, CourseId = p.CourseId, Title = p.Title, Description = p.Description,
            TechStack = p.TechStack, CompletedYear = p.CompletedYear, ProjectUrl = p.ProjectUrl, VideoUrl = p.VideoUrl,
            ImageUrlsText = string.Join(Environment.NewLine, images.Select(i => i.ImageUrl)),
            ExistingImages = images
        };
        await FillCoursesAsync(vm);
        return View("Form", vm);
    }

    [HttpPost("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, ProjectFormViewModel vm)
    {
        vm.ProjectId = id;
        await FillCoursesAsync(vm);
        vm.ExistingImages = await _images.GetByProjectAsync(id);  // what the database really holds right now

        var images = FormRules.ParseImageLines(vm.ImageUrlsText);
        FormRules.CheckProject(vm, ModelState, images);
        if (!ModelState.IsValid) return View("Form", vm);

        try
        {
            if (!await _projects.UpdateWithImagesAsync(ToProject(vm), images)) return NotFound();   // ONE transaction
            TempData["Success"] = $"Project “{vm.Title}” updated ({images.Count} image(s)).";
            return RedirectToAction("Index", "Editor");
        }
        catch (ImageSaveException ex)
        {
            ModelState.AddModelError(nameof(vm.ImageUrlsText),
                $"Line {ex.Line} ({ex.Url}): {PgErrors.ToMessage(ex.Pg)} Nothing was saved; the old images are unchanged.");
            return View("Form", vm);
        }
        catch (PostgresException ex)
        {
            ModelState.AddModelError(string.Empty, PgErrors.ToMessage(ex) + " Nothing was saved.");
            return View("Form", vm);
        }
    }

    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _projects.GetByIdAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpPost("Delete/{id:int}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var p = await _projects.GetByIdAsync(id);
        if (p == null) return NotFound();
        try
        {
            await _projects.DeleteAsync(id);                      // images are deleted by ON DELETE CASCADE
            TempData["Success"] = $"Project “{p.Title}” and its images deleted.";
            return RedirectToAction("Index", "Editor");
        }
        catch (PostgresException ex)
        {
            ModelState.AddModelError(string.Empty, PgErrors.ToMessage(ex));
            return View("Delete", p);
        }
    }

    private async Task FillCoursesAsync(ProjectFormViewModel vm)
    {
        var courses = await _courses.GetAllAsync();
        vm.CourseOptions = courses
            .Select(c => new SelectListItem($"{c.CourseCode} — {c.CourseTitle}", c.CourseId.ToString()))
            .ToList();
    }

    private static Project ToProject(ProjectFormViewModel vm) => new()
    {
        ProjectId = vm.ProjectId,
        CourseId = vm.CourseId ?? 0,
        Title = vm.Title!,
        Description = vm.Description,
        TechStack = vm.TechStack,
        CompletedYear = vm.CompletedYear ?? 0,
        ProjectUrl = vm.ProjectUrl,
        VideoUrl = vm.VideoUrl
    };
}
