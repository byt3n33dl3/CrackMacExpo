#pragma once

//#ifdef  MYDLL_EXPORTS 
///*Enabled as "export" while compiling the dll project*/
//#define DLLEXPORT __declspec(dllexport)  
//#else
///*Enabled as "import" in the Client side for using already created dll file*/
//#define DLLEXPORT __declspec(dllimport)  
//#endif

#include <string>

/* =================================== Public API's =================================== */
#ifdef __cplusplus    // If used by C++ code,

std::string base64_encode(unsigned char const*, unsigned int len);
std::string base64_decode(std::string const& s);
//std::string encodeBase64(const std::string& data, size_t size);
//std::string decodeBase64(const std::string& data, size_t size);

#endif
