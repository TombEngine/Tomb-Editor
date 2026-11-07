using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombEditor.Features.Dialogs.EventSetEditor;

/// <summary>Searchable popup list of node functions (WinForms FormFunctionList): sections of functions, a search box and the description of the selected function.</summary>
public partial class FunctionListWindow : Window
{
    private readonly List<FunctionTreeNode> _sections = [];
    private bool _closing;

    /// <summary>Gets the function that was chosen, or null when the list was dismissed.</summary>
    public NodeFunction? ChosenFunction { get; private set; }

    private NodeFunction? _selectedFunction;

    public FunctionListWindow(IEnumerable<NodeFunction> functions, Point screenPosition)
    {
        InitializeComponent();

        foreach (var group in functions.GroupBy(f => f.Section))
        {
            var section = new FunctionTreeNode(string.IsNullOrEmpty(group.Key) ? "Misc" : group.Key);

            // Conditions are listed before actions inside every section.
            foreach (var function in group.OrderBy(f => !f.Conditional))
                section.Children.Add(new FunctionTreeNode(function.Name ?? string.Empty, function));

            _sections.Add(section);
        }

        functionTree.ItemsSource = _sections;

        // The window opens with its top-left corner at the cursor, kept inside the screen.
        Left = Math.Min(screenPosition.X, SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - Width);
        Top = Math.Min(screenPosition.Y, SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - Height);

        Loaded += (_, _) => searchBox.Focus();
        // Closed after the activation change has finished: closing during it hands the focus to another application.
        Deactivated += (_, _) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => CloseWith(null)));
    }

    private IEnumerable<FunctionTreeNode> Leaves => _sections.SelectMany(section => section.Children);

    private void CloseWith(NodeFunction? function)
    {
        if (_closing)
            return;

        _closing = true;
        ChosenFunction = function;
        var owner = Owner;
        Close();

        // After a choice or Escape the editor takes the focus back; after a click elsewhere it keeps what the user picked.
        if (function is not null || Keyboard.IsKeyDown(Key.Escape))
            owner?.Activate();
    }

    private void SearchNodes(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var nodes = Leaves.Where(n => !string.IsNullOrEmpty(n.Text)).ToList();
        var current = functionTree.SelectedItem is FunctionTreeNode selected ? nodes.IndexOf(selected) : -1;
        var exactMatch = nodes.Any(n => n.Text.Contains(text, StringComparison.OrdinalIgnoreCase));

        for (var i = current + 1; i <= nodes.Count; i++)
        {
            if (i == nodes.Count)
            {
                if (current == -1)
                    break;

                // Restart the search from the top.
                i = -1;
                current = -1;
                continue;
            }

            var match = nodes[i].Text.Contains(text, StringComparison.OrdinalIgnoreCase);
            var distance = Levenshtein.DistanceSubstring(nodes[i].Text.ToLowerInvariant(), text.ToLowerInvariant(), out _);

            if ((exactMatch && match) || (!exactMatch && distance < 2))
            {
                current = i;
                break;
            }
        }

        if (current == -1)
            return;

        var owner = _sections.First(section => section.Children.Contains(nodes[current]));
        owner.IsExpanded = true;
        nodes[current].IsSelected = true;
    }

    private void ApplySelected()
    {
        if (_selectedFunction is not null)
            CloseWith(_selectedFunction);
    }

    private void FunctionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        descriptionText.Text = string.Empty;
        _selectedFunction = null;

        if (e.NewValue is not FunctionTreeNode { Function: { } function })
            return;

        _selectedFunction = function;
        descriptionText.Text = TextExtensions.SingleLineToMultiLine(function.Description);
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            SearchNodes(searchBox.Text);
        else if (e.Key == Key.Escape)
            CloseWith(null);
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e) => SearchNodes(searchBox.Text);

    private void ExpandButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var section in _sections)
            section.IsExpanded = true;
    }

    private void FunctionTree_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ApplySelected();
        else if (e.Key == Key.Escape)
            CloseWith(null);
    }

    private void FunctionTree_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ApplySelected();

    /// <summary>A section or a function of the list.</summary>
    private sealed class FunctionTreeNode(string text, NodeFunction? function = null) : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _isSelected;

        public string Text { get; } = text;

        public NodeFunction? Function { get; } = function;

        public string Kind => Function is null ? "Section" : Function.Conditional ? "Condition" : "Action";

        public ObservableCollection<FunctionTreeNode> Children { get; } = [];

        public bool IsExpanded
        {
            get => _isExpanded;
            set => Set(ref _isExpanded, value, nameof(IsExpanded));
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value, nameof(IsSelected));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Set(ref bool field, bool value, string name)
        {
            if (field == value)
                return;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}