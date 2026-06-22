using Komponenta2.Interfaces;
using Shared.Models;

namespace Komponenta2.Services.Strategies
{
    public class MinimalOxygenStrategy : IStatisticsStrategy
    {
        public double CalculateStatistics(List<WaterQualityReading> readings)
        {
            return readings.Min(x => x.OxygenLevel);
        }
    }
}
