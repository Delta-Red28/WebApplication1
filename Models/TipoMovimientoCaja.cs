using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("TipoMovimientoCaja")]
[Index("Nombre", Name = "UQ_TipoMovimiento", IsUnique = true)]
public partial class TipoMovimientoCaja
{
    // ============================================================
    // CLAVE PRINCIPAL
    // ============================================================

    [Key]
    public int IdTipoMovimiento { get; set; }


    // ============================================================
    // NOMBRE
    // ============================================================
    //
    // Ejemplos:
    //
    // Ingreso
    // Egreso
    //
    // El nombre debe ser único.
    //
    // ============================================================

    [Required]
    [StringLength(40)]
    public string Nombre { get; set; } = null!;


    // ============================================================
    // ESTADO
    // ============================================================
    //
    // true  = activo
    // false = inactivo
    //
    // Un tipo inactivo no debe utilizarse para crear nuevos
    // movimientos de caja.
    //
    // ============================================================

    [Required]
    public bool Estado { get; set; }


    // ============================================================
    // MOVIMIENTOS DE CAJA
    // ============================================================

    [InverseProperty(nameof(MovimientoCaja.IdTipoMovimientoNavigation))]
    public virtual ICollection<MovimientoCaja> MovimientoCajas { get; set; }
        = new List<MovimientoCaja>();
}