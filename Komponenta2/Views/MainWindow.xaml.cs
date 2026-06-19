using System.Windows;
using Komponenta2.ViewModels;

namespace Komponenta2.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
