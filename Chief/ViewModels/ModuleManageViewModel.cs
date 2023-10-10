using System;
using Chief.I18N;
using Chief.Models;

namespace Chief.ViewModels;

public class ModuleManageViewModel : PageViewModel
{
    private readonly ModuleSource _source;
    public ModuleManageViewModel() => _source = ModuleSource.Woolang;
    public ModuleManageViewModel(ModuleSource source) => _source = source;

    public string SourceName =>
        _source switch
        {
            ModuleSource.Woolang => Resource.Woolang,
            ModuleSource.Baozi => Resource.Baozi,
            ModuleSource.JoyEngine => Resource.JoyECS,
            _ => throw new ArgumentOutOfRangeException()
        };
}