using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	/// <inheritdoc/>
	protected override async Task OnLanguageKeyDown(KeyEventArgs e)
	{
		if (e.Key == Key.Escape && (IsCompletionWindowOpen || _signatureHelpController.IsVisible || _specialToolTip.IsOpen))
		{
			CloseCompletionWindow();
			DismissTransientToolTips();
			e.Handled = true;
			return;
		}

		if (_signatureHelpController.IsVisible && (e.Key == Key.Back || e.Key == Key.Delete))
			ScheduleSignatureHelpRefresh();

		await base.OnLanguageKeyDown(e).ConfigureAwait(true);
	}

	private void TextEditor_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
		{
			CloseCompletionWindow();
			DismissTransientToolTips();
		}
	}

	/// <summary>
	/// Attempts to resolve and navigate to the symbol definition at the current caret position.
	/// </summary>
	/// <returns>A task that completes once the navigation attempt finishes.</returns>
	public Task NavigateToDefinitionAtCaretAsync()
		=> TryNavigateDefinitionAsync(CaretOffset, CancellationToken.None);

	private Task<bool> TryNavigateDefinitionAsync(int offset, CancellationToken cancellationToken)
		=> _definitionNavigationController.TryNavigateAsync(offset, cancellationToken);
}
