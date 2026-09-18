-- !Name "Add point light to moveable"
-- !Section "Dynamic lights"
-- !Description "Add a point light to a moveable."
-- !Description "Updated for TombEngine Version 1.6 and above."
-- !Arguments "NewLine, Moveables, 80, Select moveable to attach light to."
-- !Arguments "Numerical, 20, [ 0 | 100 | 0 ] , Select mesh number of moveable. \nThis can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 20, { TEN.Color(128,128,128) }, Light color."
-- !Arguments "Numerical, 20, [ 0 | 100 | 0 ], { 20 }, (Optional) Light radius (in clicks of 256 world units)."
-- !Arguments "Vector3 , 60, [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Offset (relative)"
-- !Arguments "NewLine, Boolean, 50, Casts Dynamic Shadow"
-- !Arguments "String, 50, [ NoMultiline ], (Optional) A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required."

LevelFuncs.Engine.Node.MoveableLight = function(moveable, meshnumber, lightcolor, range, effectOffset, shadow, name)

	local entityPos = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
	effectOffset = effectOffset or Vec3(0, 0, 0)
	local offset = (entityPos) + (effectOffset)
	TEN.Effects.EmitLight(offset, lightcolor, range, shadow, name)
end

-- !Name "Add point light to static"
-- !Section "Dynamic lights"
-- !Description "Add a point light to a static object."
-- !Description "Updated for TombEngine Version 1.6 and above."
-- !Arguments "NewLine, Statics, 80, Select static to attach light to."
-- !Arguments "Color, 20, { TEN.Color(128,128,128) }, Light color."
-- !Arguments "NewLine, Numerical, 40, [ 0 | 100 | 0 ], { 20 }, (Optional) Light radius (in clicks of 256 world units)."
-- !Arguments "Vector3 , 60, [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Offset (relative)"
-- !Arguments "NewLine, Boolean, 50, Casts Dynamic Shadow"
-- !Arguments "String, 50, [ NoMultiline ], (Optional) A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required."

LevelFuncs.Engine.Node.StaticLight = function(static, lightcolor, range, effectOffset, shadow, name)

	local entityPos = TEN.Objects.GetStaticByName(static):GetPosition()

	local offset = (entityPos) + (effectOffset)
	TEN.Effects.EmitLight(offset, lightcolor, range, shadow, name)
end

-- !Name "Add point light to volume"
-- !Section "Dynamic lights"
-- !Description "Add a point light to a volume."
-- !Arguments "NewLine, Volumes, 100, Select volume to attach light to.\nLight range automatically scales with volume."
-- !Arguments "NewLine, Color, 50, { TEN.Color(128,128,128) }, Light color."
-- !Arguments "Vector3 , 50, [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Offset (relative)"
-- !Arguments "NewLine, Boolean, 50, Casts Dynamic Shadow"
-- !Arguments "String, 50, [ NoMultiline ], (Optional) A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required."

LevelFuncs.Engine.Node.VolumeLight = function(volume, lightcolor, effectOffset, shadow, name)

	local vol = TEN.Objects.GetVolumeByName(volume)
	local entityPos = vol:GetPosition()
	effectOffset = effectOffset or Vec3(0, 0, 0)
	local offset = (entityPos) + (effectOffset)

	-- The engine EmitLight radius is in clicks (256 world units per click). The
	-- volume scale is expressed in world units, so divide the largest scale
	-- component by 256 to keep the light covering the widest extent of the volume.
	local scale = vol:GetScale()
	local radius = math.max(scale.x, scale.y, scale.z) / 256

	TEN.Effects.EmitLight(offset, lightcolor, radius, shadow, name)
end

-- !Name "Add spotlight to a moveable"
-- !Section "Dynamic lights"
-- !Description "Adds a spotlight to a moveable."
-- !Arguments "NewLine, Moveables, 80, Select moveable to attach light to."
-- !Arguments "Numerical, 20, [ 0 | 100 | 0 ] , Select mesh number of moveable. \nThis can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 50, { TEN.Color(128,128,128) }, Light color."
-- !Arguments "Boolean, 50, Cast dynamic shadow"
-- !Arguments "NewLine, Numerical, 25, [ 0 | 100 | 0 ], { 5 }, Light falloff (in clicks of 256 world units)."
-- !Arguments "Numerical, 25, [ 0 | 100 | 0 ], { 20 }, Light distance (in clicks of 256 world units)."
-- !Arguments "Numerical, 25, [ 0 | 100 | 0 ], { 10 }, Light radius (in clicks of 256 world units)."
-- !Arguments "String, 25, [ NoMultiline ], A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required."
-- !Arguments "NewLine, Vector3 , 50, [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Position offset (x y z)"
-- !Arguments "Vector3, 50, [ -360 | 360 | 0 | 5 | 45 ], { TEN.Vec3(0,0,0) }, Rotation (x y z)"

LevelFuncs.Engine.Node.MoveableSpotLight = function(moveable, meshnumber, color, shadow, falloff, distance, radius, name, effectOffset, rotation)

	local entityPos = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
	local offset = entityPos + (effectOffset or Vec3(0, 0, 0))
	rotation = rotation or Vec3(0, 0, 0)
	local direction = Vec3(0, -1, 0):Rotate(TEN.Rotation(rotation.x, rotation.y, rotation.z))

	TEN.Effects.EmitSpotLight(offset, direction, color, radius, falloff, distance, shadow, name)
end

-- !Name "Add dynamic aiming spotlight to moveable"
-- !Section "Dynamic lights"
-- !Description "Adds a spotlight to a moveable, aiming it at a target moveable."
-- !Arguments "NewLine, Moveables, 50, Select source moveable to attach light to."
-- !Arguments "Moveables, 50, Select target moveable for the light to aim at."
-- !Arguments "NewLine, Numerical, 20, [ 0 | 100 | 0 ] , Select mesh number of moveable. \nThis can be found in the Animation Editor within Wadtool."
-- !Arguments "Color, 50, { TEN.Color(128,128,128) }, Light color."
-- !Arguments "Boolean, 50, Casts dynamic shadow"
-- !Arguments "NewLine, Numerical, 33, [ 0 | 100 | 0 ], { 5 }, Light falloff (in clicks of 256 world units)."
-- !Arguments "Numerical, 33, [ 0 | 100 | 0 ], { 20 }, Light distance (in clicks of 256 world units)."
-- !Arguments "Numerical, 34, [ 0 | 100 | 0 ], { 10 }, Light radius (in clicks of 256 world units)."
-- !Arguments "NewLine, 50, Vector3 , [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Offset"
-- !Arguments "String, 50, [ NoMultiline ], A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required."

LevelFuncs.Engine.Node.MoveableSpotLightToTarget = function(moveable, target, meshnumber, color, shadow, falloff, distance, radius, effectOffset, name)

	local entityPos = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
	local offset = entityPos + (effectOffset or Vec3(0, 0, 0))

	local direction
	if target ~= nil and target ~= "" then
		direction = offset:Direction(TEN.Objects.GetMoveableByName(target):GetPosition())
	else
		direction = Vec3(0, -1, 0)
	end

	TEN.Effects.EmitSpotLight(offset, direction, color, radius, falloff, distance, shadow, name)
end

-- !Name "Add fog bulb to moveable"
-- !Section "Dynamic lights"
-- !Description "Add a fog bulb to a moveable."
-- !Arguments "NewLine, Moveables, 80, Select moveable to attach effect to."
-- !Arguments "Numerical, 20, [ 0 | 100 | 0 ] , Select mesh number of moveable. \nThis can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 50, { TEN.Color(128,128,128) }, Light color."
-- !Arguments "Numerical, 25, [ 0 | 100 | 0 ], { 20 }, (Optional) Light radius (in clicks of 256 world units)."
-- !Arguments "Numerical, 25, [ 0 | 255 | 0 ], { 128 }, (Optional) Effect density."
-- !Arguments "NewLine, Vector3 , 50, [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Offset (relative)"
-- !Arguments "String, 50, [ NoMultiline ], (Optional) A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required."

LevelFuncs.Engine.Node.MoveableFogBulb = function(moveable, meshnumber, lightcolor, radius, density, effectOffset, name)

	local entityPos = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
	effectOffset = effectOffset or Vec3(0, 0, 0)
	local offset = (entityPos) + (effectOffset)
	TEN.Effects.EmitFogBulb(offset, radius, density, lightcolor, name)
end

-- !Name "Add fog bulb to static"
-- !Section "Dynamic lights"
-- !Description "Add a fog bulb light to a static object."
-- !Arguments "NewLine, Statics, 100, Select static to attach effect to."
-- !Arguments "NewLine, Color, 50, { TEN.Color(128,128,128) }, Light color."
-- !Arguments "Numerical, 25, [ 0 | 100 | 0 ], { 20 }, (Optional) Light radius (in clicks of 256 world units)."
-- !Arguments "Numerical, 25, [ 0 | 255 | 0 ], { 128 }, (Optional) Effect density."
-- !Arguments "NewLine, Vector3 , 50, [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Offset (relative)"
-- !Arguments "String, 50, [ NoMultiline ], (Optional) A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required."

LevelFuncs.Engine.Node.StaticFogBulb = function(static, lightcolor, radius, density, effectOffset, name)

	local entityPos = TEN.Objects.GetStaticByName(static):GetPosition()
	effectOffset = effectOffset or Vec3(0, 0, 0)
	local offset = entityPos + effectOffset
	TEN.Effects.EmitFogBulb(offset, radius, density, lightcolor, name)
end

-- !Name "Add fog bulb to volume"
-- !Section "Dynamic lights"
-- !Description "Add a fog bulb light to volume."
-- !Arguments "NewLine, Volumes, 100, Select volume to attach effect to.\nFog bulb range automatically scales with volume."
-- !Arguments "NewLine, Color, 50, { TEN.Color(128,128,128) }, Light color."
-- !Arguments "Numerical, 50, [ 0 | 255 | 0 ], { 128 }, (Optional) Effect density."
-- !Arguments "NewLine, Vector3 , 50, [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Offset (relative)"
-- !Arguments "String, 50, [ NoMultiline ], (Optional) A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required."

LevelFuncs.Engine.Node.VolumeFogBulb = function(volume, lightcolor, density, effectOffset, name)

	local vol = TEN.Objects.GetVolumeByName(volume)
	local entityPos = vol:GetPosition()
	effectOffset = effectOffset or Vec3(0, 0, 0)
	local offset = (entityPos) + (effectOffset)

	-- The engine EmitFogBulb radius is in clicks (256 world units per click). The
	-- volume scale is expressed in world units, so divide the largest scale
	-- component by 256 to keep the fog bulb covering the widest extent of the volume.
	local scale = vol:GetScale()
	local radius = math.max(scale.x, scale.y, scale.z) / 256

	TEN.Effects.EmitFogBulb(offset, radius, density, lightcolor, name)
end
