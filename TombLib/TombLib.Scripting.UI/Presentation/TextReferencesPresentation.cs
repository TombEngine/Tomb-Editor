namespace TombLib.Scripting.UI.Presentation;

/// <summary>
/// Carries the localized text used by the references presentation.
/// </summary>
/// <param name="NoActiveDocumentText">The text shown when no document is active.</param>
/// <param name="UnsupportedText">The text shown when the active editor does not support references.</param>
/// <param name="LoadingText">The text shown while references are loading.</param>
/// <param name="EmptyText">The text shown when no references were found.</param>
public sealed record class TextReferencesPresentation(
	string NoActiveDocumentText,
	string UnsupportedText,
	string LoadingText,
	string EmptyText);
