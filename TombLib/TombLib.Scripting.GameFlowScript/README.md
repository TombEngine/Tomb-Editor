# TombLib.Scripting.GameFlowScript

Purpose: GameFlowScript language provider for the GameFlow (NG) script format.

Implements the shared neutral provider contracts for GameFlowScript:
- completion - `GameFlowCompletionProvider`
- hover - `GameFlowHoverProvider`
- definition - `GameFlowDefinitionProvider`

Provides the editor (`GameFlowEditor`), configuration (`GameFlowEditorConfiguration`), the
language-services composition root (`GameFlowLanguageServices`), the immutable
`GameFlowDefinitionCatalog`, document and line services, the `ScriptCompiler` build
workflow, the `ScriptReplacer` and `LanguageStringWriter` writers, content nodes, and color
scheme resources.

Depends on `TombLib.Scripting.UI` and `TombLib.Scripting`. See the
[provider-library stabilization plan](../../TombLib.Scripting_Provider_Libraries_Stabilization_Plan.md)
for the current provider standards and extension-point guidance.
