using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("TipoComprobante")]
public partial class TipoComprobante
{
    [Key]
    public int IdTipoComprobante { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    [InverseProperty("IdTipoComprobanteNavigation")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
