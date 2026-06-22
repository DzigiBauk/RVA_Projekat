namespace Komponenta2.Interfaces
{
    public interface IStatisticsStrategyFactory
    {
        IStatisticsStrategy Get(string method);
    }
}
