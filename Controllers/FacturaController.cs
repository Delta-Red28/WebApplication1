using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class FacturaController : Controller
    {
        private readonly RestauranteContext _context;

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

        private const int ESTADO_PEDIDO_FACTURADO = 6;
        private const int ESTADO_PEDIDO_CANCELADO = 7;

        // ============================================================
        // ESTADOS DE PAGO
        // ============================================================

        private const int ESTADO_PAGO_CONFIRMADO = 2;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public FacturaController(RestauranteContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var facturas = await _context.Facturas
                .Include(f => f.IdPedidoNavigation)
                .Include(f => f.IdEstadoFacturaNavigation)
                .Include(f => f.IdMonedaNavigation)
                .Include(f => f.IdTipoComprobanteNavigation)
                .Include(f => f.IdSerieFacturaNavigation)
                .OrderByDescending(f => f.FechaFactura)
                .ToListAsync();

            return View(facturas);
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

            var factura = await _context.Facturas
                .Include(f => f.IdPedidoNavigation)
                .Include(f => f.IdEstadoFacturaNavigation)
                .Include(f => f.IdMonedaNavigation)
                .Include(f => f.IdTipoComprobanteNavigation)
                .Include(f => f.IdSerieFacturaNavigation)
                .Include(f => f.IdMotivoAnulacionNavigation)
                .Include(f => f.IdUsuarioAnuloNavigation)
                .Include(f => f.IdImpuestoNavigation)
                .Include(f => f.Pagos)
                    .ThenInclude(p => p.IdMetodoPagoNavigation)
                .Include(f => f.Pagos)
                    .ThenInclude(p => p.IdEstadoPagoNavigation)
                .Include(f => f.Pagos)
                    .ThenInclude(p => p.IdMonedaNavigation)
                .FirstOrDefaultAsync(f =>
                    f.IdFactura == id.Value);

            if (factura == null)
            {
                return NotFound();
            }

            var historial = await _context.HistorialFacturas
                .Include(h => h.IdEstadoFacturaNavigation)
                .Include(h => h.IdUsuarioNavigation)
                .Where(h => h.IdFactura == factura.IdFactura)
                .OrderByDescending(h => h.Fecha)
                .ToListAsync();

            ViewBag.Historial = historial;

            return View(factura);
        }

        // ============================================================
        // CREATE GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var usuario = ObtenerUsuarioActual();

            if (usuario <= 0)
            {
                TempData["Error"] =
                    "No se pudo identificar al usuario actual.";

                return RedirectToAction("Index", "Home");
            }

            var factura = new Factura
            {
                IdEstadoFactura = ESTADO_FACTURA_PENDIENTE,
                FechaFactura = DateTime.Now,
                SaldoPendiente = 0m
            };

            await CargarListas(factura);

            return View(factura);
        }

        // ============================================================
        // CREATE POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Factura factura)
        {
            var idUsuario = ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo identificar al usuario actual.");
            }

            // ========================================================
            // PEDIDO
            // ========================================================

            Pedido? pedido = null;

            if (factura.IdPedido > 0)
            {
                pedido = await _context.Pedidos
                    .Include(p => p.IdEstadoPedidoNavigation)
                    .Include(p => p.Factura)
                    .FirstOrDefaultAsync(p =>
                        p.IdPedido == factura.IdPedido);

                if (pedido == null)
                {
                    ModelState.AddModelError(
                        "IdPedido",
                        "El pedido seleccionado no existe.");
                }
            }
            else
            {
                ModelState.AddModelError(
                    "IdPedido",
                    "Debe seleccionar un pedido.");
            }

            // ========================================================
            // VALIDAR ESTADO DEL PEDIDO
            // ========================================================

            if (pedido != null)
            {
                if (pedido.IdEstadoPedido == ESTADO_PEDIDO_CANCELADO)
                {
                    ModelState.AddModelError(
                        "IdPedido",
                        "No se puede crear una factura para un pedido cancelado.");
                }
            }

            // ========================================================
            // VALIDAR QUE NO ESTÉ YA FACTURADO
            // ========================================================

            if (pedido != null)
            {
                var facturaExistente = await _context.Facturas
                    .AnyAsync(f =>
                        f.IdPedido == pedido.IdPedido);

                if (facturaExistente)
                {
                    ModelState.AddModelError(
                        "IdPedido",
                        "El pedido seleccionado ya tiene una factura.");
                }
            }

            // ========================================================
            // MONEDA
            // ========================================================

            Monedum? moneda = null;

            if (factura.IdMoneda > 0)
            {
                moneda = await _context.Moneda
                    .FirstOrDefaultAsync(m =>
                        m.IdMoneda == factura.IdMoneda);

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
            // TIPO COMPROBANTE
            // ========================================================

            TipoComprobante? tipoComprobante = null;

            if (factura.IdTipoComprobante > 0)
            {
                tipoComprobante = await _context.TipoComprobantes
                    .FirstOrDefaultAsync(t =>
                        t.IdTipoComprobante ==
                        factura.IdTipoComprobante);

                if (tipoComprobante == null ||
                    !tipoComprobante.Estado)
                {
                    ModelState.AddModelError(
                        "IdTipoComprobante",
                        "El tipo de comprobante seleccionado no es válido.");
                }
            }
            else
            {
                ModelState.AddModelError(
                    "IdTipoComprobante",
                    "Debe seleccionar un tipo de comprobante.");
            }

            // ========================================================
            // SERIE
            // ========================================================

            SerieFactura? serie = null;

            if (factura.IdSerieFactura > 0)
            {
                serie = await _context.SerieFacturas
                    .FirstOrDefaultAsync(s =>
                        s.IdSerieFactura ==
                        factura.IdSerieFactura);

                if (serie == null || !serie.Estado)
                {
                    ModelState.AddModelError(
                        "IdSerieFactura",
                        "La serie seleccionada no es válida.");
                }
            }
            else
            {
                ModelState.AddModelError(
                    "IdSerieFactura",
                    "Debe seleccionar una serie.");
            }

            // ========================================================
            // IMPUESTO / IVA
            // ========================================================

            Impuesto? impuesto = null;

            if (factura.IdImpuesto.HasValue &&
                factura.IdImpuesto.Value > 0)
            {
                impuesto = await _context.Impuestos
                    .FirstOrDefaultAsync(i =>
                        i.IdImpuesto ==
                        factura.IdImpuesto.Value);

                if (impuesto == null || !impuesto.Estado)
                {
                    ModelState.AddModelError(
                        "IdImpuesto",
                        "El impuesto seleccionado no es válido.");
                }
            }

            // ========================================================
            // FECHA
            // ========================================================

            if (factura.FechaFactura == default)
            {
                factura.FechaFactura = DateTime.Now;
            }

            // ========================================================
            // CALCULAR IMPORTES
            // ========================================================

            if (pedido != null)
            {
                CalcularImportesFactura(
                    factura,
                    pedido,
                    impuesto);
            }

            // ========================================================
            // VALIDAR TOTAL
            // ========================================================

            if (pedido != null && pedido.Total <= 0)
            {
                ModelState.AddModelError(
                    "IdPedido",
                    "El pedido seleccionado no tiene un total válido para facturar.");
            }

            // ========================================================
            // ESTADO FACTURA
            // ========================================================

            factura.IdEstadoFactura =
                ESTADO_FACTURA_PENDIENTE;

            var estadoPendiente =
                await _context.EstadoFacturas
                    .FirstOrDefaultAsync(e =>
                        e.IdEstadoFactura ==
                        ESTADO_FACTURA_PENDIENTE &&
                        e.Estado);

            if (estadoPendiente == null)
            {
                ModelState.AddModelError(
                    "",
                    "No existe un estado activo para facturas pendientes.");
            }

            // ========================================================
            // SI HAY ERRORES
            // ========================================================

            if (!ModelState.IsValid)
            {
                await CargarListas(factura);

                return View(factura);
            }

            // ========================================================
            // TRANSACCIÓN
            // ========================================================

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // ----------------------------------------------------
                // RECARGAR PEDIDO
                // ----------------------------------------------------

                pedido = await _context.Pedidos
                    .Include(p => p.IdEstadoPedidoNavigation)
                    .FirstOrDefaultAsync(p =>
                        p.IdPedido == factura.IdPedido);

                if (pedido == null)
                {
                    throw new InvalidOperationException(
                        "No se encontró el pedido seleccionado.");
                }

                // ----------------------------------------------------
                // VALIDAR CANCELACIÓN NUEVAMENTE
                // ----------------------------------------------------

                if (pedido.IdEstadoPedido ==
                    ESTADO_PEDIDO_CANCELADO)
                {
                    throw new InvalidOperationException(
                        "No se puede facturar un pedido cancelado.");
                }

                // ----------------------------------------------------
                // VALIDAR FACTURA EXISTENTE
                // ----------------------------------------------------

                var yaTieneFactura =
                    await _context.Facturas
                        .AnyAsync(f =>
                            f.IdPedido == pedido.IdPedido);

                if (yaTieneFactura)
                {
                    throw new InvalidOperationException(
                        "El pedido seleccionado ya tiene una factura.");
                }

                // ----------------------------------------------------
                // RECARGAR IMPUESTO
                // ----------------------------------------------------

                impuesto = null;

                if (factura.IdImpuesto.HasValue &&
                    factura.IdImpuesto.Value > 0)
                {
                    impuesto = await _context.Impuestos
                        .FirstOrDefaultAsync(i =>
                            i.IdImpuesto ==
                            factura.IdImpuesto.Value &&
                            i.Estado);

                    if (impuesto == null)
                    {
                        throw new InvalidOperationException(
                            "El impuesto seleccionado no está disponible.");
                    }
                }

                // ----------------------------------------------------
                // CALCULAR IMPORTES DEFINITIVAMENTE
                // ----------------------------------------------------

                CalcularImportesFactura(
                    factura,
                    pedido,
                    impuesto);

                factura.IdEstadoFactura =
                    ESTADO_FACTURA_PENDIENTE;

                factura.IdPedidoNavigation = pedido;

                if (moneda != null)
                {
                    factura.IdMonedaNavigation = moneda;
                }

                if (tipoComprobante != null)
                {
                    factura.IdTipoComprobanteNavigation =
                        tipoComprobante;
                }

                if (serie == null ||
                    !serie.Estado)
                {
                    serie = await _context.SerieFacturas
                        .FirstOrDefaultAsync(s =>
                            s.IdSerieFactura ==
                            factura.IdSerieFactura &&
                            s.Estado);
                }

                if (serie == null)
                {
                    throw new InvalidOperationException(
                        "La serie seleccionada no está disponible.");
                }

                factura.IdSerieFacturaNavigation = serie;

                if (impuesto != null)
                {
                    factura.IdImpuestoNavigation = impuesto;
                }

                // ----------------------------------------------------
                // GENERAR NÚMERO
                // ----------------------------------------------------

                int siguienteNumero =
                    serie.NumeroActual + 1;

                string numero =
                    siguienteNumero
                        .ToString()
                        .PadLeft(
                            serie.LongitudNumero,
                            '0');

                factura.NumeroFactura =
                    $"{serie.Prefijo}{numero}";

                // ----------------------------------------------------
                // VALIDAR NÚMERO
                // ----------------------------------------------------

                bool numeroExiste =
                    await _context.Facturas
                        .AnyAsync(f =>
                            f.NumeroFactura ==
                            factura.NumeroFactura);

                if (numeroExiste)
                {
                    throw new InvalidOperationException(
                        $"El número de factura {factura.NumeroFactura} ya existe.");
                }

                // ----------------------------------------------------
                // ACTUALIZAR CONSECUTIVO
                // ----------------------------------------------------

                serie.NumeroActual =
                    siguienteNumero;

                // ----------------------------------------------------
                // AGREGAR FACTURA
                // ----------------------------------------------------

                _context.Facturas.Add(factura);

                // ----------------------------------------------------
                // MARCAR PEDIDO COMO FACTURADO
                // ----------------------------------------------------

                pedido.IdEstadoPedido =
                    ESTADO_PEDIDO_FACTURADO;

                await _context.SaveChangesAsync();

                // ----------------------------------------------------
                // HISTORIAL
                // ----------------------------------------------------

                var historial = new HistorialFactura
                {
                    IdFactura =
                        factura.IdFactura,

                    IdEstadoFactura =
                        ESTADO_FACTURA_PENDIENTE,

                    IdUsuario =
                        idUsuario,

                    Observacion =
                        $"Factura #{factura.NumeroFactura} creada " +
                        $"para el pedido #{pedido.IdPedido}. " +
                        $"Subtotal: {factura.Subtotal:N2}. " +
                        $"Descuento: {factura.Descuento:N2}. " +
                        $"Impuesto: {factura.Impuesto:N2}. " +
                        $"Total: {factura.Total:N2}.",

                    Fecha =
                        DateTime.Now
                };

                _context.HistorialFacturas.Add(historial);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Factura #{factura.NumeroFactura} creada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = factura.IdFactura
                    });
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    ex.Message);

                await CargarListas(factura);

                return View(factura);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al crear la factura: " +
                    ex.Message);

                await CargarListas(factura);

                return View(factura);
            }
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

            var factura = await _context.Facturas
                .Include(f => f.IdPedidoNavigation)
                .Include(f => f.IdEstadoFacturaNavigation)
                .Include(f => f.IdMonedaNavigation)
                .Include(f => f.IdTipoComprobanteNavigation)
                .Include(f => f.IdSerieFacturaNavigation)
                .Include(f => f.IdImpuestoNavigation)
                .Include(f => f.Pagos)
                .FirstOrDefaultAsync(f =>
                    f.IdFactura == id.Value);

            if (factura == null)
            {
                return NotFound();
            }

            if (factura.IdEstadoFactura ==
                ESTADO_FACTURA_ANULADA)
            {
                TempData["Error"] =
                    "No se puede editar una factura anulada.";

                return RedirectToAction(nameof(Index));
            }

            await CargarListas(factura);

            return View(factura);
        }

        // ============================================================
        // EDIT POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Factura factura)
        {
            if (id != factura.IdFactura)
            {
                return NotFound();
            }

            int idUsuario =
                ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo identificar al usuario actual.");
            }

            var facturaOriginal =
                await _context.Facturas
                    .FirstOrDefaultAsync(f =>
                        f.IdFactura == id);

            if (facturaOriginal == null)
            {
                return NotFound();
            }

            if (facturaOriginal.IdEstadoFactura ==
                ESTADO_FACTURA_ANULADA)
            {
                TempData["Error"] =
                    "No se puede editar una factura anulada.";

                return RedirectToAction(nameof(Index));
            }

            // ========================================================
            // NO CAMBIAR PEDIDO
            // ========================================================

            if (factura.IdPedido !=
                facturaOriginal.IdPedido)
            {
                ModelState.AddModelError(
                    "IdPedido",
                    "No se puede cambiar el pedido de una factura existente.");
            }

            // ========================================================
            // NÚMERO Y SERIE
            // ========================================================

            factura.NumeroFactura =
                facturaOriginal.NumeroFactura;

            factura.IdSerieFactura =
                facturaOriginal.IdSerieFactura;

            // ========================================================
            // PAGOS
            // ========================================================

            var tienePagos =
                await _context.Pagos
                    .AnyAsync(p =>
                        p.IdFactura == id &&
                        p.IdEstadoPago ==
                        ESTADO_PAGO_CONFIRMADO);

            // ========================================================
            // MONEDA
            // ========================================================

            var moneda =
                await _context.Moneda
                    .FirstOrDefaultAsync(m =>
                        m.IdMoneda == factura.IdMoneda);

            if (moneda == null || !moneda.Estado)
            {
                ModelState.AddModelError(
                    "IdMoneda",
                    "La moneda seleccionada no es válida.");
            }

            if (tienePagos &&
                factura.IdMoneda !=
                facturaOriginal.IdMoneda)
            {
                ModelState.AddModelError(
                    "IdMoneda",
                    "No se puede cambiar la moneda de una factura que ya tiene pagos.");
            }

            // ========================================================
            // TIPO COMPROBANTE
            // ========================================================

            var tipoComprobante =
                await _context.TipoComprobantes
                    .FirstOrDefaultAsync(t =>
                        t.IdTipoComprobante ==
                        factura.IdTipoComprobante &&
                        t.Estado);

            if (tipoComprobante == null)
            {
                ModelState.AddModelError(
                    "IdTipoComprobante",
                    "El tipo de comprobante seleccionado no es válido.");
            }

            // ========================================================
            // IMPUESTO
            // ========================================================

            Impuesto? impuesto = null;

            if (factura.IdImpuesto.HasValue &&
                factura.IdImpuesto.Value > 0)
            {
                impuesto =
                    await _context.Impuestos
                        .FirstOrDefaultAsync(i =>
                            i.IdImpuesto ==
                            factura.IdImpuesto.Value &&
                            i.Estado);

                if (impuesto == null)
                {
                    ModelState.AddModelError(
                        "IdImpuesto",
                        "El impuesto seleccionado no es válido.");
                }
            }

            // ========================================================
            // FECHA
            // ========================================================

            if (factura.FechaFactura == default)
            {
                factura.FechaFactura =
                    facturaOriginal.FechaFactura;
            }

            // ========================================================
            // LOS IMPORTES NO VIENEN DEL FORMULARIO
            // ========================================================

            factura.Subtotal =
                facturaOriginal.Subtotal;

            factura.Descuento =
                facturaOriginal.Descuento;

            factura.Impuesto =
                facturaOriginal.Impuesto;

            factura.Total =
                facturaOriginal.Total;

            factura.SaldoPendiente =
                facturaOriginal.SaldoPendiente;

            factura.IdEstadoFactura =
                facturaOriginal.IdEstadoFactura;

            // ========================================================
            // ERRORES
            // ========================================================

            if (!ModelState.IsValid)
            {
                await CargarListas(factura);

                return View(factura);
            }

            // ========================================================
            // ACTUALIZAR
            // ========================================================

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                facturaOriginal.IdMoneda =
                    factura.IdMoneda;

                facturaOriginal.IdTipoComprobante =
                    factura.IdTipoComprobante;

                facturaOriginal.IdImpuesto =
                    factura.IdImpuesto;

                facturaOriginal.FechaFactura =
                    factura.FechaFactura;

                facturaOriginal.Observacion =
                    factura.Observacion;

                await _context.SaveChangesAsync();

                var historial = new HistorialFactura
                {
                    IdFactura =
                        facturaOriginal.IdFactura,

                    IdEstadoFactura =
                        facturaOriginal.IdEstadoFactura,

                    IdUsuario =
                        idUsuario,

                    Observacion =
                        $"Factura #{facturaOriginal.NumeroFactura} modificada.",

                    Fecha =
                        DateTime.Now
                };

                _context.HistorialFacturas.Add(historial);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Factura #{facturaOriginal.NumeroFactura} actualizada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = facturaOriginal.IdFactura
                    });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al modificar la factura.");

                await CargarListas(factura);

                return View(factura);
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

            var factura = await _context.Facturas
                .Include(f => f.IdPedidoNavigation)
                .Include(f => f.IdEstadoFacturaNavigation)
                .Include(f => f.IdMonedaNavigation)
                .Include(f => f.IdTipoComprobanteNavigation)
                .Include(f => f.IdSerieFacturaNavigation)
                .Include(f => f.IdMotivoAnulacionNavigation)
                .FirstOrDefaultAsync(f =>
                    f.IdFactura == id.Value);

            if (factura == null)
            {
                return NotFound();
            }

            if (factura.IdEstadoFactura ==
                ESTADO_FACTURA_ANULADA)
            {
                TempData["Error"] =
                    "La factura ya se encuentra anulada.";

                return RedirectToAction(nameof(Index));
            }

            await CargarListasAnulacion();

            return View(factura);
        }

        // ============================================================
        // DELETE POST
        // ============================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id,
            int idMotivoAnulacion,
            string? observacionAnulacion)
        {
            int idUsuario =
                ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                TempData["Error"] =
                    "No se pudo identificar al usuario actual.";

                return RedirectToAction(nameof(Index));
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
                    "La factura ya se encuentra anulada.";

                return RedirectToAction(nameof(Index));
            }

            // ========================================================
            // MOTIVO
            // ========================================================

            var motivo =
                await _context.MotivoAnulacions
                    .FirstOrDefaultAsync(m =>
                        m.IdMotivoAnulacion ==
                        idMotivoAnulacion &&
                        m.Estado);

            if (motivo == null)
            {
                TempData["Error"] =
                    "Debe seleccionar un motivo de anulación válido.";

                return RedirectToAction(
                    nameof(Delete),
                    new { id });
            }

            // ========================================================
            // PAGOS
            // ========================================================

            var tienePagos =
                await _context.Pagos
                    .AnyAsync(p =>
                        p.IdFactura == factura.IdFactura &&
                        p.IdEstadoPago ==
                        ESTADO_PAGO_CONFIRMADO);

            if (tienePagos)
            {
                TempData["Error"] =
                    "No se puede anular la factura porque tiene pagos confirmados asociados.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                factura.IdEstadoFactura =
                    ESTADO_FACTURA_ANULADA;

                factura.IdMotivoAnulacion =
                    idMotivoAnulacion;

                factura.FechaAnulacion =
                    DateTime.Now;

                factura.IdUsuarioAnulo =
                    idUsuario;

                factura.ObservacionAnulacion =
                    observacionAnulacion;

                var historial = new HistorialFactura
                {
                    IdFactura =
                        factura.IdFactura,

                    IdEstadoFactura =
                        ESTADO_FACTURA_ANULADA,

                    IdUsuario =
                        idUsuario,

                    Observacion =
                        $"Factura #{factura.NumeroFactura} anulada. " +
                        $"Motivo: {motivo.Nombre}. " +
                        $"{observacionAnulacion}",

                    Fecha =
                        DateTime.Now
                };

                _context.HistorialFacturas.Add(historial);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Factura #{factura.NumeroFactura} anulada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Ocurrió un error al anular la factura.";

                return RedirectToAction(
                    nameof(Delete),
                    new { id });
            }
        }

        // ============================================================
        // BUSCAR FACTURA
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> BuscarFactura(
            string? buscar,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? idEstadoFactura)
        {
            var query =
                _context.Facturas
                    .Include(f => f.IdPedidoNavigation)
                    .Include(f => f.IdEstadoFacturaNavigation)
                    .Include(f => f.IdMonedaNavigation)
                    .Include(f => f.IdTipoComprobanteNavigation)
                    .Include(f => f.IdSerieFacturaNavigation)
                    .Include(f => f.IdImpuestoNavigation)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                if (int.TryParse(
                    buscar,
                    out int numero))
                {
                    query = query.Where(f =>
                        f.IdFactura == numero ||
                        f.IdPedido == numero);
                }
                else
                {
                    query = query.Where(f =>
                        f.NumeroFactura.Contains(buscar) ||
                        f.IdPedidoNavigation.NumeroPedido.Contains(buscar));
                }
            }

            if (fechaInicio.HasValue)
            {
                var fecha =
                    fechaInicio.Value.Date;

                query = query.Where(f =>
                    f.FechaFactura >= fecha);
            }

            if (fechaFin.HasValue)
            {
                var fechaHasta =
                    fechaFin.Value.Date.AddDays(1);

                query = query.Where(f =>
                    f.FechaFactura < fechaHasta);
            }

            if (idEstadoFactura.HasValue &&
                idEstadoFactura.Value > 0)
            {
                query = query.Where(f =>
                    f.IdEstadoFactura ==
                    idEstadoFactura.Value);
            }

            var facturas =
                await query
                    .OrderByDescending(f =>
                        f.FechaFactura)
                    .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.IdEstadoFactura = idEstadoFactura;

            ViewBag.EstadosFactura =
                await _context.EstadoFacturas
                    .Where(e => e.Estado)
                    .OrderBy(e =>
                        e.IdEstadoFactura)
                    .ToListAsync();

            return View(facturas);
        }

        // ============================================================
        // HISTORIAL
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Historial(int id)
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

            var historial =
                await _context.HistorialFacturas
                    .Include(h =>
                        h.IdEstadoFacturaNavigation)
                    .Include(h =>
                        h.IdUsuarioNavigation)
                    .Where(h =>
                        h.IdFactura == id)
                    .OrderByDescending(h =>
                        h.Fecha)
                    .ToListAsync();

            ViewBag.Factura = factura;

            return View(historial);
        }

        // ============================================================
        // CREAR FACTURA PARA PEDIDO
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> CreateParaPedido(
            int id)
        {
            var pedido =
                await _context.Pedidos
                    .Include(p => p.Factura)
                    .Include(p => p.IdEstadoPedidoNavigation)
                    .FirstOrDefaultAsync(p =>
                        p.IdPedido == id);

            if (pedido == null)
            {
                return NotFound();
            }

            // ========================================================
            // NO FACTURAR CANCELADOS
            // ========================================================

            if (pedido.IdEstadoPedido ==
                ESTADO_PEDIDO_CANCELADO)
            {
                TempData["Error"] =
                    "No se puede crear una factura para un pedido cancelado.";

                return RedirectToAction(
                    "Index",
                    "Pedido");
            }

            // ========================================================
            // YA FACTURADO
            // ========================================================

            if (pedido.Factura != null)
            {
                TempData["Error"] =
                    "El pedido ya tiene una factura asociada.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = pedido.Factura.IdFactura
                    });
            }

            // ========================================================
            // CREAR MODELO CON IMPORTES DEL PEDIDO
            // ========================================================

            var factura =
                new Factura
                {
                    IdPedido =
                        pedido.IdPedido,

                    Subtotal =
                        pedido.Subtotal,

                    Descuento =
                        pedido.Descuento,

                    Impuesto =
                        pedido.Impuesto,

                    Total =
                        pedido.Total,

                    SaldoPendiente =
                        pedido.Total,

                    IdEstadoFactura =
                        ESTADO_FACTURA_PENDIENTE,

                    FechaFactura =
                        DateTime.Now
                };

            await CargarListas(factura);

            ViewBag.Pedido = pedido;

            return View("Create", factura);
        }

        // ============================================================
        // CALCULAR IMPORTES DE FACTURA
        // ============================================================

        private void CalcularImportesFactura(
            Factura factura,
            Pedido pedido,
            Impuesto? impuesto)
        {
            // --------------------------------------------------------
            // SUBTOTAL Y DESCUENTO VIENEN DEL PEDIDO
            // --------------------------------------------------------

            decimal subtotal =
                pedido.Subtotal;

            decimal descuento =
                pedido.Descuento;

            // --------------------------------------------------------
            // BASE IMPONIBLE
            // --------------------------------------------------------

            decimal baseImponible =
                subtotal - descuento;

            if (baseImponible < 0)
            {
                baseImponible = 0;
            }

            // --------------------------------------------------------
            // IVA / IMPUESTO
            // --------------------------------------------------------

            decimal importeImpuesto = 0m;

            if (impuesto != null)
            {
                importeImpuesto =
                    Math.Round(
                        baseImponible *
                        (impuesto.Porcentaje / 100m),
                        2);
            }
            else
            {
                // Si no se selecciona impuesto,
                // se utiliza el impuesto registrado en el pedido.
                importeImpuesto =
                    pedido.Impuesto;
            }

            // --------------------------------------------------------
            // TOTAL
            // --------------------------------------------------------

            decimal total =
                baseImponible +
                importeImpuesto;

            factura.Subtotal =
                Math.Round(subtotal, 2);

            factura.Descuento =
                Math.Round(descuento, 2);

            factura.Impuesto =
                Math.Round(importeImpuesto, 2);

            factura.Total =
                Math.Round(total, 2);

            factura.SaldoPendiente =
                Math.Round(total, 2);
        }

        // ============================================================
        // CARGAR LISTAS
        // ============================================================

        private async Task CargarListas(
            Factura? factura = null)
        {
            // --------------------------------------------------------
            // PEDIDOS DISPONIBLES
            // --------------------------------------------------------

            var pedidos =
                await _context.Pedidos
                    .Include(p => p.Factura)
                    .Include(p => p.IdEstadoPedidoNavigation)
                    .Where(p =>
                        p.Factura == null &&
                        p.IdEstadoPedido !=
                        ESTADO_PEDIDO_CANCELADO)
                    .OrderByDescending(p =>
                        p.FechaPedido)
                    .ToListAsync();

            // --------------------------------------------------------
            // AGREGAR PEDIDO ACTUAL SI ES NECESARIO
            // --------------------------------------------------------

            if (factura != null &&
                factura.IdPedido > 0)
            {
                var pedidoActual =
                    await _context.Pedidos
                        .Include(p => p.IdEstadoPedidoNavigation)
                        .FirstOrDefaultAsync(p =>
                            p.IdPedido ==
                            factura.IdPedido);

                if (pedidoActual != null &&
                    pedidoActual.IdEstadoPedido !=
                    ESTADO_PEDIDO_CANCELADO &&
                    !pedidos.Any(p =>
                        p.IdPedido ==
                        pedidoActual.IdPedido))
                {
                    pedidos.Add(pedidoActual);
                }
            }

            ViewBag.Pedidos =
                pedidos;

            // --------------------------------------------------------
            // MONEDAS
            // --------------------------------------------------------

            ViewBag.Monedas =
                await _context.Moneda
                    .Where(m =>
                        m.Estado)
                    .OrderBy(m =>
                        m.Nombre)
                    .ToListAsync();

            // --------------------------------------------------------
            // TIPOS DE COMPROBANTE
            // --------------------------------------------------------

            ViewBag.TiposComprobante =
                await _context.TipoComprobantes
                    .Where(t =>
                        t.Estado)
                    .OrderBy(t =>
                        t.Nombre)
                    .ToListAsync();

            // --------------------------------------------------------
            // SERIES
            // --------------------------------------------------------

            ViewBag.SeriesFactura =
                await _context.SerieFacturas
                    .Where(s =>
                        s.Estado)
                    .OrderBy(s =>
                        s.Nombre)
                    .ToListAsync();

            // --------------------------------------------------------
            // IMPUESTOS
            // --------------------------------------------------------

            ViewBag.Impuestos =
                await _context.Impuestos
                    .Where(i =>
                        i.Estado)
                    .OrderBy(i =>
                        i.Nombre)
                    .ToListAsync();

            // --------------------------------------------------------
            // ESTADOS FACTURA
            // --------------------------------------------------------

            ViewBag.EstadosFactura =
                await _context.EstadoFacturas
                    .Where(e =>
                        e.Estado)
                    .OrderBy(e =>
                        e.IdEstadoFactura)
                    .ToListAsync();
        }

        // ============================================================
        // CARGAR MOTIVOS DE ANULACIÓN
        // ============================================================

        private async Task CargarListasAnulacion()
        {
            ViewBag.MotivosAnulacion =
                await _context.MotivoAnulacions
                    .Where(m =>
                        m.Estado)
                    .OrderBy(m =>
                        m.Nombre)
                    .ToListAsync();
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
        // FACTURA EXISTS
        // ============================================================

        private bool FacturaExists(int id)
        {
            return _context.Facturas
                .Any(f =>
                    f.IdFactura == id);
        }
    }
}