## General

- Language: **C# targeting .NET 8** (desktop application).
- Description: A level editor suite for a family of block-based, room-based and portal-based 3D games (classic Tomb Raider).

- Files use Windows line endings. Do not use Unicode symbols, only standard ASCII symbols.
- `using` directives are grouped and sorted with `DarkUI` namespaces references first, then `System` namespaces, followed by third-party and local namespaces.
- Namespace declarations and type definitions put the opening brace on a new line.
- Prefer to group all feature-related functionality within a self-contained module or modules. Avoid generating large code chunks over 10-15 lines in existing modules and offload it to helper functions instead.

## Formatting

- **Indentation** is four spaces; tabs are not used.

- **Braces**:
  - Always use braces for multi-statement blocks.
  - Do not use braces for single-statement blocks, unless they are within multiple `else if` conditions where surrounding statements are multi-line.
  
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

  - Anonymous delegates and lambdas should keep the brace on the same line:
    `delegate () { ... }` or `() => { ... }`.
	
- **Line breaks and spacing**:
  - A blank line separates logically distinct groups of members (fields, constructors, public methods, private helpers, etc.).
  - Spaces around binary operators (`=`, `+`, `==`, etc.) and after commas.
  - A single space follows keyword `if`/`for`/`while` before the opening parenthesis.
  - Expressions may be broken into multiple lines and aligned with the previous line's indentation level to improve readability.
  - However, chained LINQ method calls, lambdas or function/method arguments should not be broken into multiple lines, unless they reach more than 150 symbols in length.
  
  - Do not collapse early exits or single-statement conditions into a single line: 
  
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

- **PascalCase** for public types, methods, properties and events.
- **camelCase** for private fields and local variables. Private fields should start with an underscore (`_editor`, `_primaryControlFocused`). Local variables should not start with an underscore.
- Constants and `static readonly` fields use PascalCase rather than ALL_CAPS.
- Enum members use PascalCase.
- Interfaces are prefixed with `I` and use PascalCase (`IScaleable`).
- Methods and variables should use clear, descriptive names and generally avoid Hungarian notation. Avoid using short non-descriptive names, such as `s2`, `rwh`, `fmp`, unless underlying meaning is brief (e.g. X coordinate is `x`, counter is `i`).

## Members and Access

- Fields are generally declared as `public` or `private readonly` depending on usage; expose state via properties where appropriate.
- `var` type should be preferred where possible, when the right-hand type is evident from the initializer.
- Explicit typing should be only used when it is required by logic or compiler, or when type name is shorter than 6 symbols (e.g. `int`, `bool`, `float`).

## Control Flow and Syntax

- Avoid excessive condition nesting and use early exits / breaks where possible.
- LINQ and lambda expressions are used for collections (`FirstOrDefault`, `Where`, `Any`, etc.).
- Exception and error handling is done with `try`/`catch`, and caught exceptions are logged with [NLog](https://nlog-project.org/) where appropriate.
- Warnings must also be logged by NLog, if cause for the incorrect behaviour is user action.

## Comments

- When comments appear they are single-line `//`. Block comments (`/* ... */`) are rare.
- Comments are sparse. Code relies on meaningful names rather than inline documentation.
- Do not use `<summary>` if surrounding code and/or module isn't already using it. Only add `<summary>` for non-private methods with high complexity.
- If module or function implements complex functionality, a brief description (2-3 lines) may be added in front of it, separated by a blank line from the function body.
- All descriptive comments should end with a full stop (`.`).

## Code Grouping

- Large methods should group related actions together, separated by blank lines.
- Constants and static helpers should appear at the top of a class.
- One-liner lambdas may be grouped together, if they share similar meaning or functionality.

## User Interface Implementation

- For existing controls and containers based on `DarkUI` WinForms-based framework, prefer to use existing `DarkUI` controls.
- For new controls and containers with complex logic, or where WinForms may not perform fast enough, prefer `DarkUI.WPF` framework. Use `GeometryIOSettingsWindow` as a reference.
- Use `CommunityToolkit` functionality where possible.

## Performance

- For 3D rendering controls, prefer more performant approaches and attempt to locally cache repeatedly used data within a function scope.
- Avoid scenarios where bulk data updates may cause event floods, because the project relies heavily on event subscriptions in multiple controls and sub-controls.
- Use `Parallel` in bulk operations where possible to maximize the performance. Avoid using it in thread-unsafe contexts and while operating on serial data sets.