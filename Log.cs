namespace Yeelight.Switch;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using YeelightAPI;

public class Log : INotifyPropertyChanged
{
    private const int MaxLogLength = 10_000;

    private string text = "";

    public string Text
    {
        get => text;
        private set
        {
            if (value.Length > MaxLogLength) value = value[^MaxLogLength..];
            SetProp(ref text, value);
        }
    }

    public void Info(FormattableString msg) => Text += $"{msg}\n";

    public void Error(FormattableString msg) => Text += $"ERROR: {msg}\n";

    public async Task<T> Task<T>(Task<T> task, FormattableString msg)
    {
        Text += $"{msg} ... ";
        var result = await task;
        Text += PrintResult(result);
        Text += '\n';
        return result;
    }

    public void DeviceNotification(object _, NotificationReceivedEventArgs e) => Info($"notification received: {e.Result}");

    public void DeviceError(object _, UnhandledExceptionEventArgs e) => Error($"{e.ExceptionObject}");

    public void Newline() => Text += '\n';

    private static string PrintResult<T>(T x) => x switch
    {
        bool b => b ? "done" : "FAILED",
        IEnumerable<Device> e => e.Count().ToString(),
        _ => x?.ToString() ?? ""
    };


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
