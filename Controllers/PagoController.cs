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

        // ============================================================
        // ESTADOS DE PEDIDO
        // ============================================================

        private const int ESTADO_PEDIDO_LISTO = 4;
        private const int ESTADO_PEDIDO_ENTREGADO = 5;

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
                .Include(p => p.MovimientoCajas)
                .FirstOrDefaultAsync(p => p.IdPago == id.Value);

            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // ============================================================
        // COMPROBANTE DEL PAGO
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Comprobante(int id)
        {
            var pago = await _context.Pagos
                .Include(p => p.IdFacturaNavigation)
                .Include(p => p.IdMetodoPagoNavigation)
                .Include(p => p.IdMonedaNavigation)
                .Include(p => p.IdBancoNavigation)
                .Include(p => p.IdTipoTarjetaNavigation)
                .Include(p => p.IdEstadoPagoNavigation)
                .Include(p => p.MovimientoCajas)
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (pago == null)
            {
                return NotFound();
            }

            if (pago.IdEstadoPago == ESTADO_PAGO_ANULADO)
            {
                TempData["Error"] = "No se puede imprimir un pago anulado.";
                return RedirectToAction(nameof(Index));
            }

            var factura = await _context.Facturas
                .Include(f => f.IdEstadoFacturaNavigation)
                .FirstOrDefaultAsync(f => f.IdFactura == pago.IdFactura);

            if (factura == null)
            {
                return NotFound();
            }

            ViewBag.Factura = factura;

            return View(pago);
        }

        // ============================================================
        // CREATE GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Create(int? idCaja)
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

            if (idCaja.HasValue &&
                idCaja.Value != caja.IdCaja)
            {
                TempData["Error"] =
                    "La caja seleccionada no corresponde a tu caja abierta.";

                return RedirectToAction(
                    "Details",
                    "Caja",
                    new { id = caja.IdCaja });
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
        public async Task<IActionResult> Create(
            Pago pago,
            int? idCaja)
        {
            int idUsuario = ObtenerUsuarioActual();

            // ========================================================
            // USUARIO
            // ========================================================

            if (idUsuario <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo identificar al usuario actual.");
            }

            // ========================================================
            // CAJA ABIERTA
            // ========================================================

            Caja? caja = null;

            if (idUsuario > 0)
            {
                caja = await ObtenerCajaAbierta(idUsuario);
            }

            if (caja == null)
            {
                ModelState.AddModelError(
                    "",
                    "No tienes una caja abierta. Debes abrir una caja antes de registrar el pago.");
            }

            if (idCaja.HasValue &&
                caja != null &&
                idCaja.Value != caja.IdCaja)
            {
                ModelState.AddModelError(
                    "",
                    "La caja seleccionada no corresponde a tu caja abierta.");
            }

            // ========================================================
            // FACTURA
            // ========================================================

            Factura? factura = null;

            if (pago.IdFactura <= 0)
            {
                ModelState.AddModelError(
                    "IdFactura",
                    "Debe seleccionar una factura.");
            }
            else
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

            // ========================================================
            // MÉTODO DE PAGO
            // ========================================================

            MetodoPago? metodoPago = null;

            if (pago.IdMetodoPago <= 0)
            {
                ModelState.AddModelError(
                    "IdMetodoPago",
                    "Debe seleccionar un método de pago.");
            }
            else
            {
                metodoPago = await _context.MetodoPagos
                    .FirstOrDefaultAsync(m =>
                        m.IdMetodoPago == pago.IdMetodoPago);

                if (metodoPago == null)
                {
                    ModelState.AddModelError(
                        "IdMetodoPago",
                        "El método de pago seleccionado no existe.");
                }
                else if (!metodoPago.Estado)
                {
                    ModelState.AddModelError(
                        "IdMetodoPago",
                        "El método de pago seleccionado está inactivo.");
                }
            }

            // ========================================================
            // MONEDA
            // ========================================================

            Monedum? moneda = null;

            if (pago.IdMoneda <= 0)
            {
                ModelState.AddModelError(
                    "IdMoneda",
                    "Debe seleccionar una moneda.");
            }
            else
            {
                moneda = await _context.Moneda
                    .FirstOrDefaultAsync(m =>
                        m.IdMoneda == pago.IdMoneda);

                if (moneda == null)
                {
                    ModelState.AddModelError(
                        "IdMoneda",
                        "La moneda seleccionada no existe.");
                }
                else if (!moneda.Estado)
                {
                    ModelState.AddModelError(
                        "IdMoneda",
                        "La moneda seleccionada está inactiva.");
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
            // MONTO CONTRA SALDO
            // ========================================================

            if (factura != null)
            {
                if (factura.IdEstadoFactura ==
                    ESTADO_FACTURA_ANULADA)
                {
                    ModelState.AddModelError(
                        "IdFactura",
                        "No se puede registrar un pago para una factura anulada.");
                }

                if (factura.SaldoPendiente <= 0)
                {
                    ModelState.AddModelError(
                        "IdFactura",
                        "La factura ya está completamente pagada.");
                }

                if (pago.Monto > factura.SaldoPendiente)
                {
                    ModelState.AddModelError(
                        "Monto",
                        $"El monto ({pago.Monto:N2}) no puede ser mayor al saldo pendiente ({factura.SaldoPendiente:N2}).");
                }

                if (pago.IdMoneda != factura.IdMoneda)
                {
                    ModelState.AddModelError(
                        "IdMoneda",
                        "La moneda del pago debe coincidir con la moneda de la factura.");
                }
            }

            // ========================================================
            // MONTO RECIBIDO
            // ========================================================

            if (pago.MontoRecibido.HasValue)
            {
                if (pago.MontoRecibido.Value < 0)
                {
                    ModelState.AddModelError(
                        "MontoRecibido",
                        "El monto recibido no puede ser negativo.");
                }
                else if (
                    pago.MontoRecibido.Value > 0 &&
                    pago.MontoRecibido.Value < pago.Monto)
                {
                    ModelState.AddModelError(
                        "MontoRecibido",
                        "El monto recibido no puede ser menor que el monto del pago.");
                }
            }

            // ========================================================
            // FECHA
            // ========================================================

            if (pago.FechaPago == default)
            {
                pago.FechaPago = DateTime.Now;
            }

            // ========================================================
            // ESTADO DE PAGO
            // ========================================================

            pago.IdEstadoPago = ESTADO_PAGO_CONFIRMADO;

            var estadoPago = await _context.EstadoPagos
                .FirstOrDefaultAsync(e =>
                    e.IdEstadoPago ==
                    ESTADO_PAGO_CONFIRMADO &&
                    e.Estado);

            if (estadoPago == null)
            {
                ModelState.AddModelError(
                    "",
                    "No existe un estado activo para pagos confirmados. Se esperaba IdEstadoPago = 2.");
            }

            // ========================================================
            // BANCO
            // ========================================================

            if (pago.IdBanco.HasValue)
            {
                var banco = await _context.Bancos
                    .FirstOrDefaultAsync(b =>
                        b.IdBanco == pago.IdBanco.Value);

                if (banco == null)
                {
                    ModelState.AddModelError(
                        "IdBanco",
                        "El banco seleccionado no existe.");
                }
                else if (!banco.Estado)
                {
                    ModelState.AddModelError(
                        "IdBanco",
                        "El banco seleccionado está inactivo.");
                }
            }

            // ========================================================
            // TIPO DE TARJETA
            // ========================================================

            if (pago.IdTipoTarjeta.HasValue)
            {
                var tipoTarjeta = await _context.TipoTarjeta
                    .FirstOrDefaultAsync(t =>
                        t.IdTipoTarjeta ==
                        pago.IdTipoTarjeta.Value);

                if (tipoTarjeta == null)
                {
                    ModelState.AddModelError(
                        "IdTipoTarjeta",
                        "El tipo de tarjeta seleccionado no existe.");
                }
                else if (!tipoTarjeta.Estado)
                {
                    ModelState.AddModelError(
                        "IdTipoTarjeta",
                        "El tipo de tarjeta seleccionado está inactivo.");
                }
            }

            // ========================================================
            // VALIDACIÓN FINAL
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
            // TIPO MOVIMIENTO INGRESO
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
                ViewBag.Factura = factura;

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
                // CAJA
                // ----------------------------------------------------

                if (caja == null)
                {
                    throw new InvalidOperationException(
                        "No se encontró la caja abierta.");
                }

                if (!await EsCajaAbiertaAsync(caja))
                {
                    throw new InvalidOperationException(
                        "La caja ya no se encuentra abierta.");
                }

                // ----------------------------------------------------
                // FACTURA
                // ----------------------------------------------------

                factura = await _context.Facturas
                    .FirstOrDefaultAsync(f =>
                        f.IdFactura == pago.IdFactura);

                if (factura == null)
                {
                    throw new InvalidOperationException(
                        "No se encontró la factura asociada al pago.");
                }

                if (factura.IdEstadoFactura ==
                    ESTADO_FACTURA_ANULADA)
                {
                    throw new InvalidOperationException(
                        "La factura está anulada.");
                }

                if (factura.SaldoPendiente <= 0)
                {
                    throw new InvalidOperationException(
                        "La factura ya está completamente pagada.");
                }

                if (pago.Monto <= 0)
                {
                    throw new InvalidOperationException(
                        "El monto del pago debe ser mayor que cero.");
                }

                if (pago.Monto >
                    factura.SaldoPendiente)
                {
                    throw new InvalidOperationException(
                        $"El monto ({pago.Monto:N2}) no puede ser mayor al saldo pendiente ({factura.SaldoPendiente:N2}).");
                }

                if (pago.IdMoneda != factura.IdMoneda)
                {
                    throw new InvalidOperationException(
                        "La moneda del pago no coincide con la moneda de la factura.");
                }

                // ----------------------------------------------------
                // MONTO RECIBIDO
                // ----------------------------------------------------

                if (pago.MontoRecibido.HasValue &&
                    pago.MontoRecibido.Value > 0 &&
                    pago.MontoRecibido.Value < pago.Monto)
                {
                    throw new InvalidOperationException(
                        "El monto recibido no puede ser menor que el monto del pago.");
                }

                // ----------------------------------------------------
                // ESTADO CONFIRMADO
                // ----------------------------------------------------

                pago.IdEstadoPago =
                    ESTADO_PAGO_CONFIRMADO;

                // ----------------------------------------------------
                // CREAR PAGO
                // ----------------------------------------------------

                _context.Pagos.Add(pago);

                await _context.SaveChangesAsync();

                // ----------------------------------------------------
                // ACTUALIZAR SALDO DE FACTURA
                // ----------------------------------------------------

                factura.SaldoPendiente -=
                    pago.Monto;

                if (factura.SaldoPendiente < 0)
                {
                    factura.SaldoPendiente = 0;
                }

                ActualizarEstadoFactura(factura);

                // ----------------------------------------------------
                // ACTUALIZAR PEDIDO
                // ----------------------------------------------------

                await ActualizarEstadoPedidoPorFactura(factura);

                // ----------------------------------------------------
                // CREAR MOVIMIENTO DE CAJA
                // ----------------------------------------------------

                var movimiento = new MovimientoCaja
                {
                    IdCaja = caja.IdCaja,

                    IdTipoMovimiento =
                        tipoIngreso.IdTipoMovimiento,

                    IdPago = pago.IdPago,

                    IdMoneda = pago.IdMoneda,

                    Monto = pago.Monto,

                    Descripcion =
                        $"Pago #{pago.IdPago} - Factura #{pago.IdFactura}",

                    FechaMovimiento =
                        pago.FechaPago,

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
                // HISTORIAL DE FACTURA
                // ----------------------------------------------------

                decimal cambio = 0;

                if (pago.MontoRecibido.HasValue &&
                    pago.MontoRecibido.Value > pago.Monto)
                {
                    cambio =
                        pago.MontoRecibido.Value -
                        pago.Monto;
                }

                string observacion =
                    $"Pago #{pago.IdPago} registrado. " +
                    $"Monto aplicado: {pago.Monto:N2}. " +
                    $"Saldo pendiente: {factura.SaldoPendiente:N2}.";

                if (pago.MontoRecibido.HasValue &&
                    pago.MontoRecibido.Value > 0)
                {
                    observacion +=
                        $" Monto recibido: {pago.MontoRecibido.Value:N2}.";

                    if (cambio > 0)
                    {
                        observacion +=
                            $" Cambio entregado: {cambio:N2}.";
                    }
                }

                var historial = new HistorialFactura
                {
                    IdFactura =
                        factura.IdFactura,

                    IdEstadoFactura =
                        factura.IdEstadoFactura,

                    IdUsuario =
                        idUsuario,

                    Observacion =
                        observacion,

                    Fecha =
                        DateTime.Now
                };

                _context.HistorialFacturas.Add(historial);

                // ----------------------------------------------------
                // GUARDAR TODO
                // ----------------------------------------------------

                await _context.SaveChangesAsync();

                // ----------------------------------------------------
                // COMMIT
                // ----------------------------------------------------

                await transaction.CommitAsync();

                // ====================================================
                // AQUÍ ESTÁ EL CAMBIO IMPORTANTE
                // ====================================================

                return RedirectToAction(
                    nameof(Comprobante),
                    new
                    {
                        id = pago.IdPago
                    });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    $"Error al registrar el pago: {ex.Message}" +
                    (ex.InnerException != null
                        ? $" | Detalle: {ex.InnerException.Message}"
                        : ""));

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

            var caja =
                await ObtenerCajaAbierta(idUsuario);

            if (caja == null)
            {
                TempData["Error"] =
                    "Debes abrir una caja antes de registrar un pago.";

                return RedirectToAction(
                    "Details",
                    "Factura",
                    new { id });
            }

            var factura =
                await _context.Facturas
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
                IdFactura =
                    factura.IdFactura,

                IdMoneda =
                    factura.IdMoneda,

                IdEstadoPago =
                    ESTADO_PAGO_CONFIRMADO,

                FechaPago =
                    DateTime.Now,

                Monto =
                    factura.SaldoPendiente
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

            var movimiento =
                await _context.MovimientoCajas
                    .FirstOrDefaultAsync(m =>
                        m.IdPago == pago.IdPago &&
                        !m.Anulado);

            if (movimiento == null)
            {
                TempData["Error"] =
                    "No se encontró el movimiento de caja asociado al pago.";

                return RedirectToAction(nameof(Index));
            }

            var caja =
                await _context.Cajas
                    .Include(c =>
                        c.IdEstadoCajaNavigation)
                    .FirstOrDefaultAsync(c =>
                        c.IdCaja == movimiento.IdCaja);

            if (caja == null)
            {
                TempData["Error"] =
                    "No se encontró la caja asociada al pago.";

                return RedirectToAction(nameof(Index));
            }

            if (!await EsCajaAbiertaAsync(caja))
            {
                TempData["Error"] =
                    "No se puede editar un pago cuya caja ya está cerrada.";

                return RedirectToAction(nameof(Index));
            }

            await CargarListas(pago);

            ViewBag.Caja = caja;
            ViewBag.Factura = pago.IdFacturaNavigation;

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

            var pagoOriginal =
                await _context.Pagos
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

            var factura =
                await _context.Facturas
                    .FirstOrDefaultAsync(f =>
                        f.IdFactura ==
                        pagoOriginal.IdFactura);

            if (factura == null)
            {
                ModelState.AddModelError(
                    "",
                    "No se encontró la factura asociada al pago.");
            }

            if (pago.IdFactura !=
                pagoOriginal.IdFactura)
            {
                ModelState.AddModelError(
                    "IdFactura",
                    "No se puede cambiar la factura de un pago existente. Anule el pago y registre uno nuevo.");
            }

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
                cajaOriginal =
                    await _context.Cajas
                        .Include(c =>
                            c.IdEstadoCajaNavigation)
                        .FirstOrDefaultAsync(c =>
                            c.IdCaja ==
                            movimientoOriginal.IdCaja);

                if (cajaOriginal == null)
                {
                    ModelState.AddModelError(
                        "",
                        "No se encontró la caja asociada al pago.");
                }
            }

            if (cajaOriginal != null &&
                !await EsCajaAbiertaAsync(cajaOriginal))
            {
                ModelState.AddModelError(
                    "",
                    "No se puede editar un pago cuya caja ya está cerrada.");
            }

            if (pago.Monto <= 0)
            {
                ModelState.AddModelError(
                    "Monto",
                    "El monto debe ser mayor que cero.");
            }

            if (pago.MontoRecibido.HasValue)
            {
                if (pago.MontoRecibido.Value < 0)
                {
                    ModelState.AddModelError(
                        "MontoRecibido",
                        "El monto recibido no puede ser negativo.");
                }
                else if (
                    pago.MontoRecibido.Value > 0 &&
                    pago.MontoRecibido.Value < pago.Monto)
                {
                    ModelState.AddModelError(
                        "MontoRecibido",
                        "El monto recibido no puede ser menor que el monto del pago.");
                }
            }

            if (pago.FechaPago == default)
            {
                pago.FechaPago =
                    pagoOriginal.FechaPago;
            }

            var moneda =
                await _context.Moneda
                    .FirstOrDefaultAsync(m =>
                        m.IdMoneda == pago.IdMoneda);

            if (moneda == null)
            {
                ModelState.AddModelError(
                    "IdMoneda",
                    "La moneda seleccionada no existe.");
            }
            else if (!moneda.Estado)
            {
                ModelState.AddModelError(
                    "IdMoneda",
                    "La moneda seleccionada está inactiva.");
            }

            var metodoPago =
                await _context.MetodoPagos
                    .FirstOrDefaultAsync(m =>
                        m.IdMetodoPago ==
                        pago.IdMetodoPago);

            if (metodoPago == null)
            {
                ModelState.AddModelError(
                    "IdMetodoPago",
                    "El método de pago seleccionado no existe.");
            }
            else if (!metodoPago.Estado)
            {
                ModelState.AddModelError(
                    "IdMetodoPago",
                    "El método de pago seleccionado está inactivo.");
            }

            if (pago.IdBanco.HasValue)
            {
                var banco =
                    await _context.Bancos
                        .FirstOrDefaultAsync(b =>
                            b.IdBanco == pago.IdBanco.Value);

                if (banco == null)
                {
                    ModelState.AddModelError(
                        "IdBanco",
                        "El banco seleccionado no existe.");
                }
                else if (!banco.Estado)
                {
                    ModelState.AddModelError(
                        "IdBanco",
                        "El banco seleccionado está inactivo.");
                }
            }

            if (pago.IdTipoTarjeta.HasValue)
            {
                var tipoTarjeta =
                    await _context.TipoTarjeta
                        .FirstOrDefaultAsync(t =>
                            t.IdTipoTarjeta ==
                            pago.IdTipoTarjeta.Value);

                if (tipoTarjeta == null)
                {
                    ModelState.AddModelError(
                        "IdTipoTarjeta",
                        "El tipo de tarjeta seleccionado no existe.");
                }
                else if (!tipoTarjeta.Estado)
                {
                    ModelState.AddModelError(
                        "IdTipoTarjeta",
                        "El tipo de tarjeta seleccionado está inactivo.");
                }
            }

            pago.IdEstadoPago =
                ESTADO_PAGO_CONFIRMADO;

            var estadoPago =
                await _context.EstadoPagos
                    .FirstOrDefaultAsync(e =>
                        e.IdEstadoPago ==
                        ESTADO_PAGO_CONFIRMADO &&
                        e.Estado);

            if (estadoPago == null)
            {
                ModelState.AddModelError(
                    "",
                    "No existe un estado activo para pagos confirmados.");
            }

            if (factura != null)
            {
                if (pago.IdMoneda != factura.IdMoneda)
                {
                    ModelState.AddModelError(
                        "IdMoneda",
                        "La moneda del pago debe coincidir con la moneda de la factura.");
                }

                if (factura.IdEstadoFactura ==
                    ESTADO_FACTURA_ANULADA)
                {
                    ModelState.AddModelError(
                        "IdFactura",
                        "No se puede modificar un pago de una factura anulada.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarListas(pago);

                ViewBag.Caja = cajaOriginal;

                if (factura != null)
                {
                    ViewBag.Factura = factura;
                }

                return View(pago);
            }

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

                if (!await EsCajaAbiertaAsync(cajaOriginal))
                {
                    throw new InvalidOperationException(
                        "La caja asociada al pago ya está cerrada.");
                }

                decimal montoAnterior =
                    pagoOriginal.Monto;

                factura.SaldoPendiente +=
                    montoAnterior;

                if (factura.SaldoPendiente >
                    factura.Total)
                {
                    factura.SaldoPendiente =
                        factura.Total;
                }

                if (pago.Monto <= 0)
                {
                    throw new InvalidOperationException(
                        "El monto debe ser mayor que cero.");
                }

                if (pago.Monto >
                    factura.SaldoPendiente)
                {
                    throw new InvalidOperationException(
                        $"El nuevo monto ({pago.Monto:N2}) no puede ser mayor al saldo disponible ({factura.SaldoPendiente:N2}).");
                }

                if (pago.IdMoneda != factura.IdMoneda)
                {
                    throw new InvalidOperationException(
                        "La moneda del pago no coincide con la moneda de la factura.");
                }

                if (pago.MontoRecibido.HasValue &&
                    pago.MontoRecibido.Value > 0 &&
                    pago.MontoRecibido.Value < pago.Monto)
                {
                    throw new InvalidOperationException(
                        "El monto recibido no puede ser menor que el monto del pago.");
                }

                factura.SaldoPendiente -=
                    pago.Monto;

                if (factura.SaldoPendiente < 0)
                {
                    factura.SaldoPendiente = 0;
                }

                ActualizarEstadoFactura(factura);

                await ActualizarEstadoPedidoPorFactura(factura);

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

                movimientoOriginal.IdMoneda =
                    pago.IdMoneda;

                movimientoOriginal.Monto =
                    pago.Monto;

                movimientoOriginal.FechaMovimiento =
                    pago.FechaPago;

                movimientoOriginal.Descripcion =
                    $"Pago #{pagoOriginal.IdPago} - Factura #{pagoOriginal.IdFactura}";

                await ActualizarTotalesCaja(
                    cajaOriginal);

                decimal cambio = 0;

                if (pago.MontoRecibido.HasValue &&
                    pago.MontoRecibido.Value > pago.Monto)
                {
                    cambio =
                        pago.MontoRecibido.Value -
                        pago.Monto;
                }

                string observacion =
                    $"Pago #{pagoOriginal.IdPago} modificado. " +
                    $"Monto anterior: {montoAnterior:N2}. " +
                    $"Nuevo monto: {pago.Monto:N2}. " +
                    $"Saldo pendiente: {factura.SaldoPendiente:N2}.";

                if (pago.MontoRecibido.HasValue &&
                    pago.MontoRecibido.Value > 0)
                {
                    observacion +=
                        $" Monto recibido: {pago.MontoRecibido.Value:N2}.";

                    if (cambio > 0)
                    {
                        observacion +=
                            $" Cambio entregado: {cambio:N2}.";
                    }
                }

                var historial =
                    new HistorialFactura
                    {
                        IdFactura =
                            factura.IdFactura,

                        IdEstadoFactura =
                            factura.IdEstadoFactura,

                        IdUsuario =
                            idUsuario,

                        Observacion =
                            observacion,

                        Fecha =
                            DateTime.Now
                    };

                _context.HistorialFacturas.Add(
                    historial);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return RedirectToAction(
                    nameof(Comprobante),
                    new
                    {
                        id = pagoOriginal.IdPago
                    });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    $"Error al modificar el pago: {ex.Message}" +
                    (ex.InnerException != null
                        ? $" | Detalle: {ex.InnerException.Message}"
                        : ""));

                await CargarListas(pago);

                ViewBag.Caja = cajaOriginal;

                if (factura != null)
                {
                    ViewBag.Factura = factura;
                }

                return View(pago);
            }
        }

        // ============================================================
        // DELETE GET
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
                .Include(p => p.IdEstadoPagoNavigation)
                .Include(p => p.IdMonedaNavigation)
                .Include(p => p.IdBancoNavigation)
                .Include(p => p.IdTipoTarjetaNavigation)
                .Include(p => p.IdMetodoPagoNavigation)
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
        // DELETE POST / ANULAR PAGO
        // ============================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            int idUsuario = ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                TempData["Error"] =
                    "No se pudo identificar al usuario actual.";

                return RedirectToAction(nameof(Index));
            }

            var pago =
                await _context.Pagos
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

            var movimiento =
                await _context.MovimientoCajas
                    .FirstOrDefaultAsync(m =>
                        m.IdPago == pago.IdPago &&
                        !m.Anulado);

            if (movimiento == null)
            {
                TempData["Error"] =
                    "No se encontró el movimiento de caja asociado al pago.";

                return RedirectToAction(nameof(Index));
            }

            var caja =
                await _context.Cajas
                    .Include(c =>
                        c.IdEstadoCajaNavigation)
                    .FirstOrDefaultAsync(c =>
                        c.IdCaja == movimiento.IdCaja);

            if (caja == null)
            {
                TempData["Error"] =
                    "No se encontró la caja asociada al pago.";

                return RedirectToAction(nameof(Index));
            }

            if (!await EsCajaAbiertaAsync(caja))
            {
                TempData["Error"] =
                    "No se puede anular un pago de una caja cerrada.";

                return RedirectToAction(nameof(Index));
            }

            var factura =
                await _context.Facturas
                    .FirstOrDefaultAsync(f =>
                        f.IdFactura ==
                        pago.IdFactura);

            if (factura == null)
            {
                TempData["Error"] =
                    "No se encontró la factura asociada al pago.";

                return RedirectToAction(nameof(Index));
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                if (!await EsCajaAbiertaAsync(caja))
                {
                    throw new InvalidOperationException(
                        "La caja ya está cerrada.");
                }

                pago.IdEstadoPago =
                    ESTADO_PAGO_ANULADO;

                movimiento.Anulado = true;

                movimiento.FechaAnulacion =
                    DateTime.Now;

                movimiento.IdUsuarioAnulo =
                    idUsuario;

                movimiento.MotivoAnulacion =
                    $"Anulación del pago #{pago.IdPago}";

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

                    await ActualizarEstadoPedidoPorFactura(
                        factura);

                    decimal cambio = 0;

                    if (pago.MontoRecibido.HasValue &&
                        pago.MontoRecibido.Value > pago.Monto)
                    {
                        cambio =
                            pago.MontoRecibido.Value -
                            pago.Monto;
                    }

                    string observacion =
                        $"Pago #{pago.IdPago} anulado. " +
                        $"Monto devuelto al saldo: {pago.Monto:N2}. " +
                        $"Saldo pendiente: {factura.SaldoPendiente:N2}.";

                    if (cambio > 0)
                    {
                        observacion +=
                            $" El pago original había recibido " +
                            $"{pago.MontoRecibido!.Value:N2} " +
                            $"y entregado cambio de {cambio:N2}.";
                    }

                    var historial =
                        new HistorialFactura
                        {
                            IdFactura =
                                factura.IdFactura,

                            IdEstadoFactura =
                                factura.IdEstadoFactura,

                            IdUsuario =
                                idUsuario,

                            Observacion =
                                observacion,

                            Fecha =
                                DateTime.Now
                        };

                    _context.HistorialFacturas.Add(
                        historial);
                }

                await ActualizarTotalesCaja(caja);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Pago #{pago.IdPago} anulado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    $"Error al anular el pago: {ex.Message}" +
                    (ex.InnerException != null
                        ? $" | Detalle: {ex.InnerException.Message}"
                        : "");

                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // PAGOS POR FACTURA
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> PorFactura(int id)
        {
            var factura =
                await _context.Facturas
                    .Include(f =>
                        f.IdEstadoFacturaNavigation)
                    .FirstOrDefaultAsync(f =>
                        f.IdFactura == id);

            if (factura == null)
            {
                return NotFound();
            }

            var pagos =
                await _context.Pagos
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

        [HttpGet]
        public async Task<IActionResult> BuscarPago(
            string? buscar,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? idCaja)
        {
            var query =
                _context.Pagos
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
                        p.IdMetodoPagoNavigation.Nombre
                            .Contains(buscar));
                }
            }

            if (fechaInicio.HasValue)
            {
                var fecha =
                    fechaInicio.Value.Date;

                query = query.Where(p =>
                    p.FechaPago >= fecha);
            }

            if (fechaFin.HasValue)
            {
                var fechaHasta =
                    fechaFin.Value.Date.AddDays(1);

                query = query.Where(p =>
                    p.FechaPago < fechaHasta);
            }

            if (idCaja.HasValue)
            {
                query = query.Where(p =>
                    p.MovimientoCajas
                        .Any(m =>
                            m.IdCaja == idCaja.Value &&
                            !m.Anulado));
            }

            var pagos =
                await query
                    .OrderByDescending(p =>
                        p.FechaPago)
                    .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.IdCaja = idCaja;

            if (idCaja.HasValue)
            {
                var caja =
                    await _context.Cajas
                        .Include(c =>
                            c.IdEstadoCajaNavigation)
                        .FirstOrDefaultAsync(c =>
                            c.IdCaja ==
                            idCaja.Value);

                ViewBag.Caja = caja;
            }

            return View(pagos);
        }

        // ============================================================
        // CARGAR LISTAS
        // ============================================================

        private async Task CargarListas(
            Pago? pago = null)
        {
            var facturas =
                await _context.Facturas
                    .Where(f =>
                        f.IdEstadoFactura !=
                            ESTADO_FACTURA_ANULADA &&
                        f.SaldoPendiente > 0)
                    .OrderByDescending(f =>
                        f.FechaFactura)
                    .ToListAsync();

            if (pago != null &&
                pago.IdFactura > 0)
            {
                var facturaActual =
                    await _context.Facturas
                        .FirstOrDefaultAsync(f =>
                            f.IdFactura ==
                            pago.IdFactura);

                if (facturaActual != null &&
                    !facturas.Any(f =>
                        f.IdFactura ==
                        facturaActual.IdFactura))
                {
                    facturas.Add(facturaActual);
                }
            }

            ViewBag.Facturas = facturas;

            ViewBag.MetodosPago =
                await _context.MetodoPagos
                    .Where(m => m.Estado)
                    .OrderBy(m => m.Nombre)
                    .ToListAsync();

            ViewBag.Monedas =
                await _context.Moneda
                    .Where(m => m.Estado)
                    .OrderBy(m => m.Nombre)
                    .ToListAsync();

            ViewBag.Bancos =
                await _context.Bancos
                    .Where(b => b.Estado)
                    .OrderBy(b => b.Nombre)
                    .ToListAsync();

            ViewBag.TiposTarjeta =
                await _context.TipoTarjeta
                    .Where(t => t.Estado)
                    .OrderBy(t => t.Nombre)
                    .ToListAsync();

            ViewBag.EstadosPago =
                await _context.EstadoPagos
                    .Where(e => e.Estado)
                    .OrderBy(e => e.IdEstadoPago)
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
        // ACTUALIZAR ESTADO PEDIDO SEGÚN FACTURA
        // ============================================================

        private async Task ActualizarEstadoPedidoPorFactura(
            Factura factura)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == factura.IdPedido);

            if (pedido == null)
            {
                throw new InvalidOperationException(
                    "No se encontró el pedido asociado a la factura.");
            }

            if (factura.IdEstadoFactura ==
                ESTADO_FACTURA_PAGADA)
            {
                if (pedido.IdEstadoPedido !=
                    ESTADO_PEDIDO_LISTO)
                {
                    throw new InvalidOperationException(
                        "El pedido debe estar en estado LISTO para poder marcarlo como ENTREGADO.");
                }

                pedido.IdEstadoPedido =
                    ESTADO_PEDIDO_ENTREGADO;

                _context.Pedidos.Update(pedido);

                return;
            }

            if (pedido.IdEstadoPedido ==
                ESTADO_PEDIDO_ENTREGADO)
            {
                pedido.IdEstadoPedido =
                    ESTADO_PEDIDO_LISTO;

                _context.Pedidos.Update(pedido);
            }
        }

        // ============================================================
        // ACTUALIZAR TOTALES CAJA
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
                        m.IdCaja ==
                            caja.IdCaja &&
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
                else
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

                if (codigoMoneda == "NIO")
                {
                    if (esIngreso)
                    {
                        caja.TotalIngresosCordobas +=
                            movimiento.Monto;
                    }
                    else
                    {
                        caja.TotalEgresosCordobas +=
                            movimiento.Monto;
                    }
                }
                else if (codigoMoneda == "USD")
                {
                    if (esIngreso)
                    {
                        caja.TotalIngresosDolares +=
                            movimiento.Monto;
                    }
                    else
                    {
                        caja.TotalEgresosDolares +=
                            movimiento.Monto;
                    }
                }
            }

            caja.TotalSistema =
                caja.MontoInicial +
                caja.TotalIngresos -
                caja.TotalEgresos;

            caja.TotalSistemaCordobas =
                caja.MontoInicialCordobas +
                caja.TotalIngresosCordobas -
                caja.TotalEgresosCordobas;

            caja.TotalSistemaDolares =
                caja.MontoInicialDolares +
                caja.TotalIngresosDolares -
                caja.TotalEgresosDolares;
        }

        // ============================================================
        // ES INGRESO
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
        // ES EGRESO
        // ============================================================

        private bool EsEgreso(string tipo)
        {
            return tipo == "egreso"
                || tipo == "salida"
                || tipo == "retiro"
                || tipo == "gasto";
        }

        // ============================================================
        // CAJA ABIERTA DEL USUARIO
        // ============================================================

        private async Task<Caja?>
            ObtenerCajaAbiertaUsuario()
        {
            int idUsuario =
                ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                return null;
            }

            return await ObtenerCajaAbierta(
                idUsuario);
        }

        // ============================================================
        // VALIDAR CAJA ABIERTA
        // ============================================================

        private async Task<bool>
            EsCajaAbiertaAsync(
                Caja caja)
        {
            var estadoAbierta =
                await ObtenerEstadoCaja("Abierta");

            if (estadoAbierta == null)
            {
                return false;
            }

            return caja.IdEstadoCaja ==
                       estadoAbierta.IdEstadoCaja
                   &&
                   caja.FechaCierre == null;
        }

        // ============================================================
        // USUARIO ACTUAL
        // ============================================================

        private int ObtenerUsuarioActual()
        {
            var claim =
                User.FindFirst("IdUsuario")
                ??
                User.FindFirst(
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