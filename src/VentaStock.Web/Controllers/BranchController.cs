using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Security;
using VentaStock.Web.Security;

namespace VentaStock.Web.Controllers;

[Authorize(Roles = SystemRoles.Owner)]
[UserAccessDenied]
public class BranchController(IBranchService branchService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var searchTerm = search?.Trim();
        ViewData["SearchTerm"] = searchTerm;
        return View(await branchService.GetAllAsync(searchTerm));
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateBranchDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBranchDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await branchService.CreateAsync(dto);
                TempData["SuccessMessage"] = "Sucursal creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await branchService.GetForEditByIdAsync(id);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditBranchDto dto)
    {
        if (id != dto.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await branchService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Sucursal actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await branchService.GetForDeleteByIdAsync(id);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await branchService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Sucursal eliminada correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}

