using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
    Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword);
}

public class SqlAccountStore : IAccountStore
{
    private readonly GiftersDbContext _db;
    private const int SaltSize = 16;
    private const int SubkeySize = 32;
    private const int Iterations = 100_000;

    public SqlAccountStore(GiftersDbContext db) => _db = db;

    public Task EnsureLoadedAsync() => Task.CompletedTask;

    public async Task<bool> CreateAsync(string username, string password, string role)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(role))
            return false;

        username = username.Trim();
        if (await UserExistsAsync(username))
            return false;

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Pbkdf2(password, salt, Iterations, SubkeySize);
        _db.Accounts.Add(new AccountRecord(username, Convert.ToBase64String(hash), Convert.ToBase64String(salt), role));

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        var account = await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Username == username.Trim());
        if (account is null)
            return false;

        var salt = Convert.FromBase64String(account.SaltBase64);
        var expected = Convert.FromBase64String(account.PasswordHashBase64);
        var actual = Pbkdf2(password, salt, Iterations, expected.Length);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    public Task<bool> UserExistsAsync(string username) =>
        _db.Accounts.AnyAsync(a => a.Username == username.Trim());

    public async Task<IReadOnlyList<string>> GetAllUsernamesAsync() =>
        await _db.Accounts.AsNoTracking().OrderBy(a => a.Username).Select(a => a.Username).ToListAsync();

    public async Task<string?> GetRoleAsync(string username) =>
        await _db.Accounts.AsNoTracking().Where(a => a.Username == username.Trim()).Select(a => a.Role).FirstOrDefaultAsync();

    public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(newPassword))
            return false;

        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Username == username.Trim());
        if (account is null)
            return false;

        var salt = Convert.FromBase64String(account.SaltBase64);
        var expected = Convert.FromBase64String(account.PasswordHashBase64);
        var actual = Pbkdf2(currentPassword, salt, Iterations, expected.Length);
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
            return false;

        var newSalt = RandomNumberGenerator.GetBytes(SaltSize);
        var newHash = Pbkdf2(newPassword, newSalt, Iterations, SubkeySize);
        _db.Entry(account).CurrentValues.SetValues(account with
        {
            PasswordHashBase64 = Convert.ToBase64String(newHash),
            SaltBase64 = Convert.ToBase64String(newSalt)
        });
        await _db.SaveChangesAsync();
        return true;
    }

    private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int outputBytes)
    {
        using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return derive.GetBytes(outputBytes);
    }
}