using BuildLogic45.Insurance.Core.Policies;
using BuildLogic45.Insurance.Infrastructure.Persistence;
using BuildLogic45.Insurance.Infrastructure.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BuildLogic45.Insurance.Infrastructure.Tests.Postgres;

/// <summary>
/// Runs against a real PostgreSQL server. Set <c>INSURANCE_TEST_POSTGRES</c> to a connection string
/// whose user may create databases (e.g. <c>Host=localhost;Username=postgres;Password=postgres</c>);
/// otherwise the tests are skipped. xUnit creates a new instance per test, so each test gets its
/// own freshly migrated and seeded database, which is dropped afterwards.
/// </summary>
public sealed class PostgresPolicyRepositoryTests : IAsyncLifetime
{
    private const string ConnectionStringVariable = "INSURANCE_TEST_POSTGRES";

    private static readonly string? ServerConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);

    private readonly string _databaseName = $"insurance_test_{Guid.NewGuid():N}";
    private InsuranceDbContext _db = null!;
    private PostgresPolicyRepository _repository = null!;

    public async Task InitializeAsync()
    {
        if (ServerConnectionString is null)
        {
            return;
        }

        var connectionString = new NpgsqlConnectionStringBuilder(ServerConnectionString) { Database = _databaseName }.ConnectionString;
        _db = new InsuranceDbContext(new DbContextOptionsBuilder<InsuranceDbContext>().UseNpgsql(connectionString).Options);
        await _db.Database.MigrateAsync();

        _db.Policies.AddRange(PolicySeedData.Create());
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        _repository = new PostgresPolicyRepository(_db);
    }

    public async Task DisposeAsync()
    {
        if (ServerConnectionString is null)
        {
            return;
        }

        await _db.Database.EnsureDeletedAsync();
        await _db.DisposeAsync();
    }

    [SkippableFact]
    public async Task GetByPolicyNumber_IsCaseInsensitive()
    {
        SkipWithoutDatabase();

        var policy = await _repository.GetByPolicyNumberAsync("pol-2025-000102");

        Assert.NotNull(policy);
        Assert.Equal("Emily Johnson", policy.PolicyHolderName);
        Assert.Equal(new DateOnly(2025, 6, 15), policy.EffectiveDate);
        Assert.Equal(1_820m, policy.Premium);
        Assert.Equal(PolicyType.Home, policy.Type);
    }

    [SkippableFact]
    public async Task Search_MatchesTextCaseInsensitively_AndFilters()
    {
        SkipWithoutDatabase();

        var byText = await _repository.SearchAsync(new PolicySearchCriteria { Query = "SHARMA" });
        var byTypeAndStatus = await _repository.SearchAsync(new PolicySearchCriteria { Type = PolicyType.Auto, Status = PolicyStatus.Active });
        var byDate = await _repository.SearchAsync(new PolicySearchCriteria
        {
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
        });

        Assert.Equal(2, byText.TotalCount);
        Assert.All(byTypeAndStatus.Items, p => Assert.Equal((PolicyType.Auto, PolicyStatus.Active), (p.Type, p.Status)));
        Assert.Equal(2, byTypeAndStatus.TotalCount);
        Assert.Equal(["POL-2026-000108", "POL-2026-000111"], byDate.Items.Select(p => p.PolicyNumber));
    }

    [SkippableFact]
    public async Task Search_TreatsLikeWildcardsLiterally()
    {
        SkipWithoutDatabase();

        var result = await _repository.SearchAsync(new PolicySearchCriteria { Query = "%" });

        Assert.Equal(0, result.TotalCount);
    }

    [SkippableFact]
    public async Task Search_PagesInPolicyNumberOrder()
    {
        SkipWithoutDatabase();

        var page = await _repository.SearchAsync(new PolicySearchCriteria { Page = 2, PageSize = 5 });

        Assert.Equal(12, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(5, page.Items.Count);
        Assert.Equal(page.Items.Select(p => p.PolicyNumber).Order(StringComparer.Ordinal), page.Items.Select(p => p.PolicyNumber));
    }

    [SkippableFact]
    public async Task Add_ThenUpdate_ThenDelete_RoundTrips()
    {
        SkipWithoutDatabase();
        var now = new DateTimeOffset(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
        var policy = new Policy
        {
            Id = Guid.CreateVersion7(now),
            PolicyNumber = "POL-2026-999999",
            PolicyHolderName = "Jane Doe",
            PolicyHolderEmail = "jane.doe@example.com",
            Type = PolicyType.Home,
            Status = PolicyStatus.Draft,
            EffectiveDate = new DateOnly(2026, 11, 1),
            ExpirationDate = new DateOnly(2027, 11, 1),
            Premium = 1_500.25m,
            CoverageAmount = 300_000m,
            Deductible = 1_000m,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await _repository.AddAsync(policy);
        Assert.Equal(policy, await _repository.GetByIdAsync(policy.Id));

        var updated = policy with { Status = PolicyStatus.Active, Premium = 1_600m, UpdatedAtUtc = now.AddHours(1) };
        Assert.True(await _repository.UpdateAsync(updated));
        Assert.Equal(updated, await _repository.GetByIdAsync(policy.Id));

        Assert.True(await _repository.DeleteAsync(policy.Id));
        Assert.Null(await _repository.GetByIdAsync(policy.Id));
        Assert.False(await _repository.DeleteAsync(policy.Id));
    }

    [SkippableFact]
    public async Task Update_ReturnsFalse_WhenPolicyDoesNotExist()
    {
        SkipWithoutDatabase();
        var existing = await _repository.GetByPolicyNumberAsync("POL-2025-000101");

        var result = await _repository.UpdateAsync(existing! with { Id = Guid.NewGuid() });

        Assert.False(result);
    }

    [SkippableFact]
    public async Task Add_Throws_WhenPolicyNumberAlreadyExists()
    {
        SkipWithoutDatabase();
        var existing = await _repository.GetByPolicyNumberAsync("POL-2025-000101");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.AddAsync(existing! with { Id = Guid.NewGuid() }));
    }

    private static void SkipWithoutDatabase() =>
        Skip.If(ServerConnectionString is null, $"Set {ConnectionStringVariable} to run PostgreSQL tests.");
}
