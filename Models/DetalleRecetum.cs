using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Index("IdInsumo", Name = "IX_DetalleReceta_Insumo")]
[Index("IdReceta", Name = "IX_DetalleReceta_Receta")]
[Index("IdReceta", "IdInsumo", Name = "UQ_DetalleReceta", IsUnique = true)]
public partial class DetalleRecetum
{
    [Key]
    public int IdDetalleReceta { get; set; }

    public int IdReceta { get; set; }

    public int IdInsumo { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal Cantidad { get; set; }

    public int IdUnidadMedida { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    [ForeignKey("IdInsumo")]
    [InverseProperty("DetalleReceta")]
    public virtual Insumo IdInsumoNavigation { get; set; } = null!;

    [ForeignKey("IdReceta")]
    [InverseProperty("DetalleReceta")]
    public virtual Recetum IdRecetaNavigation { get; set; } = null!;

    [ForeignKey("IdUnidadMedida")]
    [InverseProperty("DetalleReceta")]
    public virtual UnidadMedidum IdUnidadMedidaNavigation { get; set; } = null!;
}
