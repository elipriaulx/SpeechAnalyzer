; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Speech Analyzer MSEA"
#define MyAppVersion "3.1.0.136"
#define MyAppPublisher "SIL International, Inc."
#define MyAppURL "http://www.speechanalyzer.sil.org/"
#define MyAppExeName "SA.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{E4ECCF52-56A6-47C9-9B4D-11B8C5CDBF29}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\SIL\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=C:\Working\SIL\MSEA\Install\Output
OutputBaseFilename=SpeechAnalyzerMSEA
Compression=lzma
SolidCompression=yes
SetupLogging=yes
PrivilegesRequired=admin
UsePreviousAppDir=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1
           
[Files]
Source: "C:\Working\SIL\MSEA\Output\Release\SA.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\SA.exe"; DestDir: "{app}"; DestName: "SAServer.exe"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\ECInterfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\mbrola.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\SA_DSP.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\SA_ENU.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\SA_DEU.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\SilEncConverters40.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\SAUtils.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\yeti.mmedia.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\yeti.wmfsdk.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Output\Release\zGraph.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Src\Release Notes.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\usp10.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\Microsoft\vcredist_x86.exe"; DestDir: "{app}\components"; Flags: ignoreversion; Check: VCRedistCheck()
Source: "C:\Working\SIL\MSEA\DistFiles\Bmp2png.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\xerces\xerces-c_3_1.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\xerces\notice"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\xerces\license"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\What's New.pdf"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\Speech_Analyzer_Help.chm"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\Using_Help.chm"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\uriparser\uriparser_copying"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\iso639.txt"; DestDir: "{app}"; Flags: ignoreversion
;samples
Source: "C:\Working\SIL\MSEA\DistFiles\Samples\*"; DestDir: "{userdocs}\Speech Analyzer\Samples"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\Samples\Music\*"; DestDir: "{userdocs}\Speech Analyzer\Samples\Music"; Flags: ignoreversion
;training
Source: "C:\Working\SIL\MSEA\DistFiles\Training\*.pdf"; DestDir: "{app}\Training"; Flags: ignoreversion
;fonts
Source: "C:\Working\SIL\MSEA\DistFiles\Fonts\DoulosSILR.ttf"; DestDir: "{fonts}"; FontInstall: "Doulos SIL"; Flags: onlyifdoesntexist uninsneveruninstall
Source: "C:\Working\SIL\MSEA\Lib\Fonts\Musique\MusiqueU.ttf"; DestDir: "{fonts}"; FontInstall: "Musique"; Flags: onlyifdoesntexist uninsneveruninstall
Source: "C:\Working\SIL\MSEA\Lib\Fonts\Musique\Musique Unicode.ttf"; DestDir: "{fonts}"; FontInstall: "Musique Unicode"; Flags: onlyifdoesntexist uninsneveruninstall
;dtds for musicxml
Source: "C:\Working\SIL\MSEA\DistFiles\MusicXML\dtds\*" ; DestDir: "{app}\dtds"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

;.NET 3.5 = .NET 2.0 + .NET 3.5
;Due to the way .NET 3.5 is "just" an add-on to .NET 3.0 which is "just" an add-on to .NET 2.0, 
; the tools in the .NET 2.0 directory are still the ones to use.  
; The CLR version number is the same for all three of these frameworks (2.0.50727).
[Run]
Filename: "{app}\components\vcredist_x86.exe"; Parameters: "/q"; WorkingDir: "{app}\components"; Flags: waituntilterminated skipifdoesntexist; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; StatusMsg: "Installing Microsoft Visual C++ 2010 SP1 Redistributable"
Filename: "{win}\Microsoft.NET\Framework\v2.0.50727\regasm.exe"; Parameters: "SAUtils.dll /tlb:SAUtils.tlb /codebase"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden; Description: "Registering SpeechToolsUtils"
Filename: "{win}\Microsoft.NET\Framework\v2.0.50727\regasm.exe"; Parameters: "yeti.mmedia.dll /tlb:yeti.mmedia.tlb /codebase"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden; Description: "Registering yeti.mmedia"
Filename: "{win}\Microsoft.NET\Framework\v2.0.50727\regasm.exe"; Parameters: "yeti.wmfsdk.dll /tlb:yeti.wmfsdk.tlb /codebase"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden; Description: "Registering yeti.wmfsdk"

[UninstallRun]
Filename: "{win}\Microsoft.NET\Framework\v2.0.50727\regasm.exe"; Parameters: "SAUtils.dll /unregister"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden
Filename: "{win}\Microsoft.NET\Framework\v2.0.50727\regasm.exe"; Parameters: "yeti.mmedia.dll /unregister"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden
Filename: "{win}\Microsoft.NET\Framework\v2.0.50727\regasm.exe"; Parameters: "yeti.wmfsdk.dll /unregister"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden

[Dirs]
Name: "{app}\components"
Name: "{app}\Samples"
Name: "{app}\Samples\Music"
Name: "{app}\Training"

[InstallDelete]
Type: files; Name: "{app}\SA MSEA Release Notes.txt"
Type: files; Name: "{app}\SA MSEA Roadmap.txt"
Type: files; Name: "{app}\Roadmap.txt"

[UninstallDelete]
Type: files; Name: "{app}\SAUtils.tlb"
Type: files; Name: "{app}\yeti.mmedia.tlb"
Type: files; Name: "{app}\yeti.wmfsdk.tlb"

[Registry]
Root: HKLM64; Subkey: "Software\SIL\Speech Analyzer"; ValueType: string; ValueName: "Location"; ValueData: "{app}\SA.exe"; Check: IsWin64 
Root: HKLM32; Subkey: "Software\SIL\Speech Analyzer"; ValueType: string; ValueName: "Location"; ValueData: "{app}\SA.exe"

[Code]
function VCRedistCheck(): Boolean;
begin
  Result := (FileExists(ExpandConstant('{win}\WinSxS\x86_Microsoft.VC90.MFC_1fc8b3b9a1e18e3b_9.0.30729.1_x-ww_405b0943\mfc90.dll'))=false);
end;

/////////////////////////////////////////////////////////////////////
function GetMSEAUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
  Log('uninstallstring for msea:');
  Log(sUnInstallString);
end;

/////////////////////////////////////////////////////////////////////
function Has31UninstallString(): Boolean;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  // old 3.1 product id
  sUnInstPath := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{D99E9365-BB4F-4430-875C-BD5516EE92DA}';
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Log('uninstallstring for 31:');
  Log(sUnInstallString);
  if (sUnInstallString <> '') then
    begin
      Result := TRUE
      Log('result=true')
    end
  else
    begin
      Result := FALSE;
      Log('result=false')
    end;
end;

/////////////////////////////////////////////////////////////////////
function UnInstallMSEA(): Boolean;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin

  // default return value
  Result := FALSE;

  // get the uninstall string of the old app
  sUnInstallString := GetMSEAUninstallString();
  if sUnInstallString <> '' then begin
    Log('uninstalling msea');
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_SHOW, ewWaitUntilTerminated, iResultCode) then
      begin
        Log('uninstall succeeded');
        Result := TRUE;
      end
    else
      begin
        Log('unistall exec failed');
        Result := FALSE;
      end;
  end else
    Result := FALSE;
end;

/////////////////////////////////////////////////////////////////////
function UnInstall_3_1(): Boolean;
var
  iResultCode: Integer;
begin

  // default return value
  Result := FALSE;

  // get the uninstall string of the old app
  if Has31UninstallString() then begin
    Log('uninstalling 3.1');
    if Exec('MsiExec.exe', '/x {D99E9365-BB4F-4430-875C-BD5516EE92DA} /quiet /passive /norestart','', SW_SHOW, ewWaitUntilTerminated, iResultCode) then
      begin
        Log('uninstall succeeded');
        Result := TRUE;
      end
    else
      begin
        Log('unistall exec failed');
        Result := FALSE;
      end;
  end else
    Result := FALSE;

end;

/////////////////////////////////////////////////////////////////////
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then
  begin
    UnInstallMSEA();
    UnInstall_3_1();
  end;
end;

function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1'          .NET Framework 1.1
//    'v2.0'          .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//    'v4.5.1'        .NET Framework 4.5.1
//    'v4.5.2'        .NET Framework 4.5.2
//    'v4.6'          .NET Framework 4.6
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key, versionKey: string;
    install, release, serviceCount, versionRelease: cardinal;
    success: boolean;
begin
    versionKey := version;
    versionRelease := 0;

    // .NET 1.1 and 2.0 embed release number in version key
    if version = 'v1.1' then begin
        versionKey := 'v1.1.4322';
    end else if version = 'v2.0' then begin
        versionKey := 'v2.0.50727';
    end

    // .NET 4.5 and newer install as update to .NET 4.0 Full
    else if Pos('v4.', version) = 1 then begin
        versionKey := 'v4\Full';
        case version of
          'v4.5':   versionRelease := 378389;
          'v4.5.1': versionRelease := 378675; // or 378758 on Windows 8 and older
          'v4.5.2': versionRelease := 379893;
          'v4.6':   versionRelease := 393295; // or 393297 on Windows 8.1 and older
        end;
    end;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + versionKey;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0 and newer use value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 and newer use additional value Release
    if versionRelease > 0 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= versionRelease);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;


function InitializeSetup(): Boolean;
Var
  msg : String;
  version : String;
  intro : String;
  body : String;
  bad : Boolean;

begin
  bad := false;
  result := true;
  if not IsDotNetDetected('v2.0', 0) then begin
    version := '.NET Framework 2.0';
    bad := true;
  end else begin
    if not IsDotNetDetected('v2.0', 0) then begin
      version := '.NET Framework 3.5';
      bad := true;
    end;
  end;

  if (bad) then begin
    intro := 'SpeechAnalyzer requires Microsoft '+version+'.'#13#13
             'Please use '#34'Windows Features'#34' to install this version, and'#13
             'then re-run the SpeechAnalyzer setup program.'#13#13
             'How to install '+version+':'#13;

    if (GetWindowsVersion >= $06020000) then begin
      // starting with win8, the procedure is different
      body := '1) Search for '#39'Windows Features'#39' using the Start menu.'#13
              '2) Click on the text: '#39'Turn Windows Features On or Off'#39'.'#13
              '3) In the '#39'Windows Features'#39' dialog box, select:'#13
              '       '#39'.NET Framework 3.5 (includes .NET 2.0 and 3.0)'#39'.'#13
              '4) When you click OK, then it will take a few seconds to enable'#13
              '   the feature.'#13
              '   ** You may get a message that Windows wants to '#39'download an'#13
              '   update for this feature'#39'.  If you don'#39't have Internet access,'#13
              '   then click on '#34'Don'#39't connect to Windows Update'#34'.'#13;
    end else begin;
      // win 7 and early procedure
      body := '1) Search for '#39'Windows Features'#39' using the Start menu.'#13
              '2) In the '#39'Windows Features'#39' dialog box, select:'#13
              '       '#39'.NET Framework 3.5 (includes .NET 2.0 and 3.0)'#39'.'#13
              '3) When you click OK, then it will take a few seconds to enable'#13
              '   the feature.'#13
              '   ** You may get a message that Windows wants to '#39'download an'#13
              '   update for this feature'#39'.  If you don'#39't have Internet access,'#13
              '   then click on '#34'Don'#39't connect to Windows Update'#34'.'#13;
    end;
    msg := intro + body;
    MsgBox(msg,mbInformation, MB_OK);
    result := false;
  end;

end;
