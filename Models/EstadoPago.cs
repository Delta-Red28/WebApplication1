using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoPago")]
[Index("Nombre", Name = "UQ_EstadoPago", IsUnique = true)]
public partial class EstadoPago
{
    [Key]
    public int IdEstadoPago { get; set; }

    [StringLength(40)]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    [InverseProperty(nameof(Pago.IdEstadoPagoNavigation))]
    public virtual ICollection<Pago> Pagos { get; set; }
        = new List<Pago>();
}