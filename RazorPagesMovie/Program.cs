using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using System.Globalization;
var builder = WebApplication.CreateBuilder(args);
// Контекст фільмів (у вас вже є)
builder.Services.AddDbContext<RazorPagesMovieContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("RazorPagesMovieContext")));
// Якщо SQLite, то:
// options.UseSqlite(builder.Configuration.GetConnectionString("RazorPagesMovieContext"))
// Контекст Identity (та сама БД/рядок)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("RazorPagesMovieContext")));
// або UseSqlite(...), якщо у вас SQLite
// Identity + Ролі
// Identity з ролями + вбудований UI
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // для демо
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();
// Політика "лише Admin"
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Admin"));
});
// Закриваємо папку /Movies
builder.Services.AddRazorPages(options =>

{
    options.Conventions.AuthorizeFolder("/Movies", "RequireAdministratorRole");
    options.Conventions.AuthorizeFolder("/Reviews", "RequireAdministratorRole"); // ← додаємо
});
var app = builder.Build();


var defaultCulture = new CultureInfo("en-US");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

app.UseRequestLocalization(localizationOptions);


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // <— важливо
app.UseAuthorization(); // <— важливо
app.MapRazorPages();
// ВАЖЛИВО: сідер ДО app.Run()
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAdminAsync(scope.ServiceProvider, app.Configuration);
}
app.Run();