using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using gifters.Models;

namespace gifters.Data;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record DashboardData(
    int DonationsCount,
    int VolunteersCount,
    IReadOnlyList<Donation> RecentDonations,
    IReadOnlyList<VolunteerRegistration> RecentVolunteers);

public interface IRoomDb
{
    Task EnsureLoadedAsync();
    Task AddDonationAsync(Donation donation);
    Task AddVolunteerAsync(VolunteerRegistration registration);
    Task<int> GetDonationsCountAsync();
    Task<int> GetVolunteersCountAsync();
    Task<IReadOnlyList<Donation>> GetDonationsAsync(int limit = 10);
    Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsAsync(int limit = 10);
    Task<IReadOnlyList<Donation>> GetAllDonationsAsync();
    Task<IReadOnlyList<VolunteerRegistration>> GetAllVolunteerRegistrationsAsync();
    Task<PagedResult<Donation>> GetDonationsPageAsync(int pageNumber, int pageSize);
    Task<PagedResult<VolunteerRegistration>> GetVolunteerRegistrationsPageAsync(int pageNumber, int pageSize);
    Task<DashboardData> GetDashboardDataAsync();
    Task<bool> DeleteDonationAsync(int id);
    Task<bool> DeleteVolunteerAsync(int id);
    Task<IReadOnlyList<Donation>> GetDonationsByUserAsync(string username, int limit = 50);
    Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsByUserAsync(string username, int limit = 50);
}

public class RoomDb : IRoomDb
{
    private readonly GiftersDbContext _db;
    private readonly IDbContextFactory<GiftersDbContext>? _dbFactory;

    public RoomDb(GiftersDbContext db)
    {
        _db = db;
    }

    public RoomDb(GiftersDbContext db, IDbContextFactory<GiftersDbContext> dbFactory)
    {
        _db = db;
        _dbFactory = dbFactory;
    }

    public Task EnsureLoadedAsync() => Task.CompletedTask;

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        if (_dbFactory is null)
        {
            return new DashboardData(
                await _db.Donations.CountAsync(),
                await _db.VolunteerRegistrations.CountAsync(),
                await _db.Donations.AsNoTracking().OrderByDescending(d => d.CreatedAt).Take(5).ToListAsync(),
                await _db.VolunteerRegistrations.AsNoTracking().OrderByDescending(v => v.CreatedAt).Take(5).ToListAsync());
        }

        await using var countDb = await _dbFactory.CreateDbContextAsync();
        await using var recentDonationsDb = await _dbFactory.CreateDbContextAsync();
        await using var recentVolunteersDb = await _dbFactory.CreateDbContextAsync();

        var donationsCount = await countDb.Donations.CountAsync();
        var volunteersCount = await countDb.VolunteerRegistrations.CountAsync();
        var donationsTask = recentDonationsDb.Donations.AsNoTracking()
            .OrderByDescending(d => d.CreatedAt).Take(5).ToListAsync();
        var volunteersTask = recentVolunteersDb.VolunteerRegistrations.AsNoTracking()
            .OrderByDescending(v => v.CreatedAt).Take(5).ToListAsync();

        await Task.WhenAll(donationsTask, volunteersTask);
        return new DashboardData(
            donationsCount,
            volunteersCount,
            donationsTask.Result,
            volunteersTask.Result);
    }

    public async Task AddDonationAsync(Donation donation)
    {
        ArgumentNullException.ThrowIfNull(donation);
        donation.CreatedAt = DateTime.UtcNow;
        _db.Donations.Add(donation);
        await _db.SaveChangesAsync();
    }

    public async Task AddVolunteerAsync(VolunteerRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        registration.CreatedAt = DateTime.UtcNow;
        _db.VolunteerRegistrations.Add(registration);
        await _db.SaveChangesAsync();
    }

    public Task<int> GetDonationsCountAsync() => _db.Donations.CountAsync();
    public Task<int> GetVolunteersCountAsync() => _db.VolunteerRegistrations.CountAsync();

    public async Task<IReadOnlyList<Donation>> GetDonationsAsync(int limit = 10) =>
        await _db.Donations.AsNoTracking().OrderByDescending(d => d.CreatedAt).Take(limit).ToListAsync();

    public async Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsAsync(int limit = 10) =>
        await _db.VolunteerRegistrations.AsNoTracking().OrderByDescending(v => v.CreatedAt).Take(limit).ToListAsync();

    public async Task<IReadOnlyList<Donation>> GetAllDonationsAsync() =>
        await _db.Donations.AsNoTracking().OrderByDescending(d => d.CreatedAt).ToListAsync();

    public async Task<IReadOnlyList<VolunteerRegistration>> GetAllVolunteerRegistrationsAsync() =>
        await _db.VolunteerRegistrations.AsNoTracking().OrderByDescending(v => v.CreatedAt).ToListAsync();

    public async Task<PagedResult<Donation>> GetDonationsPageAsync(int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var total = await _db.Donations.CountAsync();
        var items = await _db.Donations.AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResult<Donation>(items, total, pageNumber, pageSize);
    }

    public async Task<PagedResult<VolunteerRegistration>> GetVolunteerRegistrationsPageAsync(int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var total = await _db.VolunteerRegistrations.CountAsync();
        var items = await _db.VolunteerRegistrations.AsNoTracking()
            .OrderByDescending(v => v.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResult<VolunteerRegistration>(items, total, pageNumber, pageSize);
    }

    public async Task<bool> DeleteDonationAsync(int id)
    {
        var donation = await _db.Donations.FindAsync(id);
        if (donation is null) return false;
        _db.Donations.Remove(donation);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVolunteerAsync(int id)
    {
        var registration = await _db.VolunteerRegistrations.FindAsync(id);
        if (registration is null) return false;
        _db.VolunteerRegistrations.Remove(registration);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<Donation>> GetDonationsByUserAsync(string username, int limit = 50) =>
        await _db.Donations.AsNoTracking().Where(d => d.SubmittedBy == username)
            .OrderByDescending(d => d.CreatedAt).Take(limit).ToListAsync();

    public async Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsByUserAsync(string username, int limit = 50) =>
        await _db.VolunteerRegistrations.AsNoTracking().Where(v => v.SubmittedBy == username)
            .OrderByDescending(v => v.CreatedAt).Take(limit).ToListAsync();
}