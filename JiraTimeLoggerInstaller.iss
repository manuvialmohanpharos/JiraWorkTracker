; Jira Time Logger Installer Script
[Setup]
AppName=Jira Time Logger
AppVersion=1.0
DefaultDirName={autopf}\JiraTimeLogger
DefaultGroupName=Jira Time Logger
OutputDir=.
OutputBaseFilename=JiraTimeLoggerSetup
Compression=lzma
SolidCompression=yes

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Jira Time Logger"; Filename: "{app}\JiraWorkTracker.exe"
Name: "{group}\Uninstall Jira Time Logger"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\JiraWorkTracker.exe"; Description: "Launch Jira Time Logger"; Flags: nowait postinstall skipifsilent