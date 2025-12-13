## TEN Node function script files metadata reference

### Overview

This folder contains lua script files which will be referenced by both TE and TEN to work with node trigger system.
TE uses metadata from comments in these files to build UI, while TEN uses actual functions to execute node scripts.

On level compile, all files from this directory are copied to TEN `Scripts\NodeCatalogs` folder and executed
on level start-up, given that level contains any volumes. Therefore, NodeFunctions.lua file in TEN
`Scripts\NodeCatalogs` folder should never be modified.

### Format

Lua node scripts should follow this convention: several metadata entries should be followed by actual function
signature which should start with conventional **LevelFuncs.Engine.Node.** prefix. Function signature should be
followed by function body in arbitrary format. Amount of argument metadata entries should be the same as actual
function arguments, and should be listed in the same order.

### Argument naming and order

Actual function arguments (e.g. `arg1`, `arg2` and `arg3` in `LevelFuncs.Engine.Node.MyFunction(arg1, arg2, arg3)`)
should not be renamed after node went into production with official TE release - otherwise TE will not be able to
identify arguments on project file reloading. However, since TE version 1.8, it is allowed to rearrange node
arguments, if argument names were left untouched. You still need to preserve metadata entry order to correspond to
function order though.

Builders must be advised that they should migrate to version 1.8 as soon as possible, because skipping version 
1.8, together with possible node argument reorderings in the future, may lead to data loss in existing node setups.

### Metadata entry reference

Comment metadata entry reference (metadata block is indicated by a keyword which starts with !):

 - **!Name "NAME"** - NAME will be visible name for this function in node editor.

 - **!Conditional "True"** - this is a condition node, otherwise if "False", it's action
   (or if **!Conditional** is not specified).

 - **!Description "DESC"** - DESC will be a tooltip for a given function. You can use `\n` symbol to begin from a 
   new line.
   
 - **!Section "SECTION"** - this will define where the node will be found inside Tomb Editor.

 - **!Arguments "ARGDESC1" "ARGDESC2" "ARGDESC..."** - infinite amount of args, with **ARGDESC** parameters
   separated by commas as follows:

   - **0-100** - numerical value specifying width of control in percent of node width. For whole-line controls,
    this can be omitted.

   - **NewLine** - keyword which specifies if this and following arguments should appear on new UI line, until
    another argument with another **NewLine** is encountered.

   - **Boolean, Numerical, Vector3, String, Color, LuaScript, Moveables, Statics, Cameras, Sinks, FlybyCameras,
    Volumes, Rooms, SoundEffects, WadSlots, Enumeration, CompareOperator** - keywords which specify argument type
    and its appearance in UI.

   - **{DEFAULT}** - default value for this argument, contained in brackets. For Numerical value type, it can be
     provided as is, e.g. `{100}`. For String value type, default string must be quoted, e.g. `{"Default string"}`.
     Complex value types, such as Color or Vector3, should use TEN Lua API notation, e.g. `TEN.Color(255,255,255)`.

   - **[ENUMDESC1 | ENUMDESC2 | ENUMDESC...]** - custom enumeration descriptors for this argument, as follows:
  
      - For Numerical and Vector3 value types, first and second ENUMDESC values determine min/max UI range of
        value. Optional third argument specifies amount of decimal places (for integer, set it to 0). Optional
        fourth and fifth values determine mousewheel increment and alternate increment (with shift button 
        pressed) respectively.
      - For String value type, specifying **NoMultiline** as ENUMDESC hides button which engages multiline editing.
      - For Moveables and WadSlots lists, ENUMDESC values will filter out object ID names which contain any of
        ENUMDESCs only.
      - For Enumeration, ENUMDESC values will be displayed as is but converted to numericals on compilation
        in order of appearance.

   - **Any other string value except listed above** - tooltip for a given argument control.
   
 - **!Ignore** - if this keyword is used, nearest encountered function declaration will be ignored. Useful if you
   need to place helper functions which must be ignored by parser (however, it is recommended to use `_System.lua`
   file for those).


Metadata blocks can appear in any order.

Metadata parsing happens until real function block starts (which should start with **LevelFuncs.Engine.Node.** 
prefix).

Conditional node functions (those with **!Conditional = "True"** specified) must return boolean value, otherwise
their behaviour is undefined.

ARGDESC parameters can appear in any order, e.g. `!Argument "Foo, Numerical, 20"` is equal to
`!Argument "Numerical, 20, Foo"`.

There could be several **!Argument** blocks, which will append arguments to previously parsed ones.

There could be several **!Description** blocks, which will append as a new line to previous description block.

ENUMDESC parameters should NOT be quoted, or else parsing will fail miserably.


### Argument types (those you specify under **!Arguments** keyword)

   - **Boolean** - a value which can be either `true` or `false`. Appears as a checkbox with argument description
     as a label.
   - **Numerical** - numerical floating-point value. Range, limits and decimals can be specified by **ENUMDESC**
     descriptors (see above).
   - **Vector2** - Two floating-point values, can be used for 2D on-screen position. Range, limits and
     decimals can be specified by **ENUMDESC** descriptors (see above).
   - **Vector3** - Three floating-point values, can be used either for position or rotation. Range, limits and
     decimals can be specified by **ENUMDESC** descriptors (see above).
   - **String** - Raw text string, length is unlimited. If parsed to functions which draw on-screen strings, `\n` 
     combination may be used to start a new line.
   - **Time** - Time value, consists of 4 fields - hours, minutes, seconds and centiseconds.
   - **Color** - RGB color value, appears as color picker in UI.
   - **LuaScript** - Existing lua function list from level script file.
   - **EventSets** - Existing event sets currently present in level.
   - **Moveables** - A list of moveables in level which have lua names assigned.
   - **Statics** - A list of statics in level which have lua names assigned.
   - **Cameras** - Same as above, but cameras.
   - **Sinks** - Same as above, but sinks.
   - **FlybyCameras** - Same as above, but flybys.
   - **Volumes** - Same as above, but volumes.
   - **Rooms** - A list of existing rooms in level. Accessed by room name specified in TE UI.
   - **SoundEffects** - A list of sound effects. Internally converted to numerical effect slot ID.
   - **WadSlots** - A list of object slots which exist in all loaded wads. Internally accessed by numerical ID and/or 
     `Objects.ObjID.` lua enumeration which is identical to TE/TEN object slot enumeration.
   - **SpriteSlots** - Similar to previous type, but for sprite sequence slots.
   - **Enumeration** - Custom enumeration determined by **ENUMDESC** descriptors. Internally these descriptors are 
     converted to numerical index.
   - **CompareOperator** - Comparison operator enumeration, ranging from equal to various less-or-equal and more-or-equal
     operators. Internally converted to numerical value and should be passed to `LevelFuncs.CompareValue` helper function
     along with operand and reference to check against, like this: `LevelFuncs.CompareValue(operand, reference, operator)`.

### Example

```
-- !Name "Check moveable health"
-- !Description "Compares selected moveable health with given value."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Moveable to check" "NewLine, CompareOperator, 70, Kind of check"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 0 ], Hit points value" 

LevelFuncs.CheckEntityHealth = function(moveableName, operator, value)
	local health = TEN.Objects.GetMoveableByName(entityName):GetHP()
	return LevelFuncs.CompareValue(health, value, operator)
end
```

Here, **!Arguments** entry lists 3 parameters, which will be called "Moveable to check", "Kind of check"
and "Hit points value" respectively. First argument, "Moveable to check", will occupy whole second line of a
node UI, because width is not explicitly stated for it. Next two arguments, "Kind of check" and "Hit points
value", both will occupy same next line, because **NewLine** keyword is not specified for third argument.
Second argument, "Kind of check", will occupy 70 percent of line width, while "Hit points value" will occupy
30 percent. Note that arguments sitting on the same line should sum to 100 percent width, otherwise
UI symmetry is not guaranteed.

Also note that "Hit points value" argument is placed on separate **!Arguments** block, and this is correct, 
since it will appear after previous argument block. Also, being numerical argument, it will allow user to
only define values between 0 and 1000. All other values will be clamped. Decimal places for a value won't
show, as third parameter in square brackets is set to 0.

**LevelFuncs.CheckEntityHealth** function declaration should contain same amount of arguments and in the same
order as metadata argument entry. Therefore, **moveableName** will be read from "Moveable to check" UI argument,
**operator** will be read from "Kind of check", and so on.
