#nullable enable

using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using TombEditor.Features.Toolbar;
using TombLib.LevelData;
using TombLib.WPF;

namespace TombEditor;

/// <summary>
/// Attached properties that bind a WPF <see cref="ButtonBase"/> (Button, ToggleButton...) to a
/// registered TombEditor <c>CommandHandler</c> command and, for <see cref="ToggleButton"/>,
/// to a boolean flag on <see cref="Configuration"/> or to <see cref="Editor.Mode"/>. The
/// MainView previous toolbar binds via <c>Tag -> Command</c> in
/// <c>CommandHandler.AssignCommandsToControls</c> and updates <c>Checked</c> state from
/// <c>RefreshControls(settings)</c>; this attached collection provides the same behaviour
/// declaratively for WPF toolbars.
/// </summary>
public static class EditorToolButton
{
    #region Command (Button + ToggleButton)

    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command",
        typeof(string),
        typeof(EditorToolButton),
        new PropertyMetadata(null, OnCommandChanged));

    public static string? GetCommand(DependencyObject obj) => (string?)obj.GetValue(CommandProperty);
    public static void SetCommand(DependencyObject obj, string? value) => obj.SetValue(CommandProperty, value);

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ButtonBase button || e.NewValue is not string commandName || string.IsNullOrWhiteSpace(commandName))
            return;

        CommandObj? cmd;
        try
        {
            cmd = CommandHandler.GetCommand(commandName);
        }
        catch
        {
            return;
        }

        if (cmd is null)
            return;

        var hotkeys = Editor.Instance.Configuration.UI_Hotkeys[commandName];
        var hotkeyDisplay = string.Join(", ",
            hotkeys.Select(h => h.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)));

        button.ToolTip = string.IsNullOrEmpty(hotkeyDisplay)
            ? cmd.FriendlyName
            : cmd.FriendlyName + " (" + hotkeyDisplay + ")";

        button.Command = new RelayCommand(() =>
        {
            cmd.Execute?.Invoke(new CommandArgs
            {
                Editor = Editor.Instance,
                Window = WPFUtils.GetWindowOwner(),
            });
        });

        ToolbarEnableRules.Attach(button, commandName);
    }

    #endregion

    #region ConfigFlag (ToggleButton <-> bool property on Configuration)

    public static readonly DependencyProperty ConfigFlagProperty = DependencyProperty.RegisterAttached(
        "ConfigFlag",
        typeof(string),
        typeof(EditorToolButton),
        new PropertyMetadata(null, OnConfigFlagChanged));

    public static string? GetConfigFlag(DependencyObject obj) => (string?)obj.GetValue(ConfigFlagProperty);
    public static void SetConfigFlag(DependencyObject obj, string? value) => obj.SetValue(ConfigFlagProperty, value);

    private static void OnConfigFlagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ToggleButton toggle || e.NewValue is not string flagName || string.IsNullOrWhiteSpace(flagName))
            return;

        var prop = typeof(Configuration).GetProperty(flagName, BindingFlags.Public | BindingFlags.Instance);
        if (prop is null || prop.PropertyType != typeof(bool))
            return;

        ApplyConfigFlag(toggle, prop);
        SubscribeWeak(toggle, ev =>
        {
            if (ev is Editor.ConfigurationChangedEvent or Editor.InitEvent)
                ApplyConfigFlag(toggle, prop);
        });
    }

    private static void ApplyConfigFlag(ToggleButton toggle, PropertyInfo prop)
    {
        var value = (bool)prop.GetValue(Editor.Instance.Configuration)!;
        if (toggle.IsChecked != value)
            toggle.IsChecked = value;
    }

    #endregion

    #region ActiveMode (ToggleButton <-> Editor.Mode)

    public static readonly DependencyProperty ActiveModeProperty = DependencyProperty.RegisterAttached(
        "ActiveMode",
        typeof(EditorMode?),
        typeof(EditorToolButton),
        new PropertyMetadata(null, OnActiveModeChanged));

    public static EditorMode? GetActiveMode(DependencyObject obj) => (EditorMode?)obj.GetValue(ActiveModeProperty);
    public static void SetActiveMode(DependencyObject obj, EditorMode? value) => obj.SetValue(ActiveModeProperty, value);

    private static void OnActiveModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ToggleButton toggle || e.NewValue is not EditorMode mode)
            return;

        ApplyMode(toggle, mode);
        SubscribeWeak(toggle, ev =>
        {
            if (ev is Editor.ModeChangedEvent or Editor.InitEvent)
                ApplyMode(toggle, mode);
        });
    }

    private static void ApplyMode(ToggleButton toggle, EditorMode mode)
    {
        var active = Editor.Instance.Mode == mode;
        if (toggle.IsChecked != active)
            toggle.IsChecked = active;
    }

    #endregion

    #region PortalOpacity / PortalEffect (ToggleButton <-> selected portal)

    public static readonly DependencyProperty PortalOpacityProperty = DependencyProperty.RegisterAttached(
        "PortalOpacity",
        typeof(PortalOpacity?),
        typeof(EditorToolButton),
        new PropertyMetadata(null, OnPortalOpacityChanged));

    public static PortalOpacity? GetPortalOpacity(DependencyObject obj) => (PortalOpacity?)obj.GetValue(PortalOpacityProperty);
    public static void SetPortalOpacity(DependencyObject obj, PortalOpacity? value) => obj.SetValue(PortalOpacityProperty, value);

    private static void OnPortalOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ToggleButton toggle || e.NewValue is not PortalOpacity opacity)
            return;

        ApplyPortalOpacity(toggle, opacity);
        SubscribeWeak(toggle, ev =>
        {
            if (ev is Editor.SelectedObjectChangedEvent or Editor.ObjectChangedEvent or Editor.InitEvent)
                ApplyPortalOpacity(toggle, opacity);
        });
    }

    private static void ApplyPortalOpacity(ToggleButton toggle, PortalOpacity opacity)
    {
        // Mirrors MainView.RefreshControls: the opacity buttons highlight the selected portal's
        // current mode and are only usable while a portal is selected.
        var portal = Editor.Instance.SelectedObject as PortalInstance;
        toggle.IsEnabled = portal is not null;

        var active = portal is not null && portal.Opacity == opacity;
        if (toggle.IsChecked != active)
            toggle.IsChecked = active;
    }

    public static readonly DependencyProperty PortalEffectProperty = DependencyProperty.RegisterAttached(
        "PortalEffect",
        typeof(PortalEffectType?),
        typeof(EditorToolButton),
        new PropertyMetadata(null, OnPortalEffectChanged));

    public static PortalEffectType? GetPortalEffect(DependencyObject obj) => (PortalEffectType?)obj.GetValue(PortalEffectProperty);
    public static void SetPortalEffect(DependencyObject obj, PortalEffectType? value) => obj.SetValue(PortalEffectProperty, value);

    private static void OnPortalEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ToggleButton toggle || e.NewValue is not PortalEffectType effect)
            return;

        ApplyPortalEffect(toggle, effect);
        SubscribeWeak(toggle, ev =>
        {
            if (ev is Editor.SelectedObjectChangedEvent or Editor.ObjectChangedEvent or Editor.InitEvent
                or Editor.LevelChangedEvent or Editor.GameVersionChangedEvent)
                ApplyPortalEffect(toggle, effect);
        });
    }

    private static void ApplyPortalEffect(ToggleButton toggle, PortalEffectType effect)
    {
        // The classic mirror effect is TombEngine-only, matching butMirror's enable rule in MainView.
        var portal = Editor.Instance.SelectedObject as PortalInstance;
        toggle.IsEnabled = portal is not null && (Editor.Instance.Level?.IsTombEngine ?? false);

        var active = portal is not null && portal.Effect == effect;
        if (toggle.IsChecked != active)
            toggle.IsChecked = active;
    }

    #endregion

    #region Weak event subscription

    /// <summary>
    /// Subscribes the given handler to <c>Editor.Instance.EditorEventRaised</c> while holding only
    /// a <see cref="WeakReference{T}"/> to the target button. When the button is collected the
    /// handler self-unsubscribes on the next event, so MainWindow lifecycles don't leak handlers.
    /// </summary>
    internal static void SubscribeWeak(ButtonBase toggle, Action<IEditorEvent> onEvent)
    {
        var weak = new WeakReference<ButtonBase>(toggle);
        Action<IEditorEvent>? handler = null;
        handler = ev =>
        {
            if (!weak.TryGetTarget(out _))
            {
                if (handler is not null)
                    Editor.Instance.EditorEventRaised -= handler;
                return;
            }
            onEvent(ev);
        };
        Editor.Instance.EditorEventRaised += handler;
    }

    #endregion
}
