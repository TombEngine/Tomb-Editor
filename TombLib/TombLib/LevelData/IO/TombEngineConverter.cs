using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.IO
{
    public static class TombEngineConverter
    {
        public static readonly string ReferenceWadPath = Path.Combine(DefaultPaths.ProgramDirectory, "Assets", "Wads", "TombEngine.wad2");

        public static string Start(string fileName, IWin32Window owner, IProgressReporter progressReporter, CancellationToken cancelToken)
        {
            if (!File.Exists(fileName))
            {
                progressReporter.ReportWarn("The specified project doesn't exist.");
                return string.Empty;
            }

            progressReporter.ReportInfo("TombEngine Project Converter");
            progressReporter.ReportInfo(" ");

            string newProject = ConvertProject(fileName, progressReporter, cancelToken);

            if (string.IsNullOrEmpty(newProject))
            {
                throw new Exception("There was an error while converting your project.");
            }

            progressReporter.ReportInfo(" ");
            progressReporter.ReportInfo("Project was converted successfully!");
            progressReporter.ReportInfo("Notes about the conversion:");
            progressReporter.ReportInfo(" ");
            progressReporter.ReportInfo("   - Enemies now have MESHSWAP slots (e.g., BADDY2 -> MESHSWAP_BADDY2), use WadTool for remapping these slots.");
            progressReporter.ReportInfo("   - Some slots are not supported and were added as ANIMATING above 16.");
            progressReporter.ReportInfo("   - TEN uses different slots for every sprite (fire, smoke). They were added from a reference Wad2.");
            progressReporter.ReportInfo("   - Soundmap has been updated to the TombEngine map. Sounds are converted from their legacy counterparts automatically");
            progressReporter.ReportInfo("   - TRNG triggers are not supported and won't be compiled.");
            progressReporter.ReportInfo("   - TEN requires new LARA object which was copied from reference Wad2.");
            progressReporter.ReportInfo("   - Weapon anims and holsters objects have changed and were also converted.");


            return newProject;
        }

        public static WadMoveable ConvertMoveable(this WadMoveable moveable, TRVersion.Game sourceVersion, Wad2 refWad, IProgressReporter progressReporter = null)
        {
            if (sourceVersion == TRVersion.Game.TombEngine)
                return moveable;

            string newSlotName = TrCatalog.GetMoveableTombEngineSlot(sourceVersion, moveable.Id.TypeId);

            switch (newSlotName)
            {
                // We need to copy mesh 14 to 7 for {WEAPON}_ANIM for back weapons

                case "SHOTGUN_ANIM":
                case "CROSSBOW_ANIM":
                case "HK_ANIM":
                case "HARPOON_ANIM":
                case "GRENADE_ANIM":
                case "ROCKET_ANIM":
                    {
                        if (moveable.Bones.Count < 15)
                            break;

                        progressReporter?.ReportInfo("    Swapping back holster mesh #14 to mesh #7 for " + newSlotName);

                        var mesh = moveable.Bones[14].Mesh.Clone();
                        var oldMesh = moveable.Bones[7].Mesh.Clone();

                        for (int i = 0; i < mesh.VertexPositions.Count; i++)
                        {
                            var pos = mesh.VertexPositions[i];
                            pos.Y += 200;
                            pos.Z -= 20;
                            mesh.VertexPositions[i] = pos;
                        }
                        mesh.BoundingBox = mesh.CalculateBoundingBox();

                        moveable.Bones[7].Mesh = mesh;
                        moveable.Bones[14].Mesh = oldMesh;
                    }
                    break;

                // For holsters, we need to put holsters meshes in 1 and 4 and apply the same skeleton from LARA

                case "LARA_HOLSTERS":
                case "LARA_HOLSTERS_PISTOLS":
                case "LARA_HOLSTERS_UZIS":
                case "LARA_HOLSTERS_REVOLVER":
                    {
                        if (moveable.Bones.Count <= 8)
                            break;

                        if (!refWad.Moveables.ContainsKey(new WadMoveableId(0)))
                            break;

                        progressReporter?.ReportInfo("    Copying holsters meshes for " + newSlotName);

                        var laraMoveable = refWad.Moveables[new WadMoveableId(0)];
                        var newBones = new List<WadBone>();
                        var newMeshes = new List<WadMesh>();

                        foreach (var oldBone in laraMoveable.Bones)
                        {
                            newBones.Add(oldBone.Clone());
                        }

                        foreach (var oldMesh in laraMoveable.Meshes)
                        {
                            newMeshes.Add(oldMesh.Clone());
                        }

                        newBones[1].Mesh = moveable.Bones[4].Mesh.Clone();
                        newMeshes[1] = newBones[1].Mesh;

                        newBones[4].Mesh = moveable.Bones[8].Mesh.Clone();
                        newMeshes[4] = newBones[4].Mesh;

                        moveable.Bones.Clear();
                        moveable.Bones.AddRange(newBones);
                        moveable.Meshes.Clear();
                        moveable.Meshes.AddRange(newMeshes);
                    }
                    break;

                // Correct pivot points

                case "EXPANDING_PLATFORM":
                    {
                        progressReporter?.ReportInfo("    Adjusting mesh and pivot point for " + newSlotName);

                        for (int m = 0; m < moveable.Bones.Count; m++)
                        {
                            var mesh = moveable.Bones[m].Mesh.Clone();

                            for (int i = 0; i < mesh.VertexPositions.Count; i++)
                            {
                                Vector3 pos = mesh.VertexPositions[i];
                                pos.Z += 512;
                                mesh.VertexPositions[i] = pos;
                            }
                            moveable.Bones[m].Mesh = mesh;
                            moveable.Meshes[m] = mesh;
                        }

                        foreach (var anim in moveable.Animations)
                        {
                            foreach (var frame in anim.KeyFrames)
                            {
                                BoundingBox bb = frame.BoundingBox;
                                bb.Maximum.Z += 512;
                                bb.Minimum.Z += 512;
                                frame.BoundingBox = bb;
                            }
                        }
                    }
                    break;

                // Remove hardcoded TR4 collision box

                case "ANIMATING12":
                case "ANIMATING13":
                case "ANIMATING14":
                case "ANIMATING15":
                case "ANIMATING16":
                case "FIREROPE":
                    {
                        if (sourceVersion != TRVersion.Game.TR4)
                            break;

                        progressReporter?.ReportInfo("    Removing collision for " + newSlotName);

                        foreach (var anim in moveable.Animations)
                        {
                            foreach (var frame in anim.KeyFrames)
                            {
                                frame.BoundingBox = new BoundingBox();
                            }
                        }
                    }
                    break;

                // Add hardcoded TR4 collision box

                case "TURN_SWITCH":
                    {
                        if (sourceVersion != TRVersion.Game.TR4)
                            break;

                        progressReporter?.ReportInfo("    Modifying collision for " + newSlotName);

                        foreach (var anim in moveable.Animations)
                        {
                            foreach (var frame in anim.KeyFrames)
                            {
                                frame.BoundingBox = new BoundingBox(new Vector3(-256, frame.BoundingBox.Minimum.Y, -256),
                                                                    new Vector3(256, frame.BoundingBox.Maximum.Y, 256));
                            }
                        }
                    }
                    break;

                // Remove hardcoded TR5 sphere

                case "LASERHEAD":
                    {
                        if (sourceVersion != TRVersion.Game.TR5)
                            break;

                        progressReporter?.ReportInfo("    Removing collision for " + newSlotName);

                        if (moveable.Meshes.Count > 0)
                            moveable.Meshes[0].BoundingSphere = new BoundingSphere(new Vector3(0, 100, -70), 340);
                    }
                    break;
            }

            // Detect flipped switch states

            if (newSlotName.Contains("SWITCH_TYPE"))
            {
                if (moveable.Animations.Count > 0 &&
                    moveable.Animations.Any(a => a.KeyFrames.Count == 1) &&
                    moveable.Animations.First(a => a.KeyFrames.Count == 1).StateId == 0)
                {
                    progressReporter?.ReportInfo("    Fixing switch state ID order for " + newSlotName);

                    foreach (var anim in moveable.Animations)
                    {
                        if (anim.StateId > 1 || anim.KeyFrames.Count != 1)
                            continue;

                        var newStateId = (ushort)(1 - anim.StateId);

                        foreach (var sc in anim.StateChanges)
                            if (sc.StateId == newStateId)
                                sc.StateId = anim.StateId;

                        anim.StateId = newStateId;
                    }
                }
            }

            // Adjust bridge thickness

            if (newSlotName == "TWOBLOCK_PLATFORM" ||
                newSlotName.Contains("FALLING_BLOCK") ||
                newSlotName.Contains("TRAPDOOR"))
            {
                progressReporter?.ReportInfo("    Adjusting bridge object collision box " + newSlotName);

                if (moveable.Animations.Count > 0)
                {
                    var anim = moveable.Animations[0];
                    for (int f = 0; f < anim.KeyFrames.Count; f++)
                    {
                        var oldBB = anim.KeyFrames[f].BoundingBox;
                        oldBB.Maximum = new Vector3(oldBB.Maximum.X, oldBB.Maximum.Y * 0.8f, oldBB.Maximum.Z);
                        anim.KeyFrames[f].BoundingBox = oldBB;
                    }
                }
            }

            // Fix bridge collisions

            if (newSlotName.Contains("BRIDGE") ||
                newSlotName.Contains("TRAPDOOR") ||
                newSlotName.Contains("RAISING_BLOCK") ||
                newSlotName.Contains("TWOBLOCK_PLATFORM") ||
                (newSlotName.Contains("PUSHABLE") && sourceVersion.Native() <= TRVersion.Game.TR3))
            {
                progressReporter?.ReportInfo("    Adjusting bridge bounds for " + newSlotName);

                if (moveable.Animations.Count > 0)
                {
                    var anim = moveable.Animations[0];
                    for (int f = 0; f < anim.KeyFrames.Count; f++)
                    {
                        var oldBB = anim.KeyFrames[f].BoundingBox;

                        var newMaxX = Math.Sign(oldBB.Maximum.X) * MathC.NearestPowerOf2((int)Math.Abs(oldBB.Maximum.X));
                        var newMaxZ = Math.Sign(oldBB.Maximum.Z) * MathC.NearestPowerOf2((int)Math.Abs(oldBB.Maximum.Z));
                        var newMinX = Math.Sign(oldBB.Minimum.X) * MathC.NearestPowerOf2((int)Math.Abs(oldBB.Minimum.X));
                        var newMinZ = Math.Sign(oldBB.Minimum.Z) * MathC.NearestPowerOf2((int)Math.Abs(oldBB.Minimum.Z));

                        oldBB.Maximum = new Vector3(newMaxX, oldBB.Maximum.Y, newMaxZ);
                        oldBB.Minimum = new Vector3(newMinX, oldBB.Minimum.Y, newMinZ);

                        anim.KeyFrames[f].BoundingBox = oldBB;
                    }
                }
            }

            // Remap sound slots

            foreach (var animation in moveable.Animations)
                foreach (var command in animation.AnimCommands)
                {
                    if (command.Type != WadAnimCommandType.PlaySound)
                        continue;

                    try
                    {
                        command.ConvertLegacyConditions();
                        uint soundId = (uint)command.Parameter2;
                        command.Parameter2 = (short)TrCatalog.GetTombEngineSound(sourceVersion, soundId);
                    }
                    catch (Exception)
                    {
                        // No sound found, just continue
                        continue;
                    }
                }

            return moveable;
        }

        private static string ConvertProject(string source, IProgressReporter progressReporter, CancellationToken cancelToken)
        {
            try
            {
                // Load new TombEngine reference Wad2
                Wad2 referenceWad = Wad2Loader.LoadFromFile(ReferenceWadPath, true);

                // Load level and all related resources
                Level level = Path.GetExtension(source).ToLower() == ".prj" ?
                    PrjLoader.LoadFromPrj(source, string.Empty, true, false, null, cancelToken) : Prj2Loader.LoadFromPrj2(source, null, cancelToken, new Prj2Loader.Settings());

                if (level == null)
                {
                    progressReporter.ReportWarn("Error while loading level.");
                    return string.Empty;
                }

                if (level.Settings.GameVersion is not TRVersion.Game.TR4 and not TRVersion.Game.TRNG)
                {
                    if (level.Settings.GameVersion == TRVersion.Game.TombEngine)
                        progressReporter.ReportWarn("You are trying to convert a project which is already TEN project.");
                    else
                        progressReporter.ReportWarn("Only TR4 and TRNG projects can be converted to TEN at this time.");

                    return string.Empty;
                }

                // Now convert resources to new format
                List<ReferencedWad> newWads = new List<ReferencedWad>();
                Dictionary<uint, uint> remappedSlots = new Dictionary<uint, uint>();

                string newFileName;
                string newPath;
                bool addedTimex = false;

                foreach (ReferencedWad wadRef in level.Settings.Wads)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    Wad2 wad = wadRef.Wad;
                    Wad2 newWad = new Wad2 { GameVersion = TRVersion.Game.TombEngine };

                    // Get base remap object which is ANIMATING33 (first animating which doesn't exist in any legacy engines)
                    bool isMoveable;
                    uint remappedObjectIndex = TrCatalog.GetItemIndex(TRVersion.Game.TombEngine, "ANIMATING33", out isMoveable).Value;

                    // Copy all objects to new wad
                    foreach (KeyValuePair<WadMoveableId, WadMoveable> moveable in wad.Moveables)
                    {
                        uint newSlot;
                        string oldId = TrCatalog.GetMoveableName(TRVersion.Game.TR4, moveable.Key.TypeId);
                        string newId = TrCatalog.GetMoveableTombEngineSlot(TRVersion.Game.TR4, moveable.Key.TypeId);

                        if (string.IsNullOrEmpty(newId))
                        {
                            newSlot = remappedObjectIndex;
                            newId = TrCatalog.GetMoveableName(TRVersion.Game.TombEngine, newSlot);
                            progressReporter.ReportWarn("    Slot " + oldId + " is not supported by TombEngine and it will be remapped to " + newId);
                            remappedObjectIndex++;
                        }
                        else
                        {
                            uint? found = TrCatalog.GetItemIndex(TRVersion.Game.TombEngine, newId, out isMoveable);
                            if (!found.HasValue)
                            {
                                continue;
                            }
                            else
                            {
                                newSlot = found.Value;
                            }
                        }

                        string newSlotName = TrCatalog.GetMoveableName(TRVersion.Game.TombEngine, newSlot);

                        progressReporter.ReportInfo(oldId + " → [" + newSlot + "] " + newSlotName);

                        if (newSlotName == "BINOCULAR_GRAPHICS" || newSlotName == "TARGET_GRAPHICS")
                        {
                            progressReporter.ReportInfo("    Skipping unneeded " + newSlotName + " slot");
                            continue;
                        }

                        moveable.Value.ConvertMoveable(level.Settings.GameVersion, referenceWad, progressReporter);

                        if (!addedTimex &&
                            (newSlotName == "MEMCARD_LOAD_INV_ITEM" || newSlotName == "MEMCARD_SAVE_INV_ITEM" ||
                             newSlotName == "PC_LOAD_INV_ITEM" || newSlotName == "PC_SAVE_INV_ITEM"))
                        {
                            progressReporter.ReportInfo("    Adding STOPWATCH from reference Wad2");

                            uint timexIndex = TrCatalog.GetItemIndex(TRVersion.Game.TombEngine, "STOPWATCH_ITEM", out isMoveable).Value;
                            newWad.Add(new WadMoveableId(timexIndex), referenceWad.Moveables[new WadMoveableId(timexIndex)]);
                            addedTimex = true;
                        }

                        if (newSlotName == "MOTORBIKE")
                        {
                            progressReporter.ReportInfo("    Adding motorbike animations for motorbike");

                            uint animsIndex = TrCatalog.GetItemIndex(TRVersion.Game.TombEngine, "MOTORBIKE_LARA_ANIMS", out isMoveable).Value;
                            newWad.Add(new WadMoveableId(animsIndex), referenceWad.Moveables[new WadMoveableId(animsIndex)]);
                            addedTimex = true;
                        }

                        if (newSlotName == "JEEP")
                        {
                            progressReporter.ReportInfo("    Adding jeep animations for motorbike");

                            uint animsIndex = TrCatalog.GetItemIndex(TRVersion.Game.TombEngine, "JEEP_LARA_ANIMS", out isMoveable).Value;
                            newWad.Add(new WadMoveableId(animsIndex), referenceWad.Moveables[new WadMoveableId(animsIndex)]);
                            addedTimex = true;
                        }

                        if (newSlotName == "LARA")
                        {
                            newWad.Add(new WadMoveableId(0), referenceWad.Moveables[new WadMoveableId(0)]);
                        }
                        else
                        {
                            newWad.Add(new WadMoveableId(newSlot), moveable.Value);
                            if (!addedTimex && newSlotName == "STOPWATCH_ITEM")
                            {
                                addedTimex = true;
                            }
                        }

                        if (!remappedSlots.ContainsKey(moveable.Key.TypeId))
                        {
                            remappedSlots.Add(moveable.Key.TypeId, newSlot);
                        }
                    }

                    // Copy all statics
                    foreach (KeyValuePair<WadStaticId, WadStatic> staticModel in wad.Statics)
                    {
                        newWad.Statics.Add(staticModel.Key, staticModel.Value);
                    }

                    // Copy all sprite sequences
                    foreach (KeyValuePair<WadSpriteSequenceId, WadSpriteSequence> sequence in wad.SpriteSequences)
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        uint newSlot;
                        string oldId = TrCatalog.GetMoveableName(TRVersion.Game.TR4, sequence.Key.TypeId);
                        string newId = TrCatalog.GetMoveableTombEngineSlot(TRVersion.Game.TR4, sequence.Key.TypeId);

                        if (string.IsNullOrEmpty(newId))
                        {
                            newSlot = remappedObjectIndex;
                            remappedObjectIndex++;
                        }
                        else
                        {
                            uint? found = TrCatalog.GetItemIndex(TRVersion.Game.TombEngine, newId, out isMoveable);
                            if (!found.HasValue)
                            {
                                continue;
                            }
                            else
                            {
                                newSlot = found.Value;
                            }
                        }

                        string newSlotName = TrCatalog.GetSpriteSequenceName(TRVersion.Game.TombEngine, newSlot);

                        progressReporter.ReportInfo(oldId + ": → [" + newSlot + "] " + newSlotName);

                        if (newSlotName == "DEFAULT_SPRITES")
                        {
                            progressReporter.ReportInfo("    DEFAULT_SPRITES found, adding new sprites slots from a reference Wad2");

                            // Open TombEngine sprites Wad2
                            foreach (KeyValuePair<WadSpriteSequenceId, WadSpriteSequence> spr in referenceWad.SpriteSequences)
                            {
                                if (!newWad.Contains(spr.Key))
                                {
                                    progressReporter.ReportInfo("    Adding " + TrCatalog.GetSpriteSequenceName(TRVersion.Game.TombEngine, spr.Key.TypeId));
                                    newWad.Add(spr.Key, spr.Value);
                                }
                            }
                        }
                        else
                        {
                            newWad.Add(new WadSpriteSequenceId(newSlot), sequence.Value);
                        }
                    }

                    // Copy all sounds
                    newWad.Sounds = wad.Sounds;

                    // Set comment to indicate that wad was autoconverted
                    newWad.UserNotes = "Autoconverted from " + Path.GetFileName(source);

                    // Save the Wad2 
                    newFileName = Path.GetFileNameWithoutExtension(wad.FileName);
                    newPath = Path.Combine(
                        Path.GetDirectoryName(source),
                        Path.GetFileNameWithoutExtension(wadRef.Path) + "_TombEngine.wad2");

                    progressReporter.ReportInfo("Saving " + wadRef.Path + " to " + newPath);

                    Wad2Writer.SaveToFile(newWad, newPath);

                    newWads.Add(new ReferencedWad(
                        level.Settings,
                        level.Settings.MakeRelative(newPath, VariableType.LevelDirectory)
                        ));
                }

                level.Settings.Wads.Clear();
                level.Settings.Wads.AddRange(newWads);

                foreach (Room room in level.Rooms)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    if (room != null)
                    {
                        foreach (var instance in room.Objects)
                        {
                            if (instance is MoveableInstance)
                            {
                                var mov = instance as MoveableInstance;

                                if (!remappedSlots.ContainsKey(mov.WadObjectId.TypeId))
                                {
                                    progressReporter.ReportWarn("Slot not found! " + mov.WadObjectId.TypeId);
                                }
                                else
                                {
                                    mov.WadObjectId = new WadMoveableId(remappedSlots[mov.WadObjectId.TypeId]);
                                }

                                // Untint dynamically lit moveables
                                var model = level.Settings.WadTryGetMoveable(mov.WadObjectId);
                                if (model != null && model.Meshes.Any(m => m.LightingType == WadMeshLightingType.Normals))
                                {
                                    mov.Color = Vector3.One;
                                }

                                // Convert legacy teeth spikes OCBs to real rotations
                                if (mov.WadObjectId.ToString(TRVersion.Game.TombEngine).Contains("TEETH_SPIKES"))
                                {
                                    var SPyoffs = new List<short> { -1024, 0, -512, 0, 0, 0, -512, 0 };
                                    var SPxzoffs = new List<short> { 0, 0, 512, 0, 0, 0, -512, 0 };
                                    var SPDETyoffs = new List<short> { 1024, 512, 512, 512, 0, 512, 512, 512 };
                                    var rotations = new List<float> { 180.0f, 225.0f, 270.0f, 315.0f, 0.0f, 45.0f, 90.0f, 135.0f };

                                    int angle = mov.Ocb & 7;
                                    (mov as IRotateableYXRoll).RotationY = 0.0f;

                                    if ((mov.Ocb & 8) != 0)
                                    {
                                        (mov as IRotateableYXRoll).RotationX = rotations[angle];
                                        mov.Position -= new Vector3(0.0f, 0.0f, SPxzoffs[angle]);
                                    }
                                    else
                                    {
                                        (mov as IRotateableYXRoll).Roll = -rotations[angle];
                                        mov.Position += new Vector3(SPxzoffs[angle], 0.0f, 0.0f);
                                    }

                                    mov.Position -= new Vector3(0.0f, SPyoffs[angle], 0.0f);

                                    if ((mov.Ocb & 16) != 0)
                                        mov.Ocb = 1;
                                    else if ((mov.Ocb & 32) != 0)
                                        mov.Ocb = 2;
                                    else
                                        mov.Ocb = 0;
                                }
                            }

                            if (instance is StaticInstance)
                            {
                                var st = instance as StaticInstance;

                                // Untint dynamically lit statics
                                var model = level.Settings.WadTryGetStatic(st.WadObjectId);
                                if (model != null && model.Mesh.LightingType == WadMeshLightingType.Normals)
                                {
                                    st.Color = Vector3.One;
                                }
                            }

                            if (instance is LightInstance)
                            {
                                var light = instance as LightInstance;

                                if (light.Type == LightType.FogBulb)
                                    light.Intensity /= 5.0f;
                            }

                            if (instance is IHasLuaName)
                                (instance as IHasLuaName).AllocateNewLuaName();
                        }
                    }
                }

                level.Settings.GameVersion = TRVersion.Game.TombEngine;
                level.Settings.ConvertLevelExtension();

                newPath = Path.Combine(
                    Path.GetDirectoryName(source),
                    Path.GetFileNameWithoutExtension(source) + "_TombEngine.prj2");

                Prj2Writer.SaveToPrj2(newPath, level);

                return newPath;
            }
            catch (Exception ex)
            {
                progressReporter.ReportWarn(ex.Message);
                return string.Empty;
            }
        }
    }
}
