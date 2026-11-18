#nullable enable

using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Threading;
using TombLib.Graphics;
using TombLib.Input;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Rendering.DirectX11;
using TombLib.Wad;

namespace TombLib.WPF.Controls.Rendering;

/// <summary>Renders animated WAD previews and imported geometry within a native WPF viewport.</summary>
public abstract class PanelItemPreview : RenderingPanel
{
    private const float RotationSpeed = 0.005f;
    private const float RotationStep = 0.000125f;
    private readonly DispatcherTimer _animationTimer = new() { Interval = TimeSpan.FromMilliseconds(15) };
    private RenderingTextureAllocator? _textureAllocator;
    private GraphicsDevice? _legacyDevice;
    private WadRenderer? _wadRenderer;
    private IWadObject? _currentObject;
    private bool _animatePreview = true;
    private int _currentFrame;
    private int _frameTimeout;
    private float _rotationFactor;
    private float _lastX;
    private float _lastY;

    /// <summary>Gets or sets the object displayed by this viewport.</summary>
    public IWadObject? CurrentObject
    {
        get => _currentObject;
        set
        {
            if (_currentObject != value)
                _currentFrame = 0;

            _currentObject = value;
            _animationTimer.IsEnabled = value is WadSpriteSequence { Sprites.Count: > 0 } || (ValidObject(value) && AnimatePreview);
            Invalidate();
        }
    }

    /// <summary>Gets or sets the preview camera.</summary>
    public ArcBallCamera Camera { get; set; } = new(new Vector3(0.0f, 256.0f, 0.0f), 0.0f, 0.0f,
        -(float)Math.PI / 2.0f, (float)Math.PI / 2.0f, 2048.0f, 100.0f, 1000000.0f, (float)Math.PI / 4.0f);

    /// <summary>Gets or sets whether alpha-blended object surfaces are rendered.</summary>
    public bool DrawTransparency { get; set; }

    /// <summary>Gets or sets whether the preview rotates automatically.</summary>
    public bool AnimatePreview
    {
        get => _animatePreview;
        set
        {
            _animatePreview = value;
            _animationTimer.IsEnabled = CurrentObject is WadSpriteSequence || (value && ValidObject(CurrentObject));
            _rotationFactor = 0.0f;
        }
    }

    public abstract float FieldOfView { get; }
    public abstract float NavigationSpeedMouseWheelZoom { get; }
    public abstract float NavigationSpeedMouseZoom { get; }
    public abstract float NavigationSpeedMouseTranslate { get; }
    public abstract float NavigationSpeedMouseRotate { get; }
    public abstract bool ReadOnly { get; }

    protected PanelItemPreview()
    {
        _animationTimer.Tick += OnAnimationTick;
    }

    public override void InitializeRendering(RenderingDevice device, bool antialias = false,
        ObjectRenderingQuality objectQuality = ObjectRenderingQuality.Undefined)
    {
        base.InitializeRendering(device, antialias, objectQuality);
        _textureAllocator?.Dispose();
        _wadRenderer?.Dispose();
        _textureAllocator = device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(1024, 1024, 1) });
        _legacyDevice = DeviceManager.DefaultDeviceManager.___LegacyDevice;
        _wadRenderer = new WadRenderer(_legacyDevice, true, true, 1024, 512, false);
        ResetCamera();
    }

    /// <summary>Fits the camera to the current object.</summary>
    public void ResetCamera()
    {
        Camera = _wadRenderer is not null && CurrentObject is not null
            ? WadObjectRenderHelper.CreateCameraForObject(CurrentObject, _wadRenderer, FieldOfView) ?? CreateDefaultCamera()
            : CreateDefaultCamera();
    }

    private ArcBallCamera CreateDefaultCamera() => new(new Vector3(0.0f, 256.0f, 0.0f), 0.0f, 0.0f,
        -(float)Math.PI / 2.0f, (float)Math.PI / 2.0f, 2048.0f, 100.0f, 1000000.0f, FieldOfView * (float)Math.PI / 180.0f);

    /// <summary>Releases render caches for objects no longer used by the preview.</summary>
    public void GarbageCollect() => _wadRenderer?.GarbageCollect();

    private static bool ValidObject(IWadObject? value) => value switch
    {
        WadMoveable moveable => moveable.Meshes.Any(mesh => mesh.VertexPositions.Count > 0),
        WadStatic staticObject => staticObject.Mesh.VertexPositions.Count > 0,
        WadSpriteSequence sequence => sequence.Sprites.Count > 0,
        ImportedGeometry => true,
        _ => false
    };

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        if (!IsOwnerActive)
            return;

        if (CurrentObject is WadSpriteSequence sequence)
        {
            if (++_frameTimeout < 20)
                return;

            _frameTimeout = 0;
            _currentFrame = sequence.Sprites.Count > 0 ? (_currentFrame + 1) % sequence.Sprites.Count : 0;
        }
        else if (AnimatePreview && CurrentObject is not null)
        {
            _rotationFactor = Math.Min(RotationSpeed, _rotationFactor + RotationStep);
            Camera.Rotate(_rotationFactor, 0.0f);
        }
        else
        {
            return;
        }

        Invalidate();
    }

    protected override void OnDraw()
    {
        var currentObject = CurrentObject;
        if (!ValidObject(currentObject) || currentObject is null || _wadRenderer is null || _legacyDevice is null || _textureAllocator is null)
            return;

        ((Dx11RenderingSwapChain)SwapChain).BindForce();
        ((Dx11RenderingDevice)Device).ResetState();
        var viewProjection = Camera.GetViewProjectionMatrix(Width, Height);
        if (currentObject is WadSpriteSequence sequence)
        {
            if (_currentFrame >= sequence.Sprites.Count)
                return;

            var sprite = sequence.Sprites[_currentFrame];
            float aspectAdjustment = ((float)Width / Height) / ((float)sprite.Texture.Image.Width / sprite.Texture.Image.Height);
            var factor = Vector2.Min(new Vector2(1.0f / aspectAdjustment, aspectAdjustment), Vector2.One);
            SwapChain.RenderSprites(_textureAllocator, false, true, new List<Sprite> { new()
            {
                Texture = sprite.Texture.Image,
                PosStart = -0.9f * factor,
                PosEnd = 0.9f * factor
            } });
        }
        else
        {
            WadObjectRenderHelper.RenderObject(currentObject, _wadRenderer, _legacyDevice, viewProjection, Camera.GetPosition(), DrawTransparency);
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (!Focused && IsOwnerActive)
            Focus();
    }

    protected override void OnMouseWheel(PointerEventArgs e)
    {
        base.OnMouseWheel(e);
        Camera.Zoom(-e.Delta * NavigationSpeedMouseWheelZoom);
        Invalidate();
    }

    protected override void OnMouseDown(PointerEventArgs e)
    {
        base.OnMouseDown(e);
        _lastX = e.X;
        _lastY = e.Y;
        if (CurrentObject is not WadSpriteSequence && e.Button != PointerButtons.Left)
        {
            _animationTimer.Stop();
            _rotationFactor = 0.0f;
        }
    }

    protected override void OnMouseUp(PointerEventArgs e)
    {
        base.OnMouseUp(e);
        if (CurrentObject is not WadSpriteSequence)
            _animationTimer.IsEnabled = AnimatePreview && ValidObject(CurrentObject);
    }

    protected override void OnMouseMove(PointerEventArgs e)
    {
        base.OnMouseMove(e);
        if (CurrentObject is null || (e.Button != PointerButtons.Right && e.Button != PointerButtons.Middle))
            return;

        float deltaX = (e.X - _lastX) / Height;
        float deltaY = (e.Y - _lastY) / Height;
        _lastX = e.X;
        _lastY = e.Y;
        if (e.Button == PointerButtons.Right)
        {
            if (ModifierKeys.HasFlag(EditorKey.Control))
                Camera.Zoom(-deltaY * NavigationSpeedMouseZoom);
            else if (!ModifierKeys.HasFlag(EditorKey.Shift))
                Camera.Rotate(deltaX * NavigationSpeedMouseRotate, -deltaY * NavigationSpeedMouseRotate);
        }

        if ((e.Button == PointerButtons.Right && ModifierKeys.HasFlag(EditorKey.Shift)) || e.Button == PointerButtons.Middle)
            Camera.MoveCameraPlane(new Vector3(deltaX, deltaY, 0.0f) * NavigationSpeedMouseTranslate);

        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= OnAnimationTick;
            _wadRenderer?.Dispose();
            _textureAllocator?.Dispose();
        }

        base.Dispose(disposing);
    }
}
