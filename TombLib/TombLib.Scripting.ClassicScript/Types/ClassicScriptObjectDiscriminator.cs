using Nickelony.IDEKit.IntelliSense.Navigation;

namespace TombLib.Scripting.ClassicScript.Types;

/// <summary>
/// Identifies the ClassicScript object kind of a definition lookup.
/// </summary>
/// <param name="ObjectType">The ClassicScript object kind.</param>
public sealed record ClassicScriptObjectDiscriminator(ObjectType ObjectType) : TextDefinitionDiscriminator;
