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

    [Required]
    public int IdCaja { get; set; }


    // ============================================================
    // TIPO DE MOVIMIENTO
    // ============================================================

    [Required]
    public int IdTipoMovimiento { get; set; }


    // ============================================================
    // PAGO
    // ============================================================
    //
    // NULL = movimiento manual
    //
    // CON VALOR = movimiento generado por un Pago
    //
    // Regla:
    //
    // Un pago que afecta caja debe tener exactamente un
    // MovimientoCaja asociado.
    //
    // Los movimientos manuales mantienen IdPago = null.
    //
    // ============================================================

    public int? IdPago { get; set; }


    // ============================================================
    // MONEDA
    // ============================================================

    [Required]
    public int? IdMoneda { get; set; }


    // ============================================================
    // MONTO
    // ============================================================

    [Required]
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

    [Required]
    public DateTime FechaMovimiento { get; set; }


    // ============================================================
    // ANULACIÓN
    // ============================================================
    //
    // IMPORTANTE:
    //
    // Un movimiento NO se elimina físicamente.
    //
    // Cuando se anula:
    //
    //     Anulado = true
    //
    // Y desde ese momento:
    //
    //     NO afecta los totales de caja.
    //
    // ============================================================

    public bool Anulado { get; set; }


    /// <summary>
    /// Fecha y hora en que se anuló el movimiento.
    /// </summary>
    public DateTime? FechaAnulacion { get; set; }


    /// <summary>
    /// Usuario que realizó la anulación.
    /// </summary>
    public int? IdUsuarioAnulo { get; set; }


    /// <summary>
    /// Motivo de la anulación.
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