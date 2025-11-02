namespace Yeelight.Switch;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using YeelightAPI;

public class Log : ViewModel {
    private const int MaxLogLength = 10_000;

    private string text = "";

    public string Text {
        get => text;
        private set {
            if (value.Length > MaxLogLength) value = value[^MaxLogLength..];
            _ = SetProp(ref text, value);
        }
    }

    public void Info(FormattableString msg) => Text += $"{msg}\n";

    public void Error(FormattableString msg) => Text += $"ERROR: {msg}\n";

    public async Task<T> Task<T>(Task<T> task, FormattableString msg) {
        Text += $"{msg} ... ";
        var result = await task;
        Text += PrintResult(result);
        Text += '\n';
        return result;
    }

    public void DeviceNotification(object _, NotificationReceivedEventArgs e) => Info($"notification received: {e.Result}");

    public void DeviceError(object _, UnhandledExceptionEventArgs e) => Error($"{e.ExceptionObject}");

    public void Newline() => Text += '\n';

    private static string PrintResult<T>(T x) => x switch {
        bool b => b ? "done" : "FAILED",
        IEnumerable<Device> e => e.Count().ToString(),
        _ => x?.ToString() ?? ""
    };
}
