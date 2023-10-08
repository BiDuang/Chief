using System;
using Avalonia.Platform;
using Avalonia.Styling;

namespace Chief.Utils;

public static class ThemeConverter
{
    public static ThemeVariant PlatformThemeVar2AppThemeVar(PlatformThemeVariant systemTheme)
    {
        return systemTheme switch
        {
            PlatformThemeVariant.Light => ThemeVariant.Light,
            PlatformThemeVariant.Dark => ThemeVariant.Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(systemTheme), systemTheme, null)
        };
    }
}