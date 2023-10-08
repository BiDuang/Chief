using System.Windows.Input;
using Chief.Models;
using Chief.Views.Pages;
using ReactiveUI;

namespace Chief.ViewModels;

public class IndexViewModel : ViewModelBase
{
    public static ICommand GotoWoolangCommand => ReactiveCommand.Create(() =>
    {
        MainWindowViewModel.Instance!.CurrentContent = new ModuleManage
            { DataContext = new ModuleManageViewModel(ModuleSource.Woolang) };
    });

    public static ICommand GotoBaoziCommand => ReactiveCommand.Create(() =>
    {
        MainWindowViewModel.Instance!.CurrentContent = new ModuleManage
            { DataContext = new ModuleManageViewModel(ModuleSource.Baozi) };
    });

    public static ICommand GotoJoyEngineCommand => ReactiveCommand.Create(() =>
    {
        MainWindowViewModel.Instance!.CurrentContent = new ModuleManage
            { DataContext = new ModuleManageViewModel(ModuleSource.JoyEngine) };
    });
}