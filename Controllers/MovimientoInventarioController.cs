using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class MovimientosInventarioController : Controller
    {
        private readonly RestauranteContext _context;
        private readonly InventarioService _inventarioService;

        public MovimientosInventarioController(
            RestauranteContext context,
            InventarioService inventarioService)
        {
            _context = context;
            _inventarioService = inventarioService;
        }

        // ============================================================
        // CREAR
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarCombos();

            return View(new EntradaInventarioViewModel());
        }

        // ============================================================
        // ENTRADA
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrada(
            EntradaInventarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombos();
                return View("Crear", model);
            }

            // --------------------------------------------------------
            // CAMBIAR ESTO POR EL USUARIO AUTENTICADO
            // --------------------------------------------------------

            var idUsuario = ObtenerIdUsuarioActual();

            var resultado =
                await _inventarioService
                    .RegistrarEntradaAsync(
                        model,
                        idUsuario);

            if (!resultado.Exito)
            {
                ModelState.AddModelError(
                    "",
                    resultado.Mensaje);

                await CargarCombos();

                return View("Crear", model);
            }

            TempData["Success"] =
                resultado.Mensaje;

            return RedirectToAction(
                "Index",
                "Inventario");
        }

        // ============================================================
        // SALIDA
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salida(
            int idInsumo,
            int idUbicacion,
            decimal cantidad,
            string? referencia,
            string? descripcion)
        {
            if (cantidad <= 0)
            {
                TempData["Error"] =
                    "La cantidad debe ser mayor que cero.";

                return RedirectToAction(
                    "Crear");
            }

            var idUsuario =
                ObtenerIdUsuarioActual();

            var resultado =
                await _inventarioService
                    .RegistrarSalidaAsync(
                        idInsumo,
                        idUbicacion,
                        cantidad,
                        idUsuario,
                        referencia,
                        descripcion);

            if (!resultado.Exito)
            {
                TempData["Error"] =
                    resultado.Mensaje;

                return RedirectToAction(
                    "Crear");
            }

            TempData["Success"] =
                resultado.Mensaje;

            return RedirectToAction(
                "Index",
                "Inventario");
        }

        // ============================================================
        // COMBOS
        // ============================================================

        private async Task CargarCombos()
        {
            var insumos = await _context.Insumos
                .AsNoTracking()
                .Where(x => x.Estado)
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            var ubicaciones =
                await _context.UbicacionInventarios
                    .AsNoTracking()
                    .Where(x => x.Estado)
                    .OrderBy(x => x.Nombre)
                    .ToListAsync();

            ViewBag.Insumos =
                new SelectList(
                    insumos,
                    "IdInsumo",
                    "Nombre");

            ViewBag.Ubicaciones =
                new SelectList(
                    ubicaciones,
                    "IdUbicacion",
                    "Nombre");
        }

        // ============================================================
        // USUARIO ACTUAL
        // ============================================================

        private int ObtenerIdUsuarioActual()
        {
            var claim =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier);

            if (claim != null &&
                int.TryParse(claim.Value, out int idUsuario))
            {
                return idUsuario;
            }

            // SOLO para desarrollo.
            // Reemplazar por el usuario autenticado.
            return 1;
        }
    }
}