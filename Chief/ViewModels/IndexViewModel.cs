using System.Windows.Input;
using Chief.Models;
using Chief.PageTransitions;
using Chief.Views.Pages;
using ReactiveUI;

namespace Chief.ViewModels;

public class IndexViewModel : ViewModelBase
{
    public static ICommand GotoWoolangCommand => ReactiveCommand.Create(() =>
    {
        MainWindowViewModel.Instance!.Navigate<ModuleManage, ModuleManageViewModel>(
            new ModuleManageViewModel(ModuleSource.Woolang),
            transition: new DrillTransition());
    });

    public static ICommand GotoBaoziCommand => ReactiveCommand.Create(() =>
    {
        MainWindowViewModel.Instance!.Navigate<ModuleManage, ModuleManageViewModel>(
            new ModuleManageViewModel(ModuleSource.Baozi),
            transition: new DrillTransition());
    });

    public static ICommand GotoJoyEngineCommand => ReactiveCommand.Create(() =>
    {
        MainWindowViewModel.Instance!.Navigate<ModuleManage, ModuleManageViewModel>(
            new ModuleManageViewModel(ModuleSource.JoyEngine),
            transition: new DrillTransition());
    });
}