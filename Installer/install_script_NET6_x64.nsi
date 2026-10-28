!include LogicLib.nsh
!include MUI2.nsh
!include UnInst.nsh
!include WinVer.nsh
!include x64.nsh

!cd "..\BuildRelease (x64)\net6.0-windows"

!define UNINST_LOCALIZE

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
  
  ; Create an exclusion list
  !insertmacro UNINSTALLER_DATA_BEGIN
  
  ; Add/install files
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
  
  ; Store uninstaller data
  !insertmacro UNINSTALLER_DATA_END
  
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

LangString UNINST_EXCLUDE_ERROR ${LANG_ENGLISH} "Error creating an exclusion list."
LangString UNINST_DATA_ERROR ${LANG_ENGLISH} "Error creating the uninstaller data: $\r$\nCannot find an exclusion list."
LangString UNINST_DAT_NOT_FOUND ${LANG_ENGLISH} "$UNINST_DAT not found, unable to perform uninstall. Manually delete files."
LangString UNINST_DAT_MISSING ${LANG_ENGLISH} "$UNINST_DAT is missing, some elements could not be removed. These can be removed manually."
LangString UNINST_DEL_FILE ${LANG_ENGLISH} "Delete File"

Section "Uninstall"
  
  Call un.registerExtensions
  
  ; Terminate uninstaller if the .dat file does not exist (optimal)
  !define UNINST_TERMINATE
  
  ; Delete files
  !insertmacro UNINST_DELETE "$INSTDIR" "${UninstName}"

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
  
  ; Remove stray directories
  RMDir /r "$INSTDIR\TIDE"
  RMDir /r "$INSTDIR\Runtimes"
  RMDir /r "$INSTDIR\Resources"
  RMDir /r "$INSTDIR\Rendering"
  RMDir /r "$INSTDIR\Configs"
  RMDir /r "$INSTDIR\Catalogs"
  RMDir /r "$INSTDIR\Assets"

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