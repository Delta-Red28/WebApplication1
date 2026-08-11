using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("HistorialFactura")]
public partial class HistorialFactura
{
    [Key]
    public int IdHistorialFactura { get; set; }

    public int IdFactura { get; set; }

    public int IdEstadoFactura { get; set; }

    public int IdUsuario { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    public DateTime Fecha { get; set; }

    [ForeignKey("IdEstadoFactura")]
    [InverseProperty("HistorialFacturas")]
    public virtual EstadoFactura IdEstadoFacturaNavigation { get; set; } = null!;

    [ForeignKey("IdFactura")]
    [InverseProperty("HistorialFacturas")]
    public virtual Factura IdFacturaNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("HistorialFacturas")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
