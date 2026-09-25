using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gifters.Data;
using gifters.Models;

namespace gifters.Pages.Employee;

public class DonationsModel : PageModel
{
    private readonly IRoomDb _roomDb;

    public DonationsModel(IRoomDb roomDb)
    {
        _roomDb = roomDb;
    }

    public IReadOnlyList<Donation> Donations { get; private set; } = new List<Donation>();

    [TempData]
    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        await _roomDb.EnsureLoadedAsync();
        Donations = await _roomDb.GetAllDonationsAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var ok = await _roomDb.DeleteDonationAsync(id);
        Message = ok ? "Donation deleted." : "Donation not found.";
        return RedirectToPage();
    }
}