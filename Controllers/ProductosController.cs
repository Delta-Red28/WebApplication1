using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ProductosController : Controller
    {
        private readonly RestauranteContext _context;

        public ProductosController(RestauranteContext context)
        {
            _context = context;
        }


        // ============================================================
        // GET: Productos
        // ============================================================

        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdEstadoProductoNavigation)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(productos);
        }


        // ============================================================
        // GET: Productos/Details/5
        // ============================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            // --------------------------------------------------------
            // Buscar producto
            // --------------------------------------------------------

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdEstadoProductoNavigation)
                .Include(p => p.Recetum!)
                    .ThenInclude(r => r.IdEstadoRecetaNavigation)
                .Include(p => p.Recetum!)
                    .ThenInclude(r => r.DetalleReceta)
                        .ThenInclude(d => d.IdInsumoNavigation)
                .Include(p => p.Recetum!)
                    .ThenInclude(r => r.DetalleReceta)
                        .ThenInclude(d => d.IdUnidadMedidaNavigation)
                .FirstOrDefaultAsync(p => p.IdProducto == id);


            if (producto == null)
            {
                return NotFound();
            }


            // ========================================================
            // CARGAR EXISTENCIAS DE LOS INGREDIENTES
            // ========================================================

            if (producto.Recetum != null &&
                producto.Recetum.DetalleReceta.Any())
            {
                var idsInsumos = producto.Recetum.DetalleReceta
                    .Select(d => d.IdInsumo)
                    .Distinct()
                    .ToList();


                var existencias = await _context.Existencia
                    .Include(e => e.IdUbicacionNavigation)
                    .Where(e => idsInsumos.Contains(e.IdInsumo))
                    .ToListAsync();


                // ----------------------------------------------------
                // Guardamos las existencias en ViewBag
                //
                // Se utiliza un diccionario:
                //
                // IdInsumo -> lista de existencias
                // ----------------------------------------------------

                ViewBag.Existencias = existencias
                    .GroupBy(e => e.IdInsumo)
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToList());
            }
            else
            {
                ViewBag.Existencias =
                    new Dictionary<int, List<Existencium>>();
            }


            // ========================================================
            // CALCULAR CUÁNTOS PLATILLOS SE PUEDEN PREPARAR
            // ========================================================

            decimal? unidadesPreparables = null;


            if (producto.Recetum != null &&
                producto.Recetum.DetalleReceta.Any())
            {
                var cantidadesDisponibles =
                    new List<decimal>();


                foreach (var detalle in producto.Recetum.DetalleReceta)
                {
                    var existencias =
                        ((Dictionary<int, List<Existencium>>)ViewBag.Existencias)
                        .TryGetValue(
                            detalle.IdInsumo,
                            out var listaExistencias)
                            ? listaExistencias
                            : new List<Existencium>();


                    // ------------------------------------------------
                    // Sumar existencia de todas las ubicaciones
                    // ------------------------------------------------

                    var stockTotal = existencias
                        .Sum(e => e.StockActual);


                    // ------------------------------------------------
                    // Evitar división entre cero
                    // ------------------------------------------------

                    if (detalle.Cantidad > 0)
                    {
                        var posibles =
                            stockTotal / detalle.Cantidad;

                        cantidadesDisponibles.Add(posibles);
                    }
                }


                if (cantidadesDisponibles.Any())
                {
                    unidadesPreparables =
                        Math.Floor(cantidadesDisponibles.Min());
                }
            }


            ViewBag.UnidadesPreparables =
                unidadesPreparables;


            return View(producto);
        }


        // ============================================================
        // GET: Productos/Create
        // ============================================================

        public async Task<IActionResult> Create(int? idCategoria)
        {
            // --------------------------------------------------------
            // Si se está creando desde una categoría,
            // comprobar que exista.
            // --------------------------------------------------------

            if (idCategoria.HasValue)
            {
                var categoriaExiste = await _context.Categoria
                    .AnyAsync(c =>
                        c.IdCategoria == idCategoria.Value);

                if (!categoriaExiste)
                {
                    return NotFound();
                }
            }


            // --------------------------------------------------------
            // Crear producto
            // --------------------------------------------------------

            var producto = new Producto
            {
                Estado = true,
                FechaRegistro = DateTime.Now,
                ControlaInventario = false
            };


            // --------------------------------------------------------
            // Mantener categoría
            // --------------------------------------------------------

            if (idCategoria.HasValue)
            {
                producto.IdCategoria = idCategoria.Value;
            }


            // --------------------------------------------------------
            // Cargar listas
            // --------------------------------------------------------

            await CargarListas(
                producto.IdCategoria,
                null);


            return View(producto);
        }


        // ============================================================
        // POST: Productos/Create
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("IdCategoria,IdEstadoProducto,Nombre,Descripcion,PrecioVenta,Costo,Imagen,TiempoPreparacion,CodigoProducto,ControlaInventario")]
            Producto producto)
        {
            // ========================================================
            // VALIDAR CATEGORÍA
            // ========================================================

            var categoriaExiste = await _context.Categoria
                .AnyAsync(c =>
                    c.IdCategoria == producto.IdCategoria);

            if (!categoriaExiste)
            {
                ModelState.AddModelError(
                    "IdCategoria",
                    "Debes seleccionar una categoría válida.");
            }


            // ========================================================
            // VALIDAR ESTADO
            // ========================================================

            var estadoExiste = await _context.EstadoProductos
                .AnyAsync(e =>
                    e.IdEstadoProducto ==
                    producto.IdEstadoProducto);

            if (!estadoExiste)
            {
                ModelState.AddModelError(
                    "IdEstadoProducto",
                    "Debes seleccionar un estado de producto válido.");
            }


            // ========================================================
            // VALIDAR CÓDIGO DUPLICADO
            // ========================================================

            if (!string.IsNullOrWhiteSpace(
                producto.CodigoProducto))
            {
                var codigoExiste = await _context.Productos
                    .AnyAsync(p =>
                        p.CodigoProducto ==
                        producto.CodigoProducto);

                if (codigoExiste)
                {
                    ModelState.AddModelError(
                        "CodigoProducto",
                        "Ya existe un producto con este código.");
                }
            }


            // ========================================================
            // GUARDAR
            // ========================================================

            if (ModelState.IsValid)
            {
                producto.Estado = true;
                producto.FechaRegistro = DateTime.Now;

                _context.Productos.Add(producto);

                await _context.SaveChangesAsync();


                TempData["Mensaje"] =
                    "El platillo fue creado correctamente.";


                return RedirectToAction(
                    "Details",
                    "Categorias",
                    new
                    {
                        id = producto.IdCategoria
                    });
            }


            // ========================================================
            // ERRORES
            // ========================================================

            await CargarListas(
                producto.IdCategoria,
                producto.IdEstadoProducto);

            return View(producto);
        }


        // ============================================================
        // GET: Productos/Edit/5
        // ============================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var producto = await _context.Productos
                .FindAsync(id);


            if (producto == null)
            {
                return NotFound();
            }


            await CargarListas(
                producto.IdCategoria,
                producto.IdEstadoProducto);


            return View(producto);
        }


        // ============================================================
        // POST: Productos/Edit/5
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdProducto,IdCategoria,IdEstadoProducto,Nombre,Descripcion,PrecioVenta,Costo,Imagen,TiempoPreparacion,CodigoProducto,ControlaInventario")]
            Producto producto)
        {
            // --------------------------------------------------------
            // Verificar ID
            // --------------------------------------------------------

            if (id != producto.IdProducto)
            {
                return NotFound();
            }


            // --------------------------------------------------------
            // Código duplicado
            // --------------------------------------------------------

            var codigoExiste = await _context.Productos
                .AnyAsync(p =>
                    p.CodigoProducto ==
                    producto.CodigoProducto &&
                    p.IdProducto != producto.IdProducto);

            if (codigoExiste)
            {
                ModelState.AddModelError(
                    "CodigoProducto",
                    "Ya existe otro producto con este código.");
            }


            // --------------------------------------------------------
            // Categoría
            // --------------------------------------------------------

            var categoriaExiste = await _context.Categoria
                .AnyAsync(c =>
                    c.IdCategoria ==
                    producto.IdCategoria);

            if (!categoriaExiste)
            {
                ModelState.AddModelError(
                    "IdCategoria",
                    "La categoría seleccionada no existe.");
            }


            // --------------------------------------------------------
            // Estado
            // --------------------------------------------------------

            var estadoExiste = await _context.EstadoProductos
                .AnyAsync(e =>
                    e.IdEstadoProducto ==
                    producto.IdEstadoProducto);

            if (!estadoExiste)
            {
                ModelState.AddModelError(
                    "IdEstadoProducto",
                    "El estado del producto seleccionado no existe.");
            }


            // --------------------------------------------------------
            // Si hay errores
            // --------------------------------------------------------

            if (!ModelState.IsValid)
            {
                await CargarListas(
                    producto.IdCategoria,
                    producto.IdEstadoProducto);

                return View(producto);
            }


            // --------------------------------------------------------
            // Buscar producto original
            // --------------------------------------------------------

            var productoExistente =
                await _context.Productos.FindAsync(id);


            if (productoExistente == null)
            {
                return NotFound();
            }


            // --------------------------------------------------------
            // Actualizar
            // --------------------------------------------------------

            productoExistente.IdCategoria =
                producto.IdCategoria;

            productoExistente.IdEstadoProducto =
                producto.IdEstadoProducto;

            productoExistente.Nombre =
                producto.Nombre;

            productoExistente.Descripcion =
                producto.Descripcion;

            productoExistente.PrecioVenta =
                producto.PrecioVenta;

            productoExistente.Costo =
                producto.Costo;

            productoExistente.Imagen =
                producto.Imagen;

            productoExistente.TiempoPreparacion =
                producto.TiempoPreparacion;

            productoExistente.CodigoProducto =
                producto.CodigoProducto;

            productoExistente.ControlaInventario =
                producto.ControlaInventario;


            await _context.SaveChangesAsync();


            TempData["Mensaje"] =
                "El platillo fue actualizado correctamente.";


            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = productoExistente.IdProducto
                });
        }


        // ============================================================
        // POST: Productos/Desactivar/5
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var producto =
                await _context.Productos.FindAsync(id);


            if (producto == null)
            {
                return NotFound();
            }


            if (!producto.Estado)
            {
                TempData["Error"] =
                    "El platillo ya se encuentra inactivo.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            producto.Estado = false;

            await _context.SaveChangesAsync();


            TempData["Mensaje"] =
                "El platillo fue desactivado correctamente.";


            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // ============================================================
        // POST: Productos/Activar/5
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id)
        {
            var producto =
                await _context.Productos.FindAsync(id);


            if (producto == null)
            {
                return NotFound();
            }


            if (producto.Estado)
            {
                TempData["Error"] =
                    "El platillo ya se encuentra activo.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            producto.Estado = true;

            await _context.SaveChangesAsync();


            TempData["Mensaje"] =
                "El platillo fue activado correctamente.";


            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // ============================================================
        // MÉTODO AUXILIAR
        // ============================================================

        private async Task CargarListas(
            int? categoriaSeleccionada = null,
            int? estadoSeleccionado = null)
        {
            // --------------------------------------------------------
            // Categorías
            // --------------------------------------------------------

            ViewBag.Categorias = new SelectList(
                await _context.Categoria
                    .Where(c =>
                        c.Estado ||
                        c.IdCategoria ==
                        categoriaSeleccionada)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync(),
                "IdCategoria",
                "Nombre",
                categoriaSeleccionada);


            // --------------------------------------------------------
            // Estados
            // --------------------------------------------------------

            ViewBag.EstadosProducto = new SelectList(
                await _context.EstadoProductos
                    .OrderBy(e => e.Nombre)
                    .ToListAsync(),
                "IdEstadoProducto",
                "Nombre",
                estadoSeleccionado);
        }
    }
}