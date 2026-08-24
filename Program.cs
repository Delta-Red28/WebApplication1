using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONEXIÓN A LA BASE DE DATOS
// ============================================================

builder.Services.AddDbContext<RestauranteContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));


// ============================================================
// SERVICIOS DE LA APLICACIÓN
// ============================================================

builder.Services.AddScoped<InventarioService>();


// ============================================================
// AUTENTICACIÓN MEDIANTE COOKIES
// ============================================================

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


// ============================================================
// AUTORIZACIÓN
// ============================================================

builder.Services.AddAuthorization();


// ============================================================
// MVC
// ============================================================

builder.Services.AddControllersWithViews();


var app = builder.Build();


// ============================================================
// CONFIGURACIÓN DEL PIPELINE
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();


// ============================================================
// AUTENTICACIÓN
// ============================================================

app.UseAuthentication();


// ============================================================
// CONTROL DE ESTADO DEL USUARIO
// ============================================================
//
// Este middleware verifica:
//
// 1. Si el usuario está desactivado.
//    -> Se cierra su sesión y vuelve al Login.
//
// 2. Si el usuario debe cambiar su contraseña.
//    -> Solo podrá acceder a:
//       - Login
//       - CambiarPassword
//       - Logout
//       - AccessDenied
//
// Esto evita que un usuario desactivado pueda continuar
// utilizando el sistema con una sesión que ya estaba abierta.
// ============================================================

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // =====================================================
        // RUTAS QUE NO DEBEN SER BLOQUEADAS
        // =====================================================

        bool esAccountPermitido =
            path.Equals(
                "/Account/Login",
                StringComparison.OrdinalIgnoreCase)

            ||

            path.Equals(
                "/Account/CambiarPassword",
                StringComparison.OrdinalIgnoreCase)

            ||

            path.Equals(
                "/Account/Logout",
                StringComparison.OrdinalIgnoreCase)

            ||

            path.Equals(
                "/Account/AccessDenied",
                StringComparison.OrdinalIgnoreCase);


        // =====================================================
        // OBTENER ID DEL USUARIO AUTENTICADO
        // =====================================================

        var idUsuarioClaim =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


        if (int.TryParse(idUsuarioClaim, out int idUsuario))
        {
            var dbContext =
                context.RequestServices
                    .GetRequiredService<RestauranteContext>();


            // =================================================
            // CONSULTAR ESTADO DEL USUARIO
            // =================================================

            var usuario = await dbContext.Usuarios
                .AsNoTracking()
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => new
                {
                    u.Estado,
                    u.DebeCambiarPassword
                })
                .FirstOrDefaultAsync();


            // =================================================
            // USUARIO NO EXISTE
            // =================================================

            if (usuario == null)
            {
                await context.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                context.Response.Redirect("/Account/Login");

                return;
            }


            // =================================================
            // USUARIO DESACTIVADO
            // =================================================
            //
            // Si el administrador o gerente desactiva al usuario
            // mientras este tiene una sesión abierta, la próxima
            // petición cerrará automáticamente su sesión.
            // =================================================

            if (!usuario.Estado)
            {
                await context.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                context.Response.Redirect(
                    "/Account/Login?mensaje=desactivado"
                );

                return;
            }


            // =================================================
            // USUARIO DEBE CAMBIAR CONTRASEÑA
            // =================================================

            if (!esAccountPermitido &&
                usuario.DebeCambiarPassword)
            {
                context.Response.Redirect(
                    "/Account/CambiarPassword"
                );

                return;
            }
        }
    }

    await next();
});


// ============================================================
// AUTORIZACIÓN
// ============================================================

app.UseAuthorization();


// ============================================================
// ARCHIVOS ESTÁTICOS
// ============================================================

app.MapStaticAssets();


// ============================================================
// RUTA PRINCIPAL
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();


// ============================================================
// INICIALIZAR DATOS NECESARIOS
// ============================================================

await SeedData.InicializarAsync(app.Services);


// ============================================================
// INICIAR APLICACIÓN
// ============================================================

app.Run();