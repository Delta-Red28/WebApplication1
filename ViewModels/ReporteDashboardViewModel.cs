using System;
using System.Collections.Generic;

namespace WebApplication1.ViewModels.Reportes
{
    /// <summary>
    /// ViewModel principal del dashboard de reportes.
    /// </summary>
    public class ReporteDashboardViewModel
    {
        // ============================================================
        // PERÍODO DEL REPORTE
        // ============================================================

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }


        // ============================================================
        // FACTURAS
        // ============================================================

        /// <summary>
        /// Cantidad total de facturas generadas en el período.
        /// </summary>
        public int CantidadFacturas { get; set; }

        /// <summary>
        /// Total facturado en el período.
        /// </summary>
        public decimal TotalFacturado { get; set; }

        /// <summary>
        /// Total de descuentos aplicados.
        /// </summary>
        public decimal TotalDescuentos { get; set; }

        /// <summary>
        /// Total de impuestos generados.
        /// </summary>
        public decimal TotalImpuestos { get; set; }

        /// <summary>
        /// Total pendiente por cobrar.
        /// </summary>
        public decimal TotalSaldoPendiente { get; set; }


        // ============================================================
        // PAGOS
        // ============================================================

        /// <summary>
        /// Cantidad total de pagos registrados en el período.
        /// </summary>
        public int CantidadPagos { get; set; }

        /// <summary>
        /// Monto total de pagos registrados en el período.
        /// </summary>
        public decimal TotalPagos { get; set; }


        // ============================================================
        // COMPRAS
        // ============================================================

        /// <summary>
        /// Cantidad total de compras realizadas en el período.
        /// </summary>
        public int CantidadCompras { get; set; }

        /// <summary>
        /// Total de compras realizadas en el período.
        /// </summary>
        public decimal TotalCompras { get; set; }


        // ============================================================
        // CLIENTES
        // ============================================================

        /// <summary>
        /// Cantidad de clientes activos.
        /// </summary>
        public int CantidadClientes { get; set; }


        // ============================================================
        // PROVEEDORES
        // ============================================================

        /// <summary>
        /// Cantidad de proveedores activos.
        /// </summary>
        public int CantidadProveedores { get; set; }


        // ============================================================
        // PRODUCTOS
        // ============================================================

        /// <summary>
        /// Cantidad de productos activos.
        /// </summary>
        public int CantidadProductos { get; set; }


        // ============================================================
        // GRÁFICA DE VENTAS
        // ============================================================

        /// <summary>
        /// Ventas agrupadas por fecha.
        /// </summary>
        public List<ReporteVentaGraficaViewModel> VentasPorFecha { get; set; }
            = new List<ReporteVentaGraficaViewModel>();


        // ============================================================
        // GRÁFICA DE MÉTODOS DE PAGO
        // ============================================================

        /// <summary>
        /// Pagos agrupados por método de pago.
        ///
        /// Ejemplo:
        ///
        /// Efectivo       C$ 5,000.00
        /// Tarjeta        C$ 2,000.00
        /// Transferencia  C$ 1,500.00
        /// </summary>
        public List<ReporteMetodoPagoGraficaViewModel> PagosPorMetodo { get; set; }
            = new List<ReporteMetodoPagoGraficaViewModel>();
    }


    // =================================================================
    // VENTAS POR FECHA
    // =================================================================

    public class ReporteVentaGraficaViewModel
    {
        /// <summary>
        /// Fecha de la venta.
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Total vendido en esa fecha.
        /// </summary>
        public decimal Total { get; set; }
    }


    // =================================================================
    // PAGOS POR MÉTODO
    // =================================================================

    public class ReporteMetodoPagoGraficaViewModel
    {
        /// <summary>
        /// Nombre del método de pago.
        ///
        /// Ejemplo:
        /// Efectivo
        /// Tarjeta
        /// Transferencia
        /// </summary>
        public string MetodoPago { get; set; } = string.Empty;


        /// <summary>
        /// Total monetario recibido mediante ese método.
        /// </summary>
        public decimal Total { get; set; }


        /// <summary>
        /// Cantidad de pagos realizados mediante ese método.
        /// </summary>
        public int Cantidad { get; set; }


        /// <summary>
        /// Nombre de la moneda utilizada.
        ///
        /// Ejemplo:
        /// Córdoba
        /// Dólar
        /// </summary>
        public string Moneda { get; set; } = string.Empty;


        /// <summary>
        /// Código ISO de la moneda.
        ///
        /// Ejemplo:
        /// NIO
        /// USD
        /// </summary>
        public string CodigoMoneda { get; set; } = string.Empty;


        /// <summary>
        /// Símbolo de la moneda.
        ///
        /// Ejemplo:
        /// C$
        /// $
        /// </summary>
        public string SimboloMoneda { get; set; } = string.Empty;
    }
}