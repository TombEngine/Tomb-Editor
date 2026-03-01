# TEN Property Catalog Format

Property catalog files define custom Lua properties for TombEngine moveable and static object types. They are loaded automatically by Tomb Editor and WadTool from this folder at startup. Multiple XML files are merged; if the same property `internalName` is defined for the same object type in more than one file, the last file loaded (alphabetical order) takes priority.

---

## File Structure

```xml
<?xml version="1.0" encoding="utf-8"?>
<propertyCatalog>

  <moveable id="73">
    <property ... />
  </moveable>

  <static id="0">
    <property ... />
  </static>

</propertyCatalog>
```

The root element must be `<propertyCatalog>`. It may contain any number of `<moveable>` and `<static>` children in any order.

---

## Object Elements — `<moveable>` and `<static>`

| Attribute | Required | Description |
|-----------|----------|-------------|
| `id`      | Yes      | Numeric slot ID(s) this block targets. Supports all formats below. |

### `id` Formats

| Format  | Example           | Description                        |
|---------|-------------------|------------------------------------|
| Single  | `id="73"`         | One slot                           |
| List    | `id="73,74,75"`   | Explicit list of slots             |
| Range   | `id="73-80"`      | Inclusive range                    |
| Mixed   | `id="0-5,73,100"` | Any combination of the above       |

Each `<moveable>` or `<static>` block contains one or more `<property>` child elements. The same block may be repeated for the same ID in different files — properties are merged by `internalName`.

---

## Property Element — `<property>`

```xml
<property
    internalName  = "myProp"
    displayName   = "My Property"
    type          = "Float"
    defaultValue  = "1.0"
    category      = "Physics"
    description   = "Controls something important."
    hasAlpha      = "false"
    entries       = "Option A, Option B, Option C" />
```

### Attributes

| Attribute      | Required          | Default              | Description |
|----------------|-------------------|----------------------|-------------|
| `internalName` | **Yes**           | —                    | Lua API property name used in `SetProperty` / `GetProperty` calls. Case-insensitive in the editor; exact case is passed to the Lua API. |
| `displayName`  | **Yes**           | —                    | Human-readable label shown in the property grid. |
| `type`         | **Yes**           | —                    | Value type. See [Supported Types](#supported-types). |
| `defaultValue` | No                | Type-specific zero   | Initial value for new objects. Accepts both `defaultValue` and the shorter alias `default`. |
| `category`     | No                | *(none)*             | Groups properties under a collapsible header in the grid. Properties without a category appear at the top level. |
| `description`  | No                | *(none)*             | Tooltip text shown when hovering over the property name or value. |
| `hasAlpha`     | No (`Color` only) | `false`              | When `true`, an extra alpha (opacity) numeric field is shown next to the color picker. Ignored for all other types. |
| `entries`      | No (`Enum` only)  | —                    | Comma-separated list of entry names, e.g. `"Normal, Aggressive, Calm"`. Alternative to child `<entry>` nodes. |

> **Tip:** `displayName` falls back to `internalName` if omitted, but it is strongly recommended to always provide a friendly label.

---

## Supported Types

### `Bool`

A true/false toggle. Rendered as a checkbox.

| Attribute     | Format            | Example   |
|---------------|-------------------|-----------|
| `defaultValue`| `true` or `false` | `"false"` |

Compiles to Lua `true` / `false`.

---

### `Int`

A whole number. Rendered as a numeric spinner.

| Attribute     | Format            | Example |
|---------------|-------------------|---------|
| `defaultValue`| Integer literal   | `"100"` |

Compiles to a Lua integer literal.

---

### `Float`

A decimal number. Rendered as a numeric spinner with two decimal places.

| Attribute     | Format             | Example   |
|---------------|--------------------|-----------|
| `defaultValue`| Floating-point literal (`.` separator) | `"3.14"` |

Compiles to a Lua number literal.

---

### `String`

Free text. Rendered as a text box.

| Attribute     | Format       | Example      |
|---------------|--------------|--------------|
| `defaultValue`| Any text     | `"Lara"`     |

Compiles to a quoted Lua string. Inner quotes and backslashes are escaped automatically.

---

### `Vec2`

A 2D vector (X, Y). Rendered as two numeric fields.

| Attribute     | Format                              | Example    |
|---------------|-------------------------------------|------------|
| `defaultValue`| Two comma-separated floats          | `"20, 28"` |

Compiles to `TEN.Vec2(x, y)`.

---

### `Vec3`

A 3D vector (X, Y, Z). Rendered as three numeric fields.

| Attribute     | Format                              | Example       |
|---------------|-------------------------------------|---------------|
| `defaultValue`| Three comma-separated floats        | `"0, 100, 0"` |

Compiles to `TEN.Vec3(x, y, z)`.

---

### `Rotation`

Three Euler angles in degrees (X, Y, Z), each clamped to 0–360. Rendered as three numeric fields.

| Attribute     | Format                              | Example      |
|---------------|-------------------------------------|--------------|
| `defaultValue`| Three comma-separated floats        | `"0, 90, 0"` |

Compiles to `TEN.Rotation(x, y, z)`.

---

### `Color`

An RGB or RGBA color. Rendered as a color picker button. The alpha field is hidden unless `hasAlpha="true"`.

| Attribute     | Format                                          | Example                   |
|---------------|-------------------------------------------------|---------------------------|
| `defaultValue`| Three or four comma-separated 0–255 integers    | `"255, 128, 0"` or `"255, 0, 0, 128"` |
| `hasAlpha`    | `true` / `false`                                | `"true"`                  |

Compiles to `TEN.Color(r, g, b)` or `TEN.Color(r, g, b, a)`.

---

### `Time`

Hours, minutes, seconds and centiseconds. Rendered as four labeled text fields.

| Attribute     | Format                                          | Example         |
|---------------|-------------------------------------------------|-----------------|
| `defaultValue`| Four comma-separated integers: `h, m, s, cs`   | `"0, 1, 30, 0"` |

Compiles to `TEN.Time({h, m, s, cs})`.

---

### `Enum`

A named selection backed by a 0-based integer index. Rendered as a dropdown (ComboBox).

The entry list may be provided in two ways:

**Option A — inline `entries` attribute (compact):**

```xml
<property internalName="behavior"
          displayName="Behavior"
          type="Enum"
          entries="Normal, Aggressive, Passive"
          defaultValue="Normal"
          category="AI" />
```

**Option B — child `<entry>` nodes (verbose, suitable for many entries):**

```xml
<property internalName="priority"
          displayName="Priority"
          type="Enum"
          defaultValue="Medium"
          category="Logic">
  <entry value="None" />
  <entry value="Low" />
  <entry value="Medium" />
  <entry value="High" />
</property>
```

Both forms are equivalent. If both are present, the `entries` attribute takes precedence.

| Attribute     | Format                                              | Example    |
|---------------|-----------------------------------------------------|------------|
| `entries`     | Comma-separated entry names                        | `"A, B, C"` |
| `defaultValue`| Entry name **or** 0-based integer index            | `"Normal"` or `"0"` |

Compiles to a Lua integer: the first entry is `0`, the second is `1`, and so on.

---

## Two-Level Property System

Properties operate at two levels that the engine resolves at runtime:

| Level | Scope | Where edited |
|-------|-------|--------------|
| **Level 1 — Global** | All instances of an object type share one value | WadTool (stored in `.wad2`) |
| **Level 2 — Instance** | A single placed object overrides the global value | Tomb Editor (stored in `.prj2`) |

The compiled Lua script emitted per-level contains calls for both layers:

```lua
-- Level 1: type-wide defaults
TEN.Objects.SetMoveableProperty(TEN.Objects.ObjID.BADDY1, "behavior", 1)

-- Level 2: per-instance override (only when explicitly changed)
TEN.Objects.GetMoveableByName("baddy_boss"):SetProperty("behavior", 2)
```

---

## File Naming and Organisation

- Any `.xml` file anywhere inside this folder (including sub-folders) is loaded.
- Files are processed in alphabetical path order. Later files override earlier ones for matching `internalName` + object keys.
- Organise files however you prefer — by object, category, or game chapter.

---

## Example

See [Example.xml](Example.xml) for a runnable reference covering all types.
