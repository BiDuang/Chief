using System.Windows.Input;
using Chief.PageTransitions;
using Chief.Views.Pages;

namespace Chief.ViewModels;

public class PageViewModel : ViewModelBase
{
    public ICommand GotoIndexCommand { get; } = ReactiveUI.ReactiveCommand.Create(() =>
    {
        MainWindowViewModel.Instance!.Navigate<Index, IndexViewModel>(new DrillTransition
            { Backward = true });
    });
}