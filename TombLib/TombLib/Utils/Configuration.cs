#nullable enable

using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using TombLib.Utils;

namespace TombLib;

public abstract class ConfigurationBase
{
    public enum VariableType
    {
        EditorDirectory
    }

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public const string VariableBegin = "$(";
    public const string VariableEnd = ")";
    public static readonly char Dir = Path.DirectorySeparatorChar;

    public abstract string ConfigName { get; }

    [XmlIgnore]
    public LogLevel Log_MinLevel { get; set; } = LogLevel.Debug;
    [XmlElement(nameof(Log_MinLevel))]
    public string Log_MinLevelSerialized
    {
        get { return Log_MinLevel.Name; }
        set { Log_MinLevel = LogLevel.FromString(value); }
    }
    public bool Log_WriteToFile { get; set; } = true;
    public int Log_ArchiveN { get; set; } = 0;

    public string GetVariable(VariableType variableType)
    {
        switch (variableType)
        {
            case VariableType.EditorDirectory:
                return Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) ?? AppContext.BaseDirectory;
            default:
                throw new ArgumentException();
        }
    }

    public string ParseVariables(string path, params VariableType[] excluded)
    {
        int startIndex = 0;
        do
        {
            // Find variable
            startIndex = path.IndexOf(VariableBegin, startIndex);
            if (startIndex == -1)
                break;
            int afterStartIndex = startIndex + VariableBegin.Length;
            int endIndex = path.IndexOf(VariableEnd, afterStartIndex);
            if (endIndex == -1)
                break;
            string variableName = path.Substring(afterStartIndex, endIndex - afterStartIndex);

            // Parse variable
            VariableType variableType;
            if (!Enum.TryParse(variableName, out variableType) ||
                excluded.Contains(variableType))
            {
                startIndex = endIndex + VariableEnd.Length;
                continue;
            }
            string variableContent = GetVariable(variableType);
            path = path.Remove(startIndex, endIndex + VariableEnd.Length - startIndex);
            path = path.Insert(startIndex, variableContent);
            startIndex += variableContent.Length;
        } while (true);

        return path;
    }

    public string GetDefaultPath()
    {
        // Since version 1.3, all configs are moved to /Configs subfolder.
        // This code must remain here so users won't lose their settings on drop-in replacement.

        var startDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
        var oldPath  = Path.Combine(startDir, ConfigName);
        var newPath  = Path.Combine(startDir, "Configs", ConfigName);

        if (!Directory.Exists(Path.GetDirectoryName(newPath)))
            Directory.CreateDirectory(Path.Combine(startDir, "Configs"));

        if (File.Exists(oldPath))
        {
            if (File.Exists(newPath)) File.Delete(newPath);
            File.Move(oldPath, newPath);
        }

        return newPath;
    }

    public void Save(Stream stream)
        => XmlUtils.WriteXmlFile(stream, GetType(), this);

    public void Save(string path)
        => XmlUtils.WriteXmlFile(path, GetType(), this);

    public void Save()
        => Save(GetDefaultPath());

    public void SaveTry()
    {
        if (!string.IsNullOrEmpty(GetDefaultPath()))
        {
            try
            {
                Save();
            }
            catch (Exception exc)
            {
                logger.Info(exc, "Unable to save configuration to \"" + GetDefaultPath() + "\"");
            }
        }
    }

    public T Load<T>(Stream stream) where T : ConfigurationBase
        => XmlUtils.ReadXmlFile<T>(stream);

    public T Load<T>(string filePath) where T : ConfigurationBase
        => XmlUtils.ReadXmlFile<T>(filePath);

    public T Load<T>() where T : ConfigurationBase
        => Load<T>(GetDefaultPath());

    public T LoadOrUseDefault<T>(ICollection<LogEventInfo>? log = null) where T : ConfigurationBase, new()
    {
        string path = GetDefaultPath();
        if (!File.Exists(path))
        {
            log?.Add(new LogEventInfo(LogLevel.Info, logger.Name, null, "Unable to load configuration from \"" + path + "\"", null, new FileNotFoundException("File not found", path)));
            return new T();
        }

        try
        {
            return Load<T>();
        }
        catch (Exception exc)
        {
            log?.Add(new LogEventInfo(LogLevel.Info, logger.Name, null, "Unable to load configuration from \"" + path + "\"", null, exc));
            return new T();
        }
    }
}
