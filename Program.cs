using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Security.Claims;

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


// ============================================================
// BLOQUEO DE USUARIOS QUE DEBEN CAMBIAR SU CONTRASEÑA
// ============================================================
//
// Si el usuario está autenticado y todavía debe cambiar
// su contraseña, solamente podrá utilizar las acciones:
//
// /Account/Login
// /Account/CambiarPassword
// /Account/Logout
// /Account/AccessDenied
//
// Cualquier otra ruta será enviada a CambiarPassword.
// ============================================================

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        bool esAccountPermitido =
            path.Equals("/Account/Login",
                StringComparison.OrdinalIgnoreCase)
            ||
            path.Equals("/Account/CambiarPassword",
                StringComparison.OrdinalIgnoreCase)
            ||
            path.Equals("/Account/Logout",
                StringComparison.OrdinalIgnoreCase)
            ||
            path.Equals("/Account/AccessDenied",
                StringComparison.OrdinalIgnoreCase);

        if (!esAccountPermitido)
        {
            var idUsuarioClaim =
                context.User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (int.TryParse(
                    idUsuarioClaim,
                    out int idUsuario))
            {
                var dbContext =
                    context.RequestServices
                        .GetRequiredService<RestauranteContext>();

                var debeCambiarPassword =
                    await dbContext.Usuarios
                        .AsNoTracking()
                        .Where(u => u.IdUsuario == idUsuario)
                        .Select(u => new
                        {
                            u.DebeCambiarPassword,
                            u.Estado
                        })
                        .FirstOrDefaultAsync();

                if (debeCambiarPassword != null &&
                    debeCambiarPassword.Estado &&
                    debeCambiarPassword.DebeCambiarPassword)
                {
                    context.Response.Redirect(
                        "/Account/CambiarPassword"
                    );

                    return;
                }
            }
        }
    }

    await next();
});

app.UseAuthorization();

// Archivos estáticos
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

// Inicializar datos necesarios
await SeedData.InicializarAsync(app.Services);

app.Run();