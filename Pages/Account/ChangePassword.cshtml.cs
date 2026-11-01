using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gifters.Data;

namespace gifters.Pages.Account;

[Authorize]
public class ChangePasswordModel : PageModel
{
    private readonly IAccountStore _accounts;

    public ChangePasswordModel(IAccountStore accounts)
    {
        _accounts = accounts;
    }

    [BindProperty]
    public string CurrentPassword { get; set; } = string.Empty;

    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmNewPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (NewPassword != ConfirmNewPassword)
        {
            ErrorMessage = "New passwords do not match.";
            return Page();
        }

        var user = User?.Identity?.Name ?? string.Empty;
        var ok = await _accounts.ChangePasswordAsync(user, CurrentPassword, NewPassword);
        if (!ok)
        {
            ErrorMessage = "Current password invalid or change failed.";
            return Page();
        }

        SuccessMessage = "Password changed successfully.";
        ModelState.Clear();
        return Page();
    }
}