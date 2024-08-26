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



#include "http.h"
#include <iostream>
#include <arpa/inet.h>
#include <netdb.h>
#include <sys/socket.h>
#include <unistd.h>
#include "utilities.h"
#include "json.h"


std::wstring httpPost(const std::wstring &hostname, const std::wstring &request) {
    struct sockaddr_in sockaddr_in;
    unsigned short server_port = 80;

    /* Build the socket. */
    struct protoent* protoent = getprotobyname("tcp");
    if (protoent == NULL) {
        perror("getprotobyname");
        exit(-1);
    }
    int socket_file_descriptor = socket(AF_INET, SOCK_STREAM, protoent->p_proto);
    if (socket_file_descriptor == -1) {
        perror("socket");
        exit(-1);
    }

    /* Build the address. */
    struct hostent *hostent = gethostbyname(ws2s(hostname).c_str());
    if (hostent == NULL) {
        std::wcerr << "error: gethostbyname(\"" << hostname << "\")" << std::endl;
        exit(-1);
    }
    in_addr_t in_addr = inet_addr(inet_ntoa(*(struct in_addr*)*(hostent->h_addr_list)));
    if (in_addr == (in_addr_t)-1) {
        std::cerr << "error: inet_addr(\"" << *(hostent->h_addr_list) << "\")" << std::endl;
        exit(-1);
    }
    sockaddr_in.sin_addr.s_addr = in_addr;
    sockaddr_in.sin_family = AF_INET;
    sockaddr_in.sin_port = htons(server_port);

    /* Actually connect. */
    if (connect(socket_file_descriptor, (struct sockaddr*)&sockaddr_in, sizeof(sockaddr_in)) == -1) {
        //perror("connect");
        close(socket_file_descriptor);
        return std::wstring();
    }

    ssize_t nbytes_total;
    ssize_t nbytes_last;
    size_t request_len = request.length();

    /* Send HTTP request. */
    nbytes_total = 0;
    // while (nbytes_total < request_len) {
    //     nbytes_last = write(socket_file_descriptor, &request[nbytes_total], request_len - nbytes_total);
    //     if (nbytes_last == -1) {
    //         perror("write");
    //         close(socket_file_descriptor);
    //         return std::wstring();
    //     }
    //     nbytes_total += nbytes_last;
    // }

    while (nbytes_total < request_len) {            // send data to server
    std::wstring wideChunk = request.substr(nbytes_total);
    std::string narrowChunk = ws2s(wideChunk);
    size_t narrowBytesToSend = narrowChunk.length();  
    int nbytes_last = write(socket_file_descriptor, narrowChunk.c_str(), static_cast<int>(narrowBytesToSend));
    if (nbytes_last == -1) {
      //  std::cerr << "send failed with error: " << WSAGetLastErrorFunc() << std::endl;           
        close(socket_file_descriptor);
     //   cleanupFunc();
     //   FreeLibrary(hWS2_32);
        return std::wstring();
    }
        nbytes_total += nbytes_last; // Increment by the number of bytes sent
    }

    /* Read the response with timeout. */
    char readBuff[READ_BUFFER_SIZE] = {};
    ssize_t nbytes=0;

    fd_set read_fds;
    FD_ZERO(&read_fds);
    FD_SET(socket_file_descriptor, &read_fds);

    struct timeval timeout;
    timeout.tv_sec = TIMEOUT_SEC; 
    timeout.tv_usec = 0;

    std::string tmpDataRead;
    int select_result = select(socket_file_descriptor + 1, &read_fds, NULL, NULL, &timeout);
    if (select_result == -1) {
        perror("select");
        close(socket_file_descriptor);
        return std::wstring();
    }
    else if (select_result == 0) {
        // Timeout occurred, no data received within 2 seconds
        //std::cout << "Timeout occurred. No data received within 2 seconds." << std::endl;
        close(socket_file_descriptor);
        return std::wstring();
    }
    if (FD_ISSET(socket_file_descriptor, &read_fds)) {
        while ((nbytes = read(socket_file_descriptor, readBuff, READ_BUFFER_SIZE)) > 0) {
            // Process the received data
            //write(STDOUT_FILENO, readBuff, nbytes);
            readBuff[nbytes] = 0x00;
            tmpDataRead += readBuff;
        }
    }
    if (nbytes == -1) {
        perror("read");
        close(socket_file_descriptor);
        return std::wstring();
    }
    std::wstring readBuffStr(s2ws(tmpDataRead));
    size_t found = readBuffStr.find(L"\r\n\r\n");
    std::wstring decodedData;
    if (found != std::string::npos){
        std::string b64Data = extractBase64Data(readBuffStr.substr(found));
        decodedData = s2ws(base64_decode(b64Data.c_str()));
    }
    close(socket_file_descriptor);
    return decodedData;
}