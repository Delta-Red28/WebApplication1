using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Claims;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly RestauranteContext _context;

        public PerfilController(RestauranteContext context)
        {
            _context = context;
        }


        // ============================================================
        // PERFIL - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ========================================================
            // OBTENER ID DEL USUARIO AUTENTICADO
            // ========================================================

            var idUsuario = ObtenerIdUsuarioActual();

            if (idUsuario == null)
            {
                return Unauthorized();
            }


            // ========================================================
            // BUSCAR USUARIO
            // ========================================================

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == idUsuario.Value
                );


            // ========================================================
            // USUARIO NO EXISTE
            // ========================================================

            if (usuario == null)
            {
                return NotFound();
            }


            // ========================================================
            // MOSTRAR PERFIL
            // ========================================================

            return View(usuario);
        }


        // ============================================================
        // EDITAR PERFIL - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            // ========================================================
            // OBTENER ID DEL USUARIO AUTENTICADO
            // ========================================================

            var idUsuario = ObtenerIdUsuarioActual();

            if (idUsuario == null)
            {
                return Unauthorized();
            }


            // ========================================================
            // BUSCAR USUARIO
            // ========================================================

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == idUsuario.Value
                );


            // ========================================================
            // USUARIO NO EXISTE
            // ========================================================

            if (usuario == null)
            {
                return NotFound();
            }


            // ========================================================
            // MOSTRAR FORMULARIO
            // ========================================================

            return View(usuario);
        }


        // ============================================================
        // EDITAR PERFIL - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            string? nombres,
            string? apellidos,
            string? correo,
            string? telefono)
        {
            // ========================================================
            // OBTENER ID DEL USUARIO AUTENTICADO
            // ========================================================

            var idUsuario = ObtenerIdUsuarioActual();

            if (idUsuario == null)
            {
                return Unauthorized();
            }


            // ========================================================
            // LIMPIAR DATOS
            // ========================================================

            nombres = nombres?.Trim();
            apellidos = apellidos?.Trim();
            correo = correo?.Trim();
            telefono = telefono?.Trim();


            // ========================================================
            // VALIDAR NOMBRES
            // ========================================================

            if (string.IsNullOrWhiteSpace(nombres))
            {
                ModelState.AddModelError(
                    "Nombres",
                    "Debe ingresar sus nombres."
                );
            }
            else if (nombres.Length < 2)
            {
                ModelState.AddModelError(
                    "Nombres",
                    "Los nombres deben tener al menos 2 caracteres."
                );
            }


            // ========================================================
            // VALIDAR APELLIDOS
            // ========================================================

            if (string.IsNullOrWhiteSpace(apellidos))
            {
                ModelState.AddModelError(
                    "Apellidos",
                    "Debe ingresar sus apellidos."
                );
            }
            else if (apellidos.Length < 2)
            {
                ModelState.AddModelError(
                    "Apellidos",
                    "Los apellidos deben tener al menos 2 caracteres."
                );
            }


            // ========================================================
            // VALIDAR CORREO
            // ========================================================

            if (string.IsNullOrWhiteSpace(correo))
            {
                ModelState.AddModelError(
                    "Correo",
                    "Debe ingresar su correo electrónico."
                );
            }
            else
            {
                try
                {
                    var mailAddress = new MailAddress(correo);

                    if (!mailAddress.Address.Equals(
                            correo,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(
                            "Correo",
                            "Ingrese un correo electrónico válido."
                        );
                    }
                }
                catch
                {
                    ModelState.AddModelError(
                        "Correo",
                        "Ingrese un correo electrónico válido."
                    );
                }
            }


            // ========================================================
            // VALIDAR TELÉFONO
            // ========================================================

            if (!string.IsNullOrWhiteSpace(telefono))
            {
                if (telefono.Length > 20)
                {
                    ModelState.AddModelError(
                        "Telefono",
                        "El teléfono no puede superar los 20 caracteres."
                    );
                }
            }


            // ========================================================
            // BUSCAR USUARIO ACTUAL
            // ========================================================

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(
                    u => u.IdUsuario == idUsuario.Value
                );


            // ========================================================
            // USUARIO NO EXISTE
            // ========================================================

            if (usuario == null)
            {
                return NotFound();
            }


            // ========================================================
            // COMPROBAR CORREO DUPLICADO
            // ========================================================

            if (!string.IsNullOrWhiteSpace(correo))
            {
                bool correoExiste = await _context.Usuarios
                    .AnyAsync(u =>
                        u.IdUsuario != usuario.IdUsuario &&
                        u.Correo == correo
                    );

                if (correoExiste)
                {
                    ModelState.AddModelError(
                        "Correo",
                        "El correo electrónico ya está registrado por otro usuario."
                    );
                }
            }


            // ========================================================
            // SI HAY ERRORES
            // ========================================================

            if (!ModelState.IsValid)
            {
                return View(usuario);
            }


            // ========================================================
            // ACTUALIZAR ÚNICAMENTE DATOS PERSONALES
            // ========================================================

            usuario.Nombres = nombres!;
            usuario.Apellidos = apellidos!;
            usuario.Correo = correo!;


            // ========================================================
            // TELÉFONO
            // ========================================================

            if (string.IsNullOrWhiteSpace(telefono))
            {
                usuario.Telefono = null;
            }
            else
            {
                usuario.Telefono = telefono;
            }


            // ========================================================
            // GUARDAR CAMBIOS
            // ========================================================

            await _context.SaveChangesAsync();


            // ========================================================
            // ACTUALIZAR CLAIMS DE LA SESIÓN
            // ========================================================

            await ActualizarClaimsAsync(usuario);


            // ========================================================
            // MENSAJE
            // ========================================================

            TempData["Mensaje"] =
                "Tu información personal fue actualizada correctamente.";


            // ========================================================
            // VOLVER AL PERFIL
            // ========================================================

            return RedirectToAction(
                nameof(Index)
            );
        }


        // ============================================================
        // OBTENER ID DEL USUARIO ACTUAL
        // ============================================================

        private int? ObtenerIdUsuarioActual()
        {
            var claimId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (int.TryParse(
                claimId,
                out int idUsuario))
            {
                return idUsuario;
            }

            return null;
        }


        // ============================================================
        // ACTUALIZAR CLAIMS DE LA SESIÓN
        // ============================================================

        private async Task ActualizarClaimsAsync(
            Usuario usuario)
        {
            // ========================================================
            // OBTENER DATOS ACTUALIZADOS
            // ========================================================

            string nombres =
                usuario.Nombres ?? "";

            string apellidos =
                usuario.Apellidos ?? "";

            string nombreUsuario =
                usuario.Usuario1 ?? "";

            string correo =
                usuario.Correo ?? "";

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
            // OBTENER ROL
            // ========================================================

            string nombreRol =
                usuario.IdRolNavigation?.Nombre ?? "Usuario";


            // ========================================================
            // CREAR NUEVOS CLAIMS
            // ========================================================

            var claims = new List<Claim>
            {
                // ID

                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()
                ),


                // USUARIO

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
            // CREAR NUEVA IDENTIDAD
            // ========================================================

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );


            // ========================================================
            // CREAR NUEVO PRINCIPAL
            // ========================================================

            var principal =
                new ClaimsPrincipal(identity);


            // ========================================================
            // REGENERAR COOKIE DE AUTENTICACIÓN
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
        }
    }
}