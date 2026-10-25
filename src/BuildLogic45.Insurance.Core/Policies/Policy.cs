namespace BuildLogic45.Insurance.Core.Policies;

public sealed record Policy
{
    public required Guid Id { get; init; }

    /// <summary>Human-readable business key, e.g. POL-2026-000123.</summary>
    public required string PolicyNumber { get; init; }

    public required string PolicyHolderName { get; init; }
    public required string PolicyHolderEmail { get; init; }
    public required PolicyType Type { get; init; }
    public required PolicyStatus Status { get; init; }
    public required DateOnly EffectiveDate { get; init; }
    public required DateOnly ExpirationDate { get; init; }

    /// <summary>Annual premium in the policy currency.</summary>
    public required decimal Premium { get; init; }

    public required decimal CoverageAmount { get; init; }
    public required decimal Deductible { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
