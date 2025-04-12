using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using wizbotupdater.ViewModels;

namespace wizbotupdater;

public class ViewLocator : IDataTemplate
{

    public Control? Build(object? param)
    {
        if (param is null)
            return null;
        
        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
#pragma warning disable IL2057
        var type = Type.GetType(name);
#pragma warning restore IL2057

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
