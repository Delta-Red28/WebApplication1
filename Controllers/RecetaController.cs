using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RecetasController : Controller
    {
        private readonly RestauranteContext _context;

        public RecetasController(RestauranteContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: Recetas
        // Lista de recetas
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var recetas = await _context.Receta
                .Include(r => r.IdProductoNavigation)
                .Include(r => r.IdEstadoRecetaNavigation)
                .OrderByDescending(r => r.IdReceta)
                .ToListAsync();

            return View(recetas);
        }

        // ============================================================
        // GET: Recetas/Details/5
        // Detalle de una receta
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receta = await _context.Receta
                .Include(r => r.IdProductoNavigation)
                .Include(r => r.IdEstadoRecetaNavigation)
                .Include(r => r.DetalleReceta)
                    .ThenInclude(d => d.IdInsumoNavigation)
                .Include(r => r.DetalleReceta)
                    .ThenInclude(d => d.IdUnidadMedidaNavigation)
                .FirstOrDefaultAsync(r => r.IdReceta == id);

            if (receta == null)
            {
                return NotFound();
            }

            return View(receta);
        }

        // ============================================================
        // GET: Recetas/Create
        // ============================================================
        public async Task<IActionResult> Create()
        {
            await CargarCombos();

            return View();
        }

        // ============================================================
        // POST: Recetas/Create
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("IdReceta,IdProducto,IdEstadoReceta,Nombre,Version,Rendimiento,Observacion,FechaRegistro")]
            Recetum receta)
        {
            // El IdReceta es autogenerado por SQL Server
            ModelState.Remove(nameof(Recetum.IdReceta));

            // FechaRegistro la coloca la base de datos
            ModelState.Remove(nameof(Recetum.FechaRegistro));

            // Validar producto
            if (receta.IdProducto <= 0)
            {
                ModelState.AddModelError(
                    nameof(Recetum.IdProducto),
                    "Debe seleccionar un producto.");
            }

            // Validar estado
            if (receta.IdEstadoReceta <= 0)
            {
                ModelState.AddModelError(
                    nameof(Recetum.IdEstadoReceta),
                    "Debe seleccionar un estado de receta.");
            }

            // Validar nombre
            if (string.IsNullOrWhiteSpace(receta.Nombre))
            {
                ModelState.AddModelError(
                    nameof(Recetum.Nombre),
                    "El nombre de la receta es obligatorio.");
            }

            // Validar rendimiento
            if (receta.Rendimiento <= 0)
            {
                ModelState.AddModelError(
                    nameof(Recetum.Rendimiento),
                    "El rendimiento debe ser mayor que cero.");
            }

            // Validar versión
            if (receta.Version <= 0)
            {
                ModelState.AddModelError(
                    nameof(Recetum.Version),
                    "La versión debe ser mayor que cero.");
            }

            // Verificar que el producto exista
            if (receta.IdProducto > 0)
            {
                bool productoExiste = await _context.Productos
                    .AnyAsync(p => p.IdProducto == receta.IdProducto);

                if (!productoExiste)
                {
                    ModelState.AddModelError(
                        nameof(Recetum.IdProducto),
                        "El producto seleccionado no existe.");
                }
            }

            // Verificar que el estado exista
            if (receta.IdEstadoReceta > 0)
            {
                bool estadoExiste = await _context.EstadoReceta
                    .AnyAsync(e =>
                        e.IdEstadoReceta == receta.IdEstadoReceta);

                if (!estadoExiste)
                {
                    ModelState.AddModelError(
                        nameof(Recetum.IdEstadoReceta),
                        "El estado de receta seleccionado no existe.");
                }
            }

            // Un producto solo puede tener una receta
            if (receta.IdProducto > 0)
            {
                bool yaTieneReceta = await _context.Receta
                    .AnyAsync(r => r.IdProducto == receta.IdProducto);

                if (yaTieneReceta)
                {
                    ModelState.AddModelError(
                        nameof(Recetum.IdProducto),
                        "El producto seleccionado ya tiene una receta.");
                }
            }

            if (ModelState.IsValid)
            {
                receta.Nombre = receta.Nombre.Trim();

                if (string.IsNullOrWhiteSpace(receta.Observacion))
                {
                    receta.Observacion = null;
                }
                else
                {
                    receta.Observacion =
                        receta.Observacion.Trim();
                }

                // Fecha por defecto
                if (receta.FechaRegistro == default)
                {
                    receta.FechaRegistro = DateTime.Now;
                }

                _context.Receta.Add(receta);

                await _context.SaveChangesAsync();

                TempData["Mensaje"] =
                    "La receta fue creada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            await CargarCombos(
                receta.IdProducto,
                receta.IdEstadoReceta);

            return View(receta);
        }

        // ============================================================
        // GET: Recetas/Edit/5
        // ============================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receta = await _context.Receta
                .FindAsync(id);

            if (receta == null)
            {
                return NotFound();
            }

            await CargarCombos(
                receta.IdProducto,
                receta.IdEstadoReceta);

            return View(receta);
        }

        // ============================================================
        // POST: Recetas/Edit/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdReceta,IdProducto,IdEstadoReceta,Nombre,Version,Rendimiento,Observacion,FechaRegistro")]
            Recetum receta)
        {
            if (id != receta.IdReceta)
            {
                return NotFound();
            }

            // Validaciones
            if (receta.IdProducto <= 0)
            {
                ModelState.AddModelError(
                    nameof(Recetum.IdProducto),
                    "Debe seleccionar un producto.");
            }

            if (receta.IdEstadoReceta <= 0)
            {
                ModelState.AddModelError(
                    nameof(Recetum.IdEstadoReceta),
                    "Debe seleccionar un estado de receta.");
            }

            if (string.IsNullOrWhiteSpace(receta.Nombre))
            {
                ModelState.AddModelError(
                    nameof(Recetum.Nombre),
                    "El nombre de la receta es obligatorio.");
            }

            if (receta.Rendimiento <= 0)
            {
                ModelState.AddModelError(
                    nameof(Recetum.Rendimiento),
                    "El rendimiento debe ser mayor que cero.");
            }

            if (receta.Version <= 0)
            {
                ModelState.AddModelError(
                    nameof(Recetum.Version),
                    "La versión debe ser mayor que cero.");
            }

            // Verificar producto
            if (receta.IdProducto > 0)
            {
                bool productoExiste = await _context.Productos
                    .AnyAsync(p => p.IdProducto == receta.IdProducto);

                if (!productoExiste)
                {
                    ModelState.AddModelError(
                        nameof(Recetum.IdProducto),
                        "El producto seleccionado no existe.");
                }
            }

            // Verificar estado
            if (receta.IdEstadoReceta > 0)
            {
                bool estadoExiste = await _context.EstadoReceta
                    .AnyAsync(e =>
                        e.IdEstadoReceta == receta.IdEstadoReceta);

                if (!estadoExiste)
                {
                    ModelState.AddModelError(
                        nameof(Recetum.IdEstadoReceta),
                        "El estado de receta seleccionado no existe.");
                }
            }

            // Verificar que otro producto no tenga receta
            if (receta.IdProducto > 0)
            {
                bool productoTieneOtraReceta =
                    await _context.Receta
                        .AnyAsync(r =>
                            r.IdProducto == receta.IdProducto &&
                            r.IdReceta != receta.IdReceta);

                if (productoTieneOtraReceta)
                {
                    ModelState.AddModelError(
                        nameof(Recetum.IdProducto),
                        "El producto seleccionado ya tiene otra receta.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    receta.Nombre = receta.Nombre.Trim();

                    if (string.IsNullOrWhiteSpace(receta.Observacion))
                    {
                        receta.Observacion = null;
                    }
                    else
                    {
                        receta.Observacion =
                            receta.Observacion.Trim();
                    }

                    _context.Update(receta);

                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] =
                        "La receta fue actualizada correctamente.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecetaExists(receta.IdReceta))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            await CargarCombos(
                receta.IdProducto,
                receta.IdEstadoReceta);

            return View(receta);
        }

        // ============================================================
        // GET: Recetas/Delete/5
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receta = await _context.Receta
                .Include(r => r.IdProductoNavigation)
                .Include(r => r.IdEstadoRecetaNavigation)
                .FirstOrDefaultAsync(r =>
                    r.IdReceta == id);

            if (receta == null)
            {
                return NotFound();
            }

            return View(receta);
        }

        // ============================================================
        // POST: Recetas/Delete/5
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var receta = await _context.Receta
                .Include(r => r.DetalleReceta)
                .FirstOrDefaultAsync(r =>
                    r.IdReceta == id);

            if (receta == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Verificar si tiene detalles
            if (receta.DetalleReceta != null &&
                receta.DetalleReceta.Any())
            {
                TempData["Error"] =
                    "No se puede eliminar la receta porque tiene ingredientes asociados.";

                return RedirectToAction(nameof(Index));
            }

            _context.Receta.Remove(receta);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La receta fue eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: Recetas/Ingredientes/5
        // Muestra los ingredientes de una receta
        // ============================================================
        public async Task<IActionResult> Ingredientes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receta = await _context.Receta
                .Include(r => r.IdProductoNavigation)
                .FirstOrDefaultAsync(r =>
                    r.IdReceta == id);

            if (receta == null)
            {
                return NotFound();
            }

            var detalles = await _context.DetalleReceta
                .Include(d => d.IdInsumoNavigation)
                .Include(d => d.IdUnidadMedidaNavigation)
                .Where(d => d.IdReceta == id)
                .OrderBy(d => d.IdDetalleReceta)
                .ToListAsync();

            ViewBag.Receta = receta;

            return View(detalles);
        }

        // ============================================================
        // GET: Recetas/AgregarIngrediente/5
        // ============================================================
        public async Task<IActionResult> AgregarIngrediente(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receta = await _context.Receta
                .Include(r => r.IdProductoNavigation)
                .FirstOrDefaultAsync(r =>
                    r.IdReceta == id);

            if (receta == null)
            {
                return NotFound();
            }

            var detalle = new DetalleRecetum
            {
                IdReceta = receta.IdReceta
            };

            ViewBag.Receta = receta;

            await CargarIngredientes();
            await CargarUnidades();

            return View(detalle);
        }

        // ============================================================
        // POST: Recetas/AgregarIngrediente
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarIngrediente(
            [Bind("IdReceta,IdInsumo,Cantidad,IdUnidadMedida,Observacion")]
            DetalleRecetum detalle)
        {
            // Validar receta
            var receta = await _context.Receta
                .Include(r => r.IdProductoNavigation)
                .FirstOrDefaultAsync(r =>
                    r.IdReceta == detalle.IdReceta);

            if (receta == null)
            {
                return NotFound();
            }

            if (detalle.IdInsumo <= 0)
            {
                ModelState.AddModelError(
                    nameof(DetalleRecetum.IdInsumo),
                    "Debe seleccionar un insumo.");
            }

            if (detalle.Cantidad <= 0)
            {
                ModelState.AddModelError(
                    nameof(DetalleRecetum.Cantidad),
                    "La cantidad debe ser mayor que cero.");
            }

            if (detalle.IdUnidadMedida <= 0)
            {
                ModelState.AddModelError(
                    nameof(DetalleRecetum.IdUnidadMedida),
                    "Debe seleccionar una unidad de medida.");
            }

            // Verificar insumo
            if (detalle.IdInsumo > 0)
            {
                bool insumoExiste = await _context.Insumos
                    .AnyAsync(i =>
                        i.IdInsumo == detalle.IdInsumo);

                if (!insumoExiste)
                {
                    ModelState.AddModelError(
                        nameof(DetalleRecetum.IdInsumo),
                        "El insumo seleccionado no existe.");
                }
            }

            // Verificar unidad
            if (detalle.IdUnidadMedida > 0)
            {
                bool unidadExiste = await _context.UnidadMedida
                    .AnyAsync(u =>
                        u.IdUnidadMedida ==
                        detalle.IdUnidadMedida);

                if (!unidadExiste)
                {
                    ModelState.AddModelError(
                        nameof(DetalleRecetum.IdUnidadMedida),
                        "La unidad de medida seleccionada no existe.");
                }
            }

            // Un mismo insumo no puede repetirse en la receta
            if (detalle.IdInsumo > 0)
            {
                bool yaExiste = await _context.DetalleReceta
                    .AnyAsync(d =>
                        d.IdReceta == detalle.IdReceta &&
                        d.IdInsumo == detalle.IdInsumo);

                if (yaExiste)
                {
                    ModelState.AddModelError(
                        nameof(DetalleRecetum.IdInsumo),
                        "Este insumo ya está agregado a la receta.");
                }
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(detalle.Observacion))
                {
                    detalle.Observacion = null;
                }
                else
                {
                    detalle.Observacion =
                        detalle.Observacion.Trim();
                }

                _context.DetalleReceta.Add(detalle);

                await _context.SaveChangesAsync();

                TempData["Mensaje"] =
                    "Ingrediente agregado correctamente.";

                return RedirectToAction(
                    nameof(Ingredientes),
                    new { id = detalle.IdReceta });
            }

            ViewBag.Receta = receta;

            await CargarIngredientes(detalle.IdInsumo);
            await CargarUnidades(detalle.IdUnidadMedida);

            return View(detalle);
        }

        // ============================================================
        // GET: Recetas/EditarIngrediente/5
        // ============================================================
        public async Task<IActionResult> EditarIngrediente(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalle = await _context.DetalleReceta
                .Include(d => d.IdRecetaNavigation)
                    .ThenInclude(r => r.IdProductoNavigation)
                .FirstOrDefaultAsync(d =>
                    d.IdDetalleReceta == id);

            if (detalle == null)
            {
                return NotFound();
            }

            ViewBag.Receta = detalle.IdRecetaNavigation;

            await CargarIngredientes(detalle.IdInsumo);
            await CargarUnidades(detalle.IdUnidadMedida);

            return View(detalle);
        }

        // ============================================================
        // POST: Recetas/EditarIngrediente/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarIngrediente(
            int id,
            [Bind("IdDetalleReceta,IdReceta,IdInsumo,Cantidad,IdUnidadMedida,Observacion")]
            DetalleRecetum detalle)
        {
            if (id != detalle.IdDetalleReceta)
            {
                return NotFound();
            }

            var receta = await _context.Receta
                .Include(r => r.IdProductoNavigation)
                .FirstOrDefaultAsync(r =>
                    r.IdReceta == detalle.IdReceta);

            if (receta == null)
            {
                return NotFound();
            }

            if (detalle.IdInsumo <= 0)
            {
                ModelState.AddModelError(
                    nameof(DetalleRecetum.IdInsumo),
                    "Debe seleccionar un insumo.");
            }

            if (detalle.Cantidad <= 0)
            {
                ModelState.AddModelError(
                    nameof(DetalleRecetum.Cantidad),
                    "La cantidad debe ser mayor que cero.");
            }

            if (detalle.IdUnidadMedida <= 0)
            {
                ModelState.AddModelError(
                    nameof(DetalleRecetum.IdUnidadMedida),
                    "Debe seleccionar una unidad de medida.");
            }

            // Evitar insumos duplicados
            if (detalle.IdInsumo > 0)
            {
                bool duplicado = await _context.DetalleReceta
                    .AnyAsync(d =>
                        d.IdReceta == detalle.IdReceta &&
                        d.IdInsumo == detalle.IdInsumo &&
                        d.IdDetalleReceta !=
                            detalle.IdDetalleReceta);

                if (duplicado)
                {
                    ModelState.AddModelError(
                        nameof(DetalleRecetum.IdInsumo),
                        "Este insumo ya existe en la receta.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(
                        detalle.Observacion))
                    {
                        detalle.Observacion = null;
                    }
                    else
                    {
                        detalle.Observacion =
                            detalle.Observacion.Trim();
                    }

                    _context.DetalleReceta.Update(detalle);

                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] =
                        "Ingrediente actualizado correctamente.";

                    return RedirectToAction(
                        nameof(Ingredientes),
                        new { id = detalle.IdReceta });
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool existe = await _context.DetalleReceta
                        .AnyAsync(d =>
                            d.IdDetalleReceta ==
                            detalle.IdDetalleReceta);

                    if (!existe)
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            ViewBag.Receta = receta;

            await CargarIngredientes(detalle.IdInsumo);
            await CargarUnidades(detalle.IdUnidadMedida);

            return View(detalle);
        }

        // ============================================================
        // POST: Recetas/EliminarIngrediente/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarIngrediente(
            int id)
        {
            var detalle = await _context.DetalleReceta
                .FindAsync(id);

            if (detalle == null)
            {
                return NotFound();
            }

            int idReceta = detalle.IdReceta;

            _context.DetalleReceta.Remove(detalle);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Ingrediente eliminado correctamente.";

            return RedirectToAction(
                nameof(Ingredientes),
                new { id = idReceta });
        }

        // ============================================================
        // MÉTODOS AUXILIARES
        // ============================================================

        private async Task CargarCombos(
            int? idProductoSeleccionado = null,
            int? idEstadoSeleccionado = null)
        {
            var productos = await _context.Productos
                .Where(p => p.Estado)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            var estados = await _context.EstadoReceta
                .Where(e => e.Estado)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            ViewBag.IdProducto = new SelectList(
                productos,
                "IdProducto",
                "Nombre",
                idProductoSeleccionado);

            ViewBag.IdEstadoReceta = new SelectList(
                estados,
                "IdEstadoReceta",
                "Nombre",
                idEstadoSeleccionado);
        }

        private async Task CargarIngredientes(
            int? idInsumoSeleccionado = null)
        {
            var insumos = await _context.Insumos
                .Where(i => i.Estado)
                .OrderBy(i => i.Nombre)
                .ToListAsync();

            ViewBag.IdInsumo = new SelectList(
                insumos,
                "IdInsumo",
                "Nombre",
                idInsumoSeleccionado);
        }

        private async Task CargarUnidades(
            int? idUnidadSeleccionada = null)
        {
            var unidades = await _context.UnidadMedida
                .Where(u => u.Estado)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            ViewBag.IdUnidadMedida = new SelectList(
                unidades,
                "IdUnidadMedida",
                "Nombre",
                idUnidadSeleccionada);
        }

        private bool RecetaExists(int id)
        {
            return _context.Receta
                .Any(r => r.IdReceta == id);
        }
    }
}