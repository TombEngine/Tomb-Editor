using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiParameter : INamedObject, ITypedObject, IDescribedObject
{
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Indicates whether this parameter is optional.
	/// </summary>
	public bool Optional { get; set; }

	/// <summary>
	/// The default value for this parameter, if it is optional.
	/// </summary>
	public string DefaultValue { get; set; } = string.Empty;
}
