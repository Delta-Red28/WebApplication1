using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("SerieFactura")]
[Index("Nombre", Name = "UQ_SerieFactura_Nombre", IsUnique = true)]
[Index("Prefijo", Name = "UQ_SerieFactura_Prefijo", IsUnique = true)]
public partial class SerieFactura
{
    [Key]
    public int IdSerieFactura { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string Prefijo { get; set; } = null!;

    public int NumeroActual { get; set; }

    public byte LongitudNumero { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("IdSerieFacturaNavigation")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
