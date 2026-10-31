#nullable enable

using System.Windows;
using System.Windows.Controls;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>Picks the editor <see cref="DataTemplate"/> for an <see cref="ArgumentViewModel"/> by its <see cref="ArgumentEditorKind"/>.</summary>
    public sealed class ArgumentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? BooleanTemplate { get; set; }
        public DataTemplate? NumericalTemplate { get; set; }
        public DataTemplate? Vector2Template { get; set; }
        public DataTemplate? Vector3Template { get; set; }
        public DataTemplate? ColorTemplate { get; set; }
        public DataTemplate? TimeTemplate { get; set; }
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? ListTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is not ArgumentViewModel argument)
                return base.SelectTemplate(item, container);

            return argument.Kind switch
            {
                ArgumentEditorKind.Boolean => BooleanTemplate,
                ArgumentEditorKind.Numerical => NumericalTemplate,
                ArgumentEditorKind.Vector2 => Vector2Template,
                ArgumentEditorKind.Vector3 => Vector3Template,
                ArgumentEditorKind.Color => ColorTemplate,
                ArgumentEditorKind.Time => TimeTemplate,
                ArgumentEditorKind.String => StringTemplate,
                ArgumentEditorKind.List => ListTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
