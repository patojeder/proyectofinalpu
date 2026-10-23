using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Security.Permissions;
using VentaStock.Web.Security;

namespace VentaStock.Web.Controllers;

[Authorize]
[UserAccessDenied]
public class ServiceController(IServiceCatalogService serviceCatalogService, ICurrentUser currentUser) : Controller
{
    [HttpGet]
    [Authorize(Policy = ServicePermissions.Read)]
    public async Task<IActionResult> Index() => View(await serviceCatalogService.GetAllAsync());

    [HttpGet]
    [Authorize(Policy = ServicePermissions.Create)]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View(new CreateServiceDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = ServicePermissions.Create)]
    public async Task<IActionResult> Create(CreateServiceDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await serviceCatalogService.CreateAsync(dto);
                TempData["SuccessMessage"] = "Servicio creado correctamente.";
                return RedirectAfterMutation();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        await LoadCategoriesAsync();
        return View(dto);
    }

    [HttpGet]
    [Authorize(Policy = ServicePermissions.Update)]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await serviceCatalogService.GetForEditByIdAsync(id);
        if (dto is null) return NotFound();
        await LoadCategoriesAsync();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = ServicePermissions.Update)]
    public async Task<IActionResult> Edit(int id, EditServiceDto dto)
    {
        if (id != dto.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await serviceCatalogService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Servicio actualizado correctamente.";
                return RedirectAfterMutation();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        await LoadCategoriesAsync();
        return View(dto);
    }

    [HttpGet]
    [Authorize(Policy = ServicePermissions.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await serviceCatalogService.GetForDeleteByIdAsync(id);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = ServicePermissions.Delete)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await serviceCatalogService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Servicio eliminado correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectAfterMutation();
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await serviceCatalogService.GetCategoriesForSelectAsync();
        ViewBag.Categories = new MultiSelectList(categories, nameof(CategorySelectDto.Id), nameof(CategorySelectDto.Name));
    }

    private IActionResult RedirectAfterMutation() => currentUser.HasPermission(ServicePermissions.Read)
        ? RedirectToAction(nameof(Index))
        : RedirectToAction("Index", "Home");
}
