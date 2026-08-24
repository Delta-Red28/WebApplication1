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
            var insumo = await _context.Insumos
                .AsNoTracking()
                .Include(x => x.IdCategoriaInsumoNavigation)
                .Include(x => x.IdMarcaNavigation)
                .Include(x => x.IdEstadoInsumoNavigation)
                .Include(x => x.IdUnidadMedidaNavigation)
                .FirstOrDefaultAsync(x =>
                    x.IdInsumo == idInsumo);

            if (insumo == null)
                return NotFound();

            var existencias = await _context.Existencia
                .AsNoTracking()
                .Include(x => x.IdUbicacionNavigation)
                .Where(x => x.IdInsumo == idInsumo)
                .OrderBy(x => x.IdUbicacionNavigation.Nombre)
                .ToListAsync();

            if (idUbicacion.HasValue)
            {
                existencias = existencias
                    .Where(x =>
                        x.IdUbicacion ==
                        idUbicacion.Value)
                    .ToList();
            }

            ViewBag.Insumo = insumo;

            return View(existencias);
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