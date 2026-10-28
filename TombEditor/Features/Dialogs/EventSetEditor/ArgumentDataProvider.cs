#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>A single selectable entry for a listable argument: display text + boxed Lua value (+ optional source object).</summary>
    public sealed class ArgumentListItem
    {
        public ArgumentListItem(string display, string value, object? tag = null)
        {
            Display = display;
            Value = value;
            Tag = tag;
        }

        public string Display { get; }
        public string Value { get; }
        public object? Tag { get; }
        public override string ToString() => Display;
    }

    public enum ArgumentListAction
    {
        None,
        Locate,
        Play
    }

    /// <summary>
    /// Supplies the selectable item lists for the listable <see cref="ArgumentType"/>s and performs the
    /// locate / play-sound actions. Mirrors <c>NodeEditor.PopulateCachedNodeLists</c> and the list-building
    /// switch in the previous <c>ArgumentEditor</c>.
    /// </summary>
    public sealed class ArgumentDataProvider
    {
        private readonly Editor _editor;
        private readonly Level _level;
        private readonly IReadOnlyList<string> _luaFunctions;

        private readonly List<MoveableInstance> _moveables;
        private readonly List<StaticInstance> _statics;
        private readonly List<CameraInstance> _cameras;
        private readonly List<SinkInstance> _sinks;
        private readonly List<FlybyCameraInstance> _flybys;
        private readonly List<VolumeInstance> _volumes;
        private readonly List<string> _wadSlots;
        private readonly List<string> _spriteSlots;
        private readonly List<string> _soundTracks;
        private readonly List<string> _videos;
        private readonly IReadOnlyList<WadSoundInfo> _soundInfos;
        private readonly IReadOnlyList<Room> _rooms;

        public ArgumentDataProvider(Editor editor, IReadOnlyList<string> luaFunctions)
        {
            _editor = editor;
            _level = editor.Level;
            _luaFunctions = luaFunctions;

            var allObjects = _level.GetAllObjects();
            _moveables = allObjects.OfType<MoveableInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName) &&
                            !TombLib.Wad.Catalog.TrCatalog.IsMoveableAI(TRVersion.Game.TombEngine, o.WadObjectId.TypeId)).ToList();
            _statics = allObjects.OfType<StaticInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName)).ToList();
            _cameras = allObjects.OfType<CameraInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName)).ToList();
            _sinks = allObjects.OfType<SinkInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName)).ToList();
            _flybys = allObjects.OfType<FlybyCameraInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName)).ToList();
            _volumes = allObjects.OfType<VolumeInstance>().Where(o => !string.IsNullOrEmpty(o.LuaName)).ToList();
            _wadSlots = _level.Settings.WadGetAllMoveables().Select(m => TombLib.Wad.Catalog.TrCatalog.GetMoveableName(_level.Settings.GameVersion, m.Key.TypeId)).ToList();
            _spriteSlots = _level.Settings.WadGetAllSpriteSequences().Select(m => TombLib.Wad.Catalog.TrCatalog.GetSpriteSequenceName(_level.Settings.GameVersion, m.Key.TypeId)).ToList();
            _soundTracks = _level.Settings.GetListOfSoundtracks();
            _videos = _level.Settings.GetListOfVideos();
            _soundInfos = _level.Settings.GlobalSoundMap;
            _rooms = _level.ExistingRooms;
        }

        public static bool IsListType(ArgumentType type) => type >= ArgumentType.LuaScript;

        public static ArgumentListAction ActionFor(ArgumentType type)
        {
            switch (type)
            {
                case ArgumentType.SoundTracks:
                case ArgumentType.SoundEffects:
                    return ArgumentListAction.Play;
                case ArgumentType.Sinks:
                case ArgumentType.Statics:
                case ArgumentType.Moveables:
                case ArgumentType.Volumes:
                case ArgumentType.Cameras:
                case ArgumentType.FlybyCameras:
                    return ArgumentListAction.Locate;
                default:
                    return ArgumentListAction.None;
            }
        }

        public List<ArgumentListItem> GetItems(ArgumentType type, ArgumentLayout layout)
        {
            var items = new List<ArgumentListItem>();

            switch (type)
            {
                case ArgumentType.LuaScript:
                    foreach (var item in _luaFunctions)
                        items.Add(new ArgumentListItem(item, "LevelFuncs." + item));
                    break;
                case ArgumentType.VolumeEventSets:
                    foreach (var item in _level.Settings.VolumeEventSets.Select(s => s.Name))
                        items.Add(new ArgumentListItem(item, TextExtensions.Quote(item)));
                    break;
                case ArgumentType.GlobalEventSets:
                    foreach (var item in _level.Settings.GlobalEventSets.Select(s => s.Name))
                        items.Add(new ArgumentListItem(item, TextExtensions.Quote(item)));
                    break;
                case ArgumentType.Sinks:
                    foreach (var item in _sinks)
                        items.Add(MakeLuaNameItem(item));
                    break;
                case ArgumentType.Statics:
                    foreach (var item in _statics)
                        items.Add(MakeLuaNameItem(item));
                    break;
                case ArgumentType.Moveables:
                    items.Add(new ArgumentListItem("[ Activator ]", LuaSyntax.ActivatorNamePrefix));
                    foreach (var item in _moveables.Where(s => layout.CustomEnumeration.Count == 0 ||
                                 layout.CustomEnumeration.Any(e => s.WadObjectId.ShortName(TRVersion.Game.TombEngine)
                                     .IndexOf(e, StringComparison.InvariantCultureIgnoreCase) != -1)))
                        items.Add(MakeLuaNameItem(item));
                    break;
                case ArgumentType.Volumes:
                    foreach (var item in _volumes)
                        items.Add(MakeLuaNameItem(item));
                    break;
                case ArgumentType.Cameras:
                    foreach (var item in _cameras)
                        items.Add(MakeLuaNameItem(item));
                    break;
                case ArgumentType.FlybyCameras:
                    foreach (var item in _flybys)
                        items.Add(MakeLuaNameItem(item));
                    break;
                case ArgumentType.Rooms:
                    foreach (var item in _rooms)
                        items.Add(new ArgumentListItem(items.Count + ": " + item, TextExtensions.Quote(item.Name)));
                    break;
                case ArgumentType.SoundEffects:
                    foreach (var item in _soundInfos)
                        items.Add(new ArgumentListItem(item.ToString(), item.Id.ToString(), item));
                    break;
                case ArgumentType.SoundTracks:
                    foreach (var item in _soundTracks)
                        items.Add(new ArgumentListItem(Path.GetFileNameWithoutExtension(item), TextExtensions.Quote(item), item));
                    break;
                case ArgumentType.Videos:
                    foreach (var item in _videos)
                        items.Add(new ArgumentListItem(Path.GetFileNameWithoutExtension(item), TextExtensions.Quote(item)));
                    break;
                case ArgumentType.CompareOperator:
                    foreach (var item in Enum.GetValues(typeof(ConditionType)))
                        items.Add(new ArgumentListItem(item.ToString().SplitCamelcase(), items.Count.ToString()));
                    break;
                case ArgumentType.VolumeEvents:
                    foreach (var item in Event.VolumeEventTypes)
                        items.Add(new ArgumentListItem(item.ToString().SplitCamelcase(), items.Count.ToString()));
                    break;
                case ArgumentType.GlobalEvents:
                    foreach (var item in Event.GlobalEventTypes)
                        items.Add(new ArgumentListItem(item.ToString().SplitCamelcase(), items.Count.ToString()));
                    break;
                case ArgumentType.SpriteSlots:
                    foreach (var item in _spriteSlots.Where(s => layout.CustomEnumeration.Count == 0 ||
                                 layout.CustomEnumeration.Any(e => s.IndexOf(e, StringComparison.InvariantCultureIgnoreCase) != -1)))
                        items.Add(new ArgumentListItem(item, LuaSyntax.ObjectIDPrefix + LuaSyntax.Splitter + item));
                    break;
                case ArgumentType.WadSlots:
                    foreach (var item in _wadSlots.Where(s => layout.CustomEnumeration.Count == 0 ||
                                 layout.CustomEnumeration.Any(e => s.IndexOf(e, StringComparison.InvariantCultureIgnoreCase) != -1)))
                        items.Add(new ArgumentListItem(item, LuaSyntax.ObjectIDPrefix + LuaSyntax.Splitter + item));
                    break;
                case ArgumentType.Enumeration:
                    foreach (var item in layout.CustomEnumeration)
                        items.Add(new ArgumentListItem(item, layout.CustomEnumeration.IndexOf(item).ToString()));
                    break;
            }

            return items;
        }

        private static ArgumentListItem MakeLuaNameItem(PositionAndScriptBasedObjectInstance item)
            => new ArgumentListItem(item.ToShortString(), TextExtensions.Quote(item.LuaName ?? string.Empty), item);

        public void Locate(ArgumentListItem? item)
        {
            if (item?.Tag is ObjectInstance instance)
                _editor.ShowObject(instance);
        }

        public void Play(ArgumentType type, ArgumentListItem? item)
        {
            if (item == null)
                return;

            if (type == ArgumentType.SoundEffects && item.Tag is WadSoundInfo soundInfo)
            {
                WadSoundPlayer.PlaySoundInfo(_level, soundInfo);
            }
            else if (type == ArgumentType.SoundTracks && item.Tag is string track)
            {
                WadSoundPlayer.StopSample();
                WadSoundPlayer.PlaySoundtrack(_level, track);
            }
        }
    }
}
