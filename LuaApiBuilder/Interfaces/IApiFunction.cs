using LuaApiBuilder.Objects;

namespace LuaApiBuilder.Interfaces;

/// <summary>
/// Represents a function-like API element, such as a function or method, with parameters and return values.
/// </summary>
public interface IApiFunction : INamedObject, ISummarizedObject, IDescribedObject
{
	/// <summary>
	/// The list of parameters for the function or method.
	/// </summary>
	List<ApiParameter> Parameters { get; }

	/// <summary>
	/// The list of return values for the function or method.
	/// </summary>
	List<ApiReturn> Returns { get; }
}
