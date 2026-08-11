using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Index("Nombre", Name = "UQ_EstadoReceta", IsUnique = true)]
public partial class EstadoRecetum
{
    [Key]
    public int IdEstadoReceta { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [StringLength(200)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("IdEstadoRecetaNavigation")]
    public virtual ICollection<Recetum> Receta { get; set; } = new List<Recetum>();
}
