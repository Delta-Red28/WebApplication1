using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly RestauranteContext _context;

        public HomeController(RestauranteContext context)
        {
            _context = context;
        }

        // GET: /Home
        // GET: /Home/Index
        public async Task<IActionResult> Index()
        {
            // Obtener el ID del usuario autenticado
            var claimId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier);

            if (claimId == null ||
                !int.TryParse(claimId.Value, out int idUsuario))
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener nuevamente el usuario desde la BD
            // para tener información actualizada de rol y permisos.
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                    .ThenInclude(r => r.IdPermisos)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null || !usuario.Estado)
            {
                return RedirectToAction("Login", "Account");
            }

            // Si todavía debe cambiar la contraseña,
            // no puede entrar al Dashboard.
            if (usuario.DebeCambiarPassword)
            {
                return RedirectToAction(
                    "CambiarPassword",
                    "Account");
            }

            // Información del usuario para el Dashboard
            ViewBag.NombreCompleto =
                $"{usuario.Nombres} {usuario.Apellidos}";

            ViewBag.Usuario = usuario.Usuario1;

            ViewBag.Rol =
                usuario.IdRolNavigation?.Nombre ?? "Sin rol";

            ViewBag.Correo = usuario.Correo;

            // Solo permisos activos
            var permisos = usuario.IdRolNavigation?
                .IdPermisos
                .Where(p => p.Estado)
                .OrderBy(p => p.Modulo)
                .ThenBy(p => p.Nombre)
                .ToList()
                ?? new List<Permiso>();

            ViewBag.Permisos = permisos;

            return View();
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}