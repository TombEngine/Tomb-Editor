// DataTemplateSelector that picks the correct editor template
// based on the Lua property type (Bool, Int, Float, String, Vec2, Vec3, etc.).
// Used by LuaPropertyGridControl to dynamically render the value column.

using System.Windows;
using System.Windows.Controls;
using TombLib.LuaProperties;

namespace TombLib.Forms.Views
{
    /// <summary>
    /// Selects the correct DataTemplate for a property row's value editor
    /// based on the property's <see cref="LuaPropertyType"/>.
    /// </summary>
    public class LuaPropertyEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BoolTemplate { get; set; }
        public DataTemplate IntTemplate { get; set; }
        public DataTemplate FloatTemplate { get; set; }
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate Vec2Template { get; set; }
        public DataTemplate Vec3Template { get; set; }
        public DataTemplate RotationTemplate { get; set; }
        public DataTemplate ColorTemplate { get; set; }
        public DataTemplate TimeTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ViewModels.LuaPropertyRowViewModel row)
            {
                switch (row.PropertyType)
                {
                    case LuaPropertyType.Bool: return BoolTemplate;
                    case LuaPropertyType.Int: return IntTemplate;
                    case LuaPropertyType.Float: return FloatTemplate;
                    case LuaPropertyType.String: return StringTemplate;
                    case LuaPropertyType.Vec2: return Vec2Template;
                    case LuaPropertyType.Vec3: return Vec3Template;
                    case LuaPropertyType.Rotation: return RotationTemplate;
                    case LuaPropertyType.Color: return ColorTemplate;
                    case LuaPropertyType.Time: return TimeTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
