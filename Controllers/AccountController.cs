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


        // LOGIN - GET
       

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }


        // LOGIN - POST

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string usuario,
            string password,
            string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            // VALIDACIONES BÁSICAS

            if (string.IsNullOrWhiteSpace(usuario))
            {
                ModelState.AddModelError(
                    "usuario",
                    "Debe ingresar su usuario."
                );
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "password",
                    "Debe ingresar su contraseña."
                );
            }

            if (!ModelState.IsValid)
            {
                return View();
            }


            usuario = usuario.Trim();


            // BUSCAR USUARIO

            var usuarioDb = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u =>
                    u.Usuario1 == usuario);


            // USUARIO NO EXISTE

            if (usuarioDb == null)
            {
                ModelState.AddModelError(
                    "",
                    "Usuario o contraseña incorrectos."
                );

                return View();
            }


            // USUARIO INACTIVO

            if (!usuarioDb.Estado)
            {
                ModelState.AddModelError(
                    "",
                    "El usuario se encuentra inactivo."
                );

                return View();
            }


            // VALIDAR CONTRASEÑA

            bool passwordCorrecta = false;

            try
            {
                passwordCorrecta =
                    BCrypt.Net.BCrypt.Verify(
                        password,
                        usuarioDb.PasswordHash
                    );
            }
            catch
            {
                passwordCorrecta = false;
            }


            // CONTRASEÑA INCORRECTA

            if (!passwordCorrecta)
            {
                usuarioDb.IntentosFallidos++;

                await _context.SaveChangesAsync();

                ModelState.AddModelError(
                    "",
                    "Usuario o contraseña incorrectos."
                );

                return View();
            }


            // LOGIN CORRECTO

            usuarioDb.IntentosFallidos = 0;
            usuarioDb.UltimoAcceso = DateTime.Now;

            await _context.SaveChangesAsync();


            // OBTENER ROL

            string nombreRol =
                usuarioDb.IdRolNavigation?.Nombre
                ?? "";


            // CREAR CLAIMS

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
                    "NombreCompleto",
                    $"{usuarioDb.Nombres} {usuarioDb.Apellidos}"
                ),

                new Claim(
                    ClaimTypes.Email,
                    usuarioDb.Correo
                ),

                new Claim(
                    ClaimTypes.Role,
                    nombreRol
                )
            };


            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );


            var principal = new ClaimsPrincipal(identity);


            // CREAR SESIÓN

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true
                }
            );


            // CAMBIO OBLIGATORIO DE CONTRASEÑA

            if (usuarioDb.DebeCambiarPassword)
            {
                return RedirectToAction(
                    "CambiarPassword",
                    "Account"
                );
            }


            // RETURN URL

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }


            // INICIO DEL SISTEMA

            return RedirectToAction(
                "Index",
                "Home"
            );
        }


        // LOGOUT

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(nameof(Login));
        }


        // ACCESS DENIED

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // CAMBIAR PASSWORD - GET

        [Authorize]
        [HttpGet]
        public IActionResult CambiarPassword()
        {
            return View();
        }


        // CAMBIAR PASSWORD - POST

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(
            string passwordActual,
            string nuevaPassword,
            string confirmarPassword)
        {
            // VALIDACIONES

            if (string.IsNullOrWhiteSpace(passwordActual))
            {
                ModelState.AddModelError(
                    "passwordActual",
                    "Debe ingresar su contraseña actual."
                );
            }

            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                ModelState.AddModelError(
                    "nuevaPassword",
                    "Debe ingresar una nueva contraseña."
                );
            }

            if (string.IsNullOrWhiteSpace(confirmarPassword))
            {
                ModelState.AddModelError(
                    "confirmarPassword",
                    "Debe confirmar la nueva contraseña."
                );
            }

            if (!string.IsNullOrWhiteSpace(nuevaPassword) &&
                !string.IsNullOrWhiteSpace(confirmarPassword) &&
                nuevaPassword != confirmarPassword)
            {
                ModelState.AddModelError(
                    "confirmarPassword",
                    "Las contraseñas no coinciden."
                );
            }


            if (!ModelState.IsValid)
            {
                return View();
            }


            
            // OBTENER ID DEL USUARIO LOGUEADO

            var claimId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            if (!int.TryParse(claimId, out int idUsuario))
            {
                return RedirectToAction(nameof(Login));
            }


            // BUSCAR USUARIO

            var usuarioDb = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == idUsuario);


            if (usuarioDb == null)
            {
                return NotFound();
            }


            // VALIDAR PASSWORD ACTUAL

            bool passwordCorrecta = false;

            try
            {
                passwordCorrecta =
                    BCrypt.Net.BCrypt.Verify(
                        passwordActual,
                        usuarioDb.PasswordHash
                    );
            }
            catch
            {
                passwordCorrecta = false;
            }


            if (!passwordCorrecta)
            {
                ModelState.AddModelError(
                    "passwordActual",
                    "La contraseña actual es incorrecta."
                );

                return View();
            }


            // EVITAR MISMA CONTRASEÑA

            if (BCrypt.Net.BCrypt.Verify(
                    nuevaPassword,
                    usuarioDb.PasswordHash))
            {
                ModelState.AddModelError(
                    "nuevaPassword",
                    "La nueva contraseña debe ser diferente."
                );

                return View();
            }


            // GUARDAR NUEVA CONTRASEÑA

            usuarioDb.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    nuevaPassword
                );

            usuarioDb.DebeCambiarPassword = false;
            usuarioDb.FechaCambioPassword = DateTime.Now;
            usuarioDb.IntentosFallidos = 0;


            await _context.SaveChangesAsync();


            TempData["Mensaje"] =
                "Contraseña actualizada correctamente.";


            return RedirectToAction(
                "Index",
                "Home"
            );
        }
    }
}