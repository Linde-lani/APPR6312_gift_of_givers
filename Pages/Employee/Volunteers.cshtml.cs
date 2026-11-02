using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gifters.Data;
using gifters.Models;

namespace gifters.Pages.Employee;

public class VolunteersModel : PageModel
{
    private readonly IRoomDb _roomDb;
    private const int PageSize = 20;

    public VolunteersModel(IRoomDb roomDb)
    {
        _roomDb = roomDb;
    }

    public IReadOnlyList<VolunteerRegistration> Volunteers { get; private set; } = new List<VolunteerRegistration>();
    public int PageNumber { get; private set; }
    public int TotalPages { get; private set; }

    [TempData]
    public string? Message { get; set; }

    public async Task OnGetAsync(int page = 1)
    {
        var result = await _roomDb.GetVolunteerRegistrationsPageAsync(page, PageSize);
        Volunteers = result.Items;
        PageNumber = result.PageNumber;
        TotalPages = result.TotalPages;
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, int page = 1)
    {
        var ok = await _roomDb.DeleteVolunteerAsync(id);
        Message = ok ? "Volunteer registration deleted." : "Record not found.";
        return RedirectToPage(new { page });
    }
}