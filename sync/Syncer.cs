namespace Yeelight.Switch.Sync;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YeelightAPI;

public class Syncer(Device device) {
    private CancellationTokenSource? cts;

    private Task? syncLoop;

    private Screenshot? screenshot;

    [MemberNotNullWhen(true, nameof(syncLoop), nameof(cts))]
    public bool Running { get; private set; }

    private string screen = PrimaryScreenName;
    public string Screen {
        get => screen;
        set {
            screen = value;
            screenshot = Screenshot.FromScreenName(Screen);
        }
    }

    public int Brightness { get; set; } = 100;

    private int? smooth;
    public int Smooth {
        get => smooth ?? 0;
        set => smooth = value <= 0 ? null : value;
    }

    public int Fps { get; set; } = 30;

    public int SampleStep { get; set; } = 2;

    public static string[] ScreenNames { get; } = [.. System.Windows.Forms.Screen.AllScreens.Select(s => s.DeviceName.TrimStart('\\', '.'))];

    public static string PrimaryScreenName { get; } = System.Windows.Forms.Screen.PrimaryScreen!.DeviceName.TrimStart('\\', '.');

    public void Start() {
        cts = new();
        syncLoop = Loop(cts.Token);
        Running = true;
    }

    public async Task Stop() {
        if (!Running) return;
        await cts.CancelAsync();
        await syncLoop;
        cts.Dispose();
        Running = false;
    }

    private async Task Loop(CancellationToken ct) {
        screenshot = Screenshot.FromScreenName(Screen);
        Color? prevColor = null;
        var prevBright = 0;
        var delay = 1000 / Fps;

        while (!ct.IsCancellationRequested) {
            screenshot.Refresh();

            var color = screenshot.GetAverageColor(SampleStep);
            var brightFactor = Math.Clamp(Brightness, 1, 100) / 100f;
            var bright = (color.GetBrightness() * brightFactor).Scale(1, 100);

            if (color.R < 10 && color.G < 10 && color.B < 10)
                color = Color.Black;

            if (color != prevColor)
                _ = await device.SetRGBColor(color.R, color.G, color.B, Smooth);

            if (bright != prevBright)
                _ = await device.SetBrightness(bright, Smooth);

            prevColor = color;
            prevBright = bright;

            await Task.Delay(delay, CancellationToken.None);
        }
    }
}
