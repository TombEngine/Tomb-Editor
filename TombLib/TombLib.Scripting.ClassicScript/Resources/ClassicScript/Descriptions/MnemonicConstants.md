## ADD_BLOOD
Add blood (vlady) to current object.

## ADD_FLAME
Add fire to current object.

## ADD_LIGHT_BLINK
Used in AddEffect command
With this value you add a blinking light to some moveable.
The blinking light is the same of flare light, where the intensity of the light changes a bit in random way.

## ADD_LIGHT_FLAT
Used in AddEffect command
With this value you add a light of wished color to some moveable.
For "flat light" we mean a common non-blinking, non-spot light.
It's like a light-bulb.
Anyway, you can create a ritmic blink also with flat light setting correct tick frame in DurateEmit and DuratePause field.
If you use this method the blinking will be anyway different than ADD_LIGHT_BLINK since in that case the blinking is random to simulare a fire, while the blinking you get with flat light + durate emit and durate pause will be always a perfect blink, like it happens with some electronic machines.

## ADD_LIGHT_GLOVE
Used in AddEffect command
This light has the shape of a sphere. Differently than other lights this glove light is opaque, i.e. not transparent, like the yellow halo around the lights in a fogged night.

## ADD_LIGHT_SPOT
Used in AddEffect command
This light works like a light-spot, i.e. it generates a cone of light following a given direction like the headlight of sidecar.
For this light type it's important the facing to choose the direction of the light cone.
The default facing is that of moveable where you add this effect, anyway you can change this facing using the FADD_ flags in FlagsEffect (FADD_) field.

Remarks:
- You could have trouble if there is in same time the jeep or boats using headerlight, because this add effect uses the same light resources to generate this light-spot.
- With this kind of light you have to type  a very high Intensity value, about 200.
- You cann't change the standard yellow-white color for spot light.

## ADD_MIST
Add mist to current object. Mist are sprays of water. You see the mist in waterfall.
When you use ADD_MIST there are four additional fields in AddEffect command and the new syntax of AddEffect command becomes the following:

Syntax: AddEffect=Id, EffectType (ADD_), FlagsEffect (FADD_), JointType (JOINT_), DispX, DispY, DispZ, DurateEmit, DuratePause, Extra1 SizeMistBall, Extra2 NumberOfMistBalls, Extra3 ColorMist, Extra4 PersistenceOfMist

These are the descriptions of extra parameters:

Extra1 = Size of mist ball. Default value is 12
Extra2 = Number of mist balls. Default is 1. You can set a max of 4 mist balls, they will be placer over an ideal line. The mist line will be oriented following facing of current moveable,anyway you can rotate the facing of mist line using rotate flags.
Extra3 = Color of mist. You can set a MIST_COL_ value. You find all colors in reference panel of NG_Center program
Extra4 = Persistence time of mist. Default value = 6. You should use this field only if the moveable is moving fastling and you want let a wake. Bigger values, longer wakes.

## ADD_SMOKE
Add smoke to current object.

## AMMO_ADD_GUN_SHELL
Used in Customize=CUST_AMMO command
It used to add the shell falling down after shooting.
If you use this flag it will be used the mesh of slot GUNSHELL for the shell.

## AMMO_ADD_SHOTGUN_SHELL
Used in Customize=CUST_AMMO command
It used to add the shell falling down after shooting.
If you use this flag it will be used the mesh of slot SHOTGUNSHELL for the shell.

## AMMO_PUSH_LARA
When lara shots the back stroke will move lara backwards. You could use this flag with heavy ammo like grenade. You have to type in Extra field the distance of movement. The distance units are 1024 = one sector, 256 = one click.

## AMMO_PUSH_TARGET
When the ammo will hit the enemy, he will be pushed (moved) like if the impact of ammo had moved him. You have to type in Extra field the distance of movement. The distance units are 1024 = one sector, 256 = one click.

## AMMO_REMOVE_SHOTGUN_SHELL
Used in Customize=CUST_AMMO command
This flag remove the shell case effect. You should use this flag only for shotgun ammo.

## AMMO_SET_GRENADE_TIMER
To use only with greande ammo. You can change the number of second required to do explode the grenade. The default value is 4 seconds. You have to type the number of seconds in Extra field.

## BAR_AIR
Used in Customize=CUST_BAR

## BAR_COLD
Used in Customize=CUST_BAR command
Customize the bar showed in cold water rooms.
Pease, note that damage bar color could be changed also in Damage= script command. Trng will use color of Damage command in the case you type IGNORE in Color1 field of bar in CUST_BAR command.

## BAR_CUSTOM1
Used in Customize=CUST_BAR command.
The Custom bar is different by other health, dash, air  bars, because in this case there is no predefined target for it.
This is a free bar you can use for you new skills.
In the Customize= command you set the size and colors like for other bars, the only difference is that in (final) extra field you have to type a Variable Placefolder to identify the value used by your custom bar.
For example if you want the bar shows the value of Local Byte Delta1 variable you'll type as Extra field the value #0048 (see Variable placefolder list in Reference panel of NG_Center) and everytime you'll show the bar with some flipeffect the value used will be get by Local Byte Delta1.
When you want clear the custom bar just you set 0 in Local Byte Delta1, ect.
The custom bars have the full value with 100 and the empty value with 0, anyway using variables you can modify the source value to get correct values.
For example, if you have your original value that could be in the range 0 - 1000 (that is different by default range 0 - 100) just you divide your original value by 10 before showing the bar.
When you need of this pre-compute it's better don't set as variable for custom bar the your real variable but another variable, like Current Value variable, and then you'll perform the compute and you copy the result in Current Value and then you show the bar.
Using the flipeffects for variables it's easy adjust your source value with 0 - 100 range. Another example could be if you want have an inverse bar, where the bar if fully filled when the value is 0 and it is empty when the value is 100.
In this case the compute to perform will be: 100 - {My original value} = {value to put in bar variable).
Remarks:
- If you want add a text to describe the mean of your custom bar you have to use the flag FBAR_SHOW_BAR_NAME. See the description of this flag for more inos.
- Once you showed your custom bar, it's not necessary peform another show bar flipeffect everytime you update the bar value, just simply you change the bar variable and the bar will show new updated value byself.

## BAR_CUSTOM2
Used in Customize=CUST_BAR command
Set the second customizable bar. See the description of BAR_CUSTOM1 constant for more informations about customize bars.

## BAR_CUSTOM3
Used in Customize=CUST_BAR command
Set the third customizable bar. See the description of BAR_CUSTOM1 constant for more informations about customize bars.

## BAR_CUSTOM4
Used in Customize=CUST_BAR command
Set the fourth customizable bar. See the description of BAR_CUSTOM1 constant for more informations about customize bars.

## BAR_DAMAGE
Used in Customize=CUST_BAR command
Customize the bar showed in damage rooms.
Pease, note that damage bar color could be changed also in Damage= script command. Trng will use color of Damage command in the case you type IGNORE in Color1 field of bar in CUST_BAR command.

## BAR_DASH
Used in Customize=CUST_BAR

## BAR_HEALTH
Used in Customize=CUST_BAR

## BAR_LOAD_LEVEL
Used in Customize=CUST_BAR

## BINF_COMPASS
Used into Customize=CUST_BINOCULARS command
To show infos about compass in the binocular view, you have to add this BINF_COMPASS flag.
Then you'll have to set in CompassRectAndFlags field a BINT_... value to choose the kind of compass view. You can choose only between BINT_STRIP value (to use an image with graduate strip about cardinal scale) or BINT_NUMERIC value to show degrees of cardinal points in numeric format.

## BINF_LIGHTNESS
Used into Customize=CUST_BINOCULARS command
If you wish add infos about lightness intensity you add this flag.
The lightness intensity will be computed only on a little, centered in the middle of the screeen, rectangle.
If you show the lightness infos in numeric format, it will be used the ISO/DIN scale (0 / 39).
Differently, using a bar, the full bar will be the max light while the full dark will be an empty bar.

## BINF_LIGHT_SWITCH
Used into Customize=CUST_BINOCULARS command
With this flag you enable a graphic switch to show when player is using light in binocular view.
The switch will be a rectangle or cirlce zone where the color will change in according with switching on or off of the light.
In the rectangle linked with light switch, you set as foreground color the color signals the switched ON of light, while the background color will be used when light is unused.

## BINF_NOTATION_EXTENDED
Used into Customize=CUST_BINOCULARS command
When you enabled compass infos but you didn't use the compass strip, the info about compass will be showed as a number of degrees.
The short notation is like: 123�
where the degrees begin from 0 (north) increasing in clockwise sorting passing through 90� (EAST), 180� (SOUTH) and 270� (WEST)
If you wish, you can use the extended notation, where the degrees will cover only 0-89� degrees, beginning from first cardinal point.
Example:  "N 45�"
It means NE (or 45�)
"E 20�"  means "110�" in (about) South-east direction

## BINF_PROGRESSIVE_ZOOM
Used into Customize=CUST_BINOCULARS command
This flag changes the speed and progression of zoom.
Old method was very fast and it had always the same speed.
The new (progressive) method tries to begin slowly, increasing the speed only when player keep continuosly down the scale in key.
The progressive zoom works better to have a fine tuning of scale factor, for this reason it is suggested when you set the super zoom skill, since in that situation the speed of old method work badly with highest magnifying.

## BINF_SEXTANT
Used into Customize=CUST_BINOCULARS command
To add sextant to binocular view you have to add this flag.
Sextant gives infos about vertical degrees of current view. When lara looks up the degrees will be increased, while then she looks lower the degrees will decrease.
If you use this flag you have also to set a BINT_... value in SextantRectAndFlags field to choose the kind of data showed for sextant.

## BINF_SOUND_ON_LIGHT
Used into Customize=CUST_BINOCULARS command
When you used the BINF_LIGHT_SWITCH flag to show a switch for light mode, you can add also the BINF_SOUND_ON_LIGHT flag to enable a sfx sound when the player hit (first time) the light mode.
By default will be used the SFX 369 (LARA_CLICK_SWITCH)  but you can change this value using the Customize=CUST_SFX with the TS_BINOCULAR_LIGHT constant.

## BINF_SOUND_ON_ZOOM
Used into Customize=CUST_BINOCULARS command
If you wish having a sound while the player is zooming in or out, you can add the BINF_SOUND_ON_ZOOM flag to the flags.
By default will be played the SFX 309 (MAPPER_MOVE), anyway you can modify this sound using the Customize=CUST_SFX with the TS_BINOCULAR_ZOOM constant

## BINF_SUPER_ZOOM
Used into Customize=CUST_BINOCULARS command
This flag improves the zoom-in factor of binocular.
The max magnifying will pass from 4.2x upto about 40x.
Note that you can omit this flag from script, and enable it using a flipeffect. This method may be useful in game to simulate a change to binocular to improve it thanks to some target reached by lara in game.
This means that it's possible improve binoculars in game, but it's not possible to do the opposite, removing the superzoom once it has been supplied.

## BINF_SWING_COMPASS
Used into Customize=CUST_BINOCULARS command
When you use compass strip, you can add the BINF_SWING_COMPASS flag, to simulate a random swinging of compass everytime player move the binoculars.

## BINF_SWING_SEXTANT
Used into Customize=CUST_BINOCULARS command
With this flag you enable a simulation of swinging for sextant value (in strip mode) when the player moves the binoculars

## BINF_ZOOM
Used into Customize=CUST_BINOCULARS command
To enable the showing of infos about current zoom-in factor.
See also the description of ZoomRectAndFlags field.

## BINT_BAR
Used into Customize=CUST_BINOCULARS command
If you wish a solid bar misure the intensity of some value in binocular screen, you have to use the BINT_BAR value in same field with ID for rectangle.
Note: remember that when you use a bar, it's necessary you set in the PARAM_RECT command (linked with current binocular infos to show...) the values for Foreground color (the main color of the bar) and BackGround color (the background color, the empty color, of the bar).

## BINT_NUMERIC
Used into Customize=CUST_BINOCULARS command
To show the info about compass in numeric format you have to use this BINT_NUMERIC type in the CompassRectAndFlags field.
The default format is as simple number of degrees from 0 to 359.
Anywya you can use also the extended format adding to the FLAGS (BINF_...) field the BINF_NOTATION_EXTENDED flag.
See description of BINF_NOTATION_EXTENDED flag for more infos

## BINT_PATTERN_BAR
Used into Customize=CUST_BINOCULARS command
A pattern bar is like a bar, where the level of some value will be showed as length of a solid bar, but with pattern bar you can use a pattern instead by a solid color.
To draw your pattern bar just drawing it in the binocular screen, drawing as it should be at full size, then in the rectangle (in the script) to set the size of the bar you should type in the BackGround color field of PARAM_RECT command, the color below the pattern bar, i.e. the color used to erase the pattern to simulate a shorter bar.
For example if your pattern bar is above a black zone you'll set a black color in background field.

## BINT_ROUND
Used into Customize=CUST_BINOCULARS command
Currently this type works only in LightnessRectAndFlags field to force the light switch to have a circle shape.

## BINT_STRIP
Used into Customize=CUST_BINOCULARS command
You can show data whereby a strip for compass and sextant.
A strip is a slim image with a graduate scale.
The strip for compass should have always the size of 1400 x 64
You should use the compass strip in lightning demo level as reference for position of cardinal points and degrees.

## BKGDF_ADD_COLOR_AND
Used into Customize=CUST_BACKGROUND command
This flag perform an AND operation between the game scene colors and the color whom you set the ColorRGB ID in Parameter field.
The AND operation returns a darker image with a preponderance of the color you linked with it.
With BKGDF_ADD_COLOR_AND flag, it's better set color numbers with many bit set to 1, like 255, 127, 63, 31 or 15
For example to have the game scene changed with a dominant red color you could use the color:
ColorRGB= ID, 0, 127, 0
Above color, preserve the green tone while clear fully all other tones.
If you don't wish loosing all other colors, you can set some value also in other tones but with lower values:
ColorRGB= ID, 15, 127, 15

Note: This flag works only togheter with BKGDT_INVENTORY type + BKGDF_KEEP_GAME_SCREEN flag since it performs a change on original game scene.

## BKGDF_ADD_COLOR_OR
Used into Customize=CUST_BACKGROUND command
This flag perform an OR operation between the game scene colors and the color whom you set the ColorRGB ID in Parameter field.
The Or operation returns a lighter image with a preponderance of the color you linked with it.
With BKGDF_ADD_COLOR_OR flag, it's better set colors with perfect power by 2, like 128, 64, 32 or 16
For example to have the game scene changed with a dominant red color you could use the color:
ColorRGB= ID, 128, 0, 0

Note: This flag works only togheter with BKGDT_INVENTORY type + BKGDF_KEEP_GAME_SCREEN flag since it performs a change on original game scene.

## BKGDF_ADD_COLOR_XOR
Used into Customize=CUST_BACKGROUND command
This flag perform a XOR (exclusive OR) operation between the game scene colors and the color whom you set the ColorRGB ID in Parameter field.
This flag creates a result similar to a abstract artwork, where all colors will be exchanged in chaotic way.
Anyway, since each original color will be always replaced always with the same (new) color, the scene will remain understandable but with weird colors.
With this flag it's better using numbers with many bits, like 255, 127, 63, 31, 15
If you wish invert only a color tone you can let to 0  other tones and in this way these will be no changed.
For example to change only red tones of original game scene you can use as color:
ColorRGB= ID, 255, 0, 0

Note: This flag works only togheter with BKGDT_INVENTORY type + BKGDF_KEEP_GAME_SCREEN flag since it performs a change on original game scene.

## BKGDF_ADD_DARKNESS
Used into Customize=CUST_BACKGROUND command
This flag add a dark shadow to original game scene, giving the idea of a transparent effect like a plate glass over the game scene.
In Paramater field you have to set the level of darkness with a number between 3 and 255 where,  where 3 is almost no darness and 255 is full black. So you'll use intermediate values.
Reasonable settings are in the range between 50 / 200

Note: This flag works only togheter with BKGDT_INVENTORY type + BKGDF_KEEP_GAME_SCREEN flag since it performs a change on original game scene.

## BKGDF_ADD_NEGATIVE_EFFECT
Used into Customize=CUST_BACKGROUND command
This flag inverts all colors of game scene creating an effect like a negative film.
This effect works alone, ignoring further color set in Paramater field.

Note: This flag works only togheter with BKGDT_INVENTORY type + BKGDF_KEEP_GAME_SCREEN flag since it performs a change on original game scene.

## BKGDF_COLORIZE
Used into Customize=CUST_BACKGROUND command
This flag works only with BKGDT_BINOCULAR and BKGDT_LASER_SIGHT types.
You can add a color to the screen when lara is looking with the binoculars or lasersight.
When you use this flag, you have to type in Parameter field, the ID of a ColorRGB= command with the wished color to use.

## BKGDF_HIDE_LOADING_BAR
Used into Customize=CUST_BACKGROUND command
If you are cutomizing the BKGDT_LOADING_LEVEL screen, you use this flag to hide the loading bar.

## BKGDF_HIDE_SPRITE_LASER_SIGHT
Used into Customize=CUST_BACKGROUND command
This flag works in according with the BKGDT_LASER_SIGHT type.
If you wish remove that little red point, a sprite, you can add this flag.

## BKGDF_KEEP_GAME_SCREEN
Used into Customize=CUST_BACKGROUND command
If you don't wish use a custom image for background but simply using the current game screen, i.e. the last image in game, you can add this flag.
When you use BKGDF_KEEP_GAME_SCREEN flag, you can omit the image number in image field.
Note: this setting works fine only where there is a good game image first of its phase, as it happens for inventory, but it's not advisable for title, and neither enabled for binocular or lasersight types.

## BKGDF_MINIMAL_LOADING_TIME
Used into Customize=CUST_BACKGROUND command
This flag works in according with the BKGDT_LOADING_LEVEL type.
If you wish that the image will be showed at least for a given time, you can add the BKGDF_MINIMAL_LOADING_TIME flag, and set in Parameter field the number of seconds in that the image will be (at least) showed.
Note: in the case the loading of next level will require an higher time to be loaded you cann't reduce this loading time with this setting, of course.
You should use this flag only to be sure that the player was able to see the image you used for loading screen, for example, because there are some texts you wish he was able to read before next level began.

## BKGDF_SEMI_TRANSPARENT
Used into Customize=CUST_BACKGROUND command
This flag works only paired to BKGDF_KEEP_GAME_SCREEN flag.
With BKGDF_SEMI_TRANSPARENT flag you can force a custom image (with ImageId set in ImageId field of Customize=CUST_BACKGROUND command) over the current game scene, choosing the transparency level of your custom image.
You have to type in Parameter field the opacity level of your custom image with a value, where 0 = fully transparent and 255 = fully opaque.
Reasonable values will be in the range: 100 / 150.

## BKGDF_SKIP_LOADING_TIME
Used into Customize=CUST_BACKGROUND command
This flag works only togheter with BKGDF_MINIMAL_LOADING_TIME flag.
If you use BKGDF_MINIMAL_LOADING_TIME flag, the background for loading level will be kept on screen until the time you set, in spite the effective loading of level could be already completed.
If you wish that the player was able to skip in advance the background and go to next level, you can add the BKGDF_SKIP_LOADING_TIME flag and player will be able to skip the background with escape, space or action (ctrl, really) key.
Note: in background image, when you use this flag, you should add a text to inform player about this chance, pasting in the image a text like "Escape to skip" or other with wished command.

## BKGDT_BINOCULAR
Used into Customize=CUST_BACKGROUND command
Replace the old binocular mask with the image supplied in the Image command linked with the CUST_BACKGROUND script command.
Since the image has to be in full screen (other settings will be ignored) you have to use transparent settings for the image.

## BKGDT_INVENTORY
Used into Customize=CUST_BACKGROUND command
Used to set the image for inventory screen and all intermediate screen in game: like pause menu, options screen, statistic screen, load game and save game screens.

## BKGDT_LASER_SIGHT
Used into Customize=CUST_BACKGROUND command
Replace the mask of laser-sight with the Image command linked with the CUST_BACKGROUND script command.
Since the image has to be in full screen (other settings will be ignored) you have to use transparent settings for the image.

## BKGDT_LOADING_LEVEL
Used into Customize=CUST_BACKGROUND command
Used to set an image for loading level phase.
Please not this is not the load game screen where you see the savegames to load, but it is the moment when there is the progress bar and next is loading.

## BKGDT_TITLE
Used into Customize=CUST_BACKGROUND command
NOT YET WORKING. DO NOT USE!
Used to change the title flycamera with a fixed image.
This setting will affect also all other sub-screen that the user can reach from title: new game, load game and options.
Note: the setting doesn't stop the flycamera but the image will cover it. If you wish you can having an image with some transparent zones from those zones it will be showed the below fly camera.

## BUGF_DART_NO_POISON_LARA
Used with Customize=CUST_FIX_BUGS command
In default Tomb4 there was a bug about poisoning with darts.
While the poison of scorpion and harpies worked correctly with the screen deformation and lara losing HP, the poison of dart remained only an instant and it was immediatly removed.

## BUGF_LAND_WATER_SFX_ENEMIES
Used with Customize=CUST_FIX_BUGS command
In default tomb4 there was a bug about management of sfx sound in animation commands for some enemies.
There was no right handling for water/land with sound effect played as animcommands.
This bug was not present with all enemies, for instance the land/water sounds worked fine with skeleton or baddy1 or 2, but it didn't work with guide or big scorpion.

## BUGF_TRANSPARENT_WHITE_ON_FOG
Used with Customize=CUST_FIX_BUGS command
Used to fix the bug in water textures when there is the distance fog enabled in the level. The transparent texture will become full white.

When you enabled this fix the water textures will ignore the fog density.
You should use it when your level has a depth fog, anyway this fix could create some trouble with fog bulbs closed to water.

## CDM_NO_SAVE
Used by Customize=CUST_CD_SINGLE_PLAYBACK.
Using this setting the further single-playback cd will be ignored in saving game and when player will reload that savegame the single cd will be not played.

## CDM_RESTORE_FROM_BEGIN
Used by Customize=CUST_CD_SINGLE_PLAYBACK
This settin force trng engine to save the single-playback cd and restore it but always from begin and not from the effective position it had while the game had been saved.
This setting it's adivsable for little and meaningful audio track, like a sound where lara was speaking.

## CDM_RESTORE_FROM_MIDDLE
Used by Customize=CUST_CD_SINGLE_PLAYBACK.
This is the default behavior. If you don't set any CUST_CD_SINGLE_PLAYBACK customize in your script.dat the TRNG engine will work using the CDM_RESTORE_FROM_MIDDLE.
With this setting the sound will be saved remembering its current position and it will be restore playing it from that precise position.

## CL_BLINKING_WHITE
## CL_BLUE
## CL_DARKMETAL
## CL_GOLD
## CL_METAL
## CL_RED
## CL_WHITE
## CL_YELLOW
## CODE_ACTION
Used in Plugin= command.
In DisableFeatureArray of Plugin command you can disabling the changes that, given plugin, did about a standard trng action trigger.
For instance if you don't agree the changes about action 41 you can type in DisableFeatureArray the value:

CODE_ACTION + 41

## CODE_CONDITION
Used in Plugin= command.
In DisableFeatureArray of Plugin command you can disabling the changes that, given plugin, did about a standard trng condition trigger.
For instance if don't agree the changes about condition 23 you can type in DisableFeatureArray the value:

CODE_CONDITION + 23

## CODE_FLIPEFFECT
Used in Plugin= command.
In DisableFeatureArray of Plugin command you can disabling the changes that, given plugin, did about a standard trng flipeffect.
For instance if you don't agree the changes about flipeffect 132 you can type in DisableFeatureArray the value:

CODE_FLIPEFFECT + 132

And trng will forbid to given plugin to change code of flipeffect 132

## COLL_ANIMATINGS
Used with Customize=CUST_SET_STILL_COLLISION command.
It enables the still collision when Lara touches an ANIMATING slot item.

## COLL_DOORS
Used with Customize=CUST_SET_STILL_COLLISION command.
It enables the still collision when Lara touches a door.
Remark: this doesn't work with underwater doors or trap-doors.

## COLL_FAKE_WALLS
Used with Customize=CUST_SET_STILL_COLLISION command.
This flag enable the still collision on some moveables used in game to simulate walls, ceiling or floor:

SMASHABLE_BIKE_WALL, SMASHABLE_BIKE_FLOOR, FALLING_CEILING, FALLING_BLOCK, FALLING_BLOCK2, BURNING_FLOOR, ONEBLOCK_PLATFORM, TWOBLOCK_PLATFORM, RAISING_BLOCK1, RAISING_BLOCK1, EXPANDING_PLATFORM, BRIDGE_FLAT, BRIDGE_TILT1, BRIDGE_TILT2 and all PUSHABLE OBJECTS.

Remark: about some moveable used as "floor" or "ceiling", the still collision will be applied only in horizontal movement, for example when lara splats her face vs the edge of a bridge.

## COLL_FAST_TURNING
Used with Customize=CUST_SET_STILL_COLLISION command.
When there are still collision enabled, and Lara touches an item with an acute angle, she will be not  stopped but only turned to continue her race in correct direction to avoid the obstacle.
Using the COLL_FAST_TURNING flag you can increase the rotation speed in above situation.

Remark: the fast turning could be suggested when you place static items with an intermediate facing (non-hortogonal facing) with 45 degrees respect room's walls.

## COLL_NO_SLIDING
Used with Customize=CUST_SET_STILL_COLLISION command.
This setting set a very drastic collision mode: removing all sliding features lara will be always stopped, indifferently about current angle of impact with the item.

## COLL_NO_SPLAT
Used with Customize=CUST_SET_STILL_COLLISION command.
Since the still collision tries to emulate the normal collision of Lara with the walls, when Lara touches an item with correct height the "splat" animation will be performed, like it happens also with the walls. Anyway, if you wish disable the splat and having only an immediate frozen of Lara you can add the COLL_NO_SPLAT flag.

## COLL_PANELS
Used with Customize=CUST_SET_STILL_COLLISION command.
It enables the still collision when Lara touches some (moveable) PANEL item.

Remarks:

1) The PANEL_... items have been added with the TRNG engine. They are some invisible moveables used only to set specific collision in some side of the rooms.
They have slot enclosed in the range 472 - 479 and names like PANEL_BORDER, PANEL_CORNER ect.

2) The still collisions have some problems when it is necessary decide about sliding/stopping with an item having a diagonal facing (with 45 degrees angle).
These troubles are given by the difficulty to value correctly the diagonal shape of the item, anyway using the specific items: PANEL_DIAGONAL or PANEL_MIDDLE_CORNER, you can enjoy of a specific code to manage in correct way at least these diagonal items. Therefore, if you have troubles with some diagonal item (when lara continues to move legs/arms) you could try to solve the problem in this way:
- Disable the collision for this item
- Place in same position a PANEL_DIAGONAL item to cover same collision position

## COLL_STATICS
Used with Customize=CUST_SET_STILL_COLLISION command.
It enables the still collision when lara touches a static item.

## COLL_STOP_ON_45_DEGREES
Used with Customize=CUST_SET_STILL_COLLISION command.
Since the still collision method tries to simulate same behavior of wall collision, also in still collision, in some circustances, lara will be turned to orient her in correct dirction.

The decision about when stopping lara and when moving and turning her will be taken in according with current angle of Lara's direction with the item surface.
For example if lara hits the item with an angle of 90 degrees, lara will be surely always stopped.

Differently, when the angle is acute, for example only 30 degrees, lara will be NOT stopped but she will continue her race and the engine will turn her to follow the shape of the item.
The angle limit for these two behaviors is about of 45 degrees.
In default mode with 45 degress lara will be turned and not stopped, anyway if you wish you can stop lara also with 45 degrees adding the COLL_STOP_ON_45_DEGREES flag.

## COLL_VEHICLES
Used with Customize=CUST_SET_STILL_COLLISION command.
It enables the still collision when Lara touches some (earther) vehicle like jeep or sidecar.

Remark: This settings has no effect on water vehicles like boats.

## COLTYPE_SET_COLOR
Used in Parameters=PARAM_COLOR_ITEM. Using this coltype you change simply the color of item using the first color pointed by Index1ColorRGB field.

## COLTYPE_SET_PULSE
Used in Parameters=PARAM_COLOR_ITEM. The pulse use only first color, typed in Index1ColorRGB field. The engine will light up and down the color Index1ColorRGB. If you use this coltype it's adivsable to set as base color a not too light color since the engine will increase the lightning (and then reducing). For example using the (bad choice) white as color you'll have no result because it has already max lightning.
You can set the speed of pulse choosing a value in SpeedChange field.

## COLTYPE_SHADE_COLORS
Used in Parameters=PARAM_COLOR_ITEM. The shade colors shade from first color (Index1ColorRGB) to second color (Index2ColorRGB) and vice versa.
You can set the speed of shading typing a value in SpeedChange field.

## CUST_ADD_DEATH_ANIMATION
Permits to add a dying animation for some semigod or other immortal creature.

Syntax: Customize=CUST_ADD_DEATH_ANIMATION, SlotOfCreature, AnimNumber

This command is useful above all to kill the immortal creatures changed with Enemy script comamnd using the NEF_SET_AS_MORTAL field.

 You should use Enemy command to set the NEF_SET_AS_MORTAL  flag for this immortal.
Then you should also use CUST_ADD_DEATH_ANIMATION to set a new dead animation, where, with animation editor, you will have added a [Die] anim command.
For example to kill setha you could use following two rows in script.txt:

Enemy=		SETHA, 30, NEF_SET_AS_MORTAL+NEF_HIT_BLOOD, IGNORE, 0, IGNORE,IGNORE,IGNORE
Customize=	CUST_ADD_DEATH_ANIMATION, SETHA, 17

Where, the "30" in Enemy command is the starting vitality (HP) of Seth (very low but it's only an example) and the "17" in Customize command is the animation of seth we'll use to kill him when his vitality reaches zero.
In the wad of current level, I added to 17th animation of seth a [Die] anim command.
Remark: it should be better if you add really a new animation slot in seth as death animation, instead using an already present animation.
You could copy an existing animation and then add it as new animation, then you add a [Die] animcommand and you use this new (duplicated) animation (with [die] command) as new death animation.

Differently, about usage of CUST_ADD_DEATH_ANIMATION with creature already owning a death animation this customize should be no useful since if you want change the death animation just you perform this operation using Animation Editor program.

SlotOfCreature field
--------------------
In this field you set the number of slot (or its name) of creature to customize.
Remark: in the reality only some slot type will have some effect. You cann't use a slot of creature that has already a dying animation or a moveable that is not a creature.
Pratically the only slots will be elaborated correctly from this customize are:

SKELETON
GUIDE
VON_CROY
SETHA
MUMMY
SPHINX
MUTANT
DEMIGOD1
DEMIGOD2
DEMIGOD3
HAMMERHEAD
AHMET
LARA_DOUBLE
SENTRY_GUN

Remark: about above list, I've not had the time to verify it this customize works fine for each creature of above list, and neither to verify if it works on other creatures non present in the list.
Thorically you can try to use this customize on every creature but I'm not sure about results you'll get with creatures exlcuded from the list.

AnimNumber field
----------------
AnimNumber is the number of animation to set when the creature reachs vitality = 0.
This value works like in Animation Editor, it starts from zero.

WARNING: differently by other cutstomize types, the CUST_ADD_DEATH_ANIMATION constant cann't to be used in [Title] level to extend its setting to all [Level] sections.

## CUST_AMMO
Used to customize ammo of weapons.

Syntax: Customize=CUST_AMMO, SlotOfAmmo, Ammo flags (AMMO_....), Damage, ShotsForBox, ShotsWithWeapon, Extra, IdTriggerGroupWhenHitEnemy, DamageForExplosion, Speed, Gravity, IdAddEffectToAmmo, IdTriggerGroupAtEnd

Description of fields
---------------------

SlotOfAmmo field
----------------
You have to type in this field the slot of ammo you mean customize.
You can choose one of following values:

PISTOLS_AMMO_ITEM
UZI_AMMO_ITEM
SHOTGUN_AMMO1_ITEM (normal)
SHOTGUN_AMMO2_ITEM (wideshot)
CROSSBOW_AMMO1_ITEM (normal)
CROSSBOW_AMMO2_ITEM (explosive)
CROSSBOW_AMMO3_ITEM (poisoned)
GRENADE_GUN_AMMO1_ITEM (normal)
GRENADE_GUN_AMMO2_ITEM (power)
GRENADE_GUN_AMMO3_ITEM (lightning)
SIXSHOOTER_AMMO_ITEM

Ammo flags (AMMO_....) field
----------------------------
You can add in this field one or more AMMO_ flags to set different features of current ammo.
Type IGNORE if you don't want use any AMMO_ flag.
See description of AMMO_ flags in MNEMONIC CONSTANTS list of Reference panel of NG Center program.

Damage field
------------
You can change the normal damage (to Lara) of this ammo.
If for this ammo is not foreseen a damage (like for GRENADE_GUN_AMMO3_ITEM (lightning)) the damage will be ignored.
Remark: this is a normal damage, while if this ammo in also explosive you have to set the damage for explosion in other field named DamageForExplosion (see below)

Default values:

Ammo              Default     MaxValue
-----------------------------------
Pistols           1           255
UZI               1           255
Revolver          21          255
ShotGun normal    3 (*6)    255
ShotGun Wide      3 (*5)    1000
GrenadeGun Normal 20          255
GrenadeGun Power  20          1000
CrossBow Normal   5           255
CrossBow Explos.  5           1000
-----------------------------------

Remarks:
* Some ammo have 0 damage because their damage is only for explosion, anyway you can force also a common damage for them.
* The wide shot shotgun ammo are not really more powerful like the people thinks.
The damage for them is computed like a random rain of fragments where the wide property reduces the number of fragments reaching the target. Usually this number is only 5 while for normal shotgun ammo is 6, hence the normal shotgun ammo are most powerful than wide shot ammo, the only exception is when the enemy is really very big.
If you wish render really most powerfull the wideshot shotgun ammo just you set some value for damage in this field and it will be added to compute for damage.

ShotsForBox field
-----------------
When Lara picks up an ammo box, a given number of single shots will be added in inventory for that ammo.
Using this field you can set what is the number of shots for each ammo box of this kind.

Ammo                    Default          MaxForBox
------------------------------------------------------------
PISTOLS_AMMO_ITEM       Unlimited (-2)   1000
UZI_AMMO_ITEM           30               1000
SHOTGUN_AMMO1_ITEM      6                1000
SHOTGUN_AMMO2_ITEM      6                42
CROSSBOW_AMMO1_ITEM     10               1000
CROSSBOW_AMMO2_ITEM     10               255
CROSSBOW_AMMO3_ITEM     10               255
GRENADE_GUN_AMMO1_ITEM  10               255
GRENADE_GUN_AMMO2_ITEM  4                255
GRENADE_GUN_AMMO3_ITEM  4                255
SIXSHOOTER_AMMO_ITEM    6                1000
------------------------------------------------------------

Remarks:

* If you want let unchanged this field type IGNORE

* There are technical reasons (within the orignal tomb4 code) because some ammo have different max value you can set.
Please don't pass over the showed limits otherwise you'll have crashes or bad working of TRNG engine.

* The value -1 should be mean "unlimited ammo" but really this is not possible because the -1 value is the same of IGNORE value used in NG_Center, for this reason if you want set as number of shot for box an "unlimited ammo" quantity you should type another negative number different than -1, for example -2.

ShotsWithWeapon field
---------------------
Everytime Lara picks up a weapon (not ammo  box , but a *weapon*) the tomb4 engine gives to her also some shots for that weapons. It's a present, pheraps to mean that shots are within the weapon when Lara pickups it.
Anyway you can change the number of given shots with weapon, using this field.
If you wish you can set also 0  (no shots "present") this value, and I retain more logical this choice, however you can use any value you wish.

Default values:

Given with weapon     Number Shots      MaxAllowedValue
----------------------------------------------------------------
Pistols               Unlimited (-2)    1000
Revolver              6                 255
UZI                   30                255
ShotGun               6   (normal)      42
CrossBow              10  (normal)      255
GrenadeGun            10  (normal)      255
----------------------------------------------------------------

Remarks:

* If you want let unchanged this field type IGNORE

* There are technical reasons (within the orignal tomb4 code) because some ammo have different max value you can set.
Please don't pass over the showed limits otherwise you'll have crashes or bad working of TRNG engine.

* The value -1 should be mean "unlimited ammo" but really this is not possible because the -1 value is the same of IGNORE value used in NG_Center, for this reason if you want set as number of given shot with weapon an "unlimited ammo" quantity you should type another negative number different than -1, for example -2.

Extra field
-----------
This optional field could be used in some circustance for special ammo.
Read the AMMO_ constant descriptions to discover the possible usage of this field.

IdTriggerGroupWhenHitEnemy field
--------------------------------
You can perform a TriggerGroup script command, when this ammo hit some enemy.
If you wish use this feature you have to type the ID of TriggerGroup script command.

Remarks:
* When the enemy will be reached by ammo, the game engine will perform the TriggerGroup you set and it will set as "Found item" the index of enemy hit by ammo.
If you want perform some special ACTION stored in trigger group on hit enemy, remember to add to exported trigger the flag TGROUP_USE_FOUND_ITEM_INDEX, in this way the action trigger you placed in trigger group will use the index of enemy hit by ammo, instead of moveable you had set originally in exported action trigger.
Using this method you can give special functions to this ammo.

* If you don't want perform any trigger group type IGNORE in this field.

DamageForExplosion field
-------------------------
This field used to set damage for explosive ammo, like normal and powerfull grenade ammo or for explosive crossbow ammo.
Remark: when you customize crossbow poisoned dart you can use this field to set the intensity of poison. The default value for poison is 1

Default value is 30 for all explsive ammo kinds.
Max value for damage is 1000.

Speed field
-----------
For the visible ammo (like grenades and crossbow darts) you can change the (horizontal) speed.

Default values:      Default   MaxValue
-------------------------------------------------
Grenade (all types): 128       1024
Darts (all types):   512       1024
-------------------------------------------------

Remark: you can set different speed for each grenade or dart type. This mean you can have, for example, a dart (like explosive) moving fastly than poisoned dart.

Gravity field
-------------
For the visible ammo (like grenades and crossbow darts) you can change the gravity.

Default values:      Default   MaxValue
------------------------------------------------
Grenade (all types)  3         255
Darts (all types)    0         255
------------------------------------------------

Remarks:
* The gravity is not simply a vertical (down) speed but it is an accelleration value used to incread the vertical speed.
* For darts it was not foreseen the usage of gravity, but you can use it if you wish

IdAddEffectToAmmo field
-----------------------
This setting works only for visibile ammo (grenade and dart).
If you wish you can set a special effect using a AddEffect script command, and then type in this field its id to attach that effect to this ammo everytime it will be shot.
For example if you create an AddEffect to add a blue mist wake and then type the id in this field, the ammo will have a blue mist wake.

IdTriggerGroupAtEnd field
-------------------------
This setting works only for visibile ammo (grenade and dart).
This field works in similar way of previous described field "IdTriggerGroupWhenHitEnemy" but in this case it's not important if the ammo hit or less some enemy. The TriggerGroup, of that you typed the id, will be performed non just the ammo hit anything: enemies, wall, floor, statics ect.
In this TriggerGroup the "found enemy index" will be the index of current ammo. If you place in the triggergroup some ACTION you can force to perform this action on ammo item (in final position) using the constant TGROUP_USE_FOUND_ITEM_INDEX.

## CUST_BACKGROUND
Used with Customize command

Syntax: Customize= CUST_BACKGROUND, BackGroundType (BKGDT_...), Flags (BKGDF_...), Parameter, ImageId

You can customize the background used for different game phases, using an image from pix folder.

BackGroundType (BKGDT_...) field
---------------------------------
You type in this field the type of background to customize.
See the BKGDT_ values in the reference panel of NG_Center for more infos

Flags (BKGDF_...) field
-----------------------
You can add one or more flags to modify some feature of the chosen background
If you don't wish  use flags you can type IGNORE in this field.

See the BKGDF_ flags in the reference panel of NG_Center for more infos

Parameter field
---------------
This is a field that can have different meanings in according with some flag set in Flags field.
See the BKGDF_ flags in the reference panel of NG_Center for more infos

ImageId field
-------------
You type here the id of the Image= command to use as background.
You should type an Image= command first (above) of the Customize=CUST_BACKGROUND.
It's strongly suggested to use the IF_PRELOAD flag in the Image command, to have a faster displaying in game.

Note: many fields and flags of Image command will be ignored, like those about effects, anyway, in some background types, you can set in the Image command the usage of audio track and the background will use that value as background music.

Example:

Image= 3, 4, IF_PRELOAD+IF_PLAY_AUDIO_TRACK+IF_LOOP_AUDIO_TRACK, IGNORE, 105, IGNORE, IGNORE, IGNORE, IGNORE

Customize= CUST_BACKGROUND, BKGDT_INVENTORY, IGNORE, IGNORE, 3

Above commands will set the "image4.bmp" (from "trle\pix" folder) as new background for inventory/pause/load game/save game, and it will be played the audio track 105 in loop mode.

## CUST_BAR
It allows to customize all default tomb4 bars (like air bar, hp bar,loading bar ect) setting position, size, colors and animation mode.

Syntax: Customize=CUST_BAR, BarType (BAR_...), FlagsBar (FBAR_...), XOrigin, YOrigin, XSize, YSize, IdColor1, IdColor2, Extra

BarType (BAR_...) field
-----------------------
In this field you type a BAR_ constant to specify what is the bar you are customizing.
See the BAR_ values in MNEMONIC CONSTANTS of Reference panel of NG_Center.

FlagsBar (FBAR_...) field
-------------------------
You can add two or more FBAR_ flags to specify some animation mode (the variation of color in dynamic bars)
If you don't wish to use any flag type IGNORE

XOrigin, YOrigin, XSize, YSize fields
-------------------------------------
These four fields permit to change the position and size of current bar on the screen.
Remember that you are not forced to modify really these fields, if you want preserve the original position and size of the bar just you type four IGNORE values in these fields.

All values you type in these fields are in pixel and they are computed to work with game screen of 640x480.
This method is necessary since you cann't know what it is will the effective game screen size while the game will be played (the player could change the settings in tomb4 setup).
So you compute the position and size using as reference when the game is working at 640x480 pixels, then, when the game will change its size, the trng engine will adapte in proportional way the coordinates you set.

Following are the default values:

TypeBar   OrgX   OrgY  SizeX  SizeY
-------------------------------------
HealthBar   8      8    150    12
DashBar   481      8    150    12
AirBar    481     26    150    12
LoadBar    20    444    600    15
-------------------------------------

Remark: there is some rounding in change between full screen and windowed mode, usually with gaps of 1 or 2 pixels.

IdColor1 field
--------------
In this field you have to type a Id to identify a ColorRGB= script command with rgb color to use as main color for bar.
For example if you want have a pure red you can type:
ColorRGB=3, 255,0,0
The ID is 3, so you'll type the value 3 in IdColor1 field to set the red as main color (the main color is the color of full bar).

WARNING: about the ColorRGB command you reference with IdColor it's necessary it was typed FIRST of Customize=CUST_BAR command otherwise when TRNG engine is parsing the CUST_BAR command the ColorRGB (if you type it down) will result as missing and it will be used a black color.

The default colors for main color are:
BarType   Red  Green Blue
--------------------------
HealthBar 255  0     0
DashBar   0    255   0
AirBar    0    0     255
--------------------------

IdColor2 field
--------------
This field work like IdColor1 (See above description) but to set the background color of bar
Default value in tomb4 for this color is black (0,0,0)

Extra field
-----------
This field may accept different values in according with further FBAR_ flags.
Read the description of FBAR_ constants to discover the usage of Extra field

## CUST_BIKE_VS_ENEMIES
Used with Customize command
Syntax: Customize=CUST_BIKE_VS_ENEMIES, Slot+HIT_ flag array
This customize allows to choose what will happen when the bike collide with some specific slot.
In the past, the bike killed all mortal enemies, while with those they were immortals it passed across them like they were made of air.
From 1.2.2.5 version there is a new collision management where you can choose what enemies will be killed, what pushed away, what hurted ect.

You use the HIT_.. flags to set the behavior of impact with that slot item.

For example if you wish that the SKELETON was killed you can type the command:

Customize=CUST_BIKE_VS_ENEMIES, SKELETON + HIT_KILL

you can also place two or more flags:

Customize=CUST_BIKE_VS_ENEMIES, SKELETON+HIT_KILL+HIT_PUSH_AWAY

in above case, the skeleton will be pushed away and killed in same moment

You can customize many slots in the same CUST_BIKE_VS_ENEMIES command.
example:
Customize=CUST_BIKE_VS_ENEMIES, SKELETON + HIT_KILL, KNIGHTS_TEMPLAR+HIT_PUSH_AWAY

Note: if you omit to place the CUST_BIKE_VS_ENEMIES customization the default behavior will be this:

			HIT_PUSH_AWAY, // SKELETON
			HIT_PUSH_AWAY, // GUIDE
			HIT_PUSH_AWAY, // VON_CROY
			HIT_PUSH_AWAY , //SETHA
			HIT_PUSH_AWAY, // MUMMY
			HIT_WALL, // SPHINX
			HIT_WALL, // HORSEMAN
			HIT_WALL, // SCORPION
			HIT_PUSH_AWAY, // JEAN_YVES
			HIT_WALL, // KNIGHTS_TEMPLAR
			HIT_PUSH_AWAY, // MUTANT
			HIT_WALL,  // HORSE
			HIT_PUSH_AWAY | HIT_HURT, // DEMIGOD1
			HIT_PUSH_AWAY | HIT_HURT, // DEMIGOD2
			HIT_PUSH_AWAY | HIT_HURT, // DEMIGOD
			HIT_PUSH_AWAY | HIT_HURT,  // AHMET
			HIT_WALL, // LASER_HEAD
			HIT_WALL, // LASER_HEAD_BASE
			HIT_WALL,  // LASER_HEAD_TENTACLE
			HIT_HURT, // HYDRA

While all other enemies will be killed at firt impact.

Note: you can not use the HIT_HURT for immortal enemies, but you can use HIT_KILL+HIT_EXPLODE flags and they will be killed whereby an explosion.

See all HIT_ flags in the Reference panel of NG_Center for more infos.

## CUST_BINOCULARS
Used with Customize command
Syntax: Customize=CUST_BINOCULARS, FLAGS (BINF_...), Parameter, CompassImage, CompassRectAndFlags, SextantImage, SextantRectAndFlags, LightnessRectAndFlags, ZoomRectAndFlags, LightSwitchRectAndFlags, FontID

This customize allows to set many new features to binocular management but it works only in according with the usage of Customize= CUST_BACKGROUND, BKGDT_BINOCULAR ... command.
Once you added a cust_background for binoculars, you can furtherly customize this binoculars with this, CUST_BINOCULARS, customize command.
If you omit the customized background for binoculars the cust_background will have no effect.

IMPORTANT NOTE: while when you use CUST_BACKGROUND for binoculars, alone, the binocular image for background can have any size, when you wish customize advanced features for binoculars with this CUST_BINOCULARS command, you have to use as background image for binoculars an image with size 1024 x 768.
This requirement is necessary to work on a well-known size to grant correct alignment between the background and the different values, bars or strips that will be added to it.

FLAGS (BINF_...) field
----------------------
In this field you add BINF_... flags to set the work mode of binoculars.
The most of these flags are to set what special feature to enable for binoculars.

Parameter field
---------------
In field it could be required some extra setting in accoding the some BINF_.. flag.
See description of BINF_... flags for more infos.

CompassImage field
-------------------
If you use the BINF_COMPASS flag in Flags field, and then, in the next CompassRectAndFlags field, you'll set the BINT_STRIP type, you have also to set in current CompassImage field the number of image to use for the compass strip.

Note: if you do not use BINF_COMPASS flag or you use BINT_NUMERIC type for the compass, you can type IGNORE in this field.

About the number of image to type in this field, remember that the number you type in this field is NOT an Id of some Image script command but it is own the number of image in trle\pix folder.
For example if the image with the compass strip is "image13.bmp", you'll type 13 in this field.
The compass strip image has to be exactly of 1400x64 pixels.
About its look, it's more easy, to understand how it should be drawn, if you look the example of lightning demo, where it used.
Pratically the compass strip will be a graduated scale with cardinal points (north, east, west, south) and different degrees, that will be used, moving it, to indicate where binoculars is aiming in that moment.
Note: the compass strip has a double scale because, to preserve infinite rotation, it is necessary having two instances for each cardinal point and degrees.
Look the example of demo lightning to study how it works.

CompassRectAndFlags field
-------------------------
If you enabled the compass, with the BINF_COMPASS flag, you have to type here the id of PARAM_RECT command where you stored the position and size of rect where the compass data will be drawn.
Then you can add to this rect id, a BINT_ type to set as showing the compass data. You can use only BINT_STRIP or BINT_NUMERIC types for the compass.

Remember to set origin and size of the rectangle, always using as reference a screen of 1024x768 pixel.
About the shape of the rectangle you should choose a wide rectangle (the base very wider than heigh) when you use a BINT_STRIP type for the compass.
While for the BINT_NUMERIC type, the difference between base and height should be less but you should anyway remember that in the BINT_NUMERIC type the compass data will be printed with the format like "NE 32" and therefor these characters require some space in the base of the rectangle.

Notes:

- For this field is not foreseen the usage of BINT_BAR type.
- The size of the font used to print the text, when you use the BINT_NUMERIC type, is given by the height of PARAM_RECT ractangle. For this reason, if you wish a bigger character for the compass data just you increase the height field in PARAM_RECT command.

SextantImage field
------------------
If you wish show a sextant with vertical degress about where binoculars are looking, and you  set the BINT_STRIP type about sextant data, you have to type in this field the numbe of image for the sextant strip.

About the number of image to type in this field, remember that the number you type in this field is NOT an Id of some Image script command but it is own the number of image in trle\pix folder.

The sextant strip image has to be exaclty of 64x1520 pixels.
Differently by the compass strip image, the sextant image has always a vertical shape.

Note: you have to create a sextant image of given resolution (64x1520) anyway in game you can show a littler (more slim) strip, choosing in SextantRect a rectangle with a width less than 64 pixel, in this case will be taken always the lefter strip of the original image.

About the look of this image and how it works I suggest to study the lightnings demo you find on trng official website ( http://www.trlevelmanager.eu/ng.htm ) in the demo section.

SextantRectAndFlags field
-------------------------
If you use a sextant you have to type in this fild the id of PARAM_RECT command where you store the positon and size of the ractangle that will host the sextant data.
You have also to add to the rect id the BINT_ type about how to show sextant data.
You can choose BINT_STRIP or BINT_NUMERIC types.

LightnessRectAndFlags field
---------------------------
If you enabled the data about lightness, you have to type in this field the id of PARAM_RECT command with the position and size where it will be drawn.
Remember that the binocular background has always 1024x768 size and the data of ractangle should work in according with this base resolution.

You have also to add to the rect id, the BINT_ type about how to show sextant data.
You can choose BINT_BAR or BINT_NUMERIC types.
Note that if you choose the BINT_NUMERIC, the lightness level will be described in ISO/DIN scale, with values in the range between 1 and 39, where 39 is the sunlight and 1 is the full dark.

ZoomRectAndFlags field
--------------------------
If you wish having show infos about current zoom-in factor, you can add in Flags field the BINF_ZOOM flag, and then type in this ZoomRectAndFlags field the id of PARAM_RECT where show values about current zoom factor.
Please, note that you can use for this field only BINT_NUMERIC type.

LightSwitchRectAndFlags field
-----------------------------
If you added the BINF_LIGHT_SWITCH flag to flags field, now you have to set in this LightSwitchRectAndFlags field, the id of PARAM_RECT command with the infos about the position for switch light.
The switch light is a button with two states, on and off, to show on the screen when player enabled the illuminator in binocular view.
Please note that in the PARAM_RECT you have to use the ForeColor field to set the "on" color of the button, while in the backcolor the color for "off" state.
This button is simply a box of a given (by you) size, the only change you can do it's to add the BINT_ROUND type, to convert the box in a circle.

FontID field
------------
When you let literal or numeric info showing, you have to choose the font to use for these text typing.
Note: about the size of the font, it will be arranged (stretched or enlarged) to be compatible with the height field of rectangle for the given text.

## CUST_CAMERA
You can customize many properties of Game Camera with this customize setting.

Syntax: Customize=CUST_CAMERA, Flags (FCAM_...), DistanceChaseCam, VOrientChaseCam, HOrientChaseCam, DistanceCombatCam, VOrientCombatCam, DistanceLookCam, HeightLookCam, SpeedCamera

Remark: you can customize different automatic camera modes with this customize: the "chase" camera, that is the camera that always follows lara, the "combat" camera, that works like the "chase" camera but it will be engaded when lara extracts the weapons, and the "look" camera that is when the player enables lara looks byself choosing the direction. The combat has some settings a bit different respect than "chase" camera.

Flags (FCAM_...) field
----------------------
You can type in this field one or more FCAM_ flags to change some behavior of Camera.
See the list of FCAM_ values in  MNEMONIC CONSTANTS list of Reference panel of NG_Center program.
Remark: you can type IGNORE in this field if you don't wish set any flag.

DistanceChaseCam field
----------------------
This value works for "chase" camera or "follow me" we could say.
This field is the distance (behind lara) where is the camera.
The default value is  1536 ($600), where 1024 ($400) is one game sector.
If you reduce this value the camera will be closest to lara and all level (and lara) will seem more near. If you increase this value the distance from lara will be larger and lara will appear littler.

Remark: if you type a negative value in this field, the camera will be in front of lara instead of at her back.
It's interesting note as the HOrientChaseCam field with $8000 value and the DistanceChaseCam with a negative value get the same target but only using only one of them in same time. In fact, if you set $8000 as HOrient see description below) and also a negative Distance you'll get newly the usual camera following lara from back.

You can type IGNORE in this field if you don't wish change this value

VOrientChaseCam field
--------------------
This field works for "chase" camera, and it is the vertical difference of orientation (facing) of camera respect to lara.
To understand what it "vertical orientation" try to look the tutorial for animation and testposition commands. In that help file there is an image showing own the Vertical Orientation.
The default value is -1820 and, since it is a difference (like degrees difference respect to a line parallel to floor) this means the cam is a bit higher than lara.
If we choose +1820 the camera should be a bit lower than lara.
If we type -16384 the camera should exactly overt the top of lara (90 degrees respect to line parallel to floor passing for lara)
If we type +16384 the camera should below lara with 90 degrees angle, but this setting could work only when lara is monkey or she is falling down.
If we type 0 (zero) the camera will be exactly with same Y origin of lara, because the camera and lara should be on same line parallel to floor.

Remark: some settings in this field could be ignored when the value is so much big ( in absolute value) to cause to show lara upside down. For example the value -32767 should get a camera looking lara in the face but with lara appears upside down. In the reality, using this value (-32767) you'll get the same result typing -16384 and this happens because the game engine uses a cut-off function to forbid to show lara upside down. For above reason the valid range for this field is from -16384 to +16384

You can type IGNORE in this field if you don't want change it.

HOrientChaseCam field
---------------------
This field is a bit complicated to explain.
It works like HOrientDifference of TestPosition command.
The orientantion is also named "facing" i.e. where an object is looking.
This value changes the facing of Source camera. The values used could be: 0 (default, the cam looks lara from back), $4000 (the cam looks lara from her left), $c000 (the cam looks lara from her right), $8000 (the cam looks lara in front).

You can type IGNORE to let the default value.

DistanceCombatCam field
-----------------------
This field set the distance when lara is in modality combat camera.
The description about the concept of "cam distance" is the same you can read for above field about DistanceChaseCam field.

Default value was $600

VOrientCombatCam field
---------------------
This field works like the VOrientChaseCam field.
The default value is -2730

See description of VOrientChaseCam field to understand how it works.

Remark: There is no "HOrientCombatCam" field for Combat camera because in "combat camera" mode the horient will be always set by engine in according with position of selected enemy and lara, for this reason no good result could be get forcing the HOrient in combat camera mode.

DistanceLookCam field
---------------------
This field works like other "Distance ..." fields but with an opposite sign.
For example, the defult value for DistanceLookCam is -1024 and it means that the camera is exactly one sector behind lara.
If you used +1204 the look camera will be forward than lara and lara should be not showed in game screen.

HeightLookCam field
-------------------
This field don't work like VOrient field in this case you type an Y displacment from lara's neck

The default value is +16, and it means that the Y position of camera it's a bit lower the center of the lara's neck.

Remark: I discourage to change this value, using little values you cann't notice the difference while using big values the results are not good.

SpeedCamera field
-----------------
The speed of camera set the time required to move camera from current (any) position to ideal position set by above field.
If you set a high speed the camera will move fastly to reach ideal position but the disadvantage could be an jerkily movement.
With low speed the camera will have a moving but it could remain back to lara when she move fastly.

You can type IGNORE in this field to let the default value.

Remark: I discourage to modify this value because the default setting is an ideal value and, more, the speedcamera changes in many circustances everytime the engine requires to pass from a camera mode to another.

## CUST_CD_SINGLE_PLAYBACK
Permits to customize the management of Cd audio track performed for a single playback.

Syntax: Customize=CUST_CD_SINGLE_PLAYBACK, CdMode value (CDM_...)

This customize work only about saving/reloading of single-playback audio tracks in progress while the player saved the game.
You can see the different choice looking for CDM_.. constant descriptions.

## CUST_DARTS
Used by Customize= command.

Syntax: Customize=CUST_DARTS, IdCustDarts, Dart Flags (DRT_ ...), IdAddEffect, Speed, EmittingTimer, PrimaryColorIDRgb, SecondaryColorIDRgb, IdTriggerGroup

With this customize you can change speed and color of darts. Someone believes the darts are not visible but in the reality they are not very well visible only because they are too fast. Just you reduce the speed and change the color to see them very well.
With this customize you can also restyle fully the dart: using some add effect or using them like a laser sensory ray.

IdCustDarts field
-----------------
Differently by other customizations you can set two or more customize profile for dards and then enable in same time one or another typing in OCB field of dart emitter object the IdCustDarts value to locate the customization to use.
For example if you create a command like:

Customize=CUST_DARTS, 3, ....

When you want customize in NGLE a dart emitter with these settings just you type in OCB field of this emitter the number "3"

Remark: the IdCustDarts value should be always greather than 0 becuae the 0 in OCB field will be read with the mean of: "no customize for this dart emitter"

Dart Flags (DRT_ ...) field
---------------------------
You can add one or more DRT_ flags to enable different features for darts.
Read the description of DRT_ constants in Reference panel of NG_Center program.
If you don't wish use any flag you can type IGNORE in this field.

IdAddEffect field
-----------------
If you use the DRT_ADD_EFFECT flag, you have to type in IdAddEffect field the ID of AddEffect script command to add to each dart.

Speed field
-----------
The speed value set the value to add to coordinates of dart to move it in the space.
Bigger values mean bigger speed.
The default value is 256 ($100) and this is a very fast speed.
So probably if you wish change it, you'll decrease it to have the chance to see the darts.
You can type IGNORE in this field to let unchanged the speed.

EmittingTimer field
-------------------
This value is in tick frames, where one second = 30 tick frames.
The default value is 24 tick frames, and this means the engine wait 24 tick frames (a bit less than one second) before shooting another dart.
If you increase this value there will be more time between a shooting and following.
You can type IGNORE in this field to let unchanged this value.

PrimaryColorIDRgb field
-----------------------
If you want change the main color of dart you can type in this field the ID of some ColorRGB= command with the rgb values to use.
The default color is like cream.
You can type IGNORE in this field to preserve standard color.

SecondaryColorIDRgb field
-------------------------
If you want change the secondary color of dart you can type in this field the ID of some ColorRGB= command with the rgb values to use.
The secondary color is black and it used for the borders. It is a good color for border anyway if you use this dart in very dark room you could use the white for border to give the idea of brightness of the darts.
You can type IGNORE in this field to preserve standard color.

IdTriggerGroup field
--------------------
If you use the DRT_PERFOM_TRIGGERGROUP flag you have to type in IdTriggerGroup field the ID of triggerGroup to perform when a dart touches Lara.
See description of DRT_PERFOM_TRIGGERGROUP flag for more informations.

## CUST_DISABLE_FORCING_ANIM_96
Used by Customize= command

Syntax: Customize=CUST_DISABLE_FORCING_ANIM_96

By default, when lara is moving left or right in hang mode and reach a corner the engine force the animation 96.
This behavior is good of course, but if you are writting new hanging animation that forcing of animation 96 could create troubles to your new hanging management.
Using this cust value the engine will not force anim 96, allowing to use custom animation to round the corner

## CUST_DISABLE_MISSING_SOUNDS
used in Customize command.

Syntax: Customize=CUST_DISABLE_MISSING_SOUNDS

This setting, disable in old tomb4 code the playing of obsolete sound effects like the sound for bubbles (37 - LARA_BUBBLES) and that for pickup item (62 - LARA_PICKUP).
You should use this setting when you use Boats or other new object that uses some of above old sounds. In fact, in old example about boats on Trng website, there was a boring bad sound when lara swims underwater, and also when she picked up some item. The reason was the new boat object required to use some new sounds but, since there was way to add new sound slot, I tried to use unused sound effect of old tomb4. Unfortunately, also these (missing) sounds had a code in tomb4 to play them, so when I added the sounds for boats, the tomb4 engine played some of these sounds also as bubbles or pickup.
Setting CUST_DISABLE_MISSING_SOUNDS, the old codes for bubbles and pickup will no more play sound s62 and 37, but new objects like boats or other, will be able to play the new sounds assigned for above sound slots.

## CUST_DISABLE_PUSH_AWAY_ANIMATION
Used into Customize=CUST_BACKGROUND command
This customize disable forever, in current level, the push-away animation of lara, when she has been touched and moved from enemies.
The push-away animations are the 125, 126, 127 and 128 animations in lara slot.

## CUST_DISABLE_SCREAMING_HEAD
The CUST_DISABLE_SCREAMING_HEAD disable the change of normal Lara head (from Lara Skin slot) with screaming head when she is shooting (from Lara Scream slot).

Syntax: Customize=CUST_DISABLE_SCREAMING_HEAD

The reason to disable the change of heads could be when you perform a standard swap mesh for lara but the screaming head remain untouched. In this circustance when player will shoot the new lara Head will be replaced with the old screaming head of Lara ruining the new mesh coerence.
Differently if you set this CUST value, Lara will have always a single head avoiding above problem after a swap mesh operation.
Remark: another way to solve the problem is to use an advanced Swap mesh, replacing also the mesh in Lara Scream slot, using a swap mesh where is present also "Shooting Head".

## CUST_ESCAPE_FLY_CAMERA
Used to allow at player the break of some flyby camera sequence hitting the ESCape key.

Syntax: Customize=CUST_ESCAPE_FLY_CAMERA, ENABLED/DISABLED, KeyBoardScanCode

If you want allow the break of flyby sequence you have to set as ENABLED this customize.
About KeyBoardScancode value it will be the code of keystroke to use to exit from flyby sequence. You find the list of scan codes in "Reference" panel of NG_Center, in the section named "KEYBOARD SCANCODES list".

If you type IGNORE in this field the default value will be 1 (ESCAPE key)

Remark: if you want permit to skip the flyby cameras it's advisable set some print text in your level to inform the player about this new chance, otherwise nobody will try to hit escape during flyby sequences.

## CUST_FIX_BUGS
Used with Customize= command
Syntax: Customize=CUST_FIX_BUGS, BugsToFix flags (BUGF_...)

This customize accepts one or more BUGF_... flag to fix some bug in tomb raider engine.
The reason to let customizable the bug fixing (instead by fixing always byself) is because some level builders could have built some skill own based on some bug of old engine, other the chance that a bug fixing in some critical module of tomb raider could generate some unwished collateral effect.

Note: in BUGF_ flags you'll find some bug fixing already seen with other mnemonic constant in other script commands, like CUST_FIX_WATER_FOG_BUG  or DRT_FIX_POISON_BUG.
You can fix above bugs in both ways: with old (above) constants or with the new BUGF_ constants.
Anyway in the future all bugs to fix will be set only as BUGF_ constants to find them more easily in a single command.

See the BUGF_ constant in Reference panel of NG Center for more infos.

## CUST_FIX_WATER_FOG_BUG
Used to fix the bug in water textures when there is the distance fog enabled in the level.

Syntax: Customize=CUST_FIX_WATER_FOG_BUG, ENABLED/DISABLED

When you enabled this fix the water textures will be ignore the fog density.
You should use it when your level has a depth fog, anyway this fix could create some trouble with fog bulbs closed to water.

## CUST_FLARE
Used in Customize= command.
Syntax: Customize=CUST_FLARE, Flags for Flare (FFL_....), SecondsOfLifeTime, Red, Green, Blue, Intensity

Flags for Flare (FFL_....) field
---------------------------------
You can add one or more FFL_ flags to enable special features about flare.
You can type IGNORE in this field you are not interested to set flags.
See description of FFL_ constants in [Reference] panel of NG_Center program.

SecondsOfLifeTime field
-----------------------
If you wish override the durate of flare you can type in this field a value in seconds for life-time of lightning flare.
The default value was 30 seconds.
You can type IGNORE in this field to preserve the default value.

Red, Green, Blue fields
-----------------------
In these three field you can set the middle color of flare light.
Remark: this color will be not applied in first and last phase of the life-time of flare: the switching on and the switching off phases.
You can type IGNORE in these three field to preserve their default values (red = 128 , Green = 192, Blue=0)

Intensity field
---------------
You can change the middle light intensity of flare.
The default value was 16.
You can type ignore in this field to preserve default value.

## CUST_FMV_CUTSCENE
Used to enable to customize the viewing of FMVs (cutscenes base on videos) in

Syntax: Customize=CUST_FMV_CUTSCENE, FMV_ flags

You can type one or more FMV_... flags to set some features about the showing of videos (FMVS) in game.
See the description of FMV_... constants in [Reference] panel of NG_Center for more infos.

## CUST_HAIR_TYPE
Used to force the hair of lara in game.

Syntax: Customize=CUST_HAIR_TYPE, HairType  (HAIR_...)

You choose the kind of hair setting an HAIR_  constant value.
You find all HAIR_ constants and their description in Reference panel of NgCenter in MNEMONIC_CONSTANTS list.
Remark: This command is not able to change byself the mesh of hair but simply it used to inform the engine about what kind of hair object you have copied in your wad file.
For example you can use a young lara (get from angkor wat) but don't set in script.dat the command YoungLara=ENABLED. In this way you'll be able to use young lara with correct hair type (using CUST_HAIR_TYPE)and keeping, in same time, also the weapons like for adult lara, because the engine will consider lara as adult lara since you have not type YoungLara=ENABLED.
Pratically, this customize value (CUST_HAIR_TYPE) is useful to divide the setting about hair from that about weapons. If you use old command YoungLara you cann't distinguish setting about hair from setting for weapons, or you choose "two plaits" of your lara but then you renounce also the weapons, otherwise, you choose adult lara and you have the weapons but you'll have always the single ponytail look.
Differently, using CUST_HAIR_TYPE  you can use YoungLara only to set or less the weapons, while you'll use Customize=CUST_HAIR_TYPE  to set the hair style.

## CUST_HARPOON
Used in Customize command.
Syntax: Customize=CUST_HARPOON, HarpoonFlags (HRP_...), TopBorder,DistanceFromCam,Orient_X, Orient_Y, Orient_Z, HarpoonSpeed, HarpoonGravity

This customize enable the change of Crossbow to get an Harpoon weapon like tomb raider 3 adventure.
To use this customize it's necessary also you copy in your wad file the
CROSSBOW_ANIM slot you find in harpoon.wad (you can find it in http://www.trlevelmanager.eu/ng.htm in Source Sample section).
That slot has been changed with the original HARPOON_ANIM get from Tomb Raider 3 level.

HarpoonFlags (HRP_...) field
----------------------------
You can add one or more HRP_ flags to enable specific settings of new Harpoon gun.

TopBorder field
---------------
Adjust the position of harpoon moving up or down in 2d view.
If you want move  up the harpoon decrease this number, while if you want move down the harpoon increase this number
You can type IGNORE in this field and it will be let the default value =0.

DistanceFromCam field
----------------------
Distance from (virtual) cam
This argument  is  useful to increase or reduce the size of object in inventory.
If you increase the distance the object will become more little
If you decrease the distance the object will become more big.
This value is in game units (1 square = 1024 units)

You can type IGNORE in this field to let the default value: $400

Orient_X, Orient_Y, Orient_Z fields
-----------------------------------
These 3 fields set the orientation of cam on X, Y and Z axis.
It's complicated to explain but if you try to change this value you'll understand what I mean.
The values for this argument could be go from $0000 to $FFFF but usually will be used only four values:

$0000 = North (top view)
$4000 = East (right view)
$8000 = South (bottom view)
$C000 = West (left view)

You can type IGNORE in these fields and it will be used the default values:
Orient_X = $B000
Orient_Y = $C000
Orient_Z = $C000

HarpoonSpeed field
------------------
You can change the speed of harpoon.
If you type IGNORE in this field, it will be used the default value, that it is different in according with current inclination respect to the ground.
Reasonable values for this field are enclosed in the interval 100 / 300.
The spees will be different when the harpoon travels in air or in water.
The speed in water will be a 1/4 less than value used for air.
For example if you type 200 in this field, the spees in air will be 200, while underwater it will be 150

HarpoonGravity field
--------------------
The gravity is an increent to move the harpoon down, to simulate the gravity.
The value you type will be used in different way in according if the harpoon is moving in the air on in the water.
When the harpoon is in the air it will be used the original HarpoonGravity value.
While when the harpoon is travelling in the water the gravity will be divided by 2 before using it.
For example if you type in this field the value 16, the gravity in hair will be 16, while in the water it will be 8.
If you type 0 in this field the harpoon will be no affected by gravity force.
You can type IGNORE in this field to let the default value (24).

## CUST_INNER_SCREENSHOT
Used to enable the saving of game image in all savegames
Syntax: Customize=CUST_INNER_SCREENSHOT, QualityScreenshotFlags (QSF_...)
When you use CUST_INNER_SCREENSHOT in a level section, in all savegames will be saved also a little image of current game image.
This option will be used by TRLM2009 to see image corresponding at all savegames, and also for new savegame panel to set the quality of inner images.

QualityScreenshot (QSF_...) field
--------------------------------
You can set  in this field two QSF_ flags. One QSF_SIZE_... flag and (optional) a QSF_TRUE_COLOR  flag.
About the settings to use for inner screenshots you should decide if you want a good quality but with also a big size of savegame, or a low quality with a little savegame.
See description of QSF_ flags in Reference panel for more infos.

## CUST_KEEP_DEAD_ENEMIES
By default when an enemy dies, his body disappear. If you wish you can keep his body forever, like in previous tomb raider adventures.

Syntax: Customize=CUST_KEEP_DEAD_ENEMIES, ENABLED/DISABLED

Remark: The bodies killed whereby explosion will be NOT preserved for obvious reasons.

## CUST_KEEP_LARA_HP
Used with Customize command.

Syntax: Customize=CUST_KEEP_LARA_HP, TargetLevel

With this customize you can avoid the automatic recharging of lara HP when she jumps to another level from the current.

TargetLevel field
-----------------
You type in this field the index of the target level where lara will arrive keeping previous HP value.
Example:
We suppose that your adventure has three levels, where the level 1 and the level  2 are the two halfs of a wide environment and lara jumps on and back from these two levels many times.
Then there is the level 3 where lara will go only after solved all puzzles of first two levels.
In this situation you could wish that lara was not able to recharge her HP simply going on and back from the gate between level 1 and level 2.
So to keep same hp in above situation you should place in [Level] section of level 1 the command:

Customize=CUST_KEEP_LARA_HP, 2

While the in the [Level] section of level 2:

Customize=CUST_KEEP_LARA_HP, 1

About level 3 is not necessary type anything since when this command is missing by default lara will be recharged when she jumps to level 3

Remark: if you wish keep always the HP of lara, i.e. no recharge for all levels, you can use the special constants KLH_ALL_LEVELS instead of the target level:

Customize=CUST_KEEP_LARA_HP, KLH_ALL_LEVELS

In this way, for any level where lara jumps, starting from current, she will keep the old HP value.

## CUST_LIGHT_OBJECT
Used in Customize command.
Syntax: Customize=CUST_LIGHT_OBJECT 34, SlotLight, Red, Green, Blue, Intensity, Time
With this customize you can modifiy colors and light intensity of light objects:
AMBER_LIGHT
WHITE_LIGHT
BLINKING_LIGHT
You can change the main color of these objects, their light intensity and in some circustance the blinking time or other timing parameter.

SlotLight field
---------------
In this field you can type the slot name or number of light object to customize.
Currently the only light objects allowed are: AMBER_LIGHT, WHITE_LIGHT, BLINKING_LIGHT

Red, Green, Blue fields
-----------------------
Type in these three fields the intensity of red, green and blue.
Each color intensity works in the range 0 - 255, where 255 is higest intensity and 0 is null.

Intensity field
---------------
This field set the brightness and widness of the light.
If you type IGNORE in this field it will be used the default value 18.

Time field
----------
All lights have some blinking and this field permits to change the time of blinking.
These are default values of Time for the different light objects:

AMBER_LIGHT = -2048
WHITE_LIGHT = 160
BLINKING_LIGHT = 30

## CUST_LOOK_TRASPARENT
Permit to disable the transparency of lara while she is looking.

Syntax: Customize=CUST_LOOK_TRASPARENT, ENABLED/DISABLED

If you want remove transparency for Lara use DISABLED

## CUST_NEW_SOUND_ENGINE
Syntax: Customize=CUST_NEW_SOUND_ENGINE, NewSoundEngine flags (NSE_...), SoundExtension (SEXT_...), LongFadeOut, ShortFadeOut

You can customize some features of new sound engine based on bass.dll created by Un4seen Developments Ltd and supported by TRNG engine.
Remember that the new sound engine will be enabled at least you don't disable it explicity using script command:
NewSoundEngine= DISABLED
Field description for CUST_NEW_SOUND_ENGINE:
NewSoundEngine flags (NSE_...) field
---------------------------------------
You can change some features of new sound engine adding one or more NSE_ flags.
See description for specific NSE_ values in _MNEMONIC CONSTANTS list of Reference Panel of Ng Center program.
Remark: currently there are no NSE_ values available, but some new NSE_ value will be added in future. Now you can type IGNORE in this field

SoundExtension (SEXT_...) field
-------------------------------
This field is now obsolete and typing some value in this field will have no effect.
In trng previous than 1.2.0.2 in this field was possible choose the extension of audio track (.wav, .mp3, .ogg) but now trng uses an automatic method to detect the audio tracks to play.
Now trng checks for each track number (the first three digits "020" ...) what extension is really available.
If there is a single file with that track number this audio track will be played.
When there are two or more audio files for same track number, trng will play the first found file in following extension sequence: ".ogg", ".mp3", ".wav" ... all other supported audio format from bass.dll
For example if in audio folder there are the following files:

034.wav
034_Interlude.mp3

159_FlyBy.mp3
159_TopView.ogg

005.wav

Trng will play the audio files:

034_Interlude.mp3
159_TopView.ogg
005.wav

This happens because the priority is for ".ogg" files, if .ogg file is missing it will be searched a .mp3 file, while if it's missing either .ogg and .mp3 extension, will be played the .wav file or other audio extensions (.mp2, .aiff ecc.)

Remark: when there are two audio tracks with same number and same .wav extension, trng will play the long name audio track.
For example:

If in audio folder there are following files:

005.wav
005_Secret.wav

Trng will play the file "005_Secret.wav"

LongFadeOut field
-------------------
For "Fade out" we mean a gradual reduction of volume upto reach the null volume.
The fade out used to stop a sound in sweet way.
The value you type in this field is in microsecond units and it should be the time require to pass from current volume to zero volume.
The "long" fade out it is used when a sound will be stopped for some change in game like loading of new level.
The default value for long fade out is 1000 ms, i.e. one second.
If you don't want change this value you can type IGNORE in this field.

ShortFadeOut field
-------------------
About the mean of "Fade out" read the above description for LongFadeOut  field.
The "short" fade out it's used when a CD sound should be stopped to permit to other CD sound to start in same channel.
In this situation the fade out should be shorter and in fact the default value is of 300 ms.
If you don't want change this value you can type IGNORE in this field.

Remark: This customize command, like all other customize commands, works both for all levels, if you place it in [Title] section, that for single specific levels, if you place many instances of it, one for each [Level] section.
You can do this, to place many Customize=CUST_NEW_SOUND_ENGINE in different levels, anyway it's better to do not change the default extension for audio files otherwise you could have some trouble. Differently, you can change the values for fadeout, level for level.
About the chance to use different NSE_ flags in different level, you should read the specific description of any specific NSE_ flags.

## CUST_NO_TIME_IN_SAVELIST
This setting permits to disable the text about game time in savegame list screen.

Syntax: Customize=CUST_NO_TIME_IN_SAVELIST, ENABLED/DISABLED

If you want remove the time game info type:
Customize=CUST_NO_TIME_IN_SAVELIST, ENABLED

This option is useful only when you set a font text very big (in width size), in fact, when the font is too big the level name could result overlapped to time infos with a very nasty effect.
If you remove the time info you'll be able to use more wider characters

## CUST_PARALLEL_BARS
You can customize the PARALLEL_BARS item in current level.

Syntax: Customize=CUST_PARALLEL_BARS, FlagsParallelBar (PB_...), SpeedForSlide, MaxTurns

FlagsParallelBar (PB_...) field
--------------------------------
You can use add one or more PB_ flags to customize all parallel bars in your level.
See the description of PB_ constants in [Reference] panel of NG_Center, in _MNEMONIC CONSTANTS section.

SpeedForSlide field
-------------------
If you have used the flag PB_LARA_CAN_SLIDE, you can set also the speed for sliding left/right of lara.
The value has like unit one block = 1024.
The defaul value is 6, if you don't want change the default value type IGNORE in this field.

MaxTurns field
--------------
By default the max meaningful number of turns is 10.
You can change this value to force the max power increasing adding turns.
Reasonable values are enclosed in the range 3 - 10

If you don't want change this value you can type IGNORE.

## CUST_PAUSE_FLY_CAMERA
Used to allow at player to stop temporary a flyby sequence keeping down the wished key.

Syntax: Customize=CUST_PAUSE_FLY_CAMERA, ENABLED/DISABLED, KeyBoardScanCode

About KeyBoardScancode value it will be the code of keystroke to use to freeze  a flyby sequence. You find the list of scan codes in "Reference" panel of NG_Center, in the section named "KEYBOARD SCANCODES list".

If you type IGNORE in this field the default value will be 25 (KeyP)

Remark: it's necessary keep down continuosly the key to freeze the flyby camera.

## CUST_RAIN
Used in Customize= command.
Syntax: Customize=CUST_RAIN, FlagsForRain (FR_...), DropSize, SprinklerAmount, MaxRain, MinRain, Float1, Float4, Float8, Float16, Extra

This command it's not necessary to do work the rain. You can also omit this command.
You could use CUST_RAIN only when you set RAIN_ALL_OUTSIDE in the Rain= command, and you omit to type [Rain] intensity in rooms of your level. In this situation you can use the CUST_RAIN customize to set the features of the rain for your level.
Remember that if you use CUST_RAIN and keep different settings for rain in different rooms (RAIN_SINGLE_ROOMS mode, with intensities 1/4 closed to [Rain] button) these differences will be very little or absent because the cust_rain overrides almost fully the individual settings for room.

FlagsForRain (FR_...) field
---------------------------
In this field you can type on more flags FR_...
You can type IGNORE in this field to omit FR_ flags.

DropSize field
--------------
This value is the width of water drop. Each drop is like a triangle where the base is the DropSize value.
You can type IGNORE in this field to preserve the default value 2.

SprinklerAmount field
------------------
This field should be the number of Sprinkler for each drop when it touches the ground.
You can type IGNORE in this field to preserve the default value 1.
Remark: if you increase too much this value you could create troubles with ohter particle resoruces like smoke and flames.

MaxRain and MinRain fields
--------------------------
I'm not sure about the meaning of these two fields, but I suppose they are a range to set the density of rains, i.e. the number of drops (MinRain) in a given space (MaxRain).
When you decrease the MaxRain, the space covered by rain in front of lara will be reduced, while when you increase the MinRain the number of single drops will be increased.
Anyway remember that if you try to get in same moment a wide space covered by the rain and a more deep rain you could get bad results. You should balance these two values: when you increase one, you have to decrease the other and vice-versa.
You can type IGNORE in these fields to preserve the default values: MaxRain = $800, MinRain=$80

Float1 field
------------
Unknown

Float4 field
------------
Unknown

Float8 field
------------
Unknown

Float16 field
-------------
Unknown

Extra field
-----------
You can set in this field some extra value required by some FR_ flags.
See the description of FR_ constnats to discover how using the Extra field.

## CUST_ROLLINGBALL_PUSHING
You can change features activated with OCB 4 or 8 in RollingBall.
Using 4 or 8 in OCB of rollingball you enable the skill of lara to activate or moving rollingball pushing it like it was a pushing object.
With CUST_ROLLINGBALL_PUSHING you can customize this effect: used animations, time for moving, distance for activation of pushing ect.

Syntax: Customize=CUST_ROLLINGBALL_PUSHING, Distance, PushAnim, FailedAnim, FrameOfMoving, FrameOfActivation, FramesOfInvulnerability, Speed

Remark: if you want omit to change some values just you type IGNORE in that field and the engine will use the default value preset in hardcoded mode.

Description of fields:

Distance
--------
Distance is the distance between Lara and the center of rolling ball.
Only when lara is at correct distance she will be able to push it.
The default value for distance is 600, where 1024 is the size of a single sector of game and 1 click is 256 units.
The default distance (600) is good for spiked rolling ball you find in tut1.wad.
Differently, if you use a non spiked rolling ball, pheraps you should increase the distance, because lara will be keeped far from collision rolling ball.

PushAnim
--------
With this field you can change the animation used to show lara is pushing rolling ball.
By default the animation is the anim number 316, i.e. the same use to push the big pushable button used as switch.

FailedAnim
-----------
This is another animation number. It used when lara is not able to move the rollingball because there is some obstacle in oppisite side of rollingball.
The default value for FailedAnim is the animation 120

FrameOfMoving
-------------
FrameOfMoving is the frame number of PushAnim animation from that the engine will move in passive way the rollingball.
For "passive way" I mean the rolling ball is not moving byself (since it's not yet activated) but it was moving by the pushing of lara.
The default value is 20th frame of PushAnim animation.
The interval while the rolling ball will be moved in passive way, it the range between FrameOfMoving and FrameOfActivation.
If you type as FrameOfMoving a number bigger than FrameOfActivation the rolling will be never moved in passive way.

FrameOfActivation
-----------------
FrameOfActivation is the frame number of PushAnim animation when the rolling will be really enabled, and it will begin to move byself.
The default value is 50th frame.

FramesOfInvulnerability
-----------------------
FramesOfInvulnerability is the number of frame of animation following the PushAnim animation, while Lara will be invulnerable.
This invulnerability it's necessary because otherwise lara should be always killed by rolling not just it has been enabled (since she is very closed to rolling ball in that moment)
So to give to lara some time to go away from rolling ball, there is some time of invulnerability.
The default value is 30 frames, where 1 second = 30 frames.
Remark: while lara is performing the PushAnim animation she is always invulnerable.

Speed
------
The "speed" is the increment used to move rolling ball when lara push it in passive mode. Each frame will be added the "speed" increment to correct coordinate (x or z).
The default value is 6.
If you increase this value the pushable will be moved fastly by Lara.
For example if you have the trouble to see lara enter in rolling ball in pushing phase this means you should: reduce the FrameOfMoving value to move immediatly rolling ball, and/or, you can increase the speed to move more fastly the rolling avoding that lara, in her movement, enters in rollingbal.

## CUST_ROLLING_BOAT
Used with Customize = command.
Syntax: Customize=CUST_ROLLING_BOAT, SlotBoat, FlagsRollingBoats (FRB_), SwingingSpeed, PitchingSpeed, RollingSFX

By default, all boats when Lara is not on board are fully still, this is not very normal when there are waves on water surface.
With this customize you can set a swinging and/or pitching movement to get more realistic the boats on the water.
You can use one or more Customize=CUST_ROLLING_BOAT commands in same level section, to have different rolling for different boats if you wish.

SlotBoat field
--------------
In this field you type the slot of boat to roll.

Remark: teorically you could use also a slot different than boats, for example if you created a fake boat in an animating slot and you wish move it in realistic way.

Note: when you set KAYAK for slotboat remember that the rolling will be performed only when the kayak is in most of its part on the water.

FlagsRollingBoats (FRB_) field
------------------------------
You can add two or more FRB_ flags to set the level of the rolling movement.
The most of FRB flags set the wideness of rolling movement for pitching or sliding.
For example using the FRB_PITCHING_HIGH flag the boat will move upper the peak about the water surface than the FRB_PITCHING_LOW flag, where the peak will move only a bit over the water surface.

See the description of FRB_ constants for more infos

SwingingSpeed field
-------------------
The swinging rolls the boat for the waves hit the boat on the sides.
If you don't wish any swinging type 0 in this field.

The speed value to type in this field works like degrees but in a specific format of tomb4 where 90 degrees = $4000.
You have to type not too exagerate values because this value will be added (or subtracted) by current facing 30 times for second.
Reasonable values are enclosed in the range $08 / $100 (or, in decimal: 8 / 256)

Tips:
* When you set a LOW value for swinging it's better reduce also the swing speed to compensate the reduction of "distance" covered in the swinging.
* Normally the swinging is less visible than pitching because the shape of the boat is slim, for this reason it's better set higher values for swing speed.

PitchingSpeed field
-------------------
The pitching rolls the boat for the waves hit the boat on the peak
If you don't wish any pitching type 0 in this field.

About the values to type read the description of SwingingSpeed field

RollingSFX field
----------------
You can type in this field the number of some SFX sound.
This sound will be played when the rolling is to max value, creating the effect of boat touches other boats or the water surface refalling down.
If you don't wish any sound type IGNORE in this field.

## CUST_SAVE_LOCUST
Used in Customize script command.

Syntax: Customize= CUST_SAVE_LOCUST, ENABLED

This customize permits to fix an old bug of tomb4 engine: when the locust swarm has been enabled, and player save the game, at reload the locusts are missing.
The reason of this bug is that the Eidos' programmers probably prefered omit the saving of data about the locusts since they are about large and with playstation 1 version it was important reduce the size of savegames.
Anyway now you can save all data about current particle of LOCUST_EMITTER slot, just you add in [Title] (for all levels) or in a specific [Level] section the row:

Customize=CUST_SAVE_LOCUST, ENABLED

Remark:
The savegame will be bigger than about 2000 bytes if you enable the saving of locusts.

## CUST_SCREENSHOT_CAPTURE
Used in Customize command.
Syntax: Customize=CUST_SCREENSHOT_CAPTURE, SecondsOfDurate, FrameGap, QualityScreenshotFlag (QSF_...)

This command should be used only in temporary way since it could slow down the game.
By default when you hit F3 key (or "." key on US keyboards) the game save the current screen in an image file with progressive names "shot1.bmp", "shot2.bmp" ect.
Using the CUST_SCREENSHOT_CAPTURE setting you can have a long serie of captured frames instead a single image like in the past.
This feature is useful when you wish find the best image in a very fast action in the game.

SecondsOfDurate field
---------------------
In this field you can set for how much time the capture sequence will work.
For example if you type 3 in this field, the screens of game will be captured for 3 seconds continuosly.

FrameGap field
--------------
If you want reduce the number of captured images you can set in this field an interval in capture phase.
If you type 0 in this field, the game will create images for each frame (so, no gap in this case)
If you type 1 it will be captured one frame each two.
If you type 2 it will be captured one frame each three, ect.

QualityScreenshotFlag (QSF_...) field
-------------------------------------
Another reason to capture a sequence of images is to show to some friend a dynamic action without using a movie.
In this situation, sending many images via internet it's advisable reduce their size.
If you type in this field some QSF_ value, you can reduce the size or the quality (omitting the QSF_TRUE_COLOR flag) of the captured images.
Remark: differently, if you want get a full screen image with max quality you should type IGNORE in this field.

## CUST_SET_CREDITS_LEVEL
The credit level should be the last level of your adventure.
In The Last Revelation the number was 39.
The target to reach is to have scrolling text with credits at end of your level.
You should also substitute the credit texts with your custom texts.
You find this text in [PSXString] of any language text file. The first text is "Programmers" (index =246)and the credit list continues until the end of [PSXString] section.
Syntax: Customize=CUST_SET_CREDITS_LEVEL, NumberOfEndLevel

## CUST_SET_INV_ITEM
Permit to set an inventory items like INVARIABLE items, i.e. items that will remain always in inventory after to be used, like the binocular.

Syntax: Customize= CUST_SET_INV_ITEM, SlotOfItem

Remark: you can place one or more "Customize=CUST_SET_INV_ITEM" commands in same level section so to assign the invariable property to many inventory items.

## CUST_SET_JEEP_KEY_SLOT
Permit to change the slot used to store the jeep key.

Syntax: Customize= CUST_SET_JEEP_KEY_SLOT, SlotForJeepKey

The reason to modify the default slot (default slot was number 175, corresponding to PUZZLE_ITEM1)of jeep key, borns when you use jeep and side-car in same level, using the new features added in trng (from version 1.1.8.6).

When you want use side-car and jeep in same level you must:

* Use last version of wad merger (version 1.98.0.103 or higher, released in may 2008), and copy in its installation folder the new "TR4Objects.dat" file you find in Multiple Vehicles sample zip file.
* Copy the jeep slots (jeep and vehicle extra) in common way in your wad
* Copy the motorbike slot in your wad in common way
* Copy the vehicle_extra slot from wad with side-car, to new slot MOTORBIKE_LARA in your wad.
To perform this operation just you click on [Copy] button (in wad merger) keeping down the key SHIFT. It will be showed a list of all available slots and  at bottom of this list you'll see the slot named MOTORBIKE_LARA, it's the new slot created to host the lara's animations for motorbike.

Performing above operation you can have jeep and sidecar in same level, but there is yet a little problem: the slot used to host jeep key it's the same used to host the object "Nitrous Oxide Feeder" used to enhance the speed of side-car.
Since both above objects (jeep key ad Feeder) are in slot puzzle_item1, there is a conflict. To solve this problem you can use this customize command to set another slot for jeep key, in this way the old slot puzzle_item1 will work only for Nitrous Oxide Feeder, while the key to switch on the jeep will be stored where you wish with this cust command.
For example if you want store the jeep key in PUZZLE_ITEM2 (slot 176) you can type in [Level] section of your level the following script command:

Customize=  CUST_SET_JEEP_KEY_SLOT, 176

or (same result):

Customize=  CUST_SET_JEEP_KEY_SLOT, PUZZLE_ITEM2

## CUST_SET_OLD_CD_TRIGGER
Used to restore old behavior of CD trigger.

Syntax: Customize=CUST_SET_OLD_CD_TRIGGER, ENABLED/DISABLED

Using new bass sound engine the CD trigger changed its behavior.
The looped sounds play on channel1 while the single-shot cd (track number less than 105) play on channel2, letting on channel 1 the previous audio track.
If you don't want this method and you prefer restore old default method you can set following command in your [Level] section:

Customize=CUST_SET_OLD_CD_TRIGGER, ENABLED

The old method played always all CD trigger sounds on channel1 and when a single-playback sound stop the previous looped audio track, when this new single-playbakc sound was complete, the last looped audio tracked was restored always on channel1 from start.

## CUST_SET_SECRET_NUMBER
Arguments: NumberOfSecrets
With this cutomize you can substitute the text in Statics screen of game where is the row: Secrets Found ".. / 70"
The value you type (NumberOfSecrets) will substitute the default "70" for max amount of secrets.

You can use this customize in two way:
* If you want show a different max amount of secrets, levels for leves, i.e. in each level a different amout of secrets for that level, you have to place a row:

Syntax:Customize=CUST_SET_SECRET_NUMBER, NumberOfSecrets

In each level of your script dat (excluding Title level)
In this way you can set different values to specify the amount of secretd for each level.

While other chance is:

* Modify the global amout of secrets it will be the same for all levels.
In this case you have to use a single Customize line and place it in level section of Title level.
Typing a single CUST_SET_SECRET_NUMBER in title level it will affect this setting for all levels.

## CUST_SET_STATIC_DAMAGE
Permits to change the value of damage or poison ocurring to Lara when she touches a static with ocb 32 or ocb 256

Syntax: Customize=CUST_SET_STATIC_DAMAGE, Damage, PoisonIntensity

Damage field
------------
You can type a value between 1 and 999.
If you set 999 lara will be killed immediatly.
If you don't change this value, the used default value is 10.

PoisonIntensity
---------------
You can type any value for poison.
Anyway the reasonable values are within range 10 / 4096
A big scorpion and Harpy have a poison intensity of 2048, while a little scorpion is 512.
If you don't change this value the used default value will be 256.

## CUST_SET_STILL_COLLISION
Used in Customize= command.
Syntax: Customize=CUST_SET_STILL_COLLISION, Collision Flags (COLL_...), LowerHeight, MoveableArray

This customize allows to enable a still collision method when Lara touches static or moveable items.

Remark: some people calls the "still" collision as "hard" collision.

By default, Lara, when she touches some item, continues to move legs and arms. This behavior is different than that you see when Lara touches a wall, where she stop her immediatly.
Using CUST_SET_STILL_COLLISION customize, you can give to item collision same behavior you see in wall collision.

CollisionFlags (COLL_....) field
--------------------------------
In this field you can type, one, two or more COLL_ flags to enable the still collision with different types of items.

See the descriptions of COLL_.. constants in MNEMONIC CONSTANTS section of Reference panel.

LowerHeight field
-----------------
In this field you can type the lower (acceptable) hight that an item may have to enable the still collision.
Theorically you can also type 0 in this field, and in this way all selected items will enable the still collision.

Anyway you should think about this little problem: when lara touches a very little (low) item with her feet, like an one-click height item, it should be weird see her stopped immediatly like wheter that little item was an unpassable obstacle.
In this situation pheraps it's better let the old "sliding" collision, used with items.
The reason of above speech, is that, when a little (low) obstacle is a moved up floor (like a little wall) lara could be able to walk over it, while with statics and moveables this is not possible, therefore we have our problem about the realism to solve.

If you type IGNORE in this field, it will be used the (reasonable) value of 300 units as lower acceptable height.

Remark: Remember the units used in this field have these references: one click = 256 units, one sector = 1024 units.

MoveableArray field
-------------------
From this field you can type one or more moveable slot using the still collision.
You can also let empty this field, of course.

Remember that you can select different moveable class using some COLL_ flag, so my suggeston is to use these COLL_ flags and only when you find some moveable not enclosed in COLL_ flags you should use the MoveableArray.

For example if you want having still collision also with the moveable POLEROPE and with the ROLLINGBALL, you type a customize command like this:

Customize=CUST_SET_STILL_COLLISION, COLL_STATICS+COLL_DOORS, IGNORE, POLEROPE, ROLLINGBALL

In this way, other to have still collision with statics and doors, you'll have still collision also with pole rope and rollingball.

Another interesting usage of MoveableArray  array, is when you want exclude some moveable from still collision.
This trick will have a sense only when you had typed some COLL_ moveable types, of course.

For example, we suppose you enabled the still collision for all doors (COLL_DOORS) but you discovered that there is some problem with a door you used for a "race vs time". In this situation when lara touches the door while it is closing, lara froze immediatly.
Since you are a good boy, you want let to the player the chance to "slide" over the closing door and for this reason, you disable the still collision only for that type of door, preserving the other doors.

In this case you have to type the number of slot to exclude as a negative number.

For example if you want exclude the DOOR_TYPE4 with slot number 325, you can type this script command:

Customize=CUST_SET_STILL_COLLISION, COLL_STATICS+COLL_DOORS, IGNORE, -325

Remark: it's better don't use the syntax "-DOOR_TYPE4" because the mixing of mnemonic constant and aritmetic sign "-" minus, could confuse NG_Center in some circustances.

## CUST_SET_TEXT_COLOR
Used in Customize= command.
Syntax: Customize=CUST_SET_TEXT_COLOR, Text Type (TT_ ...), Color for Text (CL_ ...)
This customize allows to change the default color for many texts in menu, options screen, inventory mode, statistics screen ect.
You can set a new color for wished text type simply typing the text to change, with a TT_ constant value, and typing the CL_ .. color.
For example to have the main menu drawn in red color just type in your [Level] section this command:

Customize=CUST_SET_TEXT_COLOR, TT_MAIN_MENU, CL_RED

You can type two or more Customize=CUST_SET_TEXT_COLOR commands in same [Level] or [Title] section.

Remark: it's advisable place this customize in [Title] level when you want change text of main menu, option or new level screens.
The customize typed in [Title] section will work also for all other levels until you don't place another specific customize in some [Level] section.

## CUST_SFX
Used with Customize = command.
Syntax: Customize=CUST_SFX, TypeSound (TS_...), Sound Effect Number

The CUST_SFX customize allows to override the sound effect used in some trng features, like the Diary.
You can use many Customize=CUST_SFX commands in same [Level] section.

TypeSound (TS_...) field
------------------------
Choose a TS_ constant to set what is the sound type to change.
See the TS_ constants in Reference panel of NG_Center program

Sound Effect Number field
-------------------------
Type the number of new sound to use.
You find a list of sound sfx numbers in the Reference panel of NG_Center

## CUST_SHATTER_RANGE
Syntax: Customize=CUST_SHATTER_RANGE, FirstStaticAsShatter, LastStaticAsShatter

By default you have only 10 statics with shatter attribute (from SHATTER0 to SHATTER9) but using this cust_ setting you can enlarge the number of shatter statics.
Default values are:
FirstStaticAsShatter = 50 (SHATTER0)
LastStaticAsShatter = 59 (SHATTER9)

Looking "Static list" in reference panel you can see the name (and IDs) of all static meshes and you should choice if you want have more shatter as following statics or previous statics.
For example If you want have other 10 static meshes as shatter, and you want that becomes shatter the statics following the standard shatter objects, just you type this command:

Customize=CUST_SHATTER_RANGE, 50, 69

In this way the 10 following statics (from EXTRA00 to EXTRA09) will become shatter objects.

Differently, if you don't want change EXTRA statics, and you prefer use as shatter the statics come first of standard Shatter, you can type following command:

Customize=CUST_SHATTER_RANGE, 40, 59

In this way all static from DEBRIS0 to DEBRIS9 will become shatters.
Use following short list as reference for above examples:
 40  $0029  DEBRIS0
 41  $0029: DEBRIS1
 42  $002A: DEBRIS2
 43  $002B: DEBRIS3
 44  $002C: DEBRIS4
 45  $002D: DEBRIS5
 46  $002E: DEBRIS6
 47  $002F: DEBRIS7
 48  $0030: DEBRIS8
 49  $0031: DEBRIS9
 50  $0032: SHATTER0
 51  $0033: SHATTER1
 52  $0034: SHATTER2
 53  $0035: SHATTER3
 54  $0036: SHATTER4
 55  $0037: SHATTER5
 56  $0038: SHATTER6
 57  $0039: SHATTER7
 58  $003A: SHATTER8
 59  $003B: SHATTER9
 60  $003C: EXTRA00
 61  $003D: EXTRA01
 62  $003E: EXTRA02
 63  $003F: EXTRA03
 64  $0040: EXTRA04
 65  $0041: EXTRA05
 66  $0042: EXTRA06
 67  $0043: EXTRA07
 68  $0044: EXTRA08
 69  $0045: EXTRA09

## CUST_SHATTER_SPECIFIC
Used with Customize= command
Syntax: Customize=CUST_SHATTER_SPECIFIC, Slot1,Slot2, Slot3, SlotN
In this customize you can type the moveables that will be able to destroy the shatter objects with the ocb value 8192.
Only the moveable in this list will be able to destroy the statics with 8192 ocb

Notes:
- You can type upto 250 moveable slots
- You can have only one  CUST_SHATTER_SPECIFIC for level section
- If you want control for explosion kind, remember to use GRENADE (365) for granade shot by SAS, while an ammo type (see following list) for explosive ammos fired by lara.
- If you wish set, not a moveable (like enemy) but a specific weapon that was able to destroy the given static, you can type one of these slots for weapon ammos:
PISTOLS_AMMO_ITEM
UZI_AMMO_ITEM
SHOTGUN_AMMO1_ITEM
SHOTGUN_AMMO2_ITEM
CROSSBOW_AMMO1_ITEM
CROSSBOW_AMMO2_ITEM
CROSSBOW_AMMO3_ITEM
GRENADE_GUN_AMMO1_ITEM
GRENADE_GUN_AMMO2_ITEM
GRENADE_GUN_AMMO3_ITEM
SIXSHOOTER_AMMO_ITEM

## CUST_SHOW_AMMO_COUNTER
You can use this cust value if you want enable the showing of number of current ammo yet available.

Syntax: Customize=CUST_SHOW_AMMO_COUNTER, Color, FormatFlags (FT_...) , BlinkTime, SizeCharacter (SC_...), ShowCounterFlags (SHOWC_...)

If you place a customize command with CUST_SHOW_AMMO_COUNTER, you'll have in game a text on screen showing in real-time the current number of ammo for current weapon.
Remark: the ammo counter will be showed only when lara extracted some weapon.

Color, FormatFlags (FT_...) , BlinkTime, SizeCharacter (SC_...) fields
----------------------------------------------------------------------
All above fields have the same usage and target of fields in TextFormat= script command. So you should read the description of above field in section NEW SCRIPT COMMANDS of Reference panel of NG Center program.
The fields are used to set color, size and position of text about ammo informations.

ShowCounterFlags (SHOWC_...) field
----------------------------------
In this field you can set some SHOWC_... constant flags to furhterly customize the showing of ammo counter.
See description of SHOWC_... constants in Reference panel.

## CUST_SLOT_FLAGS
Used with Customize= command
Syntax: Customize=CUST_SLOT_FLAGS, Slot, FlagsForSlot (FFS_...)

This customize supplies a generic way to customize some feature for a given slot.

Note: if you wish customize two or more slots, you have to type two or more Cutomize=CUST_SLOT_FLAGS commands, one for each slot to customize.

Slot Field
----------
Type the number or name of the slot whom assign some FFS_ flag.
See in the reference panel of ng_center, in the "mnemonic constants" section, the FFS_ flag you can use and their scope.

FlagsForSlot (FFS_...) field
-----------------------------
You can add one or more FFS_ flags, linked with plus "+" sign if they are two or more.
Please, do not use two different Customize commands for the same Slot, this operation could confuse trng engine.

## CUST_SPEED_MOVING
Syntax: Customize=CUST_SPEED_MOVING, Speed
When you use some ACTION trigger to move animating or other moveables, the engine uses as default speed the value 32.
A way to use a speed different than 32 is to type in OCB of that object a different speed, anyway this method works only for Animating objects becase other moveables have (or could have) some usage of OCB field and for this reason the TRNG engine will ignore the OCB field of object different than Animating.
If you want modify the default speed (used by all moveables different than Animating) you can use the CUST_SPEED_MOVING constant.
For example:

Customize=CUST_SPEED_MOVING, 50

Above command will force to use as default speed the value 50.

## CUST_STATIC_TRANSPARENCY
Permit to change the values for transparency of static you can assign to some static with new flipeffect trigger "Static. Transparency. .."
Syntax: Customize=CUST_STATIC_TRANSPARENCY, GlassOpacity, IceOpacity

To set transparence you have really to set the value of opacity.
The values you can set for GlassOpacity and/or IceOpacity are a percentage where:

0 % = Opacity is none, the object is fully invisible
100 % = Opacity is full, there is no transparence for object

You should type values different by 0 and 100, of course.
Using 50 % the object and background should have same presence (semi-transparent)
Using 90 % the object is almost normal (full opacity) but there is a lite transparence
Using 10 % the object is almost invisible but you can see it like a ghost.
You can type any value between 0 and 100.
The default values (used if you don't use any Customize=CUST_STATIC_TRANSPARENCY command) are:

Glass = 50 %
Ice = 81 %
Remark: In above examples I used the percentage sign "%" but you don't type it in your script command.

## CUST_TEXT_ON_FLY_SCREEN
Enables the printing of screen also during flyby camera sequence.

Syntax: Customize=CUST_TEXT_ON_FLY_SCREEN, ENABLED/DISABLED

## CUST_TITLE_FMV
Used with Customize command.
Syntax: Customize= CUST_TITLE_FMV, FmvNumber, TestMultiPlay

FmvNumber field
----------------
Type the number of fmv to play. This is the same number in the filename of the fmv. For example if you wish play the file fmv5.wmv movie, you type 5.

TestMultiPlay field
-------------------
This field may be 0 (false) or 1 (true).
If you type 0, the fmv will be showed only the first time the player launched the tomb4 engine.
If you type 1, the fmv will be played everytime the controll will pass newly at title phase. I.e., at start of the game, and when Lara dies or the player exits from the game to go back to the titles.

Example:
If you wish having a FMV at boot-strap of the game, i.e. before beginning the title level, you have to place in [title] section following commands:

FMV=	8, 1
Customize=CUST_TITLE_FMV, 8, 0

In above example the fmv to show at the boot is the fmv8, you enable the escape for this fmv and you wish that was playes only once, at start of the game.

Remember to place the fmv file in the subfolder of trle named "FMVs" and to set in script.txt in the [PCExtensions] section the extensione used from your fmvs.
For example if your movie is named "fmv8.wmv" you should type the row:

FMV=	.wmv

## CUST_TR5_UNDERWATER_COLLISIONS
Used by Customize= command.
Syntax: Customize=CUST_TR5_UNDERWATER_COLLISIONS

Adding this customize in your level you enable the collision method used in tomb raider chronicles about the underwater collisions of Lara.
The difference respect the tr4 collision is that, in tr5 engine lara, Lara will be not stopped when she touches a wall, but she will move or rotate going on in better direction.
Differently in tomb4 any contact will stop lara.

## CUST_WATERFALL_SPEED
Used with Customize= command.
Syntax: Customize=CUST_WATERFALL_SPEED, PixelScroll

With this customize you can change the speed of waterfall textures.
The default value is -7, if you set higher absolute values, like -12, the speed will be increased.
If you use a positive sign the direction of scrolling will be inverted (from down to upstairs).
The max values are -63 / +63 but in the reality the meaningful max values are -31 / +31

## CUST_WEAPON
Customize a specific weapon.

Syntax: Customize= CUST_WEAPON, SlotOfWeapon, Weapon flags (WEAP_..) , SoundForShot, FramesForRecharge, DurateFlash, Extra, MaxDistanceForAiming, FrameToTakeWeapon, FrameToLetWeapon, Random, VPositionOfWeapon, Unknown, FrameCounter, FrameMinRange, FrameMaxRange, OrigX, OrigY, OrigZ, OrigOrient

Remarks:
*If you wish, you can omit last fields of this command and all omitted fields will be seen like IGNORE fields from trng engine, i.e. for omitted fields will be preserved the default value.
This feature is always present in all Customize commands from 1.1.8.1 dll version.

*You can customize some features of weapon also using the CUST constant named CUST_AMMO. The current CUST_WEAPON works only for setting specific for weapons, while CUST_AMMO allows to customize the single ammos.

Description of fields
---------------------

SlotOfWeapon field
------------------
In this field you set the slot corresponding to weapon you mean customize.
You can choose between following slot values:

PISTOLS_ITEM
UZI_ITEM
SHOTGUN_ITEM
CROSSBOW_ITEM
GRENADE_GUN_ITEM
SIXSHOOTER_ITEM

Weapon flags (WEAP_..) field
----------------------------
You can add none, one or more WEAP_ flags to set some special features for current weapon.
If you don't want set any flag you can type IGNORE in this field.
Read the description of WEAP_ constants in MNEMONIC CONSTANTS list in Reference panel of NG_Center program.

SoundForShot field
------------------
This field allows to change the default sound effect for this weapon. This is the sound used when the weapon does fire and not the other sound when the shot hit some target.
You can type IGNORE if you want let unchanged this value.

Default values:

PISTOLS_ITEM = 8
UZI_ITEM =    43
SHOTGUN_ITEM= 45
CROSSBOW_ITEM = 235
GRENADE_GUN_ITEM (sounds are in shooting animation with playflip animcommand)
SIXSHOOTER_ITEM  = 121

FramesForRecharge field
-----------------------
This value is the number of frames (30 for second) used for auto-shot.
When player keep continuosly down the action key the FramesForRecharge value will be used to set the pause between a shot and following.
Bigger values will cause a longer pause time and hence less shots for second.

Default values:

PISTOLS_ITEM = 9
UZI_ITEM = 3
SHOTGUN_ITEM = 9
SIXSHOOTER_ITEM = 16
CROSSBOW_ITEM (Fixed, you cann't change it, it dependes by recharge animation length)
GRENADE_GUN_ITEM Fixed, you cann't change it, it dependes by recharge animation length)

DurateFlash field
-----------------
To change the number of frames (30 for second) of durate for fire flash when the weapon shots.

Default values:

PISTOLS_ITEM = 3
UZI_ITEM = 3
SHOTGUN_ITEM = 3
SIXSHOOTER_ITEM = 3
GRENADE_GUN_ITEM = 2
CROSSBOW_ITEM = 2

Extra field
-----------
This field may host different values in according with some WEAP_ flag.
Read the descriptions of WEAP_ constants to discover the value to type in this field.

MaxDistanceForAiming field
--------------------------
The automatic aiming allows to Lara to point an enmey when him is (about) in front to her and he is within a specific max aiming distance.
The default value for all weapons is 8 sectors, if you want you can set different aiming distance for each weapon.
For example if you set 0, when player will use this weapon he will be not able to have automatic aiming of enemies.
If you set 3 (sectors), using this weapon the automatic aiming will work with closed enemies, withing 3 sectors of 3d distance, ect.

Remark: Increasing too much this value it could compromise the speed of game engine because the compute to locate aimable enemies will be enlarged to very wide zones with meaningful wasting of CPU time.

FrameToTakeWeapon and FrameToLetWeapon fields
---------------------------------------------
These two fields set the number of frame of get on, get off in Lara's hands animation.
You should change these values only when you are trying to change this animation to extract weapons.
For little weapons (pistols, uzi and sixshooter) you have to set the same frame for both field, and the game engine will use that frame -1 to place the weapon in holster, while that frame +1 to place the weapon in Lara's hands.
The reason of this method is because the game engine will use always the same animation to extract weapon or to place them in holsters, only the frame will change a bit +1 or -1 in according with extract or place-in animation.

About the big weapons like shotgun, grenade-gun and crossbow you have two different frames to extract or place-in holders operation.

Following table shows the default values:

          PlaceWeaponInHand      RemoveWeaponFromHand
-----------------------------------------------------
Pistols    13 (it becomes 14)    13 (it becomes 12)
Revolver   15 (it becomes 16)    15 (it becomes 14)
UZI        13 (it becomes 14)    13 (it becomes 12)

Shotgun    10				    21
grenadeGun 10                    21
CrossBow   10	                   21

Remark: looking the orginal animations to extract weapons it could seem weird the above numbers of frames since they seem bigger than total number of frame of above animations, anyway I presume there are a sort of "FrameRate" field, like you see in animation editor program, but in this case it is an hardcoded value.
I'm not sure and you should perform some attempts, anyway you can change that misterious value using the field named FrameCounter in this same Customize command.(see below)

Random field
------------
I'm not sure about the meaning of this field.
It used to change (whereby a multiplication) a random value.
I presume it could be used to set randomly the direction or intensity for some shoting effect but I've not had the time to verify its usage.

Default values:

Pistols = 1456
Revolver= 728
UZI = 1456
Shotgun = 0 (probably for shotgun this field will be ignored)
GrenadeGun = 1456
CrossBow = 1456

VPositionOfWeapon field
-----------------------
This field apparently is a Cord Y value (up/down in 3d world) to compute the height of weapon (when lara has it in her hands) respect to water surface.
When game engine detects that the water is touching the weapon it will force the put off the weapon.
Anyway this is only an hypothesis.

Pistols = 650
Revolver= 650
UZI = 650
Shotgun = 500
GrenadeGun = 500
CrossBow = 500

Unknown field
-------------
I've not any idea about what is the target of this field.
Anyway you should discover this target performing some attempts with different values.
In my experiments I discovered only that this value is not used when lara extracts the weapon and neither when she is shooting.
Pherpas it works about aiming.

For all weapons the default value is 1820

FrameCounter field
------------------
Another misterious field.
This value I presume is a number of frame and surely it will be decremented by 1 for each game cycle.
I suppose it could be a sort of "framerate" field to multiplicate the durate of animation to slow down extract weapon animation.
This field is used only for Pistol, UZI and revolver.

Default values:

Pistols = 4
Revolver = 7
UZI = 4

FrameMinRange and FrameMaxRange fields
--------------------------------------
I've not yet discovered the precise usage of these two fields.
I have discovered only they are frame values and these two values were used like the min and max values of a range.
These two fields will be used only for Pistols, revolver and UZIs
If you change the frame to extract weapon (see above fields FrameToTakeWeapon and  FrameToLetWeapon fields) you should remember that this number of change weapon has to be within this misterious range of FrameMinRange and FrameMaxRange fields, otherwise the animation will not change weapon in correct way.

Default values:

Pistols = min: 5   max: 24
Revolver= min: 8   max: 29
UZIs =    min: 5   max: 24

OrigX, OrigY, OrigZ, OrigOrient field
-------------------------------------
These fields work only for weapon launching real (visible) ammo, like GrenadeGun and CrossBow.
If you change animation of shape of weapon meshes you could require to change also the origin of bullet at start of its movement.
The OrigX, OrigY and Origz are algebrical values (+ or - values) and they will added to default current position for bullet.
To understand how set these three values you could read the description of AddEffect script command where there are the DispX, DispY, and DispZ field that works in same way.
About the OrigOrient field, it set the degree about direction of bullet.
If you set 0 the direction will be not changed respect than default value, while if you set some positive or negative value the (horizontal) direction will be changed of wished degrees.
The way to set degree is particular.

90 degrees to east will be 16384 ($4000)
90 degrees to west will be -16384 ($C000)

You can set also intermediate values, of course, anyway you can use above example to understand how compute other values.

## DEMF_CINEMA_SCREEN
Used in Demo script command.
This flag add the cinema screen effect while the demo is playing.
For cinema effect we mean when two black bars, at top and bottom of the screen, give a wide screen effect. This is alike that  effect you see in the flyby sequence but in this case the black bars are higher to cover the (further) hp bar (otherwise it is only partially covered, bad effect) and to distinguish demo cutscenes respect flyby camera.

Note: A problem with cinema screen is that top and bottom legend demo will be covered by the black bars. For this reason if you enable cinema screen you should use a vertcal center alignment for demo legend or disable the demo legend text.

## DEMF_CROSS_FADE
Used in Demo script command.
By default the first demo will begin with a fade-in (from black to game screen), anyway if you used DEMF_PLAY_LEVEL_SEQUENCE or DEMF_PLAY_ALL_SEQUENCE, you can add cross fade to swap the two demo while the game screen is black.
If you omit this flag in a demo sequence, lara will disappear from last position of first demo, and she will appear (suddenly) in new room and position of start point of second demo.
The disvantage to use cross fade is that the last frames of first demo and the first frames of following demo, will be darked.

## DEMF_MUTE_SFX
Used in Demo script command.
This flag silent the sound effects but it lets the background music, from Audio folder.
This solution is good when you wish give to the demos a more understated layout, avoiding the shoot or jump sounds but letting the chance to have a soft background music (audio track). In this situation it's interesting playing a different audio track in demo mode to entertain the player/viewer.

Note: to silent the sound effects trng engine will change (temporarily) the sfx sound volume, setting it to zero. Oddly also with this null volume, the strongest sfx sounds could be listened in light way.

## DEMF_MUTE_TRACK
Used in Demo script command.
You can silent the audio tracks while the demo is playing with this flipeffect.
This flag doesn't affect the sound effects.

## DEMF_PERFORM_AT_START
Used in Demo script command.
Used in [Level] section, (cutscene mode)
If you use a Demo command in a [Level] section, you can add the DEMF_PERFORM_AT_START flag to force trng to play the demo at begin of the level, like a intro cutscene.
Note: if also the DEMF_PLAY_LEVEL_SEQUENCE flag is present, all demos cutscenes will be played in a sequence with no chance for player to interact with the game until last demo has been played.

## DEMF_PLAY_ALL_SEQUENCE
Used in Demo script command.
Used in [Title] section.
This flag force to perform all demos whom indices are in DemoArray field of Demo= command.
In this case it's no important the level number of single demos, all they will be played when first demo has been triggered.
Since everytime a demo can work only in the level where it had been recorded, it's better in this case, type in the array demo indices contiguous for same level to reduce the number of loading new level phases.

## DEMF_PLAY_LEVEL_SEQUENCE
Used in Demo script command.
Used in [Title] section.
With this flag, when a demo of array will be played, if the following demo is of same level, that will be performed, too, and go on for all demos with same level number.
The advantage to use this flag is to avoid the "load-new-level" phase followed by the "come-back-to-title-level" phase, since all single demos of that level will be played in sequence when their level has already been loaded.

## DEMF_PLAY_ON_KEY
Used in Demo script command.
Used in [Title] section.
If you wish that the player was able to launch himself the demos, choosing between different demos, you can add the DEMF_PLAY_ON_KEY flag.
In this situation you should also type a textinfo string to explain to the player what keys he can press.

Note: The keys are always digits, from 1 to 0, where 0 is for 10.
If you created demos in three levels of your adventure, you can use as textinfo string one like this:
"Hit "1","2", or "3" to show demos."
The number of the key is not for the position of Demo Id in arraydemo fields, and neither directly the Demo ID, but it is the level number whom a demo will be played.
If you had more than one demo for that (chosen) level, the choice will depends by the further DEMF_RANDOM or DEMF_PLAY_LEVEL_SEQUENCE flags.
As general rule, trng will avoid to show twice a demo already shown and it will give the precedence to demos not yet played.

## DEMF_QUIT_WITH_ESCAPE
Used in Demo script command.
If you wish allow to player the quitting of a demo while it's playing, you have to add the DEMF_QUIT_WITH_ESCAPE flag to DemoFlags, and when player will press Escape the current playing will be stopped and the controll will come back to the title level (if the demo was in [Title] section) or the controll will come back to player in common game (if the demo was in [Level] section)

## DEMF_RANDOM
Used in Demo script command.
Used in [Title] section.
If you wish that the demos (supposing you had more than a demo binary file) had no fixed order, i.e. the first demo to be played can be different for any game section, you can add the DEMF_RANDOM flag. In this situation has no relevance the order whom you type the Demo IDs in array demo fields, since this order will be changed randomly everytime the game will be launched.

Note: the DEMF_RANDOM flag doesn't work fine with DEMF_PLAY_LEVEL_SEQUENCE and DEMF_PLAY_ALL_SEQUENCE flags, since the contiguous same level number you set in the array it will be changed randomly.

## DEMO_FRAME
Used with SPC_PAUSE command of Parameters=PARAM_ACTOR_SPEECH command
This is a special flag that you can use only adding it to SPC_PAUSE command to force an absolute pause until current demo reaches the given frame.
See the SPC_PAUSE command for more infos.

## DENV_FLAG_FACE2BACK
Works in same way of DENV_FLAG_FACE2FACE flag but in this case the condition is true when lara is back of object. See description of DENV_FLAG_FACE2FACE flag for more informations.

## DENV_FLAG_FACE2FACE
Work togheter with ENV_ITEM_EXTRA... conditions, but you have to add this flag to DistanceForEnv field and not to field EnvCondition. You use it to require a specific couple of orienting (facing) for Lara and object to check.
For example if you want start your animation only when lara is face to face with SKELETON (slot id = 35) and distance is about half sector (512) you have to type:
In field ENV Condtion:  ENV_ITEM_EXTRA_IN_FRONT
In field Distance for env: 512 +DENV_FLAG_FACE2FACE
In field Extra : 35

Remark: the flags DENV_... are DistanceENV flags you can add to distance value to set some other settings.
About ENV_ITEM_EXTRA conditions you can add two flags DENV_ to Distance for env, to set the position about orientation of lara respect to object.
You can choose two DENV_ flags:

DENV_FLAG_FACE2FACE  (lara and object were looking face to face)
DENV_FLAG_FACE2BACK (lara is looking object while it gives the back to lara)

Remark: if you don't add any DENV_FLAG to distance, this means that the facing will be ignored.

## DGX_ADJUSTMENT_MODE
Used in DiagnosticType script command.
This DGX_ADJUSTMENT_MODE allows to you, to align some trng objects like Detector and Keypad to have correct coupling between object and text.
In some circustances, when the game screen has a resolution very high, it could born some misplacing between object and text or, for radar detector, the sprites of detected targets.
You should use this adjustment when you discovered some misalignemtn with a resolution higher than those already adjusted by Paolone.

It's better, in adjusting phase, let enabled only the DGX_ADJUSTMENT_MODE, so you should type following command in [Option] section of script file:

Diagnostic = ENABLED
DiagnosticType= DGX_ADJUSTMENT_MODE, 0

In game you'll see printed on screen many variables about position and text formatting of trng objects.

Keyboard commands in adjustment mode
------------------------------------
To choose the variable to change you use the:  [R] and [F] keys
To change the value of current selected variable use the keys: [Y] and [U] keys
Another keyboard command in adjustment mode is the [S] key. If you had launched the TOMB4_log utility (you find it in "tools" subfolder of trle folder), when you press [S] all current variables and values will be printed in log file.
You should always perform this operation at end of adjustment phase.
To locate in the log file the correct data, look for "ADJUSTMENT DATA" text, and copy and paste (to send to me ) all text between "adjustment data" and "end adjustment" in the last instance, for example:

   6985:   --------- ADJUSTMENT DATA (640 x 480) ---------
   6985:   RADAR DETECTOR:
   6985:   TargetX=1587
   6985:   TargetY=13669
   6985:   SizeTextX=128.00
   6985:   SizeTextY=170.67
   6985:   VLineX=2624
   6985:   VLineY=13715
   6985:   GapX=112
   6985:   GapY=149.33
   6985:   TextX=736
   6985:   TextY=15541
   6985:   COMPASS DETECTOR:
   6985:   VLineY=13845
   6985:   KEYPAD:
   6985:   OrgX=11056
   6985:   OrgY=17685
   6985:   TextY=2304
   6985:   --------- END ADJUSTMENT ----------

Remember to enclose alwayas also first line because I need to know the resolution used in game during your adjustment.

Remarks:

* The adjustment mode allows only temporary to fix the problem, at next game restart the values will come back to original (bad) values.
The adjustemtn should be done with the final target to send the correct adjusted value to me (Paolone) and I'll insert this new best values in next version of trng dll.
The reason that I've not performed myself this job is because to have a correct adjustment it's necessary set the game resolution to some high value but I can reach only 1024 x 768 pixles with my notebook and only a bit better (1280 x 1024) with my desktop.

So, if you have a most powerfull computer with an higher resolution you can create adjusted values for these extra reolutions those I cann't reach with my current hardware.

* In radar detector section you could change a lot of value but, pratically, it's advisable change only these three values:

TargetY  (Set the y position of detected objects on the main (planar) panel)
VLineY (Set the y position of detected objects on the secondary (vertical) panel)
TextY  (Set the y position of text (distance XX meters))

## DGX_ANIMATION
Used in DiagnosticType script command.
Show the debug data about Animation script command.
An Animation command works in debug mode when you type as animation number a negative number. In this situation the diagnostic will show if the conditions to enable the animation are corrects, and when this is true it will be showed "YES", while in opposite case "no".

## DGX_AUDIO_TRACKS
Used in DiagnosticType script command.
It enables the viewing of infos about audio tracks: the enabled channel and number of cd is playing.

## DGX_CHEATS
Used in DiagnosticType script command.
Show current typed keystrokes to get more easy set some trng cheats.
The cheats allowed in trng are based all on four-chars words:
KILL (kill all enemies)
ROOM (reverse all flip ROOMs)
IAIR (Infinite AIR)
GODS (set Lara  as invulnerable i.e she becomes as the semiGODS)
DOOR (open all DOORs of the level)
STAR (gives to lara a constant like and she brights like a STAR)

Remark: the cheats work everytime there is the diagnostic mode enabled (Diagnostic=ENABLED) and the FlyCheat=ENABLED, indifferently if you enable the viewing of them with DGX_CHEATS

## DGX_COMMON_VARIABLES
Used in DiagnosticType script command.
Show on screen the values of trng variables.
This flag enables the viewing for common variable, i.e. all variables less the Store variables.

## DGX_ERRORS
Used in DiagnosticType script command.
By default the trng engine is able to give many error messages when it detects some problem with script commands, wrong indices of moveables (in some trigger) and other directx stuff.
Unfortuntaley the most of level designers ignore them because to see these messages they should launch the tomb4_log.exe program (in the "tools" subfolder of trle) and then, at end of the game, show the log that has been caught from the logger.
Perhaps it's a bit boring this procedure, anyway now you can in easier way having the error messages showed on game display in real time if you add the DGX_ERRORS flag to the DiagnosticType command.

Note: since in the game session the errors could be so much to cover all the screen, trng uses a buffering method: it will keep on the screen an error messages only for five seconds, and more, it will ignore when a new error message is the same it had already printed on the screen in the past.

At end of the game, you'll find also a text file named "error_parsing_log.txt" in the trle folder, with all error messages.

If, at end of the playing, you don't find any "error_parsing_log.txt" file this means there have been no errors, of course.

Remark: some error messages could be very long. To remain all in the screen you should use very little font.
Anyway in the error_parsing_log file you'll fine all errors with no truncation.

## DGX_FAR_VIEW
Used in DiagnosticType script command.
This flag enables the viewing of current world far view distance applied in game.
Remark: this info requires also to have in current level a Turbo script command using adaptive far view, otherwise it will be no showed.

## DGX_FLYBY
Used in DiagnosticType script command.
Enable the info about current flyby sequence.
Remark: the info will be showed only while a flyby sequence is running.

## DGX_FOG
Used in DiagnosticType script command.
Show infos about Fog Distance: start fog, end fog and color fog.
Remark: to see these inos, other to set the DGX_FOG flag it's also necessary that in current [Level] section there is a FogRange= command.

## DGX_FPS
Used in DiagnosticType script command.
Show the current FPS (frame per second).
Note: it will be showed a couple of value: the first is the async frame rate while the second (in round parenthesis) is the sync frame rate.
Usually these two fps variales will have (about) the same value but if you use the Turbo script command with the TRB_ASYNC_FRAMES setting, the async frame might be higher than sync frame since the turbo command will try to compensate the lost of syncronous frame increasing the speed of new frame to recovery the previous losing.

## DGX_LARA
Used in DiagnosticType script command.
Show in diagnostic the info about Lara: coordinates, orienting, room, flags

## DGX_LOG_SCRIPT_COMMANDS
Used in DiagnosticType script command.
This diagnostic mode works in a different way respect other DGX_ modes.
It doesn't show anything on screen but it creates a full log you can see using TOMB4_LOG.exe utilty. You can find this utility in trle\Tools folder.
This diagnostic mode pratically enables a debug mode about GlobalTrigger, Organizer and TriggerGroup commands, showing all further variables changed by above commands.
It's advisable works with tomb raider in windowed mode to have the chance to see immediatly the tomb4_log window with last mexages. Anyway it's not required, you can also consult the full log at end of tomb4 running, using the command of tomb4_log program [Show/Last tomb.log]

Tips & Tricks:
1) You can add in Extra field of DiagnosticType comamnd the constant EDGX_CONCISE_SCRIPT_LOG to reduce the quantity of mexages in the log.
See the description of EDGX_CONCISE_SCRIPT_LOG constant for more infos.

2) You can use function Key F9 to suspend/resume the log about script comamands. You could use this feature to suspend temporary the log when lara is yet very far from the "zone" you wish check, and then reumse the log when you are ready. This trick is useful to reduce the size of log file and get more easy find the correct portion of log you are interested.

3) The debug mode of script could slow down the game, so remember to disable it when you want play normally.

## DGX_SFX_SOUNDS
Used in DiagnosticType script command.
It enables the viewing on screen of current sound sample played and infos about further missing sound sample.

## DGX_STORE_VARIABLES
Used in DiagnosticType script command.
Show on screen the values of all Store variables.

## DGX_TEXT_VARIABLES
Used in DiagnosticType script command.
Show on screen the content of testual variables.

## DGX_WEAPON_ANIMATION
Used in DiagnosticType script command.
Adding this flag you'll have on screen the infos about current weapon animation.
The animations and state id for weapon management are not visible in lara's infos because the weapon works on specific slots like SHOTGUN_ANIM, GRENADE_GUN_ANIM ecc.

## DIR_DIRECTION_LARA_LEADING_ACTOR
Used in Parameters=PARAM_MOVE_ITEM script command
The direction will be the line from lara versus the leading actor.
Please try to understand that this is different than DIR_LARA_FACING since in the case of DIR_DIRECTION_LARA_LEADING_ACTOR flag has no importance where lara is looking but lara and the leading actor will be used as two points where draw the direction.
Note: remember you can always add the DIR_INVERT_DIRECTION flag to have the opposite direction, and in this case, the direction from leading actor to lara.

## DIR_DOWN
absolute moving down

## DIR_EAST
Absolute moving to east.

## DIR_FORWARD
Used in Parameters=PARAM_MOVE_ITEM script command
Relative direction.
This flag will move the item forward respect its current facing. For instance if the item is looking to north the item will move to north but this is true for any intermediate direction.

## DIR_HEAD_FOR_EXTRA_ACTOR
Used in Parameters=PARAM_MOVE_ITEM script command
Direction from current position of item to move and the position of extra actor

## DIR_HEAD_FOR_LARA
Used in Parameters=PARAM_MOVE_ITEM script command
This direction is given from the current position of item to be moved and the position of lara as ending position.

## DIR_HEAD_FOR_LEADING_ACTOR
Used in Parameters=PARAM_MOVE_ITEM script command
Direction from current position of item to move and the position of leading actor.

## DIR_INVERT_DIRECTION
Used in Parameters=PARAM_MOVE_ITEM script command
This is a flag that you add to any DIR_ direction value.
When you add this flag a relative direction with turning, you the same kind of turning but with opposite forward movememnt.
For instance the pair:
DIR_TURNING_RIGHT_90+DIR_INVERT_DIRECTION
it will move the item backward, turning at its right.

## DIR_LARA_FACING
Used in Parameters=PARAM_MOVE_ITEM script command
The direction will be the same where lara is looking.
This direction could be useful when lara throw away an item (or bullet) in front of her.

## DIR_LEADING_ACTOR_FACING
Used in Parameters=PARAM_MOVE_ITEM script command
This direction will be the same where the leading actor is looking. This maybe useful when the leading actor throw this item in front of him.
Note: remember you can set an enemy as leading actor with the Action 87.

## DIR_LU_TURNING_180
Used in Parameters=PARAM_MOVE_ITEM script command
Relative direction with turning.
This flag will move forward the item but changing continuosly its facing, turning at left, until it will have the opposite facing of start.
See description of DIR_TURNING_LEFT_90 about customization of turning speed.

## DIR_NORTH
Absolute moving to north.

## DIR_RU_TURNING_180
Used in Parameters=PARAM_MOVE_ITEM script command
Relative direction with turning.
This flag will move forward the item but changing continuosly its facing, turning at right, until it will have the opposite facing of start.
See description of DIR_TURNING_LEFT_90 about customization of turning speed.

## DIR_SOUTH
Absolute moving to south.

## DIR_TURNING_LEFT_45
Used in Parameters=PARAM_MOVE_ITEM script command
Relative direction with turning.
This flag works like DIR_TURNING_LEFT_90 flag but in this case the turning will be only by 45 degrees.
See description of DIR_TURNING_LEFT_90 flag for more infos.

## DIR_TURNING_LEFT_90
Used in Parameters=PARAM_MOVE_ITEM script command
Relative direction with turning.
Setting this flag the item will move forward but its facing will be changed smootly until to be turned by 90 degrees respect its forward movement. With this setting you can move an item to do circumvent a corner of 90 degress that is at his left.
You can set in Extra field the turning speed typing a value that will be added for each frame to current item facing.
The units work in this way:
45 degrees = $2000
90 degrees = $4000
You can set any intermediate value, of course and, by other hand, it's better setting little values since the value you typed in Extra field will be added to current facing 30 times for second. Reasonable values are in the range 32 / 1024

Note: set only positive values, please. The direction of turning will be set by the given DIR_ value direction. When you set a left turning the turning speed (you typed) it will be subtracted to current facing of item, but you have to type only positive values.

## DIR_TURNING_RIGHT_45
Used in Parameters=PARAM_MOVE_ITEM script command
Relative direction with turning.
This flag move forward the item changing its facing until to turn it by 45 degrees respect its orginal facing.
See description of DIR_TURNING_LEFT_90 about customization of turning speed.

## DIR_TURNING_RIGHT_90
Used in Parameters=PARAM_MOVE_ITEM script command
Relative direction with turning.
This flag move forward the item changing its facing until to turn it by 90 degrees respect its orginal facing.
See description of DIR_TURNING_LEFT_90 about customization of turning speed.

## DIR_UP
Absolute moving up

## DIR_WEST
absolute moving to west

## DISABLED
## DMG_ALERT_BEEP
To use only with DMG_INDIRECT_BAR  flag.
If you set this flag, when the bar will be go down until 15 % (or less) than full value, the bar will blink and a little "beep" sound will be performed togheter with blinking.
Remark: Also when this flag is absent the bar will blink but no sound will be performed.

## DMG_BURNING_DEATH
When the time of damage bar will be completed, lara will be killed burning her with real fire.
To use only for damage rooms waterless.
If this flag is enabled, lara will burns when:
The indirect bar is empty (if indirect bar is present)
The HP bar is empty  (if you don't enabled indirect bar for damage room)
You could use this flag to simulate a room where the temperature grows until burning lara.

## DMG_BURNING_SCREAM
To use only with DMG_BURNING_DEATH flag.
If you set the DMG_BURNING_DEATH flag you can set also this flag to force lara to scream when she takes fire.
Remark: this flag works only with indirect bar, because when indirect bar is not set, lara burns and dies immediatly and so she has no time to scream.

## DMG_COLD_WATER
Specify you are using current command to set the default behavior of all cold water rooms in current level.

## DMG_INCREASE_BAR
By default, the damage bar (showed only using DMG_INDIRECT_BAR) starts full and decrease it until to empty, and when reach the empty status lara will be damaged very fastly.
If you want change this behavior to have a progress bar that it grows, starting from empty to reach full, you must use this flag.

## DMG_INDIRECT_BAR
If you want have a progress bar on screen to signal the level of current damage in current room it's necessary to use always this flag.
With indirect bar the vitality of lara reamins untouched until the indirect bar become empty (or full if you set flag DMG_INCREASE_BAR)

## DMG_LITTLE_TEXT
When you use a BarName in your Damage= command, you can choose this flag to to use a little text to show the string with bar name.

## DMG_ONLY_PAD
Lara damaged only while her feet touch the floor of current room. To use for simulation of electric floor or very hot floor.

## DMG_POISON_LARA
Non just lara enters in current room she will be poisoned with deforming effect of screen and life bar with yellow color.

## DMG_SLOW_DISAPPEARING
Set the behavior of damage bar when lara goes off by damage room to reach a safe room. If you omit this flag the damage bar will disappear instantanly, while using the flag DMG_SLOW_DISAPPEARING the damage bar will invert its direction of fulling/emptying and it will disappear only when it reach the full state (like at start of enter-in in damage room)

## DRT_ADD_EFFECT
Used in Customize=CUST_DARTS command
I you wish, you can add some effect for each dart
When you use this flag you have to type in IdAddEffect field the ID of AddEffect script command where you set the effect to add to the dart.

Remarks:
- Warning, this feature could be very cpu intensive. If you have many dart emitters in your level my suggestion is to disable the emitter already visited by Lara, using a Untrigger to free the resource for those no more used dart-emitters.
Another suggestion is to set a big Emitting timer (to slow down the emitting) and/or an high Speed value to avoid having too much darts enabled in same time.

- In the AddEffect command, pointed by IdAddEffect field, some fields will be ignored: DurateEmit and DuratePause, while other fields should have always same value: the JointType should be always set to JOINT_SINGLE_MESH value.
Pratically the engine will be interested only by the type of effect while for the  emitting time it   will be constant for the life-time of the dart.

## DRT_FIX_POISON_BUG
Used in Customize=CUST_DARTS command
In default Tomb4 there was a bug about poisoning with darts.
While the poison of scorpion and harpies worked correctly with the screen deformation and lara losing HP, the poison of dart remained only an instant and it was immediatly removed.
If you want fix this bug to get a real poisoning of lara just you add the DRT_FIX_POISON_BUG flag.

## DRT_HIDE_DART
Used in Customize=CUST_DARTS command
This flag get fully invisible the dart. The only case where you could use this setting is when you have added some effect to darts with the DRT_ADD_EFFECT flag and you wish that only the effect (like mist or fire) was visible.
This setting helps also to free the cpu of a lot of 3d graphic operations.

## DRT_NO_POISON
Used in Customize=CUST_DARTS command
By default when lara will be hit by a dart she will be poisoned.
You can use the DRT_NO_POISON to disable this feature.

## DRT_NO_SMOKE
Used in Customize=CUST_DARTS command
This flag remove the smoke when dart hit the wall and also the sound of the crash.
You should use this flag when you want give to dart the look of ray laser.

## DRT_PERFOM_TRIGGERGROUP
Used in Customize=CUST_DARTS command
If you add this flag you can transform the dart emitter in a sort of sensor line, where, when a dart touches lara, it will be perfomed some trigger.
Pratically it could be like an electronic alarm. In this case you have to type in IdTriggerGroup field the ID of trigger group to perform.

Remarks:

- You can reduce the speed and change the color (for example with red or green) to give to the dart emitting the look of a laser ray.

- If you use the dart emitter like a sensory line is advisable disable the poison with the DRT_NO_POISON flag, since it has no sense the poisoning performed by a laser ray and also the smoke and crash sound wiht the DRT_NO_SMOKE flag.

- When some dart touches Lara and the triggergroup will be performed the index of the dart emitter will be stored in the internal variable [Found item index]. In this way you could insert in the trigger group also an action trigger using the TGROUP_USE_FOUND_ITEM_INDEX flag, to disable that dart emitter with this action trigger:

; Exporting: TRIGGER(44:0) for ACTION(0)
; <#
 : LARA                       ID:0      in sector:(8,7) of Room3
; <&
 : Trigger. (Moveable) Untrigger <#
Object with (E)Timer value
; (E) : Timer= +00
; Values to add in script command: $5000, 0, $2C

and the values will be added in triggergroup command in this way:

TriggerGroup=  ....  ,$5000 + TGROUP_USE_FOUND_ITEM_INDEX, 0, $2C

to use the found index instead of that set in original trigger (it was lara)

- The best way to have a very realistic laser sensory line avoiding in same time to slow down the engine is to disable the showing of darts with the DRT_HIDE_DART flag to free cpu time.
Disable smoke and crash sounds with the DRT_NO_SMOKE.
Disable also the poisoning with DRT_NO_POISON flag.
Now we should decrease furhter the emittingtimer of a bit to have most invisible dart showed. This is necessary to avoid that Lara was able to pass our sensory line without touching the darts.
Now the only problem is that the sensory line is fully invisible. We can solve this problem using a static object with no collision builded like the tight-rope bar, and adding to it the same texture used for waterfall to give to it a pulse/flushing effect.
Then in NGLE just place the dart emitting in some direction and hight and then place also the static sensory bar in same position where the darts should be emitted to have many pulsing semi-transparent laser sensory in our very hi-tech level. Each of this sensory line could be used like trigger to activate other stuff like enemys, doors, flipmaps ect.

## DTF_ENGAGE_ALWAYS
the detector will be always present on screen, at least until some of targets exits yet in the level.

## DTF_ENGAGE_INVENTORY
Lara has to pick up the detector object and to engage it it's necessary go to in inventory and select it

## DTF_ENGAGE_IN_RANGE
the detector will be automatically showed when lara is closed to some target, within the given range (see following fields)
With engage_in_range you can set for example 50 meters and the detector will be showed when lara is to 50 meter or less from nearest target.
When lara will be more far the detector will be hidden.

## DTF_FAST_RADAR_SCAN
The pointer will turn very fast

## DTF_INVERSE_VPOINTER
Current flag works only in pointer mode.
You can invert the position of line in vertical scale (at right of detector)
By default (when you DON'T use this flag) the floating line shows where is lara, while the fixed red pointed line at center shows the position of target.
If you think it's not logical this relation you can use this flag to invert the situation. With the DTF_INVERSE_VPOINTER flag the floating point is the vertical position of target, while the fixed red line is the vertical position of Lara.

## DTF_NONE
## DTF_RADAR_MODE
This flag set the detector in radar mode. If you omit to set this flag the detector will work in Pointer mode.
There are big differences between two modes and some fields and flags will work only for a specific detector mode.
In pointer mode the detector has a pointer like a compass that shows where is the current target (current target is the first target in the list yet present in game).
Pointer mode works only on one target at time: when the first target has been picked up (or killed) the detector will start to point second target in the list and go on.
In radar mode detector is able to scan all targets in same moment just they are in the range of radar.
Note: Remember that the range of radar is different of range for activation. The range of radar is given by formula 6 * MetricScale, where the digit "6" is the (fixed) number of grid sectors of radar. For example if you set 2 meter for metric scale the target will be showed on radar only when it is to 12 meters from lara or less.
To understand radar mode is bit more difficultous than pointer mode, because the target in radar mode will be showed always with up side = north, while in pointer mode the pointer is always relative to where lara is looking. In fact, in pointer mode, when the pointer is on red sign this means lara is looking in correct direction of target, while in radar mode it's advisable that lara looks to north to understand if targerts are at her left or right.

## DTF_REQUIRED_ITEM
Detector will work only if lara picked up it, or she has it from start of level in inventory

## DTF_SWINGING_POINTER
This flag works only in Pointer mode.
By default the pointer points exactly to target, anyway if you want you can add a swinging simulation of pointer like the compass in inventory to get more realistic detector.

## EDGX_ANIMATION_SLOT
Used in DiagnosticType script command.
Adding this flag you can have on screen a signal when the condition for animationslot with negative AnimIndex is true or false.
This flag works in same way of DGX_ANIMATION flag.
You have to type an animationslot command where the AnimIndex field will have a negative value. In this way the animation will be not performed but it will be tested the conditions and when they will be true or false the result will be drawn on screen.

## EDGX_CONCISE_SCRIPT_LOG
Used in DiagnosticType script command.
You can add this flag in Extra field of DiagnosticType command to create a shorter debugging log.
When you omit this flag the log will be very large, with a lot of mexages about false conditions, frame for frame.
If you find too chaotic this full log you can create a concise log with EDGX_CONCISE_SCRIPT_LOG flag.

## EDGX_CUTSCENE_LOG
Used in DiagnosticType script command.
You can enable the creation of a log about cutscenes.
The log will be saved in trle folder with name "custscene_log.txt" and it will give infos about:

- Demo game commands of current demo.pak is playing. The game commands are  like "action", "jump", "left", "right" ect
- Speech commands of currently enabled PARAM_ACTOR_SPEECH script command.
- Organizer commands linked with current demo (only for organizer with FO_DEMO_ORGANIZER flag)

## EDGX_LARA_CORD_IN_LOG
Used in DiagnosticType script command.
This extra flag works only when you enabled the DGX_LARA to have Lara's data (animations, stateids) on screen.
If you add the EDGX_LARA_CORD_IN_LOG in ExtraDgxFlas of DiangosticType command, in the log will be showed the coordinates of Lara, too.

Note: by default the coordinates were missing in the log to reduce its size.

## EDGX_RECORDING_DEMO
Used in DiagnosticType script command.
This flag enables in game the demo recorder.
You can read the available keyboard commands on the screen, anyway it's better if you read following notes and hints.

- The max durate for recording is about 25 minutes. If you reach this limit the recording will be stopped byself.

- In first row of Recorder there will be the name of demo#.pak file currently active. If this name has a # sign in front, it means that this demo#.pak file exists really on disk.
You should take care that when you enable the recording with a demo file with the # sign in front, you are going to overwrite it.

- You can change the current demo#.pak file active, using the keys "Q" and "W".
With "Q" you reduce by 1 the current Demo index, while with "W" you increase this index.
Valid range is between 1/999

- Editing of demo files includes a simple backup method. Everytime you erase current demo.pak, using the "e" (erase) command, trng will rename current demo#.pak name as demo#.backup file. In this way the demo#.pak is no more present (erased) but its previous content will be saved in corresponding demo#.backup file.
A backup file will be created also when you begin a new (fresh) recording while it was selected an already existing demo#.pak file.
If you wish restore the backup file and to do became it the main demo.pak file (cancelling last erase/recording overwrite operation) you can use the "r" (restore) command.

- In the demo editing infos on screen you can see also the indices of currently selected Leading actor ("L-actor) or Extra Actor ("E-actor").
If you don't see above descriptions it means that you have not assigned a role for that (missing) actor role.

- If you added to the script and organizer linked with the demo you are recording or playing, it will be performed also in editing mode. Anyway you can disable it (but only in editing mode) with F4 key.

- At start, the game will move on higher free demo index. This means you can record a new demo file avoiding overwritting.
If you wish play some demo file from disk, you can use Q/W keys to choose the demo file and then hit F10 key.
If when you choose an existing demo file (you discover this because it has the # sign in front of demo name), but the info about "F10 to play the demo" is missing, this means that demo has not been recorded in current level and for this reason it's neither possible playing it in current level.

- When you are playing a demo file, you can stop hit with Escape key or trimming it with F11 or F12 keys.

While Escape stops only the playing with no change on current demo, the f11 key will remove the part from the begin of the demo until now (when you hit F11 key), while the F12 key will stop the play and also it will truncate the demo at the moment when you hit F12 key.
Therefor you can use F11 and F12 key (on playing) to edit a demo file, removing beginning or ending part from it.

Note: if the Escape command doesn't work, it happens because you typed a Demo script command in same level section where you are playing the demo.pak file, and in the demo= script command is missing the DEMF_QUIT_WITH_ESCAPE flag.
While you are yet editing demo.pak files in game, it's better add that flag to demo script command, or remove temporary the demo script command from the script with a semicolon ";" character.

- While playing is in progress you can pause the playing, freezing the game keeping down the F8 key.
This is useful to read the current frame number. Then you can use this info with a demo organizer or to set a condition C86 or C87 to perform some trigger only when demo is playing exactly that frame to have a better syncronization for special effects.
Note: everytime you hit F8 key to stop demo, the number of that frame will be saved to a log file "cutscene_log.txt" in trle folder. In this way you can stop in different moment and then read all stop-point frames.

Note: about the choice to use C86/87 conditions on the path of lara, or to use a demo organizer (an organizer with the FO_DEMO_ORGANIZER flag) syncronized with the demo, it depends by the number of trigger to perform in the progress of the demo. If you have only one or two triggers it�s more easy using C86/87 condition. While when the triggers are much more it�s better using a demo organizer.

- The F9 key allows the so called "Add recording" operation.
With this operation you continue the recording of current demo, after a previous recording or playing phase.
Note that is not always possible having the add recording: if you have just selected (with q/w keys) a demo, the add recording will be not possibile until you don't play current demo, in fact, it's necessary that lara was in the final position of current demo to continue the recording from final position of it.
If you wish replace the second part of a demo with a new recording (with f9 key), you have to play the demo and then stop it with f12 or escape key. After this command, the following f9 key will add a recording after the previous stop point, overwritting the (previous) final part.

Note: the add recording operation maybe useful in two circustances:
* When you had recorded a long and complicated demo and you wish preserve first part but, in same time, replace the final part to correct something of it.
* When you wish add many special effects to your cutscene/demo but you need to know the position of frame where set these trigger before continuing. In this case you'll record a part of the demo, then discover the frame where add a trigger, then repeat the demo, and at end, with f9 key you'll continue the recording having the trigger already placed to be able to interact with new game scene changed by above trigger.

Remark: when you stop a demo with the project of continue the recording in a second moment, you should choose a stop point where lara is still, otherwise the final demo playing will have problems of synchronization.
For instance you can not stop a recording while lara  is falling down in the empty, because when you stop del demo, lara will continue to fall down anyway, and when you'll add the recording the position of lara will be no the real final position of previous part of recording.
Alike speech when lara is running or performing other actions that she will continue to do for a bit, when you stop the playing.

- Remember that not always the playing of the demo just recorded can works fine. It happens when in the recording phase, lara changed some items in the game that will be different (missing/killed, moved, already enabled ect) for this reason, in following playing.
For instance if lara, in recording phase, enables an enemy and then she kills him, when you perform the play the enemy will be missing, since he has already been enabled and kiled.
To avoid this problem it's better saving the game before beginning a recording, in this way when you wish play the demo just reloading the savegame to come back to previous game situation and then hit F10 key to play the demo from same initial status.

- Another situation where the playing could be different than recording is when lara interacts with enemies. Since there are some random actions in some enemies, these different movement could affect the behavior of lara (when she is aiming the enemy) or when an enemy touches lara, moving (or turning) a bit her.
For this reason the more sure demos are those where lara has no enemy to aim and no enemies that touches her.

- The demo files will be saved/loaded from Data folder. In data folder you can also find a DemoInfos.txt file with a descriptive list of demo#.pak files stored in Data folder.

- There are some limitations about beginning position of lara at moment of recording.
Lara cann't be hanged on rope (and probably, pole-rope), and neither to be driving a vehicle.
It's not possible begin the recording from inventory or other "paused" screen.
Anyway, in spite of these limitation, you can to do to lara all above actions but only in the middle of the recording.
This means you can begin with lara in stand-up position and then move her to hang on pole-rope, or drive the jeep, or enter in inventory to choose an item ect.
Theorically it should be possible beginning a recording while lara is standing-up, climbing, monkeying, running, jumping, shooting, falling, floating on water or swimming underwater.
Differently, it may be problematic beginning with lara is walking (SHIFT+Arrows) because the mode to detect down/release of shift key is very particular and beginning with shift key down it will take weird troubles.

- About middle phase of recording you should avoid only to load savegames since a demo should work only within a level environment.

- The demo recorder is able to record only the standard game commands, those you can see in Option/Control Configuration screen, plus the digits between 0-9.
For this reason if you use custom scancode to perform special animation or other new features, these will be not recorded and then played.

You have also to use Action (CTRL) key to convalidate the numbers and not the [Enter/Return] key.

- Expiration of a demo file.
Probably you'll record the demos on final version of your levels, anyway in the case you changed something after having recorded a demo, you have to learn when these changes could get troubles for demo playing.

*You should avoid the change of equipment of inventory items when in the demo lara choose some item from inventory, because with some missing or more item, the movement to select correct item could change. In this situation a trick could be to record the demo when the selected item (first item selected in inventory) is own that lara will choose. In this way it will work also adding or removing items from inventory.

*It may be dangerous change lara object in wad file, adding or removing animations, frames or meshes.

*Do not change geometry of room where lara will move in the demo, of course, anyway it's not serious if you add new rooms or change textures, triggers, sounds or other non-collisional stuff.

- Please, remember the fundamenal improvement of C25 condition trigger: now there is the new mode "Demo/Custscene mode" for this condition

Thank to this condition you can place trigger (casted with above C25 condition) that will be enabled only in demo mode.
This could be useful to improve your demo/cutscene with many triggers that will be no triggered in common game mode, also if lara will come back in that room where you do pass her in the demo.
Another important benefit is that you can use these triggers (working only in demo mode) to prepare the demo with all correct enemey enabled or door opened or weapon/pickups got, ect.

For instance...

If you wish show a demo where lara fights with the final monster of your level (reached after a long path) and she is using a new weapon (that in common game will be get only after picking up many sub-items) you need for your demo to have already lara closed to the monster place, with the special weapon in inventory and with the monster already enabled.

It's important understand that the demo doesn't save all features of the game but only the lara position+status and her following movements, given from input control.

So, since (for a demo from title) lara will begin when the level is at start, she (in above example) will have NO special weapon, yet, and she will be very very far from the place with monster. About the distance is not a problem, since if you begin the recording when lara is closed to the montster room, when the demo it will be played, lara will be moved immediatly in that position.

While to solve other problems, you have to place in the sector where you begin the recording, the (C25 casted) triggers to add in inventory the special weapon and to enable (trigger) the monster.
You'll record only from this sector, closed to the monster, where lara will trigger the adding of special weapon and the monster.
All above triggers will be casted with C25 condition to be enabled ONLY in demo mode. In this way they will not affect the game playing when lara will move over them in common game.
By other hand it's necessary understand that above triggers have to be present also in final release of your level, otherwise the demo will not work. For this reason you cann't simply change temporary your level (adding triggers), to record the demo, and then remove these triggers.
To reconstruct the situation that lara should have in different moment and sides of your level, you have to place and let there, many c25 triggers that will reproduce in fast way equipment and situation that in common playing should require a lot of time to be gotten.

- Tips and tricks: Adding different view of Lara with fixed camera, flybycamera or maxtrix or potrait effect improve your demo, anyway it's complicate recording the demo and move lara correctly when you have not a normal view of the game.
The trick to solve this problem is simply to add in a second moment the triggers (with c25 condition) to enable different viewing of the game. In this way you'll record (and drive lara) with a common view to avoid errors, while after you completed the recording you place c25+camera trigger in the path of lara in the demo, and in this way in final playing you'll have many suggestive camera views.

- There is also the new flipeffect (F379) "Cutscene. Perform the demo.pak with [&index] in (E)way" to launch a demo from a [Level] section to do work it like a cutscene.
Also to create cutscene the C25 casted triggers are very important. To change camera view, setting animating with special custom animations, adding texts and new background sound or music, ect.
See above point about C25 condition for more infos.

## EDGX_SLOW_MOTION
Used in DiagnosticType script command.
You can use this flag with any combination of DGX_ diagnostic type.
When you enable the slow motion you can slow down the frame rate in game using the F11 key like switch.
Press F11 first time to enable the slow-motion.
If you wish have an extreme slow motion just press different time the F11 key.
To disable the slow-motion you have to hit ESCape key.

The slow motion is useful when you are studying some custom animation or other dynamic movements and you wish have all details about this action.

## EDGX_SWAP_VIEW
Used in DiagnosticType script command.
With this flag you enable a change of view when you press the F12 key.
The view is about the direction from that Lara will be looked.
This feature is useful when you wish study some animation or mesh of Lara but this is not visible from back view.

## EDGX_TRIGGER_TIMING
Used in DiagnosticType script command.
The trigger timing will show on screen the distance in frame ticks between a trigger activation and the following.
This could be useful to syncronize better cutscenes with organizer in the script.
Another interesting usage is to convert old method of rolling ball (to enable other trigger in sequence) with an organizer.
Organizer will reduce the used triggers on the map, moving them to the script.
The time you'll see on screen will be only the tick frames elapsed by last trigger activation.

## EF_DOUBLE_DOOR
Used in Elevator command.
Please don't confuse this flag with flags like EF_SINGLE_DOOR or EF_MULTI_DOORS.
While those flags say the number of doors to use, if a single door to move up/down with elevator, or multiple door, one for each floor, the double door specify the TYPE of a single (or multi) door, and then this door could be a single_door or a multi-door.
The double door is a door composed by two (separated) doors.
For this reason the double door is able to cover two sectors of width, like the width of elevator.
The reason to use this flag is to have two different door opening in same moment when the elevator reach a floor, to be able to cover whole width of elevator.
This flag has been added own to get above target and it requires some attemption to work correctly.
If you set the EF_DOUBLE_DOOR flag, it's necessary that the secondary door had always a ngle index higher by 1 than main door.

I explain better:

For "main" door we intend the door that has the index you type in Elevator= command.
While the "secondary" door is the other door (paired to main door) to open/close in same moment of main door.
This secondary door has to have an index exaclty +1 of main door.
For example if your main door at first floor (in a multi-door elevator) has index = 23, the secondary door should have an index  = 24 (+1 of main door).
If this rule will be violated the double door will not work in game.

If you have problems to get two consecutives indices you could use this simple trick:
Place a door in some free zone of your room, this zone is not in front of elevator, it will be only a "store" temporary zone.

Continue to place doors in this zone, checking the index assigned from ngle for each new door.
When you get two doors with consecutive indices, delete these two doors, and now place the two doors in correct position in front of elevator. In this moment you can place also other double doors for other floors of elevator (if you are using multi-door elevator).
When you completed the placement of the double doors you can come back to temporary zone and delete all no useful doors you placed to realize this trick.

Remark: if you are using like "single-door" an animating door (just place like door any animating with correct collisions) the behavior of this animating door will be different in according with usage or less of the EF_DOUBLE_DOOR flag.
If the EF_DOUBLE_DOOR flag is missing the animating door will be moved (sliding) in internal direction, i.e. the door will be opened moving the door over other panel of elevator.
Differently, when you use EF_DOUBLE_DOOR flag, the two door will be opened with external movement: the left door will be moved furtherly to left,  while the right door will be moved at its right.
Also for animating doors you have to use the rule of consecutives indices: the secondary door should have the index of main door + 1.
In the case of animating double doors it's also important that the two doors should have same orienting (facing).
Another rule about animating doors is that the door you have to set as main door (the index you type in Elevator command), it should be always the door at left of elevator, looking the elevator from door side. This last rule it's not important when you use real doors.

## EF_INNER_KEYPAD
If you want insert the keypad to choose flor inside the elevator you must set this flag and (in ngle) to place the keypad oriented on wished elevator wall. Then you'll type the index of keypad in InnerKeyPadIndex field.
Remember to use the "fake" keypad you find in Animating16_mip of ng.wad or ng2.wad and not the real keypad in switch_type1.
The reason of this choice is given by a technical problem: the switch_type requires a switch trigger to work, but you elevator floor you have to place also dummy triggers and for this reason it's not possbile place both special trigger in some sectors.
For this reason the management of "fake" keypad (of Animating16_mip) will be performed in hardcoded mode by engine.
Apparentrly the keypad will work in same way of switch_type1 but with no need of special trigger to handle it.

## EF_MODE_STOP_AND_GO
Stop and go mode is similar to yo yo mode, but in this case the elevator stop and stay for some second (2 for elevator door less, 3 seconds for elevator with doors) in each floor.
Differently by yo yo mode, using stop and go mode you can place the doors (multi or single) and you can set a various number of floors.
Also stop and go mode may heart lara (like all elevators)

## EF_MODE_YO_YO
Yo yo mode is a bit curious: elevator will move endless up and down, without any stop at floors.
Using this mode you cann't use doors, since there is no time to open and close them.
Lara will have to jump at fly in elevator.
An interesting application of yo-yo mode is to give to elevators also the role of trap, since if lara will be touched by elevator she will be killed (if she is under elevator moving down, or over elevator when elevator moves up and above lara there is the ceiling) or "disturbed" if she is climbing the wall when elevator touch her (lara will be untouched by wall)
Remarks:
1) You cann't mix yo yo mode and stop and go mode.
2) If you set elevator for yo you mode the number of floor will be always two (2) and the distance between floor will be the height of whole movement that elevator will cover before inverting direction.

## EF_MULTI_DOORS
If you want that trng engine handle for you all doors in front any floor you can set this flag and then set in IndexFirstDoor the index of door at first floor.
The management of game engine is very simple: when elevator reach a floor the door of that floor will be opened.
When elevator leaves a floor the door in that floor will be closed.
If you use this multi-doors handling is important you place all doors in your project exaclty at same vertical distance you set in ClickFloorDistance field. It's important, because is you place some door to different distance that door will be not found by trng engine and you'll have troubles.
Pratically just you place each door at same height where the elevator will stop it, floor for floor.

## EF_NONE
## EF_SINGLE_DOOR
If you use single door mode you cann't set also flag multi-door.
In single door mode the elevator has a single door linked with elevator cage.
This door will be moved up and down in according with elevator movement.
When elevator reach a flor the door will be opened, when elevator starts from a floor the door will be closed.
If you set this flag you have to type in FirstDoorIndex field the index for the door you placed near to elevator.
Remark: you can use also a "fake" door you find in slot Animating2 in ng2.wad
This animating will be moved, sliding it horizzontally, like a door by trng engine if you use single-door mode and you set its index in FirstDoorIndex field.
There is a reason to use a fake door like this: when you use a real door,in fact, in some circustances (it depends by door type and by position of door respect to rooms linking) the engine will add an invisible sector collision in front of the door when it's closed. Since it's boring in many situations you can use the fake animating door to avoid this problem.

## ENABLED
## ENV_ANIM_COMPLETE
work on current animation of lara, the condition it's true only if current animation is at last frame.

## ENV_CLIMB_LEFT_IN_CORNER
Verify if lara is near at an internal corner al left. See description of ENV_HANG_LEFT_IN_CORNER constant to understand the difference between internal and outside corners.

## ENV_CLIMB_LEFT_OUT_CORNER
Same mean of ENV_CLIMB_LEFT_IN_CORNER but for outside corner.

## ENV_CLIMB_LEFT_SPACE
Works like ENV_HANG_LEFT_SPACE but it should be used when lara is in climb mode

## ENV_CLIMB_RIGHT_IN_CORNER
Same mean of ENV_CLIMB_LEFT_IN_CORNER but for right side.

## ENV_CLIMB_RIGHT_OUT_CORNER
Same mean of ENV_CLIMB_RIGHT_IN_CORNER but for outside corner.

## ENV_CLIMB_RIGHT_SPACE
Works like ENV_HANG_RIGHT_SPACE but it should be used when lara is in climb mode

## ENV_CLIMB_WALL_AT_LEFT
Condition is true when the specific wall in same sector where is Lara, is climbable.

## ENV_CLIMB_WALL_AT_RIGHT
Condition is true when the specific wall in same sector where is Lara, is climbable.

## ENV_CLIMB_WALL_BACK
Condition is true when the specific wall in same sector where is Lara, is climbable.

## ENV_CLIMB_WALL_IN_FRONT
Condition is true when the specific wall in same sector where is Lara, is climbable.

## ENV_CONDITION_TRIGGER_GROUP
This env condition allows to perform a TriggerGroup where you stored some exported condition triggers. In DistanceForEnv you type the IdTriggerGroup to check, if the final condition of TriggerGroup is true it will be true also the Env condition for your Animation command.

## ENV_DISTANCE_CEILING
Condition is true if distance from lara to ceiling is greater or equal to value typed in DistanceForEnv.
For example type in DistanceForEnv the value $300 (3 clicks) the condition will be true if the coordinateY of lara (on her feet when she is stand up) is distance from Ceiling 3 click or more in current sector where lara is.

## ENV_DISTANCE_EAST_WALL
Used in Animation command.
Check if at given distance in East direction there is a wall.
See ENV_DISTANCE_NORTH_WALL description for more informations.

## ENV_DISTANCE_FLOOR
Verify if the distance between lara and floor under her is greather  or equal than set in DistanceForEnv field
Remar: if you want the opposite condition, i.e if distance from floor is less or equal than DistanceForField, just you add to ENV_DISTANCE_FLOOR the special flag ENV_NON_TRUE. This method work for all other condition where you want inverte the condition.

## ENV_DISTANCE_NORTH_WALL
Used in Animation command.
This condition verify if at given distance (set in DistanceForEnv field) there is a wall to north of Lara.
WARNING: this condition works in absolute way looking only for cardinal point,  ignoring the current facing of lara so it should be used only with other env conditions to apply only for some specific room in hardcoded way.
This condition considers like "wall" any floor higher than current lara position.

## ENV_DISTANCE_SOUTH_WALL
Used in Animation command.
Check if at given distance in South direction there is a wall.
See ENV_DISTANCE_NORTH_WALL description for more informations.

## ENV_DISTANCE_WEST_WALL
Used in Animation command.
Check if at given distance in West direction there is a wall.
See ENV_DISTANCE_NORTH_WALL description for more informations.

## ENV_ENEMY_SEE_LARA
Used in AnimationSlot command
It checks if current enemy is able to see lara.
In the DistanceForEnv field you type the angle of view of the enemy.
You type the view angle in degrees, where 180 means the enemy is able to control an emicycle in front of him. If you type 360 this means that the facing (orienting of enemy) will be not used to verify the chance to see lara but only the presence of obstacles (walls, items) in the line between enemy and lara.
A reasonable value for human view angle is about 160 degrees.
Remark: the angle view works for horizontal view, about the vertical view will be used the prefixed value of 90 degrees. This means that if lara is very higher or lower than enmey he will be not able to see her.
Another prefixed limitation is about the distance. The enemy will be never able to see lara when she is far 10 sectors or over.

## ENV_FLOATING
Perform animation only if lara is floating over water surface

## ENV_FLYING_DOWN
Perform animation only when lara is jumping and she is in fall down phase

## ENV_FLYING_UP
Perform animation only when lara is jumping and she is in upward phase

## ENV_FRAME_NUMBER
When you set in StateId Animation array a specified number for animation current (as negative value) you can specify also the precise frame when the condition will be true.
For example if you want you special animation starts only when lara is performing the frame number 21 of the animation 325, you type "-325" in state-id animation array (last field of Animtion command) and to set the frame 21 just set the envcondition ENV_FRAME_NUMBER while in Distance for env type 21.

## ENV_FRAME_RANGE
Used in Animation command.
This ENV condition works like ENV_FRAME_NUMBER but in this case you can set a valid frame range where the condition will be true.
I suggest to read also the description of ENV_FRAME_NUMBER constant, anyway I remember that these frame checks work when you set in stateid/animation array, one animation number, typed in negative form (for instance: if you wish execute your animation only when lara is performing the animation 32, you'll type "-32" in array of Animation command).
To set the range of frames to get true the ENV condition, you set two numbers of frame range, MaxRange and MinRange, using this formula:

MaxRange * 256 + MinRange

Then you type this value in DistanceForEnv field.

Notes:
* Both limits are enclosed in the valid range for a true condition
* The max value for MaxRange is 254, you cann't pass over it.

## ENV_FREE_HANDS
Animation will be started only while Lara has no object in her hands.
If she's taking, weapons, crowbar or she is hanging on wall (climb or monkey) the animation will be NOT performed
Note: this flag ignores the flare, since when lara has in her hand only the flare the game engine considers lara with free hands.

## ENV_HANG_LEFT_IN_CORNER
Verify if lara is near to corner at her left while she is hang mode. In DistanceForEnv you type the min distance from corner to be the condition true.
Remark: the "_IN_" means internal corner. The internal corner are that you find inside a box, while the "_OUT_" (outside) corners are them you find outside of a box.

## ENV_HANG_LEFT_OUT_CORNER
Same mean of ENV_HANG_LEFT_IN_CORNER but with reference to an Outside corner. See description of ENV_HANG_LEFT_IN_CORNER to have more infos about internal and outside corners.

## ENV_HANG_LEFT_SPACE
Verify if there is yet space at left of Lara in hang mode. In DistanceForEnv you type the space required at left to have the condition true.
Remark: you can't specify a value for distance that passes over the current sector. If you type a too big value the engine will reduce it to reach only the left border of current sector.

## ENV_HANG_RIGHT_IN_CORNER
Same mean of ENV_HANG_LEFT_IN_CORNER but for right side. See description of ENV_HANG_LEFT_IN_CORNER costant.

## ENV_HANG_RIGHT_OUT_CORNER
Same mean of ENV_HANG_RIGHT_IN_CORNER but for outside corners. See description of ENV_HANG_LEFT_IN_CORNER costant

## ENV_HANG_RIGHT_SPACE
Same of ENV_HANG_LEFT_SPACE but for right side. See description of ENV_HANG_LEFT_SPACE for more infos.

## ENV_HANG_WITH_FEET
To use only when Lara is hanged to wall corner. The condition is true only when the wall where Lara hanged is enough height to host also feet of lara.
In Distance field you can change the height of this wall, the default value is $300  (3 clicks)  but in some circustances it's better to use as Distance a lower value, like $200 (2 click)

## ENV_HOLD_EXTRA_ITEM_IN_HANDS
The condition is true when lara holds in her hands the HOLD_... item typed in Extra field.
See descriptions of HOLD_ constants in MNEMONIC LIST of Reference panel.

## ENV_HOLE_BACK_CEILING_CLIMB
You should use above condition only when Lara is monkey on the ceiling and you want create an animation that permits to lara to pass from current ceiling to an heighest climbable wall, passing directly by ceiling to climbable wall.

## ENV_HOLE_FLOOR_AT_LEFT
condition is true when there is an hole, i.e. a zone where lara is able to go down or fall.
Different ENV_HOLE_FLOOR.. set the position of adiacent square respect that where is lara.
In Distance field you can set the height (or depth) of the hole.

## ENV_HOLE_FLOOR_AT_RIGHT
condition is true when there is an hole, i.e. a zone where lara is able to go down or fall.
Different ENV_HOLE_FLOOR.. set the position of adiacent square respect that where is lara.
In Distance field you can set the height (or depth) of the hole.

## ENV_HOLE_FLOOR_BACK
condition is true when there is an hole, i.e. a zone where lara is able to go down or fall.
Different ENV_HOLE_FLOOR.. set the position of adiacent square respect that where is lara.
In Distance field you can set the height (or depth) of the hole.

## ENV_HOLE_FLOOR_IN_FRONT
condition is true when there is an hole, i.e. a zone where lara is able to go down or fall.
Different ENV_HOLE_FLOOR.. set the position of adiacent square respect that where is lara.
In Distance field you can set the height (or depth) of the hole.

## ENV_HOLE_IN_FRONT_CEILING_CLIMB
You should use above condition only when Lara is monkey on the ceiling and you want create an animation that permits to lara to pass from current ceiling to an heighest climbable wall, passing directly by ceiling to climbable wall.

## ENV_IN_LEFT_SIDE_SECTOR
This condition checks only the position of lara respect current sector where she is.
If lara is near to left border of current sector the condition is true.
Since we are speaking of "left side" this means this condition is true when lara is (virtually) touching with her left arm the (further) wall in sector at left of the sector where she is.
In DistanceForEnv you can change the comput for distance. If you set big values the condition is true also when lara is not very closed to left border of current sector, while if you set a little value the conditon will be true only when lara is very closed to left border of current sector.
Remark: probably you should use this condition togheter with other because alone is not very useful, since it doesn't check if in sector at left of current sector there is really a wall that lara could touch. So you should use this condition togheter with [ENV_NON_TRUE+ENV_NO_BLOCK_AT_LEFT] condition to verify if really lara could touch with her left arm the wall in left side.
Remark: the default value for DistanceForEnv is 128 (1024 = one sector), this is a good value to compute touching of left wall when lara is in stand up position. If you type IGNORE in distance for env will be used 128.

## ENV_IN_RIGHT_SIDE_SECTOR
This condition work in similar way of ENV_IN_LEFT_SIDE_SECTOR condition but in this case it works for right side.
Read the description of ENV_IN_LEFT_SIDE_SECTOR constant to understand the logical of this condition.

## ENV_IS_STILL
perform animation only when lara has no horizontal or vertical speed

## ENV_ITEM_EXTRA_AT_LEFT
This condition work like ENV_ITEM_EXTRA_IN_FRONT but it analyses for an item at left of lara. You have to insert the slot of wished item in Extra field and the  distance in DistanceForEnv.

## ENV_ITEM_EXTRA_AT_RIGHT
This condition work like ENV_ITEM_EXTRA_IN_FRONT but it analyses for an item at right of lara. You have to insert the slot of wished item in Extra field and the  distance in DistanceForEnv.

## ENV_ITEM_EXTRA_IN_FRONT
Verify if there is an object (moveable) with slot number = number set in ExtraSlot field of Animation command. As DistanceForEnv you have to type the distance where you want the object. (1024 = sector)

## ENV_ITEM_EXTRA_OVER
Similar to ENV_ITEM_EXTRA_IN_FRONT but in this case the object should be over lara, on same sector. In DistanceForEnv you type the height over lara (See description of ENV_ITEM_EXTRA_IN_FRONT for more infos).

## ENV_ITEM_EXTRA_UNDER
Similar to ENV_ITEM_EXTRA_OVER but in this case the object should under lara. If you set the DistanceForEnv field to 0 the object should be near to feet of lara.

## ENV_ITEM_TEST_POSITION
This condition used to detect if there is an object of given TestPosition command is in correct position with respect to Lara.
The ENV_ITEM_TEST_POSITION has same target of ENV_ITEM_EXTRA_.... conditions but in this case you can accurately compare for an item in any position with respect to Lara and in absolute precise mode.
The other env conditions have some problems to detect the exact distance when the pivot of Lara and the item are too different.
To use the ENV_ITEM_TEST_POSITION you have to type in script a "TestPosition=" script command with all data necessary for the detection, and then you'll type in DistanceForEnv field of Animation= or MultEnvCondition= commands the IdTestPosition (first argument of TestPosition).

Remark:
The ENV_ITEM_TEST_POSITION condition (like other ENV_ITEM_EXTRA_... conditions) has the limit to check only for items in same room where lara is.
If you want detect an item in a room different than Lara room, the only way is to set in TestPosition command the flag TPOS_TEST_ITEM_INDEX in this way you can set in Slot field (of TestPosition) an index of specific item instead a generic slot type.

See description of TestPosition script command in the list "SCRIPT NEW commands" of Reference panel of NG_Center program, to have more informations.

## ENV_LARA_IN_MICRO_STRIP
Used in Animation command
This environment condition works in similar way than ENV_POS_STRIP_1/2/3 flags, but (IMPORTANT) remember that it is a condition and not an ENV_POS flag so you cann't add the ENV_LARA_IN_MICRO_STRIP value to another ENV_ condition. Differently you should place this ENV_LARA_IN_MICRO_STRIP condition in a MultEnvCondition script command and, furhterly you could add to this condition some ENV_POS_ flag like the (always required with this condition) ENV_POS_HORTOGONAL flag.
The ENV_LARA_IN_MICRO_STRIP allows, respect to the ENV_POS_STRIP_1/2/3 flags, a best fine precision, since with the ENV_POS_STRIP_1/2/3 flags you can choose only between three "fat" strips, while with the ENV_LARA_IN_MICRO_STRIP you can choose any specific range within the sector where it should be Lara to get true this condition.
When you use the ENV_LARA_IN_MICRO_STRIP condition you have to type in DistanceForEnv field the range (min-max) in micro-strips units, where lara should be enclosed.
The value to type in DistanceForEnv is given by following formula:

Max*256 + Min

Where Min and max have values in the range 0 / 31
Pratically you should imaginate a sector divided by 32 strips, in the direction where lara is looking, and you can choose in what range of 32th strips it should be Lara to get true the condition.
For example if you wish have Lara very closed to outside border (pherpas to get the condition when lara is touching a wall in closed next sector) you could choose the range 0 - 2, in this way the condition it will be true wheter Lara is in 0,1 or 2 micro strip.
Remember that the first strip (value= 0) is always that closed to border where lara is looking, while the last strip (value = 31) is that closed to border at the back of Lara.

## ENV_MONKEY_CEILING
Condition is true when lara is belove a monkey ceiling.
In Distance field you can set the range of distance of monkey ceiling.
The Distance in this case is different by usual: the formula is:
MinClick + MaxClick * 256
For example if you want that the condition is true when the monkey ceiling is enclosed in range 5 (min) clicks  upto 7 (max) click, you'll have to type in distance field the value (7*256 + 5) = 1797
Remark: in hexadecimal (with '$' sign) it's more easy understand the Min and max value, for example the number 1797 in hexadecimal is $0705 where you can read the Max click ("07) and the Min click ("05")

Remark: if Lara is under the monkey ceiling but the heigh is outside of the range you set in distance field, the condition will be false and the animation will be not performed.

## ENV_MULT_CONDITION
Signal a multiple condition. The effective ENV condition flags will be stored in a MultEnvCondition script command, and in DistanceForEnv you have to type the ID of MultEnvCondition command will be used.
 The final condition will be true only if all conditions in MultEnvCondition command will be true in same time.
Examples:

If you want set in your animation command two Env condition, like: ENV_CEILING_HEIGHT with height (distance env) = $300 (3 clicks) + other condition: ENV_HOLE_FLOOR_AT_LEFT with depth (distance env) = $400 (4 clicks) you can as first step create this MultEnvCondition command:

MultEnvCondition= 1, ENV_CEILING_HEIGHT, $300, ENV_HOLE_FLOOR_AT_LEFT, $400

and then type in Animation command the reference for Id of above MultEnvCondition (1) and the ENV_ condition: ENV_MULT_CONDITION

For example:

Animation=447, KEY1_LEFT, IGNORE,IGNORE,ENV_MULT_CONDITION, 1,IGNORE,-445, -448

## ENV_MULT_OR_CONDITION
Works like ENV_MULT_CONDITION but the final condition will be true if just a single condition of MultEnvCondition command is true.
See description of ENV_MULT_CONDITION for more infos.

## ENV_NON_TRUE
Invert the current ENV condition. You have to add this value to some ENV condition when you want have that when the condition is true it will be false, and viceversa.
For example looking belove ENV conditions you'll find the ENV_NO_BLOCK_IN_FRONT condition.
If you use this alone it means "perform my special animation only if lara has NO block in front of her face".
But if you want perform an animation requiring a block in front of lara, you can use that same conditon (ENV_NO_BLOCK_IN_FRONT) linked with + ENV_NON_TRUE, to inverse the condition.
In this combination (ENV_NON_TRUE + ENV_NO_BLOCK_IN_FRONT) we have an (ugly) double negation that it means: "when lara HAS a block in front.. . perform my special animation".
You can add ENV_NON_TRUE flag to any ENV condition, anyway remember that the ENV_NON_TRUE flag has NO effect on ENV_POS ... flags, this means the ENV_POS flags will be valued always in direct way, indifferntly if you set or less an ENV_NON_TRUE flag.

## ENV_NO_BLOCK_AT_LEFT
Condition is true only if there is no wall in sector at left of lara.
In Distance (next field) you can type the height of wall. See "Distance for Env field"

## ENV_NO_BLOCK_AT_RIGHT
Condition is true only if there is no wall in sector at right of lara.
In Distance (next field) you can type the height of wall. See "Distance for Env field"

## ENV_NO_BLOCK_BACK
Condition is true only if there is no wall in sector back of lara.
In Distance (next field) you can type the height of wall. See "Distance for Env field"

## ENV_NO_BLOCK_IN_FRONT
Perform your special animation only if lara has NO block in front of her face. In "Distance for Env" field you set the height of block. For example if you set 1024 (height of one sector) trng will consider true the condition "no block in front" if the floor in front of lara is higher less than 1024 units respect current floor where lara is. While if floor in front of lara is high 1024 units or more, it will be detected a block in front of lara.

## ENV_NO_BOX_AT_LEFT
Used in AnimationSlot command
Check if at left of current enemy it's missing a box sector.
See the description of ENV_NO_BOX_IN_FRONT constant, for more infos.

## ENV_NO_BOX_AT_RIGHT
Used in AnimationSlot command
Check if at right of current enemy it's missing a box sector.
See the description of ENV_NO_BOX_IN_FRONT constant, for more infos.

## ENV_NO_BOX_BACK
Used in AnimationSlot command
Check if back of current enemy it's missing a box sector.
See the description of ENV_NO_BOX_IN_FRONT constant, for more infos.

## ENV_NO_BOX_IN_FRONT
Used in AnimationSlot command
This condition detect if in front of current moveable it's missing the gray boxes used in NGLE to stop the moving of enemies.
In the DistanceForEnv field you set distance to check in front of enemy.
Note:
The _NO_BOX_ conditions should be used with AnimationSlot command on some enemy. The usage with Lara is theorically possible but not very useful.

## ENV_ONLAND
Perform animation only if lara is on land

## ENV_ON_VEHICLE
By default the start of animation cann't happen if Lara is driving some vehicle.
Anywya if you want create new animation for lara while she is on some vehicle you have to use this ENV condition and typing in Extra field the Slot number of vehicle.
Remark: In Extra field you cann't type the name of slot but you can type the number you read at left of slot name in Slot moveable list of reference panel.

## ENV_PLAYER_IS_SLEEPING
Used in Animation= command.
This is a very original condition. Pratically it verifies from how much time are missing keyboard commands.
You have to type in DistanceForEnv the number of tickframes (one second= 30 tickframes) required to enable this condition.
For example if you want have true this condition when the player doesn't perform game commands from 3 seconds, you have to type in DistanceForEnv field the value 90 (because 3 * 30 = 90)
This condition could be used to start nice animation in stand-by mode, i.e. when the player is not playing activly (no keyboard signals) you can show an animation where lara scratches one's head, or she turns her face to look player saying: "Are you sleeping?"
If you mean use this nice animation in standby you should remember to set in this animation a state id different by standard state-ids, for example you can use the state id STATE_CONTROLLED, to avoid that some new game command cause a bad change between current (custom animation) and new standard animation.
When you use the state id neutral no new animation will be performed until lara comes back to some standard state id.
So you could create an animation with state id = STATE_CONTROLLED and as next animation the defaul animation of still stand-up.
You'll have to create a custom animation where lara starts from stand-up position, it performs the nice move, and then comes back to stand-up animation, with the next animation having the standard stand-up animation.

## ENV_PLAYER_WOKE_UP
Used in Animation= command.
This env condition is the opposite of ENV_PLAYER_IS_SLEEPING. It happens when there is any game command.
In ENV_PLAYER_IS_SLEEPING description has been described a way to create stand-by animations in death times of the game.
There is also another method, a bit more complicated, and it's in this situation you need of ENV_PLAYER_WOKE_UP.
If you want perform a custom animation when there is no game commands (player is sleeping) you could put in continue mode lara in a new position, for example that she sit down and wait.
In this situation, since you used a non-standard state-id, no keyboard command will be accepted, so it should be you with an Animation command and condtion ENV_PLAYER_WOKE_UP, to force another custom animation having the target to move lara from sit down position to common stand-up position. In this new animation it will be set like next animation the standard still stand-up animation, so from this moment lara will be newly able to receive game commands.
In game the player could see this little show:
Lara (bored) sit down, and looks at left and right, then, when player hit some game command like for example the JUMP, lara will no jump, but she'll back to stand-up position, and if player hit newly jump (or other commands) lara  will come back to respond to these commands.

## ENV_POS_CENTRAL
Please don't confuse this flag with previous
ENV_POS_IN_THE_MIDDLE.
The ENV_POS_CENTRAL works fine togheter with some
ENV_POS_STRIP.. field, to set a central position in specific strip you chose.
While the ENV_POS_IN_THE_MIDDLE cann't used with ENV_POS_STRIP because that flag already set exactly the position in sector (at center).

## ENV_POS_HORTOGONAL
For hortogonal we mean :non diagonal.
For many animation it's necessary lara is facing exaclty a wall otherwise she will be not able to climb, hang ect.
If you add this ENV_POS flag the condition will be false if lara is too diagonal respect to wall lines.

## ENV_POS_IN_THE_MIDDLE
Lara is (almost) exaclty at center of current sector. This is the position for lara when she is able to jump over a rope, for example.

## ENV_POS_LEFT_CORNER
Works like ENV_POS_RIGHT_CORNER but for left side. See descrition of ENV_POS_RIGHT_CORNER for more infos.

## ENV_POS_RIGHT_CORNER
The corner is that of current sector where lara is. This condition is true when lara is in right corner of current sector using as reference the direction where she is looking.

## ENV_POS_STRIP_1
Lara is in first (more forward) strip of three ideal strips of current sector, respect where she is looking.
You can use this flag if you want that lara is near to switch or door or other items attached to wall with lara immediatly in front of them.
Remark: you can always add to current ENV condition one or more ENV_POS_ flags to specify the position of lara respect current sector

## ENV_POS_STRIP_2
Like above, but in this case Lara is the middle strip of three stripes of current sector. See description of ENV_POS_STRIP_1 for more infos.

## ENV_POS_STRIP_3
Like above, but now she is in third strip. See description of ENV_POS_STRIP_1 for more infos.

## ENV_ROOM_IS
Used in Animation= command.
This condition check if current room  (where lara is) is the same of number typed in DistanceForEnv field.
Using this condition you can create a custom animation that it will work only in some room of the level.
If you need to check for more than one room, you should insert the ENV_ROOM condition list in a MultEnvCondition command.

Remark:
In DistanceForEnv you can type the room number you wish check, but you can also write some special constants to verify the type of room, indifferently by its number.
You can type one of ROOM_... constant to test if lara is in that moment in that wished room type.
See description of ROOM_... constants for more infos.

## ENV_SUPPORT_IN_BACK_WALL
see description for ENV_SUPPORT_IN_FRONT_WALL

## ENV_SUPPORT_IN_FRONT_WALL
All ENV_SUPPORT_... check for a support where lara could hang in wall in front (or back, left, right)
You can use this condition to start some animation where lara jumps and hang on wall in front of her, to verify if there is a wall with correct height in front of lara.
For example by default the game engine allows to lara to hang to a wall if this wall has an height in range between 4 click to 7 click.
If the wall is heigher (from 8 click respect to lara's feet) lara will be not able to hange with a up jump.
Well, if you want set same condition you have to insert in Distance for Env field this value:  $0174
For ENV_SUPPORT condition the distance env is a bit complex.
The values you have to set are:
The min height acceptable (4 click in our example)
The max height acceptable (7 click in our example)
The minimum space present over the support to place lara hands (1 click)

The he formula is:  MinHeight + MaxHeight*16 + ClickSPace * 256
In decimal this value is misterious: 4 + 7 * 16 + 1*256 = 372
Anyway if you type in in hexadecimal format is more easy understand the position of each sub-field: 372 decimal = $174  in hexadecimal.
I.e. in hexadecimal the digits are:

$0BCA

where:
'A' is click of minimum height required
'B' is click of minimum height acceptable
'C' is number of click of space over the support

About hexadecimal notions remember the numbers, usign a single digit, have following sequence:
0, 1, 2, 3, 4, 5, 6, 7, 8, 9, A (10 decimal), B (11 decimal), C (12 decimal), D (13 decimal), E (14 decimal) and F (15 decimal)
In this way you can set number of click between 0 and 15 click for each sub-field of Distance for env.

Another sample we could do, it's that to verify if, while lara is hang to support, she has enough space to move up over the new floor.
For this target we'll use a distance like $333 because:

3 click for min and max height, because the support has to be in a specific position, where are the hands of lara.
The value 3 is the distance from feet of lara, about 3 clicks
The last "3", for space, is the space necessary to host lara in stand up position.
In this way above condition will be  true only if the height of space over the support where lara hanged is at least 3 click of heigth.

## ENV_SUPPORT_IN_LEFT_WALL
see description for ENV_SUPPORT_IN_FRONT_WALL

## ENV_SUPPORT_IN_RIGHT_WALL
see description for ENV_SUPPORT_IN_FRONT_WALL

## ENV_UNDERWATER
Perform animation only if lara is underwater

## ENV_VERTICAL_ORIENT
Very particular flag.
You can set a condition in according with current vertical orienting of lara.
Normally lara has always zero like vertical orienting but in some animation this value could change.
You can set a condition about current vertical orienting using the condition ENV_VERTICAL_ORIENT and setting in "Distance for env" field the range of orienting values computed in this way:
MinValue + MaxValue * 256
Typing in hexadecimal value it's more easy to understand. For example if you want set a range like  from 3 to 8, just type $83
The values are 16, from 0 to 15 moving in clockwise direction.
Remark: if you want crate a range enclosing the zero (0) you have to invert max and min value, looking the values like hours on the clock.
For example to enable range accepting following values: 13, 14, 15, 0, 1, 2, 3  you have to type 13 like min value and 3 like max value, i.e. in hexadecimal should be: $D3  (D = 13)

## ENV_WALL_HOLE_IN_FRONT
This condition works in very similar way of ENV_SUPPORT_IN_FRONT_WALL condition, but in this case, other to check for a support in front wall, the engine will check also the height of this hole in the wall, allowing to set the min height and the max height limits of this hole.

The {distance env} field for ENV_WALL_HOLE_IN_FRONT is like that for ENV_SUPPORT_IN_FRONT_WALL condition, but there is a value in more to set the height of the hole in the wall:
The values you have to set are:
The min height acceptable (4 click in below example)
The max height acceptable (7 click in below example)
The minimum space present over the support to place lara hands (example 1 click)
The max space present over the suppot (height of hole) (example 2 clicks)

The formula is:  MinHeight + MaxHeight*16 + MinSpace * 256 + MaxSpace * 4096
In decimal this value is misterious: 4 + 7 * 16 + 1*256 + 2 * 4096 = 8564
Anyway if you type in in hexadecimal format is more easy understand the position of each sub-field: 8564 decimal = $2174  in hexadecimal.
I.e. in hexadecimal the digits are (using literal A, B, C, and D as references):

$DCBA

where:
'A' is click of minimum height required
'B' is click of minimum height acceptable
'C' is number of click of min space over the support
'D' is number of click of max space over the support

About hexadecimal notions remember the numbers, using a single digit, have following sequence:
0, 1, 2, 3, 4, 5, 6, 7, 8, 9, A (10 decimal), B (11 decimal), C (12 decimal), D (13 decimal), E (14 decimal) and F (15 decimal)
In this way you can set number of click between 0 and 15 click for each sub-field of Distance for env.

## EXTRA_MUTANT_NO_LOCUSTS
Used in Enemy= command.
If you are customizing the MUTANT slot with an Enemy script command, you can disable the swarm locust attack adding this flag in Extra field of Enemy command.

## EXTRA_TEETH_NO_DAMAGE_ON_WALKING
Used in Enemy= command.
You can enable the no-damage for lara when she moves slowly (walking).
If you set this flag in Extra field of enemy= command with TEETH_SPIKES slot, lara will be able to walk in teeth spikes with no damage.

## EXTRA_WRAITH_BURN_LARA
Used in Enemy= command.
When you type an Enemy script command to customize some wraith slot type, you can this flag to enable the burning of Lara when wraith touches some times Lara.

## FADD_CONTINUE_EMIT
If you use this flag, the values typed in DurateEmit and DuratePause will be ignored.
You should use this flag for effect types requiring a continue emission, like mist and flames.

## FADD_DURATE_ANIMATION
This flag work in very similar way of above flag fadd_durate_stateid, but in this case the value to check for condition emit/disable is the animation number.

## FADD_DURATE_SINGLE_FRAME
This flag force the emitting to work only for single frame. There will be a single emitting and then the effect will be disabled.
I think only blood can work with this flag.

## FADD_DURATE_STATEID
If you want you can keep this effect active until moveable is in current state id.
Using this flag the time durate set in trigger window will be ignored, and the effect will be removed only when moveable will change its state Id with a value different than that used at start of effect.

## FADD_FIRE_STRIP
Used in AddEffect command.
This flag may be used togheter with ADD_FLAME type.
By default the ADD_FLAME type shows a little fire in common way, while if you use the FADD_FIRE_STRIP flag it will be created an horizontal strip of fire like if the flames were shooted from a dragon (from front) or from a air-jet (from back, using also the FADD_ROTATE_180 flag).

## FADD_IGNORE_STATUS
Used in AddEffect command.
By default the add effect trigger will ignore the comand of add effect if the moveable has not yet been enabled in game or if it has already killed/removed.
Anyway in some cirucstance this compute could be wrong and with some special items the result could be to fail the adding effect operation.
If you work with some of these special items (like some pickups) you can add the
 FADD_IGNORE_STATUS flag. When trng finds this flag it ignores the status of moveable and it will apply always the effect also if item appears invisible or disabled.

## FADD_NONE
If you don't want use any FADD_ constant you can use FADD_NONE constant in FADD field.

## FADD_NO_SOUND
Used in AddEffect command.
This flag disable furhter sound linked with some effects like mist or fire.

## FADD_ROTATE_180
It works like FADD_ROTATE_90 flag but using a rotation of 180 degree. See description of FADD_ROTATE_90 flag for more infos.

## FADD_ROTATE_270
It works like FADD_ROTATE_90 flag but using a rotation of 270 degree. See description of FADD_ROTATE_90 flag for more infos.

## FADD_ROTATE_90
The fadd_rotate flags are used only in particular circustances, when you use an effect type that use the orientation (horizontal) of object to compute the direction of emitting.
In this case you can alterate the orienting of emit adding one of above flags. For example if you want the mist strip will be oriented in side instead of in front of object facing, you can use FADD_ROTATE_90 flag.

## FADD_SMOKE_EXHAUST
This flag will be used only for ADD_SMOKE type effect.
There are two different smoke emitters: the default is a common smoke will go up and disappear in fast time, while other is the exahust smoke you see also in jeep or sidecar. If you want use exhaust smoke set this flag, while you want a more traditional smoke don'use this flag.

## FADD_VORIENT_180
Used in AddEffect command.
Change vertical orienting by 180 degrees of moveable in temporary way before apply the compute of DispX, DispY, DispZ.
See also the description of FADD_VORIENT_90 flag to understand the particular situation when you should use these flags.

## FADD_VORIENT_270
Used in AddEffect command.
Change vertical orienting by 270 degrees of moveable in temporary way before apply the compute of DispX, DispY, DispZ.
See also the description of FADD_VORIENT_90 flag to understand the particular situation when you should use these flags.

## FADD_VORIENT_90
Used in AddEffect command.
This flag rotate (in temporary way) the item of 90 degrees in clockwise direction on vertical axis.

This temporary change is usefull with some particular moveables where the position of the meshes has a different vertical orienting respect than their position in first animation.
You can discover this situation using Wad Merger and verifying the mesh when there is no animation selected respest when it has been selected the first animation.

It's important understand that all FADD_VORIENT... flags are very different than the FADD_ROTATE... flags, since the FADD_ROTATE flags will be used to give an horizontal direction (facing) for those effects moving on some ideal line, like the mist strip or fire strip.
Differently, you cann't change the direction of effects using the FADD_VORIENT flags, but you'll use these flags only when you have to fix the bad vertical orienting  of some moveables (like the jeep for example).

## FAN_ALIGN_TO_ENV_POS
From version 1.1.9.8 this FAN has new properties.
When you add this flag and you set some valid ENV_POS_... flags in Evironment field (see following description) the position of lara will be forced to be the ideal position in according with ENV_POS_ values before performing your custom animation.
For example if you set ENV_POS_HORTOGONAL, an instat before starting the special animation the lara orientation will be aligned with hortogonal axis.
Remark: by default the alignment operation is a bit poor, since TRNG move immediatly lara in ideal position without any progressive movement.
Anyway from versione 1.1.9.8, when you set FAN_ALIGN_TO_ENV_POS in according with ENV_ITEM_TEST_POSITION condition, you can get a progressive self adjustment in game like you see in default tomb4 when lara open a door or push a pushable object.
This feature will use the data you set in TestPosition command and extracts from it the ideal position to reach, using intermediate values for each difference range you set.
This feature don't work fine if lara was performing a state id different by stand-up position or swimming underwater.
Warning: when you set ENV_ITEM_TEST_POSITION and the FAN_ALIGN_TO_ENV_POS the orient information will be read only from TestPosition command, so the further ENV_POS_... FLAGS you add in EnvCondition will be ignored.

## FAN_DISABLE_GRAVITY
This flag work in similar way of FAN_ENABLE_GRAVITY but in this case is to disable the gravity compute.
See description of FAN_ENABLE_GRAVITY for more infos.

## FAN_DISABLE_PUSH_AWAY
Used in Animation command.
If you wish that lara did not change her (and the your) animation when she has been touched by enemies, you can add this flag.
The push-away animation is a very hardcoded animation, where lara suspend previous animation and move herself slowly while the enemy is pushing her away.
These push-away animations have numbers: 125 126 127 128

Note: this flag disables the pushaway animation only when the animation of lara is the same you set in current Animation command.
If you use an animation chain, where, after this first animation, there is another, your custom animation, you could see lara with push-away animation while there is this second animation. To avoid this problem you can use this trick: create another Animation command for the second animation, with the FAN_DISABLE_PUSH_AWAY flag. The trick consists in the fact that this other animation command could be never performed, just using an unexisting stateid as condition for it, but this animation will be however used to store the animation numbers about when disabling push away animation for lara.

Another method to solve above problem is to disable the push away animation forever and for all lara's animations, with the CUST_DISABLE_PUSH_AWAY_ANIMATION customize command.

## FAN_ENABLE_GRAVITY
This flag enable for your animation the gravity compute.
When lara is flying or falling the engine store a flag to remember to apply to lara the gravity rules. In some cirucstances you could need to enable the gravity for your animation using this flag.

## FAN_KEEP_NEXT_STATEID
This is a very technical flag.
My suggestion about to use it or less, it's simply to try...
At start you should create an Animation command without using this flag, because it's more common don't use it, but if you see somewhat don't work, you could try adding this flag to see the result.
Technically you should use this flag when your animation is for single execution and then lara will come back to some standard animation.
While you should not use this flag when your animation has to be performed in loop, like an upward jump and many other fluid animations.

## FAN_KEYS_AS_SCANCODE
If you mean to use KeyBoard scan code in Key1 and Key2 fields, you have to add this flag  FAN_KEYS_AS_SCANCODE in FAN_ fields.
When you don't use this falg the value typed in Key1 and Key2 field will be interpreted like Game command, i.e. mnemonic constants KEY1_.. and KEY2_...
Remark: This behavior is the opposite respect for first version of Ng_Center. In previous versions you have to set a flag to specify to use game commands but now the game commands are the default setting.

## FAN_PERFORM_TRIGGER_GROUP
This flag changes fully the working mode of Animation command.
When you add this flag the value you typed in first field of Animation command, i.e. the field named "AnimIndex", will be seen as an IdTriggerGroup to perform when all conditions set in Animation command are true.
This means you could start you custom animation typing in a triggergroup a flipeffect to start a specific animation of lara, adding in same triggergroup also many other effects, and then to use an Animation command to start the trigger group when player hit correct key and all conditions are true.
In many circustances, other to perform a custom animation, you wish also to perform some special effect to change somewhat in game, in this way you add new skills for lara.
A way to realize this target is to create a custom animation where you set in some frame a NG AnimCommand to perform these "special effects" (i.e. exported triggers to perform somewhat). When animation command start  your custom animation, the NG Anim command of custom animation will be performed and it could perform many changes in game.
Well, using the FAN_PERFORM_TRIGGER_GROUP flag you can get same result but in a different way.
You can create a trigger group with many exported triggers to change some situation in game, and then, in same trigger group, to have also a flipeffect to perform the wished custom animation.
The difference between two above methods is that, using the triggergroup method, you can perform somewhat before the animation will be started, while with NGAnimCommand in custom animation, these effects could be done only while the custom animation has been started.
Remark:
Theorically you could don't add any "perform animation" flipeffect in the TriggerGroup, in this way you'll use the Animation command to perform some operations when player hits some keystrokes.
Anyway you follow this method remember to find a way to avoid the triggergroup was continuously performed a lot of times untile the conditions are true.
TRNG engine uses as method to avoid multiple performing, to disable temporary this Animation/Triggergrop until the key used to perform it first time, remains yet down, but it should be better if you place some trigger in TriggerGroup to change some condition to avoid the furhter repetitions of this triggergroup.

## FAN_RANDOM
Used by Animation script command.
With this flag you can perform different animation in random way.
You have to type in Extra field the number of random animations.
For example if you want perform one of following animations: 512, 513 or 514, when all condition in animation command are true, you'll have to type in AnimIndex field the first animation 512, while in Extra field you type 3 to inform there is a block of three random animations, starting from animation 512.
When all conditions will be true it will be performed the animation 512 or 513 or 514.
The target of this feature is to create a simulation of random events also in animations of lara. For example if you want create a falling animation you can create different kinds of falling to choose in random way.

## FAN_SET_ADDEFFECT
You can add a particle effect when your animation will be performed.
You have to create a AddEffect= script command to specify what particle effect to use. Then you add current flag FAN_SET_ADDEFFECT in FAN field, and you type the number of AddEffect command in Extra field of Animation= command.
Remark: by default the effect will have an infinite durate, anyway you can override this setting using correct flag in AddEffect= command to have the durate of effect only when some stateid or animation number is on.

## FAN_SET_BUSY_HANDS
This flag work in similar way of FAN_SET_FREE_HANDS flag but with an opposite target, of course.
If you use this flag, when your animation will be performed the game engine will be informed that the hands of Lara are now busy and she is not able to grap to wall.
Remark: It's very seldom you'll have to use really this flag, because, by default when your animation has correct state-id (for example for climb or monkey) the game engine set this flag by self

## FAN_SET_FREE_HANDS
The current flag FAN_SET_FREE_HANDS perform a specific action to inform game engine that the hands of Lara are free and, since, she will be able to hang, climb and pick up objects or weapons.
Remark: this operation to free hands will be performed ONLY if your special animation will be really started.

You should use this flag only when you special animation starts from a position where lara had busy hands (she was climbing, or hanged) but, your animation move her from wall and to do jump her. In this situation you have to inform engine now she has free hands, otherwise lara will be not able to graps a climb wall after the jump.

## FAN_SET_FREE_HANDS_TEMP
This flag works like FAN_SET_FREE_HANDS but in this case it set only temporary the "Free Hands" status. The previous status (about what lara was hanging) will be restored at end of your custom animation.
The "free hands" it's necessary when you want perform your custom animation while lara holds weapon or torch, in fact, when "Free hands" is missing, the arm of lara will not follow your custom animation.
Another difference about old FAN_SET_FREE_HANDS is that the FAN_SET_FREE_HANDS_TEMP fixes a bug where the "free hands" command didn't work when lara holded Torch.

## FAN_SET_LARA_PLACE
Used in Animation command.
This fan flag could be used when your custom animation move lara between two different places, like from "ground" to "water", or vice-versa.
You have to type the new place (where lara should be at end of your animation) in Extra field.
You can type one PLACE_ value in extra field to set the new environment of Lara.

Remark: when you create an animation to move lara between different places it's very probable you need to add a standard [SetPosition] command to move lara in new position but also to adjust the different pivot position used in according with ground, underwater, floating ect.

## FAN_SET_NEUTRAL_STATE_ID
This value forces the engine to change the real state-id number of your animation, replacing it with a neutral state-id.
The advantage is to avoid some interference created by tomb4 engine in some circustances.
You should use this FAN_ value when your special animation was breaked before reaching the last frame, only because lara is in the empty or she has problems with collisions or for status (water, falling, climbing)
When an animation is performed with neutral state id (69 number) the game engine checks only for basic collisions but it doesn't perform any specific controll about what lara is doing and where she is.

## FAN_START_FROM_EXTRA_FRAME
If you want that your animation starts not from first frame (0) but from some specific frame number, you can add the FAN_START_FROM_EXTRA_FRAME falg in FAN_ field, and then type in Extra field the number of frame from what to begin the animation.

## FBAR_DRAW_ALWAYS
Used in Customize=CUST_BAR command
This flag will keep always on screen the current bar.
The FBAR_DRAW_ALWAYS flag doesn't work with all bars.
You can use it with:
BAR_HEALTH
BAR_DASH
BAR_AIR
BAR_DAMAGE
BAR_COLD

## FBAR_SHOW_BAR_NAME
Used in Customize=CUST_BAR command
This flag works only for custom bars (BAR_CUSTOM1/2/3/4)
If add this flag to FBAR field of customize command, everytime the bar will be showed the engine will draw also the wished text under the bar.
The texts to use are prefixed:

BAR_CUSTOM1 : Extra NG String with index = 301
BAR_CUSTOM2 : Extra NG String with index = 302
BAR_CUSTOM3 : Extra NG String with index = 303
BAR_CUSTOM4 : Extra NG String with index = 304

For example if you add a text for BAR_CUSTOM1 you have to add the FBAR_SHOW_BAR_NAME flag in customize command for BAR_CUSTOM1 and the in Extra NG list add a text like this:

301: Jumping Power

Removing the previous index showed when you click on [Add string] button.

## FBAR_SOUND_BAR_ANIM
Used in Customize=CUST_BAR command. This flag creates in current bar a floating colors like you see in audio bar in Options screen.
When you use this flag you can also type in Extra field an IdColor (pointing to some ColorRGB= command) to set the mask color.
The audio bar works in particular way: there is a floating effect using two colors, the MainColor (IdColor1) and the background color (IdColor2), while the mask color will be used to paint the full side of bar.
For example if you use this flag with BAR_HEALTH and lara has 50 % of HP, the left half of bar will be colored with MaskColor to differentiate by right (empty) half.
The mask color doesn't paint fully the bar but it will be added to current floating effect.
For example if you set as MaskColor the rgb, with 0,0,0 values, you'll get no difference between left side and right side of bar because adding the 0,0,0, nothing change.
While if you set like mask color the white, with rgb values 255,255,255, the full side of bar will become fully white, losing the floating effect.
For above reasons it is a bit complicated set a good maskcolor. You should try a mask color where the rgb value was not 0 but neither 255. For example a color like: 128,128,128 (this is the default mask color if you type IGNORE in extra field), or  63,63,63  (it used in tomb4 for audio bar in some circustances)

## FBAR_USED_FOR_BOAT_FUEL
Used in Customize=CUST_BAR command
When you set in boat ocbs the values: 32 (Fuel management) + 128 (show fuel bar), you have to set a customize script command to prepare the fuel bar to show on screen.
The fuel bar is always the BAR_CUSTOM4 bar.
You can type this customize command as usual, but with two differences:

1) To the flags you have to add the FBAR_USED_FOR_BOAT_FUEL flag.

2) In the extra field of customize, you have to type the max value for fuel, i.e. the value when the boat has a full fuel and it will correspond to full bar on screen.

This value, to type in extra field, works in this way.
The fuel of the boat is, pratically, the number of frame tick for the operating time of the boat.
Everytime the boat engine is working this fuel/time will be decreased by 1 for each frame tick, when it reaches 0, the engine boat will switched off and the fuel bar will be empty.
Please note that, when the boat is running in turbo mode, the fuel will be decreased by 2 units.

Since frame ticks are 30 for seconds, to give as full fuel an operating time of 15 minutes, you should type the value 30*60*15 = 27000

Notes:
- The max value you can type in this field in 65534 that corresponds to
about 36 minutes. In the game times this is a huge time, probably you'll use a very lower value.
- Remember that the fuel management for boat uses, in hardcoded mode, the local long Delta variable (#0072) to store the current fuel of the boat. You can use this variable to show on screen the remaining time, or, converting it in litres, a representation of current litres in the boat tank.
You can also using triggers to increase or decrease the current fuel.

## FCAM_DISABLE_COMBAT_CAM
Used in Customize=CUST_CAMERA
This flag disables fully the combat camera mode anyway it doesn't affect the automatic aiming feature. The arms of lara will point the further enemy, too, but the camera mode will remain the Chase camera or other camera mode that, in same moment, was working.

## FCAM_INVISIBLE_LARA_ON_LOOK_CAM
Used in Customize=CUST_CAMERA
This flag get lara fully invisible when the player is using the look camera.
By default Lara was only semi-transparent in that situation.

## FFL_ADD_FIRE
Used in Customize=CUST_FLARE command.
Add a little flame to flare.

## FFL_ADD_GLOW_LIGHT
Used in Customize=CUST_FLARE command.
Add to flare a little colored bulb light.
Remark: the glow light will have same color you set in Customize=CUST_FLARE command.

## FFL_ADD_SMOKE_TO_SPARKS
Used in Customize=CUST_FLARE command
This flag add a little smoke when you are adding sparks to flare. Pratically this flag has effect only when you used also the FFL_ADD_SPARKS flag.

## FFL_ADD_SPARKS
Used in Customize=CUST_FLARE command.
Add to flare  sparks emission.

## FFL_FLAT_LIGHT
Used in Customize=CUST_FLARE command.
By default the flare light is a bit blinking. If you wish have a stable common light you can add this flag.

## FFS_PUSHABLE_CAN_OVERSTEP_IT
Used into Customize=CUST_SLOT_FLAGS command.
When you wish that a moveable (usually a little pickups) was oversteppable from pushable objects, you can add this flag for the wished pickup items.
Example:

Customize=CUST_SLOT_FLAGS, LASERSIGHT_ITEM, FFS_PUSHABLE_CAN_OVERSTEP_IT

With above command list the pushable object will be able to pass over the lasersight item on the floor.

## FGT_DISABLED
By default a global trigger works always, from begin of current level until it is performed with a flag FGT_SINGLE_SHOT to disable it.
Anyway if you want, you can disable your global trigger at start, and then to able it only in a second time using the flipeffect "Enable/Disable the GlobalTrigger...".

## FGT_HIDE_IN_DEBUG
Used in GlobalTrigger command.
This flag doesn't change the behavior of the globaltrigger in runtime, but it removes the debugging mexages in debug mode (DGX_LOG_SCRIPT_COMMANDS diagnostic) for this globaltrigger.
The reason to use this flag is when you are studying another script command but this global trigger continues to fill the log with many mexages not intersting for you.

## FGT_NOT_TRUE
You can use this flag to invert the global trigger condition. For example if you want have a global trigger engaged when lara is NOT poisoned you can type a GlobalTrigger command script like:

GlobalTrigger=1, FGT_NOT_TRUE, GT_LARA_POISONED, 0, ....

Remark: this flag doesn't affect the further Condition Trigger Group. Anyway in trigger group you can find a similar flag to invert the single condition triggers.

## FGT_PUSHING_COLLISION
This flag works only when used with global triggers: GT_COLLIDE_ITEM or GT_COLLIDE_SLOT
The engine computes three type of collision:
- Bound Box Collision: this collision is very easy to compute but not very precise. The engine compares two collision box of current frame for both moveables and gives positive esite if the two boxes are overlapped in some 3d space position.
This is the default collision used for global triggers GT_COLLIDE_ITEM or GT_COLLIDE_SLOT.

- Pushing item collision: This collision works like Bound Box Collision but in this case it's necessary also that an object pushes other object to give a positive esite. Theorically this collision could be considered more precise of common Bound Box Collision.
If you add the FGT_MOVING_COLLISION flag the global trigger will use own this type of "pushing"  collision.

- Mesh on Mesh collision: this is the more complex and precise computes for collisions: each mesh of first moveable will be compared (using sphere mesh) with each mesh of second moveable to detect a collision. It performed only in very seldom circustances, for example for collision of lara with some traps, like blades and swords.

## FGT_REMOVE_INPUT
Used in GlobalTrigger command.
This flag works only with GT_GAME_KEY1_COMMAND, GT_GAME_KEY2_COMMAND or GT_KEYBOARD_CODE global triggers.
If you add in Flags field the FGT_REMOVE_INPUT flag, when the condition is true, the just received game/keyboard command will be removed and game engine will be not able to detect it.
In this way you can filter or redirect some game command.
The filtering is when you discover when a given game command has been received and you add (with your triggergroup of globaltrigger) some further actions in according with that command.
The redirection is when you want remove fully the effect of that game command and you want replace them with other your effects.

## FGT_REPLACE_MANAGEMENT
Used in GlobalTrigger command.
This flag says to trng engine to abort the default procedure in tomb engine used to manage the event (the global trigger) you intercepted.
This flag should be used when you wish use you triggergroup (launched by your globaltrigger command) to manage that event and you don't wish tomb engine interferes with your operations.
Currently the only global trigger that uses this flag is GT_SELECTED_INVENTORY_ITEM

## FGT_SINGLE_SHOT
This flag set the current global trigger for single execution. When the global trigger will be executed it will be disabled to avoid further executions.

## FGT_SINGLE_SHOT_RESUMED
Used in GlobalTrigger command.
This flag is similar than FGT_SINGLE_SHOT flag, but with a big difference:

While the FGT_SINGLE_SHOT performs first time the trigger group when the condition is true and then it will be disabled forever, the new FGT_SINGLE_SHOT_RESUMED will be resumed when the condition results newly false. For this reason the FGT_SINGLE_SHOT_RESUMED works in the reality like a multi shot flag but avoiding to perform continuosly the trigger group if the condition results true continuosly.

We could describe the different working mode with this little table:

Result of Condition  multi-shot (no flag)   FGT_SINGLE_SHOT   FGT_SINGLE_SHOT_RESUMED
--------------------------------------------------------------------------------------
   FALSE             No-Run Enabled          No-Run Enabled    No-Run Enabled
   TRUE              Run Enabled             Run Enabled       Run Enabled
   TRUE              Run Enabled             No-Run Disabled   No-Run Enabled
   TRUE              Run Enabled             No-Run Disabled   No-Run Enabled
   FALSE             No-Run Enabled          No-Run Disabled   No-Run Enabled
   FALSE             No-Run Enabled          No-Run Disabled   No-Run Enabled
   TRUE              Run Enabled             No-Run Disabled   Run Enabled
   TRUE              Run Enabled             No-Run Disabled   No-Run Enabled
--------------------------------------------------------------------------------------

## FLI_DISTANCE_IN_SECTORS
Used in LogItem= command.
When you set the FLI_SHOW_DIFFERENCES flag, the distance between Lara and logged item, it will be drawn, by default, in game units, where one sector are 1024 game units.
If you wish having the distance showed in sector (pratically, divided by 1024) you can add also the FLI_DISTANCE_IN_SECTORS flag.

Note: this flag it has been thought to have a rapid info about the distance between lara and a given static item (using also FLI_STATIC_ITEM flag) to set the StaticMIP= script command for that kind of item.

Pratically, if you have to discover how many sectors of distance will be required to get almost invisible a given static item (to set that value in CLimit with -1 CStaticSlot field, to skip its drawing) you could enable diagnostic using logitem for that static and then verifying (when the StaticMip script command is not yet present in script.txt) at what distance that static will be already not visible.

## FLI_SHOW_BOUND
Used in LogItem= command.
With this flag in data of item will be added also the current size and coordinates of 3d bounding box of Item.
These infos could be useful when you are not sure about the size of item you are testing.

## FLI_SHOW_DIFFERENCES
Used in LogItem= command.
Setting this flag you get on screen also the difference between lara and the item of LogItem.
These values are similar to those required in TestPosition command so you can use these values as reference to type reasonable ranges in TestPosition command.
Remark: the differences for Orienting are exaactly the same used in TestPosition command, while for coordinate distances (x,y,z) the values showed on screen don't consider the relative axis used in TestPosition but they will be simply showed as absolute differences.
The difference between absolute and relative distance is the orientation of lara. If lara and item are both on same axis (for example Z axis) the absolute difference for Z will be the same of relative difference.
See description of TestPosition command for more infos.

## FLI_STATIC_ITEM
Used in LogItem= command.
Add this flag when you use a static index rather a moveable item index, in LogItem command.
The infos drew on screen will be the same used for moveables but, of course, some values about animation, frames ect, will have a useless (null) value.

## FMIR_ADJUST_X
Used in MirrorEffect command
You can add this flag to the index of an animating when the reflex of the item has a bad position. This could often happen in the floor mirror when the animating has not the pivot at the center of the mesh but in a corner or over a side.
This problem happen very often with the doors.

When you use this flag the engine tries to analyse the collision box of the item and then to adjust the X coordinate of the reflex.

Note: the x axis is in south/north direction in the ngle view.

## FMIR_ADJUST_Z
Used in MirrorEffect command
You can add this flag to the index of an animating when the reflex of the item has a bad position. This could often happen in the floor mirror when the animating has not the pivot at the center of the mesh but in a corner or over a side.
This problem happen very often with the doors.

When you use this flag the engine tries to analyse the collision box of the item and then to adjust the Z coordinate of the reflex.

Note: the z axis is in west/east direction in the ngle view.

## FMIR_ALTERNATE_REFLEX
Used in MirrorEffect command
This flag may be added to the index of an animating when in a first attempt you remained displeased about its reflex.
The technical explanation is that there are two different ways to simulate the reflex of an item. If first method (less FMIR_ALTERNATE_REFLEX) is bad, you can add the FMIR_ALTERNATE_REFLEX, rebuild the script to see if now it's better.
Like general rule the animating with a good reflex had at least two opposite sides that are specular between them.

## FMOV_ANVIL_GRAVITY
used in Paramaters=PARAM_MOVE_ITEM script command
This flag enable the max level of (downstairs) gravity.
The "anvil" name has been chosen as symbol of very very heavy item, in this logical you should think to anvil like it had a heavier weight than man (in spite it is not true, probably).

## FMOV_APOLLO_GRAVITY
used in Paramaters=PARAM_MOVE_ITEM script command
This flag move up the item with a slow vertical speed for a short time, than it will reach the max speed and it remain to max speed always upward.
You can change the max vertical speed with a percentage of variation to type in Speed field.
See the description of FMOV_FROG_JUMP_GRAVITY to know how compute the formula to type in that field.

## FMOV_APPLE_GRAVITY
used in Paramaters=PARAM_MOVE_ITEM script command
This is most common gravity. Begin slowly and increase fastly until to reach a middle vertical speed downward, and then increase only slowly this middle value. Like an apple falling from the tree.
You can change the middle value of vertical speed with a percentage of variation to type in Speed field.
See the description of FMOV_FROG_JUMP_GRAVITY to know how compute the formula to type in that field.

## FMOV_CAR_SPEED
used in Paramaters=PARAM_MOVE_ITEM script command
This flag will move the item, beginning from a slow speed that will be increased gradually, until to reach a max speed that will remain constant.
The value you type in Speed field will be used as percentage to change the preset speed of this effect.
If you type 100, no change will be performed. 200 will double the speed, 50 will reduce by half the preset speed. Ect.

## FMOV_EXPLOSION_GRAVITY
used in Paramaters=PARAM_MOVE_ITEM script command
This flag will move the item upper at start with the highest speed, and then it will decrease fastly.
You can change the max begin speed using percentage of variation in Speed field. See the description of FMOV_FROG_JUMP_GRAVITY to know how compute the formula to type in that field.

## FMOV_EXPLOSION_SPEED
used in Paramaters=PARAM_MOVE_ITEM script command
This flag will give to the item a speed beginning from max value and decreasing enough slowly (remaining therefor on high values) in the progress.
This happen when an explosion throws awyay all items closed to it.
Note: warning there is no check about collisions. The item will stop its race only when it reaches the distance you set in Distance field.
The value you type in Speed field will be used as percentage to change the preset speed of this explosion movement.
If you type 100, no change will be performed. 200 will double the speed (unsuggested, increase this speed), 50 will reduce by half the preset speed. Ect.

## FMOV_FROG_JUMP_GRAVITY
used in Paramaters=PARAM_MOVE_ITEM script command
This flag will be move upward the item and then it will fall down, like a jump of a frog. The shape of the jump should be regular, i.e. the ascendent (beginning) part, will have the inverse shape in descendent final part.

Note: you can type a percentage variation of vertical speed in the Speed field using following formula:

PercentageOfChangeVerticalSpeed * 256 + HorizontalSpeed

Remember that you can not type a value higher than 255.

The value of PercentageOfChangeVerticalSpeed  work around the 100 value, that should be "no change".
If you wish double the vertical speed, use 200, while if wish the half of preset vertical speed, you'll use 50.

## FMOV_HEAVY_ALL
Enable all heavy triggers that item meets in its path. You can use this feature to create a chain of events in game.

## FMOV_HEAVY_AT_END
Enable further heavy trigger in final sector of the path.

## FMOV_IGNORE_FLOOR_COLLISION
Used in Parameters=PARAM_MOVE_ITEM script command
When the movement affects a new direction or speed or gravity mode (new management from 1.2.2.7 version), by default the engine will forbid to the item to "sink" in the floor.
This means that when you set a UP/DOWN direction or a gravity mode that move downward the item, when it will reaches the floor it will be stopped.

Also with horizontal movement, if the item reaches a point of floor that is higher of its position, the engine will move upward the item to stand over the floor.
Anyway if you wish that the item was able to sink in the floor you can add the FMOV_IGNORE_FLOOR_COLLISION flag and the engine will not perform any control about floor collision.

## FMOV_INFINITE_LOOP
Used in Parameters=PARAM_MOVE_ITEM script command
Adding this flag the movement will be endless and for this reason you'll have to use some flipeffect trigger to stop it (see F177, F178, F179 triggers)

For most directions (DIR_ values) the FMOV_INFINITE_LOOP flag will invert the direction once it has been covered the given distance (typed in Distance field), anyway with some particular DIR_ values, this flag will work in a different way:

With all turning directions (DIR_TURNING_LEFT_90, DIR_TURNING_LEFT_45, DIR_LU_TURNING_180, DIR_TURNING_RIGHT_45, DIR_TURNING_RIGHT_90, DIR_RU_TURNING_180) the FMOV_INFINITE_LOOP flag, it will do continue the same movement with new turnings every linees of distance length. Using different values for turning speed, horizontal speed and distance, you can get that an item drew a circle, an elipse, a rectangle or a square, changing its facing in according with current direction.

With al "head_for" directions (DIR_HEAD_FOR_LARA, DIR_HEAD_FOR_LEADING_ACTOR, DIR_HEAD_FOR_EXTRA_ACTOR) the FMOV_INFINITE_LOOP flag it will continue to follow the given target (Lara, Leading actor or extra actor), updating the direction everytime it had completed the given distance. For instance, mixing the FMOV_INFINITE_LOOP flag with DIR_HEAD_FOR_LARA direction, you get a sort of little robot that will follow lara in any her movement, loosing the contact with her only in the linee of distance length, moving newly head for lara at end of that distance gap.

For all other directions, the FMOV_INFINITE_LOOP flag, will invert direction at end of covered distance.

Note: with all movement having a non constant speed or gravity simulation, the FMOV_INFINITE_LOOP flag will keep the max reached speed o downward gravity with no new acceleration at beginning of new cycle. The only expection is for FMOV_FROG_JUMP_GRAVITY flag, that will repeat the moving up-down gravity endless, getting the idea of a frog that will continue to jump.

## FMOV_LEAF_GRAVITY
used in Paramaters=PARAM_MOVE_ITEM script command
When the item is far from the floor, it will move down very slowly with no accelerations, like a light leaf moving slowly down.
Note: the only collision checked is for floor, not for other items or walls or ceiling.
Note: you can change the preset vertical speed typing a percentage of variation in Speed field. See the description of FMOV_FROG_JUMP_GRAVITY to know how compute the formula to type in that field.

## FMOV_MAGNET_SPEED
used in Paramaters=PARAM_MOVE_ITEM script command
This flag will set a non costant horizontal speed for the item.
The item will begin slowly to increase its speed, until to increase alwayws faster this speed. Like an item attracted from a (far) source.
The value you type in Speed field will be used as percentage to change the preset speed of this effect.
If you type 100, no change will be performed. 200 will double the speed, 50 will reduce by half the preset speed. Ect.

## FMOV_MAN_GRAVITY
used in Paramaters=PARAM_MOVE_ITEM script command
This flag will move down the item when it's above the floor.
The vertical speed will be about that a big man falling in the empty.

## FMOV_TRIGGERS_ALL
Enable all common triggers that item meets in its path. The "common" triggers are the triggers enabled by Lara. You can use this flag to do work this item like the Mechanical Scarab to enable some trap.

## FMOV_USE_EXTRA_ACTOR_INDEX
Used in Parameters=PARAM_MOVE_ITEM script command
With this flag you replace the given value in IndexItem field with the index of current enemy set as extra actor.
This is an interesting way to use over and over the same parameter=PARAM_MOVE_ITEM to do move different enemies. Just you set as extra actor a different enemy and you can use again the same PARAM_MOVE_ITEM to move also this new enemy.

## FMOV_USE_LEADING_ACTOR_INDEX
Used in Parameters=PARAM_MOVE_ITEM script command
With this flag you replace the given value in IndexItem field with the index of current enemy set as leading actor.
This is an interesting way to use over and over the same parameter=PARAM_MOVE_ITEM to do move different enemies. Just you set as leading actor a different enemy and you can use again the same PARAM_MOVE_ITEM to move also this new enemy.

## FMOV_WAIT_STAND_ON_FLOOR
Used in Parameters=PARAM_MOVE_ITEM script command
By default, the moving will be completed when the distance supplied in Distance field has been reached.
Anyway, when you set some kind of gravity, and once covered the distance the item is yet above floor, falling down, the stop of moving affects a weird effect: the item will stop in the empty, at some height from the floor.
If you wish avoid this situation you can add the FMOV_WAIT_STAND_ON_FLOOR flag, and the movement will be stopped only when, other that the distance has been covered, the item has reached the floor, falling down.

## FMV_FADE_OUT
Used in Customize=CUST_FMV_CUTSCENE command.
If you wish there was a fade out before start the FMV you can add the FMV_FADE_OUT in FlagsFMV field of Customize=CUST_FMV_CUTSCENE command.
Remark: fade-out means the game will become darker until to black and only after this changing the video will be played.

## FMV_LONG_BLACK_RESTART
Used in Customize=CUST_FMV_CUTSCENE command.
This flag is very alike than FMV_SHORT_BLACK_RESTART, so read also the description of that flag to have more infos.
This long version of black restart, remains with a black screen for one second after the end of FMV.
There are some reasons to use this long black restart (instead of shorter version).
Some changing in game required a bit of time to be engaged. For example a rope just enabled will have a starting animation, a change of position of lara will require a moving of follow-me camera to look lara correctly.
If the changes you perform after the FMV require some time you can use the long black screen to show to player only final and stable game position.

## FMV_NO_AUDIO_RESTART
Used in Customize=CUST_FMV_CUTSCENE command.
By default, trng stops the cd track before starting the FMV, and restart the same cd in same position when the FMV has been completed.
In some cirucstance you could wish to avoid to restart the cd track at end of fmv , this could happens  for example when you mean change the cd track at end of fmv or load another level.
In above situation the cd track just restarted should be immediatly newly stopped to be changed with new track. Since this operation is a bit boring to listen you can use the FMV_NO_AUDIO_RESTART flag to let the silence at end of fmv, so you can start new different cd track with some trigger in more sweet way.

## FMV_PRE_CACHE
Used in Customize=CUST_FMV_CUTSCENE command.
This flag try to fast the start of first fmv played in current game session.
It used like a PRELOAD flag for images, but in this case the fmv will be NOT loaded in memory (because the fmvs could be too big), but simply it will be inserted in Windows cache.
Pratically it happens this situation: when a file (like a fmv file) has to be loaded from a folder where the user had not yet done any access, Windowd spends a bit of time to scan whole folder and locate the file. Differently, when a first access had been already done, windows load more fastly that file.
So, to insert the TRLE\FMVs folder in windows cache you can use this flag, and trng will perform a first access to FMVs folder while the current level (where there are fmvs) is being to load.

## FMV_SHORT_BLACK_RESTART
Used in Customize=CUST_FMV_CUTSCENE command.
With this flag when the fmv has been completed the screen will be blacks for two frame (less than 1/10 of second).
In some circustance it's useful have this black screen to avoid that player can see the game image was on screen first of fmv playback.
This situation could be a problem when you want modify the game situation immediatly after the FMV.
Using the FMV_SHORT_BLACK_RESTART in according with a global trigger GT_FMV_COMPLETED, you can perform some trigger to change game situation after the movie, avoiding that player can see the old game screen.

Remark: in spite of GT_FMV_COMPLETED global trigger, if you omit a black restart the old screen game could be visible for 1/30 of second. Therefore everytime you wish modify the game screen after a FMV it's necessary add a BLACK RESTART flag, short or long.

## FO_DEMO_ORGANIZER
Used by Organizer command.
This flag changes drammatically the operative mode of current organizer to work in according with the demo.pak whom index you set in Parameter field of Organizer= script command.

A demo organizer will be used to perform some triggers in precises frame of linked demo.pak while it'is playing (or recording).
The target is to create a cutscene.

The main differences of demo organizer are:

- The FO_ENABLED flag, will be ignored. The organiser will be enabled when the demo (whom id has been typed in Paramter field of organizer) has been started. It will be the demo to start byself the organizer and never viceversa.

- The FO_LOOP flag will be ignored.

- The FO_TICK_TIME flag will be ignored, anyway a demo organizer works always in tick frames (and never seconds) but the way to type the tick frames is now different (see next paragraph)

- The "time" field of time/triggergroup pair array, now works not like distance-time from previous trigger, like it happened in common organizer, but now it works as absolute frame from beginning of the demo.
For instance if you have a demo with a length of 100 seconds for 3000 frames, if you wish perform a trigger at first frame and another to last frame you'll type the time/trigger array with following values:

0, 1, 3000, 2

Where in above example we'll perform triggergroup 1 at start, and triggergroup 2 at end of the demo.

Note: remember that you can discover the precise frame in a demo enabling the diagnostic for demo recording with the

Diagnostic= ENABLED
DiagnosticType= IGNORE, EDGX_RECORDING_DEMO

script commands.
And in game, when you play a demo you can put in pause it and read the current frame keeping down the F8 key.

Remarks:
- If you think to launch a common organizer in same moment you perform a demo the result could be leak, because the demo works also in inventory and all paused menu, changing its itnernal frame counter, while the common organizer are frozen while there is inventory.
Other to this difference, the frame counter used by common organizer is into the draw frame cycle, while that of a demo organizer is into the read-input cycle. In some cirucstances these two cycles have a different frame amount. For above reason you cann't syncronize very fine a common organizser with a demo.

- Theorically the last valid frame index inside a demo is given to AmountOfFrames - 1, so in above example it should be 2999 and not 3000. Anyway is acceptable also using the total amount of frames. The difference between Amount-1 and Amount is very lite, the frame in game it's the same, but the Amount-1 frame is yet in demo-playing mode, while the Amoun frame index is the first frame after the quitting of current demo. This difference it's meaningful only in the case of condtion triggers working on �demo is playing� status.

## FO_ENABLED
By default the organizer is disabled at start, and you have to call a flipeffect to enable it. If you want this organizer was enabled from start of level you have to add then FO_ENABLED flag.

## FO_LOOP
Adding FO_LOOP flag, the list of (time + PerformGroup) will be performed in endless way. After that the last  Performgroup has been performed, the organizer will restart from first performgroup.
Using this flag you can perform some performgroup each "number of seconds"
Remember you can use some flipeffects to start or stop an organizer, this feature is useful own when you want perform in loop mode an organizer only for a specific section of game or in some time interval.

## FO_TICK_TIME
Change time unit misure. By default, i.e. if you DON'T use this flag, the time is in seconds, but, if you want use a more precise units of time you can add this flag  in "Flags Organizer" field of Organizer, and the time you type will be interpreted like frame ticks, i.e. the minimum time unit.
The tick frame is 1/30 of second so if you mean set also some big time, to convert in seconds just you type the number of seconds * 30.
For example:
15 = half second
300 = 10 seconds

## FRB_ALLOW_DRIFT
Used with Customize=CUST_ROLLING_BOAT command
When you enable swinging or pitching on some boat you can enable on them the drift effect, too.
The drift is when the boat moves byself a bit its position in the time, sliding in some direction.
In spite this effect is a nice simulation of real beahavior of the non-moored boats, you should take care using this effect because the boat, from the start position where you placed it in the level map, it could move very far from that start point in the time necessary to the player to reach it.
So in this case you should place some barrier to stop the drift movement at least in this start point.
It's not easy to know or set the direction of drift, because this movement borns like collateral effect of pitching/swinging of original tr2/3 code.
Probably, changing the value of pitch/swing speed and heightness of the rolling, you could change also the direction of the boat. In some (few) experiment of mine, the boats move in direction of them left side but this could change using other setting for rolling effect.

Remark: while for rubber boat and motor boat there is a native procedure for drift, for the kayak (and further fake boats in animating slots) the drift procedure has been created in trng engine.
This drift simulation, to avoid risks to move kayak in positions not reachable from lara to go in newly in the kayak, tries to avoid problems keeping the kayak to at least one sector from walls (or beatch) and it avoids also to move in low depth water, one click or less.
Knowning above features you could creare an invisible barrier to stop drift from start position simply creating a low water around the kayak, or walls in at least three sides around the kayak and in this way the drift will be temporary disabled.

## FRB_PITCHING_HIGH
Used with Customize=CUST_ROLLING_BOAT command
Set an high level for boat pitching. The boat will have a large movement.

## FRB_PITCHING_LOW
Used with Customize=CUST_ROLLING_BOAT command
Set a low level for boat pitching. The boat will have a very little movement.

## FRB_PITCHING_NORMAL
Used with Customize=CUST_ROLLING_BOAT command
Normal level for pitching.
Remark: if you omit to type any FRB_PITCHING_... value, the NORMAL will be used by default.

## FRB_SWINGING_HIGH
Used with Customize=CUST_ROLLING_BOAT command
Set an high level for boat swinging. The boat will have a large movement.

## FRB_SWINGING_LOW
Used with Customize=CUST_ROLLING_BOAT command
Set a low level for boat swinging. The boat will have a very little movement.

## FRB_SWINGING_NORMAL
Used with Customize=CUST_ROLLING_BOAT command
Normal level for swinging.
Remark: if you omit to type any FRB_SWINGING_... value, the NORMAL will be used by default.

## FROT_LOOP
Perform an infinite rotation. When you use this flag the Angle rotation value will be ignored since the item will round continuosly.

## FR_ADD_DRIPS_TO_LARA
Used in Customize=CUST_RAIN command.
You can enable the drips on lara after she has been under the rain using this flag.
By default there is no drips when lara is under the rain but only after lara has been in a pool.

## FR_CORRECT_SPRINKLERS
Used in Customize=CUST_RAIN command.
In default code the elaboration of sprinklers (when the rain drop touches the ground) was a bit leak. If the room had an irregular floor (not flat at same height) many sprinlers were not visible because they were generated under ground.
If you want correct this bug you can add the FR_CORRECT_SPRINKLERS flag.

## FR_PLAY_SFX
Used in Customize=CUST_RAIN command.
You can enable the playing of a sound effect when the drops hit the floor.
You should type the number of sound effect to play in Extra field of Customize=CUST_RAIN command.
Remarks:
- By default there is no sound for rain.
- It's advisable using a sound sample set to work in loop mode.

## FSB_DISABLE_ON_COMBAT
Used in StandBy command.
When you add this flag the stand-by will not be enabled while lara is in combat mode, i.e. when she extracted the weapons.
A reason to use this flag is to give to player a chance to disable the stand-by when he doesn't like it. In this way, in fact, just keep lara the the weapons in the hands to disable the stand-by.

## FSB_DISABLE_ON_CRAMPED_SPACE
Used in StandBy command.
This flag is strongly suggested when you use stand-by in native mode, i.e. with automatic activation after a time of inactivity.
Since you cann't know where Lara will be when the stand-by starts, it could happen that lara, in that moment, is in a cramped space. In this situation the rotation of camera could be very difficultous. To avoid this problem you can set the FSB_DISABLE_ON_CRAMPED_SPACE flag and in this way the stand-by will NOT begin when lara has around to her some too closed wall.
Remark: this method works fine only when the distance you set is less than 2048, while with higher values some very far wall could be not correctly computed and it could create problems.

## FSB_EXIT_ON_ATTACK
Used in StandBy command.
This settings will quit stand-by mode when lara will be hurt by some enemies or traps.

## FSB_FLIP_DISTANCE
Used in StandBy command.
This flag incluences matrix and portrait modes, changes in sweet way the distance.
The distance will be increased and decreased with a floating range from (Distance - 50%) to (Distance + 50%)

## FSB_FLIP_H_ORIENT
Used in StandBy command.
This flag do sliding the horizontal orienting of camera in a range from -45 degrees to +45 degrees.
Remark: this setting could be used only with portrait mode, since it should have no sense using it with a matrix effect, since the matrix changes continuosly the horizontal orienting byself.

## FSB_FLIP_SPEED
Used in StandBy command.
This setting changes the rotation speed in random way.
The range will be beteen (RotateSpeed - 50%) to (RotateSpeed + 50%)
It's better usign this setting only with matrix effect.

## FSB_FLIP_V_ANGLE
Used in StandBy command.
This flag changes slowly the vertical angle moving it from a minimum of (VAngle - 50%) to a max of (VAngle + 50%)

## FSB_FREEZE_ENEMIES
Used in StandBy command.
To avoid that lara was attacked during the stand-by mode you can use the FSB_FREEZE_ENEMIES flag to stop the activity of all enemies.
The baddy will become frozen until the standby is on.

## FSB_FREEZE_LARA
Used in StandBy command.
With this setting the game input will be stopped during the stand-by mode.
Since in this way the player will have no chance to move lara and therefore to exit from stand-by mode, this setting should be used only with a stand-by started with the flipeffect trigger "Perform <&
StandBy mode for (E)seconds"

## FSB_IMMEDIATE
Used in StandBy command.
Usually the camera will start from current standard position and it will move slowly to reach the position required from the settings of StandBy command.
If you wish the camera starts immediatly in new required position you can use the FSB_IMMEDIATE flag,and the camera will be moved immediatly in the new position.

## FSB_OVERLAP_AUDIO
Used in StandBy command.
When you set a valid value in AudioTrack field, you have to decide if the standby audio will replace (temporary) the current audio track, or if the new audio will be overlapped to old audio without stopping it.
When you want that the standby audio was overlapped to old audio track you add the FSB_OVERLAP_AUDIO flag, differently, if you wish the old audio track was stopped before playing the new standby audio track, just you omit the FSB_OVERLAP_AUDIO flag.
Remark: reasonably, you should use the FSB_OVERLAP_AUDIO flag only when the audio track in the level is only a list of environment sounds, like wind, sea song ect, while if the level audio is a music, it's better stop it before playing the standby audio since two musics in same moment could be chaotic.

## FSCAM_DISABLE_COMBAT_CAM
Used in Paramaters=PARAM_SET_CAMERA
This flag disable the combat camera until the PARAM_SET_CAMERA is working.

## FSCAM_DISABLE_LOOK_CAM
Used in Paramaters=PARAM_SET_CAMERA
This flag disable the Look camera until the PARAM_SET_CAMERA is working.

## FSCA_ENDLESS
Used with Parameters=PARAM_SCALE_ITEM
This flag should be used only with dynamic effects. You add the FSCA_ENDLESS flag to create a continue inflating/deflating of the item, like a pulse effect. Once the item will have reached the final size, its size will go back to the BeginSizePercentage and go on, forever.

## FSCA_IMMEDIATE
Used with Parameters=PARAM_SCALE_ITEM
With this flag you set an immediate scaling with no dynamic effect.
In this situation the BeginSizePercentage and PercentageSpeed fields will  be ignored (you can type IGNORE in them) and the item it will be immedialty resized to the size set in FinalSizePercentage field.

## FSCA_ITEMGROUP_INDEX
Used with Parameters=PARAM_SCALE_ITEM
Adding this flag you can perform your scaling effect on a item group instead by a single item.
When you use the FSCA_ITEMGROUP_INDEX flag, the ItemIndex field of the PARAM_SCALE_ITEM command, it will be used like a ID for a GroupItem present in same [level] section.
For example if you have these two script commands in your [Level] section:

ItemGroup= 5, -143,-144,-154,-155 ;remember that you have to type negative indices for static items

Parameters=PARAM_SCALE_ITEM, 1, 5, FSCA_ITEMGROUP_INDEX+FSCA_IMMEDIATE, IGNORE, 85, IGNORE

When you perform the trigger to engage above Parameters command, all four items with indices: 143, 144, 154 and 155, will be immeditlay resized to the 80 % of their original size.

The usage of FSCA_ITEMGROUP_INDEX flag is advisable when you are creating a forest with many trees and you wish have a group of trees with 80% of size, other to the original 100% and others with 120 % for example.

## FSS_ANIMATE
Used with Parameters=PARAM_SHOW_SPRITE script command.
You add the FSS_ANIMATE flag to create an animated sprite where a sequence of spriters will be showed to create the effect of the animation.
You use the Extra Value field to customize the animation.
The number to type in Extra Value is given from the formula:
FrameDurate * 256 + NumberOfSprites
Where:
FrameDurate is the number of frame that each frame will remain on the screen.
NumberOfSprites is the number of sprites that forms the animation.
For example if your animation uses 8 sprites from the index = 4 and you wish that the animation had the max speed, where each sprite will remain on the screen for only one frame, you should type 4 in SpriteIndex field, while in the Extra Value you should type:  1*256 + 8 = 264 ($0108)

Remark:
There are two kinds of sequence:
Standard sequence, where the indices will grow upto the max and then it repeats from the min index. Like:
123412341234
And the for-back sequence, where the index reached the max will be decreased upto coming back to min:
1234321234321
If you wish use the for-back sequence you have to add the FSS_ANIMATE_FOR_BACK flag

## FSS_ANIMATE_FOR_BACK
Used with Parameters=PARAM_SHOW_SPRITE script command.
Used togheter with the FSS_ANIMATE flag to create an animated sprite with forward-backward sequence, where the sprite index will increase upto the max and then it will be decreased upto the min, like:
1234321234
Read also the FSS_ANIMATE flag for more infos.

## FSS_CLONE_SPRITE
Used with Parameters=PARAM_SHOW_SPRITE script command.
This flag works only togheter with the FSS_SHOW_SPRITE_GRID flag.
When you wish show same sprite many times, placed in a grid, you have to use both flags FSS_SHOW_SPRITE_GRID + FSS_CLONE_SPRITE.

## FSS_EFFECT_FROM_BOTTOM
Used with Parameters=PARAM_SHOW_SPRITE script command.
This is an effect to move the sprite from bottom of screen, upto the final position you set in OriginX, OriginY field.
In the Extra Value field you type the number of frames required to complete the movement. If you type low values you'll have a fast speed, while with big number you'll have a slow movement.

## FSS_EFFECT_FROM_LEFT
Used with Parameters=PARAM_SHOW_SPRITE script command.
This is an effect to move the sprite from the left of the screen, upto the final position you set in OriginX, OriginY field.
In the Extra Value field you type the number of frames required to complete the movement. If you type l

## FSS_EFFECT_FROM_RIGHT
Used with Parameters=PARAM_SHOW_SPRITE script command.
This is an effect to move the sprite from the right of the screen, upto the final position you set in OriginX, OriginY field.
In the Extra Value field you type the number of frames required to complete the movement. If you type l

## FSS_EFFECT_FROM_TOP
Used with Parameters=PARAM_SHOW_SPRITE script command.
This is an effect to move the sprite from top of the screen, upto the final position you set in OriginX, OriginY field.
In the Extra Value field you type the number of frames required to complete the movement. If you type l

## FSS_EFFECT_ZOOM
Used with Parameters=PARAM_SHOW_SPRITE script command.

With this effect the sprite will be showed with a zoom effect.
At start the sprite will be drawn at 1/10 of its size, then in [Extra Value] frames it will be mangnified until to reach the size and position you chose in OriginX,OriginY, Width, Height fiels.

In Extra Value field you should set the number of frames (1/32 of second) required to complete the zoom effect. If you set a low value the zoom will be very fast, while if the number of frame is high the effect will be slow.

## FSS_SHOW_SPRITE_GRID
Used with Parameters=PARAM_SHOW_SPRITE script command.
Each sprite has as max size 256 x 256 pixels but pratically, since wadmerger is not able to manage 256x256 sprites, the really used max size is 128x128 pixels.

If you wish use the show sprite features to show an image bigger than 128x128, you can use the trick of sprite grid adding the FSS_SHOW_SPRITE_GRID.
When you use this flag, the trigger will not show simply the given sprite but also all sprites following the given first, placing the others to form a grid of (GridX x GridY) pieces.
For example to have an image very defined, you could use 6 sprites, each of 128x128 pixels, placed in a grid of 3 sprites (columns) for 2 sprites (strips).
In this case you should type 3 in GridX field and 2 in GridY field.
All six sprites should have exactly the same size and the engine will place them on the screen to give the idea of a single big image.

Remark: by default, when you use the FSS_SHOW_SPRITE_GRID flag, the engine assumes that you have in your sprite slot the correct puzzles of sprites to compose the final image. This means that the engine will use for each frame of the grid, a sprite with growing index, beginning for the first given sprite index.
For example, if you set as SpriteIndex the value 3, and you chose like grid, 2 columns for 3 rows, the sprites used for each single cell of the grid will be:
Cell    Index
--------------
(1,1) = 3
(2,1) = 4
(1,2) = 5
(2,2) = 6
(1,3) = 7
(2,3) = 8
--------------

Differently, in the case you wish use the sprite grid only to clone always the same sprite, you should add the FSS_CLONE_SPRITE flag.

## FSS_TRANSPARENT
Used with Parameters=PARAM_SHOW_SPRITE script command.
If you wish having a semi-transparent image you can add the FSS_TRANSPARENT flag.
Remember that only with transparent sprite you can use the add color feature.

## FTYPE_SETTINGS
Used in ImportFile command.
This type is used from some script commands to have an external binary setting file, when the required settings for that command are too much to be hosted by the usual command arguments.
Note: The FTYPE_SETTINGS type requires always as ImportType the IMPORT_MEMORY setting.

## FTYPE_SOUND
Used in ImportFile command.
You should use FTYPE_SOUND value for all sound files with extensions .wav, .mp3, .ogg, .aiff, .mp2, .mp1 (i.e. all sound files supported from new sound engine implemented in TRNG) that you mean to use in game with the specific flipeffect trigger "Play imported sound with <&
Id ....".

## FTYPE_USERFILE
Used in ImportFile command.
You should use FTYPE_USERFILE type for each file non recognized by TRNG, or simply for each file that will be not used directly by game engine.
For example if you want give to final player of your level some .html files like introduction to your adventure, or .txt files, or image to be used only from player and not from tomb4.exe, then you should use the type FTYPE_USERFILE.
When TRNG engine find an imported file of FTYPE_USERFILE, it will ignore it, and the only operation it will perform on it, it will be to export it saving in correct folder of trle.
Remark: for this reason it's not logical to use the import mode IMPORT_MEMORY for FTYPE_USERFILE files because the player will be not able to see them.

## FT_BLINK_CHARS
## FT_BOTTOM_CENTER
## FT_BOTTOM_LEFT
## FT_BOTTOM_RIGHT
## FT_CENTER_CENTER
## FT_NARROW_CHARS
## FT_SIZE_ATOMIC_CHAR
This size is yet littler than micro char (FT_SIZE_MICRO_CHAR). The atomic size is the littlest size is able to be read with all screen resolution.

## FT_SIZE_DOUBLE_CHAR
## FT_SIZE_DOUBLE_HEIGHT
## FT_SIZE_DOUBLE_WIDTH
## FT_SIZE_HALF_CHAR
## FT_SIZE_HALF_HEIGHT
## FT_SIZE_HALF_WIDTH
## FT_SIZE_MICRO_CHAR
Differently by other ft_size contants this setting ignore all shape and preset size of current font and force alwyas the character of same size: squared and very little, the same font size used to show digits in Keypad switch

## FT_SIZE_NO_BORDER
Unused. It worked with old print text method for special nationalizated characters but now it has no effect.

## FT_TOP_CENTER
## FT_TOP_LEFT
## FT_TOP_RIGHT
## FT_UNDER_LEFT_BARS
The text will start from top left corner of screen, under the lowest bar (Damage bar)

## FT_UNDER_RIGHT_BARS
The text will start from top right corner of screen, under the lowest bar (Cold bar)

## GTD_IGNORE_HEIGHT
Used in parameter field of GlobalTrigger command when you use GT_DISTANCE_FROM_ITEM or GT_DISTANCE_FROM_STATIC global triggers.
See the description of above global triggers for more infos.

## GT_AFTER_RELOADING_VARIABLES
Used in GlobalTrigger= command.
This condition will be true when player has just loaded a savegame, or when lara enters in a new level using a finish trigger with no ResetHub command.
If you had saved some critical memory in variables with GT_BEFORE_SAVING_VARIABLES global trigger, you can now restore them, when the variables has been just reloaded, to create newly the same situation previously of saving game.
In Parameter field you type the level number after the reloading of game, about when perform this global trigger. So, if lara from level 1, touchs a finish trigger and she goes to level 2, you'll have to type the number "2" to reload from begin of this level.

## GT_ALWAYS
Used in GlobalTrigger= command.
This is a special global trigger.
Usually a global trigger perform the wished TriggerGroup when it happened some specific global condition in the game.
Differently the GT_ALWAYS global trigger will be always performed (if it enabled), frame or frame of the game.
You can use this global trigger to have a triggergroup working endless.

## GT_BEFORE_SAVING_VARIABLES
Used in GlobalTrigger= command.
This global trigger will be enabled an instant before trng engine saves the variables in savegame or in HUB section (used when lara steps to new level with a finish trigger).
In the Parameter field you type the level number where perform this trigger.
It's important set the correct level number because some operation to save and restore, you could have to do only on some levels but not on oters.
For xample if you did some variable poke to modify a moveable in level 1, it's important you save its value in a specific variable, different than you could do in oter level 2 where this moveable could be missing.
Remember that the store variables are all global variables and they will be changed and reloaded for all levels.
See also the tutorial for trng variables in zip file: trng_variables.zip

You can use this global trigger to save in variables some value typed previously to change moveables or pyhsics of the game.
This operation it will be necessary only when your "poke" operation in some critical memory  it's only temporary and it will be lost when player reload a savegame. In this case you have to use this global trigger to save in some variables the changes, and then restore them with other global trigger: GT_AFTER_RELOADING_VARIABLES

## GT_COLLIDE_CREATURE
This global trigger works like GT_COLLIDE_SLOT but in this case the condition will be true for each moveable that is a "creature" type.
A creature is a moveable following AI rules, it's able to move byself.
Remark: this global trigger will ignore the value typed in Parameter field.

## GT_COLLIDE_ITEM
You can detect if lara is touching moveable with index you set in Parameter field.
For more infos read also the description of GT_COLLIDE_SLOT global trigger, and FGT_PUSHING_COLLISION flag

## GT_COLLIDE_SLOT
This global trigger works like GT_COLLIDE_ITEM but in this case you have to type a slot number both a specific item index.
Pratically you can use GT_COLLIDE_SLOT to test the collision of Lara with any item of a given slot.
For example if you create this GlobalTrigger:

GlobalTrigger=1, IGNORE, GT_COLLIDE_SLOT, 41, ...

Since 41 is the slot for BADDY_1 object, everytime lara will collide with any BADDY_1 object the global trigger will be engaged.

Remark: When the collision will be detected, the item of moveable is colliding Lara, will be saved to be used from triggers stored in perform TriggerGroup. In particular way you have to use the flag TGROUP_USE_FOUND_ITEM_INDEX togheter with some trigger that works on some moveable. For example if global trigger detect a collision with moveable with slot 41 (like in above example) and this moveable has the index = 132, this index will be used for each trigger in triggergroup where you set the flag TGROUP_USE_FOUND_ITEM_INDEX.
See also description of TGROUP_USE_FOUND_ITEM_INDEX flag for more infos.

This is a little example how the TGROUP_USE_FOUND_ITEM_INDEX flag work togheter with GT_COLLIDE_SLOT global trigger:
If you type following lines in your script.txt file:

TriggerGroup=	1, $5000+TGROUP_USE_FOUND_ITEM_INDEX, 749, $E
GlobalTrigger=1, IGNORE, GT_COLLIDE_SLOT, 43, IGNORE, 1

Lara will kill each BADDY_2 enemy simply touch him.
The number 43 is the slot for BADDY_2, while the flag TGROUP_USE_FOUND_ITEM_INDEX in TriggerGroup command has been used to substitute the original target of that trigger (it was an Action trigger to kill a crocodile) with the index of moveable found in global trigger GT_COLLIDE_SLOT, i.e. the baddy2 was colliding Lara.

## GT_COLLIDE_STATIC_SLOT
The condition is true when lara touch a static of given static slot kind typed in Parameter field.
Remarks:
* Don't confuse the moveable slots with static slots. You find the static slots in the "Statics indices" list of  "Reference" panel

* About the chance to perform a triggergroup when lara touches a specific static it's not necessary a global trigger: just you add in the ocb of that static the value 2048 and then place an heavy trigger in sector under that static. When lara will touch the static the heavy trigger will be performed.

## GT_COMPLETED_SCALING_ON_ITEM
Used in GlobalTrigger= command
This global trigger will be enabled when the item (static or moveable) with the index typed in the Parameter field of GlobalTrigger command, has been resized and its rescaling is complete.
Note: this global trigger works only for dynamic (but not endless) rescaling. Pratically when you perform a dynamic rescaling from percentage to percentage with a given resizing speed.

## GT_CONDITION_GROUP
If you don't want using a GT_ global trigger to test the condition, but you want use some common CONDITION triggers stored in some TriggerGroup command script, you have to use the value  GT_CONDITION_GROUP for GlobalTrigger field. With this value the engine will check immediatly only the conditions in trigger group you typed in IdConditionGroup field.
Remark: please don't confuse about usage of GT_CONDITION_GROUP value. You cann't add this value to other GT_ global trigger values. This value is used only when you don't want type any specific global trigger but you want to use only conditions of trigger group. Anyway, if you want use a global trigger (like GT_USED_LITTLE_MEDIPACK for example) and you want add also a condition group you can do this simply setting a valid IdTriggerGroup value in IdConditionGroup field.
Pratically the GT_CONDITION_GROUP used only to set a null-doing global trigger with a condition trigger group, because if you put IGNORE in GlobalTrigger field this global trigger will be never performed and the further IdConditionGroup it will be ignored, too.

## GT_CREATED_NEW_ITEM
Used in GlobalTrigger= command
This global trigger will be enabled everytime in game it has been created an item with same slot you typed in Parameter field.
When the global trigger will be engaged the index of new created item (always a moveable) will be stored as found item index, and you can elaborate it adding to exported triggers the TGROUP_USE_FOUND_ITEM_INDEX flag.
I remember that the items that could be created are:

FLARE_ITEM (when lara throw away it)
BURNING_TORCH_ITEM (when lara throw it. Note: it's not always "burning")
CLOCKWORK_BEETLE (when lara places it on the floor)
GRENADE (shot from Sas, from Lara and from enemy jeep)
DARTS (emitted from dart emitter. Warning: if you intercept this item you'll have a huge quantity of items to manage)
CROSSBOW_BOLT (shot from lara)

while about the new created item with slots:
PISTOLS_ANIM  (*)
UZI_ANIM   (*)
SIXSHOOTER_ANIM (*)
SHOTGUN_ANIM
CROSSBOW_ANIM
GRENADE_GUN_ANIM

the matter is complicated to explain.
The hardcoded mangament of combat animations use a "fake" item only to store current animation, frame, state id of weapon animations, in spite the real item remains only main Lara.
Anyway when you detect the creation of some of above weapon lara slots, it means that lara is loading that kind of weapon and changing animation and frame in this new fake item I suppose you could be able to affect that combat animation.
(*)Note: It seems that the method of fake item created dynamically works only for shotgun, crossbow and grenade-gun weapons, while for other littlest weapons (*) the engine swaps simply the hand mesh with no new item created.
Note: if you wish try to manage weapon animation you can use this global trigger to detect the moment of extracted weapons, but to manage it you can use also new savegame memory variables beginning with "WeaponAnim" names. See trng variables triggers.

## GT_DAMAGE_BAR_LESS_THAN
This global trigger check for level of damage and it will be checked only when lara is in a damage room.
You have to type the value to comparise in Paramater field.
In spite of type of progress bar chosen for damage room (increasing or decreasing bar) this condition will check the value of damage in same way: when lara is just entered in damage room the value of damage bar is 1000, while when she remains in damage room for some time the damage level will be littler upto 0, when lara will be damaged really or burned.
Pratically with low values the situation for lara is worse.

## GT_DISTANCE_FROM_ITEM
This global trigger permits to verify the distance between Lara and some moveable, when the distance is less than that set in Paramater field the trigger will be engaged.
Reamark: the value to type in Parameter field is a bit complex because it is the combination of two values: the index of moveable to check and the distance (in real units) to compare.

Note: from 1.2.2.4 version the formula is different.

The new formula to use is:

MoveableIndex + (RealUnitDistance * 8192)

For RealUnitDistance we mean the units used in the game, instead using the number of sectors or clicks.
For example, since one sector = 1024, and one click = 256, if you wish check the distance of two sectors the value will be 2048 and you type it in the formula for Parameter in this way:

ItemIndex + 8192 * 2048

Let say the itemindex = 307, the result will be:

307 + 8192*2048 = 16777523

The max value you can use as distance is 130048, that it is 127 sectors.

This distance it will be computed with trigonometric precision as distance between the two 3d points  x,y,z  of Lara and x2,y2,z2 of Item.

Anyway, if you wish ignore the different height (Y coordinates) you can add to the formula the constant value GTD_IGNORE_HEIGHT

For example in above example it will become:

130048 + GTD_IGNORE_HEIGHT

A possible reason to use the GTD_IGNORE_HEIGHT mode, is when you wish having a fine precision on whole side of an object, like a column. In this case, if you omit the GTD_IGNORE_HEIGHT value and therefor the compute enclosed also the Y value, when lara is above of some click respect to the columne she will seem like more far from it because there is yet the distance between the y coordinate of Lara and the Y coordinate of column, on the base of the floor.
Differently, if you use the GTD_IGNORE_HEIGHT value the distance will be computed only in planar way.
Remark: when you use the GTD_IGNORE_HEIGHT value you should use also a conditional trigergroup to verify if Lara is in the correct room or in a correct height limit (vertical condition trigger) to avoid that the condition resulted true also when lara is many room over or below the item but only because she is in same x,z position in planar view.

## GT_DISTANCE_FROM_STATIC
This global trigger permit to verify the distance between Lara and a specific static object, when the distance is less than that set in Paramater field the trigger will be engaged.
In the Parameter field you have to type the result of a formula including static index and the distance. See the descritpion of this formula in the GT_DISTANCE_FROM_ITEM mnemonic constant.

## GT_ELEVATOR_STARTS_FROM_FLOOR
Used in GlobalTrigger= command.
This condition verify when the given elevator has just let the given floor.
You have to type the elevator index and floor number in Parameter field.
About the mode to compact these two numbers in a single number, read the description of GT_ELEVATOR_STOPS_AT_FLOOR global trigger

## GT_ELEVATOR_STOPS_AT_FLOOR
Used in GlobalTrigger= command.
This global trigger will be enabled when the given elevator has just stopped at given floor.
You could use this trigger to open a door in front of elevator.
You type the elevator index and the floor number in Parameter field.
Since you have to put two numbers is a single field it's necessary perform a little compute to compact the two numbers in a single number.

The formula to get this unique number is:

(FloorNumber * 4096) + ElevatorIndex

You cann't type above row in that way, of course, because NG_Center doens't recognize the "*" multiply character or the parenthesis.
Differenty you have to perform the compute and then type only the final number.
For example if you want verify when the elevator with Index = 137, reachs the first floor (floornumber= 1), the compute will be:

(1 * 4096) + 137

and we'll get:

4233

Remarks:

1) At start of the level all elevators will give the GT_ELEVATOR_STOPS_AT_FLOOR event with the floor number = 1, since all elevators have to begin with the cage at first floor.
In this way if you use this global trigger to have a door opened in front of the floor where is the elevator it will work also from start of the level.

2) There is a limitation for both values to set in parameter field:
The elevator index cann't be greather than 4095
The floor number cann't be greather than 15
If you have to use number bigger than above limits you have a big problem, because the size of single field of GlobalTrigger script command don't permit to have number bigger than above limits.

## GT_ENEMY_KILLED
You can specific in Parameter field the index of moveable to monitor. When the given moveable will be killed the global trigger will be started.
Remarks:
* You read the index of moveable when, in NGLE program, you perform a single mouse click on that object, the index will be showed in a yellow box.
* Theorically you can get a trigger activation when a creature dies also with local (common) trigger Switch. If you trigger enemy with a switch trigger and then add to this trigger some common trigge to enable doors, enemies ect, when the creature will die the trigger will be activated. Anyway the problem to use this method is that you have to cover a big surface with above triggers, because it's necessary the creature when he dies, he was on sectors with switch trigger. Differently using global trigger GT_ENEMY_KILLED the trigger will be activated indifferently by corrent position of enemy.
* For this global trigger is strongly suggested to use flag FGT_SINGLE_SHOT, otherwise it, when the condition becomes true,  will performed 30 times for second, forever.

## GT_FMV_COMPLETED
Used in GlobalTrigger= command.
This global trigger will be engaged when the FMV with number typed in Parameter field has been just completed.
This global trigger is very useful when you wish change something in game only at end of a video-cutscene (FMV).
Usally you'll use a FMV to show some fact or actions about lara and for this reason it's probably you wish that, at end of movie, something in game was changed: like loading of new level, or enabling a flipmap or moving lara in other side of level.
In all above cirustances you should use this global trigger to be sure that the triggers (stored in a triggergroup called by this GlobalTrigger command) will be performed excactly at first frame following the ending of given FMV.

## GT_GAME_KEY1_COMMAND
The condition is true when the game command (jump, action, look ect) you set in Parameter field, it has been chosen from Player.

You have to type in Parameter field the wish KEY1_.. game command to detect.

You can also type a sum of many KEY1_ constants (see MNEMONIC CONSTANTS list) but you cann't type in this global trigger a KEY2_ constant. If you want detect a KEY2_ command then you should use the twin global trigger GT_GAME_KEY2_COMMAND.

Remarks:
This condition is true not yet the game command has been sent, this means that the command has not been yet elaborated by game engine when the condition is true.
For example if the command is KEY1_JUMP, lara is not yet jumping.
For this reason, if you wish, you can also remove the game command, to prevent that the game engine can receive it, using the flag FGT_REMOVE_INPUT
See FGT_REMOVE_INPUT description for more infos.

## GT_GAME_KEY2_COMMAND
This global trigger works in same way of GT_GAME_KEY1_COMMAND, but in this case you can set in Parameter field a KEY2_.. game command.
See description of GT_GAME_KEY1_COMMAND for more infos.

## GT_KEYBOARD_CODE
This global trigger detect when player hit some specific keystroke.
You have to type the scan code value in Parameter field.
You can find all valid scan codes in Reference Panel of NG Center, in the KEYBOARD SCANCODES list.

Remark:
This command should be not used to detect standard game command, you should use the GT_GAME_KEY1_COMMAND or GT_GAME_KEY2_COMMAND global triggers for this target.
In fact, you should remember that the keystrokes linked with game commands could be changed in Options screen from player, so you cann't to be sure that a given scan code was always the same to jump or roll.
Differently you should use GT_KEYBOARD_CODE trigger when you want have your custom hot key to start some extra feature. The scan code you choose should be different by all other keyboard commands that the player could choose in Options screen. For example you could select some unused function keys like F7 or F8 to start your operation.

## GT_KEYPAD_REMOVED
Used in GlobalTrigger= command.
This global trigger is the opposite than GT_KEYPAD_SHOWED flag.
When the (big) editable keypad will be removed this global trigger will be enabled.
You can use this global trigger to remove the further text show with the other global trigger GT_KEYPAD_SHOWED.

## GT_KEYPAD_SHOWED
Used in GlobalTrigger= command.
This trigger will be enabled when the KeyPad with index typed in Parameter field will be showed.
You could use this global trigger to show a text in the precise moment when the big editable keypad will be showed, to describe to player what is its target.

## GT_LARA_HOLDS_ITEM
This global trigger is true when lara is holding in her hands the item you specified in Parameter field. The value set in Parameter could be also a vehicle or pole or rope.
Remark: in Parameter field cann't type a SLOT value but an HOLD_ constant.
You can find the list of available HOLD_ values in MNEMONIC CONSTANTS list of Reference panel.

## GT_LARA_HP_HIGHER_THAN
This global trigger will be enabled when life value for lara is higher than value you type in Parameter field. The valid range for lara vitality is between 0 - 999

## GT_LARA_HP_LESS_THAN
This global trigger will be enabled when life bar of lara (Health value) is less than value you set in Parameter field.
Remember that the health value for lara in enclosed in range between 0 (she dies) and 1000 (full line and no bar showed in game).
For example if you want reduce some capabilities of lara when she has a life less than 100 hp, you can type a GlobalTrigger like this:

GlobalTrigger=1, FGT_SINGLE_SHOT, GT_LARA_HP_LESS_THAN, 100, ...

If you use a trigger like above to remove some capability of lara you should remember also to set some way to return these capabilities to lara when her hp grows newly over that limit (100), for example you can use GT_LARA_HP_HIGHER_THAN or GT_USED_BIG_MEDIPACK or GT_USED_LITTLE_MEDIPACK.

## GT_LARA_POISONED
Condition about poisoned lara. When lara is poisoned the life bar becomes yellow and in short time the screen will be deformed.
Using this global trigger you can perform some action for example to show a text, when lara is poisoned.
In Paramater field you have to type the value used as comparison. The formula used is "IF POISON_IN_LARA grather than PARAMTER then ..." perform global trigger.
If you want that the global trigger was enable for any poison value, you can type 0 (zero) in parameter field.
DIfferently if you want the activation was only when the poison level is higher you can type a big value, like 400 or more.
Some infos about the level of poison:
Darts = 160
Little scorpion = 512 (only from 4th level)
Big scorpion = 2048
Start screen Harpy = 2048
Deforming screen when poison is higher than 256
Max level for poison = 4096

## GT_LOADED_SAVEGAME
The condition will be true when player has just started the loading of a savegame from Load Game screen.

## GT_NO_ACTION_ON_ITEM
Used in GlobalTrigger= command.
This global trigger will be true when the supplied item has no trng action enabled on it.
You should type the index of the item to check, in the "Parameter" field of GlobalTrigger command.
The trng actions are procedures used to turn, move and get other time consuming effects on moveables.
Many Action triggers you find Set Trigger Type window, begin a progressive action on that moveable.
This global trigger could be useful to discover the exact moment when an item completed its movement, for example to play a sound effect, or syncronize another action starting when the first action has been completed.

Remark: since this global trigger verify if the item has no action enabled on it, you should use for this global trigger the following flags:

the FGT_DISABLED flag, to avoid it was immediatly engaged at begin of the level, since in that moment surely the item has no action in progress. So you should start with the global trigger disabled, thanks to FGT_DISABLED flag, and then you should enable the global trigger only after some action trigger has been started on it.

the FGT_SINGLE_SHOT or FGT_SINGLE_SHOT_RESUMED flags, because otherwise the triggergroup linked with this global trigger will be ontinuosly and forever performed after the last progressive action has been completed on the item.

## GT_SAVED_SAVEGAME
The condition will be true when player has just saved current game in some savegame.

## GT_SCREEN_TIMER_REACHED
You can start your global trigger when screen timer reach the supplied number of seconds. You have to type the number of seconds in following field named Parameter.
For example, to start your trigger when timer screen reach 240 seconds (4 minutes) you can type: "GlobalTrigger=1, IGNORE, GT_SCREEN_TIMER_REACHED, 240, ..."
Remark:
* For this global trigger is strongly suggested to use flag FGT_SINGLE_SHOT, otherwise it, when the condition becomes true,  will performed 30 times for second, forever.

## GT_SELECTED_INVENTORY_ITEM
Used in GlobalTrigger= command
This global trigger is similar to GT_USED_INVENTORY_ITEM but there are important differences.
While GT_USED_INVENTORY_ITEM works only with inventory items where there is no hardcoded feature and only when lara is still in stand up position, with the GT_SELECTED_INVENTORY_ITEM global trigger you detect the usage of the item, whose slot had been typed in Parameter field, first that tomb4 engine processes it.

This fact implies some situations to remember:

1) Since there is no check about current animation of lara or environment (ground, underwater, floating) it should be you, with some conditional triggergroup, to verify if it's possible in that moment using really that item.
For example, if your item should perform an animation where lara takes in her hand this item, you should verify with right conditions if lara has free hands and she is in a reasonable state-id to manage this situation.

2) The global trigger will be engaged also when there is an hardcoded management like for the binocular, medipack, weapons, or also when there is no management but, usually the tomb refuses to manage directly that item, like it happens for lasersight item.

In the case you wish replace the default management of that hardcoded item, with yours, you have to add in the flags of the GlobalTrigger command the FGT_REPLACE_MANAGEMENT flag.
See the description of FGT_REPLACE_MANAGEMENT flag for more infos.

## GT_TITLE_SCREEN
Used in GlobalTrigger= command.
This global trigger should be used only in [Title] section.
With GT_TITLE_SCREEN you can discover what kind of screen is showing in title phase: if main titles, new game, load game, or options.
You have to type in Parameter field a TSCR_ value for wished screen type.

## GT_TRNG_G_TIMER_EQUALS
Used in GlobalTrigger= command.
Enabled when the Global trng timer has same value set in Parameter field.
The value you type in Parameter field is in frame ticks, where 1 second = 30 frame ticks.
So if you want detect when a growing timer reaches 40 seconds you have to type "1200" , since 40 * 30 = 1200.

Remark: This global trigger works ONLY while the respective timer is running, this means it's not necessary (and neiter advisable) to use FGT_SINGLE_SHOT or FGT_SINGLE_SHOT_RESUMED flags.
In the case you wish check the variation of trng timer performed in a way different than standard flipeffect "Variables. Timer. Start the <&
TRNG Timer in (E)Mode" you should avoid these global triggers and use instead the GT_CONDITION_GROUP global trigger, placing in the ConditionTriggerGroup the id of a triggergroup with exported CONDITION triggers to test the content of variable TIMER.

## GT_TRNG_L_TIMER_EQUALS
Used in GlobalTrigger= command.
Enabled when the Local trng timer has same value set in Parameter field.
Read the description of GT_TRNG_G_TIMER_EQUALS global trigger for more infos.

## GT_USED_BIG_MEDIPACK
Trigger enabled when lara select and use a big medipack.
Remark: if Lara has already full life and player select a big medipack from inventory the condition will be false, because in this situation the medipack will not really used.

## GT_USED_INVENTORY_ITEM
With this global trigger you can detect when the player chooses a specifc item from inventory. You have to specify the slot id in following field  named Parameter.
For example to enable a global trigger when player select the QUEST_ITEM2 with slotid = 253, you can type "GlobalTrigger=1, IGNORE, GT_USED_INVENTORY_ITEM , 253, ...".

Remarks:
*If you want remove the chosen item from inventory when global trigger has been engaged you have to insert in perform TriggerGroup the specific flipeffect "Inventory-Item. Remove <&
inventory-item from inventory". If you don't use this flipeffect the selected item will remain in inventory for further activations.

*Please, try to avoid to use QUEST_ITEM1 for your global trigger because QUEST_ITEM1 is used already for Detector activation.

## GT_USED_LITTLE_MEDIPACK
Trigger enabled when lara select and use a little medipack.
Remark: if Lara has already full life and player select a little medipack from inventory the condition will be false, because in this situation the medipack will not really used.

## GT_USING_BINOCULAR
Trigger enabled when lara is or not looking using binocular.
WARNING: you have to place a correct value in Paramter field. If you want check the condition for lara is using binocular you must type "128" or "$80" in Paramter field, while if you want check the condition for lara is NOT using binocular, you'll type the value 0 (zero) in Paramter field.

## GT_VSCROLL_COMPLETE
This global trigger is true when the vertical scrolling text of given string index has been completed.
In Parameter field you should type the index of extra ng string used for vertical scrolling text to monitor.
Remark: The most common usage of this global trigger is when you want create following operation:
Showing a long scrolling text on screen and freeze lara in the meanwhile disabling the keyboard input.
In this situation you can use this global trigger to enable newly the keyboard input when the scrolling text is complete.

## GT_VSCROLL_LAST_VISIBLE
This global trigger will be true when last row of vertical scroll text is fully visible on screen.
Differently from GT_VSCROLL_COMPLETE, with GT_VSCROLL_LAST_VISIBLE the condition is true when vertical scroll is not yet completed but the last row of text entered in visible area of screen.
In Parameter field you have to type the index of NG string used for scrolling operation you want check.
You can use this global trigger to create a chain of different scrolling operations with different size and color of character.
For example you can create a text with a singe row using very big characters.
You'll use this single row as a big title of following scrolling text with littler size of text.
In above situation you can use a GT_VSCROLL_LAST_VISIBLE global trigger linked to title scrolling, to show the remaining text (other scrolling text) when the title is visible on screen. The final effect will be to watch a single long text using different size or color of character but in the reality it will be two different scrolling texts working in same moment.

## HAIR_ONE_PONYTAIL
Set the hair look of adult Lara, with a single ponytail from nape.

## HAIR_ONE_TR5_PONYTAIL
Set the hair of adult Lara of tomb raider chronicle type. In tomb raider chronicle the pony tail has its hook in a position a bit different than that of tomb raider last revelation.
Remark: In the reality, in tomb raider chronicles there are three models of Lara: young lara, adult lara1  and adult Lara2. The adult lara1 has same joint as lara of the last revelation, while lara2 has a different joint for ponytail.

## HAIR_PAGE_BOY
This hair mode disable any type of plaits or ponytails. Pratically this hair mode disable floating hair for Lara.
If you set this value you should cover the hole that lara has on her nape-neck, changing the mesh of lara's head and screaming head.

## HAIR_TWO_PLAITS
Set the hair look of young lara in angkor wat.
The only reason to use this value is when you wish to have young lara with all weapons.
To realize this target you should follow this list:
1) Type in corresponding [Level] section these two commands:
Customize=	CUST_HAIR_TYPE, HAIR_TWO_PLAITS
Customize=	CUST_DISABLE_SCREAMING_HEAD
2) DON'T type any YoungLara= command, otherwise lara will have no weapons, while we want that the engine believed that lara is adult lara.
3) Start wad merger and load in left side your main wad, that you are using for your level. It's important that this level had all slots for adult lara.
4) Now load in right side the wad angkor.wad.
5) Copy ONLY these two slots: LARA_SKIN and LARA_SKIN_JOINTS

Now your level will have the young lara with all weapons, flare and crowbar animations working correctly.

## HIT_BIKE_EXPLODE
Used with CUST_BIKE_VS_ENEMIES customization.
In this case it will be the bike to explode and lara will be killed, of course.
You can mix this flag with HIT_EXPLODE flag to have a double explosion: of the item and of the bike

## HIT_EXPLODE
Used with CUST_BIKE_VS_ENEMIES customization.
With this customize the enemy hit from the vehicle will explode at first impact.
You should use this flag only when you wish kill an immortal enemy that has no dying animation.

## HIT_HURT
Used with CUST_BIKE_VS_ENEMIES customization.
The enemy hits from the bike will be injured. The grade of damage will depend by the speed of the bike at the moment of the impact.
Note: this flag doesn't work with immortal creatures.

## HIT_KILL
Used with CUST_BIKE_VS_ENEMIES customization.
The enemy touched by the bike will be killed

## HIT_PUSH_AWAY
Used with CUST_BIKE_VS_ENEMIES customization.
The enemy will be pushed away from the bike. When the bike was very fast the enemy will fly for some distance.
The distance and speed of pushed enemy will depends from many factors: the speed of the bike, the angle of impact and the size of the object. When the object is bigger it will be moved to a shorter distance.
Remarks:
You can use this flag also with moveable that are not enemies. For example you can use it with animatings.
About the usage with  other special moveables you should take care about the possible results.
If you use it with pickups you could have trouble to pickup those items after the pusing away because their position will be not at the center of the sector.
A similar speech is for pushable objects: if they lose the alignemnt with sector grid lara could be not more able to push them in common way.
About the rollingball, it will be activated about in the direction of the impact but aligned with hortogonal directions.  Note that the activation of the rollingball with HIT_PUSH_AWAY flag will work indifferently by the OCB codes you typed in the rollingball. Another effect that the HIT_PUSH_AWAY flag will have on already activated rollingballs will be to injury lara when the rollingball is yet moving in an collisional direction.

## HIT_WALL
Used with CUST_BIKE_VS_ENEMIES customization.
The enemy will remain imperturbable to the impact with the bike.
You should use this flag when the enemy is very big or very power.
The bike will crash on it like it was a wall.
Note: it should be important that the enemy was also not too fast in his movements to use this flag, otherwise he could enter in the collision box of the bike with weird results.

## HOLD_ANY_TORCH
Work for fired or out torch in same time

## HOLD_CROSSBOW
## HOLD_FIRED_TORCH
## HOLD_FLARE
## HOLD_GRENADEGUN
## HOLD_JEEP
## HOLD_KAYAK
## HOLD_MOTOR_BOAT
## HOLD_OUT_TORCH
## HOLD_PISTOLS
## HOLD_POLE
## HOLD_REVOLVER
## HOLD_ROPE
## HOLD_RUBBER_BOAT
## HOLD_SHOTGUN
## HOLD_SIDECAR
## HOLD_UZI
## HRP_DISABLE_LASER_SIGHT
Used in Customize=CUST_HARPOON command.
If you want forbid the usage of laser sight with the harpon gun, you can add this flag.

## HRP_DOUBLE_AMMO
Used in Customize=CUST_HARPOON command.
Since harpoon weaponauses the slot of crossbow,  by default the harpoon has three different ammo types (usormal, poison, explosive). If you wish having only two ammo: normal and explosive, you can use this flag.

## HRP_NO_SWIM_UNDERWATER
Used in Customize=CUST_HARPOON command.
If you set this flag, lara will be not able to swim when she holds the harpoon in her hands.
This settings should be more realistic since when lara has here hands busy it's not logical she was able to swim undwerwater.

## HRP_SINGLE_AMMO
Used in Customize=CUST_HARPOON command.
Since harpoon weapon uses the slot of crossbow,  by default the harpoon has three different ammo types (normal, poison, explosive). If you wish having the harpoon gun classic you can use this flag to have a single type of ammo in inventory for this weapon.

## IF_CRYPTIC
Used in Image= command.
If you add this flag to your Image command, the corresponding image will be crypted and its name will be changed from: Image#.bmp  to @Image#.bmp
Once the image has been crypted (and you recognize the crypted images from the "@" character in front of their name), it will be no more possible load and view this image in common ways.
Only playing the level, the trng engine will decrypt at-fly the image to show it in game.
This crypting feature has been thought to avoid that some "old-fox" player, went to watch the images in Pix folder, in advance, to discover secret maps and other stuff about the playing of your level

Notes
-----
* It's strongly suggested to perform a backup in other folder of your original (not yet crypted) images, because, once they have been crypted, there is no way to decrypt them, and therefor you'll lose any chance to modify them.

* No decrypting tool has been supplied in ng_center, because the (above) old-fox player, could download ng_center own to decrypt the images of the level he is playing. But this chance has not been foreseen.
When you crypt an image it will be for always.

* In spite the decrypting phase, performed in game, it's very faster than the crypting process, performed into NG_Center, also the decrypting it will require a bit of time, for this reason it's better using for cryptic images also the IF_PRELOAD flag, in this way the decrypting phase it will happen at the launch of the game, while in game, when the image will be triggered, it will have been already decrypted, and therefore it will be showed fastly.

## IF_EFFECT_CROSS_FADE
Used in Image= command.
The cross fade is when there is a fade out (the screen from normal color becomes black) followed by a fade in (the screen from black comes back to normal light.
The image will be displayed in the black phase.
Note: you cann't mix the effect with other effect like zoom or moving from  sides.
Neither the transparent flag works fine with cross fade, since when the image will be displayed the tomb raider screen will be black.
The cross fade works only with overlapped images, not with pop up images.

## IF_EFFECT_FROM_BOTTOM
Used in Image= command.
This effect is similar to IF_EFFECT_FROM_TOP effect but in this case the image will enter in the screen from bottom side of scren.
See description of IF_EFFECT_FROM_TOP flag for most informations.

## IF_EFFECT_FROM_LEFT
Used in Image= command.
This effect is similar to IF_EFFECT_FROM_TOP effect but in this case the image will enter in the screen from left side of scren.
See description of IF_EFFECT_FROM_TOP flag for most informations.

## IF_EFFECT_FROM_RIGHT
Used in Image= command.
This effect is similar to IF_EFFECT_FROM_TOP effect but in this case the image will enter in the screen from right side of scren.
See description of IF_EFFECT_FROM_TOP flag for most informations.

## IF_EFFECT_FROM_TOP
Used in Image= command.
This effect move the image from top of screen upto reach the final position you set in field PosX,PosY,SizeX and SizeY.
The whole moving effect will be perfomed in the frames you set in EffectTime field.
Little values as EffectTime will produce most fast in the moving.

Remarks:

* At start of this effect the image will be whole outside of screen, for this reason you can apply this effect also for images to show in full screen size.

## IF_EFFECT_ZOOM
Used in Image= command.
With this effect the image will showed with a zoom effect.
At start the image will be drawn at 1/10 of its size, then in EffectTime frames it will be mangnified until to reach the size and position you chose with XPos, YPos, SizeX and SizeY fields.
In EffectTime field you should set the number of frames (1/32 of second) required for zoom effect. If you set a low EffectTime the zoom will be very fast, while if the EffectTime is high the effect will be slow.

Remark: this effect could slow down the game when you apply it to pop up images.

## IF_FULL_SCREEN
Used in Image= command.
This settings will show the image resizing it to cover whole game screen.
When this flag is present the fields used to set origin and size of image (XPosition, YPosition, SizeX, SizeX) will be ignored, so you can fill them with four IGNORE values.

## IF_LOOP_AUDIO_TRACK
Used in Image= command.
If you enable a background audio track for your image, using the IF_PLAY_AUDIO_TRACK flag, you can choose to perform that audio track in loop (infinite) way, adding the IF_LOOP_AUDIO_TRACK flag.

## IF_OVER_FIXED_CAMERA
Used in Image= command.
By default the images will be hidden when there is a fixed camera. If you wish keep this image on game screen also when there is a fixed camera you can add the IF_OVER_FIXED_CAMERA flag.

## IF_OVER_FLYBY
Used in Image= command.
This flag works only with pop up images.
By default when a flyby sequence begins the further pop up image will be hidden, like other special objects (keypad, detector or picked up items), but if you want have a pop up image showed over a flyby sequence you can add this flag to ImageFlags field.

## IF_PLAY_AUDIO_TRACK
Used in Image= command.
If you wish have a sound start in same moment of image viewing you can add this flag and type in AudioTrack field a number between 0 and 255 to locate an audio track of AUDIO folder.
For example if you want play the track "034.wav" you have to type 34 in AudioTrack field.

## IF_POP_IMAGE
Used in Image= command.
This flag will change fully the work mode about viewing of image.
A pop up image, in fact, will be drawn over the game screen letting the game goes on, differently, a non pop up image, will be drawn over game screen while the game is still.
For this reason, the pop up image will be usually littler than game screen, used as a sort of tv logo, while, differently, the non pop up images, could be drawn covering whole game screen, and stopping it upto the view time was completed.

I wish to precise: above is not a fixed rule but only a reasonable suggestion.
The only precise rule is that with pop up image the game will go on, while with non pop up images the game will be frozen upto the viewing of image has been completed.

## IF_PRELOAD
Used in Image= command.
By default the image will be loaded from disk in same moment it should be drawn on screen.
This operation requires sometimes some instant and the game remains froze for that short period.
To avoid this stop you can force trng to preload the image while it is loading other data of current level (when you see the loading bar on screen)
In this way the image will be already in RAM memory and it will be displayed immediatly.
Remark: since the bmp images could be very heavy (one MB or more) it's advisable don't exagerate with the number of preloaded images for same level. I suggest to remain in the complessive limit of 15 Mb of preloaded images in same level.
Differently the size of images preloaded in different levels is not cumulative since trng will discard them when a level has been discarded, of course.

## IF_QUIT_ESCAPE
Used in Image= command.
This flag works only for NON pop up images.
When you show an image that freezes the game, you can set this flag to permit at player to skip the image with Escape/Inventory keystroke.

## IF_TRANSPARENCE
Used in Image= command.
If you want was applied a transparence effect on current image you can add this flag.
The transparent zone will be  where there are pixels with rgb color = $FF00FF  (red = 255; green=0; blue=255), this is the �traditional� transparent color for tomb raider textures.
With transparent images you can create, for example,a round image over the game screen, where the non visible squared corners have been filled with transparent color.

Remarks:
* The transparency effect is a bit slow to realize, so it's advisable to use it with moderation.

* If you want place the CONVERTER.exe utility in your level files to develope the images in jpg format, you should avoid to supply the transparent image in .jpg format, because the double conversion bmp-
jpg (when you place the images in your zip file) and the final re-conversion jpg-
bmp (before playing your level) could alterate the transparent color in some side, ruining the final effect of transparency. In this situation you can simply supply the other images in .jpg format, while the image with transparency will be let in original .bmp format, in this way CONVERTER.exe (or trlm2009) will convert only other images, letting untouched your transparent (bmp) image.

* When you create your transparent image you should disable the anti-aliasing feature in photoshop because, otherwise, the line between solid color and transparent color could be alterated with colors only similar to transparent color but not exaclty precise. Only the absolutly precise color 255,0,255 will be transparent, so you should avoid any distorsion.

## IGNORE
## IMPORT_MEMORY
Used in ImportFile command.
If you select as ImportType the value IMPORT_MEMORY, the corresponding file will be loaded directly in RAM memory and it will be used only from memory.
The advantage is that the file will be started more fastly since it will be not required a disk access to find and load it in memory. This could be very important for sound files, where, in some circustances, the access from hard disk could create a little slow down in frame rate of the game.

Instead, speaking about the disadvantages, the most problem is about the usage of RAM memory, and this could be a real problem if you exaggerate to import too much and too big files in IMPORT_MEMORY mode.
For example if you tried to import in memory all audio files of AUDIO folder (from 000.wav to 111.wav) it will be used about 110 Mb of RAM memory only for imported files, then, adding the memory used from TRNG engine and other memory for other level files the risk is to have leak performaneces because Windows will be forced to use virtual memory to do run the game.
It's obvious that this problem changes in according with quantity of RAM memory installed on the current PC, anyway using as medium/low value a PC with 256 MB of RAM, I suggest to limit the total amount of memory imported files to be less than 20 Mb.

My suggestion is to reduce the usage of memory imported files only when it's strictly necessary, i.e. when you want start very fastly little sound files or images.

In other circustances it's better avoid the IMPORT_MEMORY, preferring the IMPORT_TEMPORARY import mode.
See description of IMPORT_TEMPORARY value to compare the differences.
Remark: You can import upto 200 files in IMPORT_MEMORY mode.

## IMPORT_TEMPORARY
Used in ImportFile command.
When you import a file in script.dat using this import type (IMPORT_TEMPORARY) the file will be stored in script.dat only in temporary mode and it will be immediatly exported at start of game.
For "exported" we mean that the file will created newly with original name and (further) folder.
For example if you import a file using following ImportFile command:

ImportFile=1, Help\Start.htm , FTYPE_USERFILE, IMPORT_TEMPORARY

The content of "Start.htm" file in subfolder "help" of TRLE folder, will be imported in script.dat
When the game will be started (for example in other PC where the sub-folder "Help" and the file "Start.htm" are missing) the TRNG (tomb4) engine will create newly a sub-folder named "help" and it will save the file with name "start.htm"

Pratically the temporary import mode works using the script.dat like a simple "container" where store and then retry files to install in trle folder before the game starts.
About advantage and disadvantages of temporary import, the advantage consist to have a raw installer for your trle folder, removing further problem with special files (like .ogg or images) using level manager, while the disadvantage is that there is not most speed in starting of these imported files, since they will be used in game loading them in common way from hard disk.
Remark: there is no limit about the number or the size of imported files in IMPORT_TEMPORARY mode.

## JOINT_ABDOMEN
## JOINT_LEFT_ANCKLE
## JOINT_LEFT_ELBOW
## JOINT_LEFT_KNEE
## JOINT_LEFT_SHOULDER
## JOINT_LEFT_THIGH
## JOINT_LEFT_WRIST
## JOINT_NECK
## JOINT_PUBIS
## JOINT_RIGHT_ANCKLE
## JOINT_RIGHT_ELBOW
## JOINT_RIGHT_KNEE
## JOINT_RIGHT_SHOULDER
## JOINT_RIGHT_THIGH
## JOINT_RIGHT_WRIST
## JOINT_SINGLE_MESH
## KEY1_ACTION
## KEY1_DOWN
## KEY1_DRAW_WEAPON
## KEY1_JUMP
## KEY1_LEFT
## KEY1_LOOK
## KEY1_RELEASED
## KEY1_RIGHT
## KEY1_ROLL
## KEY1_UP
## KEY1_WALK
## KEY2_DASH
## KEY2_DUCK
## KEY2_USE_FLARE
## KLH_ALL_LEVELS
Used in Customize=CUST_KEEP_LARA_HP command
You use this constant to force the keeping of current HP value for all levels where lara could jump from current level.
See the description of CUST_KEEP_LARA_HP constant for more infos.

## LDF_PLAY_TRACK
Used in Diary= command
If you wish a new background music for Diary, you can use this flag adding to it a value in the range 0 - 255 to choose the audio track of AUDIO folder to play.
For example, if you want have as background music the audio track 104.wav (or 104.mp3/.ogg) you should type in DiarySoundFlags of Diary command this text:

LDF_PLAY_TRACK + 104

Remark: the LDF_PLAY_TRACK flag doesn't stop the game audio track but it overlaps the wished new audio track for diary. For this reason it could be useful, when you use the LDF_PLAY_TRACK flag, add to LDF_PLAY_TRACK flag also the LDF_SILENT flag to stop temporary the game audio track:

LDF_PLAY_TRACK + 104 + LDF_SILENT

Remark: when the diary will quit, the previous audio track of game will be restored

## LDF_SILENT
Used in Diary= command
Setting this flag, when the Lara's Diary will be showed, the background audio track of game will be suspended. This setting could be useful when you use custom audio tracks for single pages of Lara's diary.
Remark: when the diary will quit, the previous audio track of game will be restored

## LDF_SOUND_EFFECTS
Used in Diary= command
This flag is used to enable some sound effect in diary viewing.

The sound effects are:

Click on page changing (71: GENERIC_SWOOSH)
Sound for starting zoom phase (356: LIGHT_BEAM_JOBY)
Sound for non valid change of page (2: LARA_NO)

Remark: if you want use these sounds remember to add the sound 356 ( LIGHT_BEAM_JOBY) in your wad, while for other two sounds they should be always present.

## LDF_TRANSPARENT_BKG
Used in Diary command.
This flag informs trng that, in your background images, there could be transparent zones.
The transarent color is the standard transparent color used in tomb4: $FF00FF(red = 255; green=0; blue=255)
You could use this setting if you wish that the diary had a not perfect rectangular shape. For example if you wish have a rounded shape you can place transparent color in the four corners.

Remarks:
- This setting has no effect about popup images showed in little frames of some page. The pop up images are always showed supporting the transparent color.

- The LDF_TRANSPARENT_BKG flag doens't work fine with the PL_FIX_WIDE_SCREEN flag about page layout, because, when there is a wide-screen and this flag, the image will be stretched before showing it and in this phase the transparence effect will show a distorsion between real background and new stretched background in transparent zones.

- If you use LDF_TRANSPARENT_BKG flag togheter with LDF_ZOOM_START flag, for technical reasons, the image showed in the zoom effect will have no texts and neither further pop-up image. Pratically it will be zoomed only first background images for diary, and only when zoom effect has been completed, it will be added texts and pop up images.

- If you mean set a transparent zone in the background of your diary it's necessary that all (further) background images (you can change the background for each page) had the transparency in same zones, otherwise you'll get a transparency only showing a zone of previous page instead of tomb raider screen.

## LDF_ZOOM_START
Used in Diary= command
Start Diary image with a zoom effect.

## LGTN_ADD_GLOVE_LIGHT
Used with Parameters=PARAM_LIGHTNING script command
If you add this flag, there will be a glove light around the target position while the lightning is working.

## LGTN_EXPLODE_TARGET
Used with Parameters=PARAM_LIGHTNING script command
If the target position is set using a moveable index (different than lara) or a static item, you can destroy it with an explosion.

## LGTN_FIRE_LARA
Used with Parameters=PARAM_LIGHTNING script command
If the target position is set using lara index, you can burn her adding this flag.
Note: the fire will kill her in few seconds if you set also the LGTN_KILL_TARGET flag while, whether it's missing, Lara will be damaged but she we'll have the chance to survive diving herself in the water.

## LGTN_FLASH_SCREEN
Used with Parameters=PARAM_LIGHTNING script command
You can add this flag if you wish that there was a flash in the level while the lightning is running.

## LGTN_GLOBAL_SOUND
Used with Parameters=PARAM_LIGHTNING script command
By default, if you omit this flag, the sound will be local, i.w. it will be played in the target position, and when lara is far to that point the sound will be very light.
Differently the global sound will have always same (max) volume from any point of the level.
You should add the LGTN_GLOBAL_SOUND flag only for random target (from sky) lighning, while it's better don't use it for electric conductor.

## LGTN_HEARTHQUAKE
Used with Parameters=PARAM_LIGHTNING script command
Add a short heartquake effect when Lara is closed to the target position of the lightning.
The distance to enable the earthquake is about 6 sectors or less.

## LGTN_INCLINED_RANDOM
Used with Parameters=PARAM_LIGHTNING script command
When you set IGNORE as source position, trng will choose a random position over the target position.
By default trng tries to set a source point almost perpendicular with the floor (and the target item), anyway if you wish see a more inclined lightning (in radom way), you can add the LGTN_INCLINED_RANDOM flag.
Note: you should use this flag only when the target item is far from wall and other obstacle, otherwise the lightning could pass through the walls.

## LGTN_KILL_TARGET
Used with Parameters=PARAM_LIGHTNING script command
If the target postion is set with a moveable index, you can kill him/her, adding this flag.

## LGTN_LARA_SCREAM
Used with Parameters=PARAM_LIGHTNING script command
This flag works only if the target item is Lara.
There are two kinds of scream: the long scream, if you set LGTN_FIRE_LARA, while it will be used a simple "moan" if that flag is missing.

## LGTN_PLAY_SOUND
Used in PARAM_LIGHTNING script command
Enable the playing of sfx sound typed in SoundEffect field.

## LGTN_RANDOM_COLOR
Used with Parameters=PARAM_LIGHTNING script command
If you use this flag, the color will be lightly changed, adding a value floating between 0 + 47, to each gradient (red, green, blue) different than 0 and less than 208
The 0 and 208 limitations are useful to avoid that the random changes modified too drammatically the main color you chose.
To avoid this trouble, just you set to 0 or higher than 208, the gradient you don't wish was changed, and you can set in the interval 1 / 208 the gradients you allow to be changed.

## LOAD_AMMO_TYPE1
Used in Equipment command.
If your equipment command is working on a weapon slot (weapon, not ammo), you can add to the Amount field the LOAD_AMMO_TYPE1 flag to set that the ammo is currently loaded with normal ammo (ammo type 1)
Remark: pistol, revolver and UZI can use only ammo type 1. For Shotgun the ammo_type1 is the normal ammo.

Example:

Equipment=SHOTGUN_ITEM, 1+LOAD_AMMO_TYPE1
To set that the shotgun will be present in the inventoy and the loaded ammo will be normal ammo.

If you omit the ammo type currently loaded in the weapon, the engine will use as default the ammo type 1 for that weapon.

## LOAD_AMMO_TYPE2
Used in Equipment command.
If your equipment command is working on a weapon slot (weapon, not ammo), you can add to the Amount field the LOAD_AMMO_TYPE2 flag to set that the ammo is currently loaded with ammo type 2
The weapons: shotgun, crossbow and grenade gun, can use the ammo type2

## LOAD_AMMO_TYPE3
Used in Equipment command.
If your equipment command is working on a weapon slot (weapon, not ammo), you can add to the Amount field the LOAD_AMMO_TYPE3 flag to set that the ammo is currently loaded with ammo type 3
The weapons: crossbow and grenade gun, can use the ammo type3

## LOAD_LASERSIGHT
Used in Equipment command.
If your equipment command is working on a weapon slot (weapon, not ammo), you can add to the Amount field the LOAD_LASERSIGHT flag to set that the ammo has currently loaded the laser sight.
Note: Only revolver and crossbow can use laser sight.

For example the scrit command:

Equipment= CROSSBOW_ITEM, 1 + LOAD_LASERSIGHT + LOAD_AMMO_TYPE3

It means that the crossbow will be present in the inventory. It is using ammo type 3 and it has the lasersight already mounted on it.

## MIR_CEILING
Mirror on the ceiling.
Remark: If you want see lara on ceiling floor you have to use a room very little in height or a fixed camera to look both laras in same time, otherwise lara is not able to rotare her neck to see other lara over her in height position.

## MIR_EAST_WALL
Mirror on east wall

## MIR_FLOOR
Mirror on the floor

## MIR_INVERSE_WEST
Mirror on west wall but every movement and displacement of lara or other moveables will be inverted like in Tomb raider 1 when Lara meets an unskinned alien.

## MIR_NORTH_WALL
Mirror on north wall

## MIR_SOUTH_WALL
Mirror on south wall

## MIR_WEST_WALL
Mirror on west wall of current room. This was the default (and only) setting in old tomb4 egine

## MIST_COL_AZURE
## MIST_COL_BLUE
## MIST_COL_GREEN
## MIST_COL_PURPLE
## MIST_COL_RED
## MIST_COL_WHITE
## MIST_COL_YELLOW
## MPS_DISABLE
used in Plugin= command.
This flag should be used only temporarily as experiment.
When you add MPS_DISABLE flag to MainPluginSettings field, that plugin will be ignored by trng, pratically like its library.dll was not present in trle.
This is a way to verify if some problem or weirdness happens for this plugin, without the need to remove it physically from trle and ng_center folders.

Note: You should understand that the simple removing of Plugin= script command, with a ";" character for instance, could not have the same effect of MPS_DISABLE flag. Because the removing of the plugin= script command from the script, it will not affect the loading of plugin library in tomb4 and its execution.

## NEF_EASY_HEAVY_ENABLING
Used in Enemy command.
This flag gives to current enemy type the ability to enable any heavy triggers immediatly.
By default only some moveable enable heavy triggers and it's necessary place an AI_ object in target sector where there is the heavy trigger to enable.
Differently, with NEF_EASY_HEAVY_ENABLING flag the enemey will enable all heavy triggers where it moves over.

## NEF_EXPLODE
when enemy has no more vitatly it will explode

## NEF_EXPLODE_AFTER
perform animation of death and at end explode it

## NEF_HIT_BLOOD
show blood when it has been hit

## NEF_HIT_DEFAULT
let standard behavior

## NEF_HIT_FRAGMENTS
show fragments when it has been hit

## NEF_HIT_SMOKE
show smoke when it has been hit

## NEF_NONE
## NEF_NON_TARGET
lara will be not able to aim this moveable

## NEF_ONLY_EXPLODE
The enemy will become like skeleton or mummy.
Apparently the common ammos will have no effect but explosive ammos will can destroy it.

## NEF_SAVE_MESH_VISIBILITY
used for enemy command
If you use the action trigger to get invisible some mesh of a moveable, you sould create an Enemy command for the slot of that enemy, adding the NEF_SAVE_MESH_VISIBILITY flag, to force the game engine to save and restore from savegame the visibility status for each mesh of that moveable.

## NEF_SET_AS_BRIDGE_FLAT
Used in Enemy command.
Force the specified animating to work like a BRIDGE_FLAT item.
You can place below it a dummy trigger and lara will be able to walk over it.
Remarks:
It's advisable using an Animating item with this flag.
In particular way some moveable will not work because they have more than a mesh or a pivot for first frame non compatible to work like bridge item.
A good way to avoid these problems is to copy an original BRIDGE_FLAT object in an animating slot with wad merger (Copy button + SHIFT key to reassign the target slot).
Once you copied the original BRIDGE_FLAT in your object then you can modify its layout with metasequoia.

## NEF_SET_AS_BRIDGE_TILT1
Used in Enemy command.
Force the specified animating to work like a BRIDGE_TILT1 item.
The tilt1 is a bit sloped  with a difference by one click between opposite sides.
Remark: see also the description of NEF_SET_AS_BRIDGE_FLAT flag for more infos.

## NEF_SET_AS_BRIDGE_TILT2
Used in Enemy command.
Force the specified animating to work like a BRIDGE_TILT2 item.
The tilt2 is a slope with a difference by two clicks between opposite sides.
Remark: see also the description of NEF_SET_AS_BRIDGE_FLAT flag for more infos.

## NEF_SET_AS_CREATURE
experimental: used to give to a common moveable some features of creatures

## NEF_SET_AS_MORTAL
This flag may be used with slot of semigod to transform then in mortal enemies.
When you set this flag the enemy will be killed also with common ammos

## NEF_SET_AS_SEMIGOD
used for enemy command
Using this flag the enemy will become like a semigod and it will be not possible kill him.

## OBJ_MOTOR_BOAT
Define the slot with motor boat OBJECT

## OBJ_MOTOR_BOAT_ANIM
Define the slot with lara ANIMATIONS for motor boat

## OBJ_RUBBER_BOAT
Define the slot with rubber boat OBJECT

## OBJ_RUBBER_BOAT_ANIM
Define the slot with lara ANIMATIONS for rubber boat

## OTYPE_AI_DATA
Used with Parameters=PARAM_LIGHTNING script command.
You add this value to the OCB value stored in the null mesh object you wish use, to inform that is an AI_DATA item.

## OTYPE_MOVEABLE
Used with Parameters=PARAM_LIGHTNING script command.
You add this value to a moveables index to inform that it is a moveable item.

## OTYPE_STATIC
Used with Parameters=PARAM_LIGHTNING script command.
You add this value to a static index to inform that it is a static item.

## PARAM_ACTOR_SPEECH
Used with Parameters= command
Syntax: Parameters= PARAM_ACTOR_SPEECH, SpeechId, SpeechFlags (SPCF_...), Parameter, FrameRate,  SpeechSlot, HeadSlotMesh, FirstMeshIndex, SpeechMeshAmount, CommandArray

The data of PARAM_ACTOR_SPEECH command will be used to change in sequenze the head of current actor (set via action trigger) to simulate talking.

SpeechId field
--------------
Set here a progressive number to idenfity current PARAM_ACTOR_SPEECH command to choose with action trigger.

SpeechFlags (SPCF_...) field
---------------------------
You can add one or more SPCF_ flags to customize the talking effect
You can type IGNORE to omit flags.
See description of SPCF_ flags in MNEMONIC CONSTANTS section of Reference panel.

Parameter field
---------------
This field could store an optional value working in according with some SPC_ flag.
See description of SPCF_ flags to discover what you can type in Parameter field

FrameRate field
---------------
For frame rate we mean the ration between animation of the head (swapping of heads to simulate animated face) and the number of frames.
Pratically, the default framerate is 5. If you type IGNORE in this field this means you don't wish change frame rate and it will remain the default value: 5 frames for each head type.
If you type 6, each different head will be keeped for 6 frames.
Therefor if you wish slow down the moving of the mouth you have to increase the frame rate, while to increase the speed you have to reduce the frame rate

Note: the framerate value will affect only those speech command where you have not set (or it's not possible) the number of frames to keep that head.
The other speech commands, where you set the number of frame, will be not affected by frame rate field.

SpeechSlot field
----------------
You type here the slot name (or number) from where take the different heads to simulate talking.
All further heads will be taken from following meshes.
You can use different slots, each of them with only an head mesh (old method), or from a single slot with many head meshes (new method).

HeadSlotMesh field
------------------
In this field you type the index of mesh corresponding to the head of actor.
With lara it will be 14, for Jean Yves is 18, for Von Croy is 21 ect.

Note: when you use SPCF_OLD_SPEECH_SLOTS method, this HeadSlotMesh index will be the same of FirstMeshIndex field, while with new method the FirstMeshIndex value could be any you wish, and it will be very often 0, if you place first speech mesh in first mesh of speech slot.

FirstMeshIndex field
--------------------
In this field you have to type the index (the same format you see in Animation Editor of wad merger) of the first mesh for talking in SpeechSlot object.
For instance, if you use old format (SPCF_OLD_SPEECH_SLOTS flag) the mesh index for lara will be 14, because this is the index of head of lara in LARA_SPEECH_HEAD1/2/3/4 slots.
Differently, if you use new method (omitting SPCF_OLD_SPEECH_SLOTS flag), the index should be 0 for lara, while for other actors it will be the index of first talking head for the wished actor.

SpeechMeshAmount field
----------------------
In this field you type the number of different heads used to simulate talking.
Please, remember that this amount is different than number of different heads you can store, furtherly to simulate expressions (happy, sad, angry, surprised ect.) The expression heads will be used with direct command SPC_MESH and they should be not counted in the SpeechMeshAmount value.
For instance, if you use the commons Head_speech_lara1/2/3/4  you'll type 4 in SpeechMeshAmount field.

CommandArray fields
-------------------
From this field you can type a serie of different values divided by commas.
Each value is a command and it will have a SPC_ (SPeech Command) value with (+ sign) some value to set relative mesh number or number of frames, in according with speech command used.
The general formula is:

SPC_ command + Frame*64 + MeshIndex

Where:
- Frame is the number of frames to show current mesh. Range for frame is 1 / 63. If you don't set this value (letting 0), it will be used the framerate value as frame durate.

- MeshIndex is the relative index, beginning from first speech mesh of given slot. Range for meshIndex is 0 / 63

Above formula could have some exception (like it happens with SPC_HEAD/LOOK/PLAY commands.)

See description of different SPC_... commands to know the right syntax for each command.

## PARAM_BIG_NUMBERS
Used in Parameters= command to store big numbers to use in some flipeffects.

Syntax: Paramaters=PARAM_BIG_NUMBERS, Many Numbers separeted by commas

Since for technical reasons it's not possibile type in trigger type window number greather than 255, you can use this script command with PARAM_BIG_NUMBERS value to store big numbers to use in some flipeffects or actions.
The trigger allows to use these big number have a description like this:

Value in 0 index in Paramaters=PARAM_BIG_NUMBERS script command
Value in 1 index in Paramaters=PARAM_BIG_NUMBERS script command
Value in 2 index in Paramaters=PARAM_BIG_NUMBERS script command

Ect.

So if you need to use in this trigger the value 23430 you can type this script command:

Paramaters=PARAM_BIG_NUMBERS, 23430

And in trigger windows you select the index =0 of PARAM_BIG_NUMBERS command.
You can store upto 254 number in same PARAM_BIG_NUMBERS command, then you can choose a number with its index, where the first number has index=0, the second = 1`, the third = 2 ect.

Remark: you cann't type number bigger than 65536 or $FFFF. All numbers will be always used as positive values.

## PARAM_CIRCLE
Used to store the circle data to use with a fragmented triger

Syntax: Parameters=PARAM_CIRCLE, IdParamList, xCenter, yCenter, Radius

IdParamList field
-----------------
You define an univocal id number, different than others Parameters=PARAM_CIRCLE in same [Level] section.

xCenter and yCenter field
-------------------------
In these two fields you type the center of our circle. Remember that the origin of current squared game sector is (0,0) and it is in top-left corner.
The coordinate of the game square go from 0 to 1023.
The X axis is in NGLE view the west-east direction, while the Y axis go from North to South.

Tips & Tricks: you can place the center also outside of current game sector but the effect will work only inside of current game square.
A reason to place the center outside it could be when you wish having only a very light bend. In this case, if you wish, for example, having a light bending in south-side of current sector you could set the center of the circle in the sector at south of current sector, so let say, at (512, 1600). Then, to have some zone in our sector we'll set a big radius like radius = 800

Note: you can use also negative numbers for the center, and it will be necessary when you wish place the center outside and in a sector at west or at north of current game square.

Radius field
------------
The radius use game units, too. So a circle centered in current square sector, to cover all side will have a 512 value as radius.

## PARAM_COLOR_ITEM
Used as first value in Parameters script command. It baptizes a parameters list as data used from trigger to change color of some item.

Syntax: Parameters=PARAM_COLOR_ITEM, IdParamList, ColorType (COLTYPE_...), ItemIndex, Index1ColorRGB, Index2ColorRGB, SpeedChange

IdParamList field
-----------------
This is a progressive number to identify this "Parameters=PARAM_COLOR_ITEM" command script in trigger window of ngle.

ColorType (COLTYPE_...) field
-----------------------------
You can choose a COLTYPE_ constant to set the work mode of this PARAM_COLOR_ITEM.
Read the description of COLTYPE_... constants in Reference list of NG Center.
Remark: you can choose only a single COLTYPE_ value, you cann't add two or more values.

ItemIndex field
---------------
The index of static or moveable for what you want change the color.
You find this value in yellow box in NGLE when you click over some item.

Index1ColorRGB field
--------------------
Type the IdColor of some ColorRGB= with the rgb value used as first color.
The ColorRGB= have to be in same [Level] section where you place the Parameters=PARAM_COLOR_ITEM command.

Index2ColorRGB field
--------------------
If your COLTYPE_ requires two colors, you have to type the IdColor of some ColorRGB= command.

SpeedChange field
-----------------
If you use a COLTYPE_ requiring some dynamic effect (like PULSE or SHADE) you can type in SpeedChange field the number of frame for each cycle.
Rememeber that 30 frame = 1 second. So if you want the pulse, from dark color to light color required two seconds you could type 60.
Remark: the valid range for speed is min: 1 max=255 (about 8 seconds)

## PARAM_INPUT_BOX
Used with Parameters= commmand

Syntax: Parameters= PARAM_INPUT_BOX, InputBoxId, BckImageId, WFontId, MaxChars, SfxSound, Flags (RIB_  values), ExtraParam

The data of PARAM_INPUT_BOX paramaters will be used to show (whereby a flipeffect) a background image where the player will be able to type characters (letters and/or digits) on screen. At end of this operation some trng variables ("Last Input Number" and "Last Input Text") will host the typed text and, when it is only digit text, the final number typed by user.
When you use Input Box in "only digits" mode, it will work like a 2D graphic Keypad, pratically, but now you have also the chance to get common words in "password" style, accepting letters and digits.

InputBoxId field
-----------------
Set here a progressive number to idenfity current PARAM_INPUT_BOX command from others, to choose it with a flipeffect trigger.
You can have a max of 100 PARAM_INPUT_BOX  for level with ID values upto 999

BckImageId field
-----------------
You have to supply a background image to host the Input Box feature.
In this field you'll type the Id of an Image= script command with data about image to use as background for input box.

Note: since Input Box feature is a bit different matter than a common image, the settings, in the Image= script command, should follow some rules:

1) The Image= command has to be with IF_FULL_SCREEN + IF_QUIT_ESCAPE flags

2) The IF_POP_IMAGE flag should be avoided, since only "overlapped" image work fine with Input Box feature, where the program waits for user input

3) In spite the image will be NOT a pop-up image, you have to type in XPosition, YPosition, SizeX and SizeX fields, valid values.
These microunit values will be used to set the position of the text typed by user (and not, as it happens with common Image, t set the position of the image)

WFontId field
-------------
This is the ID of a WindowsFont= script command  to set type a font and color, used to write on screen what the player types with the keyboard.
Note: charsets different by western sets are not supported. Indeed, the low-level keyboard reading of Input Box process, reads a standard US/UK qwerty keyboard and, for this reason, further non-western characters will be neither "seen" by the Input Box procedure.

MaxChars field
--------------
The max number of characters that user will be able to type on screen.
This max length value is important to avoid that, a drunk player, typed plenty of letters, upto ruining the layout of graphic on screen, with text moving outside screen borders.
You should perform a test to verify how many characters will be visible on the screen, in according with the font size you chose, and the start position of text rect. Then you'll type a value (as  MaxChars) a bit less than that max visible number of characters.

SfxSound field
--------------
Index of sfx effect sound to play everytime the user hits a key of keyboard.
Please note that, if you set IGNORE in this field, this doesn't mean that there will be no sound (silent mode) but only that you wish using a preset sound for hitting key: the sfx sound 109 (MENU_SELECT).
The switching on/off of sounds, it will be affected only by usage (or less) of RIB_SOUND_ON_KEY flag.

Flags (RIB_  values) field
--------------------------
In this field you can insert one or more RIB_ (Read Input Box) flags linked with "+" operator.
In the case you typed IGNORE in this field, it will be used default settings, corresponding to following flags:
RIB_BLINK_CARET+RIB_SOUND_ON_KEY+RIB_ONLY_CAPS

ExtraParam field
----------------
This field could keep some parameter in accordingwith some RIB_ flag.

Read the descriptions of RIB_ constants in MNEMONIC CONSTANTS section of NG_Center's Reference panel, for more infos.

## PARAM_LIGHTNING
Used to store data to shot a lightning

Syntax: Parameters=PARAM_LIGHTNING, IdParamList, Lightning flags (LGTN_...), SourcePosItem, TargetPosItem, IdColorRGB, Intensity, SoundEffect, Size, ParticleDurate, IntervalTime, Alfa, Beta

IdParamList field
-----------------
You define an univocal id number, different than others Parameters=PARAM_LIGHTNING in same [Level] section.
Then you'll use this id to pass to the trigger to set current parameters.

Lightning flags (LGTN_...) field
---------------------------------
You can type one or more LGTN_ flags to customize the lightning effect.
If you wish you can type IGNORE in this field to se no flag.
See the meaning of LGTN_ flags in the Reference panel of NG_Center program, in the MNEMONIC CONSTANTS section.

SourcePosItem field
-------------------
To set a position you have to supply an item and trng will use the position of that item for the source position.
In this field you can also type IGNORE and in this case trng will use a random position in the sky (above the target position) as source position.

If you wish set a precise source position, you can type in this field:

A moveable index  + the OTYPE_MOVEABLE flag (note: in the reality you can omit it, because missing other OTYPE_ flags, the moveable type will be used)

A null mesh item OCB value (the null mesh items are like the AI_ objects or the LARA_START_POS item) + the OTYPE_AI_DATA flag
REMARK: Read carefully above row: if you wish use a LARA_START_POS, you have to type its OCB value and NOT the index.
This means you should set in that null mesh object an OCB value different than any other in your level.

A static object index + the OTYPE_STATIC flag

TargetPosItem field
-------------------
To set a position you have to supply an item and trng will use that position of that item as the target position.

You can type in this field:

A moveable index  + the OTYPE_MOVEABLE flag (note: in the reality you can omit the flag, because missing other OTYPE_ flags, the moveable type will be used as default)

A null mesh item OCB value (the null mesh items are like the AI_ objects or the LARA_START_POS item) + the OTYPE_AI_DATA flag

REMARK: Read carefully above row: if you wish use a LARA_START_POS, you have to type its OCB value and NOT the index.
This means you should set in that null mesh object an OCB value different than any other in your level.
A trick, to keep different ocb values for whole level, is to compute the ocb value, multiplying the room nummber by 10 and then use this as first ocb value for items in that room.

A static object index + the OTYPE_STATIC flag

You can type also IGNORE in this field and in this case you'll have a random (far) lightning.
You should use IGNORE in TargetPosItem field, when lara is in a wide outside landscape.
With IGNORE as target, in fact, the trng engine will choose a random target placed very far from Lara from 8 to 20 sectors, choosing the side where lara is able to see it.

IdColorRGB field
----------------
You type here the ID of an (previously typed) ColorRBG= command where you stored the color you wish use for the lightning.
If you type IGNORE in this field it will be used the white color.

Intensity field
---------------
You can type values that werea (about) in the range from 30 to 100
The intensity increase the light of the lightning and its durate.
You can type IGNORE in this field and trng will use the most common value for this setting

SoundEffect field
-----------------
Here you type the number of sound effect you wish was played when the lightning it will be performed.
Note: if you wish supply a sound effect you have to add to the Lightning field the LGTN_PLAY_SOUND flag.

Size field
----------
This should be the diameter size of the lightning but it has not a precise reference to the effective size.
I presume it was a magnifying factor about the "lighting" sprite used to create this effect.
Reasonable values for this field will be in the range from 20 to 40
If you type IGNORE in this field, trng will use the most common value for this setting

ParticleDurate field
--------------------
This value is the persistence of particles used to create the lightning.
Reasonable values are in the range from 20 to 40
If you type IGNORE in this field,  trng will use the most common value for this setting

IntervalTime field
------------------
This field is a bit complicated to explain but it's very important when you wish simulate the lightning conductor.
Differently, you should type 0 (or IGNORE) in this field when you wish simulate the sky lightning.
The value in this field will be used to compute a pause between a shooting of a lightning and the next, when you set (in the trigger) a long time.
For long time I mean also only 30 tick frames (one second) or more.
The problem is that a lightning uses a lot of particles and when you set to show a lightning for some time, in the reality it will be shout a big number of lightnings, one for each frame.
If we don't set a pause between a lightning and following, it happens something that I'm not able to describe, but it's something of evil, that you don't want see in your level.
To avoid this collapse of the particle system and lightning skill, it's necessary set a pause that it should be about the time used from the particles used for the previous lightning to let them to vanish in normal way.
The value you type in this field will work in inverse mode with the ParticleDurate field.
In this field you have to type the denominator of a fration where we have "1" as numerator.
This will be the times that the lightning will be shot, while the other times there will be a pause.
For example  1/4  means, it will be shot once every four times.
As further complication you can choose for this field only powers by 2, like 2,4 ,8, 16, 32 ect.

A reasonable value for this field is 4 or 8, but it depends by the value you typed in Intensity field and in ParticleDurate field.
Bigger values you chose for those fields and bigger should be IntervalTime, too.

Note: if you type huge value (anyway the max value is 128), you solve the collapsing problem and you get a long pause between a lightning and the next.
To compute the pause time you can perform the computation: IntervalTime / 30, to get the seconds of pause between a shot and another.
For example typing 32, you get about a lightning every second, in spite that a lightning goes on for about half second so the time with no lightning effect on the screen will be shorter

Alfa field
----------
The generic name "alfa" is because I've not yet discovered the menaning of this argument.
It has always very low values (0,1,3 are the most used). Pheraps they are flags rather than a quantity.
You can do experiments to discover what changes, modifying this argument, or you can type IGNORE in this field and it will be used the most common value

Beta field
----------
Same speech as for the above Alfa field.
Anyway in this case it appears like it was the intensity of some sharping, blurrying effect of the lightning.
Increasing this value, it becomes like smoke, and increasing more it becomes transparent until to disappear by all.
This argument has usually 3 or 5 like values.

You can type IGNORE in this field and it will be used the most common value

## PARAM_MOVE_ITEM
Used as first value in Parameters script command. You can set the values used to move a moveable or a static using the specific "Move."  flipeffect.

Syntax: Paramaters=PARAM_MOVE_ITEM, IdParamList, Flags (FMOV_...), IndexItem, Direction (DIR_...), Distance, Speed, MovingSound, FinalSound, Extra

Note: from 1.2.2.7 version it has been added a new set of DIR_ values and FMOV_ flags. The main target of these improvement was to add non constant speed and gravity simulation to movements.
The old DIR_ values and FMOV_ flags should work as in the past, anyway the FMOV_INFINITE_LOOP flag has different operative modes in accoding with DIR_ values, now. So read the new description of FMOV_INFINITE_LOOP flag for more infos.
With 1.2.2.7 version has been added a new field named "Extra" at end of other old fields.

Field descriptions:

IdParamList field
-----------------
This is a progressive number to identify this "Parameters=PARAM_MOVE_ITEM" command script in trigger window of ngle.
You'll type 1, for your first PARAM_MOVE_ITEM command, 2 for second ect.

Flags (FMOV_...) field
----------------------
You can set one or more FMOV_ values linked with + (plus) sign
See description in Reference panel to get description of different FMOV_ flags.
Remark: you can type IGNORE in this field if you don't wish use any flag.

IndexItem field
---------------
This is the index you can read in yellow frame when you click over some item in NGLE program.
Remark: you can use static or moveable anyway you have to use the correct flipeffect in according with static or moveable nature of item.
About static remember the static are owned from its room and for this reason you should avoid to move a static other to bounds of its room.

Direction (DIR_...) field
-------------------------
You have to choose one DIR_ constant to set the direction of moving.
See the DIR_ list in reference panel of NGCenter program, you find it in _MNEMONIC CONSTANTS section.

Note: from 1.2.2.7 version it has been added some FMOV_ constants to simulate different kinds of gravity. It's not suggested mixing the DIR_UP or DIR_DOWN direction with a FMOV_ flag for gravity because the result will be the overlapping of two different computation on same Y coordinate. If you are non satisfacted by this mixing and you wish only have a DIR_DOWN for the falling of an item, there are two possible solutions:

- Use the DIR_DOWN direction but setting 0 as Speed field. In this way it will be only the gravity you choose to affect the movement of the item.
- Use some horizontal direction (east, west, north, south or forward) but with a very short Distance field. In this way the item, ohter to fall down for the gravity you chose, it will move a bit in the direction you chose. In this case remember to set also the FMOV_WAIT_STAND_ON_FLOOR flag to force the movement will be completed only when the item reached the floor.

Distance field
--------------
In this field you set the distance of moving. The used units have as reference 1 sector = 1024, hence, 512 is half-sector, 256 is a click ect.
I suggest to use always multiple of 256 (one click) to avoid troubles.
The max value you can type for distance is 64512, corresponding to 63 sectors.

Speed field
-----------
The speed value is the number of units that will be added to current position to move the item.
The used units are the same of Distance field: 1 click = 256 units.
Remember that this speed will be added 30 times for second, so it's better don't exagerate to set big values as speed. A reasonable speed is enclosed in the range  from 8  to 64.
Remark: It's advisable set as speed a value that is a perfect multiple of distance, otherwise there will be a bit error in compute of final distance.

Note: From 1.2.2.7 version in the higher byte of Speed field you can set an extra value that to change the preset values for gravity when you added a FMOV_ flag for gravity.
The compute to type two values in same field is given by following formula:

ChangeVSpeed * 256 + Speed

The changeVSpeed is percentage value to increase or decrease the value of gravity. If you use 100 the gravity will be the same of preset values for that kind of gravity. If you set 50 the speed will be the half, while if you type 200 the gravity will be doubled.
In the case you do not set any value as ChangeVSpeed (i.e 0*256 +Speed), trng will see the zero value as IGNORE and therefor like it was 100 (no changes about gravity)

Moving sound field
------------------
Optional. If you wish it went played a sound effect in looped mode while the item is moving type here a number of sound effect.
You find the list of sound effect in reference panel of NG Center, in the section named "SOUND SFX indices list"
Remark: if you don't wish any sound type IGNORE in this field.

Final sound field
-----------------
Optional. If you wish it went played a sound effect when the item reaches the final position, type here a number of sound effect.
You find the list of sound effect in reference panel of NG Center, in the section named "SOUND SFX indices list"
Remark: if you don't wish any sound type IGNORE in this field.

Extra field
-----------
This field has been added from 1.2.2.7 version and it is a general purpose parameter.
See description of FMOV_ flags and DIR_ values to discover how to use the Extra field.

## PARAM_PRINT_TEXT
Used to store all informations used to print text.

Syntax: Parameters=PARAM_PRINT_TEXT, IdPrintText, Color (CL_...), FontType (FT_...), BlinkTime, DurateTime, X_Position, Y_Position

This parameter list will be required by new flipeffect trigger "Text. Print formatted ... string"

IdPrintText field
-----------------
This is the number you have to choose in Set Trigger Window with "print formatted text" trigger.

Color (CL_...) field
--------------------
The color of this text.
See the values CL_... in Reference panel.

FontType (FT_...) field
-----------------------
In this field you can set two or more FT_... constants to describe the size or the preassigned position for text.
Remark: if you wish you can omit the FT_ value to set position and instead you can set the position in pixel coordinates typing two valid values in fields: X_Position and Y_Position. See description of these fields.

BlinkTime field
---------------
Numeric value to signal interval for blinking when you set also flag FT_BLINK_CHARS in FontType field
Note: for blink time you can use only power by 2 values, like: 1, 2, 4, 8, 16, 32, 64, 128

DurateTime field
----------------
Number of seconds to show the text on screen.
If you want set as time "forever" type -1 or IGNORE in this field.
Remarks:
(1) If you set forever (IGNORE) for durate, then you can remove the string from screen using flipeffect "Text. Print. Remove (&)Extra NG String from screen"
(2) If you use this parameter data for vertical or horizontal scrolling text, the value you type in DurateTime field will be interpreted by trng as the speed value used for scrolling text.
The values for scrolling speed are the following:

0: Abs. Normal Speed  (30 fps)
1: Abs. Slow Speed (15 fps)
2: Abs. Very Slow Speed (10 fps)
3: Abs. Fast Speed (60 fps)
4: Abs. Very Fast Speed (90 fps)
5: Prop. Normal Speed  (30 fps)
6: Prop. Slow Speed (15 fps)
7: Prop. Very Slow Speed (10 fps)
8: Prop. Fast Speed (60 fps)
9: Prop. Very Fast Speed (90 fps)

If you set IGNORE in this field (used by scrolling text) the default value used will be 0 i.e.
"Abs. Normal Speed  (30 fps)"

X_Position and Y_Position fields
--------------------------------
If you set two valid values in these two fields the further FT_ value to set the position will be ignored.
Differently, if you want use some FT_ prefixed position you should type in X_Position and Y_Position fields the values IGNORE for both.
The values you type in these fields are in pixels, anyway, since you cann't to known in advance what it will be the resolution in game (each player could set a different resolution) you should set your coordinates like if screen was always in 1024x768 pixels resolution, then, if the game will have a different resolution the TRNG engine will change the position proportionally in according with the real resolution.
For example if you set Y_Position the value 384 (i.e. 768 / 2) the text will be showd in vertical half position of screen in each screen resolution thanks to trng adapting.
Remark: The Y_Position is not the top pixel of character but the baseline character. This means that if for example you use as Y_Position the value 0 the chracter will be not visible except a little bottom side.

## PARAM_QUADRILATERAL
Used to store the four vertices to define a quadrilateral to use as fragmented trigger.

Syntax: Parameters=PARAM_QUADRILATERAL, IdParamList, Xa, Ya, Xb, Yb, Xc, Yc, Xd, Yd

A quadrilateral could be a rectangle, a square but also a trapezium or a rhombus.
If it has four sides it is a quadrilateral.

It's not important what is the first point you use as A(x,y) vertex, but it's necessary you type the vertices following the perimeter in clockwise order.

You define a quadrilateral with four 2D vertices.
Each vertex is a point of a game square of 1024x1024 units.
The origin is the top-left corner (north-west corner in planar view in NGLE program)
The X axis moves from 0 (West side of the square) to 1023 (East side of the square)
The Y axis moves from 0 (North side of the square) to 1023 (South side of the square)

IdParamList field
-----------------
You define an univocal id number, different than others Parameters=PARAM_QUADRILATERAL in same [Level] section.
You'll use this number to use these data with the fragmented trigger for quadrilateral

Xa, Ya fields
-------------
Define the A(x,y) vertex

Xb, Yb fields
-------------
Define the B(x,y) vertex

Xc, Yc fields
-------------
Define the C(x,y) vertex

Xd, Yd fields
-------------
Define the D(x,y) vertex

## PARAM_RECT
Used with parameters= script command
Syntax: Parameters=PARAM_RECT, RectId, XOrigin, YOrigin, Width, Height, ForeColor, BackColor

This parameter could be used to store infos about screen rectangle.

RectId field
-------------
Id to identify this PARAM_RECT parameters. You'll use this value to reference your rectangle in the script commands or triggers that requires it.

XOrigin field
-------------
This is the X origin of the rectangle, it is the left side of rectangle.

YOrigin field
-------------
This is the Y origin of the rectangle, it is the top side of the rectangle

Width field
-----------
This is the x size, the width, of the rectangle

Height field
------------
This is the y size, the height of the rectangle

ForeColor field
----------------
Optonal field. Where it is required you can type here the ID of a ColorRGB command to set the foreground color of the rectangle
Note: usually the foreground color is the frame that bounds the rectangle
If it is not foreseen to supply a foreground color you can type IGNORE in this field

BackColor field
---------------
Optonal field. Where it is required you can type here the ID of a ColorRGB command to set the background color of the rectangle
Note: usually the background color is the inside (and wider) zone of the rectangle
If it is not foreseen to supply a background color you can type IGNORE in this field

## PARAM_ROTATE_ITEM
Used as first value in Parameters script command. It baptizes a parameters list as data for rotating of items, moveables or statics.

Syntax: Parameters=PARAM_ROTATE_ITEM, IdParamList, FlagsRotation (FROT_), ItemIndex, DirHRotation (ROTH_...), HRotationAngle, SpeedHRotation, DirVRotation (ROTV_..), VRotationAngle, SpeedVRotation, MovingSound, FinalSound

Description of fields:
-----------------------

IdParamList field
-----------------
This is a progressive number to identify this "Parameters=PARAM_ROTATE_ITEM" command script in trigger window of ngle.
You'll type 1, for your first PARAM_ROTATE_ITEM command, 2 for second ect
You have number between 1 and 99.

FlagsRotation (FROT_) field
---------------------------
You can type one or more FROT_ constants linked with + (plus) sign.
See description of FROT_ constants in Reference panel of NG Center.

ItemIndex field
---------------
This is the index of item to rotate, you can read it in yellow frame that appears when you click over some item in NGLE program.
You can choose moveable or static but remember to perform the correct flipeffect in according with nature (static or moveable) of item.

DirHRotation (ROTH_...) field
------------------------------
If you wish have an horizontal rotation you'll type in this field a ROTH_ value to set the direction of rotation (clockwise or opposite)

HRotationAngle field
--------------------
In this field you set the wished horizontal angle rotation.
The unit of measurement for this field is a bit weird but it that used in tomb4 engine.
These are some references:
$2000  = 45 degrees
$4000  = 90 degrees
$8000 = 180 degrees

For example if you want that some object rotates by 90 degrees you'll have to type in this field the value $4000 (or in decimal: 16384)

Remarks: If you set the FROT_ flag for endless rotation the VRorationAngle field will be ignored, since the item will round continuosly.

SpeedHRotation field
--------------------
The speed is a value that will be added to current orientation factor.
The unit of measurement is the same of horizontal or vertical RotationAngle, but in this field you'll have to type value very littler than final angle to reach, of course.

Remember that the value you type like speed will be added to current orienting 30 times for second.

Remark: It's advisable to use as speed values multiple of power by 2. Typying in hexadecimal is easy to remember the good speeds:
$80 , $100 , $180, $200, $280, $300  ect
If you don't use these values there is the risk that the speed value was NOT a multiple of final Rotation Angle and hence the object could be not exaclty positioned in final position.
To discover if value for RotationAngle and Speed are good, just dividing Rotation by Speed, if you get a result with decimal point the couple of values is wrong.

DirVRotation (ROTV_..) field
----------------------------
If you wish have a vertical rotation you'll type in this field a ROTV_ value to set the direction of rotation (forward or backward)
Remark: unfortunately for statics it's not foreseen a vertical rotation, so you can apply a vertical rotation only to moveable items.

VRotationAngle field
--------------------
In this field you set the wished vertical angle rotation.
The unit of measurement for this field is a bit weird but it that used in tomb4 engine.
These are some references:
$2000  = 45 degrees
$4000  = 90 degrees
$8000 = 180 degrees

For example if you want that some object rotates by 90 degrees you'll have to type in this field the value $4000 (or in decimal: 16384)

Remarks: If you set the FROT_ flag for endless rotation the VRorationAngle field will be ignored, since the item will round continuosly.

SpeedVRotation field
--------------------
This field hosts the speed of vertical rotation.
About the values to set see the description of SpeedHRotation field. The speech is the same, but in this case the rotation is vertical.

MovingSound field
-----------------
Optional field. If you wish assign a sound to item while it is rotating, type a number of sound sfx.
Set IGNORE if you don't wish any sound for moving.

FinalSound field
----------------
Optional field. If you wish perform a sound when the rotation is complete you can type a sound sfx sound in this field
Set IGNORE if you don't wish any sound for final position.

## PARAM_SCALE_ITEM
Used in Parameters= command to store data for scaling effects

Syntax: Parameters=PARAM_SCALE_ITEM, IdScaling, ItemIndex, Flags Scaling (FSCA_...), BeginSizePercentage, FinalSizePercentage, PercentageSpeed

IdScaling field
---------------
Used to locate this specific command in the script.
When you'll use this data you'll have to choose this number in Set Trigger Type window to choose the Parameters with scaling data to use in your trigger.

ItemIndex field
---------------
In this field you type the index of item to resize. You get this univocal index within NGLE program, performing a right mouse click on the item placed in the map. You read the index in the yellow frame in the rounded parenthesis.

Flags Scaling (FSCA_...) field
-------------------------------
Flag to customize the scaling effect.
You can type IGNORE if you don't use any flag.
Read the FSCFA_... values in MNEMONIC CONSTANTS section of Reference Panel of NG_Center program to know the different flags you could use.

BeginSizePercentage and FinalSizePercentage fields
--------------------------------------------------
The dynamic scaling effect works changing dynamically the size of an item  from a size to another.
The BeginSizePercentage is the beginning size of the item in the game, while the FinalSizePercentage will be the ending size of the item to complete the effect.
The values are in percentage, where 100 = the original (in the wad file) size of the item.
For example if you wish increase the size of this item from original size to the double, you'll use:
BeginSizePercentage=100
and
FinalSizePercentage=200

Remark: You can type IGNORE in the BeginSizePercentage field only when you use this script command to resize immediatly an item to the wished new size (of FinalSizePercentage field) with no dynamic effect.

PercentageSpeed field
---------------------
In this field you the value  to add (or subtract) at each frame to BeginSizePercentage to reach the FinalSizePercentage value.
Bigger it will be this value and most fast will be the dynamic resize effect.

Important: the value is not given in 100th but in 1000th, i.e. one unit = 1/1000. The reason is that in this way you can set also a very slow speed, while, differently, using effective percentage, you could have only like min speed "1/100" and in this way the min speed of the effect could be too fast.

## PARAM_SET_CAMERA
Use to store information in Parameters command to use with FlipEffect to change temporary the camera mode.

Syntax: Paramaters=PARAM_SET_CAMERA, IdSetCamera, Flags (FSCAM_... ), DistanceCam, VOrientCam, HOrientCam, SpeedCamera

IdSetCamera field
------------------
This is an univocal identifier to locate these parameters in your [Level] section using this value in flipeffect trigger to set temporary a new camera mode.

Flags (FSCAM_... ) field
------------------------
In this field you can type one or more FSCAM_.. flags to affect the behavior of this command.

You can type IGNORE if you don't wish any flag.

DistanceCam field
-----------------
This is the distance of camera from lara.
A reasonable value could be +1600

For more infos about the concept of "cam distance" see the description of CUST_CAMERA

If you don't want change this value you can type ignore, and it will be used the standard distance value set for "chase" (follow-me) camera.

VOrientCam field
----------------
Degrees difference of camera computed on horizonal line (parallel to floor) between camera and Lara.
See the CUST_CAMERA description to read more infos about Vertical Orientation field.

If you wish you can type IGNORE in this field and the engine will use the standard VOrientCam value set for "chase" camera.

HOrientCam field
----------------
Set the horizontal oriantation (facing) of camera respect to lara.

See the CUST_CAMERA description to read more infos about Horizontal Orientation field.

If you type IGNORE in this field the engine will use the HOrient value set for "chase" camera.

SpeedCamera field
-----------------
This value set the speed of camera to pass from previous camera mode (and position) to current new postion.
The speed works in opposite way: big values = slow movements, little values = fast movementes.

If you type little values the camera will reach fastly the new position but the movement could be move jerkily and it's not good to see.
If you type a big value the camera will have a more soft movement but to reach the wished position will require more time.

A reasonable value for fast moving is 1, while for slow movement is 10.

If you type IGNORE in this field the engine will use the default speed value (10).

## PARAM_SHOW_SPRITE
Used with Parameters command to store data to show sprite on screen with flipeffect show sprite.

Syntax: Parameters=PARAM_SHOW_SPRITE, IdParamShowSprite, Flags Show Sprites (FSS_...), OriginX, OriginY, Width, Height, SlotSprite, SpriteIndex, IdColorRGB, GridX, GridY, Extra Value

IdParamShowSprite field
-----------------------
Here type a progressive number to distinguish this "Parameters=PARAM_SHOW_SPRITE" command from others in same level section.
You'll use the number in this field to signal to flipeffect trigger where looking for sprite show data.

Flags Show Sprites (FSS_...) field
----------------------------------
You can type in this field FSS_.. constant values to customize the feature of your showing sprite operation.
Read the description of FSS_ constants in Reference panel of NG_Center program for more infos.

OriginX and OriginY fields
--------------------------
In these two field you should type the origin of (top-left corner) the sprite.
Both values are in micro units, so you can use the [Get Screen Frames] tool you find in [Tools] panel of NG_Center program.

Width and Height fields
-----------------------
You have to set the size that the sprite will have on the game screen.
Like for the OriginX, OriginY fields, you have to type these values in microunits, hence you should use the [Get Screen Frames] tool to choose a rectangle on the screen and then copy both four values in microunits in the OriginX, OriginY, Width and Height fields.

Remark: if you wish use a sprite grid (with the FSS_SHOW_SPRITE_GRID flag) please, don't confuse, you have to choose like witdh and height the size of a single sprite and not the final size of the sprite grid.

SlotSprite field
----------------
You should type the sprite slot with the sprite you wish use.
For example you could type DEFAULT_SPRITES, MISC_SPRITES or other slot contain sprites.

SpriteIndex field
-----------------
Here you type the index of the sprite to use within the SlotSprite given.

Remark: the first sprite has an index = 0

IdColorRGB field
----------------
You can type in this field the Id number of a previous ColorRbg= script command to add color to the sprite.

Remark: the add color feature works only when you enable the transparent feature for the sprite adding the FSS_TRANSPARENT flag. while with the full opaque sprites the IdColorRbg will be ignored.

GridX and GridY fields
----------------------
These fields are used only with FSS_SHOW_SPRITE_GRID flag.
For example if you wish show an image created with 2x1 sprites, you should type 2 in Gridx and 1 in GridY
See description of FSS_SHOW_SPRITE_GRID flag for more infos.
If you don't use FSS_SHOW_SPRITE_GRID flag the gridx, gridy flags will be ignored.

Extra Value field
------------------
This field could be used in according with some FSS_ flag.
See the descriptions of the FSS flags to discover when this field is really used.

## PARAM_SWAP_ANIMATIONS
used with Paramaters= script command
Syntax: Paramaters=PARAM_SWAP_ANIMATIONS, ASwapId, SourceFirstAnim, TargetFirstAnim, NumberOfAnimations

This paramater host data for flipeffect 393.
The F393 trigger swap animations from SourceFirstAnim to TargetFirstAnim. The animation slots swapped will be NumberOfAnimations.

Usually, you'll perform another swap of same set of animations to restore previous situation.

ASwapId field
-------------
A progressive id to identify these paramter data. You'll chose this id in F393 trigger to link it with swapping data of this specific parameters=PARAM_SWAP_ANIMATIONS command.

SourceFirstAnim field
---------------------
You type here the animation number of first animation of the group of animation slot to swap.
Remember that it's necessary that both group of animations to swap was in the same slot, i.e. store for the same object type.
Remember that the indices are zero based. This means that if you wish replace all first 8 animations (for instance) you'll type "0" to choose first animation slot.

TargetFirstAnim field
---------------------
In this field you type the first benginning animation of the second group whom swap the animations.
Please note that nothing changes if you invert the values in the TargetFirstAnim field with that of SourceFirstAnim, since this is a swapping and at end both group animation will be yet present in the slot, but in different (inverted) position.

NumberOfAnimations field
------------------------
In this field you type the number of animations to swap, i.e. the number of animations for each group to swap.
You can swap a single animation, typing 1, or a bigger number, anyway take care to do not overlap the two, target and soruce, groups.
For instance this parameter is surely wrong:

Parameteres=PARAM_SWAP_ANIMATIONS, 0,  4, 6

Because the number of animation to swap (6) is bigger than the distance between first source anim (0) and first target anim (4), in this way you'll mess the animations of that slot.

## PARAM_TRIANGLE
Used to store the three vertices to define a triangle to use as fragmented trigger.

Syntax: Parameters=PARAM_TRIANGLE, IdParamList, Xa, Ya, Xb, Yb, Xc, Yc

You define a triangle with three 2D vertices.
Each vertex is a point of a game square of 1024x1024 units.
The origin is the top-left corner (north-west corner in planar view in NGLE program)
The X axis moves from 0 (West side of the square) to 1023 (East side of the square)
The Y axis moves from 0 (North side of the square) to 1023 (South side of the square)

IdParamList field
-----------------
You define an univocal id number, different than others Parameters=PARAM_TRIANGLE in same [Level] section.
You'll use this number to use these data with the fragmented trigger for custom triangles

Xa, Ya fields
-------------
Define the A(x,y) vertex

Xb, Yb fields
-------------
Define the B(x,y) vertex

Xc, Yc fields
-------------
Define the C(x,y) vertex

Example:
to define a triangle corresponding to a north-east corner triangle with 512 of size, you should use following command

Parameters=PARAM_TRIANGLE, IdParamList, 512, 0, 1023, 0, 1023, 512

Remarks:
Using the custom triangles, and exporting the condition triggers that use them, in a conditional trigger group, you can cover pratically any shape for your trigger.
If you wish create a detailed shape, you can add different triangles, just you use the TGROUP_OR flag in the TriggerGroup command, to link the different triangle triggers.
If you wish you can mix triangles with other shapes, like circle or grid triggers, linked with the (default) TGROUP_AND, to have like triggerable zone only those points where all fragmented triggers are true in same moment.

## PARAM_WTEXT
Used to store parameters to print string with font of Windows

Syntax: Parameters=PARAM_WTEXT, IdParameter, Flags (WTF_...), WindowsFontId, TimeDurate, Left, Top, Right, Bottom

IdParameter field
-----------------
You type an id to identify this parameters command when you'll link it with some trigger to print windows texts.

Flag (WTF_... field
--------------------
You can type one or more WTF_ (Windows Text Flags) flag.
You can type IGNORE in this field if you wish omit flags.

WindowsFontId field
-------------------
This is the ID of some WindowsFont command where you set font and some settings to use in this printing.
Note: it's better that the WindowsFont command was placed first (above) of current Parameters= command.

TimeDurate field
----------------
Here you type the number of tick frames (one tick frame = 1/30th of second) that the text will be showed on the screen.
If you type IGNORE (or -1, that is the same), the text will be showed forever. In this case you'll have to call another trigger (F364) to remove the text when you wish.

Left, Top, Right, Bottom fields
-------------------------------
When you print using windows text you have to identify a bounding box in the screen where the text will be aligned in according with the alignment flags you set in the linked WindowsFont command.
The values are in microunits, where 1000 is the max width or height of screen.
For example the box:

0 , 0, 1000, 1000

Bound all screen, and this setting is good for a long, multiline central alignment.
You can use the [Get Screen Frames] in the [Tool] panel of NG_Center to compute the zone where show/align your text.
This work is very alike than that you used in Diary, with a page to text showed in an ideal box of the screen.

## PB_DOUBLE_FACE
Used in Customize=CUST_PARALLEL_BARS.
By default, the Chronicles parallel bar has a weird property: it works with long jumps only from a side, while from opposite side the jump is like disabled, lara simply performs a single pirouette and remains in the closed sector.
Probably this behavior has been studied to avoid to use parallel bar to go back in the level in some situations, anyway if you want to do work in same way the bar, indifferently by current facing, you can customize the bat with the PB_DOUBLE_FACE flag.

## PB_LARA_CAN_SLIDE
Used in Customize=CUST_PARALLEL_BARS.
By default lara is not able to move herself when she hanged to bar. She can only to turn.
Differently, adding the PB_LARA_CAN_SLIDE value in CUST_PARALLEL_BARS, lara will be able to slide to left or to right.

Remarks:

* You can place two or more closed PARALLEL_BARS items in NGLE to create a very long bar and Lara will be able to move long all this path.

* The power of jump will be affected by last parallel bar item touched by lara before letting the bar to perform the jump. This speech is useful about the ocb value set in parallel bar item. If you create a very long bar, using different parallel_bar, you could set different OCB values in these items, and lara will jump with different powerness in according with the ocb value stored in single parallel bar used for last.

## PB_MULTIPLE_ENDINGS
Used in Customize=CUST_PARALLEL_BARS.
By default Lara will perform always a correct jump, indifferently by the moment when she lets go the bar.
In this way it's not possible to wrong, because the jump is always the same.
If you use the PB_MULTIPLE_ENDINGS flag the situation changes: now lara could wrong the exiting from turning: falling vertically, handlong or in stand-up position.

Remarks:

* This option is not perfect because the state id change should require a correct, specific, next animation to continue the source animation of turning in precise way, frame for frame, while I tried to link source animation with some default animations like that of headlong diving and fast falling (with lara screaming).

* Theorically you could add your new custom animation and link them to animation 462 state id = 128, whereby the change state id editor of wad merger program.
If you create a new animation where lara loses the hanging in a different point of the turn, just you create a change state-id where you set the range of frames for 462 animation where you want that lara can let go the bar. Then you set as state id = 129, and the number of your new animation.
If you perform this attempt it's better don't use the PB_MULTIPLE_ENDINGS value because this flag perform a change of state id in hardcoded mode and this could interfere with your changes of state id.
Technically the code performs this computes:
- When detect that lara lets go the Action key: set as next state id = 129
- This change doesn't happen immediatly but only when the infinite animation 462 find a frame range in state id change correct.
- By default this happens only between frame 8 and 9 of anim 462, and the next animation will be the animation 463
You can add new animation starting from some of intermediate position of rotation, and then apply to specific frame range where lara has that starting position.

## PB_PROGRESSIVE_CHARGE
Used in Customize=CUST_PARALLEL_BARS.
By default the power of jump is given by OCB value set in parallel bar object in NGLE program.
If you add the PB_PROGRESSIVE_CHARGE flag the power of jump will depend by number of full turns around the bar. For each turn the power of jump will be increased by number in OCB value.
For example, if you type 50 in OCB field, and lara perform 3 full turn before let the Action key, the power will be 50 * 3 = 150
The max power is 10 turns, other this limit the power will be not increased furtherly.
Remark: if you use also the flag PB_LARA_CAN_SLIDE, to permit to lara to move left/right hanged at bar, everytime lara moves left/right the counting of jump power will be cleared , restarting from OCB Value.
This is useful when the player need to perform a correct number of turns but he has also to move lara in a different position of bar. In this way it's always possibile set the correct number of turns: when the player passed over the correct number of turn, just he moves left/right lara to reset and restart the counting.

## PB_SHOW_CHARGE_BAR
Used in Customize=CUST_PARALLEL_BARS.
This flag works only when you set also the flag PB_PROGRESSIVE_CHARGE.
Since when the PB_PROGRESSIVE_CHARGE flag is enabled, lara is able to increase the powerness of jump turning many times, you can enable with PB_SHOW_CHARGE_BAR the view of progress bar with current jump power.
It's advisable to add this feature because otherwise the player could have some difficulties to understand how much turns he made until that moment.

## PB_SHOW_CHARGE_COUNTER
Used in Customize=CUST_PARALLEL_BARS.
This flag works only when you set also the flag PB_PROGRESSIVE_CHARGE
The charge counter is a text showing the currnet number of turns. If you use this flag the counter text will printed on screen while lara is turning on the bar.
You can use this flag also togheter with PB_SHOW_CHARGE_BAR if you wish.

Remark: the source of charge counter text is always the PSX string with index = 220.
If you want change the text or create an other language version for it you can do, just you remember to use alwasy the PSX 220 string and to let always the formatter "%02d" in some point of the text because that formatter will be replaced with current number of turns.

## PLACE_FLOATING
Used in Animation command.
Lara is floating on water surface.

## PLACE_GROUND
Used in Animation command.
Lara is on the ground, i.e. she is out of water. For ground we could mean also when lara is falling down, other that climbing, monkey, jumping.
The most signficative description for "ground" is: "not in water"

## PLACE_LOW_WATER
Used in Animation command.
Lara is on low water. The low water is when lara touch the water with her feet but she is yet able to walk

## PLACE_SPECIAL
Used in Animation command.
I'm not sure about this status of lara. Surely it will be used when lara in on some vehicle or when she is in DOZY mode.
Probably this status means: lara is performing hardcoded animations where the physic rules about gravity or water floats will be ignored.

## PLACE_UNDERWATER
Used in Animation command.
Lara is underwater, please don't confuse this situation with PLACE_FLOATING.
Lara is underwater only when she is under the water surface and she is not able to breathe.

## PL_ADD_INFO_BAR
Used in Diary= command.
Differently by other PL_ flags, you can add the PL_ADD_INFO_BAR to another (but only one) PL_ flag.
Adding the PL_ADD_INFO_BAR to some other layout you inform trng that in current layout the bottom row of screen should be reserved and no text of diary should be typed over it.
You can use this flag when you want use the bottom strip of background image to show arrows or some short infos about keyboard commands to manage Lara's diary.
Pratically when trng detects this flag, it will format the text avoiding to cover also bottom row of screen, in this way you can dedicate that bottom zone with little icons or info texts.

Remark: the infos you could insert in Info bar are the description of keyboard commands accepted by Lara's Diary:

RIGHT ARROW  = Show next page
LEFT ARROW = Show previous page
HOME = show first page of diary
END = show last page of diary
ESCAPE = exit from Diary
SPACE = Zoom Image (show further little image of page at full screen). Newly SPACE to come back to original size.

## PL_CENTRAL_IMAGE
Used in Diary= command.
In this layout the further image will be showed at center of screen in top-half side.
Thre width of image in this layout is a bit larger than left/right layouts: the 60% of screen versus the 40% of previous layouts

## PL_CUSTOM_LAYOUT
Used in Diary= command.
If you are not satisfacted by other preset layouts you can create your own layout using this flag.
When you set PL_CUSTOM_LAYOUT (omitting all other PL_  layouts) trng engines will expect to find directly in the text to show the infos about layout to use.
In the <FORMAT
 section of text you can place these three tag formatters:

#FRAME_IMG#=XPos,YPos,SizeX,SizeY
#FRAME_T1#=XPos,YPos,SizeX,SizeY
#FRAME_T2#=XPos,YPos,SizeX,SizeY

and trng engine will use these frames to show the text and the further image.

All values are in micro units. You can use the utility [Get Screen Frames] in Tools panel of NG_Center program to get the correct values to type (XPos,YPos,SizeX,SizeY)

You can set a different layout for each page. When trng shows a page where there is no custom layout.
If you wish set a PL_ layout in Diary command, and also a format tag in some page, that page will be showed using the format tag, while other following pages with no format tag will be showed newly in PL_ layout you set in Diary command.

To know all tags you can use <FORMAT
 section of text, see the description of Diary= command in New Script commands section of Reference panel of NG_Center.

## PL_DOUBLE_PAGE
Used in Diary= command.
This layout divides the screen in two ideal pages.
The text will be formatted in two columns: first the left column (left half of screen) and then in the right column (the right half of screen)

IMAGE: if there is a (little) image to show in this layout, it will be always placed at top of left page and it will fit almost whole width of left page

TITLE: if there is a title for this page, it will be placed at top of left page. If there is also an image, the sorting (in left page) will be:

[Image]
[Title]
..text ..

Remark: if you use as image a very hight image that convers all left side, you can have text only in right page while in left side you can place this image, where it could be two little images casted one over other.

## PL_FIX_WIDE_SCREEN
Used in Diary= command.
When the game was played at full screen on a wide-screen monitor the images of Diary will be distorted and lara will become an obese dwarf.
If you want avoid this deformation you can set this flag. When trng finds the PL_FIX_WIDE_SCREEN flag, it will show the background image of diary non at full-screen but with a width a bit littler than screen to preserve the normal ratio between width-height with a ratio of 1.3 ([width] divided by [height] = 1.3)
In this wieving at left and right of diary there will be two empty columns.
This look could be good if you in background image draw a book style image.

## PL_LEFT_IMAGE
Used in Diary= command.
This layout work like a single wide page. The further image will be showed in top left corner of screen, and its size will be about 40% of Tomb Raider screen.
In this layout the text will be showed in two frames:

A first frame in top-right corner of screen, with same height of image at left.
A second and final frame in the bottom half of screen. This last frame will have the width of whole tomb screen.
Remark: further title will be elaborated as other text and it will be showed at top of top-right corner, and then, belove it, it will start the common text

## PL_LEFT_IMAGE_LOGO
Used in Diary= command
This layout is alike than PL_LEFT_IMAGE layout about the size and position of little image, but it this layout there is a single text frame, in bottom half of screen, while the top-right zone will be let empty.
This "hole" in top-right corner could seem "ugly" but it depends by the image you set as background for diary.
You could create a fixed image in the top-right corner of background image, to show a logo of your adventure, like it happend in Tomb Raider 2 and 3.

## PL_RIGHT_IMAGE
Used in Diary= command.
In this layout the further image will be showed in top-right corner, while the text will be formatted in two frames:

first frame is at top-left corner
while the second frame will fit the bottom half of tomb raider screen.
Remark: The further title will be showed at top of top-right frame before showing common text.

## PL_RIGHT_IMAGE_LOGO
Used in Diary= command.
This layout is similar than PL_RIGHT_IMAGE layout, but in this case there is a single text frame in bottom half size of tomb screen, while the frame at top-left of screen will be let empty.
Like for PL_LEFT_IMAGE_LOGO, you could use this layout to host in this "empty" top-left frame your logo for your adventure, drawing this image directly in background image for diary.

Remark: when you select a PL_LEFT_IMAGE_LOGO or PL_RIGHT_IMAGE_LOGO layout, wheter a page has no little image to show, the whole top-half of tomb screen will be let empty to avoid to type over the logo zone.

## PL_WIDE_IMAGE
Used in Diary= command.
This layout is very similar than above PL_CENTRAL_IMAGE layout, but in this case the image will be fit to cover the 90% of screen.
If you use this layout you should create the image to be wide but not very heigh, otherwise it could cover the whole tomb raider screen.
The usage of this layout could be useful to show two little image in each page, the trick is simply to create a unique image where there are two pictures one at left and the other at right.

## QSF_SIZE_260x200
Used with Customize=CUST_INNER_SCREENSHOT
The size of image with this resolution is 153 Kb for true color, while (about) 53 Kb for compressed 256 colors (non true color)

## QSF_SIZE_320x240
Used with Customize=CUST_INNER_SCREENSHOT
The size of image with this resolution is 225 Kb for true color, while (about) 77 Kb for compressed 256 colors (non true color)

## QSF_SIZE_390x300
Used with Customize=CUST_INNER_SCREENSHOT
The size of image with this resolution is 343 Kb for true color, while (about) 115 Kb for compressed 256 colors (non true color)

## QSF_SIZE_468x360
Used with Customize=CUST_INNER_SCREENSHOT
The size of image with this resolution is 493 Kb for true color, while (about) 163 Kb for compressed 256 colors (non true color)

## QSF_SIZE_520x400
Used with Customize=CUST_INNER_SCREENSHOT
The size of image with this resolution is 609 Kb for true color, while (about) 199 Kb for compressed 256 colors (non true color)

## QSF_SIZE_640x480
Used with Customize=CUST_INNER_SCREENSHOT
The size of image with this resolution is 900 Kb for true color, while (about) 288 Kb for compressed 256 colors (non true color)

## QSF_TRUE_COLOR
Used with Customize=CUST_INNER_SCREENSHOT
Adding this flag to QSF_SIZE you set a true color 24 bits for inner image.
By default, when this flag is missing, the image will be saved at 256 colors in compressed format.
The RGB images are very better than 256 compressed, of course, but the size of RGB images is very big and if you mix true color with a big size each savegame could reach 1 Mb of size and this is not a good choice because the players very often wish exchange savegames and, a too heavy savegame could be not goods in these exchanges on-line.

## RAIN_ALL_OUTSIDE
If you want have rain in all outside rooms without set "Rain" button for each room in ngle, you have to use rain_all_outside setting.
Remarks:
(1) If you use RAIN_ALL_OUTSIDE setting the trick to set the rain in intensity is simply to enable "Rain" button in first room that will be visited by lara and set in Water Intensity field (at right of multistate button water/rain/snow...) the intensity (from 1 to 4)
In this way the intensity for all level will be that you chose in that room.
Anyway if you set another "rain" room with different intensity, when lara will enter in that room the rain intensity will change like you set.

(2) If you don't mean to use rain rooms in current level it's better use command "Rain=RAIN_DISABLED" (or omit to type Rain= command) to inform trng engine to avoid a lot of computes to locate further rain rooms.

## RAIN_DISABLED
It's the same that don't insert any Rain= command in script

## RAIN_SINGLE_ROOMS
With this setting the rain will be showed only in specific
rooms signed as "Rain" in ngle and with outside status.

## RIB_ADD_SCANCODE_LIST
Used with Parameters=PARAM_INPUT_BOX
This flag allows to expand the scan codes recognized by keyboard parsing procedure.
By default input box parses and recognizes only sure key for all QWERTY keyboard layouts but ignores many keys that change for different kind of nationalized keyboards, like "?", "!", "@" ect.
With this flag you can say to InputBox procedure to check also for other keys, suppling some infos about scancodes and character to display on screen.

When you add RIB_ADD_SCANCODE_LIST flag, you have to add also a NG extra string with (fixed) number "666" (only to remember it better, don't worry)
The syntax of 666 ng string will be like this:
666: AABBCC, A1B1C1, A2B2C2 ... ect ...
Pratically you type a serie of hexadecimal number (omitting the "$" prefix) divided by commas.
Each hex number gives infos, to input box procedure, about what scan code (read by keyboard) should be parsed (the scan codes value are those you find in "KEYBOARD SCANCODES list" of Reference Panel) and that its corresponding ASCII code to draw on screen.
The "AA" pair will be the ascii code (UTF-7 set) , while the BB (and further, optional, "CC" and "DD") will be single scan code key to be down in same moment, to draw the "AA" ascii code.
For instance, if you wish having also the "?" character, you need to see in Keyboard scancodes list its scan code value.
Now you discover that it is .... MISSING.
The reason is that the scancodes show only the code for each single key of keyboard but some keys (the most) having two or more symbols for each key.
So, thinking about US/UK keyboards, the "?" is over "/" symbol, so you look for the "/" character in Keyboard scancodes list.
Now we find it: "53 ($35) Backslash /"
Well, the scan code in hex format is "$35" but to have the "?" on screen we need also to keep down the "SHIFT" key.
To complete our picking up of information we have to discover also the ascii code of "?" character.
It's not present in NG_Center the ascii list, anyway it's easy finding infos on internet, so we discover that the asci code for "?" character is $3F (or 63 in decimal but we'll work only in hex format)
To complete the required infos, we check for scancode of SHIFT: $2A
At end we can create the hex number to add "?" character to input box:
3F352A
Above hex number will add the "?" between printable characters.
You should "read" above number like a serie of 2 hex digits:
"3F": on screen we'll print ascii "3F", i.e. the "?" character
"35" when user hits the key "/" and in same moment there is the
"2A" key down (SHIFT)

Notes:
- There are two shift keys, left and right, with different scancodes, anyway, since it's not foreseen the "or" operator (to say "this key or this other ...), trng will extend left shift to same shift itself.
This means that, just you type the hex scan code for only one "SHIFT" key and trng will check also for other (left or right shift) key.

- About the ALT e CTRL keys, the speech above about SHIFT doesn't work, since tomb4 has a limited scanning and doesn't recognize [ALT GR] as right ALT but it sees all ALT like only one.
For CTRL is the same but in this case it's normal that two different CTRL gave same scan code.

- Max amount of numbers (divided by commas) is 64.

- Remember that the position of symbols, changes between different keyboard, in accoding with nationality. For this reasone if you wish support interpuntion characters (like "," ":" ";" ect.) you have only two ways:

1) Using only US/UK layout you should warn the players that some key don't work fine, showing a different character rispet that displaied on their keyboards

2) Creating two or more "langauge.dat" file, to have different scan codes in according with different layout "german" "french" "italian" ect.

- Theoratically you could scan also keys that have not a visible character to display, like "F1" "F2" ect. function keys. In this case it's better using also RIB_SHORTCUT_KEY+RIB_HIDE_TEXT  flags.
Then you'll supply, as ascii code, a special character to verify its presence after inputbox.

## RIB_ALIGN_CENTER
Used with Parameters=PARAM_INPUT_BOX
Set a central alignment of text typed by user.
The text will be centered inside of rectangle set in Image script command.
Notes:
- if you omit this flag, the text will be aligned at left
- It's not advisable using central alignment with a blinking caret because, in this case, everytime the caret will disappear, the text will become shorter and the central alignment will move the position, giving the feeling of a text shackering.

## RIB_BLINK_CARET
Used with Parameters=PARAM_INPUT_BOX
Perform a blinking of the caret while it's waiting input.
The blinking simulates the old MS-DOS prompt.

## RIB_HIDE_CARET
Used with Parameters=PARAM_INPUT_BOX
Omit to draw a underscore character "_" to simulate the cursor.
By default a cursor will be drawn always at right of last typed character.

## RIB_HIDE_TEXT
Used with Parameters=PARAM_INPUT_BOX
This flag will omit, to draw on screen, the keys hit by user.
In spite of this behaviour all valid keys typed by user will be stored in lastInputText variable and LastInputNumber (if only digits).
Usually you'll use this flag togehter with RIB_HIDE_CARET flag and, probably, to manage single key command with RIB_SHORTCUT_KEY flag.

## RIB_INPUT_BELOW_BIG_TEXT
Used with Parameters=PARAM_INPUT_BOX
This flag could be used only with RIB_PRINT_BIG_TEXT flag.
When you enable the print of a text (that in trng BigText variable) on screen, before waiting for user input, you can use the RIB_INPUT_BELOW_BIG_TEXT flag, to change the Y position (in height axis) of input box rectangle, to show input text immediately below last row of BigText drawn.
In this way the input zone will change in according with last printed text.
Note: this flag affects only the Y position of input box rect, while the relative height of rect or its width and X origin will be not modified.

## RIB_ONLY_CAPS
Used with Parameters=PARAM_INPUT_BOX
Force that all text typed by user will be in capital letters. By default, omitting this flag, the text will be drawn lower or capital letters, in according with usage or less of SHIFT key, as in common text editors.

## RIB_ONLY_DIGITS
Used with Parameters=PARAM_INPUT_BOX command
Force Input Box to accept only digits (0,1,2,3,4,5,6,7,8,9). Further letters (a,b,c ect) typed by user, will be ignored.

When you use this flag the user will be able only to type a number (with any digits) and at end , if he validates the input with ENTER, the digits he typed will be stored as number in "Last Input Number" trng variable. In this way, then you'll be able to check that value with other using flipeffect for trng variables.

Notes:
- If user quits the input box with ESCAPE command, and you used the RIB_ONLY_DIGITS flag, in "Last Input Number" there wil be the "-1" value.
- This flag is uncompatible with RIB_ONLY_LETTERS flag, of course.
- If you omit either RIB_ONLY_DIGITS and RIB_ONLY_LETTERS flags, input box will accept numbers and letters.
- If you omit the RIB_ONLY_DIGITS flag, the "Last Input Number" variable will be not affected by Input Box.

## RIB_ONLY_LETTERS
Used with Parameters=PARAM_INPUT_BOX command
Ignores digits (0123456789). With this flag the further digits typed  by user will be ignored.

## RIB_PRINT_BIG_TEXT
Used with Parameters=PARAM_INPUT_BOX
If you set this flag in RIB_Flags field, when the background image of input box has been draw, and first to read/draw the text typed by player, it will be drawn the text of trng variable "Big Text".
When you set this flag, it's necessary type also, in ExtraParam field of Parameters=PARAM_INPUT_BOX script command, the id of some Parameters=PARAM_RECT script command. The data of PARAM_RECT it will be used to set the rectangle (in micro units) where to draw the "Big Text" string. It will be used also the "ForeColor" field (of PARAM_RECT script command), to set the color of the text, while, the "BackColor" field, it will be ignored.
This is the description of RIB_PRINT_BIG_TEXT flag, anyway it should be important also explain why you should use this flag, rather to draw simply, on background image, further text.
The idea, behind RIB_PRINT_BIG_TEXT flag, is to have a generic input box, for instance with a background image showing a computer screen, that you'll be able to use over and over, with different text and therefore, requiring different passwords or input commands, to perform some operations in the game.
If you use this method, you should remember to perform the F409 trigger, to show the input box, within a TriggerGroup, where, first of F409 trigger, there will be a trigger to copy some text (from strings of .dat files) in "Big Text" variable.
In this way you'll be able to customize everytime, in run-time, the texts to show, in this "pc/control panel" input box, in game.

## RIB_PRINT_ONLY
Used with Parameters=PARAM_INPUT_BOX
With this flag, it happens that the input box will draw on the screen the "Last Input Text" typed text, (and the further BigText) without allowing to the user the chance to type text. In this way the input box works like a common overlapped image with some printed text.
Only one advantage, using Input box in this way, is to have already all settings, about position, color and font, to print the text in right position of previous inserted text.
You should use this flag only with a distinct Paramaters=PARAM_INPUT_BOX command, used only to display error messages.
Indeed, the input box with this flag doesn't allow any kind of text input but only to give the Escape command to quit the input box.

## RIB_SHORTCUT_KEY
Used with Parameters=PARAM_INPUT_BOX
Adding this flag, the input box will quit, like it has been set the ENTER command, immediately after first valid key chosen by user.
It should be reccomended, for instance, to handle a menu with a list of commands labeled with a number (1. 2. 3.) or letters (A. B. C. D.)

Note: this flag doesn't affect any other behaviour of input box, so if you wish that the caret will be hidden or the inserted text not drawn on screen, you'll have to add other flags, like RIB_HIDE_CARET and RIB_HIDE_TEXT.

## RIB_SOUND_ON_KEY
Used with Parameters=PARAM_INPUT_BOX
Perform a sound effect, whose number you typed in SfxSound field of Parameters command, everytime the player hits a key.

## ROOM_COLD
Used in according with ENV_ROOM condition in Animation or MultEnvCondition commands.
Lara is in a cold room.

## ROOM_DAMAGE
Used in according with ENV_ROOM condition in Animation or MultEnvCondition commands.
Lara is in a Damage room.

## ROOM_MIST
Used in according with ENV_ROOM condition in Animation or MultEnvCondition commands.
Lara is in a room marked as mist room (the [M] button in 2d plane)

## ROOM_OUTSIDE
Used in according with ENV_ROOM condition in Animation or MultEnvCondition commands.
Lara is outside (ponytail flys in the wind)

## ROOM_QUICKSAND
Used in according with ENV_ROOM condition in Animation or MultEnvCondition commands.
Lara is in quick sand room.

## ROOM_RAIN
Used in according with ENV_ROOM condition in Animation or MultEnvCondition commands.
Lara is in a rain room.

## ROOM_REFLEX
Used in according with ENV_ROOM condition in Animation or MultEnvCondition commands.
Lara is in a room with water light reflex (the [R] button in 2d plane).
Usually the reflex rooms are those over the water room.

## ROOM_SNOW
Used in according with ENV_ROOM condition in Animation or MultEnvCondition commands.
Lara is in a snow room

## ROOM_WATER
Used in according with ENV_ROOM condition in Animation or MultEnvCondition commands.
Lara is a water room. Please note that lara is a water room when she is floating on the water, or when she is swimming underwater, but she is NOT in water when she is simply walking in low water.

## ROTH_CLOCKWISE
Rotate in horizontal clockwise direction

## ROTH_INV_CLOCKWISE
Rotate in horizontal inverse clockwise direction

## ROTH_NONE
Disable the horizontal rotation

## ROTV_BACKWARD
Vertical backward rotation

## ROTV_FORWARD
Vertical forward rotation

## ROTV_NONE
Disable the vertical rotation

## SC_DOUBLE_HEIGHT
Set double height for characters, untouched the width

## SC_DOUBLE_SIZE
Set double width and double height for characters.

## SC_DOUBLE_WIDTH
Set double width for characters, untouched the height

## SC_HALF_HEIGHT
It will be reduced by half the height of characters, while the width will be not changed

## SC_HALF_SIZE
This value reduce by half both width and height of characters

## SC_HALF_WIDTH
It will be reduced by half the width of characters, while the height will be not changed

## SC_NORMAL
## SEQ_LOOP
The sequence will be perfomed in endless way. If you use this flag you'll have to use specific flipeffet to stop the sequence, otherwise it will be performed continuously.

## SEQ_LOOP_INVERSE
This flag works only if you set also SEQ_LOOP flag.
While the single SEQ_LOOP flag set an infinite loop with this sequence (supposing 4 textures from 0 to 3):

0 1 2 3 0 1 2 3 0 1 2 3 ....

When you set also the SEQ_LOOP_INVERSE flag, the sequence will be:

0 1 2 3 2 1 0 1 2 3 2 1 0 ..

## SEQ_STOP_AT_FIRST
If you set this flag you force engine to set always the first texture of range when the animation will be completed.
Differently, if you omit this flag the last texture showed depends by type of animation.
If you have set no loop, the last texture showed will be the texture of last index of array sequence.
If you have set a loop, the texture showed will depend by the moment when you perform the Stop texture sequence flipeffect.

## SET_ACCEPT_EXTRA_TAILINFOS
Used in Settings= command.
This setting forces trng engine to accept upto 32767 tail infos in the tr4 files.
Also using this setting the limit for ngle remains the old limit of 1024 tail infos, therefore the only reason to use this setting is when you use the program meta2tr to replace the room meshes directly in tr4 file.
Since meta2tr increases the number of tail infos you can use this setting to support the new extra tail infos.
Differently when you use the tr4 creadted by ng_tom2pc without changing, this setting is futile and probably also dangerous since it could create some trouble with animated range textures.

## SET_BLIND_SAVEGAMES
If you want forbid players of your game to use savegame editor to change savegames, just you add this flag in Settings command.
Any attempt of players to modify savegames will cause immediate crash at reload.

## SET_CRYPT_SCRIPT
Setting this flag the script.dat file will be crypted to forbid decompilation with some utilities.
If you set some security issues, like SET_DISABLE_CHEATS, it's advisable set also crypting of script.dat, otherwise some guy could simply decompile the script.dat and recompile it removing the SET_DISABLE_CHEATS flag.

## SET_DISABLE_CHEATS
Disable all hidden cheats to skip level, get infinite weapons or items.
Remark: to disable flycheat you have to use old command FlyCheat = DISABLED

## SET_FORCE_NO_WAITING_REFRESH
Used in Settings= command.
This setting, togheter with SET_FORCE_SOFT_FULL_SCREEN setting, borns to change the setting of tomb raider game to solve some problem with FMV playing or about flickering with images.
The speech about the opportunity to use this setting is the same done in the SET_FORCE_SOFT_FULL_SCREEN description: since the player is able to set or less this option byself, using the tomb raider setup window, it's better do not use this setting in the script to let the player free to enable or less it in according with the performance of

Technically this setting disables the waiting setting in the flip directx method.
Some experiment demostrates that this disabling is able to remove flickering of images from exclusive full screen resolution.

## SET_FORCE_SOFT_FULL_SCREEN
Used in Settings= command.
Note: from 1.2.2.7 version the previous SET_SOFT_FULL_SCREEN has been removed, anyway current SET_FORCE_SOFT_FULL_SCREEN constant performs the same job.
So, the real news, it's another: now the player is able to set or remove the soft full screen setting it byself, because this setting has been included in tomb raider setup window.
For this reason I changed the name of set_soft_full_screen to remember to you that now it's different that in the past.
Now you can omit any soft full screen setting, and the player will be able to enable or less it as he wish.
For this reason, it should be better do not use this forcing of soft full screen, because if you use it in the script, the player will be not able to disable it from setup window. It's more logical that it was the player to choose if to use or less this option, since he knows his computer, and how it works, better than you.

There are complicated questions about this setting.
When the game  (tomb4) works in full screen it's difficultous for trng starts a movie (FMV) because this screen mode, known as "full screen", is really an Exclusive mode, i.e. the game catches the screen and don't wish release it to other directx task (like the FMV viewer).
From version 1.2.0.9 I tried a way to stole to tomb4 the screen to play the FMV but this fight is complicated and so on some computers you could see some blinking at start and at end of FMV viewing.
If you think that these blinking are too boring you can use the  SET_FORCE_SOFT_FULL_SCREEN setting in the Settings= command.
With this setting the Exclusive Full Screen Video Mode will be converted at fly in a Software (cooperative) Full Screen mode.
Pratically the game will be showed in full screen, too, but the directx mode will be not exclusive and therefore the FMV will be showed with no need to fight. So no blinking or slow time at start or end the FMV.

Remark: This setting will have no effect if the user set tomb4 to work in windowed mode. This settings works only when tomb raider has been set to work in full screen mode.

## SET_PERFORM_FROM_CD
With this constant your game will be able to be performed from CD / DVD and generally from only-read storage support.
Pratically the reason to use this setting is when you want give your game to some friend, stored on cd and playable directly from CD with no need of installation of level editor (TRLE) folder on local disk.
When this setting is present the trng engine will save and load savegame not from current TRLE folder where is has been store, but in/from new folder in local drive C: with same name of first level description different by title.
For example if you use this option with a level described in script.txt like "City Of The Dead", trng will create a folder named:

C:\City_Of_The_Dead

and it will save and load savegames in/from this folder.

Remark:
If you want create an autostart CD in this way, you have to perform following steps:

1) Add in script.txt file in [Options] section the command:

Settings = SET_PERFORM_FROM_CD

(You can also add other SET_ flag to above line, of course)

2) Build script.txt

3) When you have in TRLE folder all level files (.dat , .tr4) necessary to play your game, you can save whole TRLE folder, including also the name of TRLE folder, in CD image to burn on cd support.

4) Before burning image you should add to image also following files, you find in Extra_NG_File.zip:

START.exe
autorun.inf

5) Now you can burn the CD and, at end, you should get following list browsing the new created CD:

START.exe
autorun.info
TRLE   (Folder)

Remark: the start.exe and autorun.inf files should be in root of CD, i.e. outside of TRLE folder

6) Now just inserting this cd in some CD reader and the game will be started and played byself, without need of installation on local disk (of target computer) of TRLE or other files or programs.

## SEXT_AIFF
Obsolete, see description of CUST_CUST_NEW_SOUND_ENGINE customize flag.

## SEXT_MP1
Obsolete, see description of CUST_CUST_NEW_SOUND_ENGINE customize flag.

## SEXT_MP2
Obsolete, see description of CUST_CUST_NEW_SOUND_ENGINE customize flag.

## SEXT_MP3
Obsolete, see description of CUST_CUST_NEW_SOUND_ENGINE customize flag.

## SEXT_MULTIPLE
Obsolete, see description of CUST_CUST_NEW_SOUND_ENGINE customize flag.

## SEXT_OGG
Obsolete, see description of CUST_CUST_NEW_SOUND_ENGINE customize flag.

## SEXT_WAV
Obsolete, see description of CUST_CUST_NEW_SOUND_ENGINE customize flag.

## SHOWC_OMIT_AMMO_NAME
Used in Cutomize=CUST_SHOW_AMMO_COUNTER command.
With this setting the current name of selected weapon/ammo will be omitter.
Therefor, instead reading "Normal CrossBow Ammo 12" you'll read only "12".

## SHOWC_USE_GRAPHIC_AMMO
Used in Cutomize=CUST_SHOW_AMMO_COUNTER command.
This setting remove the literal name of ammo and replace it with a single graphic character.
Using the TRNG font you can draw little images in some character of trng font set.
With the SHOWC_USE_GRAPHIC_AMMO the engine will print on screen one single character for each kind of ammo.
You can recognize the used graphic char for each ammo by following list:

Name_in_font_Editor     Ammo_Type
------------------------------------------------
127: [GRAPHIC]          Pistol Ammo
129: [GRAPHIC]          Revolver Ammo
138: S                  Uzi Ammo
140: OE                 Shotgun Normal Ammo
141: [GRAPHIC]          Shotgun Wideshot Ammo
142: Z                  Grenadegun Normal Ammo
143: [GRAPHIC]          Grenadegun Super Ammo
144: [GRAPHIC]          Grenadegun Flash Ammo
154: s                  Crossbow Normal Ammo
156: oe                 Crossbow Poison Ammo
157: [GRAPHIC]          Crossbow Explosive Ammo
------------------------------------------------

Remark: to add your icons in your NG font character set, you should perform following operations:

1) Go in [Tools] panel of NG_Center
2) Click on [NG Font Editor] button
3) In the NG Font Editor window click on [Choose Wad] button
4) Now select a wad where you have the NG font object.
5) Now select in combo box (belove the [START IMPORTING] button) some character, taking care to choose the correct number read from above list.
6) Now click on [Import BMP] button closed to little single character image, and select the bmp file where you stored the image for that char/ammo.
7) Select in the [Type] list the voice "Graphic" and now click the button [Recompute]
8) Repeat points 5, 6 and 7 until you assigned all images for all ammo.
9) At end click on [Exit and Save wad] button.

## SHOWC_USE_GRAPHIC_WEAPON
Used in Cutomize=CUST_SHOW_AMMO_COUNTER command.
This setting force the rimotion of literal name of ammo type and replace it with a graphic character to signal the current extracted weapon.

The character used are the following:

Name_in_font_Editor     Ammo_Type
----------------------------------
127: [GRAPHIC]          Pistol
129: [GRAPHIC]          Revolver
141: [GRAPHIC]          UZI
143: [GRAPHIC]          Shotgun
144: [GRAPHIC]          Crossbow
157: [GRAPHIC]          Grenadegun
-----------------------------------

For infos about how inserting the weapon icons in the font see the description of SHOWC_USE_GRAPHIC_AMMO constant.

## SNOW_ALL_OUTSIDE
The snow will be showed in all outside rooms, ignoring multistate button. This means that also a room without "snow" button will have the snow if it is different than Water room and it has the [O] outside status.

Remarks:
(1) If you use SNOW_ALL_OUTSIDE setting, and you want set the intensity for snow in all level, you can set a single room with "Snow" attribute in ngle and set in that room in water intensity field the value for snow intensity. That value will be used for whole level.

(2) If you don't mean to use snow rooms in current level it's better use command "Snow=SNOW_DISABLED" to inform trng engine to avoid a lot of computes to locate further snow rooms.

## SNOW_DISABLED
This settings is the same to omit to insert Snow= command. The futher rooms labeled as "Snow" room in ngle will be ignored, i.e. no snow effect will be showed.

## SNOW_SINGLE_ROOM
Only the room with "Snow" label in ngle and with [O] (outside) status will have the snow in game.
Each room will have own snow intensity read from field Water Intensity at right of multistate button Water/Rain/Snow ...

## SPCF_FREEZE_HAIR
Used with Parameters=PARAM_ACTOR_SPEECH command
There is a bug that I've not yet been able to fix. When there is the swapmesh of lara's head the hair of lara have a jump. In spite I tried to remap ponytail vertices with rigth numbers this problem is yet present.
To reduce this problem you can add the SPCF_FREEZE_HAIR flag and the ponytail will be kept down.
This solution is not so fine, of course but we suppose that when you do speech lara then you look head on her and so the ponytail will be not visible.
Differently, omitting this flag the jump of ponytail is so strong that is could be visible also looking the face of lara.

## SPCF_LOOP
Used with Parameters=PARAM_ACTOR_SPEECH command
If you are not interested to control all syllables but you wish only having lara speaking for a given number of frames in random way, you can add the SPCF_LOOP flag and then type in Parameter field the number of frames for the speaking sequence.
In this situation trng will perform the commands you inserted in loop mode until to reach the wished number of frames you set in parameter field.

Example:

Parameters= PARAM_ACTOR_SPEECH, 1, SPCF_LOOP, 120, IGNORE, LARA_SPEECH_HEAD1, 14, 0, 4, SPC_SYLL, SPC_PAUSE,SPC_SYLL+6, SPC_PAUSE,SPC_SYLL+3

With above parameters lara will talk for 4 seconds (4 * 30 = 120) moving the mouth following the sequence of commands you typed for 4 seconds.

## SPCF_OLD_SPEECH_SLOTS
Used with Parameters=PARAM_ACTOR_SPEECH command
If you wish use the old multiple slots (LARA_SPEECH_HEAD1/2/3/4  or ACTOR1_SPEECH_HEAD1/2/3/4) method, you have to add the SPCF_OLD_SPEECH_SLOTS flat.
With old method trng engine will look for the first head mesh at index = 14 (15th mesh) of given speech slot.
In the case you OMIT this flag, trng will use the first mesh (index=0) and it will be the mute face.
With old method the sorting was inverted: MaxOpenMouth/OpenMouth/LitleOpen/Mute  while with new method is Mute/LittleOpen/OpenMouth/MaxOpen
The use (or less) of this flag it will allow to manage both situations.

## SPC_ANIMATION
Used with Parameters=PARAM_ACTOR_SPEECH command
This command will play the custom animation for given moveable whom index you added to the command.
Example:

SPC_ANIMATION+15

It will perform the animation 15

## SPC_HEAD_NOD
Used with Parameters=PARAM_ACTOR_SPEECH command
This command works like SPC_HEAD_SHAKE command but in this case the head movement will be up/down insteady by left/right.
Moving up/down the head means "yes", "ok", "it's true", "I agree" ect.
Note: really the movement will be not with same angle upstairs and downstairs, since the nod is more with downstairs moving.
Trng will simulate this behavior using the angle supplied for down but only the half of that angle for upper movement.

About syntax of arguments see the description of SPC_HEAD_SHAKE command.

## SPC_HEAD_SHAKE
Used with Parameters=PARAM_ACTOR_SPEECH command
This command will simulate the shaking head to show the feeling to say: "no, I don't...", "it's not true" ect.
You can set two values with this command:
- The speed of shacking movement
- The angle in degrees of the head turning, computed for each side.

To add these values you have to use following formula:

SPC_HEAD_SHAKE + Speed * 64 + Angle

Speed value is the turning increment given in degrees. Each increment will be added at each frame.
For instance to have a shacking of 20 degrees at speed of 3 degrees for frame you'll type:

SPC_HEAD_SHAKE + 192 + 20

Where, 190 is 3 (degrees) * 64 and 20 is the angle for a single side.

Note: remember that for both arguments the max value is 63.

## SPC_LOOK_DOWN
Used with Parameters=PARAM_ACTOR_SPEECH command
Move the head downward for wished degrees and frame durate.
See the description of SPC_LOOK_RIGHT command to know the syntax of the formula.

## SPC_LOOK_LEFT
Used with Parameters=PARAM_ACTOR_SPEECH command
Move the head at left for wished degrees and frame durate.
See the description of SPC_LOOK_RIGHT command to know the syntax of the formula.

## SPC_LOOK_RIGHT
Used with Parameters=PARAM_ACTOR_SPEECH command
Move the head at right.
This command accept two arguments:
- The angle about turning respect to forward looking (0 degrees)
- The time spent for turning and still in turned position.

Since both arguments cann't be bigger than value 63, to have a larger choice the format is not in frame and single degrees but rather:
- The angle will be set in double degrees of wished degrees. In this way if you type 45 the turning will be by 90 degrees.
- The durate is in teenths of second, so with max value, 63, you can reach six seconds of right looking. The durate has to be multiplied by 64.

For instance, if you wish a turning of 45 degrees, kept for 2.5 seconds, the formula will be:

SPC_LOOK_RIGHT+22+1600

The 22 will be multiplied by 2 from trng to get 44 (about 45), while 1600 is 25 * 64, and 25 teenth of second are 2.5 seconds, of course.

Notes:

- All SPC_LOOK... and SPC_HEAD_ commands work only with Lara. If you wish have same effect for other moveable it will be necessary creating a new custom animation to move the head and then use a SPC_ANIMATION command to force that animation.

- All SPC_LOOK and SPC_HEAD commands will be overlapped to the following commands in their playing.
This means that this command will start immediatly the turning of head, but in same frame, it will begin also next command, for example to move actor's mouth.
If you wish that trng waits that the turning was completed before performing next command, you have to add after this LOOK command a SPC_PAUSE command with the same time durate of this LOOK command.

## SPC_LOOK_UP
Used with Parameters=PARAM_ACTOR_SPEECH command
Move the head upward for wished degrees and frame durate.
See the description of SPC_LOOK_RIGHT command to know the syntax of the formula.

## SPC_MESH
Used with Parameters=PARAM_ACTOR_SPEECH command
With this command you choose directly the head mesh to show.
The number you'll add to SPC_MESH command if the relative index, beginning from zero, respect first speech mesh.
For example:

SPC_MESH+4

this means: the fifth mesh (0,1,2,3,4  are 5 meshes)
If you wish set a frame durate different than framerate field, you can add the number of frames for durate:
The number of frames has to be multplied by 64:
For instance:

SPC_MESH+256+4

means: show mesh with index=4 (fifth mesh) for 4 frames (4 * 64 = 256)

Note: Since SPC_MESH command has value = 0, you can omit to type it, just you avoid to insert any other SPC_ command:

For instance:

256+4

is the same of:

SPC_MESH+256+4

If you let default frame rate for durate (useful to be able, in a second moment, to change the speed of whole sequence in fast way), you can type simply the number of mesh:

For instances these:

 2,1,0,0,2

are a sequence of SPC_MESH commands with default frame rate of durate.

## SPC_NEXT_STATEID
Used with Parameters=PARAM_ACTOR_SPEECH command
Force the next state id to the value given as argument.
Example:

SPC_NEXT_STATEID+4

Set as next state id for current actor the value 4.

## SPC_PAUSE
Used with Parameters=PARAM_ACTOR_SPEECH command
With this command you can do a pause with a closed mouth of the actor for the given frames.

SPC_PAUSE + FramesOfPause

For instance:

SPC_PAUSE + 45

With above command the actor will remain mute (closed mouth) for about one second and half (45 frames)

By default the number of frame is the durate of the pause but you can use also SPC_PAUSE to align speech commands with demo frames of current demo is playing.
In this case we speak about "absolute" frame (while the durate is a "relative" frame, i.e. relative to current demo frame + the durate you set).
To force an absolute frame you have to add to the argument also the special constant "DEMO_FRAME".

For example:

SPC_PAUSE+DEMO_FRAME+136

Above command means: "wait until the demo frame counter reachs the 136th frame.
In this way you are sure that next command will be performed at demo frame 136.
The DEMO_FRAME constant is very useful to syncrhonize speech commands with current demo.

notes:
- Please note that with this command the valid range of pause durate value (relative or absolute) is 1 / 2047
- In the case you use absolute pause (using DEMO_FRAME) but that demo frame was already elapsed there will be no pause and the next command will be immediatly performed.

## SPC_PERFORM_TG
Used with Parameters=PARAM_ACTOR_SPEECH command
It performs the TriggerGroup with the given number.
Example:

SPC_PLAY_TG+24

It will perform the triggergroup=24 from current level section of script.txt file.

## SPC_PLAY_CD
Used with Parameters=PARAM_ACTOR_SPEECH command
This command will play the audio with the supplied number.
Example:

SPC_PLAY_CD+112

it will play 112.wav file from AUDIO folder on private channel in single playing mode.

## SPC_PLAY_SFX
Used with Parameters=PARAM_ACTOR_SPEECH command
This command will play the sfx sound you set as argument.
For example:

SPC_PLAY_SFX+104

It will play the 104 sound (HECKLER_KOCH_STOP)
Range of values is 0/4095

## SPC_SEQUENCE
Used with Parameters=PARAM_ACTOR_SPEECH command
This command allows you to have a custom seuquence of meshes but for other expressions.
It simulates the opening and closing mouth with a sequence of heads, in same way you can simulate other expression, like smiling, angrious ect.

You have to supply two number:

- The index of first mesh of animated sequence
- The number of mesh used in this sequence

With this command is not possible set the frame durate of animation, it will be used the default frame rate set.

Formula to type values for this command is:

SPC_SEQUENCE + NumberOfMesh* 64 + IndexFirstMesh

For instance:

SPC_SEQUENCE + 256 + 12

It means: the aimated sequence begins from mesh with index = 12 and the mesh of sequence are 4 (4 * 64 = 256)

## SPC_SYLL
Used with Parameters=PARAM_ACTOR_SPEECH command
This command will show the simulation of a syllable.
In our convention, we define the syllable like a single animation from closed mouth to max-open mouth or vicevera.
This means that, if actor in the moment of this command, had closed the mouth, the syllable command will change heads from closed mouth to max-open mouth.
In the case, differently, at begin of this command, it had a full opened mouth, the syllable command will show in sequence all heads to do close the mouth.
Pratically the SPC_SYLL command is not a single mesh, but an animated sequence of meshes, from mix to max opening of mouth or viceversa.
If you don't add any value to this command, it will be used the default frame rate, and so, each head of animated sequence will be showed for FrameRate durate.
If you wish having a different frame rate only for this command, you can add the number of frames for each head, mutlplying it by 64:

SPC_SYLL+192

Since 192 means 3 frames (3*64= 192), each head of animated sequence will be displayed for 3 frames.

If you add a value between 1 to 64, it will be the number of syllable to be played with this command.
For instance:

SPC_SYLL+5

It will show 5 animated sequence of syllable. This means that the mouth will be opened and closed twice, and then another single syllable.

## SPC_TEXT
Used with Parameters=PARAM_ACTOR_SPEECH command
This command will draw the extra ng string whom index you typed as argument.
Example:

SPC_TEXT+23

It will draw the extra ng string with index = 23.
About this command it's important remember the flipeffects F399/400/401 whereby those you can set only once at start the position and color of texts for each actor, in this way in the speech commands you can simply set the string with SPC_TEXT command and it will be drawn with color and position for the actor linked with this speech command.
Note: all texts will remain on screen until you don't set another SPC_TEXT command for same actor, or when you use the syntax:

SPC_TEXT+0

The string with index = 0 will be read as "remove previous text and do not print any string for current actor"
You can also omit the +0 argument, of course.

SPC_TEXT

with no string index supplied has same meaning.

## SPF_BLINK_SELECTED
Used in SavegamePanel= script command.
By default the selected savegame in the list will be highlighted with a constant inversion of colors. If you want have a blink of this highlighting you can use the SPF_BLINK_SELECTED flag.

## SPF_NO_PANEL_TITLE
Used in SavegamePanel= script command.
By default in central position of first row trng will draw the title of panel savegame: �Load Game� or �Save Game�.
If you want omit this text you can use this flag.
The only reason to omit the title of panel is when you want type some title directly in the background image.
Anyway remember that in this way you cann�t handle the difference between �Load� and �Save� game.

## SPF_NO_TIME_IN_LIST
Used in SavegamePanel= script command.
By default in the savegame list will be showed the savegame with (old) format:

002  Name of Level       1 days  02:41:11

The game time is �1 days  02:41:11� part. If you use the SPF_NO_TIME_IN_LIST flag the game will be omitted and for this reason the width required to host savegame list will be less.
You could use this flag when the layout requires savegame list in left (or right) half of screen.

## SPF_PRELOAD_BKG_IMAGE
Used in SavegamePanel= script command.
By default, trng will load and show the background image of savegame panel when the player requires the save/load panel.
Since the operation to load from disk could require a short time in game could be a delay. To avoid this dealy you can add the SPF_PRELOAD_BKG_IMAGE flag and trng will load in memory the background image in advance, when the level was loading, and preserves it in memory to show fastly when it will be necessary.

Remark: the only reason to omit this flag is when you have a lot of preloaded images in same level, or when you detect a bug about preloaded background image. In very seldom cirucstances the preloaded image of saveame panel could appear damaged. This problem disappears exiting and entering newly in game but it's probably this problem happens own when there is a preloaded image and it occurs some circustance that damages the memory where the image has been stored.

## SPF_SCROLL_PAGE
Used in SavegamePanel= script command.
When the savegame list shows only a part of total managable savegames, the player will be able to scroll the saveame to see next hidden save.
By default trng will do scrolling a single savegame at once with DOWN input, but if you wish show a block of savegames in same time you can use this option.

## SPL_CENTRAL_IMAGE
Used in SavegamePanel= script command.
In this layout the image is at center of top half of screen. The image will be a bit wider than other layout formats, so this layout gives more scene to image and works fine in wide screen mode.
The savegame list will be showed in bottom half of screen.

## SPL_LEFT_IMAGE_BOTTOM_INFO
Used in SavegamePanel= script command.
In this layout the image is top left corner, while the info frame is belove the image. The savegame list will be in right half of screen.

## SPL_LEFT_IMAGE_NO_INFO
Used in SavegamePanel= script command.
This layout ha no info frame. The image will be in top left corner while savegame list will be in bottom half of screen.
Remark: with this layout in top-right corner will remain an "empty� zone, where you could place (in background image) some logo of your adventure.

## SPL_LEFT_IMAGE_RIGHT_INFO
Used in SavegamePanel= script command.
In this layout the image is in top-left side while the info frame is at right of image, i.e. in top-right side.
Hence, the list of savegame will be in bottom half of screen.

## SPL_RIGHT_IMAGE_BOTTOM_INFO
Used in SavegamePanel= script command.
In this layout the image is top right corner, the info frame will be under the image and savegame list will in left side of screen.

## SPL_RIGHT_IMAGE_LEFT_INFO
Used in SavegamePanel= script command.
In this layout the image will be in top-right corner, the info frame at left of image and the savegame list in bottom half of screen.

## SPL_RIGHT_IMAGE_NO_INFO
Used in SavegamePanel= script command.
In this layout the image is in top-right corner, there is no info frame and the savegame list will be in bottom half of screen.
Remark: since in this layout the top-left corner will reamain empty you could insert in this space a logo of your adventure drawing it directly in background image.

## SQ_HIGH_QUALITY
## SQ_LOW_QUALITY
## SQ_MEDIUM_QUALITY
## STATE_BACK
State-id for animations

## STATE_BACK_JUMP
State-id for animations

## STATE_CLIMB_DOWN
State-id for animations

## STATE_CLIMB_END
State-id for animations

## STATE_CLIMB_LEFT
State-id for animations

## STATE_CLIMB_RIGHT
State-id for animations

## STATE_CLIMB_START_AND_STANDING
State-id for animations

## STATE_CLIMB_UP
State-id for animations

## STATE_COMPRESS
State-id for animations

## STATE_CONTROLLED
State-id for animations

## STATE_CONTROLLED_117
State-id for animations

## STATE_CONTROLLED_92
State-id for animations

## STATE_CONTROLLED_93
State-id for animations

## STATE_CONTROLLED_94
State-id for animations

## STATE_CONTROLLED_97
State-id for animations

## STATE_CONTROLLED_LET
State-id for animations

## STATE_CONTROLLED_LET_96
State-id for animations

## STATE_DASH
State-id for animations

## STATE_DASH_DIVE
State-id for animations

## STATE_DEATH
State-id for animations

## STATE_DEATH_SLIDE
State-id for animations

## STATE_DIVE
State-id for animations

## STATE_DUCK
State-id for animations

## STATE_DUCK_72
State-id for animations

## STATE_DUCK_LEFT
State-id for animations

## STATE_DUCK_RIGHT
State-id for animations

## STATE_EXTERNAL_CORNER_LEFT
State-id for animations

## STATE_EXTERNAL_CORNER_RIGHT
State-id for animations

## STATE_FALL_BACK
State-id for animations

## STATE_FAST_BACK
State-id for animations

## STATE_FAST_DIVE
State-id for animations

## STATE_FAST_FALL
State-id for animations

## STATE_FAST_TURN
State-id for animations

## STATE_FAST_TURN_14
State-id for animations

## STATE_FAST_TURN_4
State-id for animations

## STATE_FORWARD_JUMP
State-id for animations

## STATE_GLIDE
State-id for animations

## STATE_HANG
State-id for animations

## STATE_HANG_LEFT
State-id for animations

## STATE_HANG_RIGHT
State-id for animations

## STATE_HANG_TURN_LEFT
State-id for animations

## STATE_HANG_TURN_RIGHT
State-id for animations

## STATE_INTERNAL_CORNER_LEFT
State-id for animations

## STATE_INTERNAL_CORNER_RIGHT
State-id for animations

## STATE_LEFT_JUMP
State-id for animations

## STATE_MONKEY_180
State-id for animations

## STATE_MONKEY_FORWARD
State-id for animations

## STATE_MONKEY_LEFT
State-id for animations

## STATE_MONKEY_RIGHT
State-id for animations

## STATE_MONKEY_STILL_OR_HANG_SWING
State-id for animations

## STATE_NULL_116
State-id for animations

## STATE_NULL_19
State-id for animations

## STATE_NULL_50
State-id for animations

## STATE_NULL_51
State-id for animations

## STATE_NULL_54
State-id for animations

## STATE_NULL_62
State-id for animations

## STATE_NULL_63
State-id for animations

## STATE_NULL_64
State-id for animations

## STATE_NULL_68
State-id for animations

## STATE_NULL_69
State-id for animations

## STATE_NULL_87
State-id for animations

## STATE_ON_ALL_FOURS_BACK
State-id for animations

## STATE_ON_ALL_FOURS_FORWARD
State-id for animations

## STATE_ON_ALL_FOURS_STANDING
State-id for animations

## STATE_ON_ALL_FOURS_TO_HANG
State-id for animations

## STATE_ON_ALL_FOURS_TURN_LEFT
State-id for animations

## STATE_ON_ALL_FOURS_TURN_RIGHT
State-id for animations

## STATE_PB_HANGING
State-id Parallel Bar

## STATE_PB_LEAP_OFF
State-id Parallel Bar

## STATE_PICK_UP
State-id for animations

## STATE_PICK_UP_98
State-id for animations

## STATE_PICK_UP_FLARE
State-id for animations

## STATE_POLE_DOWN
State-id for animations

## STATE_POLE_LEFT
State-id for animations

## STATE_POLE_RIGHT
State-id for animations

## STATE_POLE_STATIC_99
State-id for animations

## STATE_POLE_UP
State-id for animations

## STATE_PULLEY
State-id for animations

## STATE_PULL_BLOCK
State-id for animations

## STATE_PUSH_BLOCK
State-id for animations

## STATE_PUSH_PULL_READY
State-id for animations

## STATE_REACH
State-id for animations

## STATE_RIGHT_JUMP
State-id for animations

## STATE_ROLL_23
State-id for animations

## STATE_ROLL_45
State-id for animations

## STATE_ROPE
State-id for animations

## STATE_ROPE_114
State-id for animations

## STATE_ROPE_115
State-id for animations

## STATE_ROPE_CLIMB_DOWN
State-id for animations

## STATE_ROPE_CLIMB_UP
State-id for animations

## STATE_ROPE_LEFT
State-id for animations

## STATE_ROPE_RIGHT
State-id for animations

## STATE_RUN
State-id for animations

## STATE_SLIDE
State-id for animations

## STATE_SLIDE_BACK
State-id for animations

## STATE_SPECIAL
State-id for animations

## STATE_SPLAT
State-id for animations

## STATE_STEP_LEFT
State-id for animations

## STATE_STEP_RIGHT
State-id for animations

## STATE_STOP
State-id for animations

## STATE_SURF_BACK
State-id for animations

## STATE_SURF_LEFT
State-id for animations

## STATE_SURF_RIGHT
State-id for animations

## STATE_SURF_SWIM
State-id for animations

## STATE_SURF_TREAD
State-id for animations

## STATE_SWAN_DIVE
State-id for animations

## STATE_SWIM
State-id for animations

## STATE_SWITCH_ON
State-id for animations

## STATE_SWITCH_ON_126
## STATE_SWITCH_ON_41
State-id for animations

## STATE_TREAD
State-id for animations

## STATE_TR_FALL_122
State-id Tight Rope

## STATE_TR_FALL_123
State-id Tight Rope

## STATE_TR_POSE
State-id Tight Rope

## STATE_TR_WALK
State-id Tight Rope

## STATE_TURN_LEFT
State-id for animations

## STATE_TURN_RIGHT
State-id for animations

## STATE_UNDERWATER_DEATH
State-id for animations

## STATE_UP_JUMP
State-id for animations

## STATE_USE_KEY
State-id for animations

## STATE_USE_PUZZLE
State-id for animations

## STATE_WADE
State-id for animations

## STATE_WALK
State-id for animations

## STATE_WATER_OUT
State-id for animations

## STATE_WATER_ROLL
State-id for animations

## SWT_BASE_ZERO
Used in Switch script command.
By default the Switch command perform the first TriggerGroup of the list, when the PlaceFolderVariabile has value "1", it performs the second when variable = 2, ect.
Differently, if you wish the first value was "0" (to perform first triggergroup of the list) you can use this flag.

## SWT_RANDOM_MODE
Used in Switch script command.
If you want introduce random elements in your adventure you can use this flag to perform different triggers in random way.
When you use the SWT_RANDOM_MODE flag, the field VariablePlaceFolder will be ignored, and the number used to select the TriggerGroup to perform will be generated in random way.
For example if you place three triggergroup IDs in the list, when you perform this switch the trng engine will generate a random number in the range (1 - 3) (or 0 - 2 if you use BASE_ZERO flag), and then it will perform the trigger group in according with this random number.

## TCF_CREATURE
Moveable is a creature: it borns, lives and dies. It has AI to move itself

## TCF_LAND_CREATURE
## TCF_ONLY_EXPLODE
This moveable could be killed only whereby explosion (like mummy or skeleton)

## TCF_SAVEPOS
Save informations about current moveable in savegame

## TCMD_EXIT
Used in TriggerGroup= command.
This constants should be used as second value of three values, where the first was TGROUP_COMMAND.
The TCMD_EXIT command quit the execution of current triggergroup, returning as result the value you type in third (and next) argument.
About this returned value it can be "TRUE" or "FALSE".
In conditional triggergroups you use can set the final result of current triggergroup condition. If you set "TRUE" as next argument the condition will be true, while with FALSE will be false, of course.
Note: also when the Triggergroup, where you use a TCMD_EXIT command, is NOT a condition but only a serie to triggers to affect some effect in game, you HAVE to return a value as third argument and, in the case of not conditional triggergroups, you have to set always TRUE.

Examples:
The triple values:
	TGROUP_COMMAND, TCMD_EXIT, TRUE
will quit the execution of curren triggergroup, returning "true". Therefore, if current triggergroup was a conditon, the condition will be true.

The triple values:
	TGROUP_COMMAND, TCMD_EXIT, FALSE
will quit the execution of curren triggergroup, returning "true". Therefore, current triggergroup was a conditon and the condition will be false.

## TCMD_GOTO
Used in TriggerGroup= command.
This value will be used in second (of three) values.
First value had to be the TGROUP_COMMAND constant.
The  TCMD_GOTO constant forces a jump to other trigger in same triggergroup command.
The third value will be the index of trigger (group of three values) to perform.

Example:

TriggerGroup=	1, $5000, 118, $2A,              ;0 index
			   TGROUP_COMMAND, TCMD_GOTO, 3  ;1 index, jmp to trigger "$2000, 406, $1"
			   $5000, 137, $0129,            ;2 index
			   $2000, 406, $1                ;3 index

Using GOTO you can getting easier some triggergroups  when you manage complicated sequence of IF ELSE conditional groups.
For instance, now we see, in natural language, a sequence of conditons and commands in this triggergroup:

TriggerGroup=Id, ACondition ; 0
  			 	TriggerA1  ; 1
			 	TriggerA2  ; 2
			 ELSE BCondition ; 3
				TriggerB1  ; 4
				TriggerB2  ; 5
			TriggerC1  ; 6
			TriggerC2  ; 7
			TriggerC3  ; 8

Above is an, only simbolic, description of a triggergroup. Where you read "Condition" there will be three value for some exported conditional trigger, while where you read some "Trigger..", there are three values for some exported trigger.
Now, we suppose to wish perform TriggerA1/2 when ACondition is true and TriggerB1/2 when BCondition is true, like it appears in above triggergroup. But, let's say, we wish perform the C1/2/3 triggers, in all cases, after TriggerA1/A2 and after (how already it happens)  TriggerB1/2.
In last release of trng, it was possible in only one way: duplicating the TriggerC1/2/3 in this way:

TriggerGroup=Id, ACondition ; 0
			 	TriggerA1  ; 1
			 	TriggerA2  ; 2
				TriggerC1  ; 3
				TriggerC2  ; 4
				TriggerC3  ; 5
			 ELSE BCondition ; 6
				TriggerB1  ; 7
				TriggerB2  ; 8
				TriggerC1  ; 9
				TriggerC2  ; 10
				TriggerC3  ; 11

It happened because, after a condition, it will be performed ONLY the triggers after that condition, and first of next ELSE statement.
When the execution entered in "ACondition is true" the execution stopped at next found ELSE.
Now, using GOTO command we can passing over this limit, in this way:

TriggerGroup=Id, ACondition ; 0
			 	TriggerA1  ; 1
			 	TriggerA2  ; 2
			 	COMMAND GOTO  7 ;3
			 ELSE BCondition ; 4
				TriggerB1  ; 5
				TriggerB2  ; 6
			TriggerC1  ; 7
			TriggerC2  ; 8
			TriggerC3  ; 9
In above way, when the ACondition is true, it will be executed the TriggerA1/2 and then (the goto) also the TriggerC1/2/3, avoiding to have to duplicate them.

From above example we discover that the GOTO command will be used very often first of an ELSE statement, to go on to execute other triggers below, in the triggergroup.
Anyway there is another new approach that we can having with GOTO command.

In past trng versions, the execution of triggers, inside a given TriggerGroup, was only from top (first row) to bottom (last row) with further skipping of some trigger in according with result of some conditions.
Now, using GOTO you can perform newly a previous trigger, generating a "loop"
When you use this chance you should take care to avoid endless loops, otherwise the game will freeze.
There are two situations where you can use loops (whereby "goto" commands) with no trouble:

1) When you use a variable (trng variable) to keep track of number of cycles (amount of times) that you performed same triggers. In this method you'll have to insert in the cycle a condition to verify if it is the moment to quit the loop.
This method is very used to manage the arrays (vectors). In spite there is no yet many stuff about array management in NG_Center or triggers, these new skills could be added in future.
We see a little (meaningless) example:

TriggerGroup= Id,  Set A1=0	; 0
			   TriggerAlfa	; 1
			   TriggerBeta  ; 2
			   TriggerDelta ; 3
			   A1=A1+1		; 4
			   If A1 less or even than 4     ; 5
				   COMMAND GOTO 1   ; 6
			   ELSE TriggerGamma    ; 7
				   TriggerOmega    ; 8
Above is a loop, where the group of triggers (Alfa, beta and delta) will be executed four times and, at end of this cycle, it will be perfomed the Triggers Gamma and Omega.
If you follow the execution and keep in mind the value of A1, modified by "A1=A1+1" and then checked by "IF AI less or even than 4" condition, you should understand because Alfa,beta and delta triggers will be performed 4 times.

Note: please, don't try to use a loop to move items on screen, or to wait a condition different than trng variables or memory zones you change in the loop, otherwise the game will freeze. The right way to perform continuosly a triggergroup is to use, from a trigger in the level, an activation Perform Always. Trying to get same result with a cycle based on GOTO command will fail, because the game requires to draw meshes, read input command from keyboard and other stuff, to work fine, it cann't be stopped, in an endless cycle, inside a triggergroup.

2) When you want "recycle" some serie of triggers you typed above in a huge triggergroup.
This situation is alike to that already showed to skip ELSE statement, only difference is that you could use goto also to come back, to some previous trigger block, if you wish.

## TCMD_LOG
Used in TriggerGroup= command.
This constant should be used as second value of three values, where the first was TGROUP_COMMAND.
Used only for script debugging.
With this command you can enable or disable the log, like you had changed the script command:

DiagnosticType= DGX_LOG_SCRIPT_COMMANDS, EDGX_CONCISE_SCRIPT_LOG

In next value, third argument, you'll type ENABLED to enable the log, or DISABLED, to disable the log.
Please note that this command is not able to enable/disable the diagnostics but only the DGX_LOG_SCRIPT_COMMANDS flag.
This means that you should use it in this way:
Type in [Options] section of the script the commands:

Diagnostic= ENABLED
DiagnosticType= 0, EDGX_CONCISE_SCRIPT_LOG

note: you can also type some other DGX_ value instead by inserting "0". Anyway the point is that you omit to insert the "DGX_LOG_SCRIPT_COMMANDS" flag.

Then, in the triggergroup whose you want have the log, you'll type as first trigger:

	TGROUP_COMMAND, TCMD_LOG, ENABLED
and at end of the triggergroup (if you wish) you can disable the log:
	TGROUP_COMMAND, TCMD_LOG, DISABLED
In this way you can remove most of log messages about script commands whose you are not interested, to clean up the log.

Note: this command cann't remove the system log messages, about loading level, creation of lights ect. It affects only log messages about script commands.

## TCMD_PAUSE
Used in TriggerGroup= command.
This constant should be used as second value of three values, where the first was TGROUP_COMMAND.
Suggested only for script debugging.
With this command you can force a pause in game when this command trigger will be performed.
Only reason to use this chance is to verify (watching the screen) what's happening in game in that precise moment.
The usage in game-play (and therefore, not for debugging purpose) it's possible but a bit weird. If you wish freezing the game and wait a key from player, you have already some flipeffects to show strings.

You have two ways to perform the pause:
Setting in next argument (third value of this command trigger) the number of microseconds for the pause, and, elapsed this time, the game will come back to run, or setting a scan code (you take the value in "Keyboard scancode list" of Ng_Center's reference panel).
When you set a scan code the pause will go on forever, until the user will hit that key.
To recognize scan codes from microseconds you use the sign.
Negative numbers will be seen as microseconds to wait, while a positive value as a scan code to be hit to quit the pause.

## TCMD_SET_EXTRA_CONDITION
Used in TriggerGroup= command.
This constant should be used as second value of three values, where the first was TGROUP_COMMAND.
It changes the following (real) trigger of current TriggerGroup, replacing the Extra Timer value with that taken from trng variable set in next value (third value of current command trigger)

Range: 0/31
Scope: Used only with Conditional trigger. It is the "(E)xtra" parameter of conditions.

## TCMD_SET_EXTRA_TIMER
Used in TriggerGroup= command.
This constant should be used as second value of three values, where the first was TGROUP_COMMAND.
It changes the following (real) trigger of current TriggerGroup, replacing the (E) Extra Timer value with that taken from trng variable set in next value (third value of current command trigger).

Range: 0/127
Scope: Used with Flipeffects and Action triggers. It is the "(E)xtra" parameter.

I'll do only a pair of examples about this command, anyway same speech could work also for other TCMD_SET_... commands.

First Example: change the time to self-closing of a door, in according with number of found secrets.
In game-play this means: ok, you (player) could not give a ship (censored) about looking for secrets but... I advise you that, at end of the level you'll have more time to reach the final exit-door if you picked up many secrets.

About the triggergroup to realise this target it will be based on trigger (to modify dinamically with TGROUP_COMMAND) like this:

; Set Trigger Type - ACTION 43
; Exporting: TRIGGER(1323:0) for ACTION(175) {Tomb_NextGeneration}
; (#) : DOOR_TYPE1                 ID 175    in sector (2,2) of Room 0
; (&) : Trigger. (Moveable) Activate (#)Object with (E)Timer value
; (E) : Timer= +05
; Values to add in script command: $5000, 175, $52B

Above trigger should open the Door and then wait 5 seconds before closing it.

If we, first of above exported trigger "$5000, 175, $52B", used a command trigger like this:
	TGROUP_COMMAND, TCMD_SET_EXTRA_TIMER, #0800
We'll change the (E)timer value of following trigger, using the value stored in "Current Value" variable (its code is "#800")
Before of this two triggers we'll have to use trng variables, of course, to read the amount of secrets and then multiply it by some factor and then store this value in "Current Value" variable to modify the number of second to keep open the door.

Second Example: building a new custom elevator
Using the InputBox to receive a number (number of floor, for instance) and then multiply that value (stored in "Last Input Number" variable) and then we multiply it by some factor, then we can use the action trigger:

; Set Trigger Type - ACTION 30
; Exporting: TRIGGER(30:0) for ACTION(4) {Tomb_NextGeneration}
; (#) : DOOR_TYPE1                 ID 4      in sector (3,3) of Room0
; (&) : Move. Move up (#)animating for (E) clicks
; (E) :   1 clicks
; Values to add in script command: $5000, 4, $1E

And we'll use a bridge as animating to move.

Then, in the TriggerGroup, we'll set the TGROUP_COMMAND to change the Extra timer of above trigger, using the variable where  we saved the computation: LastInputNumber * (some factor, the number of clicks for floor).

## TCMD_SET_FULL_TIMER
Used in TriggerGroup= command.
This constant should be used as second value of three values, where the first was TGROUP_COMMAND.
It changes the following (real) trigger of current TriggerGroup, replacing the Full Timer value with that taken from trng variable set in next value (third value of curren command trigger).

Range: 0/32767
Scope: Used only with Flipeffects having NO (E)xtra parameter. It is the "Timer (Parameter (&))" parameter.

## TCMD_SET_OBJECT
Used in TriggerGroup= command.
This constant should be used as second value of three values, where the first was TGROUP_COMMAND.
It changes the following (real) trigger of current TriggerGroup, replacing the Object value with that taken from trng variable set in next value (third value of current command trigger).

Range: 0/4095 or 0/-4095 (see below note)
Scope: Used with Condition and Action triggers. It is the "(Object to trigger (#))" parameter.

Note: if the value is a moveable index, remember that there are two kinds of indices about moveables:
1) The index in tr4 file, used in run-time.
2) The NGLE index, that is the index you see in NGLE room editor.
You can set one or other kind but it's necessary you use the negative sign (-) to declare a tr4 moveable index when you used a tomb4 index. In this case the range becomes -1/-4096
While, if the value in source variable is positive, it should be a ngle a tomb (tr4) moveable index or another kind of paramater, different than moveable indices.
About this complication it's necessary reminding that the exported trigger used always ngle indices for moveables, for this reason if you have, in some trng variable, an internal tr4 index, you'll have to change its sign before passing it to some exported trigger with TCMD_SET_OBJECT command.
All above speech is futile if,  the object value to change, it's not really the index of some moveable but some other type of parameter.

## TCMD_SET_TIMER
Used in TriggerGroup= command.
This constant should be used as second value of three values, where the first was TGROUP_COMMAND.
It changes the following (real) trigger, of current TriggerGroup, replacing the Timer value with that taken from trng variable set in next value (third value of curren command trigger).

Range: 0/255
Scope: Used only with Flipeffects, it is the "Timer (Parameter (&)) parameter.

## TCMD_TIMER_FIELD
Used in TriggerGroup= command.
This constant should be used as second value of three values, where the first was TGROUP_COMMAND.
This command set in global timer variable the value (in seconds) typed in next argument (and third value of this command trigger).
Please do not confuse this command with the other script commands like "TCMD_SET_FULL_TIMER" or "TCMD_SET_EXTRA_TIMER".
There is a big difference: while both TCMD_SET_... commands will change the "timer/extra timer" argument of following trigger in triggergroup sequence, the TCMD_TIMER_FIELD command will NOT change any argument of following triggers, but it will store the "real timer value in seconds to use" for (*)all next triggers in current triggergroup.

To explain better the difference, just thinking to use an Action trigger like A43: "Trigger. (Moveable) Activate (#)Object with (E)Timer value".
Above trigger has its own (extra)timer field, and you could change it using the TCMD_SET_EXTRA_TIMER command.
Anyway the max value to store in (E)timer argument is 127 and, using signed values, the range will be: -64/+63
So, using TCMD_SET_EXTRA_TIMER command, you could set only a limited time in seconds for that trigger.
Differently, using TCMD_TIMER_FIELD command, you can pre-set for all timer usage in next triggers a (**)bigger value, like 1000 or -800.
Warning: please notice that the TCMD_TIMER_FIELD command will affect (*)ALL following trigger using a timer value (and not only that next trigger), so you could get unwished results if there are more than one trigger using a "timer value" in the triggergroup sequence.
In above situation there is a trick to disable the previous timer_field set value: just using another TCMD_TIMER_FIELD (before of trigger you want skip from timer_field value) with a 0 number of seconds and in this way the following triggers will not use the value set with (first) TCMD_TIMER_FIELD command and they will come back to use their own timer value set in the triggers.

(*) Note: really the only triggers that will be affected by this command are those action triggers used to trigger objects (like traditional tomb trigger) but with the chance to be exported (since they are trng Action trigger).
Since only action trigger that use really the timer value for activation, will use the value set with TCMD_TIMER_FIELD, the affected triggers will be only the A26, A41, A43 and A44 action triggers.

(**) Note: The values you can use as number of seconds has to be in the range: -1092 / + 1092 (about 18 minutes)

TRUE:1   ;Used as argument for TGROUP_COMMAND, TCMD_EXIT  command trigger

FALSE:0  ;Used as argument for TGROUP_COMMAND, TCMD_EXIT  command trigger

## TGROUP_AND
Set as operator for current condition the AND operator.
Note: The TGROUP_AND is currently unused from 1.2.2.7 dll version. To have an AND operator just avoiding to use TGROUP_OR and TGROUP_NOT.

This operator will be keeped also for following condition trigger if no other TGROUP_ flag for boolean operators will be used.
The AND operation is the default operation, this means that, if you don't use any TGROUP flags to set operators in TriggerGroup, all condition will be linked with AND operators.

Example of list of conditions + boolena operators:

  Condition1
  TGROUP_OR + Condition2
  TGROUP_AND + Condition3
  TGROUP_NOT + Condition4

Above list of condition triggers could be read in this way:

if (Condition1 OR Condition2) is TRUE  AND
	Condition3 is true  AND
	Condition4 is NOT true
	then ... perform following (non-conditional) trigger sequence

## TGROUP_COMMAND
Used in TriggerGroup= command.
Differently by other TGROUP_ values, the TGROUP_COMMAND value should be used alone(*) to introduce a fake trigger.
Usually in triggergroups there are data about real triggers exported by NGLE program, but in the case of TGROUP_COMMAND it used only to set a command to handle the execution of (others) triggers in current triggergroup.
Once you typed the TGROUP_COMMAND value in first value of three values, you'll set in second value a TCMD (Triggergroup CoMmandD constant) to set what kind of command it will be. While the third value will be the argument for given TCMD_ command.
See also description of TCMD_ constants for more infos.

(*) Note: The only exception, for the rule to use alone the TGROUP_COMMAND, is for TGROUP_ELSE flag, that you can add to TGROUP_COMMAND when it is first trigger of an ELSE block.
No other TGROUP_ flag can be added to TGROUP_COMMAND.
In the case you use a TGROUP_COMMAND with TCMD_SET... command to modify a condition trigger, the further TGROUP_OR / TGROUP_AND / TGROUP_NOT operators, should be always added to real (following) condition trigger, never to TGROUP_COMMAND fake trigger.

## TGROUP_ELSE
Set the start of a ELSE group in current TriggerGroup.
The ELSE is important to perform an alternative trigger when the first condition was false.
For example if you create a TriggerGroup with conditions to load a different level in according with some condition, you can create a TriggerGroup like following:

	Condition1 is true
		Perform trigger "Load level 4"
	else
		Perform trigger "Load title level"

Above conditional sentence could be use to perform a bonus level (4) only when some condition (condition1) is true, while is the condition is false (else) it will load the title level, to complete the adventure with no bonus level.

You can place one, two or more ELSE in your TriggerGroup, the rule to follow to understand how the TriggerGroup will work, is:

   If first conditions are true: perform first group of non conditional trigger immediatly following the first conditons.
   While if first conditions are false, skip all following non conditional trigger, until to reach an ELSE flag.
	If some ELSE flag has been found, the parsing will start from that condition/trigger as it was the start of whole TriggerGroup.
The trigger with ELSE flag could be also another condition, and it will be elaborated in usually method: if conditions are true perform following non conditional triggers, while if the conditions are false, skip all triggers upto another ELSE flag.

Using multiple ELSE you can perform different triggers in according with many different conditions.
For example, we could open different doors in according with last number typed in Keypad switch:

   Condition: If KeyPad Value = 1
		Trigger: Open door 12
   ELSE
	  Condition: if KeyPad Value=2
		Trigger: Open door 7
   ELSE
	  Condition: If keypad Value =3
		Trigger: open door 18

## TGROUP_NOT
Set as operator, to link current condition with previous conditions, the NOT operator.
The NOT operator could be read as "AND NOT" because requires the previous condition was TRUE and that the current condition was NOT true.
You use the NOT operator to invert a condition, this is very useful to create condition not yet presents in trigger condition list.
For example if there is in trigger window the condition "Lara collide with object <#
" but it's missing the inverse condition "Lara is NOT colliding with object <#
", you can use the condition for collide and then add a TGROUP_NOT flag to first value of that exported condition to transforme it in "Lara is NOT colliding with object"
See description of TGROUP_AND flag to have more information about the usage of boolean operators in TriggerGroup.

## TGROUP_OR
Set as operator, to link current condition with previous conditions, the OR operator.
When you use the OR this means that just the previous condition or current condition are true to get true the condition group upto now.
See description of TGROUP_AND flag to have more information about the usage of boolean operators in TriggerGroup.

## TGROUP_SINGLE_SHOT
Used in TriggerGroup command
Adding this flag in first number of first trigger of TriggerGroup you create a single-shot Trigger group.

Pratically this triggergroup will be performed only once in current level, indifferently by how much times for it, it will be required an execution with the "Perfrom TriggerGroup" flipeffect.

The single-shot trigger groups are very important to fix that limitation about the trigger zone where you place same trigger in two or more closed sectors in the map.
When you wish that all this trigger zone worked like a single-shot trigger you have a problem, because each trigger will work in one-shot way but individually and this means that lara could engages newly the same trigger when she will touches another sector in that trigger zone.
Currnetly the only way to avoid this problem is to export the trigger in script format and copy it in a TriggerGroup command. Then add to first number the TGROUP_SINGLE_SHOT flag, and at end replace in the level the original trigger with a "perform trigger group" flipeffect.
In this way the triggergroup will be performed only once, and when lara will pass over other trigger sectors of that same zone, noting will happen.

Remark: There is also a new flipeffect whereby it you can enable newly a single-shot triggergroup already performed.

## TGROUP_SINGLE_SHOT_RESUMED
Used in TriggerGroup command
This flag works alike the TGROUP_SINGLE_SHOT flag (see its description) but in this case it's not necessary use the flipeffect 345 to enable it newly.
In fact, a triggergroup with the TGROUP_SINGLE_SHOT_RESUMED in (first) flags word, will be enabled newly not just the current triggergroup stops to be performed. This means that the "single shot" atttribute will work only in contiguos triggered sectors, but not just lara goes out from this trigger zone, the trigger will be newly ready for further performings.

## TGROUP_USE_EXECUTOR_ITEM_INDEX
Used in TriggerGroup command.
Adding this flag to first of triplex parameter of exported trigger data, you can replace the index on that perform some action, with the item that has excutes the trigger that enables this triggergroup.
You could use this flag when you create an heavy trigger to perform this triggergroup and you want that all action trigger will be redirect to moveable that has just enabled the trigger group.
For example if you export an action trigger to kill (any) moveable, and then you perorm this triggergroup with an heavy trigger, when any moveable move on it, the action (to kill) will be applied on this moveable.
Pratically with this method you can create trigger to kill enemies (or other action on them) where it will be the same moveable to enable it.
Remark: to realize this trick is problem you need to use also an Enemy= command with the NEF_EASY_HEAVY_ENABLING to permit to many different moveables to enable all heavy triggers they meet in their path.

## TGROUP_USE_FOUND_ITEM_INDEX
Used with to force to use the index of last moveable detected by some condition.
The found index will be used in all following triggers or conditions of TriggerGroup that require an index for moveable.
Pratically using this flag, the index found from some condition will become the index to use for following condition and triggers, ignoring further moveable index of thoose triggers.
This overriding of original indices will be stopped only when it will be found another TGROUP_ flags used to change the default index of moveable to use.

Other TGROUP_ flags to change the default index for moveable are:
TGROUP_USE_OWNER_ANIM_ITEM_INDEX
TGROUP_USE_TRIGGER_ITEM_INDEX
See descriptions of above flags.
Remark. Not all condition trigger may be used to set a moveable index, of course.
You can add the flag TGROUP_USE_FOUND_ITEM_INDEX to (first value) of exported condition, only on those condition try to verify the presence of some moveable.
For example the condition trigger "Lara collides with moveable with [&] slot"  or the ENV condition "Lara has in front the object with slot = ExtraSlot"
This method to force another index to use with following triggers it's necessary to handle situation where you are not able to know exactly WHAT moveable lara will collide or she'll have in front of her.
For example if we could build a TriggerGroup to handle a fighting like following:

Conditions: "When right hand of lara touch head of [&] slot moveable"
   then: (above condition is true)  Toggle vitalty to current enemy moveable

In above trigger group, when we build our condition trigger we can set the slot of moveable for condition, for example we could use the slot "95 SAS", but when in game lara hits a SAS moveable it could be one of many SAS preset in the level, and we cann't know its index while we build the condition trigger.
If we don't know the index, we cann't set in following trigger to toggle vitality.

To solve above problems, there are some TGROUP_ flags used to get the index of moveable to use in dynamic way: when engine with some condition detects a moveable, it could use that index for following trigger.

## TGROUP_USE_ITEM_USED_BY_LARA_INDEX
Used in TriggerGroup command
Using this flag you ovverride the original item index of the exported action trigger, with the index of last item that lara used (or she is using) in interactive mode.

For example the index could be one of following moveables:
Vehicles, rope, polerope, switch 1/2/3, pushable objects, rollingball (with pusing features), parallel bar, elevator, keypad.

Remark: for technical reasons this index remains valid also when lara stopped to use it. For example when lara drives the jeep, the index of jeep becomes the item_used_by_lara and when lara goes off from jeep it remains valid until lara doesnt' use another interactive item, like switch, elevator, parallel bar ect.
For above reason you should use this exported script ONLY calling it from an animcommand in a specific lara animation that interactes with that specific item.

Example:

You wish add a fire to parallel bar only when lara is using it.
To perform above target you should:

1) Type in script text an AddEffect with data compatible with the moveable parallel bar
2) Export in script mode an action trigger to enable the above addeffect. Note that in this trigger it's not important the moveable you use for it since it will be override from our TGROUP_USE_ITEM_USED_BY_LARA_INDEX flag.
3) Type the data of exported action trigger in the script and add to its first value the TGROUP_USE_ITEM_USED_BY_LARA_INDEX flag
4) Now export, as animcommand, a trigger to perform # trigger group, using the id of our triggergroup to add the fire effect
5) Type the data of this animcommand in some frame of some lara animation used to roll around the parallel bar.

In this way when lara will begin to use a parallel bar, that bar (and only that) it will be fired.
The you could use another action trigger to finish the fire effect with same method just showed but in this case we'll insert the animcommand in a frame of an animation where lara is going off from the parallel bar.

With this method you can add effect or perform action triggers on all objects used  by lara with interesting effects.

## TGROUP_USE_OWNER_ANIM_ITEM_INDEX
Set as moveable index to use in following (and current) triggers the index of moveable own the anim command just performed.
This flag could be used only when current TriggerGroup command has been started by an animcommand. In this situation, if you use this flag the index of moveable to use it will be the index of moveable owns the animation that contains the anim command started current TriggerGroup.
See also description of TGROUP_USE_FOUND_ITEM_INDEX flag to have more informations about the mechanism of changing of moveable index

## TGROUP_USE_TRIGGER_ITEM_INDEX
Set newly as index of moveable to use for following (and current) triggers, the real source index set in exported trigger.
This flag requires to be used only when you had changed the default index in previous triggers or conditions. Differently, if you have never used any TGROUP flag to modify the index to use, it's not necessary use this flag, because by default the index of moveabl is that set in data of exported trigger.
See also description of TGROUP_USE_FOUND_ITEM_INDEX flag to have more informations about the mechanism of changing of moveable index

## TPOS_DOUBLE_HORIENT
Used in TestPosition command
If you want that the correct position to detect could be in front of object (with given H Orient) but also in the opposite side, you can set this flag.
When in TestPosition there is the TPOS_DOUBLE_HORIENT flag, the testposition condition will be true also if lara is in opposite side for what you set the ranges of testposition.
For example using an item like a gate, you can use TPOS_DOUBLE_HORIENT to permit to lara to be in correct position for both side of the gate.

Remark: this flag works fine only if the item has its pivot in same position of visible mesh.
Unfortunately, in many circustances the pivot (origin x,y,z of object) is not at center of its mesh but in a corner of sector where it will be placed.

## TPOS_FAST_ALIGNMENT
Used in TestPosition command
In the reality this flag affects the (furhter) alignemnt phase when you used the FAN_ALIGN_TO_ENV_POS constant in the Animation command that called this TestPosition command.
When you use an Animation command and you used also the FAN_ALIGN_TO_ENV_POS, when Lara is in a good position for TestPosition command, she will be moved in perfect ideal position before performing the animation.
Well, in some circustances the old method used to move lara byself in ideal position had a problem of continuos loop when she was too closed to target position to reach.
To solve this problem you could use in TestPosition command the TPOS_FAST_ALIGNMENT flag, and in this case Lara will be moved very fastly in correct position avoiding the risk of looping.
Remark: you should use the fast alignment only when you set a very low tollerance in TestPosition command, i.e. when the condition of testpostion will be true when lara is really very closed to ideal position.

## TPOS_FOUR_HORIENT
Used in TestPosition command
With this flag the correct position will be each position for each side of object.
The example could be a cube like a squared pushable object, where lara is able to interact with every side of the pushable.
Remark: To work fine, it's necessary that the object had its pivot (origin x,y,z of object) exactly in central position of object mesh.

## TPOS_OPPOSITE_FACING
Used in TestPosition command.
The alignment with some objects could fail because the main mesh has an opposite facing respect to animation 0 displacement.
You can discover this situation with Animation Editor of Wad Merger: when the position of mesh with "No Animation" is the opposite of mesh in "Animation 0" you are in this situation and the self alignment could fail.
To fix this bug you can use the TPOS_OPPOSITE_FACING flag in the TestPosition command used by your Animation command.

## TPOS_ROUND_HORIENT
Used in TestPosition command.
This flag works ignoring the facing of the object but it checks only the facing's Lara.
To understand the situation you have to think about alignment of lara with pole-rope. In this case it's not important the facing of the pole-rope but only that lara is looking to its direction(and the distance).

Since the round facing is very particular there are some special rules and limitations:

* The couple (XDistanceMin / XDistanceMax) and (ZDistanceMin / ZDistanceMax) should have the same values, anyway trng engine will read only the pair (ZDistanceMin / ZDistanceMax).

* The range (HOrientDiffMin / HOrientDiffMax) is the only orienting pair to be verified.

* If you use this flag in a  TestPosition command with an Animation command having the FAN_ALIGN_TO_ENV_POS flag, the position of Lara will be moved in ideal position but only in immediate way (no sliding or smoothed movement).
For this reason it's better using limited ranges about max / mix distance and DiffMin / DiffMax horizontal orient, to avoid a too jerk movement in game.

## TPOS_SELF_FIXING
Used in TestPosition command.
The self fixing works to recognize a problematic situation and it tries to fix it itself.
Technically, the problematic situation is when the difference between HOrient of Lara and that of Item is about $8000.
The problem is that, in short values (like tomb4 manages the horient values) the $7fff value is a great positive number (+32767), while the $8000 value is a very little (and negative) number (-32768).
This situation creates the paradox that, the "hOrientDifMin" value, you set in script command, it will be higher than "hOrientDifMax". This is bad, and tomb4 procedure return false.
The self-fixing recognize itself, this situation and tries to fix it.

## TPOS_TEST_ITEM_INDEX
Used in TestPosition command
Adding this value in Flags of TestPosition you change the mean of "Slot Moveable" field.
When you use TPOS_TEST_ITEM_INDEX flag, trng engine will consider the value in "Slot Moveable" field like an index of item to check.
You find the index in NGLE program, clicking on wished item, you'll see the index in yellow frame.
The advantage to use an index is to can test a moveable also if it is extern a lara room, while in other case (checking for slot) the testposition detects only items in same room where lara is.
By other hand, the disavantage to use this flag is that, in this way, you cann't detect all object in general way but you have to specify only one item index.

## TPOS_TURN_FACING_180
Used in TestPosition command.
This flag fix a problem with some items. In some circustance an item has a wrong facing, turned by 180 degrees, and you discover this situation when, enabling self-alignment feature, you see that lara moves in wrong position.
Only way to discover if this flag fix the probelm is trying to use it and seeing the final moving of lara to be aligned.
Note: this flag, differently by TPOS_TURN_FACING_90 flag, will affect ONLY the aligment phase, while it will be ignored in testposition phase.

## TPOS_TURN_FACING_90
Used in TestPosition command.
This flag fix a problem with some items. In some circustance an item has a wrong facing, turned by 90 degrees, and you discover this situation when, enabling self-alignment feature, you see that lara moves in wrong position.
Only way to discover if this flag fix the probelm is trying to use it and seeing the final moving of lara to be aligned.

## TRB_ADAPTIVE_FARVIEW
Used in Turbo command. TRB_ADAPTIVE_FARVIEW enable a complex compute to adapt the max far view (really the level far view) in current level in according with current performances.
Pratically, when TRNG discovers that it is not able to keep the medium value of 30 FPS (Frame per second), it will reduce progressivly the level far view until it get newly the 30 fps (really you can change the value of frame rate to keep typying a value in FPStoKeep field). When it reaches optimal frame rate and keep this good result for three second, it will try to increase level far view very slowly until to reach the value for levelfarview you set in script.dat file.
This method allows to have a variable distance to show far objects to preserve always good frame rate performances.
Remark: the TRB_ADAPTIVE_FARVIEW setting could not work fine togheter with TRB_ASYNC_FRAMES setting, because this last setting creates an irregular response in internal frame rate value and, since TRB_ADAPTIVE_FARVIEW uses own the frame rate value to perform the changes in far view, it could be confused by TRB_ASYNC_FRAMES
Remark2: If you mean use TRB_ADAPTIVE_FARVIEW it's advisable don't exagerate about setting of LevelFarView in this level.
Many level builders set LeverFarView=127 only because 127 is the max value but this number is uselessly huge, since it's very seldom you have more than 60 sector visible in direct line to watch. By other hand, setting 127 the adaptive job will be slacked because when it'll happen a bad frame rate the engine will spend much time to decrease far view from 127 sector to really meaningful value like 20 or 30 sectors. Same speech for inverse operation, when the far view has been reduced drammatically, not just the frame rate will go on newly the engine will spend many time to increase newly the far view until 127 sector.
For this reason I suggest as ideal values for adaptive farview to set LevelFarView = (about) 60 while the required fps to keep set (about) to 24 fps.

## TRB_ASYNC_FRAMES
Used in Turbo command.
From 1.2.2.7 version, this setting works in a different way respect the past.
Now when the game loses some frame, the turbo command perform two or more frames in a shorter time to recover the previously lost frame. The result in this situation is that lara will cover same distance in same time as in full frame rate  but the movements will be not always fluide. In spite this solution is not perfect in some situation is better than the big slowdown you can see when the engine is not able to support the standard 30 frames per second.

## TRB_HIGH_PRIORITY
Used in Turbo command. TRB_HIGH_PRIORITY set an higher class priority for whole tomb4 process, to increase the time supplied to tomb4 respect to other processes. Theorically this setting could advantage the game engine when in the PC there are other programs are working, or some background services like antivirus. If in your PC there are no other big process this setting will have no visible effect.

## TRB_OPTIMIZE_SORTING
Used in Turbo command. TRB_OPTIMIZE_SORTING optimizes the sorting of polygons list used by tomb4 engine before performing many draw operations.
Usually, this optimizing gives a gain of about 5 or 6 fps (frame per second)

## TRB_SELECTIVE_VIEW
Used in Turbo command. TRB_SELECTIVE_VIEW excludes all computes about static objects non visible in game (because back of current camera view).
Theorically just the directx functions perform this cutoff, but looking the source code, it appears that many computes have been performed in spite these statics object are outside of visible world. Hence, this setting allows to save some time otherwise wasted in no useful computes.

## TSB_MATRIX
Used in StandBy command.
The matrix effect create a camera turning around to Lara.

## TSB_NO_CHANGE_CAMERA
Used in StandBy command.
This standby type disable any change for look camera. You should use this type only when you use your triggergroup to customize the standby mode.

## TSB_PANORAMA
Used in StandBy command.
The panorma type works like a matrix type but where the distance, the vertical angle, and the speed of rotation will continue to change in random way.
With Panorama you can create big ranges for distance while with Matrix type the max difference between min and max limits are only +/- 50% of given distance.

In Panorma type some settings on StandBy command will be interpreted in particular way:

The further settings for FSB_FLIP_DISTANCE, FSB_FLIP_SPEED and FSB_FLIP_V_ANGLE flags will be ignored, since Panorama type flip always the distance and vertical angle.
The value in "Distance" field will be used like Max Distance, while the min distance will be always 400 (very closed to lara).

The value in "VAngle" field will be used like Max Vertical Angle allowed. Remember that negative numbers in VAngle mean "the camera is upper and look the lara that she is under it".
When you set a big max distance you should avoid to allow big positive values for vangle because the risk is that the camera tried to go underground. The only exception is when lara is on the top of a pyramid, where around to her the ground is lower than her position.

## TSB_PORTRAIT
Used in StandBy command.
In this standby type, the camera reamins in front of lara.
In this mode you could use a short distance value to look the beauty Lara in the eyes.

## TSCR_LOAD_GAME
Used with GT_TITLE_SCREEN global trigger.
Detect the [Load Game] screen.

## TSCR_MAIN_TITLES
Used with GT_TITLE_SCREEN global trigger.
Detect the main title screen, i.e. the screen where you see "New Game", "Load Game", "Options" and "Exit"

## TSCR_NEW_GAME
Used with GT_TITLE_SCREEN global trigger.
Detect the [New Game] screen, where you can select a specific level name.

## TSCR_OPTIONS
Used with GT_TITLE_SCREEN global trigger.
Detect the [Options] screen

## TS_AFTER_FALLING_AS_INVULNERABLE
Used with Customize=CUST_SFX command.
When Lara is invulnerable and she falled from big heights, she will not die but she touches the floor and comes back to stand up position after a short animation.

## TS_ANIMATING_DOOR_CLOSE
Used with Customize=CUST_SFX command.
When you use an animating (fake) door for the elevator, this is the sound when it will be closed.

## TS_ANIMATING_DOOR_OPEN
Used with Customize=CUST_SFX command.
When you use an animating (fake) door for the elevator, this is the sound when it will be opened.

## TS_BINOCULAR_LIGHT
Used with Customize=CUST_SFX command
This is a single-shot sound played when the player switch on or off the illuminator for binoculars.
Default value is 369 LARA_CLICK_SWITCH

## TS_BINOCULAR_ZOOM
Used with Customize=CUST_SFX command
This is the sound when customized binocular is zooming or unzooming.
Default value is 309 MAPPER_MOVE

## TS_DAMAGE_ROOM_BEEP_ALERT
Used with Customize=CUST_SFX command.
When in a damage room the time (bar) is almost terminated the beep sound for blinking bar of ending time.

## TS_DAMAGE_ROOM_SCREAM_BURNING
Used with Customize=CUST_SFX command.
When in a damage room the bar is empty lara will burn and this is the sound, usually a scream.

## TS_DETECTOR_SHOW
Used with Customize=CUST_SFX command.
When the detector will be showed on the screen for first time.

## TS_DIARY_CHANGE_PAGE
Used with Customize=CUST_SFX command.
This sound will be played when the user changes the page of diary with next or previous page.

## TS_DIARY_NO_PAGE
Used with Customize=CUST_SFX command.
This sound will be played when the user tries to change page of the Diary but there is no more pages in that direction (next or previous)

## TS_DIARY_ZOOM_START
Used with Customize=CUST_SFX command.
When you enabled the zoom effect at first activation of diary this is the sound that will be played.

## TS_ELEVATOR_SQUASHED_LARA
Used with Customize=CUST_SFX command.
When elevator squashed lara and kill her.

## TS_MISSING_REQUIRED_ITEM
Used with Customize=CUST_SFX command.
When the trigger "Inventory-Item. Pop up inventory screen to select the <&
Item" has been performed and the required item is missing, this sound will be played (by default it was lara say "no").

## TS_MIST_EMITTER_WITH_OCB
Used with Customize=CUST_SFX command.
This is the sound of mist emitter but only when you customized it with a ocb value different than 0.

## TS_PUSHED_ITEM_IMPACT
Used with Customize=CUST_SFX command
This is the sound that will be played when an item has been pushed away from the bike and it hits the wall or another object.
The default value is 72 GENERIC_HEAVY_THUD

## TS_SAVEGAME_PANEL_SELECTED
Used with Customize=CUST_SFX command.
When the user change the selected savegame in a customized savegame panel.

## TS_SCREENSHOT_CAPTURE
Used with Customize=CUST_SFX command.
The sound when you hit F3 key to capture the game screen image.

## TS_SHOT_HARPOON_UW
Used with Customize=CUST_SFX command
This is a single-shot sound that it will be played when you customized crossbow as harpoon-gun.
The sound will work only when lara is underwater and she show an harpoon.
The preset value for this sound is 68 PENDULUM_BLADES

## TS_VIBRATE_RESUME_FROM_FROZEN
Used with Customize=CUST_SFX command.
This is the short sound, repeated many times, when an enemy, previously frozen, is beeing to resume.

## TS_WHIRLPOOL_SINKED_LARA
Used with Customize=CUST_SFX command.
The sound when Lara and the boat will be sinked in the whirlpool.

## TT_ACTION_INVENTORY_MENU_OFF
combine examine separate ect

## TT_ACTION_INVENTORY_MENU_ON
combine examine separate ect

## TT_AMMO
text for ammo quantity

## TT_CAMERA_VIEW
Text showed when user hits F1 key to see LoadCamera values in game

## TT_CREDITS
Credit texts showed at end of adventure

## TT_EXAMINE1_BOTTOM
bottom side of text for Examine1 item

## TT_EXAMINE1_TOP
Top side of text for Examine1 item

## TT_EXAMINE3
Text of Examine3 item

## TT_ITEM_NAME
name of item selected in inventory

## TT_LEGEND
Text of Legend at start of level

## TT_LEVEL_NAME_OFF
Name of level in New Game screen

## TT_LEVEL_NAME_ON
Name of level in New Game screen

## TT_MAIN_MENU_OFF
Text of main menu unselected, New Game/ Load Game/ Options / Exit

## TT_MAIN_MENU_ON
Text for main menu

## TT_NEW_LEVEL_ARROWS
Scroll up/down arrows for new game screen

## TT_OPTION_DESCRIPTIONS
Text used to describe the option, like "Control Method"

## TT_OPTION_VALUES
Text of possible value for options, like "Joystick/Keboard"

## TT_PAUSED_MENU_ITEMS
Text for paused menu, "Statistics", "Options", "Exit to Title"

## TT_PAUSED_MENU_TITLE
Title of "Paused" menu. This text is the "Paused" word

## TT_SAVEGAME_DESCRIPTION_OFF
Text for descriptive line of savegame "012 Coastal Ruins 3 day 22:13" when non active

## TT_SAVEGAME_DESCRIPTION_ON
current selected savegame description

## TT_SAVEGAME_PANEL_TITLE
Title for savegame panel. "Load Game" or "Save Game"

## TT_SCREEN_TIMER
Text of screen timer like in Angkor wat level

## TT_SELECT_LEVEL
Title "Select Level" of new game screen

## TT_STATISTICS_DESCRIPTIONS
Text used to describe the statistics, like "Distance Travelled"

## TT_STATISTICS_VALUES
Text of statistic values, like "43m"

## WEAP_DISABLE_AUTO_SHOT
Disable the repeat function of shooting. By default when the player keep down the action key the weapon will continue to shoot, while, using this flag, the weapon will shot once and the player will have to release the action key if he want shot newly.

## WEAP_DISABLE_CHANGE_WEAPON
By default, the game engine will change the current weapon with pistols when the current weaponst runs out the ammo. If you add this flag the change of weapon will be disabled and lara will keep same weapon also when the ammo has been run out.

## WFF_BOLD
Used in WindowsFont= command.
A bold character has a big "heavy" layout, pratically the depth of drawn is bigger than normal or light character.
If the background, where the text will be showed, is not solid tint, it's better use a bold or ultra_bold character otherwire in some combination of text color - background color the border could be not well visible.

## WFF_CENTER_ALIGN
Used in WindowsFont= command.
The will be centerd at half of text frame.
This align mode is suggested for titles.

## WFF_FORCE_FIXED_PITCH
Used in WindowsFont= command.
This flag force trng to choose a font with fixed pitch charaters.
Theorically it should be sufficent to choose a font name of fixed pitch like "courier" but in the reality, performing some test, I discovered that sometimes windows font procedure prefers choose a different face name font only to reach other parameters like size of font.
To avoid this doubt you can add the WFF_FORCE_FIXED_PITCH flag and the characters will be surely with fixed pitch.
The fixed pitch (all characers will have always same width) is very useful when you want create text tables like in savegame panel.

## WFF_FROM_RIGHT_TO_LEFT
Used in WindowsFont= command
The text will be displayed from right to left.
Example:
the string:

"ABCDEF"

will be showed in this way:

"FEDCBA"

Note: this flag works only with hebrew and arabic computers and you have also to set the correct charset value in the WindowsFont.

## WFF_ITALIC
Used in WindowsFont= command.
The italic attribute creates a character a bit inclined.

## WFF_LEFT_ALIGN
Used in WindowsFont= command.
The text will be aligned on left margin of text frame.
Remark: this is also the default value and it will be used also if you type IGNORE in WindowFontFlags field

## WFF_LIGHT
Used in WindowsFont= command.
This setting generates very slim characters. You should use this setting only when background is a solid tint, otherwise the border of characters will be few visible on multicolor background.
Remark: if you don't set : light, bold or ultra_bold, the character will have a normal depth, really a bit slim, too.

## WFF_RIGHT_ALIGN
Used in WindowsFont= command.
The text will be aligned on right margin of text frame.

## WFF_ROTATE_90
Used in WindowsFont= command.
The text will be turned by 90 degrees in clockwise mode.
Note: there are some limitations for this flag.

- It will work only with single-row string. This means that the further new line characters ("\n") will be ignored.
You should print a single row at once. If you wish print many rows you'll have to print different single row one by one, changing the origin of rectangle in correct way, everytime.

- You have to use a true type font, otherwise the rotation will not work.

- In the rectangle data of Parameters=PARAM_WTEXT command it will be used the (left,top) to set the orgin, and the difference between left and right to compute the length of the text (in vertical since it has been turned) with the computation (right - left = length of vertical text)

## WFF_ROTATE_INV_90
Used in WindowsFont= command
The text will turned by 90 degress in inverted clockwise mode.

Using this flag there are some limitation and technical requirements. See the description of WFF_ROTATE_90 flag for more infos

## WFF_SHADOW
Used in WindowsFont= command.
If you add this flag in WindowsFontFlags field, each character will have a sort of shadow.
Technically this effect requires to print two times the same text, moving by a bit of pixels the position, and using two different colors.
By default the shadow will be black, anyway you can change the color for shadow using the ShadowColorRgbId field.
The shadow has two targets: create a simil-border to get more visible the text also over multicolor backgrounds, and to realize a 3d effect, where the effective text could seem it was upstairs a wall where the shadow of this text has been casted.
Remark: this effect works fine with bold or ultra-bold characters, while less well with little/light caracters.

## WFF_ULTRA_BOLD
Used in WindowsFont= command.
This is the max heavy for character: the depth of drawn will be the max available. If you want have "fat" characters this is the ideal setting but it works fine always when the size set for this font is big, differently, using a little font an ultra bold could cause a very leak text.

## WFF_UNDERLINE
Used in WindowsFont= command.
Adding the WFF_UNDERLINE flag, all text will be underlined.
This setting could be useful for text used as title.

## WFF_UNICODE
Used in WindowsFont= command
If you wish show a text with eastern charset, perhaps you'll need also to set this flag to show correctly your string.
Unicode is UTF16, a fixed 2 bytes (16 bits), encoding method.
In the case you use unicode you'll have to type your string, using the "binary hex string" format.
This is a necessary "trick" to around the problem that NG_Center program doesn't support unicode texts.
To add an unicode string in string panel of NG_Center, you'll have to:

1) Open block notes or other (simple) text editor of your computer
2) Type the original text in your language, included in two "$$$$" markers.
Example:
If your text is "MyText"
you'll type into block notes:

$$$$MyText$$$$

3) Save this text in a file on disk
4) Go to the String panel of NG_Cnter, and enable [ExtraNG] section.
5) Click to [Add new string]
6) Now click on [Import Binary String] button
7) Choose the file where you saved your original text
8) Now you can choose if you wish keep this text in an external file ("@FileName.txt") or you want keep the binary string directly in hex format.

## WFF_UTF8
Used in WindowsFont= command
If you wish print text with eastern fonts, probably you'll have to add this flag to inform trng that your text is encoded with multibytes format.
When you chose the WFF_UTF8 flag, trng engine will manage the text you wish print, as it was an UTF8 text, and this means that tnrg will convert in (at fly) in unicode before printing it.
For this reason when you add the WFF_UTF8 flag, it's futile add (or less) the WFF_UNICODE flag, since this operation will be performed by trng itself.
About the choice between UTF8 and UNICODE, you should try to see if NG_Center with the correct charset is able to support your language.
To set a charset for NG_Center, different than default ANSI/western charset, you should use the translation file.
If you have not a translation file for ng_Center, you can create a fake translation file in this way:

1) Open block notes, and type this text with your LanguageName and CharsetNumber:

[START_CONFIGURATION]
Version=LanguageName
Charset=CharSetNumber
PropFont=Arial
FixedFont=Courier
[END]

note: In above text, you have to replace the "[" character with the "less than" character, and the "]" character, with the "greater than" character. I cann't use them in this text for a problem of internal format of this help file.

2) Now save this text in NG_Center folder, giving the name "my_scripter_constants.txt"

3) Now, close NG_Center, and launch it newly.

4) Now in Settings panel you should see your language (you typed in Version= command).
If it's not selected, select it now

5) Now ng_center will support the charset you typed (CharSetNumber) and the wished font.
You have to try if you are able to type text in [Strings] section, in your language, and if this texts, after saving, quit program and launch newly program, they stand yet there.

In the case your language is not yet supported by NG_Center you'll have to use binary strings.
A binary string is sequence of hex values with the codes used to print a text.
Read the description of WFF_UNICODE flag, to understand how to get a binary string from your original text.

About the choice between UTF8 and UNICODE (it should be UTF16) it depends by your language.
Languages like chinese, corean and japanese work with UNICODE, while other middle-east languages, like russian, turkish or greek, could work with UTF8.
Anyway there are also simplified versions of chinese and japanese that may be supported by UTF8.
Probably you'll have to do some experiment to discover the best settings for your language.

## WTF_CHANGE_COLOR
Used with Parameters=PARAM_WTEXT script command
The color of text will change between foreground color and color of shadow mask slowly, giving the look of a blinking text.

## WTF_FLYING_TEXT
Used with Parameters=PARAM_WTEXT script command

This flag generates a flying/zoom effect, where the text at begin is very little, and it will be magnified fastly, until to reach the bigger size you set.
The final position of the text will be given by the rectangle (left, top, right, bottom fields) you set in PARAM_WTEXT parameters script command.

If you wish have a text at center of the screen that it will fit the whole screen and end of the flying you can set 0,0,1000,1000 as values for rectangle.

Notes:

* The time (tick frames, 30th of seconds) you set in TimeDurate field of PARAM_WTEXT command, will be used to set the time that the text will remain on the screen after it reached the final bigger size. If you type IGNORE or 0 in TimeDurate the text will remain forever and you'll have to use the F364 trigger to remove the text.
The lower time you can set in TimeDurate field for this effect is 2 (1/15th of second), while if you set 1 the effect will fail.

* It's advisable using always the WFF_CENTER_ALIGN flag in the WindowsFont command, for this effect type.

* This effect works better with single row texts, i.e. text where there is no new line character. If you use new line characters in the text, this effect will work in a different way: the text will be not vertically centered at begin of its flying but it will appear in top side of screen, like it was coming from the sky.

* You cann't use same PARAM_WTEXT command for two (or more) flying effects (also if they are using different texts) in same moment.

Note: the Size you type in the WindowsFont command linked with current PARAM_WTEXT command, will be used as ending (and bigger) size, while the beginning size is set by default to be so little to look like a little far row.

## WTF_OVER_BINOCULAR
Used with Parameters=PARAM_WTEXT script command
Keep the text over binocular view

## WTF_OVER_FIXCAMERA
Used with Parameters=PARAM_WTEXT script command
Keep the text also when there is a fixed camera

## WTF_OVER_FLYCAMERA
Used with Parameters=PARAM_WTEXT script command
Keep the text also when there is a flyby camera

## WTF_OVER_IMAGE
Used with Parameters=PARAM_WTEXT script command
This flag allows to your windows text to stand over furhter full screen image that is currently drawn on the screen.
By default, omitting this flag, the text will be disabled when a full screen image has been showed on screen.

## WTF_OVER_INVENTORY
Used with Parameters=PARAM_WTEXT script command
Set that, this text, will be showed over futher inventory and pause screen.
If you omit this flag, when the inventory pops up, the text will be removed and then it will be showed newly when the inventory screen quits.

## WTF_OVER_LASER_SIGHT
Used with Parameters=PARAM_WTEXT script command
Keep the text over laser sight view

## WTF_PULSING_TEXT
Used with Parameters=PARAM_WTEXT script command
This flag generates a text where its size will change from max size to min size, giving the idea of a pulsing heart.
Note: the Size you type in the WindowsFont command linked with current PARAM_WTEXT command, will be used as littlest size while the bigger size will be the font size current + 30 %.

It's advisable using always the WFF_CENTER_ALIGN flag in the WindowsFont command, for this effect type.

