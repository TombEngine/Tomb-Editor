local Timer = require("Engine.Timer")
local textOp = {[0] = "+", [1] = "-", [2] = "*", [3] = "/", [4] = "=", }
LevelVars.nodeTimers = {}

-- !Ignore
LevelFuncs.Engine.Node.SetTimer = function (name, debug, alignment, effects, color, pColor, x, y, scale)
    Timer.Get(name):SetUnpausedColor(color)
    Timer.Get(name):SetPausedColor(pColor)
    Timer.Get(name):SetPosition(x,y)
    Timer.Get(name):SetScale(scale)
    Timer.Get(name):SetTextOption(LevelFuncs.Engine.Node.GeneratesTextOption(alignment, effects))
    LevelVars.nodeTimers[name] = {debug = debug}
end

-- !Name "Create basic timer"
-- !Conditional "False"
-- !Description "Creates a simple countdown.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 | 1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, {false}, Loop"
-- !Arguments "NewLine, Boolean, 25, {false}, Hours"
-- !Arguments "Boolean, 25, {true}, Minutes"
-- !Arguments "Boolean , 25, {true}, Seconds"
-- !Arguments "Boolean, 25, {true}, Deciseconds"
-- !Arguments "NewLine, Boolean , 47, {false}, Debug messages in console"
-- !Arguments "Enumeration, 18, {1}, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "Enumeration, 35, {1}, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "NewLine, Color, 20, {TEN.Color(255, 255, 255)}, Timer's color"
-- !Arguments "Color, 20, {TEN.Color(255, 255, 0)}, Timer's paused color"
-- !Arguments "Numerical, 20, {50}, [ 0 | 100 | 2 ], Position X (%)\nRange [0 to 100]"
-- !Arguments "Numerical, 20, {90}, [ 0 | 100 | 2 ], Position Y (%)\nRange [0 to 100]"
-- !Arguments "Numerical, 20, {1}, [ 0.1 | 100 | 2 | 0.1 ], Scale\nRange [0.1 to 100]"
LevelFuncs.Engine.Node.CreateTimer = function(name, time, loop, hours, minutes, seconds, deciseconds, debug, alignment, effects, color, pColor, x, y, scale)
    if name ~= '' then
        local nodeTimerFormat = {hours = hours, minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        Timer.Create(name, time, loop, nodeTimerFormat)
        LevelFuncs.Engine.Node.SetTimer(name, debug, alignment, effects, color, pColor, x, y, scale)
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer '" .. name .. "' created successfully!", TEN.Util.LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create basic timer' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Create timer with function"
-- !Conditional "False"
-- !Description "Creates a countdown which will execute a `LevelFuncs` lua function upon ending.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 | 1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, {false}, Loop"
-- !Arguments "NewLine, Boolean, 25, {false}, Hours"
-- !Arguments "Boolean, 25, {true}, Minutes"
-- !Arguments "Boolean , 25, {true}, Seconds"
-- !Arguments "Boolean, 25, {true}, Deciseconds"
-- !Arguments "NewLine, Boolean , 47, {false}, Debug messages in console"
-- !Arguments "Enumeration, 18, {1}, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "Enumeration, 35, {1}, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "NewLine, Color, 20, {TEN.Color(255, 255, 255)}, Timer's color"
-- !Arguments "Color, 20, {TEN.Color(255, 255, 0)}, Timer's paused color"
-- !Arguments "Numerical, 20, {50}, [ 0 | 100 | 2 ], Position X (%)\nRange [0 to 100]"
-- !Arguments "Numerical, 20, {90}, [ 0 | 100 | 2 ], Position Y (%)\nRange [0 to 100]"
-- !Arguments "Numerical, 20, {1}, [ 0.1 | 100 | 2 | 0.1 ], Scale\nRange [0.1 to 100]"
-- !Arguments "NewLine, LuaScript, The function to call when the time is up"
-- !Arguments "NewLine, String, Arguments"
LevelFuncs.Engine.Node.CreateTimerWithFunction = function(name, time, loop, hours, minutes, seconds, deciseconds, debug, alignment, effects, color, pColor, x, y, scale, luaFunction, args)
    if name ~= '' then
        local nodeTimerFormat = {hours = hours, minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        local argsTable = args ~= '' and table.unpack(LevelFuncs.Engine.Node.SplitString(args, ",")) or nil
        Timer.Create(name, time, loop, nodeTimerFormat, luaFunction, argsTable)
        LevelFuncs.Engine.Node.SetTimer(name, debug, alignment, effects, color, pColor, x, y, scale)
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer with Function '" .. name .. "' created successfully!", TEN.Util.LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create timer with function' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Create timer with volume event set"
-- !Conditional "False"
-- !Description "Creates a countdown that triggers a volume event set upon ending.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 | 1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, {false}, Loop"
-- !Arguments "NewLine, Boolean, 25, {false}, Hours"
-- !Arguments "Boolean, 25, {true}, Minutes"
-- !Arguments "Boolean , 25, {true}, Seconds"
-- !Arguments "Boolean, 25, {true}, Deciseconds"
-- !Arguments "NewLine, Boolean , 47, {false}, Debug messages in console"
-- !Arguments "Enumeration, 18, {1}, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "Enumeration, 35, {1}, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "NewLine, Color, 20, {TEN.Color(255, 255, 255)}, Timer's color"
-- !Arguments "Color, 20, {TEN.Color(255, 255, 0)}, Timer's paused color"
-- !Arguments "Numerical, 20, {50}, [ 0 | 100 | 2 ], Position X (%)\nRange [0 to 100]"
-- !Arguments "Numerical, 20, {90}, [ 0 | 100 | 2 ], Position Y (%)\nRange [0 to 100]"
-- !Arguments "Numerical, 20, {1}, [ 0.1 | 100 | 2 | 0.1 ], Scale\nRange [0.1 to 100]"
-- !Arguments "NewLine, 66, VolumeEventSets, The event set to be called when the time is up"
-- !Arguments "VolumeEvents, 34, Event to run"
-- !Arguments "NewLine, Moveables, Activator for the event (when necessary)"
LevelFuncs.Engine.Node.CreateTimerWithEventSet = function(name, time, loop, hours, minutes, seconds, deciseconds, debug, alignment, effects, color, pColor, x, y, scale, setName, eventType, activator)
    if name ~= '' then
        local nodeTimerFormat = {hours = hours, minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        Timer.Create(name, time, loop, nodeTimerFormat, LevelFuncs.Engine.Node.RunEventSet, setName, eventType, activator)
        LevelFuncs.Engine.Node.SetTimer(name, debug, alignment, effects, color, pColor, x, y, scale)
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer with volume event set '" .. name .. "' created successfully", TEN.Util.LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create timer with volume event set' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Create timer with global event set"
-- !Conditional "False"
-- !Description "Creates a countdown that triggers a global event set upon ending.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 | 1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, {false}, Loop"
-- !Arguments "NewLine, Boolean, 25, {false}, Hours"
-- !Arguments "Boolean, 25, {true}, Minutes"
-- !Arguments "Boolean , 25, {true}, Seconds"
-- !Arguments "Boolean, 25, {true}, Deciseconds"
-- !Arguments "NewLine, Boolean , 47, {false}, Debug messages in console"
-- !Arguments "Enumeration, 18, {1}, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "Enumeration, 35, {1}, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "NewLine, Color, 20, {TEN.Color(255, 255, 255)}, Timer's color"
-- !Arguments "Color, 20, {TEN.Color(255, 255, 0)}, Timer's paused color"
-- !Arguments "Numerical, 20, {50}, [ 0 | 100 | 2 ], Position X (%)\nRange [0 to 100]"
-- !Arguments "Numerical, 20, {90}, [ 0 | 100 | 2 ], Position Y (%)\nRange [0 to 100]"
-- !Arguments "Numerical, 20, {1}, [ 0.1 | 100 | 2 | 0.1 ], Scale\nRange [0.1 to 100]"
-- !Arguments "NewLine, 70, GlobalEventSets, The event set to be called when the time is up"
-- !Arguments "GlobalEvents, 30, Event to run"
-- !Arguments "NewLine, Moveables, Activator for the event (when necessary)"
LevelFuncs.Engine.Node.CreateTimerWithGEventSet = function(name, time, loop, hours, minutes, seconds, deciseconds, debug, alignment, effects, color, pColor, x, y, scale, setName, eventType, activator)
    if name ~= '' then
        local nodeTimerFormat = {hours = hours, minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        Timer.Create(name, time, loop, nodeTimerFormat, LevelFuncs.Engine.Node.RunGlobalEventSet, setName, eventType, activator)
        LevelFuncs.Engine.Node.SetTimer(name, debug, alignment, effects, color, pColor, x, y, scale)
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer with global event set '" .. name .. "' created successfully", TEN.Util.LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create timer with global event set' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Start timer"
-- !Conditional "False"
-- !Description "Begins or resumes a timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
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
-- !Description "Stops a timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.StopTimer = function(name)
    if name ~= '' then
        if Timer.IfExists(name) then
            Timer.Get(name):Stop()
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
-- !Description "Pauses a timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.SetPausedTimer = function(name)
    if name ~= '' then
        if Timer.IfExists(name) then
            Timer.Get(name):SetPaused(true)
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
-- !Description "Changes the remaining time value (in seconds) of a specific timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 10, [ + | - | * | / | = ], {4}, Mathematical operation to perform"
-- !Arguments "Numerical, 20, [ 0 | 65535 | 1 | 0.1 | 1 ], New time remaining (in seconds)"
LevelFuncs.Engine.Node.SetRemainingTime = function(name, operator, remainingTime)
    if name ~= '' then
        if Timer.IfExists(name) then
            if (operator == 4) then
                Timer.Get(name):SetRemainingTime(remainingTime)
            else
                local value = Timer.Get(name):GetRemainingTimeInSeconds()
                Timer.Get(name):SetRemainingTime(LevelFuncs.Engine.Node.ModifyValue(remainingTime, value, operator))
            end
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
-- !Description "Changes the total duration value (in seconds) of a specific timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 10, [ + | - | * | / | = ], {4}, Mathematical operation to perform"
-- !Arguments "Numerical, 20, [ 0 | 1000 | 1 | 0.1 | 1 ], New total time (in seconds)"
LevelFuncs.Engine.Node.SetTotalTime = function(name, operator, totalTime)
    if name ~= '' then
        if Timer.IfExists(name) then
            if (operator == 4) then
                Timer.Get(name):SetTotalTime(totalTime)
            else
                local value = Timer.Get(name):GetTotalTimeInSeconds()
                Timer.Get(name):SetTotalTime(LevelFuncs.Engine.Node.ModifyValue(totalTime, value, operator))
            end
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
-- !Description "Sets an existing timer as looped or one shot.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 67, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 33, [ One shot | Looped ], Set timer as"
LevelFuncs.Engine.Node.SetLooping = function(name, looping)
    if name ~= '' then
        if Timer.IfExists(name) then
            local state = (looping == 1) and true or false
            Timer.Get(name):SetLooping(state)
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
-- !Description "Sets colours for timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 76, [ NoMultiline ], Timer name"
-- !Arguments "Color, 10, {TEN.Color(255, 255, 255)}, Timer's color
-- !Arguments "Numerical, 14, {1}, [ 0 | 1 | 2 | 0.1 ], Color transparency'"
LevelFuncs.Engine.Node.SetTimerColor = function (name, color, tColor)
    if name ~= '' then
        if Timer.IfExists(name) then
            Timer.Get(name):SetUnpausedColor(TEN.Color(color.r, color.g, color.b, (255 * tColor)))
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set timer colors' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Set timer paused color"
-- !Conditional "False"
-- !Description "Sets colours for timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 76, [ NoMultiline ], Timer name"
-- !Arguments "Color, 10, {TEN.Color(255, 255, 0)}, Timer's paused color"
-- !Arguments "Numerical, 14, {1}, [ 0 | 1 | 2 | 0.1 ], Paused color transparency"
LevelFuncs.Engine.Node.SetTimerPauseColor = function (name, pausedColor, tPausedColor)
    if name ~= '' then
        if Timer.IfExists(name) then
            Timer.Get(name):SetPausedColor(TEN.Color(pausedColor.r, pausedColor.g, pausedColor.b, (255 * tPausedColor)))
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set timer colors' node. No timer name provided", TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Modify timer position"
-- !Conditional "False"
-- !Description "sets the position of the timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
-- !Section "Timer"
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Timer name"
-- !Arguments "NewLine, Enumeration, 10, [ + | - | * | / | = ], {4}, Mathematical operation to perform for X"
-- !Arguments "Numerical, 90, [ -1000 | 1000 | 2 ], {50},  Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "NewLine, Enumeration, 10, [ + | - | * | / | = ], {4}, Mathematical operation to perform for Y"
-- !Arguments "Numerical, 90, [ -1000 | 1000 | 2 ], {90}, Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
LevelFuncs.Engine.Node.SetTimerPosition = function (name, operatorX, x, operatorY, y)
    if name ~= '' then
        if Timer.IfExists(name) then
            local valueX, valueY
            if operatorX == 4 then
                valueX = x
            else
                local oldValue = Timer.Get(name):GetPosition()
                valueX = LevelFuncs.Engine.Node.ModifyValue(x, oldValue.x, operatorX)
            end
            if operatorY == 4 then
                valueY = y
            else
                local oldValue = Timer.Get(name):GetPosition()
                valueY = LevelFuncs.Engine.Node.ModifyValue(y, oldValue.y, operatorY)
            end
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
-- !Description "Sets the scale of the timer.\nUsing this node within “On Volume Inside” or “On Loop” events may cause continuous loops and improper operation. Please carefully consider this configuration."
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

-- !Name "Get remaining time (in seconds) in console"
-- !Conditional "False"
-- !Description "Prints on console the remaining time value (in seconds) of a specific timer.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.GetRemainingTime = function(name)
    if name ~= '' then
        if Timer.IfExists(name) then
            if Timer.Get(name):IsTicking() then
                TEN.Util.PrintLog("Timer '" .. name .. "' remaining time: " .. tostring(Timer.Get(name):GetRemainingTimeInSeconds()), TEN.Util.LogLevel.INFO, true)
            end
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
            if Timer.Get(name):IsTicking() then
                TEN.Util.PrintLog("Timer '" .. name .. "' total time: " .. Timer.Get(name):GetTotalTimeInSeconds(), TEN.Util.LogLevel.INFO, true)
            end
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
-- !Arguments "Numerical, 20, [ 0 | 1000 | 1 | 0.1 | 1 ], Remaining time (in seconds)"
LevelFuncs.Engine.Node.IfRemainingTimeIs = function(name, operator, value)
    if name ~= '' then
        if Timer.IfExists(name) then
            if Timer.Get(name):IsActive() and Timer.Get(name):IsTicking() then
                local remainingTime = Timer.Get(name):GetRemainingTimeInSeconds()
                local floatValue = value + 0.0
                local result = Timer.Get(name):IfRemainingTimeIs(operator, floatValue)
                if LevelVars.nodeTimers[name].debug then
                    TEN.Util.PrintLog("If the remaining time is "..  floatValue .. ". Remaining time: " .. remainingTime .. ". Result: " .. tostring(result), TEN.Util.LogLevel.INFO, true)
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
-- !Arguments "Numerical, 20, [ 0 | 1000 | 1 | 0.1 | 1 ], Total Time (in seconds)"
LevelFuncs.Engine.Node.IfTotalTimeIs = function(name, operator, time)
    if name ~= '' then
        if Timer.IfExists(name) then
            return Timer.Get(name):IfTotalTimeIs(operator, (time + 0.0))
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", TEN.Util.LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If total time is...' node. No timer name provided", TEN.Util.LogLevel.ERROR)
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