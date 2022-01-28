namespace Yeelight.Switch;

using System.ComponentModel;
using System.Runtime.CompilerServices;

using Nito.Mvvm;

using YeelightAPI;
using YeelightAPI.Models;

public class ViewModel : INotifyPropertyChanged
{
    private Device? device;

    private bool settingBrightness;

    private int brightness;
    public int Brightness { get => brightness; set => SetProp(ref brightness, value); }

    #region Initialization

    public NotifyTask Init { get; set; }

    public ViewModel() => Init = NotifyTask.Create(() => InitDevice());

    public async Task InitDevice()
    {
        LogInfo($"searching for device...");
        DeviceLocator.MaxRetryCount = 3;
        var devices = await DeviceLocator.DiscoverAsync();

        device = devices.FirstOrDefault() ?? throw new InvalidOperationException("No device found.");

        //device.OnNotificationReceived += LogDeviceNotification;
        device.OnError += LogDeviceError;

        await LogTask(device.Connect(), $"connecting to {device}");

        var brightness = await LogTask(device.GetProp(PROPERTIES.bright), $"reading brightness");
        Brightness = int.Parse((string)brightness);
    }

    #endregion Initialization

    public Task SetBrightness(double brightness) => SetBrightness((int)Math.Round(brightness));

    public async Task SetBrightness(int brightness)
    {
        if (settingBrightness) return;
        settingBrightness = true;
        await LogTask(device!.SetBrightness(brightness), $"setting brightness to {brightness}");
        settingBrightness = false;
    }

    #region Log

    private string log = "";
    public string Log { get => log; set => SetProp(ref log, value); }

    public void LogInfo(FormattableString msg) => Log += $"{msg}\n";

    public void LogError(FormattableString msg) => Log += $"ERROR: {msg}\n";

    public async Task LogTask(Task<bool> task, FormattableString msg)
    {
        Log += $"{msg} ... ";
        Log += await task ? "done" : "FAILED";
        Log += '\n';
    }

    public async Task<object> LogTask(Task<object> task, FormattableString msg)
    {
        Log += $"{msg} ... ";
        var result = await task;
        Log += result;
        Log += '\n';
        return result;
    }

    public void LogDeviceNotification(object _, NotificationReceivedEventArgs e) => LogInfo($"notification received: {e.Result}");

    public void LogDeviceError(object _, UnhandledExceptionEventArgs e) => LogError($"{e.ExceptionObject}");

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
