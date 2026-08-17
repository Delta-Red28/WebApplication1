using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly RestauranteContext _context;

        public CategoriasController(RestauranteContext context)
        {
            _context = context;
        }


        // ============================================================
        // GET: Categorias
        // ============================================================

        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categoria
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(categorias);
        }


        // ============================================================
        // GET: Categorias/Details/5
        // ============================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.IdCategoria == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }


        // ============================================================
        // GET: Categorias/Create
        // ============================================================

        public IActionResult Create()
        {
            return View();
        }


        // ============================================================
        // POST: Categorias/Create
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Descripcion")] Categorium categoria)
        {
            if (ModelState.IsValid)
            {
                categoria.FechaRegistro = DateTime.Now;
                categoria.Estado = true;

                _context.Categoria.Add(categoria);

                await _context.SaveChangesAsync();

                TempData["Mensaje"] =
                    "La categoría fue creada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }


        // ============================================================
        // GET: Categorias/Edit/5
        // ============================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categoria
                .FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }


        // ============================================================
        // POST: Categorias/Edit/5
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdCategoria,Nombre,Descripcion")] Categorium categoria)
        {
            if (id != categoria.IdCategoria)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var categoriaExistente = await _context.Categoria
                .FindAsync(id);

            if (categoriaExistente == null)
            {
                return NotFound();
            }

            categoriaExistente.Nombre = categoria.Nombre;
            categoriaExistente.Descripcion = categoria.Descripcion;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La categoría fue actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // POST: Categorias/Desactivar/5
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var categoria = await _context.Categoria
                .FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            if (!categoria.Estado)
            {
                TempData["Error"] =
                    "La categoría ya se encuentra inactiva.";

                return RedirectToAction(nameof(Index));
            }

            categoria.Estado = false;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La categoría fue desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // POST: Categorias/Activar/5
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id)
        {
            var categoria = await _context.Categoria
                .FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            if (categoria.Estado)
            {
                TempData["Error"] =
                    "La categoría ya se encuentra activa.";

                return RedirectToAction(nameof(Index));
            }

            categoria.Estado = true;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La categoría fue activada correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // MÉTODO AUXILIAR
        // ============================================================

        private bool CategoriaExists(int id)
        {
            return _context.Categoria
                .Any(e => e.IdCategoria == id);
        }
    }
}