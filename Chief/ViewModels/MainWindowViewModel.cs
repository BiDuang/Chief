using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using I18N.Avalonia.Interface;
using ReactiveUI;

namespace Chief.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private static readonly Dictionary<ThemeVariant, Color> ThemeColors = new()
    {
        { ThemeVariant.Light, Colors.SeaShell },
        { ThemeVariant.Dark, Color.FromArgb(51, 46, 46, 20) }
    };

    private UserControl _currentContent = new Views.Pages.Index();
    private IPageTransition _transition = new CrossFade();

    private Color _themeColor = ThemeColors[Utils.ThemeConverter.PlatformThemeVar2AppThemeVar(Application.Current!
        .PlatformSettings!.GetColorValues().ThemeVariant)];

    public MainWindowViewModel(ILocalizer loc)
    {
        Instance = this;
        Application.Current!.PlatformSettings!.ColorValuesChanged += (_, _) =>
        {
            var currentTheme = Utils.ThemeConverter.PlatformThemeVar2AppThemeVar(Application.Current.PlatformSettings!
                .GetColorValues().ThemeVariant);
            BaseColor = ThemeColors[currentTheme];
        };
        loc.Language = Thread.CurrentThread.CurrentCulture.Name switch
        {
            "en-US" => new CultureInfo("en-US"),
            "zh-CN" => new CultureInfo("zh-CN"),
            "ja-JP" => new CultureInfo("ja-JP"),
            _ => new CultureInfo("en-US")
        };
    }

    public Color BaseColor
    {
        get => _themeColor;
        set => this.RaiseAndSetIfChanged(ref _themeColor, value);
    }

    public static MainWindowViewModel? Instance { get; set; }

    public UserControl CurrentContent
    {
        get => _currentContent;
        set => this.RaiseAndSetIfChanged(ref _currentContent, value);
    }

    public IPageTransition Transition
    {
        get => _transition;
        set => this.RaiseAndSetIfChanged(ref _transition, value);
    }

    public void Navigate<TV, TM>(IPageTransition? transition = null)
        where TV : UserControl, new()
        where TM : new()
    {
        Navigate<TV, TM>(new TM(), transition);
    }

    public void Navigate<TV, TM>(TM viewModel, IPageTransition? transition = null)
        where TV : UserControl, new()
    {
        Transition = transition ?? new CrossFade();
        CurrentContent = new TV()
        {
            DataContext = viewModel
        };
    }
}