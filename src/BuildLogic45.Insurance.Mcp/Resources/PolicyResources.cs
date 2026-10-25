using System.ComponentModel;
using BuildLogic45.Insurance.Core.Common;
using BuildLogic45.Insurance.Core.Policies;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace BuildLogic45.Insurance.Mcp.Resources;

[McpServerResourceType]
public sealed class PolicyResources(IPolicyService policyService)
{
    [McpServerResource(UriTemplate = PolicyContent.PolicyUriTemplate, Name = "policy", Title = "Insurance policy", MimeType = PolicyContent.JsonMimeType)]
    [Description("A single insurance policy as JSON, addressed by its id (GUID) or policy number, e.g. insurance://policies/POL-2025-000101.")]
    public async Task<string> GetPolicy(string policyIdOrNumber, CancellationToken cancellationToken)
    {
        try
        {
            var policy = await policyService.GetAsync(policyIdOrNumber, cancellationToken)
                ?? throw PolicyContent.PolicyNotFound(policyIdOrNumber);
            return PolicyContent.ToJson(policy);
        }
        catch (DomainValidationException ex)
        {
            throw new McpProtocolException(ex.Message, ex, McpErrorCode.InvalidParams);
        }
    }
}
