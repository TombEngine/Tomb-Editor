#nullable enable

using System.Windows;
using System.Windows.Controls;

namespace WadTool.Features.Dialogs.Options
{
    /// <summary>
    /// Picks the editor <see cref="DataTemplate"/> for an <see cref="OptionItem"/> from the window
    /// resources based on its <see cref="OptionItem.Kind"/>.
    /// </summary>
    public class OptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? BoolTemplate { get; set; }
        public DataTemplate? NumberTemplate { get; set; }
        public DataTemplate? TextTemplate { get; set; }
        public DataTemplate? ComboTemplate { get; set; }
        public DataTemplate? ColorTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return (item as OptionItem)?.Kind switch
            {
                OptionKind.Bool => BoolTemplate,
                OptionKind.Number => NumberTemplate,
                OptionKind.Text => TextTemplate,
                OptionKind.Combo => ComboTemplate,
                OptionKind.Color => ColorTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
