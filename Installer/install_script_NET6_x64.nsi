!include LogicLib.nsh
!include MUI2.nsh
!include WinVer.nsh
!include x64.nsh

!cd "..\BuildRelease (x64)\net6.0-windows"

!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_ABORTWARNING
!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_WELCOMEFINISHPAGE_BITMAP "..\..\TombEditor\Resources\misc\misc_InstallerSplashTEN.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "..\..\TombEditor\Resources\misc\misc_InstallerSplashTEN.bmp" 
!define MUI_ICON "..\..\Icons\ICO\TE.ico"
!define MUI_FINISHPAGE_SHOWREADME "Changes.txt"

!define DOT_MAJOR "6"
!define DOT_MINOR "0"

!define MUI_WELCOMEPAGE_TEXT \
"You are ready to install Tomb Editor ${Version_1}.${Version_2}.${Version_3}. $\r$\n\
$\r$\n\
Please make sure your system complies with following system requirements: $\r$\n\
$\r$\n\
  ${U+2022} Windows 7 or later (64-bit) $\r$\n\
  ${U+2022} Installed .NET 6 or later (64-bit)$\r$\n\
  ${U+2022} Videocard with DirectX 10 support $\r$\n\
  ${U+2022} At least 2 gigabytes of RAM $\r$\n\
$\r$\n\
This package includes a TIDE template to build Tomb Engine (TEN) levels. $\r$\n\
$\r$\n\
Enjoy! $\r$\n\
Tomb Editor dev team."

!getdllversion "TombEditor.exe" Version_

;--------------------------------

SetCompressor lzma
Unicode true
Name "Tomb Editor"
OutFile "TombEditor${Version_1}${Version_2}${Version_3}_Install.exe"
InstallDir "C:\Tomb Editor"
  
;--------------------------------

InstType "Standard"
InstType "Basic components only"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

;--------------------------------

Section "Tomb Editor" Section1

  SectionIn RO ; Always install this section
  
  ; Delete TEN node catalogs to avoid renaming clashes
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\*.*"
  
  SetOutPath $INSTDIR
  File /r \
  /x "TombEditorLog*.txt" \
  /x "WadToolLog*.txt" \
  /x "TombIDELog*.txt" \
  /x "*.prj2" \
  /x "*.pdb" \
  /x "*.so" \
  /x "*.vshost.*" \
  /x "install_script.nsi" \
  /x "TombEditorInstall.exe" \
  /x "TombEditorConfiguration.xml" \
  /x "SoundToolConfiguration.xml" \
  /x "WadToolConfiguration.xml" \
  *.* \
  
  ; Add readme from installer folder
  File "..\..\Installer\Changes.txt"
  
  ; Add resources folder if not
  File /r "Resources"
  
  ; Choose 32-bit or 64-bit d3dcompiler dll based on system version
  Rename "$INSTDIR\Native\64 bit\d3dcompiler_43.dll" "$INSTDIR\d3dcompiler_43.dll"
  
  ; Write the uninstall keys
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "DisplayName" "Tomb Editor"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "NoRepair" 1
  
  ; Write uninstaller itself
  WriteUninstaller "uninstall.exe"
  
SectionEnd

Section "Start Menu Shortcuts" Section2

  SectionIn 1 2

  CreateDirectory "$SMPROGRAMS\Tomb Editor"
  CreateShortcut "$SMPROGRAMS\Tomb Editor\Tomb Editor.lnk" "$INSTDIR\TombEditor.exe" "" "$INSTDIR\TombEditor.exe" 0
  CreateShortcut "$SMPROGRAMS\Tomb Editor\SoundTool.lnk" "$INSTDIR\SoundTool.exe" "" "$INSTDIR\SoundTool.exe" 0
  CreateShortcut "$SMPROGRAMS\Tomb Editor\WadTool.lnk" "$INSTDIR\WadTool.exe" "" "$INSTDIR\WadTool.exe" 0

  ${If} ${SectionIsSelected} ${Section2}
	CreateShortcut "$SMPROGRAMS\Tomb Editor\TombIDE.lnk" "$INSTDIR\TombIDE.exe" "" "$INSTDIR\TombIDE.exe" 0
  ${EndIf}
  
  CreateShortcut "$SMPROGRAMS\Tomb Editor\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  
SectionEnd

Section "Desktop Shortcuts" Section3

  SectionIn 1 2
  CreateShortcut "$DESKTOP\Tomb Editor.lnk" "$INSTDIR\TombEditor.exe" "" "$INSTDIR\TombEditor.exe" 0
  CreateShortcut "$DESKTOP\SoundTool.lnk" "$INSTDIR\SoundTool.exe" "" "$INSTDIR\SoundTool.exe" 0
  CreateShortcut "$DESKTOP\WadTool.lnk" "$INSTDIR\WadTool.exe" "" "$INSTDIR\WadTool.exe" 0
  
  ${If} ${SectionIsSelected} ${Section2}
    CreateShortcut "$DESKTOP\TombIDE.lnk" "$INSTDIR\TombIDE.exe" "" "$INSTDIR\TombIDE.exe" 0
  ${EndIf}

SectionEnd

Section "Associate File Types" Section4
  
  SectionIn 1 2
  
  Call .registerExtensions

SectionEnd

LangString DESC_Section1 ${LANG_ENGLISH} "Basic Tomb Editor components. Includes WadTool and SoundTool."
LangString DESC_Section2 ${LANG_ENGLISH} "Shortcuts for Tomb Editor applications in Start Menu."
LangString DESC_Section3 ${LANG_ENGLISH} "Shortcuts for Tomb Editor applications on Desktop."
LangString DESC_Section4 ${LANG_ENGLISH} "Associate file types with Tomb Editor, WadTool and TombIDE."

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${Section1} $(DESC_Section1)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section2} $(DESC_Section2)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section3} $(DESC_Section3)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section4} $(DESC_Section4)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------

; Uninstaller

Section "Uninstall"
  
  Call un.registerExtensions
  
  ; Autogenerated by Unlist script from barebones BuildRelease folder
  ; https://nsis.sourceforge.io/mediawiki/images/9/9f/Unlist.zip
  ; PLEASE UPDATE THIS BLOCK IF BUILD FILE SET IS CHANGED!
    
  Delete "$INSTDIR\TIDE\TEN\API.xml"
  Delete "$INSTDIR\TIDE\TEN\.luarc.json"
  Delete "$INSTDIR\TIDE\Templates\Sounds\TR4-TRNG.zip"
  Delete "$INSTDIR\TIDE\Templates\Sounds\TR1.zip"
  Delete "$INSTDIR\TIDE\Templates\Sounds\TEN.zip"
  Delete "$INSTDIR\TIDE\Templates\Shared\TR4-TRNG Shared Files.zip"
  Delete "$INSTDIR\TIDE\Templates\Shared\TR4-TEN Shared Audio.zip"
  Delete "$INSTDIR\TIDE\Templates\Shared\TEN Resources.zip"
  Delete "$INSTDIR\TIDE\Templates\Shared\TEN External DLLs.zip"
  Delete "$INSTDIR\TIDE\Templates\Shared\splash.xml"
  Delete "$INSTDIR\TIDE\Templates\Shared\PLAY.exe"
  Delete "$INSTDIR\TIDE\Templates\Presets\TRNG.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TR4.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TR3.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TR2X.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TR2.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TR1.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TEN.zip"
  Delete "$INSTDIR\TIDE\Templates\Extras\FLEP.zip"
  Delete "$INSTDIR\TIDE\Templates\Defaults\TR4 Resources\uklogo.pak"
  Delete "$INSTDIR\TIDE\Templates\Defaults\TR4 Resources\load.bmp"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TRNG.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TR4.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TR3.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TR2X.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TR2.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TR1.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TombEngine.ico"
  Delete "$INSTDIR\TIDE\NGC\VGE\tools\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\sound\samples\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\sound\LevelSFX Creator\sounds.txt"
  Delete "$INSTDIR\TIDE\NGC\VGE\Script\SCRIPT.TXT"
  Delete "$INSTDIR\TIDE\NGC\VGE\Script\ENGLISH.TXT"
  Delete "$INSTDIR\TIDE\NGC\VGE\pix\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\graphics\wads\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\data\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\audio\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\tomb4.exe"
  Delete "$INSTDIR\TIDE\NGC\VGE\Tomb_NextGeneration.dll"
  Delete "$INSTDIR\TIDE\NGC\VGE\SCRIPT.DAT"
  Delete "$INSTDIR\TIDE\NGC\VGE\Objects.h"
  Delete "$INSTDIR\TIDE\NGC\VGE\ngle.exe"
  Delete "$INSTDIR\TIDE\NGC\VGE\NG_Tom2Pc.exe"
  Delete "$INSTDIR\TIDE\NGC\VGE\ENGLISH.DAT"
  Delete "$INSTDIR\TIDE\NGC\TRNG\Tomb4.exe"
  Delete "$INSTDIR\TIDE\NGC\TRNG\Tomb_NextGeneration.dll"
  Delete "$INSTDIR\TIDE\NGC\TRNG\Objects.h"
  Delete "$INSTDIR\TIDE\NGC\TRNG\ngle.exe"
  Delete "$INSTDIR\TIDE\NGC\TRNG\ng_Tom2Pc.exe"
  Delete "$INSTDIR\TIDE\NGC\NG_Center.exe"
  Delete "$INSTDIR\TIDE\NGC\help_old_script_command.txt"
  Delete "$INSTDIR\TIDE\NGC\help_new_script_command.txt"
  Delete "$INSTDIR\TIDE\NGC\center_settings.bin"
  Delete "$INSTDIR\TIDE\NGC\~~!DO_NOT_USE_THIS_NG_CENTER.txt"
  Delete "$INSTDIR\TIDE\NGC\~!~DO_NOT_USE_THIS_NG_CENTER.txt"
  Delete "$INSTDIR\TIDE\NGC\!~~DO_NOT_USE_THIS_NG_CENTER.txt"
  Delete "$INSTDIR\TIDE\GFL\gameflow.exe"
  Delete "$INSTDIR\TIDE\GF3\TRGameflow.exe"
  Delete "$INSTDIR\TIDE\DOS\TR4\SCRIPT.EXE"
  Delete "$INSTDIR\TIDE\DOS\TR4\DOS4GW.EXE"
  Delete "$INSTDIR\TIDE\DOS\SDL_net.dll"
  Delete "$INSTDIR\TIDE\DOS\SDL.dll"
  Delete "$INSTDIR\TIDE\DOS\DOSBox.exe"
  Delete "$INSTDIR\Runtimes\win-x86\native\FreeImage.dll"
  Delete "$INSTDIR\Runtimes\win-x86\native\blake3_dotnet.dll"
  Delete "$INSTDIR\Runtimes\win-x86\native\assimp.dll"
  Delete "$INSTDIR\Runtimes\win-x64\native\FreeImage.dll"
  Delete "$INSTDIR\Runtimes\win-x64\native\blake3_dotnet.dll"
  Delete "$INSTDIR\Runtimes\win-x64\native\assimp.dll"
  Delete "$INSTDIR\Resources\TRX\tr2.gameflow.schema.json"
  Delete "$INSTDIR\Resources\TRX\tr1.gameflow.schema.json"
  Delete "$INSTDIR\Resources\Localization\PL\TombIDE.xml"
  Delete "$INSTDIR\Resources\Localization\EN\WadTool.json"
  Delete "$INSTDIR\Resources\Localization\EN\TombLib.json"
  Delete "$INSTDIR\Resources\Localization\EN\TombIDE.xml"
  Delete "$INSTDIR\Resources\Localization\EN\TombIDE.json"
  Delete "$INSTDIR\Resources\Localization\EN\TombEditor.json"
  Delete "$INSTDIR\Resources\Localization\EN\Common.json"
  Delete "$INSTDIR\Resources\GameFlow\TRGameflow.pdf"
  Delete "$INSTDIR\Resources\GameFlow\TRGameflow extra commands.pdf"
  Delete "$INSTDIR\Resources\ClassicScript\Descriptions\OLD Commands.rdda"
  Delete "$INSTDIR\Resources\ClassicScript\Descriptions\OCBs.rdda"
  Delete "$INSTDIR\Resources\ClassicScript\Descriptions\NEW Commands.rdda"
  Delete "$INSTDIR\Resources\ClassicScript\Descriptions\Mnemonic Constants.rdda"
  Delete "$INSTDIR\Resources\ClassicScript\VariablePlaceholders.xml"
  Delete "$INSTDIR\Resources\ClassicScript\TRNG Script Reference Manual.pdf"
  Delete "$INSTDIR\Resources\ClassicScript\StaticObjectIndices.xml"
  Delete "$INSTDIR\Resources\ClassicScript\SoundIndices.xml"
  Delete "$INSTDIR\Resources\ClassicScript\OldCommandsList.xml"
  Delete "$INSTDIR\Resources\ClassicScript\OCBList.xml"
  Delete "$INSTDIR\Resources\ClassicScript\NewCommandsList.xml"
  Delete "$INSTDIR\Resources\ClassicScript\MoveableSlotIndices.xml"
  Delete "$INSTDIR\Resources\ClassicScript\MnemonicConstants.xml"
  Delete "$INSTDIR\Resources\ClassicScript\KeyboardScancodes.xml"
  Delete "$INSTDIR\Resources\ClassicScript\EnemyDamageValues.xml"
  Delete "$INSTDIR\Resources\wt_file.ico"
  Delete "$INSTDIR\Resources\tide_file.ico"
  Delete "$INSTDIR\Resources\te_file.ico"
  Delete "$INSTDIR\Rendering\Legacy\Solid.fx"
  Delete "$INSTDIR\Rendering\Legacy\RoomGeometry.fx"
  Delete "$INSTDIR\Rendering\Legacy\Model.fx"
  Delete "$INSTDIR\Rendering\DirectX11\TextShaderVS.cso"
  Delete "$INSTDIR\Rendering\DirectX11\TextShaderPS.cso"
  Delete "$INSTDIR\Rendering\DirectX11\SpriteShaderVS.cso"
  Delete "$INSTDIR\Rendering\DirectX11\SpriteShaderPS.cso"
  Delete "$INSTDIR\Rendering\DirectX11\RoomShaderVS.cso"
  Delete "$INSTDIR\Rendering\DirectX11\RoomShaderPS.cso"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Tomb1Main\VS15.t1msch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Tomb1Main\Obsidian.t1msch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Tomb1Main\NG_Center.t1msch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Tomb1Main\Monokai.t1msch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua\Default.xml"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\GameFlowScript\VS15.gflsch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\GameFlowScript\Obsidian.gflsch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\GameFlowScript\NG_Center.gflsch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\GameFlowScript\Monokai.gflsch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\VS15.cssch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\Obsidian.cssch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\NG_Center.cssch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\Monokai.cssch"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TR5_Override.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TR2_Override.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TR1_Override.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_TR5.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_TR4.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_TR3.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_TR2.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_TR1.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_ALL_SOUNDS.xml"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Volumes.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\View.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Video.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Variables.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\UI.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Timespan Actions.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Timers.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Text.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Statics.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Sprites.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Sound.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Rooms.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Revolution.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Particles.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Moveables.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Lara.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Keypad.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Inventory.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Input.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Flyby Paths.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Flow.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Environment.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Dynamic Lights.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Diary.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Creatures.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Batch Actions.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\_System.lua"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\VAR_TEXT.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\VAR_STORES.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\VAR_NORMALS.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\VAR_LONG_STORE.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\TRANSPARENCY32.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\TIMER_SIGNED_LONG.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\TIMER_SIGNED.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\TIME_LIST_32.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\TIME_LIST_128.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\TEX_SEQUENCE.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\SWAP_MESH_SLOT.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\STATE_ID_LIST.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\SLOT_MESH_MOVEABLES.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\SET_STANDARD_MESH.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\SEQUENCE_32.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\SEQUENCE_128.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\RECHARGE_256.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\PERCENTAGE.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\NG_Constants.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\NEGATIVE_NUMBERS.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\MICRO_CLICKS.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\MEMORY_SLOT.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\MEMORY_SAVE.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\MEMORY_ITEM.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\MEMORY_INVENTORY.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\MEMORY_CODE.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\MEMORY_ANIMATION.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\MEM_INVENTORY_INDICES.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\LARA_OTHER_SLOTS.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\LARA_ANIM_SLOT.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\KEYBOARD_MODE.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\INVENTORY-ITEMS.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\INVENTORY-ITEM-INDEX.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\HALF_CLICKS_32.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\HALF_CLICKS.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\FRAG_4x4.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\FRAG_3x3.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\FRAG_2x2.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\FOG_DISTANCES.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\FMV_LIST.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\DEGREES.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\COLORS.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\COLLISION_FLOOR.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\COLLISION_CEILING.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\CLICK_DISTANCE_32.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\CD_TRACK_LIST.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\CAMERA_EFFECTS.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\BUTTONS_LIST.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\BIT_LIST.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\BACKUP_LIST.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\ANIMATION_RANGE.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\ANIMATION_LIST_B.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\ANIMATION_LIST_32C.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\ANIMATION_LIST_32B.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\ANIMATION_LIST_32A.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\ANIMATION_LIST_255.txt"
  Delete "$INSTDIR\Catalogs\Engines\TRNG\ADD_EFFECT_255.txt"
  Delete "$INSTDIR\Catalogs\Engines\TR5\States.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR5\SpriteSequences.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR5\Sounds.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR5\Moveables.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR5\Limits.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR5\Animations.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR4\Statics.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR4\States.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR4\SpriteSequences.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR4\Sounds.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR4\Moveables.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR4\Limits.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR4\Animations.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR3\States.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR3\Sounds.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR3\Moveables.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR3\Limits.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR3\Animations.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR2\States.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR2\SpriteSequences.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR2\Sounds.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR2\Moveables.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR2\Limits.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR2\Animations.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR1\States.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR1\SpriteSequences.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR1\Sounds.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR1\Moveables.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR1\Limits.xml"
  Delete "$INSTDIR\Catalogs\Engines\TR1\Animations.xml"
  Delete "$INSTDIR\Catalogs\Engines\TombEngine\Statics.xml"
  Delete "$INSTDIR\Catalogs\Engines\TombEngine\States.xml"
  Delete "$INSTDIR\Catalogs\Engines\TombEngine\SpriteSequences.xml"
  Delete "$INSTDIR\Catalogs\Engines\TombEngine\Sounds.xml"
  Delete "$INSTDIR\Catalogs\Engines\TombEngine\Moveables.xml"
  Delete "$INSTDIR\Catalogs\Engines\TombEngine\Limits.xml"
  Delete "$INSTDIR\Catalogs\Engines\TombEngine\Animations.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr5.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr4.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr3.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr3.gold.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr2.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr2.gold.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr1.xml"
  Delete "$INSTDIR\Catalogs\NgCatalog.xml"
  Delete "$INSTDIR\Assets\Wads\TombEngine.wad2"
  Delete "$INSTDIR\Assets\Textures\default.png"
  Delete "$INSTDIR\WadTool.runtimeconfig.json"
  Delete "$INSTDIR\WadTool.exe"
  Delete "$INSTDIR\WadTool.dll.config"
  Delete "$INSTDIR\WadTool.dll"
  Delete "$INSTDIR\WadTool.deps.json"
  Delete "$INSTDIR\TombLib.WPF.dll"
  Delete "$INSTDIR\TombLib.Scripting.Tomb1Main.dll"
  Delete "$INSTDIR\TombLib.Scripting.Tomb1Main.deps.json"
  Delete "$INSTDIR\TombLib.Scripting.Lua.dll"
  Delete "$INSTDIR\TombLib.Scripting.Lua.deps.json"
  Delete "$INSTDIR\TombLib.Scripting.GameFlowScript.dll"
  Delete "$INSTDIR\TombLib.Scripting.GameFlowScript.deps.json"
  Delete "$INSTDIR\TombLib.Scripting.dll"
  Delete "$INSTDIR\TombLib.Scripting.deps.json"
  Delete "$INSTDIR\TombLib.Scripting.ClassicScript.dll"
  Delete "$INSTDIR\TombLib.Scripting.ClassicScript.deps.json"
  Delete "$INSTDIR\TombLib.Rendering.dll"
  Delete "$INSTDIR\TombLib.Rendering.deps.json"
  Delete "$INSTDIR\TombLib.Forms.dll"
  Delete "$INSTDIR\TombLib.Forms.deps.json"
  Delete "$INSTDIR\TombLib.dll"
  Delete "$INSTDIR\TombLib.deps.json"
  Delete "$INSTDIR\TombIDE.Shared.dll"
  Delete "$INSTDIR\TombIDE.Shared.deps.json"
  Delete "$INSTDIR\TombIDE.ScriptingStudio.dll"
  Delete "$INSTDIR\TombIDE.ScriptingStudio.deps.json"
  Delete "$INSTDIR\TombIDE.runtimeconfig.json"
  Delete "$INSTDIR\TombIDE.ProjectMaster.dll"
  Delete "$INSTDIR\TombIDE.ProjectMaster.deps.json"
  Delete "$INSTDIR\TombIDE.exe"
  Delete "$INSTDIR\TombIDE.dll"
  Delete "$INSTDIR\TombIDE.deps.json"
  Delete "$INSTDIR\TombIDE Library Registration.runtimeconfig.json"
  Delete "$INSTDIR\TombIDE Library Registration.exe"
  Delete "$INSTDIR\TombIDE Library Registration.dll"
  Delete "$INSTDIR\TombIDE Library Registration.deps.json"
  Delete "$INSTDIR\TombEditor.runtimeconfig.json"
  Delete "$INSTDIR\TombEditor.exe"
  Delete "$INSTDIR\TombEditor.dll.config"
  Delete "$INSTDIR\TombEditor.dll"
  Delete "$INSTDIR\TombEditor.deps.json"
  Delete "$INSTDIR\System.Runtime.CompilerServices.Unsafe.dll"
  Delete "$INSTDIR\System.IO.Pipelines.dll"
  Delete "$INSTDIR\System.IO.Hashing.dll"
  Delete "$INSTDIR\SpacedGridControl.dll"
  Delete "$INSTDIR\SoundTool.runtimeconfig.json"
  Delete "$INSTDIR\SoundTool.exe"
  Delete "$INSTDIR\SoundTool.dll"
  Delete "$INSTDIR\SoundTool.deps.json"
  Delete "$INSTDIR\SharpDX.Toolkit.Graphics.dll"
  Delete "$INSTDIR\SharpDX.Toolkit.dll"
  Delete "$INSTDIR\SharpDX.Toolkit.Compiler.dll"
  Delete "$INSTDIR\SharpDX.DXGI.dll"
  Delete "$INSTDIR\SharpDX.dll"
  Delete "$INSTDIR\SharpDX.Direct3D11.Effects.dll"
  Delete "$INSTDIR\SharpDX.Direct3D11.dll"
  Delete "$INSTDIR\SharpDX.D3DCompiler.dll"
  Delete "$INSTDIR\Pfim.dll"
  Delete "$INSTDIR\NVorbis.dll"
  Delete "$INSTDIR\NLog.dll"
  Delete "$INSTDIR\Newtonsoft.Json.Schema.dll"
  Delete "$INSTDIR\Newtonsoft.Json.dll"
  Delete "$INSTDIR\NCalc.dll"
  Delete "$INSTDIR\NAudio.WinMM.dll"
  Delete "$INSTDIR\NAudio.WinForms.dll"
  Delete "$INSTDIR\NAudio.Wasapi.dll"
  Delete "$INSTDIR\NAudio.Vorbis.dll"
  Delete "$INSTDIR\NAudio.Midi.dll"
  Delete "$INSTDIR\NAudio.Flac.dll"
  Delete "$INSTDIR\NAudio.dll"
  Delete "$INSTDIR\NAudio.Core.dll"
  Delete "$INSTDIR\NAudio.Asio.dll"
  Delete "$INSTDIR\MvvmDialogs.dll"
  Delete "$INSTDIR\MiniFileAssociation.dll"
  Delete "$INSTDIR\Microsoft.Toolkit.HighPerformance.dll"
  Delete "$INSTDIR\Microsoft.NET.StringTools.dll"
  Delete "$INSTDIR\Microsoft.Extensions.DependencyInjection.dll"
  Delete "$INSTDIR\Microsoft.Extensions.DependencyInjection.Abstractions.dll"
  Delete "$INSTDIR\Microsoft.Build.Utilities.Core.dll"
  Delete "$INSTDIR\Microsoft.Build.Tasks.Core.dll"
  Delete "$INSTDIR\Microsoft.Build.Framework.dll"
  Delete "$INSTDIR\LuaApiBuilder.dll"
  Delete "$INSTDIR\K4os.Hash.xxHash.dll"
  Delete "$INSTDIR\K4os.Compression.LZ4.Streams.dll"
  Delete "$INSTDIR\K4os.Compression.LZ4.dll"
  Delete "$INSTDIR\ICSharpCode.AvalonEdit.dll"
  Delete "$INSTDIR\IconUtilities.dll"
  Delete "$INSTDIR\FreeImage.Standard.dll"
  Delete "$INSTDIR\File Association.runtimeconfig.json"
  Delete "$INSTDIR\File Association.exe"
  Delete "$INSTDIR\File Association.dll"
  Delete "$INSTDIR\File Association.deps.json"
  Delete "$INSTDIR\DarkUI.WPF.dll"
  Delete "$INSTDIR\DarkUI.WPF.deps.json"
  Delete "$INSTDIR\DarkUI.dll"
  Delete "$INSTDIR\DarkUI.deps.json"
  Delete "$INSTDIR\d3dcompiler_43.dll"
  Delete "$INSTDIR\CustomTabControl.dll"
  Delete "$INSTDIR\CustomMessageBox.WPF.dll"
  Delete "$INSTDIR\CommunityToolkit.Mvvm.dll"
  Delete "$INSTDIR\ColorThief.Netstandard.v20.dll"
  Delete "$INSTDIR\bzPSD.dll"
  Delete "$INSTDIR\Blake3.dll"
  Delete "$INSTDIR\BCnEncoder.dll"
  Delete "$INSTDIR\AssimpNet.dll"
  RMDir "$INSTDIR\TIDE\Templates\Defaults\TR4 Resources"
  RMDir "$INSTDIR\TIDE\Templates\Defaults\Game Icons"
  RMDir "$INSTDIR\TIDE\Templates\Sounds"
  RMDir "$INSTDIR\TIDE\Templates\Shared"
  RMDir "$INSTDIR\TIDE\Templates\Presets"
  RMDir "$INSTDIR\TIDE\Templates\Extras"
  RMDir "$INSTDIR\TIDE\Templates\Defaults"
  RMDir "$INSTDIR\TIDE\NGC\VGE\sound\samples"
  RMDir "$INSTDIR\TIDE\NGC\VGE\sound\LevelSFX Creator"
  RMDir "$INSTDIR\TIDE\NGC\VGE\graphics\wads"
  RMDir "$INSTDIR\TIDE\NGC\VGE\tools"
  RMDir "$INSTDIR\TIDE\NGC\VGE\sound"
  RMDir "$INSTDIR\TIDE\NGC\VGE\Script"
  RMDir "$INSTDIR\TIDE\NGC\VGE\pix"
  RMDir "$INSTDIR\TIDE\NGC\VGE\graphics"
  RMDir "$INSTDIR\TIDE\NGC\VGE\data"
  RMDir "$INSTDIR\TIDE\NGC\VGE\audio"
  RMDir "$INSTDIR\TIDE\NGC\VGE"
  RMDir "$INSTDIR\TIDE\NGC\TRNG"
  RMDir "$INSTDIR\TIDE\DOS\TR4"
  RMDir "$INSTDIR\TIDE\TEN"
  RMDir "$INSTDIR\TIDE\Templates"
  RMDir "$INSTDIR\TIDE\NGC"
  RMDir "$INSTDIR\TIDE\GFL"
  RMDir "$INSTDIR\TIDE\GF3"
  RMDir "$INSTDIR\TIDE\DOS"
  RMDir "$INSTDIR\Runtimes\win-x86\native"
  RMDir "$INSTDIR\Runtimes\win-x64\native"
  RMDir "$INSTDIR\Runtimes\win-x86"
  RMDir "$INSTDIR\Runtimes\win-x64"
  RMDir "$INSTDIR\Resources\Localization\PL"
  RMDir "$INSTDIR\Resources\Localization\EN"
  RMDir "$INSTDIR\Resources\ClassicScript\Descriptions"
  RMDir "$INSTDIR\Resources\TRX"
  RMDir "$INSTDIR\Resources\Localization"
  RMDir "$INSTDIR\Resources\GameFlow"
  RMDir "$INSTDIR\Resources\ClassicScript"
  RMDir "$INSTDIR\Rendering\Legacy"
  RMDir "$INSTDIR\Rendering\DirectX11"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\Tomb1Main"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\GameFlowScript"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes"
  RMDir "$INSTDIR\Configs\TextEditors"
  RMDir "$INSTDIR\Catalogs\Engines\TRNG"
  RMDir "$INSTDIR\Catalogs\Engines\TR5"
  RMDir "$INSTDIR\Catalogs\Engines\TR4"
  RMDir "$INSTDIR\Catalogs\Engines\TR3"
  RMDir "$INSTDIR\Catalogs\Engines\TR2"
  RMDir "$INSTDIR\Catalogs\Engines\TR1"
  RMDir "$INSTDIR\Catalogs\Engines\TombEngine"
  RMDir "$INSTDIR\Catalogs\TEN Sound Catalogs"
  RMDir "$INSTDIR\Catalogs\TEN Node Catalogs"
  RMDir "$INSTDIR\Catalogs\Engines"
  RMDir "$INSTDIR\Assets\Wads"
  RMDir "$INSTDIR\Assets\Textures"
  RMDir "$INSTDIR\TIDE"
  RMDir "$INSTDIR\Runtimes"
  RMDir "$INSTDIR\Resources"
  RMDir "$INSTDIR\Rendering"
  RMDir "$INSTDIR\Configs"
  RMDir "$INSTDIR\Catalogs"
  RMDir "$INSTDIR\Assets"

  ; End of autogenerated Unlist block.

  ; Remove logs
  Delete "$INSTDIR\TombEditorLog.txt"
  Delete "$INSTDIR\TombEditorLog*.txt"
  Delete "$INSTDIR\TombIDELog.txt"
  Delete "$INSTDIR\TombIDELog*.txt"
  Delete "$INSTDIR\SoundToolLog.txt"
  Delete "$INSTDIR\SoundToolLog*.txt"
  Delete "$INSTDIR\WadToolLog.txt"
  Delete "$INSTDIR\WadToolLog*.txt"
  
  ; Remove configs
  Delete "$INSTDIR\Configs\TombEditorConfiguration.xml"
  Delete "$INSTDIR\Configs\SoundToolConfiguration.xml"
  Delete "$INSTDIR\Configs\WadToolConfiguration.xml"
  Delete "$INSTDIR\Configs\TombIDE*.xml"
  RMDir /r "$INSTDIR\Configs"
  
  ; Remove TIDE stuff
  
  RMDir /r "$INSTDIR\TIDE"

  ; Remove readme
  Delete "$INSTDIR\Changes.txt"
  
  ; Remove dlls which were externally copied
  Delete "$INSTDIR\d3dcompiler_43.dll"
  
  ; Remove uninstaller
  Delete "$INSTDIR\uninstall.exe"
  
  ; Remove settings
  RMDir /r "$LOCALAPPDATA\TombEditor"
  RMDir /r "$LOCALAPPDATA\SoundTool"
  RMDir /r "$LOCALAPPDATA\WadTool"

  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor"
  
  ; Remove shortcuts, if any
  Delete "$DESKTOP\Tomb Editor.lnk"
  Delete "$DESKTOP\SoundTool.lnk"
  Delete "$DESKTOP\WadTool.lnk"  
  Delete "$DESKTOP\TombIDE.lnk"
  
  Delete "$SMPROGRAMS\Tomb Editor\*.*"
  RMDir "$SMPROGRAMS\Tomb Editor"

  ; Only remove program dir if it's empty
  SetOutPath $TEMP
  Push $INSTDIR
  Call un.isEmptyDir
  Pop $0
  StrCmp $0 1 0 +2
    RMDir /r $INSTDIR
  StrCmp $0 0 0 +2
    MessageBox MB_OK \
    "Installation folder contains extra files. $\r$\n\
    Check if these files are important and remove folder manually."
     
SectionEnd

;--------------------------------

; Helper functions

Function .onInit
  ${IfNot} ${AtLeastWin7}
    MessageBox MB_OK \
    "At least Windows 7 is required to use Tomb Editor. $\r$\n\
    The installer will now quit."
    Quit
  ${EndIf}
  
  ${IfNot} ${RunningX64}
    MessageBox MB_OK \
    "This version of Tomb Editor is only compatible with 64-bit systems. $\r$\n\
    The installer will now quit."
    Quit
  ${EndIf}
FunctionEnd

Function un.isEmptyDir
  Exch $0
  Push $1
  FindFirst $0 $1 "$0\*.*"
  strcmp $1 "." 0 _notempty
    FindNext $0 $1
    strcmp $1 ".." 0 _notempty
      ClearErrors
      FindNext $0 $1
      IfErrors 0 _notempty
        FindClose $0
        Pop $1
        StrCpy $0 1
        Exch $0
        goto _end
     _notempty:
       FindClose $0
       ClearErrors
       Pop $1
       StrCpy $0 0
       Exch $0
  _end:
FunctionEnd

Function .registerExtensions
  ExecShell "runas" "$INSTDIR\File Association.exe" '-111'    
FunctionEnd

Function un.registerExtensions
  
  ExecShell "runas" "$INSTDIR\File Association.exe" '-d'
  Sleep 1000
    
FunctionEnd