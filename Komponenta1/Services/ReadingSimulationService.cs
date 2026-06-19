using System.Windows.Threading;
using Komponenta1.Interfaces;
using Komponenta1.Models;
using Shared.Models;

namespace Komponenta1.Services;

public sealed class ReadingSimulationService : IReadingSimulationService
{
    private static readonly WaterQualityState[] AllStates =
        Enum.GetValues<WaterQualityState>();

    private readonly IWaterQualityReadingRepository _readingRepository;
    private readonly DispatcherTimer _timer;
    private readonly Dictionary<Guid, HashSet<WaterQualityState>> _visitedStates = [];

    public ReadingSimulationService(
        IWaterQualityReadingRepository readingRepository)
    {
        _readingRepository = readingRepository;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _timer.Tick += OnTimerTick;
    }

    public bool IsRunning => _timer.IsEnabled;

    public event EventHandler<ReadingStateChangedEventArgs>? ReadingStateChanged;

    public void Start()
    {
        if (!IsRunning)
        {
            _timer.Start();
        }
    }

    public void Stop()
    {
        if (IsRunning)
        {
            _timer.Stop();
        }
    }

    internal void SimulateOnce()
    {
        IReadOnlyList<WaterQualityReading> readings =
            _readingRepository.GetAll();
        HashSet<Guid> activeReadingIds =
            readings.Select(reading => reading.Id).ToHashSet();

        foreach (Guid removedId in _visitedStates.Keys
                     .Where(id => !activeReadingIds.Contains(id))
                     .ToList())
        {
            _visitedStates.Remove(removedId);
        }

        foreach (WaterQualityReading reading in readings)
        {
            WaterQualityState previousState = reading.State;
            HashSet<WaterQualityState> visited = GetVisitedStates(reading);
            List<WaterQualityState> candidates = AllStates
                .Where(state =>
                    state != previousState &&
                    !visited.Contains(state))
                .ToList();

            if (candidates.Count == 0)
            {
                visited.Clear();
                visited.Add(previousState);
                candidates = AllStates
                    .Where(state => state != previousState)
                    .ToList();
            }

            WaterQualityState currentState =
                candidates[Random.Shared.Next(candidates.Count)];
            reading.State = currentState;
            visited.Add(currentState);
            _readingRepository.Update(reading);

            ReadingStateChanged?.Invoke(
                this,
                new ReadingStateChangedEventArgs(
                    reading.Id,
                    previousState,
                    currentState));
        }
    }

    private HashSet<WaterQualityState> GetVisitedStates(
        WaterQualityReading reading)
    {
        if (!_visitedStates.TryGetValue(
                reading.Id,
                out HashSet<WaterQualityState>? visited))
        {
            visited = [reading.State];
            _visitedStates[reading.Id] = visited;
        }

        return visited;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        SimulateOnce();
    }
}
