using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace gifters.Data;

public record AccountRecord(string Username, string PasswordHashBase64, string SaltBase64, string Role);

public interface IAccountStore
{
    Task EnsureLoadedAsync();
    Task<bool> CreateAsync(string username, string password, string role);
    Task<bool> ValidateCredentialsAsync(string username, string password);
    Task<bool> UserExistsAsync(string username);
    Task<IReadOnlyList<string>> GetAllUsernamesAsync();
    Task<string?> GetRoleAsync(string username);

    // Added: change password support
    Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword);
}

public class FileAccountStore : IAccountStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private List<AccountRecord> _accounts = new();

    private const int SaltSize = 16;
    private const int SubkeySize = 32;
    private const int Iterations = 100_000;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public FileAccountStore(string filePath)
    {
        _filePath = filePath;
    }

    public async Task EnsureLoadedAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            if (File.Exists(_filePath))
            {
                var text = await File.ReadAllTextAsync(_filePath);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    _accounts = JsonSerializer.Deserialize<List<AccountRecord>>(text, JsonOptions) ?? new List<AccountRecord>();
                    return;
                }
            }

            _accounts = new List<AccountRecord>();
            await SaveAsync();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<bool> CreateAsync(string username, string password, string role)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(role))
            return false;

        username = username.Trim();

        await _mutex.WaitAsync();
        try
        {
            if (_accounts.Any(a => string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // create salt and hash
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var subkey = Pbkdf2(password, salt, Iterations, SubkeySize);

            var rec = new AccountRecord(
                username,
                Convert.ToBase64String(subkey),
                Convert.ToBase64String(salt),
                role
            );

            _accounts.Add(rec);
            await SaveAsync();
            return true;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        await _mutex.WaitAsync();
        try
        {
            var rec = _accounts.FirstOrDefault(a => string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
            if (rec == null) return false;

            var salt = Convert.FromBase64String(rec.SaltBase64);
            var expected = Convert.FromBase64String(rec.PasswordHashBase64);
            var actual = Pbkdf2(password, salt, Iterations, expected.Length);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        await _mutex.WaitAsync();
        try
        {
            return _accounts.Any(a => string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<IReadOnlyList<string>> GetAllUsernamesAsync()
    {
        await _mutex.WaitAsync();
        try
        {
            return _accounts.Select(a => a.Username).ToList();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<string?> GetRoleAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;
        await _mutex.WaitAsync();
        try
        {
            var rec = _accounts.FirstOrDefault(a => string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
            return rec?.Role;
        }
        finally
        {
            _mutex.Release();
        }
    }

    // Implementation for password change
    public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(newPassword))
            return false;

        await _mutex.WaitAsync();
        try
        {
            var index = _accounts.FindIndex(a => string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
                return false;

            var rec = _accounts[index];

            // validate current password
            var salt = Convert.FromBase64String(rec.SaltBase64);
            var expected = Convert.FromBase64String(rec.PasswordHashBase64);
            var actual = Pbkdf2(currentPassword, salt, Iterations, expected.Length);
            if (!CryptographicOperations.FixedTimeEquals(expected, actual))
                return false;

            // create new salt/subkey
            var newSalt = RandomNumberGenerator.GetBytes(SaltSize);
            var newSubkey = Pbkdf2(newPassword, newSalt, Iterations, SubkeySize);

            _accounts[index] = new AccountRecord(
                rec.Username,
                Convert.ToBase64String(newSubkey),
                Convert.ToBase64String(newSalt),
                rec.Role
            );

            await SaveAsync();
            return true;
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
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(_accounts, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }

    private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int outputBytes)
    {
        using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return derive.GetBytes(outputBytes);
    }
}