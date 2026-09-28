namespace TombLib.Scripting.TRX.Models;

/// <summary>
/// The JSON schema types that a GameFlow property may declare.
/// </summary>
public enum TRXGameFlowPropertyType
{
	/// <summary>
	/// The JSON object type.
	/// </summary>
	Object,

	/// <summary>
	/// The JSON array type.
	/// </summary>
	Array,

	/// <summary>
	/// The JSON string type.
	/// </summary>
	String,

	/// <summary>
	/// The JSON integer type.
	/// </summary>
	Integer,

	/// <summary>
	/// The JSON number type.
	/// </summary>
	Number,

	/// <summary>
	/// The JSON boolean type.
	/// </summary>
	Boolean,

	/// <summary>
	/// The JSON null type.
	/// </summary>
	Null
}
