using Komponenta2.Interfaces;
using Komponenta2.Services.Strategies;

namespace Komponenta2.Services
{
    public class StatisticsStrategyFactory : IStatisticsStrategyFactory
    {
        public IStatisticsStrategy Get(string method)
        {
            return method switch
            {
                "Average PH" => new AveragePhStrategy(),
                "Minimal Oxygen" => new MinimalOxygenStrategy(),
                "Critical Count" => new CriticalCountStrategy(),
                _ => throw new ArgumentException("Invalid method", nameof(method))
            };
    }
    }
}
