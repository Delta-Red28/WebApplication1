using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class InventarioController : Controller
    {
        private readonly InventarioService _inventarioService;
        private readonly RestauranteContext _context;

        public InventarioController(
            InventarioService inventarioService,
            RestauranteContext context)
        {
            _inventarioService = inventarioService;
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================

        public async Task<IActionResult> Index()
        {
            var inventario =
                await _inventarioService.ObtenerInventarioAsync();

            return View(inventario);
        }

        // ============================================================
        // DETALLE DE UN INSUMO
        // ============================================================

        public async Task<IActionResult> Detalle(
            int idInsumo,
            int? idUbicacion)
        {
            var query = _context.Existencia
                .AsNoTracking()
                .Include(x => x.IdInsumoNavigation)
                    .ThenInclude(x => x.IdUnidadMedidaNavigation)
                .Include(x => x.IdUbicacionNavigation)
                .Where(x => x.IdInsumo == idInsumo);

            // Si se especificó una ubicación,
            // mostramos solamente esa existencia.
            if (idUbicacion.HasValue)
            {
                query = query.Where(x =>
                    x.IdUbicacion == idUbicacion.Value);
            }

            var existencia = await query
                .FirstOrDefaultAsync();

            if (existencia == null)
                return NotFound();

            // Convertimos la entidad Existencium
            // al ViewModel utilizado por Inventario.
            var model = new InventarioViewModel
            {
                IdExistencia =
                    existencia.IdExistencia,

                IdInsumo =
                    existencia.IdInsumo,

                CodigoInsumo =
                    existencia.IdInsumoNavigation.CodigoInsumo,

                NombreInsumo =
                    existencia.IdInsumoNavigation.Nombre,

                UnidadMedida =
                    existencia.IdInsumoNavigation
                        .IdUnidadMedidaNavigation.Nombre,

                AbreviaturaUnidad =
                    existencia.IdInsumoNavigation
                        .IdUnidadMedidaNavigation.Abreviatura,

                IdUbicacion =
                    existencia.IdUbicacion,

                Ubicacion =
                    existencia.IdUbicacionNavigation.Nombre,

                StockActual =
                    existencia.StockActual,

                StockMinimo =
                    existencia.StockMinimo,

                StockMaximo =
                    existencia.StockMaximo,

                UltimaActualizacion =
                    existencia.UltimaActualizacion
            };

            return View(model);
        }

        // ============================================================
        // MOVIMIENTOS
        // ============================================================

        public async Task<IActionResult> Movimientos(
            int? idInsumo)
        {
            var movimientos =
                await _inventarioService
                    .ObtenerMovimientosAsync(idInsumo);

            ViewBag.IdInsumo = idInsumo;

            return View(movimientos);
        }

        // ============================================================
        // API SIMPLE PARA OBTENER STOCK
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Stock(
            int idInsumo,
            int idUbicacion)
        {
            var existencia =
                await _inventarioService
                    .ObtenerExistenciaAsync(
                        idInsumo,
                        idUbicacion);

            if (existencia == null)
            {
                return Json(new
                {
                    existe = false,
                    stock = 0
                });
            }

            return Json(new
            {
                existe = true,
                stock = existencia.StockActual,
                stockMinimo = existencia.StockMinimo,
                stockMaximo = existencia.StockMaximo
            });
        }
    }


}