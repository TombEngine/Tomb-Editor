#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ChooseRoom;

public partial class ChooseRoomWindow : Window
{
    public ChooseRoomWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}
