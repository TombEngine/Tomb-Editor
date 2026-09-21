using NLog;
using System;
using System.IO;
using TombLib.Utils;

namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Provides the XML-based load and save surface shared by editor configurations.
/// </summary>
public abstract class ConfigurationBase
{
	private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Gets the default path used when the configuration is loaded or saved without an explicit path.
	/// </summary>
	public abstract string DefaultPath { get; }

	/// <summary>
	/// Loads a configuration of the given type from a stream.
	/// </summary>
	/// <typeparam name="T">The configuration type to load.</typeparam>
	/// <param name="stream">The stream to read the configuration from.</param>
	/// <returns>
	/// The loaded configuration, or a new default instance when the stream is missing, empty or corrupt.
	/// The fallback instance is not saved automatically; callers save it to persist the defaults.
	/// </returns>
	public static T Load<T>(Stream stream) where T : ConfigurationBase, new()
	{
		try
		{
			return XmlUtils.ReadXmlFile<T>(stream);
		}
		catch (Exception exception)
		{
			s_logger.Warn(exception, "Configuration '{Type}' could not be loaded from a stream; using defaults.", typeof(T).Name);
			return new T();
		}
	}

	/// <summary>
	/// Loads a configuration of the given type from a file.
	/// </summary>
	/// <typeparam name="T">The configuration type to load.</typeparam>
	/// <param name="filePath">The path of the configuration file to read.</param>
	/// <returns>
	/// The loaded configuration, or a new default instance when the file is missing or corrupt.
	/// The fallback instance is not saved automatically; callers save it to persist the defaults.
	/// </returns>
	public static T Load<T>(string filePath) where T : ConfigurationBase, new()
	{
		try
		{
			return XmlUtils.ReadXmlFile<T>(filePath);
		}
		catch (Exception exception)
		{
			s_logger.Warn(exception, "Configuration '{Type}' could not be loaded from '{Path}'; using defaults.", typeof(T).Name, filePath);
			return new T();
		}
	}

	/// <summary>
	/// Loads a configuration of the given type from its default path.
	/// </summary>
	/// <typeparam name="T">The configuration type to load.</typeparam>
	/// <returns>The loaded configuration, or a new default instance when the file is missing or corrupt.</returns>
	public static T Load<T>() where T : ConfigurationBase, new()
		=> Load<T>(new T().DefaultPath);

	/// <summary>
	/// Saves the configuration to a stream.
	/// </summary>
	/// <param name="stream">The stream to write the configuration to.</param>
	public void Save(Stream stream)
		=> XmlUtils.WriteXmlFile(stream, GetType(), this);

	/// <summary>
	/// Saves the configuration to a file, creating the parent directory when needed.
	/// </summary>
	/// <param name="path">The path of the file to write to.</param>
	public void Save(string path)
	{
		string? directoryName = Path.GetDirectoryName(path);

		if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			Directory.CreateDirectory(directoryName);

		XmlUtils.WriteXmlFile(path, GetType(), this);
	}

	/// <summary>
	/// Saves the configuration to the default path.
	/// </summary>
	public void Save()
		=> Save(DefaultPath);
}
