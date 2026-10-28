#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLib.Forms.ViewModels;
using TombLib.LuaProperties;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace WadTool.Features.Dialogs.LuaProperties;

/// <summary>A wad object entry in the dialog's object list.</summary>
public sealed record ObjectEntry(IWadObjectId Id, string Name);

public partial class LuaPropertiesWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly WadToolClass _tool;
    private readonly Wad2 _wad;
    private readonly ILocalizationService _localization;

    // Original containers for cancel/restore (key = objectId), snapshotted on first visit per object.
    private readonly Dictionary<IWadObjectId, LuaPropertyContainer> _originalProperties = new();

    // Track whether any property value was actually modified.
    private bool _anyChanges;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private ObjectEntry? _selectedObject;

    /// <summary>All moveables and statics of the wad.</summary>
    public ObservableCollection<ObjectEntry> Objects { get; } = new();

    /// <summary>ViewModel of the embedded <see cref="TombLib.Forms.Views.LuaPropertyGridControl"/>.</summary>
    public LuaPropertyGridViewModel PropertyGrid { get; } = new();

    public LuaPropertiesWindowViewModel(
        WadToolClass tool,
        Wad2 wad,
        IWadObjectId? initialObjectId = null,
        ILocalizationService? localizationService = null)
    {
        _tool = tool;
        _wad = wad;
        _localization = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        // Track actual property modifications (the grid live-writes values to the containers).
        PropertyGrid.PropertyValueChanged += (_, _) => _anyChanges = true;

        PopulateObjectList();

        // Select the requested object, or the first one.
        SelectedObject = (initialObjectId is null
                ? null
                : Objects.FirstOrDefault(entry => entry.Id.Equals(initialObjectId)))
            ?? Objects.FirstOrDefault();
    }

    private void PopulateObjectList()
    {
        Objects.Clear();
        var gameVersion = _wad.GameVersion;

        // Add moveables
        foreach (var kvp in _wad.Moveables)
            Objects.Add(new ObjectEntry(kvp.Key, TrCatalog.GetMoveableName(gameVersion, kvp.Key.TypeId)));

        // Add statics
        foreach (var kvp in _wad.Statics)
            Objects.Add(new ObjectEntry(kvp.Key, TrCatalog.GetStaticName(gameVersion, kvp.Key.TypeId)));
    }

    partial void OnSelectedObjectChanged(ObjectEntry? value)
    {
        if (value is not null)
            LoadObject(value.Id);
    }

    private void LoadObject(IWadObjectId objectId)
    {
        IWadObject? wadObject = _wad.TryGet(objectId);
        if (wadObject is null)
            return;

        // Snapshot original state for cancel/restore (only first time per object).
        if (!_originalProperties.ContainsKey(objectId))
            _originalProperties[objectId] = GetContainer(wadObject)?.Clone() ?? new LuaPropertyContainer();

        // Determine kind and type ID.
        ObjectKind kind;
        uint typeId;
        string objectName;

        if (wadObject is WadMoveable)
        {
            kind = ObjectKind.Moveable;
            typeId = ((WadMoveableId)objectId).TypeId;
            objectName = TrCatalog.GetMoveableName(_wad.GameVersion, typeId);
        }
        else if (wadObject is WadStatic)
        {
            kind = ObjectKind.Static;
            typeId = ((WadStaticId)objectId).TypeId;
            objectName = TrCatalog.GetStaticName(_wad.GameVersion, typeId);
        }
        else
            return;

        PropertyGrid.Title = objectName;

        var definitions = LuaPropertyCatalog.GetDefinitions(kind, typeId, _wad.GameVersion);
        PropertyGrid.Load(definitions, GetContainer(wadObject));

        if (definitions.Count == 0)
            PropertyGrid.StatusMessage = _localization["NoProperties"];
    }

    private static LuaPropertyContainer? GetContainer(IWadObject wadObject) => wadObject switch
    {
        WadMoveable moveable => moveable.LuaProperties,
        WadStatic staticObj => staticObj.LuaProperties,
        _ => null
    };

    [RelayCommand]
    private void Confirm()
    {
        // Values are already written to the containers via the grid ViewModel's live-write.
        if (_anyChanges)
            _tool.ToggleUnsavedChanges();

        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        // Restore all original properties.
        foreach (var kvp in _originalProperties)
        {
            IWadObject? wadObject = _wad.TryGet(kvp.Key);
            if (wadObject is null)
                continue;

            LuaPropertyContainer? container = GetContainer(wadObject);
            if (container is not null)
            {
                container.Clear();
                foreach (var prop in kvp.Value.GetAll())
                    container.SetValue(prop.Key, prop.Value);
            }
        }

        DialogResult = false;
    }

    [RelayCommand]
    private void ResetAll() => PropertyGrid.ResetAll();
}
