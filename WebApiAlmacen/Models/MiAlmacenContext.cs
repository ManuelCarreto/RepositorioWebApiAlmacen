﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApiAlmacen.Models;

public partial class MiAlmacenContext : DbContext
{
    public MiAlmacenContext()
    {
    }

    public MiAlmacenContext(DbContextOptions<MiAlmacenContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Familia> Familias { get; set; }

    public virtual DbSet<Operacione> Operaciones { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-TTRHL1I;Initial Catalog=MiAlmacen;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Familia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Familias__3214EC07CC50D990");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Operacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Operacio__3214EC07FA94BE74");

            entity.Property(e => e.Controller).HasMaxLength(50);
            entity.Property(e => e.FechaAccion).HasColumnType("datetime");
            entity.Property(e => e.Ip).HasMaxLength(50);
            entity.Property(e => e.Operacion).HasMaxLength(50);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC076C3868BD");

            entity.Property(e => e.FechaAlta).HasColumnType("date");
            entity.Property(e => e.FotoUrl).HasColumnName("FotoURL");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Precio).HasColumnType("decimal(9, 2)");

            entity.HasOne(d => d.Familia).WithMany(p => p.Productos)
                .HasForeignKey(d => d.FamiliaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Familias_Productos");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07BC1337F9");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EnlaceCambioPass).HasMaxLength(50);
            entity.Property(e => e.EnlaceFechaEnvio).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
