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
    private const int PageSize = 20;

    public DonationsModel(IRoomDb roomDb)
    {
        _roomDb = roomDb;
    }

    public IReadOnlyList<Donation> Donations { get; private set; } = new List<Donation>();
    public int PageNumber { get; private set; }
    public int TotalPages { get; private set; }

    [TempData]
    public string? Message { get; set; }

    public async Task OnGetAsync(int page = 1)
    {
        var result = await _roomDb.GetDonationsPageAsync(page, PageSize);
        Donations = result.Items;
        PageNumber = result.PageNumber;
        TotalPages = result.TotalPages;
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int page = 1)
    {
        var ok = await _roomDb.DeleteDonationAsync(id);
        Message = ok ? "Donation deleted." : "Donation not found.";
        return RedirectToPage(new { page });
    }
}