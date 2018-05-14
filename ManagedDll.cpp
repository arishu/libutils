// ManagedDll.cpp: 定义 DLL 应用程序的导出函数。
//
#include "stdafx.h"
#include "ManagedDll.h"

using namespace System;
using namespace libutilscore::FTP;
using namespace libutilscore::IO;

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

class ManagedFTP
{
private:
	static ExecResult getResult(Tuple<bool, String^> ^ret)
	{
		ExecResult result;
		result.isSuccess = ret->Item1;
		MarshallString(ret->Item2, result.message);
		return result;
	}
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

	static ExecResult UploadToRemote(std::string localPath, std::string remotePath, bool createIfNotExist)
	{
		if (remotePath == "")
			return getResult(SharpFTP::UploadToRemote(ToSystemString(localPath), nullptr, createIfNotExist));
		else
			return getResult(SharpFTP::
				UploadToRemote(ToSystemString(localPath), ToSystemString(remotePath), createIfNotExist));
	}

	static ExecResult DownloadFromRemote(std::string remotePath, std::string localPath)
	{
		return getResult(SharpFTP::DownloadFromRemote(ToSystemString(remotePath), ToSystemString(localPath)));
	}
};


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


// ******************************************************************************/
// Exported Functions Here
// ******************************************************************************/

///==============================================================================
/// ManagedFTP
///==============================================================================
MANAGEDDLL_FUNC std::string ShowHello()
{
	return ManagedFTP::ShowHello();
}

MANAGEDDLL_FUNC ExecResult SetFtpInfo(std::string host, std::string user,
	std::string passwd, std::string remotePath)
{
	return ManagedFTP::SetFtpInfo(host, user, passwd, remotePath);
}

MANAGEDDLL_FUNC ExecResult UploadToRemote(std::string localPath, std::string remotePath, bool createIfNotExist)
{
	return ManagedFTP::UploadToRemote(localPath, remotePath, createIfNotExist);
}

MANAGEDDLL_FUNC ExecResult DownloadFromRemote(std::string remotePath, std::string localPath)
{
	return ManagedFTP::DownloadFromRemote(remotePath, localPath);
}


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