#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.ClassicScript;

public sealed partial class ReferenceBrowserViewModel : ObservableObject
{
	private readonly ClassicScriptReferenceBrowserDataProvider _dataProvider;
	private readonly IMessageService _messageService;
	private readonly ILocalizationService _localizationService;

	internal ReferenceBrowserViewModel(
		IMessageService messageService,
		ILocalizationService localizationService,
		ClassicScriptReferenceBrowserDataProvider? dataProvider = null)
	{
		ArgumentNullException.ThrowIfNull(messageService);
		ArgumentNullException.ThrowIfNull(localizationService);

		_dataProvider = dataProvider ?? new ClassicScriptReferenceBrowserDataProvider();
		_messageService = messageService;
		_localizationService = localizationService.WithKeysFor(this);

		Categories =
		[
			new ReferenceBrowserCategoryViewModel(ReferenceItemType.MnemonicConstants, _localizationService["MnemonicConstants"]),
			new ReferenceBrowserCategoryViewModel(ReferenceItemType.EnemyDamageValues, _localizationService["EnemyDamageValues"]),
			new ReferenceBrowserCategoryViewModel(ReferenceItemType.KeyboardScancodes, _localizationService["KeyboardScancodes"]),
			new ReferenceBrowserCategoryViewModel(ReferenceItemType.OCBList, _localizationService["OCBList"]),
			new ReferenceBrowserCategoryViewModel(ReferenceItemType.OldCommandsList, _localizationService["OldCommandsList"]),
			new ReferenceBrowserCategoryViewModel(ReferenceItemType.NewCommandsList, _localizationService["NewCommandsList"]),
			new ReferenceBrowserCategoryViewModel(ReferenceItemType.SoundIndices, _localizationService["SoundIndices"]),
			new ReferenceBrowserCategoryViewModel(ReferenceItemType.MoveableSlotIndices, _localizationService["MoveableSlotIndices"]),
			new ReferenceBrowserCategoryViewModel(ReferenceItemType.StaticObjectIndices, _localizationService["StaticObjectIndices"]),
			new ReferenceBrowserCategoryViewModel(ReferenceItemType.VariablePlaceholders, _localizationService["VariablePlaceholders"])
		];

		SelectedCategory = Categories[0];
	}

	public ObservableCollection<ReferenceBrowserCategoryViewModel> Categories { get; }

	public string Title => _localizationService["Title"];

	public string SearchLabel => _localizationService["SearchLabel"];

	public string CopyLabel => _localizationService["CopyLabel"];

	public DataView? Rows => _rowsTable?.DefaultView;

	[ObservableProperty]
	private string _searchText = string.Empty;

	[ObservableProperty]
	private ReferenceBrowserCategoryViewModel? _selectedCategory;

	[ObservableProperty]
	private DataRowView? _selectedRow;

	private DataTable? _rowsTable;

	/// <summary>
	/// Raised when the user double-clicks a reference definition row.
	/// </summary>
	public event EventHandler<ReferenceDefinitionEventArgs>? ReferenceDefinitionRequested;

	public bool CanCopy => SelectedRow is not null;

	partial void OnSelectedRowChanged(DataRowView? value)
		=> OnPropertyChanged(nameof(CanCopy));

	[RelayCommand]
	private void GoToReferenceDefinition()
	{
		if (TryCreateReferenceDefinition(SelectedRow) is ReferenceDefinitionEventArgs definition)
			ReferenceDefinitionRequested?.Invoke(this, definition);
	}

	[RelayCommand]
	private void CopySelectedRow()
	{
		if (SelectedRow is null)
			return;

		string rowText = string.Join("\t", SelectedRow.Row.ItemArray[..^1]);
		Clipboard.SetText(rowText);
	}

	public string GetColumnHeader(string columnName) => columnName switch
	{
		"decimal" => _localizationService["DecimalValue"],
		"hex" => _localizationService["HexadecimalValue"],
		"flag" => _localizationService["Macro"],
		"argument1" => _localizationService.Format("ArgumentRange", 1),
		"argument2" => _localizationService.Format("ArgumentRange", 2),
		"argument3" => _localizationService.Format("ArgumentRange", 3),
		"variable" => _localizationService["Variable"],
		"description" => _localizationService["Description"],
		"sounds" => _localizationService["Sounds"],
		_ => columnName
	};

	public ReferenceDefinitionEventArgs? TryCreateReferenceDefinition(DataRowView? row)
	{
		if (row is null || SelectedCategory is null)
			return null;

		return SelectedCategory.ItemType switch
		{
			ReferenceItemType.MnemonicConstants => CreateReferenceDefinition(row, 2, ReferenceType.MnemonicConstant),
			ReferenceItemType.OldCommandsList => CreateReferenceDefinition(row, 0, ReferenceType.OldCommand),
			ReferenceItemType.NewCommandsList => CreateReferenceDefinition(row, 0, ReferenceType.NewCommand),
			ReferenceItemType.OCBList => CreateReferenceDefinition(row, 0, ReferenceType.OCB),
			_ => null
		};
	}

	partial void OnSearchTextChanged(string value)
		=> ApplyFilter();

	partial void OnSelectedCategoryChanged(ReferenceBrowserCategoryViewModel? value)
	{
		foreach (ReferenceBrowserCategoryViewModel category in Categories)
			category.IsSelected = ReferenceEquals(category, value);

		LoadRows();
	}

	private void ApplyFilter()
	{
		if (_rowsTable?.DefaultView is null)
			return;

		string filter = SearchText.Trim().Replace("'", "''").Replace('%', ' ').Replace('*', ' ');
		_rowsTable.DefaultView.RowFilter = $"[_RowString] LIKE '%{filter}%'";
		OnPropertyChanged(nameof(Rows));
	}

	private static ReferenceDefinitionEventArgs? CreateReferenceDefinition(DataRowView row, int columnIndex, ReferenceType type)
	{
		if (columnIndex >= row.Row.ItemArray.Length)
			return null;

		string? keyword = row.Row[columnIndex]?.ToString();

		return string.IsNullOrWhiteSpace(keyword)
			? null
			: new ReferenceDefinitionEventArgs(keyword, type);
	}

	private void LoadRows()
	{
		if (SelectedCategory is null)
		{
			_rowsTable = null;
			OnPropertyChanged(nameof(Rows));
			return;
		}

		try
		{
			_rowsTable = _dataProvider.GetTable(SelectedCategory.ItemType);
			AddFilterRowString(_rowsTable);
			ApplyFilter();
		}
		catch (Exception ex)
		{
			_rowsTable = null;
			OnPropertyChanged(nameof(Rows));
			_messageService.ShowError(ex.Message, _localizationService["ErrorTitle"]);
		}
	}

	private static void AddFilterRowString(DataTable dataTable)
	{
		const string filterColumnName = "_RowString";

		if (dataTable.Columns.Contains(filterColumnName))
			dataTable.Columns.Remove(filterColumnName);

		DataColumn rowStringColumn = dataTable.Columns.Add(filterColumnName, typeof(string));

		foreach (DataRow dataRow in dataTable.Rows)
		{
			string rowString = string.Join("\t", dataRow.ItemArray[..^1]);
			dataRow[rowStringColumn] = rowString;
		}
	}
}
