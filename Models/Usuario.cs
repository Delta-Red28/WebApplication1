using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Usuario")]
[Index("Correo", Name = "UQ__Usuario__60695A19BB5DC0A7", IsUnique = true)]
[Index("Usuario1", Name = "UQ__Usuario__E3237CF72AA74308", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    [StringLength(80)]
    public string Nombres { get; set; } = null!;

    [StringLength(80)]
    public string Apellidos { get; set; } = null!;

    [StringLength(150)]
    public string Correo { get; set; } = null!;

    [Column("Usuario")]
    [StringLength(50)]
    public string Usuario1 { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(20)]
    public string? Telefono { get; set; }

    [StringLength(250)]
    public string? Foto { get; set; }

    public bool Estado { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int IntentosFallidos { get; set; }

    public bool DebeCambiarPassword { get; set; }

    public DateTime? FechaCambioPassword { get; set; }

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<AjusteInventario> AjusteInventarios { get; set; } = new List<AjusteInventario>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Bitacora> Bitacoras { get; set; } = new List<Bitacora>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    [InverseProperty("IdUsuarioAnuloNavigation")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<HistorialFactura> HistorialFacturas { get; set; } = new List<HistorialFactura>();

    [ForeignKey("IdRol")]
    [InverseProperty("Usuarios")]
    public virtual Rol IdRolNavigation { get; set; } = null!;

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Merma> Mermas { get; set; } = new List<Merma>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<SeguimientoCompra> SeguimientoCompras { get; set; } = new List<SeguimientoCompra>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<TransferenciaInventario> TransferenciaInventarios { get; set; } = new List<TransferenciaInventario>();
}
