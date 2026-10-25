using System.Collections.Concurrent;
using BuildLogic45.Insurance.Core.Common;
using BuildLogic45.Insurance.Core.Policies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLogic45.Insurance.Infrastructure.Persistence.InMemory;

/// <summary>
/// Thread-safe, process-local policy store. Data is lost on restart. Stand-in until the
/// PostgreSQL repository is implemented.
/// </summary>
public sealed class InMemoryPolicyRepository : IPolicyRepository
{
    private readonly ConcurrentDictionary<Guid, Policy> _policies = new();

    public InMemoryPolicyRepository(IOptions<PersistenceOptions> options, ILogger<InMemoryPolicyRepository> logger)
    {
        if (options.Value.SeedSampleData)
        {
            foreach (var policy in PolicySeedData.Create())
            {
                _policies[policy.Id] = policy;
            }

            logger.LogInformation("Seeded in-memory policy store with {Count} policies", _policies.Count);
        }
    }

    public Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_policies.GetValueOrDefault(id));

    public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default) =>
        Task.FromResult(_policies.Values.FirstOrDefault(p =>
            string.Equals(p.PolicyNumber, policyNumber, StringComparison.OrdinalIgnoreCase)));

    public Task<PagedResult<Policy>> SearchAsync(PolicySearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        IEnumerable<Policy> query = _policies.Values;

        if (criteria.Query is { } text)
        {
            query = query.Where(p =>
                p.PolicyNumber.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                p.PolicyHolderName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                p.PolicyHolderEmail.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.Type is { } type)
        {
            query = query.Where(p => p.Type == type);
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(p => p.Status == status);
        }

        if (criteria.EffectiveFrom is { } from)
        {
            query = query.Where(p => p.EffectiveDate >= from);
        }

        if (criteria.EffectiveTo is { } to)
        {
            query = query.Where(p => p.EffectiveDate <= to);
        }

        var matches = query.OrderBy(p => p.PolicyNumber, StringComparer.Ordinal).ToList();
        var page = matches
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<Policy>(page, criteria.Page, criteria.PageSize, matches.Count));
    }

    public Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        if (!_policies.TryAdd(policy.Id, policy))
        {
            throw new InvalidOperationException($"A policy with id {policy.Id} already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        if (!_policies.TryGetValue(policy.Id, out var current))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(_policies.TryUpdate(policy.Id, policy, current));
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_policies.TryRemove(id, out _));
}
