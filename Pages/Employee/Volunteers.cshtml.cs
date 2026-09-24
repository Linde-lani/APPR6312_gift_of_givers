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

    public VolunteersModel(IRoomDb roomDb)
    {
        _roomDb = roomDb;
    }

    public IReadOnlyList<VolunteerRegistration> Volunteers { get; private set; } = new List<VolunteerRegistration>();

    [TempData]
    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        await _roomDb.EnsureLoadedAsync();
        Volunteers = await _roomDb.GetAllVolunteerRegistrationsAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var ok = await _roomDb.DeleteVolunteerAsync(id);
        Message = ok ? "Volunteer registration deleted." : "Record not found.";
        return RedirectToPage();
    }
}