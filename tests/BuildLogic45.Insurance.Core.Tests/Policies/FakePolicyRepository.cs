using BuildLogic45.Insurance.Core.Common;
using BuildLogic45.Insurance.Core.Policies;

namespace BuildLogic45.Insurance.Core.Tests.Policies;

/// <summary>Minimal dictionary-backed repository so service tests stay independent of Infrastructure.</summary>
internal sealed class FakePolicyRepository : IPolicyRepository
{
    public Dictionary<Guid, Policy> Store { get; } = [];
    public PolicySearchCriteria? LastSearch { get; private set; }

    public Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Store.GetValueOrDefault(id));

    public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default) =>
        Task.FromResult(Store.Values.FirstOrDefault(p => p.PolicyNumber == policyNumber));

    public Task<PagedResult<Policy>> SearchAsync(PolicySearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        LastSearch = criteria;
        return Task.FromResult(new PagedResult<Policy>([.. Store.Values], criteria.Page, criteria.PageSize, Store.Count));
    }

    public Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        Store.Add(policy.Id, policy);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        if (!Store.ContainsKey(policy.Id))
        {
            return Task.FromResult(false);
        }

        Store[policy.Id] = policy;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Store.Remove(id));
}
