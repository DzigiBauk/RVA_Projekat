using Shared.Models;

namespace Komponenta2.Interfaces
{
    public interface IStatisticsService
    {
        double Calculate(string method, List<WaterQualityReading> readings);
    }
}
