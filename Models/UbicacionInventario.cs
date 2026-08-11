using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("UbicacionInventario")]
[Index("Nombre", Name = "UQ_UbicacionInventario", IsUnique = true)]
public partial class UbicacionInventario
{
    [Key]
    public int IdUbicacion { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("IdUbicacionNavigation")]
    public virtual ICollection<DetalleAjuste> DetalleAjustes { get; set; } = new List<DetalleAjuste>();

    [InverseProperty("IdUbicacionNavigation")]
    public virtual ICollection<Existencium> Existencia { get; set; } = new List<Existencium>();

    [InverseProperty("IdUbicacionNavigation")]
    public virtual ICollection<LoteInsumo> LoteInsumos { get; set; } = new List<LoteInsumo>();

    [InverseProperty("IdUbicacionNavigation")]
    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();

    [InverseProperty("IdUbicacionDestinoNavigation")]
    public virtual ICollection<TransferenciaInventario> TransferenciaInventarioIdUbicacionDestinoNavigations { get; set; } = new List<TransferenciaInventario>();

    [InverseProperty("IdUbicacionOrigenNavigation")]
    public virtual ICollection<TransferenciaInventario> TransferenciaInventarioIdUbicacionOrigenNavigations { get; set; } = new List<TransferenciaInventario>();
}
