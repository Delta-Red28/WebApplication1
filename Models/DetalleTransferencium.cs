using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class DetalleTransferencium
{
    [Key]
    public int IdDetalleTransferencia { get; set; }

    public int IdTransferencia { get; set; }

    public int IdInsumo { get; set; }

    public int? IdLote { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cantidad { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    [ForeignKey("IdInsumo")]
    [InverseProperty("DetalleTransferencia")]
    public virtual Insumo IdInsumoNavigation { get; set; } = null!;

    [ForeignKey("IdLote")]
    [InverseProperty("DetalleTransferencia")]
    public virtual LoteInsumo? IdLoteNavigation { get; set; }

    [ForeignKey("IdTransferencia")]
    [InverseProperty("DetalleTransferencia")]
    public virtual TransferenciaInventario IdTransferenciaNavigation { get; set; } = null!;
}
