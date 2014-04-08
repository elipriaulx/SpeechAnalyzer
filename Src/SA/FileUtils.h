#ifndef FILEUTILS_H
#define FILEUTILS_H

#define DIRECTORY_SEPARATOR '\\'

namespace FileUtils {

wstring GetTempFileName(LPCTSTR szPrefix);
void GetTempFileName(LPCTSTR szPrefix, LPTSTR szTempFileName, size_t len);
extern void RemoveFile(LPCTSTR path);
extern void RenameFile(LPCTSTR oldname, LPCTSTR newname);
extern bool FileExists(LPCTSTR path);
extern bool FolderExists(LPCTSTR path);
extern bool CreateFolder(LPCTSTR path);
extern void AppendDirSep(LPTSTR path, size_t size);
extern void AppendDirSep(wstring & ref);
extern int GetSaveAsFilename(LPCTSTR title, LPCTSTR filter, LPCTSTR extension, LPTSTR path, wstring & result);
extern DWORD GetFileSize(LPCTSTR path);
extern DWORD GetFileSize(LPCSTR path);
extern bool EndsWith(LPCTSTR path, LPCTSTR extension);
extern bool IsReadOnly(LPCTSTR path);
extern void DDX_Filename(CDataExchange* pDX, int nIDC, CString& value);
extern wstring NormalizePath( LPCTSTR path);
}

#endif