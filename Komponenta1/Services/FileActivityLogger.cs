using System.Globalization;
using System.IO;
using Komponenta1.Interfaces;

namespace Komponenta1.Services;

public sealed class FileActivityLogger : IActivityLogger
{
    private readonly object _syncRoot = new();
    private readonly string _filePath;

    public FileActivityLogger()
        : this(Path.Combine(
            AppContext.BaseDirectory,
            "Logs",
            "activity.log"))
    {
    }

    public FileActivityLogger(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = Path.GetFullPath(filePath);
    }

    public void Log(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        try
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string entry =
                $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)} | " +
                $"{message}{Environment.NewLine}";

            lock (_syncRoot)
            {
                File.AppendAllText(_filePath, entry);
            }
        }
        catch
        {
            // Logging must never interrupt the application.
        }
    }
}
