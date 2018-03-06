; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Speech Analyzer MSEA"
#define MyAppVersion "3.1.0.145"
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
Source: "C:\Working\SIL\MSEA\Release\SA.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Release\SA.exe"; DestDir: "{app}"; DestName: "SAServer.exe"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Release\mbrola.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Release\SA_DSP.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Release\SA_ENU.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Release\SA_DEU.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Release\SAUtils.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Release\yeti.mmedia.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Release\yeti.wmfsdk.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Release\zGraph.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\Src\Release Notes.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\usp10.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Working\SIL\MSEA\DistFiles\Microsoft\vc2015\vc_redist.24212.x86.exe"; DestDir: "{app}\components\vc2015"; Flags: ignoreversion;
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

[Run]
Filename: "{app}\components\vc2015\vc_redist.24212.x86.exe"; Parameters: "/q"; WorkingDir: "{app}\components\vc2015"; Flags: waituntilterminated skipifdoesntexist; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; StatusMsg: "Installing Microsoft Visual C++ 2015 Redistributable"; Check: VCRedistNeedsInstall()
Filename: "{win}\Microsoft.NET\Framework\v4.0.30319\regasm.exe"; Parameters: "SAUtils.dll /tlb:SAUtils.tlb /codebase"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden; Description: "Registering SpeechToolsUtils"
Filename: "{win}\Microsoft.NET\Framework\v4.0.30319\regasm.exe"; Parameters: "yeti.mmedia.dll /tlb:yeti.mmedia.tlb /codebase"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden; Description: "Registering yeti.mmedia"
Filename: "{win}\Microsoft.NET\Framework\v4.0.30319\regasm.exe"; Parameters: "yeti.wmfsdk.dll /tlb:yeti.wmfsdk.tlb /codebase"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden; Description: "Registering yeti.wmfsdk"

[UninstallRun]
Filename: "{win}\Microsoft.NET\Framework\v4.0.30319\regasm.exe"; Parameters: "SAUtils.dll /unregister"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden
Filename: "{win}\Microsoft.NET\Framework\v4.0.30319\regasm.exe"; Parameters: "yeti.mmedia.dll /unregister"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden
Filename: "{win}\Microsoft.NET\Framework\v4.0.30319\regasm.exe"; Parameters: "yeti.wmfsdk.dll /unregister"; WorkingDir: "{app}"; Flags: waituntilterminated runhidden

[Dirs]
Name: "{app}\components"
Name: "{app}\components\vc2015"
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
#IFDEF UNICODE
  #DEFINE AW "W"
#ELSE
  #DEFINE AW "A"
#ENDIF
type
  INSTALLSTATE = Longint;
const
  INSTALLSTATE_INVALIDARG = -2;  { An invalid parameter was passed to the function. }
  INSTALLSTATE_UNKNOWN = -1;     { The product is neither advertised or installed. }
  INSTALLSTATE_ADVERTISED = 1;   { The product is advertised but not installed. }
  INSTALLSTATE_ABSENT = 2;       { The product is installed for a different user. }
  INSTALLSTATE_DEFAULT = 5;      { The product is installed for the current user. }

  VC_2005_REDIST_X86 = '{A49F249F-0C91-497F-86DF-B2585E8E76B7}';
  VC_2005_REDIST_X64 = '{6E8E85E8-CE4B-4FF5-91F7-04999C9FAE6A}';
  VC_2005_REDIST_IA64 = '{03ED71EA-F531-4927-AABD-1C31BCE8E187}';
  VC_2005_SP1_REDIST_X86 = '{7299052B-02A4-4627-81F2-1818DA5D550D}';
  VC_2005_SP1_REDIST_X64 = '{071C9B48-7C32-4621-A0AC-3F809523288F}';
  VC_2005_SP1_REDIST_IA64 = '{0F8FB34E-675E-42ED-850B-29D98C2ECE08}';
  VC_2005_SP1_ATL_SEC_UPD_REDIST_X86 = '{837B34E3-7C30-493C-8F6A-2B0F04E2912C}';
  VC_2005_SP1_ATL_SEC_UPD_REDIST_X64 = '{6CE5BAE9-D3CA-4B99-891A-1DC6C118A5FC}';
  VC_2005_SP1_ATL_SEC_UPD_REDIST_IA64 = '{85025851-A784-46D8-950D-05CB3CA43A13}';

  VC_2008_REDIST_X86 = '{FF66E9F6-83E7-3A3E-AF14-8DE9A809A6A4}';
  VC_2008_REDIST_X64 = '{350AA351-21FA-3270-8B7A-835434E766AD}';
  VC_2008_REDIST_IA64 = '{2B547B43-DB50-3139-9EBE-37D419E0F5FA}';
  VC_2008_SP1_REDIST_X86 = '{9A25302D-30C0-39D9-BD6F-21E6EC160475}';
  VC_2008_SP1_REDIST_X64 = '{8220EEFE-38CD-377E-8595-13398D740ACE}';
  VC_2008_SP1_REDIST_IA64 = '{5827ECE1-AEB0-328E-B813-6FC68622C1F9}';
  VC_2008_SP1_ATL_SEC_UPD_REDIST_X86 = '{1F1C2DFC-2D24-3E06-BCB8-725134ADF989}';
  VC_2008_SP1_ATL_SEC_UPD_REDIST_X64 = '{4B6C7001-C7D6-3710-913E-5BC23FCE91E6}';
  VC_2008_SP1_ATL_SEC_UPD_REDIST_IA64 = '{977AD349-C2A8-39DD-9273-285C08987C7B}';
  VC_2008_SP1_MFC_SEC_UPD_REDIST_X86 = '{9BE518E6-ECC6-35A9-88E4-87755C07200F}';
  VC_2008_SP1_MFC_SEC_UPD_REDIST_X64 = '{5FCE6D76-F5DC-37AB-B2B8-22AB8CEDB1D4}';
  VC_2008_SP1_MFC_SEC_UPD_REDIST_IA64 = '{515643D1-4E9E-342F-A75A-D1F16448DC04}';

  VC_2010_REDIST_X86 = '{196BB40D-1578-3D01-B289-BEFC77A11A1E}';
  VC_2010_REDIST_X64 = '{DA5E371C-6333-3D8A-93A4-6FD5B20BCC6E}';
  VC_2010_REDIST_IA64 = '{C1A35166-4301-38E9-BA67-02823AD72A1B}';
  VC_2010_SP1_REDIST_X86 = '{F0C3E5D1-1ADE-321E-8167-68EF0DE699A5}';
  VC_2010_SP1_REDIST_X64 = '{1D8E6291-B0D5-35EC-8441-6616F567A0F7}';
  VC_2010_SP1_REDIST_IA64 = '{88C73C1C-2DE5-3B01-AFB8-B46EF4AB41CD}';

  { Microsoft Visual C++ 2012 x86 Minimum Runtime - 11.0.61030.0 (Update 4) }
  VC_2012_REDIST_MIN_UPD4_X86 = '{BD95A8CD-1D9F-35AD-981A-3E7925026EBB}';
  VC_2012_REDIST_MIN_UPD4_X64 = '{CF2BEA3C-26EA-32F8-AA9B-331F7E34BA97}';

  { Microsoft Visual C++ 2012 x86 Additional Runtime - 11.0.61030.0 (Update 4)  }
  VC_2012_REDIST_ADD_UPD4_X86 = '{B175520C-86A2-35A7-8619-86DC379688B9}';
  VC_2012_REDIST_ADD_UPD4_X64 = '{37B8F9C7-03FB-3253-8781-2517C99D7C00}';

  { Visual C++ 2013 Redistributable 12.0.21005 }
  VC_2013_REDIST_X86_MIN = '{13A4EE12-23EA-3371-91EE-EFB36DDFFF3E}';
  VC_2013_REDIST_X64_MIN = '{A749D8E6-B613-3BE3-8F5F-045C84EBA29B}';

  VC_2013_REDIST_X86_ADD = '{F8CFEB22-A2E7-3971-9EDA-4B11EDEFC185}';
  VC_2013_REDIST_X64_ADD = '{929FBD26-9020-399B-9A7A-751D61F0B942}';

  { Visual C++ 2015 Redistributable 14.0.23026 }
  VC_2015_REDIST_X86_MIN = '{A2563E55-3BEC-3828-8D67-E5E8B9E8B675}';
  VC_2015_REDIST_X64_MIN = '{0D3E9E15-DE7A-300B-96F1-B4AF12B96488}';

  VC_2015_REDIST_X86_ADD = '{BE960C1C-7BAD-3DE6-8B1A-2616FE532845}';
  VC_2015_REDIST_X64_ADD = '{BC958BD2-5DAC-3862-BB1A-C1BE0790438D}';
                                
  { Visual C++ 2015 Redistributable 14.0.23506 }
  VC_2015_REDIST_X86_14_0_23506 = '{A1C31BA5-5438-3A07-9EEE-A5FB2D0FDE36}';

  { Visual C++ 2015 Redistributable 14.0.24210 }
  VC_2015_REDIST_X86_14_0_24210 = '{8FD71E98-EE44-3844-9DAD-9CB0BBBC603C}';
  VC_2015_REDIST_X64_14_0_24210 = '{C0B2C673-ECAA-372D-94E5-E89440D087AD}';

function MsiQueryProductState(szProduct: string): INSTALLSTATE; 
  external 'MsiQueryProductState{#AW}@msi.dll stdcall';

function VCVersionInstalled(const ProductID: string): Boolean;
begin
  Result := MsiQueryProductState(ProductID) = INSTALLSTATE_DEFAULT;
end;

function VCRedistNeedsInstall: Boolean;
begin
  Result := not (VCVersionInstalled(VC_2015_REDIST_X86_14_0_24210));
  if Result then
  begin
     Log('Installing VS Redistributable')
  end
  else
  begin
     Log('Skipping VS Redistributable')
  end
end;



[Code]
function HasVSRedist(): Boolean;
var                           
  sUnInstPath: String;
  sUnInstallString: String;
begin
  // VS 2015 24210 runtime key 
  sUnInstPath := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{8FD71E98-EE44-3844-9DAD-9CB0BBBC603C}';
  sUnInstallString := '';
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE, sUnInstPath, 'UninstallString', sUnInstallString) then
      RegQueryStringValue(HKEY_CURRENT_USER, sUnInstPath, 'UninstallString', sUnInstallString);
  Log('uninstallstring for vs2015:');
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
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKEY_CURRENT_USER, sUnInstPath, 'UninstallString', sUnInstallString);
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
  if not IsDotNetDetected('v4\Full', 0) then begin
    version := '.NET Framework 4.0';
    bad := true;
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
              '       '#39'.NET Framework 4.0'#39'.'#13
              '4) When you click OK, then it will take a few seconds to enable'#13
              '   the feature.'#13
              '   ** You may get a message that Windows wants to '#39'download an'#13
              '   update for this feature'#39'.  If you don'#39't have Internet access,'#13
              '   then click on '#34'Don'#39't connect to Windows Update'#34'.'#13;
    end else begin;
      // win 7 and early procedure
      body := '1) Search for '#39'Windows Features'#39' using the Start menu.'#13
              '2) In the '#39'Windows Features'#39' dialog box, select:'#13
              '       '#39'.NET Framework 4.0'#39'.'#13
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
