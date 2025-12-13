using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombLib.IO;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombEditor
{
    [Serializable]
    public class ObjectClipboardData
    {
        private readonly byte[] _data;
        private readonly string _levelPath;

        public ObjectClipboardData(Editor editor)
        {
            // Write data
            _levelPath = editor.Level.Settings.LevelFilePath ?? "";
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(stream);
                var objectInstances = new List<ObjectInstance>();

                if (editor.SelectedObject is ObjectGroup)
                {
                    var og = (ObjectGroup)editor.SelectedObject;
                    objectInstances.AddRange(og);
                }
                else
                    objectInstances.Add(editor.SelectedObject);

                Prj2Writer.SaveToPrj2OnlyObjects(stream, editor.Level, objectInstances);
                _data = stream.GetBuffer();
            }
        }

        public Prj2Loader.LoadedObjects CreateObjects(Level level)
        {
            using (var stream = new MemoryStream(_data, false))
            {
                var loadedObjects = Prj2Loader.LoadFromPrj2OnlyObjects(_levelPath, level, stream,
                    new Prj2Loader.Settings { IgnoreTextures = true, IgnoreWads = true });
                return loadedObjects;
            }
        }

        public ObjectInstance MergeGetSingleObject(Editor editor)
        {
            var newLevelSettings = editor.Level.Settings.Clone();
            var loadedObjects = CreateObjects(editor.Level);
            
            if (loadedObjects.Objects.Count == 0)
                return null;

            var unpackedObjects = loadedObjects.Objects.Select(obj =>
            {
                obj.CopyDependentLevelSettings(
                    new Room.CopyDependentLevelSettingsArgs(null, newLevelSettings, loadedObjects.Settings, true));

                // A little workaround to detect collisions

                if (obj is IHasScriptID)
                {
                    try
                    {
                        editor.SelectedRoom.AddObject(editor.Level, obj);
                        editor.SelectedRoom.RemoveObject(editor.Level, obj);
                    }
                    catch (ScriptIdCollisionException)
                    {
                        ((IHasScriptID)obj).ScriptId = null;
                    }
                }

                if (obj is IHasLuaName)
                {
                    editor.SelectedRoom.AddObject(editor.Level, obj);
                    var luaObj = obj as IHasLuaName;

                    if (!luaObj.CanSetLuaName(luaObj.LuaName))
                        luaObj.LuaName = string.Empty;

                    editor.SelectedRoom.RemoveObject(editor.Level, obj);
                }

                if (obj is FlybyCameraInstance)
                {
                    var flyby = obj as FlybyCameraInstance;
                    var existingItems = editor.Level.GetAllObjects().OfType<FlybyCameraInstance>().Where(f => f.Sequence == flyby.Sequence).ToList();
                    if (existingItems.Count > 0 && existingItems.Any(f => f.Number == flyby.Number))
                        flyby.Number = (ushort)(existingItems.Max(f => f.Number) + 1);
                }

                if (obj is VolumeInstance)
                {
                    var vol = obj as VolumeInstance;
                    var existingEvent = editor.Level.Settings.VolumeEventSets.FirstOrDefault(e => e.Equals(vol.EventSet));
                    if (existingEvent != null)
                        vol.EventSet = existingEvent;
                }

                return obj;
            })
            .ToList();
                
			editor.UpdateLevelSettings(newLevelSettings);

            if (unpackedObjects.Count == 0)
                return null;
            else if (unpackedObjects.Count == 1)
                return unpackedObjects.FirstOrDefault();
            else
            {
                var unpackedChildren = unpackedObjects.OfType<PositionBasedObjectInstance>().ToList();
                return new ObjectGroup(unpackedChildren);
            }
        }
    }
}