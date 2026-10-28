#nullable enable

namespace SoundTool.Services;

/// <summary>Provides file selection and modal UI for the sound catalog editor.</summary>
public interface ISoundToolDialogs
{
    string? OpenFile(string title, string filter, string? initialPath = null);
    string[] OpenSamples();
    string? SaveFile(string title, string filter, string? initialPath = null);
    string? SelectFolder(string title, string? initialPath = null);
    string? Prompt(string title, string label, string value = "");
    void ShowOptions(Configuration configuration);
    void ShowAbout();
    void Close();
}
