using System.Reflection;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio;

public class ConfigurationCollection
{
	public ClassicScriptEditorConfiguration ClassicScript = ConfigurationBase.Load<ClassicScriptEditorConfiguration>();
	public LuaEditorConfiguration Lua = ConfigurationBase.Load<LuaEditorConfiguration>();
	public GameFlowEditorConfiguration GameFlowScript = ConfigurationBase.Load<GameFlowEditorConfiguration>();
	public TRXEditorConfiguration TRX = ConfigurationBase.Load<TRXEditorConfiguration>();

	public void SaveAllConfigs()
	{
		foreach (FieldInfo field in GetType().GetRuntimeFields())
			(field.GetValue(this) as ConfigurationBase)?.Save();
	}
}
