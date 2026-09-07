using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class RestauranteContext : DbContext
{
    public RestauranteContext()
    {
    }

    public RestauranteContext(DbContextOptions<RestauranteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AjusteInventario> AjusteInventarios { get; set; }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<Banco> Bancos { get; set; }

    public virtual DbSet<Bitacora> Bitacoras { get; set; }

    public virtual DbSet<Caja> Cajas { get; set; }

    public virtual DbSet<CategoriaInsumo> CategoriaInsumos { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<ConversionUnidad> ConversionUnidads { get; set; }

    public virtual DbSet<DetalleAjuste> DetalleAjustes { get; set; }

    public virtual DbSet<DetalleCompra> DetalleCompras { get; set; }

    public virtual DbSet<DetalleMerma> DetalleMermas { get; set; }

    public virtual DbSet<DetallePedido> DetallePedidos { get; set; }

    public virtual DbSet<DetalleRecetum> DetalleReceta { get; set; }

    public virtual DbSet<DetalleTransferencium> DetalleTransferencia { get; set; }

    public virtual DbSet<EstadoCaja> EstadoCajas { get; set; }

    public virtual DbSet<EstadoCompra> EstadoCompras { get; set; }

    public virtual DbSet<EstadoFactura> EstadoFacturas { get; set; }

    public virtual DbSet<EstadoInsumo> EstadoInsumos { get; set; }

    public virtual DbSet<EstadoLote> EstadoLotes { get; set; }

    public virtual DbSet<EstadoMesa> EstadoMesas { get; set; }

    public virtual DbSet<EstadoPago> EstadoPagos { get; set; }

    public virtual DbSet<EstadoPedido> EstadoPedidos { get; set; }

    public virtual DbSet<EstadoProducto> EstadoProductos { get; set; }

    public virtual DbSet<EstadoProveedor> EstadoProveedors { get; set; }

    public virtual DbSet<EstadoRecetum> EstadoReceta { get; set; }

    public virtual DbSet<EstadoReservacion> EstadoReservacions { get; set; }

    public virtual DbSet<EstadoTransferencium> EstadoTransferencia { get; set; }

    public virtual DbSet<Existencium> Existencia { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<HistorialFactura> HistorialFacturas { get; set; }

    public virtual DbSet<Impuesto> Impuestos { get; set; }

    public virtual DbSet<Insumo> Insumos { get; set; }

    public virtual DbSet<LoteInsumo> LoteInsumos { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<Merma> Mermas { get; set; }

    public virtual DbSet<Mesa> Mesas { get; set; }

    public virtual DbSet<MetodoPago> MetodoPagos { get; set; }

    public virtual DbSet<Monedum> Moneda { get; set; }

    public virtual DbSet<MotivoAnulacion> MotivoAnulacions { get; set; }

    public virtual DbSet<MotivoMerma> MotivoMermas { get; set; }

    public virtual DbSet<MovimientoCaja> MovimientoCajas { get; set; }

    public virtual DbSet<MovimientoInventario> MovimientoInventarios { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Recetum> Receta { get; set; }

    public virtual DbSet<Reservacion> Reservacions { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<SeguimientoCompra> SeguimientoCompras { get; set; }

    public virtual DbSet<SerieFactura> SerieFacturas { get; set; }

    public virtual DbSet<TipoComprobante> TipoComprobantes { get; set; }

    public virtual DbSet<TipoMovimientoCaja> TipoMovimientoCajas { get; set; }

    public virtual DbSet<TipoMovimientoInventario> TipoMovimientoInventarios { get; set; }

    public virtual DbSet<TipoPedido> TipoPedidos { get; set; }

    public virtual DbSet<TipoTarjetum> TipoTarjeta { get; set; }

    public virtual DbSet<TransferenciaInventario> TransferenciaInventarios { get; set; }

    public virtual DbSet<UbicacionInventario> UbicacionInventarios { get; set; }

    public virtual DbSet<UnidadMedidum> UnidadMedida { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AjusteInventario>(entity =>
        {
            entity.HasKey(e => e.IdAjuste)
                .HasName("PK__AjusteIn__138C27587A8F811E");

            entity.Property(e => e.FechaAjuste)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.AjusteInventarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ajuste_Usuario");
        });


        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.IdAuditoria)
                .HasName("PK__Auditori__7FD13FA0B6F03F4E");

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())", "DF_Auditoria_FechaRegistro");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.Auditoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_Usuario");
        });


        modelBuilder.Entity<Banco>(entity =>
        {
            entity.HasKey(e => e.IdBanco)
                .HasName("PK__Banco__2D3F553E4ADA24B7");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())", "DF_Banco_FechaRegistro");
        });


        modelBuilder.Entity<Bitacora>(entity =>
        {
            entity.HasKey(e => e.IdBitacora)
                .HasName("PK__Bitacora__ED3A1B1332C5B284");

            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.Bitacoras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bitacora_Usuario");
        });


        // ============================================================
        // CAJA
        // ============================================================
        // La tabla Caja tiene el trigger:
        //
        // TR_Caja_MaximoTresAbiertas
        //
        // Por eso EF Core no debe utilizar OUTPUT al guardar cambios.
        // ============================================================

        modelBuilder.Entity<Caja>(entity =>
        {
            entity.HasKey(e => e.IdCaja)
                .HasName("PK__Caja__3B7BF2C5FE28257D");

            entity.ToTable("Caja", tb =>
            {
                tb.HasTrigger("TR_Caja_MaximoTresAbiertas");
                tb.UseSqlOutputClause(false);
            });

            entity.Property(e => e.FechaApertura)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdEstadoCajaNavigation)
                .WithMany(p => p.Cajas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Caja_Estado");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.Cajas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Caja_Usuario");
        });


        modelBuilder.Entity<CategoriaInsumo>(entity =>
        {
            entity.HasKey(e => e.IdCategoriaInsumo)
                .HasName("PK__Categori__EE8BC207437D2EEE");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");
        });


        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria)
                .HasName("PK__Categori__A3C02A10C8881049");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");
        });


        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente)
                .HasName("PK__Cliente__D594664244295BB6");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Sexo)
                .IsFixedLength();
        });


        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra)
                .HasName("PK__Compra__0A5CDB5C29391D1B");

            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdEstadoCompraNavigation)
                .WithMany(p => p.Compras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_Estado");

            entity.HasOne(d => d.IdMonedaNavigation)
                .WithMany(p => p.Compras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_Moneda");

            entity.HasOne(d => d.IdProveedorNavigation)
                .WithMany(p => p.Compras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_Proveedor");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.Compras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_Usuario");
        });


        modelBuilder.Entity<ConversionUnidad>(entity =>
        {
            entity.HasKey(e => e.IdConversionUnidad)
                .HasName("PK__Conversi__362C9B6ABE640D42");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdUnidadDestinoNavigation)
                .WithMany(p => p.ConversionUnidadIdUnidadDestinoNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversion_Destino");

            entity.HasOne(d => d.IdUnidadOrigenNavigation)
                .WithMany(p => p.ConversionUnidadIdUnidadOrigenNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversion_Origen");
        });


        modelBuilder.Entity<DetalleAjuste>(entity =>
        {
            entity.HasKey(e => e.IdDetalleAjuste)
                .HasName("PK__DetalleA__BEEE31FE0B301FD9");

            entity.HasOne(d => d.IdAjusteNavigation)
                .WithMany(p => p.DetalleAjustes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleAjuste_Ajuste");

            entity.HasOne(d => d.IdInsumoNavigation)
                .WithMany(p => p.DetalleAjustes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleAjuste_Insumo");

            entity.HasOne(d => d.IdUbicacionNavigation)
                .WithMany(p => p.DetalleAjustes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleAjuste_Ubicacion");
        });


        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCompra)
                .HasName("PK__DetalleC__E046CCBB8DD8783D");

            entity.HasOne(d => d.IdCompraNavigation)
                .WithMany(p => p.DetalleCompras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompra_Compra");

            entity.HasOne(d => d.IdInsumoNavigation)
                .WithMany(p => p.DetalleCompras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompra_Insumo");
        });


        modelBuilder.Entity<DetalleMerma>(entity =>
        {
            entity.HasKey(e => e.IdDetalleMerma)
                .HasName("PK__DetalleM__8FC3DA5547F338D2");

            entity.HasOne(d => d.IdInsumoNavigation)
                .WithMany(p => p.DetalleMermas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleMerma_Insumo");

            entity.HasOne(d => d.IdLoteNavigation)
                .WithMany(p => p.DetalleMermas)
                .HasConstraintName("FK_DetalleMerma_Lote");

            entity.HasOne(d => d.IdMermaNavigation)
                .WithMany(p => p.DetalleMermas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleMerma_Merma");

            entity.HasOne(d => d.IdMotivoMermaNavigation)
                .WithMany(p => p.DetalleMermas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleMerma_Motivo");
        });


        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(e => e.IdDetallePedido)
                .HasName("PK__DetalleP__48AFFD9584F33B4C");

            entity.HasOne(d => d.IdPedidoNavigation)
                .WithMany(p => p.DetallePedidos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_Pedido");

            entity.HasOne(d => d.IdProductoNavigation)
                .WithMany(p => p.DetallePedidos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePedido_Producto");
        });


        modelBuilder.Entity<DetalleRecetum>(entity =>
        {
            entity.HasKey(e => e.IdDetalleReceta)
                .HasName("PK__DetalleR__BAA548713ED34823");

            entity.HasOne(d => d.IdInsumoNavigation)
                .WithMany(p => p.DetalleReceta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleReceta_Insumo");

            entity.HasOne(d => d.IdRecetaNavigation)
                .WithMany(p => p.DetalleReceta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleReceta_Receta");

            entity.HasOne(d => d.IdUnidadMedidaNavigation)
                .WithMany(p => p.DetalleReceta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleReceta_Unidad");
        });


        modelBuilder.Entity<DetalleTransferencium>(entity =>
        {
            entity.HasKey(e => e.IdDetalleTransferencia)
                .HasName("PK__DetalleT__FC34AAF8215F6E85");

            entity.HasOne(d => d.IdInsumoNavigation)
                .WithMany(p => p.DetalleTransferencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleTransferencia_Insumo");

            entity.HasOne(d => d.IdLoteNavigation)
                .WithMany(p => p.DetalleTransferencia)
                .HasConstraintName("FK_DetalleTransferencia_Lote");

            entity.HasOne(d => d.IdTransferenciaNavigation)
                .WithMany(p => p.DetalleTransferencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleTransferencia_Transferencia");
        });


        modelBuilder.Entity<EstadoCaja>(entity =>
        {
            entity.HasKey(e => e.IdEstadoCaja)
                .HasName("PK__EstadoCa__D80D5A5D7A516660");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<EstadoCompra>(entity =>
        {
            entity.HasKey(e => e.IdEstadoCompra)
                .HasName("PK__EstadoCo__0226D5A3257B673E");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<EstadoFactura>(entity =>
        {
            entity.HasKey(e => e.IdEstadoFactura)
                .HasName("PK__EstadoFa__D1BC26E77D2F3230");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<EstadoInsumo>(entity =>
        {
            entity.HasKey(e => e.IdEstadoInsumo)
                .HasName("PK__EstadoIn__8F2F8B722673C42E");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<EstadoLote>(entity =>
        {
            entity.HasKey(e => e.IdEstadoLote)
                .HasName("PK__EstadoLo__5E7FA81667E121C2");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<EstadoMesa>(entity =>
        {
            entity.HasKey(e => e.IdEstadoMesa)
                .HasName("PK__EstadoMe__49528404F66F3221");
        });


        modelBuilder.Entity<EstadoPago>(entity =>
        {
            entity.HasKey(e => e.IdEstadoPago)
                .HasName("PK__EstadoPa__540F03E90B6CA186");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<EstadoPedido>(entity =>
        {
            entity.HasKey(e => e.IdEstadoPedido)
                .HasName("PK__EstadoPe__86B983711CA8E72D");
        });


        modelBuilder.Entity<EstadoProducto>(entity =>
        {
            entity.HasKey(e => e.IdEstadoProducto)
                .HasName("PK__EstadoPr__F02FAF372C26323B");
        });


        modelBuilder.Entity<EstadoProveedor>(entity =>
        {
            entity.HasKey(e => e.IdEstadoProveedor)
                .HasName("PK__EstadoPr__F4B1428E4878AF0B");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<EstadoRecetum>(entity =>
        {
            entity.HasKey(e => e.IdEstadoReceta)
                .HasName("PK__EstadoRe__91880480369BCE4C");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<EstadoReservacion>(entity =>
        {
            entity.HasKey(e => e.IdEstadoReservacion)
                .HasName("PK__EstadoRe__253A80DAF98350D8");
        });


        modelBuilder.Entity<EstadoTransferencium>(entity =>
        {
            entity.HasKey(e => e.IdEstadoTransferencia)
                .HasName("PK__EstadoTr__E72EDF36A3775432");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<Existencium>(entity =>
        {
            entity.HasKey(e => e.IdExistencia)
                .HasName("PK__Existenc__28AEDEA02343E4E8");

            entity.Property(e => e.UltimaActualizacion)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdInsumoNavigation)
                .WithMany(p => p.Existencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Existencia_Insumo");

            entity.HasOne(d => d.IdUbicacionNavigation)
                .WithMany(p => p.Existencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Existencia_Ubicacion");
        });


        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.IdFactura)
                .HasName("PK__Factura__50E7BAF19E45EC0C");

            // Número de factura único
            entity.HasIndex(e => e.NumeroFactura)
                .IsUnique();

            entity.Property(e => e.FechaFactura)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.IdSerieFactura)
                .HasDefaultValue(1);

            entity.Property(e => e.IdTipoComprobante)
                .HasDefaultValue(1);

            entity.HasOne(d => d.IdEstadoFacturaNavigation)
                .WithMany(p => p.Facturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_Estado");

            entity.HasOne(d => d.IdImpuestoNavigation)
                .WithMany(p => p.Facturas)
                .HasConstraintName("FK_Factura_Impuesto");

            entity.HasOne(d => d.IdMonedaNavigation)
                .WithMany(p => p.Facturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_Moneda");

            entity.HasOne(d => d.IdMotivoAnulacionNavigation)
                .WithMany(p => p.Facturas)
                .HasConstraintName("FK_Factura_MotivoAnulacion");

            entity.HasOne(d => d.IdPedidoNavigation)
                .WithOne(p => p.Factura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_Pedido");

            entity.HasOne(d => d.IdSerieFacturaNavigation)
                .WithMany(p => p.Facturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_SerieFactura");

            entity.HasOne(d => d.IdTipoComprobanteNavigation)
                .WithMany(p => p.Facturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factura_TipoComprobante");

            entity.HasOne(d => d.IdUsuarioAnuloNavigation)
                .WithMany(p => p.Facturas)
                .HasConstraintName("FK_Factura_UsuarioAnulo");
        });


        modelBuilder.Entity<HistorialFactura>(entity =>
        {
            entity.HasKey(e => e.IdHistorialFactura)
                .HasName("PK__Historia__82F4158A1C4868C1");

            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdEstadoFacturaNavigation)
                .WithMany(p => p.HistorialFacturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialFactura_Estado");

            entity.HasOne(d => d.IdFacturaNavigation)
                .WithMany(p => p.HistorialFacturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialFactura_Factura");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.HistorialFacturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistorialFactura_Usuario");
        });


        modelBuilder.Entity<Impuesto>(entity =>
        {
            entity.HasKey(e => e.IdImpuesto)
                .HasName("PK__Impuesto__A9B88928C314FA28");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");
        });


        modelBuilder.Entity<Insumo>(entity =>
        {
            entity.HasKey(e => e.IdInsumo)
                .HasName("PK__Insumo__F378A2AF2A25A7A7");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdCategoriaInsumoNavigation)
                .WithMany(p => p.Insumos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Insumo_Categoria");

            entity.HasOne(d => d.IdEstadoInsumoNavigation)
                .WithMany(p => p.Insumos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Insumo_Estado");

            entity.HasOne(d => d.IdMarcaNavigation)
                .WithMany(p => p.Insumos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Insumo_Marca");

            entity.HasOne(d => d.IdUnidadBaseNavigation)
                .WithMany(p => p.InsumoIdUnidadBaseNavigations)
                .HasConstraintName("FK_Insumo_UnidadBase");

            entity.HasOne(d => d.IdUnidadMedidaNavigation)
                .WithMany(p => p.InsumoIdUnidadMedidaNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Insumo_Unidad");
        });


        modelBuilder.Entity<LoteInsumo>(entity =>
        {
            entity.HasKey(e => e.IdLote)
                .HasName("PK__LoteInsu__38C4EE904ACF8DD8");

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdDetalleCompraNavigation)
                .WithMany(p => p.LoteInsumos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lote_DetalleCompra");

            entity.HasOne(d => d.IdEstadoLoteNavigation)
                .WithMany(p => p.LoteInsumos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lote_Estado");

            entity.HasOne(d => d.IdInsumoNavigation)
                .WithMany(p => p.LoteInsumos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lote_Insumo");

            entity.HasOne(d => d.IdUbicacionNavigation)
                .WithMany(p => p.LoteInsumos)
                .HasConstraintName("FK_Lote_Ubicacion");
        });


        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.IdMarca)
                .HasName("PK__Marca__4076A887C88A52F5");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");
        });


        modelBuilder.Entity<Merma>(entity =>
        {
            entity.HasKey(e => e.IdMerma)
                .HasName("PK__Merma__FA054FC64EC46172");

            entity.Property(e => e.FechaMerma)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.Mermas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Merma_Usuario");
        });


        modelBuilder.Entity<Mesa>(entity =>
        {
            entity.HasKey(e => e.IdMesa)
                .HasName("PK__Mesa__4D7E81B194A92654");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.HasOne(d => d.IdEstadoMesaNavigation)
                .WithMany(p => p.Mesas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mesa_EstadoMesa");
        });


        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago)
                .HasName("PK__MetodoPa__6F49A9BE685284ED");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<Monedum>(entity =>
        {
            entity.HasKey(e => e.IdMoneda)
                .HasName("PK__Moneda__AA690671BFB2D202");

            entity.Property(e => e.CodigoIso)
                .IsFixedLength();

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");
        });


        modelBuilder.Entity<MotivoAnulacion>(entity =>
        {
            entity.HasKey(e => e.IdMotivoAnulacion)
                .HasName("PK__MotivoAn__8942B2002EB4E932");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<MotivoMerma>(entity =>
        {
            entity.HasKey(e => e.IdMotivoMerma)
                .HasName("PK__MotivoMe__39C2E1E3EA6DC597");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<MovimientoCaja>(entity =>
        {
            entity.HasKey(e => e.IdMovimientoCaja)
                .HasName("PK__Movimien__D126CD2DD937B2F8");

            entity.Property(e => e.FechaMovimiento)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdCajaNavigation)
                .WithMany(p => p.MovimientoCajas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoCaja_Caja");

            entity.HasOne(d => d.IdPagoNavigation)
                .WithMany(p => p.MovimientoCajas)
                .HasConstraintName("FK_MovimientoCaja_Pago");

            entity.HasOne(d => d.IdTipoMovimientoNavigation)
                .WithMany(p => p.MovimientoCajas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoCaja_Tipo");
        });


        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.IdMovimientoInventario)
                .HasName("PK__Movimien__F0AB878BD186E5AA");

            entity.Property(e => e.FechaMovimiento)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdInsumoNavigation)
                .WithMany(p => p.MovimientoInventarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoInventario_Insumo");

            entity.HasOne(d => d.IdLoteNavigation)
                .WithMany(p => p.MovimientoInventarios)
                .HasConstraintName("FK_MovimientoInventario_Lote");

            entity.HasOne(d => d.IdTipoMovimientoInventarioNavigation)
                .WithMany(p => p.MovimientoInventarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoInventario_Tipo");

            entity.HasOne(d => d.IdUbicacionNavigation)
                .WithMany(p => p.MovimientoInventarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoInventario_Ubicacion");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.MovimientoInventarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoInventario_Usuario");
        });


        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.IdPago)
                .HasName("PK__Pago__FC851A3A07383BBB");

            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.IdEstadoPago)
                .HasDefaultValue(2);

            entity.HasOne(d => d.IdBancoNavigation)
                .WithMany(p => p.Pagos)
                .HasConstraintName("FK_Pago_Banco");

            entity.HasOne(d => d.IdEstadoPagoNavigation)
                .WithMany(p => p.Pagos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_EstadoPago");

            entity.HasOne(d => d.IdFacturaNavigation)
                .WithMany(p => p.Pagos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_Factura");

            entity.HasOne(d => d.IdMetodoPagoNavigation)
                .WithMany(p => p.Pagos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_MetodoPago");

            entity.HasOne(d => d.IdMonedaNavigation)
                .WithMany(p => p.Pagos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_Moneda");

            entity.HasOne(d => d.IdTipoTarjetaNavigation)
                .WithMany(p => p.Pagos)
                .HasConstraintName("FK_Pago_TipoTarjeta");
        });


        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.IdPedido)
                .HasName("PK__Pedido__9D335DC30F1CB932");

            entity.Property(e => e.FechaPedido)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdClienteNavigation)
                .WithMany(p => p.Pedidos)
                .HasConstraintName("FK_Pedido_Cliente");

            entity.HasOne(d => d.IdEstadoPedidoNavigation)
                .WithMany(p => p.Pedidos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedido_EstadoPedido");

            entity.HasOne(d => d.IdMesaNavigation)
                .WithMany(p => p.Pedidos)
                .HasConstraintName("FK_Pedido_Mesa");

            entity.HasOne(d => d.IdTipoPedidoNavigation)
                .WithMany(p => p.Pedidos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedido_TipoPedido");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.Pedidos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedido_Usuario");
        });


        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.IdPermiso)
                .HasName("PK__Permiso__0D626EC834C9BA7B");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto)
                .HasName("PK__Producto__098892106DC00066");

            entity.Property(e => e.ControlaInventario)
                .HasDefaultValue(true);

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdCategoriaNavigation)
                .WithMany(p => p.Productos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Producto_Categoria");

            entity.HasOne(d => d.IdEstadoProductoNavigation)
                .WithMany(p => p.Productos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Producto_Estado");
        });


        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor)
                .HasName("PK__Proveedo__E8B631AF793C8BF3");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Pais)
                .HasDefaultValue("Nicaragua");

            entity.HasOne(d => d.IdEstadoProveedorNavigation)
                .WithMany(p => p.Proveedors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Proveedor_Estado");
        });


        modelBuilder.Entity<Recetum>(entity =>
        {
            entity.HasKey(e => e.IdReceta)
                .HasName("PK__Receta__2CEFF1572ECC0823");

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Rendimiento)
                .HasDefaultValue(1m);

            entity.Property(e => e.Version)
                .HasDefaultValue((short)1);

            entity.HasOne(d => d.IdEstadoRecetaNavigation)
                .WithMany(p => p.Receta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Receta_Estado");

            entity.HasOne(d => d.IdProductoNavigation)
                .WithOne(p => p.Recetum)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Receta_Producto");
        });


        // ============================================================
        // RESERVACION
        // ============================================================
        // La tabla Reservacion tiene el trigger:
        //
        // TR_Reservacion_NoDuplicarMesa
        //
        // Se desactiva OUTPUT para evitar el conflicto con SQL Server.
        // ============================================================

        modelBuilder.Entity<Reservacion>(entity =>
        {
            entity.HasKey(e => e.IdReservacion)
                .HasName("PK__Reservac__528246375CA7832E");

            entity.ToTable("Reservacion", tb =>
            {
                tb.HasTrigger("TR_Reservacion_NoDuplicarMesa");
                tb.UseSqlOutputClause(false);
            });

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdClienteNavigation)
                .WithMany(p => p.Reservacions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reservacion_Cliente");

            entity.HasOne(d => d.IdEstadoReservacionNavigation)
                .WithMany(p => p.Reservacions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reservacion_Estado");

            entity.HasOne(d => d.IdMesaNavigation)
                .WithMany(p => p.Reservacions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reservacion_Mesa");
        });


        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol)
                .HasName("PK__Rol__2A49584C5B9888D8");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.HasMany(d => d.IdPermisos)
                .WithMany(p => p.IdRols)
                .UsingEntity<Dictionary<string, object>>(
                    "RolPermiso",
                    r => r.HasOne<Permiso>()
                        .WithMany()
                        .HasForeignKey("IdPermiso")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolPermiso_Permiso"),
                    l => l.HasOne<Rol>()
                        .WithMany()
                        .HasForeignKey("IdRol")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolPermiso_Rol"),
                    j =>
                    {
                        j.HasKey("IdRol", "IdPermiso")
                            .HasName("PK__RolPermi__BA9F7EA0CC8C899E");

                        j.ToTable("RolPermiso");
                    });
        });


        modelBuilder.Entity<SeguimientoCompra>(entity =>
        {
            entity.HasKey(e => e.IdSeguimientoCompra)
                .HasName("PK__Seguimie__451313C5FF621F96");

            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdCompraNavigation)
                .WithMany(p => p.SeguimientoCompras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeguimientoCompra_Compra");

            entity.HasOne(d => d.IdEstadoCompraNavigation)
                .WithMany(p => p.SeguimientoCompras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeguimientoCompra_EstadoCompra");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.SeguimientoCompras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeguimientoCompra_Usuario");
        });


        modelBuilder.Entity<SerieFactura>(entity =>
        {
            entity.HasKey(e => e.IdSerieFactura)
                .HasName("PK__SerieFac__27D0664F5798203D");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.LongitudNumero)
                .HasDefaultValue((byte)6);
        });


        modelBuilder.Entity<TipoComprobante>(entity =>
        {
            entity.HasKey(e => e.IdTipoComprobante)
                .HasName("PK__TipoComp__F8411DEB5A924F89");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<TipoMovimientoCaja>(entity =>
        {
            entity.HasKey(e => e.IdTipoMovimiento)
                .HasName("PK__TipoMovi__820D7FC2CCA010E2");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<TipoMovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.IdTipoMovimientoInventario)
                .HasName("PK__TipoMovi__C942FB34F689C7A9");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<TipoPedido>(entity =>
        {
            entity.HasKey(e => e.IdTipoPedido)
                .HasName("PK__TipoPedi__4B4A7895C721C181");
        });


        modelBuilder.Entity<TipoTarjetum>(entity =>
        {
            entity.HasKey(e => e.IdTipoTarjeta)
                .HasName("PK__TipoTarj__D4F97648BDF94059");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<TransferenciaInventario>(entity =>
        {
            entity.HasKey(e => e.IdTransferencia)
                .HasName("PK__Transfer__6E5969ECA5EB9319");

            entity.Property(e => e.FechaTransferencia)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdEstadoTransferenciaNavigation)
                .WithMany(p => p.TransferenciaInventarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transferencia_Estado");

            entity.HasOne(d => d.IdUbicacionDestinoNavigation)
                .WithMany(p => p.TransferenciaInventarioIdUbicacionDestinoNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transferencia_Destino");

            entity.HasOne(d => d.IdUbicacionOrigenNavigation)
                .WithMany(p => p.TransferenciaInventarioIdUbicacionOrigenNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transferencia_Origen");

            entity.HasOne(d => d.IdUsuarioNavigation)
                .WithMany(p => p.TransferenciaInventarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transferencia_Usuario");
        });


        modelBuilder.Entity<UbicacionInventario>(entity =>
        {
            entity.HasKey(e => e.IdUbicacion)
                .HasName("PK__Ubicacio__778CAB1D3A1AE13B");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");
        });


        modelBuilder.Entity<UnidadMedidum>(entity =>
        {
            entity.HasKey(e => e.IdUnidadMedida)
                .HasName("PK__UnidadMe__18F83A93CF9C3765");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");
        });


        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario)
                .HasName("PK__Usuario__5B65BF97B11D1981");

            entity.Property(e => e.Estado)
                .HasDefaultValue(true);

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdRolNavigation)
                .WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });


        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}