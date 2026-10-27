using System.Diagnostics;
using System.IO;
using System.Linq;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.Level.Compile;

public sealed class LevelCompileService : ILevelCompileService
{
	public bool RebuildAllLevels(IGameProject project)
	{
		ILevelProject[] levels = project.GetAllValidLevelProjects();

		if (levels.Length == 0)
			return false;

		var batchList = new BatchCompileList();
		batchList.Files.AddRange(levels.Select(GetPrj2Path));

		ExecuteBatchCompile(batchList);
		return true;
	}

	public void RebuildLevel(ILevelProject level)
	{
		var batchList = new BatchCompileList();
		batchList.Files.Add(GetPrj2Path(level));

		ExecuteBatchCompile(batchList);
	}

	/// <summary>
	/// Gets the full path to the .prj2 file for the specified level.
	/// Uses the target filename if specified, otherwise uses the most recently modified .prj2 file.
	/// </summary>
	private static string GetPrj2Path(ILevelProject level)
	{
		string fileName = level.TargetPrj2FileName ?? level.GetMostRecentlyModifiedPrj2FileName();
		return Path.Combine(level.DirectoryPath, fileName);
	}

	/// <summary>
	/// Executes a batch compile by saving the batch list to a temporary XML file and launching TombEditor.exe.
	/// </summary>
	private static void ExecuteBatchCompile(BatchCompileList batchList)
	{
		string batchListFilePath = Path.Combine(Path.GetTempPath(), "tide_batch.xml");
		BatchCompileList.SaveToXml(batchListFilePath, batchList);

		var startInfo = new ProcessStartInfo
		{
			FileName = Path.Combine(DefaultPaths.ProgramDirectory, "TombEditor.exe"),
			Arguments = $"\"{batchListFilePath}\"",
			WorkingDirectory = DefaultPaths.ProgramDirectory,
			UseShellExecute = true
		};

		Process.Start(startInfo);
	}
}
