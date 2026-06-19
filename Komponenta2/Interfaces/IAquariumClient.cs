using Shared.Models;

namespace Komponenta2.Interfaces
{
    public interface IAquariumClient
    {
        Task<List<WaterQualityReading>> GetReadings(Guid speciesId, int month);
    }
}
