using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class CategoriaInsumosController : Controller
    {
        private readonly RestauranteContext _context;

        public CategoriaInsumosController(RestauranteContext context)
        {
            _context = context;
        }


        // =========================================================
        // INDEX
        // =========================================================

        // GET: CategoriaInsumos
        public async Task<IActionResult> Index()
        {
            var categorias = await _context.CategoriaInsumos
                .AsNoTracking()
                .OrderByDescending(c => c.IdCategoriaInsumo)
                .Select(c => new CategoriaInsumoIndexViewModel
                {
                    IdCategoriaInsumo = c.IdCategoriaInsumo,

                    Nombre = c.Nombre,

                    Descripcion = c.Descripcion,

                    Estado = c.Estado,

                    FechaRegistro = c.FechaRegistro,

                    CantidadInsumos = c.Insumos.Count()
                })
                .ToListAsync();

            return View(categorias);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        // GET: CategoriaInsumos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.CategoriaInsumos
                .Include(c => c.Insumos)
                .FirstOrDefaultAsync(
                    c => c.IdCategoriaInsumo == id
                );

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        // GET: CategoriaInsumos/Create
        public IActionResult Create()
        {
            return View();
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        // POST: CategoriaInsumos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "IdCategoriaInsumo," +
                "Nombre," +
                "Descripcion," +
                "Estado"
            )]
            CategoriaInsumo categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            categoria.FechaRegistro = DateTime.Now;

            _context.CategoriaInsumos.Add(categoria);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La categoría de insumo fue creada correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        // GET: CategoriaInsumos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.CategoriaInsumos
                .FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        // POST: CategoriaInsumos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "IdCategoriaInsumo," +
                "Nombre," +
                "Descripcion," +
                "Estado," +
                "FechaRegistro"
            )]
            CategoriaInsumo categoria)
        {
            if (id != categoria.IdCategoriaInsumo)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            try
            {
                _context.CategoriaInsumos.Update(categoria);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "La categoría de insumo fue actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoriaInsumoExists(
                    categoria.IdCategoriaInsumo))
                {
                    return NotFound();
                }

                throw;
            }
        }


        // =========================================================
        // DELETE - GET
        // =========================================================

        // GET: CategoriaInsumos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.CategoriaInsumos
                .Include(c => c.Insumos)
                .FirstOrDefaultAsync(
                    c => c.IdCategoriaInsumo == id
                );

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        // POST: CategoriaInsumos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.CategoriaInsumos
                .Include(c => c.Insumos)
                .FirstOrDefaultAsync(
                    c => c.IdCategoriaInsumo == id
                );

            if (categoria == null)
            {
                return NotFound();
            }


            // =====================================================
            // NO ELIMINAR CATEGORÍAS CON INSUMOS
            // =====================================================

            if (categoria.Insumos.Any())
            {
                TempData["Error"] =
                    "No se puede eliminar esta categoría porque tiene " +
                    $"{categoria.Insumos.Count} insumo(s) asociado(s).";

                return RedirectToAction(nameof(Index));
            }


            _context.CategoriaInsumos.Remove(categoria);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La categoría de insumo fue eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EXISTE
        // =========================================================

        private bool CategoriaInsumoExists(int id)
        {
            return _context.CategoriaInsumos
                .Any(e =>
                    e.IdCategoriaInsumo == id
                );
        }
    }
}