using BuildLogic45.Insurance.Core.Policies;
using PolicyRow = BuildLogic45.Insurance.Infrastructure.Persistence.Postgres.Entities.Policy;

namespace BuildLogic45.Insurance.Infrastructure.Persistence.Postgres;

/// <summary>
/// Maps between the scaffolded <see cref="PolicyRow"/> (which mirrors the table) and the domain
/// <see cref="Policy"/>. Kept separate from the scaffolded code so re-scaffolding never overwrites it.
/// </summary>
internal static class PolicyMapper
{
    public static Policy ToDomain(this PolicyRow row) => new()
    {
        Id = row.Id,
        PolicyNumber = row.PolicyNumber,
        PolicyHolderName = row.PolicyHolderName,
        PolicyHolderEmail = row.PolicyHolderEmail,
        Type = Enum.Parse<PolicyType>(row.Type),
        Status = Enum.Parse<PolicyStatus>(row.Status),
        EffectiveDate = row.EffectiveDate,
        ExpirationDate = row.ExpirationDate,
        Premium = row.Premium,
        CoverageAmount = row.CoverageAmount,
        Deductible = row.Deductible,
        // Npgsql reads timestamptz as UTC DateTime values.
        CreatedAtUtc = new DateTimeOffset(row.CreatedAtUtc),
        UpdatedAtUtc = new DateTimeOffset(row.UpdatedAtUtc),
    };

    public static PolicyRow ToRow(this Policy policy) => new()
    {
        Id = policy.Id,
        PolicyNumber = policy.PolicyNumber,
        PolicyHolderName = policy.PolicyHolderName,
        PolicyHolderEmail = policy.PolicyHolderEmail,
        Type = policy.Type.ToString(),
        Status = policy.Status.ToString(),
        EffectiveDate = policy.EffectiveDate,
        ExpirationDate = policy.ExpirationDate,
        Premium = policy.Premium,
        CoverageAmount = policy.CoverageAmount,
        Deductible = policy.Deductible,
        CreatedAtUtc = policy.CreatedAtUtc.UtcDateTime,
        UpdatedAtUtc = policy.UpdatedAtUtc.UtcDateTime,
    };
}
