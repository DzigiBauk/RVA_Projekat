using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Komponenta1.Interfaces;
using Komponenta1.Models;

namespace Komponenta1.Services;

public sealed class AquariumDataService(
    IAquaticSpeciesRepository speciesRepository,
    IWaterQualityReadingRepository readingRepository) : IAquariumDataService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            speciesRepository.ReplaceAll([]);
            readingRepository.ReplaceAll([]);
            return;
        }

        await using FileStream stream = File.OpenRead(filePath);
        if (stream.Length == 0)
        {
            speciesRepository.ReplaceAll([]);
            readingRepository.ReplaceAll([]);
            return;
        }

        AquariumData? data = await JsonSerializer.DeserializeAsync<AquariumData>(
            stream,
            SerializerOptions,
            cancellationToken);

        speciesRepository.ReplaceAll(data?.Species ?? []);
        readingRepository.ReplaceAll(data?.Readings ?? []);
    }

    public async Task SaveAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string fullPath = Path.GetFullPath(filePath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        AquariumData data = new()
        {
            Species = speciesRepository.GetAll().ToList(),
            Readings = readingRepository.GetAll().ToList()
        };

        string temporaryPath = $"{fullPath}.{Guid.NewGuid():N}.tmp";

        try
        {
            await using (FileStream stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    data,
                    SerializerOptions,
                    cancellationToken);
            }

            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
