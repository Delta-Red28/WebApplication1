using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

[Table("MovimientoCaja")]
public partial class MovimientoCaja
{
    // ============================================================
    // CLAVE PRINCIPAL
    // ============================================================

    [Key]
    public int IdMovimientoCaja { get; set; }


    // ============================================================
    // CAJA
    // ============================================================

    public int IdCaja { get; set; }


    // ============================================================
    // TIPO DE MOVIMIENTO
    // ============================================================

    public int IdTipoMovimiento { get; set; }


    // ============================================================
    // PAGO
    // ============================================================

    public int? IdPago { get; set; }


    // ============================================================
    // MONEDA
    // ============================================================

    public int? IdMoneda { get; set; }


    // ============================================================
    // MONTO
    // ============================================================

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Monto { get; set; }


    // ============================================================
    // DESCRIPCIÓN
    // ============================================================

    [StringLength(500)]
    public string? Descripcion { get; set; }


    // ============================================================
    // FECHA DEL MOVIMIENTO
    // ============================================================

    public DateTime FechaMovimiento { get; set; }


    // ============================================================
    // ANULACIÓN
    // ============================================================

    /// <summary>
    /// Indica si el movimiento fue anulado.
    /// Un movimiento anulado NO debe afectar los totales de caja.
    /// </summary>
    public bool Anulado { get; set; }


    /// <summary>
    /// Fecha y hora en que se realizó la anulación.
    /// </summary>
    public DateTime? FechaAnulacion { get; set; }


    /// <summary>
    /// Usuario que realizó la anulación.
    /// </summary>
    public int? IdUsuarioAnulo { get; set; }


    /// <summary>
    /// Motivo por el cual se anuló el movimiento.
    /// </summary>
    [StringLength(500)]
    public string? MotivoAnulacion { get; set; }


    // ============================================================
    // NAVEGACIÓN HACIA CAJA
    // ============================================================

    [ForeignKey(nameof(IdCaja))]
    public virtual Caja? IdCajaNavigation { get; set; }


    // ============================================================
    // NAVEGACIÓN HACIA TIPO DE MOVIMIENTO
    // ============================================================

    [ForeignKey(nameof(IdTipoMovimiento))]
    public virtual TipoMovimientoCaja? IdTipoMovimientoNavigation { get; set; }


    // ============================================================
    // NAVEGACIÓN HACIA PAGO
    // ============================================================

    [ForeignKey(nameof(IdPago))]
    public virtual Pago? IdPagoNavigation { get; set; }


    // ============================================================
    // NAVEGACIÓN HACIA MONEDA
    // ============================================================

    [ForeignKey(nameof(IdMoneda))]
    public virtual Monedum? IdMonedaNavigation { get; set; }


    // ============================================================
    // NAVEGACIÓN HACIA USUARIO QUE ANULÓ
    // ============================================================

    [ForeignKey(nameof(IdUsuarioAnulo))]
    public virtual Usuario? IdUsuarioAnuloNavigation { get; set; }
}