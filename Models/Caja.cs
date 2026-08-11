using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Caja")]
[Index("FechaApertura", Name = "IX_Caja_Fecha")]
[Index("IdUsuario", Name = "IX_Caja_Usuario")]
public partial class Caja
{
    [Key]
    public int IdCaja { get; set; }

    public int IdUsuario { get; set; }

    public int IdEstadoCaja { get; set; }

    public DateTime FechaApertura { get; set; }

    public DateTime? FechaCierre { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MontoInicial { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalIngresos { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalEgresos { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalSistema { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalContado { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Diferencia { get; set; }

    [StringLength(300)]
    public string? Observacion { get; set; }

    [ForeignKey("IdEstadoCaja")]
    [InverseProperty("Cajas")]
    public virtual EstadoCaja IdEstadoCajaNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Cajas")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    [InverseProperty("IdCajaNavigation")]
    public virtual ICollection<MovimientoCaja> MovimientoCajas { get; set; } = new List<MovimientoCaja>();
}
