using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Portfolio.Data;
using Portfolio.Repositories;
using Portfolio.Validation;
using Portfolio.ViewModels;

namespace Portfolio.Controllers;

/// <summary>EDITOR: add, edit and delete ONE image of a project (the project form handles the whole list at once).</summary>
[Route("Editor/Images")]
public class EditorImagesController : EditorBaseController
{
    private readonly ProjectImageRepository _images;
    private readonly ProjectRepository _projects;

    public EditorImagesController(ProjectImageRepository images, ProjectRepository projects)
    {
        _images = images;
        _projects = projects;
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(int projectId)
    {
        var p = await _projects.GetByIdAsync(projectId);
        if (p == null) return NotFound();
        return View("Form", new ImageFormViewModel { ProjectId = projectId, ProjectTitle = p.Title });
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(ImageFormViewModel vm)
    {
        var p = await _projects.GetByIdAsync(vm.ProjectId);
        if (p == null) return NotFound();
        vm.ProjectTitle = p.Title;

        FormRules.CheckImage(vm, ModelState);
        if (!ModelState.IsValid) return View("Form", vm);
        try
        {
            await _images.AddAsync(vm.ProjectId, vm.ImageUrl!);
            TempData["Success"] = "Image added.";
            return RedirectToAction("Edit", "EditorProjects", new { id = vm.ProjectId });
        }
        catch (PostgresException ex)
        {
            ModelState.AddModelError(string.Empty, PgErrors.ToMessage(ex));
            return View("Form", vm);
        }
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var img = await _images.GetByIdAsync(id);
        if (img == null) return NotFound();
        var p = await _projects.GetByIdAsync(img.ProjectId);
        return View("Form", new ImageFormViewModel
        {
            ImageId = img.ImageId, ProjectId = img.ProjectId, ImageUrl = img.ImageUrl, ProjectTitle = p?.Title ?? ""
        });
    }

    [HttpPost("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, ImageFormViewModel vm)
    {
        var img = await _images.GetByIdAsync(id);
        if (img == null) return NotFound();
        vm.ImageId = id;
        vm.ProjectId = img.ProjectId;
        vm.ProjectTitle = (await _projects.GetByIdAsync(img.ProjectId))?.Title ?? "";

        FormRules.CheckImage(vm, ModelState);
        if (!ModelState.IsValid) return View("Form", vm);
        try
        {
            await _images.UpdateUrlAsync(id, vm.ImageUrl!);
            TempData["Success"] = "Image updated.";
            return RedirectToAction("Edit", "EditorProjects", new { id = img.ProjectId });
        }
        catch (PostgresException ex)
        {
            ModelState.AddModelError(string.Empty, PgErrors.ToMessage(ex));
            return View("Form", vm);
        }
    }

    [HttpPost("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var img = await _images.GetByIdAsync(id);
        if (img == null) return NotFound();
        try
        {
            await _images.DeleteAsync(id);
            TempData["Success"] = "Image deleted.";
        }
        catch (PostgresException ex)
        {
            TempData["Error"] = PgErrors.ToMessage(ex);
        }
        return RedirectToAction("Edit", "EditorProjects", new { id = img.ProjectId });
    }
}
