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
public class ProductController(IProductService productService, ICurrentUser currentUser) : Controller
{
    [HttpGet]
    [Authorize(Policy = ProductPermissions.Read)]
    public async Task<IActionResult> Index() => View(await productService.GetAllAsync());

    [HttpGet]
    [Authorize(Policy = ProductPermissions.Create)]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View(new CreateProductDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = ProductPermissions.Create)]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        if (!CanManagePrices() && (Request.Form.ContainsKey(nameof(dto.CostPrice))
            || Request.Form.ContainsKey(nameof(dto.SalePrice))))
            return Forbid();
        if (ModelState.IsValid)
        {
            try
            {
                await productService.CreateAsync(dto);
                TempData["SuccessMessage"] = "Producto creado correctamente.";
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
    [Authorize(Policy = ProductPermissions.Update)]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await productService.GetForEditByIdAsync(id);
        if (dto is null) return NotFound();
        await LoadCategoriesAsync();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = ProductPermissions.Update)]
    public async Task<IActionResult> Edit(int id, EditProductDto dto)
    {
        if (id != dto.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await productService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Producto actualizado correctamente.";
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
    [Authorize(Policy = ProductPermissions.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await productService.GetForDeleteByIdAsync(id);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = ProductPermissions.Delete)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await productService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Producto eliminado correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectAfterMutation();
    }

    [HttpGet]
    [Authorize(Policy = ProductPermissions.UpdatePrices)]
    [Authorize(Policy = ProductPermissions.ReadCost)]
    [Authorize(Policy = ProductPermissions.ReadSalePrice)]
    public async Task<IActionResult> Prices(int id)
    {
        var dto = await productService.GetPricesByIdAsync(id);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = ProductPermissions.UpdatePrices)]
    [Authorize(Policy = ProductPermissions.ReadCost)]
    [Authorize(Policy = ProductPermissions.ReadSalePrice)]
    public async Task<IActionResult> Prices(int id, ProductPricesDto dto)
    {
        if (id != dto.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await productService.UpdatePricesAsync(dto);
                TempData["SuccessMessage"] = "Precios actualizados correctamente.";
                return RedirectAfterMutation();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        var current = await productService.GetPricesByIdAsync(id);
        if (current is null) return NotFound();
        dto.Name = current.Name;
        return View(dto);
    }

    private bool CanManagePrices() => currentUser.HasPermission(ProductPermissions.UpdatePrices)
        && currentUser.HasPermission(ProductPermissions.ReadCost)
        && currentUser.HasPermission(ProductPermissions.ReadSalePrice);

    private async Task LoadCategoriesAsync()
    {
        var categories = await productService.GetCategoriesForSelectAsync();
        ViewBag.Categories = new MultiSelectList(categories, nameof(CategorySelectDto.Id), nameof(CategorySelectDto.Name));
    }

    private IActionResult RedirectAfterMutation() => currentUser.HasPermission(ProductPermissions.Read)
        ? RedirectToAction(nameof(Index))
        : RedirectToAction("Index", "Home");
}
