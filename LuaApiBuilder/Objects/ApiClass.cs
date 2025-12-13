using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiClass : INamedObject, ITypedObject, ISummarizedObject, IDescribedObject
{
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Summary { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// The parent class this class inherits from.
	/// </summary>
	public string Inherits { get; set; } = string.Empty;

	/// <summary>
	/// Indicates whether this class has a constructor.
	/// </summary>
	public bool HasConstructor { get; set; } = true;

	/// <summary>
	/// The list of methods for this class.
	/// </summary>
	public List<ApiMethod> Methods { get; } = new();

	/// <summary>
	/// The list of fields for this class.
	/// </summary>
	public List<ApiField> Fields { get; } = new();
}
