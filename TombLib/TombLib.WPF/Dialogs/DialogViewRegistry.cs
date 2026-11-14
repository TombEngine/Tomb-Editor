using System;
using System.Collections.Generic;
using System.Windows;
using TombLib.WPF.ViewModels;
using TombLib.WPF.Views;

namespace TombLib.WPF.Dialogs;

/// <summary>Maps modal view models to explicit WPF view factories.</summary>
public sealed class DialogViewRegistry
{
    private readonly Dictionary<Type, Func<Window>> _factories = [];

    /// <summary>Registers the shared editor dialogs.</summary>
    public DialogViewRegistry()
    {
        Register<InputBoxWindowViewModel>(() => new InputBoxWindow());
        Register<GeometryIOSettingsWindowViewModel>(() => new GeometryIOSettingsWindow());
    }

    /// <summary>Registers an application's dialog factory.</summary>
    public void Register<TViewModel>(Func<Window> factory) where TViewModel : IModalDialogViewModel
        => _factories[typeof(TViewModel)] = factory;

    /// <summary>Creates a view for a registered view model type.</summary>
    public Window Create(Type viewModelType)
        => _factories.TryGetValue(viewModelType, out var factory)
            ? factory()
            : throw new InvalidOperationException($"No dialog view is registered for {viewModelType.FullName}.");
}
