using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Conexión a la base de datos
builder.Services.AddDbContext<RestauranteContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Autenticación mediante cookies
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        // Duración de la sesión
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        // Renueva la sesión mientras el usuario esté activo
        options.SlidingExpiration = true;
    });

// Autorización
builder.Services.AddAuthorization();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuración del pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Autenticación antes de autorización
app.UseAuthentication();
app.UseAuthorization();

// Archivos estáticos
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();