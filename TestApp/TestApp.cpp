// TestApp.cpp: 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <iostream>
#include <string>
//#define MANAGEDDLL_EXPORTS
//#include "ManagedDll.h"

#include <windows.h>
#include <iostream>
#include <algorithm>
#include <tchar.h>

using namespace System;
using namespace System::IO;

#define DEFAULT_BIN_DIR "D:\\workspace\\github\\libutils\\x64\\"

void getCommandLine(_TCHAR *program, _TCHAR *args[], _TCHAR *tcommand)
{
	std::string command;
	command.append(program);
	command.append(" ");
	for (int i = 0; i < strlen(*args); i++)
	{
		command.append(args[i]);
		command.append(" ");
	}

	strcpy_s(tcommand, sizeof(tcommand), command.c_str());
}

void runProcess(_TCHAR *program, _TCHAR *args[])
{
	STARTUPINFO si;
	PROCESS_INFORMATION pi;

	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	//隐藏掉可能出现的cmd命令窗口
	si.dwFlags = STARTF_USESHOWWINDOW;
	si.wShowWindow = SW_HIDE;
	ZeroMemory(&pi, sizeof(pi));

	_TCHAR *tcommand = (_TCHAR *)malloc(256);
	getCommandLine(program, args, tcommand);


	if (!CreateProcess(NULL,
		tcommand,
		NULL,
		NULL,
		FALSE,
		0,
		NULL,
		NULL,
		&si,
		&pi))
	{
		// 创建进程失败
		_TCHAR *buffer = {0};
		strncpy_s(buffer, 1024 ,DEFAULT_BIN_DIR, sizeof(DEFAULT_BIN_DIR));
		strncat_s(buffer, sizeof(buffer) ,program, sizeof(program));
		getCommandLine(program, args, tcommand);
		if (!CreateProcess(NULL,
			tcommand,
			NULL,
			NULL,
			FALSE,
			0,
			NULL,
			NULL,
			&si,
			&pi))
		{
			return;
		}
	}

	// Wait until child process exits.
	WaitForSingleObject(pi.hProcess, INFINITE);

	//关闭进程与线程处理器
	CloseHandle(pi.hProcess);
	CloseHandle(pi.hThread);
}


int _tmain()
{
	/*std::string result = ShowHello();
	printf("%s\n", result.c_str());*/

	//ExecResult ret = UploadToRemote("‪D:\\background.webp", "");

	/*ExecResult ret = SetFtpInfo("127.0.0.1", "hwc", "WVVoa2FrMVVTWHBPUkZVeQ==", "");
	printf("%d\n %s", ret.isSuccess, ret.message.c_str());

	ret = DownloadFromRemote("/background.webp", "D:\\");
	printf("%d\n %s", ret.isSuccess, ret.message.c_str());*/

	//Console::WriteLine("str: {0}", Path::GetFileName("ftp://127.0.0.1/promptboard/"));

	//Console::WriteLine("Exist: {0}\n", IsDirectoryExist(std::string("D:\\down\\files")));

	_TCHAR *args[] = { 0 };
	args[0] = (_TCHAR *)" /c notpad.exe";
	runProcess((TCHAR *)"cmd.exe", args);

	system("pause");
    return 0;
}

