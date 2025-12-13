using LuaApiBuilder.Objects;

namespace LuaApiBuilder;

public sealed class ApiResult
{
	public Dictionary<string, List<ApiFunction>> ModuleTypeFunctions { get; } = new();
	public Dictionary<string, List<ApiClass>> ModuleClasses { get; } = new();
	public Dictionary<string, List<ApiEnum>> ModuleEnums { get; } = new();
	public List<string> AllModules { get; } = new();
}
