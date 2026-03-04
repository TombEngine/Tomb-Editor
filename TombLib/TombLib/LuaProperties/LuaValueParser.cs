using System;
using System.Globalization;
using System.Linq;
using NLog;
using TombLib.Utils;

// Shared Lua value boxing/unboxing abstraction.
// Used by both the visual scripting ArgumentEditor and the property grid system.

namespace TombLib.LuaProperties
{
    /// <summary>
    /// Provides static methods for boxing (C# → Lua string) and unboxing (Lua string → C#)
    /// of property values in TombEngine's Lua format.
    /// All methods are defensive and will not throw on malformed input.
    /// </summary>
    public static class LuaValueParser
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Culture-invariant formatting to prevent locale issues with decimals.
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        #region Boxing (C# values → Lua text)

        public static string BoxBool(bool value) => value ? "true" : "false";

        public static string BoxInt(int value) => value.ToString(Inv);

        public static string BoxFloat(float value) => value.ToString(Inv);

        public static string BoxString(string value) => TextExtensions.Quote(TextExtensions.EscapeQuotes(value ?? string.Empty));

        public static string BoxVec2(float x, float y)
        {
            return LuaSyntax.Vec2TypePrefix + LuaSyntax.BracketOpen +
                   x.ToString(Inv) + LuaSyntax.Separator +
                   y.ToString(Inv) + LuaSyntax.BracketClose;
        }

        public static string BoxVec3(float x, float y, float z)
        {
            return LuaSyntax.Vec3TypePrefix + LuaSyntax.BracketOpen +
                   x.ToString(Inv) + LuaSyntax.Separator +
                   y.ToString(Inv) + LuaSyntax.Separator +
                   z.ToString(Inv) + LuaSyntax.BracketClose;
        }

        public static string BoxRotation(float x, float y, float z)
        {
            return LuaSyntax.RotationTypePrefix + LuaSyntax.BracketOpen +
                   x.ToString(Inv) + LuaSyntax.Separator +
                   y.ToString(Inv) + LuaSyntax.Separator +
                   z.ToString(Inv) + LuaSyntax.BracketClose;
        }

        public static string BoxColor(byte r, byte g, byte b, byte? a = null)
        {
            var result = LuaSyntax.ColorTypePrefix + LuaSyntax.BracketOpen +
                         r.ToString(Inv) + LuaSyntax.Separator +
                         g.ToString(Inv) + LuaSyntax.Separator +
                         b.ToString(Inv);

            if (a.HasValue)
                result += LuaSyntax.Separator + a.Value.ToString(Inv);

            result += LuaSyntax.BracketClose;
            return result;
        }

        public static string BoxTime(int hours, int minutes, int seconds, int centiseconds)
        {
            return LuaSyntax.TimeTypePrefix + LuaSyntax.BracketOpen + LuaSyntax.TableOpen +
                   hours.ToString(Inv) + LuaSyntax.Separator +
                   minutes.ToString(Inv) + LuaSyntax.Separator +
                   seconds.ToString(Inv) + LuaSyntax.Separator +
                   centiseconds.ToString(Inv) +
                   LuaSyntax.TableClose + LuaSyntax.BracketClose;
        }

        #endregion

        #region Unboxing (Lua text → C# values)

        public static bool UnboxBool(string source, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(source))
                return defaultValue;

            source = source.Trim();

            if (bool.TryParse(source, out bool boolResult))
                return boolResult;

            if (float.TryParse(source, NumberStyles.Float, Inv, out float floatResult))
                return floatResult != 0.0f;

            return defaultValue;
        }

        public static int UnboxInt(string source, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(source))
                return defaultValue;

            source = source.Trim();

            // Try int first, then float (truncated).
            if (int.TryParse(source, NumberStyles.Integer, Inv, out int intResult))
                return intResult;

            if (float.TryParse(source, NumberStyles.Float, Inv, out float floatResult))
                return (int)floatResult;

            if (bool.TryParse(source, out bool boolResult))
                return boolResult ? 1 : 0;

            return defaultValue;
        }

        public static float UnboxFloat(string source, float defaultValue = 0.0f)
        {
            if (string.IsNullOrWhiteSpace(source))
                return defaultValue;

            source = source.Trim();

            if (float.TryParse(source, NumberStyles.Float, Inv, out float result))
                return result;

            if (bool.TryParse(source, out bool boolResult))
                return boolResult ? 1.0f : 0.0f;

            return defaultValue;
        }

        public static string UnboxString(string source, string defaultValue = "")
        {
            if (source == null)
                return defaultValue;

            return TextExtensions.UnescapeQuotes(TextExtensions.Unquote(source));
        }

        public static float[] UnboxVec2(string source)
        {
            var defaults = new float[] { 0, 0 };
            if (string.IsNullOrWhiteSpace(source))
                return defaults;

            source = StripTypePrefix(source, LuaSyntax.Vec2TypePrefix);
            var components = SplitComponents(source);

            if (components.Length < 2)
            {
                logger.Warn("UnboxVec2: Expected 2 components, got " + components.Length);
                return defaults;
            }

            return new float[]
            {
                ParseFloat(components[0]),
                ParseFloat(components[1])
            };
        }

        public static float[] UnboxVec3(string source)
        {
            var defaults = new float[] { 0, 0, 0 };
            if (string.IsNullOrWhiteSpace(source))
                return defaults;

            source = StripTypePrefix(source, LuaSyntax.Vec3TypePrefix);
            var components = SplitComponents(source);

            if (components.Length < 3)
            {
                logger.Warn("UnboxVec3: Expected 3 components, got " + components.Length);
                return defaults;
            }

            return new float[]
            {
                ParseFloat(components[0]),
                ParseFloat(components[1]),
                ParseFloat(components[2])
            };
        }

        public static float[] UnboxRotation(string source)
        {
            var defaults = new float[] { 0, 0, 0 };
            if (string.IsNullOrWhiteSpace(source))
                return defaults;

            source = StripTypePrefix(source, LuaSyntax.RotationTypePrefix);
            var components = SplitComponents(source);

            if (components.Length < 3)
            {
                logger.Warn("UnboxRotation: Expected 3 components, got " + components.Length);
                return defaults;
            }

            return new float[]
            {
                ClampAngle(ParseFloat(components[0])),
                ClampAngle(ParseFloat(components[1])),
                ClampAngle(ParseFloat(components[2]))
            };
        }

        public static byte[] UnboxColor(string source)
        {
            var defaults = new byte[] { 0, 0, 0, 255 };
            if (string.IsNullOrWhiteSpace(source))
                return defaults;

            source = StripTypePrefix(source, LuaSyntax.ColorTypePrefix);
            var components = SplitComponents(source);

            if (components.Length < 3)
            {
                logger.Warn("UnboxColor: Expected at least 3 components, got " + components.Length);
                return defaults;
            }

            byte r = ClampByte(ParseFloat(components[0]));
            byte g = ClampByte(ParseFloat(components[1]));
            byte b = ClampByte(ParseFloat(components[2]));
            byte a = components.Length > 3 ? ClampByte(ParseFloat(components[3])) : (byte)255;

            return new byte[] { r, g, b, a };
        }

        public static int[] UnboxTime(string source)
        {
            var defaults = new int[] { 0, 0, 0, 0 };
            if (string.IsNullOrWhiteSpace(source))
                return defaults;

            source = StripTypePrefix(source, LuaSyntax.TimeTypePrefix);
            source = source.Replace(LuaSyntax.TableOpen, "").Replace(LuaSyntax.TableClose, "");

            var components = SplitComponents(source);

            if (components.Length < 4)
            {
                logger.Warn("UnboxTime: Expected 4 components, got " + components.Length);
                return defaults;
            }

            return new int[]
            {
                (int)ParseFloat(components[0]),
                (int)ParseFloat(components[1]),
                (int)ParseFloat(components[2]),
                (int)ParseFloat(components[3])
            };
        }

        #endregion

        #region Generic boxing by type

        /// <summary>
        /// Boxes a property value to its Lua text representation based on the declared type.
        /// Returns a default value string if the input is null or empty.
        /// </summary>
        public static string BoxByType(LuaPropertyType type, object value)
        {
            try
            {
                switch (type)
                {
                    case LuaPropertyType.Bool:
                        return BoxBool(value is bool b ? b : false);

                    case LuaPropertyType.Int:
                        return BoxInt(value is int i ? i : 0);

                    case LuaPropertyType.Float:
                        return BoxFloat(value is float f ? f : 0.0f);

                    case LuaPropertyType.String:
                        return BoxString(value as string ?? string.Empty);

                    case LuaPropertyType.Vec2:
                        if (value is float[] v2 && v2.Length >= 2)
                            return BoxVec2(v2[0], v2[1]);
                        return BoxVec2(0, 0);

                    case LuaPropertyType.Vec3:
                        if (value is float[] v3 && v3.Length >= 3)
                            return BoxVec3(v3[0], v3[1], v3[2]);
                        return BoxVec3(0, 0, 0);

                    case LuaPropertyType.Rotation:
                        if (value is float[] rot && rot.Length >= 3)
                            return BoxRotation(rot[0], rot[1], rot[2]);
                        return BoxRotation(0, 0, 0);

                    case LuaPropertyType.Color:
                        if (value is byte[] c && c.Length >= 3)
                            return c.Length > 3 && c[3] != 255 ? BoxColor(c[0], c[1], c[2], c[3]) : BoxColor(c[0], c[1], c[2]);
                        return BoxColor(0, 0, 0);

                    case LuaPropertyType.Time:
                        if (value is int[] t && t.Length >= 4)
                            return BoxTime(t[0], t[1], t[2], t[3]);
                        return BoxTime(0, 0, 0, 0);

                    case LuaPropertyType.Enum:
                        return BoxInt(value is int enumVal ? enumVal : 0); // Enum values are stored as 0-based integers.

                    default:
                        logger.Warn("BoxByType: Unknown property type {0}", type);
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "BoxByType failed for type {0}", type);
                return GetDefaultBoxedValue(type);
            }
        }

        /// <summary>
        /// Returns the default boxed Lua string for a given property type.
        /// </summary>
        public static string GetDefaultBoxedValue(LuaPropertyType type)
        {
            switch (type)
            {
                case LuaPropertyType.Bool:     return BoxBool(false);
                case LuaPropertyType.Int:      return BoxInt(0);
                case LuaPropertyType.Float:    return BoxFloat(0);
                case LuaPropertyType.String:   return BoxString(string.Empty);
                case LuaPropertyType.Vec2:     return BoxVec2(0, 0);
                case LuaPropertyType.Vec3:     return BoxVec3(0, 0, 0);
                case LuaPropertyType.Rotation: return BoxRotation(0, 0, 0);
                case LuaPropertyType.Color:    return BoxColor(0, 0, 0);
                case LuaPropertyType.Time:     return BoxTime(0, 0, 0, 0);
                case LuaPropertyType.Enum:     return BoxInt(0);
                default: return string.Empty;
            }
        }

        /// <summary>
        /// Validates that a boxed Lua string is compatible with the declared property type.
        /// Returns true if valid, false if malformed.
        /// </summary>
        public static bool ValidateBoxedValue(LuaPropertyType type, string boxedValue)
        {
            if (string.IsNullOrWhiteSpace(boxedValue))
                return false;

            try
            {
                switch (type)
                {
                    case LuaPropertyType.Bool:
                        UnboxBool(boxedValue);
                        return true;

                    case LuaPropertyType.Int:
                        return int.TryParse(boxedValue.Trim(), NumberStyles.Integer, Inv, out _) ||
                               float.TryParse(boxedValue.Trim(), NumberStyles.Float, Inv, out _);

                    case LuaPropertyType.Float:
                        return float.TryParse(boxedValue.Trim(), NumberStyles.Float, Inv, out _);

                    case LuaPropertyType.String:
                        return boxedValue.StartsWith(TextExtensions.QuoteChar) &&
                               boxedValue.EndsWith(TextExtensions.QuoteChar);

                    case LuaPropertyType.Vec2:
                        return boxedValue.StartsWith(LuaSyntax.Vec2TypePrefix);

                    case LuaPropertyType.Vec3:
                        return boxedValue.StartsWith(LuaSyntax.Vec3TypePrefix);

                    case LuaPropertyType.Rotation:
                        return boxedValue.StartsWith(LuaSyntax.RotationTypePrefix);

                    case LuaPropertyType.Color:
                        return boxedValue.StartsWith(LuaSyntax.ColorTypePrefix);

                    case LuaPropertyType.Time:
                        return boxedValue.StartsWith(LuaSyntax.TimeTypePrefix);

                    case LuaPropertyType.Enum:
                        return int.TryParse(boxedValue.Trim(), NumberStyles.Integer, Inv, out int enumIndex) && enumIndex >= 0;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Internal helpers

        /// <summary>
        /// Strips a type prefix and surrounding brackets from a Lua constructor string.
        /// E.g. "TEN.Vec3(1,2,3)" → "1,2,3"
        /// </summary>
        public static string StripTypePrefix(string source, string prefix)
        {
            if (source.StartsWith(prefix + LuaSyntax.BracketOpen) && source.EndsWith(LuaSyntax.BracketClose))
                return source.Substring(prefix.Length + 1, source.Length - prefix.Length - 2);

            return source;
        }

        /// <summary>
        /// Splits a comma-separated value string into trimmed components.
        /// Reused by both ArgumentEditor and property grid system.
        /// </summary>
        public static float[] SplitAndParseFloats(string source)
        {
            return source.Split(new string[] { LuaSyntax.Separator }, StringSplitOptions.None)
                         .Select(x => ParseFloat(x.Trim()))
                         .ToArray();
        }

        private static string[] SplitComponents(string source)
        {
            return source.Split(new string[] { LuaSyntax.Separator }, StringSplitOptions.None)
                         .Select(x => x.Trim())
                         .ToArray();
        }

        private static float ParseFloat(string source)
        {
            if (float.TryParse(source, NumberStyles.Float, Inv, out float result))
                return result;

            return 0.0f;
        }

        private static float ClampAngle(float value)
        {
            // Normalize to 0-360 range.
            value = value % 360.0f;
            if (value < 0)
                value += 360.0f;
            return value;
        }

        private static byte ClampByte(float value)
        {
            return (byte)Math.Max(0, Math.Min(255, (int)value));
        }

        #endregion
    }
}
