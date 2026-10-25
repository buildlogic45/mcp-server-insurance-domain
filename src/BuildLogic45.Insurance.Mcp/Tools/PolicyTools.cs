using System.ComponentModel;
using BuildLogic45.Insurance.Core.Common;
using BuildLogic45.Insurance.Core.Policies;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace BuildLogic45.Insurance.Mcp.Tools;

[McpServerToolType]
public sealed class PolicyTools(IPolicyService policyService)
{
    [McpServerTool(Name = "search_policies", Title = "Search policies", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Searches insurance policies. All filters are optional and combined with AND. Results are paged and ordered by policy number.")]
    public Task<PagedResult<Policy>> SearchPolicies(
        [Description("Free text matched against policy number, policy holder name and email.")] string? query = null,
        [Description("Filter by policy type.")] PolicyType? type = null,
        [Description("Filter by policy status.")] PolicyStatus? status = null,
        [Description("Only policies effective on or after this date (yyyy-MM-dd).")] DateOnly? effectiveFrom = null,
        [Description("Only policies effective on or before this date (yyyy-MM-dd).")] DateOnly? effectiveTo = null,
        [Description("1-based page number.")] int page = 1,
        [Description("Page size, 1 to 100.")] int pageSize = PolicySearchCriteria.DefaultPageSize,
        CancellationToken cancellationToken = default) =>
        Execute(() => policyService.SearchAsync(
            new PolicySearchCriteria
            {
                Query = query,
                Type = type,
                Status = status,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveTo,
                Page = page,
                PageSize = pageSize,
            },
            cancellationToken));

    [McpServerTool(Name = "get_policy", Title = "Get policy", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Gets a single insurance policy by its id (GUID) or policy number (e.g. POL-2025-000101).")]
    public Task<Policy> GetPolicy(
        [Description("Policy id (GUID) or policy number.")] string policyIdOrNumber,
        CancellationToken cancellationToken = default) =>
        Execute(async () => await policyService.GetAsync(policyIdOrNumber, cancellationToken)
            ?? throw NotFound(policyIdOrNumber));

    [McpServerTool(Name = "create_policy", Title = "Create policy", Destructive = false, OpenWorld = false)]
    [Description("Creates a new insurance policy. A policy number is generated automatically. Returns the created policy.")]
    public Task<Policy> CreatePolicy(
        [Description("Full name of the policy holder.")] string policyHolderName,
        [Description("Email address of the policy holder.")] string policyHolderEmail,
        [Description("Type of insurance.")] PolicyType type,
        [Description("Date coverage starts (yyyy-MM-dd).")] DateOnly effectiveDate,
        [Description("Date coverage ends (yyyy-MM-dd). Must be after effectiveDate.")] DateOnly expirationDate,
        [Description("Annual premium amount. Must be greater than zero.")] decimal premium,
        [Description("Maximum coverage amount. Must be greater than zero.")] decimal coverageAmount,
        [Description("Deductible amount, between zero and coverageAmount.")] decimal deductible = 0,
        [Description("Initial status. Defaults to Draft.")] PolicyStatus? status = null,
        CancellationToken cancellationToken = default) =>
        Execute(() => policyService.CreateAsync(
            new CreatePolicyRequest
            {
                PolicyHolderName = policyHolderName,
                PolicyHolderEmail = policyHolderEmail,
                Type = type,
                EffectiveDate = effectiveDate,
                ExpirationDate = expirationDate,
                Premium = premium,
                CoverageAmount = coverageAmount,
                Deductible = deductible,
                Status = status,
            },
            cancellationToken));

    [McpServerTool(Name = "update_policy", Title = "Update policy", Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description("Updates an existing insurance policy. Only the fields provided are changed. Returns the updated policy.")]
    public Task<Policy> UpdatePolicy(
        [Description("Policy id (GUID) or policy number.")] string policyIdOrNumber,
        [Description("New policy holder name.")] string? policyHolderName = null,
        [Description("New policy holder email.")] string? policyHolderEmail = null,
        [Description("New policy type.")] PolicyType? type = null,
        [Description("New policy status.")] PolicyStatus? status = null,
        [Description("New effective date (yyyy-MM-dd).")] DateOnly? effectiveDate = null,
        [Description("New expiration date (yyyy-MM-dd).")] DateOnly? expirationDate = null,
        [Description("New annual premium.")] decimal? premium = null,
        [Description("New coverage amount.")] decimal? coverageAmount = null,
        [Description("New deductible.")] decimal? deductible = null,
        CancellationToken cancellationToken = default) =>
        Execute(async () => await policyService.UpdateAsync(
                policyIdOrNumber,
                new UpdatePolicyRequest
                {
                    PolicyHolderName = policyHolderName,
                    PolicyHolderEmail = policyHolderEmail,
                    Type = type,
                    Status = status,
                    EffectiveDate = effectiveDate,
                    ExpirationDate = expirationDate,
                    Premium = premium,
                    CoverageAmount = coverageAmount,
                    Deductible = deductible,
                },
                cancellationToken)
            ?? throw NotFound(policyIdOrNumber));

    [McpServerTool(Name = "delete_policy", Title = "Delete policy", Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description("Permanently deletes an insurance policy.")]
    public Task<string> DeletePolicy(
        [Description("Policy id (GUID) or policy number.")] string policyIdOrNumber,
        CancellationToken cancellationToken = default) =>
        Execute(async () => await policyService.DeleteAsync(policyIdOrNumber, cancellationToken)
            ? $"Policy '{policyIdOrNumber}' was deleted."
            : throw NotFound(policyIdOrNumber));

    private static McpException NotFound(string policyIdOrNumber) =>
        new($"Policy '{policyIdOrNumber}' was not found.");

    /// <summary>Surfaces business-rule violations to the client as tool errors with a readable message.</summary>
    private static async Task<T> Execute<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (DomainValidationException ex)
        {
            throw new McpException(ex.Message, ex);
        }
    }
}
