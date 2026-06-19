namespace Komponenta1.Interfaces;

public interface IDialogService
{
    string? SelectFileToOpen();

    string? SelectFileToSave();

    bool Confirm(string message, string title);

    void ShowMessage(string message, string title);
}
