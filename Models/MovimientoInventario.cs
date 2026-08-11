using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("MovimientoInventario")]
[Index("FechaMovimiento", Name = "IX_MovimientoInventario_Fecha")]
[Index("IdInsumo", Name = "IX_MovimientoInventario_Insumo")]
[Index("IdTipoMovimientoInventario", Name = "IX_MovimientoInventario_Tipo")]
public partial class MovimientoInventario
{
    [Key]
    public int IdMovimientoInventario { get; set; }

    public int IdInsumo { get; set; }

    public int IdUbicacion { get; set; }

    public int IdTipoMovimientoInventario { get; set; }

    public int IdUsuario { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cantidad { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? CostoUnitario { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Referencia { get; set; }

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public int? IdLote { get; set; }

    [ForeignKey("IdInsumo")]
    [InverseProperty("MovimientoInventarios")]
    public virtual Insumo IdInsumoNavigation { get; set; } = null!;

    [ForeignKey("IdLote")]
    [InverseProperty("MovimientoInventarios")]
    public virtual LoteInsumo? IdLoteNavigation { get; set; }

    [ForeignKey("IdTipoMovimientoInventario")]
    [InverseProperty("MovimientoInventarios")]
    public virtual TipoMovimientoInventario IdTipoMovimientoInventarioNavigation { get; set; } = null!;

    [ForeignKey("IdUbicacion")]
    [InverseProperty("MovimientoInventarios")]
    public virtual UbicacionInventario IdUbicacionNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("MovimientoInventarios")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
