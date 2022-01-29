﻿namespace Yeelight.Switch;

using System;
using System.Globalization;
using System.Windows.Data;

[ValueConversion(typeof(double), typeof(double))]
public class QuadScaleConverter : IValueConverter
{
    public double Min { get; set; } = 0;

    public double Max { get; set; } = 1;

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value
        .ToDouble()
        .Clamp(Min, Max)
        .Normalize(Min, Max)
        .SquareRoot()
        .Denormalize(Min, Max);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value
        .ToDouble()
        .Clamp(Min, Max)
        .Normalize(Min, Max)
        .Square()
        .Denormalize(Min, Max);
}

public static class Ext
{
    public static double ToDouble(this object? x) => Convert.ToDouble(x);

    public static double Clamp(this double x, double min, double max) => Math.Clamp(x, min, max);

    public static double Normalize(this double x, double min, double max) => (x - min) / (max - min);

    public static double Denormalize(this double x, double min, double max) => (x * (max - min)) + min;

    public static double Square(this double x) => x * x;

    public static double SquareRoot(this double x) => Math.Sqrt(x);
}
