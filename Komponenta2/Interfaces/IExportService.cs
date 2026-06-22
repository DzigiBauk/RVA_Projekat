using Shared.Models;

namespace Komponenta2.Interfaces
{
    public interface IExportService
    {
        Task ExportAsync(string path, List<WaterQualityReading> data);
    }
}
