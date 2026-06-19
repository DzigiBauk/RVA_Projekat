using System.Runtime.Serialization;

namespace Shared.Models;

[DataContract]
public class AquaticSpecies
{
    [DataMember]
    public Guid Id { get; set; }

    [DataMember]
    public string Name { get; set; } = string.Empty;

    [DataMember]
    public string ScientificName { get; set; } = string.Empty;

    [DataMember]
    public string Habitat { get; set; } = string.Empty;

    [DataMember]
    public string WaterType { get; set; } = string.Empty;

    [DataMember]
    public int AverageLifespan { get; set; }
}
