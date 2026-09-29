namespace TombIDE.Shared.NewStructure
{
	public interface IProject
	{
		/// <summary>
		/// Display name of the project.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Path to the project's root directory.
		/// </summary>
		string DirectoryPath { get; }

		/// <summary>
		/// Determines whether the project is valid or not.
		/// </summary>
		bool IsValid(out string errorMessage);

		/// <summary>
		/// Renames the project and its directory if specified.
		/// </summary>
		void Rename(string newName, bool renameDirectory = false, bool renameTrprojFile = false);

		/// <summary>
		/// Saves the project's settings.
		/// </summary>
		void Save();
	}
}
