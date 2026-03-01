// Defines supported Lua property value types for TombEngine property system.
// These correspond to TEN Lua API types: bool, int, float, string, Vec2, Vec3, Rotation, Color, Time.

namespace TombLib.LuaProperties
{
    /// <summary>
    /// Enumerates all supported Lua property types for the TombEngine property grid system.
    /// </summary>
    public enum LuaPropertyType
    {
        Bool,
        Int,
        Float,
        String,
        Vec2,
        Vec3,
        Rotation,
        Color,
        Time
    }
}
