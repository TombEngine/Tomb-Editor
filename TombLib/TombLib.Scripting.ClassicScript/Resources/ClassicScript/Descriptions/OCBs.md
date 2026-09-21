## AI_FOLLOW
If you put two or more AI_FOLLOW (other that is below the moveable) you must set same OCB value for start AI_FOLLOW and next AI_FOLLOW.
Differently the following AI_FOLLOW objects will increase the ocb value of 1.
For example:

First AI_FOLLOW  = OCB 1 (below the moveable you want move)
Second AI_FOLLOW = OCB 1 (where moveable will go)
Third AI_FOLLOW = OCB 2 (further target)
Fourth AI_FOLLOW  = OCB 3 (ect)

Remark:
AI_FOLLOWs used for move enemy jeep should start from value -2 and follow above rule.

## Amber_light
1 = Enable an earthquake effect and it plays sound effects BOULDER_FALL and EXPLOSION2_VOLWAS80

2 = Plays sound effect MAPPER_PYRAMID_OPEN and, if this object is in an outside room, it enables a volumetric explosion effect.

Remark: This object could be affected by Pulse= script command.
If in current level there is a Pulse=ENABLED command, the amber light will be never enabled, with the only exception if the flipmap 4 is currently enabled.

## Animated_objects
666 = to stop animation when lara goes out of triggering square

## Baddy_1
Baddy 2
1 = Roll Right. He rolls about a block so this is good for triggering as Lara comes up to a doorway
2 = Jump Left. As above
3 = Ducked
4 = Climb up 4 clicks. Make sure the origin of the baddy is 4 clicks below the block he's about to enter
10 += To give to baddy unlimited ammo

You can set a sequence of baddyes (type 1) adding number 1000 for first baddy, 2000 for second ect.
In this way the second baddy will be activacted only when first baddy will be killed.

1000 += set as first baddy
2000 += set as second baddy
ect.

Same method could be used for baddy 2 but in this case you should use multiple of 100:
100 += set as first baddy 2
200 += set as second baddy 2
300 += set as third baddy 2

Remark: if you want create two different enemy sequences in same level you must create an "hole" value bigger than 1000 (or 100 for baddy_2)
For example:
We say you want then baddy 1 (that we named it "Bob") had other two enemies that will appears when he will be killed.
We will set OCB for first sequence in this way:

Baddy_1 "Bob" with ocb 1000
Other baddy_1 (for bob) with ocb 2000
Other baddy_1 (for bob) with ocb 3000

If we wish have in same room other baddy with other enemies sequence (we will name him "Richard") we should set in this way ocb codes for second sequence:

Baddy_1 "Richard" with ocb 5000 (do you see? It has not "4000" but 5000 eg. +1000 of next value for "bob" baddy_1 sequence)
Other baddy_1 for "richard" with ocb 6000
Other baddy_1 for "richard" with ocb 7000

## Blinking_Light
Apparently the ocb value is a simple countdown before starting of the light.
If you type a very big number the light will require more time to become visible after its triggering.

## Chain
1 = to hurt lara

## Clockwork_Beetle
Insert 4 in both clockwork beetle combos (mechanical scarab and key)

## Dog
0 = normal (it will become visible only after triggering)
1 = Dog immediatly visible on ground (standing down) and it will move when triggered

## Door_Type_1
If door is a double door you must set:

In left door the value 269
In right door the value 276

(From example in tut1 level)

## Door_Type_2
Enter all 1-5 buttons to make door opened at start.

Remark: setting al five buttons the further following sequence for door with trigger and antitrigger could don't work properly.
If you mean use antitrigger/trigger sequence for door it's better don't hit all buttons but let them unpressed and trigger normally the door using a hidden trigger where lara will step over before reaching the door.

## Door_Type_3
1 = Prevents door opening. Used in conjunction with cog switch to have the door opening slowly while lara pull the cog.
2= To open with a crowbar

## EarthQuake
Remark: Earth quake is nullmesh object placed and triggered to create earthquake effects � rumbling and shaking.

333 = for 16 seconds (sound but not shaking)
888 = for a 5 second quake and sound

## Emitters
It often pressing all five Buttons you get the object immediatly visibile without need of triggering

## Enemy_Jeep
To handle the enemy jeep you must use AI_FOLLOW objects. (See ocb for AI_FOLLOW objects in this table)
To stop the enemy jeep you must use a FlipEffect = 30

Remark:
In level "Desert RailRoad" of The Last Revelation enemy jeeps had following OCB values:
0 (zero)
101
102
103
104

## Falling_blocks
Any value different than 0 =  don't collapse when you stand on them but only when they will be triggered

## Flame_Emitter_1
Flame emitter 1 may burn and kill Lara.

-2 or -7: it shots continuosly the flames in horizontal, following the direction of the cone. Apparently these two ocb values have the same result.

{Negative numbers different than -2 or -7}: it shots in horizontal the flames, but, differently by -2 or -7 ocb, in this case there is a time intervals betwen a shoting and the following shoting. The different negative values have the only usage to have different ejection time when you place many flame emitter and enable them in same time. For example if you type the same negative OCB in two flame emitter, like the number -5, and you enable them in same time, they will emit the horizontal flame in same time. While if you place two different negative ocb, like -5 and -8, the two emitter, in spite to be enable in same time, they will have a different timing about shoting flame and pause time.

{Positive numbers} = Apparently the positive OCB values have no effect on shape of flames, anyway these value will be really used in a misterious way, changing the enabled raising blocks in according with "Scaled Spike". This is the result I got studying the code but it will be necessary some experiment to understand in that way the ocb in flame emitter 1 influences the raising blocks on this misterious "scaled spike".
The only sure thing, is the positive ocb values don't modify directly the flames. Other important suggestion is to don't type never number greather than 7, because otherwise will be passed over the array from 0 to 7, about the raising blocks with unpredictable results.

## Flame_Emitter_2
Note: The flame emitter 2 will not cause death of Lara

2 = to make the flame move along in the direction the cone is pointing.
Remark: this "moving" is not simply the direction of the flames, but the fire moves really its position in the room upto a disappear behind some wall of the room. To avoid to have a resource like a fire continuously enabled it's advisable set the direction of fire to move it upto a water room. When this fire reaches a water room it will disappear with a volumetric effect.

123 = Fire with standard size, i.e. the same size of flame emitter 1

{Positive numbers different than 2 and 123}: the ocb gives the height  of flames. Bigger are the ocb values and littler will be the flames. For this reason an ocb 1 will have a flame bigger than ocb = 50.

{Negative numbers} = Curiosly, the negative OCB numbers enable the flipmap corresponding to negative number of the ocb. For example if you set -1, the flimap 1 will be enabled. In this situation no fire will be drawn.
For more infos see Scales puzzle and Bedazzled�s tutorial on Skriblerz.

## Flame_Emitter_3
0 =  it is the flame used on the special "oil" water in the palace levels. Will cause death
Numbers different than 0 =  it will change the flame into the blue "lightning" as in the Karnak level.
2 or 4 = Inverts facing by 180 degrees
Numbers >= 3  the lightning tries to hit (in progressive, endless way) the Animating3 items present in the level that having the same ocb value typed in emitter3 ocb and another animating3 with ocb=0.

For example if you place an animating3 and you want have it will be used like target of lighning you have to type in its OCB the same value you typed in emitter 3. For example type 5 in emitter3 OCB and type 5 also in Animating3 OCB.

You should place always also another Animating3 with ocb = 0 because this ocb generate a double lightning.

Ocb 888, 889, 890
-----------------
These OCBs have been added from 1.2.2.4 version of trng dll.
You can use these ocb to hit lara with a blue lightning.
The difference is for the injury for lara:

888 = Kill and fire immediatly Lara
889 = Hurt lara and burn and kill her only after few seconds
890 = No damage. You'll use this ocb when you wish generate a your custom effect when you trigger this flame emitter.

Remark: the damage with value 889 changes in according with dry/wet state of lara.
If lara is underwater the damage is very low
If lara is floating on the water the damage is higher
 than underwater
If lara is dripping (after a swimming) the damage is furtherly higher
When lara is fully dry the damage will have the max intensity

## Flyby_Camera
0 = Snap to start of sequence from Lara cam
1 = Not used
2 = Loop for infinity
3 = Track Lara cam
4 = Target Lara's last position before camera trigger
5 = Target Lara's current moving position
6 = Snap back to Lara at end of sequence
7 = Cut-Cam, Jumps to a specified camera in the same sequence (Timer = cam number to jump to)
8 = Hold camera (timer = 30 X Number of seconds)
9 = Disale look key break out.
10 = Disable Lara control
11 = Enable Lara control
12 = Not used
13 = Not used
14 = Activate heavy trigger
15 = Not used

## Guide
Remark: The Guide must be used with the "Follow AI" or he will run around in circles.

BB 1 = Light Torch
BB 2 =  Activate Trap (as belove)
BB 3 + BB 5 = Read Inscription (put a heavy trigger under the AI point if you want something to happen when he reads it)
BB 4 =  Light Petrol (as above)
BB 5 = Grab Torch

All Bit Buttons... Make him disappear

Remark:
In level "Valley Of The Kings" of The Last Revelation guide had values:
9000
13000

## Hammer
1 = normal hammer ( hammer smashes, chain lifted then smashes again, and so on... )
2 = Hammer smashes while lara stays on the trigger, when she steps of, the hammer will remain in his position, so if it was lifted up it will just stop and stay that way. makes explosive sound when smashes
3 = same as above but when lara gets of the trigger it first gets lifted to the ceiling and then it stops.

## Helicopter_(flying)
Flying helicopter has internal name "Animation 1" and you find it in "Desert Railroad" of The Last Revelation.
To do fly it you must set ocb = 666 and use AI_FOLLOW is same way of Enemy Jeep (see AI_FOLLOW and Enemy jeep descriptions)

## Helicopter_(Mine)
"Mine" helicopter is the "static" half (only a side of meshs are visible) helicopter you find in Death City. Lara will can destroy it shotting.

Enter 1 in OCB of the helicopter. To make it explode, use SHATTER3 and set it up as per room 73. The fuel can does not sit directly on the trigger � it won't activate the heavy trigger if it does.

Enter 0 in OCB of the helicoter if you want use only "mined field"
With OCB = 0 the helicopter will be invisible and at its placement it will be created a mine field of 6 x 6 sectors. If lara walks over the mine field she will explode and die.
Remark: the mined field is not correctly aligned with secotor bounding.
Praticall it has a size of full 5 x 5 sectors zone + a border of 1/2 sector of width.

## Horseman
Horse
If you want have two or more couples of horseman-horse you must set for each couple the same OCB value.
For example:

Horseman1 = OCB 1
Horseman2 = OCB 2
Horse1 = OCB 1
Horse2 = OCB 2

## Jump_Switch
Set 1 as ocb if you want have a reability of lever after 'timer' seconds.
For example if you in switch trigger insert in timer field the value 12 and put '1' in ocb of jump switch the trigger (for example for a door) will be activated for 12 seconds, then this time, the door will be closed and the lever will go up newly.
Remark: not all 'jump switches' have reability function, you should use jump switch you find in guard.wad file.

## Lightning_conductor
0 = No damage
1 = It can burn Lara
2 = The engine looks for ANIMATING8 item. If it is present the Lightning conductor will use as target the ANIMATING8 item
The ocb 2 cause also a particular compute when the lightning conductor is in flipmap 1 and the flipmap has been enabled. It will be used the sound effect ELEC_ARCING_LOOP

## Little_Beetles
0 - 128 = Total number of scarab you want
1000 += Scarab appear from the floor
2000 += Scarab appear from the ceiling
4000 += Scarab slow release followed by a gush

Remarks:
- To clear all active scarabs, use a Flipeffect trigger with a value of 31.
- The beetle swarm was used in conjunction with either PUZZLE_ITEM12 or PICKUP_ITEM1, both of which are scarabs that attach to the wall and require a crowbar to pick off. The swarm of beetles sometimes shoots out of the "hole" behind the wall scarab. A special texture tile is used to create the illusion that they come from the hole in the wall. To do this you simply designate the trigger for PUZZLE_ITEM12 or PICKUP_ITEM1 as a key trigger (and type in an OCB setting of 2 to position it on the wall in-game), then trigger the LITTLE_BEETLE to the same square. (Raise LITTLE_BEETLE up to the height of the "hole") Make sure you have the correct settings in the OCB menu for the LITTLE_BEETLE and that you have left a crowbar somewhere as a pick up!

## Locust_Emitter
It can be deathable if you enter a high enough value in the OCB data field...somewhere around 96 is the limit.
OCB code defines the number of locusts.
OCB values used in the last revelation were: 12, 20 and 25

Remark:
Locust swarm can set heavy triggers in motorbike path or release a swarm from a shatter object.

## Mummy
2 = Visible on ground, it will be activated after trigger and when lara goes near to it

## Music_Scroll's
-422 = Lara will place the scroll on the "puzzle hole 2" and play the Lyre(you have to put the Lyre to the left of the puzzle hole)
Remark: you must set -422 value also in puzzle hole 2 object.

## NEW_Animating
The OCB in animating will be used only when you use an ACTION trigger to move animating.
In this circustance the OCB value will be used as speed for movement of animating.
About the value to type you should remember that 1 sector are 1024 units, and the value you'll type will be added 30 times for second to current coordinate.
For example if you type like speed the value 40 the animating will be moved of 1200 units (40 x 30 fps) for second, and this distance is a bit bigger than one sector.
Remark: If you let 0 in OCB of animating the default speed used by "move animating" trigger will be of 30 units.

## NEW_Boats
Boats, rubber boat or motorboat, accetp the same type of OCB:

You can use of following values or a sum of them to enable in same moment different features:

1 = Show lights in front of boat

+2 = The boat will be not able to enable heavy trigger

+4 = The boat will be not able to enable lara trigger

Remark. For "Lara" trigger I mean the trigger may be activated by lara.

About the value 2 and 4, you could use this limitation to create interesting situations in game: for example you can cover the bottom of a water room with trigger flipeffet:

Lara. Kill Lara in <&>way -> Default death (vitality=0

Using a common "trigger" as trigger type.

Then you set in motor boat the value
4 (the boat will be not able to enable "Lara" trigger)

More, it's better if you give to that water a dangerous look (like it was lava for example)
Now in game Lara will be killed if she enter directly in that dangerous water, but if she across the water using motorboat she'll survive because motorboat will not enable the common "trigger" on bottom of water room.
This is only an example, other interesting combinations could be created using a bit of fantasy.

+8 = Enable the "look at" feature

+16 = Set boat as "no fuel". In this situation it will be no possible switch on the boat engine. You can use trng variable triggers to set or clear this ocb in the boat in game time to stop or restore this boat.

+32 = Automatic fuel management. When there is this flag, trng will use the Local Long Delta variable to detect the current value of the fuel. When delta is 0, the boat will be in "no fuel" mode, and it will not work. When the delta has non-zero value, the boat will work. You can use variable trigger to modify the fuel in Delta variable, anyway remember that trng engine will decrease byself the value of delta variable everytime that the engine boat is working.

+64 = Set the boat as "anchored". In this situation the engine will work but the boat will be not able to move away from current position.

+128 = Show fuel bar on the screen. This flag works only togehter with 32 (automatic fuel management). With 128 + 32 ocbs, a fuel bar will be displayed on the screen everytime lara is driving the boat, to show the current level of fuel.
You have to add to the script a "Cutomize=CUST_BAR, BAR_CUSTOM4, ... " command to set the features of the fuel bar.
Read also the description of FBAR_USED_FOR_BOAT_FUEL flag in MNEMONIC CONSTANTS section of reference panel of NG Center.

## NEW_Bridges
From 1.2.2.4 version of TRNG dll are available the following OCBs for all BRIDGE objects.

Tilt Grade (0 / 63)
-------------------
The tilt grade is the level of slope for current bridge.
It is the number of click about the difference between the two opposite sides in the direction of the slope.
For example: 0 = flat (no slope), 1 = one click between the opposite sides, 4 = one sector (4 clicks) between the two sides, ect.

Note: the Tilt Grade works only for the BRIDGE_CUSTOM object, while for other BRIDGE objects the tile factor is implicite in their name: BRIDGE_FLAT = Tilt0, BRIDGE_TILT2 = Tilt2

Enable Hanging (64)
-------------------
The ability of Lara to hang over edges of the borders of the fragmented trigger zones is not good. She will be frozen and in some dynamic action she will be throwed over the footbridge with a rough movement.
To avoid all these problems the hanging features have been disabled for all bridge objects.
Anyway if you wish enable newly it for some bridge, just you add the 64 value in its OCB field.
I suggest to perform this adding only for common squared bridges (no conditional triggers) or with bridges where one or two sides are the same of game sector grid, in the hoping that the player didn't try to hang in other opposite sides. (You should forbid this using some game planning trick)

Disable sliding (128)
----------------------
By default when the tilt grade is above than 2 (from 3 to infinite), Lara will slip (slide) over that game sector.
With the bridges you can disable the sliding for any tilt grade, in spite in some circustances it could seem unrealistic.
Pratically you could use this ocb only with BRIDGE_TILT3, BRIDGE_TILT4 or BRIDGE_CUSTOM with a tilt grande >= 3
When you use this ocb lara will be able to walk normally on this sloped bridge.
I suggest to use this feature only when there is a visible reason to do, like for the staircase, where an inclination of 45 degress (tilt 4 factor) is normal, and the persons (like lara) will be able to walk over.

Depth of the Bridge (1/255 multiplied by 256)
---------------------------------------------
The depth of the bridge is important because it has the role to simulate the new ceiling when lara is belove of it, other to creare a wall with height between the up surface of the bridge and the down surface, set by depth.

By default (if you type 0 as depth) the depth is one click, like it was in old level editor.
Now you can set also values lower than one click, since the units for depth are 1/16 of game sector. This means that one click is 4 units.
In this way you can have very slim bridges, a nice chance for hanging footbridges, but also a very depth bridge, useful to simulate the collision of some big static objects placed on the floor, avoidin that lara was able to pass down of them.

## NEW_Fish_Emitter
Remark: you can compute the ocb value for this object using the OCB Calculator you find in Tools2 panel of NG_Center program. Using that tool is very easy compute the correct OCB value.

Number of Fish
--------------
The main value to set in OCB field is the number of fish.
You can type a number between 1 and 127.
Try don't exagerate about the number of fish. Remember that the max number of fish for whole level is 128 and in this quantity there will be also the little beetles you mean use, since the fish are a trng news builded using some resources of little beetles particles.
You can fill a wide water zone with only 16 or 32 fish, in particular way when you use individual fish instead of a fish shoal.

Remarks:

* If you let 0 in OCB field the trng engine will use the setting for 8 fish in a shoal that will try to attack Lara.

* The fish differenlty by locusts or beetles are able to recognize the box sectors.
With box sectors you can forbid the access to some zone in the water or you can use them to identify the presence of some static or moveable.
In fact, you should remember that the particles don't check for collision with static or moveables but only with floor, walls and ceiling. So if you place in the water some object like an old ship and you don't wish that the fish across it, you can place box sectors below the ship and in this way the fish will go around it instead acrossing it.

Slow Fish (128)
---------------
You can add the value 128 to force the fish to move slowly.
By default the fish have a good speed because they born to fill wide water zones, anyway this setting is very useful when you use these fish in a little space like a little pool or an acquarium.
See also the acquarium in Fish and Pirahna demo.

Friend Fish (256)
-----------------
By default the fish are predactor and they will try to kill lara but you can also disable the attack on lara to get some nice little fish for decorative usage.
When you add the value 256 to OCB the fish will not attack or heart lara in any way.

Individual Fish (512)
---------------------
By default the fish move in shoal like in tomb raider 3 adventure.
When the fish are in a shoal they will try to remain in same little space closed to pivot fish (the boss of the shoal).
Threre are advantage using the shoal, for example with aggresive fish so to let a chance to lara to dive in water only when the fish shoal is very far. Anyway if your target is to populate a wide water zone then it's better disable the shoal to get that the fish will be distributed in wide spaces.

Fish Type
---------
Differently by Tomb Raider 3, where all fish have always the same look, with the trng Fish Emitter  you can choose between four different fish:

Pirahna fish (+ 1024)
Clown fish (+2048)
Butterfly Fish (+4096)
Angel Fish (+8192)

Please, don't confuse the "fish type" with the aggresive behavior of the fish.
For example you can have some friendly pirahna or some aggressive Clown fish.
The fish type set only the look of the fish and not its behavior.

Remark: you can add two or more fish type in same fish emitter. When this happens the fish emitter will generate fish of different types in equal quantity.
For example if you type as ocb = 2048 + 8192 + 8 = (Clown Fish + Angel Fish + 8 fish) = 10248
The fish emitter will generate 4 clown fish and 4 angel fish.

Jump Fish (16384)
-----------------
You can allow to the fish to jump out of the water in random way.
If you use this setting you should take care to avoid that some fish jumping out of the water ending its jump on the ground, because when this happens the fish will die.
To avoid this risk you could build an high border around the water, with an height of at least one sector (4 clicks).
Anyway another method is to build a water room with a soft slope to get that, around the beaches, there is only water very low, less than 2 click. This trick could work fine because the code for the jump forbid to the fish to jump out when the water (where they are in that moment) had an height less than 2 click. So if the depth water is only very far from the beach is very improbable that the fish will be able to reach with a single jump the (far) gound.

Remark: if you check the "Fish and Pirahna" demo, you'll see that the tropical fish at start of the level jump a bit too often. This happens when there is some side with water enough depth to enable the jumping (two clicks or more) but the middle height of the water is very low. This was the situation of coastal level used in demo level. Anyway if you create a depther water the fish will jump only in seldom circustance and not so often like in that demo, this happens because the jump begins only when the fish is closed (about one click) to water surface, but when the water is very depth, the fish very often will be in too depth waters to jump out.

Timid Fish (32768)
------------------
When you use Friend Fish setting, you could add also the Timid Fish to simulate that when lara is too closed to some fish, it was afraid and will escape away.
It's not advisable using this setting in little water room but only in wide spaces.

## NEW_Guardian
The OCB of Guardian (Laser Head) will be used to reduce the count-down before the guaridan pointed Lara beginning his attacks.
If you let 0 in OCB the guardian will require about 3 seconds before attack lara for first time.
While if you set a number like 150, the guardian will attack lara immed

## NEW_Hydra
OCB 1 or OCB 2
--------------
If you place 1 or 2 in OCB of Hydra you'll work with a pair (a couple) of  Hydra objects.
In this situation you should place both hydra in same sector, setting in first object the OCB = 1, while in other object set OCB = 2.
In game the two Hydra will be (a bit) moved, to stay on two opposite corners of same side.
Another feature of hydra's couple is that both hydras will be inclinated a bit in opposite way to create a sort of "V" letter.

Remarks:

* When you work with couple of Hydras you cann't chose the facing, since the engine will force the starting facing to South (in ngle view).

* If you omit 2 and 1 in OCB, the Hydra will work like single object and it will keep the facing and position set in the project.

OCBs for Missile type
---------------------
The Missile should be the energize ball shot by Hydra to hit Lara.

You can choose only a single of following values, and then you'll add it to (further) "1" or "2" for couple setting.

0 OCB (Missile 0)
If you don't add meaninful values for Missile Type, you chose the first missile type.
Missile 0, is an energic ball of green color. It creates a little damage to Lara. (shockwave)

256 OCB (Missile 1)
Green Energize ball, but this will burn Lara.

512 OCB (Missile 2)
Yellow Energize ball. Little damage

768 OCB (Missile 3)
Azure Energize ball. Little damage. This ball curve a bit to reach Lara.

1024 OCB (Missile 4).
Azure Energize ball. Almost the same of 768 ocb, but in this case the ball doesn't curve.

1280 OCB (Missile 5)
Yellow Ball, little damage.

1536 OCB (Missile 6)
Fire Ball. It burns Lara

1792 OCB (Random Missile type)
If you set 1792 like missile type, the engine will choose in random way the missile type.
This means that, in same life time of same Hydra, it could shot different kind of missiles.
The player will be able to recognize the most dangerous balls for their color or layout: the Missile 1 (green color) and the Missile 6 (fire ball).

Other OCB settings
------------------

2048 OCB (Disable Fire)
The hydra, before shooting an energize ball, shows a little fire.
This fire is not very beauty to see, because it will be showed in a weird position of Hydra's body, anyway it could be useful for player to understand when the shot is coming soon.
If you add the value 2048 to OCB, the little fire will be no more showed.

4096 OCB (Disable Missile)
If you add the 4096 value to OCB, the hydra will no more shot energize or fire balls.
Also with this limitation the Hydra will be able to hurth lara usign its big beak.

## NEW_Jeep
Setting "1" in OCB field of the jeep you can enable a beam light for the jeep.
This light works like that of the sidecar, anyway the light cone mesh you see in sidecar is missing on the jeep.
If you wish add this mesh in the jeep you can have the showing/hiding of the new mesh (it has to be the mesh with index = 18) perfmored by trng engine when the light is on or off.
Remark: if you add this new mesh remember to set it like "invisible" at start.
See the sidecar object with Animation Editor to understand better how performing this job.

## NEW_Kayak
OCB 1 = Add a mist wake
-----------------------
Unfortunately the two wakes you saw in Tomb3 game are not available in this trng version of the kayak.
The reason is very technical: the method used in tomb3 to create those two stripes, used some features not available in tomb4 engine.
Anyway you can add a little white mist under the kayak adding the value 1 in the OCB field.

OCB 2 = Accept an Animation to go on board whereby a jump
---------------------------------------------------------
By default, using the original code from Tomb Raider 3 adventure, Lara was able to go on board only from floating (on water) position.
The missing of other methods to go on board is a problem because when the kayak is in low water lara will be not able to use newly the kayak and for this reason it will be forbidden to go off (when it is in low water) to avoid the risk to "lose" our kayak forever.
The Mudubu tr3 level had been build to have always depth waters or other tricks to forbid to lara to go off from kayak in bad positions, like using the currents (sinks).
Anyway in our case we wish use the kayak in many different environments and for this reason it should be useful having other method to go on board since this allows to go off from boat in different situations, too.
Since I'm not able to build custom animations in the kayak object (you find this object in kayak.wad on http://www.trlevelmanager.eu/ng.htm in demos section) there is no animation to go on board with a jump, anyway I create the chance for you to add in a second time this custom animation.
Another way to describe above speech is this: if you don't add new animations to go on board, the kayak should be used ONLY in depth water because in other environment, like low waters or land, lara will be not able to leave (going off) the kayak and this situation could be frustrating for the player that didn't understand because he cann't leave the kayak in many circustances.

If you create a custom animation to enter on the kayak from upstairs with a jump, you should perform following operations:

1) Create a custom animation for KAYAK_LARA slot with the following features:

Create the animation with these data:

START POSITION:
Pose of frame 39 of animation 77 of LARA slot
About the position is not possible have a perfect matching because we cann't know the horizontal speed or the exact falling angle, anyway you could place Lara about 150 units upper than Y origin of the kayak object and remember that the alignment position will be the center of kayak with z cord changed by -80, therefor pratically lara will be moved in following position before performing your animation 33:

Lara X = Kayak X
Lara Y = Current Lara Y (no change for Y coordinate)
Lara Z = Kayak Z - 80

About the facing it will be forced to be the same of kayak before executing your animation.

Remember that, the animation will be started when the collision box of Lara touches the collision box of Kayak and the position of Lara was compatible with the black hole of the kayak.

A suggestion is to perform a longer moving in vertical, for example moving  of 200 units in downstairs and about 80 in forwards, using a speed of movement alike that you see in animation 77 of LARA slot.

FINAL POSITION:
Pose and position of frame 176 of animation 3 of KAYAK_LARA slot

FACING:
Lara should have the same facing of the Kayak.
Remark: anyway, in the game, it will be accepted a little difference with a  tollerance of +/- 20 degrees.

ANIMATION EDITOR DATA:
Slot = KAYAK_LARA
Animation 33 (you have to add a new animation)
StateId = 4
Next Animation = 4

2) Create a custom animation also for KAYAK slot
When lara drives a vehicle all animations of the two slots work coupled and for this reason it's necessary that each couple of animation (same number of animation in the two slots) have always the same number of frames.

You could copy from KAYAK slot the animation 5.
This is good for our target because the kayak has a short rolling that we could use like the effect of the jump of Lara on board.
Then add a new animation in KAYAK slot and copy above animation in Animation 33 position.
It's important you add empty frames in this KAYAK animation 33 until to have same number of frames of Animation 33 you created in KAYAK_LARA slot.

In Animation editor set for this animation the same data of other Animation 33:
Slot = KAYAK
Animation 33 (you have to add a new animation)
StateId = 4
Next Animation = 4

3) Now you can set in OCB of KAYAK the value 2 to enable your custom animation
Remember that you have no need to use Animation or TestPosition commands, because all management of this custom animation is already handled by TRNG engine, just only you add the OCB 2 in the kayak object.

Remark: if you want build your animation keeping as reference the mesh of the kayak remember that the facing of kayak mesh in wad file is the opposite than that it will have in the game, so you should try to get the position of kayak from a frame of Animation 16 of KAYAK object.

OCB 4 = Accept animations to go on board from land
--------------------------------------------------
Since the kayak, differently than rubber and motor boats, is able to move also on land where you can "park" it on the beach, it should be interesting add animations to go on board when the kayak is on the beach.
To realize this target it's necessary you create the two custom animations and then you enable them adding the 4 ocb value.
For an introduction about the speech of new custom animation to go on board, read also the description of OCB 2.

If you add the animations to go on board from land you'll get also the advantage to be able to go off from boat when it is on the beach.
Differently, when these animations are missing, Lara will be not able to go off from kayak when she is on the beach.

These are the technical infos about the two custom animations to realize:

ENTERING FROM LEFT SIDE ANIMATION

START POSITION
Use the pose of Animation 103 of LARA slot (any frame is good)
About the position the following data are computed like difference respect than Kayak position. These data are compatible with the TestPosition command and if you don't understand them you could read the Animation tutorial you find on http://www.trlevelmanager.eu/ng.htm  page in the Demos section.

XDistance = -280
YDistance = 0
ZDistance = +100

Facing = $4000  (+90 degrees)

FINAL POSITION
Pose and position of frame 176 of animation 3 of KAYAK_LARA slot

ANIMATION EDITOR DATA:
Slot = KAYAK_LARA
Animation 34 (you have to add new animations)
StateId = 4
Next Animation = 4

ENTERING FROM RIGHT SIDE ANIMATION

START POSITION
Use the pose of Animation 103 of LARA slot (any frame is good)
About the position the following data are computed like difference respect than Kayak position. These data are compatible with the TestPosition command and if you don't understand them you could read the Animation tutorial you find on http://www.trlevelmanager.eu/ng.htm  page in the Demos section.

XDistance = +255 (Curiosly this value is different than the
                 complementar of left side (-280) because the
                 collision box has an asymmetric position)
YDistance = 0
ZDistance = +100

Facing = $C000  (-90 degrees)

FINAL POSITION
Pose and position of frame 176 of animation 3 of KAYAK_LARA slot

ANIMATION EDITOR DATA:
Slot = KAYAK_LARA
Animation 35 (you have to add new animations)
StateId = 4
Next Animation = 4

KAYAK ANIMATION
You have to add always also an animation in KAYAK slot in same Animation number and with same number of Frames than the KAYAK_LARA animations.

Since the entering from land don't require rolling of the kayak you could use the animation 16 of KAYAK slot, where the kayak is always still, adding the frames to reach same length of animations 34 and 35 in KAYAK_LARA object.

Remark: the trng engine will supply an automatic adjustment to reach the ideal start position before performing your custom animation. This means the position of Lara at begin of your animation it will be always exactly the same respect than kayak.
If you wish you can have an antipation about this start position, adding the OCB 4 in kayak, parking the kayak on the beach and then trying to enter from left or right side. Since in this moment the entering animations are not yet present lara will be stopped immediatly after the alignment phase with the message "missing go-in animation number XX". In this way you can see where is lara respect to kayak when your custom animation should be performed.

OCB 8 = DISABLE NORMAL TRIGGERS
-------------------------------
By default when lara is on the kayak she will engage the normal trigger, enabled normally from Lara.
If you wish disable this activation you can add the value 8 in OCB field, in this way lara on kayak will enage only HEAVY triggers and the normal triggers will be ignored.
This feature could be useful when you want create some damage for lara when she is swimming in the water but you wish also permit to her to avoid this damage when she is on the kayak.

OCB 16 = ENABLE RAPID MIST
--------------------------
This OCB tries to re-enable a feature present in tomb raider 3 but that does't work in native manner in trng engine.
The rapid mist is a white mist used to simulate the waves on the kayak when it is moving down for some slope where, in the reality, there is no water but (furtherly) a waterfall to simulate the water.
The rapid mist works only when the kayak is moving down (its "head" is aiming downstairs) and the floor is closed to kayak.
In other circustances the rapid mist will be always disabled.

OCB 32 = Enable "look around" feature
-------------------------------------
By default in tomb4 engine it's not allowed for Lara looks around when she is driving some vehicle,  anyway if you wish give to lara this chance for kayak just you add value 32 to ocb and Lara will be able to see around to her using as usual the LOOK key.

## NEW_KeyPad
Remark: you can compute the ocb value for this object using the OCB Calculator you find in Tools2 panel of NG_Center program. Using that tool is very easy compute the correct OCB value.

If you use the object stored in SWITCH_TYPE1 slot of ng.wad you'll be able to have a working Key Pad for your levels.

With keypad you can create door requiring password (keycode) or control with custom trigger for the elevators present in your level.
The keypad work fine in SWITCH_TYPE1 slot but probably it should work also in Switch_type 2 and 3.

In OCB window of switch_type object you have to set a combination of following OCB to do work it like a keypad:

0-9999 = Keycode. This is the number (secret code) that Lara will have to type to activate the switch.

10001 - 10010 =  If you set as key code number a value in the range of 10001- 10010 this means it's required a single key (like for elevator). In this case, the number of ocb describes the max number that user will be able to type. For example if you set 10007, players will be able to hit only number upto 7 (1 - 7)
This work mode is used for elevator but in short time will be added other features using this method to permit to player to choose between different choiches. For example to choose a door to open in a sequence of door, or the room where lara will be teleported.

+16384 = Signal a keypad switch. You have ALWAYS to add this value to your ocb number if you want that this switch object will work like a keypad.

Some examples:

To have a keypad requiring the secret code "7153":
7153 + 16384 = 23537
So you have to type 23537 in OCB code of keypad.

To have a keypad requiring a single number encloses in the range from "1" to "4":
10004 + 16384 = 26388
In this case the number to insert in OCB is 26388

Remark:
About the keypad mode with single key, currently there is only the trigger:

Elevator. Move <#>elevator to floor number set in last keypad operation

able to use it but many other will be added in short time.
To use above trigger just you place switch_Type and trigger it with a trigger of SWITCH type (this is the usual old method to use switch objects)
Then you set in OCB of switch type a value like (10004 + 16384 =) 26388

Now you'll have to cast another common trigger in same sector, the trigger will be the flipeffect:

Elevator. Move <#>elevator to floor number set in last keypad operation

In game the trigger to move elevator will be performed ONLY if player insert a correct value, in this sample it will be a number enclosed in range "1" upto "4".
When this happens this number will be used by trigger to move elevator as target floor.

Remark: Please don't confuse the real keypad you find in slot SWITCH_TYPE1 with the fake keypad used for elevator you find in ANIMATING16_MIP slot of ng.wad.
The fake keypad you find in this slot can be used only inside of elevator but it works in hardcoded mode when you create an elevator with internal keypad.
The reason of this duplication is that in sectors where is the elevator it was not possible to use a real SWITCH_TYPE object and switch trigger because the elevator requires also DUMMY trigger and it's not possible overlap special triggers (like SWITCH and DUMMY), so I had to use an harcoded mode to avoid this problem using a fake switch trigger but really it is not a switch trigger you can use in otherside outside of elevator.

## NEW_ParallelBars
The parallel bar is a new object imported from Tomb Raider Chronicles.
The ocb value set the power for final jump.
If you set 0, lara performs a very short jump, while with 200 she jumps about to 2 sectors of distance.
I presume that 100 = one sector, but it's only a supposition because the formula changes the horizzontal and vertical speed for jump in a complicated way and only these speeds affect the covered distance for final jump.

Remarks:
* Looking the code, apparently the compute of jump power changes in according with orienting (facing) of Parallel bar object respect to lara. I.e. if lara comes from a direction the ocb value will be used in a way, while if lara comes from opposite direction, the ocb value will be computed in other way. This means you can change the power of jump also inverting the facing of parallel bar, rotating it by 180 degrees.
I've not had time to perform many analysis about this supposition but the original chronicles code seems to work in this way.

* If you customize the parallel bar with PB_PROGRESSIVE_CHARGE flag, the value you set in OCB will be a starting point for progressive charge compute.
With PB_PROGRESSIVE_CHARGE the power of final jump will be increased by [OCB value] for each full rotation around the bar, so it's better in this case don't set too big values.
The compute begins from current OCB value (minimum power) to a max of OCB multiplicated by 10 (max value).

* When you use PB_PROGRESSIVE_CHARGE togheter with PB_LARA_CAN_SLIDE flag, the current power of jump will be reset everytime lara moves herself to left or to right.

## NEW_Pushable_Object
Remark: you can compute the ocb value for this object using the OCB Calculator you find in Tools2 panel of NG_Center program. Using that tool is very easy compute the correct OCB value.

You can get all pushable objects treadable and climbable just set correct value in OCB field in the Object window of object. (Key 'O' in NGLE while the object is selected)

0-31 = height of object collision. 1 click = 1 unit, so to set a pushable height 1 sector (4 clicks) you have to type 4
You can create pushable height upto 7 sectors in this way.

Then you can add one or more of following values to set corresponding feature:

32 = The pushable obejcts with value 32 could be throwed in the empty.
Usage of this features requires some attention:
For technical reason the engine requires to rotate (in immediate way) the facing of pushable to have always correct orienting (facing) of pushable in according with moving direction.

You'll have no problems if your pushable has same textures on its sides and a simmetrical texture on top, while in other cases, when lara was pushing the object in the empty the pushable will change orienting to have correct facing in according with direction.

You have two ways to solve this problem:
- Texturixe the pushable to have same texture on 4 sides and place on top texture an image that remaing always the same rotaing it in 90 degrees steps.
- Enable ocb values 512 or 1024 to forbid movements in some direction (east-west or south-north), and then place the pushable in the level with correct facing, looking the border where it could be throwed down.
Remark: when pushable object is falling down it will destroy shatter objects and kill mortal creatures but it will not kill the semigods.

64 = ENABLE new trng features for current pushable. WARNING, it's very important,  because ONLY if you add also the value 64 the special ocb features will work.
When the value 64 is missing, next generation engine will ignore the ocb values of current pushable.
This method to work it is necessary to allow compatibily with Planet Effect you see in Lost Library.
When you want use pushables for planet effect, you have to type in ocb only nunber between  1 and 5 in different five pushables, since in ocb value typed the value 64 is missing, trng will ignore the ocb values for pushales of planet effects.
Differently, when you want use new special features for next generation pushables , you have ALWAYS to add value 64 to other ocb values.
128 = forbid pulling
256 = forbid pushing
512 = forbid east-west
1024 = forbid south-north
2048 = climb west
4096 = climb north
8192 = climb east
16384 = climb south

About climb feature it should be used only for pushable height al least 8 clicks (8 clicks = 32 units as value to set in ocb for height)

Remarks:

*If you want have a tradition pushable (tomb4 old style) just you omit to add the value 64 in your ocb.
*If you want have some special features like forbid pushing/pulling or some direction, but you don't want that the pushable object was walkable, just you type like height the value 0 (zero) and then add only other ocb values to set the wished features.

## NEW_RollingBall
Now you can set in OCB field of Rollingball one or more (adding them) of following values:

1 = Silent mode.
Disable sound and earthquake. It is useful to use trick of hidden rollingball to enable more special triggers in sequence, avoiding sound and moving view.

2 = Kill Enemies.
Rollingball will kill all creatures it touches while it is moving.

4 = Active with pushing.
Adding the value 4 the rolling will be activated when lara pushes it.
In this situation it's not necessary place any trigger for rollingball.
With ocb 4 you can place rolling ball also on flat sector (non-sloped sector) just there are slopes on bounding sectors.
In this way lara will be able to choice the direction where move it.

Remark: this ocb requires the Lara's animation
number 316 in your wad. Not all standard wads have this animation, anyway you may find it in catacomb.wad
If animation 316 is missing in your wad it will happen nothing when lara hits Action closed to rollingball.

8 = Active with pushing and recovery.
This ocb works like ocb 4, but in this case you'll be able not only to engage first time the rollingball but also to move it infinite times from its target position.
Using this method you can move rolling ball also on flat (not sloped) floor, until to reach a new slope where throw it.

Remark: also this ocb require the animation number 316, see above info for Ocb 4.

16 = Destroy shatter objects
Rollingball will destroy the further shatter object on his path.

32 = Check for collision with water room
If your rolling could go in water you can add the ocb 32 to handle:
- Splash on water surface
- Simulation of underwater movements with different pysical behavior.

64 = Enable common triggers. The rollingball, other to enable heavy trigger as default, will enable also common triggers, i.e. the "trigger" usually enabled only by Lara.
In this way a rollingaball will be able to enable traps for lara saving her.

## NEW_SideCar
By default the sidecar has always a beam light when is working.
You can disable the beam light typing "1" in OCB field of SideCar object.

## NEW_Static_Objects
From TRNG version 1.1.8.7 you can set in each static object some OCB codes to set following features:

4  = Disable collision. Lara could pass across the static

8  = Set Glass Transparence. The static will have a glass transparency effect

16 = Set Ice Transparence. The static will have a light transparence, like the ice

32 = Damage Lara on physical contact. The static injures lara if she touches it. The damage removes 10 hp to lara (full vitality= 1000) anyway you can change this value using CUST_SET_STATIC_DAMAGE in Customize script command.
Remark: remember that this damage will be continuosly applied while lara is touching the static, this means that also a little damage could kill lara in short time.

64 = Burns Lara on physical contact. Lara will be burned anyway she will have yet some time to search water to save her.

128 = Explode killing Lara on physical contact. With this ocb you can transform this static in a sort of mine.
Remark: if you use this ocb code you should import in your wad the lara animation number 438. You can find it in newcity.wad
If this animation is missing in your level, the trng will perform anyway the explosion but lara will be killed with default death animation used when lara has vitality = 0

256 = Poison lara on physical contact. Lara will be poisoned when she touches the static. You can change the poison intensity using CUST_SET_STATIC_DAMAGE in Customize script command.

512 = Huge Collision. Inform the trng engine that current static has a collision box larger than 6 x 6 sector. By default tomb4 ignored collision larger than 6x6 sector to optimize the speed in collision check, anyway if you want create a huge static to go over the 6x6 cut-off just you add the 512 ocb. In this way your static could have a box collision upto 36x36 sectors.

1024 = Hard Shatter. In old tomb4 the shatter objects went destroyed by every weapon other by Skeleton and Templar. If you add 1024 to ocb of some shatter object you can increase its hardness and the static will be destroyed only by explosive ammo (not flash grenade)
, sphinks, jeep, sidecar and Rollingball (if it has correct OCB).

2048 = Heavy Trigger on contact. When Lara touches a static with OCB 2048 the heavy trigger placed under the static will be activated.

4096 = Scalable. This flag allows to scale the size of current static, increasing or decreasing it.
You type the ocb value to set the scale rating with following formula:
Percentage * 4 + 4096
where Percentage is a number in the range 1 - 1000, and 100 = original size.
For example if you use 1000 the static will be ten times bigger than original. If you type 1 the static will be 100 times littler than original.
About the max size the "ten times" is a theorical limit and it will work about mesh scaling but you could get an overflow in view box. In this case the item will be not visible in game from many positions.
For this reason it's better don't execed the 400 % scaling (i.e. 4 times bigger)

IMPORTANT: When you set the Scalable flag the previous (with lower value) flags will be ignored. This is necessary since this flag requires to use the ocb value to set the percentage of scaling. Anyway this flag will work like if the following ocb flags were always enabled: huge collision (512) and Heavy trigger on contact (2048)

Remarks:
*You can add above values but you cann't add the two different type of transparence.
For example if you want the static was like glass and with no collision you can type in OCB window the value (8 + 4 =) 12
Other example:
To transform a static in a bomb was able to explode lara and burn her: 64+128 = 192

*You can change in game time these attributes using the "Static. ..." flipeffects.

## NEW_SubMarine
The OCB with value less than 4096 (0 / 4095) for indicate the time durate of pause between a shooting (of missile) and the following.
The value is in tick frames, where one second = 30 tick frames.
If you let 0 in ocb, the engine will force the value 120 (4 seconds) in OCB.

Remark: it's not advisable type a number little than 3 seconds (ocb = 90) because, when this time has been completed the previous torpedo will be destroyed and another it will be launched. For this reason if you use a too short time, the missile will be not able to hit Lara, at least she was very closed to sub-marine.

Other OCB value to add to shooting time:

4096 = It allows to submarine to shoot its missile also out of water room.
By default the missile explodes on water surface avoiding it can go off from water room.

8192 = Disables the bubbles from the back of sub-marine.

## NEW_Switch_123
New trng ocb values
-------------------
From 1.2.2.3 version, you can type in Switch1/2/3 objects an animation number. This animation will be performed when lara is engaging the switch.
The correct range for animation number is: 4 / 4095

When you use this  custom trng animation feature, you can also add one or following flags to animation number:

8192 (Flip/Flop switch)
------------------------
When you add 8192 to the animation number, you enable the flip/flop (or on/off) feature, where lara will be able to active/disactive more times the switch.
In this situation you have to create two custom animations in two closed animation slots and then type the number of first animation.
For example if your custom animation to engage (first time) the switch is the number 386, you should type 8192 + 386 as OCB, and you should create also the inverse animation (for the antitriggering of the switch) to place in animation slot number 387, i.e. the following to 386.

Remark: not all switches allow the flip/flop feature.
A switch, to have flip/flop features, should have always four animations.
They, usually, will have following data:

Anim  StateId NextAnim  Description
-----------------------------------------------------------------
 0      0        0      Still, beginning position
 1      2        2      Moving from beginning pos. to ending pos.
 2      1        2      Still, ending position
 3      2        0      Moving from ending pos. to begning pos.
-----------------------------------------------------------------

Tip: you can find some correct switch in a slot different than Switch type 1/2/3 slot, like the JUMP SWITCH, and then copy this object renaming it to move it in a switch type 1/2/3 slot. For example in Guardian of Semherket wad you find a good flip/flop switch in the JUMP SWITCH slot.

4096 (Opposite state ids)
-------------------------
Looking the above animation table, and comparing it with other switches you could discover that the stateids are differents, in particular when the state id of beginning position is "1" (instead "0") and that of ending position is "0" (instead "1").
In this case you have to add to the switch ocb also the flag 4096 to inform the trng engine about this inversion of state ids.

Remark: you have to use this flag also when you don't use the flip/flop feature. You should use this flag everytime the beginning position (or "off" state) is "1" instead by "0"

Remark: the anitrigger feature, in flip/flop switches, works fine with common triggers to enable doors or flipmaps, but it cann't work byself for special trng trigger, like actions to move or change in some manner a moveable. In this case it's necessary that you create a triggergroup where it will be checked the current state id of the switch object and, in according with this value, it will be performed different triggers, for positive (triggering) action or antitriggering mode.
Then you'll perform only this triggergroup in the switch sector.

See the example in "New Mirrors" project you find in demo section of website:
http://www.trlevelmanager.eu/ng.htm

Standard (old) ocbs for switch 1/2/3
------------------------------------
0 = Animation where lara pull up a little switch
1 = to make this a "reach in the hole" to find a pick-up. (*)
2 = to make this a "reach in hole, open a door" switch
3 = to make Lara do the "push wall switch animation".
-1 = Animation where lara reachs in the low hole a pickup. Before pickuping up the item, lara will look in the hole. (different animation but same target of ocb 1.)
(*) You must set same OCB code ( = 1) also in item in the hole

## NEW_Teeth_Spikes
Really the ocb to set in TEETH_SPIKES are the same of old engine, anyway if you want enable teeth_spikes type tomb raider 1 where Lara is able to across if she moves slowly, you have to insert the OCB = 20.
Ocb 20 means: spikes point to north and static spikes

Then you have to add this line in your script.txt file in [Level] section:

Enemy= TEETH_SPIKES, IGNORE, IGNORE, IGNORE, EXTRA_TEETH_NO_DAMAGE_ON_WALKING

The EXTRA_TEETH_NO_DAMAGE_ON_WALKING flag means: the teeth spikes will work like TR1 spikes.

## NEW_Tight-Rope
With ocb values you can set the difficulty for lara to pass the tight-rope.

OCB 0
-----
Set Default mode.
With value 0 it works in same way of TR Chronicles.
In this mode the player has to set the direction opposite to further unbalance of Lara. For example, when lara is falling to right the player has to hit Left command and vice-versa

OCB 1
-----
Set Hard mode.
With value 1 lara could fall if player hits right or left command while lara was in perfect equilibre (while with ocb 0, in default mode, the game engines ignored the futile direction commands)
In Hard mode the player has to set more fastly the correct command and the random unbalance could happen more often.

OCB 2
-----
Set Very Hard Mode.
It works like "Hard Mode" but in this case the rensponse time required to correct the unbalance of lara has to be more rapid than "hard mode".

OCB 3
-----
Set Impossible Mode.
With this ocb lara will always fall down.
The only reason to use this ocb is when you want change in a second time the OCB field using the TRNG Variables.
In this way you could simualte some game dynamic where, for some reason (rain, snow, wind), lara is not able to walk on tight-rope, while then some change (the rain, snow, wind stop) lara will be newly able to pass the tight-rope.
See TRNG Variables triggers working with Item Memory. These triggers begin all with the text: "Variables. Memory. ....  Item Memory ..."

OCB -1
------
Set Easy Mode.

Easy Mode should be used only in adventures for children.
Lara cann't fall down in Easy Mode, just only hit FORWARD direction to reach the end of rope, with no risk.
In Easy Mode is also possible for lara invert the direction, using Backward arrow command, coming back to starting point, while in other game modes is very difficultous reach this target.

## NEW_WaterfallMist
Remark: you can compute the ocb value for this object using the OCB Calculator you find in Tools2 panel of NG_Center program. Using that tool is very easy compute the correct OCB value.

By default the WATERFALLMIST emitter doesn't accept OCB values but in TRNG engine you can add some ocb to set different features for mist emitting.

Remarks:

* If you let "0" in OCB field you'll use the old default mist emitter.
* Please, don't confuse the waterfallMIST with the WATERFALL1/2 objects. The emitter is a red cone in the ngle, while the waterfall1/2 are effective objects with a scrolling texture applied on them.

The formula to compute the value to type in OCB field is a bit complicated since there are different values to sum in this way:

(NumberOfBalls-1) + EmitMode * 4 + (SizeBall-1) * 16 + (EmitDurate-1) * 256 + ColorIndex * 4096 + CenterSquare

To understand the meaning of above values read following descriptions:

NumberOfBalls value (Default=4)
-------------------------------
You can have different emitting sources for mist: from 1 to 4.
In default tomb4 the value was the max: 4
These mist balls are aligned on a line to cover about the side of one sector.
If you don't wish a line (or a so long line) but only a shorter zone you can reduce this number, 1 or 2 for example.

EmitMode (Default = 0)
----------------------
This value set the frequency of emitting phase.
There are 4 preset emitting mode, so the valid range for this value is 0 / 3

0 = Normal Emit Mode. This is the same used in default tomb4. It shows like light spray.

1 = Slow Emit Mode. In this mode there is a longer pause between a emitting phase and the next

2 = Fast Emit Mode. In fast mode the emitting is almost continue and the mist ball has really the layout of a dense ball

3 = Random Emit Mode. In random way the interval is not regular but it changes in random way. This mode could be useful to simulare the mist for waves on the beach or in the rapids.

SizeBall value (default= 12)
----------------------------
The valid range is 1 / 16
This value affects the single size of a ball and also the length of the serie of the balls.
The default value is 12

EmitDurate value (default = 6)
------------------------------
The emit durate is the life time of a single particle.
Most long is the time and more intense will be the mist effect.
The valid range for this value is: 1 / 16

ColorIndex value (default = 0)
-------------------------------
In old mist emiter the color was always the same (light gray), but now you can choose between 8 different colors (range 0 / 7), following this table:

                      R   G   B
0: Light Gray        128 128 128
1: Azure             128 255 255
2: Yellow            236 252  19
3: White             255 255 255
4: Dark Green         15  93  76
5: Red               255   0   0
6: Blue                0   0 255
7: Light Green         0 255   0

CenterSquare value (default = 0 )
---------------------------------
By default the mist line will be showed not at center of the sector but on the side closed to base of red cone.
If you wish create mist own in the center of the square you should add the value 32768
So, pratically the CenterSquare could have like values:

0 = when you want put the mist line on the side of the sector
32768 = when you want let the mist line at center of current sector

## Pickups_Items
0 = the object is on the floor (pickup in an old stylee)
1 = the object is `hidden' (Lara plays a stick hand in wall type animation)
2 = the object is attached to a wall (Lara has to use the crowbar)
3 = the object is on a high pedestal
4 = the object is on a low pedestal
64 +=  Add 64 to any of the above if you want the item activates a pickup trigger.
128 = Set unlimited ammo, it works for all ammos execpt UZI
256 = Set pickup for mirror puzzle, see Coastal with crossbow.

## Pulley
OCB set when times lara will pull the rope to trigger somewhat.

Example:

1 = Just pull one time to trigger ..
2 = it needs pull two times to trigger
ect.

## Puzzle_Holes
999 =  turns off `collision' from the `puzzle done' object.
Without it, an `invisible' door would prevent Lara from go through.

## Raising_Block_1
1 = to activate rumble effect

Enter 1-5 in OCB to elevate.

## Raising_block_2
Enter "2" in OCB to lower the block
Press all five BB buttons in OCB to lower.

## Scorpion
Old OCBs
--------
The standard OCBs for scorpion are not very useful because they should work linked with a internal cutscene.
Placing a value in the range 1/6 the scorpion looks for a TROOP baddy with OCB = 1.  If it finds the Troop, its AI will be disabled and it could be manipulated with custom animations to simulate the fighting with the scorpion.

OCB 33 (Attack only Lara)
-------------------------
A new OCB has been added to force the scorpion to attack Lara (and only Lara).
By default, in fact, the scorpion tried to attack all baddies it finds, and only after killed them, it will attack Lara.
Typing 33 as ocb in the Scorpion it will attack only Lara ignoring other baddies.

## Sentry_Gun
Enter 1 in OCB to jam the gun, no entry and the gun fires at Lara as long as she is "in range". Place a SMOKE_EMMITER_BLACK on the same square for added effect (no trigger necessary).

## Seth_Blade
Enter negative number on OCB to delay triggering. Adding increments of 10 will delay by one second for each 10 units.

## Skeleton
Remark:
- You can change the initial animations by setting the trigger flags as listed below. If you don't set trigger flags the skeleton will come up out of the ground as normal. When placed, he is 20 clicks below the floor elevation.
- Put an AI_GUARD object on the skeleton to put him on guard.

1 = Jump Right. He jumps about a block
2 = Jump Left. As above
3 = Playing dead. This skeleton is visible (lying down) before being triggered and only gets up when triggered

## Smashable_Bike_Wall
Press buttons 1 through 5 in OCB to activate.

## Sphinx_(Like_a_bull)
1 = to get immediate attack vs Lara

## Steam_Emitter
888 =  to make steam escape sideways in the direction of the cone.

Note: Will cause death

## Switch_type_7
Switch type 7 are shatterable switch (action will be triggered when lara shot them)
In ocb window press all buttons 1 through 5 in OCB.

## Teeth_Spikes
Remark: Vertical in Room Edit above view Horizontal

0 = Pointing south
1 = Pointing south west
2 = Pointing west
3 = Pointing north west
4 = Pointing north
5 = Pointing north east
6 = Pointing east
7 = Pointing south east
8 = Pointing south
9 = Pointing south west
10 = Pointing west
11 = Pointing north west
12 = Pointing north
13 = Pointing north east
14 = Pointing east
15 = Pointing south east
16 +=  You can also add a value of 16 to the above to make the spikes stick out constantly (like the old TR spikes)
32 += Adding 32 to the above will force the spikes out once and then retract forever.

Remark: if you want use Teeth Spikes in conjunction with Mapper or ClockWork Beetle you must set OCB 4 (pointing north)

## Trapdoor_1
Enter 1-5 to make door open then close.

Note: Set trigger timer to amount of seconds for door to be open

## Troop
1 = Start troop with animation 27, while with other OCBs begins with animation 12

## TwoBlock_Platform
0 - 15 = Movement speed
16 * X +=  where X is number of clicks for elevation.

Set trigger type to "Dummy".

## Wall_Scarab
2 = Placed on walls.

Remark:  Often used with the scarab beetle swarm and also can be part of 4 large beetles needed for the Pyramid Puzzle in Cleopatra's Palaces. Don't forget about placing the crowbar!

## Waterfalls
Enter 668 to can antitrigger it. (You must also set [Invisible] button to get trigger/antitrigger for waterfall)

Other valid value is "2" used to get slow down the start effect. It has been used in level "The Tomb of Semerkhet" to simulate the yellow laser ray.
Also value  777 is used in the last revelation.

## White_Light
The ocb is a color in compact format, where each gradient (red, green, blue) has only 5 bits.
The first 5 bits (with lower value) are the red, then it comes the geen, and at end with higher values the blue.

Some example:

31  = Full red
992 = Full green
31744 = Full Blue

The adding some of above values (or using others with less intensity) you can get other colors.
For example:

31 + 992 = (red + green) = 1023 = yellow

## Wraith_2
2 = it dies on contact with water

