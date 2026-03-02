using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace pz_server_administrator.Data.Database.ServerTest;

public partial class ServerTestContext : DbContext
{
    public ServerTestContext()
    {
    }

    public ServerTestContext(DbContextOptions<ServerTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Allowedsteamid> Allowedsteamids { get; set; } = null!;

    public virtual DbSet<Bannedid> Bannedids { get; set; } = null!;

    public virtual DbSet<Bannedip> Bannedips { get; set; } = null!;

    public virtual DbSet<Capability> Capabilities { get; set; } = null!;

    public virtual DbSet<DefaultRole> DefaultRoles { get; set; } = null!;

    public virtual DbSet<Role> Roles { get; set; } = null!;

    public virtual DbSet<Ticket> Tickets { get; set; } = null!;

    public virtual DbSet<Userlog> Userlogs { get; set; } = null!;

    public virtual DbSet<Whitelist> Whitelists { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Allowedsteamid>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("allowedsteamid");

            entity.Property(e => e.Steamid).HasColumnName("steamid");
        });

        modelBuilder.Entity<Bannedid>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("bannedid");

            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Steamid).HasColumnName("steamid");
        });

        modelBuilder.Entity<Bannedip>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("bannedip");

            entity.HasIndex(e => e.Ip, "idx_bannedip_ip");

            entity.HasIndex(e => e.Username, "idx_bannedip_username");

            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        modelBuilder.Entity<Capability>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("capabilities");

            entity.HasIndex(e => e.Role, "idx_capabilities_role");

            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Role).HasColumnName("role");
        });

        modelBuilder.Entity<DefaultRole>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("defaultRoles");

            entity.HasIndex(e => e.Name, "idx_defaultRoles_name");

            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Role).HasColumnName("role");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");

            entity.HasIndex(e => e.Id, "idx_role_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ColorB).HasColumnName("colorB");
            entity.Property(e => e.ColorG).HasColumnName("colorG");
            entity.Property(e => e.ColorR).HasColumnName("colorR");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Position)
                .HasDefaultValue(-1)
                .HasColumnName("position");
            entity.Property(e => e.Readonly)
                .HasDefaultValueSql("false")
                .HasColumnType("BOOLEAN")
                .HasColumnName("readonly");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("tickets");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnsweredId).HasColumnName("answeredID");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Viewed)
                .HasDefaultValueSql("false")
                .HasColumnType("BOOLEAN")
                .HasColumnName("viewed");
        });

        modelBuilder.Entity<Userlog>(entity =>
        {
            entity.ToTable("userlog");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.IssuedBy).HasColumnName("issuedBy");
            entity.Property(e => e.LastUpdate).HasColumnName("lastUpdate");
            entity.Property(e => e.Text).HasColumnName("text");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        modelBuilder.Entity<Whitelist>(entity =>
        {
            entity.ToTable("whitelist");

            entity.HasIndex(e => e.Id, "id").IsUnique();

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthType)
                .HasDefaultValue(1)
                .HasColumnName("authType");
            entity.Property(e => e.DisplayName).HasColumnName("displayName");
            entity.Property(e => e.GoogleKey).HasColumnName("googleKey");
            entity.Property(e => e.LastConnection).HasColumnName("lastConnection");
            entity.Property(e => e.Ownerid).HasColumnName("ownerid");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Steamid).HasColumnName("steamid");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.World)
                .HasDefaultValue("")
                .HasColumnName("world");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
