#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows;
using TombLib.LevelData;
using TombLib.WPF;
using LevelSettingsData = TombLib.LevelData.LevelSettings;

namespace TombEditor.Features.Dialogs.LevelSettings;

/// <summary>Base class of the grid rows whose file can be reconnected with the shared reload logic of the editor.</summary>
public abstract class ReloadableRow : ObservableObject
{
    /// <summary>Raises a change notification for every displayed column.</summary>
    public void Refresh() => OnPropertyChanged(string.Empty);

    /// <summary>Asks for a new file and reconnects the resource (and other missing siblings) with the shared reload logic.</summary>
    protected void ReloadFromNewFile(LevelSettingsData settings, IReloadableResource resource, Action? changed = null)
        => EditorActions.ReloadResource(WPFUtils.GetWindowOwner(), settings, resource, true, true, () => RefreshOnUiThread(changed));

    private void RefreshOnUiThread(Action? changed)
        => Application.Current.Dispatcher.BeginInvoke(() =>
        {
            Refresh();
            changed?.Invoke();
        });
}
