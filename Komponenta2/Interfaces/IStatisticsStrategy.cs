using Shared.Models;

namespace Komponenta2.Interfaces
{
    public interface IStatisticsStrategy
    {
        double CalculateStatistics(List<WaterQualityReading> readings);
    }
}
