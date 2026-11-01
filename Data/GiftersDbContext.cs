using Microsoft.EntityFrameworkCore;
using gifters.Models;

namespace gifters.Data;

public sealed class GiftersDbContext : DbContext
{
    public GiftersDbContext(DbContextOptions<GiftersDbContext> options) : base(options) { }

    public DbSet<AccountRecord> Accounts => Set<AccountRecord>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<VolunteerRegistration> VolunteerRegistrations => Set<VolunteerRegistration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountRecord>(entity =>
        {
            entity.HasKey(account => account.Username);
            entity.Property(account => account.Username).HasMaxLength(200).IsRequired();
            entity.Property(account => account.PasswordHashBase64).HasMaxLength(200).IsRequired();
            entity.Property(account => account.SaltBase64).HasMaxLength(200).IsRequired();
            entity.Property(account => account.Role).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(donation => donation.Id);
            entity.Property(donation => donation.Amount).HasPrecision(18, 2);
            entity.Property(donation => donation.DonationType).HasMaxLength(100).IsRequired();
            entity.Property(donation => donation.Currency).HasMaxLength(10).IsRequired();
            entity.Property(donation => donation.SubmittedBy).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<VolunteerRegistration>(entity =>
        {
            entity.HasKey(registration => registration.Id);
            entity.Property(registration => registration.FullName).HasMaxLength(200).IsRequired();
            entity.Property(registration => registration.Skills).HasMaxLength(2000).IsRequired();
            entity.Property(registration => registration.Availability).HasMaxLength(500).IsRequired();
            entity.Property(registration => registration.SubmittedBy).HasMaxLength(200).IsRequired();
        });
    }
}
