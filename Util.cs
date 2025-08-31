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
        => a == null && b == null
        || a?.Equals(b, StringComparison.OrdinalIgnoreCase) == true;

    public static bool ContainsI(this string? a, string b)
        => a?.Contains(b, StringComparison.OrdinalIgnoreCase) == true;

    public static string Print(this IEnumerable<Screen> screens)
        => screens.Select(s => $"'{s.DeviceName}'").Join(", ");

    public static int Scale(this float x, int min, int max)
        => (int)Math.Round(Math.Clamp(x * max, min, max));
}
