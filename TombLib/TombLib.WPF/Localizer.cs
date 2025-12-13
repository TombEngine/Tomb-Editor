using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace TombLib.WPF;

/// <summary>
/// Provides localization support by loading language files and retrieving localized strings.
/// <para>Supports JSON-based localization files with nested key structures.</para>
/// </summary>
public sealed class Localizer : INotifyPropertyChanged
{
	private static readonly Lazy<Localizer> _instance = new(() => new Localizer());

	/// <summary>
	/// Gets the singleton instance of the Localizer.
	/// </summary>
	public static Localizer Instance => _instance.Value;

	private const string IndexerName = "Item";
	private const string IndexerArrayName = "Item[]";

	private readonly object _lockObject = new();

	private readonly HashSet<string> _preloadedFiles = new()
	{
		"Common",
		"TombEditor",
		"TombIDE",
		"TombLib",
		"WadTool"
	};

	/// <summary>
	/// Gets the localized string for the specified key.
	/// Returns a fallback string in the format "{Language}:{key}" if the key is not found.
	/// </summary>
	/// <param name="key">The localization key in the format "FileName.NestedKey" or "FileName.Parent.Child".</param>
	/// <returns>The localized string value, or a fallback string if not found.</returns>
	public string this[string key]
	{
		get
		{
			if (string.IsNullOrWhiteSpace(key))
				return CreateFallbackString(key ?? string.Empty);

			string? value = GetLocalizedValue(key);

			return value is not null
				? ProcessLocalizedValue(value)
				: CreateFallbackString(key);
		}
	}

	/// <summary>
	/// Gets the currently loaded language identifier.
	/// </summary>
	public string? Language { get; private set; }

	/// <summary>
	/// Gets a value indicating whether a language is currently loaded.
	/// </summary>
	public bool IsLanguageLoaded => Language is not null && _languageDirectory is not null;

	private string? _languageDirectory;

	private readonly Dictionary<string, JsonElement> _fileCache = new();

	/// <summary>
	/// Loads the specified language and its associated localization files.
	/// </summary>
	/// <param name="language">The language identifier (e.g., "EN", "FR", "DE").</param>
	/// <returns><see langword="true" /> if the language was loaded successfully; otherwise, <see langword="false" />.</returns>
	/// <exception cref="ArgumentException">Thrown when language is <see langword="null" /> or whitespace.</exception>
	public bool LoadLanguage(string language)
	{
		if (string.IsNullOrWhiteSpace(language))
			throw new ArgumentException("Language cannot be null or whitespace.", nameof(language));

		string languageDirectory = GetLanguageDirectory(language);

		if (!Directory.Exists(languageDirectory))
			return false;

		lock (_lockObject)
		{
			Language = language.ToUpperInvariant();

			_languageDirectory = languageDirectory;
			_fileCache.Clear();

			LoadPreloadedFiles();
			Invalidate();

			return true;
		}
	}

	/// <summary>
	/// Loads a specific localization file into the cache.
	/// </summary>
	/// <param name="fileName">The name of the file to load (without extension).</param>
	/// <returns><see langword="true" /> if the file was loaded successfully; otherwise, <see langword="false" />.</returns>
	/// <exception cref="ArgumentException">Thrown when fileName is <see langword="null" /> or whitespace.</exception>
	/// <exception cref="InvalidOperationException">Thrown when no language is currently loaded.</exception>
	public bool LoadFile(string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
			throw new ArgumentException("File name cannot be null or whitespace.", nameof(fileName));

		if (!IsLanguageLoaded)
			throw new InvalidOperationException("No language is currently loaded. Call LoadLanguage first.");

		lock (_lockObject)
			return LoadFileIntoCache(fileName);
	}

	/// <summary>
	/// Checks if a specific file is loaded in the cache.
	/// </summary>
	/// <param name="fileName">The name of the file to check.</param>
	/// <returns><see langword="true" /> if the file is loaded; otherwise, <see langword="false" />.</returns>
	public bool IsFileLoaded(string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
			return false;

		lock (_lockObject)
			return _fileCache.ContainsKey(fileName);
	}

	/// <summary>
	/// Gets all currently loaded file names.
	/// </summary>
	/// <returns>A collection of loaded file names.</returns>
	public IReadOnlyCollection<string> GetLoadedFiles()
	{
		lock (_lockObject)
		{
			return _fileCache.Keys.ToList().AsReadOnly();
		}
	}

	/// <summary>
	/// Clears all cached localization data.
	/// </summary>
	public void ClearCache()
	{
		lock (_lockObject)
		{
			_fileCache.Clear();
			Invalidate();
		}
	}

	/// <summary>
	/// Reloads the current language, refreshing all cached data.
	/// </summary>
	/// <returns><see langword="true" /> if the language was reloaded successfully; otherwise, <see langword="false" />.</returns>
	public bool ReloadLanguage()
	{
		string? currentLanguage = Language;
		return currentLanguage is not null && LoadLanguage(currentLanguage);
	}

	private static string GetLanguageDirectory(string language)
	{
		return Path.Combine(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
			"Resources",
			"Localization",
			language.ToUpperInvariant()
		);
	}

	private void LoadPreloadedFiles()
	{
		foreach (string fileName in _preloadedFiles)
			LoadFileIntoCache(fileName);
	}

	private bool LoadFileIntoCache(string fileName)
	{
		if (_languageDirectory is null)
			return false;

		string filePath = Path.Combine(_languageDirectory, $"{fileName}.json");

		if (File.Exists(filePath) && LoadJsonFile(filePath, out JsonElement element))
		{
			_fileCache[fileName] = element;
			return true;
		}

		return false;
	}

	private static bool LoadJsonFile(string filePath, out JsonElement rootElement)
	{
		try
		{
			string fileContent = File.ReadAllText(filePath);

			using JsonDocument document = JsonDocument.Parse(fileContent,
				new JsonDocumentOptions
				{
					CommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true
				}
			);

			rootElement = document.RootElement.Clone();
			return true;
		}
		catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
		{
			rootElement = default;
			return false;
		}
	}

	private string? GetLocalizedValue(string key)
	{
		string[] keyParts = key.Split('.');

		if (keyParts.Length < 2)
			return null; // Need at least "FileName.Key"

		string potentialFileName = keyParts[0];

		lock (_lockObject)
		{
			// Try to load the file if it's not in cache
			if (!_fileCache.TryGetValue(potentialFileName, out JsonElement fileElement))
			{
				if (!LoadFileIntoCache(potentialFileName))
					return null; // File couldn't be loaded

				// Try again after loading
				if (!_fileCache.TryGetValue(potentialFileName, out fileElement))
					return null;
			}

			// Remove the file name part from the key and look in the file
			string remainingKey = string.Join(".", keyParts[1..]);
			return GetNestedValue(fileElement, remainingKey);
		}
	}

	private static string? GetNestedValue(JsonElement element, string key)
	{
		if (string.IsNullOrEmpty(key))
			return null;

		string[] keyParts = key.Split('.');
		JsonElement currentElement = element;

		foreach (string keyPart in keyParts)
		{
			if (string.IsNullOrWhiteSpace(keyPart))
				return null;

			if (currentElement.TryGetProperty(keyPart, out JsonElement nextElement))
				currentElement = nextElement;
			else
				return null;
		}

		return currentElement.ValueKind is JsonValueKind.String
			? currentElement.GetString()
			: null;
	}

	private static string ProcessLocalizedValue(string value)
		=> value.Replace("\\n", "\n")
				.Replace("\\t", "\t")
				.Replace("\\r", "\r");

	private static string CreateFallbackString(string key)
		=> key.Split('.')[^1];

	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Notifies all binding clients that the localized strings may have changed.
	/// </summary>
	public void Invalidate()
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
	}
}
