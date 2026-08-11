using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("TransferenciaInventario")]
public partial class TransferenciaInventario
{
    [Key]
    public int IdTransferencia { get; set; }

    public int IdUsuario { get; set; }

    public int IdEstadoTransferencia { get; set; }

    public int IdUbicacionOrigen { get; set; }

    public int IdUbicacionDestino { get; set; }

    public DateTime FechaTransferencia { get; set; }

    [StringLength(300)]
    public string? Observacion { get; set; }

    [InverseProperty("IdTransferenciaNavigation")]
    public virtual ICollection<DetalleTransferencium> DetalleTransferencia { get; set; } = new List<DetalleTransferencium>();

    [ForeignKey("IdEstadoTransferencia")]
    [InverseProperty("TransferenciaInventarios")]
    public virtual EstadoTransferencium IdEstadoTransferenciaNavigation { get; set; } = null!;

    [ForeignKey("IdUbicacionDestino")]
    [InverseProperty("TransferenciaInventarioIdUbicacionDestinoNavigations")]
    public virtual UbicacionInventario IdUbicacionDestinoNavigation { get; set; } = null!;

    [ForeignKey("IdUbicacionOrigen")]
    [InverseProperty("TransferenciaInventarioIdUbicacionOrigenNavigations")]
    public virtual UbicacionInventario IdUbicacionOrigenNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("TransferenciaInventarios")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
