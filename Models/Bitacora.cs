using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Bitacora")]
public partial class Bitacora
{
    [Key]
    public long IdBitacora { get; set; }

    public int IdUsuario { get; set; }

    [StringLength(100)]
    public string Accion { get; set; } = null!;

    [StringLength(100)]
    public string TablaAfectada { get; set; } = null!;

    [StringLength(100)]
    public string RegistroAfectado { get; set; } = null!;

    public string? ValorAnterior { get; set; }

    public string? ValorNuevo { get; set; }

    [Column("DireccionIP")]
    [StringLength(45)]
    public string? DireccionIp { get; set; }

    [StringLength(150)]
    public string? Dispositivo { get; set; }

    public DateTime Fecha { get; set; }

    [ForeignKey("IdUsuario")]
    [InverseProperty("Bitacoras")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
