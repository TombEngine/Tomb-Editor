using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Scripting.ClassicScript.Compilers;

/// <summary>
/// Resolves the ClassicScript compiler installation paths: the NGC/VGE toolchain, the TR4
/// DOSBox compiler, and the Windows system-library files required for registration. Paths are
/// computed relative to the supplied application root, which defaults to the application base
/// directory. Windows-specific registration behavior intentionally lives here rather than in
/// the shared scripting path helper.
/// </summary>
internal sealed class ClassicScriptCompilerPaths
{
	/// <summary>
	/// Gets the default compiler path layout rooted at the application base directory.
	/// </summary>
	public static ClassicScriptCompilerPaths Default { get; } = new(AppContext.BaseDirectory);

	private readonly string _programDirectory;

	/// <summary>
	/// Initializes a new instance rooted at the supplied program directory.
	/// </summary>
	/// <param name="programDirectory">The application root directory.</param>
	public ClassicScriptCompilerPaths(string programDirectory)
		=> _programDirectory = programDirectory;

	/// <summary>
	/// Gets the application root directory.
	/// </summary>
	public string ProgramDirectory => _programDirectory;

	/// <summary>
	/// Gets the TIDE subdirectory that hosts the DOSBox and NGC toolchains.
	/// </summary>
	public string TIDEDirectory => Path.Combine(_programDirectory, "TIDE");

	/// <summary>
	/// Gets the DOSBox directory.
	/// </summary>
	public string DOSDirectory => Path.Combine(TIDEDirectory, "DOS");

	/// <summary>
	/// Gets the TR4 script compiler directory.
	/// </summary>
	public string TR4ScriptCompilerDirectory => Path.Combine(DOSDirectory, "TR4");

	/// <summary>
	/// Gets the DOSBox executable path.
	/// </summary>
	public string DOSBoxExecutable => Path.Combine(DOSDirectory, "DOSBox.exe");

	/// <summary>
	/// Gets the internal NGC toolchain directory.
	/// </summary>
	public string InternalNGCDirectory => Path.Combine(TIDEDirectory, "NGC");

	/// <summary>
	/// Gets the NGC center executable path.
	/// </summary>
	public string NGCExecutable => Path.Combine(InternalNGCDirectory, "NG_Center.exe");

	/// <summary>
	/// Gets the Virtual Game Engine directory.
	/// </summary>
	public string VGEDirectory => Path.Combine(InternalNGCDirectory, "VGE");

	/// <summary>
	/// Gets the VGE script staging directory.
	/// </summary>
	public string VGEScriptDirectory => Path.Combine(VGEDirectory, "Script");

	/// <summary>
	/// Gets the library registration executable path.
	/// </summary>
	public string LibraryRegistrationExecutable => Path.Combine(_programDirectory, "TombIDE Library Registration.exe");

	/// <summary>
	/// Gets the path of the MSComctl.ocx system file.
	/// </summary>
	public string MscomctlSystemFile => Path.Combine(SystemDirectory, "Mscomctl.ocx");

	/// <summary>
	/// Gets the path of the Richtx32.ocx system file.
	/// </summary>
	public string Richtx32SystemFile => Path.Combine(SystemDirectory, "Richtx32.ocx");

	/// <summary>
	/// Gets the path of the PicFormat32.ocx system file.
	/// </summary>
	public string PicFormat32SystemFile => Path.Combine(SystemDirectory, "PicFormat32.ocx");

	/// <summary>
	/// Gets the path of the Comdlg32.ocx system file.
	/// </summary>
	public string Comdlg32SystemFile => Path.Combine(SystemDirectory, "Comdlg32.ocx");

	private static string SystemDirectory
	{
		get
		{
			var path = new StringBuilder(260);
			SHGetSpecialFolderPath(IntPtr.Zero, path, 0x0029, false);

			return path.ToString();
		}
	}

	[DllImport("shell32.dll")]
	private static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszPath, int nFolder, bool fCreate);
}
