using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Categoria")]
[Index("Nombre", Name = "UQ_Categoria", IsUnique = true)]
public partial class Categorium
{
    [Key]
    public int IdCategoria { get; set; }

    [StringLength(160)]
    public string Nombre { get; set; } = null!;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("IdCategoriaNavigation")]
    public virtual ICollection<Producto> Productos { get; set; }
        = new List<Producto>();
}