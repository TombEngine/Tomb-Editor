using Nickelony.IDEKit.IntelliSense.Navigation;

namespace TombLib.Scripting.UI.Editors;

/// <summary>
/// Optional capability for editors that can navigate to a definition by object name.
/// Editors that resolve definitions through other means (for example LSP-driven Lua) or that
/// have no definitions (for example plain text) do not implement this interface.
/// </summary>
public interface INameBasedObjectNavigator
{
	/// <summary>
	/// Navigates to the definition of the given object name.
	/// </summary>
	/// <param name="objectName">The name of the object to navigate to.</param>
	/// <param name="identifyingObject">An optional discriminator that disambiguates the target.</param>
	void GoToObject(string objectName, TextDefinitionDiscriminator? identifyingObject = null);
}
