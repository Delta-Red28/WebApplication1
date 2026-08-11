using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Impuesto")]
[Index("Nombre", Name = "UQ_Impuesto", IsUnique = true)]
public partial class Impuesto
{
    [Key]
    public int IdImpuesto { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Porcentaje { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("IdImpuestoNavigation")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
