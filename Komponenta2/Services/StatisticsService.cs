using Komponenta2.Interfaces;
using Shared.Models;

namespace Komponenta2.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsStrategyFactory factory;

        public StatisticsService(IStatisticsStrategyFactory factory)
        {
            this.factory = factory;
        }

        public double Calculate(string method, List<WaterQualityReading> readings)
        {
            var strategy = factory.Get(method);
            return strategy.CalculateStatistics(readings);
        }
    }
}
