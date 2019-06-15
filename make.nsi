!define MyName "DropShadowDocker"

# ATTRIBUTES
Name '${MyName}'
OutFile '${MyName} Installer X7-2019.exe'
BrandingText 'http://cdrpro.ru/'
SetCompressor /SOLID lzma
XPStyle on
InstallColors /windows
ShowInstDetails hide
SetDateSave on
CRCCheck on
RequestExecutionLevel admin
LicenseData "LICENSE.txt"

# LANGUAGES
LoadLanguageFile "${NSISDIR}\Contrib\Language files\English.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\Russian.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\Turkish.nlf"

# PAGES
page license
Page components
Page instfiles

# APP PATHS
Var cdr17
Var cdr17x64
Var cdr18
Var cdr18x64
Var cdr19
Var cdr19x64
Var cdr20
Var cdr20x64
Var cdr21
Var cdr21x64

# PLUGINS
!include Sections.nsh
!include LogicLib.nsh
!include WinMessages.nsh

!macro execSection appVer appUserVer xVer
  StrCpy $0 'CorelDRAW ${appUserVer}'

  ${If} '${xVer}' != ''
    StrCpy $0 '$0 (64-Bit)'
  ${EndIf}

  DetailPrint $0

  ${If} '${xVer}' != ''
    StrCpy $1 '$cdr${appVer}x64\Programs64\Addons\${MyName}\'
  ${Else}
    StrCpy $1 '$cdr${appVer}\Programs\Addons\${MyName}\'
  ${EndIf}

  SetOutPath $1
  File AppUI.xslt
	File CorelDrw.addon
	File ${MyName}.dll
	File ${MyName}.gms
	File UserUI.xslt
!macroend

Section /o '' sec17
  ${IF} $cdr17 != ''
    !insertmacro execSection 17 X7 ''
  ${ENDIF}
SectionEnd

Section /o '' sec17x64
  ${IF} $cdr17x64 != ''
    !insertmacro execSection 17 X7 x64
  ${ENDIF}
SectionEnd

Section /o '' sec18
  ${IF} $cdr18 != ''
    !insertmacro execSection 18 X8 ''
  ${ENDIF}
SectionEnd

Section /o '' sec18x64
  ${IF} $cdr18x64 != ''
    !insertmacro execSection 18 X8 x64
  ${ENDIF}
SectionEnd

Section /o '' sec19
  ${IF} $cdr19 != ''
    !insertmacro execSection 19 2017 ''
  ${ENDIF}
SectionEnd

Section /o '' sec19x64
  ${IF} $cdr19x64 != ''
    !insertmacro execSection 19 2017 x64
  ${ENDIF}
SectionEnd

Section /o '' sec20
  ${IF} $cdr20 != ''
    !insertmacro execSection 20 2018 ''
  ${ENDIF}
SectionEnd

Section /o '' sec20x64
  ${IF} $cdr20x64 != ''
    !insertmacro execSection 20 2018 x64
  ${ENDIF}
SectionEnd

Section /o '' sec21
  ${IF} $cdr21 != ''
    !insertmacro execSection 21 2019 ''
  ${ENDIF}
SectionEnd

Section /o '' sec21x64
  ${IF} $cdr21x64 != ''
    !insertmacro execSection 21 2019 x64
  ${ENDIF}
SectionEnd

!macro checkApp appVer xVer
  ${If} '${xVer}' != ''
    SetRegView 64
  ${EndIf}

  ReadRegStr $0 HKLM 'SOFTWARE\Corel\CorelDRAW\${appVer}.0' 'ConfigDir'

  ${If} $0 == ''
    ReadRegStr $0 HKLM 'SOFTWARE\Corel\Corel DESIGNER\${appVer}.0' 'ConfigDir'
  ${EndIf}

  StrCpy $0 $0 -7

  ${If} '${xVer}' != ''
    StrCpy $cdr${appVer}x64 '$0'
    StrCpy $1 '$0\Programs64\CorelDRW.exe'
    StrCpy $2 '${sec${appVer}x64}'
  ${Else}
    StrCpy $cdr${appVer} '$0'
    StrCpy $1 '$0\Programs\CorelDRW.exe'
    StrCpy $2 '${sec${appVer}}'
  ${EndIf}

  ${IF} ${FileExists} $1
    !insertmacro SelectSection $2
  ${ENDIF}
!macroend

Function .onInit
  InitPluginsDir
  File /oname=$PluginsDir\splash.bmp 'cdrpro.bmp'
  advsplash::show 1000 600 400 0x00FF00 $PluginsDir\splash
  Pop $0

  !insertmacro checkApp 17 ''
  !insertmacro checkApp 17 x64
  !insertmacro checkApp 18 ''
  !insertmacro checkApp 18 x64
  !insertmacro checkApp 19 ''
  !insertmacro checkApp 19 x64
  !insertmacro checkApp 20 ''
  !insertmacro checkApp 20 x64
  !insertmacro checkApp 21 ''
  !insertmacro checkApp 21 x64

  ${IF} ${SectionIsSelected} ${sec17}
    SectionSetText ${sec17} 'CorelDRAW X7'
  ${ENDIF}
  ${IF} ${SectionIsSelected} ${sec17x64}
    SectionSetText ${sec17x64} 'CorelDRAW X7 (64-Bit)'
  ${ENDIF}

  ${IF} ${SectionIsSelected} ${sec18}
    SectionSetText ${sec18} 'CorelDRAW X8'
  ${ENDIF}
  ${IF} ${SectionIsSelected} ${sec18x64}
    SectionSetText ${sec18x64} 'CorelDRAW X8 (64-Bit)'
  ${ENDIF}

  ${IF} ${SectionIsSelected} ${sec19}
    SectionSetText ${sec19} 'CorelDRAW 2017'
  ${ENDIF}
  ${IF} ${SectionIsSelected} ${sec19x64}
    SectionSetText ${sec19x64} 'CorelDRAW 2017 (64-Bit)'
  ${ENDIF}

  ${IF} ${SectionIsSelected} ${sec20}
    SectionSetText ${sec20} 'CorelDRAW 2018'
  ${ENDIF}
  ${IF} ${SectionIsSelected} ${sec20x64}
    SectionSetText ${sec20x64} 'CorelDRAW 2018 (64-Bit)'
  ${ENDIF}

  ${IF} ${SectionIsSelected} ${sec21}
    SectionSetText ${sec21} 'CorelDRAW 2019'
  ${ENDIF}
  ${IF} ${SectionIsSelected} ${sec21x64}
    SectionSetText ${sec21x64} 'CorelDRAW 2019 (64-Bit)'
  ${ENDIF}
FunctionEnd
