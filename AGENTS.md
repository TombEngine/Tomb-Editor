# Repository Agent Guidance

This file provides repository context and coding guidance for agents working in
Tomb-Editor. More specific instruction files and project configuration take
precedence for the files they cover.

## Instruction Precedence

- Preserve compilability and runtime behavior before applying style preferences.
- Follow the most specific applicable instruction file and `.editorconfig`.
- Treat `MUST` and `MUST NOT` as requirements, `SHOULD` as the default
    preference, and `MAY` or `CONSIDER` as optional guidance.
- Do not revert unrelated user changes. Keep edits focused on the requested
    behavior.

## Project Context

- Language: **C# targeting .NET 8** in a Windows desktop application.
- This is a level editor suite for several 3D game engines used by the classic
    Tomb Raider series.
- Level formats are grid-based, room-based, and portal-based.
- A room is a spatial container for level geometry and game entities.
- Rooms connect through vertical or horizontal portals aligned with grid
    sectors.
- Portals may be visual (`RoomConnectionInfo.VisualType`) or traversable
    (`RoomConnectionInfo.TraversableType`).
- One grid sector is 1024 units, approximately 2 meters in world coordinates.

## Repository Workflow

Follow `.github/instructions/codebase-workflow.instructions.md` for the
Codebase Memory MCP workflow before exploring or editing this repository when
the MCP tools are available. If they are unavailable in the current
environment, skip that workflow and use the normal repository search and file
reading tools. Do not treat an unavailable optional service as a blocker, and
mention the fallback when it materially limits repository context.

## Files and Namespaces

- Source and instruction files use Windows CRLF line endings and end with one
    trailing newline.
- C# source and documentation use ASCII characters unless a file's purpose
    requires Unicode. Do not introduce Unicode punctuation into ordinary source
    comments or instructions.
- Sort `using` directives and namespace declarations alphabetically, using the
    repository's existing case and grouping convention.
- Remove unused `using` directives.
- Prefer imported namespaces over fully qualified framework types when there is
    no ambiguity. For example, use `StringComparison.Ordinal` after importing
    `System` instead of `System.StringComparison.Ordinal`.
- Prefer a file-scoped namespace when a file contains one namespace and no
    language or generated-code constraint prevents it.

## Nullability

- Keep touched code correct under the project's effective nullable configuration.
    Check the evaluated MSBuild property, including imported files such as
    `Directory.Build.props` and `Directory.Build.targets`, the project file, and
    any later overrides before adding a file-level directive. If nullable
    reference types are not already enabled for the project, add exactly one
    `#nullable enable` directive at the top of each touched C# source file.
    Do not add a redundant directive when the project already enables nullable
    reference types. Do not add it to generated or externally owned code unless
    the local instructions explicitly allow editing those files.
- Treat nullable warnings revealed by enabling the directive as part of the
    refactor: fix them in the touched code when practical, or document why a
    warning must remain. The directive enables analysis; it does not by itself
    make code null-safe.
- The per-file directives are a migration aid. Once all projects enable nullable
    reference types globally, remove redundant per-file directives in a
    dedicated cleanup rather than mixing that change into unrelated refactors.
- Use `is null` and `is not null` for null checks unless an intentional
    overloaded equality contract requires otherwise.
- Use nullability attributes such as `[NotNullWhen]`, `[MemberNotNull]`, and
    `[MaybeNullWhen]` when they accurately describe flow and improve the API.
- Use `[return: NotNullIfNotNull("value")]` when a method's return value is
    non-null whenever the named input parameter is non-null.
- Avoid the null-forgiving operator (`!`). Prefer flow analysis, explicit
    checks, annotations, or helper methods. Use `!` only for an invariant that is
    proven but cannot be expressed to the compiler.

## Architecture and Composition

- Keep feature code cohesive and organized by responsibility. Do not split code
    solely by line count; a method that mixes responsibilities, is difficult to
    test, or grows beyond roughly 50 lines is a review signal for extraction.
- Remove duplication when the duplicated behavior is genuinely the same. Use a
    shared helper or service when that reduces maintenance without creating a
    vague utility layer.
- Prefer modern .NET and C# APIs when they are supported by the target framework
    and preserve the existing behavior.
- Prefer dependency injection seams for new code. Use the temporary
    `TombLib.WPF` service locator only at existing composition boundaries.
- Organize new features as vertical slices when that keeps their models,
    services, views, and tests easy to locate without breaking dependency layers.

## Formatting

### Indentation

- Follow the nearest `.editorconfig`. Non-scripting projects use four spaces;
    do not use tabs there.
- The `TombLib/.editorconfig` override applies tabs to the `TombLib.Scripting*`
    provider projects. The same rule applies to all `TombIDE*` projects. Do not
    convert those files to spaces.
- Do not mix tabs and spaces for indentation. Use spaces for alignment only
    when the file's established format requires it.

### Braces

- Use braces for every multi-statement block.
- A single-statement body may omit braces only when it remains a simple,
    separately indented statement. In an `if` / `else if` / `else` chain,
    braces may be omitted only when every branch is a single physical line:

```csharp
if (condition)
    return;
```

- Use braces when a body contains another control-flow statement, when the
    condition or body spans multiple physical lines, or when the block is
    otherwise easier to read with braces. A single logical expression formatted
    across multiple physical lines counts as a multiline body.
- If one branch of an `if` / `else if` / `else` chain uses braces, all branches
    use braces. If all branches are simple single-line statements, all branches
    may omit braces. Put `else` and `else if` on their own line immediately
    after the preceding closing brace, with no blank line between them.
- Put opening braces for classes, methods, properties, block-scoped namespaces,
    anonymous delegates, and block-bodied lambdas on the next line.
- An empty block must always be written on one line as `{ }`, including empty
    methods, constructors, properties, delegates, and lambdas.

### Line Breaks and Spacing

- Separate logically distinct member groups and statement groups with blank
    lines. Keep a declaration next to its immediate use when they form one unit.
- Within a method, group statements by responsibility. Keep setup and local
    declarations together, separate callback or event configuration from the
    operation it controls, and separate independent actions, synchronization or
    cleanup steps, and their assertions with blank lines. Do not use blank lines
    to split statements that form one immediate operation.
- Put a blank line before a new control-flow step and after a completed block
    when more code follows in the same scope. Do not add a blank line immediately
    before the enclosing closing brace. Keep `else`, `else if`, and the `while`
    clause of a `do` statement attached to the preceding block without a blank
    line, while keeping each keyword on its own line.
- Separate consecutive guard statements with blank lines when they are distinct
    checks. Do not add blank lines inside a single logical condition or statement.
- Blank lines must contain no spaces or tabs.
- Put spaces around binary operators and after commas. Put one space between
    `if`, `for`, or `while` and its opening parenthesis.
- Break long expressions only when it improves readability. Align continuation
    lines with the expression's indentation. Keep chained calls on one line when
    they are approximately 150 characters or less; wrap longer or less readable
    chains.
- Keep early exits and single-statement conditions on separate lines rather than
    writing `if (condition) return;`.

## Naming

- Use PascalCase for public types, methods, constants, properties, events, and
    enum members.
- Use camelCase for locals. Prefix private fields with an underscore, such as
    `_editor` or `_primaryControlFocused`; locals do not use that prefix.
- Prefix private or internal static fields with `s_`, such as `s_cache` or
    `s_logger`. For thread-static fields, use `t_` instead, such as `t_buffer`.
    Keep constants and static properties in PascalCase; these are not instance
    or static field prefixes.
- Prefix interfaces with `I`, such as `IEditor`.
- Use clear descriptive names and avoid Hungarian notation. Short names are
    acceptable for conventional meanings such as `x` for a coordinate or `i` for
    a local counter.
- Do not use one-letter variable names unless the short name has a clear,
    conventional meaning.
- Do not repeat the containing type's name in a member when the context already
    supplies it. Prefer `ObjectBrushHelper.BeginStroke()` to
    `ObjectBrushHelper.BeginObjectBrushStroke()`.

## Members and Access

- Prefer private fields and expose state through the smallest suitable property
    or method. Use `readonly` for dependencies and state that does not change.
- Prefer `var` when the initializer makes the type obvious. Use an explicit
    type when it improves clarity, communicates a conversion, or the type is not
    apparent from the initializer.
- Use the `f` suffix and a decimal point for floating-point literals, including
    whole values such as `2.0f`.
- Consider `record` or `record struct` for immutable value-like data when that
    matches the surrounding API.
- Prefer an expression-bodied member for one short expression, including a
    constructor with one assignment or method call. Keep a short expression on
    one line when the declaration remains readable. A two-line arrow is
    acceptable when wrapping the declaration or arrow improves readability, but
    do not split an ordinary expression itself across physical lines. Use a block
    body when an ordinary expression or logic spans multiple physical lines, even
    when it is one logical expression.

```csharp
private string GetDisplayName(Item item)
    => item.Name;
```

- A multiline signature alone does not require a block body when a two-line arrow
    remains readable. Keep switch expressions and target-typed `new(...)`
    attached to the method signature when the result remains readable, including
    a switch expression whose arms occupy multiple physical lines. Wrap the
    expression or use a block body when it becomes too long or otherwise hard to
    read.
- A `?:` ternary that wraps onto multiple lines is multiline logic and uses a
    block body rather than a multiline arrow.
- Prefer target-typed `new(...)` when the target type is already clear from the
    assignment, field, parameter, or return type.
- Group fields by responsibility. Add a short `//` heading only when a large
    field block would otherwise be difficult to scan.
- When a constructor has several independent dependencies or repeated settings,
    consider a settings or composition object. Keep simple data-model
    constructors direct.
- Prefer collection expressions such as `[]`, `[item]`, and `[.. items]` when
    the target type supports them and the materialization has the same semantics.
    Keep `.ToList()` or `.ToArray()` when a long query is clearer that way.

## Control Flow and Syntax

- Reduce nesting with early returns, guard clauses, and breaks when they improve
    readability without hiding important control flow.
- Prefer LINQ and lambdas for clear collection transformations without side
    effects. Use loops for complex stateful logic, hot paths, or code that is more
    readable imperatively.
- Prefer pattern matching over separate type checks, casts, and equivalent
    branching when it keeps the intent clear and removes redundant code.
- Use `ArgumentNullException` guards only for parameters declared nullable
    (`string?`) where a null value is a plausible caller error. Do not guard
    non-nullable parameters: the compiler's static analysis is the contract, and
    a runtime guard on a non-nullable parameter is redundant ceremony.
- Prefer framework guard helpers such as
    `ArgumentNullException.ThrowIfNull(argument)` and
    `ArgumentOutOfRangeException.ThrowIfNegative(value)`,
    `ThrowIfZero(value)`, or `ThrowIfLessThan(value, minimum)` when they
    preserve clear behavior. Use manual validation for compound or
    domain-specific constraints when it is clearer.
- Catch exceptions only at a boundary that can recover, translate, or add useful
    context. Log caught exceptions with NLog where appropriate; do not catch only
    to log and silently continue.
- Log warnings caused by user actions through NLog with enough context to
    diagnose the condition.

## Comments and Documentation

- Use short `//` comments for inline explanations. Block comments are rare;
    XML documentation is the exception for public API documentation.
- Prefer meaningful names and clear code over comments that merely restate the
    implementation.
- Add XML documentation to public classes, methods, and properties by default
    when their purpose or contract is not obvious. Document private members only
    when their behavior is complex or non-obvious.
- In XML documentation, use `<see langword="null"/>`,
    `<see langword="true"/>`, `<see langword="false"/>`, and
    `<see langword="default"/>` for language keywords. Use `<c>-1</c>` for
    literal values when appropriate.
- Use brief section comments to divide a complex method only when extracting a
    helper would not improve the design. Descriptive comments end with a period.

## Code Organization

- Group related actions in large methods when extraction is not practical, and
    separate the groups with blank lines or short section comments.
- Place frequently reused constants and static helpers near the top of the
    class. Keep method-local constants inside the method that owns them.
- Prefer one top-level type per file. Multiple types may share a file when they
    are small and strictly coupled.
- Use partial classes only to separate responsibilities that still belong to
    one cohesive type. Extract a helper or service when the responsibility is a
    separate abstraction.
- Keep generated code and designer-managed files under their normal ownership
    boundaries. Avoid hand-editing them unless the task specifically requires it.
- Avoid methods or properties that only forward to another member without
    adding behavior, validation, abstraction, or meaningful domain terminology.
    Keep forwarding members when they intentionally hide implementation details
    or provide a stable feature-level API.
- Before adding a generic helper to a feature class, search for an existing
    shared helper. If none exists, place it in the narrowest suitable shared
    library or dedicated module.
- For broad WPF helpers such as `FindAncestor()`, check existing helpers first.
    Otherwise use the narrowest suitable library among `TombLib`,
    `TombLib.Scripting`, `TombLib.WPF`, and `DarkUI.WPF`.

## User Interface

- For WinForms controls, keep the normal `.cs` and `.Designer.cs` pair when the
    control uses the Visual Studio designer.
- Reuse the existing DarkUI controls for existing DarkUI WinForms workflows.
- Use `TombLib/TombLib.Forms/Views/GeometryIOSettingsWindow.xaml` and its view
    model as the local reference for new WPF view structure and service usage.
- Use MVVM for WPF views and controls by default. Keep presentation state,
    commands, and workflow logic in ViewModels. Direct control logic is an
    exception for genuinely performance-critical or high-frequency controls
    where MVVM overhead is measured to be material; keep domain logic and
    reusable services outside the control where possible.
- In WPF ViewModels, use `ILocalizationService` from `TombLib.WPF` for
    user-visible text and localized properties.
- In WPF XAML views, use the `LocalizeExtension` markup extension from
    `TombLib.WPF` for localized dependency-property values instead of hard-coded
    user-visible strings.
- Use XAML behaviors when they keep view interaction logic maintainable and
    reduce unnecessary code-behind. Do not use behaviors for complex domain
    logic or when the resulting XAML is harder to understand than a small,
    direct code-behind or ViewModel implementation.
- Put new generic reusable WPF controls in `DarkUI.WPF`.
- Prefer `DarkUI.WPF` for new controls whose complexity or performance needs
    make WinForms unsuitable. Preserve existing WinForms workflows unless there
    is a concrete reason to migrate them.
- Prefer CommunityToolkit functionality whenever it provides a suitable
    implementation for the need. Use it over equivalent hand-rolled code,
    including when it makes the implementation more concise or clearer.

## Performance

- In 3D rendering and other known hot paths, avoid repeated expensive work and
    cache reusable data within the operation. Measure when the tradeoff is not
    obvious.
- Batch or defer bulk updates when individual notifications would cause event
    floods. Preserve the event contract expected by existing subscribers.
- Use `Parallel` only for independent, CPU-bound, thread-safe work where its
    overhead is justified. Do not parallelize UI-bound, shared mutable, or
    inherently serial operations.

## Codebase Memory MCP

**MANDATORY: use Codebase Memory MCP graph tools FIRST — before reading files or making code changes.**

This rule applies to every request involving this codebase.

Always call `list_projects` first when you do not already know the project name, then use the `display_name` or exact `name` returned by that tool.

```json
// Step 0 — discover project names
mcp_codebase-memo_list_projects()

// Step 1 — use the project identifier returned above
mcp_codebase-memo_get_architecture({ "project": "<display_name>" })
```

### Workflow

1. Call `list_projects` to discover the correct project name.
2. Call `get_architecture(project)` to understand the codebase structure.
3. Use `search_graph` to find relevant symbols, `trace_call_path` for call chains.
4. Use `get_code_snippet` to read specific function implementations.
5. Only use `read_file` when you need exact raw content to edit a specific line.

### Available Tools (14 MCP tools)

**Indexing:**
- `index_repository(repo_path)` — Index a repository into the knowledge graph
- `list_projects` — List all indexed projects with node/edge counts
- `delete_project(project)` — Remove a project and all its graph data
- `index_status(project)` — Check indexing status

**Querying:**
- `search_graph(name_pattern, name_scope, label, file_pattern, exclude_file_pattern)` — Structured search by label, name/qualified_name, include/exclude file globs
- `trace_call_path(function_name, direction, depth)` — BFS call chain traversal
- `detect_changes(project)` — Map git diff to affected symbols + risk
- `query_graph(query)` — Execute Cypher-like graph queries (read-only)
- `get_graph_schema(project)` — Node/edge counts, relationship patterns
- `get_code_snippet(qualified_name)` — Read source code for a function
- `get_architecture(project)` — Codebase overview: languages, packages, routes, hotspots
- `search_code(pattern, project)` — Grep-like text search within indexed files
- `manage_adr(action)` — CRUD for Architecture Decision Records
- `ingest_traces(traces)` — Ingest runtime traces to validate HTTP edges
