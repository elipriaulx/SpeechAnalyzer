/////////////////////////////////////////////////////////////////////////////
// sa_g_rec.cpp:
// Implementation of the CPlotRecording class.
// Author: Todd Jones
// copyright 2000 JAARS Inc. SIL
//
// Revision History
//  08/29/2000
//         DDO Reworked so most of the drawing takes place in the parent
//             class instead of here. However, some overridden functions
//             were added here to accomodate the fact that this plot
//             cannot zoom. All this was done to solve the problem with
//             the cursors in this plot not being in the same location
//             on the screen as zoomed-in views of other plots.
//
//  09/27/2000
//         DDO Since we nolonger call this the recording graph (it's
//             called "Position View", I return "Position View" in
//             SetLegendScale rather than "Rec."
//
//  09/30/2000
//         DDO Rewrote the AdjustDataFrame function because it wasn't
//             returning the proper value when opened after other graphs
//             were in a zoomed-in state.
//
//  10/25/2000
//         DDO Changed the highlighted area so it covers what's between the
//             window edges in other graphs, not what's between the cursors.
/////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "sa_plot.h"
#include "sa_g_raw.h"
#include "sa_g_rec.h"
#include "sa_minic.h"
#include "sa_graph.h"

#include "math.h"
#include "sa_doc.h"
#include "sa_view.h"
#include "mainfrm.h"

#ifdef _DEBUG
#undef THIS_FILE
static char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//###########################################################################
// CPlotRecording
// Plot window for displaying the recording. This class only handles the
// special drawing of the data. All other work is done in the base class.

IMPLEMENT_DYNCREATE(CPlotRecording, CPlotRawData)

/////////////////////////////////////////////////////////////////////////////
// CPlotRecording message map

BEGIN_MESSAGE_MAP(CPlotRecording, CPlotRawData)
    //{{AFX_MSG_MAP(CPlotRecording)
    //}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CPlotRecording construction/destruction/creation

/***************************************************************************/
// CPlotRecording::CPlotRecording Constructor
/***************************************************************************/
CPlotRecording::CPlotRecording()
{
    m_dwRecDataFrame = 0L;
}

/***************************************************************************/
/***************************************************************************/
void  CPlotRecording::CopyTo(CPlotRecording * pT)
{
    CPlotWnd::CopyTo((CPlotWnd *)pT);
}

/***************************************************************************/
/***************************************************************************/
CPlotWnd * CPlotRecording::NewCopy(void)
{
    CPlotRecording * pRet = new CPlotRecording();
    CopyTo((CPlotRecording *)pRet);
    return (CPlotWnd *)pRet;
}

/***************************************************************************/
// CPlotRecording::~CPlotRecording Destructor
/***************************************************************************/
CPlotRecording::~CPlotRecording()
{
}

/////////////////////////////////////////////////////////////////////////////
// CPlotRecording helper functions

/////////////////////////////////////////////////////////////////////////////
// CPlotRecording message handlers

//**************************************************************************/
// 08/30/2000 - DDO Added this overriding function.
//**************************************************************************/
DWORD CPlotRecording::GetAreaPosition()
{
    return 0L;
}

//**************************************************************************/
// 08/30/2000 - DDO Added this overriding function.
//**************************************************************************/
DWORD CPlotRecording::GetAreaLength(CRect * /*pRwnd*/)
{
    return m_dwRecDataFrame;
}

/***************************************************************************/
// CPlotRecording::SetMagnify Set the magnify factor
/***************************************************************************/
void CPlotRecording::SetMagnify(double fMagnify, BOOL bRedraw)
{
    UNUSED_ALWAYS(fMagnify);  //  The recording plot does not zoom
    m_fMagnify = 1.0;
    if (bRedraw)
    {
        RedrawPlot();    // repaint whole plot window
    }
}

/***************************************************************************/
// 08/29/2000 - DDO Added this overriding function.
/***************************************************************************/
DWORD CPlotRecording::AdjustDataFrame(int nWidth)
{
    if (!m_dwRecDataFrame)
    {
        CSaDoc * pDoc = ((CMainFrame *)AfxGetMainWnd())->GetCurrDoc();

        if (!pDoc)
        {
            return 0L;
        }
        FmtParm * pFmtParm = pDoc->GetFmtParm();                         // get sa parameters format member data
        DWORD dwDataSize = pDoc->GetDataSize();
        UINT nSampleSize = pFmtParm->wBlockAlign / pFmtParm->wChannels;

        if ((DWORD)nWidth > (dwDataSize / (DWORD)nSampleSize))    // more pixels than data
        {
            m_dwRecDataFrame = (DWORD)nWidth * (DWORD)nSampleSize;    // extend data frame to number of pixels
        }
        else
        {
            m_dwRecDataFrame = dwDataSize;
        }
    }

    return m_dwRecDataFrame;
}

/***************************************************************************/
// 08/29/2000 - DDO Added this overriding function.
/***************************************************************************/
BOOL CPlotRecording::SetLegendScale()
{
    CGraphWnd * pGraph = (CGraphWnd *)GetParent();
    return pGraph->SetLegendScale(SCALE | NUMBERS, -100, 100, _T("Position View")); // set legend scale
}

/***************************************************************************/
// CPlotRecording::OnDraw drawing
// The data to draw is coming from the documents wave file. Call the
// function PlotPaintFinish at the end of the drawing to let the plot base
// class do common jobs like drawing the cursors.
/***************************************************************************/
void CPlotRecording::OnPaint()
{
    CGraphWnd * pGraph = (CGraphWnd *)GetParent();
    CSaView * pView = (CSaView *)pGraph->GetParent();

    ShowCursors(FALSE,TRUE); // The highlight area is disabled if cursors are not showing
    // This is the priary place the highlight area is updated
    CPlotRawData::SetHighLightArea((DWORD)pView->GetDataPosition(0), (DWORD)pView->GetDataPosition(0) + pView->GetDataFrame(), TRUE);

    CPlotRawData::OnPaint();
}


