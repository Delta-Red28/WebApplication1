using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Compra")]
[Index("IdEstadoCompra", "FechaCompra", Name = "IX_Compra_Estado_Fecha")]
[Index("FechaCompra", Name = "IX_Compra_Fecha")]
[Index("IdProveedor", Name = "IX_Compra_Proveedor")]
[Index("NumeroCompra", Name = "UQ_Compra_Numero", IsUnique = true)]
public partial class Compra
{
    [Key]
    public int IdCompra { get; set; }

    public int IdProveedor { get; set; }

    public int IdEstadoCompra { get; set; }

    public int IdUsuario { get; set; }

    public int IdMoneda { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string NumeroCompra { get; set; } = string.Empty;

    [StringLength(50)]
    [Unicode(false)]
    public string? NumeroFacturaProveedor { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Descuento { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Impuesto { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total { get; set; }

    [StringLength(300)]
    public string? Observacion { get; set; }

    public DateTime FechaCompra { get; set; }

    [InverseProperty("IdCompraNavigation")]
    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; }
        = new List<DetalleCompra>();

    [ForeignKey("IdEstadoCompra")]
    [InverseProperty("Compras")]
    public virtual EstadoCompra IdEstadoCompraNavigation { get; set; } = null!;

    [ForeignKey("IdMoneda")]
    [InverseProperty("Compras")]
    public virtual Monedum IdMonedaNavigation { get; set; } = null!;

    [ForeignKey("IdProveedor")]
    [InverseProperty("Compras")]
    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Compras")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    [InverseProperty("IdCompraNavigation")]
    public virtual ICollection<SeguimientoCompra> SeguimientoCompras { get; set; }
        = new List<SeguimientoCompra>();
}