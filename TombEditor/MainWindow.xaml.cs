#nullable enable

using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using DarkUI.WPF.CustomControls;
using PopUpInfo = TombLib.WPF.PopUpInfo;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TombEditor.Controls.Panel3D;
using TombEditor.Features.MenuSearch;
using TombEditor.Features.MenuState;
using TombEditor.Features.Panel3D.ObjectBrush;
using TombEditor.Features.Panel3D.ToolPalette;
using TombLib.Input;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Controls.Rendering;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombLib.WPF.ViewModels;
using TombLib.WPF.Views;

namespace TombEditor;

public partial class MainWindow : Window
{

    private readonly Editor _editor;
    private readonly Panel3D _panel3D;
    private readonly FloatingToolbox _toolPalette;
    private readonly ToolboxView _toolPaletteView;
    private readonly MainMenuStateUpdater _menuState;
    private readonly FloatingToolbox _objectBrushToolbox;
    private readonly ObjectBrushToolboxView _objectBrushView;

    // Floating info/warning/error popup that animates in at the bottom-right of the 3D viewport.
    private readonly PopUpInfo _popup = new();

    private readonly IMessageService _messageService;
    private readonly ILocalizationService _localizationService;

    // XML snapshot of the dock layout as declared in MainWindow.xaml, captured the
    // first time the window finishes loading. Switching to "Default" deserializes
    // this back so the user can always recover the baseline arrangement.
    private string _defaultDockState = string.Empty;

    public MainWindow(Editor editor)
    {
        _editor = editor;
        _messageService = ServiceLocator.ResolveService<IMessageService>();
        _localizationService = ServiceLocator.ResolveService<ILocalizationService>();
        InitializeComponent();
        _menuState = new MainMenuStateUpdater(_editor, mainMenu, stepHeightSeparator);

        // The native WPF viewport owns the Direct3D swap chain and input.
        _panel3D = new Panel3D();
        _panel3D.InitializeRendering(
            _editor.RenderingDevice,
            _editor.Configuration.Rendering3D_Antialias,
            (ObjectRenderingQuality)_editor.Configuration.Rendering3D_ObjectQuality);

        panel3DHost.Child = _panel3D;

        importedGeometryBrowserView.InitializeRendering(_editor.RenderingDevice);
        itemBrowserView.InitializeRendering(_editor.RenderingDevice);

        contentBrowserView.AttachToEditor(_editor, data => _panel3D.DoDragDrop(data, TransferEffects.Copy));

        _toolPaletteView = new ToolboxView();
        _objectBrushView = new ObjectBrushToolboxView();
        _toolPalette = new FloatingToolbox(panel3DHost, _toolPaletteView) { IsShown = true };
        _objectBrushToolbox = new FloatingToolbox(panel3DHost, _objectBrushView, horizontalGrip: false);

        _editor.EditorEventRaised += OnEditorEventRaised;

        // Route global editor hotkeys (the WPF shell has no ProcessCmdKey). Without this, keys
        // pressed while the hosted Panel3D has focus never reach the editor command system.
        InstallHotkeyFilter();

        // Apply the current configuration to the freshly-created Panel3D so toolbar toggles
        // (DrawAllRooms, DrawPortals, etc.) reflect the persisted state on first paint.
        ApplyConfigurationToPanel3D(_editor.Configuration);

        flybyTimelineView.Initialize();
        ApplyFlybyTimelineVisibility();

        // Mirror FormMain.cs:66 - raise InitEvent now that all child views/panels have
        // subscribed, so they can pull initial state (depth bar bounds, toolbar checked
        // states, browser content, etc.). Without this the WPF shell skips InitEvent
        // entirely and several views never finish wiring up.
        _editor.RaiseEvent(new Editor.InitEvent());

        // Capture the XAML-declared dock arrangement as the "Default" layout, then
        // restore the user's previously-selected custom layout (if any). Loaded fires
        // once the visual tree is realized, which is when AvalonDock's layout root
        // is in its initial state.
        Loaded += OnLoaded;
        ContentRendered += OnContentRendered;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        _defaultDockState = SerializeDockState();

        var activeName = _editor.Configuration.Window_ActiveLayoutName;
        if (string.IsNullOrEmpty(activeName))
            return;

        var active = _editor.Configuration.Window_CustomLayouts.FirstOrDefault(l => l.Name == activeName);
        if (active is not null && !string.IsNullOrEmpty(active.AvalonDockState))
            LoadDockState(active.AvalonDockState);

        // Toolbox positions are applied in OnContentRendered, once Panel3D actually has its size.
    }

    private void OnContentRendered(object sender, System.EventArgs e)
    {
        ContentRendered -= OnContentRendered;

        ApplyToolboxPositionsFrom(ActiveLayout());
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        OnEditorEventForFloatingToolboxes(obj);
        _menuState.Handle(obj);

        if (obj is Editor.DefaultControlActivationEvent { ContainerName: "FormMain" })
        {
            tbSearchMenu.Focus();
            tbSearchMenu.SelectAll();
        }

        // Suspend/resume Panel3D painting around geometry rebuilds (e.g. SmartBuildGeometry while
        // dragging geometry, and geometry undo). Mirrors MainView's Suspend/ResumeRenderingEvent
        // handling - without it the panel keeps painting half-rebuilt geometry on the shared D3D
        // immediate context, which shows up as the 3D view fragmenting into horizontal stripes.
        if (obj is Editor.SuspendRenderingEvent)
            _panel3D.AllowRendering = false;
        else if (obj is Editor.ResumeRenderingEvent)
            _panel3D.AllowRendering = true;

        // Toolbar toggle commands flip a flag on Editor.Configuration and then raise
        // ConfigurationChangedEvent (see CommandHandler entries like "DrawAllRooms"). Mirror
        // MainView.RefreshControls(...) so the hosted Panel3D actually picks up the change.
        if (obj is Editor.ConfigurationChangedEvent || obj is Editor.InitEvent)
        {
            ApplyConfigurationToPanel3D(_editor.Configuration);
            ApplyFlybyTimelineVisibility();
        }

        // Keep the window title in sync with the open level + dirty state, mirroring
        // FormMain ("Tomb Editor <ver> - <level name>[*]"). The custom window chrome's
        // title bar TextBlock binds to Window.Title, so updating it is enough.
        if (obj is Editor.LevelFileNameChangedEvent
            or Editor.HasUnsavedChangesChangedEvent
            or Editor.LevelChangedEvent
            or Editor.InitEvent)
        {
            UpdateWindowTitle();
        }

        // Floating info/warning/error popup over the 3D viewport, mirroring MainView. Owner is
        // null so it always shows (the static guard only gates previous owners); messages are
        // dismissed when the level changes.
        if (obj is Editor.MessageEvent message)
            PopUpInfo.Show(_popup, null, _panel3D, message.Message, message.Type);

        if (obj is Editor.LevelChangedEvent)
            _popup.Hide();

        if (obj is Editor.ToolWindowToggleEvent toggle)
            ToggleAnchorable(ContentIdForPanelName(toggle.Name));

        // Bring the 3D view or the 2D map forward inside the nested editor dock when the mode switches.
        // They live in their own DockingManager under the shared toolbar, so the user can tab them or
        // split them side by side while the toolbar and statistics bar above stay a single instance.
        if (obj is Editor.ModeChangedEvent)
        {
            // Re-activating an already-active document still costs a full AvalonDock layout pass
            // (~350ms on a mid-size level), so skip it when switching between 3D-based modes
            // (Geometry/FaceEdit/Lighting/ObjectPlacement all share the 3D view document).
            if (_editor.Mode == EditorMode.Map2D)
            {
                if (!view2DDocument.IsActive)
                    view2DDocument.IsActive = true;
            }
            else if (!view3DDocument.IsActive)
            {
                view3DDocument.IsActive = true;
            }
        }

        if (obj is Editor.SwitchLayoutEvent layoutEvent)
            ApplyLayoutByIndex(layoutEvent.LayoutIndex);

        // Quit editor (File > Quit, Alt+F4 command), mirroring FormMain.
        if (obj is Editor.EditorQuitEvent)
            Close();
    }

    private static readonly string ProductVersion =
        FileVersionInfo.GetVersionInfo(typeof(MainWindow).Assembly.Location).ProductVersion ?? string.Empty;

    private void TbSearchMenu_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
            MenuSearchHelper.FindAndOpen(mainMenu, tbSearchMenu.Text);
    }

    private void ButFindMenu_Click(object sender, RoutedEventArgs e) => MenuSearchHelper.FindAndOpen(mainMenu, tbSearchMenu.Text);

    private void UpdateWindowTitle()
    {
        var level = _editor.Level;
        string levelName = level is null || string.IsNullOrEmpty(level.Settings.LevelFilePath)
            ? "Untitled"
            : PathC.GetFileNameWithoutExtensionTry(level.Settings.LevelFilePath);

        Title = "Tomb Editor " + ProductVersion + " - " + levelName + (_editor.HasUnsavedChanges ? "*" : "");
    }

    private void OnEditorEventForFloatingToolboxes(IEditorEvent obj)
    {
        if (obj is not (Editor.ToolChangedEvent or Editor.ModeChangedEvent or Editor.InitEvent))
            return;

        bool showBrush = _editor.Mode == EditorMode.ObjectPlacement;

        if (showBrush && !_objectBrushToolbox.IsShown)
        {
            var position = ActiveLayout().ObjectBrushToolboxPosition;
            _objectBrushToolbox.Position = new Point(position.X, position.Y);
        }
        else if (!showBrush && _objectBrushToolbox.IsShown)
        {
            ActiveLayout().ObjectBrushToolboxPosition = new System.Drawing.Point((int)_objectBrushToolbox.Position.X, (int)_objectBrushToolbox.Position.Y);
        }

        _objectBrushToolbox.IsShown = showBrush;
    }

    private void ApplyConfigurationToPanel3D(Configuration settings)
    {
        // Clear stale selection when the kind of object the user has selected becomes hidden,
        // otherwise the gizmo would float over nothing - same defensive logic MainView uses.
        if (!settings.Rendering3D_ShowStatics && _panel3D.ShowStatics && _editor.SelectedObject is StaticInstance)
            _editor.SelectedObject = null;
        if (!settings.Rendering3D_ShowMoveables && _panel3D.ShowMoveables && _editor.SelectedObject is MoveableInstance)
            _editor.SelectedObject = null;
        if ((!settings.Rendering3D_ShowImportedGeometry && _panel3D.ShowImportedGeometry)
            || (!settings.Rendering3D_DisablePickingForImportedGeometry && _panel3D.DisablePickingForImportedGeometry))
        {
            if (_editor.SelectedObject is ImportedGeometryInstance)
                _editor.SelectedObject = null;
        }
        if (!settings.Rendering3D_ShowGhostBlocks && _panel3D.ShowGhostBlocks && _editor.SelectedObject is GhostBlockInstance)
            _editor.SelectedObject = null;
        if (!settings.Rendering3D_ShowVolumes && _panel3D.ShowVolumes && _editor.SelectedObject is VolumeInstance)
            _editor.SelectedObject = null;
        if (!settings.Rendering3D_ShowOtherObjects && _panel3D.ShowOtherObjects)
        {
            if (_editor.SelectedObject is LightInstance
                or CameraInstance
                or FlybyCameraInstance
                or SinkInstance
                or SoundSourceInstance)
            {
                _editor.SelectedObject = null;
            }
        }

        _panel3D.ShowPortals = settings.Rendering3D_ShowPortals;
        _panel3D.ShowHorizon = settings.Rendering3D_ShowHorizon;
        _panel3D.ShowAllRooms = settings.Rendering3D_ShowAllRooms;
        _panel3D.ShowRoomNames = settings.Rendering3D_ShowRoomNames;
        _panel3D.ShowCardinalDirections = settings.Rendering3D_ShowCardinalDirections;
        _panel3D.ShowIllegalSlopes = settings.Rendering3D_ShowIllegalSlopes;
        _panel3D.ShowMoveables = settings.Rendering3D_ShowMoveables;
        _panel3D.ShowStatics = settings.Rendering3D_ShowStatics;
        _panel3D.ShowImportedGeometry = settings.Rendering3D_ShowImportedGeometry;
        _panel3D.ShowGhostBlocks = settings.Rendering3D_ShowGhostBlocks;
        _panel3D.ShowOtherObjects = settings.Rendering3D_ShowOtherObjects;
        _panel3D.ShowVolumes = settings.Rendering3D_ShowVolumes;
        _panel3D.ShowBoundingBoxes = settings.Rendering3D_ShowBoundingBoxes;
        _panel3D.ShowSlideDirections = settings.Rendering3D_ShowSlideDirections;
        _panel3D.ShowExtraBlendingModes = settings.Rendering3D_ShowExtraBlendingModes;
        _panel3D.HideTransparentFaces = settings.Rendering3D_HideTransparentFaces;
        _panel3D.BilinearFilter = settings.Rendering3D_BilinearFilter;
        _panel3D.DisablePickingForImportedGeometry = settings.Rendering3D_DisablePickingForImportedGeometry;
        _panel3D.DisablePickingForHiddenRooms = settings.Rendering3D_DisablePickingForHiddenRooms;
        _panel3D.ShowLightMeshes = settings.Rendering3D_ShowLightRadius;
        _panel3D.ShowLightingWhiteTextureOnly = settings.Rendering3D_ShowLightingWhiteTextureOnly;
        _panel3D.ShowRealTintForObjects = settings.Rendering3D_ShowRealTintForObjects;

        _panel3D.Invalidate();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Ask to save unsaved changes before quitting, mirroring FormMain.OnFormClosing.
        if (!EditorActions.ContinueOnFileDrop(this.GetWin32Window(), "Exit"))
            e.Cancel = true;

        base.OnClosing(e);
    }

    protected override void OnClosed(System.EventArgs e)
    {
        // Snapshot the user's tweaks (dock layout + floating toolbox positions) into the active
        // layout before disposing the toolboxes, so they survive restart.
        SaveCurrentStateToActiveLayout();

        _editor.EditorEventRaised -= OnEditorEventRaised;
        RemoveHotkeyFilter();

        // Each migrated WPF view subscribes to Editor.EditorEventRaised in its constructor;
        // Cleanup() unsubscribes and releases ViewModels. Without this the Editor leaks event
        // handlers across MainWindow lifetimes (and ViewModels go on receiving events).
        roomsView?.Cleanup();
        roomOptionsView?.Cleanup();
        sectorOptionsView?.Cleanup();
        lightingView?.Cleanup();
        paletteView?.Cleanup();
        triggerListView?.Cleanup();
        objectListView?.Cleanup();
        itemPropertiesView?.Cleanup();
        importedGeometryBrowserView?.Cleanup();
        itemBrowserView?.Cleanup();
        texturePanelView?.Cleanup();
        contentBrowserView?.Cleanup();
        flybyTimelineView?.Cleanup();
        statisticsBarView?.Cleanup();
        statusBarView?.Cleanup();
        panel2DMap?.Dispose();
        _popup?.Dispose();
        _toolPaletteView.Cleanup();
        _objectBrushView.Cleanup();
        _toolPalette.Dispose();
        _objectBrushToolbox.Dispose();
        _panel3D?.Dispose();

        base.OnClosed(e);

        // The WPF Application is created with ShutdownMode.OnExplicitShutdown (WPFInitializer), so
        // closing the main window does NOT end Application.Run on its own. Persist config and shut the
        // app down explicitly; Program.Main then force-terminates to kill the native rendering thread.
        _editor.Configuration.SaveTry();
        System.Windows.Application.Current?.Shutdown();
    }

    #region Tool window toggle

    // ToggleToolWindow(name) -> the AvalonDock ContentId is the panel name with a lowercased
    // first letter ("TexturePanel" -> "texturePanel"). Keep the LayoutAnchorable ContentIds in
    // XAML consistent with the names raised in Command.cs.
    private static string ContentIdForPanelName(string panelName)
    {
        if (string.IsNullOrEmpty(panelName))
            return string.Empty;
        return char.ToLowerInvariant(panelName[0]) + panelName.Substring(1);
    }

    private void ToggleAnchorable(string contentId)
    {
        if (string.IsNullOrEmpty(contentId))
            return;
        if (FindAnchorable(contentId) is { } anchorable)
        {
            if (anchorable.IsHidden)
                anchorable.Show();
            else
                anchorable.Hide();
        }
        else if (contentId == "itemProperties")
        {
            // Layouts saved by builds that predate this panel do not contain the anchorable
            // (OnLayoutSerializationCallback re-attaches by ContentId, so the XAML-declared one
            // is dropped on layout load). Recreate it docked left, like ApplyFlybyTimelineVisibility.
            new LayoutAnchorable
            {
                ContentId = "itemProperties",
                CanClose = false,
                Title = "Item properties",
                IconSource = TombLib.Icons.IconSources.Load("General/edit"),
                Content = itemPropertiesView,
            }.AddToLayout(dockManager, AnchorableShowStrategy.Left | AnchorableShowStrategy.Most);
        }
    }

    private LayoutAnchorable FindAnchorable(string contentId)
        => dockManager.Layout.Descendents().OfType<LayoutAnchorable>().FirstOrDefault(a => a.ContentId == contentId);

    private void ApplyFlybyTimelineVisibility()
    {
        bool shouldBeVisible = _editor.Configuration.Window_Layout.ShowFlybyTimeline;

        // Resolve by ContentId instead of the flybyTimelineAnchorable field: LoadDockState
        // replaces the whole layout tree, so after a layout switch/reset the XAML field points
        // at a detached instance whose Show()/Hide() no longer affect the visible layout.
        var anchorable = FindAnchorable("flybyTimeline");

        // Layouts serialized by builds where the timeline did not exist (or got dropped during
        // deserialization) do not contain the anchorable at all - recreate it docked at the
        // bottom so the Window-menu toggle always works.
        if (anchorable is null)
        {
            if (!shouldBeVisible)
                return;

            anchorable = new LayoutAnchorable
            {
                ContentId = "flybyTimeline",
                CanClose = false,
                Title = "Flyby timeline",
                IconSource = TombLib.Icons.IconSources.Load("Objects/movie_projector"),
                Content = flybyTimelineView,
            };
            anchorable.AddToLayout(dockManager, AnchorableShowStrategy.Bottom | AnchorableShowStrategy.Most);
            HookFlybyTimelineAnchorable(anchorable);
            return;
        }

        HookFlybyTimelineAnchorable(anchorable);

        if (shouldBeVisible && anchorable.IsHidden)
        {
            anchorable.Show();

            // AvalonDock silently ignores Show() when the hidden anchorable's previous container
            // no longer exists (it was replaced by a layout switch/reset). Re-dock it instead.
            if (anchorable.IsHidden)
            {
                dockManager.Layout.Hidden.Remove(anchorable);
                anchorable.AddToLayout(dockManager, AnchorableShowStrategy.Bottom | AnchorableShowStrategy.Most);
            }
        }
        else if (!shouldBeVisible && !anchorable.IsHidden)
        {
            anchorable.Hide();
        }
    }

    // Keep Window_Layout.ShowFlybyTimeline in sync when the user hides the panel with its own
    // title-bar button (not via the Window menu), so the menu checkbox stays truthful and the
    // next toggle works on the first click.
    private void HookFlybyTimelineAnchorable(LayoutAnchorable anchorable)
    {
        anchorable.PropertyChanged -= OnFlybyTimelineAnchorableChanged;
        anchorable.PropertyChanged += OnFlybyTimelineAnchorableChanged;
    }

    private bool _suppressTimelineSync;

    private void OnFlybyTimelineAnchorableChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_suppressTimelineSync)
            return;

        if (e.PropertyName == nameof(LayoutAnchorable.IsHidden) && sender is LayoutAnchorable anchorable)
            _editor.Configuration.Window_Layout.ShowFlybyTimeline = !anchorable.IsHidden;
    }

    #endregion

    private void AboutMenu_Click(object sender, RoutedEventArgs e)
    {
        var about = new TombLib.WPF.Features.About.AboutWindow(
            new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/misc/misc_AboutScreen_800.png")))
        {
            Owner = this
        };
        about.ShowDialog();
    }

    private void EditMenu_SubmenuOpened(object sender, RoutedEventArgs e) => _menuState.UpdatePaste();

    private void WindowMenu_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem windowMenu)
            return;

        foreach (var item in windowMenu.Items.OfType<MenuItem>())
        {
            string commandName = EditorMenu.GetCommand(item);
            if (string.IsNullOrEmpty(commandName))
                continue;

            CommandObj cmd;
            try { cmd = CommandHandler.GetCommand(commandName); }
            catch { continue; }
            if (cmd is null || cmd.Type != CommandType.Windows)
                continue;

            item.IsCheckable = true;

            if (commandName == "ShowFlybyTimeline")
                item.IsChecked = _editor.Configuration.Window_Layout.ShowFlybyTimeline;
            else if (commandName == "ShowStatistics")
                item.IsChecked = _editor.Configuration.Window_Layout.ShowStats;
            else if (cmd.Execute is { } && FindAnchorable(NormalizedContentIdFor(commandName)) is { } anchorable)
                item.IsChecked = !anchorable.IsHidden;
        }
    }

    private static string NormalizedContentIdFor(string showCommandName)
    {
        // "ShowItemBrowser" -> "itemBrowser" - first letter lowercased after stripping "Show".
        var name = showCommandName.StartsWith("Show") ? showCommandName.Substring(4) : showCommandName;
        return ContentIdForPanelName(name);
    }

    private void OpenRecentMenu_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menu)
            return;

        menu.Items.Clear();

        var recent = Properties.Settings.Default.RecentProjects;
        var currentPath = _editor.Level?.Settings.LevelFilePath;
        bool addedAny = false;

        if (recent is not null)
        {
            foreach (var fileName in recent)
            {
                if (fileName == currentPath)
                    continue;
                if (!File.Exists(fileName))
                    continue;

                // MenuItem headers treat "_" as an access-key marker, which would render
                // "Maya1d2_21" as "Maya1d221"; double them so paths display literally.
                var item = new MenuItem { Header = fileName.Replace("_", "__") };
                item.Click += (_, _) => EditorActions.OpenLevel(this.GetWin32Window(), fileName);
                menu.Items.Add(item);
                addedAny = true;
            }
        }

        menu.Items.Add(new Separator());

        var clearItem = new MenuItem { Header = "Clear recent file list" };
        clearItem.Click += (_, _) =>
        {
            Properties.Settings.Default.RecentProjects?.Clear();
            Properties.Settings.Default.Save();
        };
        menu.Items.Add(clearItem);
    }

    #region Layout management

    private string SerializeDockState()
    {
        var serializer = new XmlLayoutSerializer(dockManager);
        using var sw = new StringWriter();
        serializer.Serialize(sw);
        return sw.ToString();
    }

    private void LoadDockState(string xml)
    {
        if (string.IsNullOrEmpty(xml))
            return;

        var serializer = new XmlLayoutSerializer(dockManager);
        serializer.LayoutSerializationCallback += OnLayoutSerializationCallback;

        // Tearing down the old tree raises visibility changes on discarded anchorables; they
        // must not be mistaken for the user hiding the timeline (see HookFlybyTimelineAnchorable).
        _suppressTimelineSync = true;
        try
        {
            using var sr = new StringReader(xml);
            serializer.Deserialize(sr);
        }
        finally
        {
            serializer.LayoutSerializationCallback -= OnLayoutSerializationCallback;
            _suppressTimelineSync = false;
        }

        // Layouts saved by builds that did not (de)serialize the timeline anchorable lack it
        // entirely; re-apply the configured visibility so it is recreated/re-shown right away.
        ApplyFlybyTimelineVisibility();
    }

    private void OnLayoutSerializationCallback(object sender, LayoutSerializationCallbackEventArgs e)
    {
        // Re-attach the existing view instances by ContentId. Any ContentId not listed
        // here is something the XML knows about but the current build no longer ships -
        // cancel so AvalonDock drops it instead of creating a phantom dock entry.
        e.Content = e.Model.ContentId switch
        {
            "rooms" => roomsView,
            "objectList" => objectListView,
            "sectorOptions" => sectorOptionsView,
            "roomOptions" => roomOptionsView,
            "triggerList" => triggerListView,
            "texturePanel" => texturePanelView,
            "itemBrowser" => itemBrowserView,
            "importedGeometryBrowser" => importedGeometryBrowserView,
            "contentBrowser" => contentBrowserView,
            "lighting" => lightingView,
            "palette" => paletteView,
            "itemProperties" => itemPropertiesView,
            "flybyTimeline" => flybyTimelineView,
            "mainView" => editorAreaHost,
            _ => null
        };

        if (e.Content is null)
            e.Cancel = true;
    }

    private void ApplyLayoutByIndex(int index)
    {
        var config = _editor.Configuration;

        if (index < 0)
        {
            // Save tweaks to whatever layout was active before switching.
            SaveCurrentStateToActiveLayout();
            config.Window_ActiveLayoutName = string.Empty;

            // Mirror FormMain.Layout_RestoreDefault: Window_Layout is the *current* layout state
            // (panel flags, toolbox positions); restoring the default resets it wholesale.
            config.Window_Layout = new NamedLayout();
            LoadDockState(_defaultDockState);
            ApplyToolboxPositionsFrom(config.Window_Layout);

            // Let flag-driven views (statistics bar, timeline checkbox, &) pick up the change.
            _editor.ConfigurationChange();
            return;
        }

        if (index >= config.Window_CustomLayouts.Count)
            return;

        var target = config.Window_CustomLayouts[index];

        // Re-selecting the already-active layout is an explicit "restore what I saved": saving
        // first would overwrite the stored state with the current (possibly messed-up) one and
        // turn the restore into a no-op. Only persist tweaks when actually switching layouts.
        if (config.Window_ActiveLayoutName != target.Name)
            SaveCurrentStateToActiveLayout();

        config.Window_ActiveLayoutName = target.Name;

        // Mirror FormMain.Layout_SwitchTo: the chosen layout becomes the current state, so its
        // ShowStats/ShowFlybyTimeline flags actually take effect on switch.
        config.Window_Layout = target.Clone();
        LoadDockState(string.IsNullOrEmpty(target.AvalonDockState) ? _defaultDockState : target.AvalonDockState);
        ApplyToolboxPositionsFrom(target);
        _editor.ConfigurationChange();
    }

    private void SaveCurrentStateToActiveLayout()
    {
        var active = ActiveLayout();

        // The dock layout is only persisted for named custom layouts; the default falls back to the
        // XAML baseline. The floating toolbox positions, however, belong to every layout (incl. default).
        if (!string.IsNullOrEmpty(_editor.Configuration.Window_ActiveLayoutName))
        {
            active.AvalonDockState = SerializeDockState();

            // Panel flags live on Window_Layout (the current-state layout); snapshot them into
            // the named layout so they round-trip on the next switch (FormMain parity).
            active.ShowStats = _editor.Configuration.Window_Layout.ShowStats;
            active.ShowFlybyTimeline = _editor.Configuration.Window_Layout.ShowFlybyTimeline;
        }

        SaveToolboxPositionsTo(active);
    }

    /// <summary>
    /// The <see cref="NamedLayout"/> backing the currently active layout - the default
    /// <see cref="Configuration.Window_Layout"/> when no custom layout is selected, otherwise the
    /// matching entry in <see cref="Configuration.Window_CustomLayouts"/>.
    /// </summary>
    private NamedLayout ActiveLayout()
    {
        var config = _editor.Configuration;
        if (string.IsNullOrEmpty(config.Window_ActiveLayoutName))
            return config.Window_Layout;

        return config.Window_CustomLayouts.FirstOrDefault(l => l.Name == config.Window_ActiveLayoutName)
            ?? config.Window_Layout;
    }

    private void SaveToolboxPositionsTo(NamedLayout layout)
    {
        layout.ToolboxPosition = new System.Drawing.Point((int)_toolPalette.Position.X, (int)_toolPalette.Position.Y);

        // The object-brush toolbox only exists in ObjectPlacement mode; when it is hidden its last
        // position was already written into the active layout by OnEditorEventForFloatingToolboxes.
        if (_objectBrushToolbox.IsShown)
            layout.ObjectBrushToolboxPosition = new System.Drawing.Point((int)_objectBrushToolbox.Position.X, (int)_objectBrushToolbox.Position.Y);
    }

    private void ApplyToolboxPositionsFrom(NamedLayout layout)
    {
        _toolPalette.Position = new Point(layout.ToolboxPosition.X, layout.ToolboxPosition.Y);

        if (_objectBrushToolbox.IsShown)
            _objectBrushToolbox.Position = new Point(layout.ObjectBrushToolboxPosition.X, layout.ObjectBrushToolboxPosition.Y);
    }

    private void LayoutsMenu_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
            return;

        menuItem.Items.Clear();

        var config = _editor.Configuration;

        var defaultItem = new MenuItem
        {
            Header = _localizationService["~TombEditor.MainWindow.Layouts_Default"],
            IsChecked = string.IsNullOrEmpty(config.Window_ActiveLayoutName)
        };
        defaultItem.Click += (_, _) => _editor.SwitchLayout(-1);
        menuItem.Items.Add(defaultItem);

        if (config.Window_CustomLayouts.Count > 0)
        {
            menuItem.Items.Add(new Separator());

            for (int i = 0; i < config.Window_CustomLayouts.Count; i++)
            {
                var layout = config.Window_CustomLayouts[i];
                var item = new MenuItem
                {
                    // "__" so user-given layout names show "_" literally instead of access keys.
                    Header = layout.Name?.Replace("_", "__"),
                    IsChecked = layout.Name == config.Window_ActiveLayoutName
                };

                if (i < Configuration.MaxWindowLayouts)
                {
                    var hotkeyName = "SwitchLayout" + (i + 1);
                    if (config.UI_Hotkeys.Any(h => h.Key == hotkeyName))
                    {
                        item.InputGestureText = string.Join(", ",
                            config.UI_Hotkeys[hotkeyName]
                                .Select(h => h.ToString())
                                .Where(s => !string.IsNullOrWhiteSpace(s)));
                    }
                }

                int layoutIndex = i;
                item.Click += (_, _) => _editor.SwitchLayout(layoutIndex);
                menuItem.Items.Add(item);
            }
        }

        menuItem.Items.Add(new Separator());

        var saveAsItem = new MenuItem { Header = _localizationService["~TombEditor.MainWindow.Layouts_SaveAs"] };
        saveAsItem.Click += (_, _) => Layout_SaveAs();
        menuItem.Items.Add(saveAsItem);

        var deleteItem = new MenuItem
        {
            Header = _localizationService["~TombEditor.MainWindow.Layouts_Delete"],
            IsEnabled = !string.IsNullOrEmpty(config.Window_ActiveLayoutName)
        };
        deleteItem.Click += (_, _) => Layout_Delete();
        menuItem.Items.Add(deleteItem);
    }

    private void Layout_SaveAs()
    {
        var config = _editor.Configuration;

        var vm = new InputBoxWindowViewModel(
            title: _localizationService["~TombEditor.MainWindow.Layouts_SaveAsTitle"],
            label: _localizationService["~TombEditor.MainWindow.Layouts_SaveAsLabel"],
            invalidNames: config.Window_CustomLayouts.Select(l => l.Name).ToArray());

        // Own this dialog directly because MainWindow is not its own DataContext.
        var win = new InputBoxWindow
        {
            DataContext = vm,
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        void onVmChanged(object _, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(vm.DialogResult) && vm.DialogResult is not null)
                win.Close();
        }
        vm.PropertyChanged += onVmChanged;
        try { win.ShowDialog(); }
        finally { vm.PropertyChanged -= onVmChanged; }

        if (vm.DialogResult is not true)
            return;

        var name = vm.Value.Trim();
        if (string.IsNullOrEmpty(name))
            return;

        // Belt-and-braces - InputBoxWindowViewModel already rejects exact matches,
        // but it's case-sensitive, so dedupe again here to stop "Foo" vs "foo".
        if (config.Window_CustomLayouts.Any(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            _messageService.ShowError(_localizationService["~TombEditor.MainWindow.Layouts_NameAlreadyExists"]);
            return;
        }

        // Capture the full current state, not just the dock arrangement - panel flags and toolbox
        // positions are part of a layout too (FormMain.SaveCurrentStateToLayout parity).
        var newLayout = new NamedLayout
        {
            Name = name,
            AvalonDockState = SerializeDockState(),
            ShowStats = config.Window_Layout.ShowStats,
            ShowFlybyTimeline = config.Window_Layout.ShowFlybyTimeline,
        };
        SaveToolboxPositionsTo(newLayout);

        config.Window_CustomLayouts.Add(newLayout);
        config.Window_ActiveLayoutName = name;
    }

    private void Layout_Delete()
    {
        var config = _editor.Configuration;
        var layout = config.Window_CustomLayouts.FirstOrDefault(l => l.Name == config.Window_ActiveLayoutName);
        if (layout is null)
            return;

        config.Window_CustomLayouts.Remove(layout);
        _editor.SwitchLayout(-1);
    }

    #endregion
}
