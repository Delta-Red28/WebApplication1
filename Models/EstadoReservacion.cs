using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoReservacion")]
[Index("Nombre", Name = "UQ__EstadoRe__75E3EFCF6D8E1510", IsUnique = true)]
public partial class EstadoReservacion
{
    [Key]
    public int IdEstadoReservacion { get; set; }

    [StringLength(40)]
    public string Nombre { get; set; } = null!;

    [InverseProperty("IdEstadoReservacionNavigation")]
    public virtual ICollection<Reservacion> Reservacions { get; set; } = new List<Reservacion>();
}
