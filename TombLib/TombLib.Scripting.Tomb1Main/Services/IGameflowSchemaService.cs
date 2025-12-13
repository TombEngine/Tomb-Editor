#nullable enable

using Newtonsoft.Json.Schema;
using TombLib.Scripting.Tomb1Main.Objects;

namespace TombLib.Scripting.Tomb1Main.Services;

public interface IGameflowSchemaService
{
	JSchema? Schema { get; }
	SchemaKeywords? GetSchemaKeywords();
}
