/////////////////////////////////////////////////////////////////////////////
// DlgAnnotation.cpp:
// Implementation of the CDlgAnnotation class.
// Author: Steve MacLean
// copyright 2012 JAARS Inc. SIL
//
// Revision History
// 1.5Test8.3
//         SDM Original version
// 1.5Test8.5
//         SDM Changed Table headings to use Initial Uppercase ("Emic" & "Etic")
// 1.5Test10.7
//         SDM Changed Import to not pad extra phonetic with spaces
// 1.5Test11.0
//         SDM replaced GetOffset() + GetDuration() with CSegment::GetStop()
//         SDM fixed bug in export which crashed if phonetic segment empty
//         SDM changed export to export AutoPitch (Grappl)
// 1.5Test11.3
//         SDM changed CSegment::AdjustPositionAll to CSegment::Adjust
//
//    07/27/2000
//         DDO Changed so these dialogs display before the user is asked
//             what file to export. Therefore, I had to move the get filename
//             dialog call into this module instead of doing it in the view
//             class when the user picks one of the export menu items.
//
/////////////////////////////////////////////////////////////////////////////
#ifndef DLGANNOTATIONIMPORT_H
#define DLGANNOTATIONIMPORT_H

/////////////////////////////////////////////////////////////////////////////
// CDlgAnnotationImport private dialog

class CDlgAnnotationImport : public CDialog
{
	// Construction
public:
	CDlgAnnotationImport(BOOL bPhonetic, BOOL bPhonemic, BOOL bOrtho, CWnd* pParent = NULL);   // standard constructor

	// Dialog Data
	//{{AFX_DATA(CDlgAnnotationImport)
	enum { IDD = IDD_ANNOTATION_WZD_IMPORT };
	BOOL    m_bGloss;
	BOOL    m_bPhonemic;
	BOOL    m_bPhonetic;
	CSaString m_szPhonemic;
	CSaString m_szGloss;
	CSaString m_szPhonetic;
	BOOL    m_bOrthographic;
	CSaString m_szOrthographic;
	//}}AFX_DATA

	// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	void SetEnable(int nItem, BOOL bEnable);

	// Generated message map functions
	//{{AFX_MSG(CDlgAnnotationImport)
	afx_msg void OnImportPlainText();
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
#endif
