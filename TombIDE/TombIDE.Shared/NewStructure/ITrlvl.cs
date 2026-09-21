namespace TombIDE.Shared.NewStructure
{
	public interface ITrlvl
	{
		string FileFormatVersion { get; }

		void WriteToFile(string filePath);
	}
}
