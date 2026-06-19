using Komponenta2.Interfaces;
using Shared.Models;

namespace Komponenta2.Services.Strategies
{
    public class AveragePhStrategy : IStatisticsStrategy
    {
        public double CalculateStatistics(List<WaterQualityReading> readings)
        {
            return readings.Average(x => x.PHLevel);
        }
    }
}
