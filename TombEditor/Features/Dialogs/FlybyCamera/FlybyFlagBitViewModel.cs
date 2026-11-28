#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;

namespace TombEditor.Features.Dialogs.FlybyCamera;

public sealed partial class FlybyFlagBitViewModel : ObservableObject
{
    [ObservableProperty] private bool _isChecked;

    public int Index { get; }
    public string Label { get; }

    public FlybyFlagBitViewModel(int index, string label, bool isChecked)
    {
        Index = index;
        Label = label;
        _isChecked = isChecked;
    }
}
