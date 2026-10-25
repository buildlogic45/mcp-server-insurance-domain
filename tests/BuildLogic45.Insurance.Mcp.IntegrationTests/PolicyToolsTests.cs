using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BuildLogic45.Insurance.Mcp.IntegrationTests;

public sealed class PolicyToolsTests : McpServerTestBase
{
    [Fact]
    public async Task ListTools_ExposesPolicyCrudTools()
    {
        var tools = await Client.ListToolsAsync();

        Assert.Equal(
            ["create_policy", "delete_policy", "get_policy", "search_policies", "update_policy"],
            tools.Select(t => t.Name).Order());
    }

    [Fact]
    public async Task SearchPolicies_FiltersSeedData()
    {
        var result = await CallAsync("search_policies", new() { ["type"] = "Auto", ["status"] = "Active" });

        Assert.Equal(2, result.GetProperty("totalCount").GetInt32());
        Assert.All(result.GetProperty("items").EnumerateArray(), p =>
        {
            Assert.Equal("Auto", p.GetProperty("type").GetString());
            Assert.Equal("Active", p.GetProperty("status").GetString());
        });
    }

    [Fact]
    public async Task SearchPolicies_FreeTextMatchesHolderName()
    {
        var result = await CallAsync("search_policies", new() { ["query"] = "sharma" });

        Assert.Equal(2, result.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task GetPolicy_BySeededPolicyNumber_ReturnsPolicy()
    {
        var policy = await CallAsync("get_policy", new() { ["policyIdOrNumber"] = "POL-2025-000102" });

        Assert.Equal("Emily Johnson", policy.GetProperty("policyHolderName").GetString());
        Assert.Equal("00000000-0000-0000-0000-000000000002", policy.GetProperty("id").GetString());
    }

    [Fact]
    public async Task CrudLifecycle_CreateReadUpdateDelete()
    {
        var created = await CallAsync("create_policy", new()
        {
            ["policyHolderName"] = "Jane Doe",
            ["policyHolderEmail"] = "jane.doe@example.com",
            ["type"] = "Home",
            ["effectiveDate"] = "2026-11-01",
            ["expirationDate"] = "2027-11-01",
            ["premium"] = 1500.50m,
            ["coverageAmount"] = 300000m,
            ["deductible"] = 1000m,
        });
        var number = created.GetProperty("policyNumber").GetString()!;
        Assert.Equal("Draft", created.GetProperty("status").GetString());

        var fetched = await CallAsync("get_policy", new() { ["policyIdOrNumber"] = created.GetProperty("id").GetString() });
        Assert.Equal(number, fetched.GetProperty("policyNumber").GetString());

        var updated = await CallAsync("update_policy", new() { ["policyIdOrNumber"] = number, ["status"] = "Active" });
        Assert.Equal("Active", updated.GetProperty("status").GetString());
        Assert.Equal(1500.50m, updated.GetProperty("premium").GetDecimal());

        var deleted = await Client.CallToolAsync("delete_policy", new Dictionary<string, object?> { ["policyIdOrNumber"] = number });
        Assert.NotEqual(true, deleted.IsError);

        var missing = await Client.CallToolAsync("get_policy", new Dictionary<string, object?> { ["policyIdOrNumber"] = number });
        Assert.True(missing.IsError);
        Assert.Contains("not found", Text(missing));
    }

    [Fact]
    public async Task CreatePolicy_InvalidInput_ReturnsToolErrorWithReason()
    {
        var result = await Client.CallToolAsync("create_policy", new Dictionary<string, object?>
        {
            ["policyHolderName"] = "Jane Doe",
            ["policyHolderEmail"] = "not-an-email",
            ["type"] = "Auto",
            ["effectiveDate"] = "2026-11-01",
            ["expirationDate"] = "2026-10-01",
            ["premium"] = 100m,
            ["coverageAmount"] = 1000m,
        });

        Assert.True(result.IsError);
        Assert.Contains("policyHolderEmail", Text(result));
        Assert.Contains("expirationDate", Text(result));
    }

    private async Task<JsonElement> CallAsync(string tool, Dictionary<string, object?> args)
    {
        var result = await Client.CallToolAsync(tool, args);
        Assert.True(result.IsError is not true, $"{tool} failed: {Text(result)}");
        return JsonDocument.Parse(Text(result)).RootElement.Clone();
    }

    private static string Text(CallToolResult result) => Text(result.Content);
}
