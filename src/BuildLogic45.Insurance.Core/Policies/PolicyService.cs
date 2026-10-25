using System.Net.Mail;
using BuildLogic45.Insurance.Core.Common;

namespace BuildLogic45.Insurance.Core.Policies;

public sealed class PolicyService(IPolicyRepository repository, TimeProvider timeProvider) : IPolicyService
{
    public const int MaxNameLength = 200;
    private const int MaxPolicyNumberAttempts = 10;

    public Task<PagedResult<Policy>> SearchAsync(PolicySearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        if (criteria.EffectiveFrom > criteria.EffectiveTo)
        {
            throw new DomainValidationException("effectiveFrom must be on or before effectiveTo.");
        }

        var normalized = criteria with
        {
            Query = string.IsNullOrWhiteSpace(criteria.Query) ? null : criteria.Query.Trim(),
            Page = Math.Max(1, criteria.Page),
            PageSize = Math.Clamp(criteria.PageSize, 1, PolicySearchCriteria.MaxPageSize),
        };

        return repository.SearchAsync(normalized, cancellationToken);
    }

    public Task<Policy?> GetAsync(string idOrPolicyNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idOrPolicyNumber))
        {
            throw new DomainValidationException("A policy id or policy number is required.");
        }

        var key = idOrPolicyNumber.Trim();
        return Guid.TryParse(key, out var id)
            ? repository.GetByIdAsync(id, cancellationToken)
            : repository.GetByPolicyNumberAsync(key, cancellationToken);
    }

    public async Task<Policy> CreateAsync(CreatePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var policy = new Policy
        {
            Id = Guid.CreateVersion7(now),
            PolicyNumber = await GeneratePolicyNumberAsync(request.EffectiveDate.Year, cancellationToken),
            PolicyHolderName = request.PolicyHolderName.Trim(),
            PolicyHolderEmail = request.PolicyHolderEmail.Trim(),
            Type = request.Type,
            Status = request.Status ?? PolicyStatus.Draft,
            EffectiveDate = request.EffectiveDate,
            ExpirationDate = request.ExpirationDate,
            Premium = request.Premium,
            CoverageAmount = request.CoverageAmount,
            Deductible = request.Deductible,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        Validate(policy);
        await repository.AddAsync(policy, cancellationToken);
        return policy;
    }

    public async Task<Policy?> UpdateAsync(string idOrPolicyNumber, UpdatePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(idOrPolicyNumber, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var updated = existing with
        {
            PolicyHolderName = request.PolicyHolderName?.Trim() ?? existing.PolicyHolderName,
            PolicyHolderEmail = request.PolicyHolderEmail?.Trim() ?? existing.PolicyHolderEmail,
            Type = request.Type ?? existing.Type,
            Status = request.Status ?? existing.Status,
            EffectiveDate = request.EffectiveDate ?? existing.EffectiveDate,
            ExpirationDate = request.ExpirationDate ?? existing.ExpirationDate,
            Premium = request.Premium ?? existing.Premium,
            CoverageAmount = request.CoverageAmount ?? existing.CoverageAmount,
            Deductible = request.Deductible ?? existing.Deductible,
            UpdatedAtUtc = timeProvider.GetUtcNow(),
        };

        Validate(updated);
        return await repository.UpdateAsync(updated, cancellationToken) ? updated : null;
    }

    public async Task<bool> DeleteAsync(string idOrPolicyNumber, CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(idOrPolicyNumber, cancellationToken);
        return existing is not null && await repository.DeleteAsync(existing.Id, cancellationToken);
    }

    private static void Validate(Policy policy)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(policy.PolicyHolderName))
        {
            errors.Add("policyHolderName is required.");
        }
        else if (policy.PolicyHolderName.Length > MaxNameLength)
        {
            errors.Add($"policyHolderName must be at most {MaxNameLength} characters.");
        }

        if (!MailAddress.TryCreate(policy.PolicyHolderEmail, out _))
        {
            errors.Add("policyHolderEmail must be a valid email address.");
        }

        if (policy.ExpirationDate <= policy.EffectiveDate)
        {
            errors.Add("expirationDate must be after effectiveDate.");
        }

        if (policy.Premium <= 0)
        {
            errors.Add("premium must be greater than zero.");
        }

        if (policy.CoverageAmount <= 0)
        {
            errors.Add("coverageAmount must be greater than zero.");
        }

        if (policy.Deductible < 0 || policy.Deductible > policy.CoverageAmount)
        {
            errors.Add("deductible must be between zero and coverageAmount.");
        }

        if (errors.Count > 0)
        {
            throw new DomainValidationException(string.Join(" ", errors));
        }
    }

    private async Task<string> GeneratePolicyNumberAsync(int year, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxPolicyNumberAttempts; attempt++)
        {
            var candidate = $"POL-{year}-{Random.Shared.Next(0, 1_000_000):D6}";
            if (await repository.GetByPolicyNumberAsync(candidate, cancellationToken) is null)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Could not generate a unique policy number.");
    }
}
