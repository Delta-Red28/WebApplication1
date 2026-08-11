using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Pedido")]
[Index("IdEstadoPedido", Name = "IX_Pedido_Estado")]
[Index("IdEstadoPedido", "FechaPedido", Name = "IX_Pedido_Estado_Fecha")]
[Index("FechaPedido", Name = "IX_Pedido_Fecha")]
[Index("IdMesa", Name = "IX_Pedido_Mesa")]
[Index("NumeroPedido", Name = "UQ_NumeroPedido", IsUnique = true)]
public partial class Pedido
{
    [Key]
    public int IdPedido { get; set; }

    public int? IdCliente { get; set; }

    public int? IdMesa { get; set; }

    public int IdUsuario { get; set; }

    public int IdTipoPedido { get; set; }

    public int IdEstadoPedido { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string NumeroPedido { get; set; } = null!;

    [StringLength(400)]
    public string? Observacion { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Descuento { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Impuesto { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total { get; set; }

    public DateTime FechaPedido { get; set; }

    [InverseProperty("IdPedidoNavigation")]
    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    [InverseProperty("IdPedidoNavigation")]
    public virtual Factura? Factura { get; set; }

    [ForeignKey("IdCliente")]
    [InverseProperty("Pedidos")]
    public virtual Cliente? IdClienteNavigation { get; set; }

    [ForeignKey("IdEstadoPedido")]
    [InverseProperty("Pedidos")]
    public virtual EstadoPedido IdEstadoPedidoNavigation { get; set; } = null!;

    [ForeignKey("IdMesa")]
    [InverseProperty("Pedidos")]
    public virtual Mesa? IdMesaNavigation { get; set; }

    [ForeignKey("IdTipoPedido")]
    [InverseProperty("Pedidos")]
    public virtual TipoPedido IdTipoPedidoNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Pedidos")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
