using BuildLogic45.Insurance.Core.Common;

namespace BuildLogic45.Insurance.Core.Policies;

public interface IPolicyService
{
    Task<PagedResult<Policy>> SearchAsync(PolicySearchCriteria criteria, CancellationToken cancellationToken = default);

    /// <param name="idOrPolicyNumber">Either the policy GUID or its policy number.</param>
    Task<Policy?> GetAsync(string idOrPolicyNumber, CancellationToken cancellationToken = default);

    Task<Policy> CreateAsync(CreatePolicyRequest request, CancellationToken cancellationToken = default);

    /// <returns>The updated policy, or <c>null</c> if it was not found.</returns>
    Task<Policy?> UpdateAsync(string idOrPolicyNumber, UpdatePolicyRequest request, CancellationToken cancellationToken = default);

    /// <returns><c>false</c> if the policy was not found.</returns>
    Task<bool> DeleteAsync(string idOrPolicyNumber, CancellationToken cancellationToken = default);
}
