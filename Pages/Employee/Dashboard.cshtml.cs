using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gifters.Data;
using gifters.Models;

namespace gifters.Pages.Employee;

public class DashboardModel : PageModel
{
    private readonly IRoomDb _roomDb;

    public DashboardModel(IRoomDb roomDb)
    {
        _roomDb = roomDb;
    }

    public int DonationsCount { get; private set; }
    public int VolunteersCount { get; private set; }

    public IReadOnlyList<Donation> RecentDonations { get; private set; } = new List<Donation>();
    public IReadOnlyList<VolunteerRegistration> RecentVolunteers { get; private set; } = new List<VolunteerRegistration>();

    // Combined activity for the Recent Activity panel
    public IReadOnlyList<ActivityItem> RecentActivity { get; private set; } = new List<ActivityItem>();

    public async Task OnGetAsync()
    {
        await _roomDb.EnsureLoadedAsync();

        DonationsCount = await _roomDb.GetDonationsCountAsync();
        VolunteersCount = await _roomDb.GetVolunteersCountAsync();

        RecentDonations = await _roomDb.GetDonationsAsync(5);
        RecentVolunteers = await _roomDb.GetVolunteerRegistrationsAsync(5);

        // Merge into activity feed, order by CreatedAt desc, take up to 10 items
        var activities = new List<ActivityItem>(RecentDonations.Count + RecentVolunteers.Count);

        activities.AddRange(RecentDonations.Select(d => new ActivityItem
        {
            Kind = "Donation",
            Title = $"{d.Amount} {d.Currency}",
            Details = string.IsNullOrWhiteSpace(d.DonationType) ? (d.Anonymous ? "Anonymous" : string.Empty) : $"{d.DonationType}{(d.Anonymous ? " — Anonymous" : string.Empty)}",
            CreatedAt = d.CreatedAt,
            Id = d.Id
        }));

        activities.AddRange(RecentVolunteers.Select(v => new ActivityItem
        {
            Kind = "Volunteer",
            Title = v.FullName,
            Details = v.Skills,
            CreatedAt = v.CreatedAt,
            Id = v.Id
        }));

        RecentActivity = activities
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .ToList();
    }
}