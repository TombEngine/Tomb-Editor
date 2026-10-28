#nullable enable

using TombLib.Utils;

namespace TombLib.Forms.ViewModels;

/// <summary>Pairs a material source texture with its resolved file path.</summary>
public sealed record MaterialTextureEntry(Texture Texture, string Path);
