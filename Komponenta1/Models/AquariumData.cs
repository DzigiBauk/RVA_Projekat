using Shared.Models;

namespace Komponenta1.Models;

public sealed class AquariumData
{
    public List<AquaticSpecies> Species { get; set; } = [];

    public List<WaterQualityReading> Readings { get; set; } = [];
}
