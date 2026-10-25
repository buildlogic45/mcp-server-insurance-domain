using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BuildLogic45.Insurance.Mcp.IntegrationTests;

public sealed class PolicyPromptsTests : McpServerTestBase
{
    [Fact]
    public async Task ListPrompts_ExposesPolicyPrompts()
    {
        var prompts = await Client.ListPromptsAsync();

        Assert.Equal(
            ["draft_renewal_notice", "new_policy_intake", "review_customer_portfolio", "summarize_policy"],
            prompts.Select(p => p.Name).Order());

        var summarize = prompts.Single(p => p.Name == "summarize_policy").ProtocolPrompt;
        Assert.Contains(summarize.Arguments!, a => a.Name == "policyIdOrNumber" && a.Required == true);
        Assert.Contains(summarize.Arguments!, a => a.Name == "audience" && a.Required != true);
    }

    [Fact]
    public async Task SummarizePolicy_EmbedsPolicyAndInstructions()
    {
        var result = await GetPromptAsync("summarize_policy", new() { ["policyIdOrNumber"] = "POL-2025-000101" });

        var embedded = Assert.Single(EmbeddedPolicies(result));
        Assert.Equal("insurance://policies/POL-2025-000101", embedded.Uri);
        Assert.Contains("Aarav Sharma", embedded.Text);
        Assert.Contains("policy holder", Instructions(result));
    }

    [Fact]
    public async Task SummarizePolicy_AgentAudience_ChangesStyle()
    {
        var result = await GetPromptAsync("summarize_policy", new() { ["policyIdOrNumber"] = "POL-2025-000101", ["audience"] = "agent" });

        Assert.Contains("insurance agent", Instructions(result));
    }

    [Fact]
    public async Task SummarizePolicy_UnknownPolicy_ReturnsResourceNotFound()
    {
        var ex = await Assert.ThrowsAsync<McpProtocolException>(() =>
            GetPromptAsync("summarize_policy", new() { ["policyIdOrNumber"] = "POL-1999-000000" }));

        Assert.Equal(McpErrorCode.ResourceNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task ReviewCustomerPortfolio_EmbedsEveryMatchingPolicy()
    {
        var result = await GetPromptAsync("review_customer_portfolio", new() { ["customer"] = "Aarav Sharma" });

        Assert.Equal(
            ["insurance://policies/POL-2025-000101", "insurance://policies/POL-2026-000111"],
            EmbeddedPolicies(result).Select(r => r.Uri).Order());
        Assert.Contains("Coverage gaps", Instructions(result));
    }

    [Fact]
    public async Task ReviewCustomerPortfolio_NoMatches_ReturnsError()
    {
        var ex = await Assert.ThrowsAsync<McpProtocolException>(() =>
            GetPromptAsync("review_customer_portfolio", new() { ["customer"] = "Nobody Atall" }));

        Assert.Contains("No policies found", ex.Message);
    }

    [Fact]
    public async Task DraftRenewalNotice_AddressesPolicyHolder()
    {
        var result = await GetPromptAsync("draft_renewal_notice", new() { ["policyIdOrNumber"] = "POL-2025-000102" });

        Assert.Single(EmbeddedPolicies(result));
        Assert.Contains("Emily Johnson <emily.johnson@example.com>", Instructions(result));
    }

    [Fact]
    public async Task NewPolicyIntake_WithType_IncludesTypeAndRules()
    {
        var result = await GetPromptAsync("new_policy_intake", new() { ["policyType"] = "home" });

        var text = Instructions(result);
        Assert.Contains("wants a Home policy", text);
        Assert.Contains("# Policy rules", text);
        Assert.Contains("create_policy", text);
        Assert.Empty(EmbeddedPolicies(result));
    }

    [Fact]
    public async Task NewPolicyIntake_InvalidType_ReturnsError()
    {
        var ex = await Assert.ThrowsAsync<McpProtocolException>(() =>
            GetPromptAsync("new_policy_intake", new() { ["policyType"] = "Pet" }));

        Assert.Contains("Unknown policy type 'Pet'", ex.Message);
    }

    private Task<GetPromptResult> GetPromptAsync(string name, Dictionary<string, object?> args) =>
        Client.GetPromptAsync(name, args).AsTask();

    private static IEnumerable<TextResourceContents> EmbeddedPolicies(GetPromptResult result) =>
        result.Messages.Select(m => m.Content).OfType<EmbeddedResourceBlock>().Select(b => (TextResourceContents)b.Resource);

    private static string Instructions(GetPromptResult result) => Text(result.Messages.Select(m => m.Content));
}
