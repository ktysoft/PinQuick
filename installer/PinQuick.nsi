!include "MUI2.nsh"
!include "x64.nsh"

Unicode true
RequestExecutionLevel admin
SetCompressor /SOLID lzma

!define APP_NAME "PinQuick"
!define APP_VERSION "0.4.0"
!define PUBLISHER "KTYSoft"
!define APP_EXE "PinQuick.App.exe"

; Betik, installer/ klasöründen çağrılmalıdır:
;   makensis /DARCH=x64 PinQuick.nsi

!ifndef ARCH
  !define ARCH "x64"
!endif

!if ${ARCH} == "x86"
  !define MACHINE_TYPE "32-bit"
!else if ${ARCH} == "arm64"
  !define MACHINE_TYPE "ARM64"
!else
  !define MACHINE_TYPE "64-bit"
!endif

; Kurulumcu dosya adı
OutFile "..\Installers\${APP_NAME}-Setup-${APP_VERSION}-${ARCH}.exe"

Name "${APP_NAME}"
Caption "${APP_NAME} ${APP_VERSION} Kurulumu"

; Sayfa kurulumcu ikonu (AppIcon.ico)
Icon "..\src\PinQuick.App\Assets\AppIcon.ico"
UninstallIcon "..\src\PinQuick.App\Assets\AppIcon.ico"

; Kurulumcu meta bilgisi (Dosya > Özellikler)
VIProductVersion "0.4.0.0"
VIAddVersionKey "ProductName" "${APP_NAME}"
VIAddVersionKey "ProductVersion" "${APP_VERSION}"
VIAddVersionKey "FileDescription" "${APP_NAME} ${APP_VERSION} Kurulumu"
VIAddVersionKey "FileVersion" "${APP_VERSION}"
VIAddVersionKey "CompanyName" "${PUBLISHER}"
VIAddVersionKey "LegalCopyright" "${APP_VERSION} ${PUBLISHER}"

; Kurulum klasörü
InstallDir "$PROGRAMFILES64\${APP_NAME}"
InstallDirRegKey HKLM "Software\${APP_NAME}" "InstallDir"

!if ${ARCH} == "x86"
  InstallDir "$PROGRAMFILES\${APP_NAME}"
  InstallDirRegKey HKLM "Software\${APP_NAME}" "InstallDir"
!endif

Var StartMenuFolder

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_RUN "$INSTDIR\${APP_EXE}"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "Turkish"
!insertmacro MUI_LANGUAGE "English"

; Mimari denetimi
Section "PinQuick" SEC_MAIN
  !if ${ARCH} == "x64"
    ${IfNot} ${RunningX64}
      MessageBox MB_OK|MB_ICONSTOP "Bu kurulum 64-bit Windows gerektirir. / This setup requires 64-bit Windows."
      Quit
    ${EndIf}
  !else if ${ARCH} == "arm64"
    ${IfNot} ${IsNativeARM64}
      MessageBox MB_OK|MB_ICONSTOP "Bu kurulum ARM64 Windows gerektirir. / This setup requires ARM64 Windows."
      Quit
    ${EndIf}
  !endif

  SetOutPath "$INSTDIR"
  File /r "..\src\PinQuick.App\bin\Release\net10.0-windows10.0.26100.0\win-${ARCH}\publish\*.*"

  ; Uygulama ikonu, kısayollar için dizine kopyalanır
  File "..\src\PinQuick.App\Assets\AppIcon.ico"

  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ; Kurulum kayıtları
  WriteRegStr HKLM "Software\${APP_NAME}" "InstallDir" "$INSTDIR"
  WriteRegStr HKLM "Software\${APP_NAME}" "Version" "${APP_VERSION}"

  ; Başlat menüsü
  StrCpy $StartMenuFolder "${APP_NAME}"
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\AppIcon.ico"
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\Kaldır.lnk" "$INSTDIR\Uninstall.exe" "" "$INSTDIR\AppIcon.ico"
  ; Masaüstü kısayolu
  CreateShortCut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\AppIcon.ico"
SectionEnd

Section "Uninstall"
  Delete "$DESKTOP\${APP_NAME}.lnk"
  RMDir /r "$SMPROGRAMS\$StartMenuFolder"

  Delete "$INSTDIR\Uninstall.exe"
  RMDir /r "$INSTDIR"

  DeleteRegKey HKLM "Software\${APP_NAME}"
SectionEnd