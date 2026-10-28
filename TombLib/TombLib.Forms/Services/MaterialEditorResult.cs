#nullable enable

namespace TombLib.Forms.Services;

/// <summary>Describes the outcome of editing texture material sidecars.</summary>
public sealed record MaterialEditorResult(bool Accepted, bool MaterialChanged, string? MaterialFileName);
