using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ReservacionController : Controller
    {
        private readonly RestauranteContext _context;

        public ReservacionController(RestauranteContext context)
        {
            _context = context;
        }


        // ============================================================
        // GET: Reservacion
        // ============================================================

        public async Task<IActionResult> Index()
        {
            var reservaciones = await _context.Reservacions
                .AsNoTracking()
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdMesaNavigation)
                .Include(r => r.IdEstadoReservacionNavigation)
                .OrderByDescending(r => r.FechaReserva)
                .ThenBy(r => r.HoraInicio)
                .ToListAsync();

            return View(reservaciones);
        }


        // ============================================================
        // GET: Reservacion/Details/5
        // ============================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservacion = await _context.Reservacions
                .AsNoTracking()
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdMesaNavigation)
                .Include(r => r.IdEstadoReservacionNavigation)
                .FirstOrDefaultAsync(r =>
                    r.IdReservacion == id);

            if (reservacion == null)
            {
                return NotFound();
            }

            return View(reservacion);
        }


        // ============================================================
        // GET: Reservacion/Create
        // ============================================================

        public async Task<IActionResult> Create()
        {
            var reservacion = new Reservacion
            {
                FechaReserva = DateOnly.FromDateTime(DateTime.Today),
                HoraInicio = new TimeOnly(12, 0),
                HoraFin = new TimeOnly(13, 0),
                CantidadPersonas = 1
            };

            await CargarDatosFormulario(reservacion);

            return View(reservacion);
        }


        // ============================================================
        // POST: Reservacion/Create
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservacion reservacion)
        {
            // ========================================================
            // IMPORTANTE
            // ========================================================
            // Las propiedades Navigation no vienen del formulario.
            // Entity Framework las utilizará posteriormente.
            //
            // Por eso no debemos permitir que MVC las valide como
            // campos obligatorios.
            // ========================================================

            ModelState.Remove(nameof(Reservacion.IdClienteNavigation));
            ModelState.Remove(nameof(Reservacion.IdMesaNavigation));
            ModelState.Remove(nameof(Reservacion.IdEstadoReservacionNavigation));


            // ========================================================
            // VALIDAR CLIENTE
            // ========================================================

            if (reservacion.IdCliente <= 0)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.IdCliente),
                    "Debe seleccionar un cliente.");
            }
            else
            {
                var cliente = await _context.Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.IdCliente == reservacion.IdCliente &&
                        c.Estado);

                if (cliente == null)
                {
                    ModelState.AddModelError(
                        nameof(Reservacion.IdCliente),
                        "El cliente seleccionado no existe o está inactivo.");
                }
            }


            // ========================================================
            // VALIDAR MESA
            // ========================================================

            Mesa? mesa = null;

            if (reservacion.IdMesa <= 0)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.IdMesa),
                    "Debe seleccionar una mesa.");
            }
            else
            {
                mesa = await _context.Mesas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m =>
                        m.IdMesa == reservacion.IdMesa &&
                        m.Estado);

                if (mesa == null)
                {
                    ModelState.AddModelError(
                        nameof(Reservacion.IdMesa),
                        "La mesa seleccionada no existe o está inactiva.");
                }
            }


            // ========================================================
            // VALIDAR CANTIDAD DE PERSONAS
            // ========================================================

            if (reservacion.CantidadPersonas <= 0)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.CantidadPersonas),
                    "La cantidad de personas debe ser mayor que cero.");
            }

            if (mesa != null &&
                reservacion.CantidadPersonas > mesa.Capacidad)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.CantidadPersonas),
                    $"La mesa seleccionada tiene una capacidad máxima de {mesa.Capacidad} personas.");
            }


            // ========================================================
            // VALIDAR FECHA
            // ========================================================

            var fechaActual =
                DateOnly.FromDateTime(DateTime.Today);

            if (reservacion.FechaReserva < fechaActual)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.FechaReserva),
                    "No se pueden realizar reservaciones para fechas pasadas.");
            }


            // ========================================================
            // VALIDAR HORARIO
            // ========================================================

            if (reservacion.HoraFin <= reservacion.HoraInicio)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.HoraFin),
                    "La hora de finalización debe ser mayor que la hora de inicio.");
            }


            // ========================================================
            // VALIDAR OBSERVACIÓN
            // ========================================================

            if (!string.IsNullOrWhiteSpace(reservacion.Observacion) &&
                reservacion.Observacion.Length > 300)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.Observacion),
                    "La observación no puede superar los 300 caracteres.");
            }


            // ========================================================
            // VALIDAR HORARIO CONTRA OTRAS RESERVACIONES
            // ========================================================

            if (ModelState.IsValid)
            {
                bool existeConflicto =
                    await ExisteConflictoHorario(
                        reservacion.IdMesa,
                        reservacion.FechaReserva,
                        reservacion.HoraInicio,
                        reservacion.HoraFin);

                if (existeConflicto)
                {
                    ModelState.AddModelError(
                        "",
                        "La mesa seleccionada ya está reservada durante el horario indicado.");
                }
            }


            // ========================================================
            // SI HAY ERRORES
            // ========================================================

            if (!ModelState.IsValid)
            {
                await CargarDatosFormulario(reservacion);

                return View(reservacion);
            }


            // ========================================================
            // ESTADO INICIAL AUTOMÁTICO
            // ========================================================
            //
            // 1 = Pendiente
            //
            // La reservación siempre comienza como Pendiente.
            // El usuario no necesita seleccionarlo manualmente.
            // ========================================================

            const int estadoPendiente = 1;

            var estadoExiste =
                await _context.EstadoReservacions
                    .AsNoTracking()
                    .AnyAsync(e =>
                        e.IdEstadoReservacion == estadoPendiente);

            if (!estadoExiste)
            {
                ModelState.AddModelError(
                    "",
                    "No existe el estado inicial 'Pendiente' en la base de datos.");

                await CargarDatosFormulario(reservacion);

                return View(reservacion);
            }

            reservacion.IdEstadoReservacion =
                estadoPendiente;


            // ========================================================
            // FECHA DE REGISTRO
            // ========================================================

            reservacion.FechaRegistro = DateTime.Now;


            // ========================================================
            // LIMPIAR PROPIEDADES DE NAVEGACIÓN
            // ========================================================

            reservacion.IdClienteNavigation = null!;
            reservacion.IdMesaNavigation = null!;
            reservacion.IdEstadoReservacionNavigation = null!;


            // ========================================================
            // GUARDAR
            // ========================================================

            try
            {
                _context.Reservacions.Add(reservacion);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "La reservación se registró correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(
                    "",
                    "No fue posible guardar la reservación debido a un conflicto con los datos. Inténtelo nuevamente.");

                await CargarDatosFormulario(reservacion);

                return View(reservacion);
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(
                    "",
                    ObtenerMensajeErrorBaseDatos(ex));

                await CargarDatosFormulario(reservacion);

                return View(reservacion);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "No fue posible guardar la reservación. Verifique los datos e inténtelo nuevamente.");

                await CargarDatosFormulario(reservacion);

                return View(reservacion);
            }
        }


        // ============================================================
        // GET: Reservacion/Edit/5
        // ============================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservacion =
                await _context.Reservacions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r =>
                        r.IdReservacion == id);

            if (reservacion == null)
            {
                return NotFound();
            }

            await CargarDatosFormulario(reservacion);

            return View(reservacion);
        }


        // ============================================================
        // POST: Reservacion/Edit/5
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Reservacion reservacion)
        {
            // ========================================================
            // QUITAR VALIDACIÓN DE NAVIGATION PROPERTIES
            // ========================================================

            ModelState.Remove(nameof(Reservacion.IdClienteNavigation));
            ModelState.Remove(nameof(Reservacion.IdMesaNavigation));
            ModelState.Remove(nameof(Reservacion.IdEstadoReservacionNavigation));


            // ========================================================
            // VALIDAR ID
            // ========================================================

            if (id != reservacion.IdReservacion)
            {
                return NotFound();
            }


            // ========================================================
            // BUSCAR REGISTRO ORIGINAL
            // ========================================================

            var existente =
                await _context.Reservacions
                    .FirstOrDefaultAsync(r =>
                        r.IdReservacion == id);

            if (existente == null)
            {
                return NotFound();
            }


            // ========================================================
            // VALIDAR CLIENTE
            // ========================================================

            if (reservacion.IdCliente <= 0)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.IdCliente),
                    "Debe seleccionar un cliente.");
            }
            else
            {
                var cliente =
                    await _context.Clientes
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c =>
                            c.IdCliente == reservacion.IdCliente &&
                            c.Estado);

                if (cliente == null)
                {
                    ModelState.AddModelError(
                        nameof(Reservacion.IdCliente),
                        "El cliente seleccionado no existe o está inactivo.");
                }
            }


            // ========================================================
            // VALIDAR MESA
            // ========================================================

            Mesa? mesa = null;

            if (reservacion.IdMesa <= 0)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.IdMesa),
                    "Debe seleccionar una mesa.");
            }
            else
            {
                mesa =
                    await _context.Mesas
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m =>
                            m.IdMesa == reservacion.IdMesa &&
                            m.Estado);

                if (mesa == null)
                {
                    ModelState.AddModelError(
                        nameof(Reservacion.IdMesa),
                        "La mesa seleccionada no existe o está inactiva.");
                }
            }


            // ========================================================
            // VALIDAR PERSONAS
            // ========================================================

            if (reservacion.CantidadPersonas <= 0)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.CantidadPersonas),
                    "La cantidad de personas debe ser mayor que cero.");
            }

            if (mesa != null &&
                reservacion.CantidadPersonas > mesa.Capacidad)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.CantidadPersonas),
                    $"La mesa seleccionada tiene una capacidad máxima de {mesa.Capacidad} personas.");
            }


            // ========================================================
            // VALIDAR FECHA
            // ========================================================

            var fechaActual =
                DateOnly.FromDateTime(DateTime.Today);

            if (reservacion.FechaReserva < fechaActual)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.FechaReserva),
                    "No se pueden realizar reservaciones para fechas pasadas.");
            }


            // ========================================================
            // VALIDAR HORARIO
            // ========================================================

            if (reservacion.HoraFin <= reservacion.HoraInicio)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.HoraFin),
                    "La hora de finalización debe ser mayor que la hora de inicio.");
            }


            // ========================================================
            // VALIDAR OBSERVACIÓN
            // ========================================================

            if (!string.IsNullOrWhiteSpace(reservacion.Observacion) &&
                reservacion.Observacion.Length > 300)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.Observacion),
                    "La observación no puede superar los 300 caracteres.");
            }


            // ========================================================
            // VALIDAR ESTADO
            // ========================================================

            if (reservacion.IdEstadoReservacion <= 0)
            {
                ModelState.AddModelError(
                    nameof(Reservacion.IdEstadoReservacion),
                    "Debe seleccionar un estado.");
            }
            else
            {
                var estadoExiste =
                    await _context.EstadoReservacions
                        .AsNoTracking()
                        .AnyAsync(e =>
                            e.IdEstadoReservacion ==
                            reservacion.IdEstadoReservacion);

                if (!estadoExiste)
                {
                    ModelState.AddModelError(
                        nameof(Reservacion.IdEstadoReservacion),
                        "El estado seleccionado no existe.");
                }
            }


            // ========================================================
            // VALIDAR CONFLICTO
            // ========================================================

            if (ModelState.IsValid)
            {
                bool existeConflicto =
                    await ExisteConflictoHorario(
                        reservacion.IdMesa,
                        reservacion.FechaReserva,
                        reservacion.HoraInicio,
                        reservacion.HoraFin,
                        reservacion.IdReservacion);

                if (existeConflicto)
                {
                    ModelState.AddModelError(
                        "",
                        "La mesa seleccionada ya está reservada durante el horario indicado.");
                }
            }


            // ========================================================
            // REGRESAR SI HAY ERRORES
            // ========================================================

            if (!ModelState.IsValid)
            {
                await CargarDatosFormulario(reservacion);

                return View(reservacion);
            }


            // ========================================================
            // ACTUALIZAR SOLO CAMPOS PERMITIDOS
            // ========================================================

            existente.IdCliente =
                reservacion.IdCliente;

            existente.IdMesa =
                reservacion.IdMesa;

            existente.IdEstadoReservacion =
                reservacion.IdEstadoReservacion;

            existente.FechaReserva =
                reservacion.FechaReserva;

            existente.CantidadPersonas =
                reservacion.CantidadPersonas;

            existente.Observacion =
                string.IsNullOrWhiteSpace(reservacion.Observacion)
                    ? null
                    : reservacion.Observacion.Trim();

            existente.HoraInicio =
                reservacion.HoraInicio;

            existente.HoraFin =
                reservacion.HoraFin;


            // ========================================================
            // GUARDAR CAMBIOS
            // ========================================================

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "La reservación se actualizó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(
                    "",
                    "La reservación fue modificada o eliminada mientras se procesaba la solicitud.");

                await CargarDatosFormulario(reservacion);

                return View(reservacion);
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(
                    "",
                    ObtenerMensajeErrorBaseDatos(ex));

                await CargarDatosFormulario(reservacion);

                return View(reservacion);
            }
        }


        // ============================================================
        // GET: Reservacion/Delete/5
        // ============================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservacion =
                await _context.Reservacions
                    .AsNoTracking()
                    .Include(r => r.IdClienteNavigation)
                    .Include(r => r.IdMesaNavigation)
                    .Include(r => r.IdEstadoReservacionNavigation)
                    .FirstOrDefaultAsync(r =>
                        r.IdReservacion == id);

            if (reservacion == null)
            {
                return NotFound();
            }

            return View(reservacion);
        }


        // ============================================================
        // POST: Reservacion/Delete/5
        // ============================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservacion =
                await _context.Reservacions
                    .FindAsync(id);

            if (reservacion == null)
            {
                return NotFound();
            }

            try
            {
                _context.Reservacions.Remove(reservacion);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "La reservación se eliminó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] =
                    ObtenerMensajeErrorBaseDatos(ex);

                return RedirectToAction(nameof(Index));
            }
        }


        // ============================================================
        // VALIDAR CONFLICTO DE HORARIO
        // ============================================================

        private async Task<bool> ExisteConflictoHorario(
            int idMesa,
            DateOnly fecha,
            TimeOnly horaInicio,
            TimeOnly horaFin,
            int? idReservacionExcluir = null)
        {
            var query =
                _context.Reservacions
                    .AsNoTracking()
                    .Where(r =>
                        r.IdMesa == idMesa &&
                        r.FechaReserva == fecha &&

                        // Solo reservaciones activas
                        r.IdEstadoReservacion != 5 &&
                        r.IdEstadoReservacion != 6 &&

                        // Detectar traslape
                        r.HoraInicio < horaFin &&
                        r.HoraFin > horaInicio
                    );

            if (idReservacionExcluir.HasValue)
            {
                query = query.Where(r =>
                    r.IdReservacion !=
                    idReservacionExcluir.Value);
            }

            return await query.AnyAsync();
        }


        // ============================================================
        // CARGAR DATOS PARA LOS FORMULARIOS
        // ============================================================

        private async Task CargarDatosFormulario(
            Reservacion? reservacion = null)
        {
            var clientes =
                await _context.Clientes
                    .AsNoTracking()
                    .Where(c => c.Estado)
                    .OrderBy(c => c.Nombres)
                    .ThenBy(c => c.Apellidos)
                    .ToListAsync();


            var mesas =
                await _context.Mesas
                    .AsNoTracking()
                    .Include(m => m.IdEstadoMesaNavigation)
                    .Where(m => m.Estado)
                    .OrderBy(m => m.NumeroMesa)
                    .ToListAsync();


            var estados =
                await _context.EstadoReservacions
                    .AsNoTracking()
                    .OrderBy(e => e.IdEstadoReservacion)
                    .ToListAsync();


            ViewBag.Clientes = clientes;

            ViewBag.Mesas = mesas;

            ViewBag.Estados = estados;
        }


        // ============================================================
        // MENSAJE DE ERROR DE BASE DE DATOS
        // ============================================================

        private string ObtenerMensajeErrorBaseDatos(
            DbUpdateException ex)
        {
            var mensaje =
                ex.InnerException?.Message ??
                ex.Message;


            // ========================================================
            // CONFLICTO DE RESERVACIÓN
            // ========================================================

            if (mensaje.Contains(
                    "La mesa ya posee una reservación",
                    StringComparison.OrdinalIgnoreCase))
            {
                return
                    "La mesa ya posee una reservación durante ese horario.";
            }


            // ========================================================
            // CLAVE FORÁNEA
            // ========================================================

            if (mensaje.Contains(
                    "FOREIGN KEY",
                    StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Los datos seleccionados no son válidos. Verifique el cliente, mesa y estado de la reservación.";
            }


            // ========================================================
            // ERROR GENERAL
            // ========================================================

            return
                "No fue posible guardar la reservación. " +
                "Verifique los datos e inténtelo nuevamente.";
        }
    }
}
