using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Services;

public sealed class WaterQualityReadingRepository : IWaterQualityReadingRepository
{
    private readonly List<WaterQualityReading> _readings = [];
    private readonly object _syncRoot = new();

    public IReadOnlyList<WaterQualityReading> GetAll()
    {
        lock (_syncRoot)
        {
            return _readings.ToList();
        }
    }

    public WaterQualityReading? GetById(Guid id)
    {
        lock (_syncRoot)
        {
            return _readings.FirstOrDefault(reading => reading.Id == id);
        }
    }

    public void Add(WaterQualityReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        lock (_syncRoot)
        {
            if (_readings.Any(existing => existing.Id == reading.Id))
            {
                throw new InvalidOperationException(
                    $"A water-quality reading with ID '{reading.Id}' already exists.");
            }

            _readings.Add(reading);
        }
    }

    public bool Update(WaterQualityReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        lock (_syncRoot)
        {
            int index = _readings.FindIndex(existing => existing.Id == reading.Id);
            if (index < 0)
            {
                return false;
            }

            _readings[index] = reading;
            return true;
        }
    }

    public bool Remove(Guid id)
    {
        lock (_syncRoot)
        {
            int index = _readings.FindIndex(reading => reading.Id == id);
            if (index < 0)
            {
                return false;
            }

            _readings.RemoveAt(index);
            return true;
        }
    }

    public void ReplaceAll(IEnumerable<WaterQualityReading> readings)
    {
        ArgumentNullException.ThrowIfNull(readings);

        List<WaterQualityReading> replacement = readings.ToList();
        if (replacement.Select(item => item.Id).Distinct().Count() != replacement.Count)
        {
            throw new InvalidOperationException("Water-quality reading IDs must be unique.");
        }

        lock (_syncRoot)
        {
            _readings.Clear();
            _readings.AddRange(replacement);
        }
    }
}
