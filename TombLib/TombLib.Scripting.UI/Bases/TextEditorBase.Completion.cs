using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	internal CompletionWindow? ActiveCompletionWindow => _completionWindowCoordinator.ActiveWindow;

	/// <summary>
	/// Gets whether the completion window is currently open.
	/// </summary>
	protected bool IsCompletionWindowOpen => _completionWindowCoordinator.IsWindowOpen;

	/// <summary>
	/// Runs a completion request that was scheduled through
	/// <see cref="Nickelony.IDEKit.AvalonEdit.LanguageFeatures.Completion.TextCompletionController.ScheduleRequest"/>.
	/// The base implementation does nothing; editors that use debounced completion override it.
	/// </summary>
	/// <returns>A task that completes after the scheduled request concluded.</returns>
	protected virtual Task RequestScheduledCompletionAsync() => Task.CompletedTask;

	/// <summary>
	/// Handles Ctrl+Space to trigger completion when completion is enabled.
	/// </summary>
	/// <param name="e">The text composition event to inspect.</param>
	/// <param name="onTriggered">The action invoked when completion should be triggered.</param>
	/// <returns><see langword="true"/> if the input was handled as a completion trigger; otherwise <see langword="false"/>.</returns>
	protected bool TryHandleCtrlSpaceCompletion(TextCompositionEventArgs e, Action onTriggered)
	{
		EnsureNotDisposed();

		if (!IntelliSenseEnabled || !CompletionEnabled || !EditorCompletionTriggerHelper.IsCtrlSpaceInput(e.Text, Keyboard.Modifiers.HasFlag(ModifierKeys.Control)))
			return false;

		if (!IsCompletionWindowOpen)
			onTriggered();

		e.Handled = true;
		return true;
	}

	/// <summary>
	/// Rebases the open completion items onto the current document version and session generation.
	/// </summary>
	/// <param name="requestDocumentVersion">The logical document version of the request.</param>
	/// <param name="requestGeneration">The session generation of the request.</param>
	protected void RebaseOpenCompletionItems(int requestDocumentVersion, int requestGeneration)
	{
		CompletionWindow? completionWindow = CompletionController.ActiveWindow;

		if (completionWindow?.CompletionList?.CompletionData is null)
			return;

		for (int i = 0; i < completionWindow.CompletionList.CompletionData.Count; i++)
		{
			if (completionWindow.CompletionList.CompletionData[i] is CompletionData completionData)
				completionData.RebaseForCurrentDocument(requestDocumentVersion, requestGeneration);
		}
	}
}
