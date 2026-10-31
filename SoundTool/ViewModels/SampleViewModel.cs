#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;

namespace SoundTool.ViewModels;

/// <summary>A sample filename and its resolved reference project location.</summary>
public partial class SampleViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _resolvedPath = string.Empty;

    public SampleViewModel(string fileName) => _fileName = fileName;
}
