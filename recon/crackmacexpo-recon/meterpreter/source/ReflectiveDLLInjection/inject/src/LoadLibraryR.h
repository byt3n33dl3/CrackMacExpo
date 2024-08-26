//===============================================================================================//
// Copyright (c) 2013, Stephen Fewer of Harmony Security (www.harmonysecurity.com)
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted 
// provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice, this list of 
// conditions and the following disclaimer.
// 
//     * Redistributions in binary form must reproduce the above copyright notice, this list of 
// conditions and the following disclaimer in the documentation and/or other materials provided 
// with the distribution.
// 
//     * Neither the name of Harmony Security nor the names of its contributors may be used to
// endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR 
// OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
// POSSIBILITY OF SUCH DAMAGE.
//===============================================================================================//

// C5045 warning was introduced in Visual Studio 2017 version 15.7
// See https://devblogs.microsoft.com/cppblog/spectre-mitigations-in-msvc/
// See https://learn.microsoft.com/en-us/cpp/preprocessor/predefined-macros?view=msvc-170
#if _MSC_VER >= 1914
#pragma warning(disable: 5045) // warning C5045: Compiler will insert Spectre mitigation for memory load if /Qspectre switch specified
#endif
#pragma warning(disable: 4820) // warning C4820: X bytes padding added after construct Y
#pragma warning(disable: 4710) // warning C4710: The specified function was marked for inline expansion, but the compiler didn't inline the function.

#ifndef _REFLECTIVEDLLINJECTION_LOADLIBRARYR_H
#define _REFLECTIVEDLLINJECTION_LOADLIBRARYR_H
//===============================================================================================//
#include "ReflectiveDLLInjection.h"

DWORD GetReflectiveLoaderOffset(VOID* lpReflectiveDllBuffer, LPCSTR cpReflectiveLoaderName);
HMODULE WINAPI LoadLibraryR(LPVOID lpBuffer, DWORD dwLength, LPCSTR cpReflectiveLoaderName);
HANDLE WINAPI LoadRemoteLibraryR(HANDLE hProcess, LPVOID lpBuffer, DWORD dwLength, LPCSTR cpReflectiveLoaderName, LPVOID lpParameter);

//===============================================================================================//
#endif
//===============================================================================================//
