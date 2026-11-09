using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RaceDayAuthAPI.DatabaseModels;

namespace RaceDayAuthAPI.Data;

public partial class RaceDayExistingDbContext : DbContext
{
    public RaceDayExistingDbContext(DbContextOptions<RaceDayExistingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Enrolment> Enrolments { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<RaceDayAuthAPI.DatabaseModels.Route> Routes { get; set; }

    public virtual DbSet<RaceDayAuthAPI.DatabaseModels.User> Users { get; set; }

    public virtual DbSet<WeatherInformation> WeatherInformations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => new { e.CategoryId, e.EventId }, "UQ_Categories_Category_Event").IsUnique();

            entity.HasIndex(e => new { e.EventId, e.Name }, "UQ_Categories_Event_Name").IsUnique();

            entity.Property(e => e.CategoryType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Distance).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Event).WithMany(p => p.Categories)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Categories_Event");
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasIndex(e => new { e.ParticipantId, e.EventId }, "UQ_Enrolments_Participant_Event").IsUnique();

            entity.Property(e => e.EnrolmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Event).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolments_Event");

            entity.HasOne(d => d.Participant).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolments_Participant");

            entity.HasOne(d => d.Category).WithMany(p => p.Enrolments)
                .HasPrincipalKey(p => new { p.CategoryId, p.EventId })
                .HasForeignKey(d => new { d.CategoryId, d.EventId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolments_Category_Event");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.Property(e => e.BannerImageLink).HasMaxLength(500);
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Distance).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.EventType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Organiser).WithMany(p => p.Events)
                .HasForeignKey(d => d.OrganiserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Events_Organiser");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasIndex(e => e.EnrolmentId, "UQ_Results_Enrolment").IsUnique();

            entity.Property(e => e.RecordedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Enrolment).WithOne(p => p.Result)
                .HasForeignKey<Result>(d => d.EnrolmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Results_Enrolment");
        });

        modelBuilder.Entity<RaceDayAuthAPI.DatabaseModels.Route>(entity =>
        {
            entity.HasIndex(e => e.EventId, "UQ_Routes_Event").IsUnique();

            entity.Property(e => e.Distance).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.ElevationGain).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.RouteMapUrl).HasMaxLength(500);

            entity.HasOne(d => d.Event).WithOne(p => p.Route)
                .HasForeignKey<RaceDayAuthAPI.DatabaseModels.Route>(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Routes_Event");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.DateRegistered)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ProfileImageLink).HasMaxLength(500);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<WeatherInformation>(entity =>
        {
            entity.HasKey(e => e.WeatherId);

            entity.ToTable("WeatherInformation");

            entity.HasIndex(e => e.EventId, "UQ_WeatherInformation_Event").IsUnique();

            entity.Property(e => e.RetrievedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Temperature).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.WeatherCondition).HasMaxLength(100);
            entity.Property(e => e.WindSpeed).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Event).WithOne(p => p.WeatherInformation)
                .HasForeignKey<WeatherInformation>(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WeatherInformation_Event");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
