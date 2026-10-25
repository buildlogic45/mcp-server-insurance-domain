using System.ComponentModel;
using BuildLogic45.Insurance.Core.Policies;
using ModelContextProtocol.Server;

namespace BuildLogic45.Insurance.Mcp.Resources;

/// <summary>Static reference data that helps a model use the policy tools correctly.</summary>
[McpServerResourceType]
public sealed class ReferenceResources
{
    [McpServerResource(UriTemplate = PolicyContent.PolicyTypesUri, Name = "policy_types", Title = "Policy types", MimeType = PolicyContent.JsonMimeType)]
    [Description("All supported policy types with a short description of each.")]
    public string GetPolicyTypes() => PolicyContent.ToJson(PolicyContent.DescribeEnum<PolicyType>());

    [McpServerResource(UriTemplate = PolicyContent.PolicyStatusesUri, Name = "policy_statuses", Title = "Policy statuses", MimeType = PolicyContent.JsonMimeType)]
    [Description("All policy statuses with what each one means.")]
    public string GetPolicyStatuses() => PolicyContent.ToJson(PolicyContent.DescribeEnum<PolicyStatus>());

    [McpServerResource(UriTemplate = PolicyContent.PolicyRulesUri, Name = "policy_rules", Title = "Policy rules", MimeType = PolicyContent.MarkdownMimeType)]
    [Description("Required fields, validation rules and behaviour for creating and updating policies.")]
    public string GetPolicyRules() => PolicyContent.RulesMarkdown();
}
