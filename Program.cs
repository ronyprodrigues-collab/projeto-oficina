using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using Services;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<OficinaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity with roles and EF stores
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<OficinaDbContext>();

// MVC with Views
builder.Services.AddControllersWithViews();
// Optional: keep OpenAPI for existing minimal APIs
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
// Ensure native WKHTMLTOPDF is loaded and register converter (best-effort; skip in design-time)
try
{
    WkhtmltopdfRuntime.EnsureLoaded();
    builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
}
catch (Exception ex)
{
    Console.WriteLine($"WKHTMLTOPDF not loaded: {ex.Message}");
}
builder.Services.AddScoped<IConfiguracoesService, ConfiguracoesService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Serve static files before auth for better performance
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Painel}/{action=Index}/{id?}");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Seed roles and default users within a scoped provider
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // Apply pending migrations
    var db = services.GetRequiredService<OficinaDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedRolesAndUsers(services);
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
