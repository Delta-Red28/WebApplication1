namespace WebApplication1.ViewModels
{
    public class MovimientoInventarioViewModel
    {
        public int IdMovimientoInventario { get; set; }

        public DateTime FechaMovimiento { get; set; }

        // INSUMO
        public int IdInsumo { get; set; }
        public string? CodigoInsumo { get; set; }
        public string? Insumo { get; set; }

        // UBICACIÓN
        public int IdUbicacion { get; set; }
        public string? Ubicacion { get; set; }

        // TIPO DE MOVIMIENTO
        public int IdTipoMovimientoInventario { get; set; }
        public string? TipoMovimiento { get; set; }

        // LOTE
        public int? IdLote { get; set; }
        public string? CodigoLote { get; set; }

        // CANTIDAD Y COSTO
        public decimal Cantidad { get; set; }
        public decimal? CostoUnitario { get; set; }

        // REFERENCIA
        public string? Referencia { get; set; }
        public string? Descripcion { get; set; }

        // USUARIO
        public int IdUsuario { get; set; }
        public string? Usuario { get; set; }
    }
}