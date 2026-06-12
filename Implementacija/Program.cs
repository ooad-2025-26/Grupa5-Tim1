using bibliotecha.Data;
using bibliotecha.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using bibliotecha.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<Korisnik>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));

builder.Services.Configure<NotificationSettings>(
    builder.Configuration.GetSection("Notifications"));

builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<NotificationProcessor>();
builder.Services.AddHostedService<NotificationBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager =
    scope.ServiceProvider.GetRequiredService<UserManager<Korisnik>>();

    foreach (var nazivUloge in Enum.GetNames<Uloga>())
    {
        if (!await roleManager.RoleExistsAsync(nazivUloge))
        {
            await roleManager.CreateAsync(
                new IdentityRole(nazivUloge));
        }
    }
    foreach (var korisnik in await userManager.Users.ToListAsync())
    {
        var ocekivanaUloga = korisnik.Uloga.ToString();
        var postojeceUloge = await userManager.GetRolesAsync(korisnik);

        if (postojeceUloge.Count == 1 &&
            postojeceUloge.Contains(ocekivanaUloga))
        {
            continue;
        }

        if (postojeceUloge.Count > 0)
        {
            var uklanjanje =
                await userManager.RemoveFromRolesAsync(korisnik, postojeceUloge);

            if (!uklanjanje.Succeeded)
            {
                continue;
            }
        }

        await userManager.AddToRoleAsync(korisnik, ocekivanaUloga);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

