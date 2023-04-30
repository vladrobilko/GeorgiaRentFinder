using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataManagement.Models;

public partial class RentFinderDbContext : DbContext
{
    public RentFinderDbContext()
    {
    }

    public RentFinderDbContext(DbContextOptions<RentFinderDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BotAdminDto> BotAdminsDto { get; set; }

    public virtual DbSet<BotTelegramDto> BotTelegramsDto { get; set; }

    public virtual DbSet<ChannelInfoDto> ChannelInfosDto { get; set; }

    public virtual DbSet<FlatCoordinateDto> FlatCoordinatesDto { get; set; }

    public virtual DbSet<FlatDateInfoDto> FlatDateInfosDto { get; set; }

    public virtual DbSet<FlatInfoDto> FlatInfosDto { get; set; }

    public virtual DbSet<FlatLinkImage> FlatLinksImage { get; set; }

    public virtual DbSet<FlatPhoneDto> FlatPhonesDto { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BotAdminDto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BotAdminDto_pkey");

            entity.ToTable("BotAdminDto");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.BotTelegram).WithMany(p => p.BotAdminsDto)
                .HasForeignKey(d => d.BotTelegramId)
                .HasConstraintName("botadmindto_bottelegramid_foreign");
        });

        modelBuilder.Entity<BotTelegramDto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BotTelegramDto_pkey");

            entity.ToTable("BotTelegramDto");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<ChannelInfoDto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ChannelInfoDto_pkey");

            entity.ToTable("ChannelInfoDto");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.BotTelegram).WithMany(p => p.ChannelInfosDto)
                .HasForeignKey(d => d.BotTelegramId)
                .HasConstraintName("channelinfodto_bottelegramid_foreign");
        });

        modelBuilder.Entity<FlatCoordinateDto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FlatCoordinateDto_pkey");

            entity.ToTable("FlatCoordinateDto");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.FlatInfo).WithMany(p => p.FlatCoordinateDtos)
                .HasForeignKey(d => d.FlatInfoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("flatcoordinatedto_flatinfoid_foreign");
        });

        modelBuilder.Entity<FlatDateInfoDto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FlatDateInfoDto_pkey");

            entity.ToTable("FlatDateInfoDto");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.FlatInfo).WithMany(p => p.FlatDateInfoDtos)
                .HasForeignKey(d => d.FlatInfoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("flatdateinfodto_flatinfoid_foreign");
        });

        modelBuilder.Entity<FlatInfoDto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FlatInfoDto_pkey");

            entity.ToTable("FlatInfoDto");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.AdditionalInformation).HasColumnType("json");
            entity.Property(e => e.ViewsOnSite).HasColumnName("ViewsOnSIte");

            entity.HasOne(d => d.FlatPhone).WithMany(p => p.FlatInfoDtos)
                .HasForeignKey(d => d.FlatPhoneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("flatinfodto_flatphoneid_foreign");
        });

        modelBuilder.Entity<FlatLinkImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FlatLinkImage_pkey");

            entity.ToTable("FlatLinkImage");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.FlatInfo).WithMany(p => p.FlatLinkImages)
                .HasForeignKey(d => d.FlatInfoId)
                .HasConstraintName("flatlinkimage_flatinfoid_foreign");
        });

        modelBuilder.Entity<FlatPhoneDto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FlatPhoneDto_pkey");

            entity.ToTable("FlatPhoneDto");

            entity.HasIndex(e => e.Number, "flatphonedto_number_unique").IsUnique();

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
