using System.Windows.Forms;

namespace TombLib.LevelData
{
    public interface IHasLuaName
    {
        string LuaName { get; set; }
        void AllocateNewLuaName();
        bool SupportsLuaName();
        bool CanSetLuaName(string newScriptId);
    }
}
