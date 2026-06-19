using Shared.Models;

namespace Komponenta1.Interfaces;

public interface IWaterQualityReadingRepository
{
    IReadOnlyList<WaterQualityReading> GetAll();

    WaterQualityReading? GetById(Guid id);

    void Add(WaterQualityReading reading);

    bool Update(WaterQualityReading reading);

    bool Remove(Guid id);

    void ReplaceAll(IEnumerable<WaterQualityReading> readings);
}
