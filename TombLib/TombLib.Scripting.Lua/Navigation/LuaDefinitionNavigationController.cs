using Nickelony.IDEKit.Core.Infrastructure;
using Nickelony.IDEKit.IntelliSense.Navigation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	/// <summary>
	/// Owns Lua definition-navigation request state and the editor-side flow for F12 and Ctrl+Click navigation.
	/// </summary>
	private sealed class LuaDefinitionNavigationController
	{
		private readonly LuaEditor _editor;
		private readonly LatestRequestCoordinator _latestRequestCoordinator = new();

		internal LuaDefinitionNavigationController(LuaEditor editor)
		{
			_editor = editor;
		}

		internal void CancelPendingRequest()
			=> _latestRequestCoordinator.CancelPendingRequest();

		internal void InvalidateRequests()
			=> _latestRequestCoordinator.Invalidate();

		internal async Task<bool> TryNavigateAsync(int offset, CancellationToken cancellationToken)
		{
			if (!_editor.IsIntelliSenseAvailable())
				return false;

			var intelliSenseProvider = _editor.IntelliSenseProvider;

			if (intelliSenseProvider is null)
				return false;

			if (!LuaEditorInteractionRules.TryGetDefinitionStartOffset(_editor.Document, offset, out int definitionOffset))
				return false;

			int requestDocumentVersion = _editor.DocumentVersion;
			int requestGeneration = _editor.SessionGeneration;

			(int line, int column) = _editor.GetPositionFromOffset(definitionOffset);
			string filePath = _editor.FilePath;
			string text = _editor.Text;

			try
			{
				return await _latestRequestCoordinator.RunAsync(
					(intelliSenseProvider, filePath, text, requestDocumentVersion, requestGeneration, line, column),
					(state, token) => state.intelliSenseProvider.GetDefinitionAsync(state.filePath, state.text, state.line, state.column, token),
					(state, location) => location is not null
						&& _editor.DocumentVersion == state.requestDocumentVersion
						&& _editor.SessionGeneration == state.requestGeneration
						&& _editor.IsLoaded
						&& _editor.IsIntelliSenseAvailable(),
					location =>
					{
						if (location is not null)
							_editor.DefinitionNavigationRequested?.Invoke(location);
					},
					cancellationToken).ConfigureAwait(true);
			}
			catch (Exception exception)
			{
				LogEditorFailure("Go to definition", exception);
				return false;
			}
		}
	}
}
