using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VentaStock.Application.DTOs;
using VentaStock.Application.Services;

namespace VentaStock.Web.Controllers;

[Authorize(Roles = "Owner,Vendedor")]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: /Category (Visible para Owner y Vendedor)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();
        return View(categories);
    }

    // --- ACCIONES EXCLUSIVAS DE OWNER ---

    // GET: /Category/Create
    [HttpGet]
    [Authorize(Roles = "Owner")]
    public IActionResult Create()
    {
        return View(new CreateCategoryDto());
    }

    // POST: /Category/Create
    [HttpPost]
    [Authorize(Roles = "Owner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _categoryService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Categoría creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Nombre", ex.Message);
            return View(dto);
        }
    }

    // GET: /Category/Edit/5
    [HttpGet]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _categoryService.GetForEditByIdAsync(id);
        if (dto is null)
            return NotFound();

        return View(dto);
    }

    // POST: /Category/Edit/5
    [HttpPost]
    [Authorize(Roles = "Owner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCategoryDto dto)
    {
        if (id != dto.IdCategoria)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _categoryService.UpdateAsync(dto);
            TempData["SuccessMessage"] = "Categoría actualizada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Nombre", ex.Message);
            return View(dto);
        }
    }

    // GET: /Category/Delete/5
    [HttpGet]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _categoryService.GetForDeleteByIdAsync(id);
        if (dto is null)
            return NotFound();

        return View(dto);
    }

    // POST: /Category/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Owner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _categoryService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Categoría eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}