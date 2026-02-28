using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace pz_server_administrator.Data.Database.Players;

public partial class PlayersContext : DbContext
{
    public PlayersContext()
    {
    }

    public PlayersContext(DbContextOptions<PlayersContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LocalPlayer> LocalPlayers { get; set; }

    public virtual DbSet<NetworkPlayer> NetworkPlayers { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalPlayer>(entity =>
        {
            entity.ToTable("localPlayers");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.IsDead)
                .HasColumnType("BOOLEAN")
                .HasColumnName("isDead");
            entity.Property(e => e.Name)
                .HasColumnType("STRING")
                .HasColumnName("name");
            entity.Property(e => e.Worldversion).HasColumnName("worldversion");
            entity.Property(e => e.Wx).HasColumnName("wx");
            entity.Property(e => e.Wy).HasColumnName("wy");
            entity.Property(e => e.X)
                .HasColumnType("FLOAT")
                .HasColumnName("x");
            entity.Property(e => e.Y)
                .HasColumnType("FLOAT")
                .HasColumnName("y");
            entity.Property(e => e.Z)
                .HasColumnType("FLOAT")
                .HasColumnName("z");
        });

        modelBuilder.Entity<NetworkPlayer>(entity =>
        {
            entity.ToTable("networkPlayers");

            entity.HasIndex(e => e.Username, "inpusername");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.IsDead)
                .HasColumnType("BOOLEAN")
                .HasColumnName("isDead");
            entity.Property(e => e.Name)
                .HasColumnType("STRING")
                .HasColumnName("name");
            entity.Property(e => e.PlayerIndex).HasColumnName("playerIndex");
            entity.Property(e => e.Steamid)
                .HasColumnType("STRING")
                .HasColumnName("steamid");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.World).HasColumnName("world");
            entity.Property(e => e.Worldversion).HasColumnName("worldversion");
            entity.Property(e => e.X)
                .HasColumnType("FLOAT")
                .HasColumnName("x");
            entity.Property(e => e.Y)
                .HasColumnType("FLOAT")
                .HasColumnName("y");
            entity.Property(e => e.Z)
                .HasColumnType("FLOAT")
                .HasColumnName("z");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
