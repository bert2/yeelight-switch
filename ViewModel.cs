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

    private Device? yeelight;

    public NotifyTask Init { get; set; }

    public ViewModel() => Init = NotifyTask.Create(() => InitDevice());

    public async Task InitDevice()
    {
        try
        {
            DeviceLocator.MaxRetryCount = 3;
            var devices = await LogTask(DeviceLocator.DiscoverAsync(), $"searching for devices");

            yeelight = devices.FirstOrDefault() ?? new Device("192.168.0.213"); // throw new InvalidOperationException("No device found.");

            //device.OnNotificationReceived += LogDeviceNotification;
            yeelight.OnError += LogDeviceError;

            _ = await LogTask(yeelight.Connect(), $"connecting to {yeelight}");

            var power = await LogTask(yeelight.GetProp(PROPERTIES.power), $"reading power");
            Power = power.Equals("on");

            var brightness = await LogTask(yeelight.GetProp(PROPERTIES.bright), $"reading brightness");
            Brightness = int.Parse((string)brightness);

            var coltemp = await LogTask(yeelight.GetProp(PROPERTIES.ct), $"reading color temperature");
            ColorTemperature = int.Parse((string)coltemp);
        }
        catch (Exception ex)
        {
            LogNewline();
            LogError($"failed to initialize: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Command execution

    private bool commandExecuting;

    public async Task Exec(Func<Device, Task<bool>> task, FormattableString msg)
    {
        if (yeelight is null || !Init.IsSuccessfullyCompleted || commandExecuting)
            return;

        try
        {
            commandExecuting = true;
            _ = await LogTask(task(yeelight), msg);
        }
        catch (Exception ex)
        {
            LogNewline();
            LogError($"failure when {msg}: {ex.Message}");
        }
        finally
        {
            commandExecuting = false;
        }
    }

    #endregion

    #region Power

    private bool power;
    public bool Power
    {
        get => power;
        set
        {
            _ = SetProp(ref power, value);
            _ = SetPower(value);
            RaisePropertyChanged(nameof(PowerButtonTooltip));
        }
    }

    public string PowerButtonTooltip => $"turn {(Power ? "off" : "on")}";

    public async Task SetPower(bool power) => await Exec(
        d => power ? d.TurnOn() : d.TurnOff(),
        $"turning device {(power ? "on" : "off")}");

    #endregion

    #region Brightness

    public const int MinBrightness = 1;

    public const int MaxBrightness = 100;

    private int brightness = MinBrightness;
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

    public async Task SetBrightness(int brightness) => await Exec(
        d => d.SetBrightness(brightness),
        $"setting brightness to {brightness}");

    #endregion

    #region Color temperature

    public const int MinColorTemperature = 1700;

    public const int MaxColorTemperature = 6500;

    private int colorTemperature = MinColorTemperature;
    public int ColorTemperature
    {
        get => colorTemperature;
        set
        {
            _ = SetProp(ref colorTemperature, value);
            _ = SetColorTemperature(value);
        }
    }

    public async Task SetColorTemperature(int coltemp) => await Exec(
        d => d.SetColorTemperature(coltemp),
        $"setting color temperature to {coltemp}");

    #endregion

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

    #endregion

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

    #endregion
}
