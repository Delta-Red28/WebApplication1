using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class DetallePedidoViewModel
    {
        // =========================================================
        // IDENTIFICACIÓN
        // =========================================================

        public int IdDetallePedido { get; set; }

        public int IdPedido { get; set; }


        // =========================================================
        // PRODUCTO
        // =========================================================

        [Required(
            ErrorMessage = "Debe seleccionar un producto.")]
        [Display(Name = "Producto")]
        public int IdProducto { get; set; }


        // =========================================================
        // CANTIDAD
        // =========================================================

        [Required(
            ErrorMessage = "La cantidad es obligatoria.")]
        [Range(
            0.01,
            999999,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        [Display(Name = "Cantidad")]
        public decimal Cantidad { get; set; } = 1;


        // =========================================================
        // PRECIO UNITARIO
        // =========================================================
        //
        // El precio mostrado en pantalla es únicamente
        // informativo.
        //
        // El controlador obtiene nuevamente el precio real
        // desde la tabla Productos antes de guardar.
        //

        [Display(Name = "Precio unitario")]
        public decimal PrecioUnitario { get; set; }


        // =========================================================
        // DESCUENTO
        // =========================================================

        [Range(
            0,
            999999999,
            ErrorMessage = "El descuento no puede ser negativo.")]
        [Display(Name = "Descuento")]
        public decimal Descuento { get; set; }


        // =========================================================
        // IMPUESTO
        // =========================================================
        //
        // Actualmente permanece en cero.
        //
        // Posteriormente se puede conectar con el módulo
        // de impuestos/facturación.
        //

        [Display(Name = "Impuesto")]
        public decimal Impuesto { get; set; }


        // =========================================================
        // SUBTOTAL
        // =========================================================

        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }


        // =========================================================
        // TOTAL DE LÍNEA
        // =========================================================

        [Display(Name = "Total")]
        public decimal TotalLinea { get; set; }


        // =========================================================
        // OBSERVACIÓN
        // =========================================================

        [StringLength(
            250,
            ErrorMessage =
                "La observación no puede superar los 250 caracteres.")]
        [Display(Name = "Observación")]
        public string? Observacion { get; set; }


        // =========================================================
        // LISTA DE PRODUCTOS
        // =========================================================
        //
        // Se utiliza para llenar el select de productos
        // en Create.cshtml y Edit.cshtml.
        //

        public IEnumerable<SelectListItem> Productos { get; set; }
            = new List<SelectListItem>();
    }
}