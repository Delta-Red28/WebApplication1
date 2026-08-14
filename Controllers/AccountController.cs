using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

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

        
    }
}