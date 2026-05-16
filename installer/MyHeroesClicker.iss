#define AppName "My Heroes Clicker"
#ifndef AppVersion
#define AppVersion "0.0.0"
#endif
#ifndef PackageRoot
#define PackageRoot "..\artifacts\package\win-x64"
#endif
#ifndef OutputDir
#define OutputDir "..\artifacts"
#endif

[Setup]
AppId={{8B4CC526-156D-4B89-8D8D-514458CC3C2B}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Skarpelen
DefaultDirName={autopf}\MyHeroesClicker
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=MyHeroesClickerSetup-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
UsePreviousAppDir=no
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\My Heroes Clicker.exe
SetupIconFile=..\MyHeroesClicker.Launcher\Assets\main_image.ico
SetupLogging=yes

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[InstallDelete]
Type: files; Name: "{app}\My Heroes Clicker.exe"
Type: filesandordirs; Name: "{app}\MyHeroesClicker.API"
Type: filesandordirs; Name: "{app}\MyHeroesClicker.DbMigrator"

[Files]
Source: "{#PackageRoot}\My Heroes Clicker.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PackageRoot}\MyHeroesClicker.API\*"; DestDir: "{app}\MyHeroesClicker.API"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PackageRoot}\MyHeroesClicker.DbMigrator\*"; DestDir: "{app}\MyHeroesClicker.DbMigrator"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\My Heroes Clicker.exe"; WorkingDir: "{app}"; IconFilename: "{app}\My Heroes Clicker.exe"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\My Heroes Clicker.exe"; WorkingDir: "{app}"; IconFilename: "{app}\My Heroes Clicker.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\My Heroes Clicker.exe"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[Code]
procedure RunDatabaseMigrations;
var
  MigratorPath: String;
  ResultCode: Integer;
begin
  MigratorPath := ExpandConstant('{app}\MyHeroesClicker.DbMigrator\MyHeroesClicker.DbMigrator.exe');

  if not Exec(MigratorPath, '', ExtractFilePath(MigratorPath), SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    RaiseException('Failed to start SQLite migrator.');
  end;

  if ResultCode <> 0 then
  begin
    RaiseException('SQLite migration failed. Exit code: ' + IntToStr(ResultCode) + '.');
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    RunDatabaseMigrations;
  end;
end;
