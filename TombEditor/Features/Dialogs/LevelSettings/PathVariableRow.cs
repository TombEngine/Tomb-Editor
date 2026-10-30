#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using LevelSettingsData = TombLib.LevelData.LevelSettings;

namespace TombEditor.Features.Dialogs.LevelSettings;

/// <summary>Row view-model for the path variables grid; the resolved value follows the current settings.</summary>
public partial class PathVariableRow(LevelSettingsData settings, string variable) : ObservableObject
{
    public string Variable { get; } = variable;

    [ObservableProperty] private string _value = settings.ParseVariables(variable);

    /// <summary>Re-resolves the variable against the current settings.</summary>
    public void Refresh() => Value = settings.ParseVariables(Variable);
}
