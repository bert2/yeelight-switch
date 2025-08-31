namespace Yeelight.Switch.Sync;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using YeelightAPI;

public class Syncer(Device device)
{
    private CancellationTokenSource? cts;

    private Task? syncLoop;

    [MemberNotNullWhen(true, nameof(syncLoop), nameof(cts))]
    public bool Running { get; private set; }

    public string Screen { get; set; } = "2";

    public int Brightness { get; set; } = 100;

    public int Smooth { get; set; } = 300;

    public int Fps { get; set; } = 30;

    public int SampleStep { get; set; } = 2;

    public void Start()
    {
        cts = new();
        syncLoop = Loop(cts.Token);
        Running = true;
    }

    public async Task Stop()
    {
        if (!Running) return;
        await cts.CancelAsync();
        await syncLoop;
        cts.Dispose();
        Running = false;
    }

    private async Task Loop(CancellationToken ct)
    {
        var screenshot = Screenshot.FromScreenName(Screen);
        Color? prevColor = null;
        var prevBright = 0;
        var delay = 1000 / Fps;

        while (!ct.IsCancellationRequested)
        {
            screenshot.Refresh();

            var color = screenshot.GetAverageColor(SampleStep);
            var brightFactor = Math.Clamp(Brightness, 1, 100) / 100f;
            var bright = (color.GetBrightness() * brightFactor).Scale(1, 100);

            if (color.R < 10 && color.G < 10 && color.B < 10)
                color = Color.Black;

            if (color != prevColor)
                await device.SetRGBColor(color.R, color.G, color.B, Smooth);

            if (bright != prevBright)
                await device.SetBrightness(bright, Smooth);

            prevColor = color;
            prevBright = bright;

            await Task.Delay(delay, CancellationToken.None);
        }
    }
}
