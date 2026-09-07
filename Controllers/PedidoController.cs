using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class PedidoController : Controller
    {
        private readonly RestauranteContext _context;

        // =========================================================
        // ESTADOS DEL PEDIDO
        // =========================================================

        private const int ESTADO_PENDIENTE = 1;
        private const int ESTADO_EN_COCINA = 2;
        private const int ESTADO_PREPARANDOSE = 3;
        private const int ESTADO_LISTO = 4;
        private const int ESTADO_ENTREGADO = 5;
        private const int ESTADO_FACTURADO = 6;
        private const int ESTADO_CANCELADO = 7;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public PedidoController(RestauranteContext context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? buscar,
            int? mes,
            int? anio)
        {
            var hoy = DateTime.Today;

            int mesSeleccionado =
                mes.HasValue &&
                mes.Value >= 1 &&
                mes.Value <= 12
                    ? mes.Value
                    : hoy.Month;

            int anioSeleccionado =
                anio.HasValue &&
                anio.Value >= 2000 &&
                anio.Value <= 2100
                    ? anio.Value
                    : hoy.Year;

            var inicioMes =
                new DateTime(
                    anioSeleccionado,
                    mesSeleccionado,
                    1);

            var inicioMesSiguiente =
                inicioMes.AddMonths(1);

            var inicioDiaActual =
                hoy;

            var inicioDiaSiguiente =
                hoy.AddDays(1);

            // =====================================================
            // OBTENER PEDIDOS
            // =====================================================

            var pedidos =
                await _context.Pedidos
                    .AsNoTracking()
                    .Include(p =>
                        p.IdClienteNavigation)
                    .Include(p =>
                        p.IdMesaNavigation)
                    .Include(p =>
                        p.IdEstadoPedidoNavigation)
                    .Include(p =>
                        p.IdTipoPedidoNavigation)
                    .Include(p =>
                        p.IdUsuarioNavigation)
                    .OrderByDescending(p =>
                        p.FechaPedido)
                    .ToListAsync();

            // =====================================================
            // FACTURAS EXISTENTES
            //
            // Se cargan una sola vez para determinar qué pedidos
            // están disponibles para cobrar.
            // =====================================================

            var pedidosConFactura =
                await _context.Facturas
                    .AsNoTracking()
                    .Select(f =>
                        f.IdPedido)
                    .ToHashSetAsync();

            // =====================================================
            // PEDIDOS DISPONIBLES PARA COBRO
            //
            // REGLA:
            //
            // Un pedido solamente puede ir a Caja/Facturación
            // cuando está LISTO y todavía no tiene factura.
            //
            // NO se permite esperar a que esté Entregado.
            // =====================================================

            var pedidosParaCobrar =
                pedidos
                    .Where(p =>
                        p.IdEstadoPedido ==
                            ESTADO_LISTO
                        &&
                        !pedidosConFactura.Contains(
                            p.IdPedido))
                    .Select(p =>
                        p.IdPedido)
                    .ToHashSet();

            ViewBag.PedidosParaCobrar =
                pedidosParaCobrar;

            // =====================================================
            // PEDIDOS DEL MES
            // =====================================================

            var pedidosMes =
                pedidos
                    .Where(p =>
                        p.FechaPedido >= inicioMes &&
                        p.FechaPedido < inicioMesSiguiente)
                    .ToList();

            // =====================================================
            // PEDIDOS DE HOY
            // =====================================================

            var pedidosHoy =
                pedidos
                    .Where(p =>
                        p.FechaPedido >= inicioDiaActual &&
                        p.FechaPedido < inicioDiaSiguiente)
                    .ToList();

            // =====================================================
            // CANCELADOS
            // =====================================================

            var pedidosCanceladosMes =
                pedidosMes
                    .Where(p =>
                        EsEstadoCancelado(
                            p.IdEstadoPedidoNavigation?.Nombre))
                    .ToList();

            var pedidosCanceladosHoy =
                pedidosHoy
                    .Where(p =>
                        EsEstadoCancelado(
                            p.IdEstadoPedidoNavigation?.Nombre))
                    .ToList();

            // =====================================================
            // VENTAS
            // =====================================================

            var pedidosVentaMes =
                pedidosMes
                    .Where(p =>
                        !EsEstadoCancelado(
                            p.IdEstadoPedidoNavigation?.Nombre))
                    .ToList();

            var pedidosVentaHoy =
                pedidosHoy
                    .Where(p =>
                        !EsEstadoCancelado(
                            p.IdEstadoPedidoNavigation?.Nombre))
                    .ToList();

            // =====================================================
            // TOTALES
            // =====================================================

            decimal totalVendidoMes =
                pedidosVentaMes.Sum(p =>
                    p.Total);

            decimal totalVendidoHoy =
                pedidosVentaHoy.Sum(p =>
                    p.Total);

            int cantidadPedidosMes =
                pedidosVentaMes.Count;

            int cantidadPedidosHoy =
                pedidosVentaHoy.Count;

            int cantidadCanceladosMes =
                pedidosCanceladosMes.Count;

            int cantidadCanceladosHoy =
                pedidosCanceladosHoy.Count;

            decimal ticketPromedioMes =
                cantidadPedidosMes > 0
                    ? totalVendidoMes /
                      cantidadPedidosMes
                    : 0m;

            decimal ticketPromedioHoy =
                cantidadPedidosHoy > 0
                    ? totalVendidoHoy /
                      cantidadPedidosHoy
                    : 0m;

            // =====================================================
            // ESTADOS
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
            // VIEWBAG
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
            // SELECTOR DE MES
            // =====================================================

            ViewBag.MesSeleccionado =
                mesSeleccionado;

            ViewBag.AnioSeleccionado =
                anioSeleccionado;

            var nombreMes =
                CultureInfo
                    .GetCultureInfo("es-ES")
                    .DateTimeFormat
                    .GetMonthName(mesSeleccionado);

            ViewBag.NombreMes =
                CultureInfo
                    .GetCultureInfo("es-ES")
                    .TextInfo
                    .ToTitleCase(nombreMes);

            // =====================================================
            // AÑOS DISPONIBLES
            // =====================================================

            int primerAnio =
                pedidos.Any()
                    ? pedidos.Min(p =>
                        p.FechaPedido.Year)
                    : hoy.Year;

            int ultimoAnio =
                Math.Max(
                    hoy.Year,
                    anioSeleccionado);

            ViewBag.AniosDisponibles =
                Enumerable
                    .Range(
                        primerAnio,
                        ultimoAnio - primerAnio + 1)
                    .OrderByDescending(a =>
                        a)
                    .ToList();

            // =====================================================
            // BÚSQUEDA
            // =====================================================

            var pedidosFiltrados =
                pedidos;

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar =
                    buscar.Trim();

                var termino =
                    NormalizarTexto(buscar);

                pedidosFiltrados =
                    pedidos
                        .Where(p =>
                        {
                            var numeroPedido =
                                NormalizarTexto(
                                    p.NumeroPedido);

                            var nombreCliente =
                                p.IdClienteNavigation != null
                                    ? NormalizarTexto(
                                        $"{p.IdClienteNavigation.Nombres} " +
                                        $"{p.IdClienteNavigation.Apellidos}")
                                    : "";

                            var nombreUsuario =
                                p.IdUsuarioNavigation != null
                                    ? NormalizarTexto(
                                        $"{p.IdUsuarioNavigation.Nombres} " +
                                        $"{p.IdUsuarioNavigation.Apellidos}")
                                    : "";

                            var usuario =
                                p.IdUsuarioNavigation != null
                                    ? NormalizarTexto(
                                        p.IdUsuarioNavigation.Usuario1)
                                    : "";

                            var estado =
                                NormalizarTexto(
                                    p.IdEstadoPedidoNavigation?.Nombre);

                            var tipoPedido =
                                NormalizarTexto(
                                    p.IdTipoPedidoNavigation?.Nombre);

                            var mesa =
                                p.IdMesaNavigation != null
                                    ? p.IdMesaNavigation
                                        .NumeroMesa
                                        .ToString()
                                    : "";

                            var fecha =
                                p.FechaPedido
                                    .ToString("dd/MM/yyyy");

                            var fechaSinFormato =
                                p.FechaPedido
                                    .ToString("ddMMyyyy");

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

            ViewBag.Buscar =
                buscar;

            return View(
                pedidosFiltrados);
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int? id)
        {
            if (id == null)
                return NotFound();

            var pedido =
                await _context.Pedidos
                    .AsNoTracking()
                    .Include(p =>
                        p.IdClienteNavigation)
                    .Include(p =>
                        p.IdMesaNavigation)
                    .Include(p =>
                        p.IdUsuarioNavigation)
                    .Include(p =>
                        p.IdEstadoPedidoNavigation)
                    .Include(p =>
                        p.IdTipoPedidoNavigation)
                    .Include(p =>
                        p.DetallePedidos)
                        .ThenInclude(d =>
                            d.IdProductoNavigation)
                    .FirstOrDefaultAsync(
                        p =>
                            p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            // =====================================================
            // COMPROBAR FACTURA
            // =====================================================

            var tieneFactura =
                await _context.Facturas
                    .AsNoTracking()
                    .AnyAsync(f =>
                        f.IdPedido ==
                        pedido.IdPedido);

            ViewBag.TieneFactura =
                tieneFactura;

            // =====================================================
            // SIGUIENTE ESTADO
            //
            // PedidoController solamente maneja:
            //
            // FACTURADO -> ENTREGADO
            //
            // Cocina maneja:
            //
            // PENDIENTE -> EN COCINA
            // EN COCINA -> PREPARÁNDOSE
            // PREPARÁNDOSE -> LISTO
            //
            // Facturación maneja:
            //
            // LISTO -> FACTURADO
            // =====================================================

            var siguienteEstado =
                ObtenerSiguienteEstadoPedido(
                    pedido.IdEstadoPedido);

            ViewBag.SiguienteEstadoId =
                siguienteEstado?.Id;

            ViewBag.SiguienteEstadoNombre =
                siguienteEstado?.Nombre ?? "";

            // =====================================================
            // ESTADOS DISPONIBLES
            //
            // Solo se muestra la transición que realmente puede
            // ejecutar PedidoController.
            // =====================================================

            ViewBag.EstadosPedido =
                siguienteEstado.HasValue
                    ? new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Value =
                                siguienteEstado.Value.Id.ToString(),

                            Text =
                                siguienteEstado.Value.Nombre
                        }
                    }
                    : new List<SelectListItem>();

            // =====================================================
            // PUEDE AVANZAR
            // =====================================================

            bool usuarioPuedeAvanzar =
                User.IsInRole("Administrador") ||
                User.IsInRole("Gerente") ||
                User.IsInRole("Mesero");

            bool puedeAvanzarEstado =
                siguienteEstado.HasValue &&
                usuarioPuedeAvanzar &&
                !EsEstadoCancelado(
                    pedido.IdEstadoPedidoNavigation?.Nombre) &&
                pedido.IdEstadoPedido !=
                    ESTADO_ENTREGADO;

            ViewBag.PuedeAvanzarEstado =
                puedeAvanzarEstado;

            // =====================================================
            // PUEDE EDITAR
            // =====================================================

            ViewBag.PuedeEditar =
                pedido.IdEstadoPedido ==
                ESTADO_PENDIENTE &&
                !tieneFactura;

            // =====================================================
            // PUEDE FACTURAR
            //
            // El pedido debe estar LISTO.
            // Nunca se factura después de entregar.
            // =====================================================

            bool usuarioPuedeFacturar =
                User.IsInRole("Administrador") ||
                User.IsInRole("Gerente");

            ViewBag.PuedeFacturar =
                pedido.IdEstadoPedido ==
                    ESTADO_LISTO &&
                !tieneFactura &&
                usuarioPuedeFacturar;

            return View(
                pedido);
        }

        // =========================================================
        // IR A COBRAR / FACTURAR
        //
        // Flujo:
        //
        // LISTO -> FACTURACIÓN -> PAGO -> FACTURADO
        //                                |
        //                                v
        //                            ENTREGADO
        //
        // PedidoController NO entrega un pedido que no esté
        // facturado/pagado.
        // =========================================================

        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> IrACobrar(
            int id)
        {
            var pedido =
                await _context.Pedidos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        p =>
                            p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            // =====================================================
            // DEBE ESTAR LISTO
            // =====================================================

            if (pedido.IdEstadoPedido !=
                ESTADO_LISTO)
            {
                TempData["Error"] =
                    "El pedido debe estar Listo antes de enviarlo a Caja.";

                return RedirectToAction(
                    nameof(Index));
            }

            // =====================================================
            // COMPROBAR SI YA TIENE FACTURA
            // =====================================================

            var tieneFactura =
                await _context.Facturas
                    .AsNoTracking()
                    .AnyAsync(f =>
                        f.IdPedido ==
                        pedido.IdPedido);

            if (tieneFactura)
            {
                TempData["Error"] =
                    "Este pedido ya tiene una factura registrada.";

                return RedirectToAction(
                    nameof(Index));
            }

            // =====================================================
            // IR A FACTURACIÓN
            // =====================================================

            return RedirectToAction(
                "Create",
                "Facturacion",
                new
                {
                    idPedido =
                        pedido.IdPedido
                });
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente,Mesero")]
        public async Task<IActionResult> Create()
        {
            var model =
                new PedidoViewModel
                {
                    FechaPedido =
                        DateTime.Now
                };

            model.Detalles ??=
                new List<DetallePedidoViewModel>();

            model.Detalles.Add(
                new DetallePedidoViewModel());

            await CargarCombos(
                model);

            return View(
                model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gerente,Mesero")]
        public async Task<IActionResult> Create(
            PedidoViewModel model)
        {
            var detalles =
                model.Detalles
                ?? new List<DetallePedidoViewModel>();

            // =====================================================
            // CLIENTE
            // =====================================================

            if (model.IdCliente == null ||
                model.IdCliente <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.IdCliente),
                    "Debe seleccionar un cliente.");
            }

            // =====================================================
            // TIPO PEDIDO
            // =====================================================

            if (model.IdTipoPedido <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.IdTipoPedido),
                    "Debe seleccionar un tipo de pedido.");
            }

            // =====================================================
            // DETALLES
            // =====================================================

            if (!detalles.Any(d =>
                    d.Cantidad > 0))
            {
                ModelState.AddModelError(
                    "",
                    "El pedido debe contener al menos un producto.");
            }

            foreach (var detalle in detalles)
            {
                if (detalle.Cantidad < 0)
                {
                    ModelState.AddModelError(
                        "",
                        "La cantidad de un producto no puede ser negativa.");
                }

                if (detalle.Cantidad > 0 &&
                    detalle.Cantidad !=
                    Math.Truncate(
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

            if (!ModelState.IsValid)
            {
                await CargarCombos(
                    model);

                return View(
                    model);
            }

            // =====================================================
            // USUARIO AUTENTICADO
            // =====================================================

            var claim =
                User.FindFirst(
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
                    .Where(d =>
                        d.Cantidad > 0)
                    .ToList();

            var idsProductos =
                detallesValidos
                    .Select(d =>
                        d.IdProducto)
                    .Distinct()
                    .ToList();

            if (!idsProductos.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Debe seleccionar al menos un producto.");

                await CargarCombos(
                    model);

                return View(
                    model);
            }

            // =====================================================
            // PRODUCTOS ACTIVOS
            // =====================================================

            var productos =
                await _context.Productos
                    .Where(p =>
                        idsProductos.Contains(
                            p.IdProducto)
                        &&
                        p.Estado)
                    .ToDictionaryAsync(
                        p =>
                            p.IdProducto);

            if (productos.Count !=
                idsProductos.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Uno o más productos no existen o están inactivos.");

                await CargarCombos(
                    model);

                return View(
                    model);
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

                var pedido =
                    new Pedido
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

                _context.Pedidos.Add(
                    pedido);

                await _context.SaveChangesAsync();

                decimal subtotalPedido =
                    0m;

                decimal descuentoPedido =
                    0m;

                decimal impuestoPedido =
                    0m;

                // =================================================
                // DETALLES
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
                        precio *
                        cantidad;

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
                        -
                        descuentoLinea
                        +
                        impuestoLinea;

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
                // TOTALES
                // =================================================

                pedido.Subtotal =
                    subtotalPedido;

                pedido.Descuento =
                    descuentoPedido;

                pedido.Impuesto =
                    impuestoPedido;

                pedido.Total =
                    subtotalPedido
                    -
                    descuentoPedido
                    +
                    impuestoPedido;

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

                await CargarCombos(
                    model);

                return View(
                    model);
            }
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente,Mesero")]
        public async Task<IActionResult> Edit(
            int? id)
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
                        p =>
                            p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            if (pedido.IdEstadoPedido !=
                ESTADO_PENDIENTE)
            {
                TempData["Error"] =
                    "Este pedido ya fue enviado a Cocina y no puede editarse.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            var tieneFactura =
                await _context.Facturas
                    .AsNoTracking()
                    .AnyAsync(f =>
                        f.IdPedido ==
                        pedido.IdPedido);

            if (tieneFactura)
            {
                TempData["Error"] =
                    "No se puede modificar un pedido que ya fue facturado.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

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

            if (!model.Detalles.Any())
            {
                model.Detalles.Add(
                    new DetallePedidoViewModel());
            }

            await CargarCombos(
                model);

            return View(
                model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gerente,Mesero")]
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
            // VALIDACIONES
            // =====================================================

            if (model.IdCliente == null ||
                model.IdCliente <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.IdCliente),
                    "Debe seleccionar un cliente.");
            }

            if (model.IdTipoPedido <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.IdTipoPedido),
                    "Debe seleccionar un tipo de pedido.");
            }

            if (!detalles.Any(d =>
                    d.Cantidad > 0))
            {
                ModelState.AddModelError(
                    "",
                    "El pedido debe contener al menos un producto.");
            }

            foreach (var detalle in detalles)
            {
                if (detalle.Cantidad < 0)
                {
                    ModelState.AddModelError(
                        "",
                        "La cantidad de un producto no puede ser negativa.");
                }

                if (detalle.Cantidad > 0 &&
                    detalle.Cantidad !=
                    Math.Truncate(
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

            if (!ModelState.IsValid)
            {
                await CargarCombos(
                    model);

                return View(
                    model);
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
                        p =>
                            p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            // =====================================================
            // SOLO PENDIENTE
            // =====================================================

            if (pedido.IdEstadoPedido !=
                ESTADO_PENDIENTE)
            {
                TempData["Error"] =
                    "Este pedido ya fue enviado a Cocina y no puede editarse.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // NO EDITAR FACTURADOS
            // =====================================================

            var tieneFactura =
                await _context.Facturas
                    .AsNoTracking()
                    .AnyAsync(f =>
                        f.IdPedido ==
                        pedido.IdPedido);

            if (tieneFactura)
            {
                TempData["Error"] =
                    "No se puede modificar un pedido que ya fue facturado.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // DETALLES VÁLIDOS
            // =====================================================

            var detallesValidos =
                detalles
                    .Where(d =>
                        d.Cantidad > 0)
                    .ToList();

            var idsProductos =
                detallesValidos
                    .Select(d =>
                        d.IdProducto)
                    .Distinct()
                    .ToList();

            if (!idsProductos.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Debe seleccionar al menos un producto.");

                await CargarCombos(
                    model);

                return View(
                    model);
            }

            // =====================================================
            // PRODUCTOS ACTIVOS
            // =====================================================

            var productos =
                await _context.Productos
                    .Where(p =>
                        idsProductos.Contains(
                            p.IdProducto)
                        &&
                        p.Estado)
                    .ToDictionaryAsync(
                        p =>
                            p.IdProducto);

            if (productos.Count !=
                idsProductos.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Uno o más productos no existen o están inactivos.");

                await CargarCombos(
                    model);

                return View(
                    model);
            }

            // =====================================================
            // TRANSACCIÓN
            // =====================================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
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

                decimal subtotalPedido =
                    0m;

                decimal descuentoPedido =
                    0m;

                decimal impuestoPedido =
                    0m;

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
                        precio *
                        item.Cantidad;

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
                        -
                        descuentoLinea
                        +
                        impuestoLinea;

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
                    -
                    descuentoPedido
                    +
                    impuestoPedido;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Mensaje"] =
                    "Pedido actualizado correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al actualizar el pedido.");

                await CargarCombos(
                    model);

                return View(
                    model);
            }
        }

        // =========================================================
        // CAMBIAR ESTADO
        //
        // PedidoController:
        //
        // FACTURADO -> ENTREGADO
        //
        // Cocina:
        //
        // PENDIENTE -> EN COCINA
        // EN COCINA -> PREPARÁNDOSE
        // PREPARÁNDOSE -> LISTO
        //
        // Facturación:
        //
        // LISTO -> FACTURADO
        //
        // IMPORTANTE:
        //
        // NUNCA se permite:
        //
        // LISTO -> ENTREGADO
        //
        // El pedido tiene que estar FACTURADO/PAGADO.
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gerente,Mesero")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            int idEstado)
        {
            var pedido =
                await _context.Pedidos
                    .Include(p =>
                        p.IdEstadoPedidoNavigation)
                    .FirstOrDefaultAsync(
                        p =>
                            p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            // =====================================================
            // CANCELADO
            // =====================================================

            if (EsEstadoCancelado(
                pedido.IdEstadoPedidoNavigation?.Nombre))
            {
                TempData["Error"] =
                    "No se puede cambiar el estado de un pedido cancelado.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // YA ENTREGADO
            // =====================================================

            if (pedido.IdEstadoPedido ==
                ESTADO_ENTREGADO)
            {
                TempData["Error"] =
                    "El pedido ya fue entregado.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // VALIDAR ESTADO
            // =====================================================

            if (idEstado <= 0)
            {
                TempData["Error"] =
                    "Debe seleccionar un estado válido.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // NO CANCELAR DESDE CAMBIAR ESTADO
            // =====================================================

            if (idEstado ==
                ESTADO_CANCELADO)
            {
                TempData["Error"] =
                    "Para cancelar un pedido utilice la opción Cancelar.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // FACTURADO NO SE ASIGNA MANUALMENTE
            //
            // Facturación debe establecer este estado cuando
            // finalice correctamente el proceso de pago.
            // =====================================================

            if (idEstado ==
                ESTADO_FACTURADO)
            {
                TempData["Error"] =
                    "El estado Facturado solamente puede establecerse mediante el proceso de facturación y pago.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // ENTREGADO
            //
            // REGLA FUNDAMENTAL:
            //
            // SOLO FACTURADO/PAGADO -> ENTREGADO
            // =====================================================

            if (idEstado ==
                ESTADO_ENTREGADO)
            {
                if (pedido.IdEstadoPedido !=
                    ESTADO_FACTURADO)
                {
                    TempData["Error"] =
                        "No se puede entregar el pedido porque todavía no ha sido pagado.";

                    return RedirectToAction(
                        nameof(Details),
                        new
                        {
                            id
                        });
                }
            }

            // =====================================================
            // OBTENER SIGUIENTE ESTADO
            // =====================================================

            var siguienteEstado =
                ObtenerSiguienteEstadoPedido(
                    pedido.IdEstadoPedido);

            if (!siguienteEstado.HasValue)
            {
                TempData["Error"] =
                    "Este pedido no tiene una transición disponible desde Pedido.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // VALIDAR TRANSICIÓN
            // =====================================================

            if (idEstado !=
                siguienteEstado.Value.Id)
            {
                TempData["Error"] =
                    $"No se puede pasar de " +
                    $"'{pedido.IdEstadoPedidoNavigation?.Nombre}' " +
                    $"a ese estado. " +
                    $"El siguiente estado permitido desde Pedido es " +
                    $"'{siguienteEstado.Value.Nombre}'.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // CAMBIAR ESTADO
            // =====================================================

            pedido.IdEstadoPedido =
                idEstado;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"Estado actualizado correctamente a " +
                $"'{siguienteEstado.Value.Nombre}'.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }

        // =========================================================
        // ANULAR / CANCELAR
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Anular(
            int id)
        {
            var pedido =
                await _context.Pedidos
                    .Include(p =>
                        p.IdEstadoPedidoNavigation)
                    .FirstOrDefaultAsync(
                        p =>
                            p.IdPedido == id);

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
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // NO CANCELAR FACTURADO
            // =====================================================

            var tieneFactura =
                await _context.Facturas
                    .AsNoTracking()
                    .AnyAsync(f =>
                        f.IdPedido ==
                        pedido.IdPedido);

            if (tieneFactura ||
                pedido.IdEstadoPedido ==
                ESTADO_FACTURADO)
            {
                TempData["Error"] =
                    "No se puede cancelar un pedido que ya fue facturado.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // BUSCAR ESTADO CANCELADO
            // =====================================================

            var estadoCancelado =
                await _context.EstadoPedidos
                    .FirstOrDefaultAsync(e =>
                        e.IdEstadoPedido ==
                        ESTADO_CANCELADO);

            if (estadoCancelado == null)
            {
                TempData["Error"] =
                    "No existe el estado Cancelado configurado con ID 7.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }

            // =====================================================
            // TRANSACCIÓN
            // =====================================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                pedido.IdEstadoPedido =
                    estadoCancelado.IdEstadoPedido;

                // =================================================
                // CONSERVAR TOTAL HISTÓRICO
                // =================================================

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Mensaje"] =
                    "Pedido cancelado correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Ocurrió un error al cancelar el pedido.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id
                    });
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

            await CargarProductosEnDetalles(
                model);
        }

        // =========================================================
        // CARGAR PRODUCTOS
        // =========================================================

        private async Task CargarProductosEnDetalles(
            PedidoViewModel model)
        {
            var productos =
                await _context.Productos
                    .AsNoTracking()
                    .Where(p =>
                        p.Estado
                        &&
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
                                p.PrecioVenta
                                    .ToString("N2")
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
            var estado =
                await _context.EstadoPedidos
                    .FirstOrDefaultAsync(e =>
                        e.IdEstadoPedido ==
                        ESTADO_PENDIENTE);

            if (estado != null)
                return estado.IdEstadoPedido;

            throw new InvalidOperationException(
                "No existe el estado Pendiente con ID 1.");
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

            int siguiente =
                1;

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

            return
                $"PED-{siguiente:000000}";
        }

        // =========================================================
        // SIGUIENTE ESTADO
        //
        // PedidoController:
        //
        // FACTURADO -> ENTREGADO
        //
        // Cocina:
        //
        // PENDIENTE -> EN COCINA
        // EN COCINA -> PREPARÁNDOSE
        // PREPARÁNDOSE -> LISTO
        //
        // Facturación:
        //
        // LISTO -> FACTURADO
        //
        // =========================================================

        private (int Id, string Nombre)?
            ObtenerSiguienteEstadoPedido(
                int estadoActual)
        {
            return estadoActual switch
            {
                // =================================================
                // SOLO UN PEDIDO FACTURADO/PAGADO PUEDE ENTREGARSE
                // =================================================

                ESTADO_FACTURADO =>
                    (
                        ESTADO_ENTREGADO,
                        "Entregado"
                    ),

                // =================================================
                // LISTO NO SE ENTREGA DIRECTAMENTE.
                //
                // Primero debe pasar por Facturación/Pago.
                // =================================================

                ESTADO_LISTO =>
                    null,

                // =================================================
                // YA ENTREGADO
                // =================================================

                ESTADO_ENTREGADO =>
                    null,

                // =================================================
                // CANCELADO
                // =================================================

                ESTADO_CANCELADO =>
                    null,

                // =================================================
                // LOS ESTADOS DE COCINA SON MANEJADOS POR
                // CocinaController.
                // =================================================

                _ =>
                    null
            };
        }

        // =========================================================
        // NORMALIZAR TEXTO
        // =========================================================

        private string NormalizarTexto(
            string? texto)
        {
            if (string.IsNullOrWhiteSpace(
                texto))
            {
                return "";
            }

            texto =
                texto
                    .Trim()
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

            return
                new string(caracteres)
                    .Normalize(
                        NormalizationForm.FormC);
        }

        // =========================================================
        // COMPROBAR ESTADO CANCELADO
        // =========================================================

        private bool EsEstadoCancelado(
            string? nombreEstado)
        {
            return EsEstado(
                nombreEstado,
                "Cancelado");
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

            return
                NormalizarTexto(
                    nombreEstado)
                ==
                NormalizarTexto(
                    estadoBuscado);
        }
    }
}