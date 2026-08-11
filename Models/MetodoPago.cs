using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("MetodoPago")]
[Index("Nombre", Name = "UQ_MetodoPago", IsUnique = true)]
public partial class MetodoPago
{
    [Key]
    public int IdMetodoPago { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    [InverseProperty("IdMetodoPagoNavigation")]
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
