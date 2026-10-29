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
    public async Task<IActionResult> Index(int? branchId, string? search, bool lowStockOnly = false)
    {
        if (search?.Trim().Length > 150)
        {
            TempData["ErrorMessage"] = "La búsqueda admite hasta 150 caracteres.";
            return RedirectToAction(nameof(Index), new { branchId });
        }
        return View(await stockService.GetAllAsync(branchId, search, lowStockOnly));
    }

    [HttpGet]
    [Authorize(Policy = StockPermissions.Receipt)]
    public async Task<IActionResult> Receipt(int id, int branchId)
    {
        var dto = await stockService.GetForReceiptAsync(id, branchId);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = StockPermissions.Receipt)]
    public async Task<IActionResult> Receipt(int id, [Bind("ProductId,BranchId,OperationId,Quantity,Reason")] StockReceiptDto dto)
    {
        if (id != dto.ProductId) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                var created = await stockService.ReceiveAsync(dto);
                TempData["SuccessMessage"] = created ? "Ingreso registrado correctamente." : "El ingreso ya estaba registrado; no se duplicó.";
                return RedirectToAction(nameof(Index), new { branchId = dto.BranchId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        var current = await stockService.GetForReceiptAsync(id, dto.BranchId);
        if (current is null) return NotFound();
        dto.Product = current.Product;
        return View(dto);
    }

    [HttpGet]
    [Authorize(Policy = StockPermissions.Adjust)]
    public async Task<IActionResult> Adjust(int id, int branchId)
    {
        var dto = await stockService.GetForAdjustmentAsync(id, branchId);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = StockPermissions.Adjust)]
    public async Task<IActionResult> Adjust(int id, [Bind("ProductId,BranchId,OperationId,CountedQuantity,ExpectedQuantity,Reason")] StockAdjustmentDto dto)
    {
        if (id != dto.ProductId) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                var created = await stockService.AdjustAsync(dto);
                TempData["SuccessMessage"] = created ? "Ajuste registrado correctamente." : "El ajuste ya estaba registrado; no se duplicó.";
                return RedirectToAction(nameof(Index), new { branchId = dto.BranchId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        var current = await stockService.GetForAdjustmentAsync(id, dto.BranchId);
        if (current is null) return NotFound();
        dto.Product = current.Product;
        dto.ExpectedQuantity = current.ExpectedQuantity;
        ModelState.Remove(nameof(dto.ExpectedQuantity));
        return View(dto);
    }

    [HttpGet]
    [Authorize(Policy = StockPermissions.Adjust)]
    public async Task<IActionResult> Settings(int id, int branchId)
    {
        var dto = await stockService.GetForSettingsAsync(id, branchId);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = StockPermissions.Adjust)]
    public async Task<IActionResult> Settings(int id, [Bind("ProductId,BranchId,Sector,Shelf,MinimumQuantity")] StockSettingsDto dto)
    {
        if (id != dto.ProductId) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await stockService.UpdateSettingsAsync(dto);
                TempData["SuccessMessage"] = "Ubicación y stock mínimo actualizados correctamente.";
                return RedirectToAction(nameof(Index), new { branchId = dto.BranchId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        var current = await stockService.GetForSettingsAsync(id, dto.BranchId);
        if (current is null) return NotFound();
        dto.Product = current.Product;
        return View(dto);
    }

    [HttpGet]
    [Authorize(Policy = StockPermissions.ReadMovements)]
    public async Task<IActionResult> History(
        int id,
        int branchId,
        DateOnly? fromDate,
        DateOnly? toDate,
        string? userName,
        string? movementType,
        int page = 1)
    {
        var filter = new StockHistoryFilterDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            UserName = userName,
            MovementType = movementType
        };

        try
        {
            var dto = await stockService.GetHistoryAsync(id, branchId, filter, page);
            return dto is null ? NotFound() : View(dto);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            var fallback = await stockService.GetHistoryAsync(id, branchId, null, 1);
            if (fallback is null) return NotFound();
            fallback.Filter = filter;
            return View(fallback);
        }
    }
}
