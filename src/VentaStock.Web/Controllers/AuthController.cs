using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Web.Security;

namespace VentaStock.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null, string? sessionStatus = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        if (sessionStatus == "invalidated")
        {
            ViewData["SessionMessage"] = "La cuenta asociada a esta sesión ya no está disponible. Si necesitás acceso, contactá al administrador.";
        }
        else if (sessionStatus == "credentialsChanged")
        {
            ViewData["SessionMessage"] = "La contraseña de esta cuenta fue reiniciada. Iniciá sesión con la contraseña temporal nueva.";
        }
        if (TempData["PasswordChangedMessage"] is string passwordChangedMessage)
        {
            ViewData["PasswordChangedMessage"] = passwordChangedMessage;
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto request, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var usuarioSesion = await _authService.ValidateCredentialsAsync(request);

        if (usuarioSesion == null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(request);
        }

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true, 
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            UserClaimsPrincipalFactory.Create(usuarioSesion),
            authProperties);

        if (usuarioSesion.MustChangePassword)
        {
            return RedirectToAction(nameof(ChangePassword));
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        if (!User.HasClaim("MustChangePassword", "true"))
            return RedirectToAction("Index", "Home");

        return View(new ChangeTemporaryPasswordDto());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangeTemporaryPasswordDto request)
    {
        if (!User.HasClaim("MustChangePassword", "true"))
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
            return View(request);

        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdText, out var userId))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        try
        {
            await _authService.CompleteTemporaryPasswordChangeAsync(userId, request.NewPassword);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["PasswordChangedMessage"] = "La contraseña se actualizó. Iniciá sesión con tu nueva contraseña.";
            return RedirectToAction(nameof(Login));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    public IActionResult Denegado()
    {
        return View();
    }
}
