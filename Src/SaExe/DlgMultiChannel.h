/////////////////////////////////////////////////////////////////////////////
// DlgMultiChannel.h:
// Implementation of the CDlgMultiChannel
//
// Author: Kent Gorham
// copyright 2012 JAARS Inc. SIL
//
// Revision History
//
/////////////////////////////////////////////////////////////////////////////
#ifndef DLGMULTICHANNEL_H
#define DLGMULTICHANNEL_H

#include "resource.h"
#include "afxwin.h"

class CSaDoc;

class CDlgMultiChannel : public CDialog
{

    DECLARE_DYNAMIC(CDlgMultiChannel)
public:
    CDlgMultiChannel(int nChannels, bool allowCombine);
    ~CDlgMultiChannel();

    enum { IDD = IDD_MULTICHANNEL };

protected:
    virtual void DoDataExchange(CDataExchange * pDX);
    virtual BOOL OnInitDialog();
    DECLARE_MESSAGE_MAP()

protected:
    CComboBox m_MultiChannelCombo;
    int m_nChannels;
    bool m_bAllowCombine;

public:
    int m_nChannel;
};

#endif
