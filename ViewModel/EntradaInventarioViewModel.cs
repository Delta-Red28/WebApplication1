using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class EntradaInventarioViewModel
    {
        [Required]
        [Display(Name = "Insumo")]
        public int IdInsumo { get; set; }

        [Required]
        [Display(Name = "Ubicación")]
        public int IdUbicacion { get; set; }

        [Required]
        [Display(Name = "Cantidad")]
        [Range(0.01, double.MaxValue)]
        public decimal Cantidad { get; set; }

        [Required]
        [Display(Name = "Costo unitario")]
        [Range(0, double.MaxValue)]
        public decimal CostoUnitario { get; set; }

        [StringLength(50)]
        [Display(Name = "Código de lote")]
        public string? CodigoLote { get; set; }

        [Display(Name = "Fecha de fabricación")]
        public DateOnly? FechaFabricacion { get; set; }

        [Display(Name = "Fecha de vencimiento")]
        public DateOnly? FechaVencimiento { get; set; }

        [StringLength(250)]
        public string? Observacion { get; set; }

        [StringLength(50)]
        public string? Referencia { get; set; }
    }
}