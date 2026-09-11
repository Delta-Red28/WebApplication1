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

        private const int ESTADO_PEDIDO_LISTO = 4;
        private const int ESTADO_PEDIDO_ENTREGADO = 5;
        private const int ESTADO_PEDIDO_FACTURADO = 6;

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
                .AsNoTracking()

                .Include(f => f.IdPedidoNavigation)
                    .ThenInclude(p => p!.IdClienteNavigation)

                .Include(f => f.IdEstadoFacturaNavigation)

                .Include(f => f.IdMonedaNavigation)

                .Include(f => f.IdTipoComprobanteNavigation)

                .Include(f => f.IdSerieFacturaNavigation)

                .Include(f => f.IdImpuestoNavigation)

                .OrderByDescending(f => f.FechaFactura)
                .ThenByDescending(f => f.IdFactura)

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

                .AsNoTracking()

                // ----------------------------------------------------
                // PEDIDO + CLIENTE
                // ----------------------------------------------------

                .Include(f => f.IdPedidoNavigation)
                    .ThenInclude(p => p!.IdClienteNavigation)

                // ----------------------------------------------------
                // ESTADO
                // ----------------------------------------------------

                .Include(f => f.IdEstadoFacturaNavigation)

                // ----------------------------------------------------
                // MONEDA
                // ----------------------------------------------------

                .Include(f => f.IdMonedaNavigation)

                // ----------------------------------------------------
                // TIPO COMPROBANTE
                // ----------------------------------------------------

                .Include(f => f.IdTipoComprobanteNavigation)

                // ----------------------------------------------------
                // SERIE
                // ----------------------------------------------------

                .Include(f => f.IdSerieFacturaNavigation)

                // ----------------------------------------------------
                // MOTIVO ANULACIÓN
                // ----------------------------------------------------

                .Include(f => f.IdMotivoAnulacionNavigation)

                // ----------------------------------------------------
                // USUARIO QUE ANULÓ
                // ----------------------------------------------------

                .Include(f => f.IdUsuarioAnuloNavigation)

                // ----------------------------------------------------
                // IMPUESTO
                // ----------------------------------------------------

                .Include(f => f.IdImpuestoNavigation)

                // ----------------------------------------------------
                // PAGOS + MÉTODO
                // ----------------------------------------------------

                .Include(f => f.Pagos)
                    .ThenInclude(p => p.IdMetodoPagoNavigation)

                // ----------------------------------------------------
                // PAGOS + ESTADO
                // ----------------------------------------------------

                .Include(f => f.Pagos)
                    .ThenInclude(p => p.IdEstadoPagoNavigation)

                // ----------------------------------------------------
                // PAGOS + MONEDA
                // ----------------------------------------------------

                .Include(f => f.Pagos)
                    .ThenInclude(p => p.IdMonedaNavigation)

                .FirstOrDefaultAsync(f =>
                    f.IdFactura == id.Value);

            if (factura == null)
            {
                return NotFound();
            }

            // ========================================================
            // HISTORIAL
            // ========================================================

            var historial = await _context.HistorialFacturas

                .AsNoTracking()

                .Include(h =>
                    h.IdEstadoFacturaNavigation)

                .Include(h =>
                    h.IdUsuarioNavigation)

                .Where(h =>
                    h.IdFactura == factura.IdFactura)

                .OrderByDescending(h =>
                    h.Fecha)

                .ToListAsync();

            ViewBag.Historial = historial;

            return View(factura);
        }

        // ============================================================
        // IMPRIMIR FACTURA
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Imprimir(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var factura = await _context.Facturas

                .AsNoTracking()

                // ----------------------------------------------------
                // PEDIDO + CLIENTE
                // ----------------------------------------------------

                .Include(f => f.IdPedidoNavigation)
                    .ThenInclude(p => p!.IdClienteNavigation)

                // ----------------------------------------------------
                // PEDIDO + DETALLES + PRODUCTO
                // ----------------------------------------------------

                .Include(f => f.IdPedidoNavigation)
                    .ThenInclude(p => p!.DetallePedidos)
                        .ThenInclude(d => d.IdProductoNavigation)

                // ----------------------------------------------------
                // ESTADO FACTURA
                // ----------------------------------------------------

                .Include(f => f.IdEstadoFacturaNavigation)

                // ----------------------------------------------------
                // MONEDA
                // ----------------------------------------------------

                .Include(f => f.IdMonedaNavigation)

                // ----------------------------------------------------
                // TIPO COMPROBANTE
                // ----------------------------------------------------

                .Include(f => f.IdTipoComprobanteNavigation)

                // ----------------------------------------------------
                // SERIE
                // ----------------------------------------------------

                .Include(f => f.IdSerieFacturaNavigation)

                // ----------------------------------------------------
                // IMPUESTO
                // ----------------------------------------------------

                .Include(f => f.IdImpuestoNavigation)

                .FirstOrDefaultAsync(f =>
                    f.IdFactura == id.Value);

            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // ============================================================
        // CREATE GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (ObtenerUsuarioActual() <= 0)
            {
                TempData["Error"] =
                    "No se pudo identificar al usuario actual.";

                return RedirectToAction("Index", "Home");
            }

            var factura = new Factura
            {
                IdEstadoFactura =
                    ESTADO_FACTURA_PENDIENTE,

                FechaFactura =
                    DateTime.Now,

                SaldoPendiente =
                    0m
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
            int idUsuario = ObtenerUsuarioActual();

            if (idUsuario <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo identificar al usuario actual.");
            }

            Pedido? pedido = null;
            Monedum? moneda = null;
            TipoComprobante? tipoComprobante = null;
            SerieFactura? serie = null;
            Impuesto? impuesto = null;

            // ========================================================
            // VALIDAR PEDIDO
            // ========================================================

            if (factura.IdPedido <= 0)
            {
                ModelState.AddModelError(
                    "IdPedido",
                    "Debe seleccionar un pedido.");
            }
            else
            {
                pedido = await _context.Pedidos

                    .AsNoTracking()

                    .Include(p =>
                        p.IdEstadoPedidoNavigation)

                    .Include(p =>
                        p.Factura)

                    .FirstOrDefaultAsync(p =>
                        p.IdPedido == factura.IdPedido);

                if (pedido == null)
                {
                    ModelState.AddModelError(
                        "IdPedido",
                        "El pedido seleccionado no existe.");
                }
            }

            if (pedido != null)
            {
                if (pedido.IdEstadoPedido !=
                    ESTADO_PEDIDO_LISTO)
                {
                    string estado =
                        pedido.IdEstadoPedidoNavigation?.Nombre
                        ?? "desconocido";

                    ModelState.AddModelError(
                        "IdPedido",
                        $"Solo se puede facturar un pedido que esté Listo. " +
                        $"El pedido actualmente está en estado \"{estado}\".");
                }

                var facturaPedido =
                    pedido.Factura;

                if (facturaPedido != null)
                {
                    ModelState.AddModelError(
                        "IdPedido",
                        "El pedido seleccionado ya tiene una factura.");
                }

                if (pedido.Total <= 0)
                {
                    ModelState.AddModelError(
                        "IdPedido",
                        "El pedido no tiene un total válido para facturar.");
                }
            }

            // ========================================================
            // VALIDAR MONEDA
            // ========================================================

            if (factura.IdMoneda <= 0)
            {
                ModelState.AddModelError(
                    "IdMoneda",
                    "Debe seleccionar una moneda.");
            }
            else
            {
                moneda = await _context.Moneda

                    .AsNoTracking()

                    .FirstOrDefaultAsync(m =>
                        m.IdMoneda == factura.IdMoneda &&
                        m.Estado);

                if (moneda == null)
                {
                    ModelState.AddModelError(
                        "IdMoneda",
                        "La moneda seleccionada no es válida.");
                }
            }

            // ========================================================
            // VALIDAR TIPO COMPROBANTE
            // ========================================================

            if (factura.IdTipoComprobante <= 0)
            {
                ModelState.AddModelError(
                    "IdTipoComprobante",
                    "Debe seleccionar un tipo de comprobante.");
            }
            else
            {
                tipoComprobante =
                    await _context.TipoComprobantes

                        .AsNoTracking()

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
            }

            // ========================================================
            // VALIDAR SERIE
            // ========================================================

            if (factura.IdSerieFactura <= 0)
            {
                ModelState.AddModelError(
                    "IdSerieFactura",
                    "Debe seleccionar una serie.");
            }
            else
            {
                serie = await _context.SerieFacturas

                    .FirstOrDefaultAsync(s =>
                        s.IdSerieFactura ==
                        factura.IdSerieFactura &&
                        s.Estado);

                if (serie == null)
                {
                    ModelState.AddModelError(
                        "IdSerieFactura",
                        "La serie seleccionada no es válida.");
                }
            }

            // ========================================================
            // VALIDAR IMPUESTO
            // ========================================================

            if (factura.IdImpuesto.HasValue &&
                factura.IdImpuesto.Value > 0)
            {
                impuesto = await _context.Impuestos

                    .AsNoTracking()

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

            factura.FechaFactura =
                DateTime.Now;

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

            factura.IdEstadoFactura =
                ESTADO_FACTURA_PENDIENTE;

            // ========================================================
            // VALIDAR ESTADO PENDIENTE
            // ========================================================

            bool estadoValido =
                await _context.EstadoFacturas

                    .AsNoTracking()

                    .AnyAsync(e =>
                        e.IdEstadoFactura ==
                        ESTADO_FACTURA_PENDIENTE &&
                        e.Estado);

            if (!estadoValido)
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
                pedido = await _context.Pedidos

                    .Include(p =>
                        p.IdEstadoPedidoNavigation)

                    .Include(p =>
                        p.Factura)

                    .FirstOrDefaultAsync(p =>
                        p.IdPedido == factura.IdPedido);

                if (pedido == null)
                {
                    throw new InvalidOperationException(
                        "No se encontró el pedido seleccionado.");
                }

                if (pedido.IdEstadoPedido !=
                    ESTADO_PEDIDO_LISTO)
                {
                    throw new InvalidOperationException(
                        "El pedido debe estar en estado \"Listo\" para poder facturarlo.");
                }

                var facturaPedido =
                    pedido.Factura;

                if (facturaPedido != null)
                {
                    throw new InvalidOperationException(
                        "El pedido seleccionado ya tiene una factura.");
                }

                if (pedido.Total <= 0)
                {
                    throw new InvalidOperationException(
                        "El pedido no tiene un total válido.");
                }

                // ----------------------------------------------------
                // RECARGAR MONEDA
                // ----------------------------------------------------

                moneda =
                    await _context.Moneda

                        .FirstOrDefaultAsync(m =>
                            m.IdMoneda ==
                            factura.IdMoneda &&
                            m.Estado);

                if (moneda == null)
                {
                    throw new InvalidOperationException(
                        "La moneda seleccionada no está disponible.");
                }

                // ----------------------------------------------------
                // RECARGAR TIPO COMPROBANTE
                // ----------------------------------------------------

                tipoComprobante =
                    await _context.TipoComprobantes

                        .FirstOrDefaultAsync(t =>
                            t.IdTipoComprobante ==
                            factura.IdTipoComprobante &&
                            t.Estado);

                if (tipoComprobante == null)
                {
                    throw new InvalidOperationException(
                        "El tipo de comprobante seleccionado no está disponible.");
                }

                // ----------------------------------------------------
                // RECARGAR SERIE
                // ----------------------------------------------------

                serie =
                    await _context.SerieFacturas

                        .FirstOrDefaultAsync(s =>
                            s.IdSerieFactura ==
                            factura.IdSerieFactura &&
                            s.Estado);

                if (serie == null)
                {
                    throw new InvalidOperationException(
                        "La serie seleccionada no está disponible.");
                }

                // ----------------------------------------------------
                // RECARGAR IMPUESTO
                // ----------------------------------------------------

                impuesto = null;

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
                        throw new InvalidOperationException(
                            "El impuesto seleccionado no está disponible.");
                    }
                }

                // ----------------------------------------------------
                // RECALCULAR IMPORTES
                // ----------------------------------------------------

                CalcularImportesFactura(
                    factura,
                    pedido,
                    impuesto);

                factura.IdEstadoFactura =
                    ESTADO_FACTURA_PENDIENTE;

                factura.FechaFactura =
                    DateTime.Now;

                factura.IdPedidoNavigation =
                    pedido;

                factura.IdMonedaNavigation =
                    moneda;

                factura.IdTipoComprobanteNavigation =
                    tipoComprobante;

                factura.IdSerieFacturaNavigation =
                    serie;

                factura.IdImpuestoNavigation =
                    impuesto;

                // ----------------------------------------------------
                // GENERAR CONSECUTIVO
                // ----------------------------------------------------

                int siguienteNumero =
                    serie.NumeroActual + 1;

                if (siguienteNumero <= 0)
                {
                    throw new InvalidOperationException(
                        "El consecutivo de la serie no es válido.");
                }

                string numero =
                    siguienteNumero
                        .ToString()
                        .PadLeft(
                            serie.LongitudNumero,
                            '0');

                factura.NumeroFactura =
                    $"{serie.Prefijo}{numero}";

                // ----------------------------------------------------
                // VERIFICAR DUPLICADO
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

                await _context.SaveChangesAsync();

                // ----------------------------------------------------
                // HISTORIAL
                // ----------------------------------------------------

                var historial =
                    new HistorialFactura
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
                            $"El pedido permanece en estado Listo hasta confirmar el pago. " +
                            $"Subtotal: {factura.Subtotal:N2}. " +
                            $"Descuento: {factura.Descuento:N2}. " +
                            $"Impuesto: {factura.Impuesto:N2}. " +
                            $"Total: {factura.Total:N2}.",

                        Fecha =
                            DateTime.Now
                    };

                _context.HistorialFacturas.Add(
                    historial);

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
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "No fue posible guardar la factura. " +
                    "Es posible que el número de factura haya sido utilizado por otra operación.");

                await CargarListas(factura);

                return View(factura);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error inesperado al crear la factura.");

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

            var factura =
                await _context.Facturas

                    .Include(f =>
                        f.IdPedidoNavigation)
                        .ThenInclude(p =>
                            p!.IdClienteNavigation)

                    .Include(f =>
                        f.IdEstadoFacturaNavigation)

                    .Include(f =>
                        f.IdMonedaNavigation)

                    .Include(f =>
                        f.IdTipoComprobanteNavigation)

                    .Include(f =>
                        f.IdSerieFacturaNavigation)

                    .Include(f =>
                        f.IdImpuestoNavigation)

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

                    .AsNoTracking()

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
            // NO CAMBIAR NÚMERO
            // ========================================================

            factura.NumeroFactura =
                facturaOriginal.NumeroFactura;

            // ========================================================
            // NO CAMBIAR SERIE
            // ========================================================

            factura.IdSerieFactura =
                facturaOriginal.IdSerieFactura;

            // ========================================================
            // PAGOS CONFIRMADOS
            // ========================================================

            bool tienePagos =
                await _context.Pagos

                    .AsNoTracking()

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
                        m.IdMoneda ==
                        factura.IdMoneda &&
                        m.Estado);

            if (moneda == null)
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
                    "No se puede cambiar la moneda de una factura que ya tiene pagos confirmados.");
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

            if (tienePagos &&
                factura.IdImpuesto !=
                facturaOriginal.IdImpuesto)
            {
                ModelState.AddModelError(
                    "IdImpuesto",
                    "No se puede cambiar el impuesto de una factura que ya tiene pagos confirmados.");
            }

            // ========================================================
            // IMPORTES INMUTABLES
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

            factura.FechaFactura =
                facturaOriginal.FechaFactura;

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
                var facturaDb =
                    await _context.Facturas

                        .FirstOrDefaultAsync(f =>
                            f.IdFactura == id);

                if (facturaDb == null)
                {
                    throw new InvalidOperationException(
                        "La factura ya no existe.");
                }

                if (facturaDb.IdEstadoFactura ==
                    ESTADO_FACTURA_ANULADA)
                {
                    throw new InvalidOperationException(
                        "No se puede modificar una factura anulada.");
                }

                // ----------------------------------------------------
                // REVALIDAR PAGOS
                // ----------------------------------------------------

                tienePagos =
                    await _context.Pagos

                        .AsNoTracking()

                        .AnyAsync(p =>
                            p.IdFactura == id &&
                            p.IdEstadoPago ==
                            ESTADO_PAGO_CONFIRMADO);

                if (tienePagos &&
                    factura.IdMoneda !=
                    facturaDb.IdMoneda)
                {
                    throw new InvalidOperationException(
                        "No se puede cambiar la moneda porque la factura tiene pagos confirmados.");
                }

                if (tienePagos &&
                    factura.IdImpuesto !=
                    facturaDb.IdImpuesto)
                {
                    throw new InvalidOperationException(
                        "No se puede cambiar el impuesto porque la factura tiene pagos confirmados.");
                }

                // ----------------------------------------------------
                // VALIDAR MONEDA
                // ----------------------------------------------------

                moneda =
                    await _context.Moneda

                        .FirstOrDefaultAsync(m =>
                            m.IdMoneda ==
                            factura.IdMoneda &&
                            m.Estado);

                if (moneda == null)
                {
                    throw new InvalidOperationException(
                        "La moneda seleccionada no está disponible.");
                }

                // ----------------------------------------------------
                // VALIDAR TIPO COMPROBANTE
                // ----------------------------------------------------

                tipoComprobante =
                    await _context.TipoComprobantes

                        .FirstOrDefaultAsync(t =>
                            t.IdTipoComprobante ==
                            factura.IdTipoComprobante &&
                            t.Estado);

                if (tipoComprobante == null)
                {
                    throw new InvalidOperationException(
                        "El tipo de comprobante seleccionado no está disponible.");
                }

                // ----------------------------------------------------
                // VALIDAR IMPUESTO
                // ----------------------------------------------------

                impuesto = null;

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
                        throw new InvalidOperationException(
                            "El impuesto seleccionado no está disponible.");
                    }
                }

                // ----------------------------------------------------
                // ACTUALIZAR CAMPOS PERMITIDOS
                // ----------------------------------------------------

                facturaDb.IdMoneda =
                    factura.IdMoneda;

                facturaDb.IdTipoComprobante =
                    factura.IdTipoComprobante;

                facturaDb.IdImpuesto =
                    factura.IdImpuesto;

                facturaDb.Observacion =
                    factura.Observacion;

                await _context.SaveChangesAsync();

                // ----------------------------------------------------
                // HISTORIAL
                // ----------------------------------------------------

                var historial =
                    new HistorialFactura
                    {
                        IdFactura =
                            facturaDb.IdFactura,

                        IdEstadoFactura =
                            facturaDb.IdEstadoFactura,

                        IdUsuario =
                            idUsuario,

                        Observacion =
                            $"Factura #{facturaDb.NumeroFactura} modificada.",

                        Fecha =
                            DateTime.Now
                    };

                _context.HistorialFacturas.Add(
                    historial);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Factura #{facturaDb.NumeroFactura} actualizada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = facturaDb.IdFactura
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
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "No fue posible modificar la factura.");

                await CargarListas(factura);

                return View(factura);
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

            var factura =
                await _context.Facturas

                    .AsNoTracking()

                    .Include(f =>
                        f.IdPedidoNavigation)
                        .ThenInclude(p =>
                            p!.IdClienteNavigation)

                    .Include(f =>
                        f.IdEstadoFacturaNavigation)

                    .Include(f =>
                        f.IdMonedaNavigation)

                    .Include(f =>
                        f.IdTipoComprobanteNavigation)

                    .Include(f =>
                        f.IdSerieFacturaNavigation)

                    .Include(f =>
                        f.IdMotivoAnulacionNavigation)

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
        // DELETE POST - ANULAR
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

                    .AsNoTracking()

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
                    new
                    {
                        id
                    });
            }

            // ========================================================
            // PAGOS CONFIRMADOS
            // ========================================================

            bool tienePagos =
                await _context.Pagos

                    .AsNoTracking()

                    .AnyAsync(p =>
                        p.IdFactura ==
                        factura.IdFactura &&
                        p.IdEstadoPago ==
                        ESTADO_PAGO_CONFIRMADO);

            if (tienePagos)
            {
                TempData["Error"] =
                    "No se puede anular la factura porque tiene pagos confirmados asociados.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // ========================================================
            // TRANSACCIÓN
            // ========================================================

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                factura =
                    await _context.Facturas

                        .FirstOrDefaultAsync(f =>
                            f.IdFactura == id);

                if (factura == null)
                {
                    throw new InvalidOperationException(
                        "La factura ya no existe.");
                }

                if (factura.IdEstadoFactura ==
                    ESTADO_FACTURA_ANULADA)
                {
                    throw new InvalidOperationException(
                        "La factura ya se encuentra anulada.");
                }

                // ----------------------------------------------------
                // VERIFICAR PAGOS NUEVAMENTE
                // ----------------------------------------------------

                tienePagos =
                    await _context.Pagos

                        .AsNoTracking()

                        .AnyAsync(p =>
                            p.IdFactura ==
                            factura.IdFactura &&
                            p.IdEstadoPago ==
                            ESTADO_PAGO_CONFIRMADO);

                if (tienePagos)
                {
                    throw new InvalidOperationException(
                        "No se puede anular la factura porque tiene pagos confirmados.");
                }

                // ----------------------------------------------------
                // ANULAR
                // ----------------------------------------------------

                factura.IdEstadoFactura =
                    ESTADO_FACTURA_ANULADA;

                factura.IdMotivoAnulacion =
                    idMotivoAnulacion;

                factura.FechaAnulacion =
                    DateTime.Now;

                factura.IdUsuarioAnulo =
                    idUsuario;

                factura.ObservacionAnulacion =
                    string.IsNullOrWhiteSpace(
                        observacionAnulacion)
                        ? null
                        : observacionAnulacion.Trim();

                // ----------------------------------------------------
                // HISTORIAL
                // ----------------------------------------------------

                string observacion =
                    $"Factura #{factura.NumeroFactura} anulada. " +
                    $"Motivo: {motivo.Nombre}.";

                if (!string.IsNullOrWhiteSpace(
                    observacionAnulacion))
                {
                    observacion +=
                        $" Observación: {observacionAnulacion.Trim()}";
                }

                var historial =
                    new HistorialFactura
                    {
                        IdFactura =
                            factura.IdFactura,

                        IdEstadoFactura =
                            ESTADO_FACTURA_ANULADA,

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

                TempData["Success"] =
                    $"Factura #{factura.NumeroFactura} anulada correctamente.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    ex.Message;

                return RedirectToAction(
                    nameof(Delete),
                    new
                    {
                        id
                    });
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "No fue posible anular la factura.";

                return RedirectToAction(
                    nameof(Delete),
                    new
                    {
                        id
                    });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Ocurrió un error al anular la factura.";

                return RedirectToAction(
                    nameof(Delete),
                    new
                    {
                        id
                    });
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

                    .AsNoTracking()

                    .Include(f =>
                        f.IdPedidoNavigation)
                        .ThenInclude(p =>
                            p!.IdClienteNavigation)

                    .Include(f =>
                        f.IdEstadoFacturaNavigation)

                    .Include(f =>
                        f.IdMonedaNavigation)

                    .Include(f =>
                        f.IdTipoComprobanteNavigation)

                    .Include(f =>
                        f.IdSerieFacturaNavigation)

                    .Include(f =>
                        f.IdImpuestoNavigation)

                    .AsQueryable();

            // ========================================================
            // TEXTO / NÚMERO
            // ========================================================

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar =
                    buscar.Trim();

                if (int.TryParse(
                    buscar,
                    out int numero))
                {
                    query =
                        query.Where(f =>
                            f.IdFactura == numero ||
                            f.IdPedido == numero);
                }
                else
                {
                    query =
                        query.Where(f =>
                            (f.NumeroFactura != null &&
                             f.NumeroFactura.Contains(buscar))

                            ||

                            (f.IdPedidoNavigation != null &&
                             f.IdPedidoNavigation.NumeroPedido != null &&
                             f.IdPedidoNavigation.NumeroPedido.Contains(buscar)));
                }
            }

            // ========================================================
            // FECHA INICIO
            // ========================================================

            if (fechaInicio.HasValue)
            {
                DateTime fecha =
                    fechaInicio.Value.Date;

                query =
                    query.Where(f =>
                        f.FechaFactura >= fecha);
            }

            // ========================================================
            // FECHA FIN
            // ========================================================

            if (fechaFin.HasValue)
            {
                DateTime fechaHasta =
                    fechaFin.Value.Date.AddDays(1);

                query =
                    query.Where(f =>
                        f.FechaFactura < fechaHasta);
            }

            // ========================================================
            // ESTADO
            // ========================================================

            if (idEstadoFactura.HasValue &&
                idEstadoFactura.Value > 0)
            {
                query =
                    query.Where(f =>
                        f.IdEstadoFactura ==
                        idEstadoFactura.Value);
            }

            // ========================================================
            // RESULTADO
            // ========================================================

            var facturas =
                await query

                    .OrderByDescending(f =>
                        f.FechaFactura)

                    .ThenByDescending(f =>
                        f.IdFactura)

                    .ToListAsync();

            // ========================================================
            // DATOS PARA LA VISTA
            // ========================================================

            ViewBag.Buscar =
                buscar;

            ViewBag.FechaInicio =
                fechaInicio;

            ViewBag.FechaFin =
                fechaFin;

            ViewBag.IdEstadoFactura =
                idEstadoFactura;

            ViewBag.EstadosFactura =
                await _context.EstadoFacturas

                    .AsNoTracking()

                    .Where(e =>
                        e.Estado)

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

                    .AsNoTracking()

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

                    .AsNoTracking()

                    .Include(h =>
                        h.IdEstadoFacturaNavigation)

                    .Include(h =>
                        h.IdUsuarioNavigation)

                    .Where(h =>
                        h.IdFactura == id)

                    .OrderByDescending(h =>
                        h.Fecha)

                    .ToListAsync();

            ViewBag.Factura =
                factura;

            return View(historial);
        }

        // ============================================================
        // CREAR FACTURA PARA PEDIDO
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> CreateParaPedido(
            int id)
        {
            if (ObtenerUsuarioActual() <= 0)
            {
                TempData["Error"] =
                    "No se pudo identificar al usuario actual.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            var pedido =
                await _context.Pedidos

                    .AsNoTracking()

                    .Include(p =>
                        p.Factura)

                    .Include(p =>
                        p.IdEstadoPedidoNavigation)

                    .Include(p =>
                        p.IdClienteNavigation)

                    .FirstOrDefaultAsync(p =>
                        p.IdPedido == id);

            if (pedido == null)
            {
                return NotFound();
            }

            // ========================================================
            // VALIDAR ESTADO
            // ========================================================

            if (pedido.IdEstadoPedido !=
                ESTADO_PEDIDO_LISTO)
            {
                string estadoActual =
                    pedido.IdEstadoPedidoNavigation?.Nombre
                    ?? "desconocido";

                TempData["Error"] =
                    $"No se puede facturar este pedido. " +
                    $"Actualmente está en estado \"{estadoActual}\". " +
                    $"El pedido debe estar en estado \"Listo\".";

                return RedirectToAction(
                    "Details",
                    "Pedido",
                    new
                    {
                        id = pedido.IdPedido
                    });
            }

            // ========================================================
            // YA FACTURADO
            // ========================================================

            var facturaExistente =
                pedido.Factura;

            if (facturaExistente != null)
            {
                TempData["Error"] =
                    "El pedido ya tiene una factura asociada.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id =
                            facturaExistente.IdFactura
                    });
            }

            // ========================================================
            // VALIDAR TOTAL
            // ========================================================

            if (pedido.Total <= 0)
            {
                TempData["Error"] =
                    "El pedido no tiene un total válido para facturar.";

                return RedirectToAction(
                    "Details",
                    "Pedido",
                    new
                    {
                        id =
                            pedido.IdPedido
                    });
            }

            // ========================================================
            // CREAR FACTURA
            // ========================================================

            var factura =
                new Factura
                {
                    IdPedido =
                        pedido.IdPedido,

                    Subtotal =
                        Math.Round(
                            pedido.Subtotal,
                            2),

                    Descuento =
                        Math.Round(
                            pedido.Descuento,
                            2),

                    Impuesto =
                        Math.Round(
                            pedido.Impuesto,
                            2),

                    Total =
                        Math.Round(
                            pedido.Total,
                            2),

                    SaldoPendiente =
                        Math.Round(
                            pedido.Total,
                            2),

                    IdEstadoFactura =
                        ESTADO_FACTURA_PENDIENTE,

                    FechaFactura =
                        DateTime.Now
                };

            await CargarListas(factura);

            ViewBag.Pedido =
                pedido;

            return View(
                "Create",
                factura);
        }

        // ============================================================
        // CALCULAR IMPORTES
        // ============================================================

        private void CalcularImportesFactura(
            Factura factura,
            Pedido pedido,
            Impuesto? impuesto)
        {
            decimal subtotal =
                Math.Round(
                    pedido.Subtotal,
                    2);

            decimal descuento =
                Math.Round(
                    pedido.Descuento,
                    2);

            if (subtotal < 0)
            {
                subtotal = 0;
            }

            if (descuento < 0)
            {
                descuento = 0;
            }

            if (descuento > subtotal)
            {
                descuento = subtotal;
            }

            decimal baseImponible =
                Math.Round(
                    subtotal - descuento,
                    2);

            decimal importeImpuesto;

            if (impuesto != null)
            {
                decimal porcentaje =
                    impuesto.Porcentaje;

                if (porcentaje < 0)
                {
                    porcentaje = 0;
                }

                importeImpuesto =
                    Math.Round(
                        baseImponible *
                        (porcentaje / 100m),
                        2);
            }
            else
            {
                importeImpuesto =
                    Math.Round(
                        Math.Max(
                            pedido.Impuesto,
                            0m),
                        2);
            }

            decimal total =
                Math.Round(
                    baseImponible +
                    importeImpuesto,
                    2);

            factura.Subtotal =
                subtotal;

            factura.Descuento =
                descuento;

            factura.Impuesto =
                importeImpuesto;

            factura.Total =
                total;

            factura.SaldoPendiente =
                total;
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

            if (factura.Total < 0)
            {
                factura.Total = 0;
            }

            if (factura.SaldoPendiente < 0)
            {
                factura.SaldoPendiente = 0;
            }

            if (factura.SaldoPendiente >
                factura.Total)
            {
                factura.SaldoPendiente =
                    factura.Total;
            }

            if (factura.SaldoPendiente <= 0)
            {
                factura.SaldoPendiente = 0;

                factura.IdEstadoFactura =
                    ESTADO_FACTURA_PAGADA;

                return;
            }

            if (factura.SaldoPendiente <
                factura.Total)
            {
                factura.IdEstadoFactura =
                    ESTADO_FACTURA_PARCIAL;

                return;
            }

            factura.IdEstadoFactura =
                ESTADO_FACTURA_PENDIENTE;
        }

        // ============================================================
        // CARGAR LISTAS
        // ============================================================

        private async Task CargarListas(
            Factura? factura = null)
        {
            var pedidos =
                await _context.Pedidos

                    .AsNoTracking()

                    .Include(p =>
                        p.Factura)

                    .Include(p =>
                        p.IdEstadoPedidoNavigation)

                    .Include(p =>
                        p.IdClienteNavigation)

                    .Where(p =>
                        p.Factura == null &&
                        p.IdEstadoPedido ==
                        ESTADO_PEDIDO_LISTO)

                    .OrderByDescending(p =>
                        p.FechaPedido)

                    .ToListAsync();

            // ========================================================
            // PEDIDO ACTUAL
            // ========================================================

            if (factura != null &&
                factura.IdPedido > 0)
            {
                var pedidoActual =
                    await _context.Pedidos

                        .AsNoTracking()

                        .Include(p =>
                            p.Factura)

                        .Include(p =>
                            p.IdEstadoPedidoNavigation)

                        .Include(p =>
                            p.IdClienteNavigation)

                        .FirstOrDefaultAsync(p =>
                            p.IdPedido ==
                            factura.IdPedido);

                if (pedidoActual != null &&
                    pedidoActual.IdEstadoPedido ==
                    ESTADO_PEDIDO_LISTO &&
                    pedidoActual.Factura == null &&
                    !pedidos.Any(p =>
                        p.IdPedido ==
                        pedidoActual.IdPedido))
                {
                    pedidos.Add(
                        pedidoActual);
                }
            }

            ViewBag.Pedidos =
                pedidos;

            // ========================================================
            // MONEDAS
            // ========================================================

            ViewBag.Monedas =
                await _context.Moneda

                    .AsNoTracking()

                    .Where(m =>
                        m.Estado)

                    .OrderBy(m =>
                        m.Nombre)

                    .ToListAsync();

            // ========================================================
            // TIPOS COMPROBANTE
            // ========================================================

            ViewBag.TiposComprobante =
                await _context.TipoComprobantes

                    .AsNoTracking()

                    .Where(t =>
                        t.Estado)

                    .OrderBy(t =>
                        t.Nombre)

                    .ToListAsync();

            // ========================================================
            // SERIES
            // ========================================================

            ViewBag.SeriesFactura =
                await _context.SerieFacturas

                    .AsNoTracking()

                    .Where(s =>
                        s.Estado)

                    .OrderBy(s =>
                        s.Nombre)

                    .ToListAsync();

            // ========================================================
            // IMPUESTOS
            // ========================================================

            ViewBag.Impuestos =
                await _context.Impuestos

                    .AsNoTracking()

                    .Where(i =>
                        i.Estado)

                    .OrderBy(i =>
                        i.Nombre)

                    .ToListAsync();

            // ========================================================
            // ESTADOS
            // ========================================================

            ViewBag.EstadosFactura =
                await _context.EstadoFacturas

                    .AsNoTracking()

                    .Where(e =>
                        e.Estado)

                    .OrderBy(e =>
                        e.IdEstadoFactura)

                    .ToListAsync();
        }

        // ============================================================
        // MOTIVOS DE ANULACIÓN
        // ============================================================

        private async Task CargarListasAnulacion()
        {
            ViewBag.MotivosAnulacion =
                await _context.MotivoAnulacions

                    .AsNoTracking()

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
        // FACTURA EXISTS
        // ============================================================

        private async Task<bool> FacturaExistsAsync(
            int id)
        {
            return await _context.Facturas

                .AsNoTracking()

                .AnyAsync(f =>
                    f.IdFactura == id);
        }
    }
}