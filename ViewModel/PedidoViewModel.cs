using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.ViewModels
{
    public class PedidoViewModel
    {
        // =========================================================
        // IDENTIFICACIÓN
        // =========================================================

        public int IdPedido { get; set; }


        // =========================================================
        // INFORMACIÓN DEL PEDIDO
        // =========================================================

        public int? IdCliente { get; set; }

        public int? IdMesa { get; set; }

        [Required(ErrorMessage = "El tipo de pedido es obligatorio.")]
        [Display(Name = "Tipo de pedido")]
        public int IdTipoPedido { get; set; }

        public int IdEstadoPedido { get; set; }

        [Display(Name = "Fecha del pedido")]
        public DateTime FechaPedido { get; set; }

        [StringLength(
            400,
            ErrorMessage = "La observación no puede superar los 400 caracteres.")]
        [Display(Name = "Observación")]
        public string? Observacion { get; set; }


        // =========================================================
        // TOTALES
        // =========================================================

        public decimal Subtotal { get; set; }

        public decimal Descuento { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }


        // =========================================================
        // COMBOS
        // =========================================================

        public IEnumerable<SelectListItem> Clientes { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> Mesas { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> TiposPedido { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> EstadosPedido { get; set; }
            = new List<SelectListItem>();


        // =========================================================
        // DETALLES
        // =========================================================

        public List<DetallePedidoViewModel> Detalles { get; set; }
            = new List<DetallePedidoViewModel>();
    }
}