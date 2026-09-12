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


        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public AccountController(RestauranteContext context)
        {
            _context = context;
        }


        // ============================================================
        // GUARDAR CAMBIOS
        // ============================================================

        private async Task<bool> GuardarCambiosAsync()
        {
            try
            {
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                Exception error = ex;

                while (error.InnerException != null)
                {
                    error = error.InnerException;
                }

                Console.WriteLine();
                Console.WriteLine("==========================================");
                Console.WriteLine("ERROR AL GUARDAR EN LA BASE DE DATOS");
                Console.WriteLine("==========================================");
                Console.WriteLine(error.Message);
                Console.WriteLine("==========================================");
                Console.WriteLine();

                ModelState.AddModelError(
                    "",
                    "No fue posible guardar los cambios en la base de datos."
                );

                return false;
            }
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
            // DATOS VALIDADOS
            // ========================================================

            string usuarioIngresado = usuario?.Trim() ?? "";
            string passwordIngresada = password ?? "";


            // ========================================================
            // BUSCAR USUARIO
            // ========================================================

            var usuarioDb = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(
                    u => u.Usuario1 == usuarioIngresado
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
                if (!string.IsNullOrWhiteSpace(
                    usuarioDb.PasswordHash))
                {
                    passwordCorrecta =
                        BCrypt.Net.BCrypt.Verify(
                            passwordIngresada,
                            usuarioDb.PasswordHash
                        );
                }
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

                if (!await GuardarCambiosAsync())
                {
                    return View();
                }

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


            // ========================================================
            // GUARDAR ÚLTIMO ACCESO
            // ========================================================

            if (!await GuardarCambiosAsync())
            {
                return View();
            }


            // ========================================================
            // OBTENER ROL
            // ========================================================

            string nombreRol =
                usuarioDb.IdRolNavigation?.Nombre
                ?? "Usuario";


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


            // ========================================================
            // NOMBRE COMPLETO
            // ========================================================

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
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuarioDb.IdUsuario.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    nombreUsuario
                ),

                new Claim(
                    ClaimTypes.GivenName,
                    nombres
                ),

                new Claim(
                    ClaimTypes.Surname,
                    apellidos
                ),

                new Claim(
                    "NombreCompleto",
                    nombreCompleto
                ),

                new Claim(
                    ClaimTypes.Email,
                    correo
                ),

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
        public async Task<IActionResult> CambiarPassword()
        {
            // ========================================================
            // OBTENER ID DEL USUARIO ACTUAL
            // ========================================================

            var claimId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            // ========================================================
            // VALIDAR ID
            // ========================================================

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
            // INDICAR SI CAMBIO ES OBLIGATORIO
            // ========================================================

            ViewBag.DebeCambiarPassword =
                usuarioDb.DebeCambiarPassword;


            // ========================================================
            // MOSTRAR VISTA
            // ========================================================

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
                    "PasswordActual",
                    "Debe ingresar su contraseña actual."
                );
            }


            // ========================================================
            // VALIDAR NUEVA PASSWORD
            // ========================================================

            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                ModelState.AddModelError(
                    "NuevaPassword",
                    "Debe ingresar una nueva contraseña."
                );
            }


            // ========================================================
            // VALIDAR CONFIRMACIÓN
            // ========================================================

            if (string.IsNullOrWhiteSpace(confirmarPassword))
            {
                ModelState.AddModelError(
                    "ConfirmarPassword",
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
                    "ConfirmarPassword",
                    "Las contraseñas no coinciden."
                );
            }


            // ========================================================
            // VALIDAR LONGITUD
            // ========================================================

            if (!string.IsNullOrWhiteSpace(nuevaPassword) &&
                nuevaPassword.Length < 8)
            {
                ModelState.AddModelError(
                    "NuevaPassword",
                    "La nueva contraseña debe tener al menos 8 caracteres."
                );
            }


            // ========================================================
            // SI HAY ERRORES
            // ========================================================

            if (!ModelState.IsValid)
            {
                var claimIdError =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    );

                if (int.TryParse(
                    claimIdError,
                    out int idUsuarioError))
                {
                    var usuarioError =
                        await _context.Usuarios
                            .FirstOrDefaultAsync(
                                u => u.IdUsuario == idUsuarioError
                            );

                    if (usuarioError != null)
                    {
                        ViewBag.DebeCambiarPassword =
                            usuarioError.DebeCambiarPassword;
                    }
                }

                return View();
            }


            // ========================================================
            // DATOS VALIDADOS
            // ========================================================

            string passwordActualIngresada =
                passwordActual ?? "";

            string nuevaPasswordIngresada =
                nuevaPassword ?? "";


            // ========================================================
            // OBTENER ID DEL USUARIO
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
                if (!string.IsNullOrWhiteSpace(
                    usuarioDb.PasswordHash))
                {
                    passwordCorrecta =
                        BCrypt.Net.BCrypt.Verify(
                            passwordActualIngresada,
                            usuarioDb.PasswordHash
                        );
                }
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
                    "PasswordActual",
                    "La contraseña actual es incorrecta."
                );

                ViewBag.DebeCambiarPassword =
                    usuarioDb.DebeCambiarPassword;

                return View();
            }


            // ========================================================
            // EVITAR MISMA PASSWORD
            // ========================================================

            bool mismaPassword = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(
                    usuarioDb.PasswordHash))
                {
                    mismaPassword =
                        BCrypt.Net.BCrypt.Verify(
                            nuevaPasswordIngresada,
                            usuarioDb.PasswordHash
                        );
                }
            }
            catch
            {
                mismaPassword = false;
            }


            // ========================================================
            // NUEVA PASSWORD IGUAL A LA ACTUAL
            // ========================================================

            if (mismaPassword)
            {
                ModelState.AddModelError(
                    "NuevaPassword",
                    "La nueva contraseña debe ser diferente de la actual."
                );

                ViewBag.DebeCambiarPassword =
                    usuarioDb.DebeCambiarPassword;

                return View();
            }


            // ========================================================
            // GENERAR NUEVO HASH
            // ========================================================

            usuarioDb.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    nuevaPasswordIngresada
                );


            // ========================================================
            // ACTUALIZAR ESTADO
            // ========================================================

            usuarioDb.DebeCambiarPassword = false;


            // ========================================================
            // FECHA DEL CAMBIO
            // ========================================================

            usuarioDb.FechaCambioPassword =
                DateTime.Now;


            // ========================================================
            // REINICIAR INTENTOS FALLIDOS
            // ========================================================

            usuarioDb.IntentosFallidos = 0;


            // ========================================================
            // GUARDAR CAMBIOS
            // ========================================================

            if (!await GuardarCambiosAsync())
            {
                ViewBag.DebeCambiarPassword =
                    usuarioDb.DebeCambiarPassword;

                return View();
            }


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