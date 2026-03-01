// ViewModel for the Lua property grid control.
// Manages a collection of LuaPropertyRowViewModels populated from XML property definitions
// and current values from a LuaPropertyContainer.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib.LuaProperties;

namespace TombLib.Forms.ViewModels
{
    /// <summary>
    /// ViewModel for the <see cref="Views.LuaPropertyGridControl"/>.
    /// Displays property definitions for a specific object type, populated
    /// with current values from a <see cref="LuaPropertyContainer"/>.
    /// </summary>
    public partial class LuaPropertyGridViewModel : ObservableObject
    {
        /// <summary>
        /// All property rows, grouped by category.
        /// </summary>
        public ObservableCollection<LuaPropertyRowViewModel> Properties { get; }
            = new ObservableCollection<LuaPropertyRowViewModel>();

        /// <summary>
        /// Distinct categories present in the current property set.
        /// Empty string represents uncategorized properties.
        /// </summary>
        public IEnumerable<string> Categories =>
            Properties.Select(p => p.Category).Distinct().OrderBy(c => string.IsNullOrEmpty(c) ? "~" : c);

        /// <summary>
        /// Returns properties grouped by category.
        /// Uncategorized properties (empty category) come first.
        /// </summary>
        public IEnumerable<IGrouping<string, LuaPropertyRowViewModel>> GroupedProperties =>
            Properties.GroupBy(p => p.Category)
                      .OrderBy(g => string.IsNullOrEmpty(g.Key) ? "" : g.Key);

        /// <summary>
        /// True if no properties are currently loaded.
        /// </summary>
        public bool IsEmpty => Properties.Count == 0;

        /// <summary>
        /// Status message displayed when the property grid is empty.
        /// </summary>
        [ObservableProperty]
        private string _statusMessage = "No properties defined for this object type.";

        /// <summary>
        /// The display title for the property grid header.
        /// </summary>
        [ObservableProperty]
        private string _title = "Properties";

        /// <summary>
        /// Fired when any property value changes. The sender is the modified row ViewModel.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        /// <summary>
        /// The currently bound property container (set during Load).
        /// </summary>
        private LuaPropertyContainer _container;

        /// <summary>
        /// Loads property definitions for the given object type and populates current values
        /// from the provided container. If container is null, defaults are used.
        /// </summary>
        /// <param name="definitions">Property definitions for this object type (from XML catalog).</param>
        /// <param name="container">Existing property container with saved values, or null.</param>
        public void Load(List<LuaPropertyDefinition> definitions, LuaPropertyContainer container)
        {
            Properties.Clear();
            _container = container;

            if (definitions == null || definitions.Count == 0)
            {
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(GroupedProperties));
                OnPropertyChanged(nameof(Categories));
                return;
            }

            foreach (var definition in definitions)
            {
                if (!definition.IsValid)
                    continue;

                // Get the current value from container, falling back to definition default.
                string currentValue = container?.GetValue(definition.InternalName)
                                      ?? definition.DefaultValue
                                      ?? LuaValueParser.GetDefaultBoxedValue(definition.Type);

                var row = new LuaPropertyRowViewModel(definition, currentValue);
                row.ValueChanged += OnRowValueChanged;
                Properties.Add(row);
            }

            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(GroupedProperties));
            OnPropertyChanged(nameof(Categories));
        }

        /// <summary>
        /// Writes all current values back to the provided container.
        /// Only values that differ from defaults are written.
        /// </summary>
        public void SaveTo(LuaPropertyContainer container)
        {
            if (container == null)
                return;

            container.Clear();

            foreach (var row in Properties)
            {
                // Only store values that differ from the catalog default.
                if (row.IsModified)
                    container.SetValue(row.Definition.InternalName, row.BoxedValue);
            }
        }

        /// <summary>
        /// Returns a new container with all modified values.
        /// </summary>
        public LuaPropertyContainer ToContainer()
        {
            var container = new LuaPropertyContainer();
            SaveTo(container);
            return container;
        }

        /// <summary>
        /// Resets all properties to their default values.
        /// </summary>
        [RelayCommand]
        public void ResetAll()
        {
            foreach (var row in Properties)
                row.ResetToDefault();
        }

        /// <summary>
        /// Clears the property grid.
        /// </summary>
        public void Clear()
        {
            foreach (var row in Properties)
                row.ValueChanged -= OnRowValueChanged;

            Properties.Clear();
            _container = null;

            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(GroupedProperties));
            OnPropertyChanged(nameof(Categories));
        }

        private void OnRowValueChanged(object sender, EventArgs e)
        {
            // Write back to container immediately when a value changes.
            if (sender is LuaPropertyRowViewModel row && _container != null)
            {
                if (row.IsModified)
                    _container.SetValue(row.Definition.InternalName, row.BoxedValue);
                else
                    _container.Remove(row.Definition.InternalName);
            }

            PropertyValueChanged?.Invoke(sender, e);
        }
    }
}
