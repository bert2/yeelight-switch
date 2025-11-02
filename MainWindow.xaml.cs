namespace Yeelight.Switch;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using MaterialDesignExtensions.Controls;

public partial class MainWindow : MaterialWindow
{
    private readonly Main viewModel;

    public MainWindow()
    {
        InitializeComponent();
        viewModel = new Main();
        DataContext = viewModel;
    }

    private void Log_TextChanged(object sender, TextChangedEventArgs _)
        => ((TextBoxBase)sender).ScrollToEnd();

    private void SetMinBrightness(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => viewModel.Brightness = Main.MinBrightness;

    private void SetMaxBrightness(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => viewModel.Brightness = Main.MaxBrightness;

    private void SetMinTemperature(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => viewModel.ColorTemp = Main.MinColorTemp;

    private void SetMaxTemperature(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => viewModel.ColorTemp = Main.MaxColorTemp;
}
