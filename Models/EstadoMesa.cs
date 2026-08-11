using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoMesa")]
[Index("Nombre", Name = "UQ__EstadoMe__75E3EFCF1277B0E7", IsUnique = true)]
public partial class EstadoMesa
{
    [Key]
    public int IdEstadoMesa { get; set; }

    [StringLength(40)]
    public string Nombre { get; set; } = null!;

    [StringLength(200)]
    public string? Descripcion { get; set; }

    [InverseProperty("IdEstadoMesaNavigation")]
    public virtual ICollection<Mesa> Mesas { get; set; } = new List<Mesa>();
}
