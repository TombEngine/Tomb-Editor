using LuaApiBuilder.Interfaces;

namespace LuaApiBuilder.Objects;

public sealed class ApiMethod : IApiFunction
{
	public string Name { get; set; } = string.Empty;
	public string Summary { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// The list of parameters for this method.
	/// </summary>
	public List<ApiParameter> Parameters { get; } = new();

	/// <summary>
	/// The list of return values for this method.
	/// </summary>
	public List<ApiReturn> Returns { get; } = new();
}
