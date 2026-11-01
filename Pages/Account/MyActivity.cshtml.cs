using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gifters.Data;
using gifters.Models;

namespace gifters.Pages.Account;

[Authorize]
public class MyActivityModel : PageModel
{
    private readonly IRoomDb _roomDb;

    public MyActivityModel(IRoomDb roomDb)
    {
        _roomDb = roomDb;
    }

    public IReadOnlyList<Donation> MyDonations { get; private set; } = new List<Donation>();
    public IReadOnlyList<VolunteerRegistration> MyVolunteers { get; private set; } = new List<VolunteerRegistration>();

    public async Task OnGetAsync()
    {
        var user = User?.Identity?.Name ?? string.Empty;
        await _roomDb.EnsureLoadedAsync();

        MyDonations = await _roomDb.GetDonationsByUserAsync(user, 50);
        MyVolunteers = await _roomDb.GetVolunteerRegistrationsByUserAsync(user, 50);
    }
}