using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly RestauranteContext _context;

        public UsuariosController(RestauranteContext context)
        {
            _context = context;
        }

        // GET: /Usuarios
        public async Task<IActionResult> Index(string? buscar)
        {
            var consulta = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                consulta = consulta.Where(u =>
                    u.Nombres.Contains(buscar) ||
                    u.Apellidos.Contains(buscar) ||
                    u.Correo.Contains(buscar) ||
                    u.Usuario1.Contains(buscar) ||
                    u.IdRolNavigation.Nombre.Contains(buscar));
            }

            var usuarios = await consulta
                .OrderBy(u => u.Nombres)
                .ThenBy(u => u.Apellidos)
                .ToListAsync();

            ViewBag.Buscar = buscar;

            var idUsuarioActual = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            ViewBag.IdUsuarioActual = idUsuarioActual;

            return View(usuarios);
        }


        // GET: /Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            var idUsuarioActual = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            ViewBag.IdUsuarioActual = idUsuarioActual;

            return View(usuario);
        }


        // GET: /Usuarios/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarRolesAsync();

            return View();
        }


        // POST: /Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Usuario usuario,
            string password)
        {
            ModelState.Remove(nameof(Usuario.PasswordHash));

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "Password",
                    "Debe ingresar una contraseña."
                );
            }

            if (!string.IsNullOrWhiteSpace(usuario.Usuario1))
            {
                bool usuarioExiste = await _context.Usuarios
                    .AnyAsync(u =>
                        u.Usuario1 == usuario.Usuario1.Trim());

                if (usuarioExiste)
                {
                    ModelState.AddModelError(
                        nameof(usuario.Usuario1),
                        "El nombre de usuario ya está registrado."
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(usuario.Correo))
            {
                bool correoExiste = await _context.Usuarios
                    .AnyAsync(u =>
                        u.Correo == usuario.Correo.Trim());

                if (correoExiste)
                {
                    ModelState.AddModelError(
                        nameof(usuario.Correo),
                        "El correo electrónico ya está registrado."
                    );
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarRolesAsync(usuario.IdRol);

                return View(usuario);
            }

            usuario.Nombres = usuario.Nombres.Trim();
            usuario.Apellidos = usuario.Apellidos.Trim();
            usuario.Correo = usuario.Correo.Trim();
            usuario.Usuario1 = usuario.Usuario1.Trim();

            if (!string.IsNullOrWhiteSpace(usuario.Telefono))
            {
                usuario.Telefono = usuario.Telefono.Trim();
            }

            usuario.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(password);

            usuario.Estado = true;
            usuario.FechaRegistro = DateTime.Now;
            usuario.IntentosFallidos = 0;
            usuario.DebeCambiarPassword = true;
            usuario.UltimoAcceso = null;
            usuario.FechaCambioPassword = null;

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Usuario creado correctamente. El usuario deberá cambiar su contraseña en el primer inicio de sesión.";

            return RedirectToAction(nameof(Index));
        }


        // GET: /Usuarios/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            await CargarRolesAsync(usuario.IdRol);

            var idUsuarioActual = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            ViewBag.IdUsuarioActual = idUsuarioActual;

            return View(usuario);
        }


        // POST: /Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Usuario modelo)
        {
            if (id != modelo.IdUsuario)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Usuario.PasswordHash));
            ModelState.Remove(nameof(Usuario.FechaRegistro));
            ModelState.Remove(nameof(Usuario.UltimoAcceso));
            ModelState.Remove(nameof(Usuario.FechaCambioPassword));
            ModelState.Remove(nameof(Usuario.IntentosFallidos));

            var usuarioDb = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuarioDb == null)
            {
                return NotFound();
            }


            // =====================================================
            // USUARIO ACTUAL
            // =====================================================

            var idUsuarioActual = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            bool esUsuarioActual =
                int.TryParse(idUsuarioActual, out int idActual) &&
                idActual == id;


            // =====================================================
            // VERIFICAR ROL SELECCIONADO
            // =====================================================

            var rolSeleccionado = await _context.Rols
                .FirstOrDefaultAsync(r => r.IdRol == modelo.IdRol);

            if (rolSeleccionado == null)
            {
                ModelState.AddModelError(
                    nameof(modelo.IdRol),
                    "El rol seleccionado no es válido."
                );
            }


            // UN ADMINISTRADOR NO PUEDE CAMBIARSE SU PROPIO ROL

            if (esUsuarioActual &&
                usuarioDb.IdRolNavigation.Nombre == "Administrador" &&
                rolSeleccionado != null &&
                rolSeleccionado.Nombre != "Administrador")
            {
                ModelState.AddModelError(
                    nameof(modelo.IdRol),
                    "No puede cambiar su propio rol de Administrador."
                );
            }


            // UN ADMINISTRADOR NO PUEDE DESACTIVARSE A SÍ MISMO

            if (esUsuarioActual && !modelo.Estado)
            {
                ModelState.AddModelError(
                    nameof(modelo.Estado),
                    "No puede desactivar su propio usuario."
                );
            }


            // NO PERMITIR QUE EL SISTEMA SE QUEDE SIN ADMINISTRADORES

            bool usuarioEsAdministrador =
                usuarioDb.IdRolNavigation.Nombre == "Administrador";

            bool nuevoRolEsAdministrador =
                rolSeleccionado != null &&
                rolSeleccionado.Nombre == "Administrador";


            if (usuarioEsAdministrador &&
                !nuevoRolEsAdministrador &&
                usuarioDb.Estado)
            {
                int administradoresActivos =
                    await _context.Usuarios
                        .Include(u => u.IdRolNavigation)
                        .CountAsync(u =>
                            u.Estado &&
                            u.IdRolNavigation.Nombre == "Administrador");

                if (administradoresActivos <= 1)
                {
                    ModelState.AddModelError(
                        nameof(modelo.IdRol),
                        "No puede quitar el rol de Administrador al último administrador activo del sistema."
                    );
                }
            }


            // NO PERMITIR DESACTIVAR AL ÚLTIMO ADMINISTRADOR

            if (usuarioEsAdministrador &&
                usuarioDb.Estado &&
                !modelo.Estado)
            {
                int administradoresActivos =
                    await _context.Usuarios
                        .Include(u => u.IdRolNavigation)
                        .CountAsync(u =>
                            u.Estado &&
                            u.IdRolNavigation.Nombre == "Administrador");

                if (administradoresActivos <= 1)
                {
                    ModelState.AddModelError(
                        nameof(modelo.Estado),
                        "No puede desactivar al último administrador activo del sistema."
                    );
                }
            }


            // COMPROBAR USUARIO DUPLICADO

            if (!string.IsNullOrWhiteSpace(modelo.Usuario1))
            {
                string nombreUsuario =
                    modelo.Usuario1.Trim();

                bool usuarioExiste = await _context.Usuarios
                    .AnyAsync(u =>
                        u.IdUsuario != id &&
                        u.Usuario1 == nombreUsuario);

                if (usuarioExiste)
                {
                    ModelState.AddModelError(
                        nameof(modelo.Usuario1),
                        "El nombre de usuario ya está registrado."
                    );
                }
            }


            // COMPROBAR CORREO DUPLICADO

            if (!string.IsNullOrWhiteSpace(modelo.Correo))
            {
                string correo =
                    modelo.Correo.Trim();

                bool correoExiste = await _context.Usuarios
                    .AnyAsync(u =>
                        u.IdUsuario != id &&
                        u.Correo == correo);

                if (correoExiste)
                {
                    ModelState.AddModelError(
                        nameof(modelo.Correo),
                        "El correo electrónico ya está registrado."
                    );
                }
            }


           
            // SI HAY ERRORES

            if (!ModelState.IsValid)
            {
                await CargarRolesAsync(modelo.IdRol);

                ViewBag.IdUsuarioActual = idUsuarioActual;

                return View(modelo);
            }


            // ACTUALIZAR DATOS

            usuarioDb.Nombres =
                modelo.Nombres.Trim();

            usuarioDb.Apellidos =
                modelo.Apellidos.Trim();

            usuarioDb.Correo =
                modelo.Correo.Trim();

            usuarioDb.Usuario1 =
                modelo.Usuario1.Trim();

            usuarioDb.IdRol =
                modelo.IdRol;

            usuarioDb.Estado =
                modelo.Estado;


            if (!string.IsNullOrWhiteSpace(modelo.Telefono))
            {
                usuarioDb.Telefono =
                    modelo.Telefono.Trim();
            }
            else
            {
                usuarioDb.Telefono = null;
            }


            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Usuario actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // POST: /Usuarios/Desactivar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }


            // NO PUEDE DESACTIVARSE A SÍ MISMO

            var idUsuarioActual = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (int.TryParse(idUsuarioActual, out int idActual) &&
                idActual == id)
            {
                TempData["Error"] =
                    "No puede desactivar su propio usuario.";

                return RedirectToAction(nameof(Index));
            }


            // NO DESACTIVAR AL ÚLTIMO ADMINISTRADOR

            if (usuario.IdRolNavigation.Nombre == "Administrador" &&
                usuario.Estado)
            {
                int administradoresActivos =
                    await _context.Usuarios
                        .Include(u => u.IdRolNavigation)
                        .CountAsync(u =>
                            u.Estado &&
                            u.IdRolNavigation.Nombre == "Administrador");

                if (administradoresActivos <= 1)
                {
                    TempData["Error"] =
                        "No puede desactivar al último administrador activo del sistema.";

                    return RedirectToAction(nameof(Index));
                }
            }


            usuario.Estado = false;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Usuario desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // POST: /Usuarios/Activar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Estado = true;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Usuario activado correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // POST: /Usuarios/RestablecerPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerPassword(
            int id,
            string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword))
            {
                TempData["Error"] =
                    "Debe ingresar una nueva contraseña.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            usuario.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(nuevaPassword);

            usuario.DebeCambiarPassword = true;
            usuario.FechaCambioPassword = null;
            usuario.IntentosFallidos = 0;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Contraseña restablecida correctamente. El usuario deberá cambiarla al iniciar sesión.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // CARGAR ROLES

        private async Task CargarRolesAsync(
            int? idRolSeleccionado = null)
        {
            var roles = await _context.Rols
                .Where(r => r.Estado)
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            ViewBag.Roles = roles;
            ViewBag.IdRolSeleccionado = idRolSeleccionado;
        }
    }
}