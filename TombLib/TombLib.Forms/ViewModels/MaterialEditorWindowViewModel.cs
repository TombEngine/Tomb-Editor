#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using TombLib.Forms.Services;
using TombLib.LevelData;
using TombLib.LuaProperties;
using TombLib.Utils;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombLib.Forms.ViewModels;

/// <summary>Edits a material document using the shared typed WPF property grid.</summary>
public partial class MaterialEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string[] MapKeys = [nameof(MaterialData.ColorMap), nameof(MaterialData.NormalMap),
        nameof(MaterialData.HeightMap), nameof(MaterialData.SpecularMap), nameof(MaterialData.RoughnessMap),
        nameof(MaterialData.AmbientOcclusionMap), nameof(MaterialData.EmissiveMap), nameof(MaterialData.AlphaMaskMap), nameof(MaterialData.AdditionalColorMap)];

    private readonly IMaterialEditorStorage _storage;
    private readonly IMessageService _messages;
    private readonly ILocalizationService _localization;
    private MaterialData _document = new();
    private MaterialTextureEntry? _selectedTexture;
    private bool _loading;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _materialName = string.Empty;
    [ObservableProperty] private MaterialTypeDefinition? _selectedType;
    [ObservableProperty] private bool _isDirty;
    [ObservableProperty] private string? _materialFileName;
    [ObservableProperty] private bool _materialChanged;
    [ObservableProperty] private bool _canEdit;

    /// <summary>Gets the available source textures.</summary>
    public IReadOnlyList<MaterialTextureEntry> Textures { get; }

    /// <summary>Gets the available material definitions, including unknown document types.</summary>
    public ObservableCollection<MaterialTypeDefinition> Types { get; } = [];

    /// <summary>Gets the selected document's texture maps.</summary>
    public ObservableCollection<MaterialMapViewModel> Maps { get; } = [];

    /// <summary>Gets the shared typed property editor.</summary>
    public LuaPropertyGridViewModel PropertyGrid { get; } = new();

    /// <summary>Gets or changes the edited texture, asking about pending changes first.</summary>
    public MaterialTextureEntry? SelectedTexture
    {
        get => _selectedTexture;
        set
        {
            if (ReferenceEquals(value, _selectedTexture) || value is null)
                return;

            if (!TryLeaveTexture())
            {
                OnPropertyChanged();
                return;
            }

            SetProperty(ref _selectedTexture, value);
            LoadTexture(value);
        }
    }

    /// <summary>Creates an editor with injectable persistence, messages and localization.</summary>
    public MaterialEditorWindowViewModel(IEnumerable<Texture> textures, Texture? selectedTexture = null,
        IMaterialEditorStorage? storage = null, IMessageService? messages = null, ILocalizationService? localization = null)
    {
        _storage = storage ?? new MaterialEditorStorage();
        _messages = ServiceLocator.ResolveService(messages);
        _localization = ServiceLocator.ResolveService(localization).WithKeysFor(this);
        Textures = [.. textures.Select(texture => new MaterialTextureEntry(texture, texture.AbsolutePath ?? texture.Image.FileName ?? string.Empty))];
        foreach (var definition in MaterialCatalog.Definitions.OrderBy(definition => definition.Id))
            Types.Add(definition);

        PropertyGrid.PropertyValueChanged += (_, _) => MarkDirty();
        SelectedTexture = Textures.FirstOrDefault(entry => ReferenceEquals(entry.Texture, selectedTexture)) ?? Textures.FirstOrDefault();
    }

    partial void OnMaterialNameChanged(string value) => MarkDirty();

    partial void OnSelectedTypeChanged(MaterialTypeDefinition? value)
    {
        if (_loading || !CanEdit || value is null)
            return;

        SavePropertyValues();
        var previous = _document.GetMaterialDefinition();
        var values = (string[])_document.Properties.Clone();
        _document.Type = value.Id;
        _document.Normalize();
        for (int index = 0; index < MaterialData.PropertyCount; index++)
        {
            var property = value.Properties[index];
            _document.Properties[index] = property.IsDefined && previous.Properties[index].Type == property.Type
                ? values[index] : MaterialCatalog.GetDefaultValue(property);
        }

        LoadProperties();
        MarkDirty();
    }

    private void LoadTexture(MaterialTextureEntry entry)
    {
        _loading = true;
        CanEdit = false;
        try
        {
            _document = _storage.Load(entry.Texture, entry.Path);
            _document.Normalize();
            MaterialName = _document.Name;
            MaterialFileName = _document.XmlMaterialFileName;
            var type = Types.FirstOrDefault(definition => definition.Id == _document.Type);
            if (type is null)
            {
                type = _document.GetMaterialDefinition();
                Types.Add(type);
            }

            SelectedType = type;
            LoadMaps(entry.Path);
            LoadProperties();
            IsDirty = false;
            CanEdit = true;
        }
        catch (Exception exception)
        {
            Logger.Error(exception, "Could not load material for {0}.", entry.Path);
            _document = new MaterialData { ColorMap = entry.Path };
            _document.Normalize();
            MaterialName = _document.Name;
            MaterialFileName = null;
            SelectedType = Types.FirstOrDefault(definition => definition.Id == 0);
            LoadMaps(entry.Path);
            LoadProperties();
            IsDirty = false;
            _messages.ShowError(_localization.Format("LoadError", entry.Path, exception.Message), _localization["Title"]);
        }
        finally
        {
            _loading = false;
        }
    }

    private void LoadMaps(string texturePath)
    {
        foreach (var map in Maps)
            map.PropertyChanged -= OnMapChanged;

        Maps.Clear();
        foreach (var key in MapKeys)
        {
            var path = key == nameof(MaterialData.ColorMap) ? texturePath : GetMap(key);
            var map = new MaterialMapViewModel(key, path, key == nameof(MaterialData.ColorMap), texturePath);
            map.PropertyChanged += OnMapChanged;
            Maps.Add(map);
        }
    }

    private string GetMap(string key) => typeof(MaterialData).GetProperty(key)?.GetValue(_document) as string ?? string.Empty;

    private void OnMapChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(MaterialMapViewModel.Path))
            MarkDirty();
    }

    private void LoadProperties()
    {
        var definitions = new List<LuaPropertyDefinition>();
        var values = new LuaPropertyContainer();
        var type = _document.GetMaterialDefinition();
        for (int index = 0; index < MaterialData.PropertyCount; index++)
        {
            var property = type.Properties[index];
            if (!property.IsDefined)
                continue;

            var name = index.ToString(CultureInfo.InvariantCulture);
            definitions.Add(CreatePropertyDefinition(name, property));
            values[name] = _document.Properties[index];
        }

        PropertyGrid.Load(definitions, values);
    }

    private static LuaPropertyDefinition CreatePropertyDefinition(string name, MaterialPropertyDefinition property) => new()
    {
        InternalName = name,
        DisplayName = property.Name,
        Description = property.Description ?? string.Empty,
        DefaultValue = MaterialCatalog.GetDefaultValue(property),
        MinValue = Math.Min(property.MinValue ?? -1000000.0f, property.MaxValue ?? 1000000.0f),
        MaxValue = Math.Max(property.MinValue ?? -1000000.0f, property.MaxValue ?? 1000000.0f),
        HasAlpha = true,
        Type = property.Type switch
        {
            MaterialPropertyType.Bool => LuaPropertyType.Bool,
            MaterialPropertyType.Int => LuaPropertyType.Int,
            MaterialPropertyType.Float => LuaPropertyType.Float,
            MaterialPropertyType.Vec2 => LuaPropertyType.Vec2,
            MaterialPropertyType.Vec3 => LuaPropertyType.Vec3,
            MaterialPropertyType.Color => LuaPropertyType.Color,
            _ => LuaPropertyType.String
        }
    };

    private void SavePropertyValues()
    {
        foreach (var row in PropertyGrid.Properties)
        {
            int index = int.Parse(row.Definition.InternalName, CultureInfo.InvariantCulture);
            _document.Properties[index] = row.BoxedValue;
        }
    }

    private bool TryLeaveTexture()
    {
        if (!IsDirty)
            return true;

        var choice = _messages.ShowConfirmation(_localization["SaveChanges"], _localization["Title"],
            SaveChoice.Save, SaveChoice.Discard, SaveChoice.Cancel, SaveChoice.Save);
        return choice == SaveChoice.Discard || choice == SaveChoice.Save && TrySave();
    }

    private void MarkDirty()
    {
        if (!_loading && CanEdit)
            IsDirty = true;
    }

    private bool TrySave()
    {
        if (!IsDirty)
            return true;

        if (SelectedTexture is not { Path.Length: > 0 } texture)
            return false;

        SavePropertyValues();
        _document.Name = MaterialName;
        foreach (var map in Maps)
            typeof(MaterialData).GetProperty(map.Key)?.SetValue(_document, map.Path);

        try
        {
            MaterialFileName = _storage.Save(texture.Path, _document);
            MaterialChanged = true;
            IsDirty = false;
            return true;
        }
        catch (Exception exception)
        {
            Logger.Error(exception, "Could not save material for {0}.", texture.Path);
            _messages.ShowError(_localization.Format("SaveError", texture.Path, exception.Message), _localization["Title"]);
            return false;
        }
    }

    [RelayCommand]
    private void Confirm()
    {
        if (TrySave())
            DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;

    private enum SaveChoice
    {
        Save,
        Discard,
        Cancel
    }
}
