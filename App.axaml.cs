using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using wizbotupdater.Services;
using wizbotupdater.ViewModels;
using wizbotupdater.Views;
using AsyncImageLoader.Loaders;
using AsyncImageLoader;

namespace wizbotupdater;

public partial class App : Application
{
    public static ServiceProvider Services { get; private set; } = null!;
    public static Window MainWindow { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Configure services
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register the JsonBotRepository as a singleton
        services.AddSingleton<IBotRepository, JsonBotRepository>();

        // Register the DialogService as a singleton
        services.AddSingleton<IDialogService, DialogService>();

        // Register the ImageLoaderService as a singleton
        services.AddSingleton<IAsyncImageLoader>(
            new DiskCachedWebImageLoader(Path.Combine(Path.GetFullPath(Path.GetTempPath()), "wizbotupdater-cache")));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            desktop.MainWindow = MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}