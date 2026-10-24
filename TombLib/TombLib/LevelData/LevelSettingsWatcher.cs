using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor
{
    public class LevelSettingsWatcher : IDisposable
    {
        private readonly FileSystemWatcherManager _watcher = new FileSystemWatcherManager();
        private readonly WeakReference<LevelSettings> _settings = new WeakReference<LevelSettings>(null);

        public class ChangedEventArgs<T> : EventArgs
        {
            public T Object { get; set; }
        }

        public EventHandler<ChangedEventArgs<LevelTexture>> TextureChanged { get; }
        public EventHandler<ChangedEventArgs<ReferencedWad>> WadChanged { get; }
        public EventHandler<ChangedEventArgs<ReferencedSoundCatalog>> SoundsCatalogChanged { get; }
        public EventHandler<ChangedEventArgs<ImportedGeometry>> ImportedGeometryChanged { get; }
        public EventHandler<ChangedEventArgs<ImportedGeometryTexture>> ImportedGeometryTexturesChanged { get; }
        public SynchronizationContext SynchronizationContext { get; }

        public LevelSettingsWatcher(
            EventHandler<ChangedEventArgs<LevelTexture>> textureChanged,
            EventHandler<ChangedEventArgs<ReferencedWad>> wadChanged,
            EventHandler<ChangedEventArgs<ReferencedSoundCatalog>> soundsCatalogChanged,
            EventHandler<ChangedEventArgs<ImportedGeometry>> importedGeometryChanged,
            SynchronizationContext synchronizationContext = null)
        {
            TextureChanged = textureChanged;
            WadChanged = wadChanged;
            SoundsCatalogChanged = soundsCatalogChanged;
            ImportedGeometryChanged = importedGeometryChanged;
            ImportedGeometryTexturesChanged = (sender, e) => importedGeometryChanged(sender, new ChangedEventArgs<ImportedGeometry>());
            SynchronizationContext = synchronizationContext ?? new NullSynchronizationContext();
        }

        private class NullSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object state) => d(state);
            public override void Send(SendOrPostCallback d, object state) => d(state);
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }

        private LevelSettings Settings
        {
            get
            {
                LevelSettings result;
                _settings.TryGetTarget(out result);
                return result;
            }
        }

        private class WatchedTexture : FileSystemWatcherManager.WatchedObj
        {
            public LevelSettingsWatcher Parent;
            public LevelTexture Texture;
            public override IEnumerable<string> Files => new[] { Parent.Settings?.MakeAbsolute(Texture.Path) };
            public override IEnumerable<string> Directories => null;
            public override string Name => PathC.GetFileNameWithoutExtensionTry(Parent.Settings?.MakeAbsolute(Texture.Path));
            public override bool IsRepresentingSameObject(FileSystemWatcherManager.WatchedObj other) => Texture.Equals((other as WatchedTexture)?.Texture);
            public override void TryReload(FileSystemWatcherManager sender, FileSystemWatcherManager.ReloadArgs e)
            {
                LevelSettings settings = Parent.Settings;
                if (settings == null)
                    return;

                var textureTemp = (LevelTexture)(Texture.Clone());
                textureTemp.Reload(settings);
                Parent.SynchronizationContext.Post(unused =>
                {
                    Texture.Assign(textureTemp);
                    Parent.TextureChanged(null, new ChangedEventArgs<LevelTexture> { Object = Texture });
                }, null);
            }
        }

        private class WatchedWad : FileSystemWatcherManager.WatchedObj
        {
            public LevelSettingsWatcher Parent;
            public ReferencedWad Wad;
            public override IEnumerable<string> Files => GetAllAssociatedFiles();
            public override IEnumerable<string> Directories => null;
            public override string Name => PathC.GetFileNameWithoutExtensionTry(Parent.Settings?.MakeAbsolute(Wad.Path));
            public override bool IsRepresentingSameObject(FileSystemWatcherManager.WatchedObj other) => Wad.Equals((other as WatchedWad)?.Wad);
            public override void TryReload(FileSystemWatcherManager sender, FileSystemWatcherManager.ReloadArgs e)
            {
                LevelSettings settings = Parent.Settings;
                if (settings == null)
                    return;

                ReferencedWad newWad = Wad.Clone();
                newWad.Reload(settings);

                if (newWad.LoadException != null)
                    throw newWad.LoadException;

                Parent.SynchronizationContext.Post(unused =>
                {
                    Wad.Assign(newWad);
                    Parent?.WadChanged(null, new ChangedEventArgs<ReferencedWad> { Object = newWad });
                }, null);
            }

            private List<string> GetAllAssociatedFiles()
            {
                var result = new List<string>() { Parent.Settings?.MakeAbsolute(Wad.Path) };

                if (!string.IsNullOrEmpty(result[0]) && result[0].EndsWith(".wad", StringComparison.InvariantCultureIgnoreCase))
                {
                    var wasPath = Path.ChangeExtension(result[0], ".was");
                    var swdPath = Path.ChangeExtension(result[0], ".swd");
                    if (File.Exists(wasPath)) result.Add(wasPath);
                    if (File.Exists(swdPath)) result.Add(swdPath);
                }
                return result;
            }
        }

        private class WatchedImportedGeometry : FileSystemWatcherManager.WatchedObj
        {
            public LevelSettingsWatcher Parent;
            public ImportedGeometry ImportedGeometry;
            public override IEnumerable<string> Files => GetAllAssociatedFiles();
            public override IEnumerable<string> Directories => null;
            public override string Name => PathC.GetFileNameWithoutExtensionTry(Parent.Settings?.MakeAbsolute(ImportedGeometry.Info.Path));
            public override bool IsRepresentingSameObject(FileSystemWatcherManager.WatchedObj other) => ImportedGeometry.Equals((other as WatchedImportedGeometry)?.ImportedGeometry);
            public override void TryReload(FileSystemWatcherManager sender, FileSystemWatcherManager.ReloadArgs e)
            {
                LevelSettings settings = Parent.Settings;
                if (settings == null)
                    return;

                Parent.SynchronizationContext.Send(unused =>
                {
                    // It would be very nice if we could do this concurrently.
                    // However we can't because reloading depends on the entire level settings :(
                    settings.ImportedGeometryUpdate(ImportedGeometry, ImportedGeometry.Info);
                }, null);
                Parent.SynchronizationContext.Post(unused =>
                {
                    Parent?.ImportedGeometryChanged(null, new ChangedEventArgs<ImportedGeometry> { Object = ImportedGeometry });
                }, null);
            }

            private List<string> GetAllAssociatedFiles()
            {
                var result = new List<string>() { Parent.Settings?.MakeAbsolute(ImportedGeometry.Info.Path) };

                if (!string.IsNullOrEmpty(result[0]) && result[0].EndsWith(".obj", StringComparison.InvariantCultureIgnoreCase))
                {
                    var mtlPath = Path.ChangeExtension(result[0], ".mtl");
                    if (File.Exists(mtlPath)) result.Add(mtlPath);
                }
                return result;
            }
        }

        private class WatchedImportedGeometryTexture : FileSystemWatcherManager.WatchedObj
        {
            public LevelSettingsWatcher Parent;
            public ImportedGeometryTexture ImportedGeometryTexture;
            public override IEnumerable<string> Files => new[] { Parent.Settings?.MakeAbsolute(ImportedGeometryTexture.AbsolutePath) };
            public override IEnumerable<string> Directories => null;
            public override string Name => PathC.GetFileNameWithoutExtensionTry(Parent.Settings?.MakeAbsolute(ImportedGeometryTexture.AbsolutePath));
            public override bool IsRepresentingSameObject(FileSystemWatcherManager.WatchedObj other) => ImportedGeometryTexture.Equals((other as WatchedImportedGeometryTexture)?.ImportedGeometryTexture);
            public override void TryReload(FileSystemWatcherManager sender, FileSystemWatcherManager.ReloadArgs e)
            {
                LevelSettings settings = Parent.Settings;
                if (settings == null)
                    return;

                ImportedGeometryTexture importedGeometryTextureReloaded = new ImportedGeometryTexture(ImportedGeometryTexture.AbsolutePath);
                Parent.SynchronizationContext.Post(unused =>
                {
                    ImportedGeometryTexture.Assign(importedGeometryTextureReloaded);
                    Parent?.ImportedGeometryTexturesChanged(null, new ChangedEventArgs<ImportedGeometryTexture> { Object = ImportedGeometryTexture });
                }, null);
            }
        }

        public void WatchLevelSettings(LevelSettings settings)
        {
            var listOfWatchedObjs = new List<FileSystemWatcherManager.WatchedObj>();
            foreach (LevelTexture texture in settings.Textures)
                listOfWatchedObjs.Add(new WatchedTexture { Parent = this, Texture = texture });
            foreach (ReferencedWad wad in settings.Wads)
                listOfWatchedObjs.Add(new WatchedWad { Parent = this, Wad = wad });
            foreach (ReferencedSoundCatalog catalog in settings.SoundCatalogs)
                listOfWatchedObjs.Add(new WatchedSoundCatalog { Parent = this, SoundsCatalog = catalog });
            foreach (ImportedGeometry importedGeometry in settings.ImportedGeometries)
                listOfWatchedObjs.Add(new WatchedImportedGeometry { Parent = this, ImportedGeometry = importedGeometry });
            foreach (ImportedGeometryTexture importedGeometryTexture in settings.ImportedGeometries.SelectMany(geometry => geometry.Textures).Distinct())
                listOfWatchedObjs.Add(new WatchedImportedGeometryTexture { Parent = this, ImportedGeometryTexture = importedGeometryTexture });
            _watcher.UpdateAllFiles(listOfWatchedObjs, () => _settings.SetTarget(settings));
        }

        private class WatchedSoundCatalog : FileSystemWatcherManager.WatchedObj
        {
            public LevelSettingsWatcher Parent;
            public ReferencedSoundCatalog SoundsCatalog;
            public override IEnumerable<string> Files => new[] { Parent.Settings?.MakeAbsolute(SoundsCatalog.Path) };
            public override IEnumerable<string> Directories => Parent.Settings?.WadSoundPaths?.Select(path => Parent.Settings?.MakeAbsolute(path.Path));
            public override string Name => PathC.GetFileNameWithoutExtensionTry(Parent.Settings?.MakeAbsolute(SoundsCatalog.Path));
            public override bool IsRepresentingSameObject(FileSystemWatcherManager.WatchedObj other) => SoundsCatalog.Equals((other as WatchedSoundCatalog)?.SoundsCatalog);
            public override void TryReload(FileSystemWatcherManager sender, FileSystemWatcherManager.ReloadArgs e)
            {
                LevelSettings settings = Parent.Settings;
                if (settings == null)
                    return;

                ReferencedSoundCatalog newCatalog = SoundsCatalog.Clone();
                newCatalog.Reload(settings);
                Parent.SynchronizationContext.Post(unused =>
                {
                    SoundsCatalog.Assign(newCatalog);
                    Parent?.SoundsCatalogChanged(null, new ChangedEventArgs<ReferencedSoundCatalog> { Object = newCatalog });
                }, null);
            }
        }

        public void RestartReloading() => _watcher.RestartReloading();

        public void StopReloading() => _watcher.StopReloading();
    }
}
