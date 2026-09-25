using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gifters.Data;
using gifters.Models;

namespace gifters.Pages.Shared;

public class DonateModel : PageModel
{
    private readonly IRoomDb _roomDb;

    public DonateModel(IRoomDb roomDb)
    {
        _roomDb = roomDb;
    }

    [BindProperty]
    public decimal Amount { get; set; }

    [BindProperty]
    public string DonationType { get; set; } = string.Empty;

    [BindProperty]
    public string Currency { get; set; } = string.Empty;

    [BindProperty]
    public bool Anonymous { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var donation = new Donation
        {
            Amount = Amount,
            DonationType = DonationType,
            Currency = Currency,
            Anonymous = Anonymous,
            SubmittedBy = User?.Identity?.Name ?? string.Empty
        };

        await _roomDb.AddDonationAsync(donation);

        SuccessMessage = "Thank you — your donation has been recorded.";

        return RedirectToPage();
    }
}