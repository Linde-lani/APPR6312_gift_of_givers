using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using gifters.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// File-backed room DB (roomdb.json)
builder.Services.AddSingleton<IRoomDb>(sp =>
{
    var path = Path.Combine(builder.Environment.ContentRootPath, "roomdb.json");
    return new RoomDb(path);
});

// File-backed account store (accounts.json)
builder.Services.AddSingleton<IAccountStore>(sp =>
{
    var path = Path.Combine(builder.Environment.ContentRootPath, "accounts.json");
    return new FileAccountStore(path);
});

// Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "GiftersAuth";
    });

// Require authentication by default; allow anonymous on specific pages
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// === Ensure storage files exist / are loaded ===
var roomDb = app.Services.GetRequiredService<IRoomDb>();
await roomDb.EnsureLoadedAsync();

var accounts = app.Services.GetRequiredService<IAccountStore>();
await accounts.EnsureLoadedAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// STATIC FILES: try to resolve a project wwwroot when running from build output
string ResolveProjectWebRoot()
{
    var configured = app.Environment.WebRootPath;
    if (!string.IsNullOrEmpty(configured) && Directory.Exists(configured))
    {
        return Path.GetFullPath(configured);
    }

    var runtimeDir = new DirectoryInfo(AppContext.BaseDirectory);
    var dir = runtimeDir;
    while (dir != null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "wwwroot")) ||
            dir.EnumerateFiles("*.csproj").Any())
        {
            var candidate = Path.Combine(dir.FullName, "wwwroot");
            if (Directory.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }

            return Path.GetFullPath(candidate);
        }

        dir = dir.Parent;
    }

    return configured ?? string.Empty;
}

var resolvedWebRoot = ResolveProjectWebRoot();

if (!string.IsNullOrEmpty(resolvedWebRoot) && Directory.Exists(resolvedWebRoot))
{
    app.Logger.LogInformation("Using resolved WebRootPath: {WebRoot}", resolvedWebRoot);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(resolvedWebRoot),
        RequestPath = ""
    });
}
else
{
    app.Logger.LogWarning("Resolved WebRootPath not found; falling back to default app.Environment.WebRootPath: {WebRoot}", app.Environment.WebRootPath);
    app.UseStaticFiles();
}

app.UseRouting();

// Authentication must run before we check the request user
app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
