// Generates Lua API call strings for property assignment during level compilation.
// Produces calls to TombEngine's property API:
//   Objects.SetMoveableProperty(ObjID.XXX, "propName", value)  -- Level 1 (global)
//   Objects.SetStaticProperty(slotID, "propName", value)       -- Level 1 (global)
//   GetMoveableByName("luaName"):SetProperty("propName", value)        -- Level 2 (instance)
//   GetStaticByName("luaName"):SetProperty("propName", value)          -- Level 2 (instance)
//
// Both layers are written; the engine handles override resolution internally.

using System.Collections.Generic;
using System.Text;
using TombLib.Utils;

namespace TombLib.LuaProperties
{
    /// <summary>
    /// Builds Lua script text for setting properties during level compilation.
    /// The generated Lua code is embedded in the compiled level file and executed
    /// by TombEngine during level load.
    /// </summary>
    public static class LuaPropertyScriptBuilder
    {
        // TEN Lua API function names
        private const string SetMoveablePropertyFunc = "TEN.Objects.SetMoveableProperty";
        private const string SetStaticPropertyFunc = "TEN.Objects.SetStaticProperty";
        private const string GetMoveableByNameFunc = "TEN.Objects.GetMoveableByName";
        private const string GetStaticByNameFunc = "TEN.Objects.GetStaticByName";

        /// <summary>
        /// Generates a Level 1 (global) Lua property assignment for a moveable object type.
        /// Output: TEN.Objects.SetMoveableProperty(TEN.Objects.ObjID.SLOT_NAME, "propName", value)
        /// </summary>
        /// <param name="slotName">The TEN slot name (e.g. "BADDY1").</param>
        /// <param name="propertyName">The internal property name.</param>
        /// <param name="boxedValue">The already-boxed Lua value string.</param>
        public static string BuildGlobalMoveableProperty(string slotName, string propertyName, string boxedValue)
        {
            return SetMoveablePropertyFunc + LuaSyntax.BracketOpen +
                   LuaSyntax.ObjectIDPrefix + LuaSyntax.Splitter + slotName +
                   LuaSyntax.Separator + LuaSyntax.Space +
                   TextExtensions.Quote(propertyName) +
                   LuaSyntax.Separator + LuaSyntax.Space +
                   boxedValue +
                   LuaSyntax.BracketClose;
        }

        /// <summary>
        /// Generates a Level 1 (global) Lua property assignment for a static object type.
        /// Output: TEN.Objects.SetStaticProperty(slotId, "propName", value)
        /// </summary>
        /// <param name="slotId">The static slot numeric ID.</param>
        /// <param name="propertyName">The internal property name.</param>
        /// <param name="boxedValue">The already-boxed Lua value string.</param>
        public static string BuildGlobalStaticProperty(uint slotId, string propertyName, string boxedValue)
        {
            return SetStaticPropertyFunc + LuaSyntax.BracketOpen +
                   slotId.ToString() +
                   LuaSyntax.Separator + LuaSyntax.Space +
                   TextExtensions.Quote(propertyName) +
                   LuaSyntax.Separator + LuaSyntax.Space +
                   boxedValue +
                   LuaSyntax.BracketClose;
        }

        /// <summary>
        /// Generates a Level 2 (instance) Lua property assignment for a moveable.
        /// Output: TEN.Objects.GetMoveableByName("luaName"):SetProperty("propName", value)
        /// </summary>
        /// <param name="luaName">The instance's LuaName identifier.</param>
        /// <param name="propertyName">The internal property name.</param>
        /// <param name="boxedValue">The already-boxed Lua value string.</param>
        public static string BuildInstanceMoveableProperty(string luaName, string propertyName, string boxedValue)
        {
            return GetMoveableByNameFunc + LuaSyntax.BracketOpen +
                   TextExtensions.Quote(luaName) +
                   LuaSyntax.BracketClose +
                   ":SetProperty" + LuaSyntax.BracketOpen +
                   TextExtensions.Quote(propertyName) +
                   LuaSyntax.Separator + LuaSyntax.Space +
                   boxedValue +
                   LuaSyntax.BracketClose;
        }

        /// <summary>
        /// Generates a Level 2 (instance) Lua property assignment for a static.
        /// Output: TEN.Objects.GetStaticByName("luaName"):SetProperty("propName", value)
        /// </summary>
        /// <param name="luaName">The instance's LuaName identifier.</param>
        /// <param name="propertyName">The internal property name.</param>
        /// <param name="boxedValue">The already-boxed Lua value string.</param>
        public static string BuildInstanceStaticProperty(string luaName, string propertyName, string boxedValue)
        {
            return GetStaticByNameFunc + LuaSyntax.BracketOpen +
                   TextExtensions.Quote(luaName) +
                   LuaSyntax.BracketClose +
                   ":SetProperty" + LuaSyntax.BracketOpen +
                   TextExtensions.Quote(propertyName) +
                   LuaSyntax.Separator + LuaSyntax.Space +
                   boxedValue +
                   LuaSyntax.BracketClose;
        }

        /// <summary>
        /// Generates a complete Lua script block for all Level 1 and Level 2 properties.
        /// The output is a series of API calls separated by newlines.
        /// </summary>
        /// <param name="globalMoveableProperties">Level 1 moveable properties: slot name → container.</param>
        /// <param name="globalStaticProperties">Level 1 static properties: slot ID → container.</param>
        /// <param name="instanceMoveableProperties">Level 2 moveable properties: lua name → container.</param>
        /// <param name="instanceStaticProperties">Level 2 static properties: lua name → container.</param>
        public static KeyValuePair<int, string> BuildFullPropertyScript(
            Dictionary<string, LuaPropertyContainer> globalMoveableProperties,
            Dictionary<uint, LuaPropertyContainer>   globalStaticProperties,
            Dictionary<string, LuaPropertyContainer> instanceMoveableProperties,
            Dictionary<string, LuaPropertyContainer> instanceStaticProperties)
        {
            int propertyCount = 0;
            var sb = new StringBuilder();

            sb.AppendLine("-- Auto-generated property assignment script");
            sb.AppendLine();
            sb.AppendLine("-- Level 1: Global object type properties");
            sb.AppendLine();

            // Level 1: Global Moveable properties
            if (globalMoveableProperties != null)
            {
                foreach (var kvp in globalMoveableProperties)
                {
                    foreach (var prop in kvp.Value.GetAll())
                    {
                        sb.AppendLine(BuildGlobalMoveableProperty(kvp.Key, prop.Key, prop.Value));
                        propertyCount++;
                    }
                }
            }

            // Level 1: Global Static properties
            if (globalStaticProperties != null)
            {
                foreach (var kvp in globalStaticProperties)
                {
                    foreach (var prop in kvp.Value.GetAll())
                    {
                        sb.AppendLine(BuildGlobalStaticProperty(kvp.Key, prop.Key, prop.Value));
                        propertyCount++;
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("-- Level 2: Instance properties");
            sb.AppendLine();

            // Level 2: Instance Moveable properties
            if (instanceMoveableProperties != null)
            {
                foreach (var kvp in instanceMoveableProperties)
                {
                    if (string.IsNullOrEmpty(kvp.Key))
                        continue; // Instance must have a LuaName

                    foreach (var prop in kvp.Value.GetAll())
                    {
                        sb.AppendLine(BuildInstanceMoveableProperty(kvp.Key, prop.Key, prop.Value));
                        propertyCount++;
                    }
                }
            }

            // Level 2: Instance Static properties
            if (instanceStaticProperties != null)
            {
                foreach (var kvp in instanceStaticProperties)
                {
                    if (string.IsNullOrEmpty(kvp.Key))
                        continue;

                    foreach (var prop in kvp.Value.GetAll())
                    {
                        sb.AppendLine(BuildInstanceStaticProperty(kvp.Key, prop.Key, prop.Value));
                        propertyCount++;
                    }
                }
            }

            return new KeyValuePair<int, string>(propertyCount, sb.ToString());
        }
    }
}
