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

LevelFuncs.Engine.Node.SetInteractionHighlightType = function(index)
	local interactionIconType =
	{
		[0] = TEN.Objects.InteractionType.PICKUP,
		[1] = TEN.Objects.InteractionType.TALK,
		[2] = TEN.Objects.InteractionType.USE,
	}	
	return interactionIconType[index]
end

-- Helper function for input nodes to get correct input.
LevelFuncs.Engine.Node.GetInput = function(index)
local keyboardInput =
    {
        [0]  = TEN.Input.ActionID.FORWARD,
        [1]  = TEN.Input.ActionID.BACK,
        [2]  = TEN.Input.ActionID.LEFT,
        [3]  = TEN.Input.ActionID.RIGHT,
        [4]  = TEN.Input.ActionID.STEP_LEFT,
        [5]  = TEN.Input.ActionID.STEP_RIGHT,
        [6]  = TEN.Input.ActionID.WALK,
        [7]  = TEN.Input.ActionID.SPRINT,
        [8]  = TEN.Input.ActionID.CROUCH,
        [9]  = TEN.Input.ActionID.JUMP,
        [10] = TEN.Input.ActionID.ROLL,
        [11] = TEN.Input.ActionID.ACTION,
        [12] = TEN.Input.ActionID.DRAW,
        [13] = TEN.Input.ActionID.LOOK,
        [14] = TEN.Input.ActionID.ACCELERATE,
        [15] = TEN.Input.ActionID.REVERSE,
        [16] = TEN.Input.ActionID.FASTER,
        [17] = TEN.Input.ActionID.SLOWER,
        [18] = TEN.Input.ActionID.BRAKE,
        [19] = TEN.Input.ActionID.FIRE,
        [20] = TEN.Input.ActionID.FLARE,
        [21] = TEN.Input.ActionID.SMALL_MEDIPACK,
        [22] = TEN.Input.ActionID.LARGE_MEDIPACK,
        [23] = TEN.Input.ActionID.PREVIOUS_WEAPON,
        [24] = TEN.Input.ActionID.NEXT_WEAPON,
        [25] = TEN.Input.ActionID.WEAPON_1,
        [26] = TEN.Input.ActionID.WEAPON_2,
        [27] = TEN.Input.ActionID.WEAPON_3,
        [28] = TEN.Input.ActionID.WEAPON_4,
        [29] = TEN.Input.ActionID.WEAPON_5,
        [30] = TEN.Input.ActionID.WEAPON_6,
        [31] = TEN.Input.ActionID.WEAPON_7,
        [32] = TEN.Input.ActionID.WEAPON_8,
        [33] = TEN.Input.ActionID.WEAPON_9,
        [34] = TEN.Input.ActionID.WEAPON_10,
        [35] = TEN.Input.ActionID.SELECT,
        [36] = TEN.Input.ActionID.DESELECT,
        [37] = TEN.Input.ActionID.PAUSE,
        [38] = TEN.Input.ActionID.INVENTORY,
        [39] = TEN.Input.ActionID.SAVE,
        [40] = TEN.Input.ActionID.LOAD,
        [41] = TEN.Input.ActionID.A,
        [42] = TEN.Input.ActionID.B,
        [43] = TEN.Input.ActionID.C,
        [44] = TEN.Input.ActionID.D,
        [45] = TEN.Input.ActionID.E,
        [46] = TEN.Input.ActionID.F,
        [47] = TEN.Input.ActionID.G,
        [48] = TEN.Input.ActionID.H,
        [49] = TEN.Input.ActionID.I,
        [50] = TEN.Input.ActionID.J,
        [51] = TEN.Input.ActionID.K,
        [52] = TEN.Input.ActionID.L,
        [53] = TEN.Input.ActionID.M,
        [54] = TEN.Input.ActionID.N,
        [55] = TEN.Input.ActionID.O,
        [56] = TEN.Input.ActionID.P,
        [57] = TEN.Input.ActionID.Q,
        [58] = TEN.Input.ActionID.R,
        [59] = TEN.Input.ActionID.S,
        [60] = TEN.Input.ActionID.T,
        [61] = TEN.Input.ActionID.U,
        [62] = TEN.Input.ActionID.V,
        [63] = TEN.Input.ActionID.W,
        [64] = TEN.Input.ActionID.X,
        [65] = TEN.Input.ActionID.Y,
        [66] = TEN.Input.ActionID.Z,
    }
	
	return keyboardInput[index]
end		