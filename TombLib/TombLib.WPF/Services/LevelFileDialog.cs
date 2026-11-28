#nullable enable

using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.Utils;

namespace TombLib.WPF.Services;

/// <summary>
/// Selects project files and folders while preserving level path variables.
/// </summary>
public static class LevelFileDialog
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>Chooses a file and optionally converts its path to a level variable.</summary>
    public static string? BrowseFile(IWindowOwner? owner, LevelSettings? levelSettings, string? currentPath, string title,
        IEnumerable<FileFormat> fileFormats, VariableType? baseDirType, bool save)
    {
        FileDialog dialog = save ? new SaveFileDialog() : new OpenFileDialog();
        dialog.Filter = fileFormats.GetFilter();
        dialog.Title = title;
        SetInitialPath(dialog, levelSettings, currentPath);

        return ShowDialog(dialog, owner) ? MakeRelative(dialog.FileName, levelSettings, baseDirType) : null;
    }

    /// <summary>Chooses a file using an absolute path.</summary>
    public static string? BrowseFile(IWindowOwner? owner, string title, IEnumerable<FileFormat> fileFormats, bool save)
        => BrowseFile(owner, null, null, title, fileFormats, null, save);

    /// <summary>Chooses multiple files and optionally converts their paths to level variables.</summary>
    public static IEnumerable<string> BrowseFiles(IWindowOwner? owner, LevelSettings? levelSettings, string? currentPath,
        string title, IEnumerable<FileFormat> fileFormats, VariableType? baseDirType = null)
    {
        var dialog = new OpenFileDialog { Multiselect = true, Filter = fileFormats.GetFilter(), Title = title };
        SetInitialPath(dialog, levelSettings, currentPath);

        return ShowDialog(dialog, owner) ? dialog.FileNames.Select(path => MakeRelative(path, levelSettings, baseDirType)) : [];
    }

    /// <summary>Chooses a folder and optionally converts its path to a level variable.</summary>
    public static string? BrowseFolder(IWindowOwner? owner, LevelSettings? levelSettings, string? currentPath, string title,
        VariableType? baseDirType)
    {
        using var dialog = new BrowseFolderDialog { Title = title, InitialFolder = GetAbsolutePath(levelSettings, currentPath) };

        return dialog.ShowDialog(owner) == DialogChoice.OK ? MakeRelative(dialog.Folder, levelSettings, baseDirType) : null;
    }

    private static string MakeRelative(string path, LevelSettings? levelSettings, VariableType? baseDirType)
        => baseDirType is not null && levelSettings is not null ? levelSettings.MakeRelative(path, baseDirType.Value) : path;

    private static string? GetAbsolutePath(LevelSettings? levelSettings, string? currentPath)
    {
        if (string.IsNullOrWhiteSpace(currentPath))
            return null;

        try
        {
            return levelSettings?.MakeAbsolute(currentPath) ?? currentPath;
        }
        catch (Exception exception)
        {
            Logger.Warn(exception, "Unable to resolve the initial dialog path.");
            return null;
        }
    }

    private static void SetInitialPath(FileDialog dialog, LevelSettings? levelSettings, string? currentPath)
    {
        var path = GetAbsolutePath(levelSettings, currentPath);

        if (string.IsNullOrWhiteSpace(path))
            return;

        if (Directory.Exists(path))
        {
            dialog.InitialDirectory = path;
        }
        else
        {
            dialog.FileName = Path.GetFileName(path);
            dialog.InitialDirectory = Path.GetDirectoryName(path) ?? string.Empty;
        }
    }

    private static bool ShowDialog(CommonDialog dialog, IWindowOwner? owner) => dialog.ShowDialog(owner) == true;
}
