local CustomDiary = require("Engine.CustomDiary")

-- !Name "Create a diary from the template file"
-- !Section "Diary"
-- !Description "Create a diary from template file stored in scripts folder."
-- !Arguments "NewLine, String, [ NoMultiline ], {DiarySetup}, Name of the file to import diary entries"


LevelFuncs.Engine.Node.DiaryImport = function(fileName)
	CustomDiary.ImportDiary(fileName)
end

-- !Name "Start or stop the diary feature"
-- !Section "Diary"
-- !Description "Starts or stops the diary feature. To be used at start of a new level to enable the diary."
-- !Arguments "Enumeration, 30, [ Start | Stop ], Status"

LevelFuncs.Engine.Node.DiaryStatus = function(value)

	if GameVars.Engine.Diaries then
		if value == 0 then
			CustomDiary.Status(true)
		elseif value == 1 then
			CustomDiary.Status(false)
		end
	end
end

-- !Name "Create a diary"
-- !Section "Diary"
-- !Description "Start a new diary with specified object slot."
-- !Arguments "NewLine, 100, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, object to call the diary with in the inventory"
-- !Arguments "Newline, SpriteSlots, 60, {TEN.Objects.ObjID.DIARY_SPRITES}, Background sprite sequence object ID"
-- !Arguments "Numerical, 20, [ 0 | 9999 | 0 ],{0}, Sprite ID for background in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, 20, {TEN.Color(255,255,255)}, Color of the diary sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], {50}, Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ -1000 | 1000 | 2 ], {50}, Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {0}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"
-- !Arguments "Numerical, 15, [ 0 | 255 | 0 ], {255}, Alpha Blend for Diary sprite"
-- !Arguments "NewLine, SoundEffects, 50, Page Flip Sound,{369}" "SoundEffects, 50, Exit Sound,{369}"

LevelFuncs.Engine.Node.CreateDiary = function(object, objectIDbg, spriteIDbg, colorbg, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode, alpha, pageSound, exitSound)
	local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
    local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)
	local pos = TEN.Vec2(posX, posY)
	local scale = TEN.Vec2(scaleX, scaleY)

	CustomDiary.Create(object, objectIDbg, spriteIDbg, colorbg, pos, rot, scale, alignM, scaleM, blendID, alpha, pageSound, exitSound)
	
end

-- !Name "Add background image to a diary"
-- !Section "Diary"
-- !Description "Add a background image to the diary."
-- !Arguments "NewLine, 100, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary object to add background to"
-- !Arguments "Newline, SpriteSlots, 60, {TEN.Objects.ObjID.DIARY_SPRITES}, Background sprite sequence object ID"
-- !Arguments "Numerical, 20, [ 0 | 9999 | 0 ], Sprite ID for background in sprite sequence\nRange[0 to 9999],{1}"
-- !Arguments "Color, 20, {TEN.Color(255,0,0)}, Color of background sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], {50}, Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ -1000 | 1000 | 2 ], {50}, Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {0}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], {2}, Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"
-- !Arguments "Numerical, 15, {128}, [ 0 | 255 | 2 ], Alpha [0 to 255]"

LevelFuncs.Engine.Node.DiaryBackgroundImage = function(object, objectIDbg, spriteIDbg, colorbg, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode, alpha) 

    local dataName = object .. "_diarydata"
	local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)
	local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
    local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
	
	if GameVars.Engine.Diaries[dataName] then
		local pos = TEN.Vec2(posX, posY)
		local scale = TEN.Vec2(scaleX, scaleY)

		local diary = CustomDiary.Get(object)
		diary:AddBackground(objectIDbg, spriteIDbg, colorbg, pos, rot, scale, alignM, scaleM, blendID, alpha)
	end
end

-- !Name "Customize notifications for a diary"
-- !Section "Diary"
-- !Description "Customize notifications for a diary."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary object to customize notifications for" "Numerical, 20, [ 1 | 65535 ], {3}, Notification Time"
-- !Arguments "Newline, SpriteSlots, 60, {TEN.Objects.ObjID.DIARY_SPRITES}, Sprite sequence object ID for notification"
-- !Arguments "Numerical, 20, [ 0 | 9999 | 0 ], Sprite ID for background in sprite sequence\nRange[0 to 9999],{2}"
-- !Arguments "Color, 20, {TEN.Color(255,255,255)}, Color of background sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], {95}, Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ -1000 | 1000 | 2 ],{95}, Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, 20, {5}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, 20, {5}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {0}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"
-- !Arguments "NewLine, SoundEffects, 100, Notification Sound,{369}" 
LevelFuncs.Engine.Node.DiaryCustomizeNotifications = function(object, notificationTime, objectID, spriteID, color, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode, notificationSound)
	
	local dataName = object .. "_diarydata"
	local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
    local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)
	local pos = TEN.Vec2(posX, posY)
	local scale = TEN.Vec2(scaleX, scaleY)

	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:CustomizeNotification(notificationTime, objectID, spriteID, color, pos, rot, scale, alignM, scaleM, blendID, notificationSound)
	end

end

-- !Name "Customize page numbers for a diary"
-- !Section "Diary"
-- !Description "Customize page numbers for a diary."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary object to customize page numbers for" "Numerical, 20, Entry Type, [ 1 | 2 ], {2}, Type 1 is just page number\nType 2 is formatted as\n (prefix page number separator total page numbers)"
-- !Arguments "NewLine, String, 35, [ NoMultiline ], Prefix" "String, 35, [ NoMultiline ], Separator" "Numerical, 15, {98}, X position, [ 0 | 100 ]" "Numerical, 15, {95}, Y position, [ 0 | 100 ]"
-- !Arguments "NewLine, Enumeration, 20, [ Left | Center | Right ], {2}, Horizontal alignment"
-- !Arguments "Enumeration, 40, [ Flat | Shadow | Blinking | Shadow + Blinking ], {1}, Effects"
-- !Arguments "Numerical, 20, {0.5}, [ 0 | 9 | 2 | 0.1 ], Scale" "Color, 20, {TEN.Color(255,255,255)}, Text color"

LevelFuncs.Engine.Node.DiaryCustomizePageNumbers = function(object, type, prefix, separator, textX, textY, textAlignment, textEffects, textScale, textColor)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then
		local pos = TEN.Vec2(textX, textY)
		local options = {}
		if (textEffects == 1 or textEffects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.SHADOW) end
		if (textEffects == 2 or textEffects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.BLINK) end
		if (textAlignment == 1) then table.insert(options, TEN.Strings.DisplayStringOption.CENTER) end
		if (textAlignment == 2) then table.insert(options, TEN.Strings.DisplayStringOption.RIGHT) end

		local diary = CustomDiary.Get(object)
		diary:CustomizePageNumbers(type, prefix, separator, pos, options, textScale, textColor)
	end
	
end

-- !Name "Customize controls for a diary"
-- !Section "Diary"
-- !Description "Customize controls for a diary."
-- !Arguments "NewLine, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary object to customize controls for"
-- !Arguments "NewLine, Numerical, 33, [ 0 | 100 ], {5}, X position" "Numerical, 33, [ 0 | 100 ] {95}, Y position"
-- !Arguments "NewLine, Enumeration, 20, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "Enumeration, 40, [ Flat | Shadow | Blinking | Shadow + Blinking ], {1}, Effects"
-- !Arguments "Numerical, 20, {1}, [ 0 | 9 | 2 | 0.1 ], {0.5}, Scale" "Color, 20, {TEN.Color(255,255,255)}, Text color"

LevelFuncs.Engine.Node.DiaryCustomizeControls = function(object, textX, textY, textAlignment, textEffects, textScale, textColor)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then
		local pos = TEN.Vec2(textX, textY)
		local options = {}
		if (textEffects == 1 or textEffects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.SHADOW) end
		if (textEffects == 2 or textEffects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.BLINK) end
		if (textAlignment == 1) then table.insert(options, TEN.Strings.DisplayStringOption.CENTER) end
		if (textAlignment == 2) then table.insert(options, TEN.Strings.DisplayStringOption.RIGHT) end

		local diary = CustomDiary.Get(object)
		diary:CustomizeControls(pos, options, textScale, textColor)
	end
	
end

-- !Name "Add a text entry to the specified page of a diary"
-- !Section "Diary"
-- !Description "Add a text entry to a diary."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary object to add a text entry" "Numerical, 20, [ 1 | 65535 ], {1}, Page number of the text entry"
-- !Arguments "NewLine, String, 70, [ Multiline ], Diary entry" "Numerical, 15, X position, [ 0 | 100 ]" "Numerical, 15, Y position, [ 0 | 100 ]"
-- !Arguments "NewLine, Enumeration, 20, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "Enumeration, 40, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "Numerical, 20, {1}, [ 0 | 9 | 2 | 0.1 ], Scale" "Color, 20, {TEN.Color(255,255,255)}, Text color"

LevelFuncs.Engine.Node.DiaryAddTextEntry = function(object, pageIndex, text, textX, textY, textAlignment, textEffects, textScale, textColor)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then

		local pos = TEN.Vec2(textX, textY)
		local options = {}
		if (textEffects == 1 or textEffects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.SHADOW) end
		if (textEffects == 2 or textEffects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.BLINK) end
		if (textAlignment == 1) then table.insert(options, TEN.Strings.DisplayStringOption.CENTER) end
		if (textAlignment == 2) then table.insert(options, TEN.Strings.DisplayStringOption.RIGHT) end

		local diary = CustomDiary.Get(object)
		diary:AddTextEntry(pageIndex, text, pos, options, textScale, textColor)
	end
	
end

-- !Name "Add an image entry to the specified page of a diary"
-- !Section "Diary"
-- !Description "Add a new image entry to a diary."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary object to add an image entry" "Numerical, 20, [ 1 | 65535 ], {1}, Page number of the image entry"
-- !Arguments "Newline, SpriteSlots, 60, {TEN.Objects.ObjID.DIARY_ENTRY_SPRITES}, Sprite sequence object ID"
-- !Arguments "Numerical, 20, [ 0 | 9999 | 0 ], Sprite ID in sprite sequence\nRange[0 to 9999],{0}"
-- !Arguments "Color, 20, {TEN.Color(255,255,255)}, Color of image entry sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {0}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"

LevelFuncs.Engine.Node.DiaryAddImageEntry = function(object, pageIndex, objectIDbg, spriteIDbg, colorbg, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)
	
	local dataName = object .. "_diarydata"
	local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
    local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)
	local pos = TEN.Vec2(posX, posY)
	local scale = TEN.Vec2(scaleX, scaleY)

	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:AddImageEntry(pageIndex, objectIDbg, spriteIDbg, colorbg, pos, rot, scale, alignM, scaleM, blendID)
	end

end

-- !Name "Add or update narration to a diary page"
-- !Section "Diary"
-- !Description "Add or update narration soundtrack to specified page number."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary object to add narration to" "Numerical, 20, [ 1 | 65535 ], {1}, Page number to add narration to"
-- !Arguments "NewLine, SoundTracks, Name of the audiotrack to add as narration to the page"

LevelFuncs.Engine.Node.DiaryNarrationPages = function(object, pageIndex, trackName)
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:AddNarration(pageIndex, trackName)
	end
end

-- !Name "Unlock diary pages till..."
-- !Section "Diary"
-- !Description "Unlock the diary pages till specified page number."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary object to unlock pages for" 
-- !Arguments "Numerical, 20, [ 1 | 65535 ], {1}, Page number to unlock till"
-- !Arguments "NewLine, Boolean, Notification"
LevelFuncs.Engine.Node.DiaryUnlockPages = function(object, index, notification)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:UnlockPages(index, notification)
	end
end

-- !Name "Unlock a single diary page"
-- !Section "Diary"
-- !Description "Unlock a single diary page. Pages are unlocked in the order this function is called, and the diary displays them in that order. This sets the diary to per-page unlock mode."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary object to unlock a page for" 
-- !Arguments "Numerical, 20, [ 1 | 65535 ], {1}, Page number to unlock"
-- !Arguments "NewLine, Boolean, Notification"
LevelFuncs.Engine.Node.DiaryUnlockPage = function(object, index, notification)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:UnlockPage(index, notification)
	end
end

-- !Name "Clear a diary page"
-- !Section "Diary"
-- !Description "Clears the specified diary page."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary object to clear the page for" 
-- !Arguments "Numerical, 20, [ 1 | 65535 ], {1}, Page number to clear"

LevelFuncs.Engine.Node.DiaryClearPage = function(object, index)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:ClearPage(index)
	end

end

-- !Name "Show diary at the specified page"
-- !Section "Diary"
-- !Description "This function displays the diary at the specified page."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary to display" 
-- !Arguments "Numerical, 20, [ 1 | 65535 ], {1}, Page number to display"
LevelFuncs.Engine.Node.ShowDiary = function(object, pageIndex)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:ShowDiary(pageIndex)
	end
end

-- !Name "Clear a diary"
-- !Section "Diary"
-- !Description "Clears a diary. Do this once a diary has served its purpose to conserve savegame memory."
-- !Arguments "NewLine, WadSlots, [ _ITEM ], {TEN.Objects.ObjID.DIARY_ITEM}, Diary to clear."

LevelFuncs.Engine.Node.ClearDiary = function(object)

	CustomDiary.Delete(object)

end