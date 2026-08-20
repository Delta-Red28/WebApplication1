using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CajaController : Controller
    {
        private readonly RestauranteContext _context;

        public CajaController(RestauranteContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? idEstadoCaja)
        {
            var query = _context.Cajas
                .Include(c => c.IdUsuarioNavigation)
                .Include(c => c.IdEstadoCajaNavigation)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                var inicio = fechaInicio.Value.Date;

                query = query.Where(c =>
                    c.FechaApertura >= inicio);
            }

            if (fechaFin.HasValue)
            {
                var fin = fechaFin.Value.Date.AddDays(1);

                query = query.Where(c =>
                    c.FechaApertura < fin);
            }

            if (idEstadoCaja.HasValue)
            {
                query = query.Where(c =>
                    c.IdEstadoCaja == idEstadoCaja.Value);
            }

            var cajas = await query
                .OrderByDescending(c => c.FechaApertura)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.IdEstadoCaja = idEstadoCaja;

            ViewBag.EstadosCaja = await _context.EstadoCajas
                .Where(e => e.Estado)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return View(cajas);
        }

        // ============================================================
        // DETAILS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caja = await _context.Cajas
                .Include(c => c.IdUsuarioNavigation)
                .Include(c => c.IdEstadoCajaNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdCaja == id.Value);

            if (caja == null)
            {
                return NotFound();
            }

            await ActualizarTotalesCaja(caja);

            await _context.SaveChangesAsync();

            var movimientos = await _context.MovimientoCajas
                .Where(m => m.IdCaja == caja.IdCaja)
                .Include(m => m.IdTipoMovimientoNavigation)
                .Include(m => m.IdPagoNavigation)
                .Include(m => m.IdMonedaNavigation)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();

            ViewBag.Movimientos = movimientos;

            return View(caja);
        }

        // ============================================================
        // ABRIR CAJA - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Abrir()
        {
            var cajaAbierta = await ObtenerCajaAbiertaUsuario();

            if (cajaAbierta != null)
            {
                TempData["Error"] =
                    $"Ya tienes una caja abierta actualmente. Caja #{cajaAbierta.IdCaja}.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = cajaAbierta.IdCaja });
            }

            var caja = new Caja
            {
                FechaApertura = DateTime.Now,

                MontoInicial = 0,

                MontoInicialCordobas = 0,
                MontoInicialDolares = 0,

                TotalIngresos = 0,
                TotalEgresos = 0,
                TotalSistema = 0,

                TotalIngresosCordobas = 0,
                TotalIngresosDolares = 0,

                TotalEgresosCordobas = 0,
                TotalEgresosDolares = 0,

                TotalSistemaCordobas = 0,
                TotalSistemaDolares = 0,

                TotalContado = null,
                TotalContadoCordobas = null,
                TotalContadoDolares = null,

                Diferencia = null,
                DiferenciaCordobas = null,
                DiferenciaDolares = null
            };

            await CargarEstadosCaja();

            return View(caja);
        }

        // ============================================================
        // ABRIR CAJA - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abrir(Caja caja)
        {
            var cajaAbierta = await ObtenerCajaAbiertaUsuario();

            if (cajaAbierta != null)
            {
                ModelState.AddModelError(
                    "",
                    $"Ya tienes una caja abierta actualmente. Caja #{cajaAbierta.IdCaja}.");
            }

            if (caja.MontoInicial < 0)
            {
                ModelState.AddModelError(
                    nameof(caja.MontoInicial),
                    "El monto inicial no puede ser negativo.");
            }

            if (caja.MontoInicialCordobas < 0)
            {
                ModelState.AddModelError(
                    nameof(caja.MontoInicialCordobas),
                    "El monto inicial en Córdobas no puede ser negativo.");
            }

            if (caja.MontoInicialDolares < 0)
            {
                ModelState.AddModelError(
                    nameof(caja.MontoInicialDolares),
                    "El monto inicial en Dólares no puede ser negativo.");
            }

            var estadoAbierta =
                await ObtenerEstadoCaja("Abierta");

            if (estadoAbierta == null)
            {
                ModelState.AddModelError(
                    "",
                    "No existe un estado de caja activo llamado 'Abierta'.");
            }

            if (!ModelState.IsValid)
            {
                await CargarEstadosCaja();

                return View(caja);
            }

            int idUsuario = ObtenerUsuarioActual();

            caja.IdUsuario = idUsuario;
            caja.IdEstadoCaja =
                estadoAbierta!.IdEstadoCaja;

            caja.FechaApertura = DateTime.Now;
            caja.FechaCierre = null;

            caja.TotalIngresos = 0;
            caja.TotalEgresos = 0;

            caja.TotalIngresosCordobas = 0;
            caja.TotalIngresosDolares = 0;

            caja.TotalEgresosCordobas = 0;
            caja.TotalEgresosDolares = 0;

            caja.TotalSistema =
                caja.MontoInicial;

            caja.TotalSistemaCordobas =
                caja.MontoInicialCordobas;

            caja.TotalSistemaDolares =
                caja.MontoInicialDolares;

            caja.TotalContado = null;
            caja.TotalContadoCordobas = null;
            caja.TotalContadoDolares = null;

            caja.Diferencia = null;
            caja.DiferenciaCordobas = null;
            caja.DiferenciaDolares = null;

            _context.Cajas.Add(caja);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Caja #{caja.IdCaja} abierta correctamente.";

            return RedirectToAction(
                nameof(Details),
                new { id = caja.IdCaja });
        }

        // ============================================================
        // CERRAR CAJA - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Cerrar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caja = await _context.Cajas
                .Include(c => c.IdEstadoCajaNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdCaja == id.Value);

            if (caja == null)
            {
                return NotFound();
            }

            if (!await EsCajaAbiertaAsync(caja))
            {
                TempData["Error"] =
                    "La caja seleccionada no está abierta.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = caja.IdCaja });
            }

            await ActualizarTotalesCaja(caja);

            return View(caja);
        }

        // ============================================================
        // CERRAR CAJA - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cerrar(
            int id,
            decimal? totalContado,
            decimal? totalContadoCordobas,
            decimal? totalContadoDolares,
            string? observacion)
        {
            var caja = await _context.Cajas
                .Include(c => c.IdEstadoCajaNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdCaja == id);

            if (caja == null)
            {
                return NotFound();
            }

            if (!await EsCajaAbiertaAsync(caja))
            {
                TempData["Error"] =
                    "La caja ya está cerrada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            var estadoCerrada =
                await ObtenerEstadoCaja("Cerrada");

            if (estadoCerrada == null)
            {
                TempData["Error"] =
                    "No existe un estado de caja activo llamado 'Cerrada'.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            if (totalContado.HasValue &&
                totalContado.Value < 0)
            {
                ModelState.AddModelError(
                    "totalContado",
                    "El total contado no puede ser negativo.");
            }

            if (totalContadoCordobas.HasValue &&
                totalContadoCordobas.Value < 0)
            {
                ModelState.AddModelError(
                    "totalContadoCordobas",
                    "El total contado en Córdobas no puede ser negativo.");
            }

            if (totalContadoDolares.HasValue &&
                totalContadoDolares.Value < 0)
            {
                ModelState.AddModelError(
                    "totalContadoDolares",
                    "El total contado en Dólares no puede ser negativo.");
            }

            if (!ModelState.IsValid)
            {
                await ActualizarTotalesCaja(caja);

                return View(caja);
            }

            await ActualizarTotalesCaja(caja);

            caja.TotalContado = totalContado;
            caja.TotalContadoCordobas =
                totalContadoCordobas;
            caja.TotalContadoDolares =
                totalContadoDolares;

            if (totalContado.HasValue)
            {
                caja.Diferencia =
                    totalContado.Value -
                    caja.TotalSistema;
            }
            else
            {
                caja.Diferencia = null;
            }

            if (totalContadoCordobas.HasValue)
            {
                caja.DiferenciaCordobas =
                    totalContadoCordobas.Value -
                    caja.TotalSistemaCordobas;
            }
            else
            {
                caja.DiferenciaCordobas = null;
            }

            if (totalContadoDolares.HasValue)
            {
                caja.DiferenciaDolares =
                    totalContadoDolares.Value -
                    caja.TotalSistemaDolares;
            }
            else
            {
                caja.DiferenciaDolares = null;
            }

            caja.Observacion =
                string.IsNullOrWhiteSpace(observacion)
                    ? null
                    : observacion.Trim();

            caja.FechaCierre = DateTime.Now;

            caja.IdEstadoCaja =
                estadoCerrada.IdEstadoCaja;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Caja #{caja.IdCaja} cerrada correctamente.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // ============================================================
        // MOVIMIENTOS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Movimientos(int id)
        {
            var caja = await _context.Cajas
                .Include(c => c.IdUsuarioNavigation)
                .Include(c => c.IdEstadoCajaNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdCaja == id);

            if (caja == null)
            {
                return NotFound();
            }

            var movimientos = await _context.MovimientoCajas
                .Where(m => m.IdCaja == id)
                .Include(m => m.IdTipoMovimientoNavigation)
                .Include(m => m.IdPagoNavigation)
                .Include(m => m.IdMonedaNavigation)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();

            ViewBag.Caja = caja;

            return View(movimientos);
        }

        // ============================================================
        // CREAR MOVIMIENTO MANUAL - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> CrearMovimiento(int id)
        {
            var caja = await _context.Cajas
                .Include(c => c.IdEstadoCajaNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdCaja == id);

            if (caja == null)
            {
                return NotFound();
            }

            if (!await EsCajaAbiertaAsync(caja))
            {
                TempData["Error"] =
                    "No se pueden registrar movimientos en una caja cerrada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            await CargarTiposMovimiento();
            await CargarMonedas();

            ViewBag.Caja = caja;

            var movimiento = new MovimientoCaja
            {
                IdCaja = caja.IdCaja,
                FechaMovimiento = DateTime.Now,
                Anulado = false
            };

            return View(movimiento);
        }

        // ============================================================
        // CREAR MOVIMIENTO MANUAL - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMovimiento(
            MovimientoCaja movimiento)
        {
            var caja = await _context.Cajas
                .Include(c => c.IdEstadoCajaNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdCaja == movimiento.IdCaja);

            if (caja == null)
            {
                return NotFound();
            }

            if (!await EsCajaAbiertaAsync(caja))
            {
                ModelState.AddModelError(
                    "",
                    "No se pueden registrar movimientos en una caja cerrada.");
            }

            if (movimiento.Monto <= 0)
            {
                ModelState.AddModelError(
                    nameof(movimiento.Monto),
                    "El monto debe ser mayor que cero.");
            }

            var tipoMovimiento =
                await _context.TipoMovimientoCajas
                    .FirstOrDefaultAsync(t =>
                        t.IdTipoMovimiento ==
                        movimiento.IdTipoMovimiento);

            if (tipoMovimiento == null ||
                !tipoMovimiento.Estado)
            {
                ModelState.AddModelError(
                    nameof(movimiento.IdTipoMovimiento),
                    "El tipo de movimiento seleccionado no es válido.");
            }

            if (!movimiento.IdMoneda.HasValue)
            {
                ModelState.AddModelError(
                    nameof(movimiento.IdMoneda),
                    "Debe seleccionar una moneda.");
            }
            else
            {
                var moneda = await _context.Moneda
                    .FirstOrDefaultAsync(m =>
                        m.IdMoneda ==
                        movimiento.IdMoneda.Value);

                if (moneda == null || !moneda.Estado)
                {
                    ModelState.AddModelError(
                        nameof(movimiento.IdMoneda),
                        "La moneda seleccionada no es válida.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarTiposMovimiento();
                await CargarMonedas();

                ViewBag.Caja = caja;

                return View(movimiento);
            }

            movimiento.IdPago = null;
            movimiento.FechaMovimiento = DateTime.Now;
            movimiento.Anulado = false;
            movimiento.FechaAnulacion = null;
            movimiento.IdUsuarioAnulo = null;
            movimiento.MotivoAnulacion = null;

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                _context.MovimientoCajas.Add(movimiento);

                await _context.SaveChangesAsync();

                await ActualizarTotalesCaja(caja);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Movimiento registrado correctamente.";

                return RedirectToAction(
                    nameof(Movimientos),
                    new { id = caja.IdCaja });
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "No fue posible registrar el movimiento.");

                await CargarTiposMovimiento();
                await CargarMonedas();

                ViewBag.Caja = caja;

                return View(movimiento);
            }
        }

        // ============================================================
        // ANULAR MOVIMIENTO - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> AnularMovimiento(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento = await _context.MovimientoCajas
                .Include(m => m.IdCajaNavigation)
                .Include(m => m.IdTipoMovimientoNavigation)
                .Include(m => m.IdMonedaNavigation)
                .Include(m => m.IdPagoNavigation)
                .FirstOrDefaultAsync(m =>
                    m.IdMovimientoCaja == id.Value);

            if (movimiento == null)
            {
                return NotFound();
            }

            if (movimiento.Anulado)
            {
                TempData["Error"] =
                    "El movimiento ya está anulado.";

                return RedirectToAction(
                    nameof(Movimientos),
                    new { id = movimiento.IdCaja });
            }

            return View(movimiento);
        }

        // ============================================================
        // ANULAR MOVIMIENTO - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnularMovimiento(
            int id,
            string? motivoAnulacion)
        {
            var movimiento = await _context.MovimientoCajas
                .Include(m => m.IdCajaNavigation)
                .FirstOrDefaultAsync(m =>
                    m.IdMovimientoCaja == id);

            if (movimiento == null)
            {
                return NotFound();
            }

            if (movimiento.Anulado)
            {
                TempData["Error"] =
                    "El movimiento ya está anulado.";

                return RedirectToAction(
                    nameof(Movimientos),
                    new { id = movimiento.IdCaja });
            }

            if (string.IsNullOrWhiteSpace(motivoAnulacion))
            {
                ModelState.AddModelError(
                    "MotivoAnulacion",
                    "Debe indicar el motivo de la anulación.");

                var movimientoView =
                    await _context.MovimientoCajas
                        .Include(m => m.IdCajaNavigation)
                        .Include(m => m.IdTipoMovimientoNavigation)
                        .Include(m => m.IdMonedaNavigation)
                        .Include(m => m.IdPagoNavigation)
                        .FirstOrDefaultAsync(m =>
                            m.IdMovimientoCaja == id);

                return View(movimientoView);
            }

            var caja = await _context.Cajas
                .Include(c => c.IdEstadoCajaNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdCaja == movimiento.IdCaja);

            if (caja == null)
            {
                return NotFound();
            }

            if (!await EsCajaAbiertaAsync(caja))
            {
                TempData["Error"] =
                    "No se puede anular un movimiento de una caja cerrada.";

                return RedirectToAction(
                    nameof(Movimientos),
                    new { id = movimiento.IdCaja });
            }

            // ========================================================
            // LOS MOVIMIENTOS ASOCIADOS A PAGOS SE
            // ANULAN DESDE PagoController
            // ========================================================

            if (movimiento.IdPago.HasValue)
            {
                TempData["Error"] =
                    "Este movimiento pertenece a un pago. " +
                    "Debe anularse desde el módulo de pagos.";

                return RedirectToAction(
                    nameof(Movimientos),
                    new { id = movimiento.IdCaja });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                movimiento.Anulado = true;

                movimiento.FechaAnulacion =
                    DateTime.Now;

                movimiento.IdUsuarioAnulo =
                    ObtenerUsuarioActual();

                movimiento.MotivoAnulacion =
                    motivoAnulacion.Trim();

                await _context.SaveChangesAsync();

                await ActualizarTotalesCaja(caja);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Movimiento anulado correctamente.";

                return RedirectToAction(
                    nameof(Movimientos),
                    new { id = movimiento.IdCaja });
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "No fue posible anular el movimiento.";

                return RedirectToAction(
                    nameof(Movimientos),
                    new { id = movimiento.IdCaja });
            }
        }

        // ============================================================
        // REGISTRAR PAGO DESDE CAJA
        // ============================================================
        //
        // Esta acción NO guarda el pago.
        //
        // Solamente verifica la caja y envía al formulario
        // correspondiente del PagoController.
        //
        // Flujo:
        //
        // Caja/RegistrarPago/2
        //        ↓
        // Pago/Create?idCaja=2
        //
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> RegistrarPago(int id)
        {
            var caja = await _context.Cajas
                .Include(c => c.IdEstadoCajaNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdCaja == id);

            if (caja == null)
            {
                return NotFound();
            }

            if (!await EsCajaAbiertaAsync(caja))
            {
                TempData["Error"] =
                    "No se puede registrar un pago porque la caja está cerrada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = caja.IdCaja });
            }

            return RedirectToAction(
                "Create",
                "Pago",
                new
                {
                    idCaja = caja.IdCaja
                });
        }

        // ============================================================
        // BUSCAR PAGOS DESDE CAJA
        // ============================================================
        //
        // Esta acción NO realiza la búsqueda.
        //
        // Solamente redirige al PagoController.
        //
        // Flujo:
        //
        // Caja/BuscarPago?idCaja=2
        //        ↓
        // Pago/BuscarPago?idCaja=2
        //        ↓
        // Views/Pago/BuscarPago.cshtml
        //
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> BuscarPago(int idCaja)
        {
            var caja = await _context.Cajas
                .Include(c => c.IdEstadoCajaNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdCaja == idCaja);

            if (caja == null)
            {
                return NotFound();
            }

            return RedirectToAction(
                "BuscarPago",
                "Pago",
                new
                {
                    idCaja = caja.IdCaja
                });
        }

        // ============================================================
        // CAJA ABIERTA
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> CajaAbierta()
        {
            var caja = await ObtenerCajaAbiertaUsuario();

            if (caja == null)
            {
                TempData["Error"] =
                    "No tienes una caja abierta actualmente.";

                return RedirectToAction(nameof(Index));
            }

            await ActualizarTotalesCaja(caja);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = caja.IdCaja });
        }

        // ============================================================
        // BUSCAR CAJA
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> BuscarCaja(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? idEstadoCaja)
        {
            var query = _context.Cajas
                .Include(c => c.IdUsuarioNavigation)
                .Include(c => c.IdEstadoCajaNavigation)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                var inicio = fechaInicio.Value.Date;

                query = query.Where(c =>
                    c.FechaApertura >= inicio);
            }

            if (fechaFin.HasValue)
            {
                var fin =
                    fechaFin.Value.Date.AddDays(1);

                query = query.Where(c =>
                    c.FechaApertura < fin);
            }

            if (idEstadoCaja.HasValue)
            {
                query = query.Where(c =>
                    c.IdEstadoCaja == idEstadoCaja.Value);
            }

            var cajas = await query
                .OrderByDescending(c =>
                    c.FechaApertura)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.IdEstadoCaja = idEstadoCaja;

            ViewBag.EstadosCaja =
                await _context.EstadoCajas
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();

            return View(cajas);
        }

        // ============================================================
        // RECALCULAR TOTALES DE CAJA
        // ============================================================

        private async Task ActualizarTotalesCaja(Caja caja)
        {
            var movimientos =
                await _context.MovimientoCajas
                    .Include(m =>
                        m.IdTipoMovimientoNavigation)
                    .Include(m =>
                        m.IdMonedaNavigation)
                    .Where(m =>
                        m.IdCaja == caja.IdCaja &&
                        !m.Anulado)
                    .ToListAsync();

            caja.TotalIngresos = 0;
            caja.TotalEgresos = 0;

            caja.TotalIngresosCordobas = 0;
            caja.TotalIngresosDolares = 0;

            caja.TotalEgresosCordobas = 0;
            caja.TotalEgresosDolares = 0;

            foreach (var movimiento in movimientos)
            {
                if (movimiento.IdTipoMovimientoNavigation == null)
                {
                    continue;
                }

                string tipo =
                    movimiento.IdTipoMovimientoNavigation.Nombre
                        .Trim()
                        .ToLowerInvariant();

                bool esIngreso =
                    EsIngreso(tipo);

                bool esEgreso =
                    EsEgreso(tipo);

                if (esIngreso == esEgreso)
                {
                    continue;
                }

                if (esIngreso)
                {
                    caja.TotalIngresos +=
                        movimiento.Monto;
                }

                if (esEgreso)
                {
                    caja.TotalEgresos +=
                        movimiento.Monto;
                }

                if (movimiento.IdMonedaNavigation == null)
                {
                    continue;
                }

                string codigoMoneda =
                    movimiento.IdMonedaNavigation.CodigoIso
                        .Trim()
                        .ToUpperInvariant();

                // ====================================================
                // CÓRDOBAS
                // ====================================================

                if (codigoMoneda == "NIO")
                {
                    if (esIngreso)
                    {
                        caja.TotalIngresosCordobas +=
                            movimiento.Monto;
                    }
                    else if (esEgreso)
                    {
                        caja.TotalEgresosCordobas +=
                            movimiento.Monto;
                    }
                }

                // ====================================================
                // DÓLARES
                // ====================================================

                else if (codigoMoneda == "USD")
                {
                    if (esIngreso)
                    {
                        caja.TotalIngresosDolares +=
                            movimiento.Monto;
                    }
                    else if (esEgreso)
                    {
                        caja.TotalEgresosDolares +=
                            movimiento.Monto;
                    }
                }
            }

            // ========================================================
            // TOTAL GENERAL
            // ========================================================

            caja.TotalSistema =
                caja.MontoInicial +
                caja.TotalIngresos -
                caja.TotalEgresos;

            // ========================================================
            // TOTAL SISTEMA CÓRDOBAS
            // ========================================================

            caja.TotalSistemaCordobas =
                caja.MontoInicialCordobas +
                caja.TotalIngresosCordobas -
                caja.TotalEgresosCordobas;

            // ========================================================
            // TOTAL SISTEMA DÓLARES
            // ========================================================

            caja.TotalSistemaDolares =
                caja.MontoInicialDolares +
                caja.TotalIngresosDolares -
                caja.TotalEgresosDolares;
        }

        // ============================================================
        // DETERMINAR INGRESO
        // ============================================================

        private bool EsIngreso(string tipo)
        {
            return tipo == "ingreso"
                || tipo == "entrada"
                || tipo == "venta"
                || tipo == "pago"
                || tipo == "cobro"
                || tipo == "abono";
        }

        // ============================================================
        // DETERMINAR EGRESO
        // ============================================================

        private bool EsEgreso(string tipo)
        {
            return tipo == "egreso"
                || tipo == "salida"
                || tipo == "retiro"
                || tipo == "gasto";
        }

        // ============================================================
        // OBTENER CAJA ABIERTA DEL USUARIO
        // ============================================================

        private async Task<Caja?> ObtenerCajaAbiertaUsuario()
        {
            int idUsuario =
                ObtenerUsuarioActual();

            var estadoAbierta =
                await ObtenerEstadoCaja("Abierta");

            if (estadoAbierta == null)
            {
                return null;
            }

            return await _context.Cajas
                .Include(c =>
                    c.IdEstadoCajaNavigation)
                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == idUsuario &&
                    c.IdEstadoCaja ==
                        estadoAbierta.IdEstadoCaja &&
                    c.FechaCierre == null);
        }

        // ============================================================
        // OBTENER ESTADO DE CAJA
        // ============================================================

        private async Task<EstadoCaja?> ObtenerEstadoCaja(
            string nombre)
        {
            nombre = nombre.Trim();

            return await _context.EstadoCajas
                .FirstOrDefaultAsync(e =>
                    e.Estado &&
                    e.Nombre.ToLower() ==
                        nombre.ToLower());
        }

        // ============================================================
        // CARGAR ESTADOS DE CAJA
        // ============================================================

        private async Task CargarEstadosCaja()
        {
            ViewBag.EstadosCaja =
                await _context.EstadoCajas
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();
        }

        // ============================================================
        // CARGAR TIPOS DE MOVIMIENTO
        // ============================================================

        private async Task CargarTiposMovimiento()
        {
            ViewBag.TiposMovimiento =
                await _context.TipoMovimientoCajas
                    .Where(t => t.Estado)
                    .OrderBy(t => t.Nombre)
                    .ToListAsync();
        }

        // ============================================================
        // CARGAR MONEDAS
        // ============================================================

        private async Task CargarMonedas()
        {
            ViewBag.Monedas =
                await _context.Moneda
                    .Where(m => m.Estado)
                    .OrderBy(m => m.Nombre)
                    .ToListAsync();
        }

        // ============================================================
        // VALIDAR CAJA ABIERTA
        // ============================================================

        private async Task<bool> EsCajaAbiertaAsync(
            Caja caja)
        {
            var estadoAbierta =
                await ObtenerEstadoCaja("Abierta");

            if (estadoAbierta == null)
            {
                return false;
            }

            return caja.IdEstadoCaja ==
                   estadoAbierta.IdEstadoCaja &&
                   caja.FechaCierre == null;
        }

        // ============================================================
        // USUARIO ACTUAL
        // ============================================================

        private int ObtenerUsuarioActual()
        {
            // ========================================================
            // TEMPORAL
            // ========================================================
            //
            // Mientras no esté conectado el sistema de autenticación,
            // utilizamos el usuario 1.
            //
            // ========================================================

            return 1;
        }
    }
}