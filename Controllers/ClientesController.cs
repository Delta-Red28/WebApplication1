using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Administrador,Gerente,Cajero,Mesero")]
    public class ClientesController : Controller
    {
        private readonly RestauranteContext _context;

        public ClientesController(RestauranteContext context)
        {
            _context = context;
        }


        // INDEX
        // Administrador, Gerente, Cajero y Mesero pueden consultar
       
        // GET: /Clientes
        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            var consulta = _context.Clientes
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                consulta = consulta.Where(c =>
                    c.Nombres.Contains(buscar) ||
                    c.Apellidos.Contains(buscar) ||
                    (c.Cedula != null && c.Cedula.Contains(buscar)) ||
                    c.Telefono.Contains(buscar) ||
                    (c.Correo != null && c.Correo.Contains(buscar)));
            }

            var clientes = await consulta
                .OrderBy(c => c.Nombres)
                .ThenBy(c => c.Apellidos)
                .ToListAsync();

            ViewBag.Buscar = buscar;

            return View(clientes);
        }


        // DETAILS
        // Administrador, Gerente, Cajero y Mesero pueden consultar
 

        // GET: /Clientes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }


        // CREATE
        // Solo Administrador y Gerente

        // GET: /Clientes/Create
        [Authorize(Roles = "Administrador,Gerente")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // POST: /Clientes/Create
        [Authorize(Roles = "Administrador,Gerente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {

            // LIMPIAR ESPACIOS

            if (!string.IsNullOrWhiteSpace(cliente.Nombres))
            {
                cliente.Nombres = cliente.Nombres.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cliente.Apellidos))
            {
                cliente.Apellidos = cliente.Apellidos.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cliente.Cedula))
            {
                cliente.Cedula = cliente.Cedula.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cliente.Telefono))
            {
                cliente.Telefono = cliente.Telefono.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cliente.Correo))
            {
                cliente.Correo = cliente.Correo.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cliente.Direccion))
            {
                cliente.Direccion = cliente.Direccion.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cliente.Sexo))
            {
                cliente.Sexo = cliente.Sexo.Trim().ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(cliente.Observaciones))
            {
                cliente.Observaciones = cliente.Observaciones.Trim();
            }



            // VALIDAR CÉDULA DUPLICADA

            if (!string.IsNullOrWhiteSpace(cliente.Cedula))
            {
                bool cedulaExiste = await _context.Clientes
                    .AnyAsync(c =>
                        c.Cedula == cliente.Cedula);

                if (cedulaExiste)
                {
                    ModelState.AddModelError(
                        nameof(cliente.Cedula),
                        "La cédula ya está registrada."
                    );
                }
            }


            // VALIDAR CORREO DUPLICADO


            if (!string.IsNullOrWhiteSpace(cliente.Correo))
            {
                bool correoExiste = await _context.Clientes
                    .AnyAsync(c =>
                        c.Correo == cliente.Correo);

                if (correoExiste)
                {
                    ModelState.AddModelError(
                        nameof(cliente.Correo),
                        "El correo electrónico ya está registrado."
                    );
                }
            }


            // VALIDAR MODELO

            if (!ModelState.IsValid)
            {
                return View(cliente);
            }


            // DATOS AUTOMÁTICOS

            cliente.Estado = true;

            cliente.FechaRegistro = DateTime.Now;


            // GUARDAR

            _context.Clientes.Add(cliente);

            await _context.SaveChangesAsync();


            TempData["Mensaje"] =
                "Cliente creado correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // EDIT
        // Solo Administrador y Gerente

        // GET: /Clientes/Edit/5
        [Authorize(Roles = "Administrador,Gerente")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }


        // POST: /Clientes/Edit/5
        [Authorize(Roles = "Administrador,Gerente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Cliente modelo)
        {
            if (id != modelo.IdCliente)
            {
                return NotFound();
            }


            // BUSCAR CLIENTE ORIGINAL

            var clienteDb = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.IdCliente == id);

            if (clienteDb == null)
            {
                return NotFound();
            }


            // LIMPIAR ESPACIOS

            modelo.Nombres =
                modelo.Nombres?.Trim() ?? "";

            modelo.Apellidos =
                modelo.Apellidos?.Trim() ?? "";


            if (!string.IsNullOrWhiteSpace(modelo.Cedula))
            {
                modelo.Cedula =
                    modelo.Cedula.Trim();
            }


            if (!string.IsNullOrWhiteSpace(modelo.Telefono))
            {
                modelo.Telefono =
                    modelo.Telefono.Trim();
            }


            if (!string.IsNullOrWhiteSpace(modelo.Correo))
            {
                modelo.Correo =
                    modelo.Correo.Trim();
            }


            if (!string.IsNullOrWhiteSpace(modelo.Direccion))
            {
                modelo.Direccion =
                    modelo.Direccion.Trim();
            }


            if (!string.IsNullOrWhiteSpace(modelo.Sexo))
            {
                modelo.Sexo =
                    modelo.Sexo.Trim().ToUpper();
            }


            if (!string.IsNullOrWhiteSpace(modelo.Observaciones))
            {
                modelo.Observaciones =
                    modelo.Observaciones.Trim();
            }


            // VALIDAR CÉDULA DUPLICADA

            if (!string.IsNullOrWhiteSpace(modelo.Cedula))
            {
                bool cedulaExiste =
                    await _context.Clientes.AnyAsync(c =>
                        c.IdCliente != id &&
                        c.Cedula == modelo.Cedula);

                if (cedulaExiste)
                {
                    ModelState.AddModelError(
                        nameof(modelo.Cedula),
                        "La cédula ya está registrada."
                    );
                }
            }


            // VALIDAR CORREO DUPLICADO

            if (!string.IsNullOrWhiteSpace(modelo.Correo))
            {
                bool correoExiste =
                    await _context.Clientes.AnyAsync(c =>
                        c.IdCliente != id &&
                        c.Correo == modelo.Correo);

                if (correoExiste)
                {
                    ModelState.AddModelError(
                        nameof(modelo.Correo),
                        "El correo electrónico ya está registrado."
                    );
                }
            }


            // VALIDAR MODELO

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }


            // ACTUALIZAR DATOS

            clienteDb.Nombres =
                modelo.Nombres;

            clienteDb.Apellidos =
                modelo.Apellidos;

            clienteDb.Cedula =
                modelo.Cedula;

            clienteDb.Telefono =
                modelo.Telefono;

            clienteDb.Correo =
                modelo.Correo;

            clienteDb.Direccion =
                modelo.Direccion;

            clienteDb.FechaNacimiento =
                modelo.FechaNacimiento;

            clienteDb.Sexo =
                modelo.Sexo;

            clienteDb.Observaciones =
                modelo.Observaciones;


            // NO MODIFICAMOS:
            //
            // IdCliente
            // Estado
            // FechaRegistro


            await _context.SaveChangesAsync();


            TempData["Mensaje"] =
                "Cliente actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // DESACTIVAR
        // Solo Administrador y Gerente

        // POST: /Clientes/Desactivar/5
        [Authorize(Roles = "Administrador,Gerente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }


            if (!cliente.Estado)
            {
                TempData["Error"] =
                    "El cliente ya se encuentra inactivo.";

                return RedirectToAction(nameof(Index));
            }


            cliente.Estado = false;

            await _context.SaveChangesAsync();


            TempData["Mensaje"] =
                "Cliente desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // ACTIVAR
        // Solo Administrador y Gerente

        // POST: /Clientes/Activar/5
        [Authorize(Roles = "Administrador,Gerente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }


            if (cliente.Estado)
            {
                TempData["Error"] =
                    "El cliente ya se encuentra activo.";

                return RedirectToAction(nameof(Index));
            }


            cliente.Estado = true;

            await _context.SaveChangesAsync();


            TempData["Mensaje"] =
                "Cliente activado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}