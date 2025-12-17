LevelVars.Engine.Keypad = LevelVars.Engine.Keypad or {}
LevelVars.Engine.Keypad.ActivatedKeypad = nil

local inventoryDelay = 0
local inventoryOpen = false
local setupComplete = false
local inputMode = false

local SOUND_MAP = {
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

local function keypadCreate(object, code)

    local dataName = object .. "_KeypadData"

    local codeS = tostring(code)

    LevelVars.Engine.Keypad[dataName]           = {}
	LevelVars.Engine.Keypad[dataName].Code      = LevelVars.Engine.Keypad[dataName].Code or codeS
    LevelVars.Engine.Keypad[dataName].CodeInput = ""
    LevelVars.Engine.Keypad[dataName].Status    = false
    LevelVars.Engine.Keypad[dataName].CursorX   = 1
    LevelVars.Engine.Keypad[dataName].CursorY   = 1

end

local function closeKeypad(keypadObject)
    
    local dataName = keypadObject .. "_KeypadData"

    local keypad = GetMoveableByName(keypadObject)
    keypad:SetItemFlags(1,0)
    LevelVars.Engine.Keypad[dataName] = nil
    LevelVars.Engine.Keypad.ActivatedKeypad = nil

end

local function activateKeypad(object)

    local keypad = GetMoveableByName(object)

    if keypad:GetItemFlags(0) == 0 then
        Lara:Interact(keypad)
    end

    if Lara:GetAnim() == 197 and Lara:GetFrame() >= 22 and Lara:GetFrame() <= 22 then
        LevelVars.Engine.Keypad.ActivatedKeypad = object
        inventoryOpen = true
    end

    if inventoryOpen == true then
        inventoryDelay = inventoryDelay + 1
        TEN.View.SetPostProcessMode(View.PostProcessMode.MONOCHROME)
        TEN.View.SetPostProcessStrength(1)
        TEN.View.SetPostProcessTint(Color(128,128,128,255))
        TEN.View.DisplayItem.ResetCamera()
        if inventoryDelay >= 2 then

            inventoryOpen = false
            inventoryDelay = 0
            setupComplete = true
            Flow.SetFreezeMode(Flow.FreezeMode.FULL)
        end
    end

end

local function exitKeypad(object, status)

    local dataName = object .. "_KeypadData"
    local keypadObject = TEN.View.DisplayItem.GetItemByName(dataName)

    LevelVars.Engine.Keypad[dataName].Status = status
    keypadObject:Remove()
    LevelVars.Engine.Keypad.ActivatedKeypad = nil
    Flow.SetFreezeMode(Flow.FreezeMode.NONE)
    
end

local function displayInput(object, keypad)
    
    local dataName = object .. "_KeypadData"

    if inputMode then
        local selectedKey = keypad[LevelVars.Engine.Keypad[dataName].CursorY][LevelVars.Engine.Keypad[dataName].CursorX]
        LevelVars.Engine.Keypad[dataName].CodeInput = tostring(selectedKey)
    end

end

-- !Name "Run a keypad (triggers)"
-- !Section "User interface"
-- !Description "Creates a keypad to activate the triggers using Trigger Triggerer."
-- !Arguments "NewLine, 80, Moveables, Keypad Object"
-- !Arguments "Numerical, 20, [ 1000 | 9999 ], Pass code"
-- !Arguments "NewLine, Moveables, Trigger Triggerer object to activate"

LevelFuncs.Engine.Node.KeypadTrigger = function(object, code, triggerer)

    local dataName = object .. "_KeypadData"
    
    if not LevelVars.Engine.Keypad[dataName] then
        keypadCreate(object, code)
        inputMode = false
    end

    if LevelVars.Engine.Keypad[dataName].Status then
        local triggerer = GetMoveableByName(triggerer)
        triggerer:Enable()
        closeKeypad(object)
    end
  
    activateKeypad(object)

end
-- !Name "Run a keypad (volume event)"
-- !Section "User interface"
-- !Description "Creates a keypad to run a volume event."
-- !Arguments "NewLine, 80, Moveables, Keypad Object"
-- !Arguments "Numerical, 20, [ 1000 | 9999 ], Pass code"
-- !Arguments "NewLine, 65, VolumeEventSets, Target event set"
-- !Arguments "VolumeEvents, 35, Event to run"

LevelFuncs.Engine.Node.KeypadVolume = function(object, code, volumeEvent, eventType)

    local dataName = object .. "_KeypadData"
    
    if not LevelVars.Engine.Keypad[dataName] then
        keypadCreate(object, code)
        inputMode = false
    end

    if LevelVars.Engine.Keypad[dataName].Status then
        TEN.Logic.HandleEvent(volumeEvent, eventType, Lara)
        closeKeypad(object)
    end

    activateKeypad(object)

end

-- !Name "Run a keypad (script function)"
-- !Section "User interface"
-- !Description "Creates a keypad to run a script function."
-- !Arguments "NewLine, 80, Moveables, Keypad Object"
-- !Arguments "Numerical, 20, [ 1000 | 9999 ], Pass code"
-- !Arguments "NewLine, LuaScript, Target Lua script function" "NewLine, String, Arguments"

LevelFuncs.Engine.Node.KeypadScript = function(object, code, funcName, args)

    local dataName = object .. "_KeypadData"

    if not LevelVars.Engine.Keypad[dataName] then
        keypadCreate(object, code)
        inputMode = false
    end
    
    if LevelVars.Engine.Keypad[dataName].Status then
        funcName(table.unpack(LevelFuncs.Engine.Node.SplitString(args, ",")))
        closeKeypad(object)
    end

    activateKeypad(object)

end

-- !Name "Run a keypad (Numerical Input)"
-- !Section "User interface"
-- !Description "Creates a keypad to input numerical number."
-- !Arguments "NewLine, Moveables, Keypad Object"
LevelFuncs.Engine.Node.KeypadInput = function(object)

    local dataName = object .. "_KeypadData"

    if not LevelVars.Engine.Keypad[dataName] then
        keypadCreate(object, 1)
        inputMode = true
    end

    activateKeypad(object)

end

-- !Name "If Numerical Input from a keypad is..."
-- !Section "User interface"
-- !Conditional "True"
-- !Description "Read a keypad's number."
-- !Arguments "NewLine, Moveables, Keypad Object"
-- !Arguments "NewLine, CompareOperator, 20"
-- !Arguments "Numerical, 20, [ 0 | 9 ], Input to test"
LevelFuncs.Engine.Node.KeypadRead = function(object, operator, value)

    local dataName = object .. "_KeypadData"

    if not LevelVars.Engine.Keypad[dataName] then
        return false
    end

    if LevelVars.Engine.Keypad[dataName].Status then
        LevelVars.Engine.Keypad.ActivatedKeypad = nil
        local numericString = LevelVars.Engine.Keypad[dataName].CodeInput:gsub("%-", "")
        local numberValue = tonumber(numericString)
        LevelVars.Engine.Keypad[dataName].CodeInput = ""
        LevelVars.Engine.Keypad[dataName].Status = false
        return LevelFuncs.Engine.Node.CompareValue(numberValue, value, operator)
    end
    

end

LevelFuncs.Engine.Node.RunKeypad = function()

    if not LevelVars.Engine.Keypad.ActivatedKeypad then
        return
    end

    if setupComplete then
        TEN.View.SetPostProcessMode(View.PostProcessMode.NONE)
        TEN.View.SetPostProcessStrength(0)
        TEN.View.SetPostProcessTint(Color(255,255,255,255))
        setupComplete = false
    end

    local object = LevelVars.Engine.Keypad.ActivatedKeypad
    local objectSlot = GetMoveableByName(object):GetObjectID()
    local dataName = object .. "_KeypadData"
    local target = TEN.View.DisplayItem(dataName, objectSlot, Vec3(0,2500,1024), Rotation(0,0,0), 4)

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
        TEN.Sound.PlaySound(SOUND_MAP[selectedKey])
        if selectedKey == "Clear" then
            LevelVars.Engine.Keypad[dataName].CodeInput = ""  -- Clear the entered code
            TEN.Sound.PlaySound(SOUND_MAP["Clear"])
        elseif selectedKey == "Enter" then
            if inputMode then
                
                for _, mesh in pairs(meshMappings) do
                    target:SetMeshVisible(mesh.dark, true)
                    target:SetMeshVisible(mesh.bright, false)
                end
                exitKeypad(object, true)
                return
            else
                if LevelVars.Engine.Keypad[dataName].CodeInput == correctCode then
                    TEN.Sound.PlaySound(SOUND_MAP["Success"])
                    for _, mesh in pairs(meshMappings) do
                        target:SetMeshVisible(mesh.dark, true)  -- Show dark keys
                        target:SetMeshVisible(mesh.bright, false)  -- Hide bright keys
                    end
                    exitKeypad(object, true)
                    return
                else
                    TEN.Sound.PlaySound(SOUND_MAP["Failure"])
                    LevelVars.Engine.Keypad[dataName].CodeInput = ""  -- Reset the entered code if incorrect
                end
            end
        else
            if string.len(LevelVars.Engine.Keypad[dataName].CodeInput) < maxCodeLength or inputMode then
                
                if inputMode then
                
                    for _, mesh in pairs(meshMappings) do
                        target:SetMeshVisible(mesh.dark, true)
                        target:SetMeshVisible(mesh.bright, false)
                    end
                    exitKeypad(object, true)
                    return
                end
                
                LevelVars.Engine.Keypad[dataName].CodeInput = LevelVars.Engine.Keypad[dataName].CodeInput .. tostring(selectedKey)

            end
        end
    elseif KeyIsHit(ActionID.FORWARD) then
        LevelVars.Engine.Keypad[dataName].CursorY = LevelVars.Engine.Keypad[dataName].CursorY -1
        TEN.Sound.PlaySound(SOUND_MAP["Click"])
    elseif KeyIsHit(ActionID.BACK)  then
        LevelVars.Engine.Keypad[dataName].CursorY = LevelVars.Engine.Keypad[dataName].CursorY + 1
        TEN.Sound.PlaySound(SOUND_MAP["Click"])
    elseif KeyIsHit(ActionID.LEFT)  then
        LevelVars.Engine.Keypad[dataName].CursorX = LevelVars.Engine.Keypad[dataName].CursorX - 1
        TEN.Sound.PlaySound(SOUND_MAP["Click"])
    elseif KeyIsHit(ActionID.RIGHT) then
        LevelVars.Engine.Keypad[dataName].CursorX = LevelVars.Engine.Keypad[dataName].CursorX + 1
        TEN.Sound.PlaySound(SOUND_MAP["Click"])
        
    elseif KeyIsHit(ActionID.INVENTORY) then
        TEN.Sound.PlaySound(SOUND_MAP["Failure"])
        LevelVars.Engine.Keypad[dataName].CodeInput = ""
        LevelVars.Engine.Keypad[dataName].CursorX = 1
        LevelVars.Engine.Keypad[dataName].CursorY = 1

        for _, mesh in pairs(meshMappings) do
            target:SetMeshVisible(mesh.dark, true)  -- Show dark keys
            target:SetMeshVisible(mesh.bright, false)  -- Hide bright keys
        end

        exitKeypad(object, false)
        return

    end

    -- Clamp cursorX within the valid range of the current row
    LevelVars.Engine.Keypad[dataName].CursorX = math.max(1, math.min(LevelVars.Engine.Keypad[dataName].CursorX, 3))

    -- Clamp cursorY within the total number of rows
    LevelVars.Engine.Keypad[dataName].CursorY = math.max(1, math.min(LevelVars.Engine.Keypad[dataName].CursorY, 4))
    
    --if Keypad is input type show the currently hovering number
    displayInput(object, keypad)

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
    local controlsText = TEN.Strings.DisplayString(codeWithDashes, TEN.Vec2(TEN.Util.PercentToScreen(55, 30)), 1.0, TEN.Color(192,192,192), false, {Strings.DisplayStringOption.RIGHT})
    ShowString(controlsText, 1 / 30)

end

TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PREFREEZE, LevelFuncs.Engine.Node.RunKeypad)