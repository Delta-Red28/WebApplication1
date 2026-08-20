using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class PedidoController : Controller
    {
        private readonly RestauranteContext _context;

        public PedidoController(RestauranteContext context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX
        // LISTADO + DASHBOARD DEL MÓDULO DE PEDIDOS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            // =====================================================
            // FECHAS
            // =====================================================

            var hoy = DateTime.Today;

            var inicioMes = new DateTime(
                hoy.Year,
                hoy.Month,
                1);

            var inicioMesSiguiente =
                inicioMes.AddMonths(1);

            var inicioDiaSiguiente =
                hoy.AddDays(1);

            // =====================================================
            // OBTENER TODOS LOS PEDIDOS
            // =====================================================

            var pedidos = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.IdMesaNavigation)
                .Include(p => p.IdEstadoPedidoNavigation)
                .Include(p => p.IdTipoPedidoNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            // =====================================================
            // PEDIDOS DEL MES
            // =====================================================

            var pedidosMes = pedidos
                .Where(p =>
                    p.FechaPedido >= inicioMes &&
                    p.FechaPedido < inicioMesSiguiente)
                .ToList();

            // =====================================================
            // PEDIDOS DE HOY
            // =====================================================

            var pedidosHoy = pedidos
                .Where(p =>
                    p.FechaPedido >= hoy &&
                    p.FechaPedido < inicioDiaSiguiente)
                .ToList();

            // =====================================================
            // PEDIDOS CANCELADOS DEL MES
            // =====================================================

            var pedidosCanceladosMes = pedidosMes
                .Where(p =>
                    EsEstadoCancelado(
                        p.IdEstadoPedidoNavigation?.Nombre))
                .ToList();

            // =====================================================
            // PEDIDOS CANCELADOS DE HOY
            // =====================================================

            var pedidosCanceladosHoy = pedidosHoy
                .Where(p =>
                    EsEstadoCancelado(
                        p.IdEstadoPedidoNavigation?.Nombre))
                .ToList();

            // =====================================================
            // PEDIDOS QUE CUENTAN COMO VENTA
            // CANCELADOS NO CUENTAN COMO VENTA
            // =====================================================

            var pedidosVentaMes = pedidosMes
                .Where(p =>
                    !EsEstadoCancelado(
                        p.IdEstadoPedidoNavigation?.Nombre))
                .ToList();

            var pedidosVentaHoy = pedidosHoy
                .Where(p =>
                    !EsEstadoCancelado(
                        p.IdEstadoPedidoNavigation?.Nombre))
                .ToList();

            // =====================================================
            // TOTAL VENDIDO DEL MES
            // =====================================================

            decimal totalVendidoMes =
                pedidosVentaMes.Sum(p => p.Total);

            // =====================================================
            // TOTAL VENDIDO HOY
            // =====================================================

            decimal totalVendidoHoy =
                pedidosVentaHoy.Sum(p => p.Total);

            // =====================================================
            // CANTIDAD DE PEDIDOS DEL MES
            // =====================================================

            int cantidadPedidosMes =
                pedidosVentaMes.Count;

            // =====================================================
            // CANTIDAD DE PEDIDOS DE HOY
            // =====================================================

            int cantidadPedidosHoy =
                pedidosVentaHoy.Count;

            // =====================================================
            // CANTIDAD DE CANCELADOS
            // =====================================================

            int cantidadCanceladosMes =
                pedidosCanceladosMes.Count;

            int cantidadCanceladosHoy =
                pedidosCanceladosHoy.Count;

            // =====================================================
            // TICKET PROMEDIO DEL MES
            // =====================================================

            decimal ticketPromedioMes =
                cantidadPedidosMes > 0
                    ? totalVendidoMes / cantidadPedidosMes
                    : 0m;

            // =====================================================
            // TICKET PROMEDIO DE HOY
            // =====================================================

            decimal ticketPromedioHoy =
                cantidadPedidosHoy > 0
                    ? totalVendidoHoy / cantidadPedidosHoy
                    : 0m;

            // =====================================================
            // ESTADOS DEL MES
            // =====================================================

            int pedidosPendientes =
                pedidosMes.Count(p =>
                    EsEstado(
                        p.IdEstadoPedidoNavigation?.Nombre,
                        "Pendiente"));

            int pedidosEnCocina =
                pedidosMes.Count(p =>
                    EsEstado(
                        p.IdEstadoPedidoNavigation?.Nombre,
                        "En cocina"));

            int pedidosPreparandose =
                pedidosMes.Count(p =>
                    EsEstado(
                        p.IdEstadoPedidoNavigation?.Nombre,
                        "Preparándose"));

            int pedidosListos =
                pedidosMes.Count(p =>
                    EsEstado(
                        p.IdEstadoPedidoNavigation?.Nombre,
                        "Listo"));

            int pedidosEntregados =
                pedidosMes.Count(p =>
                    EsEstado(
                        p.IdEstadoPedidoNavigation?.Nombre,
                        "Entregado"));

            int pedidosFacturados =
                pedidosMes.Count(p =>
                    EsEstado(
                        p.IdEstadoPedidoNavigation?.Nombre,
                        "Facturado"));

            // =====================================================
            // ENVIAR DATOS DEL DASHBOARD A LA VISTA
            // =====================================================

            ViewBag.TotalVendidoMes =
                totalVendidoMes;

            ViewBag.TotalVendidoHoy =
                totalVendidoHoy;

            ViewBag.CantidadPedidosMes =
                cantidadPedidosMes;

            ViewBag.CantidadPedidosHoy =
                cantidadPedidosHoy;

            ViewBag.PedidosCanceladosMes =
                cantidadCanceladosMes;

            ViewBag.PedidosCanceladosHoy =
                cantidadCanceladosHoy;

            ViewBag.TicketPromedioMes =
                ticketPromedioMes;

            ViewBag.TicketPromedioHoy =
                ticketPromedioHoy;

            ViewBag.PedidosPendientes =
                pedidosPendientes;

            ViewBag.PedidosEnCocina =
                pedidosEnCocina;

            ViewBag.PedidosPreparandose =
                pedidosPreparandose;

            ViewBag.PedidosListos =
                pedidosListos;

            ViewBag.PedidosEntregados =
                pedidosEntregados;

            ViewBag.PedidosFacturados =
                pedidosFacturados;

            // =====================================================
            // FILTRO DE BÚSQUEDA MEJORADO
            // =====================================================

            var pedidosFiltrados = pedidos;

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                // Normalizamos el texto para permitir búsquedas
                // sin importar mayúsculas, minúsculas o tildes.
                var termino =
                    NormalizarTexto(buscar);

                pedidosFiltrados = pedidos
                    .Where(p =>
                    {
                        // =================================================
                        // NÚMERO DE PEDIDO
                        // =================================================

                        var numeroPedido =
                            NormalizarTexto(
                                p.NumeroPedido);

                        // =================================================
                        // CLIENTE
                        // =================================================

                        var nombreCliente =
                            p.IdClienteNavigation != null
                                ? NormalizarTexto(
                                    $"{p.IdClienteNavigation.Nombres} {p.IdClienteNavigation.Apellidos}")
                                : "";

                        // =================================================
                        // USUARIO QUE CREÓ EL PEDIDO
                        // =================================================

                        var nombreUsuario =
                            p.IdUsuarioNavigation != null
                                ? NormalizarTexto(
                                    $"{p.IdUsuarioNavigation.Nombres} {p.IdUsuarioNavigation.Apellidos}")
                                : "";

                        var usuario =
                            p.IdUsuarioNavigation != null
                                ? NormalizarTexto(
                                    p.IdUsuarioNavigation.Usuario1)
                                : "";

                        // =================================================
                        // ESTADO
                        // =================================================

                        var estado =
                            NormalizarTexto(
                                p.IdEstadoPedidoNavigation?.Nombre);

                        // =================================================
                        // TIPO DE PEDIDO
                        // =================================================

                        var tipoPedido =
                            NormalizarTexto(
                                p.IdTipoPedidoNavigation?.Nombre);

                        // =================================================
                        // MESA
                        // =================================================

                        var mesa =
                            p.IdMesaNavigation != null
                                ? p.IdMesaNavigation.NumeroMesa
                                    .ToString()
                                : "";

                        // =================================================
                        // FECHA
                        // =================================================

                        var fecha =
                            p.FechaPedido.ToString(
                                "dd/MM/yyyy");

                        var fechaSinFormato =
                            p.FechaPedido.ToString(
                                "ddMMyyyy");

                        // =================================================
                        // BÚSQUEDA
                        // =================================================

                        return
                            numeroPedido.Contains(termino)
                            ||
                            nombreCliente.Contains(termino)
                            ||
                            nombreUsuario.Contains(termino)
                            ||
                            usuario.Contains(termino)
                            ||
                            estado.Contains(termino)
                            ||
                            tipoPedido.Contains(termino)
                            ||
                            mesa.Contains(termino)
                            ||
                            fecha.Contains(termino)
                            ||
                            fechaSinFormato.Contains(termino);
                    })
                    .ToList();
            }

            // =====================================================
            // GUARDAR BÚSQUEDA PARA LA VISTA
            // =====================================================

            ViewBag.Buscar = buscar;

            // =====================================================
            // DEVOLVER PEDIDOS FILTRADOS
            //
            // IMPORTANTE:
            // EL DASHBOARD SE CALCULÓ ANTES DEL FILTRO
            // =====================================================

            return View(pedidosFiltrados);
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var pedido = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.IdMesaNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdEstadoPedidoNavigation)
                .Include(p => p.IdTipoPedidoNavigation)
                .Include(p => p.DetallePedidos)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(
                    p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new PedidoViewModel
            {
                FechaPedido = DateTime.Now
            };

            model.Detalles ??=
                new List<DetallePedidoViewModel>();

            model.Detalles.Add(
                new DetallePedidoViewModel());

            await CargarCombos(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PedidoViewModel model)
        {
            var detalles =
                model.Detalles
                ?? new List<DetallePedidoViewModel>();

            // =====================================================
            // VALIDAR CLIENTE
            // =====================================================

            if (model.IdCliente <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.IdCliente),
                    "Debe seleccionar un cliente.");
            }

            // =====================================================
            // VALIDAR TIPO DE PEDIDO
            // =====================================================

            if (model.IdTipoPedido <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.IdTipoPedido),
                    "Debe seleccionar un tipo de pedido.");
            }

            // =====================================================
            // VALIDAR PRODUCTOS
            // =====================================================

            if (!detalles.Any(d => d.Cantidad > 0))
            {
                ModelState.AddModelError(
                    "",
                    "El pedido debe contener al menos un producto.");
            }

            // =====================================================
            // VALIDAR CANTIDADES Y DESCUENTOS
            // =====================================================

            foreach (var detalle in detalles)
            {
                if (detalle.Cantidad < 0)
                {
                    ModelState.AddModelError(
                        "",
                        "La cantidad de un producto no puede ser negativa.");
                }

                if (detalle.Cantidad > 0 &&
                    detalle.Cantidad != Math.Truncate(
                        detalle.Cantidad))
                {
                    ModelState.AddModelError(
                        "",
                        "La cantidad de los productos debe ser un número entero.");
                }

                if (detalle.Descuento < 0)
                {
                    ModelState.AddModelError(
                        "",
                        "El descuento no puede ser negativo.");
                }
            }

            // =====================================================
            // VALIDAR MODELO
            // =====================================================

            if (!ModelState.IsValid)
            {
                await CargarCombos(model);
                return View(model);
            }

            // =====================================================
            // USUARIO AUTENTICADO
            // =====================================================

            var claim = User.FindFirst(
                ClaimTypes.NameIdentifier);

            if (claim == null ||
                !int.TryParse(
                    claim.Value,
                    out int idUsuario))
            {
                return Unauthorized();
            }

            // =====================================================
            // DETALLES VÁLIDOS
            // =====================================================

            var detallesValidos =
                detalles
                    .Where(d => d.Cantidad > 0)
                    .ToList();

            var idsProductos =
                detallesValidos
                    .Select(d => d.IdProducto)
                    .Distinct()
                    .ToList();

            if (!idsProductos.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Debe seleccionar al menos un producto.");

                await CargarCombos(model);

                return View(model);
            }

            // =====================================================
            // VALIDAR PRODUCTOS
            // =====================================================

            var productos =
                await _context.Productos
                    .Where(p =>
                        idsProductos.Contains(p.IdProducto) &&
                        p.Estado)
                    .ToDictionaryAsync(
                        p => p.IdProducto);

            if (productos.Count != idsProductos.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Uno o más productos no existen o están inactivos.");

                await CargarCombos(model);

                return View(model);
            }

            // =====================================================
            // TRANSACCIÓN
            // =====================================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                // =================================================
                // CREAR PEDIDO
                // =================================================

                var pedido = new Pedido
                {
                    IdCliente =
                        model.IdCliente,

                    IdMesa =
                        model.IdMesa,

                    IdUsuario =
                        idUsuario,

                    IdTipoPedido =
                        model.IdTipoPedido,

                    IdEstadoPedido =
                        await ObtenerEstadoInicial(),

                    NumeroPedido =
                        await GenerarNumeroPedido(),

                    Observacion =
                        model.Observacion,

                    Subtotal =
                        0m,

                    Descuento =
                        0m,

                    Impuesto =
                        0m,

                    Total =
                        0m,

                    FechaPedido =
                        DateTime.Now
                };

                _context.Pedidos.Add(pedido);

                await _context.SaveChangesAsync();

                // =================================================
                // TOTALES
                // =================================================

                decimal subtotalPedido = 0m;
                decimal descuentoPedido = 0m;
                decimal impuestoPedido = 0m;

                // =================================================
                // CREAR DETALLES
                // =================================================

                foreach (var item in detallesValidos)
                {
                    if (!productos.TryGetValue(
                        item.IdProducto,
                        out var producto))
                    {
                        throw new InvalidOperationException(
                            "Uno de los productos seleccionados no es válido.");
                    }

                    decimal cantidad =
                        item.Cantidad;

                    decimal precio =
                        producto.PrecioVenta;

                    decimal subtotalLinea =
                        precio * cantidad;

                    decimal descuentoLinea =
                        Math.Max(
                            0m,
                            item.Descuento);

                    if (descuentoLinea >
                        subtotalLinea)
                    {
                        descuentoLinea =
                            subtotalLinea;
                    }

                    decimal impuestoLinea =
                        0m;

                    decimal totalLinea =
                        subtotalLinea
                        - descuentoLinea
                        + impuestoLinea;

                    var detalle =
                        new DetallePedido
                        {
                            IdPedido =
                                pedido.IdPedido,

                            IdProducto =
                                producto.IdProducto,

                            Cantidad =
                                cantidad,

                            PrecioUnitario =
                                precio,

                            Descuento =
                                descuentoLinea,

                            Impuesto =
                                impuestoLinea,

                            Subtotal =
                                subtotalLinea,

                            TotalLinea =
                                totalLinea,

                            Observacion =
                                item.Observacion
                        };

                    _context.DetallePedidos.Add(
                        detalle);

                    subtotalPedido +=
                        subtotalLinea;

                    descuentoPedido +=
                        descuentoLinea;

                    impuestoPedido +=
                        impuestoLinea;
                }

                // =================================================
                // ACTUALIZAR TOTALES
                // =================================================

                pedido.Subtotal =
                    subtotalPedido;

                pedido.Descuento =
                    descuentoPedido;

                pedido.Impuesto =
                    impuestoPedido;

                pedido.Total =
                    subtotalPedido
                    - descuentoPedido
                    + impuestoPedido;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Mensaje"] =
                    "Pedido creado correctamente.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al guardar el pedido.");

                await CargarCombos(model);

                return View(model);
            }
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var pedido =
                await _context.Pedidos
                    .Include(p =>
                        p.DetallePedidos)
                    .Include(p =>
                        p.IdEstadoPedidoNavigation)
                    .FirstOrDefaultAsync(
                        p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            // =====================================================
            // NO EDITAR CANCELADOS
            // =====================================================

            if (EsEstadoCancelado(
                pedido.IdEstadoPedidoNavigation?.Nombre))
            {
                TempData["Error"] =
                    "No se puede modificar un pedido cancelado.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // =====================================================
            // NO EDITAR FACTURADOS
            // =====================================================

            var tieneFactura =
                await _context.Facturas
                    .AnyAsync(f =>
                        f.IdPedido ==
                        pedido.IdPedido);

            if (tieneFactura)
            {
                TempData["Error"] =
                    "No se puede modificar un pedido que ya fue facturado.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // =====================================================
            // CREAR VIEWMODEL
            // =====================================================

            var model =
                new PedidoViewModel
                {
                    IdPedido =
                        pedido.IdPedido,

                    IdCliente =
                        pedido.IdCliente,

                    IdMesa =
                        pedido.IdMesa,

                    IdTipoPedido =
                        pedido.IdTipoPedido,

                    IdEstadoPedido =
                        pedido.IdEstadoPedido,

                    FechaPedido =
                        pedido.FechaPedido,

                    Observacion =
                        pedido.Observacion,

                    Subtotal =
                        pedido.Subtotal,

                    Descuento =
                        pedido.Descuento,

                    Impuesto =
                        pedido.Impuesto,

                    Total =
                        pedido.Total,

                    Detalles =
                        pedido.DetallePedidos
                            .Select(d =>
                                new DetallePedidoViewModel
                                {
                                    IdDetallePedido =
                                        d.IdDetallePedido,

                                    IdPedido =
                                        d.IdPedido,

                                    IdProducto =
                                        d.IdProducto,

                                    Cantidad =
                                        d.Cantidad,

                                    PrecioUnitario =
                                        d.PrecioUnitario,

                                    Descuento =
                                        d.Descuento,

                                    Impuesto =
                                        d.Impuesto,

                                    Subtotal =
                                        d.Subtotal,

                                    TotalLinea =
                                        d.TotalLinea,

                                    Observacion =
                                        d.Observacion
                                })
                            .ToList()
                };

            await CargarCombos(model);

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PedidoViewModel model)
        {
            if (id != model.IdPedido)
                return NotFound();

            var detalles =
                model.Detalles
                ?? new List<DetallePedidoViewModel>();

            // =====================================================
            // VALIDAR PRODUCTOS
            // =====================================================

            if (!detalles.Any(d => d.Cantidad > 0))
            {
                ModelState.AddModelError(
                    "",
                    "El pedido debe contener al menos un producto.");
            }

            // =====================================================
            // VALIDAR CANTIDADES
            // =====================================================

            foreach (var detalle in detalles)
            {
                if (detalle.Cantidad < 0)
                {
                    ModelState.AddModelError(
                        "",
                        "La cantidad de un producto no puede ser negativa.");
                }

                if (detalle.Cantidad > 0 &&
                    detalle.Cantidad != Math.Truncate(
                        detalle.Cantidad))
                {
                    ModelState.AddModelError(
                        "",
                        "La cantidad de los productos debe ser un número entero.");
                }

                if (detalle.Descuento < 0)
                {
                    ModelState.AddModelError(
                        "",
                        "El descuento no puede ser negativo.");
                }
            }

            // =====================================================
            // VALIDAR MODELO
            // =====================================================

            if (!ModelState.IsValid)
            {
                await CargarCombos(model);
                return View(model);
            }

            // =====================================================
            // BUSCAR PEDIDO
            // =====================================================

            var pedido =
                await _context.Pedidos
                    .Include(p =>
                        p.DetallePedidos)
                    .Include(p =>
                        p.IdEstadoPedidoNavigation)
                    .FirstOrDefaultAsync(
                        p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            // =====================================================
            // NO EDITAR CANCELADOS
            // =====================================================

            if (EsEstadoCancelado(
                pedido.IdEstadoPedidoNavigation?.Nombre))
            {
                TempData["Error"] =
                    "No se puede modificar un pedido cancelado.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // =====================================================
            // NO EDITAR FACTURADOS
            // =====================================================

            var tieneFactura =
                await _context.Facturas
                    .AnyAsync(f =>
                        f.IdPedido ==
                        pedido.IdPedido);

            if (tieneFactura)
            {
                TempData["Error"] =
                    "No se puede modificar un pedido que ya fue facturado.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // =====================================================
            // DETALLES VÁLIDOS
            // =====================================================

            var detallesValidos =
                detalles
                    .Where(d => d.Cantidad > 0)
                    .ToList();

            var idsProductos =
                detallesValidos
                    .Select(d => d.IdProducto)
                    .Distinct()
                    .ToList();

            if (!idsProductos.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Debe seleccionar al menos un producto.");

                await CargarCombos(model);

                return View(model);
            }

            // =====================================================
            // PRODUCTOS
            // =====================================================

            var productos =
                await _context.Productos
                    .Where(p =>
                        idsProductos.Contains(p.IdProducto) &&
                        p.Estado)
                    .ToDictionaryAsync(
                        p => p.IdProducto);

            if (productos.Count !=
                idsProductos.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Uno o más productos no existen o están inactivos.");

                await CargarCombos(model);

                return View(model);
            }

            // =====================================================
            // TRANSACCIÓN
            // =====================================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                // =================================================
                // ACTUALIZAR CABECERA
                // =================================================

                pedido.IdCliente =
                    model.IdCliente;

                pedido.IdMesa =
                    model.IdMesa;

                pedido.IdTipoPedido =
                    model.IdTipoPedido;

                pedido.Observacion =
                    model.Observacion;

                // =================================================
                // ELIMINAR DETALLES ANTERIORES
                // =================================================

                _context.DetallePedidos.RemoveRange(
                    pedido.DetallePedidos);

                // =================================================
                // TOTALES
                // =================================================

                decimal subtotalPedido = 0m;
                decimal descuentoPedido = 0m;
                decimal impuestoPedido = 0m;

                // =================================================
                // CREAR NUEVOS DETALLES
                // =================================================

                foreach (var item in detallesValidos)
                {
                    if (!productos.TryGetValue(
                        item.IdProducto,
                        out var producto))
                    {
                        throw new InvalidOperationException(
                            "Uno de los productos seleccionados no es válido.");
                    }

                    decimal precio =
                        producto.PrecioVenta;

                    decimal subtotalLinea =
                        precio * item.Cantidad;

                    decimal descuentoLinea =
                        Math.Max(
                            0m,
                            item.Descuento);

                    if (descuentoLinea >
                        subtotalLinea)
                    {
                        descuentoLinea =
                            subtotalLinea;
                    }

                    decimal impuestoLinea =
                        0m;

                    decimal totalLinea =
                        subtotalLinea
                        - descuentoLinea
                        + impuestoLinea;

                    var detalle =
                        new DetallePedido
                        {
                            IdPedido =
                                pedido.IdPedido,

                            IdProducto =
                                producto.IdProducto,

                            Cantidad =
                                item.Cantidad,

                            PrecioUnitario =
                                precio,

                            Descuento =
                                descuentoLinea,

                            Impuesto =
                                impuestoLinea,

                            Subtotal =
                                subtotalLinea,

                            TotalLinea =
                                totalLinea,

                            Observacion =
                                item.Observacion
                        };

                    _context.DetallePedidos.Add(
                        detalle);

                    subtotalPedido +=
                        subtotalLinea;

                    descuentoPedido +=
                        descuentoLinea;

                    impuestoPedido +=
                        impuestoLinea;
                }

                // =================================================
                // ACTUALIZAR TOTALES
                // =================================================

                pedido.Subtotal =
                    subtotalPedido;

                pedido.Descuento =
                    descuentoPedido;

                pedido.Impuesto =
                    impuestoPedido;

                pedido.Total =
                    subtotalPedido
                    - descuentoPedido
                    + impuestoPedido;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Mensaje"] =
                    "Pedido actualizado correctamente.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al actualizar el pedido.");

                await CargarCombos(model);

                return View(model);
            }
        }

        // =========================================================
        // CAMBIAR ESTADO
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(
            int id,
            int idEstado)
        {
            var pedido =
                await _context.Pedidos
                    .Include(p =>
                        p.IdEstadoPedidoNavigation)
                    .FirstOrDefaultAsync(
                        p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            // =====================================================
            // PEDIDO CANCELADO
            // =====================================================

            if (EsEstadoCancelado(
                pedido.IdEstadoPedidoNavigation?.Nombre))
            {
                TempData["Error"] =
                    "No se puede cambiar el estado de un pedido cancelado.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // =====================================================
            // BUSCAR NUEVO ESTADO
            // =====================================================

            var nuevoEstado =
                await _context.EstadoPedidos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        e =>
                            e.IdEstadoPedido ==
                            idEstado);

            if (nuevoEstado == null)
            {
                TempData["Error"] =
                    "El estado seleccionado no existe.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // =====================================================
            // NO CANCELAR DESDE CAMBIAR ESTADO
            // =====================================================

            if (EsEstadoCancelado(
                nuevoEstado.Nombre))
            {
                TempData["Error"] =
                    "Para cancelar un pedido utilice la opción Cancelar.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // =====================================================
            // ACTUALIZAR ESTADO
            // =====================================================

            pedido.IdEstadoPedido =
                nuevoEstado.IdEstadoPedido;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Estado del pedido actualizado.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================================================
        // CANCELAR / ANULAR PEDIDO
        // SOLO ADMINISTRADOR Y GERENTE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Anular(int id)
        {
            // =====================================================
            // BUSCAR PEDIDO
            // =====================================================

            var pedido =
                await _context.Pedidos
                    .Include(p =>
                        p.IdEstadoPedidoNavigation)
                    .FirstOrDefaultAsync(
                        p => p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            // =====================================================
            // YA CANCELADO
            // =====================================================

            if (EsEstadoCancelado(
                pedido.IdEstadoPedidoNavigation?.Nombre))
            {
                TempData["Error"] =
                    "El pedido ya se encuentra cancelado.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // =====================================================
            // NO CANCELAR FACTURADO
            // =====================================================

            var tieneFactura =
                await _context.Facturas
                    .AnyAsync(f =>
                        f.IdPedido ==
                        pedido.IdPedido);

            if (tieneFactura)
            {
                TempData["Error"] =
                    "No se puede cancelar un pedido que ya fue facturado.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // =====================================================
            // OBTENER ESTADO CANCELADO
            // =====================================================

            var estadoCancelado =
                await _context.EstadoPedidos
                    .FirstOrDefaultAsync(e =>
                        e.Nombre != null &&
                        e.Nombre.ToLower() ==
                        "cancelado");

            if (estadoCancelado == null)
            {
                TempData["Error"] =
                    "No existe un estado de pedido 'Cancelado' configurado.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            // =====================================================
            // TRANSACCIÓN
            // =====================================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                // =================================================
                // CAMBIAR ESTADO
                // =================================================

                pedido.IdEstadoPedido =
                    estadoCancelado.IdEstadoPedido;

                // =================================================
                // IMPORTANTE:
                // NO SE MODIFICA EL TOTAL
                // =================================================

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Mensaje"] =
                    "Pedido cancelado correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Ocurrió un error al cancelar el pedido.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
        }

        // =========================================================
        // CARGAR COMBOS
        // =========================================================

        private async Task CargarCombos(
            PedidoViewModel model)
        {
            // =====================================================
            // CLIENTES
            // =====================================================

            model.Clientes =
                await _context.Clientes
                    .AsNoTracking()
                    .Where(c =>
                        c.Estado)
                    .OrderBy(c =>
                        c.Nombres)
                    .Select(c =>
                        new SelectListItem
                        {
                            Value =
                                c.IdCliente.ToString(),

                            Text =
                                c.Nombres +
                                " " +
                                c.Apellidos
                        })
                    .ToListAsync();

            // =====================================================
            // MESAS
            // =====================================================

            model.Mesas =
                await _context.Mesas
                    .AsNoTracking()
                    .Where(m =>
                        m.Estado)
                    .OrderBy(m =>
                        m.NumeroMesa)
                    .Select(m =>
                        new SelectListItem
                        {
                            Value =
                                m.IdMesa.ToString(),

                            Text =
                                "Mesa " +
                                m.NumeroMesa
                        })
                    .ToListAsync();

            // =====================================================
            // TIPOS DE PEDIDO
            // =====================================================

            model.TiposPedido =
                await _context.TipoPedidos
                    .AsNoTracking()
                    .OrderBy(t =>
                        t.Nombre)
                    .Select(t =>
                        new SelectListItem
                        {
                            Value =
                                t.IdTipoPedido.ToString(),

                            Text =
                                t.Nombre
                        })
                    .ToListAsync();

            // =====================================================
            // ESTADOS
            // =====================================================

            model.EstadosPedido =
                await _context.EstadoPedidos
                    .AsNoTracking()
                    .OrderBy(e =>
                        e.IdEstadoPedido)
                    .Select(e =>
                        new SelectListItem
                        {
                            Value =
                                e.IdEstadoPedido.ToString(),

                            Text =
                                e.Nombre ?? ""
                        })
                    .ToListAsync();

            // =====================================================
            // PRODUCTOS
            // =====================================================

            await CargarProductosEnDetalles(model);
        }

        // =========================================================
        // CARGAR PRODUCTOS EN DETALLES
        // =========================================================

        private async Task CargarProductosEnDetalles(
            PedidoViewModel model)
        {
            var productos =
                await _context.Productos
                    .AsNoTracking()
                    .Where(p =>
                        p.Estado &&
                        p.IdEstadoProductoNavigation != null)
                    .OrderBy(p =>
                        p.Nombre)
                    .Select(p =>
                        new SelectListItem
                        {
                            Value =
                                p.IdProducto.ToString(),

                            Text =
                                p.Nombre +
                                " - C$ " +
                                p.PrecioVenta.ToString("N2")
                        })
                    .ToListAsync();

            var detalles =
                model.Detalles
                ?? new List<DetallePedidoViewModel>();

            foreach (var detalle in detalles)
            {
                detalle.Productos =
                    productos;
            }
        }

        // =========================================================
        // OBTENER ESTADO INICIAL
        // =========================================================

        private async Task<int> ObtenerEstadoInicial()
        {
            // =====================================================
            // BUSCAR ESTADO PENDIENTE
            // =====================================================

            var estado =
                await _context.EstadoPedidos
                    .FirstOrDefaultAsync(e =>
                        e.Nombre != null &&
                        e.Nombre.ToLower() ==
                        "pendiente");

            if (estado != null)
                return estado.IdEstadoPedido;

            // =====================================================
            // SI NO EXISTE PENDIENTE, USAR PRIMER ESTADO
            // =====================================================

            estado =
                await _context.EstadoPedidos
                    .OrderBy(e =>
                        e.IdEstadoPedido)
                    .FirstOrDefaultAsync();

            if (estado == null)
            {
                throw new InvalidOperationException(
                    "No existen estados de pedido configurados.");
            }

            return estado.IdEstadoPedido;
        }

        // =========================================================
        // GENERAR NÚMERO DE PEDIDO
        // =========================================================

        private async Task<string> GenerarNumeroPedido()
        {
            var ultimoNumero =
                await _context.Pedidos
                    .OrderByDescending(p =>
                        p.IdPedido)
                    .Select(p =>
                        p.NumeroPedido)
                    .FirstOrDefaultAsync();

            int siguiente = 1;

            if (!string.IsNullOrWhiteSpace(
                ultimoNumero))
            {
                var soloNumeros =
                    new string(
                        ultimoNumero
                            .Where(char.IsDigit)
                            .ToArray());

                if (int.TryParse(
                    soloNumeros,
                    out int numero))
                {
                    siguiente =
                        numero + 1;
                }
            }

            return $"PED-{siguiente:000000}";
        }

        // =========================================================
        // NORMALIZAR TEXTO PARA BÚSQUEDAS
        // =========================================================

        private string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            texto =
                texto.Trim()
                    .ToLowerInvariant();

            var textoNormalizado =
                texto.Normalize(
                    NormalizationForm.FormD);

            var caracteres =
                textoNormalizado
                    .Where(c =>
                        CharUnicodeInfo.GetUnicodeCategory(c)
                        != UnicodeCategory.NonSpacingMark)
                    .ToArray();

            return new string(caracteres)
                .Normalize(
                    NormalizationForm.FormC);
        }

        // =========================================================
        // COMPROBAR ESTADO CANCELADO
        // =========================================================

        private bool EsEstadoCancelado(
            string? nombreEstado)
        {
            if (string.IsNullOrWhiteSpace(
                nombreEstado))
            {
                return false;
            }

            return nombreEstado
                .Trim()
                .Equals(
                    "Cancelado",
                    StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // COMPROBAR ESTADO
        // =========================================================

        private bool EsEstado(
            string? nombreEstado,
            string estadoBuscado)
        {
            if (string.IsNullOrWhiteSpace(
                nombreEstado))
            {
                return false;
            }

            return nombreEstado
                .Trim()
                .Equals(
                    estadoBuscado,
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}