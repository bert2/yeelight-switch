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

    private void BrigthnessSlider_DragStarted(object _, DragStartedEventArgs __)
        => viewModel.DraggingBrightnessSlider = true;

    private async void BrigthnessSlider_DragCompleted(object _, DragCompletedEventArgs __)
    {
        viewModel.DraggingBrightnessSlider = false;
        await viewModel.SetBrightness();
    }

    private void Log_TextChanged(object sender, TextChangedEventArgs _)
        => ((TextBoxBase)sender).ScrollToEnd();
}
