using BuildLogic45.Insurance.Core.Policies;

namespace BuildLogic45.Insurance.Infrastructure.Persistence.InMemory;

/// <summary>Deterministic sample policies. Ids are 00000000-0000-0000-0000-00000000000N.</summary>
internal static class PolicySeedData
{
    private static readonly DateTimeOffset SeededAt = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<Policy> Create() =>
    [
        Policy(1, "POL-2025-000101", "Aarav Sharma", "aarav.sharma@example.com", PolicyType.Auto, PolicyStatus.Active, new(2025, 11, 1), 12, 1_150m, 50_000m, 500m),
        Policy(2, "POL-2025-000102", "Emily Johnson", "emily.johnson@example.com", PolicyType.Home, PolicyStatus.Active, new(2025, 6, 15), 12, 1_820m, 450_000m, 2_500m),
        Policy(3, "POL-2024-000103", "Carlos Mendes", "carlos.mendes@example.com", PolicyType.Life, PolicyStatus.Active, new(2024, 3, 1), 240, 960m, 500_000m, 0m),
        Policy(4, "POL-2025-000104", "Priya Nair", "priya.nair@example.com", PolicyType.Health, PolicyStatus.Active, new(2025, 1, 1), 12, 4_200m, 1_000_000m, 1_500m),
        Policy(5, "POL-2025-000105", "Liam O'Connor", "liam.oconnor@example.com", PolicyType.Travel, PolicyStatus.Expired, new(2025, 7, 10), 1, 85m, 25_000m, 100m),
        Policy(6, "POL-2024-000106", "Sophie Martin", "sophie.martin@example.com", PolicyType.Auto, PolicyStatus.Lapsed, new(2024, 9, 1), 12, 980m, 40_000m, 750m),
        Policy(7, "POL-2025-000107", "Kenji Tanaka", "kenji.tanaka@example.com", PolicyType.Home, PolicyStatus.Cancelled, new(2025, 2, 1), 12, 1_430m, 320_000m, 1_000m),
        Policy(8, "POL-2026-000108", "Fatima Al-Sayed", "fatima.alsayed@example.com", PolicyType.Health, PolicyStatus.Draft, new(2026, 2, 1), 12, 3_650m, 750_000m, 2_000m),
        Policy(9, "POL-2025-000109", "Noah Williams", "noah.williams@example.com", PolicyType.Auto, PolicyStatus.Active, new(2025, 8, 20), 6, 640m, 30_000m, 1_000m),
        Policy(10, "POL-2025-000110", "Isabella Rossi", "isabella.rossi@example.com", PolicyType.Life, PolicyStatus.Active, new(2025, 5, 5), 360, 1_250m, 750_000m, 0m),
        Policy(11, "POL-2026-000111", "Aarav Sharma", "aarav.sharma@example.com", PolicyType.Home, PolicyStatus.Draft, new(2026, 3, 1), 12, 1_560m, 380_000m, 2_000m),
        Policy(12, "POL-2025-000112", "Olivia Brown", "olivia.brown@example.com", PolicyType.Travel, PolicyStatus.Active, new(2025, 12, 20), 1, 120m, 50_000m, 150m),
    ];

    private static Policy Policy(
        int seq, string number, string holder, string email, PolicyType type, PolicyStatus status,
        DateOnly effective, int termMonths, decimal premium, decimal coverage, decimal deductible) => new()
    {
        Id = new Guid($"00000000-0000-0000-0000-{seq:D12}"),
        PolicyNumber = number,
        PolicyHolderName = holder,
        PolicyHolderEmail = email,
        Type = type,
        Status = status,
        EffectiveDate = effective,
        ExpirationDate = effective.AddMonths(termMonths),
        Premium = premium,
        CoverageAmount = coverage,
        Deductible = deductible,
        CreatedAtUtc = SeededAt,
        UpdatedAtUtc = SeededAt,
    };
}
