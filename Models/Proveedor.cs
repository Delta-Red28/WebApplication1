using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Proveedor")]
[Index("Nombre", Name = "IX_Proveedor_Nombre")]
[Index("Ruc", Name = "IX_Proveedor_RUC")]
[Index("CodigoProveedor", Name = "UQ_Proveedor_Codigo", IsUnique = true)]
public partial class Proveedor
{
    [Key]
    public int IdProveedor { get; set; }

    public int IdEstadoProveedor { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string CodigoProveedor { get; set; } = null!;

    [StringLength(150)]
    public string Nombre { get; set; } = null!;

    [StringLength(150)]
    public string? NombreComercial { get; set; }

    [Column("RUC")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Ruc { get; set; }

    [StringLength(120)]
    public string? Contacto { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? Telefono { get; set; }

    [StringLength(120)]
    public string? Correo { get; set; }

    [StringLength(250)]
    public string? Direccion { get; set; }

    [StringLength(100)]
    public string? Ciudad { get; set; }

    [StringLength(100)]
    public string? Pais { get; set; }

    [StringLength(300)]
    public string? Observacion { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("IdProveedorNavigation")]
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    [ForeignKey("IdEstadoProveedor")]
    [InverseProperty("Proveedors")]
    public virtual EstadoProveedor IdEstadoProveedorNavigation { get; set; } = null!;
}
