using Komponenta2.Interfaces;
using Shared.Models;
using System.IO;

namespace Komponenta2.Services
{
    public class ExportService : IExportService
    {
        public async Task ExportAsync(string path, List<WaterQualityReading> data)
        {
            var lines = new List<string>();

            if(!File.Exists(path))
                lines.Add("SpeciesId,MeasurementTime,PHLevel,Temperature,OxygenLevel,State");

            foreach(var r in data)
            {
                lines.Add(
                    $"{r.SpeciesId}," +
                    $"{r.MeasurementTime:yyyy-MM-dd HH:mm:ss}," +
                    $"{r.PHLevel}," +
                    $"{r.Temperature}," +
                    $"{r.OxygenLevel}," +
                    $"{r.State}");
            }

            await File.AppendAllLinesAsync(path, lines);
        }
    }
}
