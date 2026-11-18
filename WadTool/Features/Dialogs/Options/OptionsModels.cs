#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace WadTool.Features.Dialogs.Options
{
    /// <summary>
    /// The kind of editor an <see cref="OptionItem"/> is rendered with. Mirrors the control types the
    /// legacy <c>FormOptionsBase</c> supported (checkbox / numeric / text / combo / color).
    /// </summary>
    public enum OptionKind
    {
        Bool,
        Number,
        Text,
        Combo,
        Color
    }

    /// <summary>
    /// A single configurable option. <see cref="ConfigName"/> is the <see cref="Configuration"/>
    /// property name the value is read from / written to by reflection, exactly like the previous
    /// control <c>Tag</c> did in the old FormOptions.
    /// </summary>
    public partial class OptionItem : ObservableObject
    {
        public string ConfigName { get; init; } = string.Empty;
        public OptionKind Kind { get; init; }
        public string Label { get; init; } = string.Empty;
        public string? Suffix { get; init; }

        // Numeric metadata.
        public double Minimum { get; init; }
        public double Maximum { get; init; } = 1_000_000;
        public double Increment { get; init; } = 1;
        public int DecimalPlaces { get; init; }

        // Combo items.
        public IReadOnlyList<object>? Items { get; init; }

        /// <summary>The live value bound to the editor control (bool, double, string, combo item or Color).</summary>
        [ObservableProperty] private object? _value;

        [ObservableProperty] private bool _isVisible = true;
    }

    public partial class OptionGroup : ObservableObject
    {
        public string Header { get; init; } = string.Empty;
        public List<OptionItem> Items { get; } = new();

        [ObservableProperty] private bool _isVisible = true;
    }

    public partial class OptionTab : ObservableObject
    {
        public string Header { get; init; } = string.Empty;
        public List<OptionGroup> Groups { get; } = new();

        public IEnumerable<OptionItem> AllItems => Groups.SelectMany(g => g.Items);

        [ObservableProperty] private bool _isVisible = true;
    }
}
