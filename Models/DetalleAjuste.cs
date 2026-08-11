using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("DetalleAjuste")]
public partial class DetalleAjuste
{
    [Key]
    public int IdDetalleAjuste { get; set; }

    public int IdAjuste { get; set; }

    public int IdInsumo { get; set; }

    public int IdUbicacion { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal StockSistema { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal StockFisico { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Diferencia { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    [ForeignKey("IdAjuste")]
    [InverseProperty("DetalleAjustes")]
    public virtual AjusteInventario IdAjusteNavigation { get; set; } = null!;

    [ForeignKey("IdInsumo")]
    [InverseProperty("DetalleAjustes")]
    public virtual Insumo IdInsumoNavigation { get; set; } = null!;

    [ForeignKey("IdUbicacion")]
    [InverseProperty("DetalleAjustes")]
    public virtual UbicacionInventario IdUbicacionNavigation { get; set; } = null!;
}
