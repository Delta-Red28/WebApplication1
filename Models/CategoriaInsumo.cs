using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("CategoriaInsumo")]
[Index("Nombre", Name = "IX_CategoriaInsumo_Nombre")]
[Index("Nombre", Name = "UQ_CategoriaInsumo", IsUnique = true)]
public partial class CategoriaInsumo
{
    [Key]
    public int IdCategoriaInsumo { get; set; }

    [StringLength(80)]
    public string Nombre { get; set; } = null!;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("IdCategoriaInsumoNavigation")]
    public virtual ICollection<Insumo> Insumos { get; set; } = new List<Insumo>();
}
