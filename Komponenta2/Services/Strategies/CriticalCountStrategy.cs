using Komponenta2.Interfaces;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Komponenta2.Services.Strategies
{
    public class CriticalCountStrategy : IStatisticsStrategy
    {
        public double CalculateStatistics(List<WaterQualityReading> readings)
        {
            return readings.Count(r => r.State == WaterQualityState.Critical);
        }
    }
}
