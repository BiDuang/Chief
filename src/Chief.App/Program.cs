using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace Chief.App;

static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .ConfigureFonts(fm => fm.AddFontCollection(
                new EmbeddedFontCollection(
                    new Uri("fonts:HarmonyOS_Sans_SC", UriKind.Absolute),
                    new Uri("avares://Chief.App/Assets/Fonts/HarmonyOS_Sans_SC", UriKind.Absolute))))
            .With(new FontManagerOptions
            {
                DefaultFamilyName = "fonts:HarmonyOS_Sans_SC#HarmonyOS Sans SC",
                FontFallbacks = new FontFallback[]
                {
                    new() { FontFamily = new FontFamily("fonts:HarmonyOS_Sans_SC#HarmonyOS Sans") },
                }
            })
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}