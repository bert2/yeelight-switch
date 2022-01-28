namespace Yeelight.Switch;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using MaterialDesignExtensions.Controls;

public partial class MainWindow : MaterialWindow
{
    private readonly ViewModel viewModel;

    private bool brigthnessSliderDragging;

    public MainWindow()
    {
        InitializeComponent();
        viewModel = new ViewModel();
        DataContext = viewModel;
    }

    private async void BrigthnessSlider_ValueChanged(object _, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!brigthnessSliderDragging) await viewModel.SetBrightness(e.NewValue);
    }

    private void BrigthnessSlider_DragStarted(object _, DragStartedEventArgs e)
        => brigthnessSliderDragging = true;

    private async void BrigthnessSlider_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        brigthnessSliderDragging = false;
        var slider = (Slider)sender;
        await viewModel.SetBrightness(slider.Value);
    }

    private void Log_TextChanged(object sender, TextChangedEventArgs e)
        => ((TextBoxBase)sender).ScrollToEnd();
}
