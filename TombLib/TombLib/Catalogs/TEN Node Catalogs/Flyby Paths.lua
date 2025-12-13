local Timer = require("Engine.Timer")

LevelVars.Engine.TransformTimeData = {}

-- !Ignore
-- Construct timed transform data and start transform
LevelFuncs.Engine.Node.ConstructPathTimedData = function(objectName, isStatic, flyby, motionType, startPosition, endPosition, time, rotation, smooth)

	local dataName  = objectName .. "path_transform_data"
	
	if (LevelVars.Engine.TransformTimeData[dataName] ~= nil and Timer.Get(LevelVars.Engine.TransformTimeData[dataName].Name) ~= nil) then
		if (Timer.Get(LevelVars.Engine.TransformTimeData[dataName].Name):IsActive()) then
			return
		else
			Timer.Delete(LevelVars.Engine.TransformTimeData[dataName].Name)
			LevelVars.Engine.TransformTimeData[dataName] = nil
		end
	end

	LevelVars.Engine.TransformTimeData = LevelVars.Engine.TransformTimeData  or {}

	LevelVars.Engine.TransformTimeData[dataName]= {}
	
	LevelVars.Engine.TransformTimeData[dataName].Progress   	= 0
	LevelVars.Engine.TransformTimeData[dataName].Interval   	= 1 / (time * 30)
	LevelVars.Engine.TransformTimeData[dataName].IsStatic   	= isStatic
	LevelVars.Engine.TransformTimeData[dataName].ObjectName 	= objectName
	LevelVars.Engine.TransformTimeData[dataName].Name       	= dataName
	LevelVars.Engine.TransformTimeData[dataName].Flyby			= flyby
	LevelVars.Engine.TransformTimeData[dataName].MotionType		= motionType
	LevelVars.Engine.TransformTimeData[dataName].Rotation   	= rotation
	LevelVars.Engine.TransformTimeData[dataName].Smooth			= smooth
	LevelVars.Engine.TransformTimeData[dataName].NewValue   	= endPosition
	LevelVars.Engine.TransformTimeData[dataName].EndPosition	= endPosition
	LevelVars.Engine.TransformTimeData[dataName].OldValue   	= startPosition
	LevelVars.Engine.TransformTimeData[dataName].StopAtEnd		= false

	local timer = Timer.Create(dataName, 1 / 30, true, false, LevelFuncs.Engine.Node.TransformPathTimedData, dataName)
	timer:Start()
end

-- !Ignore
-- Transform object parameter using previously saved timed transform data
LevelFuncs.Engine.Node.TransformPathTimedData = function(dataName)

	local tolerance = 5e-1 -- 0.5 tolerance. The tolerance to stop the object revolution. It is required in case of smooth rotation.

	if LevelVars.Engine.TransformTimeData[dataName].MotionType == 0 then
		
		LevelVars.Engine.TransformTimeData[dataName].Progress = math.min(LevelVars.Engine.TransformTimeData[dataName].Progress + LevelVars.Engine.TransformTimeData[dataName].Interval, 1)
	
	elseif LevelVars.Engine.TransformTimeData[dataName].MotionType == 1 then
		
		LevelVars.Engine.TransformTimeData[dataName].Progress = (LevelVars.Engine.TransformTimeData[dataName].Progress + LevelVars.Engine.TransformTimeData[dataName].Interval) % 1
	
	elseif LevelVars.Engine.TransformTimeData[dataName].MotionType == 2 then
		
		if LevelVars.Engine.TransformTimeData[dataName].Direction == nil then
			LevelVars.Engine.TransformTimeData[dataName].Direction = 1  -- Start forward
		end
		
		LevelVars.Engine.TransformTimeData[dataName].Progress = LevelVars.Engine.TransformTimeData[dataName].Progress + 
			(LevelVars.Engine.TransformTimeData[dataName].Interval * LevelVars.Engine.TransformTimeData[dataName].Direction)
		
		if LevelVars.Engine.TransformTimeData[dataName].Progress >= 1 then
			LevelVars.Engine.TransformTimeData[dataName].Progress = 1
			LevelVars.Engine.TransformTimeData[dataName].Direction = -1  -- Reverse direction
		elseif LevelVars.Engine.TransformTimeData[dataName].Progress <= 0 then
			LevelVars.Engine.TransformTimeData[dataName].Progress = 0
			LevelVars.Engine.TransformTimeData[dataName].Direction = 1  -- Forward again
		end

	end

	local factor = LevelVars.Engine.TransformTimeData[dataName].Smooth 
			and LevelFuncs.Engine.Node.Smoothstep(LevelVars.Engine.TransformTimeData[dataName].Progress) 
			or LevelVars.Engine.TransformTimeData[dataName].Progress
	
	local newValue = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.TransformTimeData[dataName].OldValue, LevelVars.Engine.TransformTimeData[dataName].NewValue, factor)

	local object = LevelVars.Engine.TransformTimeData[dataName].IsStatic and TEN.Objects.GetStaticByName(LevelVars.Engine.TransformTimeData[dataName].ObjectName) or TEN.Objects.GetMoveableByName(LevelVars.Engine.TransformTimeData[dataName].ObjectName)

	local flybyPos = View.GetFlybyPosition(LevelVars.Engine.TransformTimeData[dataName].Flyby, newValue, LevelVars.Engine.TransformTimeData[dataName].MotionType == 1)
	object:SetPosition(flybyPos)

	if LevelVars.Engine.TransformTimeData[dataName].Rotation == true then
		
		local flybyRot = View.GetFlybyRotation(LevelVars.Engine.TransformTimeData[dataName].Flyby, newValue, LevelVars.Engine.TransformTimeData[dataName].MotionType == 1)
		object:SetRotation(flybyRot)
	
	end

	if LevelVars.Engine.TransformTimeData[dataName].StopAtEnd and  math.abs(newValue - LevelVars.Engine.TransformTimeData[dataName].EndPosition) < tolerance then
        	Timer.Delete(LevelVars.Engine.TransformTimeData[dataName].Name)
        	LevelVars.Engine.TransformTimeData[dataName] = nil
    	elseif LevelVars.Engine.TransformTimeData[dataName].MotionType == 0 and LevelVars.Engine.TransformTimeData[dataName].Progress >= 1 then
        	Timer.Delete(LevelVars.Engine.TransformTimeData[dataName].Name)
        	LevelVars.Engine.TransformTimeData[dataName] = nil
    	end

end

-- !Ignore
--- Delete timed transform data
LevelFuncs.Engine.Node.DeletePathTimedData = function(objectName, endPosition)
    local dataName  = objectName .. "path_transform_data"

    if LevelVars.Engine.TransformTimeData[dataName] ~= nil and Timer.Get(LevelVars.Engine.TransformTimeData[dataName].Name):IsActive() then
        LevelVars.Engine.TransformTimeData[dataName].EndPosition = endPosition
        LevelVars.Engine.TransformTimeData[dataName].StopAtEnd = true
    end
end

-- !Name "Set moveable position on a flyby path"
-- !Section "Flyby paths"
-- !Description "Sets moveable position to a specific point in a flyby path."
-- !Arguments "NewLine, Moveables"
-- !Arguments "NewLine, Numerical, 25, [ 0 | 256 ], Flyby sequence index"
-- !Arguments "Numerical, 25, [ 0 | 100 | 2 | 1 ], {0}, Position percentage (0 to 100)"
-- !Arguments "Boolean, 50, Set Rotation"

LevelFuncs.Engine.Node.SetMoveablePositionOnFlybyPath = function(moveableName, flyby, percentage, rotation)
	
	local object = TEN.Objects.GetMoveableByName(moveableName)
	object:SetPosition(View.GetFlybyPosition(flyby, percentage, false))
	if rotation then
		object:SetRotation(View.GetFlybyRotation(flyby, percentage, false))
	end
end

-- !Name "Translate moveable over flyby path"
-- !Section "Flyby paths"
-- !Description "Gradually translates a moveable over flyby path over specified timespan."
-- !Arguments "NewLine, Moveables, 67" "Numerical, 33, [ 0 | 256 ], Flyby sequence index"
-- !Arguments "NewLine, Enumeration, 34, [ One Shot | Loop | Back and Forth], {0}, Motion Type"
-- !Arguments "Numerical, 33, [ 0 | 100 | 2 | 1 ], {0}, Start position percentage (0 to 100)"
-- !Arguments "Numerical, 33, [ 0 | 100 | 2 | 1 ], {100}, End position percentage (0 to 100)"
-- !Arguments "NewLine, Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 34, Time (in seconds)" 
-- !Arguments "Boolean, 33, Set Rotation"
-- !Arguments "33, Boolean, Smooth motion"

LevelFuncs.Engine.Node.TranslateMoveableOverFlybyTimespan = function(moveableName, flyby, motionType, startPosition, endPosition, time, rotation, smooth)
	
	-- Wrap another node function call into do/end to prevent wrong parsing
	do LevelFuncs.Engine.Node.ConstructPathTimedData(moveableName, false, flyby, motionType, startPosition, endPosition, time, rotation, smooth) end
end

-- !Name "Stop the translation of a moveable"
-- !Section "Flyby paths"
-- !Description "Stops an already active moveable translation loop."
-- !Arguments "NewLine, Moveables, Moveable to stop translation"
-- !Arguments "NewLine, Numerical, [ 0 | 100 | 2 | 1 ], {0}, 20, End Position. This is the position the moveable will stop."

LevelFuncs.Engine.Node.StopMoveableOverFlybyPath = function(moveableName, endPosition)
    -- Wrap another node function call into do/end to prevent wrong parsing
    do LevelFuncs.Engine.Node.DeletePathTimedData(moveableName, endPosition) end
end

-- !Name "Set static mesh position on a flyby path"
-- !Section "Flyby paths"
-- !Description "Sets static mesh position to a specific point in a flyby path."
-- !Arguments "NewLine, Statics"
-- !Arguments "NewLine, Numerical, 25, [ 0 | 256 ], Flyby sequence index"
-- !Arguments "Numerical, 25, [ 0 | 100 | 2 | 1 ], {0}, Position percentage (0 to 100)"
-- !Arguments "Boolean, 50, Set Rotation"

LevelFuncs.Engine.Node.SetStaticPositionOnFlybyPath = function(staticName, flyby, percentage, rotation)
	
	local object = TEN.Objects.GetStaticByName(staticName)
	object:SetPosition(View.GetFlybyPosition(flyby, percentage, false))
	if rotation then
		object:SetRotation(View.GetFlybyRotation(flyby, percentage, false))
	end
end

-- !Name "Translate static mesh over flyby path"
-- !Section "Flyby paths"
-- !Description "Gradually translates a static mesh over flyby path over specified timespan."
-- !Arguments "NewLine, Statics, 67" "Numerical, 33, [ 0 | 256 ], Flyby sequence index"
-- !Arguments "NewLine, Enumeration, 34, [ One Shot | Loop | Back and Forth], {0}, Motion Type"
-- !Arguments "Numerical, 33, [ 0 | 100 | 2 | 1 ], {0}, Start position percentage (0 to 100)"
-- !Arguments "Numerical, 33, [ 0 | 100 | 2 | 1 ], {100}, End position percentage (0 to 100)"
-- !Arguments "NewLine, Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 34, Time (in seconds)" 
-- !Arguments "Boolean, 33, Set Rotation"
-- !Arguments "33, Boolean, Smooth motion"

LevelFuncs.Engine.Node.TranslatStaticOverFlybyTimespan = function(moveableName, flyby, motionType, startPosition, endPosition, time, rotation, smooth)
	
	-- Wrap another node function call into do/end to prevent wrong parsing
	do LevelFuncs.Engine.Node.ConstructPathTimedData(moveableName, true, flyby, motionType, startPosition, endPosition, time, rotation, smooth) end
end

-- !Name "Stop the translation of a static mesh"
-- !Section "Flyby paths"
-- !Description "Stops an already active static mesh translation loop."
-- !Arguments "NewLine, Statics, Static mesh to stop translation"
-- !Arguments "NewLine, Numerical, [ 0 | 100 | 2 | 1 ], {0}, 20, End Position. This is the position the moveable will stop."

LevelFuncs.Engine.Node.StopStaticOverFlybyPath = function(moveableName, endPosition)
    -- Wrap another node function call into do/end to prevent wrong parsing
    do LevelFuncs.Engine.Node.DeletePathTimedData(moveableName, endPosition) end
end
