using DarkUI.Forms;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using TombLib;
using TombLib.Forms;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.GeometryIO;
using TombLib.GeometryIO.Importers;
using TombLib.Graphics;
using TombLib.IO;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;

namespace WadTool
{
    public static class WadActions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void LoadWad(WadToolClass tool, IWin32Window owner, bool destination, string fileName)
        {
            bool isWad2 = Path.GetExtension(fileName).ToLower() == ".wad2";

            // Load the WAD/Wad2
            Wad2 newWad = null;
            try
            {
                newWad = Wad2.ImportFromFile(fileName, true, new GraphicalDialogHandler(owner), tool.Configuration.Tool_AllowTRNGDecryption);

                if (newWad.HasUnknownData)
                {
                    tool.SendMessage("Loaded wad2 is of newer version.\nSome data was lost. Don't save this wad2 and use newest version of Wad Tool.", PopupType.Warning);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exc)
            {
                logger.Info(exc, "Unable to load " + (destination ? "destination" : "source") + " file from '" + fileName + "'.");
                tool.SendMessage("Loading the file failed! \n" + exc.Message, PopupType.Error);
                return;
            }

            // Set wad
            if (destination)
            {
                tool.DestinationWad = newWad;
                tool.ToggleUnsavedChanges(false);
                AddWadToRecent(fileName);
            }
            else
            {
                if (!isWad2)
                {
                    newWad.FileName = fileName;  // Keep original path only for source old wad, as it's not saveable
                }

                tool.SourceWad = newWad;
            }
        }

        public static void LoadWadOpenFileDialog(WadToolClass tool, IWin32Window owner, bool destination)
        {
            // Open the file dialog
            using (var dialog = new OpenFileDialog())
            {
                string previousFilePath;
                if (destination)
                {
                    previousFilePath = tool.DestinationWad?.FileName;
                }
                else
                {
                    previousFilePath = tool.SourceWad?.FileName;
                }

                if (!string.IsNullOrWhiteSpace(previousFilePath))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(previousFilePath);
                        dialog.FileName = Path.GetFileName(previousFilePath);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                dialog.Filter = Wad2.FileExtensions.GetFilter();
                dialog.Title = "Open " + (destination ? "destination" : "source") + " WAD - Wad2 - Level";
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return;
                }

                LoadWad(tool, owner, destination, dialog.FileName);
            }
        }

        public static bool SaveWad(WadToolClass tool, IWin32Window owner, Wad2 wadToSave, bool ask)
        {
            if (wadToSave == null)
            {
                tool.SendMessage("You have no wad opened. Nothing to save.", PopupType.Warning);
                return false;
            }

            // Figure out the output path
            string outPath = wadToSave.FileName;
            if (!string.IsNullOrWhiteSpace(wadToSave.FileName))
            {
                try
                {
                    outPath = Path.ChangeExtension(outPath, "wad2");
                }
                catch
                {
                    // ignored
                }
            }


            // Ask about it
            if (ask || string.IsNullOrWhiteSpace(outPath))
            {
                using (var dialog = new SaveFileDialog())
                {
                    if (!string.IsNullOrWhiteSpace(outPath))
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(outPath);
                        dialog.FileName = Path.GetFileName(outPath);
                    }
                    dialog.Filter = Wad2Writer.FileFormats.GetFilter();
                    dialog.Title = "Save Wad2";
                    dialog.AddExtension = true;
                    if (dialog.ShowDialog(owner) != DialogResult.OK)
                    {
                        return false;
                    }

                    outPath = dialog.FileName;
                }
            }

            // Save the wad2
            try
            {
                wadToSave.Timestamp = DateTime.Now;
                Wad2Writer.SaveToFile(wadToSave, outPath);

                // Immediately reload new wad, if it wasn't saved before (new or imported)
                if (wadToSave.FileName == null)
                {
                    LoadWad(tool, owner, true, outPath);
                }

                // Update last actual filename and call global event to update UI etc
                wadToSave.FileName = outPath;
                tool.ToggleUnsavedChanges(false);

                // Update recent files
                if (tool.DestinationWad.FileName != outPath)
                {
                    AddWadToRecent(outPath);
                }
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to save to '" + outPath + "'");
                tool.SendMessage("Unable to save to '" + outPath + "'.   " + exc, PopupType.Error);
                return false;
            }

            return true;
        }

        public static void CreateNewWad(WadToolClass tool, IWin32Window owner)
        {
            using (var form = new FormNewWad2())
            {
                if (form.ShowDialog(owner) == DialogResult.Cancel)
                    return;

                tool.DestinationWad = new Wad2 { GameVersion = form.Version };
                tool.ToggleUnsavedChanges(false);
            }
        }

        public static bool LoadReferenceLevel(WadToolClass tool, IWin32Window owner, string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = LevelFileDialog.BrowseFile(owner, null, null, "Open Tomb Editor reference level", LevelSettings.FileFormatsLevel, null, false);
            }

            if (string.IsNullOrEmpty(path))
                return false;

            tool.ReferenceLevel = Prj2Loader.LoadFromPrj2(path, null, CancellationToken.None, new Prj2Loader.Settings { IgnoreTextures = true, IgnoreWads = true });

            if (tool.ReferenceLevel != null)
            {
                tool.Configuration.Tool_ReferenceProject = path;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void UnloadReferenceLevel(WadToolClass tool)
        {
            tool.ReferenceLevel = null;
            tool.Configuration.Tool_ReferenceProject = string.Empty;
        }

        public static IWadObjectId ChangeSlot(WadToolClass tool, IWin32Window owner)
        {
            if (tool.MainSelection?.WadArea == WadArea.Source)
                return null;

            Wad2 wad = tool.GetWad(tool.MainSelection?.WadArea);
            IWadObject wadObject = wad?.TryGet(tool.MainSelection?.Id);
            if (wad == null || wadObject == null)
            {
                tool.SendMessage("You must have an object selected", PopupType.Error);
                return null;
            }

            // Ask for the new slot
            using (var form = new FormSelectSlot(tool.DestinationWad, wadObject.Id))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return null;

                if (form.NewId == wadObject.Id)
                    return null;

                if (wad.Contains(form.NewId))
                {
                    tool.SendMessage("The slot " + form.NewId.ToString(wad.GameVersion) + " is already occupied.", PopupType.Error);
                    return null;
                }
                wad.AssignNewId(wadObject.Id, form.NewId);
            }
            tool.WadChanged(tool.MainSelection.Value.WadArea);
            return wadObject.Id;
        }

        /*public static void ExportMoveable(WadToolClass tool, IWin32Window owner, WadMoveable m)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export model";
                saveFileDialog.Filter = BaseGeometryExporter.FileExtensions.GetFilter(true);
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = "dae";
                saveFileDialog.FileName = m.Id.ShortName(TRVersion.Game.TRNG);

                if (saveFileDialog.ShowDialog(owner) == DialogResult.OK)
                {
                    using (var settingsDialog = new GeometryIOSettingsDialog(new IOGeometrySettings() { Export = true }))
                    {
                        settingsDialog.AddPreset(IOSettingsPresets.GeometryExportSettingsPresets);
                        settingsDialog.SelectPreset("Normal scale");

                        if (settingsDialog.ShowDialog(owner) == DialogResult.OK)
                        {
                            BaseGeometryExporter.GetTextureDelegate getTextureCallback = txt =>
                            {
                                return "";
                            };

                            BaseGeometryExporter exporter = BaseGeometryExporter.CreateForFile(saveFileDialog.FileName, settingsDialog.Settings, getTextureCallback);
                            new Thread(() =>
                            {
                                var resultModel = PrepareModelForExport(saveFileDialog.FileName, m);

                                if (resultModel != null)
                                {
                                    if (exporter.ExportToFile(resultModel, saveFileDialog.FileName))
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    string errorMessage = "";
                                    return;
                                }
                            }).Start();
                        }
                    }
                }
            }
        }
        */

        private static void UpdateBoneAbsolutePositions(List<WadBone> bones)
        {
            bones[0].AbsoluteTranslation = bones[0].Translation;

            var currentNode = bones[0];
            var stack = new Stack<WadBone>();

            for (int j = 1; j < bones.Count; j++)
            {
                switch (bones[j].OpCode)
                {
                    case WadLinkOpcode.NotUseStack:
                        bones[j].AbsoluteTranslation = bones[j].Translation + currentNode.AbsoluteTranslation;
                        currentNode = bones[j];
                        break;

                    case WadLinkOpcode.Pop:
                        if (stack.Count > 0)
                        {
                            currentNode = stack.Pop();
                            bones[j].AbsoluteTranslation = bones[j].Translation + currentNode.AbsoluteTranslation;
                            currentNode = bones[j];
                        }
                        else
                        {
                            bones[j].AbsoluteTranslation = bones[j].Translation;
                            currentNode = bones[j];
                        }
                        break;

                    case WadLinkOpcode.Push:
                        stack.Push(currentNode);
                        bones[j].AbsoluteTranslation = bones[j].Translation + currentNode.AbsoluteTranslation;
                        currentNode = bones[j];
                        break;

                    case WadLinkOpcode.Read:
                        if (stack.Count > 0)
                        {
                            WadBone bone = stack.Pop();
                            bones[j].AbsoluteTranslation = bones[j].Translation + bone.AbsoluteTranslation;
                            currentNode = bones[j];
                            stack.Push(bone);
                        }
                        else
                        {
                            bones[j].AbsoluteTranslation = bones[j].Translation;
                            currentNode = bones[j];
                        }
                        break;
                }
            }
        }

        /*public static IOModel PrepareModelForExport(string filePath, WadMoveable m)
        {
            var model = new IOModel();

            var tempTextures = new Dictionary<Hash, WadTexture>();
            for (int i = 0; i < m.Bones.Count; i++)
            {
                for (int j = 0; j < m.Bones[i].Mesh.Polys.Count; j++)
                {
                    var poly = m.Bones[i].Mesh.Polys[j];
                    if (!tempTextures.ContainsKey(((WadTexture)poly.Texture.Texture).Hash))
                        tempTextures.Add(((WadTexture)poly.Texture.Texture).Hash, ((WadTexture)poly.Texture.Texture));
                }
            }

            List<WadTexture> textureList = tempTextures.Values.ToList();
            textureList.Sort(delegate (WadTexture x, WadTexture y)
            {
                if (x.Image.Width > y.Image.Width)
                    return -1;
                else if (x.Image.Width < y.Image.Width)
                    return 1;
                return 0;
            });

            var texturePieces = new Dictionary<Hash, WadTexture>();
            foreach (var texture in textureList)
            {
                texturePieces.Add(texture.Hash, texture);
            }

            var pages = PackTexturesForExport(texturePieces);

            // Create the materials
            for (int i = 0; i < pages.Count; i++)
            {
                var textureFileName = "Texture_" + i + ".png";
                var path = Path.Combine(Path.GetDirectoryName(filePath), textureFileName);

                var matOpaque = new IOMaterial(Material.Material_Opaque + "_" + i, pages[i], path, false, false, 0, i);
                var matOpaqueDoubleSided = new IOMaterial(Material.Material_OpaqueDoubleSided + "_" + i, pages[i], path, false, true, 0, i);
                var matAdditiveBlending = new IOMaterial(Material.Material_AdditiveBlending + "_" + i, pages[i], path, true, false, 0, i);
                var matAdditiveBlendingDoubleSided = new IOMaterial(Material.Material_AdditiveBlendingDoubleSided + "_" + i, pages[i], path, true, true, 0, i);

                model.Materials.Add(matOpaque);
                model.Materials.Add(matOpaqueDoubleSided);
                model.Materials.Add(matAdditiveBlending);
                model.Materials.Add(matAdditiveBlendingDoubleSided);
            }

            UpdateBoneAbsolutePositions(m.Bones);

            for (int i = 0; i < m.Bones.Count; i++)
            {
                int lastIndex = 0;

                var mesh = new IOMesh(
                    "TeMov_" +
                    i +
                    "_" +
                    m.Bones[i].AbsoluteTranslation.X.ToString() +
                    "_" +
                    m.Bones[i].AbsoluteTranslation.Y.ToString() +
                    "_" +
                    m.Bones[i].AbsoluteTranslation.Z.ToString()
                    );
                model.Meshes.Add(mesh);

                mesh.Position = m.Bones[i].AbsoluteTranslation;

                foreach (var p in m.Bones[i].Mesh.Polys)
                {
                    var poly = new IOPolygon(p.Shape == WadPolygonShape.Quad ? IOPolygonShape.Quad : IOPolygonShape.Triangle);

                    mesh.Positions.Add(m.Bones[i].AbsoluteTranslation + m.Bones[i].Mesh.VerticesPositions[p.Index0]);
                    mesh.Positions.Add(m.Bones[i].AbsoluteTranslation + m.Bones[i].Mesh.VerticesPositions[p.Index1]);
                    mesh.Positions.Add(m.Bones[i].AbsoluteTranslation + m.Bones[i].Mesh.VerticesPositions[p.Index2]);
                    if (p.Shape == WadPolygonShape.Quad)
                    {
                        mesh.Positions.Add(m.Bones[i].AbsoluteTranslation + m.Bones[i].Mesh.VerticesPositions[p.Index3]);
                    }

                    mesh.Normals.Add(m.Bones[i].Mesh.VerticesNormals[p.Index0]);
                    mesh.Normals.Add(m.Bones[i].Mesh.VerticesNormals[p.Index1]);
                    mesh.Normals.Add(m.Bones[i].Mesh.VerticesNormals[p.Index2]);
                    if (p.Shape == WadPolygonShape.Quad)
                    {
                        mesh.Normals.Add(m.Bones[i].Mesh.VerticesNormals[p.Index3]);
                    }

                    var texture = texturePieces[((WadTexture)p.Texture.Texture).Hash];

                    float scale = 256.0f;

                    var offset = new Vector2
                        (
                            (texture.PositionInAtlas.X - 0.5f) / scale,
                            (texture.PositionInAtlas.Y - 0.5f) / scale
                        );
                    mesh.UV.Add(p.Texture.TexCoord0 / scale + offset);
                    mesh.UV.Add(p.Texture.TexCoord1 / scale + offset);
                    mesh.UV.Add(p.Texture.TexCoord2 / scale + offset);
                    if (p.Shape == WadPolygonShape.Quad)
                    {
                        mesh.UV.Add(p.Texture.TexCoord3 / scale + offset);
                    }


                    if (m.VerticesColors.Count >= m.VerticesPositions.Count)
                    {
                        mesh.Colors.Add(new Vector4(m.VerticesColors[p.Index0], 1.0f));
                        mesh.Colors.Add(new Vector4(m.VerticesColors[p.Index1], 1.0f));
                        mesh.Colors.Add(new Vector4(m.VerticesColors[p.Index2], 1.0f));
                        if (p.Shape == WadPolygonShape.Quad)
                        {
                            mesh.Colors.Add(new Vector4(m.VerticesColors[p.Index3], 1.0f));
                        }
                    }
                    else
                    {
                        mesh.Colors.Add(Vector4.One);
                        mesh.Colors.Add(Vector4.One);
                        mesh.Colors.Add(Vector4.One);
                        if (p.Shape == WadPolygonShape.Quad)
                        {
                            mesh.Colors.Add(Vector4.One);
                        }
                    }

                    var mat = model.Materials[0];
                    foreach (var mt in model.Materials)
                        if (mt.Page == texture.Atlas)
                            if (mt.AdditiveBlending == (p.Texture.BlendMode >= BlendMode.Additive))
                                if (mt.DoubleSided == p.Texture.DoubleSided)
                                    if (mt.Shininess == 0)
                                        mat = mt;

                    poly.Indices.Add(lastIndex + 0);
                    poly.Indices.Add(lastIndex + 1);
                    poly.Indices.Add(lastIndex + 2);
                    if (p.Shape == WadPolygonShape.Quad)
                    {
                        poly.Indices.Add(lastIndex + 3);
                    }

                    if (!mesh.Submeshes.ContainsKey(mat))
                        mesh.Submeshes.Add(mat, new IOSubmesh(mat));

                    mesh.Submeshes[mat].Polygons.Add(poly);
                    lastIndex += (p.Shape == WadPolygonShape.Quad ? 4 : 3);
                }
            }

            for (int i = 0; i < pages.Count; i++)
            {
                var textureFileName = "Texture_" + i + ".png";
                var path = Path.Combine(Path.GetDirectoryName(filePath), textureFileName);
                pages[i].Image.Save(path);
            }

            return model;
        }*/

        public static void ImportModelAsStaticMesh(WadToolClass tool, IWin32Window owner)
        {
            if (tool.DestinationWad == null)
            {
                tool.SendMessage("You must have a wad opened", PopupType.Error);
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                try
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(tool.DestinationWad.FileName);
                    dialog.FileName = Path.GetFileName(tool.DestinationWad.FileName);
                }
                catch
                {
                    // ignored
                }

                dialog.Filter = BaseGeometryImporter.FileExtensions.GetFilter();
                dialog.Title = "Select a 3D file that you want to see imported.";
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return;
                }

                var viewModel = new GeometryIOSettingsWindowViewModel(IOSettingsPresets.GeometryImportSettingsPresets, new() { ImportWadMesh = true });
                viewModel.SelectPreset(tool.Configuration.GeometryIO_LastUsedGeometryImportPresetName);

                var settingsDialog = new GeometryIOSettingsWindow { DataContext = viewModel };
                settingsDialog.SetOwner(owner);
                settingsDialog.ShowDialog();

                if (viewModel.DialogResult != true)
                    return;

                tool.Configuration.GeometryIO_LastUsedGeometryImportPresetName = viewModel.SelectedPreset?.Name;

                var settings = viewModel.GetCurrentSettings();
                var @static = new WadStatic(tool.DestinationWad.GetFirstFreeStaticMesh());
                var mesh = WadMesh.ImportFromExternalModel(dialog.FileName, settings, tool.DestinationWad.MeshTexInfosUnique.FirstOrDefault());
                if (mesh == null)
                {

                    tool.SendMessage("Error while loading the 3D model. Please check that the file " +
                                        "is one of the supported formats and that the meshes are textured", PopupType.Error);
                    return;
                }
                @static.Mesh = mesh;
                @static.VisibilityBox = @static.Mesh.BoundingBox;
                @static.CollisionBox = @static.Mesh.BoundingBox;
                tool.DestinationWad.Statics.Add(@static.Id, @static);
                tool.WadChanged(WadArea.Destination);
            }
        }

        public static Wad2 ConvertWad2ToTombEngine(WadToolClass tool, IWin32Window owner, Wad2 src)
        {
            Wad2 dest = new Wad2 { GameVersion = TRVersion.Game.TombEngine };
            Wad2 referenceWad = Wad2Loader.LoadFromFile(TombEngineConverter.ReferenceWadPath, true);

            foreach (var moveable in src.Moveables)
            {
                string compatibleSlot = TrCatalog.GetMoveableTombEngineSlot(src.GameVersion, moveable.Key.TypeId);
                if (compatibleSlot == string.Empty)
                    continue;

                uint? destId = TrCatalog.GetItemIndex(TRVersion.Game.TombEngine, compatibleSlot, out bool isMoveable);
                if (!destId.HasValue)
                    continue;

                var newId = new WadMoveableId(destId.Value);

				WadMoveable mov;
				if (newId.TypeId == 0) // Copy Lara object directly from reference wad.
				{
					mov = referenceWad.Moveables[new WadMoveableId(0)].Clone();
				}
				else
				{
					mov = moveable.Value.Clone();
					mov.ConvertMoveable(src.GameVersion, referenceWad);
				}

                dest.Add(newId, mov);
            }

            foreach (var sequence in src.SpriteSequences)
            {
                string compatibleSlot = TrCatalog.GetSpriteSequenceTombEngineSlot(src.GameVersion, sequence.Key.TypeId);
                if (compatibleSlot == "")
                    continue;

                uint? destId = TrCatalog.GetItemIndex(TRVersion.Game.TombEngine, compatibleSlot, out bool isMoveable);
                if (!destId.HasValue)
                    continue;

                var newId = new WadSpriteSequenceId(destId.Value);

                dest.Add(newId, sequence.Value);
            }

            foreach (var staticObject in src.Statics)
                dest.Add(staticObject.Key, staticObject.Value);

            return dest;
        }

        public static void ConvertSelectedObjectUVMapping(WadToolClass tool, IWin32Window owner, List<IWadObjectId> objects, bool uvMapped)
        {
            if (objects == null || objects.Count == 0 || tool.MainSelection?.WadArea == WadArea.Source)
                return;

            int counter = 0;

            Action<WadMesh> convertUVMapping = (WadMesh mesh) =>
            {
                if (mesh.Polys.Count == 0)
                    return;

                var area = new Rectangle2(0, 0, mesh.Polys[0].Texture.Texture.Image.Width, mesh.Polys[0].Texture.Texture.Image.Height);
                if (mesh.Polys.All(p => p.Texture.ParentArea == area))
                    return;

                for (int i = 0; i < mesh.Polys.Count; i++)
                {
                    var poly = mesh.Polys[i];

                    if (uvMapped)
                        poly.Texture.SetParentArea(tool.DestinationWad.GameVersion == TRVersion.Game.TombEngine ? 4096 : 256);
                    else
                        poly.Texture.ClearParentArea();

                    mesh.Polys[i] = poly;
                }

                counter++;
            };

            foreach (var o in objects)
            {
                var obj = tool.DestinationWad.TryGet(o);
                if (obj == null)
                    continue;

                if (obj is WadMoveable)
                {
                    (obj as WadMoveable).Meshes.ForEach(m => convertUVMapping(m));
                }
                else if (obj is WadStatic)
                {
                    convertUVMapping((obj as WadStatic).Mesh);
                }
            }

            if (counter == 0)
            {
                tool.SendMessage("There was no mesh data in selected objects.\nNothing was done.", PopupType.Info);
                return;
            }

            tool.WadChanged(WadArea.Destination);
            tool.SendMessage(counter + " mesh" + (counter > 1 ? "es were" : " was") + " converted to " + (uvMapped ? "UV-mapped" : "tiled") + " texture mapping mode.", PopupType.Info);
        }

        public static void ConvertSelectedObjectLighting(WadToolClass tool, IWin32Window owner, List<IWadObjectId> objects, WadMeshLightingType type)
        {
            if (objects == null || objects.Count == 0 || tool.MainSelection?.WadArea == WadArea.Source)
                return;

            int counter = 0;

            foreach (var o in objects)
            {
                var obj = tool.DestinationWad.TryGet(o);
                if (obj == null)
                    continue;

                if (obj is WadMoveable)
                {
                    foreach (var mesh in (obj as WadMoveable).Meshes)
                    {
                        if (mesh.LightingType != type)
                        {
                            mesh.LightingType = type;
                            counter++;
                        }
                    }
                }
                else if (obj is WadStatic)
                {
                    var mesh = (obj as WadStatic).Mesh;

                    if (mesh.LightingType != type)
                    {
                        mesh.LightingType = type;
                        counter++;
                    }
                }
            }

            if (counter == 0)
            {
                tool.SendMessage("All selected objects already have specified lighting type.\nNothing was done.", PopupType.Info);
                return;
            }

            tool.WadChanged(WadArea.Destination);
            tool.SendMessage(counter + " mesh" + (counter > 1 ? "es were" : " was") + " converted to specified light model.", PopupType.Info);
        }

        public static List<IWadObjectId> CopyObject(WadToolClass tool, IWin32Window owner, List<IWadObjectId> objectIdsToMove, bool alwaysChooseId)
        {
            Wad2 sourceWad = tool.SourceWad;
            Wad2 destinationWad = tool.DestinationWad;

            if (destinationWad == null || sourceWad == null || objectIdsToMove.Count == 0)
            {
                tool.SendMessage("You must have two wads loaded and at least one source object selected.", PopupType.Error);
                return null;
            }

            var listInProgress = new List<uint>();

            // Figure out the new ids if there are any id collisions
            var newIds = objectIdsToMove.ToArray();

            // If destination is TombEngine, try to remap object IDs
            if (sourceWad.GameVersion != TRVersion.Game.TombEngine && destinationWad.GameVersion == TRVersion.Game.TombEngine)
            {
                for (int i = 0; i < objectIdsToMove.Count; ++i)
                {
                    var objectId = objectIdsToMove[i];
                    if (objectId is WadMoveableId)
                    {
                        var moveableId = (WadMoveableId)objectId;

                        // Try to get a compatible slot
                        string newSlot = TrCatalog.GetMoveableTombEngineSlot(sourceWad.GameVersion, moveableId.TypeId);
                        if (newSlot == "")
                            continue;

                        // Get the new ID
                        uint? newId = TrCatalog.GetItemIndex(destinationWad.GameVersion, newSlot, out bool isMoveable);
                        if (!newId.HasValue)
                            continue;

                        // Save the new ID
                        newIds[i] = new WadMoveableId(newId.Value);
                    }
                }
            }

            for (int i = 0; i < objectIdsToMove.Count; ++i)
            {
                if (!sourceWad.Contains(objectIdsToMove[i]))
                    continue;

                if (!alwaysChooseId)
                {
                    if (!destinationWad.Contains(newIds[i]))
                    {
                        if (!newIds.Take(i).Contains(newIds[i])) // There also must not be collisions with the other custom assigned ids.
                            continue;
                    }
                }

                bool askConfirm = !alwaysChooseId;

                // Ask for the new slot
                do
                {
                    DialogResult dialogResult;
                    if (askConfirm)
                    {
                        dialogResult = DarkMessageBox.Show(owner, "The id " + newIds[i].ToString(destinationWad.GameVersion) + " is already occupied in the destination wad.\n" +
                                                         "Do you want to replace it (Yes) or to select another Id (No)?",
                                                         "Occupied slot", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    }
                    else
                    {
                        dialogResult = DialogResult.No;
                    }

                    // From this time, always ask for confirm
                    askConfirm = true;

                    if (dialogResult == DialogResult.Cancel)
                    {
                        return null;
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        using (var form = new FormSelectSlot(destinationWad, newIds[i], listInProgress))
                        {
                            if (form.ShowDialog(owner) != DialogResult.OK)
                                return null;

                            if (destinationWad.Contains(form.NewId) || newIds.Take(i).Contains(form.NewId))
                            {
                                destinationWad.Remove(form.NewId);
                                tool.WadChanged(WadArea.Destination);
                            }
                            newIds[i] = form.NewId;

                            if (form.NewId is WadStaticId)
                                listInProgress.Add(((WadStaticId)form.NewId).TypeId);
                            else if (form.NewId is WadMoveableId)
                                listInProgress.Add(((WadMoveableId)form.NewId).TypeId);
                            else if (form.NewId is WadSpriteSequenceId)
                                listInProgress.Add(((WadSpriteSequenceId)form.NewId).TypeId);

                            break;
                        }
                    }
                    else
                    {
                        destinationWad.Remove(newIds[i]);
                        tool.WadChanged(WadArea.Destination);
                        break;
                    }
                } while (destinationWad.Contains(newIds[i]) || newIds.Take(i).Contains(newIds[i])); // There also must not be collisions with the other custom assigned ids.
            }

            Wad2 referenceWad = null;

            // Move objects
            for (int i = 0; i < objectIdsToMove.Count; ++i)
            {
                var obj = sourceWad.TryGet(objectIdsToMove[i]);
                if (obj == null)
                    continue;

                // TEN moveables sometimes need conversion procedures.
                if (destinationWad.GameVersion == TRVersion.Game.TombEngine && sourceWad.GameVersion != TRVersion.Game.TombEngine && obj is WadMoveable)
                {
                    var mov = (obj as WadMoveable).Clone();

                    if (referenceWad == null)
                    {
                        if (File.Exists((TombEngineConverter.ReferenceWadPath)))
                            referenceWad = Wad2Loader.LoadFromFile(TombEngineConverter.ReferenceWadPath, true);
                        else
                            referenceWad = new Wad2();
                    }

                    if (mov.Id.TypeId == 0)
                    {
                        if (referenceWad.Moveables.ContainsKey(mov.Id))
                        {
                            var dialogResult = DarkMessageBox.Show(owner,
                                "You are trying to copy incompatible Lara object from legacy wad to TEN wad. \n" +
                                "Replace it with compatible Lara object from reference wad?",
                                "Incompatible Lara object", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (dialogResult == DialogResult.Yes)
                                mov = referenceWad.Moveables[new WadMoveableId(0)].Clone();
                        }
                        else
                        {
                            DarkMessageBox.Show(owner,
                                "Reference wad can't be loaded.\n" + 
                                "Copying original Lara object which is incompatible with TEN.",
                                "Incompatible Lara object", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        mov.ConvertMoveable(sourceWad.GameVersion, referenceWad);
                    }

                    obj = mov;
                }

                destinationWad.Add(newIds[i], obj);
            }

            // Update the situation
            tool.WadChanged(WadArea.Destination);

            // Indicate that object is copied
            string infoString = (objectIdsToMove.Count == 1 ? "Object" : "Objects") + " successfully copied.";
            tool.SendMessage(infoString, PopupType.Info);

            return newIds.ToList();
        }

        public static void EditObject(WadToolClass tool, IWin32Window owner, DeviceManager deviceManager)
        {
            Wad2 wad = tool.GetWad(tool.MainSelection?.WadArea);
            var wadObject = wad?.TryGet(tool.MainSelection?.Id);

            if (wadObject == null || tool.MainSelection?.WadArea == WadArea.Source)
                return;

            if (wad == null)
            {
                tool.SendMessage("You must have a wad loaded and an object selected.", PopupType.Error);
                return;
            }

            // Choose behaviour
            if (wadObject is WadStatic)
            {
                using (var form = new FormStaticEditor(tool, deviceManager, wad, (WadStatic)wadObject))
                {
                    if (form.ShowDialog(owner) != DialogResult.OK)
                        return;
                }

                tool.WadChanged(tool.MainSelection.Value.WadArea);
            }
            else if (wadObject is WadMoveable)
            {
                using (var form = new FormAnimationEditor(tool, deviceManager, wad, ((WadMoveable)wadObject).Id))
                {
                    if (form.ShowDialog(owner) != DialogResult.OK)
                        return;

                    tool.WadChanged(tool.MainSelection.Value.WadArea);
                }
            }
            else if (wadObject is WadSpriteSequence)
            {
                using (var form = new FormSpriteSequenceEditor(tool, wad, (WadSpriteSequence)wadObject))
                {
                    if (form.ShowDialog(owner) != DialogResult.OK)
                        return;
                }

                tool.WadChanged(tool.MainSelection.Value.WadArea);
            }
        }

        public static void DeleteObjects(WadToolClass tool, IWin32Window owner, WadArea wadArea, List<IWadObjectId> ObjectIdsToDelete)
        {
            if (ObjectIdsToDelete.Count == 0)
                return;

            if (DarkMessageBox.Show(owner, "You are about to delete " + ObjectIdsToDelete.Count + " objects. Are you sure?",
                "A question just in case...", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Wad2 wad = tool.GetWad(wadArea);
            foreach (var id in ObjectIdsToDelete)
            {
                wad.Remove(id);
            }

            tool.WadChanged(wadArea);
        }

        public static IWadObjectId CreateObject(WadToolClass tool, IWin32Window owner, IWadObject initialWadObject)
        {
            Wad2 destinationWad = tool.DestinationWad;
            if (destinationWad == null)
            {
                tool.SendMessage("You must have a destination wad opened.", PopupType.Error);
                return null;
            }

            IWadObjectId result;

            using (var form = new FormSelectSlot(tool.DestinationWad, initialWadObject.Id))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return null;

                if (destinationWad.Contains(form.NewId))
                {
                    tool.SendMessage("The slot " + form.NewId.ToString(destinationWad.GameVersion) + " is already occupied.", PopupType.Error);
                    return null;
                }

                if (initialWadObject is WadMoveable)
                {
                    var moveable = initialWadObject as WadMoveable;
                    WadBone bone = new WadBone
                    {
                        Name = "Root",
                        Mesh = CreateFakeMesh("Root")
                    };
                    moveable.Bones.Add(bone);
                }

                destinationWad.Add(form.NewId, initialWadObject);
                result = form.NewId;
            }

            tool.WadChanged(WadArea.Destination);
            tool.ToggleUnsavedChanges();

            return result;
        }

        public static WadMesh CreateFakeMesh(string name)
        {
            WadMesh mesh = new WadMesh
            {
                Name = name
            };

            mesh.VertexPositions.Add(new Vector3(-1, 1, 1)); // 0
            mesh.VertexPositions.Add(new Vector3(1, 1, 1)); // 1
            mesh.VertexPositions.Add(new Vector3(1, 1, -1)); // 2
            mesh.VertexPositions.Add(new Vector3(-1, 1, -1)); // 3
            mesh.VertexPositions.Add(new Vector3(-1, -1, 1)); // 4
            mesh.VertexPositions.Add(new Vector3(1, -1, 1)); // 5
            mesh.VertexPositions.Add(new Vector3(1, -1, -1)); // 6
            mesh.VertexPositions.Add(new Vector3(-1, -1, -1)); // 7

            for (int i = 0; i < 8; i++)
                mesh.VertexNormals.Add(Vector3.Zero);

            var texture = new TextureArea();
            var image = ImageC.Magenta;
            texture.Texture = new WadTexture(image);
            texture.TexCoord0 = new Vector2(0, 0);
            texture.TexCoord1 = new Vector2(1, 0);
            texture.TexCoord2 = new Vector2(1, 1);
            texture.TexCoord3 = new Vector2(0, 1);

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 0,
                Index1 = 1,
                Index2 = 2,
                Index3 = 3,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 2,
                Index1 = 1,
                Index2 = 5,
                Index3 = 6,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 1,
                Index1 = 0,
                Index2 = 4,
                Index3 = 5,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 0,
                Index1 = 3,
                Index2 = 7,
                Index3 = 4,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 3,
                Index1 = 2,
                Index2 = 6,
                Index3 = 7,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            mesh.Polys.Add(new WadPolygon
            {
                Index0 = 7,
                Index1 = 6,
                Index2 = 5,
                Index3 = 4,
                Shape = WadPolygonShape.Quad,
                Texture = texture
            });

            return mesh;
        }

        public static bool ExportAnimationToXml(WadMoveable moveable, WadAnimation animation, string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);

                // Serialize the animation to XML
                var xmlSerializer = new XmlSerializer(typeof(WadAnimation));
                string xml = "";

                using (var sww = new StringWriter())
                {
                    using (var tw = new XmlTextWriter(sww))
                    {
                        tw.Formatting = Formatting.Indented;
                        tw.Indentation = 4;

                        using (XmlWriter writer = XmlWriter.Create(tw))
                        {
                            xmlSerializer.Serialize(writer, animation);
                            xml = sww.ToString();
                        }
                    }
                }

                // Write XML to file
                using (var writer = new StreamWriter(File.OpenWrite(fileName)))
                {
                    writer.Write(xml);
                }

                return true;
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "'ExportAnimationToXml' failed.");
                return false;
            }
        }

        public static WadAnimation ImportAnimationFromXml(WadToolClass tool, string fileName)
        {
            try
            {
                // Read animation from XML
                var deserializer = new XmlSerializer(typeof(WadAnimation));
                TextReader reader = new StreamReader(fileName);
                object obj = deserializer.Deserialize(reader);
                var animation = (WadAnimation)obj;
                reader.Close();

                foreach (var cmd in animation.AnimCommands)
                {
                    if (cmd.Type == WadAnimCommandType.FlipEffect || cmd.Type == WadAnimCommandType.PlaySound)
                        cmd.ConvertLegacyConditions();
                }

                return animation;
            }
            catch (Exception exc)
            {
                tool.SendMessage("Unknown error while importing animation! Possible reason: not valid XML format.", PopupType.Error);
                logger.Warn(exc, "'ImportAnimationFromXml' failed.");
                return null;
            }
        }

        public static WadAnimation ImportAnimationFromTrw(string fileName, int sourceAnimIndex)
        {
            WadAnimation result = new WadAnimation();

            using (var reader = new BinaryReaderEx(File.OpenRead(fileName)))
            {
                int wmVersion = reader.ReadInt32();
                int fileType = reader.ReadInt32();

                // Most common trw type is 190, there is also rare 170 which is
                // blunt copy of wad data and requires low-level parsing, so we ignore that.

                if (wmVersion != 190)
                {
                    throw new Exception("Unknown .trw version");
                }

                if (fileType != 5)
                {
                    throw new Exception("File is not valid .trw file");
                }

                reader.ReadBytes(4); // Unused

                result.FrameRate = reader.ReadByte();

                reader.ReadBytes(1); // Unused

                result.StateId = reader.ReadUInt16();
                int speed = reader.ReadInt32();
                int accel = reader.ReadInt32();
                int speedLateral = reader.ReadInt32();
                int accelLateral = reader.ReadInt32();

                reader.ReadBytes(4); // Unused

                short nextAnimation = reader.ReadInt16();
                result.NextFrame = reader.ReadUInt16();
                ushort numStateChanges = reader.ReadUInt16();

                reader.ReadBytes(6); // Unused

                // WM keeps NextAnim as index offset, so it may be incorrectly negative
                result.NextAnimation = (ushort)(MathC.Clamp(sourceAnimIndex + nextAnimation, 0, ushort.MaxValue));

                reader.ReadBytes(4); // Unused

                ushort numCommands = reader.ReadUInt16();
                logger.Info("Trw animation commands: " + numCommands);

                for (int i = 0; i < numCommands; i++)
                {
                    var ac = new WadAnimCommand
                    {
                        Type = (WadAnimCommandType)reader.ReadUInt16(),
                        Parameter1 = reader.ReadInt16(),
                        Parameter2 = reader.ReadInt16(),
                        Parameter3 = reader.ReadInt16()
                    };

                    result.AnimCommands.Add(ac);
                }

                reader.ReadBytes(8); // Unused

                uint numKeyFrames = reader.ReadUInt32();
                logger.Info("Trw animation keyframes: " + numKeyFrames);

                if (numKeyFrames == 0)
                {
                    throw new Exception(".trw file does not contain valid frames");
                }

                for (int i = 0; i < numKeyFrames; i++)
                {
                    var frame = new WadKeyFrame();

                    short x1 = reader.ReadInt16();
                    short x2 = reader.ReadInt16();
                    int y1 = -reader.ReadInt16();
                    int y2 = -reader.ReadInt16();
                    short z1 = reader.ReadInt16();
                    short z2 = reader.ReadInt16();
                    Vector3 min = new Vector3(x1, y1, z1);
                    Vector3 max = new Vector3(x2, y2, z2);

                    frame.BoundingBox = new BoundingBox(min, max);

                    short offX = reader.ReadInt16();
                    int offY = -reader.ReadInt16();
                    short offZ = reader.ReadInt16();

                    frame.Offset = new Vector3(offX, offY, offZ);

                    ushort numBones = reader.ReadUInt16();

                    for (int j = 0; j < numBones; j++)
                    {
                        const float factor = 360.0f / 1024.0f;
                        float rotX = -reader.ReadInt16() * factor;
                        float rotY = reader.ReadInt16() * factor;
                        float rotZ = -reader.ReadInt16() * factor;

                        WadKeyFrameRotation rot = new WadKeyFrameRotation
                        {
                            Rotations = new Vector3(rotX, rotY, rotZ)
                        };
                        frame.Angles.Add(rot);
                    }

                    result.KeyFrames.Add(frame);
                }

                try
                {
                    for (int i = 0; i < numStateChanges; i++)
                    {
                        var sc = new WadStateChange
                        {
                            StateId = reader.ReadUInt16()
                        };
                        ushort numAnimDispatches = reader.ReadUInt16();

                        reader.ReadBytes(2); // Unused

                        // VB6 padding
                        int padCounter = 10;
                        reader.ReadBytes(padCounter);

                        for (int j = 0; j < numAnimDispatches; j++)
                        {
                            var disp = new WadAnimDispatch
                            {
                                InFrame = reader.ReadUInt16(),
                                OutFrame = reader.ReadUInt16()
                            };

                            nextAnimation = reader.ReadInt16();
                            disp.NextAnimation = (ushort)(MathC.Clamp(sourceAnimIndex + nextAnimation, 0, ushort.MaxValue));

                            disp.NextFrameLow = reader.ReadUInt16();

                            sc.Dispatches.Add(disp);
                            padCounter += 4; // 4 bytes per 1 dispatch, don't ask why.
                        }

                        result.StateChanges.Add(sc);

                        // VB6 padding
                        reader.ReadBytes(padCounter);
                    }
                }
                catch
                {
                    // In case unexpected VB6 padding is encountered, reset state changes
                    // and continue importing anyway.

                    logger.Warn(".trw file has incorrect state changes data block");
                    result.StateChanges.Clear();
                }

                // New velocities
                float acceleration = accel / 65536.0f;
                result.StartVelocity = speed / 65536.0f;

                float lateralAcceleration = accelLateral / 65536.0f;
                result.StartLateralVelocity = speedLateral / 65536.0f;

                if (result.KeyFrames.Count != 0 && result.FrameRate != 0)
                {
                    result.EndVelocity = result.StartVelocity + acceleration *
                                        (result.KeyFrames.Count - 1) * result.FrameRate;
                    result.EndLateralVelocity = result.StartLateralVelocity + lateralAcceleration *
                                               (result.KeyFrames.Count - 1) * result.FrameRate;
                }
                else
                {
                    // Basic foolproofness for potentially broken animations
                    result.EndVelocity = result.StartVelocity;
                    result.EndLateralVelocity = result.StartLateralVelocity;
                }

                // WM was not aware of EndFrame field so we calculate that
                result.EndFrame = (ushort)(result.GetRealNumberOfFrames() - 1);
            }

            return result;
        }

        public static WadAnimation ImportAnimationFromModel(WadToolClass tool, IWin32Window owner, int nodeCount, string fileName)
        {
            IOModel tmpModel = null;

            // Import the model
            try
            {
                var viewModel = new GeometryIOSettingsWindowViewModel(
                    IOSettingsPresets.AnimationSettingsPresets,
                    new() { ProcessAnimations = true, ProcessGeometry = false }
                );

                viewModel.SelectPreset(tool.Configuration.GeometryIO_LastUsedAnimationPresetName);

                string resultingExtension = Path.GetExtension(fileName).ToLowerInvariant();

                var settingsDialog = new GeometryIOSettingsWindow { DataContext = viewModel };
                settingsDialog.SetOwner(owner);
                settingsDialog.ShowDialog();

                if (viewModel.DialogResult != true)
                    return null;

                tool.Configuration.GeometryIO_LastUsedAnimationPresetName = viewModel.SelectedPreset?.Name;

                var settings = viewModel.GetCurrentSettings();
                var importer = BaseGeometryImporter.CreateForFile(fileName, settings, null);
                tmpModel = importer.ImportFromFile(fileName);

                // We don't support animation importing from custom-written mqo importer yet...
                if (importer is MetasequoiaImporter)
                {
                    tool.SendMessage("Metasequoia importer isn't currently supported.", PopupType.Error);
                    return null;
                }

                // If no animations, return null
                if (tmpModel.Animations.Count == 0)
                {
                    tool.SendMessage("Selected file has no supported animations!", PopupType.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                tool.SendMessage("Unknown error while importing animation. \n" + ex?.Message, PopupType.Error);
                logger.Warn(ex, "'ImportAnimationFromModel' failed.");
                return null;
            }

            IOAnimation animToImport;

            if (tmpModel.Animations.Count > 1)
            {
                using (var dialog = new AnimationImportDialog(tmpModel.Animations.Select(o => o.Name).ToList()))
                {
                    dialog.ShowDialog(owner);
                    if (dialog.DialogResult == DialogResult.Cancel)
                    {
                        return null;
                    }
                    else
                    {
                        animToImport = tmpModel.Animations[dialog.AnimationToImport];
                    }
                }
            }
            else
            {
                animToImport = tmpModel.Animations[0];
            }


            // Integrity check, for cases when something totally went wrong with assimp
            if (animToImport == null)
            {
                tool.SendMessage("Animation importer encountered serious error. No animation imported.", PopupType.Error);
                return null;
            }

            // Integrity check, is there any valid frames?
            if (animToImport.Frames.Count <= 0)
            {
                tool.SendMessage("Selected animation has no frames!", PopupType.Error);
                return null;
            }

            // Integrity check, number of bones = number of nodes?
            if (animToImport.NumNodes != nodeCount)
            {
                tool.SendMessage("Selected animation has different number of bones!", PopupType.Error);
                return null;
            }

            var animation = new WadAnimation
            {
                Name = animToImport.Name
            };

            foreach (var frame in animToImport.Frames)
            {
                var keyFrame = new WadKeyFrame
                {
                    Offset = frame.Offset
                };
                frame.Angles.ForEach(angle => keyFrame.Angles.Add(new WadKeyFrameRotation() { Rotations = angle }));

                animation.KeyFrames.Add(keyFrame);
            }

            animation.EndFrame = (ushort)(animToImport.Frames.Count - 1);

            return animation;
        }

        public static void EditSkeleton(WadToolClass tool, IWin32Window owner)
        {
            if (tool.MainSelection?.WadArea == WadArea.Source)
                return;

            Wad2 wad = tool.GetWad(tool.MainSelection.Value.WadArea);
            var moveableId = (WadMoveableId)tool.MainSelection.Value.Id;
            using (var form = new FormSkeletonEditor(tool, DeviceManager.DefaultDeviceManager, wad, moveableId))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return;

                tool.WadChanged(WadArea.Destination);
            }
        }

        private static void AddWadToRecent(string fileName)
        {
            if (Properties.Settings.Default.RecentProjects == null)
            {
                Properties.Settings.Default.RecentProjects = new List<string>();
            }

            Properties.Settings.Default.RecentProjects.RemoveAll(s => s == fileName);
            Properties.Settings.Default.RecentProjects.Insert(0, fileName);

            if (Properties.Settings.Default.RecentProjects.Count > 10)
            {
                Properties.Settings.Default.RecentProjects.RemoveRange(10, Properties.Settings.Default.RecentProjects.Count - 10);
            }

            Properties.Settings.Default.Save();
        }

        public static void ExportMesh(WadMesh mesh, WadToolClass tool, IWin32Window owner)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export mesh";
                saveFileDialog.InitialDirectory = PathC.GetDirectoryNameTry(tool.DestinationWad.FileName);
                saveFileDialog.Filter = BaseGeometryExporter.FileExtensions.GetFilter(true);
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = "obj";
                saveFileDialog.FileName = mesh.Name;

                if (saveFileDialog.ShowDialog(owner) != DialogResult.OK)
                    return;

                if (!saveFileDialog.FileName.IsANSI())
                {
                    MessageBoxes.NonANSIFilePathError(owner);
                    ExportMesh(mesh, tool, owner);
                    return;
                }

                try
                {
                    var viewModel = new GeometryIOSettingsWindowViewModel(
                        IOSettingsPresets.GeometryExportSettingsPresets,
                        new() { Export = true }
                    );

                    viewModel.SelectPreset(tool.Configuration.GeometryIO_LastUsedGeometryExportPresetName);

                    var settingsDialog = new GeometryIOSettingsWindow { DataContext = viewModel };
                    settingsDialog.SetOwner(owner);
                    settingsDialog.ShowDialog();

                    if (viewModel.DialogResult != true)
                        return;

                    tool.Configuration.GeometryIO_LastUsedGeometryExportPresetName = viewModel.SelectedPreset?.Name;

                    var settings = viewModel.GetCurrentSettings();

                    BaseGeometryExporter.GetTextureDelegate getTextureCallback = txt => "";
                    BaseGeometryExporter exporter = BaseGeometryExporter.CreateForFile(saveFileDialog.FileName, settings, getTextureCallback);
                    new Thread(() =>
                    {
                        IOModel resultModel = WadMesh.PrepareForExport(saveFileDialog.FileName, settings, mesh);
                        if (resultModel != null)
                        {
                            if (exporter.ExportToFile(resultModel, saveFileDialog.FileName))
                                return;
                        }

                        tool.SendMessage("Selected mesh is broken and can't be exported.\nPlease replace this mesh with another.");
                        return;

                    }).Start();
                }
                catch (Exception ex)
                {
                    tool.SendMessage("There was an error exporting 3D model.\nException: " + ex.Message, PopupType.Error);
                }
            }
        }

        public static WadMesh ImportMesh(WadToolClass tool, IWin32Window owner)
        {
            if (tool.DestinationWad == null)
            {
                tool.SendMessage("You must have a wad opened", PopupType.Error);
                return null;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select a 3D file that you want to see imported";
                dialog.InitialDirectory = PathC.GetDirectoryNameTry(tool.DestinationWad.FileName);
                dialog.Filter = BaseGeometryImporter.FileExtensions.GetFilter();

                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return null;

                if (!dialog.FileName.IsANSI())
                {
                    MessageBoxes.NonANSIFilePathError(owner);
                    return ImportMesh(tool, owner);
                }

                try
                {
                    var viewModel = new GeometryIOSettingsWindowViewModel(IOSettingsPresets.GeometryImportSettingsPresets, new() { ImportWadMesh = true });
                    viewModel.SelectPreset(tool.Configuration.GeometryIO_LastUsedGeometryImportPresetName);

                    var settingsDialog = new GeometryIOSettingsWindow { DataContext = viewModel };
                    settingsDialog.SetOwner(owner);
                    settingsDialog.ShowDialog();

                    if (viewModel.DialogResult != true)
                        return null;

                    tool.Configuration.GeometryIO_LastUsedGeometryImportPresetName = viewModel.SelectedPreset?.Name;

                    var settings = viewModel.GetCurrentSettings();

                    // A flag which allows to import untextured meshes.
                    settings.ProcessUntexturedGeometry = true;

                    WadMesh mesh = WadMesh.ImportFromExternalModel(dialog.FileName, settings, tool.DestinationWad.MeshTexInfosUnique.FirstOrDefault());
                    if (mesh == null)
                    {
                        tool.SendMessage("Error loading 3D model. Check that the file format \n" +
                                            "is supported, meshes are textured and texture file is present.", PopupType.Error);
                        return null;
                    }

                    return mesh;
                }
                catch (Exception ex)
                {
                    tool.SendMessage("There was an error importing 3D model.\nException: " + ex.Message, PopupType.Error);
                    return null;
                }
            }
        }
    }
}
