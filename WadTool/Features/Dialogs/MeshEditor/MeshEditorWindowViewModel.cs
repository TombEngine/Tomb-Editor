#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawing = System.Drawing;
using Media = System.Windows.Media;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib;
using TombLib.Graphics;
using TombLib.Input;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.WPF;
using TombLib.WPF.Controls.Rendering;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Input;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using WadTool.Controls;

namespace WadTool.Features.Dialogs.MeshEditor;

/// <summary>
/// Edits geometry, UV selections and materials through native WPF controls.
/// The Direct3D mesh viewport retains its rendering surface for picking and gizmos.
/// </summary>
public partial class MeshEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly IColorPickerService _colorPickerService;
    private readonly WadToolClass _tool;
    private readonly Wad2 _wad;
    private readonly DeviceManager _deviceManager;
    private readonly WadMesh? _singleMesh;
    private readonly IWadObjectId? _focusObjectId;

    private PanelRenderingMesh? _panelMesh;
    private PanelTextureMap? _panelTextureMap;

    private bool _readingValues;
    private bool _unsavedChanges;
    private bool _initializing = true;
    private MeshEditorTreeNode? _currentNode;
    private int _searchIndex = -1;

    // User-loaded textures kept until the editor closes.
    private readonly List<WadTexture> _userTextures = new();

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty] private bool _showEditingTools = true;
    [ObservableProperty] private bool _treeMode;
    [ObservableProperty] private bool _showTextureTools;
    [ObservableProperty] private bool _showAllTexturesButton = true;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private string _title = "Mesh editor";
    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private int _editingModeIndex;
    [ObservableProperty] private bool _showExtra;
    [ObservableProperty] private string _extraLabel = "Show all";
    [ObservableProperty] private bool _previewActive;

    // Toolbar toggles
    [ObservableProperty] private bool _wireframe;
    [ObservableProperty] private bool _alphaTest;
    [ObservableProperty] private bool _bilinear;
    [ObservableProperty] private bool _drawAxis;
    [ObservableProperty] private bool _hidden;
    [ObservableProperty] private bool _undoEnabled;
    [ObservableProperty] private bool _redoEnabled;
    [ObservableProperty] private bool _editingEnabled;
    [ObservableProperty] private bool _meshIoEnabled;

    // Vertex remap
    [ObservableProperty] private double _vertexNum;
    [ObservableProperty] private bool _remapEnabled;

    // Vertex effects
    [ObservableProperty] private double _glow;
    [ObservableProperty] private double _move;

    // Vertex weights (4 slots)
    [ObservableProperty] private double _weightIndex1, _weightIndex2, _weightIndex3, _weightIndex4;
    [ObservableProperty] private double _weightValue1, _weightValue2, _weightValue3, _weightValue4;

    // Face attributes
    [ObservableProperty] private bool _applyTexture = true;
    [ObservableProperty] private bool _applyBlend;
    [ObservableProperty] private bool _applySheen;
    [ObservableProperty] private double _shineStrength;
    [ObservableProperty] private int _blendModeIndex;
    [ObservableProperty] private bool _doubleSided;

    // Sphere
    [ObservableProperty] private double _sphereX, _sphereY, _sphereZ, _sphereRadius;

    // Vertex color
    private Drawing.Color _vertexColor = Drawing.Color.White;
    [ObservableProperty] private Media.Brush _vertexColorBrush = Media.Brushes.White;

    // Texture list
    [ObservableProperty] private bool _allTextures;
    [ObservableProperty] private Texture? _selectedTexture;
    [ObservableProperty] private bool _materialEditorEnabled;

    public ObservableCollection<string> BlendModes { get; } = new();
    public ObservableCollection<Texture> Textures { get; } = new();
    public ObservableCollection<MeshEditorTreeNode> RootNodes { get; } = new();

    /// <summary>Set on close (legacy public <c>SelectedMesh</c>) ? read by single-mesh callers.</summary>
    public WadMesh? SelectedMesh { get; set; }

    public PanelRenderingMesh? Panel => _panelMesh;

    /// <summary>Raised for editor MessageEvents so the window can show a popup anchored to the 3D view.</summary>
    public event Action<string, PopupType>? MessageRaised;
    /// <summary>Raised when the tree should scroll the selected node into view.</summary>
    public event Action? EnsureSelectionVisible;

    public MeshEditorWindowViewModel(WadToolClass tool, DeviceManager deviceManager, Wad2 wad,
        WadMesh? mesh = null, IWadObjectId? focusObjectId = null, IColorPickerService? colorPickerService = null)
    {
        _colorPickerService = colorPickerService ?? new ColorPickerService();
        _tool = tool;
        _wad = wad;
        _deviceManager = deviceManager;
        _singleMesh = mesh;
        _focusObjectId = focusObjectId;

        foreach (var name in TextureExtensions.BlendModeUserNames(_tool.DestinationWad.GameVersion))
            BlendModes.Add(name);
    }

    /// <summary>Called by the window once both viewports are ready; mirrors the legacy ctor + OnShown.</summary>
    public void AttachPanels(PanelRenderingMesh panelMesh, PanelTextureMap panelTextureMap)
    {
        _panelMesh = panelMesh;
        _panelTextureMap = panelTextureMap;

        _tool.EditorEventRaised += OnEditorEventRaised;
        panelMesh.InitializeRendering(_tool, _deviceManager);
        panelTextureMap.Initialize(_tool);

        ReadConfig();
        PrepareUI(_singleMesh);

        if (_focusObjectId is not null)
        {
            ShowTextureTools = _wad.GameVersion == TombLib.LevelData.TRVersion.Game.TombEngine;
            SelectFocusObject(_focusObjectId);
        }

        _initializing = false;
        UpdateUI();
        RepopulateTextureList();
    }

    public void Detach() => _tool.EditorEventRaised -= OnEditorEventRaised;

    /// <summary>Called by the window when the texture map's selection changed (legacy SelectedTextureChanged).</summary>
    public void NotifyTextureSelectionChanged() => UpdateStatusLabel();

    /// <summary>Esc handling (legacy ProcessCmdKey): clear the picked element and texture selection.</summary>
    public void ClearSelection()
    {
        if (_panelMesh is not null) _panelMesh.CurrentElement = -1;
        if (_panelTextureMap is not null) _panelTextureMap.SelectedTexture = TextureArea.None;
    }

    private EditorKey ModifierKeys => KeyboardState.ModifierKeys;
    private bool NoMesh() => _panelMesh?.Mesh is null || _panelMesh.Mesh.VertexPositions.Count == 0;

    private void ReadConfig()
    {
        AlphaTest = _tool.Configuration.MeshEditor_AlphaTest;
        DrawAxis = _tool.Configuration.MeshEditor_DrawGrid;
        Bilinear = _tool.Configuration.MeshEditor_Bilinear;
        Wireframe = _tool.Configuration.MeshEditor_Wireframe;
    }

    // ---- Tree / mesh selection ----

    private void PrepareUI(WadMesh? mesh)
    {
        if (mesh is null)
        {
            TreeMode = true;
            BuildTree();

            // Tree mode always lists all textures.
            AllTextures = true;
            ShowAllTexturesButton = false;
        }
        else
        {
            TreeMode = false;
            if (_panelMesh is not null)
                _panelMesh.Mesh = mesh;
            GetSphereValues();
        }
    }

    private void BuildTree()
    {
        RootNodes.Clear();

        var moveablesNode = new MeshEditorTreeNode("Moveables") { IsExpanded = true };
        foreach (var moveable in _wad.Moveables)
        {
            var moveableNode = new MeshEditorTreeNode(moveable.Key.ToString(_wad.GameVersion));
            for (int i = 0; i < moveable.Value.Meshes.Count(); i++)
            {
                var wadMesh = moveable.Value.Meshes.ElementAt(i);
                moveableNode.Children.Add(new MeshEditorTreeNode(wadMesh.Name, moveable.Key, i, wadMesh));
            }

            if (moveable.Value.Skin is not null)
                moveableNode.Children.Add(new MeshEditorTreeNode(moveable.Value.Skin.Name, moveable.Key,
                    moveable.Value.Meshes.Count(), moveable.Value.Skin, isSkin: true));

            moveablesNode.Children.Add(moveableNode);
        }
        RootNodes.Add(moveablesNode);

        var staticsNode = new MeshEditorTreeNode("Statics") { IsExpanded = true };
        foreach (var @static in _wad.Statics)
        {
            var staticNode = new MeshEditorTreeNode(@static.Key.ToString(_wad.GameVersion));
            staticNode.Children.Add(new MeshEditorTreeNode(@static.Value.Mesh.Name, @static.Key, 0, @static.Value.Mesh));
            staticsNode.Children.Add(staticNode);
        }
        RootNodes.Add(staticsNode);
    }

    private void SelectFocusObject(IWadObjectId obj)
    {
        bool isStatic = obj is WadStaticId;
        foreach (var leaf in EnumerateLeaves())
        {
            if (leaf.ObjectId is null || isStatic != (leaf.ObjectId is WadStaticId))
                continue;

            if ((isStatic && ((WadStaticId)obj).TypeId == ((WadStaticId)leaf.ObjectId).TypeId) ||
                (!isStatic && ((WadMoveableId)obj).TypeId == ((WadMoveableId)leaf.ObjectId).TypeId))
            {
                leaf.IsSelected = true;
                EnsureSelectionVisible?.Invoke();
                return;
            }
        }
    }

    private IEnumerable<MeshEditorTreeNode> EnumerateLeaves()
    {
        IEnumerable<MeshEditorTreeNode> Walk(MeshEditorTreeNode n)
        {
            if (n.IsMesh) yield return n;
            foreach (var c in n.Children)
                foreach (var d in Walk(c))
                    yield return d;
        }
        return RootNodes.SelectMany(Walk);
    }

    /// <summary>Called by the window when the tree selection changes.</summary>
    public void SelectTreeNode(MeshEditorTreeNode? node)
    {
        if (node is null || !node.IsMesh || _panelMesh is null)
            return;

        SaveCurrentMesh();

        if (node.WadMesh != _panelMesh.Mesh)
        {
            _tool.UndoManager.ClearAll();
            _panelMesh.Mesh = node.WadMesh;
            _currentNode = node;

            GetSphereValues();
            UpdateUI();
            RepopulateTextureList();
        }
    }

    private void SaveCurrentMesh()
    {
        if (!TreeMode || _currentNode is null || _panelMesh is null)
            return;

        var obj = _wad.TryGet(_currentNode.ObjectId);

        if (obj is WadMoveable mov)
        {
            if (mov.Meshes.Count == _currentNode.MeshIndex)
                mov.Skin = _panelMesh.Mesh;
            else
                mov.Meshes[_currentNode.MeshIndex] = mov.Bones[_currentNode.MeshIndex].Mesh = _panelMesh.Mesh;

            mov.Version = DataVersion.GetNext();
        }
        else if (obj is WadStatic stat)
        {
            stat.Mesh = _panelMesh.Mesh;
            stat.Version = DataVersion.GetNext();
        }
    }

    // ---- Editor events (live editing) ----

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is WadToolClass.MeshEditorElementChangedEvent elementChanged)
            HandleElementChanged(elementChanged.ElementIndex);

        if (obj is WadToolClass.UndoStackChangedEvent stackEvent)
        {
            UndoEnabled = stackEvent.UndoPossible;
            RedoEnabled = stackEvent.RedoPossible;
            _unsavedChanges = true;
            SaveCurrentMesh();
            UpdateUI();
            UpdateMeshTreeName();
        }

        if (obj is WadToolClass.MessageEvent message)
            MessageRaised?.Invoke(message.Message, message.Type);
    }

    private void HandleElementChanged(int newIndex)
    {
        if (_panelMesh is null)
            return;

        switch (_panelMesh.EditingMode)
        {
            case MeshEditingMode.VertexRemap:
                if (newIndex == -1) return;
                UpdateUI();
                break;

            case MeshEditingMode.VertexEffects:
                if (newIndex == -1) return;
                if (!_panelMesh.Mesh.HasAttributes) GenerateMissingVertexData();
                if (ModifierKeys == EditorKey.Alt)
                {
                    _readingValues = true;
                    Glow = _panelMesh.Mesh.VertexAttributes[newIndex].Glow;
                    Move = _panelMesh.Mesh.VertexAttributes[newIndex].Move;
                    _readingValues = false;
                }
                else
                {
                    _panelMesh.Mesh.VertexAttributes[newIndex] = new VertexAttributes { Glow = (int)Glow, Move = (int)Move };
                    _panelMesh.Invalidate();
                }
                break;

            case MeshEditingMode.VertexWeights:
                if (newIndex == -1) return;
                if (!_panelMesh.Mesh.HasWeights) GenerateMissingVertexData();
                if (ModifierKeys == EditorKey.Alt)
                    GetWeightValues(newIndex);
                else
                {
                    SetWeightValues(newIndex);
                    _panelMesh.Invalidate();
                }
                break;

            case MeshEditingMode.VertexColorsAndNormals:
                if (newIndex == -1) return;
                if (!_panelMesh.Mesh.HasColors || !_panelMesh.Mesh.HasNormals) GenerateMissingVertexData();
                if (ModifierKeys == EditorKey.Alt)
                    SetVertexColor(_panelMesh.Mesh.VertexColors[newIndex].ToDrawingColor());
                else
                {
                    _panelMesh.Mesh.VertexColors[newIndex] = _vertexColor.ToFloat3Color();
                    _panelMesh.Invalidate();
                }
                break;

            case MeshEditingMode.FaceAttributes:
                if (newIndex == -1) return;
                HandleFaceEdit(newIndex);
                break;

            case MeshEditingMode.Sphere:
                GetSphereValues();
                break;
        }

        if (_panelMesh.EditingMode != MeshEditingMode.VertexRemap &&
            _panelMesh.EditingMode != MeshEditingMode.Sphere)
            UpdateUI();
    }

    private void HandleFaceEdit(int newIndex)
    {
        if (_panelMesh is null || _panelTextureMap is null)
            return;

        var poly = _panelMesh.Mesh.Polys[newIndex];

        if (ModifierKeys == EditorKey.Alt) // Picking
        {
            if (ApplySheen)
                ShineStrength = poly.ShineStrength;

            if (ApplyBlend)
            {
                var bmIndex = poly.Texture.BlendMode.ToUserIndex();
                if (bmIndex < BlendModes.Count) BlendModeIndex = bmIndex;
                DoubleSided = poly.Texture.DoubleSided;
            }

            if (ApplyTexture)
                SelectTexture(poly.Texture);

            return;
        }

        var currTexture = poly.Texture;

        if (ApplyTexture)
        {
            if (ModifierKeys == EditorKey.None)
            {
                if ((_panelTextureMap.VisibleTexture?.IsAvailable ?? false) && _panelTextureMap.SelectedTexture != TextureArea.None)
                {
                    currTexture = _panelTextureMap.SelectedTexture;
                    if (poly.IsTriangle)
                        currTexture.TexCoord3 = currTexture.TexCoord2;
                }
            }
            else if (ModifierKeys == EditorKey.Control)
                currTexture.Mirror(poly.IsTriangle);
            else if (ModifierKeys == EditorKey.Shift)
                poly.Rotate(1, poly.IsTriangle);
        }

        if (ApplySheen)
            poly.ShineStrength = (byte)ShineStrength;

        if (ApplyBlend)
        {
            currTexture.BlendMode = TextureExtensions.ToBlendMode(BlendModeIndex);
            currTexture.DoubleSided = DoubleSided;
        }

        poly.Texture = currTexture;
        _panelMesh.Mesh.Polys[newIndex] = poly;
        _panelMesh.Invalidate();
    }

    // ---- UI sync ----

    private void UpdateUI()
    {
        if (_panelMesh is null)
            return;

        EditingEnabled = _panelMesh.Mesh is not null;

        RemapEnabled = _panelMesh.EditingMode == MeshEditingMode.VertexRemap && _panelMesh.CurrentElement != -1;
        if (RemapEnabled)
        {
            _readingValues = true;
            VertexNum = _panelMesh.CurrentElement;
            _readingValues = false;
        }

        Wireframe = _panelMesh.WireframeMode;
        AlphaTest = _panelMesh.AlphaTest;
        ShowExtra = _panelMesh.DrawExtraInfo;

        Hidden = _panelMesh.Mesh?.Hidden ?? false;

        if (!ShowEditingTools)
            _panelMesh.EditingMode = MeshEditingMode.None;

        ExtraLabel = _panelMesh.EditingMode switch
        {
            MeshEditingMode.FaceAttributes => "Show sheen",
            MeshEditingMode.VertexColorsAndNormals => "Show all normals",
            MeshEditingMode.VertexEffects => "Show all values",
            MeshEditingMode.VertexRemap => "Show all numbers",
            MeshEditingMode.VertexWeights => "Show all weights",
            MeshEditingMode.Sphere => "Show gizmo",
            _ => "Show all"
        };

        if (BlendModeIndex == -1)
            BlendModeIndex = 0;

        MeshIoEnabled = !NoMesh();

        UpdateStatusLabel();
        _panelMesh.Invalidate();
    }

    private void UpdateStatusLabel()
    {
        if (_panelMesh is null)
            return;

        var prompt = string.Empty;
        if (!NoMesh())
        {
            prompt += _panelMesh.Mesh.VertexPositions.Count + " vertices, " +
                      _panelMesh.Mesh.Polys.Count + " face" + (_panelMesh.Mesh.Polys.Count > 1 ? "s" : "");

            if (_panelMesh.Mesh.Polys.Count < 1024)
            {
                int textureCount = _panelMesh.Mesh.TextureAreas.Count;
                prompt += ", " + textureCount + " texture info" + (textureCount > 1 ? "s" : "");
            }
            prompt += ". ";
        }

        if (_panelTextureMap is not null && _panelTextureMap.SelectedTexture != TextureArea.None)
        {
            var quad = _panelTextureMap.SelectedTexture.GetRect();
            prompt += "Selected texture: " + quad.Start + " to " + quad.End;
        }

        StatusText = prompt;
        Title = "Mesh editor" + (NoMesh() ? "" : " - " + _panelMesh.Mesh.Name);
        if (!TreeMode && _unsavedChanges)
            Title += " *";
    }

    private void UpdateMeshTreeName()
    {
        if (!TreeMode || _panelMesh?.Mesh is null)
            return;

        foreach (var leaf in EnumerateLeaves())
            if (leaf.WadMesh == _panelMesh.Mesh)
                leaf.Text = _panelMesh.Mesh.Name;
    }

    private void GetSphereValues()
    {
        if (NoMesh()) return;
        _readingValues = true;
        SphereX = _panelMesh!.Mesh.BoundingSphere.Center.X;
        SphereY = _panelMesh.Mesh.BoundingSphere.Center.Y;
        SphereZ = _panelMesh.Mesh.BoundingSphere.Center.Z;
        SphereRadius = Math.Abs(_panelMesh.Mesh.BoundingSphere.Radius);
        _readingValues = false;
    }

    private void GetWeightValues(int index)
    {
        if (NoMesh() || _panelMesh!.CurrentElement == -1) return;
        _readingValues = true;
        var w = _panelMesh.Mesh.VertexWeights[index];
        WeightIndex1 = w.Index[0]; WeightIndex2 = w.Index[1]; WeightIndex3 = w.Index[2]; WeightIndex4 = w.Index[3];
        WeightValue1 = w.Weight[0]; WeightValue2 = w.Weight[1]; WeightValue3 = w.Weight[2]; WeightValue4 = w.Weight[3];
        _readingValues = false;
    }

    private void SetWeightValues(int index)
    {
        var weight = new VertexWeight();
        weight.Index[0] = (int)WeightIndex1; weight.Index[1] = (int)WeightIndex2; weight.Index[2] = (int)WeightIndex3; weight.Index[3] = (int)WeightIndex4;
        weight.Weight[0] = (float)WeightValue1; weight.Weight[1] = (float)WeightValue2; weight.Weight[2] = (float)WeightValue3; weight.Weight[3] = (float)WeightValue4;
        _panelMesh!.Mesh.VertexWeights[index] = weight;
    }

    private void GenerateMissingVertexData()
    {
        if (_panelMesh!.Mesh.GenerateMissingVertexData())
            MessageRaised?.Invoke("Missing vertex data was automatically generated for this mesh.", PopupType.Info);
    }

    private void SetVertexColor(Drawing.Color color)
    {
        _vertexColor = color;
        VertexColorBrush = new Media.SolidColorBrush(Media.Color.FromRgb(color.R, color.G, color.B));
    }

    private void SelectTexture(TextureArea tex)
    {
        if (_panelTextureMap is null) return;
        _panelTextureMap.ShowTexture(tex);
        if (tex.Texture.IsAvailable && Textures.Contains(tex.Texture))
            SelectedTexture = tex.Texture;
    }

    // ---- Property change reactions ----

    // Combo index ? MeshEditingMode: 0 FaceAttributes, 1 VertexRemap, 2 VertexColorsAndNormals,
    // 3 VertexEffects, 4 VertexWeights, 5 Sphere.
    public bool IsFaceMode => EditingModeIndex == 0;
    public bool IsRemapMode => EditingModeIndex == 1;
    public bool IsColorsMode => EditingModeIndex == 2;
    public bool IsEffectsMode => EditingModeIndex == 3;
    public bool IsWeightsMode => EditingModeIndex == 4;
    public bool IsSphereMode => EditingModeIndex == 5;

    partial void OnEditingModeIndexChanged(int value)
    {
        if (_panelMesh is null) return;
        _panelMesh.EditingMode = (MeshEditingMode)(value + 1);
        PreviewActive = false;
        OnPropertyChanged(nameof(IsFaceMode));
        OnPropertyChanged(nameof(IsRemapMode));
        OnPropertyChanged(nameof(IsColorsMode));
        OnPropertyChanged(nameof(IsEffectsMode));
        OnPropertyChanged(nameof(IsWeightsMode));
        OnPropertyChanged(nameof(IsSphereMode));
        UpdateUI();
    }

    partial void OnHiddenChanged(bool value)
    {
        if (_initializing || _panelMesh?.Mesh is null || _panelMesh.Mesh.Hidden == value) return;
        _panelMesh.Mesh.Hidden = value;
        UpdateUI();
    }

    partial void OnPreviewActiveChanged(bool value)
    {
        if (_panelMesh is null) return;
        if (value) _panelMesh.StartPreview();
        else _panelMesh.StopPreview();
    }

    partial void OnShowExtraChanged(bool value)
    {
        if (_panelMesh is null) return;
        _panelMesh.DrawExtraInfo = value;
        if (!_initializing) UpdateUI();
    }

    partial void OnSelectedTextureChanged(Texture? value)
    {
        if (_panelTextureMap is null) return;
        _panelTextureMap.ResetVisibleTexture(value, true);
        MaterialEditorEnabled = value is not null && !string.IsNullOrEmpty(value.AbsolutePath);
    }

    partial void OnAllTexturesChanged(bool value) => RepopulateTextureList();

    // ---- Texture list ----

    private bool CheckTextureExistence()
    {
        if (_panelMesh?.Mesh is null) return true;
        var textures = new HashSet<WadTexture>(_panelMesh.Mesh.TextureAreas.Select(t => t.Texture as WadTexture).Distinct());
        return _tool.DestinationWad.MeshTexturesUnique.SetEquals(textures);
    }

    private void RepopulateTextureListIfChanged()
    {
        if (!CheckTextureExistence())
            RepopulateTextureList();
    }

    private void RepopulateTextureList(bool force = false)
    {
        if (_panelMesh is null || _panelTextureMap is null)
            return;

        bool wholeWad = AllTextures;
        if (NoMesh() && !wholeWad)
        {
            Textures.Clear();
            return;
        }

        var list = new List<Texture>(_userTextures);

        if (!NoMesh())
            foreach (var poly in _panelMesh.Mesh.Polys)
                if (poly.Texture.Texture.IsAvailable && !list.Contains(poly.Texture.Texture))
                    list.Add(poly.Texture.Texture);

        if (wholeWad)
        {
            foreach (var mesh in _tool.DestinationWad.MeshesUnique)
                foreach (var poly in mesh.Polys)
                    if (poly.Texture.Texture.IsAvailable && !list.Contains(poly.Texture.Texture))
                        list.Add(poly.Texture.Texture);

            foreach (var set in _tool.DestinationWad.AnimatedTextureSets)
                foreach (var frame in set.Frames)
                    if (frame.Texture.IsAvailable && !list.Contains(frame.Texture))
                        list.Add(frame.Texture);
        }

        if (wholeWad && !force && Textures.Count == list.Count)
            return;

        Textures.Clear();
        foreach (var t in list)
            Textures.Add(t);

        if (list.Count == 0)
            SelectedTexture = null;
        else if (_panelTextureMap.SelectedTexture == TextureArea.None || !list.Contains(_panelTextureMap.SelectedTexture.Texture))
        {
            _panelTextureMap.SelectedTexture = TextureArea.None;
            SelectedTexture = list[0];
        }
        else
            SelectedTexture = list[list.IndexOf(_panelTextureMap.SelectedTexture.Texture)];
    }

    private bool CheckTextureSize(ImageC image)
    {
        if (image.Width > 2048 || image.Height > 2048)
        {
            UserDialogs.Show(Owner, Path.GetFileName(image.FileName) + " is oversized. UV precision loss may occur.\nResize texture up to 2048px and repeat.",
                "Texture is oversized", DialogIcon.Error);
            return false;
        }
        return true;
    }

    // ---- Vertex remap / autofit ----

    [RelayCommand]
    private void RemapVertex()
    {
        if (NoMesh() || _panelMesh!.CurrentElement == -1)
            return;

        if ((int)VertexNum == _panelMesh.CurrentElement)
        {
            MessageRaised?.Invoke("Please specify other vertex number.", PopupType.Error);
            return;
        }

        var newVertexIndex = (int)VertexNum;
        if (newVertexIndex >= _panelMesh.Mesh.VertexPositions.Count)
        {
            MessageRaised?.Invoke("Please specify index between 0 and " + (_panelMesh.Mesh.VertexPositions.Count - 1) + ".", PopupType.Error);
            VertexNum = _panelMesh.CurrentElement;
            return;
        }

        _tool.UndoManager.PushMeshChanged(_panelMesh);
        var count = RemapVertex(_panelMesh.CurrentElement, newVertexIndex);

        if (count > 0)
        {
            _tool.ToggleUnsavedChanges();
            var message = "Successfully replaced vertex " + _panelMesh.CurrentElement + " with " + newVertexIndex + " in " + count + " faces.";
            if (newVertexIndex > _panelMesh.SafeVertexRemapLimit)
                MessageRaised?.Invoke(message + "\nSpecified vertex number is out of recommended bounds. Glitches may happen in game.", PopupType.Warning);
            else
                MessageRaised?.Invoke(message, PopupType.Info);

            _panelMesh.CurrentElement = newVertexIndex;
        }
    }

    private int RemapVertex(int oldIndex, int newIndex)
    {
        var mesh = _panelMesh!.Mesh;
        if (oldIndex >= mesh.VertexPositions.Count || newIndex >= mesh.VertexPositions.Count)
            return 0;

        var count = 0;
        (mesh.VertexPositions[newIndex], mesh.VertexPositions[oldIndex]) = (mesh.VertexPositions[oldIndex], mesh.VertexPositions[newIndex]);

        for (int j = 0; j < mesh.Polys.Count; j++)
        {
            var done = false;
            var poly = mesh.Polys[j];

            if (poly.Index0 == oldIndex) { poly.Index0 = newIndex; done = true; } else if (poly.Index0 == newIndex) { poly.Index0 = oldIndex; done = true; }
            if (poly.Index1 == oldIndex) { poly.Index1 = newIndex; done = true; } else if (poly.Index1 == newIndex) { poly.Index1 = oldIndex; done = true; }
            if (poly.Index2 == oldIndex) { poly.Index2 = newIndex; done = true; } else if (poly.Index2 == newIndex) { poly.Index2 = oldIndex; done = true; }

            if (poly.Shape == WadPolygonShape.Quad)
                if (poly.Index3 == oldIndex) { poly.Index3 = newIndex; done = true; } else if (poly.Index3 == newIndex) { poly.Index3 = oldIndex; done = true; }

            if (done) { mesh.Polys[j] = poly; count++; }
        }
        return count;
    }

    [RelayCommand]
    private void FindVertex()
    {
        if (NoMesh()) return;
        var newVertexIndex = (int)VertexNum;
        if (newVertexIndex >= _panelMesh!.Mesh.VertexPositions.Count)
        {
            MessageRaised?.Invoke("Please specify index between 0 and " + (_panelMesh.Mesh.VertexPositions.Count - 1) + ".", PopupType.Error);
            return;
        }
        _panelMesh.CurrentElement = newVertexIndex;
    }

    [RelayCommand]
    private void AutoFit()
    {
        if (NoMesh()) return;

        if (_panelMesh!.Mesh.VertexPositions.Count < _panelMesh.SafeVertexRemapLimit)
        {
            MessageRaised?.Invoke("Vertex count is lower than remap limit. No auto-fitting is needed.", PopupType.Info);
            return;
        }

        int count = AutoFitInternal();
        if (count == 0)
            MessageRaised?.Invoke("No vertices were auto-fitted. Possibly mesh is already remapped or contains no holes.", PopupType.Warning);
        else
            MessageRaised?.Invoke("Auto-fitted " + count + " vertices.", PopupType.Info);

        _panelMesh.Invalidate();
    }

    private int AutoFitInternal()
    {
        var mesh = _panelMesh!.Mesh;
        _tool.UndoManager.PushMeshChanged(_panelMesh);

        var edges = new List<FaceEdge>();
        foreach (var poly in mesh.Polys)
        {
            edges.Add(new FaceEdge(poly.Index0, poly.Index1));
            edges.Add(new FaceEdge(poly.Index1, poly.Index2));
            if (poly.IsTriangle)
                edges.Add(new FaceEdge(poly.Index2, poly.Index0));
            else
            {
                edges.Add(new FaceEdge(poly.Index2, poly.Index3));
                edges.Add(new FaceEdge(poly.Index3, poly.Index0));
            }
        }

        var orphans = edges.GroupBy(x => x).Where(g => g.Count() == 1).Select(y => y.Key).ToList();
        var remapped = new List<int>();
        var count = 0;

        foreach (var orphan in orphans)
            foreach (var point in orphan.P)
                if (!remapped.Contains(point) && point > _panelMesh.SafeVertexRemapLimit)
                {
                    while (true)
                    {
                        if (orphans.Any(o => o.P[0] == count || o.P[1] == count)) count++;
                        else break;
                        if (count > _panelMesh.SafeVertexRemapLimit || count == mesh.VertexPositions.Count - 1)
                            return count;
                    }
                    RemapVertex(point, count);
                    remapped.Add(point);
                    count++;
                }

        return count;
    }

    private sealed class FaceEdge : IEquatable<FaceEdge>
    {
        public int[] P { get; }
        public FaceEdge(int a, int b) => P = new[] { a, b };
        public bool Equals(FaceEdge? other) => other is not null && ((other.P[0] == P[0] && other.P[1] == P[1]) || (other.P[1] == P[0] && other.P[0] == P[1]));
        public override bool Equals(object? obj) => Equals(obj as FaceEdge);
        public override int GetHashCode() => P[0].GetHashCode() ^ P[1].GetHashCode();
    }

    // ---- Vertex effects / weights / colors ----

    [RelayCommand]
    private void ApplyEffectsToAll()
    {
        if (NoMesh() || _panelMesh!.EditingMode != MeshEditingMode.VertexEffects) return;
        _tool.UndoManager.PushMeshChanged(_panelMesh);
        GenerateMissingVertexData();
        for (int i = 0; i < _panelMesh.Mesh.VertexPositions.Count; i++)
            _panelMesh.Mesh.VertexAttributes[i] = new VertexAttributes { Glow = (int)Glow, Move = (int)Move };
        _panelMesh.Invalidate();
    }

    [RelayCommand]
    private void ApplyWeightsToAll()
    {
        if (NoMesh() || _panelMesh!.EditingMode != MeshEditingMode.VertexWeights) return;
        _tool.UndoManager.PushMeshChanged(_panelMesh);
        GenerateMissingVertexData();
        for (int i = 0; i < _panelMesh.Mesh.VertexWeights.Count; i++)
        {
            var w = _panelMesh.Mesh.VertexWeights[i];
            w.Index[0] = (int)WeightIndex1; w.Index[1] = (int)WeightIndex2; w.Index[2] = (int)WeightIndex3; w.Index[3] = (int)WeightIndex4;
            w.Weight[0] = (float)WeightValue1; w.Weight[1] = (float)WeightValue2; w.Weight[2] = (float)WeightValue3; w.Weight[3] = (float)WeightValue4;
        }
        _panelMesh.ColorizeVertexWeights();
        _panelMesh.Invalidate();
    }

    [RelayCommand]
    private void RecalcNormals()
    {
        if (NoMesh() || _panelMesh!.EditingMode != MeshEditingMode.VertexColorsAndNormals) return;
        _tool.UndoManager.PushMeshChanged(_panelMesh);
        _panelMesh.Mesh.CalculateNormals();
        _panelMesh.Invalidate();
    }

    [RelayCommand]
    private void RecalcNormalsAvg()
    {
        if (NoMesh() || _panelMesh!.EditingMode != MeshEditingMode.VertexColorsAndNormals) return;
        _tool.UndoManager.PushMeshChanged(_panelMesh);
        _panelMesh.Mesh.CalculateNormals(false);
        _panelMesh.Invalidate();
    }

    [RelayCommand]
    private void ApplyShadesToAll()
    {
        if (NoMesh() || _panelMesh!.EditingMode != MeshEditingMode.VertexColorsAndNormals) return;
        _tool.UndoManager.PushMeshChanged(_panelMesh);
        GenerateMissingVertexData();
        var color = _vertexColor.ToFloat3Color();
        for (int i = 0; i < _panelMesh.Mesh.VertexPositions.Count; i++)
            _panelMesh.Mesh.VertexColors[i] = color;
        _panelMesh.Invalidate();
    }

    [RelayCommand]
    private void ConvertFromShades()
    {
        if (NoMesh()) return;
        _tool.UndoManager.PushMeshChanged(_panelMesh);
        var mesh = _panelMesh!.Mesh;
        mesh.VertexAttributes.Clear();

        if (!mesh.HasColors)
            mesh.VertexAttributes = Enumerable.Repeat(new VertexAttributes(), mesh.VertexPositions.Count).ToList();
        else
        {
            for (int i = 0; i < mesh.VertexColors.Count; i++)
            {
                var attr = new VertexAttributes();
                var luma = mesh.VertexColors[i].GetLuma();
                if (luma < 0.5f) attr.Move = (int)(luma * 2.0f * 63.0f);
                else if (luma < 1.0f) attr.Glow = (int)((luma - 0.5f) * 63.0f);
                mesh.VertexAttributes.Add(attr);
            }
            mesh.VertexColors.Clear();
        }
        _panelMesh.Invalidate();
    }

    [RelayCommand]
    private void PickColor()
    {
        var selected = _colorPickerService.PickColor(_vertexColor.ToFloat3Color(), color => SetVertexColor(color.ToDrawingColor()));
        if (selected is { } color)
            SetVertexColor(color.ToDrawingColor());
    }

    // ---- Face attributes ----

    [RelayCommand]
    private void ApplyToAllFaces()
    {
        if (NoMesh() || _panelMesh!.EditingMode != MeshEditingMode.FaceAttributes) return;
        _tool.UndoManager.PushMeshChanged(_panelMesh);

        var shiny = (byte)ShineStrength;
        var blend = TextureExtensions.ToBlendMode(BlendModeIndex);

        for (int i = 0; i < _panelMesh.Mesh.Polys.Count; i++)
        {
            var poly = _panelMesh.Mesh.Polys[i];
            if (ApplyTexture && _panelTextureMap!.SelectedTexture != TextureArea.None)
                poly.Texture = _panelTextureMap.SelectedTexture;

            var texture = poly.Texture;
            if (ApplySheen) poly.ShineStrength = shiny;
            if (ApplyBlend) { texture.BlendMode = blend; texture.DoubleSided = DoubleSided; }

            poly.Texture = texture;
            _panelMesh.Mesh.Polys[i] = poly;
        }

        UpdateStatusLabel();
        _panelMesh.Invalidate();
    }

    [RelayCommand]
    private void ToggleDoubleSide() => DoubleSided = !DoubleSided;

    // ---- Sphere ----

    partial void OnSphereXChanged(double value) => ApplySphere();
    partial void OnSphereYChanged(double value) => ApplySphere();
    partial void OnSphereZChanged(double value) => ApplySphere();
    partial void OnSphereRadiusChanged(double value) => ApplySphere();

    private void ApplySphere()
    {
        if (_readingValues || _panelMesh is null || NoMesh()) return;
        _tool.UndoManager.PushMeshChanged(_panelMesh);
        _panelMesh.Mesh.BoundingSphere = new BoundingSphere(new Vector3((float)SphereX, (float)SphereY, (float)SphereZ), (float)SphereRadius);
        _panelMesh.Invalidate();
    }

    [RelayCommand]
    private void ResetSphere()
    {
        if (NoMesh()) return;
        _tool.UndoManager.PushMeshChanged(_panelMesh);
        _panelMesh!.Mesh.BoundingSphere = _panelMesh.Mesh.CalculateBoundingSphere();
        _panelMesh.Invalidate();
        GetSphereValues();
    }

    // ---- Toolbar ----

    [RelayCommand]
    private void Undo() { _tool.UndoManager.Undo(); RepopulateTextureListIfChanged(); }

    [RelayCommand]
    private void Redo() { _tool.UndoManager.Redo(); RepopulateTextureListIfChanged(); }

    partial void OnWireframeChanged(bool value)
    {
        if (_initializing || _panelMesh is null) return;
        _tool.Configuration.MeshEditor_Wireframe = _panelMesh.WireframeMode = value;
        UpdateUI();
    }

    partial void OnAlphaTestChanged(bool value)
    {
        if (_initializing || _panelMesh is null) return;
        _tool.Configuration.MeshEditor_AlphaTest = _panelMesh.AlphaTest = value;
        UpdateUI();
    }

    partial void OnBilinearChanged(bool value)
    {
        if (_initializing || _panelMesh is null) return;
        _tool.Configuration.MeshEditor_Bilinear = _panelMesh.Bilinear = value;
        UpdateUI();
    }

    partial void OnDrawAxisChanged(bool value)
    {
        if (_initializing || _panelMesh is null) return;
        _tool.Configuration.MeshEditor_DrawGrid = _panelMesh.DrawGrid = value;
        UpdateUI();
    }

    [RelayCommand]
    private void ResetCamera() => _panelMesh?.ResetCamera();

    [RelayCommand]
    private void RotateTexture()
    {
        if (_panelTextureMap?.VisibleTexture is null || _panelTextureMap.VisibleTexture.IsUnavailable) return;
        var tr = _panelTextureMap.SelectedTexture;
        tr.Rotate(1);
        _panelTextureMap.SelectedTexture = tr;
    }

    [RelayCommand]
    private void MirrorTexture()
    {
        if (_panelTextureMap?.VisibleTexture is null || _panelTextureMap.VisibleTexture.IsUnavailable) return;
        var tm = _panelTextureMap.SelectedTexture;
        tm.Mirror();
        _panelTextureMap.SelectedTexture = tm;
    }

    [RelayCommand]
    private void ImportMesh()
    {
        var mesh = WadActions.ImportMesh(_tool, Owner);
        if (mesh is null || _panelMesh is null) return;
        _tool.UndoManager.PushMeshChanged(_panelMesh);
        _panelMesh.Mesh = mesh;
        SaveCurrentMesh();
        GetSphereValues();
        UpdateUI();
        RepopulateTextureList();
    }

    [RelayCommand]
    private void ExportMesh() => WadActions.ExportMesh(_panelMesh?.Mesh, _tool, Owner);

    [RelayCommand]
    private void ToggleHide()
    {
        if (_panelMesh?.Mesh is null) return;
        _panelMesh.Mesh.Hidden = !_panelMesh.Mesh.Hidden;
        UpdateUI();
    }

    [RelayCommand]
    private void Rename()
    {
        if (_panelMesh?.Mesh is null) return;
        string result = InputBox.Show(Owner, "Edit mesh name", "Mesh name:", _panelMesh.Mesh.Name);
        if (string.IsNullOrEmpty(result))
            return;
        if (_panelMesh.Mesh.Name.Equals(result, StringComparison.InvariantCultureIgnoreCase))
            return;

        _tool.UndoManager.PushMeshChanged(_panelMesh);
        _panelMesh.Mesh.Name = result;
        SaveCurrentMesh();
        UpdateUI();
        UpdateMeshTreeName();
    }

    [RelayCommand]
    private void Preview()
    {
        PreviewActive = !PreviewActive;
        if (PreviewActive) _panelMesh?.StartPreview();
        else _panelMesh?.StopPreview();
    }

    [RelayCommand]
    private void FindTexture()
    {
        if (!TreeMode || _panelMesh is null || _panelTextureMap is null)
            return;

        if (_panelTextureMap.SelectedTexture == TextureArea.None)
        {
            MessageRaised?.Invoke("Please select valid texture area", PopupType.Error);
            return;
        }

        var leaves = EnumerateLeaves().ToList();
        var selected = leaves.FirstOrDefault(n => n.IsSelected);
        int start = selected is null ? 0 : leaves.IndexOf(selected) + 1;
        if (start >= leaves.Count) start = 0;
        bool restarted = false;

        for (int i = start; i < leaves.Count; i++)
        {
            if (i == leaves.Count - 1 && start > 0 && !restarted) { i = 0; restarted = true; }

            var mesh = leaves[i].WadMesh!;
            var index = mesh.Polys.IndexOf(p => p.Texture.Texture == _panelTextureMap.VisibleTexture &&
                                                p.Texture.GetRect().Intersects(_panelTextureMap.SelectedTexture.GetRect()));
            if (index != -1)
            {
                leaves[i].IsSelected = true;
                EnsureSelectionVisible?.Invoke();
                _panelMesh.SelectElement(index, true);
                return;
            }
        }

        MessageRaised?.Invoke("No meshes with any textures from enclosed area were found.", PopupType.Info);
    }

    // ---- Texture commands ----

    [RelayCommand]
    private void AddEmbeddedTexture() => AddTexture(false);
    [RelayCommand]
    private void AddExternalTexture() => AddTexture(true);
    [RelayCommand]
    private void ReplaceEmbeddedTexture() => ReplaceTexture(false);
    [RelayCommand]
    private void ReplaceExternalTexture() => ReplaceTexture(true);

    private void AddTexture(bool isExternal)
    {
        var paths = LevelFileDialog.BrowseFiles(Owner, null, null, "Load texture file", ImageC.FileExtensions).ToList();
        if (paths.Count == 0) return;

        foreach (var path in paths)
        {
            var image = ImageC.FromFile(path);
            if (!CheckTextureSize(image)) continue;
            image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));

            var newTexture = new WadTexture(image);
            if (isExternal) newTexture.AbsolutePath = path;
            if (Textures.Contains(newTexture)) continue;

            Textures.Add(newTexture);
            SelectedTexture = newTexture;
            _userTextures.Add(newTexture);
        }
    }

    private void ReplaceTexture(bool isExternal)
    {
        if (_panelMesh is null || _panelTextureMap?.VisibleTexture is null || _panelTextureMap.VisibleTexture.IsUnavailable)
        {
            MessageRaised?.Invoke("Unable to replace texture.\nSelected texture is invalid.", PopupType.Error);
            return;
        }

        var path = LevelFileDialog.BrowseFile(Owner, null, null, "Load texture file", ImageC.FileExtensions, null, false);
        if (string.IsNullOrEmpty(path)) return;

        var image = ImageC.FromFile(path);
        if (!CheckTextureSize(image)) return;
        image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));
        if (!isExternal) image.FileName = string.Empty;

        var newTexture = new WadTexture(image) { AbsolutePath = image.FileName };
        var visible = _panelTextureMap.VisibleTexture;

        if (!TreeMode)
        {
            _tool.UndoManager.PushMeshChanged(_panelMesh);
            for (int i = 0; i < _panelMesh.Mesh.Polys.Count; i++)
                if (_panelMesh.Mesh.Polys[i].Texture.Texture == visible)
                {
                    var newPoly = _panelMesh.Mesh.Polys[i];
                    newPoly.Texture.Texture = newTexture;
                    _panelMesh.Mesh.Polys[i] = newPoly;
                }
        }
        else
        {
            bool used = false;
            foreach (var tex in _tool.DestinationWad.MeshTexInfosUnique)
                if (tex.Texture == visible)
                {
                    used = true;
                    for (int i = 0; i < 4; i++)
                        if (tex.TexCoords[i].X > image.Width || tex.TexCoords[i].Y > image.Height)
                        {
                            MessageRaised?.Invoke("New texture is smaller than existing one.\nPlease specify another texture.", PopupType.Error);
                            return;
                        }
                }

            if (used && UserDialogs.Show(Owner, "Replacing current texture will affect all meshes using it.\nThis action can't be undone. Continue?",
                "Confirm", DialogButtons.YesNo, DialogIcon.Question) != DialogChoice.Yes)
                return;

            foreach (var moveable in _tool.DestinationWad.Moveables.Values)
            {
                var meshList = new List<WadMesh>(moveable.Meshes);
                if (moveable.Skin is not null) meshList.Add(moveable.Skin);
                foreach (var mesh in meshList)
                    for (int i = 0; i < mesh.Polys.Count; i++)
                        if (mesh.Polys[i].Texture.Texture == visible)
                        {
                            var newPoly = mesh.Polys[i];
                            newPoly.Texture.Texture = newTexture;
                            mesh.Polys[i] = newPoly;
                        }
            }

            foreach (var stat in _tool.DestinationWad.Statics.Values)
                for (int i = 0; i < stat.Mesh.Polys.Count; i++)
                    if (stat.Mesh.Polys[i].Texture.Texture == visible)
                    {
                        var newPoly = stat.Mesh.Polys[i];
                        newPoly.Texture.Texture = newTexture;
                        stat.Mesh.Polys[i] = newPoly;
                    }
        }

        RepopulateTextureList(true);
        UpdateUI();

        if (_panelTextureMap.SelectedTexture == TextureArea.None) return;
        var newSelectedTexture = _panelTextureMap.SelectedTexture;
        newSelectedTexture.Texture = newTexture;
        SelectTexture(newSelectedTexture);
    }

    [RelayCommand]
    private void DeleteTexture()
    {
        if (_panelMesh is null || _panelTextureMap is null) return;

        if (_panelMesh.Mesh.Polys.Any(p => p.Texture.Texture == _panelTextureMap.VisibleTexture))
        {
            MessageRaised?.Invoke("Unable to remove selected texture because it's still used in mesh.", PopupType.Error);
            return;
        }
        if (_tool.DestinationWad.MeshTexturesUnique.Contains(_panelTextureMap.VisibleTexture as WadTexture))
        {
            MessageRaised?.Invoke("Unable to remove selected texture because it's still used in wad.", PopupType.Error);
            return;
        }

        if (_panelTextureMap.VisibleTexture is WadTexture wt && Textures.Contains(wt))
        {
            if (_userTextures.Contains(wt)) _userTextures.Remove(wt);
            Textures.Remove(wt);
        }

        if (Textures.Count > 0)
            SelectedTexture = Textures[0];
    }

    [RelayCommand]
    private void ExportTexture()
    {
        if (_panelTextureMap?.VisibleTexture is null || _panelTextureMap.VisibleTexture.IsUnavailable)
        {
            MessageRaised?.Invoke("Unable to save texture.\nSelected texture is invalid.", PopupType.Error);
            return;
        }

        var fileDialog = new SaveFileDialog
        {
            Filter = ImageC.SaveFileFileExtensions.GetFilter(true),
            Title = "Choose a texture file name",
            FileName = Path.GetFileNameWithoutExtension(_panelTextureMap.VisibleTexture.ToString()),
            AddExtension = true
        };
        if (fileDialog.ShowDialog(Owner) != true)
            return;

        try { _panelTextureMap.VisibleTexture.Image.SaveToFile(fileDialog.FileName); }
        catch (Exception exc) { MessageRaised?.Invoke("Unable to save texture. Exception: \n" + exc, PopupType.Error); }
    }

    [RelayCommand]
    private void MaterialEditor()
    {
        var texture = SelectedTexture as WadTexture;
        var list = Textures.Where(t => !string.IsNullOrEmpty(t.AbsolutePath));
        var result = MaterialEditorDialog.Show(list, _tool.Configuration, texture);
        if (result.MaterialChanged)
            MessageRaised?.Invoke("Material settings for current texture were saved to " + result.MaterialFileName + ".", PopupType.Info);
    }

    [RelayCommand]
    private void AnimationRanges()
    {
        var context = new WadToolAnimatedTexturesContext(_tool, _userTextures);
        var textureMap = new WpfAnimatedTextureMapView(_tool);
        var viewModel = new TombLib.WPF.Features.AnimatedTextures.AnimatedTexturesWindowViewModel(context, textureMap);
        var window = new TombLib.WPF.Features.AnimatedTextures.AnimatedTexturesWindow { DataContext = viewModel };
        window.SetOwner(Owner);
        window.ShowDialog();
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrEmpty(SearchText)) return;

        var leaves = EnumerateLeaves().ToList();
        var allNodes = new List<MeshEditorTreeNode>();
        void Collect(MeshEditorTreeNode n) { allNodes.Add(n); foreach (var c in n.Children) Collect(c); }
        foreach (var r in RootNodes) Collect(r);

        for (int i = _searchIndex + 1; i <= allNodes.Count; i++)
        {
            if (i == allNodes.Count)
            {
                if (_searchIndex == -1) break;
                i = -1; _searchIndex = -1; continue;
            }
            if (allNodes[i].Text.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1)
            {
                _searchIndex = i;
                break;
            }
        }

        if (_searchIndex != -1)
        {
            allNodes[_searchIndex].IsSelected = true;
            EnsureSelectionVisible?.Invoke();
        }
    }

    // ---- Close ----

    [RelayCommand]
    private void Ok() => DialogResult = true;

    [RelayCommand]
    private void Cancel() => DialogResult = false;

    /// <summary>Replicates the legacy OnClosing. Returns false to cancel the close.</summary>
    public bool HandleClosing()
    {
        bool ok = DialogResult == true;

        if (!TreeMode && !ok && _unsavedChanges)
        {
            var result = UserDialogs.Show(Owner, "You have unsaved changes. Do you want to save changes to current mesh?",
                "Confirm", DialogButtons.YesNoCancel, DialogIcon.Question);
            switch (result)
            {
                case DialogChoice.Yes: ok = true; break;
                case DialogChoice.No: ok = false; break;
                default: return false;
            }
        }

        // Make the resolved decision visible to callers that read DialogResult after ShowDialog.
        DialogResult = ok;

        if (TreeMode || ok)
        {
            SaveCurrentMesh();
            SelectedMesh = _panelMesh?.Mesh;
            _tool.ToggleUnsavedChanges();
            _tool.WadChanged(WadArea.Destination);
        }

        _tool.UndoManager.ClearAll();
        return true;
    }

    private static IWindowOwner Owner => WPFUtils.GetWindowOwner();
}
