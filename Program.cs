using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Services;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using System.Linq;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------
// 🔥 1. BANCO DE DADOS -> MYSQL
// --------------------------------------------
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");
var serverVersionText = builder.Configuration["Database:ServerVersion"];
if (string.IsNullOrWhiteSpace(serverVersionText))
{
    throw new InvalidOperationException("A configuração 'Database:ServerVersion' precisa ser informada (ex.: 8.0.34-mysql).");
}
var serverVersion = ServerVersion.Parse(serverVersionText);

builder.Services.AddDbContext<OficinaDbContext>(options =>
{
    options.UseMySql(defaultConnection, serverVersion, mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });
});

// --------------------------------------------
// 🔥 2. IDENTIDADE
// --------------------------------------------
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<OficinaDbContext>();

// --------------------------------------------
// 🔥 3. MVC + Views
// --------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// --------------------------------------------
// 🔥 4. SERVIÇOS DA APLICAÇÃO
// --------------------------------------------
builder.Services.AddScoped<IConfiguracoesService, ConfiguracoesService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IOficinaContext, OficinaContext>();
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

// --------------------------------------------
// 🔥 5. CULTURA pt-BR
// --------------------------------------------
var culture = new CultureInfo("pt-BR");
var locOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new List<CultureInfo> { culture },
    SupportedUICultures = new List<CultureInfo> { culture }
};
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// --------------------------------------------
// 🔥 6. COOKIES E LOGIN
// --------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddAuthorization();

var app = builder.Build();

// --------------------------------------------
// Pipeline de middleware
// --------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Cultura
app.UseRequestLocalization(locOptions);

app.UseStaticFiles();
app.UseSession();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var skip = new[]
        {
            "/oficinas/selecionar",
            "/oficinas/limpar",
            "/account",
            "/suporte",
            "/grupos",
            "/identity"
        };

        if (!skip.Any(s => path.StartsWith(s, StringComparison.OrdinalIgnoreCase)))
        {
            var oficinaContext = context.RequestServices.GetRequiredService<IOficinaContext>();
            var oficinaAtual = await oficinaContext.GetOficinaAtualAsync(context.RequestAborted);
            if (oficinaAtual == null)
            {
                var returnUrl = context.Request.Path + context.Request.QueryString;
                var redirectUrl = $"/Oficinas/Selecionar?returnUrl={Uri.EscapeDataString(returnUrl)}";
                context.Response.Redirect(redirectUrl);
                return;
            }
        }
    }

    await next();
});

app.UseAuthorization();

// Rotas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// --------------------------------------------
// 🔥 7. MIGRAÇÕES AUTOMÁTICAS + SEED
// --------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<OficinaDbContext>();
    await db.Database.MigrateAsync();

    await SeedData.SeedRolesAndUsers(services);
    await SeedData.SeedDemoData(services);
}

// --------------------------------------------
// 🔥 8. ABRIR NAVEGADOR AUTOMATICAMENTE
//     → Agora ABRE NA PORTA CORRETA!
// --------------------------------------------
_ = Task.Run(async () =>
{
    await Task.Delay(1500);

    try
    {
        var server = app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();

        var url = addresses?.Addresses?.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(url))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
    catch { }
});

app.Run();
