using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Index("IdInsumo", Name = "IX_Existencia_Insumo")]
[Index("IdUbicacion", Name = "IX_Existencia_Ubicacion")]
[Index("IdInsumo", "IdUbicacion", Name = "UQ_Existencia", IsUnique = true)]
public partial class Existencium
{
    [Key]
    public int IdExistencia { get; set; }

    public int IdInsumo { get; set; }

    public int IdUbicacion { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal StockActual { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal StockMinimo { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? StockMaximo { get; set; }

    public DateTime UltimaActualizacion { get; set; }

    [ForeignKey("IdInsumo")]
    [InverseProperty("Existencia")]
    public virtual Insumo IdInsumoNavigation { get; set; } = null!;

    [ForeignKey("IdUbicacion")]
    [InverseProperty("Existencia")]
    public virtual UbicacionInventario IdUbicacionNavigation { get; set; } = null!;
}
