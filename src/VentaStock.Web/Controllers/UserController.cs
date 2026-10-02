using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;

namespace VentaStock.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: /Users
    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllAsync();
        return View(users);
    }

    // GET: /Users/Create
    public async Task<IActionResult> Create()
    {
        await CargarRolesAsync();
        return View(new CreateUserDto());
    }

    // POST: /Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            await CargarRolesAsync(dto.IdRol);
            return View(dto);
        }

        try
        {
            await _userService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Usuario creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CargarRolesAsync(dto.IdRol);
            return View(dto);
        }
    }

    // GET: /Users/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _userService.GetForEditByIdAsync(id);
        if (dto is null)
        {
            return NotFound();
        }

        await CargarRolesAsync(dto.IdRol);
        return View(dto);
    }

    // POST: /Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserDto dto)
    {
        if (id != dto.IdUsuario)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await CargarRolesAsync(dto.IdRol);
            return View(dto);
        }

        try
        {
            await _userService.UpdateAsync(dto);
            TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CargarRolesAsync(dto.IdRol);
            return View(dto);
        }
    }

    // GET: /Users/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _userService.GetForDeleteByIdAsync(id);
        if (dto is null)
        {
            return NotFound();
        }

        return View(dto);
    }

    // POST: /Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _userService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Usuario dado de baja exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // Helper privado para poblar el <select> de Roles
    private async Task CargarRolesAsync(int? selectedRoleId = null)
    {
        var roles = await _userService.GetRolesForSelectAsync();
        ViewBag.Roles = new SelectList(roles, nameof(RoleSelectDto.IdRol), nameof(RoleSelectDto.Nombre), selectedRoleId);
    }
}