#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombLib.Utils;

public class LevelFileDialog
{
    // Returns 'null' if no file is chosen.
    // "currentPath" can be a file path or a directory path containing placeholders (or null).
    public static string? BrowseFile(IWin32Window owner, LevelSettings? levelSettings, string? currentPath, string title, IEnumerable<FileFormat> fileFormats, VariableType? baseDirType, bool save)
    {
        using (FileDialog dialog = save ? (FileDialog)new SaveFileDialog() : new OpenFileDialog())
        {
            dialog.Filter = fileFormats.GetFilter();
            dialog.Title = title;
            if (!string.IsNullOrWhiteSpace(currentPath))
                try
                {
                    string path = levelSettings?.MakeAbsolute(currentPath) ?? currentPath;
                    if (Directory.Exists(path))
                        dialog.InitialDirectory = path;
                    else
                    {
                        dialog.FileName = Path.GetFileName(path);
                        dialog.InitialDirectory = Path.GetDirectoryName(path) ?? string.Empty;
                    }
                }
                catch (Exception)
                { }
            if (dialog.ShowDialog(owner) != DialogResult.OK)
                return null;
            return baseDirType is not null && levelSettings is not null ? levelSettings.MakeRelative(dialog.FileName, baseDirType.Value) : dialog.FileName;
        }
    }

    public static string? BrowseFile(IWin32Window owner, string title, IEnumerable<FileFormat> fileFormats, bool save)
    {
        using (FileDialog dialog = save ? (FileDialog)new SaveFileDialog() : new OpenFileDialog())
        {
            dialog.Filter = fileFormats.GetFilter();
            dialog.Title = title;
            if (dialog.ShowDialog(owner) != DialogResult.OK)
                return null;
            return dialog.FileName;
        }
    }

    // Returns an empty collection if no folder is chosen.
    // "currentPath" should be a directory path containing placeholders (or null).
    public static IEnumerable<string> BrowseFiles(IWin32Window owner, LevelSettings? levelSettings, string? currentPath, string title, IEnumerable<FileFormat> fileFormats, VariableType? baseDirType = null)
    {
        using (OpenFileDialog dialog = new OpenFileDialog())
        {
            dialog.Multiselect = true;
            dialog.Filter = fileFormats.GetFilter();
            dialog.Title = title;
            if (!string.IsNullOrWhiteSpace(currentPath))
                dialog.InitialDirectory = levelSettings?.MakeAbsolute(currentPath) ?? currentPath;
            if (dialog.ShowDialog(owner) != DialogResult.OK)
                return new string[0];
            return baseDirType is not null && levelSettings is not null ? dialog.FileNames.Select(fileName => levelSettings.MakeRelative(fileName, baseDirType.Value)) : dialog.FileNames;
        }
    }

    // Returns 'null' if no folder is chosen.
    // "currentPath" should be a directory path containing placeholders (or null).
    public static string? BrowseFolder(IWin32Window owner, LevelSettings? levelSettings, string? currentPath, string title, VariableType? baseDirType)
    {
        using (var dialog = new BrowseFolderDialog())
        {
            dialog.Title = title;
            if (!string.IsNullOrWhiteSpace(currentPath))
                try
                {
                    dialog.InitialFolder = levelSettings?.MakeAbsolute(currentPath) ?? currentPath;
                }
                catch (Exception)
                { }
            if (dialog.ShowDialog(owner) != DialogResult.OK)
                return null;
            return baseDirType is not null && levelSettings is not null ? levelSettings.MakeRelative(dialog.Folder, baseDirType.Value) : dialog.Folder;
        }
    }
}
