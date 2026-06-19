using System.Collections.ObjectModel;
using Komponenta1.Commands;
using Komponenta1.Interfaces;
using Komponenta1.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Shared.Models;
using SkiaSharp;

namespace Komponenta1.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly IAquaticSpeciesRepository _speciesRepository;
    private readonly IWaterQualityReadingRepository _readingRepository;
    private readonly IAquariumDataService _dataService;
    private readonly IValidator<AquaticSpecies> _speciesValidator;
    private readonly IValidator<WaterQualityReading> _readingValidator;
    private readonly IAquaticSpeciesSearchService _speciesSearchService;
    private readonly IWaterQualityReadingSearchService _readingSearchService;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IDialogService _dialogService;
    private readonly IReadingSimulationService _simulationService;
    private readonly IActivityLogger _activityLogger;
    private readonly ICoreWcfHostService _coreWcfHostService;
    private readonly ObservableCollection<int> _optimalValues = [0];
    private readonly ObservableCollection<int> _acceptableValues = [0];
    private readonly ObservableCollection<int> _suboptimalValues = [0];
    private readonly ObservableCollection<int> _criticalValues = [0];

    private AquaticSpecies? _selectedSpecies;
    private WaterQualityReadingRow? _selectedReading;
    private Guid? _editingSpeciesId;
    private Guid? _editingReadingId;
    private string _speciesSearchText = string.Empty;
    private string _readingSearchText = string.Empty;
    private string _speciesName = string.Empty;
    private string _speciesScientificName = string.Empty;
    private string _speciesHabitat = string.Empty;
    private string _speciesWaterType = "Freshwater";
    private int _speciesAverageLifespan = 1;
    private Guid _readingSpeciesId;
    private DateTime _readingMeasurementTime = DateTime.Now;
    private double _readingPHLevel = 7;
    private double _readingTemperature = 24;
    private double _readingOxygenLevel = 8;
    private WaterQualityState _readingState = WaterQualityState.Optimal;
    private string _speciesValidationMessage = string.Empty;
    private string _readingValidationMessage = string.Empty;
    private string _statusMessage = "Ready";
    private bool _isSimulationEnabled;
    private int _optimalCount;
    private int _acceptableCount;
    private int _suboptimalCount;
    private int _criticalCount;
    private string _serviceStatus = "Stopped";

    public MainWindowViewModel(
        IAquaticSpeciesRepository speciesRepository,
        IWaterQualityReadingRepository readingRepository,
        IAquariumDataService dataService,
        IValidator<AquaticSpecies> speciesValidator,
        IValidator<WaterQualityReading> readingValidator,
        IAquaticSpeciesSearchService speciesSearchService,
        IWaterQualityReadingSearchService readingSearchService,
        ICommandExecutor commandExecutor,
        IDialogService dialogService,
        IReadingSimulationService simulationService,
        IActivityLogger activityLogger,
        ICoreWcfHostService coreWcfHostService)
    {
        _speciesRepository = speciesRepository;
        _readingRepository = readingRepository;
        _dataService = dataService;
        _speciesValidator = speciesValidator;
        _readingValidator = readingValidator;
        _speciesSearchService = speciesSearchService;
        _readingSearchService = readingSearchService;
        _commandExecutor = commandExecutor;
        _dialogService = dialogService;
        _simulationService = simulationService;
        _activityLogger = activityLogger;
        _coreWcfHostService = coreWcfHostService;
        _simulationService.ReadingStateChanged += OnReadingStateChanged;
        _coreWcfHostService.StatusChanged += OnServiceStatusChanged;
        _serviceStatus = _coreWcfHostService.Status;
        InitializeStateSeries();

        SaveSpeciesCommand = new RelayCommand(SaveSpecies, CanSaveSpecies);
        EditSpeciesCommand = new RelayCommand(EditSpecies, () => SelectedSpecies is not null);
        DeleteSpeciesCommand = new RelayCommand(DeleteSpecies, () => SelectedSpecies is not null);
        CancelSpeciesEditCommand = new RelayCommand(ResetSpeciesForm);

        SaveReadingCommand = new RelayCommand(SaveReading, CanSaveReading);
        EditReadingCommand = new RelayCommand(EditReading, () => SelectedReading is not null);
        DeleteReadingCommand = new RelayCommand(DeleteReading, () => SelectedReading is not null);
        CancelReadingEditCommand = new RelayCommand(ResetReadingForm);

        UndoCommand = new RelayCommand(Undo, () => _commandExecutor.CanUndo);
        RedoCommand = new RelayCommand(Redo, () => _commandExecutor.CanRedo);
        LoadCommand = new RelayCommand(LoadData);
        SaveCommand = new RelayCommand(SaveData);

        RefreshAll();
        ValidateSpeciesForm();
        ValidateReadingForm();
    }

    public string Title => "Aquarium Information System";

    public ObservableCollection<AquaticSpecies> Species { get; } = [];

    public ObservableCollection<AquaticSpecies> DisplayedSpecies { get; } = [];

    public ObservableCollection<WaterQualityReadingRow> DisplayedReadings { get; } = [];

    public ObservableCollection<ISeries> StateSeries { get; } = [];

    public IReadOnlyList<string> WaterTypes { get; } =
        ["Freshwater", "Saltwater", "Brackish"];

    public IReadOnlyList<WaterQualityState> WaterQualityStates { get; } =
        Enum.GetValues<WaterQualityState>();

    public RelayCommand SaveSpeciesCommand { get; }

    public RelayCommand EditSpeciesCommand { get; }

    public RelayCommand DeleteSpeciesCommand { get; }

    public RelayCommand CancelSpeciesEditCommand { get; }

    public RelayCommand SaveReadingCommand { get; }

    public RelayCommand EditReadingCommand { get; }

    public RelayCommand DeleteReadingCommand { get; }

    public RelayCommand CancelReadingEditCommand { get; }

    public RelayCommand UndoCommand { get; }

    public RelayCommand RedoCommand { get; }

    public RelayCommand LoadCommand { get; }

    public RelayCommand SaveCommand { get; }

    public AquaticSpecies? SelectedSpecies
    {
        get => _selectedSpecies;
        set
        {
            if (SetProperty(ref _selectedSpecies, value))
            {
                EditSpeciesCommand.NotifyCanExecuteChanged();
                DeleteSpeciesCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public WaterQualityReadingRow? SelectedReading
    {
        get => _selectedReading;
        set
        {
            if (SetProperty(ref _selectedReading, value))
            {
                EditReadingCommand.NotifyCanExecuteChanged();
                DeleteReadingCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string SpeciesSearchText
    {
        get => _speciesSearchText;
        set
        {
            if (SetProperty(ref _speciesSearchText, value))
            {
                RefreshSpeciesView();
            }
        }
    }

    public string ReadingSearchText
    {
        get => _readingSearchText;
        set
        {
            if (SetProperty(ref _readingSearchText, value))
            {
                RefreshReadingsView();
            }
        }
    }

    public string SpeciesName
    {
        get => _speciesName;
        set => SetSpeciesField(ref _speciesName, value);
    }

    public string SpeciesScientificName
    {
        get => _speciesScientificName;
        set => SetSpeciesField(ref _speciesScientificName, value);
    }

    public string SpeciesHabitat
    {
        get => _speciesHabitat;
        set => SetSpeciesField(ref _speciesHabitat, value);
    }

    public string SpeciesWaterType
    {
        get => _speciesWaterType;
        set => SetSpeciesField(ref _speciesWaterType, value);
    }

    public int SpeciesAverageLifespan
    {
        get => _speciesAverageLifespan;
        set
        {
            if (SetProperty(ref _speciesAverageLifespan, value))
            {
                ValidateSpeciesForm();
            }
        }
    }

    public Guid ReadingSpeciesId
    {
        get => _readingSpeciesId;
        set
        {
            if (SetProperty(ref _readingSpeciesId, value))
            {
                ValidateReadingForm();
            }
        }
    }

    public DateTime ReadingMeasurementTime
    {
        get => _readingMeasurementTime;
        set
        {
            if (SetProperty(ref _readingMeasurementTime, value))
            {
                ValidateReadingForm();
            }
        }
    }

    public double ReadingPHLevel
    {
        get => _readingPHLevel;
        set
        {
            if (SetProperty(ref _readingPHLevel, value))
            {
                ValidateReadingForm();
            }
        }
    }

    public double ReadingTemperature
    {
        get => _readingTemperature;
        set
        {
            if (SetProperty(ref _readingTemperature, value))
            {
                ValidateReadingForm();
            }
        }
    }

    public double ReadingOxygenLevel
    {
        get => _readingOxygenLevel;
        set
        {
            if (SetProperty(ref _readingOxygenLevel, value))
            {
                ValidateReadingForm();
            }
        }
    }

    public WaterQualityState ReadingState
    {
        get => _readingState;
        set => SetProperty(ref _readingState, value);
    }

    public string SpeciesValidationMessage
    {
        get => _speciesValidationMessage;
        private set => SetProperty(ref _speciesValidationMessage, value);
    }

    public string ReadingValidationMessage
    {
        get => _readingValidationMessage;
        private set => SetProperty(ref _readingValidationMessage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsSimulationEnabled
    {
        get => _isSimulationEnabled;
        set
        {
            if (!SetProperty(ref _isSimulationEnabled, value))
            {
                return;
            }

            if (value)
            {
                _simulationService.Start();
                StatusMessage = "Reading state simulation started.";
                _activityLogger.Log("Reading state simulation started.");
            }
            else
            {
                _simulationService.Stop();
                StatusMessage = "Reading state simulation stopped.";
                _activityLogger.Log("Reading state simulation stopped.");
            }

            OnPropertyChanged(nameof(SimulationStatus));
        }
    }

    public int OptimalCount
    {
        get => _optimalCount;
        private set => SetProperty(ref _optimalCount, value);
    }

    public int AcceptableCount
    {
        get => _acceptableCount;
        private set => SetProperty(ref _acceptableCount, value);
    }

    public int SuboptimalCount
    {
        get => _suboptimalCount;
        private set => SetProperty(ref _suboptimalCount, value);
    }

    public int CriticalCount
    {
        get => _criticalCount;
        private set => SetProperty(ref _criticalCount, value);
    }

    public string SimulationStatus =>
        IsSimulationEnabled ? "Running" : "Stopped";

    public string ServiceStatus
    {
        get => _serviceStatus;
        private set => SetProperty(ref _serviceStatus, value);
    }

    public string SpeciesFormTitle =>
        _editingSpeciesId.HasValue ? "Edit species" : "Add species";

    public string ReadingFormTitle =>
        _editingReadingId.HasValue ? "Edit reading" : "Add reading";

    private void SaveSpecies()
    {
        AquaticSpecies species = CreateSpeciesFromForm();
        ValidationResult result = _speciesValidator.Validate(
            species,
            _editingSpeciesId);

        if (!result.IsValid)
        {
            SpeciesValidationMessage = FormatErrors(result);
            _activityLogger.Log(
                $"Species validation failed: {SpeciesValidationMessage}");
            return;
        }

        IApplicationCommand command = _editingSpeciesId.HasValue
            ? new UpdateSpeciesCommand(_speciesRepository, species)
            : new AddSpeciesCommand(_speciesRepository, species);

        ExecuteCommand(command);
        ResetSpeciesForm();
    }

    private bool CanSaveSpecies()
    {
        return _speciesValidator
            .Validate(CreateSpeciesFromForm(), _editingSpeciesId)
            .IsValid;
    }

    private void EditSpecies()
    {
        if (SelectedSpecies is null)
        {
            return;
        }

        _editingSpeciesId = SelectedSpecies.Id;
        SpeciesName = SelectedSpecies.Name;
        SpeciesScientificName = SelectedSpecies.ScientificName;
        SpeciesHabitat = SelectedSpecies.Habitat;
        SpeciesWaterType = SelectedSpecies.WaterType;
        SpeciesAverageLifespan = SelectedSpecies.AverageLifespan;
        OnPropertyChanged(nameof(SpeciesFormTitle));
        ValidateSpeciesForm();
    }

    private void DeleteSpecies()
    {
        if (SelectedSpecies is null)
        {
            return;
        }

        int relatedReadings = _readingRepository
            .GetAll()
            .Count(reading => reading.SpeciesId == SelectedSpecies.Id);
        string message = relatedReadings == 0
            ? $"Delete '{SelectedSpecies.Name}'?"
            : $"Delete '{SelectedSpecies.Name}' and its {relatedReadings} related reading(s)?";

        if (!_dialogService.Confirm(message, "Delete species"))
        {
            return;
        }

        ExecuteCommand(
            new DeleteSpeciesCommand(
                _speciesRepository,
                _readingRepository,
                SelectedSpecies.Id));
        ResetSpeciesForm();
        ResetReadingForm();
    }

    private void ResetSpeciesForm()
    {
        _editingSpeciesId = null;
        SpeciesName = string.Empty;
        SpeciesScientificName = string.Empty;
        SpeciesHabitat = string.Empty;
        SpeciesWaterType = WaterTypes[0];
        SpeciesAverageLifespan = 1;
        OnPropertyChanged(nameof(SpeciesFormTitle));
        ValidateSpeciesForm();
    }

    private void SaveReading()
    {
        WaterQualityReading reading = CreateReadingFromForm();
        ValidationResult result = _readingValidator.Validate(
            reading,
            _editingReadingId);

        if (!result.IsValid)
        {
            ReadingValidationMessage = FormatErrors(result);
            _activityLogger.Log(
                $"Reading validation failed: {ReadingValidationMessage}");
            return;
        }

        IApplicationCommand command = _editingReadingId.HasValue
            ? new UpdateReadingCommand(_readingRepository, reading)
            : new AddReadingCommand(_readingRepository, reading);

        ExecuteCommand(command);
        ResetReadingForm();
    }

    private bool CanSaveReading()
    {
        return _readingValidator
            .Validate(CreateReadingFromForm(), _editingReadingId)
            .IsValid;
    }

    private void EditReading()
    {
        if (SelectedReading is null)
        {
            return;
        }

        WaterQualityReading reading = SelectedReading.Reading;
        _editingReadingId = reading.Id;
        ReadingSpeciesId = reading.SpeciesId;
        ReadingMeasurementTime = reading.MeasurementTime;
        ReadingPHLevel = reading.PHLevel;
        ReadingTemperature = reading.Temperature;
        ReadingOxygenLevel = reading.OxygenLevel;
        ReadingState = reading.State;
        OnPropertyChanged(nameof(ReadingFormTitle));
        ValidateReadingForm();
    }

    private void DeleteReading()
    {
        if (SelectedReading is null ||
            !_dialogService.Confirm(
                "Delete the selected reading?",
                "Delete reading"))
        {
            return;
        }

        ExecuteCommand(
            new DeleteReadingCommand(
                _readingRepository,
                SelectedReading.Id));
        ResetReadingForm();
    }

    private void ResetReadingForm()
    {
        _editingReadingId = null;
        ReadingSpeciesId = Species.FirstOrDefault()?.Id ?? Guid.Empty;
        ReadingMeasurementTime = DateTime.Now;
        ReadingPHLevel = 7;
        ReadingTemperature = 24;
        ReadingOxygenLevel = 8;
        ReadingState = WaterQualityState.Optimal;
        OnPropertyChanged(nameof(ReadingFormTitle));
        ValidateReadingForm();
    }

    private void ExecuteCommand(IApplicationCommand command)
    {
        try
        {
            _commandExecutor.Execute(command);
            StatusMessage = command.Description;
            RefreshAll();
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
            _activityLogger.Log(
                $"Operation failed: {exception.Message}");
            _dialogService.ShowMessage(exception.Message, "Operation failed");
        }

        RefreshCommandAvailability();
    }

    private void Undo()
    {
        _commandExecutor.Undo();
        StatusMessage = "Last operation undone.";
        RefreshAll();
        RefreshCommandAvailability();
    }

    private void Redo()
    {
        _commandExecutor.Redo();
        StatusMessage = "Last operation repeated.";
        RefreshAll();
        RefreshCommandAvailability();
    }

    private async void LoadData()
    {
        string? filePath = _dialogService.SelectFileToOpen();
        if (filePath is null)
        {
            return;
        }

        try
        {
            await _dataService.LoadAsync(filePath);
            _commandExecutor.Clear();
            ResetSpeciesForm();
            RefreshAll();
            ResetReadingForm();
            StatusMessage = $"Loaded {filePath}";
            _activityLogger.Log($"Loaded data from '{filePath}'.");
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
            _activityLogger.Log(
                $"Loading data from '{filePath}' failed: {exception.Message}");
            _dialogService.ShowMessage(exception.Message, "Load failed");
        }

        RefreshCommandAvailability();
    }

    private async void SaveData()
    {
        string? filePath = _dialogService.SelectFileToSave();
        if (filePath is null)
        {
            return;
        }

        try
        {
            await _dataService.SaveAsync(filePath);
            StatusMessage = $"Saved {filePath}";
            _activityLogger.Log($"Saved data to '{filePath}'.");
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
            _activityLogger.Log(
                $"Saving data to '{filePath}' failed: {exception.Message}");
            _dialogService.ShowMessage(exception.Message, "Save failed");
        }
    }

    private void RefreshAll()
    {
        ReplaceCollection(Species, _speciesRepository.GetAll());
        RefreshSpeciesView();
        RefreshReadingsView();
        RefreshDashboard();

        if (!_editingReadingId.HasValue &&
            ReadingSpeciesId == Guid.Empty &&
            Species.Count > 0)
        {
            ReadingSpeciesId = Species[0].Id;
        }
    }

    private void RefreshDashboard()
    {
        IReadOnlyList<WaterQualityReading> readings =
            _readingRepository.GetAll();

        OptimalCount = readings.Count(
            reading => reading.State == WaterQualityState.Optimal);
        AcceptableCount = readings.Count(
            reading => reading.State == WaterQualityState.Acceptable);
        SuboptimalCount = readings.Count(
            reading => reading.State == WaterQualityState.Suboptimal);
        CriticalCount = readings.Count(
            reading => reading.State == WaterQualityState.Critical);

        _optimalValues[0] = OptimalCount;
        _acceptableValues[0] = AcceptableCount;
        _suboptimalValues[0] = SuboptimalCount;
        _criticalValues[0] = CriticalCount;
    }

    private void OnReadingStateChanged(
        object? sender,
        ReadingStateChangedEventArgs eventArgs)
    {
        RefreshReadingsView();
        RefreshDashboard();
        StatusMessage =
            $"Reading {eventArgs.ReadingId} changed from " +
            $"{eventArgs.PreviousState} to {eventArgs.CurrentState}.";
        _activityLogger.Log(StatusMessage);
    }

    private void OnServiceStatusChanged(object? sender, EventArgs eventArgs)
    {
        ServiceStatus = _coreWcfHostService.Status;
    }

    private void InitializeStateSeries()
    {
        StateSeries.Add(CreateSeries(
            "Optimal",
            _optimalValues,
            new SKColor(34, 197, 94)));
        StateSeries.Add(CreateSeries(
            "Acceptable",
            _acceptableValues,
            new SKColor(59, 130, 246)));
        StateSeries.Add(CreateSeries(
            "Suboptimal",
            _suboptimalValues,
            new SKColor(249, 115, 22)));
        StateSeries.Add(CreateSeries(
            "Critical",
            _criticalValues,
            new SKColor(239, 68, 68)));
    }

    private static ISeries CreateSeries(
        string name,
        ObservableCollection<int> values,
        SKColor color)
    {
        return new PieSeries<int>
        {
            Name = name,
            Values = values,
            Fill = new SolidColorPaint(color),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 14,
            DataLabelsFormatter = point =>
                $"{point.Context.Series.Name}: {point.Coordinate.PrimaryValue:0}"
        };
    }

    private void RefreshSpeciesView()
    {
        ReplaceCollection(
            DisplayedSpecies,
            _speciesSearchService.Search(
                _speciesRepository.GetAll(),
                SpeciesSearchText));
        SelectedSpecies = null;
    }

    private void RefreshReadingsView()
    {
        IReadOnlyList<AquaticSpecies> species = _speciesRepository.GetAll();
        Dictionary<Guid, string> names = species.ToDictionary(
            item => item.Id,
            item => item.Name);
        IEnumerable<WaterQualityReadingRow> rows = _readingSearchService
            .Search(
                _readingRepository.GetAll(),
                species,
                ReadingSearchText)
            .Select(reading => new WaterQualityReadingRow(
                reading,
                names.GetValueOrDefault(reading.SpeciesId, "Unknown species")));

        ReplaceCollection(DisplayedReadings, rows);
        SelectedReading = null;
    }

    private AquaticSpecies CreateSpeciesFromForm()
    {
        return new AquaticSpecies
        {
            Id = _editingSpeciesId ?? Guid.NewGuid(),
            Name = SpeciesName.Trim(),
            ScientificName = SpeciesScientificName.Trim(),
            Habitat = SpeciesHabitat.Trim(),
            WaterType = SpeciesWaterType.Trim(),
            AverageLifespan = SpeciesAverageLifespan
        };
    }

    private WaterQualityReading CreateReadingFromForm()
    {
        return new WaterQualityReading
        {
            Id = _editingReadingId ?? Guid.NewGuid(),
            SpeciesId = ReadingSpeciesId,
            MeasurementTime = ReadingMeasurementTime,
            PHLevel = ReadingPHLevel,
            Temperature = ReadingTemperature,
            OxygenLevel = ReadingOxygenLevel,
            State = ReadingState
        };
    }

    private void ValidateSpeciesForm()
    {
        ValidationResult result = _speciesValidator.Validate(
            CreateSpeciesFromForm(),
            _editingSpeciesId);
        SpeciesValidationMessage = FormatErrors(result);
        SaveSpeciesCommand?.NotifyCanExecuteChanged();
    }

    private void ValidateReadingForm()
    {
        ValidationResult result = _readingValidator.Validate(
            CreateReadingFromForm(),
            _editingReadingId);
        ReadingValidationMessage = FormatErrors(result);
        SaveReadingCommand?.NotifyCanExecuteChanged();
    }

    private void RefreshCommandAvailability()
    {
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
        SaveSpeciesCommand.NotifyCanExecuteChanged();
        SaveReadingCommand.NotifyCanExecuteChanged();
    }

    private bool SetSpeciesField(
        ref string field,
        string value,
        [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref field, value, propertyName))
        {
            return false;
        }

        ValidateSpeciesForm();
        return true;
    }

    private static string FormatErrors(ValidationResult result)
    {
        return string.Join(
            Environment.NewLine,
            result.Errors.SelectMany(pair => pair.Value));
    }

    private static void ReplaceCollection<T>(
        ObservableCollection<T> target,
        IEnumerable<T> source)
    {
        target.Clear();
        foreach (T item in source)
        {
            target.Add(item);
        }
    }
}
