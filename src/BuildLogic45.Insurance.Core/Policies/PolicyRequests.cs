namespace BuildLogic45.Insurance.Core.Policies;

public sealed record CreatePolicyRequest
{
    public required string PolicyHolderName { get; init; }
    public required string PolicyHolderEmail { get; init; }
    public required PolicyType Type { get; init; }
    public required DateOnly EffectiveDate { get; init; }
    public required DateOnly ExpirationDate { get; init; }
    public required decimal Premium { get; init; }
    public required decimal CoverageAmount { get; init; }
    public decimal Deductible { get; init; }

    /// <summary>Defaults to <see cref="PolicyStatus.Draft"/> when not supplied.</summary>
    public PolicyStatus? Status { get; init; }
}

/// <summary>Partial update: only non-null fields are applied.</summary>
public sealed record UpdatePolicyRequest
{
    public string? PolicyHolderName { get; init; }
    public string? PolicyHolderEmail { get; init; }
    public PolicyType? Type { get; init; }
    public PolicyStatus? Status { get; init; }
    public DateOnly? EffectiveDate { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public decimal? Premium { get; init; }
    public decimal? CoverageAmount { get; init; }
    public decimal? Deductible { get; init; }
}
