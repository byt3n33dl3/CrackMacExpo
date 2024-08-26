#ifndef _METERPRETER_SOURCE_EXTENSION_STDAPI_STDAPI_SERVER_FS_SEARCH_H
#define _METERPRETER_SOURCE_EXTENSION_STDAPI_STDAPI_SERVER_FS_SEARCH_H

#include <shlwapi.h>
#include <windows.h>
#include <searchapi.h>
#include <msdasc.h>
#include <ntquery.h>
#include <cmdtree.h>

// from query.dll for WDS 2 index based searching
typedef HRESULT (WINAPI * LOCATECATALOGSW)(wchar_t *, ULONG, wchar_t *,
    ULONG *, wchar_t *, ULONG *);
typedef HRESULT (WINAPI * CIMAKEICOMMAND)(ICommand **, ULONG, DWORD *,
     wchar_t **, wchar_t **, wchar_t **);
typedef HRESULT (WINAPI * CITEXTTOFULLTREE)(wchar_t *, wchar_t *, wchar_t *,
    wchar_t *, DBCOMMANDTREE **, ULONG , LPVOID *, LCID);

typedef struct _WDS_INTERFACE
{
	BOOL bWDS2Available;
	BOOL bWDS3Available;

	// WDS 3...
	ISearchManager * pSearchManager;
	ISearchCatalogManager * pSearchCatalogManager;
	ISearchCrawlScopeManager * pCrawlScopeManager;

	// WDS 2...
	HMODULE hQuery;
	LOCATECATALOGSW pLocateCatalogsW;
	CIMAKEICOMMAND pCIMakeICommand;
	CITEXTTOFULLTREE pCITextToFullTree;

} WDS_INTERFACE;


#define FS_SEARCH_NO_DATE UINT_MAX

typedef struct _SEARCH_OPTIONS
{
	wchar_t *glob;
	wchar_t *rootDirectory;
	BOOL bResursive;
	UINT uiStartDate;
	UINT uiEndDate;
} SEARCH_OPTIONS;

// sf: The padding DWORD's ensure we dont get an IAccessor_CreateAccessor error due to alignment on x64.
typedef struct _SEARCH_ROW
{
	DBSTATUS dbSizeStatus;
	DWORD dwPadding1;
	DWORD dwSizeLength;
	DWORD dwPadding2;
	DWORD dwSizeValue;
	DWORD dwPadding3;
	DBSTATUS dbPathStatus;
	DWORD dwPadding4;
	DWORD dwPathLength;
	DWORD dwPadding5;
	wchar_t wPathValue[MAX_PATH];
	DBSTATUS dbDateStatus;
	DWORD dwPadding6;
	DWORD dwDateLength;
	DWORD dwPadding7;
	FILETIME wDateValue;
	DWORD dwPadding8;
} SEARCH_ROW;

// we manually define these ourselves...
const GUID _CLSID_CSearchManager = { 0x7D096C5F, 0xAC08, 0x4F1F, { 0xBE, 0xB7, 0x5C, 0x22, 0xC5, 0x17, 0xCE, 0x39 } };
const GUID _CLSID_MSDAInitialize = { 0x2206CDB0, 0x19C1, 0x11D1, { 0x89, 0xE0, 0x00, 0xC0, 0x4F, 0xD7, 0xA8, 0x29 } };
const GUID _IID_ISearchManager   = { 0xAB310581, 0xAC80, 0x11D1, { 0x8D, 0xF3, 0x00, 0xC0, 0x4F, 0xB6, 0xEF, 0x69 } };
const GUID _IID_IDataInitialize  = { 0x2206CCB1, 0x19C1, 0x11D1, { 0x89, 0xE0, 0x00, 0xC0, 0x4F, 0xD7, 0xA8, 0x29 } };
const GUID _IID_IDBInitialize    = { 0x0c733a8b, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };
const GUID _IID_IDBCreateSession = { 0x0c733a5d, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };
const GUID _IID_IOpenRowset      = { 0x0c733a69, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };
const GUID _IID_IDBCreateCommand = { 0x0c733a1d, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };
const GUID _IID_ICommand         = { 0x0c733a63, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };
const GUID _IID_ICommandText     = { 0x0c733a27, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };
const GUID _IID_IRowset          = { 0x0c733a7c, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };
const GUID _IID_IMultipleResults = { 0x0c733a90, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };
const GUID _IID_IAccessor        = { 0x0c733a8c, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };
const GUID _IID_IColumnsInfo     = { 0x0c733a11, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };
const GUID _IID_ICommandTree     = { 0x0c733a87, 0x2a1c, 0x11ce, { 0xad, 0xe5, 0x00, 0xaa, 0x00, 0x44, 0x77, 0x3d } };

#endif
