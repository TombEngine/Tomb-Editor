--Revolution Nodes by TrainWreck; Code references by Lwmte, Adngel. Shoutout to DaviDMRR, RichardTRLE and Stranger1992 for the help.

local Timer = require("Engine.Timer")

-- !Ignore
-- Construct timed transform data and start transform
LevelFuncs.Engine.Node.ConstructRevolveData = function(objectName, objectCentre, dataType, radius, time, startAngle, endAngle, isLoop, isCCW, isSmooth, isRotate, isCRotate, SFX)

    local dataName  = objectName .. "_revolve_data"
    local revolutionAngle = 360  -- One full revolution is 360 degrees.
    
    if LevelVars[dataName] ~= nil and Timer.Get(LevelVars[dataName].Name) ~= nil then
        if Timer.Get(LevelVars[dataName].Name):IsActive() then
            return
        end
        Timer.Delete(LevelVars[dataName].Name)
        LevelVars[dataName] = nil
    end

    LevelVars[dataName] = {}
    LevelVars[dataName].Progress   = 0
    LevelVars[dataName].Interval   = 1 / (time * 30)
    LevelVars[dataName].DataType   = dataType
    LevelVars[dataName].Radius     = radius
    LevelVars[dataName].ObjectName = objectName
    LevelVars[dataName].CentreName = objectCentre
    LevelVars[dataName].Name       = dataName
    LevelVars[dataName].StartAngle = startAngle
    LevelVars[dataName].EndAngle   = endAngle
    LevelVars[dataName].CCW        = isCCW     -- Flips the direction of the loop if loop is ticked.
    LevelVars[dataName].Loop       = isLoop    -- Loops the object continuously starting from start angle.
    LevelVars[dataName].Smooth     = isSmooth  -- Used for smooth start and stop. If loop is used then the smooth start and stop is at the start angle.
    LevelVars[dataName].Rotate     = isRotate  -- Rotate the object itself. Only used for Horizontal rotation
    LevelVars[dataName].CRotate    = isCRotate -- to rotate the center object
    LevelVars[dataName].Sfx        = SFX
    LevelVars[dataName].OldValue   = startAngle
    LevelVars[dataName].StopAtEnd  = false
    
    LevelVars[dataName].NewValue = LevelVars[dataName].Loop
    and (startAngle + (LevelVars[dataName].CCW and -revolutionAngle or revolutionAngle))
    or endAngle
    
    local timer = Timer.Create(dataName, 1 / 30, true, false, LevelFuncs.Engine.Node.TransformRevolveData, dataName)
    timer:Start()
end

-- !Ignore
-- Transform object parameter using previously saved timed transform data
LevelFuncs.Engine.Node.TransformRevolveData = function(dataName)
   
    -- Smoothly loop the progress value
    LevelVars[dataName].Progress = LevelVars[dataName].Loop
    and (LevelVars[dataName].Progress + LevelVars[dataName].Interval) % 1
    or math.min(LevelVars[dataName].Progress + LevelVars[dataName].Interval, 1)
    
    -- Function to normalize the angles to 360 degree.
    local function NormalizeAngle(angle)
        return (angle % 360 + 360) % 360
    end
    
    local factor = LevelVars[dataName].Smooth and LevelFuncs.Engine.Node.Smoothstep(LevelVars[dataName].Progress) or LevelVars[dataName].Progress
    local newValue1 = LevelFuncs.Engine.Node.Lerp(LevelVars[dataName].OldValue, LevelVars[dataName].NewValue, factor)
    local object2 = TEN.Objects.GetMoveableByName(LevelVars[dataName].ObjectName)
    local object2pos = object2:GetPosition()
    local center2 = TEN.Objects.GetMoveableByName(LevelVars[dataName].CentreName)
    local center2pos = center2:GetPosition()
    local center2rot = center2:GetRotation()
    local radius2 = LevelVars[dataName].Radius
    local angle = newValue1 * math.pi / 180
    local x = center2pos.x
    local y = center2pos.y
    local z = center2pos.z
    local normalizedNewValue1 = NormalizeAngle(newValue1)
    local normalizedEndAngle = NormalizeAngle(LevelVars[dataName].EndAngle)
    local tolerance = 5e-1 -- 0.5 tolerance. The tolerance to stop the object revolution. It is required in case of smooth rotation.
    
    if (LevelVars[dataName].DataType == 0) then
        if (center2rot.y == 0) then
            local ptx, pty = x + radius2 * math.sin(angle), y - radius2 * math.cos(angle)
            object2:SetPosition(Vec3(ptx, pty, object2pos.z))
            
            if not IsSoundPlaying(LevelVars[dataName].Sfx) then
                TEN.Sound.PlaySound(LevelVars[dataName].Sfx, object2pos)
            end
            
            if LevelVars[dataName].CRotate then    
                center2:SetRotation(Rotation(0, 0, newValue1))
            end
            
        elseif (center2rot.y == 90) then
            local ptz, pty = z + radius2 * math.sin(-angle), y - radius2 * math.cos(-angle)
            object2:SetPosition(Vec3(object2pos.x, pty, ptz))
            
            if not IsSoundPlaying(LevelVars[dataName].Sfx) then
                TEN.Sound.PlaySound(LevelVars[dataName].Sfx, object2pos)
            end
            
            if LevelVars[dataName].CRotate then    
                center2:SetRotation(Rotation(0, center2rot.y, newValue1))
            end
            
        elseif (center2rot.y == 180) then
            local ptx, pty = x + radius2 * math.sin(-angle), y - radius2 * math.cos(-angle)
            object2:SetPosition(Vec3(ptx, pty, object2pos.z))
            
            if not IsSoundPlaying(LevelVars[dataName].Sfx) then
                TEN.Sound.PlaySound(LevelVars[dataName].Sfx, object2pos)
            end
            
            if LevelVars[dataName].CRotate then    
                center2:SetRotation(Rotation(0, center2rot.y, newValue1))
            end    
            
        elseif (center2rot.y == 270) then
            local ptz, pty = z + radius2 * math.sin(angle), y - radius2 * math.cos(angle)
            object2:SetPosition(Vec3(object2pos.x, pty, ptz))
            
            if not IsSoundPlaying(LevelVars[dataName].Sfx) then
                TEN.Sound.PlaySound(LevelVars[dataName].Sfx, object2pos)
            end
            
            if LevelVars[dataName].CRotate then    
                center2:SetRotation(Rotation(0, center2rot.y, newValue1))
            end
        end
    elseif (LevelVars[dataName].DataType == 1) then
        local ptz, ptx = z + radius2 * math.sin(-angle), x + radius2 * math.cos(-angle)
        object2:SetPosition(Vec3(ptx, object2pos.y, ptz))
        
        if not IsSoundPlaying(LevelVars[dataName].Sfx) then
            TEN.Sound.PlaySound(LevelVars[dataName].Sfx, object2pos)
        end
        
        if LevelVars[dataName].CRotate then    
            center2:SetRotation(Rotation(0, newValue1, 0))
        end
        
        if LevelVars[dataName].Rotate then    
            object2:SetRotation(Rotation(0, newValue1, 0))
        end
    end

    if LevelVars[dataName].StopAtEnd and  math.abs(normalizedNewValue1 - normalizedEndAngle) < tolerance then
        Timer.Delete(LevelVars[dataName].Name)
        LevelVars[dataName] = nil
    elseif not LevelVars[dataName].Loop and LevelVars[dataName].Progress >= 1 then
        Timer.Delete(LevelVars[dataName].Name)
        LevelVars[dataName] = nil
    end
end

-- !Ignore
--- Delete timed transform data
LevelFuncs.Engine.Node.DeleteTimedData = function(objectName, endAngle)
    local dataName = objectName .. "_revolve_data"

    if LevelVars[dataName] ~= nil and Timer.Get(LevelVars[dataName].Name):IsActive() then
        LevelVars[dataName].EndAngle = endAngle
        LevelVars[dataName].StopAtEnd = true
    end
end

-- !Name "Revolve a moveable"
-- !Section "Timespan actions"
-- !Description "Rotate a moveable around another moveable over specified timespan."
-- !Arguments "NewLine, Moveables, Moveable to rotate"
-- !Arguments "NewLine, Moveables, Centerpoint moveable"
-- !Arguments "NewLine, Enumeration, [ Vertical | Horizontal], 20, Select Axis of Rotation."
-- !Arguments "Numerical, [ 1 | 65535 | 2 | 1 | 1 ], {1}, 20, Radius of Rotation"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 20, Time (in seconds)"
-- !Arguments "Numerical, [ -360 | 360 | 2 | 1 | 1 ], {0}, 20, Start Angle"
-- !Arguments "Numerical, [ -360 | 360 | 2 | 1 | 1 ], {360}, 20, End Angle. The angle the moveable will stop if loop is not ticked."
-- !Arguments "NewLine, 33, Boolean, Loop"
-- !Arguments "33, Boolean, Flip Loop"
-- !Arguments "33, Boolean, Smooth"
-- !Arguments "NewLine, 65, Boolean, Rotate (Y axis when Horizontal selected)"
-- !Arguments "35, Boolean, Rotate centerpoint"
-- !Arguments "NewLine, SoundEffects,{635}"

LevelFuncs.Engine.Node.ChangeMoveableRevolutionOverTimespan = function(moveableName, centrepoint, option, radius, time, startAngle, endAngle, isLoop, isCCW, isSmooth, isRotate, isCRotate, SFX)
    -- Wrap another node function call into do/end to prevent wrong parsing
    do LevelFuncs.Engine.Node.ConstructRevolveData(moveableName, centrepoint, option, radius, time, startAngle, endAngle, isLoop, isCCW, isSmooth, isRotate,  isCRotate, SFX) end
end

-- !Name "Stop the revolution of a moveable"
-- !Section "Timespan actions"
-- !Description "Stop an already active moveable rotation."
-- !Arguments "NewLine, Moveables, Moveable to stop rotation"
-- !Arguments "NewLine, Numerical, [ -360 | 360 | 2 | 1 | 1 ], {360}, 20, End Angle. This is the angle the moveable will stop."

LevelFuncs.Engine.Node.StopMoveableRotation = function(moveableName, endAngle)
    -- Wrap another node function call into do/end to prevent wrong parsing
    do LevelFuncs.Engine.Node.DeleteTimedData(moveableName, endAngle) end
end
