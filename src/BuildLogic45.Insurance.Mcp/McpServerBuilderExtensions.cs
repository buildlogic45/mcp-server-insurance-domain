using BuildLogic45.Insurance.Mcp.Prompts;
using BuildLogic45.Insurance.Mcp.Resources;
using BuildLogic45.Insurance.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLogic45.Insurance.Mcp;

public static class McpServerBuilderExtensions
{
    /// <summary>Registers all insurance MCP tools, prompts and resources. Shared by the stdio and HTTP hosts.</summary>
    public static IMcpServerBuilder WithInsuranceCapabilities(this IMcpServerBuilder builder) =>
        builder
            .WithTools<PolicyTools>()
            .WithPrompts<PolicyPrompts>()
            .WithResources<PolicyResources>()
            .WithResources<ReferenceResources>();
}
