using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Application.Security;
using VentaStock.Web.Security;

namespace VentaStock.Web.Controllers;

[Authorize]
[UserAccessDenied]
public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly ICurrentUser _currentUser;

    public UserController(IUserService userService, ICurrentUser currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    // GET: /User
    [Authorize(Policy = UserPermissions.Read)]
    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllAsync();
        return View(users);
    }

    // GET: /User/Create
    [Authorize(Policy = UserPermissions.Create)]
    public async Task<IActionResult> Create()
    {
        await CargarRolesAsync();
        return View(new CreateUserDto());
    }

    // POST: /User/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = UserPermissions.Create)]
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
            return RedirectAfterMutation();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CargarRolesAsync(dto.IdRol);
            return View(dto);
        }
    }

    // GET: /User/Edit/5
    [Authorize(Policy = UserPermissions.Update)]
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

    // POST: /User/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = UserPermissions.Update)]
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
            return RedirectAfterMutation();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CargarRolesAsync(dto.IdRol);
            return View(dto);
        }
    }

    // GET: /User/Delete/5
    [Authorize(Policy = UserPermissions.Deactivate)]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _userService.GetForDeleteByIdAsync(id);
        if (dto is null)
        {
            return NotFound();
        }

        return View(dto);
    }

    // POST: /User/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = UserPermissions.Deactivate)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _userService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Usuario dado de baja exitosamente.";
            return RedirectAfterMutation();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectAfterMutation();
        }
    }

    private IActionResult RedirectAfterMutation() => _currentUser.HasPermission(UserPermissions.Read)
        ? RedirectToAction(nameof(Index))
        : RedirectToAction("Index", "Home");

    // Helper privado para poblar el <select> de Roles
    private async Task CargarRolesAsync(int? selectedRoleId = null)
    {
        var roles = await _userService.GetRolesForSelectAsync();
        ViewBag.Roles = new SelectList(roles, nameof(RoleSelectDto.IdRol), nameof(RoleSelectDto.Nombre), selectedRoleId);
    }
}