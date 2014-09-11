/////////////////////////////////////////////////////////////////////////////
// DlgExportFW.cpp:
// Implementation of the CDlgExportLift class.
// Author: Kent Gorham
// copyright 2014 JAARS Inc. SIL
//
/////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include "DlgExportLift.h"
#include "Segment.h"
#include "sa_wbch.h"
#include "sa.h"
#include "sa_doc.h"
#include "sa_view.h"
#include "sa_graph.h"
#include "mainfrm.h"
#include "doclist.h"
#include "DlgResult.h"
#include <math.h>
#include "TranscriptionDataSettings.h"
#include "DlgImport.h"
#include "GlossSegment.h"
#include "PhoneticSegment.h"
#include "FileUtils.h"
#include "SplitFileUtils.h"
#include "Process\Process.h"
#include "Process\sa_p_lou.h"
#include "Process\sa_p_gra.h"
#include "Process\sa_p_pit.h"
#include "Process\sa_p_cpi.h"
#include "Process\sa_p_spi.h"
#include "Process\sa_p_mel.h"
#include "Process\sa_p_zcr.h"
#include "Process\sa_p_fra.h"
#include "Process\sa_p_spu.h"
#include "Process\sa_p_spg.h"
#include "Process\FormantTracker.h"
#include "objectostream.h"

using std::ifstream;
using std::ios;
using std::streampos;

#ifdef _DEBUG
#undef THIS_FILE
static char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CDlgExportLift dialog

BEGIN_MESSAGE_MAP(CDlgExportLift, CDialog)
    ON_BN_CLICKED(IDC_EX_SFM_INTERLINEAR, OnClickedExSfmInterlinear)
    ON_BN_CLICKED(IDC_EX_SFM_MULTIRECORD, OnClickedExSfmMultirecord)
    ON_CBN_SELCHANGE(IDC_COMBO_FIELDWORKS_PROJECT, OnSelchangeComboFieldworksProject)
    ON_CBN_KILLFOCUS(IDC_COMBO_FIELDWORKS_PROJECT, OnKillfocusComboFieldworksProject)
    ON_COMMAND(IDHELP, OnHelpExportBasic)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CExportSFM message handlers

CDlgExportLift::CDlgExportLift(LPCTSTR docTitle,
							   BOOL gloss,
							   BOOL ortho,
							   BOOL phonemic,
							   BOOL phonetic,
							   BOOL pos,
							   BOOL reference,
							   list<wstring> codes,
							   CWnd * pParent) :
    CDialog(CDlgExportLift::IDD, pParent) {

    settings.bGloss = bGlossDflt = gloss;
    settings.bOrtho = bOrthoDflt = ortho;
    settings.bPhonemic = bPhonemicDflt = phonemic;
    settings.bPhonetic = bPhoneticDflt = phonetic;
    settings.bPOS = bPOSDflt = pos;
    settings.bReference = bReferenceDflt = reference;
    settings.bPhrase = false;
    settings.szDocTitle = docTitle;
	iso = codes;
}

BOOL CDlgExportLift::OnInitDialog()
{
    CDialog::OnInitDialog();

	CSaString dir = GetFieldWorksProjectDirectory();
	bool found = true;
	if (dir.GetLength()==0) 
	{
		AfxMessageBox(IDS_NO_FIELDWORKS_PROJECT,MB_OK|MB_ICONEXCLAMATION);
		found = false;
	}

    ctlEditFieldWorksFolder.SetWindowTextW( dir);
    SetCheck(IDC_EXTAB_ANNOTATIONS, TRUE);

    CenterWindow();
    UpdateButtonState();

	list<wstring>::iterator it = iso.begin();
	while (it!=iso.end()) {
		wstring code = *it;
		ctlReferenceList.AddString(code.c_str());
		ctlOrthoList.AddString(code.c_str());
		ctlGlossList.AddString(code.c_str());
		ctlPOSList.AddString(code.c_str());
		ctlPhonemicList.AddString(code.c_str());
		ctlPhoneticList.AddString(code.c_str());
		it++;
	}

	ctlReferenceList.SetCurSel(0);
	ctlOrthoList.SetCurSel(0);
	ctlGlossList.SetCurSel(0);
	ctlPOSList.SetCurSel(0);
	ctlPhonemicList.SetCurSel(0);
	ctlPhoneticList.SetCurSel(0);

    UpdateData(FALSE);

    return TRUE;  // return TRUE  unless you set the focus to a control
}

void CDlgExportLift::DoDataExchange(CDataExchange * pDX)
{

    CDialog::DoDataExchange(pDX);
    DDX_Check(pDX, IDC_EXTAB_GLOSS, settings.bGloss);
    DDX_Check(pDX, IDC_EXTAB_ORTHO, settings.bOrtho);
    DDX_Check(pDX, IDC_EXTAB_PHONEMIC, settings.bPhonemic);
    DDX_Check(pDX, IDC_EXTAB_PHONETIC,settings. bPhonetic);
    DDX_Check(pDX, IDC_EXTAB_POS, settings.bPOS);
    DDX_Check(pDX, IDC_EXTAB_REFERENCE, settings.bReference);
    DDX_Control(pDX, IDC_EDIT_FIELDWORKS_FOLDER, ctlEditFieldWorksFolder);
    DDX_Control(pDX, IDC_COMBO_FIELDWORKS_PROJECT, ctlComboFieldWorksProject);
    DDX_Control(pDX, IDOK, ctlButtonOK);
	DDX_Control(pDX, IDC_LIST_REFERENCE, ctlReferenceList);
	DDX_Control(pDX, IDC_LIST_PHONEMIC, ctlPhonemicList);
	DDX_Control(pDX, IDC_LIST_PHONETIC, ctlPhoneticList);
	DDX_Control(pDX, IDC_LIST_ORTHO, ctlOrthoList);
	DDX_Control(pDX, IDC_LIST_GLOSS, ctlGlossList);
	DDX_Control(pDX, IDC_LIST_POS, ctlPOSList);

    if (!pDX->m_bSaveAndValidate)
    {
        ctlComboFieldWorksProject.ResetContent();

        TCHAR szPath[MAX_PATH];
        memset(szPath, 0, sizeof(szPath));
        ctlEditFieldWorksFolder.GetWindowTextW(szPath, MAX_PATH);

        CString path = szPath;
        path.Append(L"\\");
        path.Append(L"*");

        CFileFind finder;
        if (finder.FindFile(path))
		{
			BOOL more = TRUE;
			do
			{
				more = finder.FindNextFile();
				if (finder.IsDots()) continue;
				if (!finder.IsDirectory()) continue;
				TRACE(L"Found %s\n", finder.GetFileName());
				ctlComboFieldWorksProject.AddString((LPCTSTR) finder.GetFileName());
			}
			while (more);
		}
    }
    else
    {
        TCHAR szBuffer[MAX_PATH];
        GetCurrentPath(szBuffer,MAX_PATH);
        FileUtils::AppendDirSep(szBuffer,MAX_PATH);
        settings.szPath = szBuffer;

		CString buffer;
		ctlGlossList.GetWindowTextW(buffer);
		settings.gloss = buffer;

		ctlReferenceList.GetWindowTextW(buffer);
		settings.reference = buffer;

		ctlOrthoList.GetWindowTextW(buffer);
		settings.ortho = buffer;

		ctlPhonemicList.GetWindowTextW(buffer);
		settings.phonemic = buffer;

		ctlPhoneticList.GetWindowTextW(buffer);
		settings.phonetic = buffer;

		ctlPOSList.GetWindowTextW(buffer);
		settings.pos = buffer;
}
}

void CDlgExportLift::SetEnable(int nItem, BOOL bEnable)
{
    CWnd * pWnd = GetDlgItem(nItem);
    if (pWnd)
    {
        pWnd->EnableWindow(bEnable);
    }
}

void CDlgExportLift::SetCheck(int nItem, BOOL bChecked)
{
    CButton * pWnd = (CButton *) GetDlgItem(nItem);
    if (pWnd)
    {
        pWnd->SetCheck(bChecked);
    }
}

void CDlgExportLift::OnHelpExportBasic()
{

    // create the pathname
    CString szPath = AfxGetApp()->m_pszHelpFilePath;
    szPath = szPath + "::/User_Interface/Menus/File/Export/Standard_format_or_extensible_markup.htm";
    ::HtmlHelp(NULL, szPath, HH_DISPLAY_TOPIC, NULL);
}

void CDlgExportLift::OnClickedExSfmInterlinear()
{

    CButton * pWnd = (CButton *) GetDlgItem(IDC_EX_SFM_MULTIRECORD);
    if (pWnd)
    {
        pWnd->SetCheck(FALSE);
    }
}

void CDlgExportLift::OnClickedExSfmMultirecord()
{
    CButton * pWnd = (CButton *) GetDlgItem(IDC_EX_SFM_INTERLINEAR);
    if (pWnd)
    {
        pWnd->SetCheck(FALSE);
    }
}

static int CALLBACK BrowseCallbackProc(HWND hwnd, UINT uMsg, LPARAM /*lParam*/, LPARAM lpData)
{

    // If the BFFM_INITIALIZED message is received
    // set the path to the start path.
    switch (uMsg)
    {
    case BFFM_INITIALIZED:
    {
        if (NULL != lpData)
        {
            SendMessage(hwnd, BFFM_SETSELECTION, TRUE, lpData);
        }
    }
    }
    return 0;
}

void CDlgExportLift::GetCurrentPath(LPTSTR szBuffer, size_t size)
{
    wmemset(szBuffer,0,MAX_PATH);
    TCHAR szTemp[MAX_PATH];
    wmemset(szTemp,0,MAX_PATH);
        ctlEditFieldWorksFolder.GetWindowTextW(szTemp, MAX_PATH);
    wcscat_s(szBuffer,size,szTemp);
    FileUtils::AppendDirSep(szBuffer,size);
    wmemset(szTemp,0,MAX_PATH);
    int sel = ctlComboFieldWorksProject.GetCurSel();
	if (sel!=-1) {
	    ctlComboFieldWorksProject.GetLBText(sel, szTemp);
		wcscat_s(szBuffer,size,szTemp);
	}
}


void CDlgExportLift::OnSelchangeComboFieldworksProject()
{
    UpdateButtonState();
}


void CDlgExportLift::OnKillfocusComboFieldworksProject()
{
    UpdateButtonState();
}

void CDlgExportLift::UpdateButtonState()
{
    TCHAR szBuffer[MAX_PATH];
    GetCurrentPath(szBuffer,MAX_PATH);
    bool valid = FileUtils::FolderExists(szBuffer);
    bool selected = (ctlComboFieldWorksProject.GetCurSel()!=-1);
    ctlButtonOK.EnableWindow(((valid)&&(selected))?TRUE:FALSE);
}

void CDlgExportLift::WriteFileUtf8(CFile * pFile, const CSaString szString)
{

    std::string szUtf8 = szString.utf8();
    pFile->Write(szUtf8.c_str(), szUtf8.size());
}

/**
* Return the registered fieldworks project directory
* The FieldWorks application stores it's information under registry keys that are named
* according to the version - so we will recursively hunt for the ProjectsDir value
* to figure out where they are storing their projects.
*
* and to make things really interesting, FieldWorks doesn't cleanup the registry very
* well when it is uninstalled - so we need to check the validity of the registry entry.
*
*/
CSaString CDlgExportLift::GetFieldWorksProjectDirectory()
{
	// first try the 32-bit registry hive
	DWORD sam = KEY_ENUMERATE_SUB_KEYS | KEY_QUERY_VALUE | KEY_READ | KEY_WOW64_32KEY;
	wstring value;
	HKEY hRootKey = HKEY_LOCAL_MACHINE;
	wstring keyName = _T("Software\\SIL\\FieldWorks");
	if (SearchForValue( hRootKey, sam, keyName, L"ProjectsDir", value)) 
	{
		if (FileUtils::FolderExists(value.c_str())) 
		{
			return CSaString(value.c_str());
		}
	}
	hRootKey = HKEY_CURRENT_USER;
	if (SearchForValue( hRootKey, sam, keyName, L"ProjectsDir", value)) 
	{
		if (FileUtils::FolderExists(value.c_str())) 
		{
			return CSaString(value.c_str());
		}
	}

	// now try the 64-bit registry hive
	sam = KEY_ENUMERATE_SUB_KEYS | KEY_QUERY_VALUE | KEY_READ;
	hRootKey = HKEY_LOCAL_MACHINE;
	if (SearchForValue( hRootKey, sam, keyName, L"ProjectsDir", value)) 
	{
		if (FileUtils::FolderExists(value.c_str())) 
		{
			return CSaString(value.c_str());
		}
	}
	hRootKey = HKEY_CURRENT_USER;
	if (SearchForValue( hRootKey, sam, keyName, L"ProjectsDir", value)) 
	{
		if (FileUtils::FolderExists(value.c_str())) 
		{
			return CSaString(value.c_str());
		}
	}

	OSVERSIONINFO versionInfo;
    memset(&versionInfo,0,sizeof(OSVERSIONINFO));
    versionInfo.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
    BOOL result = ::GetVersionEx(&versionInfo);
    ASSERT(result);
    bool xp = versionInfo.dwMajorVersion<6;
	xp = true;
	if (xp) 
	{
		wstring path = GetShellFolderPath(CSIDL_COMMON_APPDATA);
		if (path.length()>0) 
		{
			wstring projPath = path;
			projPath.append(L"\\SIL\\FieldWorks\\Projects");
			if (FileUtils::FolderExists(projPath.c_str())) 
			{
				return CSaString(projPath.c_str());
			} 
			else 
			{
				projPath = path;
				projPath.append(L"\\SIL\\FieldWorks 7\\Projects");
				if (FileUtils::FolderExists(projPath.c_str())) 
				{
					return CSaString(projPath.c_str());
				}
			}
		}
	} 
	else 
	{
		wstring path = _wgetenv(L"ProgramData");
		if (path.length()>0) 
		{
			wstring projPath = path;
			projPath.append(L"\\SIL\\FieldWorks\\Projects");
			if (FileUtils::FolderExists(projPath.c_str())) 
			{
				return CSaString(projPath.c_str());
			} 
			else 
			{
				projPath = path;
				projPath.append(L"\\SIL\\FieldWorks 7\\Projects");
				if (FileUtils::FolderExists(projPath.c_str())) 
				{
					return CSaString(projPath.c_str());
				}
			}
		}
	}

	// it's really bad!!
	return CSaString(L"");
}

#define MAX_KEY_LENGTH 255
#define MAX_VALUE_NAME 16383
 
bool CDlgExportLift::SearchForValue( HKEY hRootKey, DWORD sam, wstring keyName, LPCTSTR valueName, wstring & value) 
{ 
	TRACE(L"Trying %s\n", keyName.c_str());

	HKEY hKey = NULL;
	DWORD retCode = RegOpenKeyEx( hRootKey, keyName.c_str(), 0, sam, &hKey);
	if (retCode!=ERROR_SUCCESS) 
	{
		TRACE(L"Unable to open %s because of %d\n", keyName.c_str(), retCode);
		return false;
	}

    TCHAR achKey[MAX_KEY_LENGTH];		// buffer for subkey name
    DWORD cbName;						// size of name string 
    TCHAR achClass[MAX_PATH] = TEXT("");  // buffer for class name 
    DWORD cchClassName = MAX_PATH;		// size of class string 
    DWORD cSubKeys=0;					// number of subkeys 
    DWORD cbMaxSubKey;					// longest subkey size 
    DWORD cchMaxClass;					// longest class string 
    DWORD cValues;						// number of values for key 
    DWORD cchMaxValue;					// longest value name 
    DWORD cbMaxValueData;				// longest value data 
    DWORD cbSecurityDescriptor;			// size of security descriptor 
    FILETIME ftLastWriteTime;			// last write time 
    TCHAR achValue[MAX_VALUE_NAME]; 
    DWORD cchValue = MAX_VALUE_NAME;

    // Get the class name and the value count. 
    retCode = RegQueryInfoKey( hKey, achClass, &cchClassName, NULL, &cSubKeys, &cbMaxSubKey, &cchMaxClass, &cValues, &cchMaxValue, &cbMaxValueData, &cbSecurityDescriptor, &ftLastWriteTime);
	if (retCode!=ERROR_SUCCESS) 
	{
		RegCloseKey( hKey);
		TRACE(L"unable to query key info %s because of %d\n", keyName.c_str(), retCode);
		return false;
	}

    // Enumerate the subkeys, until RegEnumKeyEx fails.
    if (cSubKeys>0) 
	{
        TRACE(L"Number of subkeys: %d\n", cSubKeys);
        for (DWORD i=0; i<cSubKeys; i++) 
		{ 
            cbName = MAX_KEY_LENGTH;
			memset(achKey,0,sizeof(achKey));
            retCode = RegEnumKeyEx( hKey, i, achKey, &cbName, NULL, NULL, NULL, &ftLastWriteTime); 
            if (retCode == ERROR_SUCCESS) 
			{
				wstring childKey;
				childKey = keyName;
				childKey.append(L"\\");
				childKey.append(achKey);
				if (SearchForValue( hRootKey, sam, childKey, valueName, value)) 
				{
					RegCloseKey( hKey);
					return true;
				}
            }
        }
    } 
	else 
	{
		TRACE(L"No sub keys found\n");
	}
 
    // Enumerate the key values. 
    if (cValues>0) 
	{
        TRACE(L"number of values: %d\n", cValues);
        for (DWORD i=0, retCode=ERROR_SUCCESS; i<cValues; i++) 
		{ 
            cchValue = MAX_VALUE_NAME; 
			wmemset(achValue,0,_countof(achValue));
            retCode = RegEnumValue( hKey, i, achValue, &cchValue, NULL, NULL, NULL, NULL);
            if (retCode == ERROR_SUCCESS ) 
			{
				TRACE(L"considering value %s\n",achValue);
				// is this the correct value?
				if (_wcsicmp( achValue, valueName)==0) 
				{
					TCHAR szValue[1024];
					wmemset(szValue,0,_countof(szValue));
					DWORD dwBufLen = sizeof(szValue);
					retCode = RegQueryValueEx( hKey, _T("ProjectsDir"), NULL, NULL, (LPBYTE)szValue, &dwBufLen);
					if (retCode==ERROR_SUCCESS) 
					{
						value = szValue;
						TRACE(L"success at %s : %s\n", keyName.c_str(), value.c_str());
						RegCloseKey( hKey);
						return true;
					}
				}
            } 
        }
    } 
	else 
	{
		TRACE(L"No values found\n");
	}
	TRACE(L"no joy\n");
	RegCloseKey( hKey);
	return false;
}
