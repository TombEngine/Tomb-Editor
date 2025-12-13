using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiEnum : INamedObject, ISummarizedObject, IDescribedObject
{
	public string Name { get; set; } = string.Empty;
	public string Summary { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// The list of values for this enum.
	/// </summary>
	public List<ApiEnumValue> Values { get; } = new();
}
