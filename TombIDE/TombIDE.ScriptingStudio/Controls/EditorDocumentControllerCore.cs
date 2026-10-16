#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.ClassicScript.Documents;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Controls
{
	internal readonly record struct EditorOpenResult(IEditorControl? Editor, bool IsNewDocument);

	internal sealed class EditorDocumentControllerCore
	{
		private readonly EditorFactoryService _editorFactory = new EditorFactoryService();
		private readonly List<IEditorControl> _openEditors = new();
		private readonly Version _currentEngineVersion;

		public EditorDocumentControllerCore(Version currentEngineVersion)
		{
			_currentEngineVersion = currentEngineVersion ?? throw new ArgumentNullException(nameof(currentEngineVersion));
		}

		public ScriptingDocumentRegistration? GetDocumentRegistration(IEditorControl? editor)
			=> _editorFactory.GetDocumentRegistration(editor);

		public string GetDocumentTitle(IEditorControl editor)
			=> _editorFactory.BuildTabTitle(editor.FilePath, editor.EditorType);

		public IEnumerable<IEditorControl> GetOpenEditors() => _openEditors;

		public List<string> GetFilePaths()
		{
			return _openEditors
				.Where(editor => editor != null && !string.IsNullOrWhiteSpace(editor.FilePath))
				.Select(editor => editor.FilePath)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		public IEnumerable<IEditorControl> FindEditorsOfFile(string filePath)
			=> _openEditors.Where(editor => editor.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));

		public IEditorControl? FindEditor(string filePath, EditorType editorType = EditorType.Default)
		{
			if (editorType == EditorType.Default)
				editorType = _editorFactory.GetDefaultEditorType(filePath);

			return _openEditors.FirstOrDefault(editor =>
				editor.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)
				&& editor.EditorType == editorType);
		}

		public IEditorControl? FindSourceEditor(string filePath)
			=> FindEditor(filePath, _editorFactory.GetSourceViewEditorType(filePath));

		public EditorType GetSourceViewEditorType(string filePath)
			=> _editorFactory.GetSourceViewEditorType(filePath);

		public bool ContainsEditor(IEditorControl editor)
			=> editor != null && _openEditors.Contains(editor);

		public EditorOpenResult OpenFile(string filePath, EditorType editorType = EditorType.Default, DocumentLoadOptions options = default)
		{
			EditorOpenResult openResult = CreateEditor(filePath, editorType);
			if (openResult.Editor is null || !openResult.IsNewDocument)
				return openResult;

			InitializeEditor(openResult.Editor, filePath, options);
			_openEditors.Add(openResult.Editor);

			return openResult;
		}

		public EditorOpenResult CreateEditor(string filePath, EditorType editorType = EditorType.Default)
		{
			IEditorControl? existingEditor = FindEditor(filePath, editorType);

			if (existingEditor is not null)
				return new EditorOpenResult(existingEditor, false);

			IEditorControl newEditor = _editorFactory.CreateEditor(filePath, editorType, _currentEngineVersion);

			if (newEditor is null)
				return default;

			return new EditorOpenResult(newEditor, true);
		}

		public void InitializeEditor(IEditorControl editor, string filePath, DocumentLoadOptions options = default)
		{
			ArgumentNullException.ThrowIfNull(editor);

			if (File.Exists(filePath))
				editor.Load(filePath, options);
			else
				editor.FilePath = filePath;
		}

		public void AddEditor(IEditorControl editor)
		{
			ArgumentNullException.ThrowIfNull(editor);

			if (!_openEditors.Contains(editor))
				_openEditors.Add(editor);
		}

		public void RegisterDocument(ScriptingDocumentRegistration registration)
		{
			ArgumentNullException.ThrowIfNull(registration);

			if (registration.IsFallback)
			{
				_editorFactory.SetPlainTextEditorFactory(registration.Factory, registration.DocumentMode, registration.Contributions);
				return;
			}

			_editorFactory.Register(registration);
		}

		public void RemoveEditor(IEditorControl editor)
		{
			if (editor is null)
				return;

			_openEditors.Remove(editor);
		}

	}
}
