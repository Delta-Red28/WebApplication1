using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("DetalleMerma")]
public partial class DetalleMerma
{
    [Key]
    public int IdDetalleMerma { get; set; }

    public int IdMerma { get; set; }

    public int IdInsumo { get; set; }

    public int? IdLote { get; set; }

    public int IdMotivoMerma { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cantidad { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    [ForeignKey("IdInsumo")]
    [InverseProperty("DetalleMermas")]
    public virtual Insumo IdInsumoNavigation { get; set; } = null!;

    [ForeignKey("IdLote")]
    [InverseProperty("DetalleMermas")]
    public virtual LoteInsumo? IdLoteNavigation { get; set; }

    [ForeignKey("IdMerma")]
    [InverseProperty("DetalleMermas")]
    public virtual Merma IdMermaNavigation { get; set; } = null!;

    [ForeignKey("IdMotivoMerma")]
    [InverseProperty("DetalleMermas")]
    public virtual MotivoMerma IdMotivoMermaNavigation { get; set; } = null!;
}
