using BuildLogic45.Insurance.Core.Common;
using BuildLogic45.Insurance.Core.Policies;

namespace BuildLogic45.Insurance.Core.Tests.Policies;

public class PolicyServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

    private readonly FakePolicyRepository _repository = new();
    private readonly PolicyService _service;

    public PolicyServiceTests()
    {
        _service = new PolicyService(_repository, new FixedTimeProvider(Now));
    }

    private static CreatePolicyRequest ValidRequest() => new()
    {
        PolicyHolderName = "  Jane Doe  ",
        PolicyHolderEmail = "jane.doe@example.com",
        Type = PolicyType.Auto,
        EffectiveDate = new DateOnly(2026, 11, 1),
        ExpirationDate = new DateOnly(2027, 11, 1),
        Premium = 1_000m,
        CoverageAmount = 50_000m,
        Deductible = 500m,
    };

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsPolicyWithDefaults()
    {
        var policy = await _service.CreateAsync(ValidRequest());

        Assert.Equal("Jane Doe", policy.PolicyHolderName);
        Assert.Equal(PolicyStatus.Draft, policy.Status);
        Assert.Matches(@"^POL-2026-\d{6}$", policy.PolicyNumber);
        Assert.Equal(Now, policy.CreatedAtUtc);
        Assert.Equal(Now, policy.UpdatedAtUtc);
        Assert.Same(policy, _repository.Store[policy.Id]);
    }

    [Theory]
    [InlineData("", "jane@example.com", 1000, 50000, 0, "policyHolderName")]
    [InlineData("Jane", "not-an-email", 1000, 50000, 0, "policyHolderEmail")]
    [InlineData("Jane", "jane@example.com", 0, 50000, 0, "premium")]
    [InlineData("Jane", "jane@example.com", 1000, 0, 0, "coverageAmount")]
    [InlineData("Jane", "jane@example.com", 1000, 50000, 60000, "deductible")]
    public async Task CreateAsync_InvalidRequest_ThrowsAndDoesNotPersist(
        string name, string email, double premium, double coverage, double deductible, string expectedField)
    {
        var request = ValidRequest() with
        {
            PolicyHolderName = name,
            PolicyHolderEmail = email,
            Premium = (decimal)premium,
            CoverageAmount = (decimal)coverage,
            Deductible = (decimal)deductible,
        };

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateAsync(request));

        Assert.Contains(expectedField, ex.Message);
        Assert.Empty(_repository.Store);
    }

    [Fact]
    public async Task CreateAsync_ExpirationNotAfterEffective_Throws()
    {
        var request = ValidRequest() with { ExpirationDate = new DateOnly(2026, 11, 1) };

        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateAsync(request));

        Assert.Contains("expirationDate", ex.Message);
    }

    [Fact]
    public async Task GetAsync_FindsByIdOrPolicyNumber()
    {
        var created = await _service.CreateAsync(ValidRequest());

        Assert.Equal(created, await _service.GetAsync(created.Id.ToString()));
        Assert.Equal(created, await _service.GetAsync(created.PolicyNumber));
        Assert.Null(await _service.GetAsync("POL-1999-000000"));
    }

    [Fact]
    public async Task UpdateAsync_AppliesOnlyProvidedFields()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var updated = await _service.UpdateAsync(
            created.PolicyNumber,
            new UpdatePolicyRequest { Status = PolicyStatus.Active, Premium = 1_200m });

        Assert.NotNull(updated);
        Assert.Equal(PolicyStatus.Active, updated.Status);
        Assert.Equal(1_200m, updated.Premium);
        Assert.Equal(created.PolicyHolderName, updated.PolicyHolderName);
        Assert.Equal(created.CoverageAmount, updated.CoverageAmount);
        Assert.Equal(updated, _repository.Store[created.Id]);
    }

    [Fact]
    public async Task UpdateAsync_ResultingPolicyInvalid_ThrowsAndKeepsOriginal()
    {
        var created = await _service.CreateAsync(ValidRequest());

        await Assert.ThrowsAsync<DomainValidationException>(() =>
            _service.UpdateAsync(created.Id.ToString(), new UpdatePolicyRequest { ExpirationDate = new DateOnly(2020, 1, 1) }));

        Assert.Equal(created, _repository.Store[created.Id]);
    }

    [Fact]
    public async Task UpdateAsync_UnknownPolicy_ReturnsNull()
    {
        Assert.Null(await _service.UpdateAsync(Guid.NewGuid().ToString(), new UpdatePolicyRequest()));
    }

    [Fact]
    public async Task DeleteAsync_RemovesPolicy()
    {
        var created = await _service.CreateAsync(ValidRequest());

        Assert.True(await _service.DeleteAsync(created.PolicyNumber));
        Assert.False(await _service.DeleteAsync(created.PolicyNumber));
        Assert.Empty(_repository.Store);
    }

    [Fact]
    public async Task SearchAsync_NormalizesPagingAndQuery()
    {
        await _service.SearchAsync(new PolicySearchCriteria { Query = "   ", Page = 0, PageSize = 500 });

        Assert.NotNull(_repository.LastSearch);
        Assert.Null(_repository.LastSearch.Query);
        Assert.Equal(1, _repository.LastSearch.Page);
        Assert.Equal(PolicySearchCriteria.MaxPageSize, _repository.LastSearch.PageSize);
    }

    [Fact]
    public async Task SearchAsync_InvertedDateRange_Throws()
    {
        await Assert.ThrowsAsync<DomainValidationException>(() => _service.SearchAsync(new PolicySearchCriteria
        {
            EffectiveFrom = new DateOnly(2026, 1, 1),
            EffectiveTo = new DateOnly(2025, 1, 1),
        }));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
