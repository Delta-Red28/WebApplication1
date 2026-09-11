using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.ViewModels.Reportes;

namespace WebApplication1.Services
{
    public class ReportesService
    {
        private readonly RestauranteContext _context;

        public ReportesService(RestauranteContext context)
        {
            _context = context;
        }


        // ============================================================
        // DASHBOARD PRINCIPAL
        // ============================================================

        public async Task<ReporteDashboardViewModel> ObtenerDashboardAsync(
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            // ========================================================
            // FECHAS
            // ========================================================

            DateTime fechaInicioReal = fechaInicio.Date;

            DateTime fechaFinExclusiva =
                fechaFin.Date.AddDays(1);


            // ========================================================
            // MODELO
            // ========================================================

            var modelo = new ReporteDashboardViewModel
            {
                FechaInicio = fechaInicioReal,
                FechaFin = fechaFin.Date
            };


            // ========================================================
            // FACTURAS
            // ========================================================

            var facturas = _context.Facturas
                .AsNoTracking()
                .Where(f =>
                    f.FechaFactura >= fechaInicioReal &&
                    f.FechaFactura < fechaFinExclusiva);


            modelo.CantidadFacturas =
                await facturas.CountAsync();


            modelo.TotalFacturado =
                await facturas
                    .Select(f => (decimal?)f.Total)
                    .SumAsync() ?? 0m;


            modelo.TotalDescuentos =
                await facturas
                    .Select(f => (decimal?)f.Descuento)
                    .SumAsync() ?? 0m;


            modelo.TotalImpuestos =
                await facturas
                    .Select(f => (decimal?)f.Impuesto)
                    .SumAsync() ?? 0m;


            modelo.TotalSaldoPendiente =
                await facturas
                    .Select(f => (decimal?)f.SaldoPendiente)
                    .SumAsync() ?? 0m;


            // ========================================================
            // PAGOS
            // ========================================================

            var pagos = _context.Pagos
                .AsNoTracking()
                .Where(p =>
                    p.FechaPago >= fechaInicioReal &&
                    p.FechaPago < fechaFinExclusiva);


            modelo.CantidadPagos =
                await pagos.CountAsync();


            modelo.TotalPagos =
                await pagos
                    .Select(p => (decimal?)p.Monto)
                    .SumAsync() ?? 0m;


            // ========================================================
            // COMPRAS
            // ========================================================

            var compras = _context.Compras
                .AsNoTracking()
                .Where(c =>
                    c.FechaCompra >= fechaInicioReal &&
                    c.FechaCompra < fechaFinExclusiva);


            modelo.CantidadCompras =
                await compras.CountAsync();


            modelo.TotalCompras =
                await compras
                    .Select(c => (decimal?)c.Total)
                    .SumAsync() ?? 0m;


            // ========================================================
            // CLIENTES
            // ========================================================

            modelo.CantidadClientes =
                await _context.Clientes
                    .AsNoTracking()
                    .CountAsync(c => c.Estado);


            // ========================================================
            // PROVEEDORES
            // ========================================================

            modelo.CantidadProveedores =
                await _context.Proveedors
                    .AsNoTracking()
                    .CountAsync(p => p.Estado);


            // ========================================================
            // PRODUCTOS
            // ========================================================

            modelo.CantidadProductos =
                await _context.Productos
                    .AsNoTracking()
                    .CountAsync(p => p.Estado);


            // ========================================================
            // VENTAS POR FECHA
            // ========================================================

            modelo.VentasPorFecha =
                await facturas
                    .GroupBy(f => f.FechaFactura.Date)
                    .Select(g => new ReporteVentaGraficaViewModel
                    {
                        Fecha = g.Key,

                        Total = g.Sum(f => f.Total)
                    })
                    .OrderBy(x => x.Fecha)
                    .ToListAsync();


            // ========================================================
            // PAGOS POR MÉTODO DE PAGO
            // ========================================================
            //
            // IMPORTANTE:
            //
            // No utilizamos:
            //
            // Include(p => p.IdMetodoPagoNavigation)
            //
            // para luego agrupar utilizando la navegación.
            //
            // En su lugar hacemos la proyección directamente desde
            // las relaciones de Pago.
            //
            // Esto permite obtener:
            //
            // Pago
            //  ├── Método de pago
            //  └── Moneda
            //
            // ========================================================

            modelo.PagosPorMetodo =
                await pagos
                    .Where(p =>
                        p.IdMetodoPagoNavigation != null &&
                        p.IdMonedaNavigation != null)
                    .Select(p => new
                    {
                        MetodoPago =
                            p.IdMetodoPagoNavigation!.Nombre,

                        Monto =
                            p.Monto,

                        Moneda =
                            p.IdMonedaNavigation!.Nombre,

                        CodigoMoneda =
                            p.IdMonedaNavigation!.CodigoIso,

                        SimboloMoneda =
                            p.IdMonedaNavigation!.Simbolo
                    })
                    .GroupBy(x => new
                    {
                        x.MetodoPago,
                        x.Moneda,
                        x.CodigoMoneda,
                        x.SimboloMoneda
                    })
                    .Select(g => new ReporteMetodoPagoGraficaViewModel
                    {
                        MetodoPago = g.Key.MetodoPago,

                        Total = g.Sum(x => x.Monto),

                        Cantidad = g.Count(),

                        Moneda = g.Key.Moneda,

                        CodigoMoneda = g.Key.CodigoMoneda,

                        SimboloMoneda = g.Key.SimboloMoneda
                    })
                    .OrderByDescending(x => x.Total)
                    .ToListAsync();


            // ========================================================
            // RETORNAR DASHBOARD
            // ========================================================

            return modelo;
        }


        // ============================================================
        // TOTAL DE VENTAS
        // ============================================================

        public async Task<decimal> ObtenerTotalVentasAsync(
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            DateTime fechaFinExclusiva =
                fechaFin.Date.AddDays(1);


            return await _context.Facturas
                .AsNoTracking()
                .Where(f =>
                    f.FechaFactura >= fechaInicio.Date &&
                    f.FechaFactura < fechaFinExclusiva)
                .Select(f => (decimal?)f.Total)
                .SumAsync() ?? 0m;
        }


        // ============================================================
        // TOTAL DE COMPRAS
        // ============================================================

        public async Task<decimal> ObtenerTotalComprasAsync(
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            DateTime fechaFinExclusiva =
                fechaFin.Date.AddDays(1);


            return await _context.Compras
                .AsNoTracking()
                .Where(c =>
                    c.FechaCompra >= fechaInicio.Date &&
                    c.FechaCompra < fechaFinExclusiva)
                .Select(c => (decimal?)c.Total)
                .SumAsync() ?? 0m;
        }


        // ============================================================
        // TOTAL DE PAGOS
        // ============================================================

        public async Task<decimal> ObtenerTotalPagosAsync(
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            DateTime fechaFinExclusiva =
                fechaFin.Date.AddDays(1);


            return await _context.Pagos
                .AsNoTracking()
                .Where(p =>
                    p.FechaPago >= fechaInicio.Date &&
                    p.FechaPago < fechaFinExclusiva)
                .Select(p => (decimal?)p.Monto)
                .SumAsync() ?? 0m;
        }


        // ============================================================
        // CANTIDAD DE FACTURAS
        // ============================================================

        public async Task<int> ObtenerCantidadFacturasAsync(
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            DateTime fechaFinExclusiva =
                fechaFin.Date.AddDays(1);


            return await _context.Facturas
                .AsNoTracking()
                .CountAsync(f =>
                    f.FechaFactura >= fechaInicio.Date &&
                    f.FechaFactura < fechaFinExclusiva);
        }


        // ============================================================
        // CANTIDAD DE COMPRAS
        // ============================================================

        public async Task<int> ObtenerCantidadComprasAsync(
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            DateTime fechaFinExclusiva =
                fechaFin.Date.AddDays(1);


            return await _context.Compras
                .AsNoTracking()
                .CountAsync(c =>
                    c.FechaCompra >= fechaInicio.Date &&
                    c.FechaCompra < fechaFinExclusiva);
        }


        // ============================================================
        // PRODUCTOS
        // ============================================================

        public async Task<List<Producto>> ObtenerProductosAsync()
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdEstadoProductoNavigation)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }


        // ============================================================
        // PROVEEDORES
        // ============================================================

        public async Task<List<Proveedor>> ObtenerProveedoresAsync()
        {
            return await _context.Proveedors
                .AsNoTracking()
                .Include(p => p.IdEstadoProveedorNavigation)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }


        // ============================================================
        // CLIENTES
        // ============================================================

        public async Task<List<Cliente>> ObtenerClientesAsync()
        {
            return await _context.Clientes
                .AsNoTracking()
                .Where(c => c.Estado)
                .OrderBy(c => c.Nombres)
                .ThenBy(c => c.Apellidos)
                .ToListAsync();
        }


        // ============================================================
        // MESAS
        // ============================================================

        public async Task<List<Mesa>> ObtenerMesasAsync()
        {
            return await _context.Mesas
                .AsNoTracking()
                .Include(m => m.IdEstadoMesaNavigation)
                .OrderBy(m => m.NumeroMesa)
                .ToListAsync();
        }


        // ============================================================
        // MONEDAS
        // ============================================================

        public async Task<List<Monedum>> ObtenerMonedasAsync()
        {
            return await _context.Moneda
                .AsNoTracking()
                .Where(m => m.Estado)
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }


        // ============================================================
        // MÉTODOS DE PAGO
        // ============================================================

        public async Task<List<MetodoPago>> ObtenerMetodosPagoAsync()
        {
            return await _context.MetodoPagos
                .AsNoTracking()
                .Where(m => m.Estado)
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }
    }
}