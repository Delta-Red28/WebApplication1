using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("CategoriaInsumo")]
[Index("Nombre", Name = "IX_CategoriaInsumo_Nombre")]
[Index("Nombre", Name = "UQ_CategoriaInsumo", IsUnique = true)]
public partial class CategoriaInsumo
{

    [Key]
    public int IdCategoriaInsumo { get; set; }


    [Required(ErrorMessage = "El nombre de la categoría de insumo es obligatorio.")]
    [StringLength(
        80,
        ErrorMessage = "El nombre de la categoría no puede superar los 80 caracteres."
    )]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = null!;


    [StringLength(
        250,
        ErrorMessage = "La descripción no puede superar los 250 caracteres."
    )]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }


    [Display(Name = "Estado")]
    public bool Estado { get; set; }


    [Display(Name = "Fecha de registro")]
    public DateTime FechaRegistro { get; set; }


    [InverseProperty("IdCategoriaInsumoNavigation")]
    public virtual ICollection<Insumo> Insumos { get; set; }
        = new List<Insumo>();
}