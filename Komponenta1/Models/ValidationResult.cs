namespace Komponenta1.Models;

public sealed class ValidationResult
{
    private readonly Dictionary<string, List<string>> _errors = [];

    public bool IsValid => _errors.Count == 0;

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors =>
        _errors.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<string>)pair.Value);

    public void AddError(string propertyName, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (!_errors.TryGetValue(propertyName, out List<string>? messages))
        {
            messages = [];
            _errors[propertyName] = messages;
        }

        messages.Add(message);
    }

    public IReadOnlyList<string> GetErrors(string propertyName)
    {
        return _errors.TryGetValue(propertyName, out List<string>? messages)
            ? messages
            : [];
    }
}
