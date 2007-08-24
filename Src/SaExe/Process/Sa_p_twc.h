/////////////////////////////////////////////////////////////////////////////
// sa_p_twc.h:
// Interface of the CProcessTonalWeightChart class
// Author: Corey Wenger
// copyright 1999 JAARS Inc. SIL
/////////////////////////////////////////////////////////////////////////////
#ifndef _SA_P_TWC_H
#define _SA_P_TWC_H

#include "appdefs.h"

//###########################################################################
// CProcessTonalWeightChart data processing

// CProcessTonalWeightChart defines
class CHistogram;
class CProcessMelogram;

class CProcessTonalWeightChart : public CDataProcess
{
// Construction/destruction/creation
public:
  CProcessTonalWeightChart();
  virtual ~CProcessTonalWeightChart();
  double GetMinSemitone() {return m_dMinSemitone;} // return Min Semitone
  double GetMaxSemitone() {return m_dMaxSemitone;} // return Max Semitone
  short GetBinsPerSemitone() {return m_nBinsPerSemitone;} // return # of Histogram Bins per Semitone
  CHistogram* MakeTwcHistogram(CProcessMelogram* pMelogram, int nBinsPerSemitone, double nMinSemitone, double nMaxSemitone, DWORD dwFrameStart, DWORD dwFrameSize);

// Attributes
private:
  DWORD m_dwFrameStart;
  DWORD m_dwFrameSize;
  double m_dMinSemitone;
  double m_dMaxSemitone;
  short m_nBinsPerSemitone;
  BOOL  m_bSaveCursorPositions;  // determines whether to save the start/stop cursor positions
  DWORD m_dwFrameStartSave;    // TCJ 6/23 - Save the 'dwFrameStart' value
  DWORD m_dwFrameSizeSave;    // TCJ 6/23 - Save the 'dwFrameSize' value

// Operations
protected:
    
public:
  long Process(void* pCaller, ISaDoc * pDoc, DWORD dwFrameStart, DWORD dwFrameSize, short nMinSemitone, short nMaxSemitone, int nProgress = 0, int nLevel = 1);
  
  // CLW 10/24/00 removed SaveCursorPosition code.
  // This scheme does not work if waveform data is removed.
  // A better and simpler way is starting at 0 and processing to the end of the melogram data.
};


#endif //_SA_P_TWC_H