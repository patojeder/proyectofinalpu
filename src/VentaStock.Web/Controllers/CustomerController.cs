using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Constants;
using VentaStock.Domain.Security.Permissions;
using VentaStock.Web.Security;

namespace VentaStock.Web.Controllers;

[Authorize]
[UserAccessDenied]
public class CustomerController(ICustomerService customerService, ICurrentUser currentUser) : Controller
{
    [HttpGet]
    [Authorize(Policy = CustomerPermissions.Read)]
    public async Task<IActionResult> Index(string? search)
    {
        if (search?.Trim().Length > 150)
        {
            TempData["ErrorMessage"] = "La búsqueda admite hasta 150 caracteres.";
            return RedirectToAction(nameof(Index));
        }
        return View(await customerService.GetAllAsync(search));
    }

    [HttpGet]
    [Authorize(Policy = CustomerPermissions.Create)]
    public IActionResult Create() => View(new CreateCustomerDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = CustomerPermissions.Create)]
    public async Task<IActionResult> Create([Bind("FirstName,LastName,DocumentType,DocumentNumber,CustomerType,Address,Phone")] CreateCustomerDto dto)
    {
        if (dto.CustomerType == CustomerTypes.OnAccount && !currentUser.HasPermission(CustomerPermissions.CreateOnAccount))
            return Forbid();
        RejectFinancialFields();
        if (ModelState.IsValid)
        {
            try
            {
                await customerService.CreateAsync(dto);
                TempData["SuccessMessage"] = "Cliente creado correctamente.";
                return RedirectAfterMutation();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        return View(dto);
    }

    [HttpGet]
    [Authorize(Policy = CustomerPermissions.Update)]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await customerService.GetForEditAsync(id);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = CustomerPermissions.Update)]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Version,FirstName,LastName,DocumentType,DocumentNumber,CustomerType,Address,Phone")] EditCustomerDto dto)
    {
        if (id != dto.Id) return BadRequest();
        RejectFinancialFields();
        if (ModelState.IsValid)
        {
            try
            {
                await customerService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Cliente actualizado correctamente.";
                return RedirectAfterMutation();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        return View(dto);
    }

    private void RejectFinancialFields()
    {
        if (Request.Form.ContainsKey("InitialBalance") || Request.Form.ContainsKey("Balance") || Request.Form.ContainsKey("CreditLimit"))
            ModelState.AddModelError(string.Empty, "Los saldos y límites de cuenta corriente no se modifican desde este formulario.");
    }

    private IActionResult RedirectAfterMutation() => currentUser.HasPermission(CustomerPermissions.Read)
        ? RedirectToAction(nameof(Index)) : RedirectToAction("Index", "Home");
}
