## #DEFINE
Syntax: #DEFINE  CONSTANT  VALUE
Scope: Any

The define directive creates a mporary mnemonic constant with the wished value.

The main usage of #define directive is to get easily the remapping of a group of script commands.
Since some commands require to have an univocal ID, when you insert in your script a block of commands there is the risk to have conflicts with the IDs.
To solve these conflicts in fast way you can build your script using a mnemonic constant defined by #define directive to use as base of the IDs of your commands.

For example:

TriggerGroup=	4, $8000, 112, $2C
TriggerGroup=	5, $2000, 271, $52
TriggerGroup=	6, $2000, 235, $70,  $2000, 232, $52
GlobalTrigger=	2, IGNORE, GT_CONDITION_GROUP, IGNORE, 4, 5, IGNORE
GlobalTrigger=	3, IGNORE, GT_TRNG_L_TIMER_EQUALS, 45, IGNORE, 6, INGORE

Above code shows the links between different commands using their IDs

The GlobalTrigger 2, "calls" the triggergroups with indices 4 and 5, while the globaltrigger 3 calls the triggergroup 6.

When the commands are more it becomes also more slow find all ids to change to move the id range with higher numbers to avoid conflicts preserving their links.
But using the #define directive we can change above code in this way:

#DEFINE BS_TG 4  ;the first TriggerGroup index
#DEFINE BS_GT 2  ;the first GlobalTrigger index

TriggerGroup=	BS_TG, $8000, 112, $2C
TriggerGroup=	BS_TG+1, $2000, 271, $52
TriggerGroup=	BS_TG+2, $2000, 235, $70,  $2000, 232, $52
GlobalTrigger=	BS_GT, IGNORE, GT_CONDITION_GROUP, IGNORE, BS_TG, BS_TG+1, IGNORE
GlobalTrigger=	BS_GT+1, IGNORE, GT_TRNG_L_TIMER_EQUALS, 45, IGNORE, BS_TG+2, IGNORE

In this case the final compiled code will be exactly the same, but the advantage of second version is that when you find a conflict and you have to change all triggergroup and globaltrigger indices, preserving the internal links between them, just you change only the two directive:

#DEFINE BS_TG 10
#DEFINE BS_GT 8

And all IDs will be moved higher keeping the correct inner links.

These problems of IDs remapping are very usual when the level builders exchange blocks of commands to explain how create some features.
So, your code sample will be easily adapted to any script if you take the habit to use always the #define directive to set all ID numbers

Remark: see also the #FIRST_ID directive to complete in easy way above target.

----------------

Other usages of #define directive are less useful, anyway some idea could be:
to use a define to use a single word for a group of official mnemonic constants you use very often:

#define DEBUG DGX_LOG_SCRIPT_COMMANDS+DGX_COMMON_VARIABLES+DGX_LARA
and
#define USUAL DGX_LARA+DGX_SFX_SOUNDS
+DGX_CHEATS

In this way you can change easily the Diagnostic command typing:

Diagnostic= DEBUG, 0
or
Diagnostic= USUAL, 0

-------

Some notes about the #define directive:

- The #define directive is local and it works only inside the current source. If you use include files, and therefore different sources, any directive will work  only in the source where it is. The advantage to divide the script in different sources is that in this way you can use the same directive in any source with different values.

- You cann't use two times the same CONSTANT name in same source.
Example:

#DEFINE MYNAME 12
#DEFINE MYNAME FT_BOTTOM_CENTER+FT_SIZE_ATOMIC_CHAR

- You can use in the formula (to get the VALUE) the CONSTANT name set in another define directive of same source but these directives have to be placed previous of the define that will use them:

This code is correct:

#DEFINE BS_ALL  10
#DEFINE BS_TG BS_ALL+30
#DEFINE BS_GT BS_TG+15

while this is wrong:

#DEFINE BS_ALL  10
#DEFINE BS_GT BS_TG+15
#DEFINE BS_TG BS_ALL+30

Because the value of BS_TG is not yet defined when the #DEFINE BS_GT directive will be parsed.

- You can not let blanks (spaces) between operands of the VALUE field.
This is wrong:

#DEFINE MYSUM ALFA +BETA

this is correct:

#DEFINE MYSUM ALFA+BETA

DEFINE Used to declare plugin ID in TriggerGroup command
--------------------------------------------------------
From 1.2.2.8 version, #define could be used in according with exporting of triggers and plugin management.
When an exported (from NGLE) trigger was possessed by some plugin, there is the problem to discover what is the plugin that handles that exported trigger.
NGLE when it exports a trigger for script (from Set Trigger Type window), it adds to the trigger values the Id of plugin that owns that trigger.
The problem is that the Id is only a number but how can NG_Center to know what is the plugin_name.dll that corresponds to that Plugin Id?
A first solution is given by NGLE program, that when it is exporting the trigger will read the script.txt file to discover, from Plugin= script command, what is, in ng_center the plugin ID for that plugin_name. Once it discovered the right pluginId (in ng_center) it will convert the plugin id (to insert in exported trigger) to that ng_center value.
In this way, when you'll add that exported trigger in some triggergroup command, it will work fine, since when NG_Center will read the plugin ID in the trigger, it will be able to associate it with the right plugin name since it has that information in Plugin= script command.
The problem borns when a trigger will be used in a different script.
In this situation the new script has no way to know what is the plugin_name corresponding to that plugin Id.
To help ng_center to discover the link between pluginID and pluginName you can use a #define directive.
In this situation is very important the position of this #define, since it should be declared first of triggergroup where there is the given plugin ID.

For instance:

#define @Plugin_Train  3

TriggerGroup= .......

In above script, ng_center will parse the code, it will find the link: Plugin_Train has plugin ID = 3, and then, when parsing following triggergroup, it will find a trigger with plugin ID = 3, it will know that id = 3 means "plugin_train".

It's not necessary that the #define was own in previous row of the triggergroup. It could be also 2, 3 or 20 rows first of it, anyway it's important rememeber to close that define to avoid that, the define was used for triggergroup where that linkage is not valid.
To close the previous define we'll use another #define but with the special value "CLEAR"
So, above example it will become:

#define @Plugin_Train  3

TriggerGroup= .......

#define @plugins CLEAR

Please note that you have to type the final #define own how you see in above example and not typing a row like:

#define @plugin_train CLEAR

the reason to use a fixed name "@plugins" is to simply the syntax of this management.
In fact, in the future it may be there will be many plugins and so you could need of syntax like these:

#define @Plugin_smartlights 4
#define @Plugin_robots  3
#define @Plugin_train 1

TriggerGroup= ....

#define @plugins CLEAR

In above example we gave a list of pluginIDs infos and then we cleared all with a single final "#define @plugins clear" line.

If you omit or misplace the final CLEAR line you could get bad troubles.
Now we do a more detailed sample:

We suppose you have in [Options] section these plugin= commands:

[Options]

Plugin  = 1, Plugin_people, IGNORE
Plugin  = 2, Plugin_train, IGNORE
Plugin  = 3, Plugin_demo, IGNORE

Then you export from NGLE a trigger owned of Plugin_train and you add this trigger to the TriggerGroup= 10:

TriggerGroup= 10, $024000, 132, $0012

In this situation you have no need to use #define @plugin  because NGLE set in the trigger the same PluginId it read in your script, and so, since the plugin_train had in your script the id = 2, ngle used id = 2 in the trigger.
You can see the first two hexadecimal characters of first exported number ($024000) as id. The $02 at begin is for PluginId = 2.

Ok, let's go...

Now we suppose you get from some friend a triggergroup (or more code) for some new skill, and this friend inform you that it used the plugin_people in those triggergroups. He informed you also that in his script the Plugin_people has Id = 2, while in YOUR script the id=2 is for plugin_train but you can solve this conflict using the define directive:

#define @plugin_people  2

TriggerGroup= 21, $02200, 17, $0124 ....

Above code will work fine, because when ng_center read the line "#define @plugin_people  2" it will have to associate the id=2 to plugin_people, pratically ng_center will convert the id from 2 (internal of trigger) to 1 the id for plugin_people in your script.

But if you omit the final CLEAR and you typed in you script the commands with this sorting:

#define @plugin_people  2

TriggerGroup= 21, $02200, 17, $0124 ....  ;triggergroup from your friend for plugin_people

... other commands ....

TriggerGroup= 10, $024000, 132, $0012  ;triggergroup yours for plugin_train

Ng_center will see also your triggergroup=10 as to convert, and it will change the id =2 (that it was for plugin_train) as it was for Plugin_people.

To fix this problem just you remember to close the previous #define with clear before meeting other triggergroups:

#define @plugin_people  2

TriggerGroup= 21, $02200, 17, $0124 ....  ;triggergroup from your friend for plugin_people

#define @plugins CLEAR

TriggerGroup= 10, $024000, 132, $0012  ;triggergroup yours for plugin_train

In this new code, all will work fine, since the "clear" will remove the previous define and when NG_Center will parse the triggergroup=21, not finding any pluginid definition in its internal plugin table, it will use the native pluginId, i.e. the 2 that is the right ID for plugin_train.

## #FIRST_ID
Syntax: #FIRST_ID  CommandName=FirstId
Scope: To use in [Title] or [Level] sections

This directive it has been studied to be used in the include files of the script.
The only disadvantage using include files is the problem of IDs conflicts.
Many script commands have an ID as first argument and these IDs have to be univoque for the same type of command.
In spite ng_Center is able to discover these conflicts and singal them as errors after the compiling, it should be fine to use in a include files always IDs in a range of values that is surely different than that used in main script or in other include files.
The #FIRST_ID directive tries to reach this target.
When you create an include file you should type in the first row one or more of directive like following:

#FIRST_ID  TriggerGroup=40
#FIRST_ID  GlobalTrigger=30

Thanks to above directives, when you are typing a new script command and you hit F1 to discover the first available ID for that command, NG_Center will compute the first available ID beginning in the counting form the value you type in the specific FIRST_ID directive.
So, looking the above example, if we are typing the first TriggerGroup command in this include file (but many other could exist in main script or in other include files) the ng_center will give you the value "40" as first ID for TriggerGroup.
And when you type a second triggergroup the first id will be "41"
In this way you can decide from what id will begin all script commands in your include files, reducing the risk of conflicts between different include files and/or main script file.
More, the first_id directives place at top of the include files have also an informative value when in the future you mean use newly that feature to discover immediatly the ID ranges used in that old include file.

Remarks:

- You can use defined by you menmonic constant:

#DEFINE BS_TG 30

#FIRST_ID TriggerGroup=BS_TG+10

- Don't let spaces between the operands at right of "=" sign:

This is uncorrect:

#FIRST_ID TriggerGroup=BS_TG+ 10

- The #first_id directive is local, this means it works only in same source file where it is

- To enhance the id range swapping, see also the description of #define directive

## #INCLUDE
Syntax: #INCLUDE "ExternalScriptTextFile.txt"
Scope: Any

The #include directive is not a script command, it doesn't perform any change to final script.dat.
It used only to divide and to put in order a long script.txt moving some specific blocks of commands from the main script to little text files in the SCRIPT folder.
When ng_center finds a #INCLUDE directive, it loads temporary the content of the given include file and paste it with the main script in same position where that include directive was. For this reason the response in the compiling it is always the same.
The usage of include directives is useful to put in order the script, dividing it in logical blocks.
For example when you create many script commands to realize a custom animazion or other effect, you can copy all commands for that target in a separated text file with a meaningfull name.
In this way your script could become like this:

#INCLUDE "LaraHome.txt"

#INCLUDE "AnimClimbFromWater.txt"

#INCLUDE "DistanceSensory.txt"

There are undoubt advantages in this job planning.
When you are not interested to some (already completed) feature, you have not all those script commands in the main script to create confusion.
While, when you wish work own on that feature, you can load (F5 key) the include file in Ng_center and you'll have always the meaningful commands in the editor to study and edit them.

If you work with include files read also the description of the directive #FIRST_ID.
It is very useful to reduce the risk of IDs conflicts of the commands stored in main script and include files

Remarks:

- You can type an #include directive ONLY in main script.txt file. It's now allowed use an #Include directive in a sub-file loaded from another #include directive of script.txt file

- Remember that you can load an include file in the editor hitting F5 key while the caret is over the #include line.

- You can use the command CTRL F5 to create an empty include file with wished filename, where you'll be able to store your commands.

## AddEffect
Syntax: AddEffect=Id, EffectType (ADD_), FlagsEffect (FADD_), JointType (JOINT_), DispX, DispY, DispZ, DurateEmit, DuratePause, Extra param array
Scope: to use in [Level] section
Max number of instances for level section: 99

AddEffect command is suggested only for advanced level builders.
Its more common usage is to have animcommands to use in animation editor (of wad merger program) to add particles effects to moveable only in specific moments (frame) of animation.
You can add: blood, flame, smoke and mist (like waterfall mist)
Theorically you can use add effect also like common triggers anyway the better usage is to convert the trigger in animcommand and then add it in animation of moveable.

Remark: from 1.2.2.4 version, you can add effects to static items, too.
When you add effects to statis some fields will be ignored and you can't apply it using a anim command, of course.
Arguments:

Id Field
--------
This is a simple identifier of what addeffect command of script file you refer in trigger windows. It works like first argument of PuzzleItem or PuzzleCombo commands.
For example if you type "1" this means you'll refer to this addeffect in trigger window choosing 1 like effect to add.
About valid range for id is 1/999 and the max number of instances is 99

EffectType
----------
You set in this field the ADD_ constant to choose the effect type to add. You find the full list in reference panel of ng_center program.

FlagsEffect (FADD_...) field
----------------------------
You can use some flag effects (FADD_) to override other settings about durate or behavior of effect.
See the description of FADD_ constants in Reference panel of NG_Center

Remark: when you wish add current effect to a static item some flags will be ignored while others could cause a crash, so don't use them!
Flags to do not use with static items: FADD_DURATE_ANIMATION, FADD_DURATE_STATEID, FADD_VORIENT_180, FADD_VORIENT_270, FADD_VORIENT_90

JointType (JOINT_...) field
---------------------------
Other to specify the type of effect you have to set also the exact position where to show this effect.
This compute is a bit complicated and you have to start from a point corresponding to some joint of moveable and then (see following fields DispX, DispY and DispZ) set the displacement from this joint you chose.
You can choose one of following joints:

Remark: if you add current effect to a static item this field will be ignored, since the statics have always only one mesh and no joint.

JOINT_SINGLE_MESH:0
JOINT_PUBIS:0
JOINT_LEFT_THIGH:1
JOINT_LEFT_KNEE:2
JOINT_LEFT_ANCKLE:3
JOINT_RIGHT_THIGH:4
JOINT_RIGHT_KNEE:5
JOINT_RIGHT_ANCKLE:6
JOINT_ABDOMEN:7
JOINT_NECK:8
JOINT_LEFT_SHOULDER:9
JOINT_LEFT_ELBOW:10
JOINT_LEFT_WRIST:11
JOINT_RIGHT_SHOULDER:12
JOINT_RIGHT_ELBOW:13
JOINT_RIGHT_WRIST:14

About the value JOINT_SINGLE_MESH (0), it will be used for moveable using a single mesh. In this situation the origin of effect will be the center of mesh (really the pivot but it's very often the center of single meshes)
Differently, when you want add a particle effect to multi-mesh moveables you have to set a joint like origin and then modify this origin with DispX, DispY and DispZ fields to create the effect in wished position.
See following field descriptions for more infos.

DispX, DispY, DispZ fields.
---------------------------
The three values DispX, DispY and DispZ are the distance between origin of effect you set in Joint field.
To understand how set these 3 values it's more easy to do an example.

If you want add smoke to lara's mouth (as breath) you should choose a joint near to target point, so we could choose the neck, i.e. the JOINT_NECK to type in joint field.

Then we should move this origin because we don't want the smoke in the neck but in upper and forward position.
To understand the axis X,Y,Z you can imaginate to watch lara while she has her face in front of you. (face to face)
Well, now the X,Y and Z axis have following orienting:

X Values will be negative at right hand of lara (but at your left) while the x values will become positive going to left hand of lara (but to your right)

Y Values will be negative when moving to head of lara (upward) while the Y values will be positive when moving to feet of lara (downstair)

Z Values will be positive when the direct is to your face (you are looking lara, do you remember?) while the z values will become negative go back to lara.

Following above orienting we could use these value to move the smoke in front of lara's mouth
Since we start from neck as origin, we should:

Move in front direction the smoke, because the mouth is put out of neck, so this regard the Z coord, and we'll use a positive values because the smoke will in front of lara and not back.
DispZ = +70

Then we need to move upward the smoke because the neck is down the mouth, since Y axe has negative values for upwards and positive values for downstair we could use value:
DispY = -70

About DispX we can let it to 0 because the neck and mouth are on same vertical line.

So we'll use the displacement:

JOINT_NECK, 0, -70, 70

and the smoke should be (about) in front of lara's mouth.

DurateEmit and DuratePause fields.
----------------------------------
For some effects, like smoke and blood, it's necessary alternate a phase of emitting particles with other phase of pause to perimit at particles to decay.
The values you type in above fields are number of frames.
For example if you insert as DurateEmit 3 and as DuratePause 10, the effect will be emitted for 3 consecutive frames and will be suspended for 10 frames.
Chaning these two values you can set the wished intensity of effect.

Some tip to set these two fields:
Fire and Smoke: it's better to use flag FADD_CONTINUE_EMIT  to have continue emit. In this case it's not important the values type in durate and pause emit fields because they will be ignored.
For blood: set 1 as emit and 30 for pause.
For mist: depend by result you want get. In some circustances also mist works fine with FADD_CONTINUE_EMIT flag, anyway if you want simulate sprays you can use  1 as emit and 3 as pause.

Extra Param array fields
------------------------
From this field you have optional fields changing in according with effect type.
Currently there are extra parameters only for mist and light effects.

Extra parameters for mist.
--------------------------
Extra1 = Size of mist ball. Default value is 12
Extra2 = Number of mist balls. Default is 1. You can set a max of 4 mist balls, they will be placer over an ideal line. The mist line will be oriented following facing of current moveable,anyway you can rotate the facing of mist line using rotate flags.
Extra3 = Color of mist. You can set a MIST_COL_ value. You find all colors in reference panel of NG_Center program
Extra4 = Persistence time of mist. Default value = 6. You should use this field only if the moveable is moving fastling and you want let a wake. Bigger values, longer wakes.

Extra Parameters for light effects
----------------------------------
Extra1 = Intensity of light. I don't know in detail the meaning of this value, anyway if you type bigger values you'll have most intense and widthness for your light. The light of flare has intensity = 16 and you should use values closed to this as reference. I presume it could be the number of sectors of distance reached by the light.
This value works for flat light and blink light but has no effect with spot-light.

Extra2 = Max distance for spot-light. This value works only for ADD_LIGHT_SPOT effect. The spot-light uses two 3d  points: the begin point and the end point of light cone. The first 3d point is the moveable where you added the spot, while the final point is last zone lighted by this spot computing SOurce + Distance in given direction (facing). The distance should be typed in sectors, for example the headlight of sidecar has 20 sectors of distance in default old tomb4 and it was the world far view. Anyway if you don't wish have a super power light it's better reduce this distance. If you type IGNORE in this field it will be used 12 sectors.

Extra3 = Color of the light. You can type in this field the same values MIST_COL ... constants, used for the mist effect.

Extra Parameters for ADD_FLAME effect
--------------------------------------
Extra1 = Intensity. The value for this field change fully in according with the presence or less of FADD_FIRE_STRIP
flag.

When you use the fire strip, in Extra1 you should type a value between +1000 to -8000. A reasonable value is -4000.

While when you use a common single fire you should type 0, 1, or 2 where increasing the number the size of fire will be increased.

Extra2 = Setting to burn Lara. 1 = it burns lara, 0 = no damage for Lara.

Extra3 = Setting vertical direction. 1=vertical / 0=horizontal
This field will be used only with FADD_FIRE_STRIP  flag.
If you want have the horizontal fire strip set 0 in this field.
While if you set 1 you get a fire eruction like  the fire of a (little) volcan

## Animation
Syntax: Animation=AnimIndex, KEY1_ , KEY2_ , FAN_ flags, ENV_ Environment, Distance for Env, Extra, StateId (STATE_...) or (-)AnimationIndex array  (...)
Scope: to use in [Level] section
Max number of instances for level section: 256

With Animation command you can perform new animation for lara when player hit some key.
(This command is suggested only for advanced level builders to add new moves for Lara)

Other to set the number of animation and hot key used to engage in game the animation, you should set also some condition about when this new lara command will be permitted, because perform an animation, for example a kick, while lara is underwater or hanged to rope it should be weird.

Arguments:
AnimIndex field
Here you type the number you see in Animation Editor of Wad Merger program.

Key1 and Key2 fields.
In these fields you can set a code (or two) to identify the activation key of your new animation.

Remark: the special value KEY1_RELEASED may be added to Key1 field to invert condition about keys. When KEY1_RELEASED is present the condition is true only when the specified keys are NOT down.

You can set two kinds of value in Key fields.
Keyboard scancodes or Keyboard game commands.
Both types identify a key of keyboard but there are some differences:
- ScanCodes permit to choose any key (or almost) of keyboard, while Game commands are only the prefixed keys you see in Option screen tomb raider game.
- By other hand, the Game commands have the advantage to be activated also by joystick and if player changes the setting in Option screen of tomb4, your command will continue to work.
Another difference is the way to type the valeus in Key1 and Key2 fields.
If you use Scancodes you should copy the value from Keyboard Scancode list of Reference panel of NG Center program.
While if you want to use preformatted game commands, you can use the constant KEY1_... and KEY2_ ... you find in MNEMONIC CONSTANTS of Reference panel.
When you use game commands remember you have to place the value in Key1 field or in Key2 field in according with prefix KEY1_  and KEY2_.

Differently if you use scancodes you can assign a new key, usually non used by tomb4, for example the scancode for F8 key, or for "Q" literal.

You can also set two different scancodes, writting the first in Key1 and second in Key2. In this case the animation will be performed only when both key are hit in some moment.
For example.
Animation=500, 42, 11, ...
Since 42 = (Shift left) and 11= (Number 0), the animation will be started when player hit in same moment SHIFT + 0 (zero)

Remark: if you want to use scan codes you have to add the flag FAN_KEYS_AS_SCANCODE in FAN_ field.

Note that it's a bit different when you use KeyBoard Game Commands.
With KeyBoard Game command you can insert two or more code in same Key field, just type the sum of these valuese.
For example:
You can require two or more game command in same moment, adding the constant value KEY1_  with other KEY1_.. and KEY2 with other KEY2.
For example to have the combination of game commands Action Jump and Dash, you'll have to insert in key1 field "KEY1_JUMP+KEY1_ACTION" while in Key field the valye "KEY2_DASH".

Remark: If you don't use some of Key1/2 field you should type IGNORE to signal that it is not used.

FAN_ Flags field.
-----------------
You can set one or more flags FAN_ to to set the behavior of animation command.
Read the FAN_ constant values in section _MNEMONIC CONSTANTS of Reference Panel of NG_Center to get info about FAN_ values.

ENV_ Environment field
------------------------
In Environment field you can set a single very spefic condition about the environment around to Lara.
You should use this field only when your animation requires a correct environment around lara like: climbable walls, holes, walls ect.

Remarks:
* Differently by FAN_ flags you can not type the sum of any ENV_ values, you can choose only a single ENV_ condition and you can add to it only ENV_POS_.. flags or ENV_NON_TRUE flag to inverse the mean of condition.

* If you don't want set any Environment you can type IGNORE in this field.

See description of ENV_ constants in MNEMONIC CONSTANTS sectio of Reference panel for usage of ENV_ values.

Distance for Env field
----------------------
If you set a value different by IGNORE in above field named Environment, you can set in this Distance field a number to specify some size or height or range about specific ENV_ condition you set in Environment field.
The common situation is Distance hosts the height/depth of wall/hole.
The units to use are 1024 for 1 sector, so 1 click = 256 and half sector is 512.
It's different the units when Distance field hosts a range, like for Environment named  ENV_MONKEY_CEILING. See description of ENV_MONKEY_CEILING value about how set the Distance field in that case.

Remark: you can always type in this field IGNORE and trng engine will use a default value ideal for current ENV_ condition you chose.

Extra field
This field may host different values in according with FAN flags.
Currently it accepts only slot numbers.
If you set flag FAN_WHEN_ON_VEHICLE in Extra field you should type the slot number of vehicle (really the slot on animations for that vehicle)
If you set flag FAN_USE_EXTRA_SLOT in Extra field you type the number of animation slot where find the AnimIndex you set.

StateId or AnimationId array fields
-----------------------------------
After Extra field you can type one or more values to set State-ids or Animation indices to use as condition to start your special animation.

The special animation will be performed only if at least one of given state ids, and one of animation indices,  is currently active for Lara in game in the moment that player hit the correct keystroke.

Remark: to recognize State id from Animation indices, you have to use positive values for State-IDs, while negative values for Animation Indices.
For example: the list:  "96, 112, -28, -42"
It will be read as: "Perform my animation only if state id of lara is currently = 96 or 112, and, in same moment, the current animation of Lara is 28 or 42 index.

About state id values, you can also type the mnemonic constants STATE_... that you can find in MNEMONIC CONSTANTS list in Reference panel.
If you don't understand what is a "state-id" or Animation index you can interpret then like a number to signal what lara is doing.

For example following table show most important state-id:
Climbing: $38 , $39, $3A, $3B, $3C, $3d
Falling: $09, $1d
Jumping: $19, $1A, $1b, $1C, $03
Moing on all fours: $50, $51, $47, $48
Rolling: 5
Running: 1
Walking: 0
Monkey: $4b, $4c, $4d, $4e, $4f, $52, $53
Still, stand up: 2

Remark: the '$' sign means "hexadecimal value". You can use calculator of Windows in anvanced mode to convert hex in decimal. Anyway you can type above number directly in hex typing own with $ sing in front.
Anyway you can type the numbers also in decimal, of course.

The state-ids you type are used by engine to understand when your animation is allowed and when it's not.
Normally you'll set as state id the value "2" in this way your animation will be enabled only if lara is still, on land and stand up.
But you could create particular animation to perform only when lara is climbing a wall, in this case you should type all values for climbing: $38 , $39, $3A, $3B, $3C, $3d  or only some of these values.
To discover all state id values or animation indices you can to use trng engine with Diagnostic=ENABLED  in script.dat and see the animation and state id values, or you can use Wad Merger program and enter in Animtion Editor.
When you select an animation of lara you'll can see also her "State-id" value and Index of animation.

## AnimationSlot
Syntax: AnimationSlot= Slot, AnimSlotFlags (ASF_...), AnimIndex, Key1, Key2, FAN_ flags, ENV_ Environment, Distance For Env_, Extra, StateId (STATE_...) or (-)AnimationIndex array  (...)
Scope: to use in [Level] section
Max number of instances for level section: 511

The AnimationSlot is very alike than Animation command with the difference that the AnimationSlot works on moveable different of Lara.
You can use it to create from zero a new moveable or vehicle, or to change the animations of some already existing moveable.

The most of fields and flags of AnimationSlot are the same of Animation command, therefore here it will be described only the difference between the two commands. While, to get infos about the common fields, read the description of Animation command.

Slot field
----------
In this field you type the slot number or mnemonic constant (BADDY_1, MUMMY) of the slot you mean change.

AnimSlotFlags (ASF_...) field
------------------------------
In this field you can type one or more ASF_ flags to affect AnimationSlot command.

Other fields (common to Animation command)
------------------------------------------
All fields following the (above) ActionType field, are the same of Animation command, so read the description of Animation command to have infos about those fields.
Here I'll do only a short briefing about the difference between the two commands.

(1) Since the AnimationSlot works on a specific (different by Lara) slot the animation numbers and state-ids in the final State_id, AnimIndex array, will regard the anim and state ids of that slot.

(2) Execpt for the Vehicles, the Key1 and Key2 fields are unused in AnimationSlot, since it is not the player to control the enemies.

(3) When you are building a vehicle you have to work on two different slots: the slot of the vehicle, and the slot with the lara's animation used for that vehicle.
Since lara and the vehicle will have always the same animation number in the same moment (until lara is driving it), it's not a problem set in the AnimIndex field the animation number, anyway it's not the same thing type in the Slot field (the first field of the AnimationSlot command) the vehcile slot or the Lara's anim slot, because the condition about environment and distance will regard one or the other moveable in according with this choice. For this reason, usually, you'll set the vehicle slot for animtype about the moving of the vehicle, since it has a bigger collision box and it's this collision box the most important, while you'll use the Lara's anim slot to compute other particular situations like the further injury of lara from enemies.
Remark: when you set a condition for the lara's anims the effective object that it will be used, it will be own lara, with the only differences about the animations used that they will be those of the specific lara's anim slot.

(4) When you use the env condition: ENV_CONDITION_TRIGGER_GROUP flag, the actions and conditions triggers stored in that triggergroup can be redirect to work on our enemy, vehicle, or Lara's anim slot, using one of following trigger group flags:

TGROUP_USE_ITEM_USED_BY_LARA_INDEX  This is the vehicle driven by Lara
TGROUP_USE_EXECUTOR_ITEM_INDEX  This is the index of Lara while she is driving, or it's the index of the enemy you are changing/creating with the AnimationSlot command
TGROUP_USE_FOUND_ITEM_INDEX (this is, as usual, the further item that has been found  with a testposition condition in the AnimationSlot command).

## AssignSlot
Syntax: AssignSlot=MyUsedSlot , OBJ_Type

Scope: To use in [Level] section
Number of instances for level section: 100

Remark: from 1.2.2.2 version the AssignSlot has new features, read carefully the following updated description.

MyUsedSlot field
----------------
Number or constant name to identifiy a slot position of standard tomb4 wads (example: "ENEMY_JEEP" or 34)
This slot identify where you placed your object
in slot list.

OBJ_Type field
--------------
In this field you type:

- An OBJ_ constant, to describe the type of new object you mean store in previous MyUsedSlot field
- Another existing slot from where getting the features to assing to your object.

With plugins, all new objects will require an AssignSlot and right OBJ_ constant.

While usign AssignSlot to reassing feature of some object to another you can type as SlotType any slot and all features of this slot will be forced to object you placed in MyUsedSlot slot.

For example if you place a restyled crocodile in Animating1 slot and you wish give to him the features of a real crocodile just you type in [Level] section the row:

AssignSlot= ANIMATING1, CROCODILE

You can use AssignSlot to have different enemies with same features but different looks, like two or more type of crocodile, or different baddy1 enemies but with different layouts, ect.

Remarks:

* It's probably that not all slots will accept a reassignment. The doors, waterfalls, switches and something-emitters probably will have troubles in a reassigned slot. You should use this command only to work with enemies.

* When you reassign a slot it's necessary that the both slots were really present in the wad.
For example in the command:

AssignSlot=	ANIMATING1, CROCODILE

It's necessary you have at least one real crocodile in the CROCODILE slot.

* Some enemies like Baddy1, Baddy2, Von Croy and the Guide require an alternate skin slot.
By default the alternate skin are present in following slots:

Alternate skin of Baddy1 in MESHSWAP3
Alternate skin of Baddy2 in MESHSWAP2

Alternate skin of Von Croy in MESHSWAP1
Alternate skin of the Guide in MESHSWAP2

When you reassign some of above enemies you have to supply also a new alternate skin but you cann't place it in meshswap slots, of course.

For this reason, you have to place its alternate skin in the slot immediatly following that you used.

For example if you use this script command:

AssignSlot= ANIMATING4, BADDY_1

The engine looks for the alternate skin for baddy 1 in the ANIMATING4_MIP slot, since this is the slot that follows the ANIMATING4 slot with the main object (reassigned) baddy 1.

* You can assing to a new reassigned enemies a new HP (vitality) different than other HP of the original object.

For example in above example:

AssignSlot= ANIMATING4, BADDY_1

You can force for the baddy_1 in animating4 slot a new HP different than other BADDY_1 slots, using the ENEMY= command:

Enemy= ANIMATING4, 1400, IGNORE, IGNORE, IGNORE, IGNORE, IGNORE

But you cann't set a different damage to Lara for animating4, in fact, if you type a value in an emeny command for damage it will be ignored.
The damage of reassigned enemy it will be the same of source assigned slot, in above example it will be the same damage for all others BADDY_1 objects.
For above reason, if you wish change the damage affected by a reassigned enemy you'll have to use the Enemy script command to modify the damage of original object from where you got the reassigment.

* The reassigned enemies doesn't support a MIP version

* The reassigned enemies cann't have animations different than those of original enemy. In the reality the animations of reassigned slot will be neither used in any way.

Trick: the only way to pass over above limitation is to insert in some animation of original enemy an exported flipeffect used to call a TriggerGroup in script.dat
If you type in this triggergroup also a starting condition about the slot ID of current item (see TRNG Variables demo) you could perform some animcommand only when that animation will be performed by reassigned enemy and in this animcommand you could place also the performing of another different animation. Anyway this new animation should be added always to original enemy and never to the reassigned enemy slot.

## ColorRGB
Syntax: ColorRGB=IdColor, Red, Green, Blue
Scope: To use in [Level] section
Max number of instances for level section: 199

With ColorRBG you can insert a RGB value to use in second moment with some flipeffect or actions using the IdColor as reference to locate it.

IdColor field
-------------
Progressive number to identify this color from other color you set in same [Level] section.
When you use some flipeffect or action requiring a script color you have to insert this IdColor number in trigger to locate the wished color
Valid range for IDs: 1 / 1999

Red field
---------
Intensity of Red. You can type value between 0 and 255

Green field
-----------
Intensity of green. You can type value between 0 and 255

Blue field
----------
Intensity of blue. You can type value between 0 and 255

## CombineItems
Syntax: CombineItems=FirstItem (slot), SecondItem (slot), FinalItem (slot)
Scope: To use in [Level] section
Max number of instances for level section: 92

FirstItem, SecondItem, FinalItem fields
---------------------------------------
All fields accept a slot value that you can find in ReferencePancel in the "SLOT MOVEABLES indices list" section.
Remark: in the reality you cann't use ANY slot but only slot corresponding to inventory items.
You can easily recognize these slots because they have in their name the word "_ITEM".
For example you cann't use PISTOLS_ANIM or LARA_HOLSTERS_PISTOLS but you can use PISTOLS_ITEM or PISTOLS_AMMO_ITEM.

Description
-----------
This command allows to create a new combining rule about inventory items to get a new item.
You had already the chance to combine two items, like "PUZZLE_ITEM1_COMBO1" with "PUZZLE_ITEM1_COMBO2" to get in inventory the new item "PUZZLE_ITEM1", or, to combine the "LASERSIGHT_ITEM" with "CROSSBOW_ITEM" to get the crossbow with laser-sight.

However, using the CombineItems command you can get other targets alike:

1) To create any coupling, and not only with specific "combo" items or with laser_sight + weapon.
For example you can combine "Puzzle1" with "Key2" to get any other new item.

2) You can set as final item not only the Puzzles items but also weapons, medipacks, examine items and all other inventory items.

Thanks to above chance you could, for example
, build a weapon letting to the player the target to pick up different pieces of this weapon, and only when he picked up all pieces he will be able to buil a working weapon.

You can create a final item got with 2 but also 3, 4, 5 and more, pieces.
To realize this target just you type two or more CombineItems command in same [Level] section.
For example if you wish that your final weapon was the Grenade Gun, and you wish there will be necessary three pieces to build it, you could type following script commands:

CombineItems= PUZZLE_ITEM1, PUZZLE_ITEM2, PUZZLE_ITEM3

CombineItem =
PUZZLE_ITEM3, PUZZLE_ITEM4, GRENADE_GUN_ITEM

In above commands we use the PUZZLE_ITEM3 only as intermediate item, therefore the 3 pieces to build the grenade gun will be:

(PUZZLE_ITEM1 + PUZZLE_ITEM2) + PUZZLE_ITEM4 =  GRENADE_GUN_ITEM

Remark: in spite of above example, it's not logical got a Puzzle_Item3 with Puzzle_item1+PuzzleItem2, since there are already two combos to build a PuzzleItem.
I used above example only to clear the progressive aggregate, but it should be more logical have our three pieces of weapon using some default combo items, for example:

 PUZZLE_ITEM1_COMBO1 + PUZZLE_ITEM1_COMBO2 + KEY_ITEM1_COMBO1

Then, about the  script command to use, since the combine rule to get the PUZZLE_ITEM1 with (PUZZLE_ITEM1_COMBO1 + PUZZLE_ITEM1_COMBO2) is already present, you'll have to type simply this only command:

CombineItems=  PUZZLE_ITEM1, KEY_ITEM1_COMBO1, GRENADE_GUN_ITEM

In this way, the player will pickup combo1 and combo2 for puzzle item1.
He will pickup also the third piece, the KEY_ITEM1_COMBO1, and at end when he combined (with standard combine rule) the combo1+combo2 for puzzle item1, then he will be able to combine furtherly the new Puzzle1 item with the third piece of the weapon, stored in KEY_ITEM1_COMBO1
 item.

Remarks:
* You cann't use IGNORE null value in this command
* If you type unvalid slot values the corresponding CombineItems command will be ignored by TRNG engine.
* The max number of CombineItems in same [Level] section is of 67 instances.

## CRS
Syntax: CRS=ENABLED/DISABLED
Scope: To use in [Options] section

CRS means Crash Resume System, it works in same way of CRS settings of NGLE program.
When CRS is enabled is very improbable the game was stopped by crash.

My suggestion about usage of CRS is to disable CRS (CRS=DISABLED) while you are building  your level (because it's better to know if somewhat doensn't work), while to enable CRS only before developing your final release on the net to avoid further crashes at players.

Remarks:
(1) When CRS is disabled if a crash occurs a file named Last_Crash#.txt will be created in current TRLE folder.
Another little file (always the same) will be showed non just the game has been closed, to inform you it's happened a crash and where you can locate the specific Last_Crash file with crash log.
(2) You can post crash reports on following forum: "Bug Reports about NGLE" ( http://www.aspidetr.com/forum/viewtopic.php?t=1748 )

## Customize
Syntax: Customize= CUST_ Customize Type, Arguments
Scope: To use in [Level] section.

The Customize command permits to change many little settings, replacing old default value/behavior of standard tomb4 engine with news.
You can type more Customize commands in same [Level] section.
By defaul the settings will work only for current level, anyway if you want have a setting working for all level in same time just you insert the Customize= command in [Level] section of Title level.

CUST_ Customize Type field
---------------------------
In this field you type a CUST_.. constant to specify what type of customizing you wish.
You can find the whole list of CUST_ constants in References panel of NG_Center program.

Arguments fields
----------------
About arguments that you type after CUST_ type, they are in variable number (in some circustances they are absent) and with different mean in according with current CUST_ type.
You should read the description of each specific CUST_ value to know its usage. You find all descriptions of CUST_ types in section _MNEMONIC CONSTANTS of Reference Panel of NG Center program.

## CutScene
Syntax: CutScene=ENABLED
Scope: To use in [Level] section

Cutscene command permit to singal current level like a cutscene-level.
The operations performed by this command are only two:
1) all keyboard/joystick input will be disabled for whole cutscene-level.
2) If lara has in her hands weapons or flare, force lara to throw out them to get free hand. This operation to free hands will be performed at start of cutscene level.

## Damage
Syntax: Damage=Flags DMG_ , SecondsForDeath, SecondsForBarRestore, BarColor, BarName, BlinkPercentage
Scope: To use in [Level] section
Max number of instances for level section: 2

Damage command allow you to customize appareance and behavior of damage feature in damage rooms and cold water rooms in game.
Remark: you can also don't use any Damage command in script.txt and damage rooms and cold water rooms will work fine but using default settings (see belove)

Arguments:

FlagsDMG_ = One or more constant values to set behavior of damage. You can find the list of DMG flags in Reference panel of NG_Center.
You can add different DMG_ values using plus '+' sign to sum them.
Currently are present following DMG flags:

DMG_INDIRECT_BAR
Enable progress bar for indirect damage.
When this flag is present the damage work in this way: the damage bar decrease but, until it's not empyt, there is no damage for Lara.
When the damage bar is empty, start to decrease HP (vitality) of Lara in very fast way.
Differently, if you don't set this flag the damage will effect directly the HP bar of lara.

DMG_ONLY_PAD
Restrict the damage only when lara is touching floor of current damage room. For this reason this flag will be ignored in cold water rooms.
You can use this flag to simulate a burning/electric floor in some room.

DMG_INCREASE_BAR
To use only with DMG_INDIRECT_BAR flag.
By default the damage bar starts full and decrease with the time, while using this flag you force the damage bar to start empty and increase while time elapses.
You can use this flag to simulate an increment of temperature in current room.

DMG_COLD_WATER
You can set this flag to customize Cold Water room feature.
If you don't use this flag, the setting will effect damage rooms.

DMG_POISON_LARA
Setting this flag you force a poisoning of Lara not yet she enter in damage rooms.

DMG_BURNING_DEATH
To use only for damage rooms waterless.
If this flag is enabled, lara will burns when:
The indirect bar is empty (if indirect bar is present)
The HP bar is empty  (if you don't enabled indirect bar for damage room)
You could use this flag to simulate a room where the temperature grows until burning lara.

DMG_BURNING_SCREAM
To use only with DMG_BURNING_DEATH flag.
If you set the DMG_BURNING_DEATH flag you can set also this flag to force lara to scream when she takes fire.
Remark: this flag works only with indirect bar, because when indirect bar is not set, lara burns and dies immediatly and so she has no time to scream.

DMG_SLOW_DISAPPEARING
To use only with  DMG_INDIRECT_BAR flag.
With this flag, when Lara goes off from damage/cold water room the bar will increase slowly until it's full, and only in this moment it will be removed from screen.
Differently, if you don't set this flag, the indirect bar will disappear immediatly not just Lara exits from damage room.

DMG_ALERT_BEEP
To use only with DMG_INDIRECT_BAR  flag.
If you set this flag, when the bar will be go down until 15 % (or less) than full value, the bar will blink and a little "beep" sound will be performed togheter with blinking.
Remark: Also when this flag is absent the bar will blink but no sound will be performed.

SecondsForDeath = Damage procedure use seconds as misure unit for damage and speed of bar. Pratically the number you set in this field is the number of seconds necessary to decrease fully whole bar. So, if you set a big number (for example 40 seconds) the damage is very tiny because it will be necessary many time to kill lara or to decrease whole bar. Differently, if you set a little number of seconds the damage will be very strong and lara will be killed in fast time.

SecondsForBarRestore = This field works like above field (SecondsForDeath) but in this case it controls the speed to restore fully bar when Lara goes off from cold water or damage room.

BarColor = In this field you type a RGB value to set main color of bar.
The rgb value in hexadecimal format (use '$' prefix for hexadecimal values) is: $RRGGBB
Where: RR = Red, GG=Green and BB=Blue intensity.
Some possible values are:

$f924f1  Pink (default color for cold water rooms)
$F6F923  Yellow (default color for damage rooms)
$fb8953  Orange
$FF0000  Red
$00FF00  Green
$0000FF  Blue

BarName = If you wish you can set in this field a string to describe the bar in game.
For example if you type in this field the string "Temperature",  this text will be showed in game at bottom of damage bar.
Remark:
It's necessary that the text you type in this field is present in english.txt file, in standard strings or in extrang strings sections.
A way to around this limitation is to type directly the index value of string instead to type the real text.
To know indices of strings you can use Strings panel of NG Cener program.
The index must be typed in following way:

#12    For standard string with index = 12 present in [Strings] section
!12    For extra ng string with index = 12 present in [ExtraNG] section

If you use index of string is not necessary that the string was present also in english.txt but just it is present in specific language.dat file you'll use in game.

Remark: if you don't want show a name for bar you can type in this file "IGNORE" or null sign for strings * (asterisk).

BlinkPercentage field
You can set from what percentage the bar will start to blink.
For example if you set 30 as BlinkPercentage, the bar will blinks when it 30 % or less than full bar.
If you type IGNORE in this field, it will be used the default value is 20%
Default values:
If you don't set a Damage command in script for current level the settings will be the following.

For generic damage rooms:

Damage=DMG_INDIRECT_BAR + DMG_SLOW_DISAPPEARING + DMG_ALERT_BEEP, 16, 6, $F6F923, IGNORE

For cold water rooms:

Damage=DMG_INDIRECT_BAR + DMG_SLOW_DISAPPEARING + DMG_ALERT_BEEP, 10, 5, $f924f1, IGNORE

## DefaultWindowsFont
Syntax: DefaultWindowsFont= IdWindowsFont, FLAGS (DWF_...), LineSpacing, MainMenuOffsets, NewGameTitle, LoadGameTitle, NewGameList, LoadGameList, OptionSettings, OptionCmdList, PauseScreen, StatList, InventoryItemName, ExamineText

Scope: To use in [Title] section

If you type this command in the Title section of your script, you force the tomb4 engine to use Windows font characters to show any text in the game.
All menu titles (New game/Load game/Options ect.) will be drawn whereby windows functions and using the windowds font you set in the given WindowsFont= command.

Remark:
The most advantage to use windows font is to be able to support different charset (different than default western charset).
Pratically using windows fonts you should be able to show texts in eastern languages.
Anyway, there are also some problems in windows font management.
Since the displaying of windows font is based on API windows function (and not directX functions), the drawing will be more slower than default directx management used in tomb raider.
This means there will be a light "flickering" of windows texts on screen.
Another risk is that the font you chose for windows font, it could be missing in some computer and in that case trng engine will try to locate an alike font but since it is different the format in game cuould be less good.

For above reasons I suggest to use the DefaultWindwsFont= chance only when you use an eastern language (not supported in tomb raider default), while for western level builders (and their players) I suggest to continue using the default font management, because it grants better performances and compatibile issues with all computers.

IdWindowsFont field
-------------------
You type in this field, the ID of the WindowsFont script command with the settings for the windows font you mean use as default for all system strings.
It's important you place, in the script, first the WindowsFont= command, and then (below) the DefaultWindowsFont that uses the previous.

Example:

ColorRGB=		1, 255,255,255
ColorRGB=		2, 0,0,0
WindowsFont=    1, Arial, WFF_BOLD+WFF_SHADOW, 40, 1, 2
DefaultWindowsFont=  1, IGNORE

Remark: many settings you type in the WindowsFont command, will be ignored in game, because it will be the game engine to set when use a blink text or less, a primary color (white), or a secondary color (yellow).
Anyway the size, font name and bold, italic (ect) settting will be used.

Note: I remember that form 1.2.2.7 version the WindowsFont allows to type in FontName field (in the string, pointed by that field) also the wished charset in the format:

Charset:FontName

Example:

128:Arial

Where 128 is the charset for eastern languages like japanese and chinese.
Other charset codes are:

Greek = 161
Turkish = 162
Russian = 204

About the western charset it is 0 but it's not necessary type it because when it's missing other charset it will be used always (as default) the western charset.

Flags (DWF_...) field
---------------------
In this field you can add DWF_ flags to customize this command.
If you don't mean set flags you can type IGNORE in this field.

LineSpacing field
-----------------
For line spacing I mean the vertical distance between a character row and the following.
You can reducing or increasing this space with this field.
The value is expressed in hundredths on the original value set by tomb raider engine.
For example if you type 100, there will be no change (futile in this case)
To increase of 20 % the line spacing you type 120, while to reduce by half, you type 50.

You can type IGNORE in this field to keep original value.

Offset modifiers fields...
--------------------------
From MainMenuOffsets field and following, there are fields to move up/down the position of some system text used in tomb raider menus.
The value you type is in pixel but since there are great differences in according with current resolution of tomb raider screen, you have to type these values working on a 1024 x 768 resolution, then trng will adapte these offsets in according with the actual screen resolution.

Note: remember that to move upper a text you have to set a negative value, for example -10 move the text 10 pixel higher than original position.

The names of these fields are self-explicatives, anyway we can remember shortly the strings they change:

MainMenuOffsets: First screen with "New Game", "Load Game", "Options", "Exit"

NewGameTitle: The title of "New Game" screen

LoadGameTitle: The title of "Load Game" screen

NewGameList: The list of levels in the new level screen

LoadGameList: The list of savegame infos in the load game screen

OptionSettings: All texts in main option screen: "Options", "Control Configuration", "Music Volume", "Sfx Volume", "Sound Quality", "Targeting" and their settings.

OptionCmdList: "Option", "Control method" and all the list of game commands and their settings.

PauseScreen: All texts of pause menu: "Paused", "Statistics", "Options", "Exit to title"

StatList: All texts of Statistics screen, except own the title "Statistics"

InventoryItemName: The name of the currently selected item.

ExamineText: The postion of the text linked with an examine item that you see after choosing "Examing" command

I remember that you can type IGNORE in all above fields if you don't wish change their position.

## Demo
Syntax: Demo= DemoFlags (DEMF_...), Parameter, InfoText , DemoLegendText, WaitingTime, DemoIndex array
Scope: to use in [Title] or [Level] sections

Demo command is of service to play the demo data you recorded using the EDGX_RECORDING_DEMO skill in DiagnosticType command. (See also the description of EDGX_RECORDING_DEMO flag for more infos)

This command works to show a demo of the levels of your adventure while player is yet in title level.
In above situation you'll place the Demo= command in [Title] section of the script, of course.

Anyway, if you use Demo command in a [Level] section it will work in custscene-mode, with some operative differences.

Remark: Remember that the two demo types have NO relationship between themself.
When you set a Demo in title level to play different demo.pak files, it's indifferent if the level section (where these demo.pak will be played) had or less a Demo script command.
The demo script command in a level section will be used only when the player load that level and not in demo mode from title.

DemoFlags (DEMF_...) field
--------------------------
You can add to this field one or more DEMF_ flags to customize as demo command will work.
Read the description of DEMF_ constants in MNEMONIC CONSTANTS section of reference panel.

Parameter field
---------------
It's possible that in the future some DEMF_ flag required an extra parameter to type in Parameter field.
Read the description of DEMF_ constants in MNEMONIC CONSTANTS section of reference panel to know how to use Parameter field.

InfoText field
--------------
If you wish that a text was typed in title level to inform players that it exists a demo mode and how to enable it, you add a string in string section and then copy the same text in InfoText field.
About color and position of the text, it will be used the setting of TextFormat= command in title section, and the text will placed at center of bottom line of the screen.

The InfoText string can, optionally, contain a percentage % character that it will be replaced with the second remaining to launch the demo, like in a countdown.

For instance, if you set as WaitingTime 30 seconds, and you set as InfoText string the text:

A demo will start in %d seconds

Then in the title level the player will see the time counting down in this way:

A demo will start in 30 seconds
A demo will start in 29 seconds
A demo will start in 28 seconds
A demo will start in 27 seconds
ect.
In the case you set the DEMF_PLAY_ON_KEY flag, it's strongly suggested to set always a text to inform the player, otherwise it's not probable he will discover himself the key to press.
So you could type a InfoText like:

Hit "1" or "2" or "3" keys to see a demo of different locations of this adventure.

In above case, we suppose you created demos data for three levels, the first, second and third.

See description of DEMF_PLAY_ON_KEY constant for more infos.

DemoLegendText field
---------------------
This field store a text to show while the demo is playing.
Please, don't confuse this text with the InfoText string.
While the InfoText will be showed in title level to inform players about how to start the demo and for this reason it should be a text like "Demo will begin in %d seconds" or "Hit 1 or 2 to show a Demo", the demo legend string will be drawn only in level (different than title) while the demo is in progress and therefor it could be a text like "Demo (hit escape for Menu)"

Also this text will be printed using the TextFormat setting but in this case it will be the textformat command of the level where the demo it will be played to be used.

WaitingTime field
-----------------
If you set in this field a value different than IGNORE (or -1), this value will be used as countdown to launch a demo.
The time is in seconds, and it will decrease from each player input.
This means that it's necessary that player didn't set any input command for WaitingTime before launching a demo.
If you set IGNORE in this field, the countdown method will be not used, in this case you should add the DEMF_PLAY_ON_KEY flag to DemoFlags field, otherwise no demo will be ever launched.

Note: you can also mix countdown method and DEMF_PLAY_ON_KEY flag. In this situation the demo could begin for an user input (hit the key) or because the elapsed time of countdown has been completed.

DemoIndex array fields
----------------------
In this array you can type one or more IDs for DemoID.pak file you created with the recording feature of EDGX_RECORDING_DEMO flag.

Notes:
- The Demo#.pak files have to be stored in DATA subfolder
- The name syntax of progressive Demo files is like this:
Demo1.pak
Demo2.pak
Demo3.pak
- Please, do not use IGNORE values in DemoIndex array fields.

## Detector
Syntax: Detector= Flags (DTF_..), MetricScale, MetersOfRange, Target Items array

Scope: To use in [Level] section.

In tr next generation engine there is a new object called "Detector".
With detector lara is able to locate position of some targets in real time.
There are two models of detector:
The pointer detector that you find in ng2.wad and
the radar detector that you find in ng.wad.
For both detector you must set a Detector script command in script.txt to enable it and set its features.

Fields of Detector script command:

Flags (DTF_..) field

You can add different DTF_ values in first field of detector command to set its features.

Currently there are following DTF_ flags:

DTF_ENGAGE_ALWAYS
DTF_ENGAGE_IN_RANGE
DTF_ENGAGE_INVENTORY
The DTF flags starting with "ENGAGE" set what is the way to start in game the detector.
You have to choose only one of above flags.

If you set DTF_ENGAGE_ALWAYS the detector will be always present on screen, at least until some of targets exits yet in the level.

Using DTF_ENGAGE_IN_RANGE the detector will be automatically showed when lara is closed to some target, within the given range (see following fields)
With engage_in_range you can set for example 50 meters and the detector will be showed when lara is to 50 meter or less from nearest target.
When lara will be more far the detector will be hidden.

The engage mode DTF_ENGAGE_INVENTORY set detector like other equipment of lara. She has to pick up it (the detector is in amulet of horus slot, i.e. quest1)
 the item will go in inventory, then when lara select it in inventory the detector will be showed while to remove from screen just select it newly from inventory choosing "Cancel" text.

DTF_REQUIRED_ITEM
This flag may work togheter with above engange flags to set that the Quest1 item is necessary it was present to enable detector.
For example if you use the flags DTF_ENGAGE_ALWAYS + DTF_REQUIRED_ITEM the detector will be showed (always) only if lara picked up it, while, until it's absent from inventory it will be not showed.
Note: If you choose as engage mode the DTF_ENGAGE_INVENTORY flags it's not necessary set also DTF_REQUIRED_ITEM flag because it will be added  by game engine byself.

Used with engage DTF_ENGAGE_IN_RANGE, the flag DTF_REQUIRED_ITEM requires two conditions to show detector: the quest1 item has to be in inventory (lara picked up it) and some target item is within the specified range.

DTF_RADAR_MODE
This flag set the detector in radar mode. If you omit to set this flag the detector will work in Pointer mode.
There are big differences between two modes and some fields and flags will work only for a specific detector mode.
In pointer mode the detector has a pointer like a compass that shows where is the current target (current target is the first target in the list yet present in game).
Pointer mode works only on one target at time: when the first target has been picked up (or killed) the detector will start to point second target in the list and go on.
In radar mode detector is able to scan all targets in same moment just they are in the range of radar.
Note: Remember that the range of radar is different of range for activation. The range of radar is given by formula 6 * MetricScale, where the digit "6" is the (fixed) number of grid sectors of radar. For example if you set 2 meter for metric scale the target will be showed on radar only when it is to 12 meters from lara or less.
To understand radar mode is bit more difficultous than pointer mode, because the target in radar mode will be showed always with up side = north, while in pointer mode the pointer is always relative to where lara is looking. In fact, in pointer mode, when the pointer is on red sign this means lara is looking in correct direction of target, while in radar mode it's advisable that lara looks to north to understand if targerts are at her left or right.

DTF_FAST_RADAR_SCAN
This flag used only with detector in radar mode. If you set DTF_FAST_RADAR_SCAN flag the scanning of targets will be faster.
Remember that in radar mode the position and distance of targets will be updated only when the hand of radar "touchs" the target.

DTF_SWINGING_POINTER

This flag works only in Pointer mode.
By default the pointer points exactly to target, anyway if you want you can add a swinging simulation of pointer like the compass in inventory to get more realistic detector.

DTF_INVERSE_VPOINTER
Current flag works only in pointer mode.
You can invert the position of line in vertical scale (at right of detector)
By default (when you DON'T use this flag) the floating line shows where is lara, while the fixed red pointed line at center shows the position of target.
If you think it's not logical this relation you can use this flag to invert the situation. With the DTF_INVERSE_VPOINTER flag the floating point is the vertical position of target, while the fixed red line is the vertical position of Lara.

DTF_NONE
If you don't want set any flag in this field you can use DTF_NONE flag.

MetricScale field
In this field you can type a value in meters that will be used to assign what is the distance for each sign of vertical scale or grid table (for radar).
If you set IGNORE in this field the default value used will be 2 meters.
Since a block in game is 2 meters, setting 2 as metric scale each line in detector will define a distance of 1 block (and 2 meters).
For example se in radar mode the detector show a target at 3 squares (in radar) from lara (center of radar panel) this means that target is 3 sectors and 6 meters from lara.
In pointer mode the metric scale works only for vertical panel (the vertical strip at right of detector).
Differently, in radar mode is very important the value set as metric scale, because only target in the radius of 6 * MetricScale meters.
The six value is the number of square of radius of detector.
For example if you want that in radar will be showen also targets very far, for example at 100 meters of distance (50 blocks) you should set as metric scale a value like (100 / 6) = (about) 16 meters.
So, while for pointer mode it's better to use a low value for metric scale (the default value "2" is normally good) with radar mode it's better to use big values at least you wished permit to player to see target only when him is very closed to them.

MetersOfRange field.
This field will be used only if you set the flag DTF_ENGAGE_IN_RANGE as engage mode.
When you use engange in range as activation mode the value in this field is the distance (in meters) of targets necessary to show in game the detector.
If you use radar detector could be reasonable set meters of range with same value of range visibility computed on MetricScale.
For example if you set 2 (default) as metric scale this means will be showed targets only when they are in (2*6) 12 meters from lara, now you could set same value (12) also for MetersOfRange, so the detector will be showed only when there is something to show (a target in visibility range).

Target Items array fields
In field of Target Items Array, you can type one or more indices of items (only moveables) you see in NGLE program.
If you type two or more indices you have to split them with commas.

Example:

Detector=	DTF_REQUIRED_ITEM+DTF_ENGAGE_IN_RANGE+DTF_SWINGING_POINTER + DTF_INVERSE_VPOINTER, 2, 30, 365, 372, 373, 375, 377

In above row the "2" value is metricScale, the "30" is MetersOfRange, while all other numeric values (365, 372, 373, 375, 377 ) are indices of moveables to monitor.

Remark.
The list of item array works in different way in according if you are using Radar Mode or Pointer Mode.

Pointer Mode.
In Pointer Mode, the detector will show the position only of one single item at time.
In above example the first position showed will be that of target with index 365, indifferently by position of different targets (near or far).

Only when first index (365 in above sample) will be picked up or killed (if it's a baddy) the detector will start to follow second item (372 index in above example).
Knowning this mode of work is clear that you should don't use non-killable/pickable items (like a door or an animating) because in this case the next item in the list will be never followed by detector.
If you want set anyway as target a non killable/pickuble item you have following chances:
- You can use radar mode detector because it (as you'll see) works in same moment on all targets of list
- Or you place the index of non killable item at end of list.
- There is also another chance: you use an ACTION trigger (in special trigger of NGLE) to remove (kill) this item when lara reaches it (or pass it) walking on same square with kill trigger.

Radar Mode.
In radar mode all targets will be followed in same time, anyway only targets, enough closed to enter in visibility range (MetricScale * 6), will be showed on radar screen.

The radar panel show targets in different ways in according with them vertical position.
When a target is at same height (floor) of Lara, the target will be showed like a little blinking circle.

Differently, when target is upper than lara (of 4 or more meters) it will be showed like a triangle with apex pointing to north, while when target is downstairs of lara than 4 meters or more) the triangle will point to south.
Same shapes will be showed in vertical panel (the row at right of detector)

Reading above description you may understand that the circle (for targets at same height of lara) will be really showed also when target is only a bit up or down of lara (+2 / -2 sectors).

## Diagnostic
Syntax: Diagnostic= ENABLED/DISABLED
Scope: To use in [Options] section

If you enable the diagnostic with  Diagnostic=ENABLED, in game will be showed on screen some information in real time about number of current Lara's animation, the current State-id of lara and current effective frame rate in game.

These infos are useful to discover what is the number of some animation, then you can use this number to locate animation in wad file using Animation Editor of wad merger program.

If you have problems to read in real-time above informations, remember you can also to use Tomb4_Logger.exe program to catch all diagnostic mexages and store them in a text file.
If you perform Tomb4_Logger.exe before starting tomb4 trng engine with diagnostic enabled, you'll have also in log file the LoadCamera= .. script command line with infos about current camera view to add in script.txt file for load camera screenshot
To get infos about LoadCamera in log file you have to keep down the F1 key.

This is a sample of text you can get in this way:

  21656:   LoadCamera=  6656, -2817, 14123, 6656, -2631, 12800, 2
  21672:   Animation=103   StateId=2  ($2)

## DiagnosticType
Syntax: DiagnosticType =DiagnosticType (DGX_...), Extra Dgx flags (EDGX_...)
Scope: To use in [Options] section

This command works in according wit Diagnostic=ENABLED.
When you enable the diagnostic you can set what type of diagnostic infos show using the DiagnosticType command

DiagnosticType (DGX_...) field
------------------------------
In this field you can type one or more DGX_ constants to enable the specific diagnostic informations you wish see.

Extra Dgx flags (EDGX_...) field
--------------------------------
In this extra filed could be required some EDGX_ value to furtherly customize the diagnostic type you chose.
See the description of DGX_ constants to kwnow if they accept some EDGX value in this field.

## Diary
Syntax: Diary= Id Diary, SlotDiaryItem, LaraDiaryFlags (LDF_...), BackGroundImageID, Default PageLayout (PL_...), FirstString, TitleWFontId, CommonTextWFontId
Scope: To use in [Level] section
Max number of instances for level section: 99

This command enable the Lara's diary, and permits to set some settings about Diary.
The Lara's diary should be like a log where there are texts and images to describe the adventure while it's going on.
There are flipeffects to add new pages to Lara's diary, to update the diary in according with the evolution of adventure.
The player will be able to select in inventory this item like it was an examine object, entering in Diary mode, where player can browse and read unlimited number of pages, texts and images.

Remark: at bottom of the description of Diary= command, there will be also a description about special text formatters you can insert in text to show with diary. Please, give a look also to those descriptions.

Id Diary field
--------------
You can have upto 10 different diary in all your adventure.
Each diary will continue in further levels.
For this reason it's important you type, in any [Level] section where you wish use a previous defined Diary command, the same Diary command with same ID value. The only difference it will be for the FirstString field that it will be ignored for the Diary commands
 used in further Level sections.

To distinguish the diary to use in some flipeffect trigger you have to specify its ID.
Remark: the chance to have many diary, permits to use diary like an advanced Examine item, with many infos and images you can continuosly update or increase.
Valid range for IDs: 1 / 999

SlotDiaryItem field
--------------------
In this field you type the slot corresponding to Lara's diary item.
You cann't chose any slot item, the slot has to be an inventory item, and it's suggested to use a quest item like:

QUEST_ITEM1
QUEST_ITEM2
QUEST_ITEM3
QUEST_ITEM4
QUEST_ITEM5
QUEST_ITEM6

In example "Lara's diary" on trng website, I used the QUEST_ITEM2.

LaraDiaryFlags (LDF_...) field
--------------------------------
In this field you can type one or more LDF_ flags to set how work with background audio track when the diary will be showed.
If you type IGNORE, no change will be performed about audio tracks, and the previous audio track of game will continue like it happens in Inventory.
Differently, if you wish stop game audio track and set another custom background music while player is watching the Diary you'll have to set the correct LDF_ flags.

See description of LDF_ constants in Reference panel of NG_Center program.

BackGroundImageID field
-----------------------
The Lara's diary in examine mode is a bidimensional item and for this reason it's necessary set the image for background, because this is the effective visible Diary in examine mode.
In this field you type a number and this number correspond to a bmp image in this format:

IMAGE4.bmp

For example to use above image as background, you'll type in BackGroundImageID field the value 4

Remarks:

- The bitmap image could have any size and it will be resized to fit the screen, anyway it's advisable to use an image of at least 640x480 pixel, or, better, 800 x 600 or higher

- If trng detect a wide-screen monitor, the background image will be NOT resized to full screen, but it will be enlarged to fit the heigth of screen, while for width, trng will try to preserve the size ratio, to avoid distorsions of image. In this circustance it could happen there were two short vertical columns at sides of Diary image, in these two external columns it will be visible the previous tomb raider background.

- You have to supply the image#.bmp with your level files. If the level will be installed using TRLM 2009 (Tr Level Manager 1.5) it will be able to handle in correct way the bitmap images, while for information about manual installation to give to users, trng is able to find images in different folders:

TRLE folder (i.e. in same folder of tomb4.exe)
TRLE\PIX subfolder
 (this was used also by trep, and also trlm2009 will place all bitmap images (different by Load.bmp, of course) in PIX subfolder)

- Currently trng supports only images in bitmap (.bmp) format, anyway you can insert the images in jpg format to safe space, and then add also the little utility CONVERTER.exe in PIX folderf to permit the conversion to .jpg format

Note: Trng tomb4 engine will call byself the converter.exe when it finds this utility in  PIX folder and some impage .jpg or .jpeg in same PIX folder.

Default PageLayout (PL_...) field
---------------------------------
In this field you can type the PL_ flags to choose the layout used for Diary as default.
For "layout" we mean the disposition (positions) of text, title and further little image to show on same page.
The differet layouts set where show above elements.
You should try to create a background image in according with the wished layout.
See description of PL_... constants in Reference panel of NG_Center for more infos.

FirstString field
-----------------
If you wish initialize the diary with a first text you can type in this field the text of first string.
The text you type in this field has to be the same in some extra ng string.
Remark: since it's comfortable type a long text in this field you could type in extra ng string a file-string in the form: "@MyText.txt" and then type the effective text in a file named "MyText.txt" saving it in SCRIPT folder. In this example you'll type in this field the text "@MyText.txt"
If you don't wish type any first string you can type IGNORE in this field.

Remark: in this first string or in other strings you add to diary using flipeffect "Diary. Add (&)NG String to Diary" it can type one or more pages in same moment, just in that extra ng string there is some #END_PAGE# tag to separate differente pages in same NG string.

TitleWFontId and CommonTextWFontId fields
-----------------------------------------
To set color and size of text, you have to type a WindowsFont= command script and then place in our Diary command the Identifier of that WindowsFont command.
Remember that, the WindowsFont commands used by our Diary command, have to be typed before that Diary command.
In TitleWFontId field you type the id to choose the preference for title. The title will have a big size than common text, of course.
In CommonTextWFontId field, you type the id to be used for common text

The text formatters to use in text pages
----------------------------------------
The settings you typed in Diary command will work as default values but if you wish, you can change all these settings for each page.
In each page you can insert some special formatters used to inform trng about some change about settings of diary.
The text used to give these settings will be not displayed on screen, of course.
All testual settings have to be enclosed in a couple of tag <FORMAT> and <END_FORMAT>.
You have to type one and only once the <FORMAT> tag for each page.
You can type a FORMAT section in each page, anyway remember that, if you omit to type a FORMAT section in some page, in that page will be used the default settings you typed in Diary command and not the FORMAT setting of previous page.
Within the FORMAT section you can type one or more of following text tags:

#TITLE_FONT#=(Id of WindowsFont for title)
#TEXT_FONT#=(Id of WindowsFont for common text)
#PAGE_LAYOUT#=(PL_.. constants)
#BG_AUDIO#= (identifier of audio track to play in background for this page)
#BG_IMAGE#= (identifier of image to use as new background)
#POP_IMAGE#= (identifier of image to show in this page)
#TITLE#= (Text to show at start of this page as title)

How set end page signal
-----------------------
A very important tag to use always in diary text is the end page tag.
You can type in same text many different pages but it's necessary to give an indication about when a page ends and when starts new page.
To reach this target you can insert in some point of your text a little tag formatter to signal the end page/new page in that point.
Just you type in your text the tag:

#END_PAGE#

Remark: it's important you place the #end_page# on a single empty row, this means you cann't place the end page in this way:
-------------- WRONG METHOD --------
The key is yet in bottomless pit. #END_PAGE#
Now Lara should find the key but to reach the pit she has to search...
-----------

Above position is wrong.
The correct mode to place the end page is this:

-------------- RIGHT METHOD -----------
The key is yet in bottomless pit.
#END_PAGE#
Now Lara should find the key but to reach the pit she has to search...
--------

Example of text used for diary
------------------------------
To give an idea about how it should appear a correct text to use for Diary you can see following sample:

-------- example of text used for diary ---------
<FORMAT>
#TITLE_FONT#=1
#TEXT_FONT#=2
#PAGE_LAYOUT#=PL_WIDE_IMAGE+PL_ADD_INFO_BAR
#BG_IMAGE#=1
#POP_IMAGE#=2
#TITLE#=Mission Organizer
<END_FORMAT>
This is the Mission Organizer of Lara
In this pages you can verify the targets Lara has to reach.
New pages will be adedd while the mission goes on, so remember to come back to read this document also in future, you could get many important informations
#END_PAGE#
<FORMAT>
#TITLE_FONT#=3
#PAGE_LAYOUT#=PL_DOUBLE_PAGE+PL_ADD_INFO_BAR
#BG_IMAGE#=3
#POP_IMAGE#=4
#TITLE#=Beginning...
#BG_AUDIO#=56
#FRAME_IMG#=20,10,400,500
#FRAME_T1#=520,10,400,500
#FRAME_T2#=20,600,400,400
<END_FORMAT>
Lara has to find the man stole the security key.
Last time he has been seen in the dark castel on the hill.
... other text ....
Tha man is very dangerous because he has a misterious power...
#END_PAGE#
------------- end of example of text for Lara's diary ------------

Above is a short sample of text diary with two pages and different tag formatters.
Looking above text we could describe the meaning of formatting tag.
In first page we had:

<FORMAT>
#TITLE_FONT#=1
#TEXT_FONT#=2
#PAGE_LAYOUT#=PL_WIDE_IMAGE+PL_ADD_INFO_BAR
#BG_IMAGE#=1
#POP_IMAGE#=2
#TITLE#=Mission Organizer

<END_FORMAT>

And we could read above settings in this way:
"#TITLE_FONT#=1"
Print the title using font setting stored in script command:

WindowsFont=1, ....

"#TEXT_FONT#=2"
Print the common text using settings stored in script command:

WindowsFont=2, ...

"#PAGE_LAYOUT#=PL_WIDE_IMAGE+PL_ADD_INFO_BAR"
For first page use a layout with a wide image at top of the screen in centra position, and let at bottom of screen the space to host my info bar (drawn in background image used for this first page)

"#BG_IMAGE#=1"
As background image use the file:

IMAGE1.BMP

"#POP_IMAGE#=2"
Display in top half of screen the wide image of file:

IMAGE2.BMP

"#TITLE#=Mission Organizer"
At top of wide image2.bmp, place the title:

Mission Organizer

Then it will be displayed the text:
<<
This is the Mission Organizer of Lara
In this pages you can verify the targets Lara has to reach.
New pages will be adedd while the mission goes on, so remember to come back to read this document also in future, you could get many important informations
>>

And all above are the stuff of first introductive page od Lara's Diary.

Then there is the #END_PAGE# tag, and trng will wait the player hit RIGHT arrow to pass to second page.

The setting of second page are:

<FORMAT>
#TITLE_FONT#=3
#PAGE_LAYOUT#=PL_DOUBLE_PAGE+PL_ADD_INFO_BAR
#BG_IMAGE#=3
#POP_IMAGE#=4
#TITLE#=Beginning...
#BG_AUDIO#=56
#FRAME_IMG#=20,10,400,500
#FRAME_T1#=520,10,400,500
#FRAME_T2#=20,600,400,400
<END_FORMAT>

The meanings are:

"#TITLE_FONT#=3"
Disaply the title using font (color and size) set in script command:

WindowsFont=3

"#PAGE_LAYOUT#=PL_DOUBLE_PAGE+PL_ADD_INFO_BAR"
The layout now will a double page, where the further image will be showed at top of left page
Keep the info bar at bottom of screen, too.

"#BG_IMAGE#=3"
Use as background image the image stored in file:

IMAGE3.BMP

"#POP_IMAGE#=4"
Show at top of left page the image:

IMAGE4.BMP

"#TITLE#=Beginning..."
Show over the image4.bmp, the title:

Beginning...

"#BG_AUDIO#=56"
Play the audio track stored in the file:

TRLE\AUDIO\056.wav

(but it could be also 056.mp3 or 056.ogg in according with effective presence of files)

"#FRAME_IMG#=20,10,400,500"

Set the position and size where show further little image in this page.
If in diary layoutpage flags you set the PL_CUSTOM_LAYOUT this setting will reamin also for following pages, while if you set some other PL_ layout, this setting of "#FRAME_IMG#=" will be used only for this page, while for other pages will be used the PL_ layout set in diary command.
The values of this tag (20,10,400,500) are in micro units, where 1 = 1/1000 of widht or heght of screen size.
To compute these values you can use the utility [Get Screen Frames] you find in Tools panel of NG_Center program.

"#FRAME_T1#=520,10,400,500
"

Set the position and size of first frame used by text in this page.
For "first" frame we mean the frame where place first block of text of this page.
In some layout you can have two text frame, in this case the "
#FRAME_T1#=" tag set the first of two text frames, or the unique text frame when there a single text frame.
Also this tag requires micro units, see descript of "#FRAME_IMG#=" tag for more infos.

"#FRAME_T2#=20,600,400,400
"

Set the position and size for second text frame.
When your layout foreseen two text frames you'll use the #FRAME_T2#= tag to set the position of second block of text for this page.
Also this tag requires micro units, see descript of "#FRAME_IMG#=" tag for more infos.

Remarks:

1) The background sounds in Diary mode will work only if you enabled the BASS features in your level

2) If you mistake placing an #END_PAGE# too low respect to effective visible text on the screen, the player will be not able to view some rows of current page, but no extra text will be showed over info bar

3) When some tag formatter is missing it will be used the default setting you set with Diary script command

4) If you want stop a background sound set in previous page, but you don't wish start any other audio track, you have to type the tag formatter:
#BG_AUDIO#=-1
In this way, the previous audio track will be stopped and further default audio track, set in Diary script command, will be played.

5) To host big texts in NG_Center you should use the trick to set in extra ng strings only the file reference, in this way:

45: @FirstPages.txt

Then NG_Center inserts as "exta ng string number 45" all text present in text file "FirstPages.txt"
The "FirstPages.txt" has to be present in SCRIPT folder while you build the script.dat.
While, once you builded the script.dat and english.dat, the file "FirstPages.txt" it's no more necessary and you don't should enclose it in your level files because its contents are already present in english.dat (or other language.dat file)

## Elevator
Syntax: Elevator=ElevatorIndex, ClickFloorDistance, NumberOfFloors, Elevator flags (EF_...), FirstDoorIndex, InnerKeyPadIndex, Speed, Frame items array
Scope: To use in [Level] section.
Max number of instances for level section: 20

Arguments:

ElevatorIndex= Index you see in NGLE in yellow box when you click over elevator item

ClickFloorDistance = Number of click for height of each floor. For example if there are three blocks between each floor you'll type in this field "12", since each block is 4 clicks.

NumberOfFloors = Easy, the number of floor of this elevator. For example is you type 3 it means your elevator will move in in 3 different positions (floors)

ElevatorFlags
You find all elevator flags in reference panel of NG_Center program.
You can place one or more flags using the "+" sign or type directly the sum of rispective values.
However remember that some flags are uncompatible to use in same moment, like Multi-doors and single-door flags.

Currently there are following flags:

EF_MULTI_DOORS  ($0001)
If you want that trng engine handle for you all doors in front any floor you can set this flag and then set in IndexFirstDoor the index of door at first floor.
The management of game engine is very simple: when elevator reach a floor the door of that floor will be opened.
When elevator leaves a floor the door in that floor will be closed.
If you use this multi-doors handling is important you place all doors in your project exaclty at same vertical distance you set in ClickFloorDistance field. It's important, because is you place some door to different distance that door will be not found by trng engine and you'll have troubles.
Pratically just you place each door at same height where the elevator will stop it, floor for floor.

EF_SINGLE_DOOR  ($0002)
If you use single door mode you cann't set also flag multi-door.
In single door mode the elevator has a single door linked with elevator cage.
This door will be moved up and down in according with elevator movement.
When elevator reach a flor the door will be opened, when elevator starts from a floor the door will be closed.
If you set this flag you have to type in FirstDoorIndex field the index for the door you placed near to elevator.
Remark: you can use also a "fake" door you find in slot Animating2 in ng2.wad
This animating will be moved, sliding it horizzontally, like a door by trng engine if you use single-door mode and you set its index in FirstDoorIndex field.
There is a reason to use a fake door like this: when you use a real door,in fact, in some circustances (it depends by door type and by position of door respect to rooms linking) the engine will add an invisible sector collision in front of the door when it's closed. Since it's boring in many situations you can use the fake animating door to avoid this problem.

EF_INNER_KEYPAD ($0004)
If you want insert the keypad to choose flor inside the elevator you must set this flag and (in ngle) to place the keypad oriented on wished elevator wall. Then you'll type the index of keypad in InnerKeyPadIndex field.
Remember to use the "fake" keypad you find in Animating16_mip of ng.wad or ng2.wad and not the real keypad in switch_type1.
The reason of this choice is given by a technical problem: the switch_type requires a switch trigger to work, but you elevator floor you have to place also dummy triggers and for this reason it's not possbile place both special trigger in some sectors.
For this reason the management of "fake" keypad (of Animating16_mip) will be performed in hardcoded mode by engine.
Apparentrly the keypad will work in same way of switch_type1 but with no need of special trigger to handle it.

EF_MODE_YO_YO ($0008)
Yo yo mode is a bit curious: elevator will move endless up and down, without any stop at floors.
Using this mode you cann't use doors, since there is no time to open and close them.
Lara will have to jump at fly in elevator.
An interesting application of yo-yo mode is to give to elevators also the role of trap, since if lara will be touched by elevator she will be killed (if she is under elevator moving down, or over elevator when elevator moves up and above lara there is the ceiling) or "disturbed" if she is climbing the wall when elevator touch her (lara will be untouched by wall)
Remarks:
1) You cann't mix yo yo mode and stop and go mode.
2) If you set elevator for yo you mode the number of floor will be always two (2) and the distance between floor will be the height of whole movement that elevator will cover before inverting direction.

EF_MODE_STOP_AND_GO ($0010)
Stop and go mode is similar to yo yo mode, but in this case the elevator stop and stay for some second (2 for elevator door less, 3 seconds for elevator with doors) in each floor.
Differently by yo yo mode, using stop and go mode you can place the doors (multi or single) and you can set a various number of floors.
Also stop and go mode may heart lara (like all elevators)

EF_NONE (0)
If you don't want set any flag you can type EF_NONE or the value corresponing: 0 (zero)

FirstDoorIndex= Index of first door (that at first floor). You place in this field the index for single door or multidoor.

InnerKeyPadIndex = Index of keypad you inserted in elevator. Remember also to set the EF_INNER_KEYPAD flag when you want have an inner keypad

Speed=Set the speed of movement for elevator. If you type IGNORE in this field the default speed is 20.
About speed remember that this units is 1/1024 of sector. I.e. : 1024 = 1 sector, 256 = 1 click.
Each second will be changed the pos
ition for 30 times, so it's better don't exagerate in speed value. The max value suggested is 50, if you pass over this value you could have troubles in collision computes when elevator hits lara.

Frame Items array
After speed field starts an optional list of item indices.
It's optional, so you can also don't type anything after Speed field.
Anyway the frame array is useful for many targets.
All items you type in this array will moved up and down in according with elevator movement, giving the idea to be linked with elevator cage.
For example you can create a chain to place over the ceiling of elevator to get more realistic the movement of elevator.
In ng2.wad you find animating5 that it's used in samples for yo-yo and stop and go elevators.
You can place different instances of this chain to cover all height of elevator moving and then type all indices of chains in elevator script command. In this way when elevator will be moved in game, also the chain will be moved.

Another usage of frame array is to place invisible collision panels around elevator.
You can find collision panel in Animating1 of ng2.wad (1x2 sectors) and Animating1_mip (1x1 sector to use when you set a little door for elevator cage)
You need to use collision panels only if you want place elevator in outdoor, i.e. without room walls around it.
If you place the elevator at center of big environment it's necessary you place collision panels, otherwise lara will be able to go through elevator walls of cage.
In yo-you and stop and go samples you can see like collision panel were used.
Just you place over each elevator wall the collision panel.
You should cover all walls except where is the door of elevator cage, then you type all indices of this collision panel in frame array, and the collision will be moved up ad down getting elevator cage a real closed box.
You can place about 23 frame indices in frame array but for collision panels just a maxminum of 8 items.

Remarks:
1) When you place elevator and other items for elevator (keypad, single door, chain, collision panel ect) in your project, the elevator must be at first floor.
If you want in game to have elevator in another floor you can use the elevator flipeffect to move it in wished floor but it's necessary that the elevator was always at first floor in project phase.

2) The max number of floor is 10. When you use 10 floor the 10th floor will be chosen with keypad typing the value "0"

3) You can have until 20 elevators for level, working with different modes in according with different Elevator script commands

## Enemy
Syntax: Enemy=Slot, HP (vitality),  NEF_ flags, TombFlags (TCF_...), EXTRA_ flags, Damage1, Damage2, Damage3
Scope: To use in [Level] section
Max Number of instances for level section: 100

This command permits to set different settings for specific Slot objects.
You can set the HP (vitality, i.e. resistence to shot of Lara), the damage they give to Lara (damage1, damage2 ..).
Using some flags, like Ng Enemy Flags (NEF_...) or Tomb Enemy Flags  you can set particular behavior of slot.
You should use this command only con Slot = moveable enemies (baddies) or traps, anyway you can use this command also to change all moveables but many settings will have no effect in this case.

Arguments:
Slot field
Wad slot to modify with this command. (See Slot Moveable list in Reference panel of NG_Center program)

HP (vitality) field
You can set resistance of this enemy to Lara' shots.
For example if you set a big value (1000) it will be very difficultous kill it. If you set a little value (20) it will be killed with few revolver shots.

NEF_ flags field
This field accept values NEF_... (See Mnemonic Constants list in Reference panel)
 to chose different settings.
Currently there are following values for NEF_ flags:
NEF_EXPLODE
When creature will be killed it explodes
NEF_EXPLODE_AFTER
Like "nef_explode" , but the body will explode only after death animation is complete
NEF_HIT_BLOOD
NEF_HIT_DEFAULT
NEF_HIT_FRAGMENTS
NEF_HIT_SMOKE
The NEF_HIT flags set the behavior of lara shots on the creature.
You can use only once NEF_HIT flag to set: blood, fragments, or smoke.
If you use NEF_HIT_DEFAULT the response to shots will be not changed. If you don't set any NEF_HIT ... flag, the shots will have no effect

NEF_NON_TARGET

Enemy becomes non targetable from lara and it will be immortal
NEF_NONE
Reserved, don't use. If you want omit to set NEF_ flags you must type only IGNORE in this field
NEF_ONLY_EXPLODE
The enemy will become like skeleton or mummy.
Apparently the common ammos will have no effect but explosive ammos will can destroy it.
NEF_SET_AS_CREATURE
Experimental. You can use a non enemy and non trap slot and to use this flag to transform it like an enemy. Pratically lara will can aim this  object and kill it (theorically but it's yet experimental)

NEF_SET_AS_MORTAL
This flag may be used with slot of semigod to transform then in mortal enemies.
When you set this flag the enemy will be killed also with common ammos.

TombFlags (TCF_...) field
This field is only for experiment. It corresponds to Flags field of slot structure. If you type wrong values in this field you can have many troubles.
I suggest to set this field to IGNORE and to use only NEF flags to modify behavior of object (other NEF flags will be added in short time)
Read the description of TCF_... constants to discover some known flag for this field.

If you wish to do experiments with TombFlags field, here there are some values for well-known slots:
	$3A7B = used for skeleton
	$327B = used for von croy
	$3072 = used for guide
	$267B = used for baddy 1
	$0472 = used for big scorpion

EXTRA_ flags field
------------------
For most slots you can set this field to IGNORE, anyway there are some slots that require an extra value to type in this field.

Each EXTRA_ constant works only for one specific SLOT.
See description of EXTRA_... values in Reference Panel of NG_Center in MNEMONIC_VALUES section to have a description of EXTRA_ possible values.

Damage1/2/3 fields
You can type none, one or more values for damage that current enemy cause to Lara.
The reason to accept a variable number of values is because some Slot don't damage lara, while other moveables can damage lara in different ways, like baddy can hurt lara unsing UZI or sword, and in this situation you may type different damage, one for each type of damage.
To know when an enemy has one, two, three or none damage, read the Damage Enemy list in Reference panel of NG_Center program.
Remark:
If you want modify only one damage value but no other, you can use IGNORE null value to specify "don't change this damage kind".
For example if you want change only granade damage for SAS but you don't want change the UZI damage you can type following line:

Enemy=SAS, IGNORE, IGNORE, IGNORE, 0, IGNORE, 1000
Above command don't change HP, NEF_ flags, TombFlags.
Set to 0 Extra and with IGNORE first damage (UZI), while change the granade damage to 1000 (immediate death of lara with burning)

Remark:
- In Damage1/2/3 fields you should type only positive values. In some circustances you can type negative values but the result will be to recharge Lara vitality instead of hearting her. It should be weird.
- Remember to follow the Range limits  you read in Damage Enemy list, because if you set a value outside of range you could get unpredictable results about damage (anyway no crash will happen)

## Equipment
Syntax: Equipment= SLOT item, Amount
Scope: To use in [Level] section
Max number of instances for level section: 100

Equipment force the amout of specific Slot item in inventory.
By default lara starts the first level, or a level after a ResetHub command, with the same equipment for medipack, weapons, binocular ect.
Using Equipment command you can force a different amout for each inventory item.

SLOT Item field
---------------
Type in this field a value to identify the slot corresponding to item you want set.
You find the full list of slot items in SLOT MOVEABLES of References panel of NG Center program.

Remark: not all slot names will be accepted, you should use only slot name with the word "Item" at right.

Amout field
-----------
In amout field you set the quantity of specific item in inventory.
For example if you want give no medipack to lara at start of current level you can use these two following commands:

Equipment= BIGMEDI_ITEM, 0
Equipment= SMALLMEDI_ITEM, 0

If the item you are using is a weapon you can add to the Amount field one or more LOAD_ flag to set the currently loaded ammo type and the further lasersight already mounted on the weapon.
Read the LOAD_... constant description in Mnemonic Constants section for more infos.

Remark: the range of value for Amount field is from -1 (unlimited) as min value and 127 like max value. If you pass over the 127 you'll get negative numbers with unpredictable results.

## FMV
Syntax: Fmv= NumberFmv, EnableEscape
Scope: To use in [Level] section.

NumberFmv is the number of fmv file.

EnableEscape may have value 0 or 1. Value 1 means: "permit to playet to skip movie using Escape key"

The fmv file name must have following syntax:

"FMV" + [Number] + [Extension]
For example, following names are correct for fmv files:

fmv4.wmv
fmv1.mpg
fmv43.avi

Differently it should be wrong to use following names:
fmv04.wmv     (there is a not meaningful "0" in front of "4")
fmv0.avi      (number start from "1")
fmv41      (Missing extension)
fmv5.bik    (".Bik" extension is not more supported
)

In script command "Fmv=" first number (NumberFmv) is number within the fmv file name.
For example working with file name "fmv3.wmv" you should write in script a command like:  Fmv=3,0

Remarks:

(1) Remember to set the wished extension with command "FMV=" used in [PCExtensions] section. (This "Fmv=" command has same name but don't confuse you, it is a different script command)

(2) Really Fmv= command is not a new next generation command, it was present also in standard script.exe. Anyway only using TRNG engine you can really show FMVs in game.

(3) TRNG engine will look for fmv file in following sub-folders:
- Current folder (pratically the folder where is stored also trng engine, usually named "TRLE")
- Further folder named "FMVs" in current (trle) folder  "trle\FMVs\... fmv files"
- Further folder named "Store" in current (trle) folder "trle\store\... fmv files

## FogRange
Syntax: FogRange=StartLimitDistanceFog, EndLimitDistanceFog
Scope: To use in [Level] section

Default values in tomb4:
StartLimitDistanceFog = 12 sectors
EndLimitDistanceFog = 20 sectors (or LevelFarView)

With FogRange= command you can change the limits for distance fog changing position and density of this fog.
Remark: the distance fog is visible only when VolumetricFx is disabled.

StartLimitDistanceFog field
----------------------------
The StartLimitDistanceFog value is the number of sectors from what the fog will be visibile. It used only for distance fog, i.e. the fog you get when VolumetricFx is disabled.

To increase the effect of fog you should type in this field negative numbers. Bigger is the absolute value you type and stronger is the fog density.

Remark: In spite of above speech it's not sure it's good using negative values because in this way you get also an abnormal intensity of fog color in transparent texture of level (like water) or objects (transparent or shining textures.
Pheraps in some circustances it's better reduce the EndLimitDistanceFog field when you want a distance fog with high density. In this case you can let the StartLimitDistanceFog field to 0, avoiding negative values.

EndLimitDistanceFog field
--------------------------
This value is usually set with same value of WorldFarView (or LevelFarView) anyway, if you want a very depth fog, you can reduce this value and this value will be the number of sectors where lara will be able too see.
The formula of fog distance works (about) in this way:
It begins a fog effect from StartLimitDistanceFog sector from Lara (really from the cam) and this fog effect increases its density upto reach the 100% density to  EndLimitDistanceFog sectors distance.
So if you set a very big EndLimitDistanceFog value the fog will be alwasy very light and you'll be able to see also the most far distance, while if you set a short value in EndLimitDistanceFog you'll see only upto the EndLimitDistanceFog sector distance.

See the fog_tutorial.zip file to have examples of fogrange settings with images.

## ForceBumpMapping
Syntax: ForceBumpMapping=ENABLED/DISABLED
Scope: To use in [Options] section

If you prepared bump map textures for your game, you can use this command to override settings in tomb4 setup and to force enabling of BumpMap in game.

Remark:
(1) Differntly by ForceVolumetricFx this command works only in [Options] section because it affectes all levels of your game in same moment.

## ForceVolumetricFX
Syntax: ForceVolumetricFX=ENABLED/DISABLED
Scope: To use in [Level] section

With ForceVolumetricFX command you can enable or disable volumtric fx setting in current level.
This command is useful if you want to mix fog bulbs with distance fog.

Remark:
(1) You can also enable or disable volumtric fx in dynamic way using some new flipeffects of NGLE in game.
This trick culd be interesting to "switch off" the fog bulb and "switch on" them when you want create an effect of gas or smoke after an explosion.
(2) If no ForceVolumetricFX command is used for current level then it will be used the setting set in tomb4 setup window from player.

## GlobalTrigger
Syntax: GlobalTrigger=IdGlobalTrigger, Flags Global Trigger (FGT_...), Global Trigger (GT_..), Parameter, IdConditionTriggerGroup, IdPerformTriggerGroup, IdOnFalseTriggerGroup
Scope: To use in [LevelSection]
Max number of instances for level section: 499

GlobTrigger allows to enable some trigger when some global event happens.
To understand the mean of global trigger we could start from the local triggers.
The local triggers are the common triggers we use in NGLE, placing HEAVY, PAD, or FLIPON, FLYBY triggers on same sector of level map.
These triggers are local own because they work only in located space, i.e. the sector where they are placed.
Differently, the global trigger work for whole level, and they will be enabled when some global event happens.

Arguments:

IdGlobalTrigger field
---------------------
In this field you type a progressive number, 1 for first globaltrigger of your levelsection, "2" for second and so on. It used only to identify this globaltrigger when you use a flipeffect to enabled/disable a globaltrigger.
Note: the valid range for IdGlobalTrigger field is 1 / 4999

Flags Global Trigger (FGT_...) field
------------------------------------
You can set some option about the behavior of GlobTrigger command.
Currently threre are only a FGT_ flag:

FGT_SINGLE_SHOT: This flag set the current global trigger for single execution. When the global trigger will be executed it will be disabled to avoid further executions.

Remark: if you don't wad set any FGT_ flags, you can type IGNORE in this field.

Global Trigger (GT_..) field
----------------------------
You type a constant value starting with GT_ prefix to choose the wished global trigger.

You can find the whole list of GT_ values in Reference panel of NG Center program.
Currently are available following global triggers:

GT_ENEMY_KILLED:You can specific in Parameter field the index of moveable to monitor. When the given moveable will be killed the global trigger will be started.
Remarks:
* You read the index of moveable when, in NGLE program, you perform a single mouse click on that item, the index will be showed in a yellow box.
* Theorically you can get a trigger activation when a creature dies also with local (common) trigger Switch. If you trigger enemy with a switch trigger and then add to this trigger some common trigge to enable doors, enemies ect, when the creature will die the trigger will be activated. Anyway the problem to use this method is that you have to cover a big surface with above triggers, because it's necessary the creature when he dies, he was on sectors with switch trigger. Differently using global trigger GT_ENEMY_KILLED the trigger will be activated indifferently by corrent position of enemy.

GT_SCREEN_TIMER_REACHED: You can start your global trigger when screen timer reach the supplied number of seconds. You have to type the number of seconds in following field named Parameter.
For example, to start your trigger when timer screen reach 240 seconds (4 minutes) you can type: "GlobalTrigger=1, IGNORE, GT_SCREEN_TIMER_REACHED, 240, ..."

GT_USED_INVENTORY_ITEM: With this global trigger you can detect when the player chooses a specifc item from inventory. You have to specify the slot id in following field  named Parameter.
For example to enable a global trigger when player select the QUEST_ITEM2 with slotid = 253, you can type "GlobalTrigger=1, IGNORE,  GT_USED_INVENTORY_ITEM , 253, ...".

Remarks:
*If you want remove the chosen item from inventory when global trigger has been engaged you have to insert in perform TriggerGroup the specific flipeffect "Inventory-Item. Remove <&
inventory-item from inventory". If you don't use this flipeffect the selected item will remain in inventory for further activations.

*Please, try to avoid to use QUEST_ITEM1 for your global trigger because QUEST_ITEM1 is used already for Detector activation.

Remark: there are many other global trigger GT_ values. You can find them and their description in Reference Panel of NG_Center program.

Parameter field
---------------
The value to type in this field depends by type of global trigger chosen in previous field "GT_ Global Trigger".
Read the description for specific global trigger to have more infos about value to type in Parameter field.

IdConditionTriggerGroup field
-----------------------------
This field is optional.
By default just the GT_ global trigger to set a global condition, anyway if you want perform the final IdPerformTriggerGroup, only when further conditions are true, you can create a conditional TriggerGroup script command and then set in this field the ID of that conditional triggergroup to perform another condition.
When you use a conditional TriggerGroup, the final condition will be true only when, other to be happened the event set in global trigger, also the conditional TriggerGroup is true.
If you don't want set additional conditions just you type IGNORE in this field.

IdPerformTriggerGroup field
---------------------------
In this field you type the ID (first value) of TriggerGrup command that will be performed when the global condition of GT_ trigger will be true.
Theorically in Perform TriggerGroup you can place also condition triggers, anyway these further condition triggers will be verified only if GT_ global trigger is true.

IdOnFalseTriggerGroup field
---------------------------
If you wish you can perform a triggergroup when the global condition is false, just you type in this field the ID of triggergroup to perform on false condition.

## Image
Syntax: Image=IdImageCommand, IdImageFile, ImageFlags (IF_...), EffectTime, AudioTrack, XPosition, YPosition, SizeX, SizeY
Scope: To use in [Level] section
Max number of instances for level section: 199

The Image command permits to set some data to be used for flipeffect trigger "Show Image ..."

IdImageCommand field
--------------------
In this field you have to type a progressive number that will be used to identifiy this Image command from some flipeffect trigger.
Valid range for IDs: 1 / 1999

IdImageFile field
-----------------
In this field you type the number of image to show.
This number (or ID) referes to image in the format: "IMAGE<ID>.BMP"
For example if you type 5 in this field, this mean you want show the image:

IMAGE5.bmp

TRng engine will look for images in TRLE\PIX folder, and, if it's missing in PIX folder, trng will look for image also in TRLE folder, i.e. in same foldr where is the tomb4.exe program.
Remark: Trlm2009 program will copy all the imageN.bmp images in TRLE\PIX folder.

ImageFlags (IF_...) field
--------------------------
You can specify some IF_ flags to set some features of image or about the mode to view it.
If you type IGNORE in this field the image will be showed  freezing the game and in the position and size you set in following fields of this command.

EffectTime field
----------------
This field works only for further effect you chose for this image.
If you don't apply any effect to image, this field will be ignored.
The time should be typed in tick frame units, the internal time of tomb4 engine.
One tick frame values 1/30 of second
When you set an effect for current image the EffectTime will inform the engine about how much frames will be necessary to complete the effect.
If you type little values the effect will appear fast, while typing big values as EffectTime the effect will be showed slowly.
If you type IGNORE in this field the default value is 30 ticks, i.e. one second to complete the effect.

AudioTrack field
----------------
If you want was performed an audio track at start of viewing of the image, you can type in this field a number between 0 and 255 to choose the wished audio track of AUDIO folder.
For example typing 35 it will be plyed the sound 035.wav

XPosition and YPosition fields
-------------------------------
In these two fields you type the origin, respect tomb raider screen, where placing your image.
It's important understand that the used units for position (and also size, in following fields) are NOT in pixel.
The reason is given by difficulty to kwow in advance the current tomb raider resolution.
When you create your level, and the script.dat, you cann't know if the game will be played on 640x480 screen or 1024x768 (or other...) and for this reason setting the origin of image or its size in pixel could affect many problems.
For this reason, the origin and size of image will be given in proportional units: named "micro units" (1/1000 of screen)
For example if you want locate the center of game screen the values will be XPosition = 500  and YPosition=500, since the whole width and height of game screen will have the reference value of 1000 x 1000 micro units.
Although this method could seem complicated it's the last way to have your image located with wished position and size on each screen risolution.

SizeX and SizeX fields
----------------------
In these two fields you set the size you want the image had once it has been drawn on game screen.
The values are in micro units (see descrition of XPosition, YPosition fields)
Probably you could have problem to set current ratio of image because also the height of scren  (about Y size) will be computed in micro units (1/1000)
For example if your source image is 100 x 100 pixels, i.e. it's a perfect square, you cann't set a target size 200 x 200 microunits, because the size y 200 computed on ScreenY should be bigger than computed on ScreenX, since the screen usually has a ratio of 1.3, i.e. the width of screen is 1.3 times bigger than height of screen.
To solve this problem you can use a new utility you find in Tools panel of NG_Center program: Get Screen Frames
With this utility you can, using the mouse, select a rectangular region on a ideal-tomb raider screen, and get as reply the four values XPos, YPos, SizeX and SizeY in micro units.

## ImportFile
Syntax: ImportFile= IdImport, PathFile, FileType (FTYPE_...) , ImportType (IMPORT_...)
Scope: To use in [Options] section
Max number of instances for level section: 199

With ImportFile command, you can insert in script.dat the binary image of any file.
You can have two advantages to use ImportFile:
* To have a fast starting of some files in game, like sounds. In this case you have to use a memory import to force TRNG to load directly in RAM memory your file
* To use script.dat like a container for some special files you would to be copied in some folder of target trle folder.
For example if you want have an "help" subfolder in trle folder with files like "start.htm" and some images, you can use the importfile in temporary mode to have this installation ad start of tomb4 program.

Arguments:

IdImport field
--------------
This progressive number will be used to locate this import slot. This location it's important when you use some new flipeffect to use your import file. For example if you import a file like "wagner.mp3" in memory mode you'll be able to play this audio file using flipeffect "Sound (CD) Play <&>imported file ..." and as "imported file" you should select the IDImport number.
Valid range for IDs: 1 / 1999

PathFile field
--------------
The path where find the file to import.

You cann't use absolute path like "c:\trle\audio\003.wav".
The path has to be always a relative path, using as root the current TRLE folder.
For example if you want import the wav file 003.wav, located in AUDIO folder, you'll type a path like this:

Audio\003.wav

Other example, if you want import the load.bmp file in TRLE folder, you type like path:

load.bmp

Remark: if you import the file in temporary mode (IMPORT_TEMPORARY value) the file will be exported (extracted) at first start of tomb4.exe, and the file will be written in some relative path you typed in script.txt.
This speech it's important when you place some of this files in sub-folders with non-standard names.
For example, if you create on your trle (used for level developping) the subfolder "Mp3" and you import a file from this subfolder in this way:

ImportFile=1, mp3\bach.mp3, FTYPE_SOUND, IMPORT_TEMPORARY

Above command will work in importing phase but it should be an error, because when trng will export the file, it will be extracted , creating the subfolder "mp3" and then copying the file with name "bach.mp3"
The problem is that the trng is not able to read audio tracks from folders different than AUDIO subfolders.
To avoid this problem you have two chances:

* Place always your sound files to import, in AUDIO folder of trle, toghether with other common adpmc wav files.

or...

* Import the file using a memory import.
In fact, when you import a file in IMPORT_MEMORY mode, it's not important the further folder name, since this file will be never really exported. The files imported in memory, will be loaded in their binary image, and they will be played directly from memory.
Pratically, in this case, the path name will be used only in importing phase, but the name will be ignored when the game will be played on target computer.

FileType (FTYPE_...) field
---------------------------
You can type a FTYPE_ value to inform TRNG the nature of this file.
Currently there only two values, but others will be added in future.

Read the descriptions for specific FTYPE_... values you find in MNEMONIC CONSTANTS list of Reference panel.

ImportType (IMPORT_...) field
-----------------------------
You can choose two import mode: IMPORT_MEMORY or IMPORT_TEMPORARY

IMPORT_MEMORY
-------------
In this case the file will be loaded direclty in memory at the begin of game.
The advantage is to start fastly this file, avoiding the latency necessary to access to hard disk to load a common disk file.
The difference it's visible when you start a CD track (like 104.wav) from disk, or from memory.
Reading the file from disk the game will be stopped for a short instant, while reading from memory (pre-load ad start from script.dat) the audio track will start immediatly with no slow-down.

Note: you can find any informations about this flag reading the description you find in MNEMONIC CONSTANT list in Reference panel.

IMPORT_TEMPORARY
----------------
In temporary mode the file will be stored in script.dat but it will be extracted on disk at start of the game.
This means  it will be used in game in traditional way: reading it from disk.
The advantage to use a file imported in temporary mode (rather than enclose the specific separated file) is that in this way you avoid to confuse the program Level Manager or some unexpert player in installation phase.
Files like .ogg will be ignored from level manager, and pheraps also from many beginners about level editor world.
Abotu the .mp3 files they will be handled from level manager but with the result they will be converted in .wav format, and pherpas this is not you want.
So, if you want play a sound file directly in .mp3 format, avoiding the automatic conversion in wav of TRLM, you can import this mp3 file, so level manager will be not able to see it in installation phase, but TRNG will export it in run-time, at start of game.

Note: you can find any informations about this flag reading the description you find in MNEMONIC CONSTANT list in Reference panel.

## ItemGroup
Syntax: ItemGroup=IdGroup, FirstIndexItem, Other indices for items
Scope: To Use in [Level] section
Max number of instances for level section: 99

ItemGroup permits to store a group of moveable or static indices, you get these indices from NGLE, when you click on some object you see its index showed as first number if a yellow box.
Remark: when you create a group of static indices you have to type the indices as negative numbers. For example if you wish create a group with static items with indices 12,43,55,56 you'll have to type them like: -12,-43,-55,-56

Once you set a list of items in some ItemGroup, you'll be able to perform some operations on all objects referenced by ItemGroup, with a single trigger (Flipeffects starting with description: "ItemGroup. ....")

The advantage to work with ItemGroup are two:

1) You can use many items like it was a single item, for example moving (or enabling, or rotating, or hiding) all items in same moment with a single trigger.
2) To perform some particular operation with doors or enemies sequence about activation.
See specific flipeffect triggers starting with "ItemGroup. " descriptive text.

Field description
-----------------

IdGroup field
-------------
This first field is not an object index but the ID used to locate this ItemGroup command script when you want perform some operation on it whereby a trigger.

You should start with "1" for first ItemGroup in your [Level] section, and the increase their IDs: 2, 3, 4 ect.
Valid range for id: 1 / 999

FirstIndexItem, Other indices for items fields
----------------------------------------------------------------
After the ID field, you type one or more indices of moveables read in NGLE.
You can type a max of 83 indices in each ItemGroup.

Remark: When you type indices of static items you have to type their indices as negative numbers.

## KeyPad
Syntax: KeyPad=AtStartAnimation, FrameStartPopUp, AtEndAnimation, ClickSound
Scope: to use in [Level] section

KeyPad command is optional and it used to change default animations and sounds for Keypad switch_type1 you find in ng.wad
You should use this command only if you want modify animations or sounds to enable when lara activates the keypad switch in game.

Parameters:
AtStartAnimation= Number of lara animation to show when Lara engages with Action command the keypad.
You don't wad any animation at start you can type in this field IGNORE.
Remark: the default value was 197

FrameStartPopUp=Used only if you set a valid value in above field AtStartAnimation. In this case the keypad image will be showed only when the start animation reachs this frame number. For example if you type 20 in this field, the keypad will be showed 20 frames after start animation was started.
Remarks:
* If you don't set any AtStartAnimation, this field will be ignored.
* If you set a valid AtStartAnimation value, you must set in FrameStartPopUp field a valid value, you cann't set IGNORE in this field.
AtEndAnimation= Number of animation to show when the keypad was closed.
Remarks:
*If you don't wad a final animation you can type IGNORE in this field.
*The AtEndAnimation will be showed only if player goes off from keypad with Enter/[*] key, differently, if he exits using Escape key no final animation will be performed.

ClickSound=Number of sound effect to play when player hits some key of keypad.
Remarks:
*If you don't wad any sound, type IGNORE in this field
*You can see the list of sound effect in Reference panel of NG_Center program.

## LaraStartPos
Syntax: LaraStartPos= RoomOfLSP, OcbOfLSP
scope: to use in [Level] section
With this command you can force the start position of Lara in same position of LARA_START_POS item of given room ad/or OCB

Note: main target of this command is to have two or more [Level] sections in the script, pointing to same tr4 file. In this situation you can set for each [level] section set a different start point for lara, giving to the player the feeeling that they were different levels.

RoomOfLSP field
---------------
The room index where you placed the LARA_START_POS item from where you wish lara begins the level.
You can omit this field, typing IGNORE, if you then type in next field a OCB value that is the only one in the level for LARA_START_POS items.

OcbOfLSP field
---------------
The OCB value of LARA_START_POS item from where lara will begin the level.
You can omit this value, typing IGNORE, if you had typed a valid Room number in previous field and in that room there is only one LARA_START_POS ita

## LevelFarView
Syntax: LevelFarView=NumberOfBlocks
Scope: To use in [Level] section

NumberOfBlocks is the number of sectors showed in game view.
Default value in old tomb4 was 20 sectors, this means that the mesh more far that 20 sectors from lara became black.

Valid range: Min=1  Max=127

Remark:
(1) The NumberOfBlocks you set with this command should be less or equal than value you set in [Options] section with "WorldFarView=" command, otherwise it will be ignored.
(2) For above remark you should insert in [Options] section a WordFarView= command with a big number and then set for different levels littler value with LevelFarView= in according with speed of game.
Remeber that not only the far view distance affects the speed of game but also (and overall) the number of meshes, transparent textures and moveables present in the level.
For this reason you could be forced to reduce the levelfarview for level with wide spaces and much moveables/meshes/transparent textures to enhance the speed, but in other level you could use a bigger levelfarview with no problem because the scene is more simple to draw for game engine.

## LogItem
Syntax: LogItem=FlagsLogItem (FLI_...), IndexOfItem
Scope: To use in [Options] section

This command works only when you enabled the diagnostic with the command:

Diagnostic=ENABLED

in [Options] section.

The LogItem permits to have on screen the same information you see for Lara but in this case for another moveable.

The main target of this command is to get the informations useful to type a TestPosition command to interact with some moveable to launch an own custom animation.

FlagsLogItem (FLI_...) field
----------------------------
You can type one or more FLI_ flags in this field.
Read the description of FLI_ flags in MNEMONIC LIST of Reference panel of NG_Center program.
Remark: if you don't wish set any flags you can type IGNORE in this field.

IndexOfItem field
-----------------
In this field you type the index of moveable to monitor.
You find this index in NGLE map, just you perform a left mouse click on some item to see in a yellow frame the index.

## MirrorEffect
Syntax: MirrorEffect= InFrontRoom, HiddenRoom, MirrorType (MIR_), Animating array + FMIR_ flags

Scope: To use in [Level] section
Max number of instances for level section: 100

MirrorEffect replaces old Mirror= command.
Really the old Mirror script command is yet working but you can't use new mirror type with old Mirror command.

Arguments:

InFrontRoom field
-----------------
The room number in front of mirror. This is a real room where Lara will be able to enter and move her.

HiddenRoom field
----------------
The room number place behing the mirror, where lara cann't enter.

MirrorType field
----------------
Specify a MIR_... value to set the mirror type.
You can see the list of MIR_ values in reference panel of NG center program.
Currently you can use following values:
MIR_WEST_WALL
West wall is pratically the setting used for old mirror.
The west is the position of mirror from lara position looking the room in ngle.

MIR_FLOOR
The mirror will be over floor of InFront room.

MIR_CEILING
Mirror on the ceiling of InFront room.
Remark: if you wish use a ceiling mirror it's advisable to use a room very low, otherwise lara will be not able to look other lara on the ceiling.

MIR_INVERSE_WEST
Inverse west is an horizontal mirror on the west side of InFront room, like the MIR_WEST_WALL but in inverse mirror lara and other object will be inverted like in an old level of tomb raider 1.

Animating array (+ FMIR_ flags) fields
--------------------------------------
From this field you can set one (zero) or more indices of animating present in InFrontRoom.
Using this array you get that the engine will replace all your animating in correct position and orientation in hidden room to simulate their mirror image in according with mirror type.

You can add to the index of the animating one or more FMIR_ flags.
These flags allow to adjust the reflex or the coordinates when the  pivot is not at the center of the object.
Read the FMIR_ constants in the MNEMONIC CONSTANTS section of the Reference panel for more infos.

Note you have to place a couple of animating: main animating you place in wished position in InFront room, and the clone of this animating you place in hidden room.
It's not necessary you place the clone animating in correct position but it's useful you place in same line (vertical or horizontal, in according with mirror type) to permit to engine to find the right clone for each main animating.
Pratically:
If you are using a vertical mirror, you have to place main animating in InFront room where you wish and with wished orienting.
Then you'll have to place another animating of same type in hidden room, in this situation it's important only you care to place the clone animating in same vertical line (i.e. same sector in 2d visual) than main animating, while it's not important the height of clone animating or its orientating because these settings will be set by the engine in run-time.

For horizontal mirror (like MIR_WEST_WALL) the speech in only a bit different. You'll have to place the clone animating in same sector row where is main animating.
See help file to have more infos about correct position for clone animating.

Remarks:
* Theorically you can type also indices of other moveables in Animating array, in this circustance the moveable will be dynamically update.
Anyway to do work moveables like enemies you'll have to place them in hidden room while the trigger to enable them will be in InFront room where is also the trigger to enable enemy in front of room.

* You can find the room numbers to type in InFrontRoom and HiddenRoom field, in room list of NGLE, anyway remeber to choose always the littler number when the numbers in round squares are different.
For example, if you read this text for you hidden (or InFrontRoom):

Mirror Hall (34:30)

In above case the real room number is "30" and you must type this value in MirrorEffect command.

## MultEnvCondition
Syntax: MultEnvCondition=IdMultCondition, ENV_ condition, DistanceForEnv, Extra field, array of tripled of {ENV_ Condition, DistanceForEnv, Extra field}
Scope: to use in [Level] section
Max number of instances for level section: 255

This command used only to store mutiple ENV_ (environment) condition for Animation script command.
When you want set two or more ENV conditions for your Animation command, you have to create a MultEnvCondition command with all ENV condition you wish, and then set the Id of current MultEnvCondition in field "Distance for env" of Animation command, while in field "Env Condition" of Animation command you'll type the value ENV_MULT_CONDITION to inform that the real condition have stored in MultEnvCondition command with Id = Distance for env of Animation command.

IdMultCondition field
---------------------
In this field you type simply a progressive number to identify this command. The you'll use same number in "Distance for env" field of Animation command to link it with Animation command.

Note: the valid range for IdMultCondition field is 1/2559 anyway, when you call it directly from a trigger in the level, the max valid ID is 1023

ENV_ condition field
--------------------
This field work in same way of "Env Condition" field of Animation command.
You type in this field an ENV_ condition value + (optionally) some ENV_POS_ flags.

See description of "Env condition field" in Animation command chapter for more infos.

Distance for env field
----------------------
This field work in same way of "Distance for Env" field of Animation command.
You type in this field a value to set the distance about current (previous) ENV_ condition.

See description of "Distance for Env field" in Animation command chapter for more infos.

Extra field
-----------
This field has same usage of ExtraSlot of animation command.
If you use an ENV condition requiring a value in ExtraSlot field, you can type this value in Extra field.

array of triples of {ENV_ Condition, DistanceForEnv, Extra} fields
-------------------------------------------------------------------
You can type many triples of fields "EnvCondition","DistanceForEnv", "Extra" until to a max of 125 couples.

Examples:

If you want set in your animation command two Env condition, like: ENV_CEILING_HEIGHT with height (distance env) = $300 (3 clicks) + other condition: ENV_HOLE_FLOOR_AT_LEFT with depth (distance env) = $400 (4 clicks) you can as first step create this MultEnvCondition command:

MultEnvCondition= 1, ENV_CEILING_HEIGHT, $300, IGNORE,  ENV_HOLE_FLOOR_AT_LEFT, $400, IGNORE

and then type in Animation command the reference for Id of above MultEnvCondition (1) and the ENV_ condition: ENV_MULT_CONDITION

For example:

Animation=447, KEY1_LEFT, IGNORE,IGNORE,ENV_MULT_CONDITION, 1,IGNORE,-445, -448

## NewSoundEngine
Syntax: NewSoundEngine=ENABLED/DISABLED
Scope: To use in [Option] section

The new sound engine should be the BASS 2.4 sound library created by Un4seen Developments Ltd.
This new engine is based on Bass.dll and it permits to have two channels for CD audio sound  (backgroup and foreground or channel1 and channel2) in same time.
Other features supported are:
- Play sound file different than .wav like .mp3 or .ogg files
- Fade out effect to close sweetly the previous sound before starting another cd track
- Dynamic change of frequency or volume in game wherey new flipeffect triggers.

ENABLED/DISABLED field
----------------------
By default the new sound engine is enabled, this means that, if you don't type any NewSoundEngine command in your script.dat, the new sound engine will be used.
For this reason you should use this command only if you want disable it with the command:

NewSoundEngine=DISABLED

Remarks:

* It's not necessary change the sound format when you use the new sound engine. The bass.dll is able to support the common ADPCM wav files you find in AUDIO folder.
* It's not advisable to add to your level file the bass.dll library, because TRNG uses a build-in bass.dll library and another bass.dll library could create conflicts other the fact it could be a wrong version number. (The bass.dll used by tomb_nextgeneration.dll is the version 2.4.0.1, and it's better to let this version of bass.dll)
The internal bass.dll library will be extracted at-fly when tomb4 will be started.

## Organizer
Syntax: Organizer=IdOrganizer, Flags Organizer (FO_..), Parameter, FirstTime, PerformFirstIdTriggerGroup, SecondTime, PerformSecondIdTriggerGroup, {Time and TriggerGroup Array }

Scope: to use in [Level] section
Max number of instances for level section: 499

Organizer permit to perform a list of trigger, organized in TriggerGroup script commands, at specific wished time.
For example you can create a list of trigger group, organized with Organizer command, to get to open a door and after 4 seconds, activate a baddy, after other 22 seconds enable a flipmap ect.

Parameters
----------

IdOrganizer  field
-------------------
Specify the id of current organizer. This number will be used to locate this organizer command with flipeffects to enable or disable organizer.
You should assign 1 at your first Organizer in that level section, and 2 for second organizer, ect.

Note: the valid range for IdOrganizer field is 1 / 4999

Flags Organizer (FO_..) field
-----------------------------
In this field you can add one or more FO_ constants to control the behavior of Organizer command.

You can get infos about FO_ constants from Reference panel of NG_center program, in _MNEMONIC CONSTANTS section.

Parameter field
---------------
Currently unused. It could be used in future versions.
You can type IGNORE in this field.

Couples of Time + IdTriggerGroup
---------------------------------
After Parameter field, you can two one or more couple of field to store:

Time to wait before starting ...

TriggerGroup

You can imaginate these appointment infos.
At this hour, remember to do this
at other hour, to do this other...

But in our case we'll use the seconds, of course, anyway the method is the same.

For example if we create this Organizer command:

Organizer=1,  IGNORE, IGNORE,
6, 1, 5, 2, 4, 3

We should read above data in following way:

After 6 seconds from enabling of Organizer, it will be performed the TriggerGroup
 1
After other 5 seconds, it will be performed the TriggerGroup 2
After other 4 seconds, it will be performed the TriggerGroup 3

We can understand better these data using a table:
<PRE>
Time     TRiggerGroup
-----------------------
6        TriggerGroup=1
5        TriggerGroup=2
4        TriggerGroup=3
------------------------
</PRE>

Remark: in time field you can type number up to 65535, i.e. about 18 hours.

## Parameters
Syntax: Parameters= Type of parameters (PARAM_...), IdParameterList, parameter array
Scope: to use in [Level] section

The Parameters command is a general purpose command to host parameters used by some trigger.
Since the trigger window is able to host only one or two parameters, when some trigger requires many parameters it could require a Parameters= command in script dat to get all needed parameters for its operations.

Type of parameters (PARAM_...) field
-------------------------------------
In this field you type a PARAM_ constant to describe for what trigger is  this Parameters command.
You can see the list of PARAM_ constants in Reference panel of NG Center program, in MNEMONIC CONSTANTS section.

IdParameterList field
---------------------
This is a progressive number to identify this "Parameters=" command script in trigger window of ngle.
You'll type 1, for your first Parameter command, 2 for second ect.
Remark: you can use same IdParameterList when you use two Parameters command with different PARAM_ constant. It's necessary set different ID only for Parameters with same PARAM_ constant.

Parameter array fields
----------------------
From this (third) parameter starts the parameters used by trigger.
The number and meaning of these parameters change in according with PARAM_ type.
You can have informations about different syntax looking the description of specific PARAM_ constant in Reference panel.

## Plugin
Syntax: Plugin= PluginId, PluginName, MainPluginSettings (MPS_...), DisableFeatureArray
Scope: To use in [Options] section

You have to use a Plugin command for each plugin you mean use.
This command has some differences respect other commands.

- You cann't type it into an include file but only in plane way in the [Options] section.

- You cann't use a variable defined with #define directive for its PluginID field.

- It could happen that some plugin command was inserted in your script from ng_center program, itself, only because it found a Plugin_SomeName.script file in ng_center folder.

Please, read carefully also description of PluginId field, because it's most important than other IDs of commands.

PluginId field
--------------
The id of plugin is very important about the exported (from ngle) triggers that you use in TriggerGroup commands.
When NGLE export a trigger (for the script), and that trigger was possessed of a plugin, in the three numbers exported (only in the first, really) there is hidden also the Id of that plugin.
The problem, is that the Id is only a number but to link this id to an effective plugin_Name.dll library, it's necessary some more work.
NGLE program, just a moment before exporting the trigger, will read the script.txt to discover the Id of plugin (this field, we are describing now) to set in exported trigger the right id, corresponding to the plugin name that owns that trigger.

A complicated matter...

Anyway you should remember only some rules:

- Type always in plane way the ID of plugin in Plugin= command, because the ability of NGLE program to parse the script is not so advanced like that of NG_Center. NGLE is not able to parse #define values or texts in #include files.

- Before exporting triggers from ngle, take care to have already saved on disk the script.txt ([save] or [build] buttons of ng_center will do this) otherwise ngle will read an old script.txt from disk.

- Do not change easily the IDs of plugin commands (you should do never that, you can only add a new Plugin= command for a plugin not yet loaded), because in this way the value inserted in previous triggergroups could not work since this value remained that of old plugin Id but it will be no updated to that new.

- When you take triggergroups commands from some friend, in the case these triggergroups used triggers from some plugin, you have to know wich plugin was, and what is its pluginId in the script of the friend, and then use the #define @plugin... directive to give these infos to ng_center in your script.
See also the description of #define command, in particular way the section about @plugin_name and #define "clear" attribute.

- Everytime you'll export a trigger from NGLE, handled by some plugin, NGLE, in its exporting report, will add also infos about #define @plugin directive to use to enclose the triggergroup that will host that trigger.
That directive is not really necessary when you mean use that trigger only for you, in your script, but it will become useful when you wish giving to someone that triggergroup. In this case the receiver will need of infos about the id of plugin stored in the triggers and for this reason it's better you keep that infos (about #define @plugin directive) also if only as comments closed to that triggergroup.
As just said, it's not necessary for your script, because NGLE will place in the trigger the same plugin id that it read from your script.txt file, so in that case it will work fine also omitting the #define @plugin directive.
Read infos about #define @plugin directive in [New Script commands] section of reference panel of NG_Center.

PluginName field
----------------
In this field you type the name of given plugin.
Since this is a text, you have to type same text also in the strings, of course.
The name should not contain any extension.
The extensions are like ".dll", ".script" and others that some plugin files could have.
For instance for plugin "Plugin_Marble.dll" you'll type as name only:
Plugin_Marble

MainPluginSettings (MPS_...) field
----------------------------------
In this field you can type MPS_ flags to customize some basic features of the given plugin.
The description of MPS_ values you can find, as usual, in Mnemonic Constants list of Reference panel, anwyay you should take care to use only the MPS_ flags thought for that plugin and not for others.
Each author of plugin will set his MPS_ flags, and if you have, for instance, a plugin named Plugin_Alfa, in the Plugin= script command for this plugin you should type only MPS_ flags handled by Plugin_Alfa.
In the Mnemonic constants list, when you click on some item, in the description (green) box, you'll read, at top-left corner, the plugin name in bracked parenthesis that owns that constant.
Another way to locate quickly these constnats, is to go in [Plugin] panel of NG_Center, choose from the combo box the "Plugin_Alfa" name (in our example) and then see ONLY the constants of that plugin.

The only exception to this rule (about the owner) is for the MPS_DISABLE value, that is from trng, and it disable (temporarily, I suppose) that plugin to verify if some problem was affected by it.

DisableFeatureArray fields
--------------------------
DisableFeatureArray is an array, and this means that from this field you can type one, more (or no one) values in same format.
The format for each item of this array is a sum between a CODE_ constant + one operand value.
The generic use of this fields is to disable some changes (about standard trng features) that didn't please you.

See the description of CODE_ constants for more infos.

## PreserveInventory
Syntax: PreserveInventory= ENABLED
Scope: to use in [Level] section

PreserveInventory command may be used when you use a ResetHUB= command in same [Level] section.
With ResetHUB command you set the level number that will be started from current level with some FINISH trigger.
If  you want reload lara's meshes (or change vheicle) you have to use ResetHUB. Ok, this is about old ResetHub command.
The problem with ResetHUB is the pickup items will be cleared from ResetHUB command.
If you want to use a ResetHUB and, in same time, to preserve the pick up items picked up in previous levels just you add in [Level] section the command:

PreserveInventory=ENABLED

## Rain
Syntax: Rain= RAIN_... constant
Scope: To use in [Level] section

You can have rain room in current level only if in current [Level] section is present the command "Rain=", otherwise the setting for Raing in ngle will be ignored.

Argument:
RAIN_ Constant
You can choose one of following constants:
RAIN_DISABLED
It's the same that don't insert any Rain= command in script

RAIN_SINGLE_ROOMS
With this setting the rain will be showed only in specific rooms signed as "Rain" in ngle and with outside status.

RAIN_ALL_OUTSIDE
If you want have rain in all outside rooms without set "Rain" button for each room in ngle, you have to use rain_all_outside setting.

Remarks:
(1) If you use RAIN_ALL_OUTSIDE setting the trick to set the rain in intensity is simply to enable "Rain" button in first room that will be visited by lara and set in Water Intensity field (at right of multistate button water/rain/snow...) the intensity (from 1 to 4)
In this way the intensity for all level will be that you chose in that room.
Anyway if you set another "rain" room with different intensity, when lara will enter in that room the rain intensity will change like you set.

(2) If you don't mean to use rain rooms in current level it's better use command "Rain=RAIN_DISABLED" (or omit to type Rain= command) to inform trng engine to avoid a lot of computes to locate further rain rooms.

## SavegamePanel
Syntax: SavegamePanel=SavegamePanelFlags (SPF_...), BackGroundImageId, NumberOfSave, NumberOfVisibleSave, SavegamePanelLayout (SPL_...), InfoFormatString, IdListWindowsFont, IdInfoWindowsFont, InTitleWindowsFont
Scope: To use in [Level] section

SavegamePanel command allows to customize a new savegame panel to load or save the game.
If you type this command in a [Level] section the old load/save screen will be replaced by this new savegame panel.
The most advantage to use this new savegame panel is to be able to view the inner image of each savegame to get more easy to player the choice of the correct savegame to load (or to avoid to overwrite some important savegame in saving operation).

For above reason it's strongly suggested to use this new savegame panel togheter with the customize to enable the saving of image inside of screenshot. You can realize this target adding following row:

Customize=CUST_INNER_SCREENSHOT,
QSF_SIZE_320x240+QSF_TRUE_COLOR

Remark: the QSF_ flags could be also different, of course, the only important fact is to add a Customize=CUST_INNER_SCREENSHOT command in your level section to enable the creation of screenshot image in each savegame of your adventure.

Another news in savegame panel is to be able to customize the number of savegames to handle (in old tomb this number was 15 savegames) and to choose the infos to show when a savegame has been selected.

I suggest to test and study the Savegame_panel.zip demo to understand the correct usage of this new trng feature. You can find the demo sample on http://www.trlevelmanager.eu/ng.htm or on skribblerz website.

SavegamePanelFlags (SPF_...) field
----------------------------------
In this field you can add two or more SPF_ flags to customize some feature of SavegamePanel.
If you don't wish any flags you can type IGNORE in this field.

See the Reference Panel of NG_Center to have more infos about description of SPF_ flags.

BackGroundImageId field
------------------------
It's necessary supply always a background image for savegame panel.
In this field you type the number of image that will be present in PIX subfolder.
For example if you want use as background image the file "Image8.bmp" you'll have to type in this field the value 8.

NumberOfSave field
------------------
In this field you type the number of savegames that savegame panel will be able to handle.
For example in old tomb4 this number was 15 (from savegame.0 to savegame.14).
The max number you can set is 100 and in this case in trle folder could there be upto 100 savegames from savegame.0 to savegame.99
Truly I suggest to don't select a too big number of savegames because the search for players could become difficultous with so much savegames.
Other above reason, you could create troubles to users of old level manager (but trlm2009 should be able to manage any number of savegames)
A reasonable value could be 30 but you can also let the standard number of savegames, in this case just you type IGNORE in this field.

NumberOfVisibleSave field
-------------------------
This field could seem difficultous to understand.
The number of visible savegames is different than the previous field "NumberOfSave".
When you set a big number of savegames in "NumberOfSave" field, it's possible (very probable) that it was not possible show in same moment all this long list of savegame on screen. To solve this problem trng is able to show only a short number of savegame on screen and then, to allow the scrolling of the list, to show another "page" of savegames.

The NumberOfVisibleSave field is pratically the number os savegames showed for each visible page on screen.

For example we know that in old savegame panel was showed only 15 savegames. Now if we want handle (for example) 50 savegame is obviuos that is not possible store all these 50 savegame rows in same screen, so you have to select a number of visible savegames, for example 15, and then trng will show at start first 15 savegames, then, when player hit DOWN key, trng will show next group of 15 savegame (from savegame.15 to savegame.29) and go on.

Remark: in the reality trng will scroll in this way only if you set a specific SPF_ flag, otherwise it will scroll slowly the list a row at once.

Note: If you type IGNORE in this field trng will consider that all "NumberOfSave" will be showed on screen in same moment, i.e. the "NumberOfVisibleSave" and "NumberOfSave" fields will have same value.

The general rule to follow is this: first you set fonts, image, and layout for SavegamePanel and try in game the result to see how much savegame could be showed on screen in same time, then you set this number in "NumberOfVisibleSave" field.

WARNING: If you don't set a correct number in NumberOfVisibleSave field you get only a big mess, because trng will not verify if really all required savegames will be fit on screen.

SavegamePanelLayout (SPL_...) field
-----------------------------------
You can type a single SPL_ value to set the position of different frame on the screen.
You should read the description of single SPL_ flags you find in Reference panel of NG_Center.

Pratically you have to choose where place the image of screenshot (left , right or at center) and the list of savegames.
There is also another (optional) frame: the info frame.
In info frame will be showed more infos about currently selected savegames.
For example you can wish to show the number of secrets, or the weapons availables in that selected savegame.
Seen description of next field about this chance.

InfoFormatString field
----------------------
If you use a SavegamePanelLayout with Info frame, you have to type in InfoFormatString field the string used to format the information to show for the selected savegame.
The info frame is a zone of screen where it will be showed some informations about the savegame currently selected.

I suggest to use the trick of external ng strings to store the format text, i.e. you type the text in a text file, save it in SCRIPT folder, and then link this text in extra ng strings using "@" + "name.txt".
For example if you type the format text in a file named "Info_savegames.txt" you'll add a new Extra NG strings and type in it the text "@Info_savegames.txt"

About what type in this format text you can type exactly what you want see on screen, but remembering to use special place-folder to signal a specific value in savegame.
All placefolders are enclosed in round parentheis "()" and have fixed names that trng will be able to recognize and replace with real values read from savegame.
For example if you type in info format text the following rows:

Found Secrets        (SECRETS)
Large Medipacks      (L-PACKS)
Small Medipacks      (S-PACKS)

then in game, when player will select a savegame from list, on the screen will be showed the effective values in that savegame, so, for example, we could read:

Found Secrets        5
Large Medipacks      3
Small Medipacks      12

Remark: if you want have a good alignment it's advisable select for info text a windows font with fixed width, like "courier" or "new courier" because otherwise with proportional fonts (like "Arial") you could see a bad alignment:

Found Secrets     5
Large Medipacks      3
Small Medipacks    12

The full list of place-folders is the following:

(LEVEL-NAME) The name of the level in the savegame
(SAVE-NUMBER) The progressive inner number of savegame
(SECRETS)  The found secrets
(L-PACKS)  The number of Large Medipacks
(S-PACKS)  The number of Small Medipacks
(FLARES)   The number of flares
(WEAPONS)  Show a list of the weapons present in this savegame. See (1) note in following remark section.
(GAME-TIME) The game time. See (2) note in following remark section
(DATE-TIME) The date about when current savegame has been created or last time it has been changed. The format is DD/MM/YYYY (HH:MM:SS)
(KM-DISTANCE) Number of Km of distance
(METERS-DISTANCE) Number of remaing meters of distance (See (3)
note)

Remarks:
(1) Since the weapon list could be long, it's better let a whole row to host this value using a format text like this:
----- Example of info format text ----
Available Weapons:
(WEAPONS)
---- end info format text sample -----
In this way the list will have all row of info frame to be hosted

(2) About game-time, usually this value will be showed in savegame list, as in default savegame panel, anyway if you wish you can remove the game time from savegame list with the SPF_NO_TIME_IN_LIST flag, and then add this value in specific description of selected savegame with place folder (GAME-TIME). The advantage of this method is to be able to host the savegame list in a short frame about the width.

(3) The reason to have two different values for KM and meters is to allow you to type the words "km" and "meters" in your language, or in different language one for each language file.
For example you can type in info format text:

Distanza: (KM-DISTANCE) chilometri e (METERS-DISTANCE) metri.

Above example show the description of distance in italian,  using the words "chilometri" and "metri" instead of "kilometres" and "meters".
Then in game we could read:

Distanza: 2 chilometri e 43 metri

IdListWindowsFont field
------------------------
Type the id of WindowsFont=IdNumber command where you set the style of text used for the savegame list

Remark: it's important you place the WindowsFont command first of SavegamePanel command that use it.

IdInfoWindowsFont field
------------------------
Type the id of WindowsFont=IdNumber command where you set the style of text used for the infos showed for selected savegame
Since the infos could be very much you should use a little font to be sure to host all further info texts.

Remark: it's important you place the WindowsFont command first of SavegamePanel command that use it.

InTitleWindowsFont field
------------------------
Type the id of WindowsFont=IdNumber command where you set the style of text used to show the title of panel, i.e. the text "Save Game" or "Load Game" in according with current language.

Remark: it's important you place the WindowsFont command first of SavegamePanel command that use it.

## Settings
Syntax: Settings= SET_ constants
Scope: To use in [Options] section

This command will be placed in [Options] section to set some gloal settings.

Arguments:

SET_ constants field
You can type one or more SET_ values separated by plus (+) sign.
Currently there are followin SET_ constants:

SET_DISABLE_CHEATS
Disable all hidden cheats to skip level, get infinite weapons or items.
Remark: to disable flycheat you have to use old command FlyCheat = DISABLED

SET_BLIND_SAVEGAMES
If you want forbid players of your game to use savegame editor to change savegames, just you add this flag in Settings command.
Any attempt of players to modify savegames will cause immediate crash at reload.

SET_CRYPT_SCRIPT
Setting this flag the script.dat file will be crypted to forbid decompilation with some utilities.
If you set some security issues, like SET_DISABLE_CHEATS, it's advisable set also crypting of script.dat, otherwise some guy could simply decompile the script.dat and recompile it removing the SET_DISABLE_CHEATS flag.

SET_PERFORM_FROM_CD
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

3) When you have in TRLE folder all level files (.dat , .tr4
) necessary to play your game, you can save whole TRLE folder, including also the name of TRLE folder, in CD image to burn on cd support.

4) Before burning image you should add to image also following files, you find in Extra_NG_File.zip:

START.exe
autorun.inf

5) Now you can burn the CD and, at end, you should get following list browsing the new created CD:

START.exe
autorun.info
TRLE   (Folder)

Remark: the start.exe and autorun.inf files should be in root of CD, i.e. outside of TRLE folder

6) Now just inserting this cd in some CD reader and the game will be started and played byself, without need of installation on local disk (of target computer) of TRLE or other files or programs.

## ShowLaraInTitle
Syntax: ShowLaraInTitle=ENABLED/DISABLED
Scope: to use in [Options] section

By default lara is not visible in Title level but using this command:
ShowLaraInTitle=ENABLED in [Options] section, lara will be drawn.

## Snow
Syntax: Snow= SNOW_ constant
Scope: To use in [Level] section

The snow will be showed in current level only if in current [Level] section is present the command "Snow=SNOW_SINGLE_ROOM"  or "Snow=SNOW_ALL_OUTSIDE"

SNOW_DISABLED
This settings is the same to omit to insert Snow= command. The futher rooms labeled as "Snow" room in ngle will be ignored, i.e. no snow effect will be showed.

SNOW_SINGLE_ROOM
Only the room with "Snow" label in ngle and with [O] (outside) status will have the snow in game.
Each room will have own snow intensity read from field Water Intensity at right of multistate button Water/Rain/Snow ...

SNOW_ALL_OUTSIDE
The snow will be showed in all outside room, ignoring multistate button. This means that also a room without "snow" button will have the snow if it is different than Water room and it has the [O] outside status.

Remarks:
(1) If you use SNOW_ALL_OUTSIDE setting, and you want set the intensity for snow in all level, you can set a single room with "Snow" attribute in ngle and set in that room in water intensity field the value for snow intensity. That value will be used for whole level.

(2) If you don't mean to use snow rooms in current level it's better use command "Snow=SNOW_DISABLED" to inform trng engine to avoid a lot of computes to locate further snow rooms.

## SoundSettings
Syntax: SoundSettings=Sound Quality (SQ_...), MusicVolume, SoundEffectVolume
Scope: to use in [Options] section

SoundSettings allows to force the music quality, the music and the sound effects (SFX) volumes.

Sound Quality field
-------------------
You can choose three different qualities for music:

SQ_LOW_QUALITY   (11025 hz)
SQ_MEDIUM_QUALITY (22050 hz)
SQ_HIGH_QUALITY (44100 hz)

If you type SCRIPT_IGNORE the quality will be not changed and it will be used the value currently set in game.

MusicVolume field
-----------------
You can type values between 0 (full silent) and 100 (max volume)

if you type SCRIPT_IGNORE in this field, the music volume will be not changed and it will be used the value currently set in game.

SoundEffectVolume field
-----------------------
You can type values between 0 (full silent) and 100 (max volume)

if you type SCRIPT_IGNORE in this field, the SFX volume will be not changed and it will be used the value currently set in game.

## StandBy
Syntax: StandBy=  IdStandBY, Type StandBy (TSB_..), WaitTime, Flags StandBy (FSB_...), Text, NumTexts, AudioTrack, VAngle, RotateSpeed, Distance, IdTriggerGroupBegin, IdTriggerGroupEnd
Scope: to use in [Level] section
Max number of instances for level section: 49

The StandBy command allows to enable an automatic pause-mode in game.
The stand-by will begin when the player for some time (that you set in WaitTime field) don't use any game command.
You can choose what it will happen in game: the main effect is a matrix effect, where the camera turn around to Lara. You can add texts, and new audio track to underline the stand-by mode.

IdStandBY field
---------------
Theorically the stand-by starts byself when there are the correct conditions, anyway it has been added also a flipeffect to execute a standby-mode. Thanks to this flipeffect you can use the settings of StandBy command to perform in game a camera effect.
To select the wished StandBy you choose the corresponding IdStandBY value.
Anyway, when you use StandBy in native manner, it's necessary that the IdStandBy value was = 1, in fact, only the StandBy with ID = 1, will be started when there are correct conditions.
Valid Range for IDs: 1 / 499

StandBy (TSB_..) field
----------------------
In this field you type a single TSB_ value to set the main effect for stand-by.
See the description of TSB_ constants in Reference panel of NG_Center program.

WaitTime field
--------------
Set the time with no input to activate the stand-by.
You have to type the number of seconds.

Flags StandBy (FSB_...) field
-----------------------------
You can set many FSB_ flags to customize the main method set with TSB_ value.
See the description o FSB_ constants in Reference Panel.
Remark: you can type IGNORE in this field to omit any FSB_ flag

Text field
----------
You can type the text that will be printed on screen when the stand-by is on.
For example you could show a text like "Hit a Key To continue".

NumTexts field
--------------
Set the number of texts to show during the stand-by mode.
Usually it will be "1", to show the single string you typed in previous "text" field, anyway you could show different strings in sequence.
When you type a number greater than "1", the game will show the string you typed in "Text" field, and, after 4 seconds, the string immediatly following the supplied text, in the NG Extra strings list.
For example if you typed in [NG Strings] section of the script the following strings:

23: Pause - Hit a Key
24: The longest Night
25: Created by Paolone

And you wish that above three strings were showed on screen in sequence, you'll type in "Text" field  the first string, followed in "NumTexts" field from the number of strings to show ("3"
):

StandBy=  .... , Pause - Hit a Key, 3, ....

AudioTrack field
----------------
If you wish have a new audio track during the stand-by mode you can type in this the number of CD track to play.
Differently, if you don't wish any furhter audio you can type IGNORE in this field.

VAngle field
------------
This field set if the camera is looking lara from downstair or upstair.
Positive values place the camera down than lara and therefore the camera will look upstairs, while negative values move the camera upper than lara and since lara will be seen from upstairs.
With 0 value, the camera will have same height than Lara.
The valid range of values is about from -16384 to +16384 but in the reality it's better avoid to reach exaactly the 16384 abs value.
It's advisable use zero or negative values for this field becuse if you use bigger positive values the camera should go under the floor.
Remark: 16384 = 90 degree, 8192 = 45 degrees.

You can type IGNORE in this field to let default value.

RotateSpeed field
------------------
The speed of turning around lara has like units the degrees, where to compute a whole revolution the value will be 65536 ($10000).
Since the value you type in this field will be added to current angle 30 times for seconds you can compute the value to type to reach the wished target.
For example if you wish the camera perform a quarter of revolution (90 degrees) in one second you should type 16384 (i.e. 90 degrees) / 30 (i.e. one second) = 546.
I suggest lower speed in particular way then the Distance is large, while when the distance is little you can increase the speed.

If you type too high value for speed the players will be seasick watching your stand-by.

Distance field
--------------
This is the distance between the camera (is looking lara) and Lara.
One sector is 1024 units, one click = 256 units.
The reasonable range of values could be 256 / 4096, anyway I suggest little values because when you choose to high values (higher than 2048) the camera could have problems turning around lara, entering in collision with walls.

IdTriggerGroupBegin field
-------------------------
The StandBy command supplies to you many features, about camera effects, bakcground audio and printing of texts, anyway if you wish customize furtherly your stand-by you could use this field to set a TriggerGroup to perform when the stand-by starts. In the triggergroup you could for example call an organizer, where scrolling or sliding texts will be showed, or some triggers that begin a flyby sequence.
If you don't want use a triggergroup type IGNORE in this field.

IdTriggerGroupEnd field
-----------------------
When you set a TriggerGroup at start of StandBy mode it's probable you wish also another triggergroup to remove from screen the effect added with the beginning trigger group.
For example if you had a long text to screen with infinite time you could use the End TriggerGroup to clear the screen removing that text.

## StaticMIP
Syntax: StaticMIP= MainStaticSlot, BStaticLimit, BStaticSlot, CStaticLimit, CStaticSlot
Scope: to use in [Level] or [Title] section

The target of this command is to get a faster view and better fps (frame per second) in game, when there are wide scenes with many static items.
Using StaticMip command and creating reduced low quality of your most used static items, to draw when they are very far from source view, you'll get huge improvement in speed.

With this command you can require to trng engine, to show statics with different quality respect the given Main Static, in according with current distance between the static and current source cam (usually in Lara's position).
This skill is very alike than AnimatingMIP command, of course, anyway in this case you can set two different limits for a total of three different static kinds to draw in game.

MainStaticSlot field
---------------------
In this field you type the slot number of main static.
You find the static slots in "STATIC indices list" of NG_Center's reference panel.
The main static is the static object you placed in the level map and it should be that with highest quality, of course.

BStaticLimit field
------------------
In this field you type the min distance, in sectors (1024 game units), between source view and the static, to draw the BStaticSlot replacing the MainStaticSlot.
Pratically, when the distance will be even or greater than BStaticLimit (but less than CStaticLimit), it will be drawn the static with BStaticSlot instead of that of MainStaticSlot
You should create in BStaticSlot and raw copy of MainStaticSlot, with same size and (about) look but using less meshes and less (or no) transparent textures.
Rememember that the transparent textures create a big usage of cpu (or GPU) time.

BStaticSlot field
-----------------
This is the static slot to draw when the distance, between source view and the static item, is greater than BStaticLimit and less than CStaticLimit.
Please, try to understand than, this change of static to draw, doesn't affect the slot item set in the level or the static OCB or its light. Only the mesh of BStaticSlot it will be used but all other features it will by yet that MainStaticSlot.
If you type IGNORE in this field, it means that you wish the drawing of MainStaticSlot was skipped when the distance is greater than BStaticLimit

CStaticLimit field
------------------
This field work alike the BStaticLimit but in this case it will affect the drawing of following CStaticSlot.
The CStaticLimit should be always greater than BStaticLimit, of course.
If you type IGNORE in this field, this means that you don't want supply another raw copy of MainStaticSlot.
When you type 	IGNORE as CStaticLimit the next CStaticSlot will be ignored.

CStaticSlot field
------------------
The slot of static you type in this field it will be drawn only when the distance is bigger or even than CStaticLimit distance.
The copy of MainStaticSlot you set in CStaticSlot, should be very very poor, like a simple shadow, and you should a very far distance in CStaticLimit field.
If you type IGNORE in this field this means you wish skipping the drawing of MainStaticSlot when the distance is bigger or even than CStaticLimit field.

## Switch
Syntax: Switch= SwitchId, VariablePlaceFolder, FlagsSwitch (SWT_....), Trigger Group Indices
Scope: To use in [Level] section.
Max number of instances for level section: 127

Introduction
------------
The Switch command permits to choose between a list of TriggerGroup indices in according with the value in a variabile.
In this way you can for example, ask a number to player with KeyPad and then perform different trigger group in according with the number chose by the player.
Remark: theorically you could get same result with a triggergroup filled with a long list of condition trigger and ELSE tgroup_ flags, but using the switch you can realize this target in easier and faster way,

SwitchId field
--------------
This is the ID you can choose in the flipeffect to perform a Switch command.
You have to type different progressive numbers for the IDs in same [Level] section.
Valid range for IDs: 1 / 1279

VariablePlaceFolder field
-------------------------
In this field you type the Place Folder to identify a specific TRNG Variable.
You can see the full list of Place Folders in [Reference] panel of NG_Center program, in the section named: "VARIABLE PLACEFOLDERS".
For example if you want use the variable "Local Short Beta1" you'll type the placefolder: #0052

Remark: if you type IGNORE in this field the Switch will use the variable "Last Input Number", since it is the variable that receives the value typed with KeyPad object.

FlagsSwitch (SWT_....) field
----------------------------
In this field you can type one or more SWT_ constants to affect the working mode of Switch command.
If you don't wish using flags you can type IGNORE in this field.
See the list of SWT_ constants in [Reference] panel of NG_Center program.

Trigger Group Indices fields
----------------------------
From this field you can type indices of TriggerGroup commands.
The first triggergroup will be performed when the variable will be "1", the second when it will be "2", ect.
If you type IGNORE in some field this means there will be no triggergroup for that number.
For example if you don't want perform any triggergroup when the variabile is "2" you can type a list like this:
 ..., 4, IGNORE, 7, 8
In this way when te variable will "1" it will be performed the TriggerGroup=4.
When the variable is "2" nothing will be performed.
When the variable is "3" will be performed the TriggerGroup=7
and when the variable is "4" will be performed the TriggerGroup=8

You can place upto 120 indices in this array.

## TestPosition
Syntax: TestPosition= IdTestPosition, Flags (TPOS_...), Slot Moveable, XDistanceMin, XDistanceMax, YDistanceMin, YDistanceMax, ZDistanceMin, ZDistanceMax,  HOrientDiffMin, HOrientDiffMax, VOrientDiffMin, VOrientDiffMax, ROrientDiffMin, ROrientDiffMax

Scope: To use in [Level] section
Max number of instances for level section: 99

With TestPosition you can set all data necessary to verify if lara has a correct position with respect to other item (all moveables with the given Slot)
Then you'll have to type the IdTestPosition also in Extra field of Animation= or MultEnvConditon= script commands to transform it in an effective condition.

The TestPosition= is a bit complicated command, for this reason I create a zip file containing some stuff to understand better how to use TestPosition.
The zip file is "AnimationTestPosition.zip" and it contains some images necessary to understand following explanations.
Remark: in above zip file there is also a project + script to test the env codition with TestPosition command.

Descriptions of TestPosition fields:

IdTestPosition field
--------------------
In this field you type a Identificative number to call this TestPosition command from other commands like Animation= or MultEnvConditions= (typing this id in Extra field)

If you type many TestPosition commands in same [Level] section, each of them should have a different (progressive) Id number.
Valid range for IDs: 1 / 999

Flags (TPOS_...) field
----------------------
You can type one or more TPOS_ constant values in this field to set some special features.
If you don't wish use any TPOS_ flag type IGNORE in this field.

See descriptions of TPOS_ values in Reference panel of NG_Center in MNEMONIC CONSTANTS list.

Slot Moveable field
-------------------
In this field you type the slot (the number or the name) of items you mean check.
Remark: if you add in Flags field the value TPOS_TEST_ITEM_INDEX, in the "Slot Moveable" field  you have to type directly the index of item to check.
See descrition of TPOS_TEST_ITEM_INDEX flag for more infos.

XDistanceMin and XDistanceMax
 fields
------------------------------------
In these two field you have to type the min and max valid distance from X origin of lara and X origin of item to test.
To understand what is the X axis watch the image "ExTestPosition_Axis.jpg" you find in
"AnimationTestPosition.zip" zip file.
In light blue color you see the X axis and its orientation.
The effective difference you compare with these two field is given by X_Lara_origin - X_Item_Origin.
For this reason, normally, the range you type are always a negative value for XDistanceMin and a positive value for XDistanceMax

For exaple if you choose:
XDistanceMin = -256
XDistanceMax = +256

this means you accept a tollerance of one click (a sector is 1024 units, so a click is 256 units) of difference between lara and item on X axis.
Other example, if you set 0 for min and max X distance this means you require that lara is nose vs nose with other item (for example a ninja) and neither a pixel of difference should exist.
Anyway, choosing  0 and 0 should be very stupid, of course, because you'll have always need of some tollerance.
Last example for X difference range:
If the item to test is a coffe cup, and the cup in game give a difference on X axis of +100 , this means that the cup should be about closed to left hand of lara. While if the difference was -100 the cup should be near to right hand of Lara.

When you compute the valid range for X axis (or others) you should keep in mind the concept of "Relative axis" (see next paragraph) used in TestPosition comparation.

RELATIVE AXIS description

A complication, to understand the comparations of testpostion, is that the axis are relative to current orientation of lara, while usually, in 3d world we think to have fixed x,y,z coordinates.
When you compute the difference for x relative axis for example the engine fixes the origin of 3d world to origin of lara, and it will (ideally) rotate the 3d world to have facing of lara (where she is looking) in same direction of wished axis

For above reason, if you set as Z difference (Z axis, see the ExTestPosition_Axis.jpg picture) as 256 units and the condition is true, this means that, in front of lara, the other item will be to 1 click of distance in front, indifferently if in 3d world lara is at the left or the right (watching the game screen) of other item, because the position will be changed before performing the comparation to have lara at 0,0,0 and looking in exact axis direction, i.e. whre lara is looing it will become the new Z axis direction.
The speech is complicated but using the examples of AnimationTestPosition.zip file you can understand it, and you'll discover that it works very fine.
Note: this method has been created by Eidos programmers and not by me. I'm using the specific procedure used by default tomb to test the position of items with them lara should interact.

YDistanceMin and YDistanceMax fields
-------------------------------------
This couple of values set the range for distance on Y axis.
See the image "ExTestPosition_Axis.jpg", the y axis has white color.

Many description I given to explain the X difference range are good also for Y range, anyway in this case there is further complication because while the X and Z origin of lara is always the same, at half of her body, the Y origin of lara changes in according with current animation.
I suggest to use the diagnostic to understand what is the Y origin of lara in animation from what you mean start to launch your custom animation.
Pratically you should type in [Options] section of script.txt the command:

Diagnostic= ENABLED

So in game you'll see many value about lara.
Look the "Cy=..." value.
To understand better what it means, you should build a sample project where the floor is to 0 clicks, then you should extrude some sector to have 4 clicks of height, and also some hole in the floor of 4 clicks.
The targert of these test will be to see what is the Cy of lara when she is on floor. The cy should be "0" if also the floor of room is 0 click.
When lara move up a sector hight 4 click the Cy should be Cy = -1024 because upward the y axis in negative.
Differently, if lara go down in the hole deep 4 click the Cy should become +1024 because there are 4 clicks of difference than flat floor.
These experiment are enough stupid because we already know the results, anyway I made this example to explain how discover the reference of Y origin of lara for all animations.
For example if lara hangs to edge of elevated 4 click block, we have another animation (that of hanging) and we could discover that in this animation the Y origin is no more to lara's feet but now is where lara has her knees(!)
Creating a sample project (always with floor to 0 clicks) and creating also a pool room (a room with water), we could discover other Y origins for particular standard animations for example when lara is floating on water surface the Y origin of lara is the same of Y coordinate of water surface. Also while lara is floating up and down for the waves the Cy value reamins the same. So in this case the Y origin is about on the neck of lara.

So, when you create your TestPosition command you should verify as first thing what is the point of lara where there is the Y origin in animation from what you mean launch your custom animation, and build your Y range on this reference.

Now we could perform some examples using stand-up position of lara, when the Y origin is on her feet.

If the item is a little ball, and it is in front of lara, when the ball will be heigh like the head of lara, the difference will (about) + 600 while if the difference is negative, for example - 1024 this means the ball is an hole below lara than one sector.

Differently than example for X Range, when we set a range for Y distance, it's not sure we'll use same value with - and + signs, bacause when the Y pivot (origin of lara on Y axis) is in her feet, we could require different abs values for min and max.

For example, if we want that above little ball (big like head of lara and with its Y origin in the center of sphere) was (about) in front of lara's head, we should set as Y ranges somewhat like:

YDistanceMin = 550
YDistanceMax = 650

As you see, there is no negative value in above values.
This happen because lara is height about 3 clicks, so 256*3 = 768 units (exactly 800 units).
We wish that the ball was in front of head of lara, so the ball cann't be lower than neck of lara, and the neck is about to 512 units from feet (y origin) for this reason we set as min Y distance 550, because if it was less of 550 it means the ball is very closed to feet of lara.
By other hand, we neither wish to have the ball so much over the head of lara, and since the top of lara's head is 768 the max distance should less than 768 (we used 650)

Remember that we work on relative axis (see above paragrah) so it's no important if Y origin of lara or item are positive or negative, the difference will work always in correct way.
When the Y difference (in example of the ball) is 0 this means the ball is in front of feet of lara, and hence in wrong position.

SIZE OF LARA

To understand better the size and mesh position of Lara in stand up position see the "ExTestPosition_LaraMesh.jpg" image.

In spite of some distortion
for perspective view, we could suppose following sizes for lara:

Height in stand-up position (picture A or C) = 800

Width stand-up, from hand to hand  (picture A) = 320

Depth of lara by profile (picture C) = 130

Height in all fours position (picture D) = 320

Height in climb position (picture B) = 600

ZDistanceMin and ZDistanceMax fields
-------------------------------------
With these two fields you set the distance range on Z axis.
The z axis is always the direction where lara is looking.
When the Z difference (Lara.Z - Item.Z) is negative this means that item is in front of lara, while if difference is positive lara has passed over the item and now it is at her back.

HOrientDiffMin and HOrientDiffMax fields
----------------------------------------
These two fields set the acceptable range for the relation (difference) between horizontal orientation (facing) of Lara and Item.
The horizontal orientation is given by Lara.HorizonalOrient - Item.HorizontalOrient.
To understand what I mean for "Horizontal" facing you could look the picture "ExTestPosition_H_Orienting.jpg" that you find in usual test position sample.
The default H facing of items (lara or others) is 0 and this is the position you see in NGLE when you have just placed an item in the level map.
To understand how the difference between H orientation changes you should see the project in AnimationTestPosition
.zip file.

If you set correct distance ranges (see previous fields) but you wrong the horizontal orientation difference you can get to have lara closed to item (correctly) but the item is not showing the correct side to lara.
For example if you create an object like a cabinet, and you wish create an animation where lara forces the lock to open it, it's necessary that, not only lara is closed to cabinet object, but also that lara has in front of her the lock, because if lara has in front the back of cabinet or some side, the distance could be correct but the animation should have no sense.

You should as first step find the ideal difference between H orientation and then create a range where there is at least a little tollerance.
For example with the cabinet the diffrence could be $8000 (
32768) (face to face) and the range should have a Min a few less for example $7f00  while the max bigger than same tollerance ( +/- $100 ) i.e. $8100.

VOrientDiffMin and VOrientDiffMax
 fields
----------------------------------------
These two values set the range for difference of Vertical orientation.
See the image "ExTestPosition_vOrienting.jpg" to understand how V orientation works.

This range is not very important because 99 times on 100 both items (lara and other item) will have VOrientation = 0

You could set a range as  Min= -
200 and Max = +200
The very seldom case where lara has a different VOrient is the run with rash, when she swimm underwater or she is diving.
Anyway you can use the sample project in AnimationTestPosition
.zip file to verify this situation.

ROrientDiffMin and ROrientDiffMax fields
-----------------------------------------
These two fields set the valid range for difference of Rotating Orientation between Lara and item.
Pheraps "Rotating" is not the correct term to describe this kind of facing anyway you can see how it works watching the image: "ExTestPosition_RotOrient.jpg"

It's very seldom the value changes, it should be always 0, so also the difference between lara and item should be zero.
Anyway, this value changes a bit when lara is running with rush and she is turning at left or right.
You can type as range -200  +200 but probably you can also type 0 and 0 in most situations.

## TextFormat
Syntax: TextFormat=Color (CL_...), FormatFlags (FT_...) , BlinkTime, SizeCharacterMenu (SC_...)
Scope: To use in [Level] section or [Title] section

Arguments:
Color = CL_... values (see constant list) to set default color for print string flipeffect

FormatFlags = FT_... values (see constant list) to set default position of text, default size and further blink or stretch characters
Note: You can add to FT values for text position the two flags FT_NARROW_CHARS and FT_BLINK_CHARS and all FT_SIZE.. flags using '+' character.
Example:
FormatText= CL_GOLD, FT_TOP_CENTER + FT_BLINK_CHARS, 16
In this way you set gold color as color, you set top center as position and (plus) you set also blinking characters way.
Remark: remember this setting work only for text you print using print text flipeffect.
If you want change size of tomb characters you have to set a SC_ .. value in SizeCharacters field (see belove).

BlinkTime = Numeric value to signal interval for blinking when you set also flag FT_BLINK_CHARS in FormatFlags parameter
Note: for blink time you can use only power by 2 values, like: 1, 2, 4, 8, 16, 32, 64, 128

SizeCharacterMenu = Costant value SC_ type to change default size of character in tomb4 menu.
Remark: differently by values of FT_ flags, the value you set in SizeCharacter field will effect only tomb raider default texts (menu, options, but not the legend string) but this setting will have no effect on flipeffect print text. For this reason you should use this field only in [Title] section in this way:

TextFormat=     IGNORE, IGNORE,IGNORE, SC_HALF_HEIGHT

While when you use TextFormat in [Level] section you should type in this field always IGNORE, otherwise the text for menu of tomb raider will change in irregular way in according with last level played.

Currently you can select only one of following values in this field:
SC_NORMAL
Don't change anything. The characters will have usual size
SC_HALF_WIDTH
It will be reduced by half the width of characters, while the height will be not changed
SC_HALF_HEIGHT
It will be reduced by half the height of characters, while the width will be not changed
SC_HALF_SIZE
This value reduce by half both width and height of characters
SC_DOUBLE_WIDTH
Set double width for characters, untouched the height
SC_DOUBLE_HEIGHT:5
Set double height for characters, untouched the width
SC_DOUBLE_SIZE
Se double width and double height for characters.

Default values for TextFormat command:
If you don't set any TextFormat command for current level, the default values will be used are:
TextFormat= CL_WHITE, FT_BOTTOM_CENTER, 16, SC_NORMAL

Remarks:
(1) You can change the settings for text also in game with some flipeffects, anyway to avoid to waste too much triggers for text formatting it's better to use this script command to set default values.
(2) Other to use some FT_... value to set position of text you can also add to specific string some format characters to change furtherly the position of text on screen. For example if you use a FT_TOP_LEFT but you are not happy to see the text overlapped to vitality bar you can start your text with character couple: "\n" (n = newline) in this way a first empty row will be printed and the visible text will start in lower position.
If you want move more right a LEFT alignment you can start each row of your text with character couple: "\t" (t = tabulation)
Example:
\tHi world\n\tI'm very happy to be here

## TextureSequence
Syntax: TextureSequence=IdTexSeq, FramePerSec, SEQ_ flags, Tex Indices array {...}
Scope: to use in [Level] section
Max number of instances for level section is: 99

IdTexSeq field
--------------
Progressive number to idenfity this texture sequence when you enable it using the flipeffect trigger.
You'll use "1" for first TextureSequence command, the value "2" for second TextureSequence command, ect.
Valid range is 1 / 999

FramePerSec field
------------------
In this field you set the speed of the sequence. The value is in frame per second. The max value is 30, i.e. 30 frame for second, the smallest value is 1.

SEQ_ flags field
-----------------
You can set two or more SEQ_ flags, you find the list in reference panel of NG Center program.
Currently there are following flags:

SEQ_LOOP
The sequence will be perfomed in endless way. If you use this flag you'll have to use specific flipeffet to stop the sequence, otherwise it will be performed continuously.

SEQ_LOOP_INVERSE
This flag works only if you set also SEQ_LOOP flag.
While the single SEQ_LOOP flag set an infinite loop with this sequence (supposing 4 textures from 0 to 3):

0 1 2 3 0 1 2 3 0 1 2 3 ....

When you set also the SEQ_LOOP_INVERSE flag, the sequence will be:

0 1 2 3 2 1 0 1 2 3 2 1 0 ...

SEQ_STOP_AT_FIRST
If you set this flag you force engine to set always the first texture of range when the animation will be completed.
Differently, if you omit this flag the last texture showed depends by type of animation.
If you have set no loop, the last texture showed will be the texture of last index of array sequence.
If you have set a loop, the texture showed will depend by the moment when you perform the Stop texture sequence flipeffect.

Tex Indices array {...} fields
------------------------------
The tex indices array may host up to 1000 different indices.
You'll separate the value with commas ',' in common way.
Each index will describe the index position for each texture within the specific animation range.
For example if you set four texture in tga map, with each texture show a literal "A" the first and then "B", "C" and "D"
When you want show a frame with "A" texture you'll type 0 (zero), when you want the "B" you type 1, ect.

For example if you TextureSequence command is:

TextureSequence= 1, 4, IGNORE,
3, 2, 1, 0

You'll have a sequence with ID = 1 (this is the number to set in flipeffect trigger to start/stop this sequence)

The animation will be slow, because you set 4 fps, so the image will change only four times for second.

No loop, because you set IGNORE for SEQ_ flags

While the sequence showed in game will be with following images:

D  (3)
C  (2)
B  (1)
A  (0)

Remarks:
* The ranges you can animate with TextureSequence command have to be set like "P-Frames" in NGLE.
* The max number of texture you can animate with TextureSequence is 16. This mean you can type only number enclosed in range 0 - 15 in Indices Array fields.
* You can animate same P-Frame range with different TextureSequence commands.
* The Id of P-Frame range to animate will be set in flipeffect trigger

## TriggerGroup
Syntax: TriggerGroup= IdGroup, ExportValue1 + TGROUP_ flags, ExportValue2, ExportValue3, {Other Values 1/2/3 of exported triggers or conditions)
Scope: To use in [Level] section
Max number of instances for level section: 999

TriggerGroup permits to host two or more triggers to perform them in same moment using a single flipeffect "Perform TriggerGroup..".
You can also perform a TriggerGroup using a condition trigger "Check condition of <&>TriggerGroup" and in this case the condition will be true if all conditions typed in TriggerGroup command will be true.

You can get the data to type in TriggerGroup comand, from Trigger Window of NGLE program, clicking on button named "Export Script Trigger". If you don't see any button with that name, this means that specific trigger can't be exported in script format.
You can export following triggers:

Flipeffects
Actions
Conditions

When you export a trigger as script trigger you'll get a text file containing informations like these:

---- Begin file ---------

Add following three values in your script command:

... , $2000, 96, $0

Infos about exported trigger
--------------------------------------
Type: FLIPEFFECT
<#> : Lara. Disarm Lara in <&>way
<&> : Remove All. (Weapons + Ammos)
(E) :
--------------------------------------

--- End file ----

In above example the values to add to your TriggerGroup command are the values: "$2000, 96, $0"
Other informations you read in text files are given only for reference, to remember that above three numbers perform the trigger "Lara. Disarm Lara" ...

Arguments:

IdGroup
-------
The IdGroup is a simple progressive number you type to recognize this TriggerGroup from other TriggerGroup commands in same [Level] section.
You'll use the IdGroup number to start via trigger this TriggerGroup, in fact, when you in trigger window select a flipeffect "Perform <&>Trigger Group" in the "<&>" field you'll have to choose a TriggerGroup between different triggergroup with different IdGroup values.
You should start with "1" for first TriggerGroup and continue with "2" and then "3" when you add other TriggerGroup to your [Level] section.

Note: the valid range for IdGroup field is 1 / 9999

Exported Trigger values
-----------------------
After IdGroup field, you can type a list of triple values, each group of three values have been get from an "Export Script Trigger" operation.
Pratically to keep the data of a single trigger are necessary three values and you'll type these three values in same order you see in text get from "Export Script Trigger" button.

The general syntax fo TriggerGroup becomes hence:

TriggerGroup= IdGroup,  {Alfa1, Alfa2, Alfa3}, {Beta1, Beta2, Beta3}, {...},
{Omega1, Omega2, Omega3}

Remark: the "{" "}" parenthesis don't have to be written really, they are present in above row only to idenfity the optional group of trigger data you can add in TriggerGroup command.

In above syntax, the "alfa", "beta" or "Omega" are some specific exported triggers, and the numbers "1/2/3" are the three values get for each exported trigger.

You should not modify the values you get from exporting, but you can add to first value (like "Alfa1", "Beta1"

 or "Omega1" in above example) some TGROUP_ flags to give specific means to current trigger.

You can add following TGROUP to first value of each exported trigger:

TGROUP_AND
TGROUP_NOT
TGROUP_OR
Above flags are boolean operators, they are used only to link condition triggers.
If you don't use any boolean operator the default value is always AND, this means all condition you type in sequence should be all true in same moment to get true the final condition.
Anyway in some circustances you could wish to create more particular condition.
For example if you want create a final condition was true when "Lara is climbing" or "Lara is monkey" you'll have to use the TGROUP_OR operator in this way:

   Condition1 (For lara is climbing)
   TGROUP_OR + Condition2 (For lara is monkey)

In this way just it was true at least a single condition to get true the final condition.

The TGROUP_NOT works in a different way, it is like an "AND NOT" because is linked with previous condition (if it exists) whereby an AND but inverte the mean of current condition where it has been attached.
Pratically you'll use the NOT when you don't find in condition list of trigger window the wished condition but only the opposite. Using this opposite condition with a NOT you'll inverte the mean of that condition.

TGROUP_ELSE

You can add
TGROUP_ELSE to conditions or common trigers.
When you add this flag to some trigger you start an "else block".
The else block will be performed ONLY if previous condition block gave negative esite.
You can use ELSE to perform two different triggers (or block of triggers) in according with a single conditional block.
For example if you want load a bonus level if lara found all secrets while you want perform the tile level (to end the adventure) if lara has not found all secrets, you can realize this using an else in this way:

   Condition (Lara found <&> secrets)
       (if true) Perform trigger (Load bonus level)
   TGROUP_ELSE + Trigger (Perform title level)

In this way if lara found all secrets will be loaded bonus level, else, the title level.

You can place none, two or more ELSE flag in your TriggerGroup, creating very comples conditions.

A way to use ELSE is to test a lot of condition and perform a different trigger for each different result of above conditions.

For example:
    Condition: "Lara found 1 secret"
         Trigger: Load Level 5
    ELSE Condition: "Lara found 2 secrets"
         Trigger: Load Level 6
    ELSE Condition: "Lara found 3 secrets"
         Trigger: Load level 7

In above example we used ELSE attached to other condition. In this way we can scan a list of conditions and perform a different trigger for each result.

The rule used by game to scan a trigger group is the following:

IF current_trigger is a condition: verify if condition is true
IF it's true verify other conditions, if there are no other conditions perform the first non condition trigger found and all following non condition trigger until end of triggergroup or upto first ELSE flag found.
IF current trigger is not a condition: perform it and all following and then stop at first else flag found.

IF current_trigger is a condition and the condition is false, skip current condition block and skip following non-condition block, continue parsing until to find an ELSE flag and start to verify conditions or perform trigger from this ELSE flag.

TGROUP_USE_FOUND_ITEM_INDEX
TGROUP_USE_OWNER_ANIM_ITEM_INDEX
TGROUP_USE_TRIGGER_ITEM_INDEX
Above TGROUP flag modify the "ITEM_INDEX", where for "item index" we mean the index of moveable used by trigger (like Action) or conditions.
By default each trigger, when you export it, has already an index but in some cirucstances we need to perform actions (triggers) on moveable we don't yet know.
For example to create a fight situation like this:

"When Lara hit an enemy the enmey will lose vitality"

we have the problem about how identify the index of enemy because could exist different enemies of that slot type in the level and we cann't know while we are typing our trigger, what enemy will meet lara for first.
To solve this problem we use above flags to specifiy the index to use for following triggers or conditions.
This methodo is possible only when there is a way to locate dynamically a moveable.
For example if we perform a condition like if "Lara collides with moveable of <&>slot", when the condition is true the engine will have discovered the index of moveable is touching lara. Now, if we attach to this condition the flag TGROUP_USE_FOUND_ITEM_INDEX the found index in current condition, will be used in following triggers or conditions, and we'll be able to perform a specific trigger over this enemy detected dynamically, for example to damage it "Remove vitaltiy to <#> enemy"
When we know to want use above flag we can type our source trigger choosing any object
indifferently because we know that it will be used another index of object to perform that action.

The flag TGROUP_USE_OWNER_ANIM_ITEM_INDEX is more easy to understand and to use.
You should use this flag only when current TriggerGroup will be performed by an animcommand inserted in some animation of some moveable.
In this circustante the TGROUP_USE_OWNER_ANIM_ITEM_INDEX flag will force as index to use the index of moveable where you inserted this animcommand.

The TGROUP_USE_TRIGGER_ITEM_INDEX
 flag
, reset the Index of moveable, forcing it newly to its original value.
Pratically if you in previous trigger or conditon had used a flag like TGROUP_USE_OWNER_ANIM_ITEM_INDEX  or  TGROUP_USE_FOUND_ITEM_INDEX, you can to use newly the real source indices in curent and following trigger or conditions, using this flag TGROUP_USE_TRIGGER_ITEM_INDEX.
With this flag you inform the engine to use the effective index typed originally in that trigger.

Remark: you can find other explanations and examples in Reference panel of NG Center, clicking on some TGROUP_ constant in "mnemonic costant list"

## Turbo
Syntax: Turbo=Flags (TRB_..), FPStoKeep
Scope: To use in [Level] section

Turbo command allows to set some tricks to enahance the speed of game engine.
You should use this command only when your level has some problem of frame rate, i.e. when the FPS goes under 29 FPS.

Remark: At bottom of description for Turbo command, you find some suggestion to enhance the speed of your level using some little tricks in projection phase.

Description of fields:

Flags (TRB_..) field
---------------------
You can set one or more (linking with +) TRB_ constant values
To have a description of TRB_ values read the infos about each TRB_ value you can find in Reference Panel of NG Center program, in section "MNEMONIC CONSTANTS..".

FPStoKeep field
----------------
This field used ONLY if you set also TRB_ADAPTIVE_FARVIEW setting in Flags field.
You should type the FPS value you want keep.
If you type IGNORE the default value is 29 FPS and this means that, when FPS goes belove 29 fps TRNG will reduce far view distance until to reach newly 29 fps.
If your level in some side has very low fps, for example 15 fps, it's better don't require a too high FPStoKeep value, otherwise the distance will be decreased too much. For example if you type 20 like fps to keep, trng will begin to reduce distance far view only when fps goes down 20 fps.
Pratically this field is a way to answer to following question: How much is it important for me the best frame rate, and how much the max far view distance?
If you retain very very important to have always best frame rate, you type 29 (the max value) and the game will keep always this frame rate also damaing severly the far view distance. Differently, if you thinks it's very important the far view distance, then you could type in thsi field only a low value like 20 fps, in this way the far distance will be preserved almost always, with only exception when the game is really very down about frame rate.

Remark: if you don't use the TRB_ADAPTIVE_FARVIEW setting you have to type IGNORE in this field.

****************************************************************

Some tips to enhance the speed of your level
--------------------------------------------
If the usage of Turbo command is not yet enough to get the wished result, you could use some little tricks while you are building your level to enahnce the speed.

The problem of statics...
--------------------------
The static objects require many more time to be showed than moveable objects. For this reason many "slow down" problem born own because there are too much statics in same room or nearby.
To solve this problem a trick is to replace the static objects with moveable object with same look.
For example you can replace your static copying its meshes and textures with strpix program.
Using "Export dxf" on original static object, and the command "Import dxf" while you are placed on some moveable, like an Animating, in this way also the correct textures will be moved fine.

The transparence problem...
---------------------------
For complex reason the game engine perform many extra computes when some object has transparent textures. Hence it's not possible always giving up about this usage, in some circustances you could replace the transparent texture with some more detailed opaque mesh.
For example to simulate a cirle, instead using a squared mesh with a transparent texture where the circle is the opaque side, you could creare a real circular mesh using some triangles. In this way you'll have avoied the usage of transparent texture without giving up the shape you wished.

An alternative to Adapative frame rate setting
----------------------------------------------
If you are not satisfacted by working of Adapative frame rate of Turbo command, you can set in your level some specific
zones where have a reduced far view and other with bigger far view, using new flipeffect triggers to set dynamically the current level far view value.
Using this method you should disable the adaptive far view in turbo command otherwise there will be conflicts, and the you should to study your level in game to understand what are the zone where it's important to have a gain in fps reducing the level far view.
For example if your level there is a wook with many trees very closed it's very problem you'll have some problem of speed, in this case you could place a strip of trigger to enable reduced far view when lara enters in this wood, while another bound of triggers to set a bigger far view when lara goes out from this wood.

## WindowsFont
Syntax: WindowsFont= IdWindowFont, WindowFontName, WindowsFontFlags (WFF_...), SizeFont, ColorRbgId, ShadowColorRgbId
Scope: To use in [Level] section
Max number of instances for level section: 99

WindowsFont permits to choose the settings for Windows Font.
Please, don't confuse the common game font used in Tomb Raider with Windows Font.
The Windows Fonts can be used only in special trng page layout, like Lara's Diary and other trng features that will be added in feature.

IdWindowFont field
------------------
In this field you type a progressive number used as identifier to reference this font setting from other script commands.
Valid range for IDs: 1 / 999

WindowFontName field
--------------------
You can type a standard font name.
Note: from the 1.2.2.7 trng version, you can introduce the font name with the charset code for the font.
You use the colon character ":" to linke the two sides.
Example:

204:Arial

The list of most important charset codes:

ANSI_CHARSET            0
DEFAULT_CHARSET         1
SYMBOL_CHARSET          2
SHIFTJIS_CHARSET        128
HANGEUL_CHARSET         129
GB2312_CHARSET          134
CHINESEBIG5_CHARSET     136
OEM_CHARSET             255
JOHAB_CHARSET           130
HEBREW_CHARSET          177
ARABIC_CHARSET          178
GREEK_CHARSET           161
TURKISH_CHARSET         162
VIETNAMESE_CHARSET      163
THAI_CHARSET            222
EASTEUROPE_CHARSET      238
RUSSIAN_CHARSET         204
BALTIC_CHARSET          186

Note: if we wish having western charset just omit the charset number and the value used by default will be 0, i.e. ANSI charset that is good for all western europe languages (english, german, french, spanish, italian)

Remark: currently trng engine supports only 8 bit encoding charset. Therefor you can not use unicode or other 16 bit charset.

About the font name, altough there were a lot of Windows fonts, I suggest to use only very common font names, otherwise there is the risk that the font you chose was missing in some player's computer.
The most common font names are:

Arial
Comic Sans MS
Courier
Courier New
Ms Sans Serif
Times New Roman
Verdana

Remark: the name you type in this field, it's necessary type also in strings section, like it happens for script command like Puzzle or Legend
For example if you use "Courier New" you'll type this text in this field:

WindowsFont= 1, Courier New, ...

And then you have to type this text also in [Strings] or [ExtraNG] strings section.

Note: when you set a charset number, also this number should be typed in the string. For example if you use in the windowsfont the composite font name "128:MS Gothic", you'll have to type this composite name also in the string section �128:MS Gothic�

Remark: If you omit the font name, typing IGNORE, trng will find a generic font responding to other settings, like bold and size.
Anyway to get best results it's better type always a font name.

WindowsFontFlags (WFF_...) field
---------------------------------
In this field you can type one or more WFF_ flags linked with "+" sign.
If you type IGNORE in this field trng will use default setting: WFF_BOLD+WFF_SHADOW+WFF_LEFT_ALIGN for common text settings, while for title settings will use: WFF_CENTER_ALIGN+WFF_UNDERLINE+WFF_ULTRA_BOLD

See description of WFF_ flags in Reference panel of NG_Center program.

SizeFont field
--------------
The size of fonts is computed in complicated method.
This value is the height in pixels of bigger character in a screen resolution 1024 x 768, where the width of the char is the 50 % of SizeFont you typed.
Then, this size will be stretched in according with current effective resolution of game screen.

Remark: About the space that will be necessary to host the text on the screen it's important advise that this size could be a bit different when the game work with different screen resolution, for this reason it's important you set a couple of [Size Font] and [Frame Size for text] to have some free space yet available in frame text, to compensate further adjustment.
Said in other way: if you chose a big size for your font and you are happy because all text fits in the text frame you chose, you could have a bad surprise when the game will be played on other PC or in other different resolution, because the same text could be too long to be fit in that text frame and the player could be not able to read last rows of your text.
So, to avoid this risk, it's better let a safety extra space in your text frame to compensate these variations in font size between different computers and resolutions.

Theorically you should let empty about a 25/30 % of whole space to compensate above variations.

ColorRbgId and ShadowColorRgbId
 fields
---------------------------------------
With these two fields you can set the color for font (
ColorRbgId) and for further shadow color (ShadowColorRgbId
)
The value you type is the identifier of some ColorRGB in same [Level] section.
You should choose for shadow color a very different color respect than text color.  For example, if the text color is white, you should choose black for shadow, and vice-versa.

Remark: it's necessary that the ColorRGB commands were typed BEFORE of WindowsFont command that uses them, otherwise in the parsing phase trng will be not able to locate the correct ColorRGB command because it has not yet been parsed.

## WindowTitle
Syntax: WindowTitle= NameOfCurrentLevel
Scope: To use in [Level] or [Title] section

With this command you can set a text that it will be showed in Window Title bar of the game.
This command gets a result only when the game is in Windowed mode.

Example:

WindowTitle= The Last Revenge by Paolone

Remark: As for other commands using text, you should remember to add the same text also in english.txt file.
If you want get different language version, just you type your text in english section, and then click on different language combo box and then you can overwrite the text showed in selected position.

## WorldFarView
Syntax: WorldFarView=MaxSectorDistance
Scope: To use in [Options] section

Default value in tomb4:
WorldFarView=MaxSectorDistance = 20 sectors

Valid range: Min=1  Max=127

With WorldFarView you set the max distance you'll could use in any level of your game.
You should set a bigger number with WorldFarView and littler or equal values with LevelFarView in different level in according with further speed troubles for two wide and complex scenes.

Remark:
See also LevelFarView command and FogRange command for more infos.

