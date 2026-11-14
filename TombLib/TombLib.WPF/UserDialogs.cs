using CustomMessageBox.WPF;
using System;
using TombLib.Services;

namespace TombLib.WPF;

/// <summary>Shows themed WPF messages using framework independent dialog contracts.</summary>
public static class UserDialogs
{
    /// <summary>Shows a message owned by the corresponding application window.</summary>
    public static DialogChoice Show(IWindowOwner? owner, string message, string caption = "",
        DialogButtons buttons = DialogButtons.OK, DialogIcon icon = DialogIcon.None,
        DialogDefaultButton defaultButton = DialogDefaultButton.Button1)
    {
        var parent = WPFUtils.GetOwnerWindow(owner?.Handle ?? IntPtr.Zero);
        var messageButtons = Enum.Parse<CMessageBoxButtons>(buttons.ToString());
        var messageIcon = icon switch
        {
            DialogIcon.Error => CMessageBoxIcon.Error,
            DialogIcon.Warning => CMessageBoxIcon.Warning,
            DialogIcon.Information => CMessageBoxIcon.Information,
            DialogIcon.Question => CMessageBoxIcon.Question,
            _ => CMessageBoxIcon.None
        };
        var selectedButton = Enum.Parse<CMessageBoxDefaultButton>(defaultButton.ToString());
        var result = parent is null
            ? CMessageBox.Show(message, caption, messageButtons, messageIcon, selectedButton)
            : CMessageBox.Show(parent, message, caption, messageButtons, messageIcon, selectedButton);

        return Enum.Parse<DialogChoice>(result.ToString());
    }

    /// <summary>Shows a message owned by the active application window.</summary>
    public static DialogChoice Show(string message, string caption = "", DialogButtons buttons = DialogButtons.OK,
        DialogIcon icon = DialogIcon.None, DialogDefaultButton defaultButton = DialogDefaultButton.Button1)
        => Show(null, message, caption, buttons, icon, defaultButton);

    /// <summary>Shows a message with its icon and default confirmation button.</summary>
    public static DialogChoice Show(IWindowOwner? owner, string message, string caption, DialogIcon icon)
        => Show(owner, message, caption, DialogButtons.OK, icon);

    /// <summary>Explains that a file path contains unsupported characters.</summary>
    public static DialogChoice NonANSIFilePathError(IWindowOwner? owner)
        => Show(owner, "Filename or path is invalid. Please use standard characters.", "Wrong filename", DialogIcon.Error);

    /// <summary>Explains that an object's Lua name is already assigned.</summary>
    public static DialogChoice LuaNameAlreadyTakenError(IWindowOwner? owner)
        => Show(owner, "The value of Lua Name is already taken by another object", "Error", DialogIcon.Error);
}
