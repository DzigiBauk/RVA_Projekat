using System.Windows;
using Komponenta1.ViewModels;

namespace Komponenta1.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
