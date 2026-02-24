using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Numerics;
using System.Reflection;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Rendering
{
    public enum ServiceObjectTexture
    {
        camera,
        flyby_camera,
        waypoint,
        imp_geo,
        sink,
        sound_source,
        sprite,
        ghost_block,
        volume,
        memo,
        light_effect,
        light_fog,
        light_point,
        light_shadow,
        light_spot,
        light_sun,
        unknown
    }

    public static class ServiceObjectTextures
    {
        private const int   _mipLevels = 4;          // Number of MIP levels, including root one
        private const float _mipStep = 4096.0f;      // Distance steps where mipmaps are switching
        private const float _fadeDistance = 256.0f;  // Distance at which sprite slowly fades out
        private const float _zoomRatio = 3.5f;       // Magic zoom ratio!

        private static List<ImageC> _serviceTextures;
        public static List<ImageC> Images
        {
            get
            {
                if (_serviceTextures == null)
                {
                    _serviceTextures = new List<ImageC>();
                    foreach (var name in Enum.GetNames(typeof(ServiceObjectTexture)))
                    {
                        var img = ImageC.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(nameof(TombLib)
                            + "." + nameof(Rendering) + ".ServiceObjectTextures." + name + ".png"));

                        // Add original texture
                        _serviceTextures.Add(img);

                        // Manually add MIP levels.
                        // We can't use automatic DX10 mipmaps because TRTombalized renderer.   

                        using (var bmp = img.ToBitmap())
                        {
                            for (int i = 0; i < _mipLevels; i++)
                            {
                                // Calculate next mip level dimensions
                                var mult = (i + 1) * 2;
                                var destRect = new Rectangle(0, 0, bmp.Width / mult, bmp.Height / mult);
                                var destImage = new Bitmap(bmp.Width / mult, bmp.Height / mult);

                                // Downscale and apply next MIP level
                                destImage.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
                                using (var graphics = System.Drawing.Graphics.FromImage(destImage))
                                {
                                    graphics.CompositingMode = CompositingMode.SourceCopy;
                                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                    graphics.CompositingQuality = CompositingQuality.HighQuality;

                                    using (var wrapMode = new ImageAttributes())
                                    {
                                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                        graphics.DrawImage(bmp, destRect, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, wrapMode);
                                    }
                                }

                                // Save next MIP level to image array
                                _serviceTextures.Add(ImageC.FromSystemDrawingImage(destImage));
                            }
                        }
                    }
                }
                return _serviceTextures;
            }
        }

        // Get sprite type for given instance

        public static ServiceObjectTexture GetType(ISpatial instance)
        {
            ServiceObjectTexture type;

            // Determine needed sprite type

            if (instance is LightInstance)
            {
                switch ((instance as LightInstance).Type)
                {
                    case LightType.Effect:
                        type = ServiceObjectTexture.light_effect;
                        break;
                    case LightType.FogBulb:
                        type = ServiceObjectTexture.light_fog;
                        break;
                    case LightType.Point:
                        type = ServiceObjectTexture.light_point;
                        break;
                    case LightType.Shadow:
                        type = ServiceObjectTexture.light_shadow;
                        break;
                    case LightType.Spot:
                        type = ServiceObjectTexture.light_spot;
                        break;
                    case LightType.Sun:
                        type = ServiceObjectTexture.light_sun;
                        break;
                    default:
                        type = ServiceObjectTexture.unknown;
                        break;
                }
            }
            else if (instance is MemoInstance) type = ServiceObjectTexture.memo;
            else if (instance is SinkInstance) type = ServiceObjectTexture.sink;
            else if (instance is SpriteInstance) type = ServiceObjectTexture.sprite;
            else if (instance is CameraInstance) type = ServiceObjectTexture.camera;
            else if (instance is VolumeInstance) type = ServiceObjectTexture.volume;
            else if (instance is GhostBlockInstance) type = ServiceObjectTexture.ghost_block;
            else if (instance is FlybyCameraInstance) type = ServiceObjectTexture.flyby_camera;
            else if (instance is WayPointInstance) type = ServiceObjectTexture.waypoint;
            else if (instance is SoundSourceInstance) type = ServiceObjectTexture.sound_source;
            else if (instance is ImportedGeometryInstance) type = ServiceObjectTexture.imp_geo;
            else type = ServiceObjectTexture.unknown;

            return type;
        }

        // Get unadjusted sprite bounds for given instance

        public static RectangleInt2 GetBounds(ISpatial instance)
        {
            var refIndex = (int)GetType(instance) * (_mipLevels + 1);
            var refTex   = Images[refIndex];
            var width    = (int)(refTex.Width  * _zoomRatio);
            var height   = (int)(refTex.Height * _zoomRatio);

            return new RectangleInt2(new VectorInt2(-width / 2, -height / 2), new VectorInt2(width / 2, height / 2));
        }

        // Gets sprite for specified instance type, if such sprite exists.
        // If no specific sprite for this type exists, returns default image with [?] mark.

        public static Sprite GetSprite(ISpatial instance, Vector3 camPos, Matrix4x4 camViewProjection, Size viewportSize, Vector4 color, Vector2 offset, bool noZ = false)
        {
            Vector3 absPos;
            Matrix4x4 posMatrix;

            if (instance is PositionBasedObjectInstance)
            {
                var obj = (PositionBasedObjectInstance)instance;
                posMatrix = obj.WorldPositionMatrix;
                absPos = obj.Position + obj.Room.WorldPos;
            }
            else if (instance is GhostBlockInstance)
            {
                var obj = (GhostBlockInstance)instance;
                posMatrix = obj.CenterMatrix(true);
                absPos = obj.Center(true);
            }
            else
                return null; // Unknown non-position-based instance!

            // Determine MIP level
            var distance = Vector3.Distance(absPos, camPos);
            var resolution = viewportSize.Height / 480.0f;
            var index = (int)Math.Min(Math.Max(Math.Round(distance / _mipStep / resolution), 0), _mipLevels);

            // Get the sprite and calculate dimensions            
            var refIndex  = (int)GetType(instance) * (_mipLevels + 1);
            var refTex    = Images[refIndex];
            var tex       = Images[refIndex + index];
            var width     = (int)(refTex.Width  * _zoomRatio);
            var height    = (int)(refTex.Height * _zoomRatio);
            var alignment = new Rectangle2(new Vector2(-width / 2.0f, -height / 2.0f), new Vector2(width / 2.0f, height / 2.0f));

            // Calculate screen-space position
            var heightRatio = ((float)viewportSize.Width / viewportSize.Height) * Level.SectorSizeUnit;
            var scale = (Level.SectorSizeUnit * 2.0f) / (distance != 0 ? distance : 1.0f);
            var pos = (posMatrix * camViewProjection).TransformPerspectively(new Vector3());
            var screenPos = pos.To2();

            // Add offset, if exists
            if (offset != Vector2.Zero)
                screenPos += offset / new Vector2(width, height) * scale;

            // Do fadeout if sprite is too near
            if (distance < _fadeDistance)
                color *= distance / _fadeDistance;

            // Calculate final viewport coordinates
            var start = screenPos - scale * new Vector2(alignment.End.X / heightRatio, alignment.End.Y / Level.SectorSizeUnit);
            var end   = screenPos - scale * new Vector2(alignment.Start.X / heightRatio, alignment.Start.Y / Level.SectorSizeUnit);

            // Make the sprite
            var result = new Sprite()
            {
                Texture = tex,
                PosStart = start,
                PosEnd = end,
                Tint = color
            };
            
            if (!noZ) result.Depth = pos.Z;  // Assign depth if needed
            if (pos.Z > 1.0f) result = null; // Discard out-of-bounds sprites

            return result;
        }
    }
}
