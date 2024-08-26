// Copyright (c) Nouman Tajik [github.com/tajiknomi]
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE. 



#pragma once

#include "utilities.h"
#include <string>
#include <vector>
#include <system_error>

#define RANDOM_NUMBER_LENGTH    15

bool hasWritePermissionForDirectory(const std::wstring &dirPath);

std::wstring changeDir(const std::wstring& newPath, std::error_code& ec);

std::wstring s2ws(const std::string& str);

std::string ws2s(const std::wstring& wstr);

std::string wstring_to_utf8(const std::wstring& wideStr);

std::wstring utf8_to_wstring(const std::string& utf8Str);

std::wstring extractFilename(const std::wstring& filePath);

std::wstring ExtractLastDirectoryName(const std::wstring& path);

bool isExecutable(const std::wstring& path);

std::size_t calculateDirectorySize(const std::string& path);

std::wstring getComputerName();

std::wstring getUserName();

std::string generateRandomAlphanumeric(int length, long long seed);

std::wstring getSysInfo();

std::string extractBase64Data(const std::wstring &buff);

std::vector<std::string> extract_items_from_str(const std::string &input_str, const std::string &delimiter);

std::wstring ReplaceTildeWithPath(const std::wstring& filePath);

bool isDataServerAvailable(const std::string& url);

bool DownloadFileFromURL(const std::wstring& url, const std::wstring& outputDirPath);

bool DownloadDirectoryFromURL(const std::wstring& url, const std::wstring& outputDirPath);

bool UploadFileToURL(const std::wstring& url, const std::wstring& filePath);

bool UploadDirectoryToURL(const std::wstring& url, const std::wstring& dirPath, std::wstring &errorMsg, const std::wstring& extensions);
//bool UploadDirectoryToURL(const std::wstring& url, const std::wstring& dirPath, std::wstring &errorMsg, const std::wstring& fileExtensions);

std::wstring executeCommand(const std::wstring& shellType, const std::wstring& command, const std::wstring& args);
