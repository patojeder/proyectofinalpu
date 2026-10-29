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
public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly ICurrentUser _currentUser;

    public UserController(IUserService userService, ICurrentUser currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    [Authorize(Policy = UserPermissions.Read)]
    public async Task<IActionResult> Index(string? search)
    {
        var searchTerm = search?.Trim();
        ViewData["SearchTerm"] = searchTerm;
        var users = await _userService.GetAllAsync(searchTerm);
        return View(users);
    }

    [Authorize(Policy = UserPermissions.Create)]
    public async Task<IActionResult> Create()
    {
        await LoadRolesAsync();
        return View(new CreateUserDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = UserPermissions.Create)]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(dto.RoleId);
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
            await LoadRolesAsync(dto.RoleId);
            return View(dto);
        }
    }

    [Authorize(Policy = UserPermissions.Update)]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _userService.GetForEditByIdAsync(id);
        if (dto is null)
        {
            return NotFound();
        }

        await LoadRolesAsync(dto.RoleId);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = UserPermissions.Update)]
    public async Task<IActionResult> Edit(int id, EditUserDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(dto.RoleId);
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
            await LoadRolesAsync(dto.RoleId);
            return View(dto);
        }
    }

    [Authorize(Policy = UserPermissions.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _userService.GetForDeleteByIdAsync(id);
        if (dto is null)
        {
            return NotFound();
        }

        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = UserPermissions.Delete)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _userService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Usuario eliminado exitosamente.";
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

    private async Task LoadRolesAsync(int? selectedRoleId = null)
    {
        var roles = await _userService.GetRolesForSelectAsync();
        ViewBag.Roles = new SelectList(roles, nameof(RoleSelectDto.RoleId), nameof(RoleSelectDto.Name), selectedRoleId);
    }
}
