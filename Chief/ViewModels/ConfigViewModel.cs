using System.Collections.Generic;
using Avalonia;
using Chief.Models;
using Chief.Utils;
using ReactiveUI;

namespace Chief.ViewModels;

public class ConfigViewModel : PageViewModel
{
    private bool _isChinese = Cache.Config.Instance!.AppLanguage == "zh-CN";

    public static List<ThemeMode> Themes => new()
    {
        ThemeMode.Light,
        ThemeMode.Dark,
        ThemeMode.SyncWithSystem
    };

    public static List<SupportedLanguage> Languages => new()
    {
        SupportedLanguage.English,
        SupportedLanguage.Chinese,
        SupportedLanguage.Japanese
    };

    public ThemeMode CurrentTheme
    {
        get => Cache.Config.Instance!.AppTheme;
        set
        {
            if (value == ThemeMode.SyncWithSystem)
                Application.Current!.PlatformSettings!.ColorValuesChanged +=
                    MainWindowViewModel.Instance!.SystemThemeChangedEvent;
            else
                Application.Current!.PlatformSettings!.ColorValuesChanged -=
                    MainWindowViewModel.Instance!.SystemThemeChangedEvent;

            Cache.Config.Instance!.AppTheme = value;
            MainWindowViewModel.Instance!.CurrentTheme = ThemeConverter.ThemeMode2AppThemeVar(value);
            Cache.Config.Instance.Save();
        }
    }

    public bool IsChinese
    {
        get => _isChinese;
        set => this.RaiseAndSetIfChanged(ref _isChinese, value);
    }

    public SupportedLanguage CurrentLanguage
    {
        get => LanguageConverter.LanguageCode2SupportedLanguage(Cache.Config.Instance!.AppLanguage);
        set
        {
            IsChinese = value == SupportedLanguage.Chinese;
            Cache.Config.Instance!.AppLanguage = LanguageConverter.SupportedLanguage2LanguageCode(value);
            Cache.Config.Instance.Save();
        }
    }
}