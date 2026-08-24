using System;

namespace WebApplication1.ViewModels
{
    public class MovimientoInventarioViewModel
    {
        public int IdMovimientoInventario { get; set; }

        public int IdInsumo { get; set; }

        public string NombreInsumo { get; set; } = string.Empty;

        public string CodigoInsumo { get; set; } = string.Empty;

        public int IdUbicacion { get; set; }

        public string Ubicacion { get; set; } = string.Empty;

        public int IdTipoMovimientoInventario { get; set; }

        public string TipoMovimiento { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }

        public decimal? CostoUnitario { get; set; }

        public string? Referencia { get; set; }

        public string? Descripcion { get; set; }

        public int? IdLote { get; set; }

        public string? CodigoLote { get; set; }

        public DateTime FechaMovimiento { get; set; }
    }
}