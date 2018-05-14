#pragma once

#include <string>

#ifdef MANAGEDDLL_EXPORTS
#define MANAGEDDLL_FUNC __declspec(dllexport)
#else
#define MANAGEDDLL_FUNC __declspec(dllimport)
#endif // MANAGEDDLL_EXPORTS

/* Struct containing the execution result */
struct ExecResult
{
	bool isSuccess;			/* whether or not successfully be executed */
	std::string message;	/* error message */
};

/* Test Method */
MANAGEDDLL_FUNC std::string ShowHello();

/* Set FTP infomation */
MANAGEDDLL_FUNC ExecResult SetFtpInfo(std::string host, std::string user,
	std::string passwd, std::string remotePath);

/* Upload File to Remote */
MANAGEDDLL_FUNC ExecResult UploadToRemote(std::string localPath, std::string remotePath, bool createIfNotExist);

/* Download File from Remote */
MANAGEDDLL_FUNC ExecResult DownloadFromRemote(std::string remotePath, std::string localPath);

/* Check whether or not file exist */
MANAGEDDLL_FUNC bool IsFileExist(std::string filePath);

/* Check whether or not directory exist */
MANAGEDDLL_FUNC bool IsDirectoryExist(std::string dirPath);