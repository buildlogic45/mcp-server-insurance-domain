using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BuildLogic45.Insurance.Core.Policies;

[JsonConverter(typeof(JsonStringEnumConverter<PolicyStatus>))]
public enum PolicyStatus
{
    [Description("Quoted or being prepared; not yet in force.")]
    Draft,

    [Description("In force; claims can be made.")]
    Active,

    [Description("No longer in force because a premium was not paid; may be reinstated.")]
    Lapsed,

    [Description("Terminated before its expiration date by the insurer or the policy holder.")]
    Cancelled,

    [Description("Reached its expiration date without renewal.")]
    Expired,
}
