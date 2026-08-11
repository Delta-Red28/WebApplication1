using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Marca")]
[Index("Nombre", Name = "UQ_Marca", IsUnique = true)]
public partial class Marca
{
    [Key]
    public int IdMarca { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("IdMarcaNavigation")]
    public virtual ICollection<Insumo> Insumos { get; set; } = new List<Insumo>();
}
