using System;

namespace TombLib.Scripting.TRX.Resources;

/// <summary>
/// Describes a keyword that was removed from the TRX syntax in a specific engine version.
/// </summary>
/// <param name="Keyword">The removed keyword.</param>
/// <param name="RemovedVersion">The engine version from which the keyword is unavailable.</param>
/// <param name="Message">The optional message explaining the removal.</param>
public record struct RemovedKeyword(string Keyword, Version RemovedVersion, string Message = "");
