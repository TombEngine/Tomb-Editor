using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Wad;

namespace TombLib.Controls
{
    public abstract class PanelItemPreview : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IWadObject CurrentObject
        {
            get { return _currentObject; }
            set
            {
                if (value is WadSpriteSequence &&
                   ((WadSpriteSequence)value).Sprites.Count > 0)
                {
                    if (!_animTimer.Enabled) _animTimer.Enabled = true;
                    if (_currentObject != value) _currentFrame = 0;
                }
                else
                {
                    _animTimer.Enabled = ValidObject(value) && AnimatePreview;
                }

                _currentObject = value;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }

        public bool DrawTransparency { get; set; } = false;

        public bool AnimatePreview
        {
            get { return _animatePreview; }
            set
            {
                if (_animatePreview == value)
                    return;

                _animatePreview = value;
                _animTimer.Enabled = value;
                _rotationFactor = 0.0f;
            }
        }
        private bool _animatePreview = true;

        // Preview animation state
        private Timer _animTimer = new Timer() { Interval = 15 };
        private int _currentFrame;
        private int _frameTimeout;

        private float _rotationFactor = 0.0f;
        private const float _rotationSpeed = 0.005f;
        private const float _rotationStep = 0.000125f;

        // Interaction state
        private float _lastX;
        private float _lastY;
        private IWadObject _currentObject = null;

        // Rendering state
        private RenderingTextureAllocator _textureAllocator;

        // Legacy rendering state
        private GraphicsDevice _legacyDevice;
        private WadRenderer _wadRenderer;

        public PanelItemPreview()
        {
            _animTimer.Tick += _animTimer_Tick;
        }

        private void _animTimer_Tick(object sender, EventArgs e)
        {
            if (!AnimatePreview && !(CurrentObject is WadSpriteSequence))
                return;

            if (Form.ActiveForm != this.FindForm())
                return;

            if (CurrentObject is WadSpriteSequence)
            {
                _frameTimeout++;
                if (_frameTimeout >= 20)
                {
                    _frameTimeout = 0;
                    if (_currentFrame < (CurrentObject as WadSpriteSequence).Sprites.Count - 1)
                        _currentFrame++;
                    else
                        _currentFrame = 0;
                    Invalidate();
                }
            }
            else if (AnimatePreview && CurrentObject != null)
            {
                if (_rotationFactor < _rotationSpeed)
                    _rotationFactor += _rotationStep;
                else if (_rotationFactor > _rotationSpeed)
                    _rotationFactor = _rotationSpeed;

                Camera.Rotate(_rotationFactor, 0.0f);
                Invalidate();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Rectangle clientRectangle = ClientRectangle;
            clientRectangle.Inflate(new Size(-5, -5));
        }

        public override void InitializeRendering(RenderingDevice device, bool antialias = false, ObjectRenderingQuality objectQuality = ObjectRenderingQuality.Undefined)
        {
            base.InitializeRendering(device, antialias, objectQuality);

            _textureAllocator = device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(1024, 1024, 1) });

            // Legacy rendering state
            {
                // Reset scrollbar
                _legacyDevice = DeviceManager.DefaultDeviceManager.___LegacyDevice;
                _wadRenderer = new WadRenderer(DeviceManager.DefaultDeviceManager.___LegacyDevice, true, true, 1024, 512, false);

                ResetCamera();

                // Initialize the rasterizer state for wireframe drawing
                SharpDX.Direct3D11.RasterizerStateDescription renderStateDesc =
                    new SharpDX.Direct3D11.RasterizerStateDescription
                    {
                        CullMode = SharpDX.Direct3D11.CullMode.None,
                        DepthBias = 0,
                        DepthBiasClamp = 0,
                        FillMode = SharpDX.Direct3D11.FillMode.Wireframe,
                        IsAntialiasedLineEnabled = true,
                        IsDepthClipEnabled = true,
                        IsFrontCounterClockwise = false,
                        IsMultisampleEnabled = true,
                        IsScissorEnabled = false,
                        SlopeScaledDepthBias = 0
                    };
            }
        }

        public void ResetCamera()
        {
            Func<ArcBallCamera> defaultCamera = () =>
                new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 100, 1000000, FieldOfView * (float)(Math.PI / 180));

            Camera = WadObjectRenderHelper.CreateCameraForObject(CurrentObject, _wadRenderer, FieldOfView) ?? defaultCamera();
        }

        public void GarbageCollect()
        {
            _wadRenderer?.GarbageCollect();
        }

        private bool ValidObject(IWadObject obj)
        {
            if (obj == null)
                return false;

            if (obj is WadMoveable)
            {
                return (obj as WadMoveable).Meshes.Any(m => m.VertexPositions.Count > 0);
            }
            else if (obj is WadStatic)
            {
                return (obj as WadStatic).Mesh.VertexPositions.Count > 0;
            }
            else if (obj is WadSpriteSequence)
            {
                return (obj as WadSpriteSequence).Sprites.Count > 0;
            }
            else if (obj is ImportedGeometry)
            {
                return true; // Imported geometry is implicitly always valid, as null model won't import.
            }
            else
            {
                return false; // Invalid object.
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wadRenderer?.Dispose();
                _textureAllocator?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_legacyDevice == null)
                return;
            base.OnPaint(e);
        }

        protected override void OnDraw()
        {
            if (!ValidObject(_currentObject))
                return;

            // To make sure things are in a defined state for legacy rendering...
            ((Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(Width, Height);

            if (CurrentObject is WadSpriteSequence)
            {
                var seq = (WadSpriteSequence)CurrentObject;
                if (seq.Sprites.Count <= _currentFrame)
                    return;

                WadSprite sprite = seq.Sprites[_currentFrame];
                float aspectRatioViewport = (float)ClientSize.Width / ClientSize.Height;
                float aspectRatioImage = (float)sprite.Texture.Image.Width / sprite.Texture.Image.Height;
                float aspectRatioAdjust = aspectRatioViewport / aspectRatioImage;
                Vector2 factor = Vector2.Min(new Vector2(1.0f / aspectRatioAdjust, aspectRatioAdjust), new Vector2(1.0f));

                SwapChain.RenderSprites(_textureAllocator, false, true, new List<Sprite>() { new Sprite
                {
                    Texture = sprite.Texture.Image,
                    PosStart = -0.9f * factor,
                    PosEnd = 0.9f * factor
                } });
            }
            else
            {
                WadObjectRenderHelper.RenderObject(CurrentObject, _wadRenderer, _legacyDevice, viewProjection, Camera.GetPosition(), DrawTransparency);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            // Make this control able to receive scroll and key board events...
            if (!Focused && Form.ActiveForm == FindForm())
                Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Camera.Zoom(-e.Delta * NavigationSpeedMouseWheelZoom);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _lastX = e.X;
            _lastY = e.Y;

            if (!(CurrentObject is WadSpriteSequence) && e.Button != MouseButtons.Left)
            {
                _animTimer.Stop();
                _rotationFactor = 0;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!(CurrentObject is WadSpriteSequence))
                _animTimer.Start();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (CurrentObject == null)
                return;

            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                float deltaX = (e.X - _lastX) / Height;
                float deltaY = (e.Y - _lastY) / Height;

                _lastX = e.X;
                _lastY = e.Y;

                if (e.Button == MouseButtons.Right)
                {
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                        Camera.Zoom(-deltaY * NavigationSpeedMouseZoom);
                    else if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                        Camera.Rotate(deltaX * NavigationSpeedMouseRotate,
                                     -deltaY * NavigationSpeedMouseRotate);
                }

                if ((e.Button == MouseButtons.Right && (ModifierKeys & Keys.Shift) == Keys.Shift) ||
                     e.Button == MouseButtons.Middle)
                    Camera.MoveCameraPlane(new Vector3(deltaX, deltaY, 0) * NavigationSpeedMouseTranslate);

                Invalidate();
            }
        }

        public abstract float FieldOfView { get; }
        public abstract float NavigationSpeedMouseWheelZoom { get; }
        public abstract float NavigationSpeedMouseZoom { get; }
        public abstract float NavigationSpeedMouseTranslate { get; }
        public abstract float NavigationSpeedMouseRotate { get; }
        public abstract bool ReadOnly { get; }
    }
}