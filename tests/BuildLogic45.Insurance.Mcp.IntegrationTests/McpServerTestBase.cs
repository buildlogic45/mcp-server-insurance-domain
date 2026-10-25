using Microsoft.AspNetCore.Mvc.Testing;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BuildLogic45.Insurance.Mcp.IntegrationTests;

/// <summary>
/// Hosts the HTTP server in-process and connects a real MCP client over Streamable HTTP.
/// xUnit creates a new instance per test, so each test gets a freshly seeded store.
/// </summary>
public abstract class McpServerTestBase : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory = new();

    protected McpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions { Endpoint = new Uri(_factory.Server.BaseAddress, "/mcp") },
            _factory.CreateClient());

        Client = await McpClient.CreateAsync(transport);
    }

    public async Task DisposeAsync()
    {
        await Client.DisposeAsync();
        await _factory.DisposeAsync();
    }

    protected static string Text(IEnumerable<ContentBlock> content) =>
        string.Concat(content.OfType<TextContentBlock>().Select(c => c.Text));
}
