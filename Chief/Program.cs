using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.ReactiveUI;

namespace Chief;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .ConfigureFonts(fm => fm.AddFontCollection(
                new EmbeddedFontCollection(
                    new Uri("fonts:HarmonyOS_Sans_SC", UriKind.Absolute),
                    new Uri("avares://Chief/Assets/Fonts", UriKind.Absolute))))
            .With(new FontManagerOptions
            {
                DefaultFamilyName = "fonts:HarmonyOS_Sans_SC#HarmonyOS Sans SC",
                FontFallbacks = new FontFallback[]
                {
                    new() { FontFamily = new FontFamily("fonts:HarmonyOS_Sans_SC#HarmonyOS Sans SC") },
                }
            })
            .LogToTrace()
            .UseReactiveUI();
    }
}