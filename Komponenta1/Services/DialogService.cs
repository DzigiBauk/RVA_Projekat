using System.Windows;
using Komponenta1.Interfaces;
using Microsoft.Win32;

namespace Komponenta1.Services;

public sealed class DialogService : IDialogService
{
    private const string JsonFilter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

    public string? SelectFileToOpen()
    {
        OpenFileDialog dialog = new()
        {
            Filter = JsonFilter,
            DefaultExt = ".json",
            CheckFileExists = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SelectFileToSave()
    {
        SaveFileDialog dialog = new()
        {
            Filter = JsonFilter,
            DefaultExt = ".json",
            AddExtension = true,
            FileName = "aquarium-data.json"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public bool Confirm(string message, string title)
    {
        return MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    public void ShowMessage(string message, string title)
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
