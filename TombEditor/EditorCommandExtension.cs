#nullable enable

using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using System.Windows.Markup;
using TombLib.WPF;

namespace TombEditor;

/// <summary>
/// XAML markup extension that resolves a TombEditor <see cref="CommandObj"/> by name into an
/// <see cref="ICommand"/> bound to <c>Editor.Instance</c> and the currently active owner window.
/// </summary>
/// <remarks>
/// <para>
/// Mirrors the previous <c>CommandHandler.AssignCommandsToControls</c> pattern (Tag -> Command):
/// <code>&lt;Button Command="{e:EditorCommand SwitchObjectPlacementMode}" /&gt;</code>
/// </para>
/// <para>
/// Commands are registered in the static constructor of <c>CommandHandler</c>, so by the time
/// any WPF view requests one they are already available - no init ordering issue.
/// </para>
/// </remarks>
[MarkupExtensionReturnType(typeof(ICommand))]
public sealed class EditorCommandExtension : MarkupExtension
{
    [ConstructorArgument("name")]
    public string Name { get; set; }

    public EditorCommandExtension()
    {
        Name = string.Empty;
    }

    public EditorCommandExtension(string name)
    {
        Name = name;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(Name))
            return new RelayCommand(() => { }, () => false);

        var commandName = Name;
        return new RelayCommand(() =>
        {
            var command = CommandHandler.GetCommand(commandName);
            command?.Execute?.Invoke(new CommandArgs
            {
                Editor = Editor.Instance,
                Window = WPFUtils.GetWindowOwner(),
            });
        });
    }
}
