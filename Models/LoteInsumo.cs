using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("LoteInsumo")]
[Index("IdInsumo", Name = "IX_Lote_Insumo")]
[Index("IdUbicacion", Name = "IX_Lote_Ubicacion")]
[Index("FechaVencimiento", Name = "IX_Lote_Vencimiento")]
[Index("CodigoLote", Name = "UQ_Lote_Codigo", IsUnique = true)]
public partial class LoteInsumo
{
    [Key]
    public int IdLote { get; set; }

    public int IdDetalleCompra { get; set; }

    public int IdInsumo { get; set; }

    public int IdUbicacion { get; set; }

    public int IdEstadoLote { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string CodigoLote { get; set; } = null!;

    public DateOnly? FechaFabricacion { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CantidadInicial { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CantidadDisponible { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CostoUnitario { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    public DateTime FechaRegistro { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CantidadReservada { get; set; }

    [InverseProperty("IdLoteNavigation")]
    public virtual ICollection<DetalleMerma> DetalleMermas { get; set; } = new List<DetalleMerma>();

    [InverseProperty("IdLoteNavigation")]
    public virtual ICollection<DetalleTransferencium> DetalleTransferencia { get; set; } = new List<DetalleTransferencium>();

    [ForeignKey("IdDetalleCompra")]
    [InverseProperty("LoteInsumos")]
    public virtual DetalleCompra IdDetalleCompraNavigation { get; set; } = null!;

    [ForeignKey("IdEstadoLote")]
    [InverseProperty("LoteInsumos")]
    public virtual EstadoLote IdEstadoLoteNavigation { get; set; } = null!;

    [ForeignKey("IdInsumo")]
    [InverseProperty("LoteInsumos")]
    public virtual Insumo IdInsumoNavigation { get; set; } = null!;

    [ForeignKey("IdUbicacion")]
    [InverseProperty("LoteInsumos")]
    public virtual UbicacionInventario IdUbicacionNavigation { get; set; } = null!;

    [InverseProperty("IdLoteNavigation")]
    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();
}
