-- !Name "Add item to inventory"
-- !Section "Inventory"
-- !Description "Add item to the inventory."
-- !Description "A count of 0 will add the default pickup amount of that item."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], Object to add to Lara's inventory."
-- !Arguments "20, Numerical, [ 0 | 1000 ], Amount of items to add (0 to give default amount)."
-- !Arguments "NewLine, Boolean , 100, Display the item in the pickup summary"

LevelFuncs.Engine.Node.AddInventoryItem = function(item, count, addToPickupSummary)
    TEN.Inventory.GiveItem(item, count, addToPickupSummary)
end

-- !Name "Remove item from inventory"
-- !Section "Inventory"
-- !Description "Removes specified item count from the inventory.\nA count of 0 will completely remove the item from the inventory."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], Object to add to Lara's inventory."
-- !Arguments "20, Numerical, [ 0 | 1000 ], Amount of items to remove (0 to remove all)."

LevelFuncs.Engine.Node.RemoveInventoryItem = function(item, count)
    
    local currentCount = TEN.Inventory.GetItemCount(item)

    if (count == 0 or currentCount < count) then
        TEN.Inventory.SetItemCount(item, 0)
    else
        TEN.Inventory.SetItemCount(item, currentCount - count)
    end
end

-- !Name "Set last used item"
-- !Section "Inventory"
-- !Description "Sets last used inventory item. If item will not be handled by the engine, 'No' sound will play."
-- !Arguments "NewLine, 58, WadSlots, [ _ITEM ], Item to check"

LevelFuncs.Engine.Node.SetUsedItem = function(item)
    TEN.Inventory.SetUsedItem(item)
end

-- !Name "Clear last used item"
-- !Section "Inventory"
-- !Description "Clears last used inventory item. Can be needed to avoid playing of 'No' sound."

LevelFuncs.Engine.Node.ClearUsedItem = function()
    TEN.Inventory.ClearUsedItem()
end

-- !Name "If item is present in inventory..."
-- !Section "Inventory"
-- !Description "Checks if specified inventory item is present."
-- !Conditional "True"
-- !Arguments "NewLine, WadSlots, [ _ITEM ], Item to check"

LevelFuncs.Engine.Node.TestInventoryItem = function(item)
    return (TEN.Inventory.GetItemCount(item) > 0)
end

-- !Name "If inventory item count is..."
-- !Section "Inventory"
-- !Description "Checks specified inventory item count."
-- !Conditional "True"
-- !Arguments "NewLine, 58, WadSlots, [ _ITEM ], Item to check"
-- !Arguments "CompareOperator, 29, Kind of check"
-- !Arguments "Numerical, 13, Inventory item count, [ 0 | 1000 ]"

LevelFuncs.Engine.Node.TestInventoryItemCount = function(item, operator, count)
    return LevelFuncs.Engine.Node.CompareValue(TEN.Inventory.GetItemCount(item), count, operator)
end

-- !Name "If last used item is..."
-- !Section "Inventory"
-- !Description "Checks last used inventory item. If condition is met and item is recognized, it will be automatically cleared."
-- !Conditional "True"
-- !Arguments "NewLine, WadSlots, [ _ITEM ], Item to check"

LevelFuncs.Engine.Node.TesttUsedItem = function(item)
    local itemWasUsed = (TEN.Inventory.GetUsedItem() == item)

	if itemWasUsed then
		TEN.Inventory.ClearUsedItem(item)
	end

	return itemWasUsed
end