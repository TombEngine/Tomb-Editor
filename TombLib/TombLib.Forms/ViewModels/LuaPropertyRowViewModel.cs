// ViewModel for a single property row in the Lua property grid.
// Manages the binding between a LuaPropertyDefinition and its current boxed value.

using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLib.LuaProperties;

namespace TombLib.Forms.ViewModels
{
    /// <summary>
    /// ViewModel for a single row in the Lua property grid.
    /// Wraps a <see cref="LuaPropertyDefinition"/> with an editable current value.
    /// </summary>
    public partial class LuaPropertyRowViewModel : ObservableObject
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>
        /// The property definition (read-only metadata).
        /// </summary>
        public LuaPropertyDefinition Definition { get; }

        /// <summary>
        /// Display name shown in the name column.
        /// </summary>
        public string DisplayName => Definition.DisplayName;

        /// <summary>
        /// Tooltip text for the property row.
        /// </summary>
        public string ToolTip => string.IsNullOrEmpty(Definition.Description)
            ? Definition.InternalName
            : Definition.Description;

        /// <summary>
        /// Category grouping label. Empty string means uncategorized.
        /// </summary>
        public string Category => Definition.Category ?? string.Empty;

        /// <summary>
        /// For Color properties, whether the alpha channel editor is visible.
        /// </summary>
        public bool HasAlpha => Definition.HasAlpha;

        /// <summary>
        /// The Lua property type.
        /// </summary>
        public LuaPropertyType PropertyType => Definition.Type;

        /// <summary>
        /// Current boxed Lua value string.
        /// Setting this updates all type-specific properties via notification.
        /// </summary>
        public string BoxedValue
        {
            get => _boxedValue;
            set
            {
                if (_boxedValue == value) return;
                _boxedValue = value ?? LuaValueParser.GetDefaultBoxedValue(Definition.Type);
                OnPropertyChanged();
                RefreshTypedProperties();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        private string _boxedValue;

        /// <summary>
        /// Fired when the boxed value changes (used by host to save changes).
        /// </summary>
        public event EventHandler ValueChanged;

        #region Type-specific property accessors (for data binding)

        // --- Bool ---
        public bool BoolValue
        {
            get => LuaValueParser.UnboxBool(_boxedValue);
            set => BoxedValue = LuaValueParser.BoxBool(value);
        }

        // --- Int ---
        public int IntValue
        {
            get => LuaValueParser.UnboxInt(_boxedValue);
            set => BoxedValue = LuaValueParser.BoxInt(value);
        }

        // --- Float ---
        public float FloatValue
        {
            get => LuaValueParser.UnboxFloat(_boxedValue);
            set => BoxedValue = LuaValueParser.BoxFloat(value);
        }

        // --- String ---
        public string StringValue
        {
            get => LuaValueParser.UnboxString(_boxedValue);
            set => BoxedValue = LuaValueParser.BoxString(value);
        }

        // --- Vec2 ---
        public float Vec2X
        {
            get => LuaValueParser.UnboxVec2(_boxedValue)[0];
            set { var v = LuaValueParser.UnboxVec2(_boxedValue); BoxedValue = LuaValueParser.BoxVec2(value, v[1]); }
        }
        public float Vec2Y
        {
            get => LuaValueParser.UnboxVec2(_boxedValue)[1];
            set { var v = LuaValueParser.UnboxVec2(_boxedValue); BoxedValue = LuaValueParser.BoxVec2(v[0], value); }
        }

        // --- Vec3 ---
        public float Vec3X
        {
            get => LuaValueParser.UnboxVec3(_boxedValue)[0];
            set { var v = LuaValueParser.UnboxVec3(_boxedValue); BoxedValue = LuaValueParser.BoxVec3(value, v[1], v[2]); }
        }
        public float Vec3Y
        {
            get => LuaValueParser.UnboxVec3(_boxedValue)[1];
            set { var v = LuaValueParser.UnboxVec3(_boxedValue); BoxedValue = LuaValueParser.BoxVec3(v[0], value, v[2]); }
        }
        public float Vec3Z
        {
            get => LuaValueParser.UnboxVec3(_boxedValue)[2];
            set { var v = LuaValueParser.UnboxVec3(_boxedValue); BoxedValue = LuaValueParser.BoxVec3(v[0], v[1], value); }
        }

        // --- Rotation ---
        public float RotationX
        {
            get => LuaValueParser.UnboxRotation(_boxedValue)[0];
            set { var v = LuaValueParser.UnboxRotation(_boxedValue); BoxedValue = LuaValueParser.BoxRotation(value, v[1], v[2]); }
        }
        public float RotationY
        {
            get => LuaValueParser.UnboxRotation(_boxedValue)[1];
            set { var v = LuaValueParser.UnboxRotation(_boxedValue); BoxedValue = LuaValueParser.BoxRotation(v[0], value, v[2]); }
        }
        public float RotationZ
        {
            get => LuaValueParser.UnboxRotation(_boxedValue)[2];
            set { var v = LuaValueParser.UnboxRotation(_boxedValue); BoxedValue = LuaValueParser.BoxRotation(v[0], v[1], value); }
        }

        // --- Color ---
        public byte ColorR
        {
            get => LuaValueParser.UnboxColor(_boxedValue)[0];
            set { var c = LuaValueParser.UnboxColor(_boxedValue); BoxedValue = LuaValueParser.BoxColor(value, c[1], c[2], c[3]); }
        }
        public byte ColorG
        {
            get => LuaValueParser.UnboxColor(_boxedValue)[1];
            set { var c = LuaValueParser.UnboxColor(_boxedValue); BoxedValue = LuaValueParser.BoxColor(c[0], value, c[2], c[3]); }
        }
        public byte ColorB
        {
            get => LuaValueParser.UnboxColor(_boxedValue)[2];
            set { var c = LuaValueParser.UnboxColor(_boxedValue); BoxedValue = LuaValueParser.BoxColor(c[0], c[1], value, c[3]); }
        }
        public byte ColorA
        {
            get => LuaValueParser.UnboxColor(_boxedValue)[3];
            set { var c = LuaValueParser.UnboxColor(_boxedValue); BoxedValue = LuaValueParser.BoxColor(c[0], c[1], c[2], value); }
        }

        /// <summary>
        /// WPF-bindable color brush for the color picker preview.
        /// </summary>
        public System.Windows.Media.Color WpfColor
        {
            get
            {
                var c = LuaValueParser.UnboxColor(_boxedValue);
                return System.Windows.Media.Color.FromArgb(c[3], c[0], c[1], c[2]);
            }
        }

        // --- Time ---
        public int TimeHours
        {
            get => LuaValueParser.UnboxTime(_boxedValue)[0];
            set { var t = LuaValueParser.UnboxTime(_boxedValue); BoxedValue = LuaValueParser.BoxTime(value, t[1], t[2], t[3]); }
        }
        public int TimeMinutes
        {
            get => LuaValueParser.UnboxTime(_boxedValue)[1];
            set { var t = LuaValueParser.UnboxTime(_boxedValue); BoxedValue = LuaValueParser.BoxTime(t[0], value, t[2], t[3]); }
        }
        public int TimeSeconds
        {
            get => LuaValueParser.UnboxTime(_boxedValue)[2];
            set { var t = LuaValueParser.UnboxTime(_boxedValue); BoxedValue = LuaValueParser.BoxTime(t[0], t[1], value, t[3]); }
        }
        public int TimeCentiseconds
        {
            get => LuaValueParser.UnboxTime(_boxedValue)[3];
            set { var t = LuaValueParser.UnboxTime(_boxedValue); BoxedValue = LuaValueParser.BoxTime(t[0], t[1], t[2], value); }
        }

        #endregion

        public LuaPropertyRowViewModel(LuaPropertyDefinition definition, string initialBoxedValue = null)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _boxedValue = initialBoxedValue ?? definition.DefaultValue ?? LuaValueParser.GetDefaultBoxedValue(definition.Type);
        }

        /// <summary>
        /// Resets the value to the definition's default.
        /// </summary>
        public void ResetToDefault()
        {
            BoxedValue = Definition.DefaultValue ?? LuaValueParser.GetDefaultBoxedValue(Definition.Type);
        }

        /// <summary>
        /// Returns true if the current value differs from the default.
        /// </summary>
        public bool IsModified =>
            !string.Equals(_boxedValue, Definition.DefaultValue, StringComparison.Ordinal);

        /// <summary>
        /// Notifies all typed property bindings that the underlying value changed.
        /// </summary>
        private void RefreshTypedProperties()
        {
            switch (Definition.Type)
            {
                case LuaPropertyType.Bool:
                    OnPropertyChanged(nameof(BoolValue));
                    break;
                case LuaPropertyType.Int:
                    OnPropertyChanged(nameof(IntValue));
                    break;
                case LuaPropertyType.Float:
                    OnPropertyChanged(nameof(FloatValue));
                    break;
                case LuaPropertyType.String:
                    OnPropertyChanged(nameof(StringValue));
                    break;
                case LuaPropertyType.Vec2:
                    OnPropertyChanged(nameof(Vec2X));
                    OnPropertyChanged(nameof(Vec2Y));
                    break;
                case LuaPropertyType.Vec3:
                    OnPropertyChanged(nameof(Vec3X));
                    OnPropertyChanged(nameof(Vec3Y));
                    OnPropertyChanged(nameof(Vec3Z));
                    break;
                case LuaPropertyType.Rotation:
                    OnPropertyChanged(nameof(RotationX));
                    OnPropertyChanged(nameof(RotationY));
                    OnPropertyChanged(nameof(RotationZ));
                    break;
                case LuaPropertyType.Color:
                    OnPropertyChanged(nameof(ColorR));
                    OnPropertyChanged(nameof(ColorG));
                    OnPropertyChanged(nameof(ColorB));
                    OnPropertyChanged(nameof(ColorA));
                    OnPropertyChanged(nameof(WpfColor));
                    break;
                case LuaPropertyType.Time:
                    OnPropertyChanged(nameof(TimeHours));
                    OnPropertyChanged(nameof(TimeMinutes));
                    OnPropertyChanged(nameof(TimeSeconds));
                    OnPropertyChanged(nameof(TimeCentiseconds));
                    break;
            }
        }
    }
}
