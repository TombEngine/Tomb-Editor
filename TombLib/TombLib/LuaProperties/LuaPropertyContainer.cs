using System;
using System.Collections.Generic;
using System.Linq;

// Property container that holds Lua property values in boxed (Lua text) format.
// Used for both Level 1 (global per-object-type, stored in wad2 files) and Level 2
// (per-instance, stored in prj2 files). Values are stored in their boxed Lua representatio
// so they can be written directly to Lua script blobs during level compilation.

namespace TombLib.LuaProperties
{
    /// <summary>
    /// A dictionary-like container that stores Lua property values by their internal name.
    /// All values are stored as boxed Lua strings (e.g. "true", "42", "TEN.Vec3(1,2,3)").
    /// This container is serialized into wad2/prj2 files and written to level Lua scripts.
    /// </summary>
    [Serializable]
    public class LuaPropertyContainer
    {
        /// <summary>
        /// Internal storage: property internal name → boxed Lua value string.
        /// </summary>
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the number of properties in this container.
        /// </summary>
        public int Count => _properties.Count;

        /// <summary>
        /// Returns true if this container has any properties set.
        /// </summary>
        public bool HasProperties => _properties.Count > 0;

        /// <summary>
        /// Gets or sets a boxed Lua value by its internal property name.
        /// Setting to null removes the property.
        /// </summary>
        public string this[string internalName]
        {
            get => _properties.TryGetValue(internalName, out var value) ? value : null;
            set
            {
                if (value == null)
                    _properties.Remove(internalName);
                else
                    _properties[internalName] = value;
            }
        }

        /// <summary>
        /// Gets the boxed Lua value for the given property name.
        /// Returns null if not set.
        /// </summary>
        public string GetValue(string internalName)
        {
            return _properties.TryGetValue(internalName, out var value) ? value : null;
        }

        /// <summary>
        /// Sets a boxed Lua value for the given property name.
        /// If the value is null or empty, the property is removed.
        /// </summary>
        public void SetValue(string internalName, string boxedValue)
        {
            if (string.IsNullOrEmpty(boxedValue))
                _properties.Remove(internalName);
            else
                _properties[internalName] = boxedValue;
        }

        /// <summary>
        /// Removes a property by its internal name.
        /// </summary>
        public bool Remove(string internalName)
        {
            return _properties.Remove(internalName);
        }

        /// <summary>
        /// Clears all properties.
        /// </summary>
        public void Clear()
        {
            _properties.Clear();
        }

        /// <summary>
        /// Returns true if the property is set in this container.
        /// </summary>
        public bool Contains(string internalName)
        {
            return _properties.ContainsKey(internalName);
        }

        /// <summary>
        /// Enumerates all property name/value pairs.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            return _properties.AsEnumerable();
        }

        /// <summary>
        /// Creates a deep copy of this container.
        /// </summary>
        public LuaPropertyContainer Clone()
        {
            var clone = new LuaPropertyContainer();

            foreach (var kvp in _properties)
                clone._properties[kvp.Key] = kvp.Value;

            return clone;
        }
    }
}
