using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class CocinaController : Controller
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

        public CocinaController(RestauranteContext context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX
        //
        // Filtros disponibles:
        //
        // activos
        // todos
        // pendientes
        // encocina
        // preparandose
        // listos
        // historial
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? filtro = "activos",
            string? buscar = null)
        {
            filtro = string.IsNullOrWhiteSpace(filtro)
                ? "activos"
                : filtro.Trim().ToLowerInvariant();

            buscar = string.IsNullOrWhiteSpace(buscar)
                ? null
                : buscar.Trim();

            // =====================================================
            // CONSULTA BASE
            // =====================================================

            var consulta = _context.Pedidos
                .AsNoTracking()

                .Include(p => p.IdClienteNavigation)

                .Include(p => p.IdMesaNavigation)

                .Include(p => p.IdEstadoPedidoNavigation)

                .Include(p => p.IdTipoPedidoNavigation)

                .Include(p => p.DetallePedidos)
                    .ThenInclude(d => d.IdProductoNavigation)

                .AsQueryable();

            // =====================================================
            // BÚSQUEDA
            // =====================================================

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(p =>
                    p.NumeroPedido.Contains(buscar));
            }

            // =====================================================
            // FILTROS
            // =====================================================

            switch (filtro)
            {
                case "pendientes":

                    consulta = consulta.Where(p =>
                        p.IdEstadoPedido == ESTADO_PENDIENTE);

                    break;

                case "encocina":

                    consulta = consulta.Where(p =>
                        p.IdEstadoPedido == ESTADO_EN_COCINA);

                    break;

                case "preparandose":

                    consulta = consulta.Where(p =>
                        p.IdEstadoPedido == ESTADO_PREPARANDOSE);

                    break;

                case "listos":

                    consulta = consulta.Where(p =>
                        p.IdEstadoPedido == ESTADO_LISTO);

                    break;

                case "historial":

                    consulta = consulta.Where(p =>
                        p.IdEstadoPedido == ESTADO_ENTREGADO ||
                        p.IdEstadoPedido == ESTADO_FACTURADO ||
                        p.IdEstadoPedido == ESTADO_CANCELADO);

                    break;

                case "todos":

                    // No aplicar filtro.

                    break;

                case "activos":
                default:

                    consulta = consulta.Where(p =>
                        p.IdEstadoPedido == ESTADO_PENDIENTE ||
                        p.IdEstadoPedido == ESTADO_EN_COCINA ||
                        p.IdEstadoPedido == ESTADO_PREPARANDOSE ||
                        p.IdEstadoPedido == ESTADO_LISTO);

                    break;
            }

            // =====================================================
            // ORDEN
            // =====================================================

            var pedidos = await consulta
                .OrderBy(p => p.IdEstadoPedido)
                .ThenBy(p => p.FechaPedido)
                .ToListAsync();

            // =====================================================
            // CONTADORES DE COCINA
            // =====================================================

            ViewBag.Pendientes = await _context.Pedidos
                .AsNoTracking()
                .CountAsync(p =>
                    p.IdEstadoPedido == ESTADO_PENDIENTE);

            ViewBag.EnCocina = await _context.Pedidos
                .AsNoTracking()
                .CountAsync(p =>
                    p.IdEstadoPedido == ESTADO_EN_COCINA);

            ViewBag.Preparandose = await _context.Pedidos
                .AsNoTracking()
                .CountAsync(p =>
                    p.IdEstadoPedido == ESTADO_PREPARANDOSE);

            ViewBag.Listos = await _context.Pedidos
                .AsNoTracking()
                .CountAsync(p =>
                    p.IdEstadoPedido == ESTADO_LISTO);

            // =====================================================
            // CONTADORES DEL HISTORIAL
            // =====================================================

            ViewBag.Entregados = await _context.Pedidos
                .AsNoTracking()
                .CountAsync(p =>
                    p.IdEstadoPedido == ESTADO_ENTREGADO);

            ViewBag.Facturados = await _context.Pedidos
                .AsNoTracking()
                .CountAsync(p =>
                    p.IdEstadoPedido == ESTADO_FACTURADO);

            ViewBag.Cancelados = await _context.Pedidos
                .AsNoTracking()
                .CountAsync(p =>
                    p.IdEstadoPedido == ESTADO_CANCELADO);

            // =====================================================
            // TOTAL
            // =====================================================

            ViewBag.TotalPedidos = await _context.Pedidos
                .AsNoTracking()
                .CountAsync();

            // =====================================================
            // INFORMACIÓN PARA LA VISTA
            // =====================================================

            ViewBag.Filtro = filtro;
            ViewBag.Buscar = buscar;

            return View(pedidos);
        }

        // =========================================================
        // DETALLES
        //
        // Cocina puede consultar cualquier pedido.
        // Incluso pedidos históricos.
        //
        // Pero solamente puede cambiar estados de:
        //
        // Pendiente
        // En cocina
        // Preparándose
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

                .Include(p => p.IdEstadoPedidoNavigation)

                .Include(p => p.IdTipoPedidoNavigation)

                .Include(p => p.IdUsuarioNavigation)

                .Include(p => p.DetallePedidos)
                    .ThenInclude(d => d.IdProductoNavigation)

                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            ViewBag.PuedeCambiarEstado =
                PuedeCambiarEstadoCocina(
                    pedido.IdEstadoPedido);

            ViewBag.SiguienteEstado =
                ObtenerSiguienteEstadoCocina(
                    pedido.IdEstadoPedido);

            return View(pedido);
        }

        // =========================================================
        // ENVIAR A COCINA
        //
        // Pendiente -> En cocina
        //
        // Esta acción puede ser utilizada si el módulo de Pedidos
        // necesita enviar explícitamente el pedido a Cocina.
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarACocina(int id)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            if (pedido.IdEstadoPedido != ESTADO_PENDIENTE)
            {
                TempData["Error"] =
                    "El pedido no se encuentra en estado Pendiente.";

                return RedirectToAction(nameof(Index));
            }

            pedido.IdEstadoPedido = ESTADO_EN_COCINA;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"Pedido {pedido.NumeroPedido} enviado a cocina.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // COMENZAR PREPARACIÓN
        //
        // En cocina -> Preparándose
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ComenzarPreparacion(int id)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            if (pedido.IdEstadoPedido != ESTADO_EN_COCINA)
            {
                TempData["Error"] =
                    "El pedido debe estar En cocina para comenzar la preparación.";

                return RedirectToAction(nameof(Index));
            }

            pedido.IdEstadoPedido = ESTADO_PREPARANDOSE;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"Pedido {pedido.NumeroPedido} está en preparación.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // MARCAR COMO LISTO
        //
        // Preparándose -> Listo
        //
        // AQUÍ TERMINA LA RESPONSABILIDAD DE COCINA.
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarListo(int id)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            if (pedido.IdEstadoPedido != ESTADO_PREPARANDOSE)
            {
                TempData["Error"] =
                    "El pedido debe estar Preparándose para marcarlo como Listo.";

                return RedirectToAction(nameof(Index));
            }

            pedido.IdEstadoPedido = ESTADO_LISTO;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"Pedido {pedido.NumeroPedido} está Listo.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // CAMBIAR ESTADO
        //
        // Método general para la vista.
        //
        // ÚNICAMENTE:
        //
        // 1 -> 2
        // 2 -> 3
        // 3 -> 4
        //
        // Cocina jamás podrá:
        //
        // 4 -> 5
        // 5 -> 6
        // cualquier -> 7
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p =>
                    p.IdPedido == id);

            if (pedido == null)
                return NotFound();

            var siguienteEstado =
                ObtenerSiguienteEstadoCocina(
                    pedido.IdEstadoPedido);

            if (siguienteEstado == null)
            {
                TempData["Error"] =
                    ObtenerMensajeEstadoNoModificable(
                        pedido.IdEstadoPedido);

                return RedirectToAction(nameof(Index));
            }

            pedido.IdEstadoPedido =
                siguienteEstado.Value.Id;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"Pedido {pedido.NumeroPedido} actualizado a " +
                $"'{siguienteEstado.Value.Nombre}'.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // VALIDAR SI COCINA PUEDE MODIFICAR
        // =========================================================

        private bool PuedeCambiarEstadoCocina(
            int estadoActual)
        {
            return estadoActual == ESTADO_PENDIENTE ||
                   estadoActual == ESTADO_EN_COCINA ||
                   estadoActual == ESTADO_PREPARANDOSE;
        }

        // =========================================================
        // OBTENER SIGUIENTE ESTADO
        // =========================================================

        private (int Id, string Nombre)?
            ObtenerSiguienteEstadoCocina(
                int estadoActual)
        {
            return estadoActual switch
            {
                ESTADO_PENDIENTE =>
                    (
                        ESTADO_EN_COCINA,
                        "En cocina"
                    ),

                ESTADO_EN_COCINA =>
                    (
                        ESTADO_PREPARANDOSE,
                        "Preparándose"
                    ),

                ESTADO_PREPARANDOSE =>
                    (
                        ESTADO_LISTO,
                        "Listo"
                    ),

                ESTADO_LISTO =>
                    null,

                ESTADO_ENTREGADO =>
                    null,

                ESTADO_FACTURADO =>
                    null,

                ESTADO_CANCELADO =>
                    null,

                _ =>
                    null
            };
        }

        // =========================================================
        // MENSAJES PARA PEDIDOS NO MODIFICABLES
        // =========================================================

        private string ObtenerMensajeEstadoNoModificable(
            int estadoActual)
        {
            return estadoActual switch
            {
                ESTADO_LISTO =>
                    "El pedido ya está Listo. La entrega corresponde al área de Pedidos/Mesero.",

                ESTADO_ENTREGADO =>
                    "El pedido ya fue Entregado. No puede modificarse desde Cocina.",

                ESTADO_FACTURADO =>
                    "El pedido ya fue Facturado. No puede modificarse desde Cocina.",

                ESTADO_CANCELADO =>
                    "El pedido está Cancelado. No puede modificarse desde Cocina.",

                _ =>
                    "Este pedido no tiene una transición disponible desde Cocina."
            };
        }
    }


}