namespace BuildLogic45.Insurance.Core.Policies;

public sealed record PolicySearchCriteria
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Free text matched against policy number, holder name and holder email.</summary>
    public string? Query { get; init; }

    public PolicyType? Type { get; init; }
    public PolicyStatus? Status { get; init; }

    /// <summary>Only policies whose effective date is on or after this date.</summary>
    public DateOnly? EffectiveFrom { get; init; }

    /// <summary>Only policies whose effective date is on or before this date.</summary>
    public DateOnly? EffectiveTo { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = DefaultPageSize;
}
