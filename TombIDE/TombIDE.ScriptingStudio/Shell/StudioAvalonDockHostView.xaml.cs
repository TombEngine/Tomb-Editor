#nullable enable

using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using DarkUI.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.Shared.Docking;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Shell;

public partial class StudioAvalonDockHostView : UserControl
{
	private readonly Dictionary<string, LayoutAnchorable> _anchorablesByKey = new(StringComparer.Ordinal);
	private readonly Dictionary<string, UIElement> _contentElementsByKey = new(StringComparer.Ordinal);
	private readonly Dictionary<LayoutDocument, IEditorControl> _editorsByDocument = [];
	private readonly Dictionary<IEditorControl, LayoutDocument> _documentsByEditor = [];
	private readonly Dictionary<IEditorControl, UIElement> _documentElementsByEditor = [];
	private readonly IEditorDocumentController _documentController;
	private readonly IReadOnlyDictionary<string, StudioDockPane> _panesByKey;
	private bool _syncingDocumentSelection;
	private bool _syncingTabSelection;

	public StudioAvalonDockHostView(IEditorDocumentController documentController, IReadOnlyCollection<StudioDockPane> panes)
	{
		InitializeComponent();

		_documentController = documentController ?? throw new ArgumentNullException(nameof(documentController));
		_panesByKey = (panes ?? throw new ArgumentNullException(nameof(panes)))
			.ToDictionary(static pane => pane.SerializationKey, StringComparer.Ordinal);

		DockingManager.Theme = new DarkAvalonDockTheme();
		DockingManager.ActiveContentChanged += DockingManager_ActiveContentChanged;

		_documentController.FileOpened += DocumentController_FileOpened;
		_documentController.EditorClosed += DocumentController_EditorClosed;
		_documentController.EditorTitleChanged += DocumentController_EditorTitleChanged;
		_documentController.CurrentEditorChanged += DocumentController_CurrentEditorChanged;
	}

	public bool IsPaneVisible(StudioDockPane? pane)
	{
		if (pane is null)
			return false;

		return _anchorablesByKey.TryGetValue(pane.SerializationKey, out LayoutAnchorable? anchorable)
			&& anchorable.IsVisible;
	}

	public void DetachDocumentController()
	{
		DockingManager.ActiveContentChanged -= DockingManager_ActiveContentChanged;
		_documentController.FileOpened -= DocumentController_FileOpened;
		_documentController.EditorClosed -= DocumentController_EditorClosed;
		_documentController.EditorTitleChanged -= DocumentController_EditorTitleChanged;
		_documentController.CurrentEditorChanged -= DocumentController_CurrentEditorChanged;
	}

	public bool EnsurePane(StudioDockPane? pane)
	{
		if (pane is null)
			return false;

		if (_anchorablesByKey.ContainsKey(pane.SerializationKey))
			return true;

		LayoutAnchorable? anchorable = CreateAnchorable(pane.SerializationKey, isSelected: false);

		if (anchorable is null)
			return false;

		anchorable.AddToLayout(DockingManager, pane.DefaultLocation switch
		{
			StudioDockPaneLocation.Left => AnchorableShowStrategy.Left,
			StudioDockPaneLocation.Right => AnchorableShowStrategy.Right,
			_ => AnchorableShowStrategy.Bottom
		});

		return true;
	}

	public void RestoreDefaultLayout(DockPanelState legacyLayout, Action? onLayoutRestored = null)
	{
		DockingManager.Layout = CreateLayoutRoot(legacyLayout);
		SynchronizeDocumentsFromController();
		onLayoutRestored?.Invoke();
	}

	public void RestoreLayout(string? layoutXml, DockPanelState legacyLayout, Action? onLayoutRestored = null)
	{
		if (string.IsNullOrWhiteSpace(layoutXml))
		{
			RestoreDefaultLayout(legacyLayout, onLayoutRestored);
			return;
		}

		try
		{
			DockingManager.Layout = CreateLayoutRoot(legacyLayout);

			var serializer = new XmlLayoutSerializer(DockingManager);
			serializer.LayoutSerializationCallback += Serializer_LayoutSerializationCallback;
			using var stringReader = new StringReader(layoutXml);
			serializer.Deserialize(stringReader);

			SynchronizeDocumentsFromController();
			onLayoutRestored?.Invoke();
		}
		catch
		{
			RestoreDefaultLayout(legacyLayout, onLayoutRestored);
		}
	}

	public string SaveLayout()
	{
		if (DockingManager.Layout is null)
			return string.Empty;

		var serializer = new XmlLayoutSerializer(DockingManager);
		using var stringWriter = new StringWriter();
		serializer.Serialize(stringWriter);
		return stringWriter.ToString();
	}

	public bool ShowPane(StudioDockPane? pane)
	{
		if (!EnsurePane(pane) || !_anchorablesByKey.TryGetValue(pane!.SerializationKey, out LayoutAnchorable? anchorable))
			return false;

		anchorable.Show();
		anchorable.IsActive = true;
		return anchorable.IsVisible;
	}

	public bool TogglePane(StudioDockPane? pane)
	{
		if (!EnsurePane(pane) || !_anchorablesByKey.TryGetValue(pane!.SerializationKey, out LayoutAnchorable? anchorable))
			return false;

		if (anchorable.IsVisible)
		{
			anchorable.Hide();
		}
		else
		{
			anchorable.Show();
			anchorable.IsActive = true;
		}

		return anchorable.IsVisible;
	}

	private LayoutAnchorable? CreateAnchorable(string contentId, bool isSelected)
	{
		if (!_panesByKey.TryGetValue(contentId, out StudioDockPane? pane))
			return null;

		var anchorable = new LayoutAnchorable
		{
			CanAutoHide = true,
			CanClose = false,
			CanFloat = true,
			CanHide = true,
			Content = GetContentElement(pane),
			ContentId = pane.SerializationKey,
			IsSelected = isSelected,
			Title = pane.Title
		};

		// Apply minimum sizes from the pane definition to prevent
		// zero-width or zero-height panes after layout reset or minimize.
		if (pane.DefaultSize.Width > 0)
			anchorable.FloatingWidth = Math.Max(anchorable.FloatingWidth, pane.DefaultSize.Width);

		if (pane.DefaultSize.Height > 0)
			anchorable.FloatingHeight = Math.Max(anchorable.FloatingHeight, pane.DefaultSize.Height);

		_anchorablesByKey[pane.SerializationKey] = anchorable;
		return anchorable;
	}

	private LayoutAnchorablePaneGroup? CreateAnchorablePaneGroup(DockPanelState legacyLayout, DarkDockArea area)
	{
		List<DockRegionState> regions = legacyLayout.Regions?
			.Where(region => region.Area == area)
			.ToList() ?? [];

		if (regions.Count == 0)
			return null;

		var paneGroup = new LayoutAnchorablePaneGroup
		{
			Orientation = area == DarkDockArea.Bottom ? Orientation.Horizontal : Orientation.Vertical
		};

		foreach (DockRegionState region in regions)
		{
			foreach (DockGroupState group in region.Groups ?? [])
			{
				var pane = new LayoutAnchorablePane();

				foreach (string contentId in group.Contents ?? [])
				{
					LayoutAnchorable? anchorable = CreateAnchorable(contentId, string.Equals(group.VisibleContent, contentId, StringComparison.Ordinal));

					if (anchorable is not null)
						pane.Children.Add(anchorable);
				}

				if (pane.Children.Count == 0)
					continue;

				if (region.Size.Width > 0)
					pane.DockWidth = new GridLength(region.Size.Width, GridUnitType.Pixel);

				if (region.Size.Height > 0)
					pane.DockHeight = new GridLength(region.Size.Height, GridUnitType.Pixel);

				// Prevent panes from collapsing to zero size after minimize or layout reset.
				pane.DockMinWidth = 100;
				pane.DockMinHeight = 60;

				paneGroup.Children.Add(pane);
			}
		}

		return paneGroup.Children.Count == 0 ? null : paneGroup;
	}

	private LayoutRoot CreateLayoutRoot(DockPanelState legacyLayout)
	{
		_anchorablesByKey.Clear();
		_editorsByDocument.Clear();
		_documentsByEditor.Clear();

		var rootPanel = new LayoutPanel
		{
			Orientation = Orientation.Horizontal
		};

		LayoutAnchorablePaneGroup? leftPaneGroup = CreateAnchorablePaneGroup(legacyLayout, DarkDockArea.Left);

		if (leftPaneGroup is not null)
			rootPanel.Children.Add(leftPaneGroup);

		var centerPanel = new LayoutPanel
		{
			Orientation = Orientation.Vertical
		};

		var documentPane = new LayoutDocumentPane();
		centerPanel.Children.Add(documentPane);

		LayoutAnchorablePaneGroup? bottomPaneGroup = CreateAnchorablePaneGroup(legacyLayout, DarkDockArea.Bottom);

		if (bottomPaneGroup is not null)
			centerPanel.Children.Add(bottomPaneGroup);

		rootPanel.Children.Add(centerPanel);

		LayoutAnchorablePaneGroup? rightPaneGroup = CreateAnchorablePaneGroup(legacyLayout, DarkDockArea.Right);

		if (rightPaneGroup is not null)
			rootPanel.Children.Add(rightPaneGroup);

		return new LayoutRoot
		{
			RootPanel = rootPanel
		};
	}

	private void SynchronizeDocumentsFromController()
	{
		List<IEditorControl> currentEditors = [.. _documentController.GetOpenEditors()];

		foreach (IEditorControl removedEditor in _documentsByEditor.Keys.Except(currentEditors).ToList())
			RemoveDocument(removedEditor);

		foreach (IEditorControl editor in currentEditors)
			EnsureDocument(editor);

		SynchronizeSelectedDocumentFromController();
	}

	private LayoutDocument EnsureDocument(IEditorControl editor)
	{
		if (_documentsByEditor.TryGetValue(editor, out LayoutDocument? existingDocument))
		{
			UpdateDocumentTitle(editor);
			return existingDocument;
		}

		UIElement contentElement = GetDocumentContentElement(editor);

		LayoutDocumentPane documentPane = GetDocumentPane()
			?? throw new InvalidOperationException("Unable to locate the AvalonDock document pane.");

		var document = new LayoutDocument
		{
			CanClose = true,
			CanFloat = true,
			Content = contentElement,
			ContentId = CreateDocumentContentId(editor),
			Title = _documentController.GetDocumentTitle(editor)
		};

		document.Closing += Document_Closing;
		document.IsSelectedChanged += Document_IsSelectedChanged;

		_documentsByEditor[editor] = document;
		_editorsByDocument[document] = editor;
		documentPane.Children.Add(document);

		return document;
	}

	private UIElement GetDocumentContentElement(IEditorControl editor)
	{
		if (_documentElementsByEditor.TryGetValue(editor, out UIElement? existingElement))
			return existingElement;

		UIElement element = editor switch
		{
			UIElement uiElement => uiElement,
			_ => throw new NotSupportedException($"Unsupported editor type for document hosting: {editor.GetType().FullName}")
		};

		_documentElementsByEditor[editor] = element;
		return element;
	}

	private LayoutDocumentPane? GetDocumentPane()
		=> DockingManager.Layout?.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();

	private void RemoveDocument(IEditorControl editor)
	{
		if (!_documentsByEditor.TryGetValue(editor, out LayoutDocument? document))
			return;

		document.Closing -= Document_Closing;
		document.IsSelectedChanged -= Document_IsSelectedChanged;

		_documentsByEditor.Remove(editor);
		_editorsByDocument.Remove(document);
		_documentElementsByEditor.Remove(editor);

		if (document.Parent is LayoutDocumentPane pane)
			pane.Children.Remove(document);
	}

	private void SynchronizeSelectedDocumentFromController()
	{
		if (_syncingTabSelection)
			return;

		_syncingDocumentSelection = true;

		try
		{
			if (_documentController.CurrentEditor is null || !_documentsByEditor.TryGetValue(_documentController.CurrentEditor, out LayoutDocument? document))
				return;

			document.IsSelected = true;
			document.IsActive = true;
		}
		finally
		{
			_syncingDocumentSelection = false;
		}
	}

	private void UpdateDocumentTitle(IEditorControl editor)
	{
		if (_documentsByEditor.TryGetValue(editor, out LayoutDocument? document))
			document.Title = _documentController.GetDocumentTitle(editor);
	}

	private static string CreateDocumentContentId(IEditorControl editor)
	{
		return string.IsNullOrWhiteSpace(editor.FilePath)
			? $"Document:{editor.GetHashCode():X8}"
			: $"Document:{editor.EditorType}:{editor.FilePath}";
	}

	private UIElement GetContentElement(StudioDockPane pane)
	{
		if (_contentElementsByKey.TryGetValue(pane.SerializationKey, out UIElement? existingElement))
			return existingElement;

		UIElement element = pane.Content;
		_contentElementsByKey[pane.SerializationKey] = element;
		return element;
	}

	private void Serializer_LayoutSerializationCallback(object? sender, LayoutSerializationCallbackEventArgs e)
	{
		if (e.Model is not LayoutContent layoutContent)
			return;

		if (e.Model is LayoutDocument)
		{
			e.Cancel = true;
			return;
		}

		if (!_panesByKey.TryGetValue(layoutContent.ContentId, out StudioDockPane? pane))
		{
			e.Cancel = true;
			return;
		}

		e.Content = GetContentElement(pane);
		layoutContent.Title = pane.Title;

		if (layoutContent is LayoutAnchorable anchorable)
			_anchorablesByKey[pane.SerializationKey] = anchorable;
	}

	private void DocumentController_FileOpened(object? sender, EventArgs e)
	{
		if (sender is not IEditorControl editor)
			return;

		EnsureDocument(editor);
		SynchronizeSelectedDocumentFromController();
	}

	private void DocumentController_EditorClosed(object? sender, EditorControlEventArgs e)
		=> RemoveDocument(e.Editor);

	private void DocumentController_EditorTitleChanged(object? sender, EditorControlEventArgs e)
		=> UpdateDocumentTitle(e.Editor);

	private void DocumentController_CurrentEditorChanged(object? sender, EventArgs e)
		=> SynchronizeSelectedDocumentFromController();

	private void Document_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
	{
		if (sender is not LayoutDocument document || !_editorsByDocument.TryGetValue(document, out IEditorControl? editor))
			return;

		if (_syncingDocumentSelection)
			return;

		e.Cancel = !_documentController.TryCloseEditor(editor);
	}

	private void Document_IsSelectedChanged(object? sender, EventArgs e)
	{
		if (_syncingDocumentSelection || sender is not LayoutDocument { IsSelected: true } document || !_editorsByDocument.TryGetValue(document, out IEditorControl? editor))
			return;

		ActivateDocumentEditor(editor);
	}

	private void DockingManager_ActiveContentChanged(object? sender, EventArgs e)
	{
		if (_syncingDocumentSelection || !TryGetEditorFromActiveContent(DockingManager.ActiveContent, out IEditorControl? editor))
			return;

		if (editor is null)
			return;

		ActivateDocumentEditor(editor);
	}

	private bool TryGetEditorFromActiveContent(object? activeContent, out IEditorControl? editor)
	{
		if (activeContent is LayoutDocument document && _editorsByDocument.TryGetValue(document, out editor))
			return true;

		if (activeContent is UIElement element)
		{
			foreach ((IEditorControl candidateEditor, UIElement candidateElement) in _documentElementsByEditor)
			{
				if (ReferenceEquals(candidateElement, element))
				{
					editor = candidateEditor;
					return true;
				}
			}
		}

		editor = null;
		return false;
	}

	private void ActivateDocumentEditor(IEditorControl editor)
	{
		_syncingTabSelection = true;

		try
		{
			_documentController.ActivateEditor(editor);
		}
		finally
		{
			_syncingTabSelection = false;
		}
	}
}
