using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Security.Permissions;
using VentaStock.Application.Services;
using VentaStock.Web.Security;

namespace VentaStock.Web.Controllers;

[Authorize]
[UserAccessDenied]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUser _currentUser;

    public CategoryController(ICategoryService categoryService, ICurrentUser currentUser)
    {
        _categoryService = categoryService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Policy = CategoryPermissions.Read)]
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();
        return View(categories);
    }

    [HttpGet]
    [Authorize(Policy = CategoryPermissions.Create)]
    public IActionResult Create()
    {
        return View(new CreateCategoryDto());
    }

    [HttpPost]
    [Authorize(Policy = CategoryPermissions.Create)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _categoryService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Categoría creada exitosamente.";
            return RedirectAfterMutation();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Name", ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    [Authorize(Policy = CategoryPermissions.Update)]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _categoryService.GetForEditByIdAsync(id);
        if (dto is null)
            return NotFound();

        return View(dto);
    }

    [HttpPost]
    [Authorize(Policy = CategoryPermissions.Update)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCategoryDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _categoryService.UpdateAsync(dto);
            TempData["SuccessMessage"] = "Categoría actualizada exitosamente.";
            return RedirectAfterMutation();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Name", ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    [Authorize(Policy = CategoryPermissions.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _categoryService.GetForDeleteByIdAsync(id);
        if (dto is null)
            return NotFound();

        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Policy = CategoryPermissions.Delete)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Categoría eliminada correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectAfterMutation();
    }

    private IActionResult RedirectAfterMutation() => _currentUser.HasPermission(CategoryPermissions.Read)
        ? RedirectToAction(nameof(Index))
        : RedirectToAction("Index", "Home");
}
