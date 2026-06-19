using System.Runtime.Serialization;

namespace Shared.Models;

[DataContract]
public enum WaterQualityState
{
    [EnumMember]
    Optimal,

    [EnumMember]
    Acceptable,

    [EnumMember]
    Suboptimal,

    [EnumMember]
    Critical
}
