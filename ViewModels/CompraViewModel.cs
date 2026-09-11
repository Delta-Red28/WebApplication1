using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class CompraViewModel
    {
        public int IdCompra { get; set; }

        [Required(ErrorMessage = "Seleccione un proveedor.")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "Seleccione una moneda.")]
        public int IdMoneda { get; set; }

        [Required(ErrorMessage = "Ingrese el número de compra.")]
        [StringLength(20)]
        public string NumeroCompra { get; set; } = string.Empty;

        [StringLength(50)]
        public string? NumeroFacturaProveedor { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Descuento { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }

        [StringLength(300)]
        public string? Observacion { get; set; }

        public DateTime FechaCompra { get; set; } = DateTime.Now;

        public List<DetalleCompraViewModel> Detalles { get; set; }
            = new();
    }
}