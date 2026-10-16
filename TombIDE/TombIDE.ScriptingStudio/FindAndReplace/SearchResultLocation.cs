#nullable enable

using Nickelony.IDEKit.Core.Navigation;

namespace TombIDE.ScriptingStudio.FindAndReplace;

/// <summary>
/// Describes the outcome of resolving a search result to a navigation location.
/// </summary>
/// <param name="Status">How the location was resolved.</param>
/// <param name="Location">
/// The resolved location, or <see langword="null"/> when <see cref="Status"/> is
/// <see cref="SearchResultLocationStatus.LineNotFound"/>.
/// </param>
public readonly record struct SearchResultLocation(SearchResultLocationStatus Status, NavigationLocation? Location);
