## AnimatingMIP
Syntax: AnimatingMIP= Slot , SectorDistance

To use in [Level] section

This command enable using of lower quality animation (MIP) when it has seen from a given distance.
This is useful to reduce the job of game engine

Arguments:

1 - Slot=Index of main Slot that will use a Mip version
This is number you see in name of animation in wad file.
For example in this case, that name will be: "Animating 13"

2 - SectorDistance: Distance from what it will be used the MIP object.
I presume this number is number of squares (sectors) for distance.

## ColAddHorizon
Syntax: ColAddHorizon= ENABLED/DISABLED

To use in [Level] section

To paste sky texture with horizon object.
It's a nice way to avoid black row between sky and horizon.
It works better if you use also a Layer1= command to set correct sky color value.

## DemoDisc
Syntax:  DemoDisc=DISABLED/ENABLED

To use in [Options] section.

Set if current game is only a demo.
If argument is ENABLED will be played only first level.

## Examine
Syntax:Examine= NumberOfExamine,NameInInventory,	TopBorder,DistanceFromCam,Orient_X, Orient_Y, Orient_Z, RotationFlag

To use in [Level] section

Examine command set data for a pickable item that the player wil can examine.
When player click action on examine object in inventory, it will be showed  the image of object in full screen and above this background will be showed the text you find in language.txt file.
Texts for examine object (there are only three examine object from 1 to 3) start immediatly first of [PSXStrings] section.
Three examine objects have different behavior about text to show:

Examine 1 object has no text, it is only a graphic image. It could be used to show maps with draws and special symbols.
To change this map you must use stripx to put new (high definition) textures on examine 1 object, because they will be own these texture to be showed at full screen.

Examine 2 object has two different texts: first text start in "language.txt" after header "Rules1:" and it will be showed in top half screen.
Second text start after header "Rules2:" and it will be showed in bottom half screen.
Object of examine 2 will be showed with reduced size at center of screen.

Examine 3 object is most common examine object.
Its text starts after header "PETEPOO:" and object will be showed to full screen on background while in foreground there will be the common text found after "PETEPOO:" tag.

About argument descriptions of Examine= command see description for Puzzle= command, it has the same syntax and meaning.

## FlyCheat
Syntax: FlyCheat= ENABLED/DISABLED

To use in [Options] section.

Enables or disales the possibility to do fly Lara typing on keyboard the letters: D O Z Y

Usually is useful let enabled this option only for debugging phase to check easily all room structure (and textures!) of your level.
Before developing your level you should always set this command to DISABLED

## FOG
Syntax: FOG= Red,Green,Blue

To use in [Level] section

Set fog color for current level.
Differently by fog bulbs, the FOG in script.txt set a fog effect enabled everywhere in current level but the fog will be more visibile only in wide spaces.
The fog is a good way to around the black color for very far zones (other 18 squares)

Arguments:

1 - Red intensity of fog ("210")
2 - Green Intensity of fog ("176")
3 - Blue intensity of fog ("99")

Remark:
To see fog in game the Volumetric FX must be DISABLED (!)

## Horizon
Syntax: Horizon= ENABLED/DISABLED

To use in [Level] section

Set the use of Horizon around current level.
If you set ENABLED the horizon will be visible.
If you set DISABLED the orizon will be not used and around the level will be the black color.

## InputTimeOut
Syntax: InputTimeout=FramesBy60

To use in [Options] section.

Set the frame rate of the game.
The argument is computed as Frames * seconds.
For example 18000 = 60 x 30 (30 frame per second)

It's better don't change this value because there are many old and slow computers that could have trouble using too high frame rates.

## Key
Syntax: Key= NumberOfKey,NameInInventory,	TopBorder,DistanceFromCam,Orient_X, Orient_Y, Orient_Z, RotationFlag

To use in [Level] section

The arguments and their meanings are the same of "Puzzle=" and "Examine=" commands but in this case will be defined a "key" instead of a puzzle item.

To have a description of arguments see PuzzleCombo= command.

## KeyCombo
Syntax: KeyCombo=NumberOfCombo,NumberOfPart, NameInInventory,	TopBorder,DistanceFromCam,Orient_X, Orient_Y, Orient_Z, RotationFlag

To use in [Level] section.

To set data about parts of key. It works like PuzzleCombo= but to describe part of key items instead puzzle items.

For description of arguments see PuzzleCombo= command.

## Layer1
Syntax: Layer1= Red,Green,Blue,Speed

To use in [Level] section when Horizon=ENABLED

This command set the color of sky texture and the speed of movements of sky texture

Arguments:

1 - Red Color ("160")
Set intensity of red for background sky color

2 - Green Color ("150")
Set intensity of green for background sky color

3- Blue Color ("192")
Set intensity of blue for background sky color

4 - Speed movements for sky texture ("7")
Range values:  -16   +16
If you use negative values the movement of animation  will be inverted

## Legend
Syntax: Legend=	TextToShowAtLevelBegin

To use in [Level] section AFTER {Name=} statement

Set the text will be showed at start of current level.
This mexage will be showed for about 4 seconds.

This statement is optional.

## LensFlare
Syntax: LensFlare=OrigX,OrigY,OrigZ,Red,Green, Blue

To use in [Level] section

It's used to create a light effect like shining for sun light on len of photocamera.

Note: in spite of same name of null-mesh object LensFlare this command will create a different (more irregular and suggestive effect)

Arguments:

1 - Reflex origin in X axis
2 - Reflex origin in Y axis
3 - Reflez origin in Z axis
4 - Red Intensity of reflex light
5 - Green Intensity of reflex light
6 - Blue Intensity of reflex light

To set value for origin it's adivsable use same method for LoadCamera settings (use F1 key and then to do many experiments)

## Level
Syntax: Level= FileName, NumberOfCdTrack

To use in [Level] section

Arguments:

1 - Level file ("DATA\KARNAK")
Path of .tr4 file, without .tr4 extension.

2 - Default Audio Track ("110")
Number of NNN.wav file of AUDIO folder, to use as default background sound for current level

## Lightning
Syntax: Lightning= ENABLED/DISABLED

To use in [Level] section

Enables or disables thunders (sound) and lightning (white flashes) on the sky.
You'll enable this effect for levels where there is a storm

## LoadCamera
Syntax: LoadCamera= SourceX,SourceY,SourceZ,TargetX,TargetY,TargetZ,SourceRoom

Set the position of cam to create an image to use in 'load game' screen in game.

To use in [Level] section

Arguments:

1 - Source X position of cam in game units ("76406")
2 - Source Y position of cam in game units ("-3880")
3 - Source Z position of cam in game units ("40584")
4 - Target (where the cam is watching) X position ("75794")
5 - Target Y position ("-3220")
6 - Target Z position ("40328")
7 - Number of room where is placed the cam ("23")

It's not reasonable set manually these value.
You must read them and copy them in script.txt file using F1 key (1) while you are in game.
In top left corner of screen will be showed above numbers that are in according with current view the game in that moment.

Remark:
(1) F1 feature works only when in script.txt the command FlyCheat =	ENABLED.

(2) If there are two LoadCamera= commands in same [level] section, the game will use the second LoadCamera= command to set screen for loading savegame and reload screen after the death of lara.
Probably the first LoadCamera= will be used in some circustances with Finish trigger to jump to next level but I'm not sure.

## LoadSave
Syntax: LoadSave= ENABLED/DISABLED

To use in [Options] section

Enable or disable the load / save operation in game.
If you set DISABLED in this command, the player will can not load game or save the game.

This command seems not very usefull, because it works for ALL levels.
It's strange, pheraps it's theorically possible set in [level] section a pickup item with data for Save game to create a game method like in tomb raider 3 for playstation, where player can save only after had picked up a blu crystal.
To explore...

## Mirror
Syntax: Mirror= RoomNumber, XOriginRoomBy1024

To use in [Level] section

Set a mirror effect.

Arguments:

1 -RoomNumber. This is the number of room where lara can see the mirror, pratically is the room in front of mirror (hidden) room.

2 -XOriginRoomBy1024 = X Origin in room edit units.
This number is in hexadecimal format.
You read this value in Info panel belove the 3d panel
To get the final value to type in Mirror= command you have to add 1 to X Origin and then muplicating this sum for 1024.

For example if X origin of room in front of mirror is 12 (decimal)
You get the hexadecimal number in following way:

12 + 1 = 13
13 * 1024 = 13312
13312 = (converted in hexadecimal=) $3400

Remark: the mirror in room edit must be always over left wall of room in front of mirror.

## Name
Syntax: Name= NameForNewGameScreen

Remark: to use at start of [Level] section

Set the name of level. This name will be showed in New Game screen.
If you change this name in script.txt file you must change in same way the name in english.txt file.

## Pickup
Syntax: Pickup=	NumberOfPickup,NameInInventory,	TopBorder,DistanceFromCam,Orient_X, Orient_Y, Orient_Z, RotationFlag

To use in [Level] section

The arguments and their meanings are the same of "Puzzle=" and "Examine=" commands but in this case will be defined a generic pickable item  instead of a puzzle or key item.

To have a description of arguments see PuzzleCombo= command.

## PickupCombo
Syntax: PickupCombo=NumberOfPickup, NumberOfPart, NameInInventory,	TopBorder,DistanceFromCam,Orient_X, Orient_Y, Orient_Z, RotationFlag

To use in [Level] section.

To set data about parts of pickable (not puzzles) items. It works like PuzzleCombo= but to describe part of generic pickup  items instead puzzle items.

For description of arguments see PuzzleCombo= command.

## PlayAnyLevel
Syntax: PlayAnyLevel= ENABLED/DISABLED

To use in [Options] section

Set if player will can choose the level to play in any order or if the game will start automatically from first level of script.txt file (excluding title.tr4 level, of course)

It's a common error of beginners let this command to ENABLED setting.
Really enabled is useful only for original level editor levels but when you create your adventure you should set always as DISABLED this command in way to start from first level and then go on in following levels only when player reachs this result in game.

## Puzzle
Syntax: Puzzle=NumberOfPuzzle,NameInInventory,	TopBorder,DistanceFromCam,Orient_X, Orient_Y, Orient_Z, RotationFlag

To use in [Level] section

Set data for a pickable item.
For description see "PuzzleCombo=" command
The arguments are the same, the only difference is that in Puzzle= command is missing the "Index of part" argument.

## PuzzleCombo
Syntax: PuzzleCombo= NumberOfPuzzle, NumberOfPart,NameInInventory,	TopBorder,DistanceFromCam,Orient_X, Orient_Y, Orient_Z, RotationFlag

To use in [Level] section

Set data for puzzle combo object you'll use in current level.
Note: combo is a Combined Object, it's format from two or more parts.
Before using it (putting it in hole item) the different parts will have been pasted togheter.

Arguments:

1 - Index  of Object ("1")
This number is the same of "puzzle done" object name in wad file.
For example if current combo puzzle owns to puzzle item 4, in this field there will be the number "4"
Also for other command (pickup, key ect) values same rule, for example:
Puzzle Item 3 will have "3" in this field.
Key 5 will have "5" in this field. ect.

2 - Index of part of the object  ("1")
For example if this combo item has 3 parts, there will in script.txt three PuzzleCombo statement.
In first PuzzleCombo the index of part will be "1", in second "2" and in third "3"

3 - Name of object  ("Sun Disk")
This name will be showed in inventory when player picked up this item
If you change the name in script.txt file you must perform the SAME change in english.txt.
About other language.txt file is not necessary have same name, but script.txt and english.txt names must have always the same names.

Following six numbers are hexadecimal values to set view of object in inventory
$0000,$0180,$0000,$0000,$0000,$0002
To explain better we'll name them:
A Number, B Number, C Number, D Number, E Number and F Number

4 - A Number ($0000)
Position (in pixel) from top border of screen.
Remark: not always the origin of object is its top-left visible point. The origin depends by position of object in 3d structure. For this reason, it's useful sometimes to  increase or decrease this A number to have the object at same height of other inventory objects.
You can insert also negative numbers, but you must type it in hexadecimal format. For example to set -10 (10 pixels over the top border) you should  type the number $FFF6

5 - B Number ($0180)
Distance from (virtual) cam
This argument  is  useful to increase or reduce the size of object in inventory.
If you increase the distance the object will become more little
If you decrease the distance the object will become more big.
This value is in game units (1 square = 512 units)
For example if you set the value $0200 (512 in decimal) you'll have the view of object like if you watch it in game while it is at one square of distance.
If you set $0000 in this field the object will be huge because it will be in front of cam and it will cover the whole screen.
Usually you'll use value enclosed in range: $0200 - $0500

6 - C Number ("$0000")
This argument set the orientation of cam on X axis.
It's complicated to explain but if you try to change this value you'll understand what I mean.
The values for this argument could be go from $0000 to $FFFF but usually will be used only four values:

$0000 = North (top view)
$4000 = East (right view)
$8000 = South (bottom view)
$C000 = West (left view)

7 - D Number ("$0000")
Same meaning of C Number but it regards the Y axis.

8 - E Number ("$0000")
Same meaning  of C and D numbers but this regards Z Axis.

9 - F Number ("$0002")
Bit mask to enabled/disable special functions.

$0002 = Enable the rotation of object
$0008 = I'm not sure, pherpas it forces rotation center of object at center of its bounding box.

## RemoveAmulet
Syntax: RemoveAmulet = ENABLED/DISABLED

To use in [Level] action

If RemoveAmulet = Enabled, will be removed from inventory the amulet in current level.

## ResetHUB
Syntax: ResetHUB= NumberOfLevel

To use in [Level] section

Force the game engine to reload also default meshes like that of Lara or side-car, jeep ect.
You must use this command if in next level lara will have different mesh structures

Arguments:

1 - Level number first of that is necessary reload lara meshes ("3")

Pratically, if in level 2 you use young lara, while in next level number 3, you use the "old" lara, you must type in [Level] section of level 2 the command:
ResetHUB= 3
to force reloading before loading the third level (the next).

Remark:
Different textures (skin) don't require ResetHUB command.

## ResidentCut
Syntax: ResidentCut= NumberOfCutscene, RoomNumber

To use in [Level] section

Declare an internal cut scen in current level.

Arguments:

1 - Index of internal cut scene ("1")
This index start from 1

2 - Room number ("5")
The room number where start the cutscene.

## Timer
Syntax: Timer= ENABLED/DISABLED

To use in [Level] section

Enables or disables the overlapped timer used in Race For The Iris

Remark:

To start the timer over the screen you must:

- To put Von Croy object in your level
- Trigger (to get visibile) Von Croy
- Trigger flip effect 45

To stop the timer in traditional (game) ways is not easy.
However with Media Manager you can use an equipment media to set savegame field named "Screen timer" to 0. This method works correctly only if FIRST to perform this media in game has been triggered flip effect 45 with timer -1 (to stop movements of von croy) or flip effect 46 (to kill Von Croy)

## Title
Syntax: Title=	ENABLED/DISABLED

To use in [Options] section

To set if use or less the title.tr4 level as background for introduction screen.
The title.tr4 is a common level but with a flyby camera sequence. It will be used only for 'decorative' effects.

## Train
Syntax: Train= ENABLED/DISABLED

To use in [Level] section

Enables / disable the train effect.
With train effect the train is still while the horizon rounds in loop to give the idea of the train movement.

If you use Train=Enabled you should use also AddColHorizon and Layer1 command.

## Unknown
There are some commands that is not clear how to do work them.

If you wish do experiments here there is the list of unknown script commands:

Weather=
To use (I presume) in [Level] section
Probably works with ENABLED/DISABLED because in script.exe there is no magic word like "rain" or "snow"
Remark: This command has been removed in Next Generation engine, and subtistuted with couple of commands Rain=Enabled/Disabled and Snow=Enabled/Disabled

StarField=
From its name it seems to regard night sky with stars but it's not clear how work.

Layer2=
The syntax of arguments is the same of Layer1= but what is the difference?

Security=
This command is in [Options] section and has always the value: $55 (eg 85 decimal)
But what is this number?
Pheraps regards enabling/disabling other cheats, like that for infinite savegames, weapons ect.
Or is it used to ability the verifying of savegame with checksum compute?
Mistery...

Pulse=
Probably the syntax is:
Pulse=ENABLED/DISABLED

NoLevel=
It's very bizzare, but this command is really used in Tomb Raider the last revelation.
This is the whole [level] section where it has been used:

[Level]
Name= Yes
NoLevel= ENABLED
Level= A,0

Of course, there is no A.tr4 level available.
Probably it's an hardcoded method to handle the conditional tests.
After seneth game there are two different paths and above "YES" level comes exactly before of that level.
Another NoLevel command is used in other level named "NO".

## UVRotate
Syntax: UVrotate= SpeedOfScrollingTextures

To use in [Level] section.

Set the speed of UV Scrolling textures.

Arguments:

1- Speed texture animation ("8")
Apparently this number is a single byte (8 bit)
This means that write -2  or 254 has same results.

Range seems enclosed in -16  +16

If number is negative (or greather than 127)  direction of animation will be inverted.

Remark:
In previous versions of TRNG documentation it was been wrongly described this command how a way to affect the speed of waterfall textures. In the reality this command doesn't affect the waterfall textures but only the scrolling textures applied on the floor/walls of the level.
If you wish change the speed of waterfall textures you can use this script command:

Customize= CUST_WATERFALL_SPEED, NewSpeed

Where NewSpeed is a negative value to set the speed of scrolling.
See the description of CUST_WATERFALL_SPEED constant in Reference panel for more infos.

## YoungLara
Syntax: YoungLara = ENABLED/DISABLED

To use in [Level] section

Used to "inform" the game engine that in current level will be used the young lara.
Since young lara is very different by old lara (young has not weapons, and she has two hair trix while the old lara has only one) it's necessary advise the game engine.

## [Language]
Start Language section.

List of language.txt file with different languages.
For each File= statement will be created a language.dat file (for example JAPAN.DAT file)

Syntax:

[Language]
File=	0,ENGLISH.TXT
File=	1,FRENCH.TXT
File=	2,GERMAN.TXT
File=	3,ITALIAN.TXT
File=	4,SPANISH.TXT
File=	5,US.TXT
File=	6,JAPAN.TXT
File=	7,DUTCH.TXT

Arguments:

1 - Index number ("1,")
Sort position for language.dat.  The file with number "1" is  important because level names and puzzle item names in this file must be the same of them in script.txt file.

2 - Txt file name ("ENGLISH.TXT")
Source txt file where get the texts for that language

You can add at bottom of list other language.txt file

## [Level]
Start the Level section.
Each tr4 file must have its own [level] section, where you set audio track, level name, puzzle item reference and more.

Syntax:

[Level]
Name=		Playable Tutorial Level
Legend=		The year 2000, somewhere in Egypt...
Horizon=	ENABLED
Layer1=		160,160,192,7
PuzzleCombo=	3,1,Cartouche Piece 1,	$0000,$0400,$0000,$0000,$0000,$0002
PuzzleCombo=	3,2,Cartouche Piece 2,	$0000,$0400,$0000,$0000,$0000,$0002
Puzzle=		3,Ba Cartouche,		$0000,$0400,$0000,$c000,$8000,$0002
Puzzle=		5,Eye Of Horus,		$0017,$0500,$0000,$0000,$0000,$0002
PuzzleCombo=	5,1,Eye Piece,		$0017,$0500,$0000,$0000,$0000,$0002
PuzzleCombo=	5,2,Eye Piece,		$0017,$0500,$0000,$0000,$0000,$0002
Puzzle=		6,The Hand Of Orion,	$0000,$0400,$8000,$c000,$0000,$0002
Puzzle=		8,The Hand Of Sirius,	$0000,$0400,$8000,$c000,$0000,$0002
LoadCamera=	0,0,0,0,0,0,255
LoadCamera=	11088,-1100,28896,11119,-1399,31486,0
Level=		DATA\TUT1,107

For descriptions of [Level] statements see statement names in current table

## [Options]
Set the start of Option section

Syntax:

[Options]
LoadSave=	ENABLED
Title=		ENABLED
PlayAnyLevel=	ENABLED
InputTimeout=	18000		; frames * seconds = 60x30
FlyCheat=	ENABLED
Security=	$55
DemoDisc=	DISABLED

For descriptions of above statements see the specific statement names in  current table

## [PCExtensions]
Syntax:

[PCxtensions]
Level= .TR4
Cut= .TR4
FMV= .BIK

Assign file extensions for PC game version

1 - Level file (Level=)
2 - Cutscene files (CUT=)
3 - Movies (FMV=)

It's not useful to change these values.

## [PSXExtensions]
Syntax:

[PSXExtensions]
Level= .PSX
Cut= .CUT
FMV= .FMV

Assign file extensions for Playstation 1 game version

1 - Level file (Level=)
2 - Cutscene files (CUT=)
3 - Movies (FMV=)

It's not useful to change these values.

## [Title]
Start the Title section.
Title section regards the creation of title.tr4 file will be showed at start of game, when on screen there are the menu: [New game] [Load game] [Options] ect.

Statements in [Title] section are the same of [Level] sections but in [title] section are missing all statements for puzzle items.

For description of [Title] statments see statement names in current table

