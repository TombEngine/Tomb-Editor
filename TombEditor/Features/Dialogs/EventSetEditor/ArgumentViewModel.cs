#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaColor = System.Windows.Media.Color;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    public enum ArgumentEditorKind
    {
        Boolean,
        Numerical,
        Vector2,
        Vector3,
        Color,
        Time,
        String,
        List
    }

    /// <summary>
    /// One editable argument inside a node. Reads/writes the boxed Lua-syntax string on the underlying
    /// <see cref="TriggerNodeArgument"/>, porting the box/unbox logic of the previous <c>ArgumentEditor</c>.
    /// </summary>
    public partial class ArgumentViewModel : ObservableObject
    {
        private readonly System.Collections.Generic.List<TriggerNodeArgument> _arguments;
        private readonly int _index;
        private readonly ArgumentLayout _layout;
        private readonly ArgumentDataProvider _provider;
        private readonly double _nodeWidth;
        private readonly IColorPickerService _colorPickerService;
        private bool _loading;

        public ArgumentEditorKind Kind { get; }
        public string Caption => string.IsNullOrEmpty(_layout.Name) ? _layout.Description : _layout.Name;
        public string Description => _layout.Description;
        public bool NewLine => _layout.NewLine;
        public double FieldWidth => _layout.Width;

        /// <summary>
        /// Pixel width of the field, derived from the catalogue's percentage <see cref="ArgumentLayout.Width"/>
        /// (0-100) relative to the node body, so fields respect the length declared in the XML catalogues.
        /// </summary>
        public double PixelWidth => Math.Max(24.0, (_nodeWidth - 16.0) * (_layout.Width / 100.0));

        // Numerical limits (shared by Numerical / Vector2 / Vector3).
        public double Minimum { get; private set; } = -1000000.0;
        public double Maximum { get; private set; } = 1000000.0;
        public double Increment { get; private set; } = 1.0;
        public double LargeIncrement { get; private set; } = 5.0;
        public int DecimalPlaces { get; private set; }

        public ArgumentViewModel(System.Collections.Generic.List<TriggerNodeArgument> arguments, int index, ArgumentLayout layout, ArgumentDataProvider provider, double nodeWidth,
            IColorPickerService? colorPickerService = null)
        {
            _arguments = arguments;
            _index = index;
            _layout = layout;
            _provider = provider;
            _nodeWidth = nodeWidth;
            _colorPickerService = ServiceLocator.ResolveService(colorPickerService);
            Kind = MapKind(layout.Type);

            ParseNumericLimits();

            if (Kind == ArgumentEditorKind.List)
            {
                Items = _provider.GetItems(layout.Type, layout);
                ListAction = ArgumentDataProvider.ActionFor(layout.Type);
            }
            else
            {
                Items = Array.Empty<ArgumentListItem>();
            }

            Unbox(_arguments[_index].Value ?? string.Empty);
        }

        private static ArgumentEditorKind MapKind(ArgumentType type) => type switch
        {
            ArgumentType.Boolean => ArgumentEditorKind.Boolean,
            ArgumentType.Numerical => ArgumentEditorKind.Numerical,
            ArgumentType.Vector2 => ArgumentEditorKind.Vector2,
            ArgumentType.Vector3 => ArgumentEditorKind.Vector3,
            ArgumentType.Color => ArgumentEditorKind.Color,
            ArgumentType.Time => ArgumentEditorKind.Time,
            ArgumentType.String => ArgumentEditorKind.String,
            _ => ArgumentEditorKind.List
        };

        private void ParseNumericLimits()
        {
            var custom = _layout.CustomEnumeration;
            if (_layout.Type is not (ArgumentType.Numerical or ArgumentType.Vector2 or ArgumentType.Vector3) || custom.Count < 2)
                return;

            // Catalogue values are written with invariant decimal points, so parse them culture-independently.
            if (float.TryParse(custom[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float min)) Minimum = min;
            if (float.TryParse(custom[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float max)) Maximum = max;
            if (custom.Count >= 3 && int.TryParse(custom[2], out int dec)) DecimalPlaces = dec;
            if (custom.Count >= 4 && float.TryParse(custom[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float step1)) Increment = step1;
            if (custom.Count >= 5 && float.TryParse(custom[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float step2)) LargeIncrement = step2;
        }

        // Boolean.

        [ObservableProperty] private bool _boolValue;
        partial void OnBoolValueChanged(bool value) => Box();

        // Numerical.

        [ObservableProperty] private double _numValue;
        partial void OnNumValueChanged(double value) => Box();

        // Vector2 / Vector3.

        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private double _z;
        partial void OnXChanged(double value) => Box();
        partial void OnYChanged(double value) => Box();
        partial void OnZChanged(double value) => Box();

        // Time.

        [ObservableProperty] private double _hours;
        [ObservableProperty] private double _minutes;
        [ObservableProperty] private double _seconds;
        [ObservableProperty] private double _cents;
        partial void OnHoursChanged(double value) => Box();
        partial void OnMinutesChanged(double value) => Box();
        partial void OnSecondsChanged(double value) => Box();
        partial void OnCentsChanged(double value) => Box();

        // String.

        public bool IsMultiline => Kind == ArgumentEditorKind.String && !_layout.CustomEnumeration.Contains("NoMultiline");

        [ObservableProperty] private string _stringValue = string.Empty;
        partial void OnStringValueChanged(string value) => Box();

        // Color.

        [ObservableProperty] private MediaColor _colorValue = MediaColor.FromRgb(0, 0, 0);
        partial void OnColorValueChanged(MediaColor value)
        {
            OnPropertyChanged(nameof(ColorBrush));
            Box();
        }

        public System.Windows.Media.Brush ColorBrush => new System.Windows.Media.SolidColorBrush(ColorValue);

        [RelayCommand]
        private void PickColor()
        {
            MediaColor oldColor = ColorValue;

            // Live-preview while picking; restore the old color on cancel.
            Vector3? picked = _colorPickerService.PickColor(
                new Vector3(oldColor.R, oldColor.G, oldColor.B) / 255.0f,
                c => ColorValue = ToMediaColor(c));

            ColorValue = picked is { } color ? ToMediaColor(color) : oldColor;
        }

        private static MediaColor ToMediaColor(Vector3 color) => MediaColor.FromRgb(
            (byte)Math.Clamp(Math.Round(color.X * 255.0), 0.0, 255.0),
            (byte)Math.Clamp(Math.Round(color.Y * 255.0), 0.0, 255.0),
            (byte)Math.Clamp(Math.Round(color.Z * 255.0), 0.0, 255.0));

        // List.

        public IReadOnlyList<ArgumentListItem> Items { get; }
        public ArgumentListAction ListAction { get; }
        public bool HasAction => ListAction != ArgumentListAction.None;

        [ObservableProperty] private ArgumentListItem? _selectedItem;
        partial void OnSelectedItemChanged(ArgumentListItem? value) => Box();

        [RelayCommand]
        private void RunAction()
        {
            switch (ListAction)
            {
                case ArgumentListAction.Locate:
                    _provider.Locate(SelectedItem);
                    break;
                case ArgumentListAction.Play:
                    _provider.Play(_layout.Type, SelectedItem);
                    break;
            }
        }

        // Drag-drop from the editor (ports the previous ArgumentEditor drop handlers).

        public bool AcceptsDrop => _layout.Type is ArgumentType.Vector3 or ArgumentType.Color or ArgumentType.WadSlots || IsLuaNameDrop;

        private bool IsLuaNameDrop => _layout.Type is ArgumentType.Moveables or ArgumentType.Cameras or
            ArgumentType.FlybyCameras or ArgumentType.Sinks or ArgumentType.Statics or ArgumentType.Volumes;

        public void HandleDrop(object? data)
        {
            if (data == null)
                return;

            switch (_layout.Type)
            {
                case ArgumentType.Vector3:
                    if (data is PositionBasedObjectInstance position)
                    {
                        X = position.WorldPosition.X;
                        Y = -position.WorldPosition.Y;
                        Z = position.WorldPosition.Z;
                    }
                    break;

                case ArgumentType.Color:
                    if (data is IColorable colorable)
                    {
                        var color = colorable.Color * 0.5f;
                        ColorValue = MediaColor.FromRgb(ToByte(color.X), ToByte(color.Y), ToByte(color.Z));
                    }
                    break;

                case ArgumentType.WadSlots:
                    if (data is WadMoveable moveable)
                        SelectByValue(LuaSyntax.ObjectIDPrefix + LuaSyntax.Splitter + TrCatalog.GetMoveableName(TRVersion.Game.TombEngine, moveable.Id.TypeId));
                    break;

                default:
                    if (IsLuaNameDrop && data is PositionAndScriptBasedObjectInstance instance && !string.IsNullOrEmpty(instance.LuaName))
                        SelectByValue(TextExtensions.Quote(instance.LuaName));
                    break;
            }
        }

        private void SelectByValue(string value)
        {
            var item = Items.FirstOrDefault(i => i.Value == value);
            if (item != null)
                SelectedItem = item;
        }

        private static byte ToByte(float value) => (byte)Math.Clamp(value * 255.0f, 0, 255);

        // Boxing (view -> model Lua string).

        private void Box()
        {
            if (_loading)
                return;

            var argument = _arguments[_index];
            argument.Value = Kind switch
            {
                ArgumentEditorKind.Boolean => BoolValue.ToString().ToLowerInvariant(),
                ArgumentEditorKind.Numerical => F(NumValue),
                ArgumentEditorKind.Vector2 => LuaSyntax.Vec2TypePrefix + LuaSyntax.BracketOpen + F(X) + LuaSyntax.Separator + F(Y) + LuaSyntax.BracketClose,
                ArgumentEditorKind.Vector3 => LuaSyntax.Vec3TypePrefix + LuaSyntax.BracketOpen + F(X) + LuaSyntax.Separator + F(Y) + LuaSyntax.Separator + F(Z) + LuaSyntax.BracketClose,
                ArgumentEditorKind.Color => LuaSyntax.ColorTypePrefix + LuaSyntax.BracketOpen + ColorValue.R + LuaSyntax.Separator + ColorValue.G + LuaSyntax.Separator + ColorValue.B + LuaSyntax.BracketClose,
                ArgumentEditorKind.Time => LuaSyntax.TimeTypePrefix + LuaSyntax.BracketOpen + LuaSyntax.TableOpen + (int)Hours + LuaSyntax.Separator + (int)Minutes + LuaSyntax.Separator + (int)Seconds + LuaSyntax.Separator + (int)Cents + LuaSyntax.TableClose + LuaSyntax.BracketClose,
                ArgumentEditorKind.String => TextExtensions.Quote(TextExtensions.EscapeQuotes(StringValue)),
                ArgumentEditorKind.List => SelectedItem?.Value ?? string.Empty,
                _ => argument.Value
            };
            _arguments[_index] = argument;
        }

        // Lua scripts require invariant decimal points, so never serialize with the current culture.
        private static string F(double value) => ((float)value).ToString(CultureInfo.InvariantCulture);

        // Unboxing (model Lua string -> view).

        private void Unbox(string source)
        {
            _loading = true;
            try
            {
                switch (Kind)
                {
                    case ArgumentEditorKind.Boolean:
                        if (float.TryParse(source, NumberStyles.Float, CultureInfo.InvariantCulture, out float bf))
                            BoolValue = bf != 0.0f;
                        else if (bool.TryParse(source, out bool bb))
                            BoolValue = bb;
                        else
                            BoolValue = false;
                        break;

                    case ArgumentEditorKind.Numerical:
                        double n;
                        if (bool.TryParse(source, out bool pb))
                            n = pb ? 1.0 : 0.0;
                        else if (!double.TryParse(source, NumberStyles.Float, CultureInfo.InvariantCulture, out n))
                            n = 0.0;
                        NumValue = Clamp(Math.Round(n, DecimalPlaces));
                        break;

                    case ArgumentEditorKind.Vector2:
                        {
                            var f = UnboxVector(Strip(source, LuaSyntax.Vec2TypePrefix));
                            X = Clamp(Get(f, 0));
                            Y = Clamp(Get(f, 1));
                            break;
                        }

                    case ArgumentEditorKind.Vector3:
                        {
                            var f = UnboxVector(Strip(source, LuaSyntax.Vec3TypePrefix));
                            X = Clamp(Get(f, 0));
                            Y = Clamp(Get(f, 1));
                            Z = Clamp(Get(f, 2));
                            break;
                        }

                    case ArgumentEditorKind.Color:
                        {
                            var f = UnboxVector(Strip(source, LuaSyntax.ColorTypePrefix));
                            byte r = (byte)Math.Clamp(Get(f, 0), 0, 255);
                            byte g = (byte)Math.Clamp(Get(f, 1), 0, 255);
                            byte b = (byte)Math.Clamp(Get(f, 2), 0, 255);
                            ColorValue = MediaColor.FromRgb(r, g, b);
                            break;
                        }

                    case ArgumentEditorKind.Time:
                        {
                            string s = source;
                            string prefix = LuaSyntax.TimeTypePrefix + LuaSyntax.BracketOpen + LuaSyntax.TableOpen;
                            if (s.StartsWith(prefix) && s.EndsWith(LuaSyntax.TableClose + LuaSyntax.BracketClose))
                                s = s.Substring(prefix.Length, s.Length - prefix.Length - 2);
                            var f = UnboxVector(s);
                            Hours = Get(f, 0);
                            Minutes = Get(f, 1);
                            Seconds = Get(f, 2);
                            Cents = Get(f, 3);
                            break;
                        }

                    case ArgumentEditorKind.String:
                        StringValue = TextExtensions.UnescapeQuotes(TextExtensions.Unquote(source));
                        break;

                    case ArgumentEditorKind.List:
                        {
                            var item = Items.FirstOrDefault(i => i.Value == source);
                            if (item == null)
                            {
                                float index;
                                if (bool.TryParse(source, out bool pv))
                                    index = pv ? 1 : 0;
                                else if (!float.TryParse(source, NumberStyles.Float, CultureInfo.InvariantCulture, out index))
                                    index = -1;

                                if (index >= 0 && index < Items.Count)
                                    item = Items[(int)index];
                                else
                                    item = Items.Count > 0 ? Items[0] : null;
                            }
                            SelectedItem = item;
                            break;
                        }
                }
            }
            finally
            {
                _loading = false;
            }

            // Write back the normalized boxed value so the model matches the editor's representation.
            Box();
        }

        private double Clamp(double value) => Math.Clamp(value, Minimum, Maximum);

        private static string Strip(string source, string prefix)
        {
            if (source.StartsWith(prefix + LuaSyntax.BracketOpen) && source.EndsWith(LuaSyntax.BracketClose))
                return source.Substring(prefix.Length + 1, source.Length - prefix.Length - 2);
            return source;
        }

        private static double Get(double[] values, int index) => index < values.Length ? values[index] : 0.0;

        private static double[] UnboxVector(string source)
        {
            return source.Split(new[] { LuaSyntax.Separator }, StringSplitOptions.None)
                .Select(x => double.TryParse(x.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double r) ? r : 0.0)
                .ToArray();
        }
    }
}
