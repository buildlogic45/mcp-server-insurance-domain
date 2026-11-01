using System;
using System.Collections.Generic;
using BuildLogic45.Insurance.Infrastructure.Persistence.Postgres.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildLogic45.Insurance.Infrastructure.Persistence.Postgres;

public partial class InsuranceDbContext : DbContext
{
    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Policy> Policies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_policies");

            entity.ToTable("policies");

            entity.HasIndex(e => e.EffectiveDate, "ix_policies_effective_date");

            entity.HasIndex(e => new { e.Type, e.Status }, "ix_policies_type_status");

            entity.HasIndex(e => e.PolicyNumber, "uq_policies_policy_number").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CoverageAmount)
                .HasPrecision(14, 2)
                .HasColumnName("coverage_amount");
            entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc");
            entity.Property(e => e.Deductible)
                .HasPrecision(14, 2)
                .HasColumnName("deductible");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.PolicyHolderEmail)
                .HasMaxLength(320)
                .HasColumnName("policy_holder_email");
            entity.Property(e => e.PolicyHolderName)
                .HasMaxLength(200)
                .HasColumnName("policy_holder_name");
            entity.Property(e => e.PolicyNumber)
                .HasMaxLength(32)
                .UseCollation("C")
                .HasColumnName("policy_number");
            entity.Property(e => e.Premium)
                .HasPrecision(14, 2)
                .HasColumnName("premium");
            entity.Property(e => e.Status)
                .HasMaxLength(16)
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(16)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
