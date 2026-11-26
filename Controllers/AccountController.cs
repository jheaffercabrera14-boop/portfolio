using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Security;
using Portfolio.ViewModels;

namespace Portfolio.Controllers;

/// <summary>Editor sign-in / sign-out (extra credit).</summary>
public class AccountController : Controller
{
    private readonly EditorCredentialChecker _checker;
    public AccountController(EditorCredentialChecker checker) => _checker = checker;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!_checker.IsConfigured)
        {
            ModelState.AddModelError(string.Empty,
                "Editor sign-in is not configured. Set Editor:Username and Editor:Password (see README.md, step 3).");
            return View(vm);
        }

        if (!_checker.Validate(vm.Username, vm.Password))
        {
            ModelState.AddModelError(string.Empty, "Wrong username or password.");
            vm.Password = null;
            return View(vm);
        }

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, vm.Username!), new Claim(ClaimTypes.Role, "Editor") },
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        // Only follow LOCAL return urls (prevents open-redirect attacks).
        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return LocalRedirect(vm.ReturnUrl);
        return RedirectToAction("Index", "Editor");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
