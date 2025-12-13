using System.IO;
using System.Threading.Tasks;

namespace TombLib.Utils;

/// <summary>
/// Provides utility methods for working with files, including retry mechanisms for common file operations.
/// </summary>
public static class FileUtils
{
	/// <summary>
	/// Attempts to copy a file with multiple retries if the operation fails due to IO exceptions.
	/// </summary>
	/// <param name="sourceFile">The path of the source file to copy.</param>
	/// <param name="destFile">The path where the file should be copied to.</param>
	/// <param name="maxAttempts">The maximum number of attempts to try copying the file. Default is 5.</param>
	/// <param name="delayMs">The delay in milliseconds between retry attempts. Default is 1000ms.</param>
	/// <returns><see langword="true" /> if the file was successfully copied; otherwise, <see langword="false" />.</returns>
	public static async Task<bool> TryCopyFileWithRetryAsync(string sourceFile, string destFile, int maxAttempts = 5, int delayMs = 1000)
	{
		for (int attempt = 0; attempt < maxAttempts; attempt++)
		{
			try
			{
				File.Copy(sourceFile, destFile);
				return true;
			}
			catch (IOException)
			{
				if (attempt == maxAttempts - 1)
					return false;

				await Task.Delay(delayMs);
			}
		}

		return false;
	}

	/// <summary>
	/// Attempts to delete a file with multiple retries if the operation fails due to IO exceptions.
	/// </summary>
	/// <param name="filePath">The path of the file to delete.</param>
	/// <param name="maxAttempts">The maximum number of attempts to try deleting the file. Default is 5.</param>
	/// <param name="delayMs">The delay in milliseconds between retry attempts. Default is 1000ms.</param>
	/// <returns>A task representing the asynchronous delete operation.</returns>
	public static async Task TryDeleteFileWithRetryAsync(string filePath, int maxAttempts = 5, int delayMs = 1000)
	{
		if (!File.Exists(filePath))
			return;

		for (int attempt = 0; attempt < maxAttempts; attempt++)
		{
			try
			{
				File.Delete(filePath);
				return;
			}
			catch (IOException)
			{
				if (attempt == maxAttempts - 1)
					break;

				await Task.Delay(delayMs);
			}
		}
	}
}
