# TombLib.Scripting.TRX

Purpose: TRX language provider for the Tomb1Main (TRX) level-definition format.

Implements the shared neutral provider contracts for TRX, all driven by the immutable
GameFlow schema:
- completion - `TRXGameFlowCompletionService`
- hover - `TRXGameFlowHoverService`
- definition - `TRXDefinitionProvider`
- diagnostics - `ErrorDetector`

Provides the editor (`TRXEditor`), configuration (`TRXEditorConfiguration`), the
language-services composition root (`TRXLanguageServices`), the schema service
(`TRXGameFlowSchemaService` / `ITRXGameFlowSchemaService`), content nodes
(`TRXNodesProvider`), the `ScriptReplacer` writer, and resource catalogs such as `Keywords`.

Depends on `TombLib.Scripting.UI` and `TombLib.Scripting`. See the
[provider-library stabilization plan](../../TombLib.Scripting_Provider_Libraries_Stabilization_Plan.md)
for the current provider standards and extension-point guidance.
