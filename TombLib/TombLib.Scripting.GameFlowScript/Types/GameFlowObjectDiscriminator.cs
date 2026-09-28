using Nickelony.IDEKit.IntelliSense.Navigation;

namespace TombLib.Scripting.GameFlowScript.Types;

/// <summary>
/// Identifies the GameFlow object kind of a definition lookup.
/// </summary>
/// <param name="ObjectType">The GameFlow object kind.</param>
public sealed record GameFlowObjectDiscriminator(ObjectType ObjectType) : TextDefinitionDiscriminator;
