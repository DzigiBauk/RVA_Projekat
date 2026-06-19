using System.Runtime.Serialization;

namespace Shared.Models;

[DataContract]
public class WaterQualityReading
{
    [DataMember]
    public Guid Id { get; set; }

    [DataMember]
    public Guid SpeciesId { get; set; }

    [DataMember]
    public DateTime MeasurementTime { get; set; }

    [DataMember]
    public double PHLevel { get; set; }

    [DataMember]
    public double Temperature { get; set; }

    [DataMember]
    public double OxygenLevel { get; set; }

    [DataMember]
    public WaterQualityState State { get; set; }
}
