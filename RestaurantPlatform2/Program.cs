using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantPlatform.Data;
using RestaurantPlatform2.Data;
using System.Globalization;
using OfficeOpenXml;  // За EPPlus
using QuestPDF.Infrastructure; // За QuestPDF

var builder = WebApplication.CreateBuilder(args);

// Локализация – десетични числа със запетая
var supportedCultures = new[] { new CultureInfo("bg-BG") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("bg-BG"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

// База данни и Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Сесии
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Задаване на култура
app.UseRequestLocalization(localizationOptions);

// Създаване на админ и роли
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin@site.com";
    string adminPassword = "Admin123!";

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Middleware за валидна сесия
app.Use(async (context, next) =>
{
    if (context.User.Identity.IsAuthenticated && !context.Session.Keys.Contains("IsActive"))
    {
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        context.Response.Redirect("/Account/SessionExpired");
        return;
    }

    if (context.User.Identity.IsAuthenticated)
    {
        context.Session.SetString("IsActive", "true");
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed данни
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
    await SeedData.InitializeRolesAndAdminAsync(services);
}

// Задаване на лицензи
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;  // Тук задаваме лиценза за EPPlus

QuestPDF.Settings.License = LicenseType.Community;

app.Run();
