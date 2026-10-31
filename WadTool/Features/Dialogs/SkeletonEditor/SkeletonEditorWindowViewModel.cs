#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using TombLib.WPF.ViewModels;
using TombLib.WPF.Views;
using WadTool.Controls;
using WadTool.Features.Dialogs.MeshEditor;

namespace WadTool.Features.Dialogs.SkeletonEditor;

/// <summary>
/// WPF counterpart of the legacy <c>FormSkeletonEditor</c>. Edits a clone of a moveable's mesh tree
/// (bone hierarchy, per-bone pivot / lighting / mesh, optional skin) using the WPF
/// <see cref="PanelRenderingSkeleton"/> 3D view, committing back to the wad on OK.
/// </summary>
public partial class SkeletonEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly WadToolClass _tool;
    private readonly DeviceManager _deviceManager;
    private readonly Wad2 _wad;
    private readonly WadMoveable _moveable;

    private PanelRenderingSkeleton? _panel;
    private List<WadMeshBoneNode> _bones = new();
    private Dictionary<WadMeshBoneNode, BoneTreeNode> _nodesDictionary = new();
    private WadMeshBoneNode? _lastBone;
    private bool _initializing = true;

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty] private double _translationX;
    [ObservableProperty] private double _translationY;
    [ObservableProperty] private double _translationZ;
    [ObservableProperty] private int _lightTypeIndex;

    [ObservableProperty] private bool _drawGizmo = true;
    [ObservableProperty] private bool _drawGrid = true;

    [ObservableProperty] private bool _skinVisible;
    [ObservableProperty] private string _skinText = string.Empty;

    [ObservableProperty] private bool _popChecked;
    [ObservableProperty] private bool _pushChecked;

    public ObservableCollection<BoneTreeNode> RootNodes { get; } = new();

    public string Title { get; }

    /// <summary>Raised for editor MessageEvents so the window can show a popup anchored to the 3D view.</summary>
    public event System.Action<string, PopupType>? MessageRaised;

    public SkeletonEditorWindowViewModel(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMoveableId moveableId)
    {
        _tool = tool;
        _deviceManager = deviceManager;
        _wad = wad;
        _moveable = _wad.Moveables[moveableId].Clone();

        Title = "Skeleton editor - " + _moveable.Id.ToString(_wad.GameVersion);
    }

    /// <summary>Called by the window once the rendering panel is hosted; mirrors the legacy ctor body.</summary>
    public void AttachPanel(PanelRenderingSkeleton panel)
    {
        _panel = panel;
        panel.Configuration = _tool.Configuration;
        panel.InitializeRendering(_tool, _deviceManager);

        _tool.EditorEventRaised += OnEditorEventRaised;

        // Resolve the skin meshes (TEN dummy-mesh replacement), then clone the skeleton.
        WadMoveable skin;
        var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(_tool.DestinationWad.GameVersion, _moveable.Id.TypeId));
        if (_tool.DestinationWad.Moveables.ContainsKey(skinId))
            skin = _moveable.ReplaceDummyMeshes(_tool.DestinationWad.Moveables[skinId]);
        else
            skin = _tool.DestinationWad.Moveables[_moveable.Id];

        _bones = new List<WadMeshBoneNode>();
        for (int i = 0; i < _moveable.Bones.Count; i++)
        {
            var boneNode = new WadMeshBoneNode(skin.Meshes[i], _moveable.Bones[i]);
            boneNode.Bone.Translation = _moveable.Bones[i].Translation;
            boneNode.GlobalTransform = Matrix4x4.Identity;
            _bones.Add(boneNode);
        }

        RebuildTree();
        panel.Skeleton = _bones;

        if (_panel.SelectedNode is null && RootNodes.Count > 0)
        {
            _panel.SelectedNode = RootNodes[0].BoneNode;
            RootNodes[0].IsSelected = true;
        }

        _initializing = false;
        UpdateUI();
    }

    public void Detach() => _tool.EditorEventRaised -= OnEditorEventRaised;

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        switch (obj)
        {
            case WadToolClass.BoneOffsetMovedEvent:
                UpdateUI();
                if (RootNodes.Count > 0)
                    UpdateSkeletonMatrices(RootNodes[0], Matrix4x4.Identity);
                break;

            case WadToolClass.BonePickedEvent:
                UpdateUI();
                SyncTreeSelectionFromPanel();
                break;

            case WadToolClass.MessageEvent message:
                MessageRaised?.Invoke(message.Message, message.Type);
                break;
        }
    }

    // ---- Selection ----

    /// <summary>Called by the window when the tree selection changes.</summary>
    public void SelectTreeNode(BoneTreeNode? node)
    {
        if (_panel is null || node is null)
            return;

        _panel.SelectedNode = node.BoneNode;
        UpdateUI();
    }

    /// <summary>Selects the tree node matching the panel's current bone (after a 3D pick).</summary>
    public void SyncTreeSelectionFromPanel()
    {
        if (_panel?.SelectedNode is null)
            return;

        if (_nodesDictionary.TryGetValue(_panel.SelectedNode, out BoneTreeNode? node))
            node.IsSelected = true;
    }

    private WadMeshBoneNode? SelectedBone => _panel?.SelectedNode;

    // ---- Tree building (legacy LoadSkeleton) ----

    /// <summary>
    /// Tree label for a bone: list index, stack opcode, bone name and the mesh it drives.
    /// The mesh name matters because several bones often share a name but differ by mesh.
    /// </summary>
    private string GetBoneName(WadMeshBoneNode bone)
    {
        string op = bone.Bone?.OpCode switch
        {
            WadLinkOpcode.Pop => "POP ",
            WadLinkOpcode.Push => "PUSH ",
            WadLinkOpcode.Read => "READ ",
            _ => ""
        };

        string boneName = (bone.Bone?.Name ?? "<no bone>") + " (" + (bone.Mesh?.Name ?? "<no mesh>") + ")";
        return _bones.FindIndex(b => b == bone) + ": " + op + boneName;
    }

    private void RebuildTree()
    {
        RootNodes.Clear();
        _nodesDictionary = new Dictionary<WadMeshBoneNode, BoneTreeNode>();

        if (_bones.Count == 0)
            return;

        var rootNode = new BoneTreeNode(_bones[0], GetBoneName(_bones[0]));
        RootNodes.Add(rootNode);
        _nodesDictionary.Add(_bones[0], rootNode);

        var currentNode = rootNode;
        var stack = new Stack<BoneTreeNode>();

        for (int j = 1; j < _bones.Count; j++)
        {
            int linkX = (int)_bones[j].Bone.Translation.X;
            int linkY = (int)_bones[j].Bone.Translation.Y;
            int linkZ = (int)_bones[j].Bone.Translation.Z;

            var boneNode = _bones[j];
            var newNode = new BoneTreeNode(_bones[j], GetBoneName(_bones[j]));
            _nodesDictionary.Add(_bones[j], newNode);

            switch (_bones[j].Bone.OpCode)
            {
                case WadLinkOpcode.NotUseStack:
                    boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);
                    currentNode.Children.Add(newNode);
                    currentNode = newNode;
                    break;

                case WadLinkOpcode.Pop:
                    boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);
                    if (stack.Count > 0)
                    {
                        currentNode = stack.Pop();
                        currentNode.Children.Add(newNode);
                        currentNode = newNode;
                    }
                    else
                    {
                        RootNodes.Add(newNode);
                        currentNode = newNode;
                    }
                    break;

                case WadLinkOpcode.Push:
                    stack.Push(currentNode);
                    boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);
                    currentNode.Children.Add(newNode);
                    currentNode = newNode;
                    break;

                case WadLinkOpcode.Read:
                    boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);
                    if (stack.Count > 0)
                    {
                        var bone = stack.Pop();
                        bone.Children.Add(newNode);
                        currentNode = newNode;
                        stack.Push(bone);
                    }
                    else
                    {
                        RootNodes.Add(newNode);
                        currentNode = newNode;
                    }
                    break;
            }
        }

        if (RootNodes.Count > 0)
            UpdateSkeletonMatrices(RootNodes[0], Matrix4x4.Identity);

        // Legacy ExpandSkeleton: nodes are expanded by default; re-select the remembered bone.
        if (_lastBone is not null && _nodesDictionary.TryGetValue(_lastBone, out BoneTreeNode? last))
            last.IsSelected = true;
    }

    private void UpdateSkeletonMatrices(BoneTreeNode current, Matrix4x4 parentTransform)
    {
        current.BoneNode.GlobalTransform = current.BoneNode.Bone.Transform * parentTransform;
        foreach (var child in current.Children)
            UpdateSkeletonMatrices(child, current.BoneNode.GlobalTransform);
    }

    /// <summary>Rebuilds the tree after a structural change and refreshes the panel (legacy reload block).</summary>
    private void ReloadAfterChange(WadMeshBoneNode? lastBone, bool updateModel)
    {
        _lastBone = lastBone;
        RebuildTree();

        if (_panel is null)
            return;

        _panel.Skeleton = _bones;
        if (updateModel)
            _panel.UpdateModel();
        _panel.Invalidate();
    }

    private void UpdateUI()
    {
        bool previous = _initializing;
        _initializing = true;

        SkinVisible = _wad.GameVersion == TRVersion.Game.TombEngine;
        SkinText = "Skin: " + (_moveable.Skin?.Name ?? "None");

        if (SelectedBone is not null)
        {
            TranslationX = SelectedBone.Bone.Translation.X;
            TranslationY = SelectedBone.Bone.Translation.Y;
            TranslationZ = SelectedBone.Bone.Translation.Z;
            LightTypeIndex = SelectedBone.Bone.Mesh.LightingType == WadMeshLightingType.Normals ? 0 : 1;
            PopChecked = SelectedBone.Bone.OpCode is WadLinkOpcode.Pop or WadLinkOpcode.Read;
            PushChecked = SelectedBone.Bone.OpCode is WadLinkOpcode.Push or WadLinkOpcode.Read;
            _panel?.Invalidate();
        }

        _initializing = previous;
    }

    // ---- Pivot / lighting setters ----

    partial void OnTranslationXChanged(double value) => SetTranslation(0, value);
    partial void OnTranslationYChanged(double value) => SetTranslation(1, value);
    partial void OnTranslationZChanged(double value) => SetTranslation(2, value);

    private void SetTranslation(int axis, double value)
    {
        if (_initializing || SelectedBone is null)
            return;

        var t = SelectedBone.Bone.Translation;
        SelectedBone.Bone.Translation = axis switch
        {
            0 => new Vector3((float)value, t.Y, t.Z),
            1 => new Vector3(t.X, (float)value, t.Z),
            _ => new Vector3(t.X, t.Y, (float)value)
        };
        _tool.BoneOffsetMoved();
    }

    partial void OnLightTypeIndexChanged(int value)
    {
        if (_initializing || SelectedBone is null)
            return;

        SelectedBone.Bone.Mesh.LightingType = value == 0 ? WadMeshLightingType.Normals : WadMeshLightingType.VertexColors;
        _panel?.Invalidate();
    }

    partial void OnDrawGizmoChanged(bool value)
    {
        if (_panel is null) return;
        _panel.DrawGizmo = value;
        _panel.Invalidate();
    }

    partial void OnDrawGridChanged(bool value)
    {
        if (_panel is null) return;
        _panel.DrawGrid = value;
        _panel.Invalidate();
    }

    // ---- Structural bone operations ----

    [RelayCommand]
    private void TogglePop()
    {
        if (SelectedBone is null)
            return;

        SelectedBone.Bone.OpCode ^= WadLinkOpcode.Pop;
        ReloadAfterChange(SelectedBone, updateModel: false);
    }

    [RelayCommand]
    private void TogglePush()
    {
        if (SelectedBone is null)
            return;

        SelectedBone.Bone.OpCode ^= WadLinkOpcode.Push;
        ReloadAfterChange(SelectedBone, updateModel: false);
    }

    [RelayCommand]
    private void MoveBoneUp()
    {
        if (SelectedBone is null)
            return;

        int oldIndex = _bones.IndexOf(SelectedBone);
        if (oldIndex <= 0)
            return;

        (_bones[oldIndex], _bones[oldIndex - 1]) = (_bones[oldIndex - 1], _bones[oldIndex]);

        foreach (var animation in _moveable.Animations)
            foreach (var kf in animation.KeyFrames)
                (kf.Angles[oldIndex], kf.Angles[oldIndex - 1]) = (kf.Angles[oldIndex - 1], kf.Angles[oldIndex]);

        ReloadAfterChange(_bones[oldIndex - 1], updateModel: false);
    }

    [RelayCommand]
    private void MoveBoneDown()
    {
        if (SelectedBone is null)
            return;

        int oldIndex = _bones.IndexOf(SelectedBone);
        if (oldIndex == _bones.Count - 1 || oldIndex < 0)
            return;

        (_bones[oldIndex], _bones[oldIndex + 1]) = (_bones[oldIndex + 1], _bones[oldIndex]);

        foreach (var animation in _moveable.Animations)
            foreach (var kf in animation.KeyFrames)
                (kf.Angles[oldIndex], kf.Angles[oldIndex + 1]) = (kf.Angles[oldIndex + 1], kf.Angles[oldIndex]);

        ReloadAfterChange(_bones[oldIndex + 1], updateModel: false);
    }

    private void InsertNewBone(WadMesh mesh, WadMeshBoneNode parentNode)
    {
        var bone = new WadBone { Mesh = mesh, Name = "Bone_" + mesh.Name, OpCode = WadLinkOpcode.NotUseStack };
        var node = new WadMeshBoneNode(mesh, bone);

        int index = _bones.IndexOf(parentNode);

        if (index < 0 && _bones.Count > 0)
            return;

        _bones.Insert(index + 1, node);

        if (_bones.Count == 1 && index == -1)
            index = 0;

        foreach (var animation in _moveable.Animations)
            foreach (var kf in animation.KeyFrames)
                kf.Angles.Insert(index, new WadKeyFrameRotation());

        ReloadAfterChange(node, updateModel: true);
    }

    private void ReplaceExistingBone(WadMesh mesh, WadMeshBoneNode node)
    {
        if (mesh.Name.StartsWith("TeMov_"))
        {
            string[] tokens = mesh.Name.Split('_');
            Vector3 offset = new(float.Parse(tokens[2]), float.Parse(tokens[3]), float.Parse(tokens[4]));
            for (int i = 0; i < mesh.VertexPositions.Count; i++)
                mesh.VertexPositions[i] -= offset;
        }

        node.Bone.Mesh = mesh;
        node.Mesh = mesh;

        ReloadAfterChange(node, updateModel: true);
        UpdateUI();
    }

    [RelayCommand]
    private void DeleteBone()
    {
        if (SelectedBone is null)
            return;

        if (_nodesDictionary.Count <= 1)
        {
            UserDialogs.Show("Root bone can't be deleted.", "Warning",
                DialogButtons.OK, DialogIcon.Error);
            return;
        }

        var theNode = SelectedBone;

        if (UserDialogs.Show(Owner, "Are you really sure to delete bone '" + theNode.Bone.Name + "'?\n" +
                                "Angles associated to this bone will be deleted from all keyframes of all animations.",
                                "Delete bone", DialogButtons.YesNo, DialogIcon.Question) != DialogChoice.Yes)
            return;

        int index = _bones.IndexOf(theNode);

        if (index < 0)
            return;

        _bones.RemoveAt(index);

        foreach (var animation in _moveable.Animations)
            foreach (var kf in animation.KeyFrames)
                kf.Angles.RemoveAt(index);

        // Select the previous bone so the panel never keeps pointing at the deleted one.
        var neighbour = _bones.Count == 0 ? null : _bones[Math.Max(index - 1, 0)];

        ReloadAfterChange(neighbour, updateModel: false);

        if (_panel is not null)
            _panel.SelectedNode = neighbour;

        UpdateUI();
    }

    [RelayCommand]
    private void RenameBone()
    {
        if (SelectedBone is null)
            return;

        var theNode = SelectedBone;
        string result = InputBox.Show(Owner, "Rename bone", "Insert the name of the bone:", theNode.Bone.Name);
        if (!string.IsNullOrEmpty(result))
        {
            theNode.Bone.Name = result;
            if (_nodesDictionary.TryGetValue(theNode, out BoneTreeNode? node))
                node.Text = GetBoneName(theNode);
            _panel?.Invalidate();
        }
    }

    [RelayCommand]
    private void AddChildBoneFromFile()
    {
        if (SelectedBone is null)
            return;

        var mesh = WadActions.ImportMesh(_tool, Owner);
        if (mesh is null)
            return;

        InsertNewBone(mesh, SelectedBone);
    }

    [RelayCommand]
    private void AddChildBoneFromWad2()
    {
        if (SelectedBone is null)
            return;

        var meshViewModel = new MeshEditorWindowViewModel(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad) { ShowEditingTools = false };
        var meshDialog = new MeshEditorWindow { DataContext = meshViewModel };
        meshDialog.SetOwner(Owner);
        meshDialog.ShowDialog();
        if (meshViewModel.DialogResult != true || meshViewModel.SelectedMesh is null)
            return;

        InsertNewBone(meshViewModel.SelectedMesh.Clone(), SelectedBone);
    }

    [RelayCommand]
    private void ReplaceBoneFromFile()
    {
        if (SelectedBone is null)
            return;

        var mesh = WadActions.ImportMesh(_tool, Owner);
        if (mesh is null)
            return;

        ReplaceExistingBone(mesh, SelectedBone);
    }

    [RelayCommand]
    private void ReplaceBoneFromWad2()
    {
        if (SelectedBone is null)
            return;

        var meshViewModel = new MeshEditorWindowViewModel(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad) { ShowEditingTools = false };
        var meshDialog = new MeshEditorWindow { DataContext = meshViewModel };
        meshDialog.SetOwner(Owner);
        meshDialog.ShowDialog();
        if (meshViewModel.DialogResult != true || meshViewModel.SelectedMesh is null)
            return;

        ReplaceExistingBone(meshViewModel.SelectedMesh.Clone(), SelectedBone);
    }

    [RelayCommand]
    private void EditMesh()
    {
        if (SelectedBone is null)
            return;

        var theNode = SelectedBone;
        var meshViewModel = new MeshEditorWindowViewModel(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad, theNode.Mesh.Clone());
        var meshDialog = new MeshEditorWindow { DataContext = meshViewModel };
        meshDialog.SetOwner(Owner);
        meshDialog.ShowDialog();
        if (meshViewModel.DialogResult != true || meshViewModel.SelectedMesh is null)
            return;

        ReplaceExistingBone(meshViewModel.SelectedMesh.Clone(), theNode);
    }

    [RelayCommand]
    private void ReplaceAllBonesFromFile()
    {
        FileDialog dialog = new OpenFileDialog
        {
            InitialDirectory = PathC.GetDirectoryNameTry(_tool.DestinationWad.FileName),
            FileName = PathC.GetFileNameTry(_tool.DestinationWad.FileName),
            Filter = BaseGeometryImporter.FileExtensions.GetFilter(),
            Title = "Select a 3D file that you want to see imported."
        };
        if (dialog.ShowDialog(Owner) != true)
            return;

        var viewModel = new GeometryIOSettingsWindowViewModel(IOSettingsPresets.GeometryImportSettingsPresets);
        viewModel.SelectPreset(_tool.Configuration.GeometryIO_LastUsedGeometryImportPresetName);

        var settingsDialog = new GeometryIOSettingsWindow { DataContext = viewModel };
        settingsDialog.SetOwner(Owner);
        settingsDialog.ShowDialog();

        if (viewModel.DialogResult != true)
            return;

        _tool.Configuration.GeometryIO_LastUsedGeometryImportPresetName = viewModel.SelectedPreset?.Name;

        var settings = viewModel.GetCurrentSettings();
        var meshes = WadMesh.ImportFromExternalModel(dialog.FileName, settings, false, _tool.DestinationWad.MeshTexInfosUnique.FirstOrDefault());
        if (meshes is null || meshes.Count == 0)
        {
            MessageRaised?.Invoke("No meshes were imported. Selected 3D file is broken or has no valid data.", PopupType.Warning);
            return;
        }

        int meshCount;
        if (meshes.Count > _bones.Count)
        {
            meshCount = _bones.Count;
            MessageRaised?.Invoke("Mesh count is higher in imported model. Only first " + _bones.Count + " will be imported.", PopupType.Error);
        }
        else if (meshes.Count < _bones.Count)
        {
            meshCount = meshes.Count;
            MessageRaised?.Invoke("Mesh count is lower in imported model. Only meshes up to " + meshes.Count + " will be replaced.", PopupType.Error);
        }
        else
            meshCount = _bones.Count;

        for (int i = 0; i < meshCount; i++)
            ReplaceExistingBone(meshes[i], _bones[i]);
    }

    [RelayCommand]
    private void ExportMesh()
    {
        if (SelectedBone is not null)
            WadActions.ExportMesh(SelectedBone.Bone.Mesh, _tool, Owner);
    }

    [RelayCommand]
    private void SetToAll()
    {
        var lightType = LightTypeIndex == 0 ? WadMeshLightingType.Normals : WadMeshLightingType.VertexColors;
        foreach (var mesh in _moveable.Meshes)
            mesh.LightingType = lightType;
        UpdateUI();
    }

    [RelayCommand]
    private void SetSkin()
    {
        var mesh = WadActions.ImportMesh(_tool, Owner);
        if (mesh is null)
            return;

        if (_moveable.Meshes.All(m => !m.Hidden))
        {
            if (UserDialogs.Show(Owner, "Do you want to hide all unskinned meshes for this model?\n" +
                                    "You can unhide them later in the mesh editor.", "Hide unskinned meshes",
                                    DialogButtons.YesNo, DialogIcon.Question) == DialogChoice.Yes)
                _moveable.Meshes.ForEach(m => m.Hidden = true);
        }

        _moveable.Skin = mesh;
        UpdateUI();
    }

    [RelayCommand]
    private void ClearSkin()
    {
        _moveable.Skin = null;
        UpdateUI();
    }

    [RelayCommand]
    private void Save()
    {
        if (!SaveChanges())
            return;

        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;

    private bool SaveChanges()
    {
        int numPop = _bones.Count(b => b.Bone.OpCode == WadLinkOpcode.Pop);
        int numPush = _bones.Count(b => b.Bone.OpCode == WadLinkOpcode.Push);

        // More PUSH than POP is fine; the opposite leaks into previous moveables in the list.
        if (numPop > numPush)
            return UserDialogs.Show(Owner, "Your mesh tree is unbalanced, you have added more POP than PUSH.",
                "Error", DialogButtons.OKCancel, DialogIcon.Error) == DialogChoice.OK;

        if (RootNodes.Count > 1)
            return UserDialogs.Show(Owner, "Your mesh tree is unbalanced, you must have a single bone as root.",
                "Error", DialogButtons.OKCancel, DialogIcon.Error) == DialogChoice.OK;

        _moveable.Bones.Clear();
        foreach (var bone in _bones)
            _moveable.Bones.Add(bone.Bone);

        _wad.Moveables[_moveable.Id] = _moveable;
        _moveable.Version = DataVersion.GetNext();

        _tool.ToggleUnsavedChanges();
        return true;
    }

    private static IWindowOwner Owner => WPFUtils.GetWindowOwner();
}
