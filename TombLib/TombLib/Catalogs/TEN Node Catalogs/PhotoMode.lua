local PhotoMode = require("Engine.PhotoMode.PhotoMode")
-- !Name "Unlock outfit in Photo Mode"
-- !Section "Photo mode"
-- !Description "Unlocks outfit in photo mode for specified name."
-- !Arguments "NewLine, String, 100, Outfit Name set in PhotoMode Outfits.lua"
LevelFuncs.Engine.Node.PhotoModeUnlockOutfit = function(outfitName)
    PhotoMode.UnlockOutfit(outfitName)
end

-- !Name "Unlock Accessory in Photo Mode"
-- !Section "Photo mode"
-- !Description "Unlocks an accessory in Photo Mode for the specified name."
-- !Arguments "NewLine, String, 100, Accessory Name set in PhotoModeSetup.lua"
LevelFuncs.Engine.Node.PhotoModeUnlockAccessory = function(accessoryName)
    PhotoMode.UnlockAccessory(accessoryName)
end

-- !Name "Unlock Expression in Photo Mode"
-- !Section "Photo mode"
-- !Description "Unlocks an expression in Photo Mode for the specified name."
-- !Arguments "NewLine, String, 100, Expression Name set in PhotoModeSetup.lua"
LevelFuncs.Engine.Node.PhotoModeUnlockExpression = function(expressionName)
    PhotoMode.UnlockExpression(expressionName)
end

-- !Name "Unlock Frame in Photo Mode"
-- !Section "Photo mode"
-- !Description "Unlocks a frame overlay in Photo Mode for the specified name."
-- !Arguments "NewLine, String, 100, Frame Name set in PhotoModeSetup.lua"
LevelFuncs.Engine.Node.PhotoModeUnlockFrame = function(frameName)
    PhotoMode.UnlockFrame(frameName)
end

-- !Name "Unlock Pose in Photo Mode"
-- !Section "Photo mode"
-- !Description "Unlocks a pose in Photo Mode for the specified name."
-- !Arguments "NewLine, String, 100, Pose Name set in PhotoModeSetup.lua"
LevelFuncs.Engine.Node.PhotoModeUnlockPose = function(poseName)
    PhotoMode.UnlockPose(poseName)
end

-- !Name "Clear Data in Photo Mode"
-- !Section "Photo mode"
-- !Description "Clears all unlocked Photo Mode content so it no longer appears in the selectors."
LevelFuncs.Engine.Node.PhotoModeClearData = function()
    PhotoMode.ClearData()
end