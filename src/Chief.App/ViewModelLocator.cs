using System;
using Chief.App.Services;
using Chief.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Chief.App;

public class ViewModelLocator
{
    public IServiceProvider Provider { get; init; }

    public ViewModelLocator()
    {
        Provider = ConfigureServices();
    }

    private IServiceProvider ConfigureServices()
    {
        var container = new ServiceCollection();
        
        container.AddSingleton<NavigationService>();

        container.AddScoped<HomeViewModel>();
        
        return container.BuildServiceProvider();
    }

    public HomeViewModel Home => Provider.GetRequiredService<HomeViewModel>();
}