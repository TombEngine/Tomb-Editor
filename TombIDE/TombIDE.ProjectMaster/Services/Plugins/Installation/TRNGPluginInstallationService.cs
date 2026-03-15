using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using TombIDE.ProjectMaster.Services.FileExtraction;
using TombIDE.ProjectMaster.Services.Plugins.Metadata;
using TombIDE.ProjectMaster.Services.Plugins.Models;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Plugins.Installation;

public sealed class TRNGPluginInstallationService : IPluginInstallationService
{
	private const string PluginDllPattern = "plugin_*.dll";
	private const string PluginDllRegexPattern = @"plugin_.*\.dll";

	private readonly IPluginMetadataService _metadataService;
	private readonly IFileExtractionService _fileExtractionService;

	public TRNGPluginInstallationService(IPluginMetadataService metadataService, IFileExtractionService fileExtractionService)
	{
		_metadataService = metadataService;
		_fileExtractionService = fileExtractionService;
	}

	public PluginInfo InstallPlugin(IGameProject project, PluginInstallationSource source) => source.Type switch
	{
		PluginInstallationSourceType.Archive => InstallFromArchive(project, source.Path),
		PluginInstallationSourceType.Folder => InstallFromFolder(project, source.Path),
		_ => throw new NotSupportedException($"Installation source type '{source.Type}' is not supported.")
	};

	public void RemovePlugin(IGameProject project, PluginInfo plugin)
	{
		// Delete DLL file from engine directory
		string engineDllFilePath = Path.Combine(project.GetEngineRootDirectoryPath(), plugin.DllFileName);

		if (File.Exists(engineDllFilePath))
			FileSystem.DeleteFile(engineDllFilePath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

		// Delete plugin directory
		if (Directory.Exists(plugin.DirectoryPath))
			FileSystem.DeleteDirectory(plugin.DirectoryPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
	}

	private PluginInfo InstallFromArchive(IGameProject project, string archivePath)
	{
		var pluginsDirectory = new DirectoryInfo(project.PluginsDirectoryPath);

		if (!pluginsDirectory.Exists)
		{
			pluginsDirectory.Create();
			pluginsDirectory = new DirectoryInfo(pluginsDirectory.FullName);
		}

		using var fileStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
		using var archive = new ZipArchive(fileStream);

		IReadOnlyList<ZipArchiveEntry> dllFileEntries = archive.Entries
			.Where(entry => Regex.IsMatch(entry.Name, PluginDllRegexPattern, RegexOptions.IgnoreCase))
			.ToList();

		if (dllFileEntries.Count == 0)
			throw new ArgumentException("Selected archive doesn't contain a valid plugin DLL file.");
		else if (dllFileEntries.Count > 1)
			throw new ArgumentException("Selected archive contains more than one valid plugin .dll file.");

		ZipArchiveEntry dllFileEntry = dllFileEntries[0];
		string dllFileName = dllFileEntry.Name;

		// Find the sub-path of the DLL within the archive, as the plugin .dll may not be at the root
		string dllSubPath = dllFileEntry.FullName[..^dllFileName.Length];

		string unzipDirectoryPath = Path.Combine(pluginsDirectory.FullName, Path.GetFileNameWithoutExtension(dllFileName));

		IEnumerable<ZipArchiveEntry> entriesToExtract = archive.Entries.Where(entry =>
			entry.FullName.StartsWith(dllSubPath, StringComparison.OrdinalIgnoreCase));

		_fileExtractionService.ExtractEntries(entriesToExtract, unzipDirectoryPath, subPathToTrim: dllSubPath);

		return _metadataService.ReadPluginMetadata(unzipDirectoryPath, dllFileName);
	}

	private PluginInfo InstallFromFolder(IGameProject project, string folderPath)
	{
		var pluginsDirectory = new DirectoryInfo(project.PluginsDirectoryPath);

		if (!pluginsDirectory.Exists)
		{
			pluginsDirectory.Create();
			pluginsDirectory = new DirectoryInfo(pluginsDirectory.FullName);
		}

		var selectedDir = new DirectoryInfo(folderPath);

		FileInfo[] dllFiles = selectedDir.GetFiles(PluginDllPattern, System.IO.SearchOption.TopDirectoryOnly);

		if (dllFiles.Length == 0)
			throw new ArgumentException("Selected folder doesn't contain a valid plugin DLL file.");
		else if (dllFiles.Length > 1)
			throw new ArgumentException("Selected folder contains more than one valid plugin DLL file.");

		FileInfo dllFile = dllFiles[0];

		string dllFileName = dllFile.Name;
		string dllSubPath = dllFile.FullName[..^dllFileName.Length];

		string copyDirectoryPath = Path.Combine(pluginsDirectory.FullName, Path.GetFileNameWithoutExtension(dllFileName));

		IEnumerable<FileInfo> filesToCopy = selectedDir.GetFiles("*", System.IO.SearchOption.TopDirectoryOnly)
			.Where(file => file.FullName.StartsWith(dllSubPath, StringComparison.OrdinalIgnoreCase));

		_fileExtractionService.CopyFilesToDirectory(filesToCopy, copyDirectoryPath, subPathToTrim: dllSubPath);

		return _metadataService.ReadPluginMetadata(copyDirectoryPath, dllFileName);
	}
}
