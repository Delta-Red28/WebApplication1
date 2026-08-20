using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PagoController : Controller
    {
        private readonly RestauranteContext _context;

        // ============================================================
        // ESTADOS DE PAGO
        // ============================================================

        private const int ESTADO_PAGO_CONFIRMADO = 2;
        private const int ESTADO_PAGO_ANULADO = 4;

        // ============================================================
        // ESTADOS DE FACTURA
        // ============================================================

        private const int ESTADO_FACTURA_PENDIENTE = 1;
        private const int ESTADO_FACTURA_PAGADA = 2;
        private const int ESTADO_FACTURA_ANULADA = 3;
        private const int ESTADO_FACTURA_PARCIAL = 4;

        public PagoController(RestauranteContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pagos = await _context.Pagos
                .Include(p => p.IdFacturaNavigation)
                .Include(p => p.IdMetodoPagoNavigation)
                .Include(p => p.IdMonedaNavigation)
                .Include(p => p.IdBancoNavigation)
                .Include(p => p.IdTipoTarjetaNavigation)
                .Include(p => p.IdEstadoPagoNavigation)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            return View(pagos);
        }

        // ============================================================
        // REGISTRAR PAGO
        // ============================================================
        // Esta acción es la que debe utilizar Caja cuando el usuario
        // selecciona "Registrar Pago".
        //
        // RegistrarPago -> Pago/Create
        // ============================================================

        [HttpGet]
        public IActionResult RegistrarPago()
        {
            return RedirectToAction(nameof(Create));
        }

        // ============================================================
        // DETAILS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.IdFacturaNavigation)
                .Include(p => p.IdMetodoPagoNavigation)
                .Include(p => p.IdMonedaNavigation)
                .Include(p => p.IdBancoNavigation)
                .Include(p => p.IdTipoTarjetaNavigation)
                .Include(p => p.IdEstadoPagoNavigation)
                .FirstOrDefaultAsync(p => p.IdPago == id.Value);

            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // ============================================================
        // CREATE GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int idUsuario = ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                TempData["Error"] =
                    "No se pudo identificar al usuario actual.";

                return RedirectToAction("Index", "Home");
            }

            var caja = await ObtenerCajaAbierta(idUsuario);

            if (caja == null)
            {
                TempData["Error"] =
                    "No tienes una caja abierta. Debes abrir una caja antes de registrar pagos.";

                return RedirectToAction("Index", "Caja");
            }

            var pago = new Pago
            {
                IdEstadoPago = ESTADO_PAGO_CONFIRMADO,
                FechaPago = DateTime.Now
            };

            await CargarListas(pago);

            ViewBag.Caja = caja;

            return View(pago);
        }

        // ============================================================
        // CREATE POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pago pago)
        {
            int idUsuario = ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo identificar al usuario actual.");
            }

            // ========================================================
            // CAJA
            // ========================================================

            var caja = await ObtenerCajaAbierta(idUsuario);

            if (caja == null)
            {
                ModelState.AddModelError(
                    "",
                    "No tienes una caja abierta. Debes abrir una caja antes de registrar el pago.");
            }

            // ========================================================
            // FACTURA
            // ========================================================

            Factura? factura = null;

            if (pago.IdFactura > 0)
            {
                factura = await _context.Facturas
                    .FirstOrDefaultAsync(f =>
                        f.IdFactura == pago.IdFactura);

                if (factura == null)
                {
                    ModelState.AddModelError(
                        "IdFactura",
                        "La factura seleccionada no existe.");
                }
            }
            else
            {
                ModelState.AddModelError(
                    "IdFactura",
                    "Debe seleccionar una factura.");
            }

            // ========================================================
            // MÉTODO DE PAGO
            // ========================================================

            if (pago.IdMetodoPago <= 0)
            {
                ModelState.AddModelError(
                    "IdMetodoPago",
                    "Debe seleccionar un método de pago.");
            }
            else
            {
                var metodoPago = await _context.MetodoPagos
                    .FirstOrDefaultAsync(m =>
                        m.IdMetodoPago == pago.IdMetodoPago);

                if (metodoPago == null || !metodoPago.Estado)
                {
                    ModelState.AddModelError(
                        "IdMetodoPago",
                        "El método de pago seleccionado no es válido.");
                }
            }

            // ========================================================
            // MONEDA
            // ========================================================

            Monedum? moneda = null;

            if (pago.IdMoneda > 0)
            {
                moneda = await _context.Moneda
                    .FirstOrDefaultAsync(m =>
                        m.IdMoneda == pago.IdMoneda);

                if (moneda == null || !moneda.Estado)
                {
                    ModelState.AddModelError(
                        "IdMoneda",
                        "La moneda seleccionada no es válida.");
                }
            }
            else
            {
                ModelState.AddModelError(
                    "IdMoneda",
                    "Debe seleccionar una moneda.");
            }

            // ========================================================
            // MONTO
            // ========================================================

            if (pago.Monto <= 0)
            {
                ModelState.AddModelError(
                    "Monto",
                    "El monto debe ser mayor que cero.");
            }

            // ========================================================
            // FECHA
            // ========================================================

            if (pago.FechaPago == default)
            {
                pago.FechaPago = DateTime.Now;
            }

            // ========================================================
            // ESTADO
            // ========================================================

            pago.IdEstadoPago = ESTADO_PAGO_CONFIRMADO;

            var estadoPago = await _context.EstadoPagos
                .FirstOrDefaultAsync(e =>
                    e.IdEstadoPago == ESTADO_PAGO_CONFIRMADO &&
                    e.Estado);

            if (estadoPago == null)
            {
                ModelState.AddModelError(
                    "",
                    "No existe un estado activo para pagos confirmados.");
            }

            // ========================================================
            // BANCO
            // ========================================================

            if (pago.IdBanco.HasValue)
            {
                var banco = await _context.Bancos
                    .FirstOrDefaultAsync(b =>
                        b.IdBanco == pago.IdBanco.Value);

                if (banco == null || !banco.Estado)
                {
                    ModelState.AddModelError(
                        "IdBanco",
                        "El banco seleccionado no es válido.");
                }
            }

            // ========================================================
            // TIPO DE TARJETA
            // ========================================================

            if (pago.IdTipoTarjeta.HasValue)
            {
                var tipoTarjeta = await _context.TipoTarjeta
                    .FirstOrDefaultAsync(t =>
                        t.IdTipoTarjeta == pago.IdTipoTarjeta.Value);

                if (tipoTarjeta == null || !tipoTarjeta.Estado)
                {
                    ModelState.AddModelError(
                        "IdTipoTarjeta",
                        "El tipo de tarjeta seleccionado no es válido.");
                }
            }

            // ========================================================
            // FACTURA ANULADA
            // ========================================================

            if (factura != null &&
                factura.IdEstadoFactura == ESTADO_FACTURA_ANULADA)
            {
                ModelState.AddModelError(
                    "IdFactura",
                    "No se puede registrar un pago para una factura anulada.");
            }

            // ========================================================
            // FACTURA PAGADA
            // ========================================================

            if (factura != null &&
                factura.SaldoPendiente <= 0)
            {
                ModelState.AddModelError(
                    "IdFactura",
                    "La factura ya está completamente pagada.");
            }

            // ========================================================
            // MONTO CONTRA SALDO
            // ========================================================

            if (factura != null &&
                pago.Monto > factura.SaldoPendiente)
            {
                ModelState.AddModelError(
                    "Monto",
                    $"El monto ({pago.Monto:N2}) no puede ser mayor " +
                    $"al saldo pendiente ({factura.SaldoPendiente:N2}).");
            }

            // ========================================================
            // MONEDA DE FACTURA
            // ========================================================

            if (factura != null &&
                pago.IdMoneda != factura.IdMoneda)
            {
                ModelState.AddModelError(
                    "IdMoneda",
                    "La moneda del pago debe coincidir con la moneda de la factura.");
            }

            // ========================================================
            // ERRORES
            // ========================================================

            if (!ModelState.IsValid)
            {
                await CargarListas(pago);

                ViewBag.Caja = caja;

                if (factura != null)
                {
                    ViewBag.Factura = factura;
                }

                return View(pago);
            }

            // ========================================================
            // TIPO INGRESO
            // ========================================================

            var tipoIngreso =
                await ObtenerTipoMovimientoIngreso();

            if (tipoIngreso == null)
            {
                ModelState.AddModelError(
                    "",
                    "No existe un tipo de movimiento de caja activo llamado 'Ingreso'.");

                await CargarListas(pago);

                ViewBag.Caja = caja;

                if (factura != null)
                {
                    ViewBag.Factura = factura;
                }

                return View(pago);
            }

            // ========================================================
            // TRANSACCIÓN
            // ========================================================

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // ----------------------------------------------------
                // PAGO
                // ----------------------------------------------------

                pago.IdEstadoPago =
                    ESTADO_PAGO_CONFIRMADO;

                _context.Pagos.Add(pago);

                await _context.SaveChangesAsync();

                // ----------------------------------------------------
                // ACTUALIZAR FACTURA
                // ----------------------------------------------------

                factura!.SaldoPendiente -= pago.Monto;

                if (factura.SaldoPendiente < 0)
                {
                    factura.SaldoPendiente = 0;
                }

                ActualizarEstadoFactura(factura);

                // ----------------------------------------------------
                // MOVIMIENTO DE CAJA
                // ----------------------------------------------------

                var movimiento = new MovimientoCaja
                {
                    IdCaja = caja!.IdCaja,
                    IdTipoMovimiento = tipoIngreso.IdTipoMovimiento,
                    IdPago = pago.IdPago,
                    IdMoneda = pago.IdMoneda,
                    Monto = pago.Monto,
                    Descripcion =
                        $"Pago #{pago.IdPago} - Factura #{pago.IdFactura}",
                    FechaMovimiento = pago.FechaPago,
                    Anulado = false,
                    FechaAnulacion = null,
                    IdUsuarioAnulo = null,
                    MotivoAnulacion = null
                };

                _context.MovimientoCajas.Add(movimiento);

                // ----------------------------------------------------
                // RECALCULAR CAJA
                // ----------------------------------------------------

                await ActualizarTotalesCaja(caja);

                // ----------------------------------------------------
                // HISTORIAL
                // ----------------------------------------------------

                var historial = new HistorialFactura
                {
                    IdFactura = factura.IdFactura,
                    IdEstadoFactura = factura.IdEstadoFactura,
                    IdUsuario = idUsuario,
                    Observacion =
                        $"Pago #{pago.IdPago} registrado por " +
                        $"{pago.Monto:N2}. " +
                        $"Saldo pendiente: " +
                        $"{factura.SaldoPendiente:N2}",
                    Fecha = DateTime.Now
                };

                _context.HistorialFacturas.Add(historial);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Pago #{pago.IdPago} registrado correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = pago.IdPago });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al registrar el pago.");

                await CargarListas(pago);

                ViewBag.Caja = caja;

                if (factura != null)
                {
                    ViewBag.Factura = factura;
                }

                return View(pago);
            }
        }

        // ============================================================
        // CREATE PARA FACTURA
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> CreateParaFactura(int id)
        {
            int idUsuario = ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                TempData["Error"] =
                    "No se pudo identificar al usuario actual.";

                return RedirectToAction(
                    "Details",
                    "Factura",
                    new { id });
            }

            var caja = await ObtenerCajaAbierta(idUsuario);

            if (caja == null)
            {
                TempData["Error"] =
                    "Debes abrir una caja antes de registrar un pago.";

                return RedirectToAction(
                    "Details",
                    "Factura",
                    new { id });
            }

            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f =>
                    f.IdFactura == id);

            if (factura == null)
            {
                return NotFound();
            }

            if (factura.IdEstadoFactura ==
                ESTADO_FACTURA_ANULADA)
            {
                TempData["Error"] =
                    "No se puede registrar un pago para una factura anulada.";

                return RedirectToAction(
                    "Details",
                    "Factura",
                    new { id });
            }

            if (factura.SaldoPendiente <= 0)
            {
                TempData["Error"] =
                    "La factura ya está completamente pagada.";

                return RedirectToAction(
                    "Details",
                    "Factura",
                    new { id });
            }

            var pago = new Pago
            {
                IdFactura = factura.IdFactura,
                IdMoneda = factura.IdMoneda,
                IdEstadoPago = ESTADO_PAGO_CONFIRMADO,
                FechaPago = DateTime.Now,
                Monto = factura.SaldoPendiente
            };

            await CargarListas(pago);

            ViewBag.Factura = factura;
            ViewBag.Caja = caja;

            return View("Create", pago);
        }

        // ============================================================
        // EDIT GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.IdFacturaNavigation)
                .Include(p => p.IdMetodoPagoNavigation)
                .Include(p => p.IdMonedaNavigation)
                .Include(p => p.IdBancoNavigation)
                .Include(p => p.IdTipoTarjetaNavigation)
                .Include(p => p.IdEstadoPagoNavigation)
                .FirstOrDefaultAsync(p =>
                    p.IdPago == id.Value);

            if (pago == null)
            {
                return NotFound();
            }

            if (pago.IdEstadoPago ==
                ESTADO_PAGO_ANULADO)
            {
                TempData["Error"] =
                    "No se puede editar un pago anulado.";

                return RedirectToAction(nameof(Index));
            }

            await CargarListas(pago);

            return View(pago);
        }

        // ============================================================
        // EDIT POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Pago pago)
        {
            if (id != pago.IdPago)
            {
                return NotFound();
            }

            int idUsuario = ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo identificar al usuario actual.");
            }

            // ========================================================
            // PAGO ORIGINAL
            // ========================================================

            var pagoOriginal = await _context.Pagos
                .FirstOrDefaultAsync(p =>
                    p.IdPago == id);

            if (pagoOriginal == null)
            {
                return NotFound();
            }

            if (pagoOriginal.IdEstadoPago ==
                ESTADO_PAGO_ANULADO)
            {
                TempData["Error"] =
                    "No se puede editar un pago anulado.";

                return RedirectToAction(nameof(Index));
            }

            // ========================================================
            // FACTURA ORIGINAL
            // ========================================================

            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f =>
                    f.IdFactura == pagoOriginal.IdFactura);

            if (factura == null)
            {
                ModelState.AddModelError(
                    "",
                    "No se encontró la factura asociada al pago.");
            }

            // ========================================================
            // NO CAMBIAR FACTURA
            // ========================================================

            if (pago.IdFactura != pagoOriginal.IdFactura)
            {
                ModelState.AddModelError(
                    "IdFactura",
                    "No se puede cambiar la factura de un pago existente. Anule el pago y registre uno nuevo.");
            }

            // ========================================================
            // CAJA / MOVIMIENTO
            // ========================================================

            var movimientoOriginal =
                await _context.MovimientoCajas
                    .FirstOrDefaultAsync(m =>
                        m.IdPago == pagoOriginal.IdPago &&
                        !m.Anulado);

            if (movimientoOriginal == null)
            {
                ModelState.AddModelError(
                    "",
                    "No se encontró el movimiento de caja asociado al pago.");
            }

            Caja? cajaOriginal = null;

            if (movimientoOriginal != null)
            {
                cajaOriginal = await _context.Cajas
                    .FirstOrDefaultAsync(c =>
                        c.IdCaja == movimientoOriginal.IdCaja);

                if (cajaOriginal == null)
                {
                    ModelState.AddModelError(
                        "",
                        "No se encontró la caja asociada al pago.");
                }
            }

            // ========================================================
            // MONTO
            // ========================================================

            if (pago.Monto <= 0)
            {
                ModelState.AddModelError(
                    "Monto",
                    "El monto debe ser mayor que cero.");
            }

            // ========================================================
            // FECHA
            // ========================================================

            if (pago.FechaPago == default)
            {
                pago.FechaPago =
                    pagoOriginal.FechaPago;
            }

            // ========================================================
            // MONEDA
            // ========================================================

            var moneda = await _context.Moneda
                .FirstOrDefaultAsync(m =>
                    m.IdMoneda == pago.IdMoneda);

            if (moneda == null || !moneda.Estado)
            {
                ModelState.AddModelError(
                    "IdMoneda",
                    "La moneda seleccionada no es válida.");
            }

            // ========================================================
            // MÉTODO DE PAGO
            // ========================================================

            var metodoPago = await _context.MetodoPagos
                .FirstOrDefaultAsync(m =>
                    m.IdMetodoPago == pago.IdMetodoPago);

            if (metodoPago == null || !metodoPago.Estado)
            {
                ModelState.AddModelError(
                    "IdMetodoPago",
                    "El método de pago seleccionado no es válido.");
            }

            // ========================================================
            // BANCO
            // ========================================================

            if (pago.IdBanco.HasValue)
            {
                var banco = await _context.Bancos
                    .FirstOrDefaultAsync(b =>
                        b.IdBanco == pago.IdBanco.Value);

                if (banco == null || !banco.Estado)
                {
                    ModelState.AddModelError(
                        "IdBanco",
                        "El banco seleccionado no es válido.");
                }
            }

            // ========================================================
            // TIPO TARJETA
            // ========================================================

            if (pago.IdTipoTarjeta.HasValue)
            {
                var tipoTarjeta = await _context.TipoTarjeta
                    .FirstOrDefaultAsync(t =>
                        t.IdTipoTarjeta ==
                        pago.IdTipoTarjeta.Value);

                if (tipoTarjeta == null || !tipoTarjeta.Estado)
                {
                    ModelState.AddModelError(
                        "IdTipoTarjeta",
                        "El tipo de tarjeta seleccionado no es válido.");
                }
            }

            // ========================================================
            // ESTADO
            // ========================================================

            if (pago.IdEstadoPago ==
                ESTADO_PAGO_ANULADO)
            {
                ModelState.AddModelError(
                    "IdEstadoPago",
                    "Para anular un pago utiliza la opción Anular.");
            }

            pago.IdEstadoPago =
                ESTADO_PAGO_CONFIRMADO;

            // ========================================================
            // MONEDA DE FACTURA
            // ========================================================

            if (factura != null &&
                pago.IdMoneda != factura.IdMoneda)
            {
                ModelState.AddModelError(
                    "IdMoneda",
                    "La moneda del pago debe coincidir con la moneda de la factura.");
            }

            // ========================================================
            // FACTURA ANULADA
            // ========================================================

            if (factura != null &&
                factura.IdEstadoFactura ==
                ESTADO_FACTURA_ANULADA)
            {
                ModelState.AddModelError(
                    "IdFactura",
                    "No se puede modificar un pago de una factura anulada.");
            }

            // ========================================================
            // ERRORES
            // ========================================================

            if (!ModelState.IsValid)
            {
                await CargarListas(pago);

                return View(pago);
            }

            // ========================================================
            // TRANSACCIÓN
            // ========================================================

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                if (factura == null)
                {
                    throw new InvalidOperationException(
                        "No se encontró la factura asociada.");
                }

                if (movimientoOriginal == null)
                {
                    throw new InvalidOperationException(
                        "No se encontró el movimiento de caja asociado.");
                }

                if (cajaOriginal == null)
                {
                    throw new InvalidOperationException(
                        "No se encontró la caja asociada.");
                }

                // ====================================================
                // RECONSTRUIR SALDO
                // ====================================================

                factura.SaldoPendiente +=
                    pagoOriginal.Monto;

                if (factura.SaldoPendiente >
                    factura.Total)
                {
                    factura.SaldoPendiente =
                        factura.Total;
                }

                // ====================================================
                // VALIDAR NUEVO MONTO
                // ====================================================

                if (pago.Monto >
                    factura.SaldoPendiente)
                {
                    throw new InvalidOperationException(
                        $"El monto ({pago.Monto:N2}) no puede ser mayor " +
                        $"al saldo disponible " +
                        $"({factura.SaldoPendiente:N2}).");
                }

                // ====================================================
                // APLICAR NUEVO MONTO
                // ====================================================

                factura.SaldoPendiente -=
                    pago.Monto;

                if (factura.SaldoPendiente < 0)
                {
                    factura.SaldoPendiente = 0;
                }

                ActualizarEstadoFactura(factura);

                // ====================================================
                // ACTUALIZAR PAGO
                // ====================================================

                pagoOriginal.IdMetodoPago =
                    pago.IdMetodoPago;

                pagoOriginal.IdMoneda =
                    pago.IdMoneda;

                pagoOriginal.IdBanco =
                    pago.IdBanco;

                pagoOriginal.IdTipoTarjeta =
                    pago.IdTipoTarjeta;

                pagoOriginal.Monto =
                    pago.Monto;

                pagoOriginal.Referencia =
                    pago.Referencia;

                pagoOriginal.NumeroAutorizacion =
                    pago.NumeroAutorizacion;

                pagoOriginal.Observacion =
                    pago.Observacion;

                pagoOriginal.FechaPago =
                    pago.FechaPago;

                pagoOriginal.MontoRecibido =
                    pago.MontoRecibido;

                pagoOriginal.IdEstadoPago =
                    ESTADO_PAGO_CONFIRMADO;

                // ====================================================
                // ACTUALIZAR MOVIMIENTO
                // ====================================================

                movimientoOriginal.IdMoneda =
                    pago.IdMoneda;

                movimientoOriginal.Monto =
                    pago.Monto;

                movimientoOriginal.FechaMovimiento =
                    pago.FechaPago;

                movimientoOriginal.Descripcion =
                    $"Pago #{pagoOriginal.IdPago} - " +
                    $"Factura #{pagoOriginal.IdFactura}";

                // ====================================================
                // RECALCULAR CAJA
                // ====================================================

                await ActualizarTotalesCaja(
                    cajaOriginal);

                // ====================================================
                // HISTORIAL
                // ====================================================

                var historial = new HistorialFactura
                {
                    IdFactura =
                        factura.IdFactura,

                    IdEstadoFactura =
                        factura.IdEstadoFactura,

                    IdUsuario =
                        idUsuario,

                    Observacion =
                        $"Pago #{pagoOriginal.IdPago} modificado. " +
                        $"Monto anterior: " +
                        $"{pagoOriginal.Monto:N2}. " +
                        $"Nuevo monto: " +
                        $"{pago.Monto:N2}. " +
                        $"Saldo pendiente: " +
                        $"{factura.SaldoPendiente:N2}",

                    Fecha =
                        DateTime.Now
                };

                _context.HistorialFacturas.Add(
                    historial);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Pago #{pagoOriginal.IdPago} actualizado correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = pagoOriginal.IdPago
                    });
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    ex.Message);

                await CargarListas(pago);

                return View(pago);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al modificar el pago.");

                await CargarListas(pago);

                return View(pago);
            }
        }

        // ============================================================
        // DELETE / ANULAR GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.IdFacturaNavigation)
                .Include(p => p.IdMetodoPagoNavigation)
                .Include(p => p.IdEstadoPagoNavigation)
                .Include(p => p.IdMonedaNavigation)
                .Include(p => p.IdBancoNavigation)
                .Include(p => p.IdTipoTarjetaNavigation)
                .FirstOrDefaultAsync(p =>
                    p.IdPago == id.Value);

            if (pago == null)
            {
                return NotFound();
            }

            if (pago.IdEstadoPago ==
                ESTADO_PAGO_ANULADO)
            {
                TempData["Error"] =
                    "El pago ya se encuentra anulado.";

                return RedirectToAction(nameof(Index));
            }

            return View(pago);
        }

        // ============================================================
        // DELETE / ANULAR POST
        // ============================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int idUsuario = ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                TempData["Error"] =
                    "No se pudo identificar al usuario actual.";

                return RedirectToAction(nameof(Index));
            }

            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p =>
                    p.IdPago == id);

            if (pago == null)
            {
                return NotFound();
            }

            if (pago.IdEstadoPago ==
                ESTADO_PAGO_ANULADO)
            {
                TempData["Error"] =
                    "El pago ya se encuentra anulado.";

                return RedirectToAction(nameof(Index));
            }

            // ========================================================
            // MOVIMIENTO
            // ========================================================

            var movimiento = await _context.MovimientoCajas
                .FirstOrDefaultAsync(m =>
                    m.IdPago == pago.IdPago &&
                    !m.Anulado);

            if (movimiento == null)
            {
                TempData["Error"] =
                    "No se encontró el movimiento de caja asociado al pago.";

                return RedirectToAction(nameof(Index));
            }

            // ========================================================
            // CAJA
            // ========================================================

            var caja = await _context.Cajas
                .FirstOrDefaultAsync(c =>
                    c.IdCaja == movimiento.IdCaja);

            if (caja == null)
            {
                TempData["Error"] =
                    "No se encontró la caja asociada al pago.";

                return RedirectToAction(nameof(Index));
            }

            // ========================================================
            // FACTURA
            // ========================================================

            var factura = await _context.Facturas
                .FirstOrDefaultAsync(f =>
                    f.IdFactura == pago.IdFactura);

            if (factura == null)
            {
                TempData["Error"] =
                    "No se encontró la factura asociada al pago.";

                return RedirectToAction(nameof(Index));
            }

            // ========================================================
            // TRANSACCIÓN
            // ========================================================

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // ====================================================
                // ANULAR PAGO
                // ====================================================

                pago.IdEstadoPago =
                    ESTADO_PAGO_ANULADO;

                // ====================================================
                // ANULAR MOVIMIENTO
                // ====================================================

                movimiento.Anulado = true;

                movimiento.FechaAnulacion =
                    DateTime.Now;

                movimiento.IdUsuarioAnulo =
                    idUsuario;

                movimiento.MotivoAnulacion =
                    $"Anulación del pago #{pago.IdPago}";

                // ====================================================
                // RESTAURAR FACTURA
                // ====================================================

                if (factura.IdEstadoFactura !=
                    ESTADO_FACTURA_ANULADA)
                {
                    factura.SaldoPendiente +=
                        pago.Monto;

                    if (factura.SaldoPendiente >
                        factura.Total)
                    {
                        factura.SaldoPendiente =
                            factura.Total;
                    }

                    ActualizarEstadoFactura(
                        factura);

                    // ================================================
                    // HISTORIAL
                    // ================================================

                    var historial = new HistorialFactura
                    {
                        IdFactura =
                            factura.IdFactura,

                        IdEstadoFactura =
                            factura.IdEstadoFactura,

                        IdUsuario =
                            idUsuario,

                        Observacion =
                            $"Pago #{pago.IdPago} anulado. " +
                            $"Monto devuelto al saldo: " +
                            $"{pago.Monto:N2}. " +
                            $"Saldo pendiente: " +
                            $"{factura.SaldoPendiente:N2}",

                        Fecha =
                            DateTime.Now
                    };

                    _context.HistorialFacturas.Add(
                        historial);
                }

                // ====================================================
                // RECALCULAR CAJA
                // ====================================================

                await ActualizarTotalesCaja(
                    caja);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Pago #{pago.IdPago} anulado correctamente.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Ocurrió un error al anular el pago.";

                return RedirectToAction(
                    nameof(Index));
            }
        }

        // ============================================================
        // PAGOS POR FACTURA
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> PorFactura(int id)
        {
            var factura = await _context.Facturas
                .Include(f =>
                    f.IdEstadoFacturaNavigation)
                .FirstOrDefaultAsync(f =>
                    f.IdFactura == id);

            if (factura == null)
            {
                return NotFound();
            }

            var pagos = await _context.Pagos
                .Where(p =>
                    p.IdFactura == id)
                .Include(p =>
                    p.IdMetodoPagoNavigation)
                .Include(p =>
                    p.IdBancoNavigation)
                .Include(p =>
                    p.IdTipoTarjetaNavigation)
                .Include(p =>
                    p.IdMonedaNavigation)
                .Include(p =>
                    p.IdEstadoPagoNavigation)
                .OrderByDescending(p =>
                    p.FechaPago)
                .ToListAsync();

            ViewBag.Factura = factura;

            return View(pagos);
        }

        // ============================================================
        // BUSCAR PAGO
        // ============================================================
        //
        // Esta acción es independiente de RegistrarPago.
        //
        // BuscarPago -> Pago/BuscarPago
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> BuscarPago(
            string? buscar,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var query = _context.Pagos
                .Include(p =>
                    p.IdFacturaNavigation)
                .Include(p =>
                    p.IdMetodoPagoNavigation)
                .Include(p =>
                    p.IdMonedaNavigation)
                .Include(p =>
                    p.IdBancoNavigation)
                .Include(p =>
                    p.IdTipoTarjetaNavigation)
                .Include(p =>
                    p.IdEstadoPagoNavigation)
                .AsQueryable();

            // ========================================================
            // BÚSQUEDA
            // ========================================================

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                if (int.TryParse(
                    buscar,
                    out int numero))
                {
                    query = query.Where(p =>
                        p.IdPago == numero ||
                        p.IdFactura == numero);
                }
                else
                {
                    query = query.Where(p =>
                        p.IdMetodoPagoNavigation != null &&
                        p.IdMetodoPagoNavigation.Nombre.Contains(
                            buscar));
                }
            }

            // ========================================================
            // FECHA INICIO
            // ========================================================

            if (fechaInicio.HasValue)
            {
                var fecha =
                    fechaInicio.Value.Date;

                query = query.Where(p =>
                    p.FechaPago >= fecha);
            }

            // ========================================================
            // FECHA FIN
            // ========================================================

            if (fechaFin.HasValue)
            {
                var fechaHasta =
                    fechaFin.Value.Date.AddDays(1);

                query = query.Where(p =>
                    p.FechaPago < fechaHasta);
            }

            var pagos = await query
                .OrderByDescending(p =>
                    p.FechaPago)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View(pagos);
        }

        // ============================================================
        // CARGAR LISTAS
        // ============================================================

        private async Task CargarListas(Pago? pago = null)
        {
            ViewBag.Facturas =
                await _context.Facturas
                    .Where(f =>
                        f.IdEstadoFactura !=
                        ESTADO_FACTURA_ANULADA &&
                        f.SaldoPendiente > 0)
                    .OrderByDescending(f =>
                        f.FechaFactura)
                    .ToListAsync();

            // --------------------------------------------------------
            // Si estamos editando o trabajando con una factura
            // específica, agregamos la factura actual aunque su
            // saldo pendiente sea 0.
            // --------------------------------------------------------

            if (pago != null &&
                pago.IdFactura > 0)
            {
                var facturaActual =
                    await _context.Facturas
                        .FirstOrDefaultAsync(f =>
                            f.IdFactura ==
                            pago.IdFactura);

                if (facturaActual != null)
                {
                    var facturas =
                        ViewBag.Facturas
                        as System.Collections.Generic.List<Factura>;

                    if (facturas != null &&
                        !facturas.Any(f =>
                            f.IdFactura ==
                            facturaActual.IdFactura))
                    {
                        facturas.Add(facturaActual);
                    }
                }
            }

            // ========================================================
            // MÉTODOS DE PAGO
            // ========================================================

            ViewBag.MetodosPago =
                await _context.MetodoPagos
                    .Where(m =>
                        m.Estado)
                    .OrderBy(m =>
                        m.Nombre)
                    .ToListAsync();

            // ========================================================
            // MONEDAS
            // ========================================================

            ViewBag.Monedas =
                await _context.Moneda
                    .Where(m =>
                        m.Estado)
                    .OrderBy(m =>
                        m.Nombre)
                    .ToListAsync();

            // ========================================================
            // BANCOS
            // ========================================================

            ViewBag.Bancos =
                await _context.Bancos
                    .Where(b =>
                        b.Estado)
                    .OrderBy(b =>
                        b.Nombre)
                    .ToListAsync();

            // ========================================================
            // TIPOS DE TARJETA
            // ========================================================

            ViewBag.TiposTarjeta =
                await _context.TipoTarjeta
                    .Where(t =>
                        t.Estado)
                    .OrderBy(t =>
                        t.Nombre)
                    .ToListAsync();

            // ========================================================
            // ESTADOS DE PAGO
            // ========================================================

            ViewBag.EstadosPago =
                await _context.EstadoPagos
                    .Where(e =>
                        e.Estado)
                    .OrderBy(e =>
                        e.IdEstadoPago)
                    .ToListAsync();
        }

        // ============================================================
        // OBTENER CAJA ABIERTA
        // ============================================================

        private async Task<Caja?> ObtenerCajaAbierta(
            int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return null;
            }

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
        // OBTENER ESTADO CAJA
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
        // OBTENER TIPO MOVIMIENTO INGRESO
        // ============================================================

        private async Task<TipoMovimientoCaja?>
            ObtenerTipoMovimientoIngreso()
        {
            return await _context.TipoMovimientoCajas
                .FirstOrDefaultAsync(t =>
                    t.Estado &&
                    t.Nombre.ToLower() ==
                        "ingreso");
        }

        // ============================================================
        // ACTUALIZAR ESTADO FACTURA
        // ============================================================

        private void ActualizarEstadoFactura(
            Factura factura)
        {
            if (factura.IdEstadoFactura ==
                ESTADO_FACTURA_ANULADA)
            {
                return;
            }

            if (factura.SaldoPendiente <= 0)
            {
                factura.SaldoPendiente = 0;

                factura.IdEstadoFactura =
                    ESTADO_FACTURA_PAGADA;
            }
            else if (factura.SaldoPendiente <
                     factura.Total)
            {
                factura.IdEstadoFactura =
                    ESTADO_FACTURA_PARCIAL;
            }
            else
            {
                factura.IdEstadoFactura =
                    ESTADO_FACTURA_PENDIENTE;
            }
        }

        // ============================================================
        // ACTUALIZAR TOTALES DE CAJA
        // ============================================================

        private async Task ActualizarTotalesCaja(
            Caja caja)
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

            // ========================================================
            // REINICIAR TOTALES
            // ========================================================

            caja.TotalIngresos = 0;
            caja.TotalEgresos = 0;

            caja.TotalIngresosCordobas = 0;
            caja.TotalIngresosDolares = 0;

            caja.TotalEgresosCordobas = 0;
            caja.TotalEgresosDolares = 0;

            // ========================================================
            // RECORRER MOVIMIENTOS
            // ========================================================

            foreach (var movimiento in movimientos)
            {
                if (movimiento.IdTipoMovimientoNavigation == null)
                {
                    continue;
                }

                string tipo =
                    movimiento.IdTipoMovimientoNavigation.Nombre
                        .Trim()
                        .ToLower();

                bool esIngreso =
                    EsIngreso(tipo);

                bool esEgreso =
                    EsEgreso(tipo);

                if (!esIngreso && !esEgreso)
                {
                    continue;
                }

                // ----------------------------------------------------
                // TOTALES GENERALES
                // ----------------------------------------------------

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

                // ----------------------------------------------------
                // MONEDA
                // ----------------------------------------------------

                if (!movimiento.IdMoneda.HasValue)
                {
                    continue;
                }

                var moneda =
                    movimiento.IdMonedaNavigation;

                if (moneda == null)
                {
                    continue;
                }

                if (moneda.EsMonedaBase)
                {
                    if (esIngreso)
                    {
                        caja.TotalIngresosCordobas +=
                            movimiento.Monto;
                    }

                    if (esEgreso)
                    {
                        caja.TotalEgresosCordobas +=
                            movimiento.Monto;
                    }
                }
                else if (
                    moneda.CodigoIso.Equals(
                        "USD",
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (esIngreso)
                    {
                        caja.TotalIngresosDolares +=
                            movimiento.Monto;
                    }

                    if (esEgreso)
                    {
                        caja.TotalEgresosDolares +=
                            movimiento.Monto;
                    }
                }
            }

            // ========================================================
            // TOTAL SISTEMA
            // ========================================================

            caja.TotalSistema =
                caja.MontoInicial +
                caja.TotalIngresos -
                caja.TotalEgresos;

            // ========================================================
            // TOTAL CÓRDOBAS
            // ========================================================

            caja.TotalSistemaCordobas =
                caja.MontoInicialCordobas +
                caja.TotalIngresosCordobas -
                caja.TotalEgresosCordobas;

            // ========================================================
            // TOTAL DÓLARES
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
            return tipo.Contains("ingreso")
                || tipo.Contains("entrada")
                || tipo.Contains("venta")
                || tipo.Contains("pago");
        }

        // ============================================================
        // DETERMINAR EGRESO
        // ============================================================

        private bool EsEgreso(string tipo)
        {
            return tipo.Contains("egreso")
                || tipo.Contains("salida")
                || tipo.Contains("retiro")
                || tipo.Contains("gasto");
        }

        // ============================================================
        // USUARIO ACTUAL
        // ============================================================

        private int ObtenerUsuarioActual()
        {
            var claim =
                User.FindFirst("IdUsuario")
                ?? User.FindFirst(
                    ClaimTypes.NameIdentifier);

            if (claim != null &&
                int.TryParse(
                    claim.Value,
                    out int idUsuario))
            {
                return idUsuario;
            }

            return 0;
        }

        // ============================================================
        // EXISTENCIA
        // ============================================================

        private bool PagoExists(int id)
        {
            return _context.Pagos
                .Any(p =>
                    p.IdPago == id);
        }
    }
}