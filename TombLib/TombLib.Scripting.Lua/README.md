# TombLib.Scripting.Lua

Purpose: Lua language provider for Tomb Engine scripts.

Unlike the other language packages, Lua is driven by an external language server
(`Nickelony.LanguageServer.Lua`) rather than local catalogs. The editor (`LuaEditor`) and
its configuration (`LuaEditorConfiguration`) compose directly instead of through a
language-services aggregate. Hover, signature help, and definition navigation controllers
are private to the editor.

Provides theme support (`LuaTheme`, `LuaThemeRepository`), semantic highlighting
(`LuaSemanticTokensColorizer`), parser and indentation services (`LuaLineParser`, the
auto-indentation strategies), completion presentation (`LuaCompletionIconFactory`), and the
Tomb Engine document services (`TombEngineLevelScriptService`,
`TombEngineLanguageScriptService`).

Depends on `TombLib.Scripting.UI`, `TombLib.Scripting`, and `TombLib.WPF`. See the
[provider-library stabilization plan](../../TombLib.Scripting_Provider_Libraries_Stabilization_Plan.md)
for the current provider standards and extension-point guidance.
