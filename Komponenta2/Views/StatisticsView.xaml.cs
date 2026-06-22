using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Komponenta2.ViewModels;

namespace Komponenta2.Views;

public partial class StatisticsView : Window
{
    private readonly StatisticsViewModel viewModel;

    public StatisticsView(StatisticsViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        DataContext = viewModel;

        Loaded += StatisticsView_Loaded;

        this.viewModel.ShowToastRequested += OnShowToastRequested;
    }

    private async void StatisticsView_Loaded(object sender, RoutedEventArgs e)
    {
        await viewModel.InitializeAsync();
    }

    private async void OnShowToastRequested()
    {
        await Dispatcher.InvokeAsync(async () =>
        {
            var show = (Storyboard)FindResource("ShowToast");
            var hide = (Storyboard)FindResource("HideToast");

            show.Begin();

            await Task.Delay(2500);

            hide.Begin();
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        viewModel.ShowToastRequested -= OnShowToastRequested;
        base.OnClosed(e);
    }
}