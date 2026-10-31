using CustomMessageBox.WPF;
using DarkUI.WPF;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Media;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombLib.WPF;

public static class WPFInitializer
{
    /// <summary>
    /// Initializes WPF application, DarkUI theme, and dependency injection services.
    /// </summary>
    /// <param name="theme">Color theme to apply at startup. Can be swapped at runtime via <see cref="ThemeManager.Apply" />.</param>
    /// <returns>A <see cref="ServiceCollection" /> with core services already registered.</returns>
    public static ServiceCollection InitializeWPF(Theme theme = Theme.Dark)
    {
        Localizer.Instance.LoadLanguage("en");

        // Initialize WPF resources.
        var wpfApp = new Application
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };

        // Add the DarkUI control templates. Color brushes are added separately via
        // ThemeManager so the palette can be swapped without re-merging templates.
        wpfApp.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/DarkUI.WPF;component/Generic.xaml")
        });

        ThemeManager.Apply(theme);

        CMessageBox.WindowStyleOverride = (Style)wpfApp.Resources["CustomWindowStyle"];
        CMessageBox.UsePathIconsByDefault = true;

        if (wpfApp.TryFindResource("Brush_Background_Alternative") is SolidColorBrush brush)
            CMessageBox.DefaultButtonsPanelBackground = brush;

        // Set up dependency injection.
        var services = new ServiceCollection();

        services.AddSingleton<DialogViewRegistry>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageService, MessageBoxService>();
        services.AddTransient<ILocalizationService, LocalizationService>();

        return services;
    }
}
