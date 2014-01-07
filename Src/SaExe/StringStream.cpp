/////////////////////////////////////////////////////////////////////////////
// stringstream.cpp
// Implementation of textual iostreams of objects
// copyright 2014 JAARS Inc. SIL
/////////////////////////////////////////////////////////////////////////////
#include "Stdafx.h"
#include "StringStream.h"
#include <windows.h>
#include <fstream>
#include <string.h>
#include <assert.h>
#include "defs.h"

using std::ofstream;
using std::ifstream;
using std::ios;

// ==========================================================================
CStringStream::CStringStream(LPCTSTR data) :
m_ios()
{
	m_ios.str(data);
    m_pszMStringBuf = new wchar_t[maxsizMString];
    m_pszEnd = NULL;
    m_pszMarker = NULL;
    m_pszString = NULL;
    m_bUnRead = FALSE;
    m_chEndOfLine = '\n';  // Default for Mac and Windows
}

CStringStream::~CStringStream()
{
    delete [] m_pszMStringBuf;
}

size_t CStringStream::GetBufferSize()
{
	return maxsizMString;
}

bool CStringStream::bAtEnd()
{
    // MRP: TO DO!!! Check whether the following (and bReadField) is right...
    // 1. If marker or contents at EOF with no newline;
    // 2. If explicit Ctrl-Z (code 26) in file.

    // Have read to end if no "unread" field, and all chars have been read
    // from the stream. NOTE: Because no attempt has yet been made actually
    // to get chars from beyond the end of the stream, eof() will NOT be true.
    return ((!m_bUnRead) && (m_ios.peek() == std::char_traits<wchar_t>::eof()));
}

bool CStringStream::bAtBackslash()
{
    // Stream represents Standard Format if a field has already been read
    // successfully, or if the first char of the stream is a backslash.
    return ((m_bUnRead) || (m_ios.peek() == '\\'));
}

bool CStringStream::bAtBeginMarker()
{

    LPCTSTR pszMarkerRead = NULL;
    LPCTSTR pszStringRead = NULL;
    ReadMarkedString(&pszMarkerRead, &pszStringRead);
    bool bBeginMarker = (*pszMarkerRead == '+');
    UnReadMarkedString();
    return bBeginMarker;
}

bool CStringStream::bAtBeginOrEndMarker()
{
    LPCTSTR pszMarkerRead = NULL;
    LPCTSTR pszStringRead = NULL;
    ReadMarkedString(&pszMarkerRead, &pszStringRead);

    bool bBeginOrEndMarker = (*pszMarkerRead == '+' || *pszMarkerRead == '-');
    UnReadMarkedString();

    return bBeginOrEndMarker;
}

bool CStringStream::bAtBeginMarker(LPCTSTR pszMarker, LPCTSTR pszName)
{

    LPCTSTR pszMarkerRead = NULL;
    LPCTSTR pszStringRead = NULL;
    if (!bReadBeginMarker(pszMarker))
    {
        return FALSE;
    }
    UnReadMarkedString();
    ReadMarkedString(&pszMarkerRead, &pszStringRead);
    bool b = ((!pszName) || (wcscmp(pszName, pszStringRead)==0));   // If name given, it must match
    UnReadMarkedString();
    return b;
}

bool CStringStream::bAtEndMarker(LPCTSTR pszMarker)
{
    if (!bReadEndMarker(pszMarker))
    {
        return false;
    }

    UnReadMarkedString();
    return true;
}

bool CStringStream::bReadBeginMarker(LPCTSTR pszMarker, LPTSTR psName, size_t size)
{

    LPCTSTR pszMarkerRead = NULL;
    LPCTSTR pszStringRead = NULL;
    ReadMarkedString( &pszMarkerRead, &pszStringRead);

    BOOL bBeginMarker = ((*pszMarkerRead == '+') && (wcscmp(pszMarker, pszMarkerRead+1)==0));
    if (!bBeginMarker)
    {
        UnReadMarkedString();
        return FALSE;
    }

    memset(psName,0,size);
    wcscpy_s(psName, size, pszStringRead);
    return TRUE;
}

bool CStringStream::bReadMarker(wchar_t cFirstChar, LPCTSTR pszMarker)
{
    LPCTSTR pszMarkerRead = NULL;
    LPCTSTR pszStringRead = NULL;
    ReadMarkedString(&pszMarkerRead, &pszStringRead);

    BOOL bMarkerFound = (*pszMarkerRead == cFirstChar) && (wcscmp(pszMarker, pszMarkerRead+1)==0);
    if (!bMarkerFound)
    {
        UnReadMarkedString();
        return FALSE;
    }

    return TRUE;
}

#define MAXMKRSIZE 50

bool CStringStream::bReadBeginMarkerWithQualifier(LPCTSTR pszMarker, LPCTSTR pszQualifier)
{
    if (!pszQualifier)
    {
        return bReadBeginMarker(pszMarker);
    }

    wchar_t buf[MAXMKRSIZE];
    assert(wcslen(pszMarker) + wcslen(pszQualifier) + 1 < MAXMKRSIZE);
    wcscpy_s(buf, _countof(buf), pszMarker);
    wcscat_s(buf, _countof(buf), pszQualifier);
    return bReadMarker('+', buf);
}

bool CStringStream::bReadEndMarkerWithQualifier(LPCTSTR pszMarker, LPCTSTR pszQualifier)
{
    if (!pszQualifier)
    {
        return bReadEndMarker(pszMarker);
    }

    wchar_t buf[MAXMKRSIZE];
    assert(wcslen(pszMarker) + wcslen(pszQualifier) + 1 < MAXMKRSIZE);
    wcscpy_s(buf, _countof(buf), pszMarker);
    wcscat_s(buf, _countof(buf), pszQualifier);
    return bReadMarker('-', buf);
}

bool CStringStream::bReadString( LPCTSTR pszMarker, LPTSTR szResult, size_t size)
{
    LPCTSTR read = NULL;
    bool result = bReadString(pszMarker, &read);
    if (result)
    {
        wcscpy_s(szResult, size, read);
    }
    return result;
}

bool CStringStream::bReadString(LPCTSTR pszMarker, LPCTSTR * s)
{
    LPCTSTR pszMarkerRead = NULL;
    LPCTSTR pszStringRead = NULL;
    ReadMarkedString(&pszMarkerRead, &pszStringRead);

    if (wcscmp(pszMarker, pszMarkerRead)!=0)
    {
        UnReadMarkedString();
        return false;
    }

    *s = pszStringRead;
    return true;
}

bool CStringStream::bReadBool(LPCTSTR pszMarker, bool & b)
{
    LPCTSTR s = NULL;
    if (!bReadString(pszMarker, &s))
    {
        return FALSE;
    }

    b = (_wtof(s) != 0);
    return TRUE;
}

bool CStringStream::bReadCOLORREF(LPCTSTR pszMarker, COLORREF & rgb)
{
    LPCTSTR s = NULL;
    if (!bReadString(pszMarker, &s))
    {
        return FALSE;
    }

    short int r = 0;
    short int g = 0;
    short int b = 0;
    // 1997-05-17 MRP: sscanf not defined in DLLs -- reimplement
    if (swscanf_s( s, L"%hd,%hd,%hd", &r, &g, &b) == 0)
    {
        return FALSE;
    }

    rgb = RGB(r,g,b);
    return TRUE;
}

bool CStringStream::bReadInteger(LPCTSTR pszMarker, int & i)
{
    LPCTSTR s = NULL;
    if (!bReadString(pszMarker, &s))
    {
        return FALSE;
    }

    i = _wtoi(s);
    return TRUE;
}

bool CStringStream::bReadDouble(LPCTSTR pszMarker, double & i)
{
    LPCTSTR s = NULL;
    if (!bReadString(pszMarker, &s))
    {
        return false;
    }

    i = _wtof(s);
    return true;
}

bool CStringStream::bReadNumberOf(LPCTSTR pszMarker, unsigned long & num)
{
    LPCTSTR s = NULL;
    if (!bReadString(pszMarker, &s))
    {
        return FALSE;
    }

    if (swscanf_s(s, L"%lu", &num) == 0)
    {
        return FALSE;
    }

    return TRUE;
}

void CStringStream::ReadMarkedString()   // Generic eat of one marker and its string
{
    LPCTSTR pszMarkerRead = NULL;
    LPCTSTR pszStringRead = NULL;
    ReadMarkedString(&pszMarkerRead, &pszStringRead);
}

// Read given end marker, stop any begin marker, for error recovery on objects with unrecognized names AB 1-18-95
bool CStringStream::bEnd(LPCTSTR pszMarker)
{
    if (bReadEndMarker(pszMarker))     // If desired end marker, return true for successfully found end
    {
        return TRUE;
    }
    else if (bAtBeginMarker())     // If somebody else's begin marker, return true for found end
    {
        return TRUE;
    }
    else     // Else, unrecognized marker, eat marker and return false for not at end yet
    {
        LPCTSTR pszMarkerRead = NULL;
        LPCTSTR pszStringRead = NULL;
        ReadMarkedString(&pszMarkerRead, &pszStringRead);
        return FALSE;
    }
}

bool CStringStream::bEndWithQualifier(LPCTSTR pszMarker, LPCTSTR pszQualifier)
{
    if (!pszQualifier)
    {
        return bEnd(pszMarker);
    }

    wchar_t buf[MAXMKRSIZE];
    assert(wcslen(pszMarker) +wcslen(pszQualifier) + 1 < MAXMKRSIZE);
    wcscpy_s(buf, _countof(buf), pszMarker);
    wcscat_s(buf, _countof(buf), pszQualifier);
    return bEnd(buf);
}

#ifndef NO_INTERFACE
bool CStringStream::bReadWindowPlacement(LPCTSTR pszMarker, WINDOWPLACEMENT & wpl)
{
    LPCTSTR sPlacement = NULL;
    if (!bReadString(pszMarker, &sPlacement))
    {
        return FALSE;
    }

    wpl.length = sizeof(wpl);
    wpl.flags = WPF_SETMINPOSITION;
    wchar_t pszShowState[7];
    int nItems = swscanf_s( sPlacement, L"%s %d %d %d %d %d %d %d %d",pszShowState, _countof(pszShowState), &wpl.ptMinPosition.x,
                          &wpl.ptMinPosition.y,
                          &wpl.ptMaxPosition.x,
                          &wpl.ptMaxPosition.y,
                          &wpl.rcNormalPosition.left,
                          &wpl.rcNormalPosition.top,
                          &wpl.rcNormalPosition.right,
                          &wpl.rcNormalPosition.bottom);

    if (nItems != 9)
    {
        return FALSE;
    }

    if (wcscmp(pszShowState, L"normal")==0)
    {
        wpl.showCmd = SW_SHOWNORMAL;
    }
    else if (wcscmp(pszShowState, L"min")==0)
    {
        wpl.showCmd = SW_SHOWMINIMIZED;
    }
    else if (wcscmp(pszShowState, L"max")==0)
    {
        wpl.showCmd = SW_SHOWMAXIMIZED;
    }
    else
    {
        wpl.showCmd = SW_SHOWNORMAL;
    };

    return TRUE;
}
#endif  // not NO_INTERFACE

void CStringStream::ReadMarkedString( LPCTSTR * ppszMarker, LPCTSTR * ppszString, bool bTrimWhiteSpace)
{
    if (m_bUnRead)
    {
        *ppszMarker = m_pszMarker;
        *ppszString = m_pszString;
        m_bUnRead = FALSE;
        return;
    }

    ReadMarkedLine(ppszMarker, ppszString);

    // Read any additional lines of the marked string
    int ich;
    while (((ich = m_ios.peek()) != std::char_traits<wchar_t>::eof()) && (ich != '\\'))
    {
        *m_pszEnd++ = '\n';
        ReadLine();
    }

    if (bTrimWhiteSpace)
    {
        // 1995-10-02 MRP: Clean this up and separate out for use of
        // settings properties but *not* Standard Format data fields.
        // Trim white space from the beginning and end of the string
        for (; Shw_bAtWhiteSpace(m_pszString); m_pszString += 1)
        {
            if (*m_pszString == 0)
            {
                break;
            }
        }
        if (*m_pszString)    // string contains non-ws
        {
            wchar_t * psz = m_pszString + wcslen(m_pszString) - 1; // last char
            for (; Shw_bAtWhiteSpace(psz); psz -= 1)
            {
                assert(psz != m_pszString);    // have not moved past beginning
                *psz = '\0';  // remove last char
            }
        }
    }

    *ppszString = m_pszString;
}

void CStringStream::UnReadMarkedString()
{
    assert(!m_bUnRead);
    m_bUnRead = TRUE;
}


void CStringStream::ReadMarkedLine(LPCTSTR * ppszMarker, LPCTSTR * ppszString)
{
    assert(!m_bUnRead);
    LPTSTR psz = m_pszEnd = m_pszMStringBuf;
    *psz = '\0';
    ReadLine();

    // Caller is responsible to not to read to end, and also to check that
    // the file is indeed Standard Format (i.e. it begins with a backslash)
    assert(*psz == '\\');
    psz += 1; // move past the backslash

    m_pszMarker = psz;
    Length lenMarker = wcscspn(psz, L" \t\n");
    // 1996-11-04 MRP: This temporary patch will skip the rest of a field
    // that contains a backslash at the beginning of one of its lines.
    // The real fix is to use the read-line-ahead approach in sfstream.cpp
    // ASSERT( lenMarker != 0 ); // RECOVERY
    psz += lenMarker;

    if (*psz == ' ')
    {
        // A space delimiting the marker is not considered part of the string
        *psz = '\0'; // so the marker's null termination can take its place
        psz += 1;
    }
    else
    {
        // 1995-10-04 MRP: At first I thought this shifting was redundant
        // if no white space followed the marker. If the field consisted
        // of just that one line that would be true. If, however, the
        // field's content follows in additional lines we must do it.
        //
        // Move the marker left one position (covering the backslash)
        m_pszMarker = m_pszMStringBuf;
        memcpy(m_pszMarker, m_pszMStringBuf + 1, lenMarker);
        assert(m_pszMarker + lenMarker == psz - 1);
        m_pszMarker[lenMarker] = '\0'; // making a place for its null
    }

    *ppszMarker = m_pszMarker; // beginning of marker
    *ppszString = m_pszString = psz; // beginning of string
}

void CStringStream::ReadLine()
{
    assert(!m_bUnRead);
    // 1995-04-12 MRP: use get in order to detect line longer than buffer
    Length sizRemaining = maxsizMString - (m_pszEnd - m_pszMStringBuf);
    m_ios.get(m_pszEnd, sizRemaining, m_chEndOfLine);
    m_ios.clear(); // if get encounters an empty line it will set fail bit, must be cleared
    size_t lenLine = wcslen(m_pszEnd);
    m_pszEnd += lenLine;
    int ichNext = m_ios.get();
    if ((ichNext != m_chEndOfLine) && 
		(ichNext != std::char_traits<wchar_t>::eof()))
    {
        assert(FALSE);    // at end of input buffer-- RECOVERY
    }
}

// **************************************************************************

bool CStringStream::bReadUInt(LPCTSTR pszMarker, UINT & ui)   // Read an unsigned integer (unsigned short)
{
    LPCTSTR s;
    if (!bReadString(pszMarker, &s))
    {
        return false;
    }

    if (swscanf_s(s, L"%hu", &ui) == 0)
    {
        return false;
    }

    return true;
}

void CStringStream::SkipToEndMarker(LPCTSTR pszMarker)
{
    while (!bAtEnd())
    {
        if (bReadEndMarker(pszMarker))
        {
            break;
        }
        ReadMarkedString();  // Skip unexpected field
    }
}

void CStringStream::Rewind()
{
	m_ios.clear();
    m_ios.seekg(0);
	m_ios.clear();
    // reset all internal data
    m_pszMStringBuf = new wchar_t[maxsizMString];
    m_pszEnd = NULL;
    m_pszMarker = NULL;
    m_pszString = NULL;
    m_bUnRead = FALSE;
    m_chEndOfLine = '\n';  // Default for Mac and Windows
}

bool CStringStream::bReadBeginMarker(LPCTSTR pszMarker)
{
    // Read the desired begin marker (pszMarker with a plus sign)
    return bReadMarker('+', pszMarker);
}

bool CStringStream::bReadEndMarker(LPCTSTR pszMarker)
{
    // Read the desired end marker (pszMarker with a minus sign)
    return bReadMarker('-', pszMarker);
}

bool CStringStream::bReadDWord(LPCTSTR pszMarker, DWORD & dw)
{
    // Read a DWORD (long unsigned integer)
    return bReadNumberOf(pszMarker, dw);
}

void CStringStream::PeekMarkedString( LPCTSTR * ppszMarker, LPTSTR pszString, size_t len, bool bTrimWhiteSpace)
{
    LPCTSTR read = 0;
    peekMarkedString( ppszMarker, &read, bTrimWhiteSpace);
    wcscpy_s( pszString, len, read);
}

void CStringStream::peekMarkedString( LPCTSTR * ppszMarker, LPCTSTR * ppszString, bool bTrimWhiteSpace)
{
    // Read any marked string
    ReadMarkedString( ppszMarker, ppszString, bTrimWhiteSpace);
    // Store most recently read marked string for next read
    UnReadMarkedString();
}

bool CStringStream::bFail() 
{
	return m_ios.fail();
}

void CStringStream::GetLine( LPTSTR buffer, size_t size) 
{
  m_ios.getline( buffer, size);
}

int CStringStream::Peek()
{
	return m_ios.peek();
}

streampos CStringStream::TellG()
{
	return m_ios.tellg();
}

void CStringStream::Clear()
{
	return m_ios.clear();
}

void CStringStream::SeekG( streampos pos)
{
	m_ios.seekg(pos);
}

bool CStringStream::Shw_bAtWhiteSpace( LPCTSTR psz)
{
    return (strchr(" \t\n", *psz) != NULL);
}

bool CStringStream::ReadStreamString( LPCTSTR pszMarker, CSaString & szResult)
{
	size_t size = GetBufferSize()+1;
	LPTSTR buffer = new wchar_t[size];
	try
	{
		memset( buffer, 0, size);
		bool result = bReadString (pszMarker, buffer, size);
		if (result)
		{
			szResult = buffer;
		}
		delete [] buffer;
		buffer = NULL;
	    return result;
	}
	catch(...)
	{
		if (buffer!=NULL)
		{
			delete [] buffer;
			buffer = NULL;
		}
	}
	return false;
}
