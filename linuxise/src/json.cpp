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



#include "rapidjson/document.h"
#include "rapidjson/writer.h"
#include "rapidjson/stringbuffer.h"
#include "rapidjson/error/en.h"
#include "json.h"
#include <codecvt>
#include <iomanip>
#include <locale>
#include <iostream>         // DELETE THIS

// Helper Functions
std::string wstring_to_utf8(const std::wstring& wideStr) {
    std::wstring_convert<std::codecvt_utf8<wchar_t>> converter;
    return converter.to_bytes(wideStr);
}
std::wstring utf8_to_wstring(const std::string& str)
{
    std::wstring_convert<std::codecvt_utf8<wchar_t>> myconv;
    return myconv.from_bytes(str);
}

// Json API's
std::wstring to_json(const std::vector<std::wstring>& data) {
    rapidjson::Document document;
    rapidjson::Value jsonValue(rapidjson::kObjectType);

    if (data.size() % 2 == 0) {
        for (size_t i = 0; i < data.size(); i += 2) {
            const std::wstring& key = data[i];
            const std::wstring& value = data[i + 1];

            rapidjson::Value utf8Key(wstring_to_utf8(key).c_str(), document.GetAllocator());
            rapidjson::Value utf8Value(wstring_to_utf8(value).c_str(), document.GetAllocator());

            jsonValue.AddMember(utf8Key.Move(), utf8Value.Move(), document.GetAllocator());
        }
    }

    // Convert the JSON object to a string
    rapidjson::StringBuffer buffer;
    rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
    jsonValue.Accept(writer);

    // Convert UTF-8 encoded JSON string to std::wstring
    return utf8_to_wstring(buffer.GetString());
}

std::vector<std::wstring> from_json(const std::wstring& jsonData) {
    // Convert the input jsonData (std::wstring) to UTF-8 encoded std::string
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    std::string utf8jsonData = converter.to_bytes(jsonData);

    rapidjson::Document document;
    document.Parse(utf8jsonData.c_str());

    if (document.HasParseError()) {
        // Handle parse error if needed
        return {};
    }

    std::vector<std::wstring> parsedData;

    if (document.IsObject()) {
        for (auto it = document.MemberBegin(); it != document.MemberEnd(); ++it) {
            const std::wstring key = utf8_to_wstring(it->name.GetString());
            const std::wstring value = utf8_to_wstring(it->value.GetString());

            parsedData.push_back(key);
            parsedData.push_back(value);
        }
    }

    return parsedData;
}

std::wstring json_ExtractValue(const std::wstring& jsonData, const std::wstring& key) {
    // Convert the input jsonData and key to UTF-8 encoded std::string
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    std::string utf8jsonData = converter.to_bytes(jsonData);
    std::string utf8key = converter.to_bytes(key);

    rapidjson::Document document;
    document.Parse(utf8jsonData.c_str());

    if (!document.IsObject()) {
        // Handle error if needed
        return L"";
    }

    if (!document.HasMember(utf8key.c_str())) {
        // Handle error if needed
        return L"";
    }

    const rapidjson::Value& value = document[utf8key.c_str()];

    if (!value.IsString()) {
        // Handle error if needed
        return L"";
    }

    std::string utf8Value = value.GetString();
    std::wstring wstringValue = utf8_to_wstring(utf8Value);

    return wstringValue;
}

std::wstring json_AppendKeyValue(const std::wstring& jsonData, const std::wstring& key, const std::wstring& value) {

    // Convert the input jsonData, key, and value to UTF-8 encoded std::string
    std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
    std::string utf8jsonData = converter.to_bytes(jsonData);
    std::string utf8key = converter.to_bytes(key);
    std::string utf8value = converter.to_bytes(value);      // THIS TROWS ON ls ~/Desktop/testing
    
    rapidjson::Document document;
    document.Parse(utf8jsonData.c_str());

    if (!document.IsObject()) {
        // Handle error if needed
        return jsonData;
    }

    rapidjson::Value jsonValue(utf8value.c_str(), document.GetAllocator());

    rapidjson::Document::AllocatorType& allocator = document.GetAllocator();
    rapidjson::Value jsonKey(utf8key.c_str(), allocator);
    document.AddMember(jsonKey, jsonValue, allocator);

    rapidjson::StringBuffer buffer;
    rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
    document.Accept(writer);

    // Convert the UTF-8 encoded JSON string to std::wstring
    std::string utf8Result = buffer.GetString();
    std::wstring wResult = utf8_to_wstring(utf8Result);

    return wResult;
}
