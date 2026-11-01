using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gifters.Data;
using gifters.Models;

namespace gifters.Pages.Shared;

public class VolunteerModel : PageModel
{
    private readonly IRoomDb _roomDb;

    public VolunteerModel(IRoomDb roomDb)
    {
        _roomDb = roomDb;
    }

    [BindProperty]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    public string Skills { get; set; } = string.Empty;

    [BindProperty]
    public string Availability { get; set; } = string.Empty;

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

        var registration = new VolunteerRegistration
        {
            FullName = FullName,
            Skills = Skills,
            Availability = Availability,
            SubmittedBy = User?.Identity?.Name ?? string.Empty
        };

        await _roomDb.AddVolunteerAsync(registration);

        SuccessMessage = "Thanks — your volunteer signup has been recorded.";

        return RedirectToPage();
    }
}