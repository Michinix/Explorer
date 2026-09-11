; Installateur Windows basique pour Explorer (Inno Setup).
; Compilation : iscc Explorer.iss /DAppVersion=1.2.3 /DSourceDir=<dossier publish> /DOutputDir=<dossier de sortie>
; (des valeurs par défaut raisonnables sont utilisées si ces paramètres ne sont pas fournis,
;  pratique pour compiler manuellement en local depuis ce dossier après un `dotnet publish`).

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\..\publish"
#endif
#ifndef OutputDir
  #define OutputDir "..\..\dist"
#endif

#define AppName "Explorer"
#define AppPublisher "Michinix"
#define AppExeName "Explorer.exe"
#define AppId "276E4906-1EE7-4711-9F6F-288BBEE10CE6"

[Setup]
AppId={{#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}
OutputDir={#OutputDir}
OutputBaseFilename=ExplorerSetup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes
PrivilegesRequiredOverridesAllowed=dialog
SetupIconFile=..\..\Explorer\Assets\Logo.ico

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Désinstaller {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Créer un raccourci sur le Bureau"; GroupDescription: "Raccourcis :"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Lancer {#AppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Nettoie tout fichier généré par l'application dans son propre dossier d'installation
; (logs, cache, etc.) en plus des fichiers installés, pour ne rien laisser derrière.
Type: filesandordirs; Name: "{app}"
