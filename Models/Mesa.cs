using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Mesa")]
[Index("IdEstadoMesa", Name = "IX_Mesa_Estado")]
[Index("NumeroMesa", Name = "UQ_Mesa_Numero", IsUnique = true)]
public partial class Mesa
{
    [Key]
    public int IdMesa { get; set; }

    public int NumeroMesa { get; set; }

    public int Capacidad { get; set; }

    public int IdEstadoMesa { get; set; }

    [StringLength(100)]
    public string? Ubicacion { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    public bool Estado { get; set; }

    [ForeignKey("IdEstadoMesa")]
    [InverseProperty("Mesas")]
    public virtual EstadoMesa IdEstadoMesaNavigation { get; set; } = null!;

    [InverseProperty("IdMesaNavigation")]
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    [InverseProperty("IdMesaNavigation")]
    public virtual ICollection<Reservacion> Reservacions { get; set; } = new List<Reservacion>();
}
