using System.Runtime.Serialization;

namespace Shared.Contracts;

[DataContract]
public sealed class ReadingsRequest
{
    [DataMember]
    public Guid SpeciesId { get; set; }

    [DataMember]
    public int Year { get; set; }

    [DataMember]
    public int Month { get; set; }
}
