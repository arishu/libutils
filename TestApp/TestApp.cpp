// TestApp.cpp: 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <iostream>
#include <string>
#define MANAGEDDLL_EXPORTS
#include "ManagedDll.h"

using namespace System;
using namespace System::IO;

int _tmain()
{
	/*std::string result = ShowHello();
	printf("%s\n", result.c_str());*/

	//ExecResult ret = UploadToRemote("‪D:\\background.webp", "");

	/*ExecResult ret = SetFtpInfo("192.168.3.3", "hwc", "WVVoa2FrMVVTWHBPUkZVeQ==", "");
	printf("%d\n %s", ret.isSuccess, ret.message.c_str());

	ret = DownloadFromRemote("/background.webp", "D:\\workspace\\visual-studio\\2017\\repos\\libutils\\x64\\Debug\\");
	printf("%d\n %s", ret.isSuccess, ret.message.c_str());*/

	//Console::WriteLine("str: {0}", Path::GetFileName("ftp://127.0.0.1/promptboard/"));

	Console::WriteLine("Exist: {0}\n", IsDirectoryExist(std::string("D:\\down\\files")));

	system("pause");
    return 0;
}

