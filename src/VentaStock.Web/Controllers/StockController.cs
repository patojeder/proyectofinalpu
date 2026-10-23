using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Security.Permissions;
using VentaStock.Web.Security;

namespace VentaStock.Web.Controllers;

[Authorize]
[UserAccessDenied]
public class StockController(IStockService stockService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, bool lowStockOnly = false)
    {
        if (search?.Trim().Length > 150)
        {
            TempData["ErrorMessage"] = "La búsqueda admite hasta 150 caracteres.";
            return RedirectToAction(nameof(Index));
        }
        return View(await stockService.GetAllAsync(search, lowStockOnly));
    }

    [HttpGet]
    [Authorize(Policy = StockPermissions.Receipt)]
    public async Task<IActionResult> Receipt(int id)
    {
        var dto = await stockService.GetForReceiptAsync(id);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = StockPermissions.Receipt)]
    public async Task<IActionResult> Receipt(int id, [Bind("ProductId,OperationId,Quantity,Reason")] StockReceiptDto dto)
    {
        if (id != dto.ProductId) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                var created = await stockService.ReceiveAsync(dto);
                TempData["SuccessMessage"] = created ? "Ingreso registrado correctamente." : "El ingreso ya estaba registrado; no se duplicó.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        var current = await stockService.GetForReceiptAsync(id);
        if (current is null) return NotFound();
        dto.Product = current.Product;
        return View(dto);
    }

    [HttpGet]
    [Authorize(Policy = StockPermissions.Adjust)]
    public async Task<IActionResult> Adjust(int id)
    {
        var dto = await stockService.GetForAdjustmentAsync(id);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = StockPermissions.Adjust)]
    public async Task<IActionResult> Adjust(int id, [Bind("ProductId,OperationId,CountedQuantity,ExpectedQuantity,Reason")] StockAdjustmentDto dto)
    {
        if (id != dto.ProductId) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                var created = await stockService.AdjustAsync(dto);
                TempData["SuccessMessage"] = created ? "Ajuste registrado correctamente." : "El ajuste ya estaba registrado; no se duplicó.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        var current = await stockService.GetForAdjustmentAsync(id);
        if (current is null) return NotFound();
        dto.Product = current.Product;
        dto.ExpectedQuantity = current.ExpectedQuantity;
        ModelState.Remove(nameof(dto.ExpectedQuantity));
        return View(dto);
    }

    [HttpGet]
    [Authorize(Policy = StockPermissions.ReadMovements)]
    public async Task<IActionResult> History(int id, int page = 1)
    {
        var dto = await stockService.GetHistoryAsync(id, page);
        return dto is null ? NotFound() : View(dto);
    }
}
