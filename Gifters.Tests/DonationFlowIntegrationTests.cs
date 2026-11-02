using System.Security.Claims;
using gifters.Data;
using gifters.Pages.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gifters.Tests;

public class DonationFlowIntegrationTests
{
    [Fact]
    public async Task DonateFormSubmission_PersistsDonationInSqlServer()
    {
        var databaseName = $"GiftersIntegration_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<GiftersDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        try
        {
            await using (var setupContext = new GiftersDbContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();
            }

            await using (var context = new GiftersDbContext(options))
            {
                var roomDb = new RoomDb(context);
                var model = new DonateModel(roomDb)
                {
                    Amount = 1250.50m,
                    DonationType = "One-Time",
                    Currency = "ZAR",
                    Anonymous = false
                };
                var httpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.Name, "integration-user") },
                        "TestAuthentication"))
                };
                model.PageContext = new PageContext
                {
                    HttpContext = httpContext
                };

                await model.OnPostAsync();
            }

            await using (var verificationContext = new GiftersDbContext(options))
            {
                var donation = await verificationContext.Donations.SingleAsync();

                Assert.Equal(1250.50m, donation.Amount);
                Assert.Equal("One-Time", donation.DonationType);
                Assert.Equal("ZAR", donation.Currency);
                Assert.Equal("integration-user", donation.SubmittedBy);
                Assert.False(donation.Anonymous);
            }
        }
        finally
        {
            await using var cleanupContext = new GiftersDbContext(options);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }
}
