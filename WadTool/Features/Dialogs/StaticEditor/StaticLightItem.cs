#nullable enable

using TombLib.Wad;

namespace WadTool.Features.Dialogs.StaticEditor;

/// <summary>One entry in the lights list (legacy <c>lstLights</c> tree node: "Light #N" + the <see cref="WadLight"/> tag).</summary>
public sealed record StaticLightItem(WadLight Light, string Name);
