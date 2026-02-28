using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace pz_server_administrator.Data.Database.Mods;

/// <summary>
/// Contexto de base de datos para la gestión avanzada de mods.
/// </summary>
public partial class ModsContext : DbContext
{
    public ModsContext() { }

    public ModsContext(DbContextOptions<ModsContext> options) : base(options) { }

    public virtual DbSet<WorkshopItem> WorkshopItems { get; set; } = default!;
    public virtual DbSet<ModInstance> ModInstances { get; set; } = default!;
    public virtual DbSet<CloudProfile> CloudProfiles { get; set; } = default!;
    public virtual DbSet<ModIncompatibility> ModIncompatibilities { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkshopItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("WorkshopItems");
        });

        modelBuilder.Entity<ModInstance>(entity =>
        {
            entity.HasKey(e => e.ModId);
            entity.ToTable("ModInstances");

            entity.HasOne(d => d.WorkshopItem)
                .WithMany(p => p.Instances)
                .HasForeignKey(d => d.WorkshopItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CloudProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("CloudProfile");
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<ModIncompatibility>(entity =>
        {
            entity.HasKey(e => new { e.SourceModId, e.TargetModId });
            entity.ToTable("ModIncompatibilities");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

/// <summary>
/// Representa una relación de incompatibilidad entre dos mods.
/// </summary>
public class ModIncompatibility
{
    public string SourceModId { get; set; } = null!;
    public string TargetModId { get; set; } = null!;
    public string? Reason { get; set; }
    public string? Severity { get; set; } // ej: "Critical", "Warning"
}
