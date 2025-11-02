namespace Yeelight.Switch;

using System.Windows.Media;

using Nito.Mvvm;

using Yeelight.Switch.Sync;

using YeelightAPI;
using YeelightAPI.Models;

public class Main : ViewModel {
    private Device? yeelight;

    private Syncer? syncer;

    public Log Log { get; } = new Log();

    #region Init

    public NotifyTask Init { get; }

    public Main() => Init = NotifyTask.Create(InitDevice);

    private async Task InitDevice() {
        try {
            DeviceLocator.MaxRetryCount = 3;
            var devices = await Log.Task(DeviceLocator.DiscoverAsync(), $"searching for devices");

            yeelight = devices.FirstOrDefault() ?? new Device("192.168.0.213"); // throw new InvalidOperationException("No device found.");

            //device.OnNotificationReceived += LogDeviceNotification;
            yeelight.OnError += Log.DeviceError;

            _ = await Log.Task(yeelight.Connect(), $"connecting to {yeelight}");

            var power = await Log.Task(yeelight.GetProp(PROPERTIES.power), $"reading power");
            Power = power.Equals("on");

            var brightness = await Log.Task(yeelight.GetProp(PROPERTIES.bright), $"reading brightness");
            Brightness = int.Parse((string)brightness);

            var coltemp = await Log.Task(yeelight.GetProp(PROPERTIES.ct), $"reading color temperature");
            ColorTemp = int.Parse((string)coltemp);

            if (Power) _ = await Log.Task(yeelight.StartMusicMode(), $"starting music mode");

            syncer = new(yeelight);
        } catch (Exception ex) {
            Log.Newline();
            Log.Error($"failed to initialize: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Command execution

    private bool commandExecuting;

    public async Task Exec(Action task, FormattableString msg)
        => await Exec(_ => { task(); return Task.FromResult(true); }, msg);

    public async Task Exec(Func<Task> task, FormattableString msg)
        => await Exec(async _ => { await task(); return true; }, msg);

    public async Task Exec(Func<Device, Task<bool>> task, FormattableString msg) {
        if (yeelight is null || !Init.IsSuccessfullyCompleted || commandExecuting)
            return;

        try {
            commandExecuting = true;
            _ = await Log.Task(task(yeelight), msg);
        } catch (Exception ex) {
            Log.Newline();
            Log.Error($"failure when {msg}: {ex.Message}");
        } finally {
            commandExecuting = false;
        }
    }

    #endregion

    #region Power

    private bool power;
    public bool Power {
        get => power;
        set {
            _ = SetProp(ref power, value);
            _ = SetPower(value);
            RaisePropertyChanged(nameof(PowerButtonTooltip));
        }
    }

    public string PowerButtonTooltip => $"turn {(Power ? "off" : "on")}";

    private async Task SetPower(bool power) {
        if (power) {
            await Exec(d => d.TurnOn(), $"turning device on");
            await Exec(d => d.StartMusicMode(), $"starting music mode");
        } else {
            await Exec(d => d.StopMusicMode(), $"stopping music mode");
            await Exec(d => d.TurnOff(), $"turning device off");
        }
    }

    #endregion

    #region Brightness

    public const int MinBrightness = 1;

    public const int MaxBrightness = 100;

    private int brightness = MinBrightness;
    public int Brightness {
        get => brightness;
        set {
            _ = SetProp(ref brightness, value);
            _ = SetBrightness(value);
        }
    }

    public DoubleCollection BrightnessTicks { get; } = [.. QuadScaleConverter.Range(MinBrightness, MaxBrightness)];

    private async Task SetBrightness(int brightness) {
        if (syncRunning)
            await Exec(() => syncer!.Brightness = brightness, $"setting sync brightness to {brightness}");
        else
            await Exec(d => d.SetBrightness(brightness), $"setting brightness to {brightness}");
    }

    #endregion

    #region Color temperature

    public const int MinColorTemp = 1700;

    public const int MaxColorTemp = 6500;

    private int colorTemp = MinColorTemp;
    public int ColorTemp {
        get => colorTemp;
        set {
            _ = SetProp(ref colorTemp, value);
            _ = SetColorTemp(value);
        }
    }

    public DoubleCollection ColorTempTicks { get; } = [.. Enumerable
        .Range(MinColorTemp, MaxColorTemp)
        .Select(x => x.ToDouble().SqrtScale(MinColorTemp, MaxColorTemp))];

    public async Task SetColorTemp(int coltemp) => await Exec(
        d => d.SetColorTemperature(coltemp),
        $"setting color temperature to {coltemp}");

    #endregion

    #region Sync toggle

    private bool syncRunning;
    public bool SyncRunning {
        get => syncRunning;
        set {
            _ = SetProp(ref syncRunning, value);
            _ = ToggleSync();
            RaisePropertyChanged(nameof(SyncButtonTooltip));
        }
    }

    public string SyncButtonTooltip => $"{(syncRunning ? "stop" : "start")} syncing";

    public async Task ToggleSync() => await Exec(async () => {
        if (syncer!.Running)
            await syncer.Stop();
        else
            syncer.Start();
    }, $"{(syncer!.Running ? "stopping" : "starting")} sync");

    #endregion

    #region Screen

    public string[] Screens { get; } = Syncer.ScreenNames;

    private string selectedScreen = Syncer.PrimaryScreenName;
    public string SelectedScreen {
        get => selectedScreen;
        set {
            _ = SetProp(ref selectedScreen, value);
            _ = SetScreen(value);
        }
    }

    private async Task SetScreen(string screen) => await Exec(() => syncer!.Screen = screen, $"setting sync screen to {screen}");

    #endregion

    #region Smooth

    private int smooth;
    public int Smooth {
        get => smooth;
        set {
            _ = SetProp(ref smooth, value);
            _ = SetSmooth(value);
        }
    }

    private async Task SetSmooth(int smooth) => await Exec(() => syncer!.Smooth = smooth, $"setting sync smooth to {smooth}");

    #endregion
}
