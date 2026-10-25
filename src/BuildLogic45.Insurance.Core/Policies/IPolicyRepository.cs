using BuildLogic45.Insurance.Core.Common;

namespace BuildLogic45.Insurance.Core.Policies;

/// <summary>
/// Persistence abstraction for policies. Implemented in-memory for now; a PostgreSQL
/// implementation will replace it without changes to the service or MCP layers.
/// </summary>
public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default);
    Task<PagedResult<Policy>> SearchAsync(PolicySearchCriteria criteria, CancellationToken cancellationToken = default);
    Task AddAsync(Policy policy, CancellationToken cancellationToken = default);

    /// <returns><c>false</c> if the policy no longer exists.</returns>
    Task<bool> UpdateAsync(Policy policy, CancellationToken cancellationToken = default);

    /// <returns><c>false</c> if the policy did not exist.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
