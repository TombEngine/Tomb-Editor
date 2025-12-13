using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using TombLib.LevelData.SectorEnums;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerClassicTR : LevelCompiler
    {
        private Room[] _sortedRooms;
        private readonly Dictionary<Room, tr_room> _tempRooms = new Dictionary<Room, tr_room>(new ReferenceEqualityComparer<Room>());

        private bool _supportsTRNGPlugins = false;
        private readonly Dictionary<ushort, string> _plugins = new();
        private readonly ScriptIdTable<IHasScriptID> _scriptingIdsTable;
        private byte[] _texture32Data;
        private readonly List<ushort> _floorData = new List<ushort>();
        private readonly List<byte> _pluginFloorData = new List<byte>();
        private readonly List<tr_mesh> _meshes = new List<tr_mesh>();
        private readonly List<uint> _meshPointers = new List<uint>();
        private readonly List<tr_animation> _animations = new List<tr_animation>();
        private readonly List<tr_state_change> _stateChanges = new List<tr_state_change>();
        private readonly List<tr_anim_dispatch> _animDispatches = new List<tr_anim_dispatch>();
        private readonly List<short> _animCommands = new List<short>();
        private readonly List<int> _meshTrees = new List<int>();
        private readonly List<short> _frames = new List<short>();
        private List<tr_moveable> _moveables = new List<tr_moveable>();
        private readonly List<tr_staticmesh> _staticMeshes = new List<tr_staticmesh>();

        private List<tr_sprite_texture> _spriteTextures = new List<tr_sprite_texture>();
        private List<tr_sprite_sequence> _spriteSequences = new List<tr_sprite_sequence>();
        private readonly List<tr_camera> _cameras = new List<tr_camera>();
        private readonly List<tr4_flyby_camera> _flyByCameras = new List<tr4_flyby_camera>();
        private readonly List<tr_sound_source> _soundSources = new List<tr_sound_source>();
        private tr_box[] _boxes = new tr_box[0];
        private ushort[] _overlaps = new ushort[0];
        private tr_zone[] _zones = new tr_zone[0];

        private readonly List<tr_item> _items = new List<tr_item>();
        private List<tr_ai_item> _aiItems = new List<tr_ai_item>();

        private Util.TexInfoManager _textureInfoManager;

        // Temporary dictionaries for mapping editor IDs to level IDs
        private Dictionary<MoveableInstance, int> _moveablesTable;
        private Dictionary<CameraInstance, int> _cameraTable;
        private Dictionary<SinkInstance, int> _sinkTable;
        private Dictionary<MoveableInstance, int> _aiObjectsTable;
        private Dictionary<SoundSourceInstance, int> _soundSourcesTable;
        private Dictionary<FlybyCameraInstance, int> _flybyTable;
        private Dictionary<StaticInstance, int> _staticsTable;

        // Collected game limits
        private Dictionary<Limit, int> _limits;

        public LevelCompilerClassicTR(Level level, string dest, IProgressReporter progressReporter)
            : base(level, dest, progressReporter)
        {
            _scriptingIdsTable = level.GlobalScriptingIdsTable.Clone();

            _limits = new Dictionary<Limit, int>();
            foreach (Limit limit in Enum.GetValues(typeof(Limit)))
                _limits.Add(limit, TrCatalog.GetLimit(level.Settings.GameVersion, limit));
        }

        public override CompilerStatistics CompileLevel(CancellationToken cancelToken)
        {
            string ngVersion = null;

            if (_level.IsNG)
            {
                ngVersion = DetectTRNGVersion();
                _supportsTRNGPlugins = new Version(ngVersion) >= new Version(1, 3, 0, 0);
            }

            ReportProgress(0, "Tomb Raider Level Compiler");

            if (_level.Settings.Wads.All(wad => wad.Wad == null))
                throw new NotSupportedException("A wad must be loaded to compile the final level.");

            _textureInfoManager = new Util.TexInfoManager(_level, _progressReporter, _limits[Limit.TexPageSize]);
            cancelToken.ThrowIfCancellationRequested();
            // Try to shuffle rooms to accomodate for more vertically connected ones
            _sortedRooms = _level.GetRearrangedRooms(_progressReporter);

            // Write plugins if TRNG is detected
            if (_supportsTRNGPlugins)
                WritePlugins();

            // Prepare level data
            ConvertWad2DataToTrData(_level);
            BuildRooms();
            cancelToken.ThrowIfCancellationRequested();

            // Compile textures
            ReportProgress(30, "Packing textures");
            _textureInfoManager.LayOutAllData(_level.Settings.GameVersion);

            ReportProgress(35, "   Number of TexInfos: " + _textureInfoManager.TexInfoCount);
            ReportProgress(35, "   Number of anim texture sequences: " + _textureInfoManager.AnimatedTextures.Count);

            int texInfoLimit = _limits[Limit.TexInfos];

            if (_textureInfoManager.TexInfoCount > texInfoLimit)
                _progressReporter.ReportWarn("TexInfo number overflow, maximum is " + texInfoLimit + ". Please reduce level complexity.");

            cancelToken.ThrowIfCancellationRequested();

            GetAllReachableRooms();
            BuildPathFindingData();
            PrepareSoundSources();
            PrepareItems();
            BuildCamerasAndSinks();
            BuildFloorData();
            cancelToken.ThrowIfCancellationRequested();

            // Combine the texture data collected
            var pageCount = PrepareTextures();

            int texPageLimit = _limits[Limit.TexPages];

            if (pageCount > texPageLimit)
                _progressReporter.ReportWarn("Level has " + pageCount + " texture pages, while limit is " + texPageLimit + ". Use less textures, reduce padding or turn on aggressive texture packing in level settings.");

            _progressReporter.ReportInfo("\nWriting level file...\n");

            //Write the level
            switch (_level.Settings.GameVersion)
            {
                case TRVersion.Game.TR1:
                     WriteLevelTr1();
                     break;
                case TRVersion.Game.TR2:
                     WriteLevelTr2();
                     break;
                 case TRVersion.Game.TR3:
                     WriteLevelTr3();
                     break;
                case TRVersion.Game.TR4:
                    WriteLevelTr4();
                    break;
                case TRVersion.Game.TRNG:
                    WriteLevelTr4(ngVersion);
                    break;
                case TRVersion.Game.TR5:
                    WriteLevelTr5();
                    break;
                case TRVersion.Game.TR1X or TRVersion.Game.TR2X:
                    WriteLevelTrx();
                    break;
                default:
                    throw new NotImplementedException("The selected game engine is not supported yet");
            }

            // Needed to make decision about backup (delete or restore)
            _compiledSuccessfully = !cancelToken.IsCancellationRequested;

            _progressReporter.ReportInfo("\nOutput file: " + _finalDest);

            // Return statistics
            return new CompilerStatistics
            {
                BoxCount = _boxes.Length,
                OverlapCount = _overlaps.Length,
                ObjectTextureCount = _textureInfoManager.TexInfoCount,
            };
        }

        private void WritePlugins()
        {
            ReportProgress(1, "Writing plugins");

            foreach (KeyValuePair<ushort, TriggerParameterUshort> plugin in _level.Settings.GetPluginRange())
            {
                _plugins.Add(plugin.Key, plugin.Value.Name);
                _progressReporter.ReportInfo("Plugin: " + plugin.Value.Name + " with ID " + plugin.Key);
            }
        }

        private void PrepareSoundSources()
        {
            ReportProgress(40, "Building sound sources");

            _soundSourcesTable = new Dictionary<SoundSourceInstance, int>(new ReferenceEqualityComparer<SoundSourceInstance>());

            foreach (var room in _sortedRooms.Where(room => room != null))
                foreach (var obj in room.Objects.OfType<SoundSourceInstance>())
                    _soundSourcesTable.Add(obj, _soundSourcesTable.Count);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var instance in _soundSourcesTable.Keys)
            {
                if (instance.IsEmpty)
                    continue;

                WadSoundInfo soundInfo = _level.Settings.WadTryGetSoundInfo(instance.SoundId);
                if (soundInfo == null)
                {
                    _progressReporter.ReportWarn("Sound (" + instance.SoundNameToDisplay + ") for sound source in room '" + instance.Room + "' at '" + instance.Position + "' is missing.");
                    continue;
                }

                ushort flags = 0;

                switch (instance.PlayMode)
                {
                    case SoundSourcePlayMode.Automatic:
                        flags = instance.Room.Alternated ? (instance.Room.IsAlternate ? (ushort)0x40 : (ushort)0x80) : (ushort)0xC0;
                        break;
                    case SoundSourcePlayMode.Always:
                        flags = 0xC0;
                        break;
                    case SoundSourcePlayMode.OnlyInBaseRoom:
                        flags = 0x80;
                        break;
                    case SoundSourcePlayMode.OnlyInAlternateRoom:
                        flags = 0x40;
                        break;
                }

                Vector3 position = instance.Room.WorldPos + instance.Position;
                _soundSources.Add(new tr_sound_source
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    SoundID = (ushort)instance.SoundId,
                    Flags = flags
                });
            }

            ReportProgress(41, "    Number of sound sources: " + _soundSources.Count);
        }

        private void BuildCamerasAndSinks()
        {
            ReportProgress(46, "Building cameras and sinks");

            {
                int cameraSinkID = 0;
                int flybyID = 0;
                _cameraTable = new Dictionary<CameraInstance, int>(new ReferenceEqualityComparer<CameraInstance>());
                _sinkTable = new Dictionary<SinkInstance, int>(new ReferenceEqualityComparer<SinkInstance>());
                _flybyTable = new Dictionary<FlybyCameraInstance, int>(new ReferenceEqualityComparer<FlybyCameraInstance>());
                foreach (var room in _sortedRooms.Where(room => room != null))
                {
                    foreach (var obj in room.Objects.OfType<CameraInstance>())
                        _cameraTable.Add(obj, cameraSinkID++);
                    foreach (var obj in room.Objects.OfType<FlybyCameraInstance>())
                        _flybyTable.Add(obj, flybyID++);
                }
                foreach (var room in _sortedRooms.Where(room => room != null))
                {
                    foreach (var obj in room.Objects.OfType<SinkInstance>())
                        _sinkTable.Add(obj, cameraSinkID++);
                }
            }

            foreach (var instance in _cameraTable.Keys)
            {
                Vector3 position = instance.Room.WorldPos + instance.Position;
                ushort flags = 0x0;

                if (instance.CameraMode == CameraInstanceMode.Sniper)
                {
                    flags = 0x3;
                }
                else
                {
                    if (instance.CameraMode == CameraInstanceMode.Locked)
                        flags = 0x1;
                    if (instance.GlideOut)
                        flags |= 0x4;
                }

                _cameras.Add(new tr_camera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = (short)_roomRemapping[instance.Room],
                    Flags = flags
                });
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var instance in _sinkTable.Keys)
            {
                int xSector = (int)Math.Floor(instance.Position.X / Level.SectorSizeUnit);
                int zSector = (int)Math.Floor(instance.Position.Z / Level.SectorSizeUnit);

                var tempRoom = _tempRooms[instance.Room];
                Vector3 position = instance.Room.WorldPos + instance.Position;

                ushort boxIndex;
                if (_level.Settings.GameVersion.Native() >= TRVersion.Game.TR3)
                    boxIndex = (ushort)((tempRoom.Sectors[tempRoom.NumZSectors * xSector + zSector].BoxIndex & 0x7FF0) >> 4);
                else
                    boxIndex = tempRoom.Sectors[tempRoom.NumZSectors * xSector + zSector].BoxIndex;

                _cameras.Add(new tr_camera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)-Math.Round(position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = instance.Strength,
                    Flags = boxIndex
                });
            }

            foreach (var instance in _flybyTable.Keys)
            {
                Vector3 direction = instance.GetDirection();
                Vector3 position = instance.Room.WorldPos + instance.Position;
                ushort rollTo65536 = (ushort)(65536 - Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Roll * (65536.0 / 360.0)))));
                _flyByCameras.Add(new tr4_flyby_camera
                {
                    X = (int)Math.Round(position.X),
                    Y = (int)Math.Round(-position.Y),
                    Z = (int)Math.Round(position.Z),
                    Room = _roomRemapping[instance.Room],
                    FOV = (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Fov * (65536.0 / 360.0)))),
                    Roll = unchecked((short)rollTo65536),
                    Timer = (ushort)instance.Timer,
                    Speed = (ushort)Math.Round(Math.Max(0, Math.Min(ushort.MaxValue, instance.Speed * 655.0f))),
                    Sequence = (byte)instance.Sequence,
                    Index = (byte)instance.Number,
                    Flags = instance.Flags,
                    DirectionX = (int) Math.Round(position.X + Level.SectorSizeUnit * direction.X),
                    DirectionY = (int)-Math.Round(position.Y + Level.SectorSizeUnit * direction.Y),
                    DirectionZ = (int) Math.Round(position.Z + Level.SectorSizeUnit * direction.Z),
                });
            }
            _flyByCameras.Sort(new tr4_flyby_camera.ComparerFlyBy());

            // Check camera duplicates
            int lastSeq   = -1;
            int lastIndex = -1;

            for (int i = 0; i < _flyByCameras.Count; i++)
            {
                if(_flyByCameras[i].Sequence != lastSeq)
                {
                    lastSeq = _flyByCameras[i].Sequence;
                    lastIndex = -1;
                }

                if (_flyByCameras[i].Index == lastIndex && _flyByCameras[i].Sequence == lastSeq)
                    _progressReporter.ReportWarn("Warning: flyby sequence " + _flyByCameras[i].Sequence + " has duplicated camera with ID " + lastIndex);
                lastIndex = _flyByCameras[i].Index;
            }

            ReportProgress(47, "    Number of cameras: " + _cameraTable.Count);
            ReportProgress(47, "    Number of flyby cameras: " + _flyByCameras.Count);
            ReportProgress(47, "    Number of sinks: " + _sinkTable.Count);
        }

        private static tr_room_sector GetSector(tr_room room, int x, int z)
        {
            return room.Sectors[room.NumZSectors * x + z];
        }

        private static void SaveSector(tr_room room, int x, int z, tr_room_sector sector)
        {
            room.Sectors[room.NumZSectors * x + z] = sector;
        }

        private void GetAllReachableRooms()
        {
            foreach (var room in _sortedRooms.Where(r => r != null))
                _tempRooms[room].ReachableRooms = new List<Room>();

            foreach (var room in _sortedRooms.Where(r => r != null))
            {
                GetAllReachableRoomsUp(room, room);
                GetAllReachableRoomsDown(room, room);
            }
        }

        private void GetAllReachableRoomsUp(Room baseRoom, Room currentRoom)
        {
            // Wall portals
            foreach (var p in currentRoom.Portals)
            {
                if (p.Direction == PortalDirection.Floor || p.Direction == PortalDirection.Ceiling)
                    continue;

                var tempRoom = _tempRooms[baseRoom];
                if (!tempRoom.ReachableRooms.Contains(p.AdjoiningRoom))
                    tempRoom.ReachableRooms.Add(p.AdjoiningRoom);
            }

            // Ceiling portals
            foreach (var p in currentRoom.Portals)
            {
                if (p.Direction != PortalDirection.Ceiling)
                    continue;

                var tempRoom = _tempRooms[baseRoom];
                if (tempRoom.ReachableRooms.Contains(p.AdjoiningRoom))
                    continue;

                tempRoom.ReachableRooms.Add(p.AdjoiningRoom);
                GetAllReachableRoomsUp(baseRoom, p.AdjoiningRoom);
            }
        }

        private void GetAllReachableRoomsDown(Room baseRoom, Room currentRoom)
        {
            // portali laterali
            foreach (var p in currentRoom.Portals)
            {
                if (p.Direction == PortalDirection.Floor || p.Direction == PortalDirection.Ceiling)
                    continue;

                var tempRoom = _tempRooms[baseRoom];
                if (!tempRoom.ReachableRooms.Contains(p.AdjoiningRoom))
                    tempRoom.ReachableRooms.Add(p.AdjoiningRoom);
            }

            foreach (var p in currentRoom.Portals)
            {
                if (p.Direction != PortalDirection.Floor)
                    continue;

                var tempRoom = _tempRooms[baseRoom];
                if (tempRoom.ReachableRooms.Contains(p.AdjoiningRoom))
                    continue;

                tempRoom.ReachableRooms.Add(p.AdjoiningRoom);
                GetAllReachableRoomsDown(baseRoom, p.AdjoiningRoom);
            }
        }

        private void FindBottomFloor(ref Room room, ref int x, ref int z)
        {
            while (room.GetFloorRoomConnectionInfo(new VectorInt2(x, z)).TraversableType == Room.RoomConnectionType.FullPortal)
            {
                var sector = room.Sectors[x, z];
                x += room.Position.X - sector.FloorPortal.AdjoiningRoom.Position.X;
                z += room.Position.Z - sector.FloorPortal.AdjoiningRoom.Position.Z;
                room = sector.FloorPortal.AdjoiningRoom;
            }
        }

        private bool FindMonkeyFloor(Room room, int x, int z)
        {
            FindBottomFloor(ref room, ref x, ref z);
            return room.Sectors[x, z].HasFlag(SectorFlags.Monkey);
        }

        private void PrepareItems()
        {
            bool isNewTR = _level.Settings.GameVersion.Native() > TRVersion.Game.TR3;

            ReportProgress(42, "Building items table");

            _moveablesTable = new Dictionary<MoveableInstance, int>(new ReferenceEqualityComparer<MoveableInstance>());
            _aiObjectsTable = new Dictionary<MoveableInstance, int>(new ReferenceEqualityComparer<MoveableInstance>());

            bool laraPlaced = false;

            foreach (var room in _sortedRooms.Where(room => room != null))
                foreach (var instance in room.Objects.OfType<MoveableInstance>())
                {
                    var wadMoveable = _level.Settings.WadTryGetMoveable(instance.WadObjectId);
                    if (wadMoveable == null)
                    {
                        _progressReporter.ReportWarn("Moveable '" + instance + "' was not included in the level because it is missing the *.wad file.");
                        continue;
                    }

                    // Keep track on Lara count, since more than 1 Lara causes engine to crash.
                    if (wadMoveable.Id.TypeId == 0)
                    {
                        if (laraPlaced)
                        {
                            _progressReporter.ReportWarn("Extra Lara was found and removed from room " + instance.Room + " to prevent crashes. Please use only one Lara in level.");
                            continue;
                        }
                        else
                            laraPlaced = true;
                    }

                    Vector3 position = instance.Room.WorldPos + instance.Position;
                    double angle = Math.Round(instance.RotationY * (65536.0 / 360.0));
                    ushort angleInt = unchecked((ushort)Math.Max(0, Math.Min(ushort.MaxValue, angle)));

                    // Split AI objects and normal objects (only for TR4+)
                    if (isNewTR && TrCatalog.IsMoveableAI(_level.Settings.GameVersion, wadMoveable.Id.TypeId))
                    {
                        _aiItems.Add(new tr_ai_item
                        {
                            X = (int)Math.Round(position.X),
                            Y = (int)-Math.Round(position.Y),
                            Z = (int)Math.Round(position.Z),
                            ObjectID = checked((ushort)instance.WadObjectId.TypeId),
                            Room = (ushort)_roomRemapping[instance.Room],
                            Angle = angleInt,
                            OCB = instance.Ocb,
                            Flags = (ushort)(instance.CodeBits << 1)
                        });
                        _aiObjectsTable.Add(instance, _aiObjectsTable.Count);
                    }
                    else
                    {
                        int flags = (instance.CodeBits << 9) | (instance.ClearBody ? 0x80 : 0) | (instance.Invisible ? 0x100 : 0);

                        var instanceColor = instance.Color;

                        // HACK: in TR3+, moveables have RGB components swapped to BGR
                        if (_level.Settings.GameVersion.Native() > TRVersion.Game.TR2)
                            instanceColor = new Vector3(instance.Color.Z, instance.Color.Y, instance.Color.X);

                        // HACK: original tom2pc/winroomedit compiler forced tint to be reset to 1.0f in case
                        // it is applied to moveable objects with non-static lighting.
                        if (!instance.CanBeColored())
                            instanceColor = Vector3.One;

                        // Calculate TR color
                        ushort color = instanceColor.Equals(Vector3.One) ? (ushort)0xFFFF : PackLightColor(instanceColor, _level.Settings).Item2;

                        // Substitute ID is needed to convert visible menu items to pick-up sprites in TR1-2
                        var realID = TrCatalog.GetSubstituteID(_level.Settings.GameVersion, instance.WadObjectId.TypeId);

                        _items.Add(new tr_item
                        {
                            X = (int)Math.Round(position.X),
                            Y = (int)-Math.Round(position.Y),
                            Z = (int)Math.Round(position.Z),
                            ObjectID = checked((ushort)realID),
                            Room = (short)_roomRemapping[instance.Room],
                            Angle = angleInt,
                            Intensity1 = color,
                            Ocb = isNewTR ? instance.Ocb : unchecked((short)color),
                            Flags = unchecked((ushort)flags)
                        });
                        _moveablesTable.Add(instance, _moveablesTable.Count);
                    }
                }

            // Sort AI objects and put all LARA_START_POS objects (last AI object by ID) in front
            if (_level.Settings.GameVersion.Native() > TRVersion.Game.TR3)
            {
                _aiItems = _aiItems.OrderByDescending(item => item.ObjectID).ThenBy(item => item.OCB).ToList();
                ReportProgress(45, "    Number of AI objects: " + _aiItems.Count);
            }

            ReportProgress(45, "    Number of items: " + _items.Count);

            int maxSafeItemCount = _limits[Limit.ItemSafeCount];
            int maxItemCount     = _limits[Limit.ItemMaxCount];

            if (_items.Count >= maxItemCount)
                _progressReporter.ReportWarn("Level has more than " + maxItemCount + " moveables. This may lead to crash.");

            if (_items.Count >= maxSafeItemCount)
                _progressReporter.ReportWarn("Moveable count is beyond " + maxSafeItemCount + ", which may lead to savegame handling issues.");
        }

        public string DetectTRNGVersion()
        {
            var buffer = PathC.GetDirectoryNameTry(_level.Settings.MakeAbsolute(_level.Settings.GameExecutableFilePath)) + "\\Tomb_NextGeneration.dll";
            if (File.Exists(buffer))
            {
                buffer = (FileVersionInfo.GetVersionInfo(buffer)).ProductVersion.Replace(",", ".").Replace(" ", string.Empty);
                _progressReporter.ReportInfo("TRNG found, version is " + buffer);
            }
            else
            {
                buffer = "1.3.0.6";
                _progressReporter.ReportWarn("Tomb_NextGeneration.dll wasn't found in game directory. Probably you're using TRNG target on vanilla TR4/TRLE?");
            }

            return buffer;
        }

        public List<tr_cinematicFrame> GetCinematicFrames()
        {
            var result = new List<tr_cinematicFrame>();

            var allObjects = _level.GetAllObjects().OfType<PositionBasedObjectInstance>().ToList();
            if (allObjects.Count == 0)
                return result;

            var allFlybys = allObjects.OfType<FlybyCameraInstance>().OrderBy(f => f.Sequence).ThenBy(f => f.Number).ToList();
            if (allFlybys.Count == 0)
                return result;

            var lara = _level.Settings.WadTryGetMoveable(WadMoveableId.Lara);
            if (lara == null)
                return result;

            var laraItem = allObjects.FirstOrDefault(obj => obj is ItemInstance &&
                                    ((ItemInstance)obj).ItemType == new ItemType(WadMoveableId.Lara));
            if (laraItem == null)
                return result;

            var origin = laraItem.WorldPosition;
            var rotationOffset = -(laraItem as IRotateableY).RotationY * Math.PI / 180.0f;
            var sin = (float)Math.Sin(-rotationOffset);
            var cos = (float)Math.Cos(-rotationOffset);

            var groups = allFlybys.GroupBy(f => f.Sequence).ToList();

            _progressReporter.ReportInfo("Converting " + groups.Count + " flyby sequence" + (groups.Count == 1 ? "" : "s") + " to cinematic frames");

            foreach (var flybys in groups)
            {
                var settings = flybys.Select(f => new Vector3(f.Roll, f.Fov, f.Speed)).ToList();

                var positions = flybys.Select(f =>
                {
                    var distance = f.WorldPosition - laraItem.WorldPosition;
                    var x = distance.X * cos - distance.Z * sin + laraItem.WorldPosition.X;
                    var z = distance.X * sin + distance.Z * cos + laraItem.WorldPosition.Z;

                    return new Vector3(x, f.WorldPosition.Y, z);
                }
                ).ToList();

                var targets = flybys.Select(f =>
                {
                    var mxR = Matrix4x4.CreateFromYawPitchRoll(f.GetRotationYRadians(), -f.GetRotationXRadians(), f.GetRotationRollRadians());
                    var mxT = Matrix4x4.CreateTranslation(0, 0, Level.SectorSizeUnit);
                    var trans = f.WorldPosition + (mxT * mxR).Translation;

                    var distance = trans - laraItem.WorldPosition;
                    var x = distance.X * cos - distance.Z * sin + laraItem.WorldPosition.X;
                    var z = distance.X * sin + distance.Z * cos + laraItem.WorldPosition.Z;

                    return new Vector3(x , trans.Y, z);
                }
                ).ToList();

                const float minFlybySpeed = 0.01f;
                const float maxFlybySpeed = 100.0f;
                const float framesPerUnit = 60.0f;

                var grain = (int)Math.Round((framesPerUnit / minFlybySpeed)) * positions.Count;

                var cPositions = Spline.Calculate(positions, grain);
                var cTargets   = Spline.Calculate(targets,   grain);
                var cSettings  = Spline.Calculate(settings,  grain);

                var currentCamera = 0;
                for (float pos = 0.0f; ;)
                {
                    var i = (int)Math.Round(pos);
                    if (i >= cPositions.Count)
                        break;

                    var frame = new tr_cinematicFrame()
                    {
                        TargetX = (short) (cTargets[i].X   - origin.X),
                        TargetY = (short)-(cTargets[i].Y   - origin.Y),
                        TargetZ = (short) (cTargets[i].Z   - origin.Z),
                        PosX    = (short) (cPositions[i].X - origin.X),
                        PosY    = (short)-(cPositions[i].Y - origin.Y),
                        PosZ    = (short) (cPositions[i].Z - origin.Z),
                        Fov     = (ushort)(cSettings[i].Y * 100),
                        Roll    = (ushort)Math.Max(0, Math.Min(ushort.MaxValue,
                                          Math.Round((cSettings[i].X) * (65536.0 / 360.0))))
                    };

                    var nextCamera = MathC.Clamp(i / (grain / positions.Count), 0, positions.Count - 1);
                    if (nextCamera > currentCamera)
                    {
                        var flyby = flybys.ElementAt(nextCamera);
                        if (flyby.Timer > 0)
                            for (int t = 0; t < flyby.Timer; t++)
                                result.Add(frame);

                        currentCamera = nextCamera;
                    }

                    result.Add(frame);
                    pos += MathC.Clamp(cSettings[i].Z, minFlybySpeed, maxFlybySpeed) * maxFlybySpeed;
                }
            }

            _progressReporter.ReportInfo("    Num cinematic frames: " + result.Count);

            return result;
        }
    }
}
