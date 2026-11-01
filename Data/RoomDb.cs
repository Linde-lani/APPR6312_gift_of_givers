using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using gifters.Models;

namespace gifters.Data;

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
    Task<bool> DeleteDonationAsync(int id);
    Task<bool> DeleteVolunteerAsync(int id);
    Task<IReadOnlyList<Donation>> GetDonationsByUserAsync(string username, int limit = 50);
    Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsByUserAsync(string username, int limit = 50);
}

public class RoomDb : IRoomDb
{
    private readonly GiftersDbContext _db;

    public RoomDb(GiftersDbContext db) => _db = db;

    public Task EnsureLoadedAsync() => Task.CompletedTask;

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

    public async Task<bool> DeleteDonationAsync(int id)
    {
        var donation = await _db.Donations.FindAsync(id);
        if (donation is null)
            return false;

        _db.Donations.Remove(donation);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVolunteerAsync(int id)
    {
        var registration = await _db.VolunteerRegistrations.FindAsync(id);
        if (registration is null)
            return false;

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