using System.Windows.Threading;
using Komponenta1.Interfaces;
using Komponenta1.Models;
using Shared.Models;

namespace Komponenta1.Services;

public sealed class ReadingSimulationService : IReadingSimulationService
{
    private static readonly WaterQualityState[] AllStates =
        Enum.GetValues<WaterQualityState>();

    private readonly IAquaticSpeciesRepository _speciesRepository;
    private readonly IWaterQualityReadingRepository _readingRepository;
    private readonly DispatcherTimer _timer;
    private readonly Dictionary<Guid, HashSet<WaterQualityState>> _visitedStatesBySpecies = [];

    public ReadingSimulationService(
        IAquaticSpeciesRepository speciesRepository,
        IWaterQualityReadingRepository readingRepository)
    {
        _speciesRepository = speciesRepository;
        _readingRepository = readingRepository;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _timer.Tick += OnTimerTick;
    }

    public bool IsRunning => _timer.IsEnabled;

    public event EventHandler<ReadingGeneratedEventArgs>? ReadingGenerated;

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
        IReadOnlyList<AquaticSpecies> species = _speciesRepository.GetAll();
        IReadOnlyList<WaterQualityReading> readings =
            _readingRepository.GetAll();
        HashSet<Guid> activeSpeciesIds =
            species.Select(item => item.Id).ToHashSet();

        foreach (Guid removedId in _visitedStatesBySpecies.Keys
                     .Where(id => !activeSpeciesIds.Contains(id))
                     .ToList())
        {
            _visitedStatesBySpecies.Remove(removedId);
        }

        foreach (AquaticSpecies item in species)
        {
            WaterQualityReading baseline = GetLatestReadingOrDefault(
                item.Id,
                readings);
            WaterQualityState state = GetNextState(item.Id, baseline.State);
            WaterQualityReading generated = CreateGeneratedReading(
                baseline,
                state);

            _readingRepository.Add(generated);

            ReadingGenerated?.Invoke(
                this,
                new ReadingGeneratedEventArgs(
                    generated.Id,
                    generated.SpeciesId,
                    generated.MeasurementTime,
                    generated.State));
        }
    }

    private static WaterQualityReading GetLatestReadingOrDefault(
        Guid speciesId,
        IEnumerable<WaterQualityReading> readings)
    {
        return readings
            .Where(reading => reading.SpeciesId == speciesId)
            .OrderByDescending(reading => reading.MeasurementTime)
            .FirstOrDefault()
            ?? new WaterQualityReading
            {
                SpeciesId = speciesId,
                MeasurementTime = DateTime.Now,
                PHLevel = 7,
                Temperature = 24,
                OxygenLevel = 8,
                State = WaterQualityState.Optimal
            };
    }

    private WaterQualityState GetNextState(
        Guid speciesId,
        WaterQualityState previousState)
    {
        HashSet<WaterQualityState> visited = GetVisitedStates(speciesId);
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

        WaterQualityState nextState =
            candidates[Random.Shared.Next(candidates.Count)];
        visited.Add(nextState);
        return nextState;
    }

    private HashSet<WaterQualityState> GetVisitedStates(Guid speciesId)
    {
        if (!_visitedStatesBySpecies.TryGetValue(
                speciesId,
                out HashSet<WaterQualityState>? visited))
        {
            visited = [];
            _visitedStatesBySpecies[speciesId] = visited;
        }

        return visited;
    }

    private static WaterQualityReading CreateGeneratedReading(
        WaterQualityReading baseline,
        WaterQualityState state)
    {
        return new WaterQualityReading
        {
            Id = Guid.NewGuid(),
            SpeciesId = baseline.SpeciesId,
            MeasurementTime = DateTime.Now,
            PHLevel = Vary(
                baseline.PHLevel,
                0.2,
                WaterQualityReadingValidator.MinimumPH,
                WaterQualityReadingValidator.MaximumPH),
            Temperature = Vary(
                baseline.Temperature,
                0.5,
                WaterQualityReadingValidator.MinimumTemperature,
                WaterQualityReadingValidator.MaximumTemperature),
            OxygenLevel = Vary(
                baseline.OxygenLevel,
                0.3,
                WaterQualityReadingValidator.MinimumOxygen,
                WaterQualityReadingValidator.MaximumOxygen),
            State = state
        };
    }

    private static double Vary(
        double value,
        double maximumDelta,
        double minimum,
        double maximum)
    {
        double delta = (Random.Shared.NextDouble() * 2 - 1) * maximumDelta;
        double varied = Math.Clamp(value + delta, minimum, maximum);
        return Math.Round(varied, 2);
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        SimulateOnce();
    }
}
