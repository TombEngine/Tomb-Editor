-- !Name "If static mesh is visible..."
-- !Section "Static mesh parameters"
-- !Description "Checks if static mesh is visible."
-- !Conditional "True"
-- !Arguments "NewLine, Statics"

LevelFuncs.Engine.Node.TestStaticActivity = function(staticName)
	return TEN.Objects.GetStaticByName(staticName):GetActive()
end

-- !Name "If position of a static mesh is within range..."
-- !Section "Static mesh parameters"
-- !Description "Checks if static mesh's current position is within specified range."
-- !Description "If single-dimension check is needed, set other dimensions to values well out of level bounds."
-- !Conditional "True"
-- !Arguments "NewLine, Statics"
-- !Arguments "NewLine, Vector3, [ -1000000 | 1000000 ], Upper position bound" "NewLine, Vector3, [ -1000000 | 1000000 ], Lower position bound"

LevelFuncs.Engine.Node.TestStaticPosition = function(staticName, pos1, pos2)
	local pos = TEN.Objects.GetStaticByName(staticName):GetPosition()
	return (pos.x >= pos1.x and pos.x <= pos2.x and
		pos.y >= pos1.y and pos.y <= pos2.y and
		pos.z >= pos1.z and pos.z <= pos2.z)
end

-- !Name "If rotation of a static mesh is within range..."
-- !Section "Static mesh parameters"
-- !Description "Checks if static mesh's current rotation is within specified range."
-- !Conditional "True"
-- !Arguments "NewLine, Statics, 70"
-- !Arguments "Numerical, 15, [ 0 | 359 ], In range (in degrees)" "Numerical, 15, [ 0 | 359 ], Out range (in degrees)"

LevelFuncs.Engine.Node.TestStaticRotation = function(staticName, rot1, rot2)
	local rot = TEN.Objects.GetStaticByName(staticName):GetRotation().y
	return (rot >= rot1 and rot <= rot2)
end

-- !Name "If scale of a static mesh is..."
-- !Section "Static mesh parameters"
-- !Description "Compares selected static mesh scale with given value."
-- !Conditional "True"
-- !Arguments "NewLine, Statics, Static mesh to check" "NewLine, CompareOperator, 70, Kind of check"
-- !Arguments "Numerical, 30, Scale value, [ 0 | 256 | 2 ]"

LevelFuncs.Engine.Node.TestStaticScale = function(staticName, operator, value)
	local scale = TEN.Objects.GetStaticByName(staticName):GetScale().x
	return LevelFuncs.Engine.Node.CompareValue(scale, value, operator)
end

-- !Name "If hit points of a shatterable static mesh is..."
-- !Section "Static mesh parameters"
-- !Description "Compares selected static mesh hit points with given value.\nThis node is only applicable to static meshes in SHATTER slots (50-59)"
-- !Conditional "True"
-- !Arguments "NewLine, Statics, Static mesh to check" "NewLine, CompareOperator, 70, Kind of check"
-- !Arguments "Numerical, 30, Scale value, [ 0 | 1024 | 0 ]"

LevelFuncs.Engine.Node.TestStaticHP = function(staticName, operator, value)
	local hp = TEN.Objects.GetStaticByName(staticName):GetHP()
	return LevelFuncs.Engine.Node.CompareValue(hp, value, operator)
end

-- !Name "If static mesh is collidable..."
-- !Section "Static mesh parameters"
-- !Description "Checks if given static mesh is collidable."
-- !Conditional "True"
-- !Arguments "NewLine, Statics, Static mesh to check"

LevelFuncs.Engine.Node.TestStaticCollision = function(staticName)
	return TEN.Objects.GetStaticByName(staticName):GetCollidable()
end

-- !Name "If collision of a static mesh is solid..."
-- !Section "Static mesh parameters"
-- !Description "Checks if given static mesh's collision mode is solid."
-- !Conditional "True"
-- !Arguments "NewLine, Statics, Static mesh to check"

LevelFuncs.Engine.Node.TestStaticCollisionMode = function(staticName)
	return TEN.Objects.GetStaticByName(staticName):GetSolid()
end

-- !Name "If color of a static is..."
-- !Section "Static mesh parameters"
-- !Description "Checks if the specified static is a certain color."
-- !Conditional "True"
-- !Arguments "NewLine, Statics, 80, Static to check" "Color, 20, Color to compare"

LevelFuncs.Engine.Node.TestStaticColor = function(staticName, targetColor)
    local stat = TEN.Objects.GetStaticByName(staticName):GetColor()
    return stat == targetColor
end		

-- !Name "Enable static mesh"
-- !Section "Static mesh state"
-- !Description "Enables static mesh, e.g. after shattering or manually disabling it."
-- !Arguments "NewLine, Statics"

LevelFuncs.Engine.Node.EnableStatic = function(staticName)
	TEN.Objects.GetStaticByName(staticName):Enable()
end

-- !Name "Disable static mesh"
-- !Section "Static mesh state"
-- !Description "Disables static mesh."
-- !Arguments "NewLine, Statics"

LevelFuncs.Engine.Node.DisableStatic = function(staticName)
	TEN.Objects.GetStaticByName(staticName):Disable()
end

-- !Name "Play sound near static mesh"
-- !Section "Static mesh state"
-- !Description "Plays specified sound ID around specified static mesh."
-- !Arguments "NewLine, Statics, Static mesh to play sound around" "NewLine, SoundEffects, Sound to play"

LevelFuncs.Engine.Node.PlaySoundAroundStatic = function(staticName, soundID)
	TEN.Sound.PlaySound(soundID, TEN.Objects.GetStaticByName(staticName):GetPosition())
end

-- !Name "Modify position of a static mesh"
-- !Section "Static mesh parameters"
-- !Description "Set given static mesh position."
-- !Arguments "NewLine, Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 75, Position value to define"
-- !Arguments "NewLine, Statics"

LevelFuncs.Engine.Node.SetStaticPosition = function(operation, value, staticName)
	local stat = TEN.Objects.GetStaticByName(staticName)

	if (operation == 0) then
		local position = stat:GetPosition();
		position.x = position.x + value.x
		position.y = position.y + value.y
		position.z = position.z + value.z
		stat:SetPosition(position)
	else
		stat:SetPosition(value)
	end
end

-- !Name "Modify rotation of a static mesh"
-- !Section "Static mesh parameters"
-- !Description "Set given static mesh rotation."
-- !Arguments "Enumeration, [ Change | Set ], 20, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ -360 | 360 | 2 | 1 | 5 ], 15, Rotation value to define", "NewLine, Statics"

LevelFuncs.Engine.Node.SetStaticRotation = function(operation, value, staticName)
	local stat = TEN.Objects.GetStaticByName(staticName)
	local rotation = stat:GetRotation();

	if (operation == 0) then
		rotation.y = LevelFuncs.Engine.Node.WrapRotation(rotation.y, value)
	else
		rotation.y = value
	end

	stat:SetRotation(rotation)
end

-- !Name "Modify scale of a static mesh"
-- !Section "Static mesh parameters"
-- !Description "Set given static mesh scale."
-- !Arguments "Enumeration, [ Change | Set ], 20, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ 0 | 256 | 2 | 1 | 5 ], {1}, 15, Scale value to define", "NewLine, Statics"

LevelFuncs.Engine.Node.SetStaticScale = function(operation, value, staticName)
	local stat = TEN.Objects.GetStaticByName(staticName)

	if (operation == 0) then
		local scale = stat:GetScale().x;
		stat:SetScale(scale + value)
	else
		stat:SetScale(value)
	end
end

-- !Name "Shift static mesh towards its direction"
-- !Section "Static mesh parameters"
-- !Description "Shifts static mesh to a relative distance, towards the direction it is facing."
-- !Arguments "NewLine, Statics, Statics mesh to move, 85"
-- !Arguments "Numerical, [ -65535 | 65535 ], {256}, 15, Distance"

LevelFuncs.Engine.Node.ShiftStatic = function(staticName, distance)
	local static = TEN.Objects.GetStaticByName(staticName)

	local angle = math.rad(static:GetRotation().y)
	local dx = distance * math.sin(angle)
	local dz = distance * math.cos(angle)

	local newPosition = static:GetPosition()

	newPosition.x = newPosition.x + dx
	newPosition.z = newPosition.z + dz

	static:SetPosition(newPosition)
end

-- !Name "Set static mesh color"
-- !Section "Static mesh parameters"
-- !Description "Sets static mesh tint to a given value."
-- !Arguments "NewLine, Statics, 80" "Color, 20, Static mesh color"

LevelFuncs.Engine.Node.SetStaticColor = function(staticName, color)
	color.a = TEN.Objects.GetStaticByName(staticName):GetColor().a
	TEN.Objects.GetStaticByName(staticName):SetColor(color)
end

-- !Name "Set static mesh transparency"
-- !Section "Static mesh parameters"
-- !Description "Sets static mesh transparency to a given value."
-- !Arguments "NewLine, Statics, 80" "Numerical, 20, [ 0 | 255 | 0 | 1 | 5 ], {255}, Static mesh transparency"

LevelFuncs.Engine.Node.SetStaticTransparency = function(staticName, transparency)
	local color = TEN.Objects.GetStaticByName(staticName):GetColor()
	color.a = transparency
	TEN.Objects.GetStaticByName(staticName):SetColor(color)
end

-- !Name "Set static mesh collision state"
-- !Section "Static mesh parameters"
-- !Description "If set, static will be collidable, if not set, it will be traversable."
-- !Arguments "Boolean, 20, {true}, Collidable"  "NewLine, Statics"

LevelFuncs.Engine.Node.SetStaticCollisionStatus = function(solid, staticName)
	TEN.Objects.GetStaticByName(staticName):SetCollidable(solid)
end

-- !Name "Set static mesh collision mode"
-- !Section "Static mesh parameters"
-- !Description "If solid flag is unset, collision will be soft."
-- !Arguments "Boolean, 15, {true}, Solid"  "NewLine, Statics"

LevelFuncs.Engine.Node.SetStaticCollisionMode = function(solid, staticName)
	TEN.Objects.GetStaticByName(staticName):SetSolid(solid)
end

-- !Name "Shatter static mesh"
-- !Section "Static mesh state"
-- !Description "Shatters static mesh.\nAlso applicable to non-shatterable statics."
-- !Arguments "NewLine, Statics"

LevelFuncs.Engine.Node.ShatterStatic = function(staticName)
	TEN.Objects.GetStaticByName(staticName):Shatter()
end

-- !Name "Set hit points for a shatterable static mesh"
-- !Section "Static mesh parameters"
-- !Description "Sets hit points for shatter objects.\nThis node is only applicable to static meshes in SHATTER slots (50-59)"
-- !Arguments "Newline,Statics, 70, Static object."
-- !Arguments "Numerical, 30, [ 0 | 1024 ], Hit Points"

LevelFuncs.Engine.Node.SetShatterHP = function(staticName, HP)
    TEN.Objects.GetStaticByName(staticName):SetHP(HP)
end

