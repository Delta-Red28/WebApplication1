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
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    ));


// ============================================================
// SERVICIOS
// ============================================================

builder.Services.AddScoped<InventarioService>();


// ============================================================
// AUTENTICACIÓN
// ============================================================

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

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
// PRUEBA SEEDDATA
// ============================================================

Console.WriteLine("");
Console.WriteLine("########################################");
Console.WriteLine("VOY A EJECUTAR SEEDDATA");
Console.WriteLine("########################################");
Console.WriteLine("");

try
{
    await SeedData.InicializarAsync(app.Services);

    Console.WriteLine("");
    Console.WriteLine("########################################");
    Console.WriteLine("SEEDDATA TERMINÓ CORRECTAMENTE");
    Console.WriteLine("########################################");
    Console.WriteLine("");
}
catch (Exception ex)
{
    Console.WriteLine("");
    Console.WriteLine("########################################");
    Console.WriteLine("ERROR EN SEEDDATA");
    Console.WriteLine("########################################");
    Console.WriteLine(ex.ToString());
    Console.WriteLine("");

    throw;
}


// ============================================================
// PIPELINE
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

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var path =
            context.Request.Path.Value ?? string.Empty;

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
                    .GetRequiredService<
                        RestauranteContext
                    >();


            var usuario =
                await dbContext.Usuarios
                    .AsNoTracking()
                    .Where(
                        u => u.IdUsuario == idUsuario
                    )
                    .Select(
                        u => new
                        {
                            u.Estado,
                            u.DebeCambiarPassword
                        }
                    )
                    .FirstOrDefaultAsync();


            if (usuario == null)
            {
                await context.SignOutAsync(
                    CookieAuthenticationDefaults
                        .AuthenticationScheme
                );

                context.Response.Redirect(
                    "/Account/Login"
                );

                return;
            }


            if (!usuario.Estado)
            {
                await context.SignOutAsync(
                    CookieAuthenticationDefaults
                        .AuthenticationScheme
                );

                context.Response.Redirect(
                    "/Account/Login?mensaje=desactivado"
                );

                return;
            }


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
    pattern:
        "{controller=Account}/{action=Login}/{id?}"
)
.WithStaticAssets();


// ============================================================
// INICIAR APLICACIÓN
// ============================================================

app.Run();