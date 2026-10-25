using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BuildLogic45.Insurance.Mcp.IntegrationTests;

public sealed class PolicyResourcesTests : McpServerTestBase
{
    [Fact]
    public async Task ListResources_ExposesReferenceData()
    {
        var resources = await Client.ListResourcesAsync();

        Assert.Equal(
            ["insurance://reference/policy-rules", "insurance://reference/policy-statuses", "insurance://reference/policy-types"],
            resources.Select(r => r.Uri).Order());
    }

    [Fact]
    public async Task ListResourceTemplates_ExposesPolicyTemplate()
    {
        var templates = await Client.ListResourceTemplatesAsync();

        var template = Assert.Single(templates);
        Assert.Equal("insurance://policies/{policyIdOrNumber}", template.UriTemplate);
        Assert.Equal("application/json", template.MimeType);
    }

    [Theory]
    [InlineData("POL-2025-000102")]
    [InlineData("00000000-0000-0000-0000-000000000002")]
    public async Task ReadPolicy_ByNumberOrId_ReturnsPolicyJson(string key)
    {
        var result = await Client.ReadResourceAsync($"insurance://policies/{key}");

        var content = Assert.IsType<TextResourceContents>(Assert.Single(result.Contents));
        Assert.Equal("application/json", content.MimeType);
        using var json = JsonDocument.Parse(content.Text);
        Assert.Equal("Emily Johnson", json.RootElement.GetProperty("policyHolderName").GetString());
        Assert.Equal("POL-2025-000102", json.RootElement.GetProperty("policyNumber").GetString());
    }

    [Fact]
    public async Task ReadPolicy_Unknown_ReturnsResourceNotFound()
    {
        var ex = await Assert.ThrowsAsync<McpProtocolException>(() =>
            Client.ReadResourceAsync("insurance://policies/POL-1999-000000").AsTask());

        Assert.Equal(McpErrorCode.ResourceNotFound, ex.ErrorCode);
        Assert.Contains("POL-1999-000000", ex.Message);
    }

    [Theory]
    [InlineData("insurance://reference/policy-types", new[] { "Auto", "Home", "Life", "Health", "Travel" })]
    [InlineData("insurance://reference/policy-statuses", new[] { "Draft", "Active", "Lapsed", "Cancelled", "Expired" })]
    public async Task ReadEnumReference_ListsEveryValueWithDescription(string uri, string[] expected)
    {
        var result = await Client.ReadResourceAsync(uri);

        var content = Assert.IsType<TextResourceContents>(Assert.Single(result.Contents));
        var items = JsonDocument.Parse(content.Text).RootElement.EnumerateArray().ToList();
        Assert.Equal(expected, items.Select(i => i.GetProperty("name").GetString()));
        Assert.All(items, i => Assert.False(string.IsNullOrWhiteSpace(i.GetProperty("description").GetString())));
    }

    [Fact]
    public async Task ReadRules_ReturnsMarkdown()
    {
        var result = await Client.ReadResourceAsync("insurance://reference/policy-rules");

        var content = Assert.IsType<TextResourceContents>(Assert.Single(result.Contents));
        Assert.Equal("text/markdown", content.MimeType);
        Assert.StartsWith("# Policy rules", content.Text);
        Assert.Contains("at most 200 characters", content.Text);
    }
}
