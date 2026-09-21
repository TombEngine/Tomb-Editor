#nullable enable

using DarkUI.Controls;
using Nickelony.IDEKit.Core.Requests;
using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TombIDE.ScriptingStudio.DocumentOutline;

internal sealed class ContentNodesRefreshCoordinator
{
	private readonly LatestRequestCoordinator _latestRequestCoordinator = new();

	public void InvalidatePendingRequests()
		=> _latestRequestCoordinator.Invalidate();

	public void RequestRefresh(
		ITextDocumentSymbolProvider nodesProvider,
		string content,
		string filter,
		Func<ITextDocumentSymbolProvider, bool> canApplyRefresh,
		Action<IReadOnlyList<DarkTreeNode>> applyNodes)
	{
		ArgumentNullException.ThrowIfNull(nodesProvider);
		ArgumentNullException.ThrowIfNull(canApplyRefresh);
		ArgumentNullException.ThrowIfNull(applyNodes);

		_ = RefreshAsync(nodesProvider, content ?? string.Empty, filter ?? string.Empty, canApplyRefresh, applyNodes);
	}

	private async Task RefreshAsync(
		ITextDocumentSymbolProvider nodesProvider,
		string content,
		string filter,
		Func<ITextDocumentSymbolProvider, bool> canApplyRefresh,
		Action<IReadOnlyList<DarkTreeNode>> applyNodes)
	{
		try
		{
			await _latestRequestCoordinator.RunAsync(
				(nodesProvider, content, filter),
				static (state, token) => Task.Run(() =>
				{
					IReadOnlyList<TextDocumentSymbol> symbols = state.nodesProvider.GetSymbols(
						new TextDocumentSymbolRequest(state.content, state.filter));

					return DocumentSymbolTreeNodeConverter.ToTreeNodes(symbols);
				}, token),
				(state, _) => canApplyRefresh(state.nodesProvider),
				applyNodes);
		}
		catch
		{
			// Node generation is best-effort; a failed or superseded refresh is simply skipped.
		}
	}
}
