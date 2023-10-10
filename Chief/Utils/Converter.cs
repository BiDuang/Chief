using System;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Styling;
using Chief.Models;

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

    public static ThemeVariant ThemeMode2AppThemeVar(ThemeMode themeMode)
    {
        return themeMode switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            ThemeMode.SyncWithSystem => PlatformThemeVar2AppThemeVar(Application.Current!.PlatformSettings!
                .GetColorValues().ThemeVariant),
            _ => throw new ArgumentOutOfRangeException(nameof(themeMode), themeMode, null)
        };
    }
}

public static class LanguageConverter
{
    public static string SupportedLanguage2LanguageCode(SupportedLanguage language)
    {
        return language switch
        {
            SupportedLanguage.English => "en-US",
            SupportedLanguage.Chinese => "zh-CN",
            SupportedLanguage.Japanese => "ja-JP",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };
    }

    public static SupportedLanguage LanguageCode2SupportedLanguage(string languageCode)
    {
        return languageCode switch
        {
            "en-US" => SupportedLanguage.English,
            "zh-CN" => SupportedLanguage.Chinese,
            "ja-JP" => SupportedLanguage.Japanese,
            _ => throw new ArgumentOutOfRangeException(nameof(languageCode), languageCode, null)
        };
    }
}