using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class MovimientosInventarioController : Controller
    {
        private readonly RestauranteContext _context;

        public MovimientosInventarioController(
            RestauranteContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // HISTORIAL DE MOVIMIENTOS
        // ============================================================

        public async Task<IActionResult> Index()
        {
            var movimientos = await _context.MovimientoInventarios
                .AsNoTracking()

                .Include(m => m.IdInsumoNavigation)
                .Include(m => m.IdTipoMovimientoInventarioNavigation)
                .Include(m => m.IdUbicacionNavigation)
                .Include(m => m.IdLoteNavigation)
                .Include(m => m.IdUsuarioNavigation)

                .OrderByDescending(m => m.FechaMovimiento)

                .Select(m => new MovimientoInventarioViewModel
                {
                    IdMovimientoInventario =
                        m.IdMovimientoInventario,

                    FechaMovimiento =
                        m.FechaMovimiento,

                    IdInsumo =
                        m.IdInsumo,

                    CodigoInsumo =
                        m.IdInsumoNavigation.CodigoInsumo,

                    Insumo =
                        m.IdInsumoNavigation.Nombre,

                    IdUbicacion =
                        m.IdUbicacion,

                    Ubicacion =
                        m.IdUbicacionNavigation.Nombre,

                    IdTipoMovimientoInventario =
                        m.IdTipoMovimientoInventario,

                    TipoMovimiento =
                        m.IdTipoMovimientoInventarioNavigation.Nombre,

                    IdLote =
                        m.IdLote,

                    CodigoLote =
                        m.IdLoteNavigation != null
                            ? m.IdLoteNavigation.CodigoLote
                            : null,

                    Cantidad =
                        m.Cantidad,

                    CostoUnitario =
                        m.CostoUnitario,

                    Referencia =
                        m.Referencia,

                    Descripcion =
                        m.Descripcion,

                    IdUsuario =
                        m.IdUsuario,

                    Usuario =
                        m.IdUsuarioNavigation.Nombres
                        + " " +
                        m.IdUsuarioNavigation.Apellidos
                })
                .ToListAsync();

            return View(movimientos);
        }

        // ============================================================
        // DETAILS
        // ============================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento =
                await _context.MovimientoInventarios
                    .AsNoTracking()

                    .Include(m => m.IdInsumoNavigation)
                    .Include(m => m.IdTipoMovimientoInventarioNavigation)
                    .Include(m => m.IdUbicacionNavigation)
                    .Include(m => m.IdLoteNavigation)
                    .Include(m => m.IdUsuarioNavigation)

                    .FirstOrDefaultAsync(m =>
                        m.IdMovimientoInventario == id.Value);

            if (movimiento == null)
            {
                return NotFound();
            }

            return View(movimiento);
        }

        // ============================================================
        // CREATE - GET
        // ============================================================

        [HttpGet]
        public IActionResult Create()
        {
            CargarCombos();

            return View();
        }

        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MovimientoInventario movimiento)
        {
            if (movimiento.Cantidad <= 0)
            {
                ModelState.AddModelError(
                    nameof(movimiento.Cantidad),
                    "La cantidad debe ser mayor que cero.");
            }

            var insumo =
                await _context.Insumos
                    .FirstOrDefaultAsync(i =>
                        i.IdInsumo == movimiento.IdInsumo &&
                        i.Estado);

            if (insumo == null)
            {
                ModelState.AddModelError(
                    nameof(movimiento.IdInsumo),
                    "El insumo seleccionado no existe o está inactivo.");
            }

            var ubicacion =
                await _context.UbicacionInventarios
                    .FirstOrDefaultAsync(u =>
                        u.IdUbicacion == movimiento.IdUbicacion &&
                        u.Estado);

            if (ubicacion == null)
            {
                ModelState.AddModelError(
                    nameof(movimiento.IdUbicacion),
                    "La ubicación seleccionada no existe o está inactiva.");
            }

            var tipoMovimiento =
                await _context.TipoMovimientoInventarios
                    .FirstOrDefaultAsync(t =>
                        t.IdTipoMovimientoInventario ==
                        movimiento.IdTipoMovimientoInventario &&
                        t.Estado);

            if (tipoMovimiento == null)
            {
                ModelState.AddModelError(
                    nameof(movimiento.IdTipoMovimientoInventario),
                    "El tipo de movimiento seleccionado no existe o está inactivo.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(movimiento);

                return View(movimiento);
            }

            bool esEntrada =
                EsMovimientoEntrada(tipoMovimiento!.Nombre);

            bool esSalida =
                EsMovimientoSalida(tipoMovimiento.Nombre);

            if (!esEntrada && !esSalida)
            {
                ModelState.AddModelError(
                    nameof(movimiento.IdTipoMovimientoInventario),
                    "El tipo de movimiento no está configurado como entrada o salida.");

                CargarCombos(movimiento);

                return View(movimiento);
            }

            // ========================================================
            // USUARIO
            // ========================================================

            if (movimiento.IdUsuario <= 0)
            {
                var usuario =
                    await _context.Usuarios
                        .Where(u => u.Estado)
                        .OrderBy(u => u.IdUsuario)
                        .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    ModelState.AddModelError(
                        "",
                        "No existe un usuario activo para registrar el movimiento.");

                    CargarCombos(movimiento);

                    return View(movimiento);
                }

                movimiento.IdUsuario =
                    usuario.IdUsuario;
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var existencia =
                    await _context.Existencia
                        .FirstOrDefaultAsync(e =>
                            e.IdInsumo == movimiento.IdInsumo &&
                            e.IdUbicacion == movimiento.IdUbicacion);

                if (existencia == null)
                {
                    if (esSalida)
                    {
                        ModelState.AddModelError(
                            "",
                            "No existe existencia para este insumo en la ubicación seleccionada.");

                        await transaction.RollbackAsync();

                        CargarCombos(movimiento);

                        return View(movimiento);
                    }

                    existencia = new Existencium
                    {
                        IdInsumo =
                            movimiento.IdInsumo,

                        IdUbicacion =
                            movimiento.IdUbicacion,

                        StockActual = 0,

                        StockMinimo =
                            insumo!.StockMinimo,

                        StockMaximo =
                            insumo.StockMaximo,

                        UltimaActualizacion =
                            DateTime.Now
                    };

                    _context.Existencia.Add(existencia);
                }

                if (esSalida)
                {
                    if (existencia.StockActual <
                        movimiento.Cantidad)
                    {
                        ModelState.AddModelError(
                            nameof(movimiento.Cantidad),
                            $"Stock insuficiente. Disponible: {existencia.StockActual:N2}.");

                        await transaction.RollbackAsync();

                        CargarCombos(movimiento);

                        return View(movimiento);
                    }

                    existencia.StockActual -=
                        movimiento.Cantidad;
                }

                if (esEntrada)
                {
                    existencia.StockActual +=
                        movimiento.Cantidad;
                }

                existencia.StockMinimo =
                    insumo!.StockMinimo;

                existencia.StockMaximo =
                    insumo.StockMaximo;

                existencia.UltimaActualizacion =
                    DateTime.Now;

                // ====================================================
                // LOTE
                // ====================================================

                if (movimiento.IdLote.HasValue)
                {
                    var lote =
                        await _context.LoteInsumos
                            .FirstOrDefaultAsync(l =>
                                l.IdLote ==
                                movimiento.IdLote.Value);

                    if (lote == null)
                    {
                        ModelState.AddModelError(
                            nameof(movimiento.IdLote),
                            "El lote seleccionado no existe.");

                        await transaction.RollbackAsync();

                        CargarCombos(movimiento);

                        return View(movimiento);
                    }

                    if (lote.IdInsumo !=
                        movimiento.IdInsumo)
                    {
                        ModelState.AddModelError(
                            nameof(movimiento.IdLote),
                            "El lote seleccionado no pertenece al insumo.");

                        await transaction.RollbackAsync();

                        CargarCombos(movimiento);

                        return View(movimiento);
                    }

                    if (esSalida)
                    {
                        if (lote.CantidadDisponible <
                            movimiento.Cantidad)
                        {
                            ModelState.AddModelError(
                                nameof(movimiento.Cantidad),
                                $"Cantidad insuficiente en el lote. Disponible: {lote.CantidadDisponible:N2}.");

                            await transaction.RollbackAsync();

                            CargarCombos(movimiento);

                            return View(movimiento);
                        }

                        lote.CantidadDisponible -=
                            movimiento.Cantidad;
                    }

                    if (esEntrada)
                    {
                        lote.CantidadDisponible +=
                            movimiento.Cantidad;
                    }
                }

                if (movimiento.FechaMovimiento == default)
                {
                    movimiento.FechaMovimiento =
                        DateTime.Now;
                }

                _context.MovimientoInventarios.Add(
                    movimiento);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Mensaje"] =
                    "Movimiento de inventario registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al registrar el movimiento.");

                CargarCombos(movimiento);

                return View(movimiento);
            }
        }

        // ============================================================
        // AJUSTE - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Ajuste(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento =
                await _context.MovimientoInventarios
                    .AsNoTracking()
                    .Include(m => m.IdInsumoNavigation)
                    .Include(m => m.IdUbicacionNavigation)
                    .Include(m => m.IdLoteNavigation)
                    .FirstOrDefaultAsync(m =>
                        m.IdMovimientoInventario == id.Value);

            if (movimiento == null)
            {
                return NotFound();
            }

            // --------------------------------------------------------
            // BUSCAR EXISTENCIA ACTUAL
            // --------------------------------------------------------

            var existencia =
                await _context.Existencia
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e =>
                        e.IdInsumo == movimiento.IdInsumo &&
                        e.IdUbicacion == movimiento.IdUbicacion);

            ViewBag.StockActual =
                existencia?.StockActual ?? 0;

            ViewBag.CodigoInsumo =
                movimiento.IdInsumoNavigation.CodigoInsumo;

            ViewBag.Insumo =
                movimiento.IdInsumoNavigation.Nombre;

            ViewBag.Ubicacion =
                movimiento.IdUbicacionNavigation.Nombre;

            ViewBag.CodigoLote =
                movimiento.IdLoteNavigation?.CodigoLote;

            ViewBag.IdMovimientoOriginal =
                movimiento.IdMovimientoInventario;

            ViewBag.FechaMovimientoOriginal =
                movimiento.FechaMovimiento;

            ViewBag.CantidadOriginal =
                movimiento.Cantidad;

            ViewBag.TipoMovimientoOriginal =
                movimiento.IdTipoMovimientoInventario;

            // --------------------------------------------------------
            // BUSCAR TIPO DE AJUSTE
            // --------------------------------------------------------

            var tiposAjuste =
                await _context.TipoMovimientoInventarios
                    .Where(t =>
                        t.Estado &&
                        t.Nombre.ToLower().Contains("ajuste"))
                    .OrderBy(t => t.Nombre)
                    .ToListAsync();

            ViewBag.TiposAjuste =
                new SelectList(
                    tiposAjuste,
                    "IdTipoMovimientoInventario",
                    "Nombre");

            return View(movimiento);
        }

        // ============================================================
        // AJUSTE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ajuste(
            int id,
            decimal cantidadAjuste,
            int idTipoMovimientoInventario,
            string? descripcion)
        {
            // --------------------------------------------------------
            // VALIDAR CANTIDAD
            // --------------------------------------------------------

            if (cantidadAjuste <= 0)
            {
                TempData["Error"] =
                    "La cantidad del ajuste debe ser mayor que cero.";

                return RedirectToAction(
                    nameof(Ajuste),
                    new { id });
            }

            // --------------------------------------------------------
            // BUSCAR MOVIMIENTO ORIGINAL
            // --------------------------------------------------------

            var movimientoOriginal =
                await _context.MovimientoInventarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m =>
                        m.IdMovimientoInventario == id);

            if (movimientoOriginal == null)
            {
                return NotFound();
            }

            // --------------------------------------------------------
            // VALIDAR TIPO DE AJUSTE
            // --------------------------------------------------------

            var tipoAjuste =
                await _context.TipoMovimientoInventarios
                    .FirstOrDefaultAsync(t =>
                        t.IdTipoMovimientoInventario ==
                        idTipoMovimientoInventario &&
                        t.Estado);

            if (tipoAjuste == null)
            {
                TempData["Error"] =
                    "El tipo de ajuste seleccionado no existe o está inactivo.";

                return RedirectToAction(
                    nameof(Ajuste),
                    new { id });
            }

            var nombreTipo =
                tipoAjuste.Nombre
                    .Trim()
                    .ToLower();

            if (!nombreTipo.Contains("ajuste"))
            {
                TempData["Error"] =
                    "El tipo de movimiento seleccionado no corresponde a un ajuste.";

                return RedirectToAction(
                    nameof(Ajuste),
                    new { id });
            }

            // --------------------------------------------------------
            // DETERMINAR SI EL AJUSTE SUMA O RESTA
            // --------------------------------------------------------

            bool esEntrada =
                EsMovimientoEntrada(nombreTipo);

            bool esSalida =
                EsMovimientoSalida(nombreTipo);

            // Si el nombre es simplemente "Ajuste",
            // se requiere indicar explícitamente entrada/salida.
            if (!esEntrada && !esSalida)
            {
                TempData["Error"] =
                    "El tipo de ajuste debe indicar si es entrada o salida. " +
                    "Ejemplo: 'Ajuste Entrada' o 'Ajuste Salida'.";

                return RedirectToAction(
                    nameof(Ajuste),
                    new { id });
            }

            // --------------------------------------------------------
            // OBTENER USUARIO ACTUAL
            // --------------------------------------------------------

            int idUsuario = 0;

            var claim =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                int.TryParse(
                    claim.Value,
                    out idUsuario);
            }

            // --------------------------------------------------------
            // SI NO HAY CLAIM, USAR USUARIO ACTIVO
            // --------------------------------------------------------

            if (idUsuario <= 0)
            {
                var usuario =
                    await _context.Usuarios
                        .Where(u => u.Estado)
                        .OrderBy(u => u.IdUsuario)
                        .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    TempData["Error"] =
                        "No existe un usuario activo para registrar el ajuste.";

                    return RedirectToAction(
                        nameof(Ajuste),
                        new { id });
                }

                idUsuario =
                    usuario.IdUsuario;
            }

            // ========================================================
            // TRANSACCIÓN
            // ========================================================

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // ----------------------------------------------------
                // EXISTENCIA
                // ----------------------------------------------------

                var existencia =
                    await _context.Existencia
                        .FirstOrDefaultAsync(e =>
                            e.IdInsumo ==
                                movimientoOriginal.IdInsumo
                            &&
                            e.IdUbicacion ==
                                movimientoOriginal.IdUbicacion);

                // ----------------------------------------------------
                // CREAR EXISTENCIA SI ES AJUSTE DE ENTRADA
                // ----------------------------------------------------

                if (existencia == null)
                {
                    if (esSalida)
                    {
                        await transaction.RollbackAsync();

                        TempData["Error"] =
                            "No existe existencia para realizar este ajuste de salida.";

                        return RedirectToAction(
                            nameof(Ajuste),
                            new { id });
                    }

                    var insumo =
                        await _context.Insumos
                            .FirstOrDefaultAsync(i =>
                                i.IdInsumo ==
                                movimientoOriginal.IdInsumo);

                    if (insumo == null)
                    {
                        await transaction.RollbackAsync();

                        TempData["Error"] =
                            "El insumo del movimiento no existe.";

                        return RedirectToAction(
                            nameof(Ajuste),
                            new { id });
                    }

                    existencia = new Existencium
                    {
                        IdInsumo =
                            movimientoOriginal.IdInsumo,

                        IdUbicacion =
                            movimientoOriginal.IdUbicacion,

                        StockActual = 0,

                        StockMinimo =
                            insumo.StockMinimo,

                        StockMaximo =
                            insumo.StockMaximo,

                        UltimaActualizacion =
                            DateTime.Now
                    };

                    _context.Existencia.Add(existencia);
                }

                // ----------------------------------------------------
                // AJUSTE DE SALIDA
                // ----------------------------------------------------

                if (esSalida)
                {
                    if (existencia.StockActual <
                        cantidadAjuste)
                    {
                        await transaction.RollbackAsync();

                        TempData["Error"] =
                            $"Stock insuficiente para el ajuste. " +
                            $"Disponible: {existencia.StockActual:N2}.";

                        return RedirectToAction(
                            nameof(Ajuste),
                            new { id });
                    }

                    existencia.StockActual -=
                        cantidadAjuste;
                }

                // ----------------------------------------------------
                // AJUSTE DE ENTRADA
                // ----------------------------------------------------

                if (esEntrada)
                {
                    existencia.StockActual +=
                        cantidadAjuste;
                }

                existencia.UltimaActualizacion =
                    DateTime.Now;

                // ====================================================
                // LOTE
                // ====================================================

                if (movimientoOriginal.IdLote.HasValue)
                {
                    var lote =
                        await _context.LoteInsumos
                            .FirstOrDefaultAsync(l =>
                                l.IdLote ==
                                movimientoOriginal.IdLote.Value);

                    if (lote == null)
                    {
                        await transaction.RollbackAsync();

                        TempData["Error"] =
                            "El lote asociado al movimiento no existe.";

                        return RedirectToAction(
                            nameof(Ajuste),
                            new { id });
                    }

                    if (esSalida)
                    {
                        if (lote.CantidadDisponible <
                            cantidadAjuste)
                        {
                            await transaction.RollbackAsync();

                            TempData["Error"] =
                                $"Cantidad insuficiente en el lote. " +
                                $"Disponible: {lote.CantidadDisponible:N2}.";

                            return RedirectToAction(
                                nameof(Ajuste),
                                new { id });
                        }

                        lote.CantidadDisponible -=
                            cantidadAjuste;
                    }

                    if (esEntrada)
                    {
                        lote.CantidadDisponible +=
                            cantidadAjuste;
                    }
                }

                // ====================================================
                // DESCRIPCIÓN DEL AJUSTE
                // ====================================================

                var descripcionAjuste =
                    string.IsNullOrWhiteSpace(descripcion)
                        ? $"Ajuste del movimiento #{id}"
                        : descripcion.Trim();

                descripcionAjuste =
                    $"Ajuste del movimiento #{id}: " +
                    descripcionAjuste;

                // ====================================================
                // CREAR NUEVO MOVIMIENTO
                // ====================================================

                var nuevoMovimiento =
                    new MovimientoInventario
                    {
                        IdInsumo =
                            movimientoOriginal.IdInsumo,

                        IdUbicacion =
                            movimientoOriginal.IdUbicacion,

                        IdTipoMovimientoInventario =
                            idTipoMovimientoInventario,

                        IdUsuario =
                            idUsuario,

                        IdLote =
                            movimientoOriginal.IdLote,

                        Cantidad =
                            cantidadAjuste,

                        CostoUnitario =
                            movimientoOriginal.CostoUnitario,

                        Referencia =
                            $"AJUSTE-{id}",

                        Descripcion =
                            descripcionAjuste,

                        FechaMovimiento =
                            DateTime.Now
                    };

                _context.MovimientoInventarios.Add(
                    nuevoMovimiento);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Mensaje"] =
                    $"Ajuste registrado correctamente. " +
                    $"Se creó el movimiento #{nuevoMovimiento.IdMovimientoInventario}.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id =
                            nuevoMovimiento.IdMovimientoInventario
                    });
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Ocurrió un error al registrar el ajuste.";

                return RedirectToAction(
                    nameof(Ajuste),
                    new { id });
            }
        }

        // ============================================================
        // EDIT
        // LOS MOVIMIENTOS NO SE EDITAN
        // ============================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento =
                await _context.MovimientoInventarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m =>
                        m.IdMovimientoInventario ==
                        id.Value);

            if (movimiento == null)
            {
                return NotFound();
            }

            TempData["Mensaje"] =
                "Los movimientos registrados no se editan. " +
                "Para corregir un movimiento debe registrarse un ajuste.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id =
                        movimiento.IdMovimientoInventario
                });
        }

        // ============================================================
        // DELETE
        // ============================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento =
                await _context.MovimientoInventarios
                    .AsNoTracking()

                    .Include(m => m.IdInsumoNavigation)
                    .Include(m => m.IdTipoMovimientoInventarioNavigation)
                    .Include(m => m.IdUbicacionNavigation)
                    .Include(m => m.IdLoteNavigation)
                    .Include(m => m.IdUsuarioNavigation)

                    .FirstOrDefaultAsync(m =>
                        m.IdMovimientoInventario ==
                        id.Value);

            if (movimiento == null)
            {
                return NotFound();
            }

            return View(movimiento);
        }

        // ============================================================
        // DELETE CONFIRMED
        // LOS MOVIMIENTOS NO SE ELIMINAN
        // ============================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            TempData["Mensaje"] =
                "Los movimientos de inventario forman parte del historial y no pueden eliminarse.";

            return RedirectToAction(
                nameof(Index));
        }

        // ============================================================
        // CARGAR COMBOS
        // ============================================================

        private void CargarCombos(
            MovimientoInventario? movimiento = null)
        {
            ViewData["IdInsumo"] =
                new SelectList(
                    _context.Insumos
                        .Where(i => i.Estado)
                        .OrderBy(i => i.Nombre)
                        .ToList(),
                    "IdInsumo",
                    "Nombre",
                    movimiento?.IdInsumo);

            ViewData["IdUbicacion"] =
                new SelectList(
                    _context.UbicacionInventarios
                        .Where(u => u.Estado)
                        .OrderBy(u => u.Nombre)
                        .ToList(),
                    "IdUbicacion",
                    "Nombre",
                    movimiento?.IdUbicacion);

            ViewData["IdTipoMovimientoInventario"] =
                new SelectList(
                    _context.TipoMovimientoInventarios
                        .Where(t => t.Estado)
                        .OrderBy(t => t.Nombre)
                        .ToList(),
                    "IdTipoMovimientoInventario",
                    "Nombre",
                    movimiento?.IdTipoMovimientoInventario);

            ViewData["IdLote"] =
                new SelectList(
                    _context.LoteInsumos
                        .Include(l =>
                            l.IdInsumoNavigation)
                        .OrderByDescending(l =>
                            l.FechaRegistro)
                        .Select(l => new
                        {
                            l.IdLote,

                            Nombre =
                                l.CodigoLote
                                + " - "
                                + l.IdInsumoNavigation.Nombre
                        })
                        .ToList(),
                    "IdLote",
                    "Nombre",
                    movimiento?.IdLote);

            ViewData["IdUsuario"] =
                new SelectList(
                    _context.Usuarios
                        .Where(u => u.Estado)
                        .OrderBy(u => u.Nombres)
                        .ThenBy(u => u.Apellidos)
                        .ToList(),
                    "IdUsuario",
                    "Nombres",
                    movimiento?.IdUsuario);
        }

        // ============================================================
        // DETERMINAR ENTRADA
        // ============================================================

        private bool EsMovimientoEntrada(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return false;
            }

            nombre =
                nombre.Trim().ToLower();

            return nombre.Contains("entrada")
                || nombre.Contains("compra")
                || nombre.Contains("ingreso")
                || nombre.Contains("recepción")
                || nombre.Contains("recepcion");
        }

        // ============================================================
        // DETERMINAR SALIDA
        // ============================================================

        private bool EsMovimientoSalida(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return false;
            }

            nombre =
                nombre.Trim().ToLower();

            return nombre.Contains("salida")
                || nombre.Contains("consumo")
                || nombre.Contains("merma")
                || nombre.Contains("despacho")
                || nombre.Contains("egreso");
        }
    }

}