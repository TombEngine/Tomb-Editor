-- !Name "If flipmap is active..."
-- !Section "Rooms"
-- !Description "Checks if specified flipmap is active or not. If flipmap is set to -1, checks if any flipmap is active."
-- !Conditional "True"
-- !Arguments "Numerical, 15, [ -1 | 254 | 0 ]"

LevelFuncs.Engine.Node.GetFlipMapStatus = function(flipmapNumber)
	return TEN.Flow.GetFlipMapStatus(flipmapNumber)
end

-- !Name "If room is enabled..."
-- !Section "Rooms"
-- !Description "Checks if room is enabled (disabled means it's in flipped state)."
-- !Conditional "True"
-- !Arguments "NewLine, Rooms"

LevelFuncs.Engine.Node.TestRoomEnabled = function(roomName)
	return TEN.Objects.GetRoomByName(roomName):GetActive()
end

-- !Name "If room flag is set..."
-- !Section "Rooms"
-- !Description "Checks if specified room flag is set."
-- !Conditional "True"
-- !Arguments "NewLine, Rooms, 70"
-- !Arguments "Enumeration, [ Water | Quicksand | Skybox | Wind | Cold | Damage | No lensflare ], 30, Flag type"

LevelFuncs.Engine.Node.TestRoomFlag = function(roomName, flag)
	local flagIndex = LevelFuncs.Engine.Node.GetRoomFlag(flag)
	return TEN.Objects.GetRoomByName(roomName):GetFlag(flagIndex)
end

-- !Name "If room reverb type is..."
-- !Section "Rooms"
-- !Description "Checks if room reverb type is equal to specified."
-- !Conditional "True"
-- !Arguments "NewLine, Rooms, 75"
-- !Arguments "Enumeration, [ None | Small | Medium | Large | Pipe ], 25, Reverb type"

LevelFuncs.Engine.Node.TestRoomReverbType = function(roomName, reverbType)
	return (TEN.Objects.GetRoomByName(roomName):GetReverbType() == reverbType)
end

-- !Name "If room tag is present..."
-- !Section "Rooms"
-- !Description "Checks if room has specified tag."
-- !Conditional "True"
-- !Arguments "NewLine, Rooms, 70"
-- !Arguments "String, Tag to search"

LevelFuncs.Engine.Node.TestRoomTag = function(roomName, tag)
	return TEN.Objects.GetRoomByName(roomName):IsTagPresent(tag)
end

-- !Name "Set property flag for a room"
-- !Section "Rooms"
-- !Description "Sets or unsets selected room property flag.\nWater and Quicksand properties can't be combined.'"
-- !Arguments "NewLine, Rooms, 70"
-- !Arguments "Enumeration, [ Water | Quicksand | Skybox | Wind | Cold | Damage | No lensflare ], 25, Flag type"
-- !Arguments "Boolean, 5"

LevelFuncs.Engine.Node.SetRoomFlag = function(roomName, flag, value)
	local flagIndex = LevelFuncs.Engine.Node.GetRoomFlag(flag)
	TEN.Objects.GetRoomByName(roomName):SetFlag(flagIndex, value)
end

-- !Name "Set property flag for all rooms by tag"
-- !Section "Rooms"
-- !Description "Sets property flag for all rooms with specified tag."
-- !Arguments "NewLine, String, 70, Tag to search for"
-- !Arguments "Enumeration, [ Water | Quicksand | Skybox | Wind | Cold | Damage | No lensflare ], 25, Flag type"
-- !Arguments "Boolean, 5"

LevelFuncs.Engine.Node.SetRoomFlagByTag = function(tag, flag, value)
	local flagIndex = LevelFuncs.Engine.Node.GetRoomFlag(flag)
	local list = TEN.Objects.GetRoomsByTag(tag)

	for k, room in pairs(list) do
		room:SetFlag(flagIndex, value)
	end
end

-- !Name "Set reverb type for a room"
-- !Section "Rooms"
-- !Description "Sets selected room reverb type to specified."
-- !Arguments "NewLine, Rooms, 75"
-- !Arguments "Enumeration, [ None | Small | Medium | Large | Pipe ], 25, Reverb type"

LevelFuncs.Engine.Node.SetRoomReverbType = function(roomName, reverbType)
	TEN.Objects.GetRoomByName(roomName):SetReverbType(reverbType)
end

-- !Name "Set reverb type for all rooms by tag"
-- !Section "Rooms"
-- !Description "Sets reverb type to specified for all rooms with specified tag."
-- !Arguments "NewLine, String, 75, Tag to search for"
-- !Arguments "Enumeration, [ None | Small | Medium | Large | Pipe ], 25, Reverb type"

LevelFuncs.Engine.Node.SetRoomReverbTypeByTag = function(tag, reverbType)
	local list = TEN.Objects.GetRoomsByTag(tag)

	for k, room in pairs(list) do
		room:SetReverbType(reverbType)
	end
end

-- !Name "Toggle flipmap"
-- !Section "Rooms"
-- !Description "Sets specified flipmap number, if it's not set, or unsets it if it is."
-- !Arguments "Numerical, 15, [ 0 | 254 | 0 ]"

LevelFuncs.Engine.Node.ToggleFlipMap = function(flipmapNumber)
	TEN.Flow.FlipMap(flipmapNumber)
end
