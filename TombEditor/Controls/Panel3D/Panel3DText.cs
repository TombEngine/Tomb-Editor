using System;
using System.Numerics;
using TombLib.LevelData;
using TombLib;
using TombLib.Rendering;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private Text CreateTextTagForObject(Matrix4x4 matrix, string message)
        {
            if (matrix.TransformPerspectively(new Vector3()).Z > 1.0f)
                return null; // Discard text on the back

            if (_editor.CameraPreviewMode != CameraPreviewType.None)
                return null;

            return new Text
            {
                Font = _fontDefault,
                TextAlignment = new Vector2(0.0f, 0.0f),
                PixelPos = new VectorInt2(10, -10),
                Pos = matrix.TransformPerspectively(new Vector3()).To2(),
                Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                String = message
            };
        }

        private string GetObjectTriggerString(ObjectInstance instance)
        {
            string message = "";
            foreach (var room in _editor.Level.ExistingRooms)
                foreach (var trigger in room.Triggers)
                    if (trigger.Target == instance || trigger.Timer == instance || trigger.Extra == instance)
                        message += "\nTriggered in Room " + trigger.Room + " on sectors " + trigger.Area;
            return message;
        }

        private static string GetObjectRotationString(Room room, PositionBasedObjectInstance instance)
        {
            string message = string.Empty;

            if (instance is IRotateableY)
                message += "Rotation Y: " + Math.Round((instance as IRotateableY).RotationY);

            if (instance is IRotateableYX && (instance as IRotateableYX).RotationX != 0.0f)
                message += " Rotation X: " + Math.Round((instance as IRotateableYX).RotationX);

            if (instance is IRotateableYXRoll && (instance as IRotateableYXRoll).Roll != 0.0f)
                message += " Roll: " + Math.Round((instance as IRotateableYXRoll).Roll);

            return message;
        }

        private static string GetObjectPositionString(Room room, PositionBasedObjectInstance instance)
        {
            // Get the distance between point and floor in units
            float height = (instance.Position.Y - GetFloorHeight(room, instance.Position)) / Level.FullClickHeight;

            string message = "Pos: [" + (int)instance.WorldPosition.X + ", " + -(int)instance.WorldPosition.Y + ", " + (int)instance.WorldPosition.Z + "]";
            message += "\nSector Pos: [" + instance.SectorPosition.X + ", " + instance.SectorPosition.Y + "], " + height + " clicks";

            return message;
        }
    }
}
