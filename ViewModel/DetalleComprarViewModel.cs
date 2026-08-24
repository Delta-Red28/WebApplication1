using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class DetalleCompraViewModel
    {
        public int IdDetalleCompra { get; set; }

        [Required(ErrorMessage = "Seleccione un insumo.")]
        public int IdInsumo { get; set; }

        public string? NombreInsumo { get; set; }

        [Range(
            0.001,
            double.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public decimal Cantidad { get; set; }

        [Range(
            0,
            double.MaxValue,
            ErrorMessage = "El precio no puede ser negativo.")]
        public decimal PrecioUnitario { get; set; }

        public decimal Descuento { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TotalLinea { get; set; }

        public DateOnly? FechaVencimiento { get; set; }

        [StringLength(50)]
        public string? Lote { get; set; }

        [StringLength(250)]
        public string? Observacion { get; set; }
    }
}