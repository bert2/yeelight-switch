namespace Yeelight.Switch;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using MaterialDesignExtensions.Controls;

public partial class MainWindow : MaterialWindow
{
    private readonly ViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();
        viewModel = new ViewModel();
        DataContext = viewModel;
    }

    private void Log_TextChanged(object sender, TextChangedEventArgs _)
        => ((TextBoxBase)sender).ScrollToEnd();

    private void SetMinBrightness(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => viewModel.Brightness = ViewModel.MinBrightness;

    private void SetMaxBrightness(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => viewModel.Brightness = ViewModel.MaxBrightness;

    private void SetMinTemperature(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => viewModel.ColorTemp = ViewModel.MinColorTemp;

    private void SetMaxTemperature(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => viewModel.ColorTemp = ViewModel.MaxColorTemp;
}
