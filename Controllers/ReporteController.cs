using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels.Reportes;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Controllers
{
    public class ReportesController : Controller
    {
        private readonly RestauranteContext _context;
        private readonly ReportesService _reportesService;

        public ReportesController(
            RestauranteContext context,
            ReportesService reportesService)
        {
            _context = context;
            _reportesService = reportesService;
        }

        // ============================================================
        // DASHBOARD PRINCIPAL
        // ============================================================

        public async Task<IActionResult> Index(
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            // ========================================================
            // FECHAS
            // ========================================================

            var inicio = fechaInicio?.Date
                ?? new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            var fin = fechaFin?.Date
                ?? inicio.AddMonths(1).AddDays(-1);

            // Para incluir completamente el último día
            var finExclusivo = fin.AddDays(1);

            var model = new ReporteDashboardViewModel
            {
                FechaInicio = inicio,
                FechaFin = fin
            };

            // ========================================================
            // FACTURAS
            // ========================================================

            var facturas = await _context.Facturas
                .Where(f =>
                    f.FechaFactura >= inicio &&
                    f.FechaFactura < finExclusivo)
                .ToListAsync();

            model.CantidadFacturas = facturas.Count;

            model.TotalFacturado = facturas
                .Sum(f => f.Total);

            model.TotalDescuentos = facturas
                .Sum(f => f.Descuento);

            model.TotalImpuestos = facturas
                .Sum(f => f.Impuesto);

            model.TotalSaldoPendiente = facturas
                .Sum(f => f.SaldoPendiente);

            // ========================================================
            // PAGOS
            // ========================================================
            //
            // IMPORTANTE:
            // Se cargan explícitamente las relaciones necesarias
            // para que IdMetodoPagoNavigation no llegue NULL.
            //
            // ========================================================

            var pagos = await _context.Pagos
                .Include(p => p.IdMetodoPagoNavigation)
                .Include(p => p.IdMonedaNavigation)
                .Where(p =>
                    p.FechaPago >= inicio &&
                    p.FechaPago < finExclusivo)
                .ToListAsync();

            model.CantidadPagos = pagos.Count;

            model.TotalPagos = pagos
                .Sum(p => p.Monto);

            // ========================================================
            // COMPRAS
            // ========================================================

            var compras = await _context.Compras
                .Where(c =>
                    c.FechaCompra >= inicio &&
                    c.FechaCompra < finExclusivo)
                .ToListAsync();

            model.CantidadCompras = compras.Count;

            model.TotalCompras = compras
                .Sum(c => c.Total);

            // ========================================================
            // CLIENTES
            // ========================================================

            model.CantidadClientes = await _context.Clientes
                .CountAsync(c => c.Estado);

            // ========================================================
            // PROVEEDORES
            // ========================================================

            model.CantidadProveedores = await _context.Proveedors
                .CountAsync(p => p.Estado);

            // ========================================================
            // PRODUCTOS
            // ========================================================

            model.CantidadProductos = await _context.Productos
                .CountAsync(p => p.Estado);

            // ========================================================
            // VENTAS POR FECHA
            // ========================================================

            model.VentasPorFecha = facturas
                .GroupBy(f => f.FechaFactura.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ReporteVentaGraficaViewModel
                {
                    Fecha = g.Key,
                    Total = g.Sum(f => f.Total)
                })
                .ToList();

            // ========================================================
            // PAGOS POR MÉTODO
            // ========================================================
            //
            // Se agrupa por el nombre real del método.
            //
            // Si por alguna razón existe un pago cuyo método no
            // existe en la tabla MetodoPago, se mostrará
            // "Sin método".
            //
            // ========================================================

            model.PagosPorMetodo = pagos
                .GroupBy(p =>
                    p.IdMetodoPagoNavigation != null &&
                    !string.IsNullOrWhiteSpace(
                        p.IdMetodoPagoNavigation.Nombre)
                        ? p.IdMetodoPagoNavigation.Nombre.Trim()
                        : "Sin método")
                .OrderByDescending(g =>
                    g.Sum(p => p.Monto))
                .Select(g => new ReporteMetodoPagoGraficaViewModel
                {
                    MetodoPago = g.Key,

                    Total = g.Sum(p =>
                        p.Monto),

                    Cantidad = g.Count()
                })
                .ToList();

            return View(model);
        }
    }
}