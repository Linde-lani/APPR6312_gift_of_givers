using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gifters.Data;

namespace gifters.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IAccountStore _accounts;

    public RegisterModel(IAccountStore accounts)
    {
        _accounts = accounts;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty]
    public string Role { get; set; } = "Supporter";

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Username and password are required.";
            return Page();
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return Page();
        }

        var allowedRoles = new[] { "Supporter", "Employee" };
        if (string.IsNullOrWhiteSpace(Role) || !allowedRoles.Contains(Role))
        {
            ErrorMessage = "Invalid role selected.";
            return Page();
        }

        var created = await _accounts.CreateAsync(Username.Trim(), Password, Role);
        if (!created)
        {
            ErrorMessage = "A user with that username already exists.";
            return Page();
        }

        SuccessMessage = "Account created. You can now sign in.";
        ModelState.Clear();
        return Page();
    }
}   