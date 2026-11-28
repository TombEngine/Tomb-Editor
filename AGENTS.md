## General Project Information

- Language: **C# targeting .NET 8** (desktop application).
- This is a level editor suite for a family of 3D game engines used in the classic Tomb Raider series.
- Level formats are grid-based, room-based, and portal-based.
- A room is a spatial container for level geometry and game entities.
- Rooms are connected by vertical or horizontal portals, strictly aligned with grid sectors.
- Portals may be visual (`RoomConnectionInfo.VisualType`) or traversable (`RoomConnectionInfo.TraversableType`).
- One grid sector consists of 1024 units, which roughly equals 2 meters in real-world coordinates.

## General Guidelines

### Files and Namespaces

- Files must use Windows line endings. Only standard ASCII symbols are allowed; do not use Unicode symbols.
- Every document should end with a trailing newline.
- `using` directives and namespace declarations should always be sorted alphabetically.
- Remove unused `using` statements.
- Prefer importing namespaces over fully qualifying framework types when there is no ambiguity. Remove redundant qualifiers such as `System.StringComparison` and use `StringComparison` directly when no namespace conflict exists.
- Prefer file-scoped namespaces where a file contains a single namespace and no language constraint prevents it. If a block-scoped namespace is still required, place the opening brace on a new line and sort multiple namespace declarations alphabetically.

### Nullability

- Each refactor should enable nullable reference types for the touched code. Add `#nullable enabled` at the top of the file only when the project does not already enable nullables, and update the touched code to use nullable annotations and checks correctly.
- Always use `is null` / `is not null` rather than `== null` / `!= null`.
- Prefer nullability attributes and helpers such as `[NotNullWhen]`, `[MemberNotNull]`, `[MaybeNullWhen]` and related annotations when they improve flow analysis and keep the API clear.
- Avoid the null-forgiving operator (`!`) where possible. Prefer flow analysis, null checks, annotations and helper methods instead, and use `!` only when it is truly necessary to express a proven invariant the compiler cannot infer.

### Architecture and Composition

- Keep feature-related functionality within self-contained modules. Avoid large code blocks over 10-15 lines in existing modules; move that logic into helpers or dedicated types.
- Always look for opportunities to de-duplicate code and fix duplication where suitable. Prefer shared helpers or extracted modules when similar code appears within a module, class or feature scope.
- Prefer modern .NET and C# conventions when project-specific guidance does not require something else.
- Design new code with service-based composition in mind. Favor dependency injection seams, and use the temporary `TombLib.WPF` service locator only in code paths that already depend on it.
- Keep vertical slice architecture in mind when choosing where new features, helpers and dependencies should live.

## Formatting

### Indentation

- Indentation uses four spaces; tabs are not used.

### Braces

- Always use braces for multi-statement blocks.
- Single-line conditions should not use braces when the entire `if` / `else if` / `else` chain stays single-line.
- Multi-line conditions or multi-line bodies must always use braces.
- If any branch in an `if` / `else if` / `else` chain uses braces, all sibling branches should use braces as well.
- Opening curly brace `{` for structures, classes and methods should be on the next line, not on the same line:

    ```csharp
    public class Foo
    {
        public void Bar()
        {
            if (condition)
            {
                ...
            }
        }
    }
    ```

- Anonymous delegates and lambdas should keep the brace on the same line: `delegate () { ... }` or `() => { ... }`.

### Line Breaks and Spacing

- A blank line separates logically distinct groups of members (fields, constructors, public methods, private helpers, etc.).
- Within method bodies, use a blank line between logically distinct statements and before a control-flow block that starts a new step.
- Avoid whitespace-only lines or dead indentation; blank lines should be truly blank.
- Spaces around binary operators (`=`, `+`, `==`, etc.) and after commas.
- A single space follows keyword `if` / `for` / `while` before the opening parenthesis.
- Expressions may be broken into multiple lines and aligned with the previous line's indentation level to improve readability.
- Chained LINQ method calls, lambdas or function arguments should stay on one line unless they exceed roughly 150 characters.
- Do not collapse early exits or single-statement conditions into one line.

  Bad example:
  ```csharp
  if (condition) return;
  ```

  Do this instead:
  ```csharp
  if (condition)
      return;
  ```

## Naming

- **PascalCase** for public types, methods, constants, properties and events.
- **camelCase** for private fields and local variables. Private fields should start with an underscore (`_editor`, `_primaryControlFocused`). Local variables should not start with an underscore.
- Constants and `static readonly` fields use PascalCase rather than ALL_CAPS.
- Enum members use PascalCase.
- Interfaces are prefixed with `I` and use PascalCase (`IScaleable`).
- Methods and variables should use clear, descriptive names and generally avoid Hungarian notation. Avoid using short non-descriptive names, such as `s2`, `rwh`, `fmp`, unless underlying meaning is brief (e.g. X coordinate is `x`, counter is `i`).
- Class method and field names should not repeat words from a class name itself (e.g. `ObjectBrushHelper.BeginObjectBrushStroke` is a bad name, but `ObjectBrushHelper.BeginStroke` is a good name).

## Members and Access

- Fields are generally declared as `public` or `private readonly` depending on usage; expose state via properties where appropriate.
- `var` type should be preferred where possible, when the right-hand type is evident from the initializer.
- Explicit typing should only be used when it is required by logic or compiler, or when the type name is shorter than 6 symbols (e.g. `int`, `bool`, `float`).
- For floating-point numbers, always use `f` postfix and decimal, even if value is not fractional (e.g. `2.0f`).
- Consider `record` or `record struct` when value semantics, immutability, or concise data-carrier behavior make them a better fit than a class or struct.
- Prefer expression-bodied members for methods or properties whose implementation is a single readable line.
- Prefer collection expressions (`[]`, `[item]`, `[..items]`) over `Array.Empty<T>()`, explicit array or list construction, or simple `.ToArray()` / `.ToList()` materialization when the target type supports them and the result stays clear.

## Control Flow and Syntax

- Avoid excessive condition nesting and use early exits / breaks where possible.
- LINQ and lambda expressions are used for collections (`FirstOrDefault`, `Where`, `Any`, etc.).
- Use pattern matching where it keeps the code clearer or removes redundant casts, temporary variables or branching.
- Under nullable-aware code, avoid throwing `ArgumentNullException` for non-nullable parameters when the guard adds no meaningful value.
- When an exception type exposes helper APIs such as `ArgumentNullException.ThrowIfNull`, prefer those helpers over manual `if` blocks when the behavior stays clear.
- Exception and error handling is done with `try`/`catch`, and caught exceptions are logged with [NLog](https://nlog-project.org/) where appropriate.
- Warnings caused by user action should also be logged through NLog.

## Comments

- When comments appear they are single-line `//`. Block comments (`/* ... */`) are rare.
- Comments are sparse. Code relies on meaningful names rather than inline documentation.
- Add XML documentation to classes where it clarifies intent, and to public methods and public properties by default. Use XML documentation for private members only when the behavior is complex enough that names alone are not sufficient.
- If a module or function implements complex functionality, use brief section comments to split long methods into smaller, digestible steps.
- All descriptive comments should end with a full stop (`.`).

## Code Grouping

- Large methods should group related actions together, separated by blank lines and short section comments when they cannot be broken apart further.
- Constants and static helpers that are used several times should appear at the top of a class.
- Constants that are used only within a scope of a method, should be declared within this method.
- One-liner lambdas may be grouped together, if they share similar meaning or functionality.
- Prefer one top-level type per file when practical. Keep multiple classes, enums, records or interfaces in the same file only when they are strictly coupled.
- When a class grows too large in size or scope, split it into smaller partial classes organized by responsibility. Use partial classes only when the responsibilities still belong to the same type; otherwise extract a dedicated helper, service or type instead.
- Avoid one-line wrapper methods unless they remove duplication, enforce a policy, or provide meaning beyond a direct redirect.
- Do not keep generic helper methods inside the same feature class. First check whether a suitable shared helper already exists elsewhere in the codebase; otherwise extract the helper into the most suitable shared library project or dedicated module.
- If a helper method is broad in scope, such as a general WPF helper like `FindAncestor()`, first verify whether an equivalent already exists. If not, place it in the narrowest suitable shared library among `TombLib`, `TombLib.Scripting`, `TombLib.WPF` and `DarkUI.WPF` rather than adding it to a feature-local helper class.

## User Interface Implementation

- For WinForms-based workflows, maintain the existing Visual Studio module pair for each control or unit: `.cs` and `.Designer.cs`.
- For existing WinForms-based `DarkUI` controls and containers, prefer to use existing WinForms-based `DarkUI` controls.
- For new WPF views and view models, use `GeometryIOSettingsWindow` as the reference for structure, localization and service usage patterns.
- When writing WPF UI, prioritize localization and the existing localization infrastructure from `TombLib.WPF`.
- Creating new generic WPF controls should be delegated to `DarkUI.WPF`.
- For new controls and containers with complex logic, or where WinForms may not perform fast enough, prefer `DarkUI.WPF`.
- Use `CommunityToolkit` functionality where possible.

## Performance

- For 3D rendering controls, prefer more performant approaches and locally cache frequently used data within the function scope whenever possible.
- Avoid scenarios where bulk data updates may cause event floods, as the project relies heavily on event subscriptions across multiple controls and sub-controls.
- Use `Parallel` for bulk operations to maximize performance. Avoid using it in thread-unsafe contexts or when operating on serial data sets.
