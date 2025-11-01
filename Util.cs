namespace Yeelight.Switch;

public static class Util
{
    public static IEnumerable<T> AsSingleton<T>(this T x)
    {
        yield return x;
    }

    public static string Join(this IEnumerable<string> strs, string sep = ", ")
        => string.Join(sep, strs);

    public static bool EqualsI(this string? a, string? b)
        => (a == null && b == null)
        || a?.Equals(b, StringComparison.OrdinalIgnoreCase) == true;

    public static bool ContainsI(this string? a, string b)
        => a?.Contains(b, StringComparison.OrdinalIgnoreCase) == true;

    public static string Print(this IEnumerable<Screen> screens)
        => screens.Select(s => $"'{s.DeviceName}'").Join(", ");

    public static int Scale(this float x, int min, int max)
        => (int)Math.Round(Math.Clamp(x * max, min, max));

    public static double ToDouble(this object? x) => Convert.ToDouble(x);

    public static double SqScale(this double x, double min, double max)
        => x.Clamp(min, max).Normalize(min, max).Sq().Denormalize(min, max);

    public static double SqrtScale(this double x, double min, double max)
        => x.Clamp(min, max).Normalize(min, max).Sqrt().Denormalize(min, max);

    public static double Clamp(this double x, double min, double max) => Math.Clamp(x, min, max);

    public static double Normalize(this double x, double min, double max) => (x - min) / (max - min);

    public static double Denormalize(this double x, double min, double max) => (x * (max - min)) + min;

    public static double Sq(this double x) => x * x;

    public static double Sqrt(this double x) => Math.Sqrt(x);
}
