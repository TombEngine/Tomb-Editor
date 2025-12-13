LevelVars.Engine.Keypad = LevelVars.Engine.Keypad or {}
LevelVars.Engine.ActivatedKeypad = nil

-- !Name "Create a keypad"
-- !Section "User interface"
-- !Description "Creates a keypad."
-- !Arguments "NewLine, 80, Moveables, Keypad Object"
-- !Arguments "Numerical, 20, [ 1000 | 9999 ], Pass code"
-- !Arguments "NewLine, Volumes, Volume to use for the keypad"

LevelFuncs.Engine.Node.KeypadCreate = function(object, code, volume)

    local dataName = object .. "_KeypadData"

    local codeS = tostring(code)

    LevelVars.Engine.Keypad[dataName]           = {}
	LevelVars.Engine.Keypad[dataName].Code      = LevelVars.Engine.Keypad[dataName].Code or codeS
    LevelVars.Engine.Keypad[dataName].CodeInput = ""
    LevelVars.Engine.Keypad[dataName].Volume    = volume
    LevelVars.Engine.Keypad[dataName].Status    = false
    LevelVars.Engine.Keypad[dataName].CursorX   = 1
    LevelVars.Engine.Keypad[dataName].CursorY   = 1

end

-- !Name "Run a keypad (triggers)"
-- !Section "User interface"
-- !Description "Creates a keypad to activate the triggers using Trigger Triggerer."
-- !Arguments "NewLine, Moveables, Keypad object"
-- !Arguments "NewLine, Moveables, Trigger Triggerer object to activate"

LevelFuncs.Engine.Node.KeypadTrigger = function(object, triggerer)

    local dataName = object .. "_KeypadData"
    
    if LevelVars.Engine.Keypad[dataName].Status then
        local triggerer = GetMoveableByName(triggerer)
        local volume = GetVolumeByName(LevelVars.Engine.Keypad[dataName].Volume)
        triggerer:Enable()
        LevelVars.Engine.Keypad[dataName] = nil
        LevelVars.Engine.ActivatedKeypad = nil
        volume:Disable()
    end
  
    LevelFuncs.Engine.ActivateKeypad(object)

end
-- !Name "Run a keypad (volume event)"
-- !Section "User interface"
-- !Description "Creates a keypad to run a volume event."
-- !Arguments "NewLine, Moveables, Keypad object"
-- !Arguments "NewLine, 65, VolumeEventSets, Target event set"
-- !Arguments "VolumeEvents, 35, Event to run"

LevelFuncs.Engine.Node.KeypadVolume = function(object, volumeEvent, eventType)

    local dataName = object .. "_KeypadData"
    
    if LevelVars.Engine.Keypad[dataName].Status then
        local volume = GetVolumeByName(LevelVars.Engine.Keypad[dataName].Volume)
        LevelVars.Engine.Keypad[dataName] = nil
        LevelVars.Engine.ActivatedKeypad  = nil
        TEN.Logic.HandleEvent(volumeEvent, eventType, Lara)
        volume:Disable()
    end

    LevelFuncs.Engine.ActivateKeypad(object)

end

-- !Name "Run a keypad (script function)"
-- !Section "User interface"
-- !Description "Creates a keypad to run a script function."
-- !Arguments "NewLine, Moveables, Keypad object"
-- !Arguments "NewLine, LuaScript, Target Lua script function" "NewLine, String, Arguments"

LevelFuncs.Engine.Node.KeypadScript = function(object, funcName, args)

    local dataName = object .. "_KeypadData"
    
    if LevelVars.Engine.Keypad[dataName].Status then
        local volume = GetVolumeByName(LevelVars.Engine.Keypad[dataName].Volume)
        LevelVars.Engine.Keypad[dataName] = nil
        LevelVars.Engine.ActivatedKeypad = nil
        funcName(table.unpack(LevelFuncs.Engine.Node.SplitString(args, ",")))
        volume:Disable()
    end

    LevelFuncs.Engine.ActivateKeypad(object)

end

LevelFuncs.Engine.ActivateKeypad = function(object)

    local target = GetMoveableByName(object)

    Lara:Interact(target)

    if Lara:GetAnim() == 197 and Lara:GetFrame() >= 22 and Lara:GetFrame() <= 22 then
        Lara:SetVisible(false)
        View.SetFOV(30)
        LevelVars.Engine.ActivatedKeypad = object
        TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PREFREEZE, LevelFuncs.Engine.RunKeypad)
        Flow.SetFreezeMode(Flow.FreezeMode.SPECTATOR)
    end

end

LevelFuncs.Engine.ExitKeypad = function(object, status)

    local cameraObject = GetMoveableByName("keypadCam1")
    local dataName = object .. "_KeypadData"

    LevelVars.Engine.Keypad[dataName].Status = status
    View.SetFOV(80)
    Lara:SetVisible(true)
    ResetObjCamera()
    cameraObject:Destroy()
    Flow.SetFreezeMode(Flow.FreezeMode.NONE)
    TEN.Logic.RemoveCallback(TEN.Logic.CallbackPoint.PREFREEZE, LevelFuncs.Engine.RunKeypad)

end

LevelFuncs.Engine.RunKeypad = function()

    local soundIDs = {
        ["Clear"] = 983,    -- TR5_Keypad_Hash (Cancel)
        ["Enter"] = 984,    -- TR5_Keypad_Asterisk (Confirm)
        [0] = 985,          -- TR5_Keypad_0
        [1] = 986,          -- TR5_Keypad_1
        [2] = 987,          -- TR5_Keypad_2
        [3] = 988,          -- TR5_Keypad_3
        [4] = 989,          -- TR5_Keypad_4
        [5] = 990,          -- TR5_Keypad_5
        [6] = 991,          -- TR5_Keypad_6
        [7] = 992,          -- TR5_Keypad_7
        [8] = 993,          -- TR5_Keypad_8
        [9] = 994,          -- TR5_Keypad_9
        ["Failure"] = 995,  -- TR5_Keypad_Entry_No
        ["Success"] = 996,  -- TR5_Keypad_Entry_Yes
        ["Click"] = 644,    -- TR2_Click
    }

    local object = LevelVars.Engine.ActivatedKeypad 
    local dataName = object .. "_KeypadData"
    local target = GetMoveableByName(object)
    local targetPos = target:GetPosition()
    local targetRot = target:GetRotation()
    local targetRoom = target:GetRoomNumber()

    local offset = 296
    local heightOffset = 618
    local cameraPos = targetPos
	
    if (targetRot.y == 0) then
        cameraPos = Vec3(targetPos.x, targetPos.y-heightOffset, targetPos.z - offset)
    elseif (targetRot.y == 90) then
        cameraPos = Vec3(targetPos.x- offset, targetPos.y-heightOffset, targetPos.z)
    elseif (targetRot.y == 180) then
        cameraPos = Vec3(targetPos.x, targetPos.y-heightOffset, targetPos.z + offset)
    elseif (targetRot.y == 270) then
        cameraPos = Vec3(targetPos.x+ offset, targetPos.y-heightOffset, targetPos.z )
    end

    if not IsNameInUse("keypadCam1")  then
        Moveable(TEN.Objects.ObjID.CAMERA_TARGET, "keypadCam1", cameraPos, Rotation(0,0,0), targetRoom)
    end

    local cameraObject = GetMoveableByName("keypadCam1")

    cameraObject:SetPosition(cameraPos)
    cameraObject:SetRoomNumber(targetRoom)
    cameraObject:AttachObjCamera(0, target, 0)

    local keypad = {
        {1, 2, 3},
        {4, 5, 6},
        {7, 8, 9},
        {"Clear", 0, "Enter"}
    }

    -- Mesh mappings (1-12 dark keys, 13-24 bright keys)
    local meshMappings = {
        [1] = {dark = 13, bright = 1}, [2] = {dark = 14, bright = 2}, [3] = {dark = 15, bright = 3},
        [4] = {dark = 16, bright = 4}, [5] = {dark = 17, bright = 5}, [6] = {dark = 18, bright = 6},
        [7] = {dark = 19, bright = 7}, [8] = {dark = 20, bright = 8}, [9] = {dark = 21, bright = 9},
        ["Clear"] = {dark = 22, bright = 10}, [0] = {dark = 23, bright = 11}, ["Enter"] = {dark = 24, bright = 12}
    }
 
    -- Starting cursor position
    local correctCode = LevelVars.Engine.Keypad[dataName].Code
    local maxCodeLength = string.len(correctCode)

    if KeyIsHit(ActionID.ACTION) then
        local selectedKey = keypad[LevelVars.Engine.Keypad[dataName].CursorY][LevelVars.Engine.Keypad[dataName].CursorX]
        TEN.Sound.PlaySound(soundIDs[selectedKey])
        if selectedKey == "Clear" then
            LevelVars.Engine.Keypad[dataName].CodeInput = ""  -- Clear the entered code
            TEN.Sound.PlaySound(soundIDs["Clear"])
        elseif selectedKey == "Enter" then
            if LevelVars.Engine.Keypad[dataName].CodeInput == correctCode then
                TEN.Sound.PlaySound(soundIDs["Success"])
                for _, mesh in pairs(meshMappings) do
                    target:SetMeshVisible(mesh.dark, true)  -- Show dark keys
                    target:SetMeshVisible(mesh.bright, false)  -- Hide bright keys
                end
                LevelFuncs.Engine.ExitKeypad(object, true)
                return
            else
                TEN.Sound.PlaySound(soundIDs["Failure"])
                LevelVars.Engine.Keypad[dataName].CodeInput = ""  -- Reset the entered code if incorrect
            end
        else
            if string.len(LevelVars.Engine.Keypad[dataName].CodeInput) < maxCodeLength then
                LevelVars.Engine.Keypad[dataName].CodeInput = LevelVars.Engine.Keypad[dataName].CodeInput .. tostring(selectedKey)
            end
        end
    elseif KeyIsHit(ActionID.FORWARD) then
        LevelVars.Engine.Keypad[dataName].CursorY = LevelVars.Engine.Keypad[dataName].CursorY -1
        TEN.Sound.PlaySound(soundIDs["Click"])
    elseif KeyIsHit(ActionID.BACK)  then
        LevelVars.Engine.Keypad[dataName].CursorY = LevelVars.Engine.Keypad[dataName].CursorY + 1
        TEN.Sound.PlaySound(soundIDs["Click"])
    elseif KeyIsHit(ActionID.LEFT)  then
        LevelVars.Engine.Keypad[dataName].CursorX = LevelVars.Engine.Keypad[dataName].CursorX - 1
        TEN.Sound.PlaySound(soundIDs["Click"])
    elseif KeyIsHit(ActionID.RIGHT) then
        LevelVars.Engine.Keypad[dataName].CursorX = LevelVars.Engine.Keypad[dataName].CursorX + 1
        TEN.Sound.PlaySound(soundIDs["Click"])
    elseif KeyIsHit(ActionID.INVENTORY) then
        TEN.Sound.PlaySound(soundIDs["Failure"])
        LevelVars.Engine.Keypad[dataName].CodeInput = ""
        LevelVars.Engine.Keypad[dataName].CursorX = 1
        LevelVars.Engine.Keypad[dataName].CursorY = 1

        for _, mesh in pairs(meshMappings) do
            target:SetMeshVisible(mesh.dark, true)  -- Show dark keys
            target:SetMeshVisible(mesh.bright, false)  -- Hide bright keys
        end

        LevelFuncs.Engine.ExitKeypad(object, false)
        return

    end

    -- Clamp cursorX within the valid range of the current row
    LevelVars.Engine.Keypad[dataName].CursorX = math.max(1, math.min(LevelVars.Engine.Keypad[dataName].CursorX, 3))

    -- Clamp cursorY within the total number of rows
    LevelVars.Engine.Keypad[dataName].CursorY = math.max(1, math.min(LevelVars.Engine.Keypad[dataName].CursorY, 4))

    -- Function to format entered code with dashes
        local codeWithDashes = LevelVars.Engine.Keypad[dataName].CodeInput or ""
        while string.len(codeWithDashes) < maxCodeLength do
            codeWithDashes = codeWithDashes .. "-"
        end

    -- Function to update mesh visibility
    for y = 1, #keypad do
        for x = 1, #keypad[y] do
            local key = keypad[y][x]
            if meshMappings[key] then
                if x == LevelVars.Engine.Keypad[dataName].CursorX and y == LevelVars.Engine.Keypad[dataName].CursorY then
                    -- Highlight the selected key (bright mesh)
                    target:SetMeshVisible(meshMappings[key].dark, false)
                    target:SetMeshVisible(meshMappings[key].bright, true)
                else
                    -- Show dark keys for others
                    target:SetMeshVisible(meshMappings[key].dark, true)
                    target:SetMeshVisible(meshMappings[key].bright, false)
                end
            end
        end
    end

    -- Display entered code with dashes
    local controlsText = TEN.Strings.DisplayString(codeWithDashes, TEN.Vec2(TEN.Util.PercentToScreen(57.5, 19.5)), 1.60, TEN.Color(255,255,255), false, {Strings.DisplayStringOption.RIGHT})
    ShowString(controlsText, 1 / 30)

end
