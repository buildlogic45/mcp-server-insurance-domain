using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using BuildLogic45.Insurance.Core.Policies;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace BuildLogic45.Insurance.Mcp;

/// <summary>Shared URIs and content builders so tools, resources and prompts describe policies the same way.</summary>
internal static class PolicyContent
{
    public const string PolicyUriTemplate = "insurance://policies/{policyIdOrNumber}";
    public const string PolicyTypesUri = "insurance://reference/policy-types";
    public const string PolicyStatusesUri = "insurance://reference/policy-statuses";
    public const string PolicyRulesUri = "insurance://reference/policy-rules";

    public const string JsonMimeType = "application/json";
    public const string MarkdownMimeType = "text/markdown";

    public static string PolicyUri(Policy policy) => $"insurance://policies/{policy.PolicyNumber}";

    /// <summary>Serializes with the same options the SDK uses for tool results.</summary>
    public static string ToJson<T>(T value) => JsonSerializer.Serialize(value, McpJsonUtilities.DefaultOptions);

    public static TextResourceContents ToResource(Policy policy) => new()
    {
        Uri = PolicyUri(policy),
        MimeType = JsonMimeType,
        Text = ToJson(policy),
    };

    /// <summary>Enum members with their <see cref="DescriptionAttribute"/> text, for reference data.</summary>
    public static IReadOnlyList<EnumValueInfo> DescribeEnum<TEnum>() where TEnum : struct, Enum =>
    [
        .. Enum.GetValues<TEnum>().Select(value => new EnumValueInfo(
            value.ToString(),
            typeof(TEnum).GetField(value.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty)),
    ];

    public static string RulesMarkdown() => $"""
        # Policy rules

        ## Required fields when creating a policy
        - `policyHolderName`: full name, at most {PolicyService.MaxNameLength} characters.
        - `policyHolderEmail`: a valid email address.
        - `type`: one of {string.Join(", ", Enum.GetNames<PolicyType>().Select(n => $"`{n}`"))}.
        - `effectiveDate` and `expirationDate`: `yyyy-MM-dd`; expiration must be after the effective date.
        - `premium`: annual premium, greater than zero.
        - `coverageAmount`: maximum cover, greater than zero.

        ## Optional fields
        - `deductible`: defaults to 0; must be between 0 and `coverageAmount`.
        - `status`: defaults to `Draft`. Allowed: {string.Join(", ", Enum.GetNames<PolicyStatus>().Select(n => $"`{n}`"))}.

        ## Behaviour
        - A policy number (`POL-<effective year>-<6 digits>`) is generated on creation.
        - Updates are partial; the resulting policy must still satisfy every rule above.
        - Policies can be addressed by id (GUID) or policy number.
        - Deletion is permanent.
        """;

    /// <summary>Thrown from prompts and resources when a policy does not exist.</summary>
    public static McpProtocolException PolicyNotFound(string policyIdOrNumber) =>
        new($"Policy '{policyIdOrNumber}' was not found.", McpErrorCode.ResourceNotFound);
}

public sealed record EnumValueInfo(string Name, string Description);
