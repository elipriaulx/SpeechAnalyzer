/////////////////////////////////////////////////////////////////////////////
// sa_p_fra.h:
// Interface of the CProcessFragments class.
// Author: Alec Epting
// copyright 1998 JAARS Inc. SIL
/////////////////////////////////////////////////////////////////////////////
#ifndef _SA_P_FRA_H
#define _SA_P_FRA_H 

#include "appdefs.h"
#include "dsp\fragment.h"

//###########################################################################             
// CProcessFragments data processing

class CProcessFragments : public CDataProcess
{
// Construction/destruction/creation
public:
    CProcessFragments();
    virtual ~CProcessFragments();

// Attributes
private:
    CFragment *m_pFragmenter;
    ULONG m_dwFragmentCount;
    ULONG m_dwWaveIndex;
    ULONG m_dwPitchIndex;
    ULONG m_dwFragmentIndex;

// Operations
public:
    long Process(void* pCaller, ISaDoc* pDoc, int nProgress = 0, int nLevel = 1);
    ULONG GetBufferLength();
    FRAG_PARMS* GetFragmentBlock(ULONG dwFragmentIndex);
    const FRAG_PARMS& GetFragmentParms(ULONG dwFragmentIndex);
    ULONG GetFragmentCount() {return m_dwFragmentCount;}
    ULONG GetFragmentIndex(ULONG dwWaveIndex);
};


#endif //_SA_P_FRA_H