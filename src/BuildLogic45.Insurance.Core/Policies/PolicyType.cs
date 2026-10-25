using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BuildLogic45.Insurance.Core.Policies;

[JsonConverter(typeof(JsonStringEnumConverter<PolicyType>))]
public enum PolicyType
{
    [Description("Motor vehicle cover: collision, liability and theft.")]
    Auto,

    [Description("Buildings and contents cover for a residential property.")]
    Home,

    [Description("Pays a sum to beneficiaries on the death of the insured; usually a long term.")]
    Life,

    [Description("Medical and hospital expenses for the insured.")]
    Health,

    [Description("Short-term cover for trips: medical emergencies, cancellation and lost baggage.")]
    Travel,
}
