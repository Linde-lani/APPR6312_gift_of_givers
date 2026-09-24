using gifters.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register file-backed RoomDb (roomdb.json placed in app content root)
builder.Services.AddSingleton<IRoomDb>(sp =>
{
    var path = System.IO.Path.Combine(builder.Environment.ContentRootPath, "roomdb.json");
    return new RoomDb(path);
});

var app = builder.Build();

// Ensure storage file exists / is loaded
var roomDb = app.Services.GetRequiredService<IRoomDb>();
await roomDb.EnsureLoadedAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
