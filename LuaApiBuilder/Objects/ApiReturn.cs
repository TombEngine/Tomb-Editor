using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiReturn : ITypedObject, IDescribedObject
{
	public string Type { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
}
