using NLog;
using System;
using System.Collections.Generic;
using System.Windows;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.WPF.Services;

/// <summary>Resolves missing project resources through WPF dialogs on the UI dispatcher.</summary>
public sealed class GraphicalDialogHandler(IWindowOwner? owner) : IDialogHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly object _gate = new();

    /// <summary>Handles resource replacement prompts on the application dispatcher.</summary>
    public static void HandleDialog(IDialogDescription description, IWindowOwner? owner)
    {
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(() => HandleDialog(description, owner));
            return;
        }

        switch (description)
        {
            case DialogDescriptonTextureUnloadable texture:
                RetryLoad(owner, texture.Settings, () => texture.Texture.Path, () => texture.Texture.LoadException,
                    path => texture.Texture.SetPath(texture.Settings, path), "texture", "Load a texture", ImageC.FileExtensions);
                break;
            case DialogDescriptonWadUnloadable wad:
                RetryLoad(owner, wad.Settings, () => wad.Wad.Path, () => wad.Wad.LoadException,
                    path => wad.Wad.SetPath(wad.Settings, path), "objects", "Load an object file (*.wad)", Wad2.FileExtensions);
                break;
            case DialogDescriptonSoundsCatalogUnloadable sounds:
                RetryLoad(owner, sounds.Settings, () => sounds.Sounds.Path, () => sounds.Sounds.LoadException,
                    path => sounds.Sounds.SetPath(sounds.Settings, path), "sound catalog", "Load a sound catalog (*.sfx)", WadSounds.FileExtensions);
                break;
            default:
                Logger.Info("Ignored dialog event of type {0}.", description.GetType().FullName);
                break;
        }
    }

    /// <summary>Serializes resource replacement requests from background loaders.</summary>
    public void RaiseDialog(IDialogDescription description)
    {
        lock (_gate)
        {
            HandleDialog(description, owner);
        }
    }

    private static void RetryLoad(IWindowOwner? owner, LevelSettings settings, Func<string> getPath, Func<Exception?> getException,
        Action<string> setPath, string resourceName, string title, IEnumerable<FileFormat> formats)
    {
        while (getException() is { } exception)
        {
            Logger.Warn(exception, "Unable to load {0} resource {1}.", resourceName, getPath());
            var choice = UserDialogs.Show(owner,
                $"The {resourceName} file '{settings.MakeAbsolute(getPath())}' could not be loaded: {exception.Message}.\nDo you want to load a substituting file now?",
                "Open project", DialogButtons.YesNoCancel, DialogIcon.Warning, DialogDefaultButton.Button2);

            if (choice == DialogChoice.No)
                return;

            if (choice != DialogChoice.Yes)
                throw new OperationCanceledException($"Canceled because the {resourceName} file was not loadable.");

            var path = LevelFileDialog.BrowseFile(owner, settings, getPath(), title, formats, VariableType.LevelDirectory, false);

            if (path is not null)
                setPath(path);
        }
    }
}
