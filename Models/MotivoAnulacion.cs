using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("MotivoAnulacion")]
[Index("Nombre", Name = "UQ_MotivoAnulacion", IsUnique = true)]
public partial class MotivoAnulacion
{
    [Key]
    public int IdMotivoAnulacion { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    [InverseProperty("IdMotivoAnulacionNavigation")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
