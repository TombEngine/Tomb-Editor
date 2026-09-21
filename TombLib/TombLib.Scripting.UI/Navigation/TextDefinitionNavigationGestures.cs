using System;

namespace TombLib.Scripting.UI.Navigation;

/// <summary>
/// Specifies the input gestures the <see cref="TextDefinitionTriggerController"/> handles.
/// </summary>
/// <remarks>
/// Hosts can disable individual gestures and keep the rest of the controller, or skip the controller entirely
/// and drive definition navigation from commands instead.
/// </remarks>
[Flags]
public enum TextDefinitionNavigationGestures
{
	/// <summary>
	/// No gesture is handled.
	/// </summary>
	None = 0,

	/// <summary>
	/// F12 pressed without any modifier keys is handled.
	/// </summary>
	F12 = 1 << 0,

	/// <summary>
	/// A single Ctrl+LeftClick is handled.
	/// </summary>
	ControlClick = 1 << 1,

	/// <summary>
	/// Both <see cref="F12"/> and <see cref="ControlClick"/> are handled.
	/// </summary>
	Default = F12 | ControlClick
}
