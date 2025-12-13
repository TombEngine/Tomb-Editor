-- !Name "Draw display sprite"
-- !Section "Sprites"
-- !Conditional "False"
-- !Description "Draw a display sprite on the frame that this node is active."
-- !Arguments "Newline, SpriteSlots, 61, Sprite sequence object ID"
-- !Arguments "Number, 19, [ 0 | 9999 | 0 ], Sprite ID in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, 20, Color of sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Number, 15, [ -9999 | 9999 | 0 ], Draw priority\nRange [-9999 to 9999]. Negative values will draw sprite above strings."
-- !Arguments "Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], Blend mode"

LevelFuncs.Engine.Node.DisplaySprite = function(objectID, spriteID, color, posX, posY, rot, scaleX, scaleY, priority,
                                                alignMode, scaleMode, blendMode)
    local pos = TEN.Vec2(posX, posY)
    local scale = TEN.Vec2(scaleX, scaleY)
    local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
    local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)
    local sprite = TEN.DisplaySprite(objectID, spriteID, pos, rot, scale, color)
    sprite:Draw(priority, alignM, scaleM, blendID)
end

-- !Name "Draw display sprite at mouse position"
-- !Section "Sprites"
-- !Conditional "False"
-- !Description "Draw a display sprite under the mouse cursor."
-- !Arguments "Newline, SpriteSlots, 61, Sprite sequence object ID"
-- !Arguments "Number, 19, [ 0 | 9999 | 0 ], Sprite ID in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, 20, Color of sprite"
-- !Arguments "Newline, Number, 34, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Number, 33, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, 33, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Number, 15, [ -9999 | 9999 | 0 ], Draw priority\nRange [-9999 to 9999]. Negative values will draw sprite above strings."
-- !Arguments "Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], Blend mode"

LevelFuncs.Engine.Node.DisplaySpriteAtMousePosition = function(objectID, spriteID, color, rot, scaleX, scaleY, priority,
                                                alignMode, scaleMode, blendMode)
    local pos = TEN.Input.GetCursorDisplayPosition()
    local scale = TEN.Vec2(scaleX, scaleY)
    local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
    local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)
    local sprite = TEN.DisplaySprite(objectID, spriteID, pos, rot, scale, color)
    sprite:Draw(priority, alignM, scaleM, blendID)
end