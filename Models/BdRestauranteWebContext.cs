using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class BdRestauranteWebContext : DbContext
{
    public BdRestauranteWebContext()
    {
    }

    public BdRestauranteWebContext(DbContextOptions<BdRestauranteWebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Caja> Cajas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=LAPTOP-I7ACGHOB\\SQLEXPRESS;Database=BD_RestauranteWeb;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Caja>(entity =>
        {
            entity.HasKey(e => e.IdCaja).HasName("PK__Caja__3B7BF2C5FE28257D");

            entity.ToTable("Caja");

            entity.HasIndex(e => e.FechaApertura, "IX_Caja_Fecha");

            entity.HasIndex(e => e.IdUsuario, "IX_Caja_Usuario");

            entity.Property(e => e.Diferencia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DiferenciaCordobas).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DiferenciaDolares).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FechaApertura).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MontoInicial).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MontoInicialCordobas).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MontoInicialDolares).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Observacion).HasMaxLength(300);
            entity.Property(e => e.TotalContado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalContadoCordobas).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalContadoDolares).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalEgresos).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalEgresosCordobas).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalEgresosDolares).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalIngresos).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalIngresosCordobas).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalIngresosDolares).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalSistema).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalSistemaCordobas).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalSistemaDolares).HasColumnType("decimal(18, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
