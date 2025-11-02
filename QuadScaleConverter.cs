namespace Yeelight.Switch;

using System;
using System.Globalization;
using System.Windows.Data;

[ValueConversion(typeof(double), typeof(double))]
public class QuadScaleConverter : IValueConverter {
    public double Min { get; set; } = 0;

    public double Max { get; set; } = 1;

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value.ToDouble().SqrtScale(Min, Max);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value.ToDouble().SqScale(Min, Max);

    public static IEnumerable<double> Range(int min, int max)
        => Enumerable.Range(min, max).Select(x => x.ToDouble().SqrtScale(min, max));
}
