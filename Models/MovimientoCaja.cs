using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("MovimientoCaja")]
[Index("IdCaja", Name = "IX_MovimientoCaja_Caja")]
[Index("FechaMovimiento", Name = "IX_MovimientoCaja_Fecha")]
public partial class MovimientoCaja
{
    [Key]
    public int IdMovimientoCaja { get; set; }

    public int IdCaja { get; set; }

    public int IdTipoMovimiento { get; set; }

    public int? IdPago { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Monto { get; set; }

    [StringLength(250)]
    public string Descripcion { get; set; } = null!;

    public DateTime FechaMovimiento { get; set; }

    [ForeignKey("IdCaja")]
    [InverseProperty("MovimientoCajas")]
    public virtual Caja IdCajaNavigation { get; set; } = null!;

    [ForeignKey("IdPago")]
    [InverseProperty("MovimientoCajas")]
    public virtual Pago? IdPagoNavigation { get; set; }

    [ForeignKey("IdTipoMovimiento")]
    [InverseProperty("MovimientoCajas")]
    public virtual TipoMovimientoCaja IdTipoMovimientoNavigation { get; set; } = null!;
}
