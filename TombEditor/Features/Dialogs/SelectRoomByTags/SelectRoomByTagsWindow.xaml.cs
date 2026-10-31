#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.SelectRoomByTags;

public partial class SelectRoomByTagsWindow : Window
{
    public SelectRoomByTagsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}
