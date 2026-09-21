#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.CommandSurface;

internal sealed class TypedDocumentCommandSurfaceProvider : IStudioDocumentCommandSurfaceProvider
{
	private sealed record StudioDocumentCommandSurfaceDefinition(
		IReadOnlyList<StudioToolStripItem> ContextMenuItems,
		IReadOnlyList<StudioToolStripItem> MenuStripItems,
		IReadOnlyList<StudioToolStripItem> ToolStripItems);

	private readonly StudioDocumentCommandSurfaceDefinition _definition;

	private TypedDocumentCommandSurfaceProvider(StudioDocumentCommandSurfaceDefinition definition)
	{
		_definition = definition;
	}

	public static TypedDocumentCommandSurfaceProvider CreateClassicScript()
		=> Create(CreateClassicScriptContextMenuItems(), CreateClassicScriptMenuStripItems(), CreateScriptToolStripItems());

	public static TypedDocumentCommandSurfaceProvider CreateGameFlowScript()
		=> Create(CreateGameFlowContextMenuItems(), CreateGameFlowMenuStripItems(), CreateScriptToolStripItems());

	public static TypedDocumentCommandSurfaceProvider CreateLua()
		=> Create(CreateLuaContextMenuItems(), CreateLuaMenuStripItems(), CreateScriptToolStripItems());

	public static TypedDocumentCommandSurfaceProvider CreatePlainText()
		=> Create(CreatePlainTextContextMenuItems(), CreatePlainTextMenuStripItems(), CreatePlainTextToolStripItems());

	public static TypedDocumentCommandSurfaceProvider CreateStrings()
		=> Create(CreateStringsContextMenuItems(), CreateStringsMenuStripItems(), CreateStringsToolStripItems());

	public static TypedDocumentCommandSurfaceProvider CreateTrx()
		=> Create(CreateTrxContextMenuItems(), CreateTrxMenuStripItems(), CreateScriptToolStripItems());

	private static TypedDocumentCommandSurfaceProvider Create(
		IReadOnlyList<StudioToolStripItem> contextMenuItems,
		IReadOnlyList<StudioToolStripItem> menuStripItems,
		IReadOnlyList<StudioToolStripItem> toolStripItems)
		=> new(new(contextMenuItems, menuStripItems, toolStripItems));

	public IReadOnlyList<StudioToolStripItem> GetContextMenuItems(IEditorControl editor)
		=> StudioCommandSurfaceItemFactory.CloneItems(_definition.ContextMenuItems);

	public IReadOnlyList<StudioToolStripItem> GetMenuStripItems(IEditorControl editor)
		=> StudioCommandSurfaceItemFactory.CloneItems(_definition.MenuStripItems);

	public IReadOnlyList<StudioToolStripItem> GetToolStripItems(IEditorControl editor)
		=> StudioCommandSurfaceItemFactory.CloneItems(_definition.ToolStripItems);

	private static IReadOnlyList<StudioToolStripItem> CreateClassicScriptMenuStripItems() =>
	[
		CreateDocumentRoot([
			CreateConvertRoot(),
			CreateSeparator(),
			CreateCommandItem("Reindent", UICommand.Reindent),
			CreateTrimWhitespaceItem(),
			CreateSeparator(),
			CreateToggleCommentItem(),
			CreateCommandItem("CommentOut", UICommand.CommentOut, icon: "Comment_16"),
			CreateCommandItem("Uncomment", UICommand.Uncomment, icon: "Uncomment_16"),
			CreateSeparator(),
			.. CreateBookmarkMenuItems()])
	];

	private static IReadOnlyList<StudioToolStripItem> CreateGameFlowMenuStripItems() =>
	[
		CreateDocumentRoot([
			CreateConvertRoot(),
			CreateSeparator(),
			CreateTrimWhitespaceItem(),
			CreateSeparator(),
			CreateToggleCommentItem(),
			CreateCommandItem("CommentOut", UICommand.CommentOut, icon: "Comment_16"),
			CreateCommandItem("Uncomment", UICommand.Uncomment, icon: "Uncomment_16"),
			CreateSeparator(),
			.. CreateBookmarkMenuItems()])
	];

	private static IReadOnlyList<StudioToolStripItem> CreateLuaMenuStripItems() =>
	[
		CreateDocumentRoot([
			CreateConvertRoot(),
			CreateSeparator(),
			CreateCommandItem("Reindent", UICommand.Reindent),
			CreateTrimWhitespaceItem(),
			CreateSeparator(),
			CreateCommandItem("GoToDefinition", UICommand.GoToDefinition),
			CreateCommandItem("FindReferences", UICommand.FindReferences),
			CreateCommandItem("RenameSymbol", UICommand.RenameSymbol),
			CreateCommandItem("NavigateBack", UICommand.NavigateBack, icon: "Left_16"),
			CreateCommandItem("NavigateForward", UICommand.NavigateForward, icon: "Right_16"),
			CreateSeparator(),
			CreateToggleCommentItem(),
			CreateCommandItem("CommentOut", UICommand.CommentOut, icon: "Comment_16"),
			CreateCommandItem("Uncomment", UICommand.Uncomment, icon: "Uncomment_16"),
			CreateSeparator(),
			.. CreateBookmarkMenuItems()])
	];

	private static IReadOnlyList<StudioToolStripItem> CreatePlainTextMenuStripItems() =>
	[
		CreateDocumentRoot([
			CreateConvertRoot(),
			CreateSeparator(),
			CreateTrimWhitespaceItem(),
			CreateSeparator(),
			.. CreateBookmarkMenuItems()])
	];

	private static IReadOnlyList<StudioToolStripItem> CreateStringsMenuStripItems() =>
	[
		CreateDocumentRoot(
			CreateCommandItem("PrevSection", UICommand.PrevSection, icon: "Left_16"),
			CreateCommandItem("NextSection", UICommand.NextSection, icon: "Right_16"),
			CreateSeparator(),
			CreateCommandItem("ClearString", UICommand.ClearString, icon: "Eraser_16"),
			CreateSeparator(),
			CreateCommandItem("RemoveLastString", UICommand.RemoveLastString, icon: "Trash_16"))
	];

	private static IReadOnlyList<StudioToolStripItem> CreateTrxMenuStripItems()
		=> CreateGameFlowMenuStripItems();

	private static IReadOnlyList<StudioToolStripItem> CreateScriptToolStripItems() =>
	[
		CreateSeparator(),
		CreateCommandItem("CommentOut", UICommand.CommentOut, icon: "Comment_16"),
		CreateCommandItem("Uncomment", UICommand.Uncomment, icon: "Uncomment_16"),
		CreateSeparator(),
		CreateCommandItem("ToggleBookmark", UICommand.ToggleBookmark, icon: "Bookmark_16"),
		CreateCommandItem("PrevBookmark", UICommand.PrevBookmark, icon: "PrevBookmark_16"),
		CreateCommandItem("NextBookmark", UICommand.NextBookmark, icon: "NextBookmark_16"),
		CreateCommandItem("ClearBookmarks", UICommand.ClearBookmarks, icon: "ClearBookmarks_16")
	];

	private static IReadOnlyList<StudioToolStripItem> CreatePlainTextToolStripItems() =>
	[
		CreateSeparator(),
		CreateCommandItem("ToggleBookmark", UICommand.ToggleBookmark, icon: "Bookmark_16"),
		CreateCommandItem("PrevBookmark", UICommand.PrevBookmark, icon: "PrevBookmark_16"),
		CreateCommandItem("NextBookmark", UICommand.NextBookmark, icon: "NextBookmark_16"),
		CreateCommandItem("ClearBookmarks", UICommand.ClearBookmarks, icon: "ClearBookmarks_16")
	];

	private static IReadOnlyList<StudioToolStripItem> CreateStringsToolStripItems() =>
	[
		CreateSeparator(),
		CreateCommandItem("PrevSection", UICommand.PrevSection, icon: "Left_16"),
		CreateCommandItem("NextSection", UICommand.NextSection, icon: "Right_16"),
		CreateSeparator(),
		CreateCommandItem("ClearString", UICommand.ClearString, icon: "Eraser_16"),
		CreateSeparator(),
		CreateCommandItem("RemoveLastString", UICommand.RemoveLastString, icon: "Trash_16")
	];

	private static IReadOnlyList<StudioToolStripItem> CreateClassicScriptContextMenuItems() =>
	[
		.. CreateCutCopyPasteItems(),
		CreateSeparator(),
		CreateToggleCommentItem(),
		CreateCommandItem("CommentOut", UICommand.CommentOut, icon: "Comment_16"),
		CreateCommandItem("Uncomment", UICommand.Uncomment, icon: "Uncomment_16"),
		CreateSeparator(),
		CreateCommandItem("ToggleBookmark", UICommand.ToggleBookmark, icon: "Bookmark_16"),
		CreateSeparator(),
		CreateCommandItem("TypeFirstAvailableId", UICommand.TypeFirstAvailableId, icon: "Asterisk_16"),
		CreateCommandItem("NewFileAtCaret", UICommand.NewFileAtCaret, icon: "New_16")
	];

	private static IReadOnlyList<StudioToolStripItem> CreateGameFlowContextMenuItems() =>
	[
		.. CreateCutCopyPasteItems(),
		CreateSeparator(),
		CreateToggleCommentItem(),
		CreateCommandItem("CommentOut", UICommand.CommentOut, icon: "Comment_16"),
		CreateCommandItem("Uncomment", UICommand.Uncomment, icon: "Uncomment_16"),
		CreateSeparator(),
		CreateCommandItem("ToggleBookmark", UICommand.ToggleBookmark, icon: "Bookmark_16")
	];

	private static IReadOnlyList<StudioToolStripItem> CreateLuaContextMenuItems() =>
	[
		.. CreateCutCopyPasteItems(),
		CreateSeparator(),
		CreateCommandItem("GoToDefinition", UICommand.GoToDefinition),
		CreateCommandItem("FindReferences", UICommand.FindReferences),
		CreateCommandItem("RenameSymbol", UICommand.RenameSymbol),
		CreateCommandItem("NavigateBack", UICommand.NavigateBack, icon: "Left_16"),
		CreateCommandItem("NavigateForward", UICommand.NavigateForward, icon: "Right_16"),
		CreateSeparator(),
		CreateToggleCommentItem(),
		CreateCommandItem("CommentOut", UICommand.CommentOut, icon: "Comment_16"),
		CreateCommandItem("Uncomment", UICommand.Uncomment, icon: "Uncomment_16"),
		CreateSeparator(),
		CreateCommandItem("ToggleBookmark", UICommand.ToggleBookmark, icon: "Bookmark_16")
	];

	private static IReadOnlyList<StudioToolStripItem> CreatePlainTextContextMenuItems() =>
	[
		.. CreateCutCopyPasteItems(),
		CreateSeparator(),
		CreateCommandItem("ToggleBookmark", UICommand.ToggleBookmark, icon: "Bookmark_16")
	];

	private static IReadOnlyList<StudioToolStripItem> CreateStringsContextMenuItems() =>
	[
		.. CreateCutCopyPasteItems()
	];

	private static IReadOnlyList<StudioToolStripItem> CreateTrxContextMenuItems()
		=> CreateGameFlowContextMenuItems();

	private static StudioToolStripItem CreateCommandItem(
		string langKey,
		UICommand command,
		string icon = "",
		string shortcutDisplayText = "",
		bool checkOnClick = false,
		int? position = null)
		=> StudioCommandSurfaceItemFactory.CreateCommandItem(langKey, command, icon, shortcutDisplayText, checkOnClick, position);

	private static StudioToolStripItem CreateConvertRoot()
	{
		return StudioCommandSurfaceItemFactory.CreateGroupItem(
			"Convert",
			CreateCommandItem("TabsToSpaces", UICommand.TabsToSpaces),
			CreateCommandItem("SpacesToTabs", UICommand.SpacesToTabs));
	}

	private static StudioToolStripItem[] CreateCutCopyPasteItems() =>
	[
		CreateCommandItem("Cut", UICommand.Cut, icon: "Cut_16"),
		CreateCommandItem("Copy", UICommand.Copy, icon: "Copy_16"),
		CreateCommandItem("Paste", UICommand.Paste, icon: "Clipboard_16")
	];

	private static StudioToolStripItem CreateDocumentRoot(params StudioToolStripItem[] items)
		=> StudioCommandSurfaceItemFactory.CreateRootItem("Document", 2, items);

	private static StudioSeparator CreateSeparator()
		=> StudioCommandSurfaceItemFactory.CreateSeparator();

	private static StudioToolStripItem CreateToggleCommentItem()
		=> CreateCommandItem("ToggleComment", UICommand.ToggleComment, icon: "Comment_16", shortcutDisplayText: "Ctrl+/");

	private static StudioToolStripItem CreateTrimWhitespaceItem()
		=> CreateCommandItem("TrimWhitespace", UICommand.TrimWhiteSpace);

	private static StudioToolStripItem[] CreateBookmarkMenuItems() =>
	[
		CreateCommandItem("ToggleBookmark", UICommand.ToggleBookmark, icon: "Bookmark_16"),
		CreateCommandItem("PrevBookmark", UICommand.PrevBookmark, icon: "PrevBookmark_16", shortcutDisplayText: "Ctrl+Comma"),
		CreateCommandItem("NextBookmark", UICommand.NextBookmark, icon: "NextBookmark_16", shortcutDisplayText: "Ctrl+Period"),
		CreateCommandItem("ClearBookmarks", UICommand.ClearBookmarks, icon: "ClearBookmarks_16")
	];
}
