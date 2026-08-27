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

    public virtual DbSet<Cliente> Clientes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=LAPTOP-I7ACGHOB\\SQLEXPRESS;Database=BD_RestauranteWeb;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__Cliente__D594664244295BB6");

            entity.ToTable("Cliente");

            entity.HasIndex(e => new { e.Estado, e.Nombres, e.Apellidos }, "IX_Cliente_Estado_Nombres");

            entity.HasIndex(e => new { e.Nombres, e.Apellidos }, "IX_Cliente_Nombres");

            entity.HasIndex(e => e.Telefono, "IX_Cliente_Telefono");

            entity.HasIndex(e => e.Cedula, "UQ_Cliente_Cedula").IsUnique();

            entity.HasIndex(e => e.Correo, "UQ_Cliente_Correo").IsUnique();

            entity.Property(e => e.Apellidos).HasMaxLength(80);
            entity.Property(e => e.Cedula).HasMaxLength(20);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombres).HasMaxLength(80);
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.Sexo)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
