local Timer = require("Engine.Timer")

-- !Ignore
-- Construct timed transform data and start transform

LevelVars.Engine.WeatherData = {}
LevelFuncs.Engine.Node.ConstructWeatherTimedData = function(dataType, operand, newValue, time, smooth)

	local prefix = nil
	local value  = nil
	
	if (dataType == 0) then 
		prefix = "_fog"
		value = TEN.Flow.GetCurrentLevel().fog.color
	elseif (dataType == 1) then
		prefix = "_lensflare"
        local pitch = TEN.Flow.GetCurrentLevel().lensFlare.pitch
		local yaw = TEN.Flow.GetCurrentLevel().lensFlare.yaw
		value = Vec2(pitch, yaw)
	elseif (dataType == 2) then
		prefix = "_skylayer1"
        local color = TEN.Flow.GetCurrentLevel().layer1.color
		value = {x1 = color.r, x2 = color.g, x3 = color.b, x4 = TEN.Flow.GetCurrentLevel().layer1.speed}
	elseif (dataType == 3) then
		prefix = "_skylayer2"
		local color = TEN.Flow.GetCurrentLevel().layer2.color
		value = {x1 = color.r, x2 = color.g, x3 = color.b, x4 = TEN.Flow.GetCurrentLevel().layer2.speed}
    elseif (dataType == 4) then
		prefix = "_starfield"
        local starCount = TEN.Flow.GetCurrentLevel().starfield.starCount
        local meteorCount = TEN.Flow.GetCurrentLevel().starfield.meteorCount
		local meteorDensity = TEN.Flow.GetCurrentLevel().starfield.meteorSpawnDensity
		local meteorVelocity = TEN.Flow.GetCurrentLevel().starfield.meteorVelocity
		value = {x1 = starCount, x2 = meteorCount, x3 = meteorDensity, x4 = meteorVelocity}
	elseif (dataType == 5) then
		prefix = "_weatherStrength"
        value = TEN.Flow.GetCurrentLevel().weatherStrength
	elseif (dataType == 6) then
		prefix = "_horizon1Position"
        value = TEN.Flow.GetCurrentLevel().horizon1.position
	elseif (dataType == 7) then
		prefix = "_horizon1Rotation"
        value = TEN.Flow.GetCurrentLevel().horizon1.rotation
	elseif (dataType == 8) then
		prefix = "_horizon1Transparency"
        value = TEN.Flow.GetCurrentLevel().horizon1.transparency
	elseif (dataType == 9) then
		prefix = "_horizon2Position"
        value = TEN.Flow.GetCurrentLevel().horizon2.position
	elseif (dataType == 10) then
		prefix = "_horizon2Rotation"
        value = TEN.Flow.GetCurrentLevel().horizon2.rotation
	elseif (dataType == 11) then
		prefix = "_horizon2Transparency"
        value = TEN.Flow.GetCurrentLevel().horizon2.transparency
	elseif (dataType == 12) then
		prefix = "_lensflareColor"
		value = TEN.Flow.GetCurrentLevel().lensFlare.color
	elseif (dataType == 13) then
		prefix = "_fogDistance"
		value = Vec2(TEN.Flow.GetCurrentLevel().fog.minDistance, TEN.Flow.GetCurrentLevel().fog.maxDistance)
	end

	local dataName  = "Weather" .. prefix .. "_transform_data"
	
	if (LevelVars.Engine.WeatherData[dataName] ~= nil and Timer.Get(LevelVars.Engine.WeatherData[dataName].Name) ~= nil) then
		if (Timer.Get(LevelVars.Engine.WeatherData[dataName].Name):IsActive()) then
			return
		else
			Timer.Delete(LevelVars.Engine.WeatherData[dataName].Name)
			LevelVars.Engine.WeatherData[dataName] = nil
		end
	end

	LevelVars.Engine.WeatherData = LevelVars.Engine.WeatherData or {}

	LevelVars.Engine.WeatherData[dataName] = {}

	LevelVars.Engine.WeatherData[dataName].Progress   = 0
	LevelVars.Engine.WeatherData[dataName].Interval   = 1 / (time * 30)
	LevelVars.Engine.WeatherData[dataName].Smooth     = smooth
	LevelVars.Engine.WeatherData[dataName].DataType   = dataType
    LevelVars.Engine.WeatherData[dataName].Operand    = operand
	LevelVars.Engine.WeatherData[dataName].Name       = dataName
	LevelVars.Engine.WeatherData[dataName].NewValue   = newValue
	LevelVars.Engine.WeatherData[dataName].OldValue   = value

	local timer = Timer.Create(dataName, 1 / 30, true, false, LevelFuncs.Engine.Node.TransformWeatherTimedData, dataName)
	timer:Start()

end

-- !Ignore
-- Transform object parameter using previously saved timed transform data
LevelFuncs.Engine.Node.TransformWeatherTimedData = function(dataName)

	LevelVars.Engine.WeatherData[dataName].Progress = math.min(LevelVars.Engine.WeatherData[dataName].Progress + LevelVars.Engine.WeatherData[dataName].Interval, 1)
	local factor = LevelVars.Engine.WeatherData[dataName].Smooth and LevelFuncs.Engine.Node.Smoothstep(LevelVars.Engine.WeatherData[dataName].Progress) or LevelVars.Engine.WeatherData[dataName].Progress
	
	local newValue1
	local newValue2
	local newValue3
	local newValue4
	
	if (LevelVars.Engine.WeatherData[dataName].Operand == 0) then
        newValue1 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.r, LevelVars.Engine.WeatherData[dataName].NewValue.r, factor)
        newValue2 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.g, LevelVars.Engine.WeatherData[dataName].NewValue.g, factor)
        newValue3 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.b, LevelVars.Engine.WeatherData[dataName].NewValue.b, factor)
        newValue4 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.a, LevelVars.Engine.WeatherData[dataName].NewValue.a, factor)
    elseif
    (LevelVars.Engine.WeatherData[dataName].Operand == 1) then
        newValue1 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.x, LevelVars.Engine.WeatherData[dataName].NewValue.x, factor)
		newValue2 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.y, LevelVars.Engine.WeatherData[dataName].NewValue.y, factor)
	elseif 
    (LevelVars.Engine.WeatherData[dataName].Operand == 2) then
		newValue1 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.x, LevelVars.Engine.WeatherData[dataName].NewValue.x, factor)
		newValue2 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.y, LevelVars.Engine.WeatherData[dataName].NewValue.y, factor)
	    newValue3 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.z, LevelVars.Engine.WeatherData[dataName].NewValue.z, factor)
    elseif 
    (LevelVars.Engine.WeatherData[dataName].Operand == 3) then
		newValue1 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.x1, LevelVars.Engine.WeatherData[dataName].NewValue.x1, factor)
		newValue2 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.x2, LevelVars.Engine.WeatherData[dataName].NewValue.x2, factor)
	    newValue3 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.x3, LevelVars.Engine.WeatherData[dataName].NewValue.x3, factor)
        newValue4 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue.x4, LevelVars.Engine.WeatherData[dataName].NewValue.x4, factor)
	elseif
	(LevelVars.Engine.WeatherData[dataName].Operand == 4) then
        newValue1 = LevelFuncs.Engine.Node.Lerp(LevelVars.Engine.WeatherData[dataName].OldValue, LevelVars.Engine.WeatherData[dataName].NewValue, factor)   
	end

	if (LevelVars.Engine.WeatherData[dataName].DataType == 0) then
        TEN.Flow.GetCurrentLevel().fog.color = Color(newValue1, newValue2, newValue3)
    elseif (LevelVars.Engine.WeatherData[dataName].DataType == 1) then
        TEN.Flow.GetCurrentLevel().lensFlare.pitch = newValue1
        TEN.Flow.GetCurrentLevel().lensFlare.yaw = newValue2
	elseif (LevelVars.Engine.WeatherData[dataName].DataType == 2) then
		TEN.Flow.GetCurrentLevel().layer1.color = Color(newValue1, newValue2, newValue3)
		TEN.Flow.GetCurrentLevel().layer1.speed = newValue4
	elseif (LevelVars.Engine.WeatherData[dataName].DataType == 3) then
		TEN.Flow.GetCurrentLevel().layer2.color = Color(newValue1, newValue2, newValue3)
		TEN.Flow.GetCurrentLevel().layer2.speed = newValue4
    elseif (LevelVars.Engine.WeatherData[dataName].DataType == 4) then
        TEN.Flow.GetCurrentLevel().starfield.starCount = newValue1
        TEN.Flow.GetCurrentLevel().starfield.meteorCount = newValue2
		TEN.Flow.GetCurrentLevel().starfield.meteorSpawnDensity = newValue3
		TEN.Flow.GetCurrentLevel().starfield.meteorVelocity = newValue4
    elseif (LevelVars.Engine.WeatherData[dataName].DataType == 5) then
       TEN.Flow.GetCurrentLevel().weatherStrength = newValue1
	elseif (LevelVars.Engine.WeatherData[dataName].DataType == 6) then
		TEN.Flow.GetCurrentLevel().horizon1.position = Vec3(newValue1, newValue2, newValue3)
	elseif (LevelVars.Engine.WeatherData[dataName].DataType == 7) then
		TEN.Flow.GetCurrentLevel().horizon1.rotation = Rotation(newValue1, newValue2, newValue3)
	elseif (LevelVars.Engine.WeatherData[dataName].DataType == 8) then
		TEN.Flow.GetCurrentLevel().horizon1.transparency = newValue1
	elseif (LevelVars.Engine.WeatherData[dataName].DataType == 9) then
		TEN.Flow.GetCurrentLevel().horizon2.position = Vec3(newValue1, newValue2, newValue3)
	elseif (LevelVars.Engine.WeatherData[dataName].DataType == 10) then
		TEN.Flow.GetCurrentLevel().horizon2.rotation = Rotation(newValue1, newValue2, newValue3)
	elseif (LevelVars.Engine.WeatherData[dataName].DataType == 11) then
		TEN.Flow.GetCurrentLevel().horizon2.transparency = newValue1
	elseif (LevelVars.Engine.WeatherData[dataName].DataType == 12) then
		TEN.Flow.GetCurrentLevel().lensFlare.color = Color(newValue1, newValue2, newValue3)
	elseif (LevelVars.Engine.WeatherData[dataName].DataType == 13) then
		TEN.Flow.GetCurrentLevel().fog.minDistance = newValue1
		TEN.Flow.GetCurrentLevel().fog.maxDistance = newValue2
	end
		
	if (LevelVars.Engine.WeatherData[dataName].Progress >= 1) then
		Timer.Delete(LevelVars.Engine.WeatherData[dataName].Name)
		LevelVars.Engine.WeatherData[dataName] = nil
	end

end

-- !Name "If minimum fog distance is..."
-- !Section "Environment"
-- !Description "Checks current minimum (near) fog distance value in sectors."
-- !Conditional "True"
-- !Arguments "CompareOperator, 25, Compare operation" "Numerical, 15, [ 0 | 1024 | 2 | 1 ], Minimum fog distance to check"
LevelFuncs.Engine.Node.TestFogMinDistance = function(operator, number)
	return LevelFuncs.Engine.Node.CompareValue(TEN.Flow.GetCurrentLevel().fog.minDistance, number, operator)
end

-- !Name "If maximum fog distance is..."
-- !Section "Environment"
-- !Description "Checks current maximum (far) fog distance value in sectors."
-- !Conditional "True"
-- !Arguments "CompareOperator, 25, Compare operation" "Numerical, 15, [ 0 | 1024 | 2 | 1 ], Maximum fog distance to check"
LevelFuncs.Engine.Node.TestFogMaxDistance = function(operator, number)
	return LevelFuncs.Engine.Node.CompareValue(TEN.Flow.GetCurrentLevel().fog.maxDistance, number, operator)
end

-- !Name "If fog color is..."
-- !Section "Environment"
-- !Description "Checks current fog color."
-- !Conditional "True"
-- !Arguments "Color, 20, Fog color"
LevelFuncs.Engine.Node.TestFogColor = function(color)
	local fog = TEN.Flow.GetCurrentLevel().fog
	return (color.r == fog.color.r and color.g == fog.color.g and color.b == fog.color.b)
end

-- !Name "Set fog minimum distance"
-- !Section "Environment"
-- !Description "Sets fog minimum (near) distance to specified value in sectors."
-- !Arguments "Numerical, 15, [ 0 | 1024 | 2 | 1  ], Minimum fog distance"
LevelFuncs.Engine.Node.SetFogMinDistance = function(distance)
	TEN.Flow.GetCurrentLevel().fog.minDistance = distance
end

-- !Name "Set fog maximum distance"
-- !Section "Environment"
-- !Description "Sets fog maximum (far) distance to specified value in sectors."
-- !Arguments "Numerical, 15, [ 0 | 1024 | 2 | 1  ], Maximum fog distance"
LevelFuncs.Engine.Node.SetFogMaxDistance = function(distance)
	TEN.Flow.GetCurrentLevel().fog.maxDistance = distance
end

-- !Name "Set fog color"
-- !Section "Environment"
-- !Description "Sets fog color to specified."
-- !Arguments "Color, 20, Fog color"
LevelFuncs.Engine.Node.SetFogColor = function(color)
	TEN.Flow.GetCurrentLevel().fog.color = color
end

-- !Name "Change fog color over time"
-- !Section "Environment"
-- !Description "Changes fog color over specified time."
-- !Arguments "NewLine, Color, 20, Fog color" "Numerical, 20, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, Time in seconds" "35, Boolean, Smooth motion"
LevelFuncs.Engine.Node.ChangeFogColorOverTime = function(color, time, smooth)
	
	do LevelFuncs.Engine.Node.ConstructWeatherTimedData(0, 0, color, time, smooth) end

end

-- !Name "Change fog distance over time"
-- !Section "Environment"
-- !Description "Change fog distance over specified time."
-- !Arguments "Newline, Numerical, 25, [ -1024 | 1024 | 2 | 1 ], {1}, Minimum distance"
-- !Arguments "Numerical, 25, [ -1024 | 1024 | 2 | 1 ], {1}, Maximum distnace"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 25, Time (in seconds)" 
-- !Arguments "25, Boolean, Relative"
LevelFuncs.Engine.Node.ChangeFogDistanceOverTime = function(minDistance, maxDistance, time, relative)
	
	if (relative) then
		minDistance = TEN.Flow.GetCurrentLevel().fog.minDistance + minDistance
		maxDistance = TEN.Flow.GetCurrentLevel().fog.maxDistance + maxDistance
	end
		
	do LevelFuncs.Engine.Node.ConstructWeatherTimedData(13, 1, TEN.Vec2(minDistance, maxDistance), time, true) end

end

-- !Name "If draw distance is..."
-- !Section "Environment"
-- !Description "Checks current draw distance value in sectors."
-- !Conditional "True"
-- !Arguments "CompareOperator, 25, Compare operation" "Numerical, 15, [ 0 | 1024 | 0 | 1 ], Draw distance to check"
LevelFuncs.Engine.Node.TestDrawDistance = function(operator, number)
	return LevelFuncs.Engine.Node.CompareValue(TEN.Flow.GetCurrentLevel().farView, number, operator)
end

-- !Name "Set draw distance"
-- !Section "Environment"
-- !Description "Sets draw distance to a specified value (in sectors)."
-- !Arguments "Numerical, 15, [ 4 | 1024 | 0 | 1 ], {20}, Maximum draw distance in sectors"
LevelFuncs.Engine.Node.SetDrawDistance = function(distance)
	TEN.Flow.GetCurrentLevel().farView = distance
end

-- !Name "If lens flare is enabled..."
-- !Section "Environment"
-- !Description "Checks if lens flare is currently enabled."
-- !Conditional "True"
LevelFuncs.Engine.Node.TestLensFlare = function()
	return TEN.Flow.GetCurrentLevel().lensFlare.enabled
end

-- !Name "Enable lens flare"
-- !Section "Environment"
-- !Description "Enables a lens flare with specified parameters."
-- !Arguments "Numerical, 15, [ 0 | 360 ], Pitch" "Numerical, 15, [ 0 | 360 ], Yaw" "Color, 20, Lens flare color"
LevelFuncs.Engine.Node.EnableLensFlare = function(pitch, yaw, color)
    TEN.Flow.GetCurrentLevel().lensFlare = Flow.LensFlare(pitch, yaw, color)
end

-- !Name "Disable lens flare"
-- !Section "Environment"
-- !Description "Disables a lens flare."
LevelFuncs.Engine.Node.DisableLensFlare = function()
    TEN.Flow.GetCurrentLevel().lensFlare.enabled = false
end

-- !Name "Change lens flare position over time"
-- !Section "Environment"
-- !Description "Changes lens flare position over specified time."
-- !Arguments "NewLine, Numerical, 20, [ 0 | 360 ], Pitch" "Numerical, 20, [ 0 | 360 ], Yaw"
-- !Arguments "Numerical, 20, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, Time in seconds" "40, Boolean, Smooth motion"
LevelFuncs.Engine.Node.ChangeLensFlarePosOverTime = function(pitch, yaw, time, smooth)
	
    if TEN.Flow.GetCurrentLevel().lensFlare:GetEnabled() == true then
        do LevelFuncs.Engine.Node.ConstructWeatherTimedData(1, 1, Vec2(pitch, yaw), time, smooth) end
    end

end

-- !Name "Change lens flare color over time"
-- !Section "Environment"
-- !Description "Changes lens flare color over specified time."
-- !Arguments "Color, 15, Lens flare color" "Numerical, 15, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, Time in seconds"
LevelFuncs.Engine.Node.ChangeLensFlareColorOverTime = function(color, time)
	
    if TEN.Flow.GetCurrentLevel().lensFlare:GetEnabled() == true then
        do LevelFuncs.Engine.Node.ConstructWeatherTimedData(12, 0, color, time, true) end
    end

end

-- !Name "If storm is enabled..."
-- !Section "Environment"
-- !Description "Checks if storm is currently enabled."
-- !Conditional "True"
LevelFuncs.Engine.Node.TestStorm = function()
	return TEN.Flow.GetCurrentLevel().storm
end

-- !Name "Set storm"
-- !Section "Environment"
-- !Description "Enables or disables a lightning storm."
-- !Arguments "Boolean, 15, Storm"
LevelFuncs.Engine.Node.SetStorm = function(storm)
    TEN.Flow.GetCurrentLevel().storm = storm
end

-- !Name "If weather is..."
-- !Section "Environment"
-- !Description "Checks if weather is currently set to a given type."
-- !Conditional "True"
-- !Arguments "Enumeration, 25, [ None | Rain | Snow ]
LevelFuncs.Engine.Node.TestWeather = function(weather)
	return TEN.Flow.GetCurrentLevel().weather == weather
end

-- !Name "Set weather"
-- !Section "Environment"
-- !Description "Sets weather conditions."
-- !Arguments "NewLine, Enumeration, 50, [ None | Rain | Snow ], {0}, Weather type" "Numerical, 30, [ 0 | 1 | 2 | 0.1 ], Weather strength"
-- !Arguments "Boolean, 20, Clustered"
LevelFuncs.Engine.Node.SetWeather = function(weather, strength, clustered)
    TEN.Flow.GetCurrentLevel().weather = weather
	TEN.Flow.GetCurrentLevel().weatherStrength = strength
	TEN.Flow.GetCurrentLevel().weatherClustering = clustered
end

-- !Name "Change weather strength over time"
-- !Section "Environment"
-- !Description "Changes weather strength over specified time."
-- !Arguments "NewLine, Numerical, 20, [ 0 | 1 | 2 | 0.1 ], Strength"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 20, Time (in seconds)" 
-- !Arguments "30, Boolean, Relative"
LevelFuncs.Engine.Node.ChangeWeatherOverTime = function(newStrength, time, relative)
	
	if (relative) then
		newStrength = TEN.Flow.GetCurrentLevel().weatherStrength + newStrength
	end
		
	do LevelFuncs.Engine.Node.ConstructWeatherTimedData(5, 4, newStrength, time, true) end

end

-- !Name "If starfield is visible..."
-- !Section "Environment"
-- !Description "Checks if any stars or meteors are currently visible."
-- !Conditional "True"
LevelFuncs.Engine.Node.TestStarField = function()
	return TEN.Flow.GetCurrentLevel().starfield.starCount > 0 or TEN.Flow.GetCurrentLevel().starfield.meteorCount > 0
end

-- !Name "Set starfield"
-- !Section "Environment"
-- !Description "Sets starfield parameters, such as stars and meteors."
-- !Arguments "NewLine, Numerical, 25, [ 0 | 6000 ], Star count" "Numerical, 25, [ 0 | 100 ], Meteor count"
-- !Arguments "Numerical, 25, [ 0 | 10 ], Meteor spawn density" "Numerical, 25, [ 0 | 40 ], Meteor velocity"
LevelFuncs.Engine.Node.SetStarField = function(stars, meteors, meteorSpawnDensity, meteorVel)
	
	TEN.Flow.GetCurrentLevel().starfield.starCount = stars
    TEN.Flow.GetCurrentLevel().starfield.meteorCount = meteors
	TEN.Flow.GetCurrentLevel().starfield.meteorSpawnDensity = meteorSpawnDensity
	TEN.Flow.GetCurrentLevel().starfield.meteorVelocity = meteorVel

end

-- !Name "Change starfield over time"
-- !Section "Environment"
-- !Description "Changes stars and meteors over specified time."
-- !Arguments "NewLine, Numerical, 20, [ 0 | 6000 ], Stars count" "Numerical, 20, [ 0 | 100 ], Meteors count"
-- !Arguments "Numerical, 20, [ 0 | 10 ], Meteors spawn density" "Numerical, 20, [ 0 | 40 ], Meteors velocity" 
-- !Arguments "Numerical, 20, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, Time in seconds"
LevelFuncs.Engine.Node.ChangeStarFieldDensityOverTime = function(starCount, meteorCount, meteorDensity, meteorVelocity,  time)
	
	local structure = {x1 = starCount, x2 = meteorCount, x3 = meteorDensity, x4 = meteorVelocity}
    do LevelFuncs.Engine.Node.ConstructWeatherTimedData(4, 3, structure, time, true) end

end

-- !Name "If sky layer color is..."
-- !Section "Environment"
-- !Conditional "True"
-- !Description "Checks if specified sky layer is set to a specified color."
-- !Arguments "Enumeration, 20, [ Layer 1 | Layer 2 ], {0}, Sky layer" "Color, 15, Sky layer color"
LevelFuncs.Engine.Node.TestSkyLayer = function(type, color)
	
	if (type == 0) then
		return TEN.Flow.GetCurrentLevel().layer1.color == color
	elseif (type == 1) then
		return TEN.Flow.GetCurrentLevel().layer2.color == color
	end

end

-- !Name "Set sky layer"
-- !Section "Environment"
-- !Description "Sets sky layer parameters."
-- !Arguments "Enumeration, 20, [ Layer 1 | Layer 2 ], {0}, Sky layer" "Color, 15, Sky layer color" "Numerical, 15, [ -64 | 64 ], Speed" 
LevelFuncs.Engine.Node.SetSkyLayer = function(type, color, speed)
	
	if (type == 0) then
		TEN.Flow.GetCurrentLevel().layer1 = Flow.SkyLayer.new(color, speed)
	elseif (type == 1) then
		TEN.Flow.GetCurrentLevel().layer2 = Flow.SkyLayer.new(color, speed)	
	end
    
end

-- !Name "Change sky layer over time"
-- !Section "Environment"
-- !Description "Changes sky layer parameters over specified time."
-- !Arguments "NewLine, Enumeration, 20, [ Layer 1 | Layer 2 ], {0}, Sky layer" "Color, 15, Sky layer color"
-- !Arguments "Numerical, 15, [ -64 | 64 ], Speed" "Numerical, 15, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, Time in seconds" "35, Boolean, Smooth transition"
LevelFuncs.Engine.Node.ChangeSkyLayerOverTime = function(type, color, speed, time, smooth)

	local structure = {x1 = color.r, x2 = color.g, x3 = color.b, x4 = speed}

	if (type == 0) then
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(2, 3, structure, time, smooth) end
	elseif (type == 1) then
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(3, 3, structure, time, smooth) end
	end

end

-- !Name "If horizon is visible..."
-- !Section "Environment"
-- !Conditional "True"
-- !Description "Checks if specified horizon layer is visible."
-- !Arguments "Enumeration, 25, [ Layer 1 | Layer 2 ], {0}, Horizon layer"
LevelFuncs.Engine.Node.TestHorizonLayer = function(type)
	
	if (type == 0) then
		return TEN.Flow.GetCurrentLevel().horizon1.enabled
	elseif (type == 1) then
		return TEN.Flow.GetCurrentLevel().horizon2.enabled
	end

end

-- !Name "Enable horizon"
-- !Section "Environment"
-- !Description "Enables specified horizon layer and sets it to use specified object ID."
-- !Arguments "Enumeration, 25, [ Horizon 1 | Horizon 2 ], {0}, Horizon layer"
-- !Arguments "NewLine, WadSlots, {TEN.Objects.ObjID.HORIZON}, Choose moveable slot to use as a horizon"
LevelFuncs.Engine.Node.EnableHorizon = function(type, slot)
	
	if (type == 0) then
		TEN.Flow.GetCurrentLevel().horizon1 = Flow.Horizon(slot)
	elseif (type == 1) then
		TEN.Flow.GetCurrentLevel().horizon2 = Flow.Horizon(slot)
	end
	
end

-- !Name "Disable horizon"
-- !Section "Environment"
-- !Description "Disables specified horizon layer."
-- !Arguments "Enumeration, 25, [ Horizon 1 | Horizon 2 ], {0}, Horizon layer"
LevelFuncs.Engine.Node.DisableHorizon = function(type)
	
	if (type == 0) then
		TEN.Flow.GetCurrentLevel().horizon1.enabled = false
	elseif (type == 1) then
		TEN.Flow.GetCurrentLevel().horizon2.enabled = false
	end
	
end

-- !Name "Set position of a horizon"
-- !Section "Environment"
-- !Description "Sets position of a specified horizon layer."
-- !Arguments "NewLine, Enumeration, 34, [ Horizon 1 | Horizon 2 ], {0}, Horizon layer"
-- !Arguments "35, Boolean, Relative coordinates"
-- !Arguments "NewLine, Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 100, New position value to define"
LevelFuncs.Engine.Node.SetHorizonPosition = function(type, relative, newPosition)
	
	if (type == 0) then
		if (relative) then
			newPosition = TEN.Flow.GetCurrentLevel().horizon1.position + newPosition
		end
		TEN.Flow.GetCurrentLevel().horizon1.position = newPosition
	elseif (type == 1) then
		if (relative) then
			newPosition = TEN.Flow.GetCurrentLevel().horizon2.position + newPosition
		end
		TEN.Flow.GetCurrentLevel().horizon2.position = newPosition
	end
	
end

-- !Name "Set rotation of a horizon"
-- !Section "Environment"
-- !Description "Sets rotation of a specified horizon layer."
-- !Arguments "NewLine, Enumeration, 34, [ Horizon 1 | Horizon 2 ], {0}, Horizon layer"
-- !Arguments "35, Boolean, Relative coordinates"
-- !Arguments "NewLine, Vector3, [ -360 | 360 | 0 | 1 | 1 ], 100, New rotation value to define"
LevelFuncs.Engine.Node.SetHorizonRotation = function(type, relative, newRotation)
	
	if (type == 0) then
		if (relative) then
			currentRotation = Vec3(TEN.Flow.GetCurrentLevel().horizon1.rotation.x, TEN.Flow.GetCurrentLevel().horizon1.rotation.y, TEN.Flow.GetCurrentLevel().horizon1.rotation.z)
			newRotation = currentRotation + newRotation
		end
		TEN.Flow.GetCurrentLevel().horizon1.rotation = Rotation(newRotation.x, newRotation.y, newRotation.z)
	elseif (type == 1) then
		if (relative) then
			currentRotation = Vec3(TEN.Flow.GetCurrentLevel().horizon2.rotation.x, TEN.Flow.GetCurrentLevel().horizon2.rotation.y, TEN.Flow.GetCurrentLevel().horizon2.rotation.z)
			newRotation = currentRotation + newRotation
		end
		TEN.Flow.GetCurrentLevel().horizon2.rotation = Rotation(newRotation.x, newRotation.y, newRotation.z)
	end
	
end

-- !Name "Set transparency of a horizon"
-- !Section "Environment"
-- !Description "Sets transparency of a specified horizon layer."
-- !Arguments "Enumeration, 25, [ Horizon 1 | Horizon 2 ], {0}, Horizon layer"
-- !Arguments "Numerical, 15, [ 0 | 1 | 2 | 0.1 | 1 ], {1} Transparency"
LevelFuncs.Engine.Node.SetHorizonTransparency = function(type, transparency)
	
	if (type == 0) then
		TEN.Flow.GetCurrentLevel().horizon1.transparency = transparency
	elseif (type == 1) then
		TEN.Flow.GetCurrentLevel().horizon2.transparency = transparency
	end
	
end

-- !Name "Change position of a horizon over time"
-- !Section "Environment"
-- !Description "Gradually changes position of a specified horizon layer over specified timespan."
-- !Arguments "Enumeration, 25, [ Horizon 1 | Horizon 2 ], {0}, Horizon layer"
-- !Arguments "NewLine, Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 100, New position value to define"
-- !Arguments "NewLine, Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 34, Time (in seconds)" "Boolean, 35, Relative coordinates" "Boolean, 31, Smooth motion"
LevelFuncs.Engine.Node.ChangeHorizonPositionOverTimespan = function(type, newPosition, time, relative, smooth)
		
	if (type == 0) then
		if (relative) then
			newPosition = TEN.Flow.GetCurrentLevel().horizon1.position + newPosition
		end
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(6, 2, newPosition, time, smooth) end	
	elseif (type == 1) then
		if (relative) then
			newPosition = TEN.Flow.GetCurrentLevel().horizon2.position + newPosition
		end
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(9, 2, newPosition, time, smooth) end
	end
	
end

-- !Name "Change rotation of a horizon over time"
-- !Section "Environment"
-- !Description "Gradually changes rotation of a specified horizon layer over specified timespan."
-- !Arguments "Enumeration, 25, [ Horizon 1 | Horizon 2 ], {0}, Horizon layer"
-- !Arguments "NewLine, Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 100, New rotation value to define"
-- !Arguments "NewLine, Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 34, Time (in seconds)" "Boolean, 35, Relative coordinates" "Boolean, 31, Smooth motion"
LevelFuncs.Engine.Node.ChangeHorizonRotationOverTimespan = function(type, newRotation, time, relative, smooth)
	
	if (type == 0) then
		if (relative) then
			currentRotation = Vec3(TEN.Flow.GetCurrentLevel().horizon1.rotation.x, TEN.Flow.GetCurrentLevel().horizon1.rotation.y, TEN.Flow.GetCurrentLevel().horizon1.rotation.z)
			newRotation = currentRotation + newRotation
		end
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(7, 2, newRotation, time, smooth) end	
	elseif (type == 1) then
		if (relative) then
			currentRotation = Vec3(TEN.Flow.GetCurrentLevel().horizon2.rotation.x, TEN.Flow.GetCurrentLevel().horizon2.rotation.y, TEN.Flow.GetCurrentLevel().horizon2.rotation.z)
			newRotation = currentRotation + newRotation
		end
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(10, 2, newRotation, time, smooth) end
	end
end

-- !Name "Change transparency of a horizon over time"
-- !Section "Environment"
-- !Description "Gradually changes transparency of a specified horizon layer over specified timespan."
-- !Arguments "NewLine, Enumeration, 25, [ Horizon 1 | Horizon 2 ], {0}, Horizon layer"
-- !Arguments "Numerical, 20, [ 0 | 1 | 2 | 0.1 | 1 ], {1} Transparency" 
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 20, Time (in seconds)"
-- !Arguments "Boolean, 35, Relative coordinates"
LevelFuncs.Engine.Node.ChangeHorizonTransparencyOverTimespan = function(type, newTransparency, time, relative)
	
	if (type == 0) then
		if (relative) then
			newTransparency = TEN.Flow.GetCurrentLevel().horizon1.transparency + newTransparency
		end
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(8, 4, newTransparency, time, true) end	
	elseif (type == 1) then
		if (relative) then
			newTransparency = TEN.Flow.GetCurrentLevel().horizon2.transparency + newTransparency
		end
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(11, 4, newTransparency, time, true) end
	end
	
end

-- !Name "Set rotation speed of a horizon"
-- !Section "Environment"
-- !Description "Sets a constant rotation speed of a specified horizon layer."
-- !Arguments "Enumeration, 25, [ Horizon 1 | Horizon 2 ], {0}, Horizon layer"
-- !Arguments "NewLine, Vector3, [ -360 | 360 | 2 ], 100, Rotation speed in degrees per second"
LevelFuncs.Engine.Node.SetHorizonRotationSpeed = function(type, speed)

	LevelVars.Engine.WeatherData = LevelVars.Engine.WeatherData or {}

	if (type == 0 and TEN.Flow.GetCurrentLevel().horizon1.enabled == true) then
		LevelVars.Engine.WeatherData["Horizon1Speed"] = Vec3(speed.x/30, speed.y/30, speed.z/30)
		AddCallback(TEN.Logic.CallbackPoint.PRECONTROLPHASE,LevelFuncs.Engine.Node.RotateHorizon1)
	elseif (type == 1 and TEN.Flow.GetCurrentLevel().horizon2.enabled == true) then
		LevelVars.Engine.WeatherData["Horizon2Speed"] = Vec3(speed.x/30, speed.y/30, speed.z/30)
		AddCallback(TEN.Logic.CallbackPoint.PRECONTROLPHASE,LevelFuncs.Engine.Node.RotateHorizon2)
	end
	
end

-- !Name "Stop rotation of a horizon"
-- !Section "Environment"
-- !Description "Stops rotation of a horizon."
-- !Arguments "Enumeration, 25, [ Horizon 1 | Horizon 2 ], {0}, Horizon layer"
LevelFuncs.Engine.Node.StopHorizonRotation = function(type)
	
	if (type == 0 and TEN.Flow.GetCurrentLevel().horizon1.enabled == true) then
		RemoveCallback(TEN.Logic.CallbackPoint.PRECONTROLPHASE, LevelFuncs.Engine.Node.RotateHorizon1)
	elseif (type == 1 and TEN.Flow.GetCurrentLevel().horizon2.enabled == true) then
		RemoveCallback(TEN.Logic.CallbackPoint.PRECONTROLPHASE, LevelFuncs.Engine.Node.RotateHorizon2)
	end

end

-- !Name "Swap horizon with a crossfade over time"
-- !Section "Environment"
-- !Description "Automatically swaps two horizons with a crossfade over specified timespan."
-- !Arguments "NewLine, WadSlots, 75, {TEN.Objects.ObjID.HORIZON}, Fade in which horizon"
-- !Arguments "Numerical, 25 [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, Time (in seconds)"
LevelFuncs.Engine.Node.FadeHorizonFromSlotOverTimespan = function(slot, time)

	if (TEN.Flow.GetCurrentLevel().horizon1.transparency == 1) then

		if (TEN.Flow.GetCurrentLevel().horizon1.objectID == slot) then return end

		TEN.Flow.GetCurrentLevel().horizon2.transparency = 0
		TEN.Flow.GetCurrentLevel().horizon2.enabled = true
		TEN.Flow.GetCurrentLevel().horizon2.objectID = slot
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(8, 4, 0, time, true) end
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(11, 4, 1, time, true) end

	elseif (TEN.Flow.GetCurrentLevel().horizon2.transparency == 1) then

		if (TEN.Flow.GetCurrentLevel().horizon2.objectID == slot) then return end

		TEN.Flow.GetCurrentLevel().horizon1.transparency = 0
		TEN.Flow.GetCurrentLevel().horizon1.enabled = true
		TEN.Flow.GetCurrentLevel().horizon1.objectID = slot
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(8, 4, 1, time, true) end
		do LevelFuncs.Engine.Node.ConstructWeatherTimedData(11, 4, 0, time, true) end

	end

end

-- !Ignore
-- RotateHorizons
LevelFuncs.Engine.Node.RotateHorizon1 = function()

	if TEN.Flow.GetCurrentLevel().horizon1.enabled == true and LevelVars.Engine.WeatherData["Horizon1Speed"] then
		
		local rotation = Vec3(TEN.Flow.GetCurrentLevel().horizon1.rotation.x, TEN.Flow.GetCurrentLevel().horizon1.rotation.y, TEN.Flow.GetCurrentLevel().horizon1.rotation.z) + LevelVars.Engine.WeatherData["Horizon1Speed"]
		TEN.Flow.GetCurrentLevel().horizon1.rotation = Rotation(rotation.x, rotation.y, rotation.z)

	end

end

-- !Ignore
-- RotateHorizons
LevelFuncs.Engine.Node.RotateHorizon2 = function()

	if TEN.Flow.GetCurrentLevel().horizon2.enabled == true and LevelVars.Engine.WeatherData["Horizon2Speed"] then

		local rotation = Vec3(TEN.Flow.GetCurrentLevel().horizon2.rotation.x, TEN.Flow.GetCurrentLevel().horizon2.rotation.y, TEN.Flow.GetCurrentLevel().horizon2.rotation.z) + LevelVars.Engine.WeatherData["Horizon2Speed"]
		TEN.Flow.GetCurrentLevel().horizon2.rotation = Rotation(rotation.x, rotation.y, rotation.z)

	end

end
