#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>Animated preview (ports the shared preview timer + GetPerspectivePreview).</summary>
    public partial class AnimatedTexturesWindowViewModel
    {
        private const int PreviewSize = 128;

        private DispatcherTimer? _previewTimer;
        private AnimatedTextureFrame? _previewCurrentFrame;
        private int _previewCurrentRepeatTimes;
        private static ImageC _checkerboard = BuildCheckerboard();

        // UV-rotate scrolling state.
        private double _lastX;
        private double _lastY;

        // Rendered-frame cache, value-keyed like the shared _imageCache: each unique frame is
        // rendered only once, so a normal tick is just a dictionary lookup. (Re-rendering the
        // 128x128 perspective preview on every tick made the animation sluggish.)
        private readonly record struct PreviewFrameKey(ImageC Image, Vector2 TexCoord0, Vector2 TexCoord1, Vector2 TexCoord2, Vector2 TexCoord3);
        private readonly Cache<PreviewFrameKey, ImageSource> _previewFrameCache = new(512,
            key => RenderImage(key.Image, key.TexCoord0, key.TexCoord1, key.TexCoord2, key.TexCoord3, PreviewSize));

        [ObservableProperty] private ImageSource? _previewImage;
        [ObservableProperty] private int _previewProgress;
        [ObservableProperty] private int _previewMaximum;
        [ObservableProperty] private bool _tooManyFrames;
        [ObservableProperty] private string _tooManyFramesText = string.Empty;

        private void InitPreview()
        {
            // Render priority: the default (Background) only fires once the dispatcher is otherwise
            // idle, which delays/coalesces ticks and makes the animation visibly sluggish.
            _previewTimer = new DispatcherTimer(DispatcherPriority.Render);
            _previewTimer.Tick += (s, e) => PreviewTick();
        }

        private void StopPreview()
        {
            // Drop the timer so later OnAnimatedTexturesChanged calls (e.g. the final one raised
            // while the window closes) cannot restart it; UpdatePreviewState guards against null.
            _previewTimer?.Stop();
            _previewTimer = null;
        }

        /// <summary>Recomputes the timer interval, progress range and TRNG frame-count warning for the current set.</summary>
        private void UpdatePreviewState()
        {
            if (_previewTimer == null)
                return;

            int frameCount = SelectedSet?.Frames.Count ?? 0;
            if (frameCount == 0 || SelectedSet == null)
            {
                _previewTimer.Stop();
                PreviewImage = null;
                PreviewMaximum = 0;
                PreviewProgress = 0;
            }
            else
            {
                double fps = IsTombEngine && SelectedSet.AnimationType == AnimatedTextureAnimationType.UVRotate ? 30.0 : Math.Max(SelectedSet.Fps, 0.001);
                var interval = TimeSpan.FromMilliseconds(Math.Clamp(Math.Round(1000.0 / fps), 1, int.MaxValue));

                // Touching Interval (or Start) on a running DispatcherTimer resets its countdown:
                // With low-FPS sets (e.g. 0.2 = a 5s interval), repeated calls caused by settings
                // edits or texture-map selection changes would keep the tick from ever firing.
                // Reset the scroll position and timer only when something actually changed.
                if (_previewTimer.Interval != interval)
                {
                    _lastX = 0;
                    _lastY = 0;
                    _previewTimer.Interval = interval;
                }

                if (!_previewTimer.IsEnabled)
                    _previewTimer.Start();
            }

            int totalFrames = 0;
            if (SelectedSet != null)
                foreach (var frame in SelectedSet.Frames)
                    totalFrames += frame.Repeat;

            TooManyFrames = IsTrng && totalFrames > MaxLegacyFrames;
            if (TooManyFrames)
                TooManyFramesText = _localizationService.Format("TooManyFrames", totalFrames, MaxLegacyFrames);
        }

        private void PreviewTick()
        {
            var set = SelectedSet;
            int frameCount = set?.Frames.Count ?? 0;
            if (set == null || frameCount == 0)
            {
                PreviewImage = null;
                return;
            }

            int frameIndex = 0;

            if (set.AnimationType == AnimatedTextureAnimationType.Frames)
            {
                if (++_previewCurrentRepeatTimes < (_previewCurrentFrame?.Repeat ?? 0))
                    return;

                for (int i = 0; i < frameCount; ++i)
                    if (set.Frames[i] == _previewCurrentFrame)
                    {
                        frameIndex = (i + 1) % frameCount;
                        break;
                    }

                _previewCurrentRepeatTimes = 0;
            }
            else if (SelectedFrame != null)
            {
                int index = set.Frames.IndexOf(SelectedFrame.Model);
                if (index >= 0)
                    frameIndex = index;
            }

            _previewCurrentFrame = set.Frames[frameIndex];

            PreviewMaximum = frameCount - 1;
            PreviewProgress = frameIndex;

            ImageSource? baseImage = _previewCurrentFrame.Texture?.Image is { } image
                ? _previewFrameCache[new PreviewFrameKey(image, _previewCurrentFrame.TexCoord0, _previewCurrentFrame.TexCoord1, _previewCurrentFrame.TexCoord2, _previewCurrentFrame.TexCoord3)]
                : null;

            PreviewImage = baseImage != null && set.IsUvRotate
                ? ComposeUvRotate(baseImage, set)
                : baseImage;
        }

        /// <summary>Tiles and scrolls the frame to simulate UV-rotate (ports the shared preview Paint).</summary>
        private ImageSource ComposeUvRotate(ImageSource baseImage, AnimatedTextureSet set)
        {
            const double tile = 64.0;
            const double canvas = PreviewSize;

            var visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                if (IsTrng)
                {
                    double y = Math.Floor(_lastY) * 2.0;
                    dc.DrawImage(baseImage, new Rect(0, y - canvas, canvas, canvas));
                    dc.DrawImage(baseImage, new Rect(0, y, canvas, canvas));

                    _lastY += set.UvRotate;
                    if (_lastY >= tile && set.UvRotate > 0) _lastY = 0;
                    if (_lastY <= 0 && set.UvRotate < 0) _lastY = tile;
                }
                else
                {
                    double theta = (set.TenUvRotateDirection + 90.0) * (Math.PI / 180.0);
                    double dirX = Math.Cos(theta);
                    double dirY = Math.Sin(theta);
                    double step = (canvas / 30.0) * set.TenUvRotateSpeed;

                    _lastX = (_lastX + step * dirX) % tile;
                    _lastY = (_lastY + step * dirY) % tile;
                    if (_lastX < 0) _lastX += tile;
                    if (_lastY < 0) _lastY += tile;

                    double dx = _lastX * 2.0;
                    double dy = _lastY * 2.0;

                    dc.DrawImage(baseImage, new Rect(dx - canvas, dy - canvas, canvas, canvas));
                    dc.DrawImage(baseImage, new Rect(dx, dy - canvas, canvas, canvas));
                    dc.DrawImage(baseImage, new Rect(dx - canvas, dy, canvas, canvas));
                    dc.DrawImage(baseImage, new Rect(dx, dy, canvas, canvas));
                }
            }

            var bitmap = new RenderTargetBitmap(PreviewSize, PreviewSize, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>Renders a single frame's perspective preview at the given pixel size (used for the preview pane and grid thumbnails).</summary>
        public static ImageSource? RenderFrame(AnimatedTextureFrame frame, int size)
        {
            if (frame.Texture?.Image is not { } image)
                return null;

            return RenderImage(image, frame.TexCoord0, frame.TexCoord1, frame.TexCoord2, frame.TexCoord3, size);
        }

        private static ImageSource RenderImage(ImageC image, Vector2 texCoord0, Vector2 texCoord1, Vector2 texCoord2, Vector2 texCoord3, int size)
        {
            ImageC preview = GetPerspectivePreview(image, texCoord0, texCoord1, texCoord2, texCoord3, size, size);

            var bitmap = new WriteableBitmap(preview.Width, preview.Height, 96, 96, PixelFormats.Bgra32, null);
            byte[] bytes = preview.ToByteArray();
            bitmap.WritePixels(new Int32Rect(0, 0, preview.Width, preview.Height), bytes, preview.Width * 4, 0);
            bitmap.Freeze();
            return bitmap;
        }

        private static ImageC BuildCheckerboard()
        {
            const int size = 16;
            const int cell = 8;
            var image = ImageC.CreateNew(size, size);
            var light = new ColorC(0x60, 0x60, 0x60);
            var dark = new ColorC(0x40, 0x40, 0x40);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    image.SetPixel(x, y, ((x / cell + y / cell) % 2 == 0) ? light : dark);
            return image;
        }

        private static ImageC GetPerspectivePreview(ImageC input, Vector2 texCoord01, Vector2 texCoord00, Vector2 texCoord10, Vector2 texCoord11, int width, int height)
        {
            ImageC output = ImageC.CreateNew(width, height);

            float xTexCoordFactor = 1.0f / width;
            float yTexCoordFactor = 1.0f / width;
            Vector2 max = input.Size - new Vector2(1.0f);

            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                {
                    float outputTexCoordX = (x + 0.5f) * xTexCoordFactor;
                    float outputTexCoordY = (y + 0.5f) * yTexCoordFactor;
                    Vector2 inputTexCoord = texCoord00 * ((1.0f - outputTexCoordX) * (1.0f - outputTexCoordY)) +
                                            texCoord01 * ((1.0f - outputTexCoordX) * outputTexCoordY) +
                                            texCoord10 * (outputTexCoordX * (1.0f - outputTexCoordY)) +
                                            texCoord11 * (outputTexCoordX * outputTexCoordY);

                    inputTexCoord -= new Vector2(0.5f);
                    inputTexCoord = Vector2.Min(Vector2.Max(inputTexCoord, new Vector2()), max);

                    int firstX = (int)inputTexCoord.X;
                    int firstY = (int)inputTexCoord.Y;
                    int secondX = Math.Min(firstX + 1, input.Width - 1);
                    int secondY = Math.Min(firstY + 1, input.Height - 1);
                    float secondFactorX = inputTexCoord.X - firstX;
                    float secondFactorY = inputTexCoord.Y - firstY;

                    Vector4 foregroundPixel =
                        (Vector4)input.GetPixel(secondX, secondY) * (secondFactorX * secondFactorY) +
                        (Vector4)input.GetPixel(secondX, firstY) * (secondFactorX * (1.0f - secondFactorY)) +
                        (Vector4)input.GetPixel(firstX, secondY) * ((1.0f - secondFactorX) * secondFactorY) +
                        (Vector4)input.GetPixel(firstX, firstY) * ((1.0f - secondFactorX) * (1.0f - secondFactorY));

                    ColorC backgroundPixel = _checkerboard.GetPixel(x % _checkerboard.Size.X, y % _checkerboard.Size.Y);
                    output.SetPixel(x, y, (ColorC)ColorC.Mix(backgroundPixel, foregroundPixel));
                }

            return output;
        }
    }
}
