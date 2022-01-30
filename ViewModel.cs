namespace Yeelight.Switch;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

using Nito.Mvvm;

using YeelightAPI;
using YeelightAPI.Models;

public class ViewModel : INotifyPropertyChanged
{
    #region Initialization

    private Device? device;

    public NotifyTask Init { get; set; }

    public ViewModel() => Init = NotifyTask.Create(() => InitDevice());

    public async Task InitDevice()
    {
        try
        {
            DeviceLocator.MaxRetryCount = 3;
            var devices = await LogTask(DeviceLocator.DiscoverAsync(), $"searching for devices");

            device = devices.FirstOrDefault() ?? throw new InvalidOperationException("No device found.");

            //device.OnNotificationReceived += LogDeviceNotification;
            device.OnError += LogDeviceError;

            _ = await LogTask(device.Connect(), $"connecting to {device}");

            var brightness = await LogTask(device.GetProp(PROPERTIES.bright), $"reading brightness");
            Brightness = int.Parse((string)brightness);
        }
        catch (Exception ex)
        {
            LogNewline();
            LogError($"failed to initialize: {ex.Message}");
        }
    }

    #endregion Initialization

    #region Brightness

    public const int MinBrightness = 1;

    public const int MaxBrightness = 100;

    private bool settingBrightness;

    public bool DraggingBrightnessSlider { get; set; }

    private int brightness;
    public int Brightness
    {
        get => brightness;
        set
        {
            _ = SetProp(ref brightness, value);
            _ = SetBrightness(value);
        }
    }

    public DoubleCollection BrightnessTicks { get; } = new DoubleCollection(Enumerable
        .Range(MinBrightness, MaxBrightness)
        .Select(x => x.ToDouble().SqrtScale(MinBrightness, MaxBrightness)));

    public Task SetBrightness() => SetBrightness(Brightness);

    public async Task SetBrightness(int brightness)
    {
        if (settingBrightness || DraggingBrightnessSlider || device is null || Init.IsNotCompleted)
            return;

        try
        {
            settingBrightness = true;
            _ = await LogTask(device.SetBrightness(brightness), $"setting brightness to {brightness}");
        }
        catch (Exception ex)
        {
            LogNewline();
            LogError($"failed to set brightness: {ex.Message}");
        }
        finally
        {
            settingBrightness = false;
        }
    }

    #endregion Brightness

    #region Log

    private string log = "";
    public string Log { get => log; set => SetProp(ref log, value); }

    public void LogInfo(FormattableString msg) => Log += $"{msg}\n";

    public void LogError(FormattableString msg) => Log += $"ERROR: {msg}\n";

    public async Task<T> LogTask<T>(Task<T> task, FormattableString msg)
    {
        Log += $"{msg} ... ";
        var result = await task;
        Log += PrintResult(result);
        Log += '\n';
        return result;
    }

    public void LogDeviceNotification(object _, NotificationReceivedEventArgs e) => LogInfo($"notification received: {e.Result}");

    public void LogDeviceError(object _, UnhandledExceptionEventArgs e) => LogError($"{e.ExceptionObject}");

    public void LogNewline() => Log += '\n';

    private static string PrintResult<T>(T x) => x switch
    {
        bool b => b ? "done" : "FAILED",
        IEnumerable<Device> e => e.Count().ToString(),
        _ => x?.ToString() ?? ""
    };

    #endregion Log

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged([CallerMemberName] string? property = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    private bool SetProp<T>(ref T backingField, T value, [CallerMemberName] string? property = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
            return false;

        backingField = value;
        RaisePropertyChanged(property);
        return true;
    }

    #endregion INotifyPropertyChanged
}
