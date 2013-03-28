/////////////////////////////////////////////////////////////////////////////
// DlgAdvancedParse.h:
// Interface of the CDlgAdvancedParse
//
// Author: Urs Ruchti
// copyright 2000 JAARS Inc. SIL
//
// Revision History
//
// 01/12/2000
//    SDM   Extract from sa_dlg.h
//
/////////////////////////////////////////////////////////////////////////////
#ifndef DLGADVANCEDPARSEWORDS_H
#define DLGADVANCEDPARSEWORDS_H

#include "sa_dlwnd.h"
#include "resource.h"
#include "afxwin.h"

class CSaDoc;

class CDlgAdvancedParseWords : public CDialog
{
    DECLARE_DYNAMIC(CDlgAdvancedParseWords)

    // Construction/destruction/creation
public:
    CDlgAdvancedParseWords(CSaDoc * pDoc); // standard constructor
    virtual ~CDlgAdvancedParseWords();

    BOOL Create();
    void Show(LPCTSTR title);

    // Attributes
private:
    CSpinControl m_SpinBreak;
    CSpinControl m_SpinMaxThreshold;
    CSpinControl m_SpinMinThreshold;

    // Dialog Data
    //{{AFX_DATA(CDlgAdvancedParse)
    enum { IDD = IDD_ADVANCEDPARSEWORD };
    int m_nBreakWidth;
    int m_nMaxThreshold;
    int m_nMinThreshold;
    //}}AFX_DATA

    // Operations
protected:
    virtual void DoDataExchange(CDataExchange * pDX); // DDX/DDV support
    void Apply();
    void Undo();

    // Generated message map functions
    //{{AFX_MSG(CDlgAdvancedParseWords)
    virtual BOOL OnInitDialog();
    afx_msg void OnBreakWidthScroll();
    afx_msg void OnMaxThresholdScroll();
    afx_msg void OnMinThresholdScroll();
    afx_msg void OnBnClickedApply();
    //}}AFX_MSG
    DECLARE_MESSAGE_MAP()
    virtual void OnOK();
    virtual void OnCancel();
public:

    CButton m_ApplyButton;
    CButton m_OKButton;
    CSaDoc * m_pDoc;
};
#endif
