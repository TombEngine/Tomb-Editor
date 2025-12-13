-- !Name "Run volume event for specified moveables"
-- !Section "Batch actions"
-- !Description "Runs another volume event for moveables matching the criteria, passing every moveable as an activator to that event."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use" "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, VolumeEventSets, 65, Target event set" "VolumeEvents, 35, Event to run"

LevelFuncs.Engine.Node.RunVolumeEventForSpecifiedMoveables = function(objectId, namePart, setName, eventType)
	local moveables = TEN.Objects.GetMoveablesBySlot(objectId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
	        TEN.Logic.HandleEvent(setName, eventType, moveable)
        end
    end
end

-- !Name "Set hit points for specified moveables"
-- !Section "Batch actions"
-- !Description "Set hit points for moveables matching the criteria."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use" "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Numerical, [ 0 | 32767 | 0 ], Hit points to set"

LevelFuncs.Engine.Node.SetHitPointsForSpecifiedMoveables = function(objectId, namePart, hitPoints)
	local moveables = TEN.Objects.GetMoveablesBySlot(objectId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            moveable:SetHP(hitPoints)
        end
    end
end

-- !Name "Set OCB for specified moveables"
-- !Section "Batch actions"
-- !Description "Set OCB for moveables matching the criteria."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use" "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Numerical, [ -32768 | 32767 | 0 ], OCB value to set"

LevelFuncs.Engine.Node.SetOCBForSpecifiedMoveables = function(objectId, namePart, ocb)
	local moveables = TEN.Objects.GetMoveablesBySlot(objectId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            moveable:SetOCB(ocb)
        end
    end
end

-- !Name "Set color for specified moveables"
-- !Section "Batch actions"
-- !Description "Set color for moveables matching the criteria."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use" "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Color, {TEN.Color(128,128,128)}, Color to set"

LevelFuncs.Engine.Node.SetColorForSpecifiedMoveables = function(objectId, namePart, color)
	local moveables = TEN.Objects.GetMoveablesBySlot(objectId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            moveable:SetColor(color)
        end
    end
end

-- !Name "Set animation for specified moveables"
-- !Section "Batch actions"
-- !Description "Set animation for moveables matching the criteria."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use" "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Numerical, [ 0 | 1000 ], Animation ID to set" 

LevelFuncs.Engine.Node.SetAnimationForSpecifiedMoveables = function(objectId, namePart, animID)
	local moveables = TEN.Objects.GetMoveablesBySlot(objectId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            moveable:SetAnim(animID)
        end
    end
end

-- !Name "Set effect for specified moveables"
-- !Section "Batch actions"
-- !Description "Set effect for moveables matching the criteria."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use" "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Enumeration, 80, [ None | Fire | Sparks | Smoke | Electric ignite | Red ignite ], Effect type to set"
-- !Arguments "Numerical, [ 0 | 65535 | 2 | 0.1 | 1 ], {0}, 20, Time (in seconds) after which effect turns off\n0 = infinite" 

LevelFuncs.Engine.Node.SetEffectForSpecifiedMoveables = function(objectId, namePart, effectID, timeout)
	local moveables = TEN.Objects.GetMoveablesBySlot(objectId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            moveable:SetEffect(effectID, timeout)
        end
    end
end

-- !Name "Meshswap specified moveables"
-- !Section "Batch actions"
-- !Description "Meshswap moveables matching the criteria."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use" "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, WadSlots, Meshswap object ID to use"

LevelFuncs.Engine.Node.MeshswapForSpecifiedMoveables = function(objectId, namePart, slotID)
	local moveables = TEN.Objects.GetMoveablesBySlot(objectId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            for mesh = 0, (moveable:GetMeshCount() - 1) do
                moveable:SwapMesh(mesh, slotID)
            end
        end
    end
end

-- !Name "Change position of specified moveables"
-- !Section "Batch actions"
-- !Description "Change position of moveables matching the criteria."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use" "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 65, New position value to define"
-- !Arguments "35, Boolean, Relative coordinates"
-- !Arguments "NewLine, Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 65, Time (in seconds)" 
-- !Arguments "35, Boolean, Smooth motion"

LevelFuncs.Engine.Node.ChangePositionForSpecifiedMoveables = function(objectId, namePart, newPosition, relative, time, smooth)
	local moveables = TEN.Objects.GetMoveablesBySlot(objectId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, moveable in pairs(moveables) do
        local name = moveable:GetName()
        if noNameSearch or (string.find(name, namePart)) then
            LevelFuncs.Engine.Node.ChangeMoveablePositionOverTimespan(name, newPosition, relative, time, smooth)
        end
    end
end

-- !Name "Set hit points for specified static meshes"
-- !Section "Batch actions"
-- !Description "Set hit points for static meshes matching the criteria."
-- !Arguments "NewLine, Numerical, 34, [ 0 | 1000 | 0 ], Static mesh ID to use" "String, 33, Word to search in a static name"
-- !Arguments "Numerical, 33, [ 0 | 32767 | 0 ], Hit points to set"

LevelFuncs.Engine.Node.SetHitPointsForSpecifiedStatics = function(slotId, namePart, hitPoints)
	local statics =  TEN.Objects.GetStaticsBySlot(slotId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, stat in pairs(statics) do
        if noNameSearch or (string.find(stat:GetName(), namePart)) then
            stat:SetHP(hitPoints)
        end
    end
end

-- !Name "Set collision type for specified static meshes"
-- !Section "Batch actions"
-- !Description "Set collision type (soft or hard) for static meshes matching the criteria."
-- !Arguments "NewLine, Numerical, 34, [ 0 | 1000 | 0 ], Static mesh ID to use" "String, 33, Word to search in a static name"
-- !Arguments "Boolean, 33, {true}, Hard collision"

LevelFuncs.Engine.Node.SetCollisionModeForSpecifiedStatics = function(slotId, namePart, softCollision)
	local statics = TEN.Objects.GetStaticsBySlot(slotId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, stat in pairs(statics) do
        if noNameSearch or (string.find(stat:GetName(), namePart)) then
            stat:SetSolid(softCollision)
        end
    end
end

-- !Name "Change position of specified statics"
-- !Section "Batch actions"
-- !Description "Change position of statics matching the criteria."
-- !Arguments "NewLine, Numerical, 34, [ 0 | 1000 | 0 ], Static mesh ID to use" "String, 66, Word to search in a static name"
-- !Arguments "NewLine, Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 65, New position value to define"
-- !Arguments "35, Boolean, Relative coordinates"
-- !Arguments "NewLine, Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 65, Time (in seconds)" 
-- !Arguments "35, Boolean, Smooth motion"
LevelFuncs.Engine.Node.ChangePositionForSpecifiedStatics = function(slotId, namePart, newPosition, relative, time, smooth)
	local statics = TEN.Objects.GetStaticsBySlot(slotId)
    local noNameSearch = LevelFuncs.Engine.Node.StringIsEmpty(namePart)

    for _, stat in pairs(statics) do
        local name = stat:GetName()
        if noNameSearch or (string.find(name, namePart)) then
            LevelFuncs.Engine.Node.ChangeStaticPositionOverTimespan(name, newPosition, relative, time, smooth)
        end
    end
end