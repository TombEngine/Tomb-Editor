using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Wad.Catalog
{
    public enum Limit
    {
        TexPages,
        TexPageSize,
        TexInfos,
        SoundBitsPerSample,
        SoundSampleRate,
        SoundSampleSize,
        SoundSampleCount,
        SoundMapSize,
        ItemMaxCount,
        ItemSafeCount,
        BoxLimit,
        BoxMinCount,
        OverlapLimit,
        RoomVertexCount,
        RoomFaceCount,
        RoomLightCount,
        RoomDimensions,
        RoomMaxCount,
        RoomSafeCount,
        FloorHeight,
        CornerHeight,
        WorldDimensions,

        NG_SoundMapSize
    }

    public class TrCatalog
    {
        private struct Item
        {
            public List<string> Names { get; set; }
            public string Description { get; set; }
            public string TombEngineSlot { get; set; }
            public uint SkinId { get; set; }
            public int SubstituteId { get; set; }
            public bool AIObject { get; set; }
            public bool Shatterable { get; set; }
            public bool FreeRotation { get; set; }
            public bool IsHidden { get; set; }
            public bool IsEssential { get; set; }
        }

        private struct ItemSound
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool FixedByDefault { get; set; }
            public int TombEngineSlot { get; set; }
        }

        private struct ItemAnimation
        {
            public uint Item { get; set; }
            public uint Animation { get; set; }
            public string Name { get; set; }
        }

        private struct ItemState
        {
            public uint Item { get; set; }
            public uint State { get; set; }
            public string Name { get; set; }
        }

        private class Game
        {
            internal TRVersion.Game Version { get; private set; }
            internal SortedList<Limit, int> Limits { get; private set; } = new SortedList<Limit, int>();
            internal SortedList<uint, Item> Moveables { get; private set; } = new SortedList<uint, Item>();
            internal SortedList<uint, Item> SpriteSequences { get; private set; } = new SortedList<uint, Item>();
            internal SortedList<uint, Item> Statics { get; private set; } = new SortedList<uint, Item>();
            internal SortedList<uint, ItemSound> Sounds { get; private set; } = new SortedList<uint, ItemSound>();
            internal List<ItemAnimation> Animations { get; private set; } = new List<ItemAnimation>();
            internal List<ItemState> States { get; private set; } = new List<ItemState>();

            public Game(TRVersion.Game version)
            {
                Version = version;
            }
        }

        private static readonly Dictionary<TRVersion.Game, Game> Games = new Dictionary<TRVersion.Game, Game>();

        public static int PredictSoundMapSize(TRVersion.Game version, bool IsNg, int numDemoData)
        {
            if (version == TRVersion.Game.TR4 && IsNg && numDemoData != 0)
            {
                return numDemoData;
            }
            else
            {
                return GetLimit(version.Native(), Limit.SoundMapSize);
            }
        }

        private static Item? GetMoveable(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return null;

            Item entry;
            if (!game.Moveables.TryGetValue(id, out entry))
                return null;

            return entry;
        }

        public static string GetMoveableName(TRVersion.Game version, uint id) => GetMoveable(version, id)?.Names.LastOrDefault() ?? "Moveable #" + id;
        public static string GetMoveableTombEngineSlot(TRVersion.Game version, uint id) => GetMoveable(version, id)?.TombEngineSlot ?? string.Empty;
        public static uint GetMoveableSkin(TRVersion.Game version, uint id) => GetMoveable(version, id)?.SkinId ?? id;
        public static bool IsMoveableAI(TRVersion.Game version, uint id) => GetMoveable(version, id)?.AIObject ?? false;
        public static bool IsHidden(TRVersion.Game version, uint id) => GetMoveable(version, id)?.IsHidden ?? false;
        public static bool IsEssential(TRVersion.Game version, uint id) => GetMoveable(version, id)?.IsEssential ?? false;
        public static bool IsFreelyRotateable(TRVersion.Game version, uint id) => GetMoveable(version, id)?.FreeRotation ?? false;

        public static string GetSpriteSequenceTombEngineSlot(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return string.Empty;

            Item entry;
            if (!game.SpriteSequences.TryGetValue(id, out entry))
                return string.Empty;

            return game.SpriteSequences[id].TombEngineSlot;
        }

        public static uint GetTombEngineSound(TRVersion.Game version, uint id)
        {
            // In the new soundmap, first 370 sound slots are the same of TR4 for compatibility
            if ((version == TRVersion.Game.TR4 || version == TRVersion.Game.TRNG) && id < 370)
                return id;

            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                throw new Exception("Game not found");

            ItemSound entry;
            if (!game.Sounds.TryGetValue(id, out entry))
                throw new Exception("Sound ID not found");

            var newSlot = game.Sounds[id].TombEngineSlot;
            return newSlot == -1 ? id : (uint)game.Sounds[id].TombEngineSlot;
        }

        public static uint GetSubstituteID(TRVersion.Game version, uint id)
        {
            var mov = GetMoveable(version, id);
            if (mov == null || mov.Value.SubstituteId == -1)
                return id;
            else
                return (uint)mov.Value.SubstituteId;
        }

        public static string GetStaticName(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return "Static #" + id;

            Item entry;
            if (!game.Statics.TryGetValue(id, out entry))
                return "Static #" + id;

            return game.Statics[id].Names.LastOrDefault();
        }

        public static bool IsStaticShatterable(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return false;

            Item entry;
            if (!game.Statics.TryGetValue(id, out entry))
                return false;

            return entry.Shatterable;
        }

        public static uint? GetItemIndex(TRVersion.Game version, string name, out bool isMoveable)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
            {
                isMoveable = false;
                return null;
            }

            KeyValuePair<uint, Item> entry = game.Moveables.FirstOrDefault(item => item.Value.Names.Any(possibleName => possibleName.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
            if (entry.Value.Names != null)
            {
                isMoveable = true;
                return entry.Key;
            }

            entry = game.Statics.FirstOrDefault(item => item.Value.Names.Any(possibleName => possibleName.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
            if (entry.Value.Names != null)
            {
                isMoveable = false;
                return entry.Key;
            }

            entry = game.SpriteSequences.FirstOrDefault(item => item.Value.Names.Any(possibleName => possibleName.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
            if (entry.Value.Names != null)
            {
                isMoveable = false;
                return entry.Key;
            }

            isMoveable = false;
            return null;
        }

        public static string GetOriginalSoundName(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return "UNKNOWN_SOUND_" + id;

            ItemSound entry;
            if (!game.Sounds.TryGetValue(id, out entry))
                return "UNKNOWN_SOUND_" + id;

            return game.Sounds[id].Name;
        }

        public static int TryGetSoundInfoIdByDescription(TRVersion.Game version, string name)
        {
            SortedList<uint, ItemSound> sounds = Games[version.Native()].Sounds;
            foreach (KeyValuePair<uint, ItemSound> pair in sounds)
            {
                if (pair.Value.Description.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (int)pair.Key;
                }
            }

            return -1;
        }

        public static string GetSpriteSequenceName(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return "Sprite sequence #" + id;

            Item entry;
            if (!game.SpriteSequences.TryGetValue(id, out entry))
                return "Sprite sequence #" + id;

            return game.SpriteSequences[id].Names.LastOrDefault();
        }

        public static bool IsSoundFixedByDefault(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return false;

            ItemSound entry;
            if (!game.Sounds.TryGetValue(id, out entry))
                return false;

            return game.Sounds[id].FixedByDefault;
        }

        public static string GetAnimationName(TRVersion.Game version, uint objectId, uint animId)
        {
            Game game;
            ItemAnimation entry = new ItemAnimation();
            Games.TryGetValue(version.Native(), out game);

            if (game != null)
            {
                entry = game.Animations.FirstOrDefault(item => item.Item == objectId && item.Animation == animId);
            }

            if (entry.Name == null)
            {
                List<KeyValuePair<TRVersion.Game, Game>> otherGames = Games.Where(g => g.Key <= version.Native()).ToList();
                otherGames.Reverse();

                foreach (KeyValuePair<TRVersion.Game, Game> otherGame in otherGames)
                {
                    entry = otherGame.Value.Animations.FirstOrDefault(item => item.Item == objectId && item.Animation == animId);
                    if (entry.Name != null)
                        break;
                }
            }

            if (entry.Name == null)
                return "Animation " + animId;
            else
                return entry.Name;
        }

        public static string GetStateName(TRVersion.Game version, uint objectId, uint stateId)
        {
            Game game;
            ItemState entry = new ItemState();
            Games.TryGetValue(version.Native(), out game);

            if (game != null)
                entry = game.States.FirstOrDefault(item => item.Item == objectId && item.State == stateId);

            if (entry.Name == null)
            {
                List<KeyValuePair<TRVersion.Game, Game>> otherGames = Games.Where(g => g.Key <= version.Native()).ToList();
                otherGames.Reverse();

                foreach (KeyValuePair<TRVersion.Game, Game> otherGame in otherGames)
                {
                    entry = otherGame.Value.States.FirstOrDefault(item => item.Item == objectId && item.State == stateId);
                    if (entry.Name != null)
                        break;
                }
            }

            if (entry.Name == null)
                return "Unknown state " + stateId;
            else
                return entry.Name;
        }

        public static int TryToGetStateID(TRVersion.Game version, uint objectId, string stateName)
        {
            Game game;
            ItemState entry = new ItemState();
            Games.TryGetValue(version.Native(), out game);

            foreach (bool precise in new[] { true, false })
            {
                if (game != null)
                {
                    var sortedStates = game.States.Where(item => item.Item == objectId).OrderBy(item => item.State);

                    if (precise)
                        entry = sortedStates.FirstOrDefault(item => item.Name.ToLower().Equals(stateName.ToLower()));
                    else
                        entry = sortedStates.FirstOrDefault(item => item.Name.ToLower().Contains(stateName.ToLower()));

                    if (!string.IsNullOrEmpty(entry.Name))
                        break;

                    foreach (KeyValuePair<TRVersion.Game, Game> otherGame in Games.Where(g => g.Key <= version.Native()))
                    {
                        sortedStates = otherGame.Value.States.Where(item => item.Item == objectId).OrderBy(item => item.State);

                        if (precise)
                            entry = sortedStates.FirstOrDefault(item => item.Name.ToLower().Equals(stateName.ToLower()));
                        else
                            entry = sortedStates.FirstOrDefault(item => item.Name.ToLower().Contains(stateName.ToLower()));

                        if (!string.IsNullOrEmpty(entry.Name))
                            break;
                    }

                    if (!string.IsNullOrEmpty(entry.Name))
                        break;
                }
            }

            if (entry.Name == null)
                return -1;
            else
                return (int)entry.State;
        }

        public static int GetLimit(TRVersion.Game version, Limit limit)
        {
            Game game;
            int value = int.MinValue;
            Games.TryGetValue(version.Native(), out game);

            if (game != null)
                value = game.Limits.FirstOrDefault(item => item.Key == limit).Value;

            if (value == int.MinValue)
            {
                List<KeyValuePair<TRVersion.Game, Game>> otherGames = Games.Where(g => g.Key <= version.Native()).ToList();
                otherGames.Reverse();

                foreach (KeyValuePair<TRVersion.Game, Game> otherGame in otherGames)
                {
                    value = game.Limits.FirstOrDefault(item => item.Key == limit).Value;
                    if (value != int.MinValue)
                        break;
                }
            }

            return value;
        }

        public static IDictionary<uint, string> GetAllMoveables(TRVersion.Game version)
        {
            return Games[version.Native()].Moveables.DicSelect(item => item.Value.Names.LastOrDefault());
        }

        public static IDictionary<uint, string> GetAllStatics(TRVersion.Game version)
        {
            return Games[version.Native()].Statics.DicSelect(item => item.Value.Names.LastOrDefault());
        }

        public static IDictionary<uint, string> GetAllSpriteSequences(TRVersion.Game version)
        {
            return Games[version.Native()].SpriteSequences.DicSelect(item => item.Value.Names.LastOrDefault());
        }

        public static IDictionary<uint, string> GetAllSounds(TRVersion.Game version)
        {
            return Games[version.Native()].Sounds.DicSelect(item => item.Value.Name);
        }

        public static IDictionary<uint, string> GetAllFixedByDefaultSounds(TRVersion.Game version)
        {
            return Games[version.Native()].Sounds
                .DicWhere(sound => sound.Value.FixedByDefault)
                .DicSelect(item => item.Value.Name);
        }

        public static string GetVersionString(TRVersion.Game version)
        {
            switch (version)
            {
                case TRVersion.Game.TR1:
                    return "Tomb Raider";
                case TRVersion.Game.TR1X:
                    return "Tomb Raider (TR1X)";
                case TRVersion.Game.TR2:
                    return "Tomb Raider 2";
                case TRVersion.Game.TR2X:
                    return "Tomb Raider 2 (TR2X)";
                case TRVersion.Game.TR3:
                    return "Tomb Raider 3";
                case TRVersion.Game.TR4:
                    return "Tomb Raider 4";
                case TRVersion.Game.TRNG:
                    return "TRNG";
                case TRVersion.Game.TR5:
                    return "Tomb Raider 5";
                case TRVersion.Game.TombEngine:
                    return "TombEngine";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static XmlDocument CombineCatalogs(string rootFolder)
        {
			XmlDocument combinedCatalog = new XmlDocument();
            combinedCatalog.AppendChild(combinedCatalog.CreateXmlDeclaration("1.0", "UTF-8",null));
            combinedCatalog.AppendChild(combinedCatalog.CreateElement("trcatalog"));
			foreach (var gameString in TRVersion.NativeVersions.Select(v => v.Native().ToString()))
			{
				var gameCatalogFolder = Path.Combine(rootFolder, gameString);
                if (!Directory.Exists(gameCatalogFolder))
                    continue;
                var newGameNode = combinedCatalog.CreateElement("game");
				newGameNode.SetAttribute("id", gameString);
				var gameCatalogFragments = Directory.EnumerateFiles(gameCatalogFolder, "*.xml");
				foreach (var catalog in gameCatalogFragments)
				{
					XmlDocument catalogFragment = new XmlDocument();
					catalogFragment.Load(catalog);
					foreach (XmlNode nodeToImport in catalogFragment.DocumentElement.SelectNodes("/*"))
					{
						var importedNode = combinedCatalog.ImportNode(nodeToImport, true);
						newGameNode.AppendChild(importedNode);

					}
				}
				combinedCatalog.DocumentElement.AppendChild(newGameNode);
			}
			return combinedCatalog;
		}

        public static void LoadCatalog(string rootFolder)
        {
            XmlDocument document = CombineCatalogs(rootFolder);

            XmlNodeList gamesNodes = document.DocumentElement.SelectNodes("/game");
            foreach (XmlNode gameNode in document.DocumentElement.ChildNodes)
            {
                if (gameNode.Name != "game")
                    continue;

                var stringVersion = gameNode.Attributes["id"].Value;
                var version = Enum.GetValues(typeof(TRVersion.Game)).Cast<TRVersion.Game>().First(s => stringVersion == s.ToString());
                var game = new Game(version);

                // Parse limits
                XmlNode limits = gameNode.SelectSingleNode("limits");
                if (limits != null)
                {
                    string[] names = Enum.GetNames(typeof(Limit));

                    foreach (XmlNode limitNode in limits.ChildNodes)
                    {
                        if (limitNode.Name != "limit")
                            continue;

                        string name = limitNode.Attributes["name"]?.Value ?? string.Empty;
                        int value = int.Parse(limitNode.Attributes["value"]?.Value ?? int.MinValue.ToString());

                        if (string.IsNullOrEmpty(name) || !names.Any(n => n == name) || value == int.MinValue)
                            continue;

                        game.Limits.Add((Limit)Enum.Parse(typeof(Limit), name), value);
                    }
                }

                // Parse moveables
                XmlNode moveables = gameNode.SelectSingleNode("moveables");
                if (moveables != null)
                {
                    foreach (XmlNode moveableNode in moveables.ChildNodes)
                    {
                        if (moveableNode.Name != "moveable")
                            continue;

                        uint id = uint.Parse(moveableNode.Attributes["id"]?.Value ?? 0.ToString());
                        string[] names = (moveableNode.Attributes["name"]?.Value ?? string.Empty).Split('|');

                        uint skinId = uint.Parse(moveableNode.Attributes["use_body_from"]?.Value ?? id.ToString());
                        int substituteId = int.Parse(moveableNode.Attributes["id2"]?.Value ?? "-1");
                        bool isAI = bool.Parse(moveableNode.Attributes["ai"]?.Value ?? "false");
                        bool isFreeRotation = bool.Parse(moveableNode.Attributes["freeRot"]?.Value ?? "false");
                        bool hidden = bool.Parse(moveableNode.Attributes["hidden"]?.Value ?? "false");
                        bool essential = bool.Parse(moveableNode.Attributes["essential"]?.Value ?? "true");
                        string tombEngineSlot = moveableNode.Attributes["ten"]?.Value ?? string.Empty;

                        game.Moveables.Add(id, new Item
                        {
                            Names = new List<string>(names),
                            SkinId = skinId,
                            SubstituteId = substituteId,
                            AIObject = isAI,
                            TombEngineSlot = tombEngineSlot,
                            FreeRotation = isFreeRotation,
                            IsHidden = hidden,
                            IsEssential = essential
                        });
                    }
                }

                // Parse statics
                XmlNode statics = gameNode.SelectSingleNode("statics");
                if (statics != null)
                {
                    foreach (XmlNode staticNode in statics.ChildNodes)
                    {
                        if (staticNode.Name != "static")
                            continue;

                        uint id = uint.Parse(staticNode.Attributes["id"].Value);
                        string[] names = (staticNode.Attributes["name"]?.Value ?? "").Split('|');
                        bool shatter = bool.Parse(staticNode.Attributes["shatter"]?.Value ?? "false");
                        game.Statics.Add(id, new Item { Names = new List<string>(names), Shatterable = shatter });
                    }
                }

                // Parse sounds
                XmlNode sounds = gameNode.SelectSingleNode("sounds");
                if (sounds != null)
                {
                    foreach (XmlNode soundNode in sounds.ChildNodes)
                    {
                        if (soundNode.Name != "sound")
                            continue;

                        uint id = uint.Parse(soundNode.Attributes["id"].Value);
                        string name = soundNode.Attributes["name"]?.Value ?? "";
                        string description = soundNode.Attributes["description"]?.Value ?? "";
                        int tombEngineSoundId = int.Parse(soundNode.Attributes["ten"]?.Value ?? "-1");
                        bool fixedByDefault = bool.Parse(soundNode.Attributes["hardcoded"]?.Value ?? "false");
                        game.Sounds.Add(id, new ItemSound { Name = name, FixedByDefault = fixedByDefault, Description = description, TombEngineSlot = tombEngineSoundId });
                    }
                }

                // Parse sprite sequences
                XmlNode spriteSequences = gameNode.SelectSingleNode("sprite_sequences");
                if (spriteSequences != null)
                {
                    foreach (XmlNode spriteSequenceNode in spriteSequences.ChildNodes)
                    {
                        if (spriteSequenceNode.Name != "sprite_sequence")
                            continue;

                        string tombEngineSlot = spriteSequenceNode.Attributes["ten"]?.Value ?? string.Empty;
                        uint id = uint.Parse(spriteSequenceNode.Attributes["id"].Value);
                        string[] names = (spriteSequenceNode.Attributes["name"]?.Value ?? "").Split('|');
                        game.SpriteSequences.Add(id, new Item { Names = new List<string>(names), TombEngineSlot = tombEngineSlot });
                    }
                }

                // Parse animations
                XmlNode animations = gameNode.SelectSingleNode("animations");
                if (animations != null)
                {
                    foreach (XmlNode originalNameNode in animations.ChildNodes)
                    {
                        if (originalNameNode.Name != "anim")
                            continue;

                        uint item = uint.Parse(originalNameNode.Attributes["item"].Value);
                        uint id = uint.Parse(originalNameNode.Attributes["id"].Value);
                        string name = originalNameNode.Attributes["name"].Value;

                        game.Animations.Add(new ItemAnimation { Name = name, Animation = id, Item = item });
                    }
                }

                // Parse states
                XmlNode states = gameNode.SelectSingleNode("states");
                if (states != null)
                {
                    foreach (XmlNode originalNameNode in states.ChildNodes)
                    {
                        if (originalNameNode.Name != "state")
                            continue;

                        uint item = uint.Parse(originalNameNode.Attributes["item"].Value);
                        uint id = uint.Parse(originalNameNode.Attributes["id"].Value);
                        string name = originalNameNode.Attributes["name"].Value;

                        game.States.Add(new ItemState { Name = name, State = id, Item = item });
                    }
                }

                Games.Add(version, game);
            }
        }
    }
}
