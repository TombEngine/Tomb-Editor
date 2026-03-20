local START_DELAY = 0.25
local BURN_DURATION = 1024

local SOUNDS =
{
    FIRE_LOOP = 150,
    SHATTER = 347 
}

local FIRE_OFFSETS =
{   --displacement, rotation, size
    {1219.784, 94.488, 2},
    {924.822, -37.256, 2},
    {1067.333, -203.896, 2},
    {143.108, -206.638, 2},
    {826.482, -85.558, 2},
    {576.056, -127.676, 1},
    {526.847, -48.063, 1},
    {1253.348, -60.981, 1},
    {593.997, -225.522, 1},
    {1263.924, -24.288, 1},
    {572.503, 25.722, 1},
    {811.951, 84.365, 1},
    {1252.326, -252.225, 0.5},
    {1250.485, -226.342, 0.5},
    {922.085, -188.477, 0.5},
    {635.232, -16.846, 0.5}
}

LevelVars.Engine.BurningFloor = {}
LevelVars.Engine.BurningFloor.Active = false
LevelVars.Engine.BurningFloor.Counter = BURN_DURATION
LevelVars.Engine.BurningFloor.ItemName = nil
LevelVars.Engine.BurningFloor.PlayerInVolume = false

-- !Name "Burning Floor"
-- !Section "Objects"
-- !Description "Create a Burning Floor Object."
-- !Arguments "NewLine, Moveables"
-- !Arguments "NewLine, Moveables"
LevelFuncs.Engine.Node.BurningFloor = function(moveableName, activator)

    local object = TEN.Objects.GetMoveableByName(activator)
    local burningFloor = TEN.Objects.GetMoveableByName(moveableName)

    LevelVars.Engine.BurningFloor.PlayerInVolume = false

    if object:GetObjectID() == TEN.Objects.ObjID.BURNING_TORCH_ITEM and object:GetItemFlags(3) == 1 and (math.abs(object:GetPosition().y - burningFloor:GetPosition().y) < 64) and burningFloor:GetStatus() ~= Objects.MoveableStatus.DEACTIVATED then
        LevelVars.Engine.BurningFloor.ItemName = moveableName
        LevelVars.Engine.BurningFloor.Active = true
        object:Destroy()
    end

    if object:GetObjectID() == TEN.Objects.ObjID.LARA and (math.abs(object:GetPosition().y - burningFloor:GetPosition().y) < 32) then
        LevelVars.Engine.BurningFloor.PlayerInVolume = true
    end

end

LevelFuncs.Engine.Node.RunBurningFloor = function()

    if not LevelVars.Engine.BurningFloor.Active then
        return
    end

    local burningFloor = TEN.Objects.GetMoveableByName(LevelVars.Engine.BurningFloor.ItemName)

    LevelVars.Engine.BurningFloor.Counter = math.max(0, LevelVars.Engine.BurningFloor.Counter - 1)
    local elapsed = BURN_DURATION - LevelVars.Engine.BurningFloor.Counter
    local normalizedTime = elapsed / BURN_DURATION
    local colorLife = 1 - normalizedTime
    local c = math.floor(colorLife * 128)
    burningFloor:SetColor(Color(c, c, c))

    local fireLife = 0
    if normalizedTime >= START_DELAY then
        local t = (normalizedTime - START_DELAY) / (1 - START_DELAY)
        fireLife = math.sin(t * math.pi)
    end
    
    local position = burningFloor:GetPosition()
    local yaw = burningFloor:GetRotation().y

    for _, entry in ipairs(FIRE_OFFSETS) do
        local distance = entry[1]
        local angleDeg = entry[2]
        local size = entry[3]

        local rot = Rotation(0, yaw + angleDeg, 0)
        local firePos = position:Translate(rot, distance)

        local fireStrength = fireLife * (0.8 + size * 0.6)
        if fireStrength > 0.02 then
            TEN.Effects.EmitFire(firePos, fireStrength)
        end
    end

    TEN.Sound.PlaySound(SOUNDS.FIRE_LOOP)

    if (math.abs(Lara:GetPosition().y - burningFloor:GetPosition().y) < 64) and normalizedTime >= START_DELAY and LevelVars.Engine.BurningFloor.PlayerInVolume then
        Lara:SetEffect(Effects.EffectID.FIRE)
        local ocb = burningFloor:GetOCB()
        TEN.Sound.PlaySound(SOUNDS.SHATTER)
        burningFloor:Shatter()
        TEN.Flow.FlipMap(ocb)
        Lara:SetHP(20)
        LevelVars.Engine.BurningFloor.Active = false
        return
    end

    if LevelVars.Engine.BurningFloor.Counter == 0 then
        local ocb = burningFloor:GetOCB()
        TEN.Sound.PlaySound(SOUNDS.SHATTER)
        burningFloor:Shatter()
        TEN.Flow.FlipMap(ocb)
        LevelVars.Engine.BurningFloor.Active = false
        LevelVars.Engine.BurningFloor.Counter = BURN_DURATION
        LevelVars.Engine.BurningFloor.ItemName = nil
    end

end

TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PRELOOP, LevelFuncs.Engine.Node.RunBurningFloor)
