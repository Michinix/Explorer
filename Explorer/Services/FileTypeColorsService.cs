using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace Explorer.Services;

public static class FileTypeColorsService
{
    private static readonly Dictionary<string, IBrush> Brushes = new();

    public static IBrush GetBrush(string type)
    {
        var key = string.IsNullOrEmpty(type) ? "—" : type;

        if (!Brushes.TryGetValue(key, out var brush))
        {
            var hue = Random.Shared.Next(360);
            brush = new SolidColorBrush(new HslColor(1, hue, 0.55, 0.50).ToRgb());
            Brushes[key] = brush;
        }

        return brush;
    }
}