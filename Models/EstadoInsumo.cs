using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoInsumo")]
[Index("Nombre", Name = "UQ_EstadoInsumo", IsUnique = true)]
public partial class EstadoInsumo
{
    [Key]
    public int IdEstadoInsumo { get; set; }

    [StringLength(40)]
    public string Nombre { get; set; } = null!;

    [StringLength(150)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("IdEstadoInsumoNavigation")]
    public virtual ICollection<Insumo> Insumos { get; set; } = new List<Insumo>();
}
