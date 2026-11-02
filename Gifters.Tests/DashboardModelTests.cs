using gifters.Data;
using gifters.Models;
using gifters.Pages.Employee;
using Xunit;

namespace Gifters.Tests;

public class DashboardModelTests
{
    [Fact]
    public async Task OnGetAsync_LoadsDonationAndVolunteerCounts()
    {
        var roomDb = new FakeRoomDb
        {
            Donations = new List<Donation> { NewDonation(1, DateTime.UtcNow) },
            Volunteers = new List<VolunteerRegistration>
            {
                new() { Id = 1, FullName = "Alex", CreatedAt = DateTime.UtcNow }
            }
        };
        var model = new DashboardModel(roomDb);

        await model.OnGetAsync();

        Assert.Equal(1, model.DonationsCount);
        Assert.Equal(1, model.VolunteersCount);
        Assert.Single(model.RecentDonations);
        Assert.Single(model.RecentVolunteers);
    }

    [Fact]
    public async Task OnGetAsync_MergesActivitiesNewestFirstAndLimitsToTen()
    {
        var baseTime = DateTime.UtcNow.AddMinutes(-20);
        var roomDb = new FakeRoomDb
        {
            Donations = Enumerable.Range(1, 6)
                .Select(index => NewDonation(index, baseTime.AddMinutes(index)))
                .ToList(),
            Volunteers = Enumerable.Range(1, 6)
                .Select(index => new VolunteerRegistration
                {
                    Id = index,
                    FullName = $"Volunteer {index}",
                    CreatedAt = baseTime.AddMinutes(index + 6)
                })
                .ToList()
        };
        var model = new DashboardModel(roomDb);

        await model.OnGetAsync();

        Assert.Equal(10, model.RecentActivity.Count);
        Assert.Equal("Volunteer 5", model.RecentActivity[0].Title);
        Assert.Equal("Donation", model.RecentActivity[5].Kind);
        Assert.True(model.RecentActivity.Zip(model.RecentActivity.Skip(1), (current, next) => current.CreatedAt >= next.CreatedAt).All(value => value));
    }

    [Fact]
    public async Task OnGetAsync_FormatsAnonymousDonationDetails()
    {
        var roomDb = new FakeRoomDb
        {
            Donations = new List<Donation>
            {
                new()
                {
                    Id = 7,
                    Amount = 500,
                    Currency = "ZAR",
                    DonationType = "One-Time",
                    Anonymous = true,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
        var model = new DashboardModel(roomDb);

        await model.OnGetAsync();

        var activity = Assert.Single(model.RecentActivity);
        Assert.Equal("500 ZAR", activity.Title);
        Assert.StartsWith("One-Time", activity.Details);
        Assert.Contains("Anonymous", activity.Details);
    }

    private static Donation NewDonation(int id, DateTime createdAt) => new()
    {
        Id = id,
        Amount = id * 100,
        Currency = "ZAR",
        DonationType = "One-Time",
        CreatedAt = createdAt
    };

    private sealed class FakeRoomDb : IRoomDb
    {
        public List<Donation> Donations { get; set; } = new();
        public List<VolunteerRegistration> Volunteers { get; set; } = new();

        public Task EnsureLoadedAsync() => Task.CompletedTask;
        public Task<DashboardData> GetDashboardDataAsync() => Task.FromResult(new DashboardData(
            Donations.Count,
            Volunteers.Count,
            Donations.Take(5).ToList(),
            Volunteers.Take(5).ToList()));
        public Task AddDonationAsync(Donation donation) => Task.CompletedTask;
        public Task AddVolunteerAsync(VolunteerRegistration registration) => Task.CompletedTask;
        public Task<int> GetDonationsCountAsync() => Task.FromResult(Donations.Count);
        public Task<int> GetVolunteersCountAsync() => Task.FromResult(Volunteers.Count);
        public Task<IReadOnlyList<Donation>> GetDonationsAsync(int limit = 10) =>
            Task.FromResult<IReadOnlyList<Donation>>(Donations.Take(limit).ToList());
        public Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsAsync(int limit = 10) =>
            Task.FromResult<IReadOnlyList<VolunteerRegistration>>(Volunteers.Take(limit).ToList());
        public Task<IReadOnlyList<Donation>> GetAllDonationsAsync() =>
            Task.FromResult<IReadOnlyList<Donation>>(Donations);
        public Task<IReadOnlyList<VolunteerRegistration>> GetAllVolunteerRegistrationsAsync() =>
            Task.FromResult<IReadOnlyList<VolunteerRegistration>>(Volunteers);
        public Task<PagedResult<Donation>> GetDonationsPageAsync(int pageNumber, int pageSize) =>
            Task.FromResult(new PagedResult<Donation>(Donations.Take(pageSize).ToList(), Donations.Count, pageNumber, pageSize));
        public Task<PagedResult<VolunteerRegistration>> GetVolunteerRegistrationsPageAsync(int pageNumber, int pageSize) =>
            Task.FromResult(new PagedResult<VolunteerRegistration>(Volunteers.Take(pageSize).ToList(), Volunteers.Count, pageNumber, pageSize));
        public Task<bool> DeleteDonationAsync(int id) => Task.FromResult(false);
        public Task<bool> DeleteVolunteerAsync(int id) => Task.FromResult(false);
        public Task<IReadOnlyList<Donation>> GetDonationsByUserAsync(string username, int limit = 50) =>
            Task.FromResult<IReadOnlyList<Donation>>(Donations.Where(d => d.SubmittedBy == username).Take(limit).ToList());
        public Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsByUserAsync(string username, int limit = 50) =>
            Task.FromResult<IReadOnlyList<VolunteerRegistration>>(Volunteers.Where(v => v.SubmittedBy == username).Take(limit).ToList());
    }
}
