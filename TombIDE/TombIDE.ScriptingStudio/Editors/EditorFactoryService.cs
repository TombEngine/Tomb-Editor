#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Editors;

internal sealed class EditorFactoryService
{
	private readonly ConditionalWeakTable<IEditorControl, ScriptingDocumentRegistration> _editorRegistrations = [];
	// Registration order is the explicit priority policy: earlier registrations win ties.
	private readonly List<ScriptingDocumentRegistration> _registrations = [];

	private ScriptingDocumentRegistration _plainTextRegistration = CreatePlainTextRegistration(null, DocumentMode.PlainText, ScriptingDocumentContributions.None);

	public string BuildTabTitle(string filePath, EditorType editorType)
	{
		string tabTypeText = editorType == GetDefaultEditorType(filePath) ? string.Empty : $" [{editorType}]";
		return Path.GetFileName(filePath) + tabTypeText;
	}

	public IEditorControl CreateEditor(string filePath, EditorType editorType, Version engineVersion)
	{
		ScriptingDocumentRegistration registration = ResolveRegistration(filePath, editorType) ?? _plainTextRegistration;
		IEditorControl editor = registration.Factory(engineVersion);

		_editorRegistrations.Add(editor, registration);

		return editor;
	}

	internal ScriptingDocumentRegistration? GetDocumentRegistration(IEditorControl? editor)
		=> editor is not null && _editorRegistrations.TryGetValue(editor, out ScriptingDocumentRegistration? registration)
			? registration
			: null;

	internal ScriptingDocumentContributions GetDocumentContributions(IEditorControl editor)
	{
		ArgumentNullException.ThrowIfNull(editor);

		return GetDocumentRegistration(editor)?.Contributions
			?? ScriptingDocumentContributions.None;
	}

	public EditorType GetDefaultEditorType(string filePath)
		=> ResolveDefaultRegistration(filePath)?.EditorType ?? EditorType.Text;

	public EditorType GetSourceViewEditorType(string filePath)
	{
		return _registrations.FirstOrDefault(registration => registration.SupportsFile(filePath) && registration.EditorType == EditorType.Text)?.EditorType
			?? GetDefaultEditorType(filePath);
	}

	/// <summary>
	/// Adds a registration using insertion order as its priority.
	/// Default registrations are preferred over non-default registrations before this
	/// ordering is used as the fallback for a supported file.
	/// </summary>
	public void Register(ScriptingDocumentRegistration registration)
	{
		ArgumentNullException.ThrowIfNull(registration);

		_registrations.Add(registration);
	}

	public void SetPlainTextEditorFactory(Func<Version, IEditorControl>? factory, DocumentMode documentMode, ScriptingDocumentContributions contributions)
	{
		_plainTextRegistration = CreatePlainTextRegistration(factory, documentMode, contributions);
	}

	private ScriptingDocumentRegistration? ResolveDefaultRegistration(string filePath)
	{
		return _registrations.FirstOrDefault(registration => registration.SupportsFile(filePath) && registration.IsDefaultForFile(filePath))
			?? _registrations.FirstOrDefault(registration => registration.SupportsFile(filePath));
	}

	private static IEditorControl CreateDefaultPlainTextEditor(Version _)
		=> new PlainTextEditor();

	private static ScriptingDocumentRegistration CreatePlainTextRegistration(
		Func<Version, IEditorControl>? factory,
		DocumentMode documentMode,
		ScriptingDocumentContributions contributions)
		=> new(
			EditorType.Text,
			documentMode,
			static _ => false,
			static _ => false,
			factory ?? CreateDefaultPlainTextEditor,
			contributions,
			isFallback: true);

	private ScriptingDocumentRegistration? ResolveRegistration(string filePath, EditorType editorType)
	{
		ScriptingDocumentRegistration? defaultRegistration = ResolveDefaultRegistration(filePath);

		if (editorType == EditorType.Default)
			return defaultRegistration;

		return _registrations.FirstOrDefault(registration => registration.SupportsFile(filePath) && registration.EditorType == editorType)
			?? defaultRegistration;
	}
}
