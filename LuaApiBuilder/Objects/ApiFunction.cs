using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiFunction : IApiFunction
{
	/// <summary>
	/// The name of the module this function belongs to.
	/// </summary>
	public string Module { get; set; } = string.Empty;

	public string Name { get; set; } = string.Empty;
	public string Summary { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// The name or identifier of the caller associated with the current operation.
	/// It indicates that this function is part of an instance of a class.
	/// </summary>
	public string? Caller { get; set; }

	public List<ApiParameter> Parameters { get; } = new();
	public List<ApiReturn> Returns { get; } = new();
}
