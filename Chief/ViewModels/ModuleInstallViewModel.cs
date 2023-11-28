using System;
using Chief.I18N;
using Chief.Models;
using Chief.Utils;

namespace Chief.ViewModels;

public class ModuleInstallViewModel(ModuleSource source) : ViewModelBase
{
    public ModuleInstallViewModel() : this(ModuleSource.Woolang)
    {
    }

    public string SourceName =>
        source switch
        {
            ModuleSource.Woolang =>
                Resource.ResourceManager.GetString("Woolang", Cache.Config.Instance!.GetAppCultureInfo())!,
            ModuleSource.Baozi => Resource.ResourceManager.GetString("Baozi",
                Cache.Config.Instance!.GetAppCultureInfo())!,
            ModuleSource.JoyEngine => Resource.ResourceManager.GetString("JoyECS",
                Cache.Config.Instance!.GetAppCultureInfo())!,
            _ => throw new ArgumentOutOfRangeException()
        };
}