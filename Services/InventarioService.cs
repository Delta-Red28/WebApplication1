using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Services
{
    public class InventarioService
    {
        private readonly RestauranteContext _context;

        public InventarioService(RestauranteContext context)
        {
            _context = context;
        }

        // ============================================================
        // OBTENER INVENTARIO
        // ============================================================

        public async Task<List<InventarioViewModel>> ObtenerInventarioAsync()
        {
            return await _context.Existencia
                .AsNoTracking()
                .Include(e => e.IdInsumoNavigation)
                    .ThenInclude(i => i.IdUnidadMedidaNavigation)
                .Include(e => e.IdUbicacionNavigation)
                .Select(e => new InventarioViewModel
                {
                    IdExistencia = e.IdExistencia,

                    IdInsumo = e.IdInsumo,

                    CodigoInsumo =
                        e.IdInsumoNavigation.CodigoInsumo,

                    NombreInsumo =
                        e.IdInsumoNavigation.Nombre,

                    UnidadMedida =
                        e.IdInsumoNavigation
                            .IdUnidadMedidaNavigation.Nombre,

                    AbreviaturaUnidad =
                        e.IdInsumoNavigation
                            .IdUnidadMedidaNavigation.Abreviatura,

                    IdUbicacion = e.IdUbicacion,

                    Ubicacion =
                        e.IdUbicacionNavigation.Nombre,

                    StockActual = e.StockActual,

                    StockMinimo = e.StockMinimo,

                    StockMaximo = e.StockMaximo,

                    UltimaActualizacion =
                        e.UltimaActualizacion
                })
                .OrderBy(x => x.NombreInsumo)
                .ThenBy(x => x.Ubicacion)
                .ToListAsync();
        }

        // ============================================================
        // OBTENER EXISTENCIA
        // ============================================================

        public async Task<Existencium?> ObtenerExistenciaAsync(
            int idInsumo,
            int idUbicacion)
        {
            return await _context.Existencia
                .FirstOrDefaultAsync(x =>
                    x.IdInsumo == idInsumo &&
                    x.IdUbicacion == idUbicacion);
        }

        // ============================================================
        // OBTENER O CREAR EXISTENCIA
        // ============================================================

        private async Task<Existencium> ObtenerOCrearExistenciaAsync(
            int idInsumo,
            int idUbicacion)
        {
            var existencia = await _context.Existencia
                .FirstOrDefaultAsync(x =>
                    x.IdInsumo == idInsumo &&
                    x.IdUbicacion == idUbicacion);

            if (existencia != null)
                return existencia;

            var insumo = await _context.Insumos
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.IdInsumo == idInsumo);

            if (insumo == null)
                throw new Exception("El insumo no existe.");

            existencia = new Existencium
            {
                IdInsumo = idInsumo,
                IdUbicacion = idUbicacion,
                StockActual = 0,
                StockMinimo = insumo.StockMinimo,
                StockMaximo = insumo.StockMaximo,
                UltimaActualizacion = DateTime.Now
            };

            _context.Existencia.Add(existencia);

            return existencia;
        }

        // ============================================================
        // OBTENER TIPO DE MOVIMIENTO
        // ============================================================

        private async Task<TipoMovimientoInventario?>
            ObtenerTipoMovimientoAsync(string palabra)
        {
            return await _context.TipoMovimientoInventarios
                .FirstOrDefaultAsync(x =>
                    x.Estado &&
                    x.Nombre.ToLower().Contains(
                        palabra.ToLower()));
        }

        // ============================================================
        // ENTRADA MANUAL
        // ============================================================

        public async Task<(bool Exito, string Mensaje)>
            RegistrarEntradaAsync(
                EntradaInventarioViewModel model,
                int idUsuario)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                if (model.Cantidad <= 0)
                {
                    return (
                        false,
                        "La cantidad debe ser mayor que cero.");
                }

                var insumo = await _context.Insumos
                    .FirstOrDefaultAsync(x =>
                        x.IdInsumo == model.IdInsumo &&
                        x.Estado);

                if (insumo == null)
                {
                    return (
                        false,
                        "El insumo no existe o está inactivo.");
                }

                var ubicacion =
                    await _context.UbicacionInventarios
                        .FirstOrDefaultAsync(x =>
                            x.IdUbicacion == model.IdUbicacion &&
                            x.Estado);

                if (ubicacion == null)
                {
                    return (
                        false,
                        "La ubicación no existe o está inactiva.");
                }

                // ----------------------------------------------------
                // EXISTENCIA
                // ----------------------------------------------------

                var existencia =
                    await ObtenerOCrearExistenciaAsync(
                        model.IdInsumo,
                        model.IdUbicacion);

                existencia.StockActual += model.Cantidad;

                existencia.UltimaActualizacion =
                    DateTime.Now;

                // ----------------------------------------------------
                // TIPO DE MOVIMIENTO
                // ----------------------------------------------------

                var tipoEntrada =
                    await ObtenerTipoMovimientoAsync("entrada");

                if (tipoEntrada == null)
                {
                    tipoEntrada =
                        await ObtenerTipoMovimientoAsync("ingreso");
                }

                if (tipoEntrada == null)
                {
                    return (
                        false,
                        "No existe un tipo de movimiento de entrada activo.");
                }

                // ----------------------------------------------------
                // LOTE
                // ----------------------------------------------------

                int? idLote = null;

                if (!string.IsNullOrWhiteSpace(model.CodigoLote))
                {
                    var loteExistente =
                        await _context.LoteInsumos
                            .FirstOrDefaultAsync(x =>
                                x.CodigoLote ==
                                    model.CodigoLote &&
                                x.IdInsumo ==
                                    model.IdInsumo &&
                                x.IdUbicacion ==
                                    model.IdUbicacion);

                    if (loteExistente != null)
                    {
                        loteExistente.CantidadDisponible +=
                            model.Cantidad;

                        idLote =
                            loteExistente.IdLote;
                    }
                    else
                    {
                        return (
                            false,
                            "El lote indicado no existe. Los lotes nuevos deben generarse desde una compra.");
                    }
                }

                // ----------------------------------------------------
                // MOVIMIENTO
                // ----------------------------------------------------

                var movimiento =
                    new MovimientoInventario
                    {
                        IdInsumo =
                            model.IdInsumo,

                        IdUbicacion =
                            model.IdUbicacion,

                        IdTipoMovimientoInventario =
                            tipoEntrada
                                .IdTipoMovimientoInventario,

                        IdUsuario =
                            idUsuario,

                        Cantidad =
                            model.Cantidad,

                        CostoUnitario =
                            model.CostoUnitario,

                        Referencia =
                            model.Referencia,

                        Descripcion =
                            model.Observacion,

                        FechaMovimiento =
                            DateTime.Now,

                        IdLote =
                            idLote
                    };

                _context.MovimientoInventarios.Add(
                    movimiento);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (
                    true,
                    "Entrada registrada correctamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return (
                    false,
                    $"No se pudo registrar la entrada: {ex.Message}");
            }
        }

        // ============================================================
        // SALIDA
        // ============================================================

        public async Task<(bool Exito, string Mensaje)>
            RegistrarSalidaAsync(
                int idInsumo,
                int idUbicacion,
                decimal cantidad,
                int idUsuario,
                string? referencia = null,
                string? descripcion = null)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                if (cantidad <= 0)
                {
                    return (
                        false,
                        "La cantidad debe ser mayor que cero.");
                }

                var existencia =
                    await ObtenerExistenciaAsync(
                        idInsumo,
                        idUbicacion);

                if (existencia == null)
                {
                    return (
                        false,
                        "No existe existencia para este insumo.");
                }

                if (existencia.StockActual < cantidad)
                {
                    return (
                        false,
                        $"Stock insuficiente. Disponible: {existencia.StockActual}");
                }

                var tipoSalida =
                    await ObtenerTipoMovimientoAsync("salida");

                if (tipoSalida == null)
                {
                    return (
                        false,
                        "No existe un tipo de movimiento de salida activo.");
                }

                // ----------------------------------------------------
                // LOTES FEFO
                // ----------------------------------------------------

                var cantidadPendiente = cantidad;

                var lotes =
                    await _context.LoteInsumos
                        .Where(x =>
                            x.IdInsumo == idInsumo &&
                            x.IdUbicacion == idUbicacion &&
                            x.CantidadDisponible > 0)
                        .OrderBy(x =>
                            x.FechaVencimiento == null)
                        .ThenBy(x =>
                            x.FechaVencimiento)
                        .ThenBy(x =>
                            x.IdLote)
                        .ToListAsync();

                foreach (var lote in lotes)
                {
                    if (cantidadPendiente <= 0)
                        break;

                    var cantidadLote =
                        Math.Min(
                            lote.CantidadDisponible,
                            cantidadPendiente);

                    lote.CantidadDisponible -=
                        cantidadLote;

                    cantidadPendiente -=
                        cantidadLote;

                    var movimiento =
                        new MovimientoInventario
                        {
                            IdInsumo =
                                idInsumo,

                            IdUbicacion =
                                idUbicacion,

                            IdTipoMovimientoInventario =
                                tipoSalida
                                    .IdTipoMovimientoInventario,

                            IdUsuario =
                                idUsuario,

                            Cantidad =
                                cantidadLote,

                            CostoUnitario =
                                lote.CostoUnitario,

                            Referencia =
                                referencia,

                            Descripcion =
                                descripcion,

                            FechaMovimiento =
                                DateTime.Now,

                            IdLote =
                                lote.IdLote
                        };

                    _context.MovimientoInventarios.Add(
                        movimiento);
                }

                // ----------------------------------------------------
                // SI NO HAY LOTES SUFICIENTES
                // ----------------------------------------------------

                if (cantidadPendiente > 0)
                {
                    var movimientoSinLote =
                        new MovimientoInventario
                        {
                            IdInsumo =
                                idInsumo,

                            IdUbicacion =
                                idUbicacion,

                            IdTipoMovimientoInventario =
                                tipoSalida
                                    .IdTipoMovimientoInventario,

                            IdUsuario =
                                idUsuario,

                            Cantidad =
                                cantidadPendiente,

                            CostoUnitario =
                                null,

                            Referencia =
                                referencia,

                            Descripcion =
                                descripcion,

                            FechaMovimiento =
                                DateTime.Now,

                            IdLote =
                                null
                        };

                    _context.MovimientoInventarios.Add(
                        movimientoSinLote);
                }

                // ----------------------------------------------------
                // ACTUALIZAR EXISTENCIA
                // ----------------------------------------------------

                existencia.StockActual -= cantidad;

                existencia.UltimaActualizacion =
                    DateTime.Now;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (
                    true,
                    "Salida registrada correctamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return (
                    false,
                    $"No se pudo registrar la salida: {ex.Message}");
            }
        }

        // ============================================================
        // OBTENER MOVIMIENTOS
        // ============================================================

        public async Task<List<MovimientoInventarioViewModel>>
            ObtenerMovimientosAsync(
                int? idInsumo = null)
        {
            var query =
                _context.MovimientoInventarios
                    .AsNoTracking()
                    .Include(x =>
                        x.IdInsumoNavigation)
                    .Include(x =>
                        x.IdUbicacionNavigation)
                    .Include(x =>
                        x.IdTipoMovimientoInventarioNavigation)
                    .Include(x =>
                        x.IdLoteNavigation)
                    .AsQueryable();

            if (idInsumo.HasValue)
            {
                query = query.Where(x =>
                    x.IdInsumo ==
                    idInsumo.Value);
            }

            return await query
                .OrderByDescending(x =>
                    x.FechaMovimiento)
                .Select(x =>
                    new MovimientoInventarioViewModel
                    {
                        IdMovimientoInventario =
                            x.IdMovimientoInventario,

                        IdInsumo =
                            x.IdInsumo,

                        NombreInsumo =
                            x.IdInsumoNavigation.Nombre,

                        CodigoInsumo =
                            x.IdInsumoNavigation
                                .CodigoInsumo,

                        IdUbicacion =
                            x.IdUbicacion,

                        Ubicacion =
                            x.IdUbicacionNavigation
                                .Nombre,

                        IdTipoMovimientoInventario =
                            x.IdTipoMovimientoInventario,

                        TipoMovimiento =
                            x.IdTipoMovimientoInventarioNavigation
                                .Nombre,

                        Cantidad =
                            x.Cantidad,

                        CostoUnitario =
                            x.CostoUnitario,

                        Referencia =
                            x.Referencia,

                        Descripcion =
                            x.Descripcion,

                        IdLote =
                            x.IdLote,

                        CodigoLote =
                            x.IdLoteNavigation != null
                                ? x.IdLoteNavigation.CodigoLote
                                : null,

                        FechaMovimiento =
                            x.FechaMovimiento
                    })
                .ToListAsync();
        }
    }
}