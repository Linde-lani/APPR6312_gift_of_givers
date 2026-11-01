using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gifters.Data;

namespace gifters.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IAccountStore _accounts;

    public LoginModel(IAccountStore accounts)
    {
        _accounts = accounts;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; private set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Username and password are required.";
            return Page();
        }

        var ok = await _accounts.ValidateCredentialsAsync(Username.Trim(), Password);
        if (!ok)
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        var role = await _accounts.GetRoleAsync(Username.Trim()) ?? string.Empty;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, Username.Trim())
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // Redirect by role (if ReturnUrl present and local, respect it)
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Employee/Dashboard");
        }

        // Supporter (normal user) goes to My Activity (shows their donations & signups)
        return RedirectToPage("/Account/MyActivity");
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Account/Login");
    }
}