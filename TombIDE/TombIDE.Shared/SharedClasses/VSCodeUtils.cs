using CustomMessageBox.WPF;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Interop;
using IWinFormsWindow = System.Windows.Forms.IWin32Window;

namespace TombIDE.Shared.SharedClasses;

/// <summary>
/// Provides utility methods for interacting with Visual Studio Code.
/// </summary>
public static class VSCodeUtils
{
	private const string VSCODE_PATH_VARIABLE = "code";

	private static readonly string[] _possiblePaths = new[]
	{
		Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Programs\Microsoft VS Code\bin\code"),
		Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Microsoft VS Code\bin\code"),
		Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft VS Code\bin\code")
	};

	/// <summary>
	/// Opens the specified directory in Visual Studio Code.
	/// </summary>
	/// <param name="config">The IDE configuration.</param>
	/// <param name="directoryPath">The directory path to open in VSCode.</param>
	public static void OpenDirectoryInVSCode(IWinFormsWindow dialogOwner, IDEConfiguration config, string directoryPath)
	{
		bool isValidVSCodeExecutable = !string.IsNullOrWhiteSpace(config.VSCodePath)
			&& File.Exists(config.VSCodePath)
			&& Path.GetFileName(config.VSCodePath) == VSCODE_PATH_VARIABLE;

		string codePath = isValidVSCodeExecutable ? config.VSCodePath : FindVSCodePath();

		if (codePath is null)
		{
			codePath = PromptForVSCodeInstallation(dialogOwner);

			if (codePath is null)
				return; // User cancelled the operation
		}

		// Store the verified VS Code path for future use
		if (!string.IsNullOrWhiteSpace(codePath) && (string.IsNullOrWhiteSpace(config.VSCodePath) || config.VSCodePath != codePath))
		{
			config.VSCodePath = codePath;
			config.Save();
		}

		CheckAndInstallLuaExtension(dialogOwner, config, codePath);

		// Open VSCode with the directory
		Process.Start(new ProcessStartInfo
		{
			FileName = "cmd.exe",
			Arguments = BuildCmdArguments(codePath, $"\"{directoryPath}\""),
			UseShellExecute = false,
			CreateNoWindow = true
		});
	}

	/// <summary>
	/// Finds the VSCode executable path from common installation locations.
	/// </summary>
	/// <returns>The path to the VSCode executable, or <see langword="null" /> if not found.</returns>
	private static string FindVSCodePath()
	{
		if (IsValidVSCodeExecutable(VSCODE_PATH_VARIABLE))
			return VSCODE_PATH_VARIABLE; // Assume 'code' is in PATH

		string possiblePath = _possiblePaths.FirstOrDefault(File.Exists);

		if (possiblePath is not null && IsValidVSCodeExecutable(possiblePath))
			return possiblePath;

		return null;
	}

	/// <summary>
	/// Prompts the user to install VSCode or select a custom VSCode installation.
	/// </summary>
	/// <returns>The path to the VSCode executable, or <see langword="null" /> if the user cancelled.</returns>
	private static string PromptForVSCodeInstallation(IWinFormsWindow dialogOwner)
	{
		var messageBox = new CMessageBox(
			"Visual Studio Code is not installed on this system. Would you like to install it now?\n\n" +
			"If you're using a portable version of VSCode, or it was installed it in a custom directory,\n" +
			"please use the \"Select Code.exe file...\" option to select a custom \"Code.exe\" file location.",
			"VSCode not found", CMessageBoxIcon.Question);

		SetMessageBoxOwner(messageBox, dialogOwner);

		CMessageBoxResult result = messageBox.Show(
			new CMessageBoxButton<CMessageBoxResult>("Yes", CMessageBoxResult.Yes),
			new CMessageBoxButton<CMessageBoxResult>("No", CMessageBoxResult.No),
			new CMessageBoxButton<CMessageBoxResult>("Select Code.exe file...", CMessageBoxResult.Continue));

		if (result == CMessageBoxResult.Yes)
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = "https://code.visualstudio.com/",
				UseShellExecute = true
			});

			return null;
		}
		else if (result == CMessageBoxResult.Continue)
		{
			return SelectCustomVSCodeExecutable(dialogOwner);
		}

		return null;
	}

	/// <summary>
	/// Allows the user to browse for a custom VSCode executable.
	/// </summary>
	/// <returns>The path to the custom VSCode executable, or <see langword="null" /> if the selection was invalid or cancelled.</returns>
	private static string SelectCustomVSCodeExecutable(IWinFormsWindow dialogOwner)
	{
		using var dialog = new OpenFileDialog
		{
			Title = "Select Visual Studio Code's \"Code.exe\" File",
			Filter = "Executable Files (*.exe)|*.exe",
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
		};

		if (dialog.ShowDialog() != DialogResult.OK || !File.Exists(dialog.FileName))
			return null;

		string codePath = dialog.FileName;

		// Verify if the selected file is indeed VSCode
		string fileName = Path.GetFileName(codePath);

		if (fileName != "Code.exe")
		{
			ShowInvalidExecutableError(dialogOwner);
			return null;
		}

		// Select the `code` file inside the /bin/ directory
		codePath = Path.Combine(Path.GetDirectoryName(codePath), "bin", VSCODE_PATH_VARIABLE);

		if (!File.Exists(codePath) || !IsValidVSCodeExecutable(codePath))
		{
			ShowInvalidExecutableError(dialogOwner);
			return null;
		}

		return codePath;
	}

	/// <summary>
	/// Verifies that the given path points to a valid VSCode executable.
	/// </summary>
	/// <param name="codePath">The path to the VSCode executable to verify.</param>
	/// <returns><see langword="true" /> if the path is a valid VSCode executable, <see langword="false" /> otherwise.</returns>
	private static bool IsValidVSCodeExecutable(string codePath)
	{
		var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "cmd.exe",
				Arguments = BuildCmdArguments(codePath, "--version"),
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			}
		};

		process.Start();
		string output = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		return !string.IsNullOrWhiteSpace(output);
	}

	/// <summary>
	/// Constructs the command line arguments for starting VSCode.
	/// </summary>
	/// <param name="codePath">The path to the VSCode executable.</param>
	/// <param name="codeArguments">The arguments to pass to VSCode.</param>
	/// <returns>The command line arguments for starting VSCode.</returns>
	private static string BuildCmdArguments(string codePath, string codeArguments) => codePath == VSCODE_PATH_VARIABLE
		? $"/c \"{VSCODE_PATH_VARIABLE} {codeArguments}\"" // Use the PATH variable directly
		: $"/c \"\"{codePath}\" {codeArguments}\""; // Use the full path to the executable

	/// <summary>
	/// Shows an error message indicating that the selected file is not a valid VSCode executable.
	/// </summary>
	private static void ShowInvalidExecutableError(IWinFormsWindow dialogOwner)
	{
		var messageBox = new CMessageBox(
			"The selected file is not a valid Visual Studio Code executable.\n" +
			"Please select the \"Code.exe\" file located in the VSCode installation directory.",
			"Invalid executable", CMessageBoxIcon.Error);

		SetMessageBoxOwner(messageBox, dialogOwner);
		messageBox.Show(CMessageBoxButtons.OK);
	}

	/// <summary>
	/// Sets the owner of the message box to the specified WinForms window.
	/// </summary>
	/// <param name="messageBox">Message box to set the owner for.</param>
	/// <param name="owner">WinForms owner window to set for the message box.</param>
	private static void SetMessageBoxOwner(CMessageBox messageBox, IWinFormsWindow owner)
	{
		var interopHelper = new WindowInteropHelper(messageBox);
		interopHelper.Owner = owner.Handle;
	}

	/// <summary>
	/// Checks if the Lua extension is installed in VSCode and offers to install it if it's not.
	/// </summary>
	/// <param name="config">The IDE configuration.</param>
	/// <param name="codePath">The path to the VSCode executable.</param>
	private static void CheckAndInstallLuaExtension(IWinFormsWindow dialogOwner, IDEConfiguration config, string codePath)
	{
		if (config.DoNotAskToInstallLuaExtension)
			return;

		// Check if Lua extension is already installed
		if (IsLuaExtensionInstalled(codePath))
			return;

		var messageBox = new CMessageBox(
			"The Lua extension for Visual Studio Code is not installed.\n" +
			"Would you like to install sumneko.lua now?",
			"Lua extension not found", CMessageBoxIcon.Question);

		SetMessageBoxOwner(messageBox, dialogOwner);

		CMessageBoxResult result = messageBox.Show(
			new CMessageBoxButton<CMessageBoxResult>("Yes", CMessageBoxResult.Yes),
			new CMessageBoxButton<CMessageBoxResult>("No", CMessageBoxResult.No),
			new CMessageBoxButton<CMessageBoxResult>("Don't Ask Again", CMessageBoxResult.Ignore));

		if (result == CMessageBoxResult.Yes)
		{
			InstallLuaExtension(codePath);

			config.DoNotAskToInstallLuaExtension = true;
			config.Save();
		}
		else if (result == CMessageBoxResult.Ignore)
		{
			config.DoNotAskToInstallLuaExtension = true;
			config.Save();
		}
	}

	/// <summary>
	/// Checks if the Lua extension is installed in VSCode.
	/// </summary>
	/// <param name="codePath">The path to the VSCode executable.</param>
	/// <returns>True if the Lua extension is installed, false otherwise.</returns>
	private static bool IsLuaExtensionInstalled(string codePath)
	{
		try
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = BuildCmdArguments(codePath, "--list-extensions"),
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};

			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			return output.Split('\n').Any(line
				=> line.Trim().Equals("sumneko.lua", StringComparison.OrdinalIgnoreCase)
				|| line.Trim().Equals("Lua", StringComparison.OrdinalIgnoreCase));
		}
		catch
		{
			return false; // Assume not installed in case of errors
		}
	}

	/// <summary>
	/// Installs the Lua extension for VSCode.
	/// </summary>
	/// <param name="codePath">The path to the VSCode executable.</param>
	private static void InstallLuaExtension(string codePath)
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = "cmd.exe",
			Arguments = BuildCmdArguments(codePath, "--install-extension sumneko.lua"),
			UseShellExecute = false,
			CreateNoWindow = true
		});
	}
}
