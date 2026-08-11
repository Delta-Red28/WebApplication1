using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("TipoMovimientoCaja")]
[Index("Nombre", Name = "UQ_TipoMovimiento", IsUnique = true)]
public partial class TipoMovimientoCaja
{
    [Key]
    public int IdTipoMovimiento { get; set; }

    [StringLength(40)]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    [InverseProperty("IdTipoMovimientoNavigation")]
    public virtual ICollection<MovimientoCaja> MovimientoCajas { get; set; } = new List<MovimientoCaja>();
}
