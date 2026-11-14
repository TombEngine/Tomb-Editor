#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TombEditor.Features.Dialogs.Options
{
    /// <summary>
    /// The kind of editor an <see cref="OptionItem"/> is rendered with. Mirrors the control types the
    /// legacy <c>FormOptionsBase</c> supported (checkbox / numeric / text / combo / color), plus the
    /// special color-scheme preset picker.
    /// </summary>
    public enum OptionKind
    {
        Bool,
        Number,
        Text,
        Combo,
        Color,
        ColorSchemePreset,
        BoolWithNumber,
        ComboWithBool,
        BoolRow
    }

    /// <summary>
    /// A single configurable option. <see cref="ConfigName"/> is the <see cref="Configuration"/>
    /// property (or <c>UI_ColorScheme</c> field) name the value is read from / written to by reflection,
    /// exactly like the previous control <c>Tag</c> did in the old FormOptions.
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

        // Combo / preset items.
        public IReadOnlyList<object>? Items { get; init; }

        /// <summary>Companion options shown on the same row as this option (composite kinds only).</summary>
        public List<OptionItem> Children { get; } = new();

        /// <summary>Returns true when the label of this option or of any companion contains the query.</summary>
        public bool MatchesQuery(string query)
            => query.Length == 0
                || Label.Contains(query, StringComparison.OrdinalIgnoreCase)
                || Children.Any(c => c.Label.Contains(query, StringComparison.OrdinalIgnoreCase));

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

        public IEnumerable<OptionItem> AllItems => Groups.SelectMany(g => g.Items).SelectMany(i => i.Children.Prepend(i));

        [ObservableProperty] private bool _isVisible = true;
    }
}
