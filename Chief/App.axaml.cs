using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Chief.Utils;
using Chief.ViewModels;
using Chief.Views;
using I18N.Avalonia;
using I18N.Avalonia.Interface;
using Splat;

namespace Chief;

public class App : Application
{
    public override void Initialize()
    {
        Cache.Config.LoadConfig();
        AvaloniaXamlLoader.Load(this);
    }

    public override void RegisterServices()
    {
        base.RegisterServices();
        Locator.CurrentMutable.RegisterLazySingleton(() => new Localizer(I18N.Resource.ResourceManager),
            typeof(ILocalizer));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}