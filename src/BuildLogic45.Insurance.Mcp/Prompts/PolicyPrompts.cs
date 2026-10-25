using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BuildLogic45.Insurance.Core.Common;
using BuildLogic45.Insurance.Core.Policies;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BuildLogic45.Insurance.Mcp.Prompts;

/// <summary>
/// Reusable prompt templates. Prompts that need policy data load it server-side and embed it as
/// resources, so the model starts with accurate facts instead of having to call tools first.
/// </summary>
[McpServerPromptType]
public sealed class PolicyPrompts(IPolicyService policyService, TimeProvider timeProvider)
{
    private const string CustomerAudience = "customer";
    private const string AgentAudience = "agent";

    [McpServerPrompt(Name = "summarize_policy", Title = "Summarize a policy")]
    [Description("Explains an insurance policy in plain language for a customer, or concisely for an insurance agent.")]
    public async Task<IEnumerable<PromptMessage>> SummarizePolicy(
        [Description("Policy id (GUID) or policy number, e.g. POL-2025-000101.")] string policyIdOrNumber,
        [Description("Who the summary is for: 'customer' (plain language, default) or 'agent' (concise, technical).")]
        [AllowedValues(CustomerAudience, AgentAudience)] string? audience = CustomerAudience,
        CancellationToken cancellationToken = default)
    {
        var policy = await GetPolicyAsync(policyIdOrNumber, cancellationToken);

        var style = string.Equals(audience, AgentAudience, StringComparison.OrdinalIgnoreCase)
            ? "Write for an insurance agent: a concise bullet list of key terms, financial exposure and anything that needs action."
            : "Write for the policy holder: plain, friendly language with no jargon. Explain what is covered, what they pay and what they would pay themselves (the deductible) if they claimed.";

        return
        [
            Embed(policy),
            Text($"""
                Summarize the insurance policy above. Today's date is {Today:yyyy-MM-dd}.
                {style}
                Include its status and what that status means, the coverage period and how long remains, the premium, the coverage amount and the deductible.
                Use only the facts in the policy data; do not invent terms or exclusions.
                """),
        ];
    }

    [McpServerPrompt(Name = "review_customer_portfolio", Title = "Review a customer's portfolio")]
    [Description("Reviews all policies held by a customer and highlights coverage gaps, lapsed or expiring policies and suggested next steps.")]
    public async Task<IEnumerable<PromptMessage>> ReviewCustomerPortfolio(
        [Description("Customer name or email (partial matches allowed), e.g. 'Aarav Sharma' or 'aarav.sharma@example.com'.")] string customer,
        CancellationToken cancellationToken = default)
    {
        var result = await Execute(() => policyService.SearchAsync(
            new PolicySearchCriteria { Query = customer, PageSize = PolicySearchCriteria.MaxPageSize },
            cancellationToken));

        if (result.TotalCount == 0)
        {
            throw new McpException($"No policies found for customer '{customer}'.");
        }

        var truncation = result.TotalCount > result.Items.Count
            ? $" Only the first {result.Items.Count} of {result.TotalCount} matching policies are included."
            : string.Empty;

        return
        [
            .. result.Items.Select(Embed),
            Text($"""
                The policies above match the customer '{customer}'.{truncation} Today's date is {Today:yyyy-MM-dd}.
                If they belong to more than one person, say so and review each person separately.

                Produce a portfolio review with these sections:
                1. Overview: a table of each policy (number, type, status, coverage period, premium, coverage).
                2. Attention needed: policies that are lapsed, cancelled, expired, still in draft, or expire within 60 days.
                3. Coverage gaps: common policy types (Auto, Home, Life, Health, Travel) the customer does not hold in an active state, and whether that looks like a risk.
                4. Recommended next steps, as a short prioritised list.

                Base everything on the policy data; clearly label suggestions as suggestions.
                """),
        ];
    }

    [McpServerPrompt(Name = "draft_renewal_notice", Title = "Draft a renewal notice")]
    [Description("Drafts a renewal reminder email to the policy holder for an existing policy.")]
    public async Task<IEnumerable<PromptMessage>> DraftRenewalNotice(
        [Description("Policy id (GUID) or policy number, e.g. POL-2025-000101.")] string policyIdOrNumber,
        CancellationToken cancellationToken = default)
    {
        var policy = await GetPolicyAsync(policyIdOrNumber, cancellationToken);
        var daysRemaining = policy.ExpirationDate.DayNumber - Today.DayNumber;
        var timing = daysRemaining >= 0
            ? $"It expires in {daysRemaining} day(s), on {policy.ExpirationDate:yyyy-MM-dd}."
            : $"It expired {-daysRemaining} day(s) ago, on {policy.ExpirationDate:yyyy-MM-dd}.";

        return
        [
            Embed(policy),
            Text($"""
                Draft a renewal reminder email for the policy above. Today's date is {Today:yyyy-MM-dd}. {timing}

                Requirements:
                - Address it to {policy.PolicyHolderName} <{policy.PolicyHolderEmail}>, with a clear subject line.
                - Mention the policy number, policy type, expiration date, current annual premium and coverage amount.
                - Keep it short, friendly and professional, with a clear call to action to renew.
                - Do not promise a new premium or new terms; say a renewal quote will follow.
                - If the policy status is Cancelled or Draft, do not draft an email. Instead explain why a renewal notice is not appropriate.
                - If it has already expired or lapsed, adjust the wording to encourage reinstatement rather than renewal.
                """),
        ];
    }

    [McpServerPrompt(Name = "new_policy_intake", Title = "New policy intake")]
    [Description("Guides the assistant through collecting the details for a new policy, confirming them and then creating it with the create_policy tool.")]
    public IEnumerable<PromptMessage> NewPolicyIntake(
        [Description("Optional policy type to start with: Auto, Home, Life, Health or Travel.")]
        [AllowedValues(null, nameof(PolicyType.Auto), nameof(PolicyType.Home), nameof(PolicyType.Life), nameof(PolicyType.Health), nameof(PolicyType.Travel))]
        string? policyType = null)
    {
        string typeInstruction;
        if (string.IsNullOrWhiteSpace(policyType))
        {
            typeInstruction = "First ask which type of policy the customer wants.";
        }
        else if (Enum.TryParse<PolicyType>(policyType, ignoreCase: true, out var type))
        {
            typeInstruction = $"The customer wants a {type} policy, so the type is already known.";
        }
        else
        {
            throw new McpException($"Unknown policy type '{policyType}'. Allowed: {string.Join(", ", Enum.GetNames<PolicyType>())}.");
        }

        return
        [
            Text($"""
                You are helping an insurance agent set up a new policy. Today's date is {Today:yyyy-MM-dd}.
                {typeInstruction}

                Follow these rules:

                {PolicyContent.RulesMarkdown()}

                Process:
                1. Ask for the missing details, a few at a time. Suggest a one-year term starting today if the customer is unsure.
                2. Check the answers against the rules above and ask again for anything invalid.
                3. Show a summary of all values and ask for explicit confirmation.
                4. Only after confirmation, call the `create_policy` tool, then report the generated policy number.
                """),
        ];
    }

    private DateOnly Today => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

    private async Task<Policy> GetPolicyAsync(string policyIdOrNumber, CancellationToken cancellationToken) =>
        await Execute(() => policyService.GetAsync(policyIdOrNumber, cancellationToken))
        ?? throw PolicyContent.PolicyNotFound(policyIdOrNumber);

    private static PromptMessage Embed(Policy policy) => new()
    {
        Role = Role.User,
        Content = new EmbeddedResourceBlock { Resource = PolicyContent.ToResource(policy) },
    };

    private static PromptMessage Text(string text) => new()
    {
        Role = Role.User,
        Content = new TextContentBlock { Text = text },
    };

    private static async Task<T> Execute<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (DomainValidationException ex)
        {
            throw new McpProtocolException(ex.Message, ex, McpErrorCode.InvalidParams);
        }
    }
}
