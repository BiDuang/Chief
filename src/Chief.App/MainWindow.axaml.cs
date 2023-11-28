using System;
using Avalonia.Interactivity;
using Chief.App.Models;
using Chief.App.Services;
using Chief.App.Views;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace Chief.App;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();

        // not working in Linux
        // TitleBar.ExtendsContentIntoTitleBar = true;
        
        ViewModelLocator.Instance.Provider.GetRequiredService<NavigationService>().RegisterHandler(Navigate);

        var menu = new PageLink[]
        {
            new("Home", "Home", typeof(HomeView)),
            new("Toolchain", "Toolbox", typeof(ToolchainView)),
            new("Packages", "Apps", typeof(PackageView))
        };
        var footer = new PageLink[]
        {
            new("Settings","Settings",typeof(SettingsView)),
        };
        Navigation.MenuItemsSource = menu;
        Navigation.FooterMenuItemsSource = footer;
        Navigation.SelectedItem = Navigation.MenuItemsSource.ElementAt(0);
        Navigate(menu[0].Page);
    }

    private void Navigate(Type pageType, object? parameter = null, NavigationTransitionInfo? transition = null)
    {
        if (transition != null)
        {
            Root.Navigate(pageType, parameter);
        }
        else if (parameter != null)
        {
            Root.Navigate(pageType, parameter);
        }
        else
        {
            Root.Navigate(pageType);
        }
    }

    private void Navigation_OnItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer.Tag is PageLink link)
            ViewModelLocator.Instance.Provider.GetRequiredService<NavigationService>()
                .Navigate(link.Page, e.RecommendedNavigationTransitionInfo);
    }
}