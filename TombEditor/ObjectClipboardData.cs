using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombEditor.Features.FlybyTimeline;
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

                ResolveIdentifierCollisions(editor, obj);

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

            NormalizePastedFlybyCameras(editor, unpackedObjects);

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

        private static void ResolveIdentifierCollisions(Editor editor, ObjectInstance obj)
        {
            ResetCollidingScriptId(editor, obj);
            ResetCollidingLuaName(editor, obj);
        }

        private static void ResetCollidingScriptId(Editor editor, ObjectInstance obj)
        {
            if (obj is not IHasScriptID scriptObject || !scriptObject.ScriptId.HasValue)
                return;

            if (editor.Level.GlobalScriptingIdsTable[(int)scriptObject.ScriptId.Value] != null)
                scriptObject.ScriptId = null;
        }

        private static void ResetCollidingLuaName(Editor editor, ObjectInstance obj)
        {
            if (obj is not IHasLuaName luaObject || string.IsNullOrEmpty(luaObject.LuaName))
                return;

            if (editor.Level.GetAllObjects().OfType<IHasLuaName>().Any(existingObject => existingObject.LuaName == luaObject.LuaName))
                luaObject.LuaName = string.Empty;
        }

        private static void NormalizePastedFlybyCameras(Editor editor, IReadOnlyCollection<ObjectInstance> unpackedObjects)
        {
            var pastedFlybys = unpackedObjects.OfType<FlybyCameraInstance>().ToList();

            if (pastedFlybys.Count == 0)
                return;

            // Reassign all pasted cameras to the currently visible sequence, or sequence 0 when none exists.
            ushort targetSequence = editor.SelectedFlybySequence ?? 0;

            foreach (var camera in pastedFlybys)
                camera.Sequence = targetSequence;

            var nextNumberBySequence = editor.Level.GetAllObjects()
                .OfType<FlybyCameraInstance>()
                .GroupBy(camera => camera.Sequence)
                .ToDictionary(group => group.Key, group => group.Max(camera => (int)camera.Number) + 1);

            foreach (var sequenceGroup in pastedFlybys.GroupBy(camera => camera.Sequence))
            {
                // Start numbering for the target sequence from the next available number, or 0 when no existing cameras are present.
                nextNumberBySequence.TryGetValue(sequenceGroup.Key, out int nextNumber);

                var remappedNumbers = new Dictionary<ushort, ushort>();
                var orderedSequenceGroup = sequenceGroup.OrderBy(camera => camera.Number).ToList();

                foreach (var camera in orderedSequenceGroup)
                {
                    ushort originalNumber = camera.Number;
                    ushort remappedNumber = (ushort)nextNumber++;

                    camera.Number = remappedNumber;
                    remappedNumbers[originalNumber] = remappedNumber;
                }

                foreach (var camera in orderedSequenceGroup)
                {
                    if ((camera.Flags & FlybyConstants.FlagCameraCut) != 0 &&
                        remappedNumbers.TryGetValue((ushort)camera.Timer, out ushort remappedTarget))
                    {
                        camera.Timer = (short)remappedTarget;
                    }
                }
            }
        }
    }
}
