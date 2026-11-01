using BuildLogic45.Insurance.Core.Policies;
using Microsoft.EntityFrameworkCore;

namespace BuildLogic45.Insurance.Infrastructure.Persistence.Postgres;

public sealed class InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : DbContext(options)
{
    public DbSet<Policy> Policies => Set<Policy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Policy>(policy =>
        {
            policy.ToTable("policies");
            policy.HasKey(p => p.Id);

            // "C" collation sorts byte-wise, matching the ordinal ordering of the in-memory store.
            policy.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
            policy.Property(p => p.PolicyNumber).HasColumnName("policy_number").HasMaxLength(32).UseCollation("C");
            policy.Property(p => p.PolicyHolderName).HasColumnName("policy_holder_name").HasMaxLength(PolicyService.MaxNameLength);
            policy.Property(p => p.PolicyHolderEmail).HasColumnName("policy_holder_email").HasMaxLength(320);

            // Enums are stored as text so the data stays readable and reordering the enum is safe.
            policy.Property(p => p.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(16);
            policy.Property(p => p.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);

            policy.Property(p => p.EffectiveDate).HasColumnName("effective_date");
            policy.Property(p => p.ExpirationDate).HasColumnName("expiration_date");
            policy.Property(p => p.Premium).HasColumnName("premium").HasPrecision(14, 2);
            policy.Property(p => p.CoverageAmount).HasColumnName("coverage_amount").HasPrecision(14, 2);
            policy.Property(p => p.Deductible).HasColumnName("deductible").HasPrecision(14, 2);
            policy.Property(p => p.CreatedAtUtc).HasColumnName("created_at_utc");
            policy.Property(p => p.UpdatedAtUtc).HasColumnName("updated_at_utc");

            policy.HasIndex(p => p.PolicyNumber).IsUnique();
            policy.HasIndex(p => new { p.Type, p.Status });
            policy.HasIndex(p => p.EffectiveDate);
        });
    }
}
