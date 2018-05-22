#pragma once

#include <Windows.h>
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
MANAGEDDLL_FUNC ExecResult SetFtpInfo(std::string operationId, std::string host, std::string user,
	std::string passwd, std::string remotePath);

/* Get Execute Result */
MANAGEDDLL_FUNC ExecResult GetExecResult(std::string operationId);

/* Check whether or not file exist */
MANAGEDDLL_FUNC bool IsFileExist(std::string filePath);

/* Check whether or not directory exist */
MANAGEDDLL_FUNC bool IsDirectoryExist(std::string dirPath);

/* Run a program */
MANAGEDDLL_FUNC ExecResult RunProgram(std::string fileName, bool showHide,
	int windowStyle, std::string params);

/* Send request to server */
MANAGEDDLL_FUNC ExecResult SendWebResuest(std::string content);
