-- !Name "If key is hit..."
-- !Section "Input"
-- !Description "Checks if specific game key has just been hit (will be true only for 1 frame, until the key is released and hit again)."
-- !Conditional "True"
-- !Arguments "Enumeration, 35, [ Forward | Back | Left | Right | Step Left | Step Right | Walk | Sprint | Crouch | Jump | Roll | Action | Draw | Look | Accelerate (Vehicle) | Reverse (Vehicle) | Faster (Vehicle) | Slower (Vehicle) | Brake (Vehicle) | Fire (Vehicle) | Flare | Small Medipack | Large Medipack | Previous Weapon | Next Weapon | Weapon 1 | Weapon 2 | Weapon 3 | Weapon 4 | Weapon 5 | Weapon 6 | Weapon 7 | Weapon 8 | Weapon 9 | Weapon 10 | Select | Deselect | Pause | Inventory | Save | Load ]"

LevelFuncs.Engine.Node.KeyIsHit = function(keyCode)
	return TEN.Input.KeyIsHit(keyCode)
end

-- !Name "If key is held..."
-- !Section "Input"
-- !Description "Checks if specific game key is being held (will be true as long as the player keeps their finger on that key)."
-- !Conditional "True"
-- !Arguments "Enumeration, 35, [ Forward | Back | Left | Right | Step Left | Step Right | Walk | Sprint | Crouch | Jump | Roll | Action | Draw | Look | Accelerate (Vehicle) | Reverse (Vehicle) | Faster (Vehicle) | Slower (Vehicle) | Brake (Vehicle) | Fire (Vehicle) | Flare | Small Medipack | Large Medipack | Previous Weapon | Next Weapon | Weapon 1 | Weapon 2 | Weapon 3 | Weapon 4 | Weapon 5 | Weapon 6 | Weapon 7 | Weapon 8 | Weapon 9 | Weapon 10 | Select | Deselect | Pause | Inventory | Save | Load ]"

LevelFuncs.Engine.Node.KeyIsHeld = function(keyCode)
	return TEN.Input.KeyIsHeld(keyCode)
end

-- !Name "Push a key"
-- !Section "Input"
-- !Description "Emulates push of a specified key. If called continuously, registers as a hold event, otherwise as a hit."
-- !Arguments "Enumeration, 35, [ Forward | Back | Left | Right | Step Left | Step Right | Walk | Sprint | Crouch | Jump | Roll | Action | Draw | Look | Accelerate (Vehicle) | Reverse (Vehicle) | Faster (Vehicle) | Slower (Vehicle) | Brake (Vehicle) | Fire (Vehicle) | Flare | Small Medipack | Large Medipack | Previous Weapon | Next Weapon | Weapon 1 | Weapon 2 | Weapon 3 | Weapon 4 | Weapon 5 | Weapon 6 | Weapon 7 | Weapon 8 | Weapon 9 | Weapon 10 | Select | Deselect | Pause | Inventory | Save | Load ]"

LevelFuncs.Engine.Node.PushKey = function(keyCode)
	return TEN.Input.KeyPush(keyCode)
end

-- !Name "Clear a key"
-- !Section "Input"
-- !Description "Blocks specified key from pushing."
-- !Arguments "Enumeration, 35, [ Forward | Back | Left | Right | Step Left | Step Right | Walk | Sprint | Crouch | Jump | Roll | Action | Draw | Look | Accelerate (Vehicle) | Reverse (Vehicle) | Faster (Vehicle) | Slower (Vehicle) | Brake (Vehicle) | Fire (Vehicle) | Flare | Small Medipack | Large Medipack | Previous Weapon | Next Weapon | Weapon 1 | Weapon 2 | Weapon 3 | Weapon 4 | Weapon 5 | Weapon 6 | Weapon 7 | Weapon 8 | Weapon 9 | Weapon 10 | Select | Deselect | Pause | Inventory | Save | Load ]"

LevelFuncs.Engine.Node.BlockKey = function(keyCode)
	return TEN.Input.KeyClear(keyCode)
end

-- !Name "Clear all controls"
-- !Section "Input"
-- !Description "Blocks all keys from pushing."

LevelFuncs.Engine.Node.BlockAllKeys = function()
	return TEN.Input.ClearAllKeys()
end

-- !Name "Vibrate game controller"
-- !Section "Input"
-- !Description "Vibrates game controller, if vibration feature is available and setting is on."
-- !Arguments "Numerical, 15, [ 0 | 1 | 2 | 0.25 | 0.5 ], Vibration strength"
-- !Arguments "Numerical, 15, [ 0 | 10 | 2 | 0.25 | 0.5 ], Vibration time in seconds"

LevelFuncs.Engine.Node.Vibrate = function(strength, time)
    TEN.Input.Vibrate(strength, time)
end