using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoCaja")]
[Index("Nombre", Name = "UQ_EstadoCaja", IsUnique = true)]
public partial class EstadoCaja
{
    [Key]
    public int IdEstadoCaja { get; set; }

    [StringLength(40)]
    public string Nombre { get; set; } = null!;

    [StringLength(150)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("IdEstadoCajaNavigation")]
    public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();
}
