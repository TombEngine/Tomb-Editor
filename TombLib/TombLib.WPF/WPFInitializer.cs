using CustomMessageBox.WPF;
using Microsoft.Extensions.DependencyInjection;
using MvvmDialogs;
using System;
using System.Windows;
using System.Windows.Media;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombLib.WPF;

public static class WPFInitializer
{
	/// <summary>
	/// Initializes WPF application, DarkUI theme, and dependency injection services.
	/// </summary>
	/// <returns>A <see cref="ServiceCollection" /> with core services already registered.</returns>
	public static ServiceCollection InitializeWPF()
	{
		Localizer.Instance.LoadLanguage("en");

		// Initialize WPF resources
		var wpfApp = new Application
		{
			ShutdownMode = ShutdownMode.OnExplicitShutdown
		};

		// Add the DarkUI theme to the WPF application
		wpfApp.Resources.MergedDictionaries.Add(new ResourceDictionary
		{
			Source = new Uri("pack://application:,,,/DarkUI.WPF;component/Generic.xaml")
		});

		// Use DarkColours theme (default DarkUI look)
		wpfApp.Resources.MergedDictionaries.Add(new ResourceDictionary
		{
			Source = new Uri("pack://application:,,,/DarkUI.WPF;component/Dictionaries/DarkColors.xaml")
		});

		CMessageBox.WindowStyleOverride = (Style)wpfApp.Resources["CustomWindowStyle"];
		CMessageBox.UsePathIconsByDefault = true;

		if (wpfApp.TryFindResource("Brush_Background_Alternative") is SolidColorBrush brush)
			CMessageBox.DefaultButtonsPanelBackground = brush;

		// Setup dependency injection
		var services = new ServiceCollection();

		services.AddSingleton<IDialogService, DialogService>();
		services.AddSingleton<IMessageService, MessageBoxService>();
		services.AddTransient<ILocalizationService, LocalizationService>();

		return services;
	}
}
