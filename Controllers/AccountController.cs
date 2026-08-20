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


        // ============================================================
        // LOGIN - GET
        // ============================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }


        // ============================================================
        // LOGIN - POST
        // ============================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string? usuario,
            string? password,
            string? returnUrl = null)
        {
            // ========================================================
            // CONSERVAR RETURN URL
            // ========================================================

            ViewBag.ReturnUrl = returnUrl;


            // ========================================================
            // VALIDAR USUARIO
            // ========================================================

            if (string.IsNullOrWhiteSpace(usuario))
            {
                ModelState.AddModelError(
                    "usuario",
                    "Debe ingresar su usuario."
                );
            }


            // ========================================================
            // VALIDAR CONTRASEÑA
            // ========================================================

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "password",
                    "Debe ingresar su contraseña."
                );
            }


            // ========================================================
            // SI HAY ERRORES
            // ========================================================

            if (!ModelState.IsValid)
            {
                return View();
            }


            // ========================================================
            // LIMPIAR USUARIO
            // ========================================================

            usuario = usuario.Trim();


            // ========================================================
            // BUSCAR USUARIO
            // ========================================================

            var usuarioDb = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u =>
                    u.Usuario1 == usuario
                );


            // ========================================================
            // USUARIO NO EXISTE
            // ========================================================

            if (usuarioDb == null)
            {
                ModelState.AddModelError(
                    "",
                    "El usuario ingresado no existe."
                );

                return View();
            }


            // ========================================================
            // USUARIO INACTIVO
            // ========================================================

            if (!usuarioDb.Estado)
            {
                ModelState.AddModelError(
                    "",
                    "El usuario se encuentra inactivo."
                );

                return View();
            }


            // ========================================================
            // VALIDAR CONTRASEÑA
            // ========================================================

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


            // ========================================================
            // CONTRASEÑA INCORRECTA
            // ========================================================

            if (!passwordCorrecta)
            {
                usuarioDb.IntentosFallidos++;

                await _context.SaveChangesAsync();

                ModelState.AddModelError(
                    "",
                    "La contraseña ingresada es incorrecta."
                );

                return View();
            }


            // ========================================================
            // LOGIN CORRECTO
            // ========================================================

            usuarioDb.IntentosFallidos = 0;

            usuarioDb.UltimoAcceso = DateTime.Now;

            await _context.SaveChangesAsync();


            // ========================================================
            // OBTENER ROL
            // ========================================================

            string nombreRol =
                usuarioDb.IdRolNavigation?.Nombre ?? "Usuario";


            // ========================================================
            // OBTENER DATOS DEL USUARIO
            // ========================================================

            string nombres =
                usuarioDb.Nombres ?? "";

            string apellidos =
                usuarioDb.Apellidos ?? "";

            string nombreUsuario =
                usuarioDb.Usuario1 ?? "";

            string correo =
                usuarioDb.Correo ?? "";

            string nombreCompleto =
                $"{nombres} {apellidos}".Trim();


            // ========================================================
            // SI NO HAY NOMBRE COMPLETO
            // ========================================================

            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                nombreCompleto = nombreUsuario;
            }


            // ========================================================
            // CREAR CLAIMS
            // ========================================================

            var claims = new List<Claim>
            {
                // ID DEL USUARIO

                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuarioDb.IdUsuario.ToString()
                ),


                // NOMBRE DE USUARIO

                new Claim(
                    ClaimTypes.Name,
                    nombreUsuario
                ),


                // NOMBRES

                new Claim(
                    ClaimTypes.GivenName,
                    nombres
                ),


                // APELLIDOS

                new Claim(
                    ClaimTypes.Surname,
                    apellidos
                ),


                // NOMBRE COMPLETO

                new Claim(
                    "NombreCompleto",
                    nombreCompleto
                ),


                // CORREO

                new Claim(
                    ClaimTypes.Email,
                    correo
                ),


                // ROL

                new Claim(
                    ClaimTypes.Role,
                    nombreRol
                )
            };


            // ========================================================
            // CREAR IDENTIDAD
            // ========================================================

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );


            // ========================================================
            // CREAR PRINCIPAL
            // ========================================================

            var principal =
                new ClaimsPrincipal(identity);


            // ========================================================
            // CREAR SESIÓN
            // ========================================================

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true
                }
            );


            // ========================================================
            // CAMBIO OBLIGATORIO DE CONTRASEÑA
            // ========================================================

            if (usuarioDb.DebeCambiarPassword)
            {
                return RedirectToAction(
                    nameof(CambiarPassword),
                    "Account"
                );
            }


            // ========================================================
            // RETURN URL
            // ========================================================

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }


            // ========================================================
            // IR AL INICIO
            // ========================================================

            return RedirectToAction(
                "Index",
                "Home"
            );
        }


        // ============================================================
        // LOGOUT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                nameof(Login)
            );
        }


        // ============================================================
        // ACCESS DENIED
        // ============================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // ============================================================
        // CAMBIAR PASSWORD - GET
        // ============================================================

        [Authorize]
        [HttpGet]
        public IActionResult CambiarPassword()
        {
            return View();
        }


        // ============================================================
        // CAMBIAR PASSWORD - POST
        // ============================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(
            string? passwordActual,
            string? nuevaPassword,
            string? confirmarPassword)
        {
            // ========================================================
            // VALIDAR PASSWORD ACTUAL
            // ========================================================

            if (string.IsNullOrWhiteSpace(passwordActual))
            {
                ModelState.AddModelError(
                    "passwordActual",
                    "Debe ingresar su contraseña actual."
                );
            }


            // ========================================================
            // VALIDAR NUEVA PASSWORD
            // ========================================================

            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                ModelState.AddModelError(
                    "nuevaPassword",
                    "Debe ingresar una nueva contraseña."
                );
            }


            // ========================================================
            // VALIDAR CONFIRMACIÓN
            // ========================================================

            if (string.IsNullOrWhiteSpace(confirmarPassword))
            {
                ModelState.AddModelError(
                    "confirmarPassword",
                    "Debe confirmar la nueva contraseña."
                );
            }


            // ========================================================
            // VALIDAR QUE COINCIDAN
            // ========================================================

            if (!string.IsNullOrWhiteSpace(nuevaPassword) &&
                !string.IsNullOrWhiteSpace(confirmarPassword) &&
                nuevaPassword != confirmarPassword)
            {
                ModelState.AddModelError(
                    "confirmarPassword",
                    "Las contraseñas no coinciden."
                );
            }


            // ========================================================
            // SI HAY ERRORES
            // ========================================================

            if (!ModelState.IsValid)
            {
                return View();
            }


            // ========================================================
            // OBTENER ID DEL USUARIO ACTUAL
            // ========================================================

            var claimId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            if (!int.TryParse(
                claimId,
                out int idUsuario))
            {
                return RedirectToAction(
                    nameof(Login)
                );
            }


            // ========================================================
            // BUSCAR USUARIO
            // ========================================================

            var usuarioDb =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        u => u.IdUsuario == idUsuario
                    );


            // ========================================================
            // USUARIO NO EXISTE
            // ========================================================

            if (usuarioDb == null)
            {
                return NotFound();
            }


            // ========================================================
            // VALIDAR PASSWORD ACTUAL
            // ========================================================

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


            // ========================================================
            // PASSWORD ACTUAL INCORRECTA
            // ========================================================

            if (!passwordCorrecta)
            {
                ModelState.AddModelError(
                    "passwordActual",
                    "La contraseña actual es incorrecta."
                );

                return View();
            }


            // ========================================================
            // EVITAR MISMA PASSWORD
            // ========================================================

            bool mismaPassword = false;

            try
            {
                mismaPassword =
                    BCrypt.Net.BCrypt.Verify(
                        nuevaPassword,
                        usuarioDb.PasswordHash
                    );
            }
            catch
            {
                mismaPassword = false;
            }


            if (mismaPassword)
            {
                ModelState.AddModelError(
                    "nuevaPassword",
                    "La nueva contraseña debe ser diferente."
                );

                return View();
            }


            // ========================================================
            // GENERAR NUEVO HASH
            // ========================================================

            usuarioDb.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    nuevaPassword
                );


            // ========================================================
            // ACTUALIZAR ESTADO DE CONTRASEÑA
            // ========================================================

            usuarioDb.DebeCambiarPassword = false;

            usuarioDb.FechaCambioPassword =
                DateTime.Now;

            usuarioDb.IntentosFallidos = 0;


            // ========================================================
            // GUARDAR CAMBIOS
            // ========================================================

            await _context.SaveChangesAsync();


            // ========================================================
            // MENSAJE
            // ========================================================

            TempData["Mensaje"] =
                "Contraseña actualizada correctamente.";


            // ========================================================
            // VOLVER AL INICIO
            // ========================================================

            return RedirectToAction(
                "Index",
                "Home"
            );
        }
    }
}