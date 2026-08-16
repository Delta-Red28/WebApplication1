using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly RestauranteContext _context;

        public AccountController(RestauranteContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Account/Login
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string usuario, string password)
        {
            if (string.IsNullOrWhiteSpace(usuario) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Debe ingresar usuario y contraseña.";
                return View();
            }

            usuario = usuario.Trim();

            var usuarioDb = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Usuario1 == usuario);

            if (usuarioDb == null)
            {
                ViewBag.Error = "El usuario o la contraseña son incorrectos.";
                return View();
            }

            if (!usuarioDb.Estado)
            {
                ViewBag.Error = "El usuario se encuentra inactivo.";
                return View();
            }

            bool passwordCorrecta;

            try
            {
                passwordCorrecta = BCrypt.Net.BCrypt.Verify(
                    password,
                    usuarioDb.PasswordHash
                );
            }
            catch
            {
                passwordCorrecta = false;
            }

            if (!passwordCorrecta)
            {
                usuarioDb.IntentosFallidos++;

                await _context.SaveChangesAsync();

                ViewBag.Error = "El usuario o la contraseña son incorrectos.";
                return View();
            }

            // Login correcto
            usuarioDb.IntentosFallidos = 0;
            usuarioDb.UltimoAcceso = DateTime.Now;

            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuarioDb.IdUsuario.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    usuarioDb.Usuario1
                ),

                new Claim(
                    ClaimTypes.GivenName,
                    usuarioDb.Nombres
                ),

                new Claim(
                    ClaimTypes.Surname,
                    usuarioDb.Apellidos
                ),

                new Claim(
                    ClaimTypes.Email,
                    usuarioDb.Correo
                ),

                new Claim(
                    ClaimTypes.Role,
                    usuarioDb.IdRolNavigation.Nombre
                )
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            // Si debe cambiar la contraseña, no puede entrar al Home.
            if (usuarioDb.DebeCambiarPassword)
            {
                return RedirectToAction(
                    "CambiarPassword",
                    "Account"
                );
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/CambiarPassword
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CambiarPassword()
        {
            var idUsuarioClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                await CerrarSesionAsync();

                return RedirectToAction("Login", "Account");
            }

            var usuarioDb = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuarioDb == null || !usuarioDb.Estado)
            {
                await CerrarSesionAsync();

                return RedirectToAction("Login", "Account");
            }

            // Si ya no está obligado a cambiar contraseña,
            // no necesita permanecer en esta pantalla.
            if (!usuarioDb.DebeCambiarPassword)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new CambiarPasswordViewModel());
        }

        // POST: /Account/CambiarPassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(
            CambiarPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var idUsuarioClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                await CerrarSesionAsync();

                return RedirectToAction("Login", "Account");
            }

            // Volvemos a consultar el usuario directamente desde la BD.
            var usuarioDb = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuarioDb == null || !usuarioDb.Estado)
            {
                await CerrarSesionAsync();

                return RedirectToAction("Login", "Account");
            }

            // Si ya no está obligado a cambiar contraseña,
            // no procesamos nuevamente el formulario.
            if (!usuarioDb.DebeCambiarPassword)
            {
                return RedirectToAction("Index", "Home");
            }

            // Verificar contraseña actual con BCrypt.
            bool passwordActualCorrecta;

            try
            {
                passwordActualCorrecta = BCrypt.Net.BCrypt.Verify(
                    model.PasswordActual,
                    usuarioDb.PasswordHash
                );
            }
            catch
            {
                passwordActualCorrecta = false;
            }

            if (!passwordActualCorrecta)
            {
                ModelState.AddModelError(
                    nameof(model.PasswordActual),
                    "La contraseña actual es incorrecta."
                );

                return View(model);
            }

            // Evitar reutilizar exactamente la misma contraseña.
            bool nuevaPasswordEsLaMisma;

            try
            {
                nuevaPasswordEsLaMisma = BCrypt.Net.BCrypt.Verify(
                    model.NuevaPassword,
                    usuarioDb.PasswordHash
                );
            }
            catch
            {
                nuevaPasswordEsLaMisma = false;
            }

            if (nuevaPasswordEsLaMisma)
            {
                ModelState.AddModelError(
                    nameof(model.NuevaPassword),
                    "La nueva contraseña debe ser diferente de la contraseña actual."
                );

                return View(model);
            }

            // Generar nuevo hash BCrypt.
            usuarioDb.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    model.NuevaPassword
                );

            // Marcar que ya no necesita cambiar la contraseña.
            usuarioDb.DebeCambiarPassword = false;

            // Registrar fecha y hora del cambio.
            usuarioDb.FechaCambioPassword = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Contraseña actualizada correctamente.";

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await CerrarSesionAsync();

            Response.Headers["Cache-Control"] =
                "no-cache, no-store, must-revalidate";

            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task CerrarSesionAsync()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            Response.Headers["Cache-Control"] =
                "no-cache, no-store, must-revalidate";

            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
        }
    }

    public class CambiarPasswordViewModel
    {
        [Required(
            ErrorMessage = "Debe ingresar su contraseña actual."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(
            ErrorMessage = "Debe ingresar una nueva contraseña."
        )]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "La nueva contraseña debe tener entre 8 y 100 caracteres."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required(
            ErrorMessage = "Debe confirmar la nueva contraseña."
        )]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NuevaPassword),
            ErrorMessage = "Las contraseñas nuevas no coinciden."
        )]
        [Display(Name = "Confirmar nueva contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}