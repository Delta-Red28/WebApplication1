using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Index("Modulo", Name = "IX_Auditoria_Modulo")]
[Index("IdUsuario", "FechaRegistro", Name = "IX_Auditoria_Usuario_Fecha", IsDescending = new[] { false, true })]
public partial class Auditorium
{
    [Key]
    public int IdAuditoria { get; set; }

    public int IdUsuario { get; set; }

    [StringLength(100)]
    public string Modulo { get; set; } = null!;

    [StringLength(100)]
    public string Accion { get; set; } = null!;

    [StringLength(100)]
    public string? TablaAfectada { get; set; }

    public int? IdRegistro { get; set; }

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Column("DireccionIP")]
    [StringLength(45)]
    public string? DireccionIp { get; set; }

    [StringLength(100)]
    public string? Equipo { get; set; }

    public DateTime FechaRegistro { get; set; }

    [ForeignKey("IdUsuario")]
    [InverseProperty("Auditoria")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
