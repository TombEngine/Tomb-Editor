using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiParameter : INamedObject, ITypedObject, IDescribedObject, IOptionalObject
{
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public bool Optional { get; set; }
	public string DefaultValue { get; set; } = string.Empty;
}
