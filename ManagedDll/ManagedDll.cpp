// ManagedDll.cpp: 定义 DLL 应用程序的导出函数。
//
#include "stdafx.h"
#include "ManagedDll.h"

#pragma region Using Directives
using namespace System;
using namespace libutilscore::FTP;
using namespace libutilscore::IO;
using namespace libutilscore::Core;
using namespace libutilscore::Net;
#pragma endregion

#pragma region Helper Functions

/* Convert System::String to standard string */
static void MarshallString(String ^csstr, std::string &stdstr)
{
	using namespace Runtime::InteropServices;
	const char *chars = (const char*)(Marshal::StringToHGlobalAnsi(csstr)).ToPointer();
	stdstr = chars;
	Marshal::FreeHGlobal(IntPtr((void *)chars));
}

static void MarshallWstring(String ^csstr, std::wstring &wstr)
{
	using namespace Runtime::InteropServices;
	const wchar_t *wchars = (const wchar_t*)(Marshal::StringToHGlobalAnsi(csstr)).ToPointer();
	wstr = wchars;
	Marshal::FreeHGlobal(IntPtr((void *)wchars));
}

static String ^ ToSystemString(std::string &stdstr)
{
	return gcnew System::String(stdstr.c_str());
}

static String ^ ToSystemString(std::wstring &stdwstr)
{
	return gcnew System::String(stdwstr.c_str());
}

static ExecResult getResult(Tuple<bool, String^> ^ret)
{
	ExecResult result;
	result.isSuccess = ret->Item1;
	MarshallString(ret->Item2, result.message);
	return result;
}
#pragma endregion

#pragma region ManagedFTP Class

class ManagedFTP
{
private:
	
public:
	static std::string ShowHello()
	{
		String ^ message = SharpFTP::ShowHello();
		std::string result;
		MarshallString(message, result);
		return result;
	}

	static ExecResult SetFtpInfo(std::string host, std::string user,
		std::string passwd, std::string remotePath)
	{
		return getResult(SharpFTP
			::SetFtpInfo(ToSystemString(host), ToSystemString(user),
				ToSystemString(passwd), ToSystemString(remotePath)));
	}

	static ExecResult GetExecResult(std::string operationId)
	{
		return getResult(SharpFTP::GetExecResult(ToSystemString(operationId)));
	}

	/*static ExecResult UploadToRemote(std::string localPath, std::string remotePath, bool createIfNotExist)
	{
		if (remotePath == "")
			return getResult(SharpFTP::UploadToRemote(ToSystemString(localPath), nullptr, createIfNotExist));
		else
			return getResult(SharpFTP::
				UploadToRemote(ToSystemString(localPath), ToSystemString(remotePath), createIfNotExist));
	}

	static ExecResult DownloadFromRemote(std::string operationId, std::string remotePath, std::string localPath)
	{
		return getResult(SharpFTP::DownloadFromRemote(ToSystemString(operationId), ToSystemString(remotePath), ToSystemString(localPath)));
	}*/
};

#pragma endregion

#pragma region ManagedIO Class
class ManagedIO
{
private:

public:
	static bool IsFileExist(std::string filePath)
	{
		return LocalFileSystem::IsFileExist(ToSystemString(filePath));
	}

	static bool IsDirectoryExist(std::string dirPath)
	{
		return LocalFileSystem::IsDirectoryExist(ToSystemString(dirPath));
	}
};

#pragma endregion

#pragma region ManagedProcess Class

class ManagedProcess
{
private:

public:
	static ExecResult RunProgram(std::string fileName, bool showHide, 
		int windowStyle, std::string params)
	{
		return getResult(SharpProcess::Create(ToSystemString(fileName),
			showHide, windowStyle, ToSystemString(params)));
	}
};

#pragma endregion

#pragma region ManagedNet Class

class ManagedNet
{
public:
	static ExecResult SendWebResuest(std::string content)
	{
		SharpWebClient swc;
		return getResult(swc.SendWebRequest(ToSystemString(content)));
	}
};

#pragma endregion

#pragma region Exported Functions

///==============================================================================
/// ManagedFTP
///==============================================================================
MANAGEDDLL_FUNC std::string ShowHello()
{
	return ManagedFTP::ShowHello();
}

MANAGEDDLL_FUNC void SetFtpInfo(std::string operationId, std::string host, std::string user,
	std::string passwd, std::string remotePath)
{
	std::string content("cfg");
	content.append("," + operationId + "," + host + "," + user + "," + passwd + "," + remotePath);
	ManagedNet::SendWebResuest(content);
}

MANAGEDDLL_FUNC ExecResult GetExecResult(std::string operationId)
{
	return ManagedFTP::GetExecResult(operationId);
}

//MANAGEDDLL_FUNC ExecResult UploadToRemote(std::string localPath, std::string remotePath, bool createIfNotExist)
//{
//	return ManagedFTP::UploadToRemote(localPath, remotePath, createIfNotExist);
//}
//
//MANAGEDDLL_FUNC ExecResult DownloadFromRemote(std::string operationId, std::string remotePath, std::string localPath)
//{
//	return ManagedFTP::DownloadFromRemote(operationId, remotePath, localPath);
//}


///==============================================================================
/// ManagedIO
///==============================================================================
MANAGEDDLL_FUNC bool IsFileExist(std::string filePath)
{
	return ManagedIO::IsFileExist(filePath);
}

MANAGEDDLL_FUNC bool IsDirectoryExist(std::string dirPath)
{
	return ManagedIO::IsDirectoryExist(dirPath);
}

///==============================================================================
/// ManagedProcess
///==============================================================================
MANAGEDDLL_FUNC ExecResult RunProgram(std::string fileName, bool showHide,
	int windowStyle, std::string params)
{
	return ManagedProcess::RunProgram(fileName, showHide, windowStyle, params);
}

///==============================================================================
/// ManagedNet
///==============================================================================
MANAGEDDLL_FUNC ExecResult SendWebResuest(std::string content)
{
	return ManagedNet::SendWebResuest(content);
}

#pragma endregion