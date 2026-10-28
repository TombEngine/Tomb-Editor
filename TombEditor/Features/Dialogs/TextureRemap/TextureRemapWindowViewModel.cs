#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.Utils;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.TextureRemap;

public partial class TextureRemapWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly Editor _editor;
    private readonly IMessageService _messageService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SourceImageSize))]
    private LevelTexture? _sourceTexture;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DestinationImageSize))]
    private LevelTexture? _destinationTexture;

    [ObservableProperty] private Vector2 _sourceStart = Vector2.Zero;
    [ObservableProperty] private Vector2 _sourceEnd = Vector2.Zero;
    [ObservableProperty] private Vector2 _destinationStart = Vector2.Zero;
    [ObservableProperty] private float _scaling = 1.0f;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDestinationPanelEnabled))]
    private bool _untextureCompletely;

    [ObservableProperty] private bool _restrictToSelectedRooms;
    [ObservableProperty] private bool _remapAnimTextures;
    [ObservableProperty] private string _statusText = string.Empty;

    public ObservableCollection<LevelTexture> Textures { get; } = new();

    public Vector2 SourceImageSize => SourceTexture?.Image.Size ?? Vector2.Zero;
    public Vector2 DestinationImageSize => DestinationTexture?.Image.Size ?? Vector2.Zero;

    public bool IsDestinationPanelEnabled => !UntextureCompletely;

    public event EventHandler? RemapRectanglesChanged;

    public TextureRemapWindowViewModel(
        Editor? editor = null,
        IMessageService? messageService = null,
        ILocalizationService? localizationService = null)
    {
        _editor = editor ?? Editor.Instance;
        _messageService = ServiceLocator.ResolveService(messageService);
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        foreach (var tex in _editor.Level.Settings.Textures)
            Textures.Add(tex);

        _sourceTexture = Textures.FirstOrDefault();
        _destinationTexture = Textures.FirstOrDefault();

        if (_sourceTexture is not null)
            _sourceEnd = _sourceTexture.Image.Size;
    }

    partial void OnSourceTextureChanged(LevelTexture? value)
    {
        if (value is null)
            return;

        var bounds = new Rectangle2(Vector2.Zero, value.Image.Size);

        if (!bounds.Contains(SourceStart) || !bounds.Contains(SourceEnd))
        {
            SourceStart = Vector2.Zero;
            SourceEnd = value.Image.Size;
        }

        RemapRectanglesChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnDestinationTextureChanged(LevelTexture? value)
        => RemapRectanglesChanged?.Invoke(this, EventArgs.Empty);

    partial void OnScalingChanged(float value)
    {
        UpdateScaling();
        RemapRectanglesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clamps <see cref="Scaling"/> so the destination rectangle never leaves the destination image.
    /// </summary>
    public void UpdateScaling()
    {
        if (DestinationTexture is null)
            return;

        Vector2 destEnd = DestinationStart + Vector2.Abs(SourceEnd - SourceStart) * Scaling;
        Vector2 destSize = DestinationTexture.Image.Size;

        if (destEnd.X > destSize.X || destEnd.Y > destSize.Y)
        {
            Vector2 areaSize = Vector2.Abs(SourceEnd - SourceStart);

            if (areaSize.X <= 0 || areaSize.Y <= 0)
                return;

            float maxFactor = (float)Math.Floor(Math.Min(
                (destSize.X - DestinationStart.X) / areaSize.X,
                (destSize.Y - DestinationStart.Y) / areaSize.Y));

            if (maxFactor < 1)
                maxFactor = 1;

            Scaling = maxFactor; // re-enters OnScalingChanged; will hit the normal branch below
            return;
        }
    }

    public void NotifyRemapRectanglesChanged()
        => RemapRectanglesChanged?.Invoke(this, EventArgs.Empty);

    private bool SourceContains(Vector2 texCoord)
    {
        return SourceStart.X <= texCoord.X && SourceStart.Y <= texCoord.Y
            && SourceEnd.X >= texCoord.X && SourceEnd.Y >= texCoord.Y;
    }

    private bool SourceEquals(Rectangle2 rect)
        => SourceStart.X == rect.Start.X && SourceStart.Y == rect.Start.Y
        && SourceEnd.X == rect.End.X && SourceEnd.Y == rect.End.Y;

    private AnimatedTextureFrame RemapFrame(AnimatedTextureFrame source, float scale)
    {
        var dummy = new TextureArea
        {
            TexCoord0 = source.TexCoord0,
            TexCoord1 = source.TexCoord1,
            TexCoord2 = source.TexCoord2,
            TexCoord3 = source.TexCoord3
        };

        dummy = RemapTexture(dummy, scale);
        source.TexCoord0 = dummy.TexCoord0;
        source.TexCoord1 = dummy.TexCoord1;
        source.TexCoord2 = dummy.TexCoord2;
        source.TexCoord3 = dummy.TexCoord3;
        return source;
    }

    private TextureArea RemapTexture(TextureArea source, float scale)
    {
        var bounds = source.GetRect();

        if (bounds.Width * scale < 1.0f || bounds.Height * scale < 1.0f)
            scale = 1.0f;

        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0:
                    if (source.TexCoord0.X != bounds.X0)
                        source.TexCoord0.X = bounds.X0 + (source.TexCoord0.X - bounds.X0) * scale;

                    if (source.TexCoord0.Y != bounds.Y0)
                        source.TexCoord0.Y = bounds.Y0 + (source.TexCoord0.Y - bounds.Y0) * scale;

                    break;
                case 1:
                    if (source.TexCoord1.X != bounds.X0)
                        source.TexCoord1.X = bounds.X0 + (source.TexCoord1.X - bounds.X0) * scale;

                    if (source.TexCoord1.Y != bounds.Y0)
                        source.TexCoord1.Y = bounds.Y0 + (source.TexCoord1.Y - bounds.Y0) * scale;

                    break;
                case 2:
                    if (source.TexCoord2.X != bounds.X0)
                        source.TexCoord2.X = bounds.X0 + (source.TexCoord2.X - bounds.X0) * scale;

                    if (source.TexCoord2.Y != bounds.Y0)
                        source.TexCoord2.Y = bounds.Y0 + (source.TexCoord2.Y - bounds.Y0) * scale;

                    break;
                case 3:
                    if (source.TexCoord3.X != bounds.X0)
                        source.TexCoord3.X = bounds.X0 + (source.TexCoord3.X - bounds.X0) * scale;

                    if (source.TexCoord3.Y != bounds.Y0)
                        source.TexCoord3.Y = bounds.Y0 + (source.TexCoord3.Y - bounds.Y0) * scale;

                    break;
            }
        }

        var distance = ((bounds.Start - SourceStart) * scale) - (bounds.Start - SourceStart);
        var shift = DestinationStart - SourceStart;

        source.TexCoord0 += distance + shift;
        source.TexCoord1 += distance + shift;
        source.TexCoord2 += distance + shift;
        source.TexCoord3 += distance + shift;

        return source;
    }

    [RelayCommand]
    private void ApplyRemap()
    {
        if (SourceTexture is null || DestinationTexture is null)
        {
            _messageService.ShowError(_localizationService["MissingTextureMessage"]);
            return;
        }

        Level level = _editor.Level;

        IEnumerable<Room> relevantRooms = RestrictToSelectedRooms
            ? _editor.SelectedRooms
            : _editor.Level.ExistingRooms;

        var undoList = new List<UndoRedoInstance>();
        int roomTextureCount = 0;

        foreach (Room room in relevantRooms)
        {
            foreach (Sector sector in room.Sectors)
            {
                foreach (SectorFace face in sector.GetFaceTextures().Keys)
                {
                    TextureArea current = sector.GetFaceTexture(face);

                    if (current.Texture == SourceTexture
                        && SourceContains(current.TexCoord0)
                        && SourceContains(current.TexCoord1)
                        && SourceContains(current.TexCoord2)
                        && SourceContains(current.TexCoord3))
                    {
                        if (!undoList.Any(item => ((EditorUndoRedoInstance)item).Room == room))
                            undoList.Add(new GeometryUndoInstance(_editor.UndoManager, room));

                        current = RemapTexture(current, Scaling);
                        current.Texture = DestinationTexture;

                        if (UntextureCompletely)
                            current.Texture = null;

                        current.ParentArea = SourceEquals(current.ParentArea)
                            ? new Rectangle2(DestinationStart, DestinationStart + (SourceEnd - SourceStart) * Scaling)
                            : Rectangle2.Zero;

                        sector.SetFaceTexture(face, current);
                        roomTextureCount++;
                    }
                }
            }
        }

        if (undoList.Count > 0)
            _editor.UndoManager.Push(undoList);

        int animTextureCount = 0;

        if (RemapAnimTextures)
        {
            foreach (AnimatedTextureSet set in level.Settings.AnimatedTextureSets)
            {
                var toRemove = new List<AnimatedTextureFrame>();

                foreach (AnimatedTextureFrame frame in set.Frames)
                {
                    if (frame.Texture == SourceTexture
                        && SourceContains(frame.TexCoord0)
                        && SourceContains(frame.TexCoord1)
                        && SourceContains(frame.TexCoord2)
                        && SourceContains(frame.TexCoord3))
                    {
                        var newFrame = RemapFrame(frame, Scaling);
                        frame.TexCoord0 = newFrame.TexCoord0;
                        frame.TexCoord1 = newFrame.TexCoord1;
                        frame.TexCoord2 = newFrame.TexCoord2;
                        frame.TexCoord3 = newFrame.TexCoord3;
                        frame.Texture = DestinationTexture;
                        animTextureCount++;

                        if (UntextureCompletely)
                            toRemove.Add(frame);
                    }
                }

                set.Frames.RemoveAll(f => toRemove.Contains(f));
            }
        }

        Parallel.ForEach(relevantRooms, room => room.Rebuild(_editor.ShouldRelight, _editor.Configuration.Rendering3D_HighQualityLightPreview));

        foreach (Room room in relevantRooms)
            _editor.RoomTextureChange(room);

        StatusText = RemapAnimTextures
            ? _localizationService.Format("StatusWithAnims", roomTextureCount, animTextureCount)
            : _localizationService.Format("StatusRoomsOnly", roomTextureCount);
    }

    [RelayCommand]
    private void Close()
    {
        DialogResult = false;
    }
}
