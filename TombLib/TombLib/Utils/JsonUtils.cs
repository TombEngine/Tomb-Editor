using System.IO;
using System.Text.Json;

namespace TombLib.Utils
{
	public static class JsonUtils
	{
		private static readonly JsonSerializerOptions ReadOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			AllowTrailingCommas = true,
			ReadCommentHandling = JsonCommentHandling.Skip
		};

		public static T ReadJsonFile<T>(string filePath)
		{
			string json = File.ReadAllText(filePath);

			return JsonSerializer.Deserialize<T>(json, ReadOptions)
				?? throw new InvalidDataException($"The JSON file '{filePath}' did not contain an object of type '{typeof(T).Name}'.");
		}
	}
}
