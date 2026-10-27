-- Lookup of Lara's traversal states, grouped by traversal mode (the enumeration exposed by the node
-- below). Built once at module scope so the node performs an O(1) lookup instead of rebuilding tables
-- and doing a linear scan on every call.
local TRAVERSAL_STATES =
{
	[0] = -- CLIMB
	{
		[10] = true,  -- hang state
		[19] = true,  -- grabbing (pulling up)
		[55] = true,  -- climbing out of water
		[56] = true,  -- idle on ladder
		[57] = true,  -- ladder up
		[58] = true,  -- ladder left
		[59] = true,  -- ladder stop
		[60] = true,  -- ladder right
		[61] = true,  -- ladder down
		[88] = true,  -- crawl to hang
		[107] = true, -- shimmy outer left
		[108] = true, -- shimmy outer right
		[109] = true, -- shimmy inner left
		[110] = true, -- shimmy inner right
		[138] = true, -- ladder to crouch
	},

	[1] = -- CRAWL
	{
		[71] = true,  -- crouch idle
		[72] = true,  -- crouch roll
		[80] = true,  -- crawling idle
		[81] = true,  -- crawl forward
		[84] = true,  -- crawling turn left
		[85] = true,  -- crawling turn right
		[86] = true,  -- crawling backwards
		[105] = true, -- crouch turn left
		[106] = true, -- crouch turn right
		[161] = true, -- crawl step up
		[162] = true, -- crawl step down
		[167] = true, -- 1 step crouch vault
		[168] = true, -- 2 step crouch vault
		[169] = true, -- 3 step crouch vault
		[171] = true, -- crouch turn 180
		[172] = true, -- crawl turn 180
	},

	[2] = -- HORIZONTAL_BAR
	{
		[128] = true, -- horizontal bar swing
		[129] = true, -- horizontal bar leap
	},

	[3] = -- MONKEY_SWING
	{
		[75] = true,  -- monkey swing idle
		[76] = true,  -- monkey swing forward
		[77] = true,  -- monkey swing shimmy left
		[78] = true,  -- monkey swing shimmy right
		[79] = true,  -- monkey swing turn 180
		[82] = true,  -- monkey turn left
		[83] = true,  -- monkey turn right
	},

	[4] = -- POLE_VAULT
	{
		[99] = true,  -- pole idle
		[100] = true, -- pole up
		[101] = true, -- pole down
		[102] = true, -- pole turn clockwise
		[103] = true, -- pole turn counterclockwise
	},

	[5] = -- ROPE_SWING
	{
		[90] = true,  -- rope turn clockwise
		[91] = true,  -- rope turn counterclockwise
		[111] = true, -- rope idle
		[112] = true, -- rope up
		[113] = true, -- rope down
		[114] = true, -- rope swing
		[115] = true, -- rope unknown
	},

	[6] = -- SWIM
	{
		[13] = true,  -- swimming idle
		[17] = true,  -- swim forward
		[18] = true,  -- swim inertia
		[35] = true,  -- dive
		[40] = true,  -- use switch (shared with dry land, see water room check below)
		[42] = true,  -- use key (shared with dry land)
		[43] = true,  -- use puzzle (shared with dry land)
		[44] = true,  -- underwater death
		[66] = true,  -- underwater roll
		[67] = true,  -- pickup flare
		[89] = true,  -- misc control (opening door, trapdoor, kick; shared with dry land)
		[93] = true,  -- trapdoor floor open (shared with dry land)
	},

	[7] = -- TIGHTROPE
	{
		[119] = true, -- Tightrope idle
		[120] = true, -- Tightrope turn 180
		[121] = true, -- Tightrope walk
		[122] = true, -- Tightrope unbalance left
		[123] = true, -- Tightrope unbalance right
		[124] = true, -- Tightrope enter
		[125] = true, -- Tightrope dismount
	},
}

-- Interaction states in the swim group (switches, keys, puzzles) are shared with their
-- dry-land counterparts, so the swim check additionally requires a water room.
local SWIM_MODE = 6

-- !Name "If Lara traversal state is..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks Lara's current traversal state."
-- !Arguments "Enumeration, [ Climb | Crawl | Horizontal Bar | Monkey Swing | Pole Vault | Rope Swing | Swim | Tightrope ], 30, Traversal state to test."

LevelFuncs.Engine.Node.TestLaraTraversalState = function(mode)
	local states = TRAVERSAL_STATES[mode]

	if states == nil or states[TEN.Objects.Lara:GetState()] ~= true then
		return false
	end

	if mode == SWIM_MODE then
		return TEN.Objects.Lara:GetRoom():GetFlag(TEN.Objects.RoomFlagID.WATER)
	end

	return true
end
