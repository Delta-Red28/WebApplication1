using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Cliente")]
[Index("Estado", "Nombres", "Apellidos", Name = "IX_Cliente_Estado_Nombres")]
[Index("Nombres", "Apellidos", Name = "IX_Cliente_Nombres")]
[Index("Telefono", Name = "IX_Cliente_Telefono")]
[Index("Cedula", Name = "UQ_Cliente_Cedula", IsUnique = true)]
[Index("Correo", Name = "UQ_Cliente_Correo", IsUnique = true)]
public partial class Cliente
{
    [Key]
    public int IdCliente { get; set; }

    [StringLength(80)]
    public string Nombres { get; set; } = null!;

    [StringLength(80)]
    public string Apellidos { get; set; } = null!;

    [StringLength(20)]
    public string? Cedula { get; set; }

    [StringLength(20)]
    public string Telefono { get; set; } = null!;

    [StringLength(150)]
    public string? Correo { get; set; }

    [StringLength(250)]
    public string? Direccion { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? Sexo { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<Reservacion> Reservacions { get; set; } = new List<Reservacion>();
}