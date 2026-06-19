using Shared.Models;

namespace Komponenta2.Services.Adapters
{
    public class WaterQualityAdapter
    {
        public Dictionary<string, List<WaterQualityReading>> Adapt(List<WaterQualityReading> readings, List<AquaticSpecies> species)
        {
            return readings
                .GroupBy(r =>
                {
                    var name = species.First(s => s.Id == r.SpeciesId).Name;
                    return $"{r.SpeciesId}-{name}";
                })
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
