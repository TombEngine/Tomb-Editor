LevelFuncs.Engine.Node = {}

-- Helper function for value comparisons. Any function which uses
-- CompareOperator arguments should use this helper function for comparison.
LevelFuncs.Engine.Node.CompareValue = function(operand, reference, operator)
	local result = false

	-- Fix Lua-specific treatment of bools as non-numerical values
	if (operand == false) then operand = 0 end;
	if (operand == true) then operand = 1 end;
	if (reference == false) then reference = 0 end;
	if (reference == true) then reference = 1 end;

	if (operator == 0 and operand == reference) then result = true end
	if (operator == 1 and operand ~= reference) then result = true end
	if (operator == 2 and operand < reference) then result = true end
	if (operator == 3 and operand <= reference) then result = true end
	if (operator == 4 and operand > reference) then result = true end
	if (operator == 5 and operand >= reference) then result = true end
	return result
end

-- Helper function for value modification.
LevelFuncs.Engine.Node.ModifyValue = function(operand, reference, operator)
	local result = reference
	if (operator == 0) then result = reference + operand end
	if (operator == 1) then result = reference - operand end
	if (operator == 2) then result = reference * operand end
	if (operator == 3) then result = reference / operand end
	if (operator == 4) then result = operand end
	return result
end

-- Helper function for easy generation of a display string with all parameters set.
LevelFuncs.Engine.Node.GenerateString = function(textOrKey, x, y, scale, alignment, effects, color, alpha)
	local options = {}
	if (effects == 1 or effects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.SHADOW) end
	if (effects == 2 or effects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.BLINK) end
	if (alignment == 1) then table.insert(options, TEN.Strings.DisplayStringOption.CENTER) end
	if (alignment == 2) then table.insert(options, TEN.Strings.DisplayStringOption.RIGHT) end
	if (alpha ~= nil) then color.a = alpha * 255 end
	local rX, rY = TEN.Util.PercentToScreen(x, y)
	return TEN.Strings.DisplayString(textOrKey, TEN.Vec2(rX, rY), scale, color, TEN.Flow.IsStringPresent(textOrKey), options)
end

-- Helper function for easy generation text option for display string.
LevelFuncs.Engine.Node.GeneratesTextOption = function (alignment, effects)
	local options = {}
	if (effects == 1 or effects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.SHADOW) end
	if (effects == 2 or effects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.BLINK) end
	if (alignment == 1) then table.insert(options, TEN.Strings.DisplayStringOption.CENTER) end
	if (alignment == 2) then table.insert(options, TEN.Strings.DisplayStringOption.RIGHT) end
	return options
end

-- Helper function to split string using specified delimiter.
LevelFuncs.Engine.Node.SplitString = function(inputStr, delimiter)
	if inputStr == nil then
		inputStr = "%s"
	end

	local t = {}
	for str in string.gmatch(inputStr, "([^" .. delimiter .. "]+)") do
		table.insert(t, str)
	end

	return t
end

LevelFuncs.Engine.Node.StringIsEmpty = function(str)
	return (str == nil or str == '')
end

-- Wrap angle value around 360
LevelFuncs.Engine.Node.WrapRotation = function(source, value)
	if (value == 0) then
		return source
	end

	local rot = source + value
	if (rot > 360) then
		rot = rot - 360
	elseif (rot < 0) then
		rot = 360 + rot
	end
	return rot
end

LevelFuncs.Engine.Node.Smoothstep = function(source)
	source = math.max(0, math.min(1, source))
	return ((source ^ 3) * (source * (source * 6 - 15) + 10))
end

LevelFuncs.Engine.Node.Lerp = function(val1, val2, factor)
	return val1 * (1 - factor) + val2 * factor
end

-- Convert UI enum to room flag ID enum
LevelFuncs.Engine.Node.GetRoomFlag = function(value)
	local roomFlagID =
	{
		[0] = Objects.RoomFlagID.WATER,
		[1] = Objects.RoomFlagID.QUICKSAND,
		[2] = Objects.RoomFlagID.SKYBOX,
		[3] = Objects.RoomFlagID.WIND,
		[4] = Objects.RoomFlagID.COLD,
		[5] = Objects.RoomFlagID.DAMAGE,
		[6] = Objects.RoomFlagID.NOLENSFLARE,
	}
	return roomFlagID[value]
end

LevelFuncs.Engine.Node.GetSoundTrackType = function(value)
	local SoundTrackType =
	{
		[0] = Sound.SoundTrackType.ONESHOT,
		[1] = Sound.SoundTrackType.LOOPED,
		[2] = Sound.SoundTrackType.VOICE,
	}
	return SoundTrackType[value]
end

LevelFuncs.Engine.Node.GetBlendMode = function(index)
	local blendID =
	{
		[0] = TEN.Effects.BlendID.OPAQUE,
		[1] = TEN.Effects.BlendID.ALPHATEST,
		[2] = TEN.Effects.BlendID.ADDITIVE,
		[3] = TEN.Effects.BlendID.NOZTEST,
		[4] = TEN.Effects.BlendID.SUBTRACTIVE,
		[5] = TEN.Effects.BlendID.WIREFRAME,
		[6] = TEN.Effects.BlendID.EXCLUDE,
		[7] = TEN.Effects.BlendID.SCREEN,
		[8] = TEN.Effects.BlendID.LIGHTEN,
		[9] = TEN.Effects.BlendID.ALPHABLEND
	}
	return blendID[index]
end

LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode = function(index)
	local displaySpriteAlignMode =
	{
		[0] = TEN.View.AlignMode.CENTER,
		[1] = TEN.View.AlignMode.CENTER_TOP,
		[2] = TEN.View.AlignMode.CENTER_BOTTOM,
		[3] = TEN.View.AlignMode.CENTER_LEFT,
		[4] = TEN.View.AlignMode.CENTER_RIGHT,
		[5] = TEN.View.AlignMode.TOP_LEFT,
		[6] = TEN.View.AlignMode.TOP_RIGHT,
		[7] = TEN.View.AlignMode.BOTTOM_LEFT,
		[8] = TEN.View.AlignMode.BOTTOM_RIGHT
	}
	return displaySpriteAlignMode[index]
end

LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode = function(index)
	local displaySpriteScaleMode =
	{
		[0] = TEN.View.ScaleMode.FIT,
		[1] = TEN.View.ScaleMode.FILL,
		[2] = TEN.View.ScaleMode.STRETCH
	}
	return displaySpriteScaleMode[index]
end

LevelFuncs.Engine.Node.GetGameStatus = function(index)
	local gameStatus =
	{
		[0] = Flow.GameStatus.NORMAL,
		[1] = Flow.GameStatus.NEW_GAME,
		[2] = Flow.GameStatus.LOAD_GAME,
		[3] = Flow.GameStatus.EXIT_GAME,
		[4] = Flow.GameStatus.EXIT_TO_TITLE,
		[5] = Flow.GameStatus.LARA_DEAD,
		[6] = Flow.GameStatus.LEVEL_COMPLETE
	}
	return gameStatus[index]
end

LevelFuncs.Engine.Node.GetFreezeMode = function(index)
	local freezeMode =
	{
		[0] = Flow.FreezeMode.NONE,
		[1] = Flow.FreezeMode.FULL,
		[2] = Flow.FreezeMode.SPECTATOR,
		[3] = Flow.FreezeMode.PLAYER
	}
	return freezeMode[index]
end

LevelFuncs.Engine.Node.SetPostProcessMode = function(index)
	local postProcessMode =
	{
		[0] = TEN.View.PostProcessMode.NONE,
		[1] = TEN.View.PostProcessMode.MONOCHROME,
		[2] = TEN.View.PostProcessMode.NEGATIVE,
		[3] = TEN.View.PostProcessMode.EXCLUSION,
	}
	return postProcessMode[index]
end