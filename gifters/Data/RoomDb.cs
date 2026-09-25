using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using gifters.Models;

namespace gifters.Data;

public interface IRoomDb
{
    Task EnsureLoadedAsync();
    Task AddDonationAsync(Donation donation);
    Task AddVolunteerAsync(VolunteerRegistration registration);

    // Read APIs added for dashboard / admin usage
    Task<int> GetDonationsCountAsync();
    Task<int> GetVolunteersCountAsync();
    Task<IReadOnlyList<Donation>> GetDonationsAsync(int limit = 10);
    Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsAsync(int limit = 10);

    // Admin operations
    Task<IReadOnlyList<Donation>> GetAllDonationsAsync();
    Task<IReadOnlyList<VolunteerRegistration>> GetAllVolunteerRegistrationsAsync();
    Task<bool> DeleteDonationAsync(int id);
    Task<bool> DeleteVolunteerAsync(int id);

    // New read methods for user-specific data
    Task<IReadOnlyList<Donation>> GetDonationsByUserAsync(string username, int limit = 50);
    Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsByUserAsync(string username, int limit = 50);
}

public class RoomDb : IRoomDb
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private DatabaseState? _state;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public RoomDb(string filePath)
    {
        _filePath = filePath;
    }

    public async Task EnsureLoadedAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            if (_state != null)
            {
                return;
            }

            if (File.Exists(_filePath))
            {
                var text = await File.ReadAllTextAsync(_filePath);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    _state = JsonSerializer.Deserialize<DatabaseState>(text, JsonOptions) ?? CreateEmptyState();
                    return;
                }
            }

            _state = CreateEmptyState();
            await SaveAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task AddDonationAsync(Donation donation)
    {
        if (donation is null) throw new ArgumentNullException(nameof(donation));

        await _mutex.WaitAsync();
        try
        {
            _state ??= CreateEmptyState();

            // assign Id if not provided
            var nextId = (_state.Donations.Count == 0) ? 1 : _state.Donations.Max(d => d.Id) + 1;
            donation.Id = donation.Id == 0 ? nextId : donation.Id;

            donation.CreatedAt = DateTime.UtcNow;
            _state.Donations.Add(donation);
            await SaveAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task AddVolunteerAsync(VolunteerRegistration registration)
    {
        if (registration is null) throw new ArgumentNullException(nameof(registration));

        await _mutex.WaitAsync();
        try
        {
            _state ??= CreateEmptyState();

            var nextId = (_state.VolunteerRegistrations.Count == 0) ? 1 : _state.VolunteerRegistrations.Max(v => v.Id) + 1;
            registration.Id = registration.Id == 0 ? nextId : registration.Id;

            registration.CreatedAt = DateTime.UtcNow;
            _state.VolunteerRegistrations.Add(registration);
            await SaveAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    // Read APIs

    public async Task<int> GetDonationsCountAsync()
    {
        await EnsureLoadedAsync();
        await _mutex.WaitAsync();
        try
        {
            return _state?.Donations.Count ?? 0;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<int> GetVolunteersCountAsync()
    {
        await EnsureLoadedAsync();
        await _mutex.WaitAsync();
        try
        {
            return _state?.VolunteerRegistrations.Count ?? 0;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<IReadOnlyList<Donation>> GetDonationsAsync(int limit = 10)
    {
        await EnsureLoadedAsync();
        await _mutex.WaitAsync();
        try
        {
            var list = _state?.Donations
                        .OrderByDescending(d => d.CreatedAt)
                        .Take(limit)
                        .ToList()
                      ?? new List<Donation>();
            return list;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsAsync(int limit = 10)
    {
        await EnsureLoadedAsync();
        await _mutex.WaitAsync();
        try
        {
            var list = _state?.VolunteerRegistrations
                        .OrderByDescending(v => v.CreatedAt)
                        .Take(limit)
                        .ToList()
                      ?? new List<VolunteerRegistration>();
            return list;
        }
        finally
        {
            _mutex.Release();
        }
    }

    // Admin operations

    public async Task<IReadOnlyList<Donation>> GetAllDonationsAsync()
    {
        await EnsureLoadedAsync();
        await _mutex.WaitAsync();
        try
        {
            return _state?.Donations
                .OrderByDescending(d => d.CreatedAt)
                .ToList() ?? new List<Donation>();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<IReadOnlyList<VolunteerRegistration>> GetAllVolunteerRegistrationsAsync()
    {
        await EnsureLoadedAsync();
        await _mutex.WaitAsync();
        try
        {
            return _state?.VolunteerRegistrations
                .OrderByDescending(v => v.CreatedAt)
                .ToList() ?? new List<VolunteerRegistration>();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<bool> DeleteDonationAsync(int id)
    {
        await EnsureLoadedAsync();
        await _mutex.WaitAsync();
        try
        {
            var d = _state?.Donations.FirstOrDefault(x => x.Id == id);
            if (d == null) return false;
            _state!.Donations.Remove(d);
            await SaveAsync();
            return true;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<bool> DeleteVolunteerAsync(int id)
    {
        await EnsureLoadedAsync();
        await _mutex.WaitAsync();
        try
        {
            var v = _state?.VolunteerRegistrations.FirstOrDefault(x => x.Id == id);
            if (v == null) return false;
            _state!.VolunteerRegistrations.Remove(v);
            await SaveAsync();
            return true;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<IReadOnlyList<Donation>> GetDonationsByUserAsync(string username, int limit = 50)
    {
        await EnsureLoadedAsync();
        await _mutex.WaitAsync();
        try
        {
            var list = _state?.Donations
                        .Where(d => !string.IsNullOrEmpty(d.SubmittedBy) && string.Equals(d.SubmittedBy, username, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(d => d.CreatedAt)
                        .Take(limit)
                        .ToList()
                      ?? new List<Donation>();
            return list;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<IReadOnlyList<VolunteerRegistration>> GetVolunteerRegistrationsByUserAsync(string username, int limit = 50)
    {
        await EnsureLoadedAsync();
        await _mutex.WaitAsync();
        try
        {
            var list = _state?.VolunteerRegistrations
                        .Where(v => !string.IsNullOrEmpty(v.SubmittedBy) && string.Equals(v.SubmittedBy, username, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(v => v.CreatedAt)
                        .Take(limit)
                        .ToList()
                      ?? new List<VolunteerRegistration>();
            return list;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async Task SaveAsync()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(_state, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }

    private static DatabaseState CreateEmptyState() =>
        new() { Donations = new List<Donation>(), VolunteerRegistrations = new List<VolunteerRegistration>() };

    private class DatabaseState
    {
        public List<Donation> Donations { get; set; } = new();
        public List<VolunteerRegistration> VolunteerRegistrations { get; set; } = new();
    }
}   