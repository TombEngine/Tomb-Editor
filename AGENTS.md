## General Project Information

- Language: **C# targeting .NET 8** (desktop application).
- This is a level editor suite for a family of 3D game engines used in the classic Tomb Raider series.
- Level formats are grid-based, room-based, and portal-based.
- A room is a spatial container for level geometry and game entities.
- Rooms are connected by vertical or horizontal portals, strictly aligned with grid sectors.
- Portals may be visual (`RoomConnectionInfo.VisualType`) or traversable (`RoomConnectionInfo.TraversableType`).
- One grid sector consists of 1024 units, which roughly equals 2 meters in real-world coordinates.

## General Guidelines

- Files must use Windows line endings. Only standard ASCII symbols are allowed; do not use Unicode symbols.
- `using` directives are grouped and sorted as follows: `DarkUI` namespaces first, then `System` namespaces, followed by third-party and local namespaces.
- Namespace declarations and type definitions should place the opening brace on a new line.
- Prefer grouping all feature-related functionality within a self-contained module or modules. Avoid creating large code blocks over 10–15 lines in existing modules; instead, offload code to helper functions.
- Avoid duplicating and copypasting code. Implement helper methods instead, whenever similar code is used within a given module, class or feature scope.

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
- Explicit typing should be only used when it is required by logic or compiler, or when type name is shorter than 6 symbols (e.g. `int`, `bool`, `float`).
- For floating-point numbers, always use `f` postfix and decimal, even if value is not fractional (e.g. `2.0f`).

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
- Constants and static helpers that are used several times should appear at the top of a class.
- Constants that are used only within a scope of a method, should be declared within this method.
- One-liner lambdas may be grouped together, if they share similar meaning or functionality.

## User Interface Implementation

- For WinForms-based workflows, maintain the existing Visual Studio module pair for each control or unit: `.cs` and `.Designer.cs`.
- For existing WinForms-based `DarkUI` controls and containers, prefer to use existing WinForms-based `DarkUI` controls.
- For new controls and containers with complex logic, or where WinForms may not perform fast enough, prefer `DarkUI.WPF` framework. Use `GeometryIOSettingsWindow` as a reference.
- Use `CommunityToolkit` functionality where possible.

## Performance

- For 3D rendering controls, prefer more performant approaches and locally cache frequently used data within the function scope whenever possible.
- Avoid scenarios where bulk data updates may cause event floods, as the project relies heavily on event subscriptions across multiple controls and sub-controls.
- Use `Parallel` for bulk operations to maximize performance. Avoid using it in thread-unsafe contexts or when operating on serial data sets.