using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("DetalleCompra")]
public partial class DetalleCompra
{
    [Key]
    public int IdDetalleCompra { get; set; }

    public int IdCompra { get; set; }

    public int IdInsumo { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cantidad { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Descuento { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Impuesto { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalLinea { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Lote { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    [ForeignKey("IdCompra")]
    [InverseProperty("DetalleCompras")]
    public virtual Compra IdCompraNavigation { get; set; } = null!;

    [ForeignKey("IdInsumo")]
    [InverseProperty("DetalleCompras")]
    public virtual Insumo IdInsumoNavigation { get; set; } = null!;

    [InverseProperty("IdDetalleCompraNavigation")]
    public virtual ICollection<LoteInsumo> LoteInsumos { get; set; } = new List<LoteInsumo>();
}
