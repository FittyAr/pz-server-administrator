using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace pz_server_administrator.Data.Database.Vehicles;

public partial class VehiclesContext : DbContext
{
    public VehiclesContext()
    {
    }

    public VehiclesContext(DbContextOptions<VehiclesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Vehicle> Vehicles { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("vehicles");

            entity.HasIndex(e => e.Wx, "ivwx");

            entity.HasIndex(e => e.Wy, "ivwy");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.Worldversion).HasColumnName("worldversion");
            entity.Property(e => e.Wx).HasColumnName("wx");
            entity.Property(e => e.Wy).HasColumnName("wy");
            entity.Property(e => e.X)
                .HasColumnType("FLOAT")
                .HasColumnName("x");
            entity.Property(e => e.Y)
                .HasColumnType("FLOAT")
                .HasColumnName("y");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
