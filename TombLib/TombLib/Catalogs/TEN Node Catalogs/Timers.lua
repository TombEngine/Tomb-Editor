local Timer = require("Engine.Timer")
local textOp = {[0] = "+", [1] = "-", [2] = "*", [3] = "/", [4] = "=", }
local textCompareOp = {[0] = "equal to", [1] = "greater than", [2] = "less than", [3] = "greater than or equal to", [4] = "less than or equal to", [5] = "not equal to", }
LevelVars.nodeTimers = {}

local SetTimer = function (name, debug, alignment, effects, color, pColor, x, y, scale)
    Timer.Get(name):SetUnpausedColor(color)
    Timer.Get(name):SetPausedColor(pColor)
    Timer.Get(name):SetPosition(x,y)
    Timer.Get(name):SetScale(scale)
    Timer.Get(name):SetTextOption(LevelFuncs.Engine.Node.GeneratesTextOption(alignment, effects))
    LevelVars.nodeTimers[name] = {debug = debug}
    LevelVars.nodeTimers[name].alignment = alignment
    LevelVars.nodeTimers[name].effects = effects
end

local CreateStruct = function (name)
    if not LevelVars.nodeTimers[name] then
        LevelVars.nodeTimers[name] = {}
    end
end

-- !Name "Create basic timer"
-- !Conditional "False"
-- !Description "Creates a simple countdown.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 2 | 0.1 | 1 ], The duration of the timer in seconds (internally rounded to the nearest game frame)"
-- !Arguments "Boolean , 13, {false}, Loop"
-- !Arguments "NewLine, Boolean, 25, {false}, Hours"
-- !Arguments "Boolean, 25, {true}, Minutes"
-- !Arguments "Boolean , 25, {true}, Seconds"
-- !Arguments "Boolean, 25, {true}, Centiseconds"
-- !Arguments "NewLine, Color, 20, {TEN.Color(255, 255, 255)}, Timer's color"
-- !Arguments "Color, 20, {TEN.Color(255, 255, 0)}, Timer's paused color"
-- !Arguments "Numerical, 20, {50}, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, {90}, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Enumeration, 20, {1}, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "NewLine, Enumeration, 35, {1}, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "Numerical, 16, {1}, [ 0.1 | 100 | 2 | 0.1 ], Scale\nRange [0.1 to 100]"
-- !Arguments "Boolean , 47, {false}, Debug messages in console"
LevelFuncs.Engine.Node.CreateTimer = function(name, time, loop, hours, minutes, seconds, deciseconds, color, pColor, x, y, alignment, effects, scale, debug)
    if name ~= '' then
        local nodeTimerFormat = {hours = hours, minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        Timer.Create(name, time, loop, nodeTimerFormat)
        SetTimer(name, debug, alignment, effects, color, pColor, x, y, scale)
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer '" .. name .. "' created successfully!", TEN.Util.LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create basic timer' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Create timer with function"
-- !Conditional "False"
-- !Description "Creates a countdown which will execute a `LevelFuncs` lua function upon ending.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 2 | 0.1 | 1 ], The duration of the timer in seconds (internally rounded to the nearest game frame)"
-- !Arguments "Boolean , 13, {false}, Loop"
-- !Arguments "NewLine, Boolean, 25, {false}, Hours"
-- !Arguments "Boolean, 25, {true}, Minutes"
-- !Arguments "Boolean , 25, {true}, Seconds"
-- !Arguments "Boolean, 25, {true}, Centiseconds"
-- !Arguments "NewLine, Color, 20, {TEN.Color(255, 255, 255)}, Timer's color"
-- !Arguments "Color, 20, {TEN.Color(255, 255, 0)}, Timer's paused color"
-- !Arguments "Numerical, 20, {50}, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, {90}, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Enumeration, 20, {1}, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "NewLine, Enumeration, 35, {1}, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "Numerical, 16, {1}, [ 0.1 | 100 | 2 | 0.1 ], Scale\nRange [0.1 to 100]"
-- !Arguments "Boolean , 47, {false}, Debug messages in console"
-- !Arguments "NewLine, LuaScript, The function to call when the time is up"
-- !Arguments "NewLine, String, Arguments"
LevelFuncs.Engine.Node.CreateTimerWithFunction = function(name, time, loop, hours, minutes, seconds, deciseconds, color, pColor, x, y, alignment, effects, scale, debug, luaFunction, args)
    if name ~= '' then
        local nodeTimerFormat = {hours = hours, minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        local argsTable = args ~= '' and table.unpack(LevelFuncs.Engine.Node.SplitString(args, ",")) or nil
        Timer.Create(name, time, loop, nodeTimerFormat, luaFunction, argsTable)
        SetTimer(name, debug, alignment, effects, color, pColor, x, y, scale)
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer with Function '" .. name .. "' created successfully!", TEN.Util.LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create timer with function' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Create timer with volume event set"
-- !Conditional "False"
-- !Description "Creates a countdown that triggers a volume event set upon ending.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 2 | 0.1 | 1 ], The duration of the timer in seconds (internally rounded to the nearest game frame)"
-- !Arguments "Boolean , 13, {false}, Loop"
-- !Arguments "NewLine, Boolean, 25, {false}, Hours"
-- !Arguments "Boolean, 25, {true}, Minutes"
-- !Arguments "Boolean , 25, {true}, Seconds"
-- !Arguments "Boolean, 25, {true}, Centiseconds"
-- !Arguments "NewLine, Color, 20, {TEN.Color(255, 255, 255)}, Timer's color"
-- !Arguments "Color, 20, {TEN.Color(255, 255, 0)}, Timer's paused color"
-- !Arguments "Numerical, 20, {50}, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, {90}, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Enumeration, 20, {1}, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "NewLine, Enumeration, 35, {1}, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "Numerical, 16, {1}, [ 0.1 | 100 | 2 | 0.1 ], Scale\nRange [0.1 to 100]"
-- !Arguments "Boolean , 47, {false}, Debug messages in console"
-- !Arguments "NewLine, 66, VolumeEventSets, The event set to be called when the time is up"
-- !Arguments "VolumeEvents, 34, Event to run"
-- !Arguments "NewLine, Moveables, Activator for the event (when necessary)"
LevelFuncs.Engine.Node.CreateTimerWithEventSet = function(name, time, loop, hours, minutes, seconds, deciseconds, color, pColor, x, y, alignment, effects, scale, debug, setName, eventType, activator)
    if name ~= '' then
        local nodeTimerFormat = {hours = hours, minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        Timer.Create(name, time, loop, nodeTimerFormat, LevelFuncs.Engine.Node.RunEventSet, setName, eventType, activator)
        SetTimer(name, debug, alignment, effects, color, pColor, x, y, scale)
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer with volume event set '" .. name .. "' created successfully", TEN.Util.LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create timer with volume event set' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Create timer with global event set"
-- !Conditional "False"
-- !Description "Creates a countdown that triggers a global event set upon ending.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 2 | 0.1 | 1 ], The duration of the timer in seconds (internally rounded to the nearest game frame)"
-- !Arguments "Boolean , 13, {false}, Loop"
-- !Arguments "NewLine, Boolean, 25, {false}, Hours"
-- !Arguments "Boolean, 25, {true}, Minutes"
-- !Arguments "Boolean , 25, {true}, Seconds"
-- !Arguments "Boolean, 25, {true}, Centiseconds"
-- !Arguments "NewLine, Color, 20, {TEN.Color(255, 255, 255)}, Timer's color"
-- !Arguments "Color, 20, {TEN.Color(255, 255, 0)}, Timer's paused color"
-- !Arguments "Numerical, 20, {50}, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, {90}, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Enumeration, 20, {1}, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "NewLine, Enumeration, 35, {1}, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "Numerical, 16, {1}, [ 0.1 | 100 | 2 | 0.1 ], Scale\nRange [0.1 to 100]"
-- !Arguments "Boolean , 47, {false}, Debug messages in console"
-- !Arguments "NewLine, 70, GlobalEventSets, The event set to be called when the time is up"
-- !Arguments "GlobalEvents, 30, Event to run"
-- !Arguments "NewLine, Moveables, Activator for the event (when necessary)"
LevelFuncs.Engine.Node.CreateTimerWithGEventSet = function(name, time, loop, hours, minutes, seconds, deciseconds, color, pColor, x, y, alignment, effects, scale, debug, setName, eventType, activator)
    if name ~= '' then
        local nodeTimerFormat = {hours = hours, minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        Timer.Create(name, time, loop, nodeTimerFormat, LevelFuncs.Engine.Node.RunGlobalEventSet, setName, eventType, activator)
        SetTimer(name, debug, alignment, effects, color, pColor, x, y, scale)
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer with global event set '" .. name .. "' created successfully", TEN.Util.LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create timer with global event set' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Start timer"
-- !Conditional "False"
-- !Description "Begins or resumes a timer.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
-- !Arguments "NewLine, Boolean , Reset timer when expired"
LevelFuncs.Engine.Node.StartTimer = function(name, reset)
    if name ~= '' then
        if Timer.IfExists(name) then
            if Timer.Get(name):IsPaused() and Timer.Get(name):IsActive() then
                Timer.Get(name):Start()
            else
                Timer.Get(name):Start(reset)
            end
            CreateStruct(name)
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' has started", TEN.Util.LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Start Timer' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Stop timer"
-- !Conditional "False"
-- !Description "Stops a timer.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.StopTimer = function(name)
    if name ~= '' then
        if Timer.IfExists(name) then
            Timer.Get(name):Stop()
            CreateStruct(name)
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' has been stopped", TEN.Util.LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Stop Timer' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Pause timer"
-- !Conditional "False"
-- !Description "Pauses a timer.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.SetPausedTimer = function(name)
    if name ~= '' then
        if Timer.IfExists(name) then
            Timer.Get(name):SetPaused(true)
            CreateStruct(name)
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' has been paused", TEN.Util.LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Pause timer' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Modify remaining time"
-- !Conditional "False"
-- !Description "Changes the remaining time value (in seconds) of a specific timer.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 10, [ + | - | * | / | = ], {4}, Mathematical operation to perform"
-- !Arguments "Numerical, 20, [ 0 | 1000 | 2 | 0.1 | 1 ], New time remaining in seconds (internally rounded to the nearest game frame)"
LevelFuncs.Engine.Node.SetRemainingTime = function(name, operator, remainingTime)
    if name ~= '' then
        if Timer.IfExists(name) then
            if (operator == 4) then
                Timer.Get(name):SetRemainingTime(remainingTime)
            else
                local value = Timer.Get(name):GetRemainingTimeInSeconds()
                Timer.Get(name):SetRemainingTime(LevelFuncs.Engine.Node.ModifyValue(remainingTime, value, operator))
            end
            CreateStruct(name)
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Set remaining time of '" .. name .. "' timer " .. textOp[operator] .. remainingTime .. ". Remaining time : " .. Timer.Get(name):GetRemainingTimeInSeconds(), TEN.Util.LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set Remaining Time' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Modify total time"
-- !Conditional "False"
-- !Description "Changes the total duration value (in seconds) of a specific timer.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 10, [ + | - | * | / | = ], {4}, Mathematical operation to perform"
-- !Arguments "Numerical, 20, [ 0 | 1000 | 2 | 0.1 | 1 ], New total time in seconds (internally rounded to the nearest game frame)"
LevelFuncs.Engine.Node.SetTotalTime = function(name, operator, totalTime)
    if name ~= '' then
        if Timer.IfExists(name) then
            if (operator == 4) then
                Timer.Get(name):SetTotalTime(totalTime)
            else
                local value = Timer.Get(name):GetTotalTimeInSeconds()
                Timer.Get(name):SetTotalTime(LevelFuncs.Engine.Node.ModifyValue(totalTime, value, operator))
            end
            CreateStruct(name)
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Set total time of '" .. name .. "' timer " .. textOp[operator] .. totalTime .. ". Total time : " .. Timer.Get(name):GetTotalTimeInSeconds(), TEN.Util.LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set Total Time' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Set timer loop"
-- !Conditional "False"
-- !Description "Sets an existing timer as looped or one shot.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 67, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 33, [ One shot | Looped ], Set timer as"
LevelFuncs.Engine.Node.SetLooping = function(name, looping)
    if name ~= '' then
        if Timer.IfExists(name) then
            local state = (looping == 1) and true or false
            Timer.Get(name):SetLooping(state)
            CreateStruct(name)
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' loop: " .. tostring(state), TEN.Util.LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set Timer Loop' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Set timer color"
-- !Conditional "False"
-- !Description "Sets colours for timer.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 76, [ NoMultiline ], Timer name"
-- !Arguments "Color, 10, {TEN.Color(255, 255, 255)}, Timer's color"
-- !Arguments "Numerical, 14, {1}, [ 0 | 1 | 2 | 0.1 ], Color transparency"
LevelFuncs.Engine.Node.SetTimerColor = function (name, color, tColor)
    if name ~= '' then
        if Timer.IfExists(name) then
            color.a = (255 * tColor)
            Timer.Get(name):SetUnpausedColor(color)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set timer colors' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Set timer paused color"
-- !Conditional "False"
-- !Description "Sets colours for timer.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 76, [ NoMultiline ], Timer name"
-- !Arguments "Color, 10, {TEN.Color(255, 255, 0)}, Timer's paused color"
-- !Arguments "Numerical, 14, {1}, [ 0 | 1 | 2 | 0.1 ], Paused color transparency"
LevelFuncs.Engine.Node.SetTimerPauseColor = function (name, pausedColor, tPausedColor)
    if name ~= '' then
        if Timer.IfExists(name) then
            pausedColor.a = (255 * tPausedColor)
            Timer.Get(name):SetPausedColor(pausedColor)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set timer colors' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Modify timer position"
-- !Conditional "False"
-- !Description "sets the position of the timer.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Timer name"
-- !Arguments "NewLine, Boolean, 25, {true}, Set X position"
-- !Arguments "Enumeration, 10, [ + | - | * | / | = ], {4}, Mathematical operation to perform for X"
-- !Arguments "Numerical, 65, [ -1000 | 1000 | 2 ], {50},  Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "NewLine, Boolean, 25, {true}, Set Y position"
-- !Arguments "Enumeration, 10, [ + | - | * | / | = ], {4}, Mathematical operation to perform for Y"
-- !Arguments "Numerical, 65, [ -1000 | 1000 | 2 ], {90}, Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
LevelFuncs.Engine.Node.SetTimerPosition = function (name, setX, operatorX, x, setY, operatorY, y)
    if name ~= '' then
        if Timer.IfExists(name) then
            local oldPos = Timer.Get(name):GetPosition()
            local valueX = setX and (operatorX == 4 and x or LevelFuncs.Engine.Node.ModifyValue(x, oldPos.x, operatorX)) or oldPos.x
            local valueY = setY and (operatorY == 4 and y or LevelFuncs.Engine.Node.ModifyValue(y, oldPos.y, operatorY)) or oldPos.y
            Timer.Get(name):SetPosition(valueX, valueY)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Modify timer position' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Modify timer scale"
-- !Conditional "False"
-- !Description "Sets the scale of the timer.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 10, [ + | - | * | / | = ], {4}, Mathematical operation to perform"
-- !Arguments "Numerical, 20, [ 0.1 | 100 | 1 | 0.1 ], {1}, Scale\nRange [0.1 to 100]"
LevelFuncs.Engine.Node.SetTimerScale = function (name, operator, scale)
    if name ~= '' then
        if Timer.IfExists(name) then
            local value
            if operator == 4 then
                value = scale
            else
                local oldValue = Timer.Get(name):GetScale()
                value = LevelFuncs.Engine.Node.ModifyValue(scale, oldValue, operator)
            end
            Timer.Get(name):SetScale(value)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Modify timer scale' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Modify timer alignment"
-- !Conditional "False"
-- !Description "Sets the horizontal alignment of the timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 80, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 20, {1}, [ Left | Center | Right ], Alignment"
LevelFuncs.Engine.Node.SetTimerAlignment = function (name, alignment)
    if name ~= '' then
        if Timer.IfExists(name) then
            CreateStruct(name)
            if LevelVars.nodeTimers[name].alignment == nil then
                TEN.Util.PrintLog("Error in the 'Modify timer alignment' node. Timer '" .. name .. "' has no alignment set. No changes will be applied", TEN.Util.LogLevel.ERROR)
            else
                Timer.Get(name):SetTextOption(LevelFuncs.Engine.Node.GeneratesTextOption(alignment, LevelVars.nodeTimers[name].effects))
                LevelVars.nodeTimers[name].alignment = alignment
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Modify timer alignment' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Modify timer effects"
-- !Conditional "False"
-- !Description "Sets the effects of the timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 65, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 35, {1}, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
LevelFuncs.Engine.Node.SetTimerEffects = function (name, effects)
    if name ~= '' then
        if Timer.IfExists(name) then
            CreateStruct(name)
            if LevelVars.nodeTimers[name].effects == nil then
                TEN.Util.PrintLog("Error in the 'Modify timer effects' node. Timer '" .. name .. "' has no effects set. No changes will be applied", TEN.Util.LogLevel.ERROR)
            else
                Timer.Get(name):SetTextOption(LevelFuncs.Engine.Node.GeneratesTextOption(LevelVars.nodeTimers[name].alignment, effects))
                LevelVars.nodeTimers[name].effects = effects
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Modify timer effects' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Set debug messages in console"
-- !Conditional "False"
-- !Description "Enables or disables debug messages in console for a specific timer.\nNote: using this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation."
-- !Section "Timer"
-- !Arguments "NewLine, String, 80, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 20, {1}, [ Enable | Disable ], Debug messages in console"
LevelFuncs.Engine.Node.SetDebugMessages = function(name, debug)
    if name ~= '' then
        if Timer.IfExists(name) then
            CreateStruct(name)
            LevelVars.nodeTimers[name].debug = (debug == 1) and true or false
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' debug messages in console: " .. tostring(LevelVars.nodeTimers[name].debug), TEN.Util.LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set Debug Messages' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Get remaining time (in seconds) in console"
-- !Conditional "False"
-- !Description "Prints on console the remaining time value (in seconds) of a specific timer.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.GetRemainingTime = function(name)
    if name ~= '' then
        if Timer.IfExists(name) then
            TEN.Util.PrintLog("Timer '" .. name .. "' remaining time: " .. tostring(Timer.Get(name):GetRemainingTimeInSeconds()), TEN.Util.LogLevel.INFO, true)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Get remaining time (in seconds) in console' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Get total time (in seconds) in console"
-- !Conditional "False"
-- !Description "Prints on console the total time value (in seconds) of a specific timer.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.GetTotalTime = function(name)
    if name ~= '' then
        if Timer.IfExists(name) then
            TEN.Util.PrintLog("Timer '" .. name .. "' total time: " .. Timer.Get(name):GetTotalTimeInSeconds(), TEN.Util.LogLevel.INFO, true)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Get total time (in seconds) in console' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "If timer active..."
-- !Conditional "True"
-- !Description "Checks if a specific timer is active.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IsTimerActive = function(name)
    if name ~= '' then
        if Timer.IfExists(name) then
            return Timer.Get(name):IsActive()
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If the timer active...' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "If timer is paused..."
-- !Conditional "True"
-- !Description "Checks if a specific timer is paused.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IsTimerPaused = function(name)
    if name ~= '' then
        if Timer.IfExists(name) then
            return Timer.Get(name):IsPaused()
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If the timer is paused...' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "If timer has expired..."
-- !Conditional "True"
-- !Description "Checks if a specific timer is expired.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IfTimerExpired = function(name)
    if name ~= '' then
        if Timer.IfExists(name) then
            return Timer.Get(name):IfRemainingTimeIs(0, 0.0)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If timer has expired...' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "If remaining time is..."
-- !Conditional "True"
-- !Description "Checks if the remaining time is equal to, greater than, less than..\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Timer name"
-- !Arguments "CompareOperator, 30"
-- !Arguments "Numerical, 20, [ 0 | 1000 | 2 | 0.1 | 1 ], Remaining time in seconds (internally rounded to the nearest game frame)"
LevelFuncs.Engine.Node.IfRemainingTimeIs = function(name, operator, value)
    if name ~= '' then
        if Timer.IfExists(name) then
            local timer = Timer.Get(name)
            if timer:IsActive() then
                local result
                result = timer:IfRemainingTimeIs(operator, value)
                CreateStruct(name)
                if LevelVars.nodeTimers[name].debug then
                    local floatValue = value + 0.00
                    local remainingTime = timer:GetRemainingTimeInSeconds()
                    TEN.Util.PrintLog("If the remaining time (".. remainingTime ..") is " .. textCompareOp[operator] .. " " ..  floatValue .. ". Result: " .. tostring(result), TEN.Util.LogLevel.INFO, true)
                end
                return result
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If remaining time is...' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "If total time is..."
-- !Conditional "True"
-- !Description "Checks if the Total Time is equal to, greater than, less than..\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Timer name"
-- !Arguments "CompareOperator, 30"
-- !Arguments "Numerical, 20, [ 0 | 1000 | 2 | 0.1 | 1 ], Total time in seconds (internally rounded to the nearest game frame)"
LevelFuncs.Engine.Node.IfTotalTimeIs = function(name, operator, time)
    if name ~= '' then
        if Timer.IfExists(name) then
            local result = Timer.Get(name):IfTotalTimeIs(operator, time)
            CreateStruct(name)
            if LevelVars.nodeTimers[name].debug then
                local totalTime = Timer.Get(name):GetTotalTimeInSeconds()
                TEN.Util.PrintLog("If the total time (".. totalTime ..") is " .. textCompareOp[operator] .. " " ..  (time + 0.0) .. ". Result: " .. tostring(result), TEN.Util.LogLevel.INFO, true)
            end
            return result
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If total time is...' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "If timer position is..."
-- !Conditional "True"
-- !Description "Checks if a specific timer is at a specific position.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Timer name"
-- !Arguments "NewLine, Boolean, 30, {true}, Check X position"
-- !Arguments "CompareOperator, 30, Comparison type for X"
-- !Arguments "Numerical, 40, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "NewLine, Boolean, 30, {true}, Check Y position"
-- !Arguments "CompareOperator, 30, Comparison type for Y"
-- !Arguments "Numerical, 40, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
LevelFuncs.Engine.Node.IfPositionIs = function(name, checkX, operatorX, valueX, checkY, operatorY, valueY)
    if name ~= '' then
        if Timer.IfExists(name) then
            local position = Timer.Get(name):GetPosition()
            local resultX, resultY = true, true
            if checkX then
                resultX = LevelFuncs.Engine.Node.CompareValue(position.x, valueX, operatorX)
            end
            if checkY then
                resultY = LevelFuncs.Engine.Node.CompareValue(position.y, valueY, operatorY)
            end
            if LevelVars.nodeTimers[name].debug then
                local posX = position.x
                local posY = position.y
                local checkText = "Checking timer '" .. name .. "' position. "
                if checkX then
                    checkText = checkText .. "X: " .. posX .. " is " .. textCompareOp[operatorX] .. " " .. valueX .. ". Result: " .. tostring(resultX) .. ". "
                end
                if checkY then
                    checkText = checkText .. "Y: " .. posY .. " is " .. textCompareOp[operatorY] .. " " .. valueY .. ". Result: " .. tostring(resultY) .. "."
                end
                TEN.Util.PrintLog(checkText, TEN.Util.LogLevel.INFO, true)
            end
            return (resultX and resultY)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If timer position is...' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "If timer scale is..."
-- !Conditional "True"
-- !Description "Checks if a specific timer is at a specific scale.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Timer name"
-- !Arguments "CompareOperator, 30"
-- !Arguments "Numerical, 20, [ 0.1 | 100 | 2 | 0.01 ], Scale\nRange [0.10 to 100]"
LevelFuncs.Engine.Node.IfScaleIs = function(name, operator, scale)
    if name ~= '' then
        if Timer.IfExists(name) then
            local timerScale = Timer.Get(name):GetScale()
            return LevelFuncs.Engine.Node.CompareValue(timerScale, scale, operator)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If timer scale is...' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "If timer exists..."
-- !Conditional "True"
-- !Description "Checks if a specific timer exists."
-- !Section "Timer"
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IfTimerExists = function(name)
    if name ~= '' then
        return Timer.IfExists(name)
    else
        TEN.Util.PrintLog("Error in the 'If timer exists...' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end