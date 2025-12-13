using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiField : INamedObject, ITypedObject, ISummarizedObject, IDescribedObject
{
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Summary { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
}
