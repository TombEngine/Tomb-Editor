using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.ClassicScript.Syntaxes;

namespace TombLib.Tests;

// Phase 6 migration tests: prove that the unified Commands.json catalog preserves the
// exact command names, kinds, sections, and syntax values that used to live in
// CommandCatalog.json and the four Syntaxes/*.json files.
[TestClass]
public class ClassicScriptCommandsMigrationTests
{
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();
	private readonly ClassicScriptSyntaxCatalogService _syntaxCatalogService = new();

	private static readonly string[] ExpectedSections =
	[
		"Language", "Level", "Options", "PCExtensions", "PSXExtensions", "Title",
		"Strings", "PSXStrings", "PCStrings", "ExtraNG"
	];

	private static readonly string[] ExpectedNewCommands =
	[
		"#DEFINE", "#FIRST_ID", "#INCLUDE", "AddEffect", "Animation", "AnimationSlot",
		"AssignSlot", "ColorRGB", "CombineItems", "CRS", "Customize", "CutScene",
		"Damage", "DefaultWindowsFont", "Demo", "Detector", "Diagnostic",
		"DiagnosticType", "Diary", "Elevator", "Enemy", "Equipment", "FMV", "FogRange",
		"ForceBumpMapping", "ForceVolumetricFX", "GlobalTrigger", "Image", "ImportFile",
		"ItemGroup", "KeyPad", "LaraStartPos", "LevelFarView", "LogItem", "MirrorEffect",
		"MultEnvCondition", "NewSoundEngine", "Organizer", "Parameters", "Plugin",
		"PreserveInventory", "Rain", "SavegamePanel", "Settings", "ShowLaraInTitle",
		"Snow", "SoundSettings", "StandBy", "StaticMIP", "Switch", "TestPosition",
		"TextFormat", "TextureSequence", "TriggerGroup", "Turbo", "WindowsFont",
		"WindowTitle", "WorldFarView"
	];

	private static readonly string[] ExpectedOldCommands =
	[
		"AnimatingMIP", "ColAddHorizon", "DemoDisc", "Examine", "FlyCheat", "Fog",
		"Horizon", "InputTimeout", "Key", "KeyCombo", "Layer1", "Legend", "LensFlare",
		"Level", "Lightning", "LoadCamera", "LoadSave", "Mirror", "Name", "Pickup",
		"PickupCombo", "PlayAnyLevel", "Puzzle", "PuzzleCombo", "RemoveAmulet",
		"ResetHUB", "ResidentCut", "Timer", "Title", "Train", "UVRotate", "YoungLara",
		"Cut", "File", "FMV", "Layer2", "NoLevel", "Pulse", "Security", "StarField",
		"Unknown"
	];

	[TestMethod]
	public void Sections_PreserveTheTenLegacySectionsInOrder()
	{
		CollectionAssert.AreEqual(ExpectedSections, _commandCatalogService.Sections.ToArray());
	}

	[TestMethod]
	public void NewCommands_PreserveAllFiftyEightNamesInOrder()
	{
		CollectionAssert.AreEqual(ExpectedNewCommands, _commandCatalogService.NewCommands.ToArray());
	}

	[TestMethod]
	public void OldCommands_PreserveAllFortyOneNamesInOrder()
	{
		CollectionAssert.AreEqual(ExpectedOldCommands, _commandCatalogService.OldCommands.ToArray());
	}

	[TestMethod]
	public void CommandKinds_AreUnchangedForEveryKnownCommand()
	{
		foreach (string name in ExpectedNewCommands)
			Assert.IsFalse(_commandCatalogService.IsOldCommand(name), $"'{name}' must not be old");

		// FMV is listed in both kinds and is treated as new, so it is not old.
		foreach (string name in ExpectedOldCommands.Where(name => !name.Equals("FMV", StringComparison.OrdinalIgnoreCase)))
			Assert.IsTrue(_commandCatalogService.IsOldCommand(name), $"'{name}' must be old");
	}

	[TestMethod]
	public void CommandSyntaxDefinitions_PreserveAllNinetyNineEntriesOldBeforeNew()
	{
		IReadOnlyList<ClassicScriptSyntaxDefinition> definitions = _syntaxCatalogService.GetCommandSyntaxDefinitions();

		Assert.AreEqual(99, definitions.Count);

		// The first 41 definitions are the old command syntaxes (in canonical command-list
		// order, ending with the legacy array-only "Security"), the rest are new ones.
		Assert.AreEqual("AnimatingMIP", definitions[0].Key);
		Assert.AreEqual("Security", definitions[40].Key);
		Assert.AreEqual("#DEFINE", definitions[41].Key);
		Assert.AreEqual("WorldFarView", definitions[^1].Key);
	}

	[TestMethod]
	public void CommandSyntaxValues_AreUnchangedForRepresentativeCommands()
	{
		Assert.AreEqual(
			"[Level] AddEffect= {ID}, {Effect Type (ADD_...)}, {Effect Flag (FADD_...)}, {Joint Type (JOINT_...)}, {ORIGIN_X_DISTANCE}, {ORIGIN_Y_DISTANCE}, {ORIGIN_Z_DISTANCE}, {EMIT_DURATION}, {PAUSE_DURATION}, {Extra Params (*Array*)}",
			_syntaxCatalogService.GetCommandSyntax("AddEffect"));

		Assert.AreEqual(
			"[Level] Legend= {MESSAGE_STRING}",
			_syntaxCatalogService.GetCommandSyntax("Legend"));

		Assert.AreEqual(
			"[Level] Customize= {TYPE (CUST_...)}, {Arguments (*Array*)}",
			_syntaxCatalogService.GetCommandSyntax("Customize"));

		Assert.AreEqual(
			"[Level] Parameters= {TYPE (PARAM_...)}, {PARAM_LIST_ID}, {Parameters (*Array*)}",
			_syntaxCatalogService.GetCommandSyntax("Parameters"));
	}

	[TestMethod]
	public void CommandSyntaxMetadata_IsDerivedIdentically()
	{
		ClassicScriptSyntaxDefinition customize = _syntaxCatalogService.GetCommandDefinition("Customize")
			?? throw new AssertFailedException("Customize syntax definition was not loaded.");

		Assert.AreEqual("Level", customize.ApplicableSection);
		Assert.AreEqual(2, customize.ArgumentCount);
		Assert.IsTrue(customize.HasArrayArguments);

		ClassicScriptSyntaxDefinition legend = _syntaxCatalogService.GetCommandDefinition("Legend")
			?? throw new AssertFailedException("Legend syntax definition was not loaded.");

		Assert.AreEqual("Level", legend.ApplicableSection);
		Assert.AreEqual(1, legend.ArgumentCount);
		Assert.IsFalse(legend.HasArrayArguments);
	}

	[TestMethod]
	public void SectionVariationSyntaxKeys_ArePreserved()
	{
		// Level / Cut / FMV resolve to section-context syntax variants.
		Assert.AreEqual("[Level] Level= {LEVEL_PATH}, {SOUND_ID}", _syntaxCatalogService.GetCommandSyntax("LevelLevel"));
		Assert.AreEqual("[PCExtensions] Level= .TR4 ; Default value", _syntaxCatalogService.GetCommandSyntax("LevelPC"));
		Assert.AreEqual("[PSXExtensions] Level= .PSX ; Default value", _syntaxCatalogService.GetCommandSyntax("LevelPSX"));

		Assert.AreEqual("[PCExtensions] Cut= .TR4 ; Default value", _syntaxCatalogService.GetCommandSyntax("CutPC"));
		Assert.AreEqual("[PSXExtensions] Cut= .CUT ; Default value", _syntaxCatalogService.GetCommandSyntax("CutPSX"));

		Assert.AreEqual("[PCExtensions] FMV= .BIK ; Default value", _syntaxCatalogService.GetCommandSyntax("FMVPC"));
		Assert.AreEqual("[PSXExtensions] FMV= .FMV ; Default value", _syntaxCatalogService.GetCommandSyntax("FMVPSX"));
		Assert.AreEqual("[Level] FMV= NumberFmv, EnableEscape", _syntaxCatalogService.GetCommandSyntax("FMVLevel"));
	}

	[TestMethod]
	public void CustomizeAndParameterSyntaxes_PreserveAllEntries()
	{
		Assert.AreEqual(
			"[Level] Customize= CUST_BAR, {Bar type (BAR_...)}, {Flags (FBAR_...)}, {X origin}, {Y origin}, {X size}, {Y size}, {Color1 ID}, {Color2 ID}, {Extra}",
			_syntaxCatalogService.GetCustomizeSyntax("CUST_BAR"));

		Assert.AreEqual(
			"[Level] Parameters= PARAM_RECT, RectId, XOrigin, YOrigin, Width, Height, ForeColor, BackColor",
			_syntaxCatalogService.GetParameterSyntax("PARAM_RECT"));

		// Spot-check the full sets resolve (54 customize, 17 parameter entries).
		string[] customizeKeys =
		[
			"CUST_ADD_DEATH_ANIMATION", "CUST_AMMO", "CUST_BACKGROUND", "CUST_BAR", "CUST_BIKE_VS_ENEMIES",
			"CUST_BINOCULARS", "CUST_CAMERA", "CUST_CD_SINGLE_PLAYBACK", "CUST_DARTS", "CUST_DISABLE_FORCING_ANIM_96",
			"CUST_DISABLE_MISSING_SOUNDS", "CUST_DISABLE_PUSH_AWAY_ANIMATION", "CUST_DISABLE_SCREAMING_HEAD",
			"CUST_ESCAPE_FLY_CAMERA", "CUST_FIX_BUGS", "CUST_FIX_WATER_FOG_BUG", "CUST_FLARE", "CUST_FMV_CUTSCENE",
			"CUST_HAIR_TYPE", "CUST_HARPOON", "CUST_INNER_SCREENSHOT", "CUST_KEEP_DEAD_ENEMIES", "CUST_KEEP_LARA_HP",
			"CUST_LIGHT_OBJECT", "CUST_LOOK_TRASPARENT", "CUST_NEW_SOUND_ENGINE", "CUST_NO_TIME_IN_SAVELIST",
			"CUST_PARALLEL_BARS", "CUST_PAUSE_FLY_CAMERA", "CUST_RAIN", "CUST_ROLLINGBALL_PUSHING",
			"CUST_ROLLING_BOAT", "CUST_SAVE_LOCUST", "CUST_SCREENSHOT_CAPTURE", "CUST_SET_CREDITS_LEVEL",
			"CUST_SET_INV_ITEM", "CUST_SET_JEEP_KEY_SLOT", "CUST_SET_OLD_CD_TRIGGER", "CUST_SET_SECRET_NUMBER",
			"CUST_SET_STATIC_DAMAGE", "CUST_SET_STILL_COLLISION", "CUST_SET_TEXT_COLOR", "CUST_SFX",
			"CUST_SHATTER_RANGE", "CUST_SHATTER_SPECIFIC", "CUST_SHOW_AMMO_COUNTER", "CUST_SLOT_FLAGS",
			"CUST_SPEED_MOVING", "CUST_STATIC_TRANSPARENCY", "CUST_TEXT_ON_FLY_SCREEN", "CUST_TITLE_FMV",
			"CUST_TR5_UNDERWATER_COLLISIONS", "CUST_WATERFALL_SPEED", "CUST_WEAPON"
		];

		foreach (string key in customizeKeys)
			Assert.IsFalse(string.IsNullOrWhiteSpace(_syntaxCatalogService.GetCustomizeSyntax(key)), $"'{key}' must resolve");

		string[] parameterKeys =
		[
			"PARAM_ACTOR_SPEECH", "PARAM_BIG_NUMBERS", "PARAM_CIRCLE", "PARAM_COLOR_ITEM", "PARAM_INPUT_BOX",
			"PARAM_LIGHTNING", "PARAM_MOVE_ITEM", "PARAM_PRINT_TEXT", "PARAM_QUADRILATERAL", "PARAM_RECT",
			"PARAM_ROTATE_ITEM", "PARAM_SCALE_ITEM", "PARAM_SET_CAMERA", "PARAM_SHOW_SPRITE",
			"PARAM_SWAP_ANIMATIONS", "PARAM_TRIANGLE", "PARAM_WTEXT"
		];

		foreach (string key in parameterKeys)
			Assert.IsFalse(string.IsNullOrWhiteSpace(_syntaxCatalogService.GetParameterSyntax(key)), $"'{key}' must resolve");
	}
}
