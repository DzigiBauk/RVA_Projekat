using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Komponenta2.Commands;
using Komponenta2.Interfaces;
using Komponenta2.Services.Adapters;
using Shared.Models;

namespace Komponenta2.ViewModels;

public class StatisticsViewModel : INotifyPropertyChanged
{
    private readonly IAquariumClient client;
    private readonly WaterQualityAdapter adapter;
    private readonly IStatisticsService statisticsService;
    private readonly IExportService exportService;

    private Dictionary<string, List<WaterQualityReading>> data = new();

    private AquaticSpecies? selectedSpecies;
    private int selectedMonth;
    private string? selectedMethod;
    private double result;
    private string? statusMessage;

    public RelayCommand LoadCommand { get; }
    public RelayCommand CalculateCommand { get; }
    public RelayCommand ExportCommand { get; }

    public Action? ShowToastRequested;

    #region Collections

    public ObservableCollection<AquaticSpecies> Species { get; }

    public ObservableCollection<string> DisplayItems { get; }

    public ObservableCollection<string> Methods { get; }
    public ObservableCollection<int> Months { get; } = new()
    {
        1,2,3,4,5,6,7,8,9,10,11,12
    };

    #endregion

    public StatisticsViewModel(
        IAquariumClient client, 
        WaterQualityAdapter adapter,
        IStatisticsService statisticsService,
        IExportService exportService)
    {
        this.client = client;
        this.adapter = adapter;
        this.statisticsService = statisticsService;
        this.exportService = exportService; 

        Species = new ObservableCollection<AquaticSpecies>();
        DisplayItems = new ObservableCollection<string>();

        LoadCommand = new RelayCommand(async () => await LoadAsync());
        CalculateCommand = new RelayCommand(Calculate);
        ExportCommand = new RelayCommand(async () => await ExportAsync("export.csv"));

        Methods = new ObservableCollection<string>
        {
            "Average PH",
            "Minimal Oxygen",
            "Critical Count"
        };
    }   

    #region Selected Properties

    public AquaticSpecies? SelectedSpecies
    {
        get => selectedSpecies;
        set { selectedSpecies = value; OnPropertyChanged(); }
    }

    public int SelectedMonth
    {
        get => selectedMonth;
        set { selectedMonth = value; OnPropertyChanged(); }
    }

    public string? SelectedMethod
    {
        get => selectedMethod;
        set { selectedMethod = value; OnPropertyChanged(); }
    }

    public string? StatusMessage
    {
        get => statusMessage;
        set 
        { 
            statusMessage = value; 
            OnPropertyChanged(); 
            ShowToastRequested?.Invoke();
        }
    }

    #endregion

    #region Result

    public double Result
    {
        get => result;
        set { result = value; OnPropertyChanged(); }
    }

    #endregion

    #region Methods

    public async Task InitializeAsync()
    {
        var species = await client.GetSpecies();

        Species.Clear();

        foreach (var s in species)
            Species.Add(s);
    }

    public async Task LoadAsync()
    {
        if (SelectedSpecies is null)
            return;

        var readings = await client.GetReadings(
            SelectedSpecies.Id,
            SelectedMonth);

        data = adapter.Adapt(readings, Species.ToList());

        FillDisplay();
    }

    public void Calculate()
    {
        if (SelectedMethod is null)
            return;

        var all = data.Values.SelectMany(x => x).ToList();

        Result = statisticsService.Calculate(SelectedMethod, all);
    }
    public async Task ExportAsync(string path)
    {
        try
        {
            var all = data.Values.SelectMany(x => x).ToList();

            await exportService.ExportAsync(path, all);

            StatusMessage = "Export uspešno završen ✔";
        }
        catch (Exception ex)
        {
            StatusMessage = "Export neuspešan: " + ex.Message;
        }
    }

    private void FillDisplay()
    {
        DisplayItems.Clear();

        foreach (var group in data)
        {
            foreach (var r in group.Value)
            {
                DisplayItems.Add(
                    $"({r.SpeciesId}, {r.MeasurementTime:HH:mm dd/MM/yyyy}) -> " +
                    $"[{r.PHLevel}, {r.Temperature}, {r.OxygenLevel}]");
            }
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    #endregion
}