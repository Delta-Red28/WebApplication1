using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoLote")]
[Index("Nombre", Name = "UQ_EstadoLote", IsUnique = true)]
public partial class EstadoLote
{
    [Key]
    public int IdEstadoLote { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [StringLength(200)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("IdEstadoLoteNavigation")]
    public virtual ICollection<LoteInsumo> LoteInsumos { get; set; } = new List<LoteInsumo>();
}
