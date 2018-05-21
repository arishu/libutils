// libutils.cpp: 定义 DLL 应用程序的导出函数。
//

#include "libutils.h"
#include "lua52\lua.hpp"
#include "ManagedDll.h"
#pragma comment(lib, "rpcrt4.lib")  // UuidCreate - Minimum supported OS Win 2000
#include <windows.h>
#include <iostream>
#include <algorithm>
#include <tchar.h>

#define LIBUTILS_NAME "libutils"
#define REGISTRY_FTP_NAME "FtpInfo"
#define DEFAULT_BIN_DIR "C:\\Program Files\\Lua\\5.2\\"
//#define DEFAULT_BIN_DIR "D:\\workspace\\github\\libutils\\x64\\"

static const char* const ERROR_ARGUMENT_COUNT = "参数数目错误！";
static const char* const ERROR_ARGUMENT_TYPE = "参数类型错误！";
static const char* const ERROR_ARGUMENT_EMPTY = "参数不能为空! ";

/* Ftp信息结构体 */
typedef struct FtpCfg {
	std::string host;
	std::string user;
	std::string passwd;
	std::string remotePath;
} FTPINFO;

/* 打印栈信息 */
void stackDump(lua_State *L) {
	int i;
	int top = lua_gettop(L);
	for (i = 1; i <= top; i++) {
		int t = lua_type(L, i);
		switch (t)
		{
		case LUA_TSTRING:
			printf("####'%s'", lua_tostring(L, i));
			break;
		case LUA_TBOOLEAN:
			printf(lua_toboolean(L, i) ? "####true" : "####false");
			break;
		case LUA_TNUMBER:
			printf("####%g", lua_tonumber(L, i));
			break;
		default:
			printf("####%s", lua_typename(L, t));
			break;
		}
		printf("  ");
	}
	printf("\n");
}

// 发生错误,报告错误
//void ErrorMsg(lua_State* luaEnv, const char* pszErrorInfo)
//{
//	lua_Debug ar;
//	//lua_getglobal(luaEnv, "setFtpInfo");
//	//lua_getinfo(luaEnv, ">nSltufL", &ar);
//	//lua_getstack(luaEnv, 2, &ar);
//	std::string errMsg = std::string();
//	errMsg.append("调用函数 ");
//	errMsg.append(ar.name);
//	errMsg.append(pszErrorInfo);
//
//	lua_pushstring(luaEnv, errMsg.c_str());
//	lua_error(luaEnv);
//}

// 检测函数调用参数个数是否正常
void CheckParamCount(lua_State* luaEnv, int paramMinCount, int paramMaxCount, const char *funcName)
{
	// lua_gettop获取栈中元素个数. 
	int top = lua_gettop(luaEnv);
	if (paramMinCount > top || top > paramMaxCount)
	{
		lua_pushfstring(luaEnv, "调用函数 %s 错误: %s\n", __FUNCTION__, ERROR_ARGUMENT_COUNT);
		lua_error(luaEnv);
	}
}

/* 检查参数类型是否正确 */
void CheckParamType(lua_State* luaEnv, int index, int type, const char *funcName)
{
	if (lua_type(luaEnv, index) != type)
	{
		lua_pushfstring(luaEnv, "调用函数 %s 错误: %s\n", __FUNCTION__ ,ERROR_ARGUMENT_TYPE);
		lua_error(luaEnv);
	}
}

/* 检查参数是否为空 */
void ArgsNonEmpty(lua_State* luaEnv, int index)
{
	if (lua_isnil(luaEnv, index) == 1 || luaL_len(luaEnv, index) == 0)
	{
		luaL_argerror(luaEnv, index, ERROR_ARGUMENT_EMPTY);
	}
}

/* 测试方法 */
static int showHello(lua_State *luaEnv)
{
	std::string result = ShowHello();
	printf("%s\n", result.c_str());
	lua_pushstring(luaEnv, "Hello World");
	return 1;
}

/* 判断文件是否存在 */
static int isFileExist(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 1, 1, __FUNCTION__);
	CheckParamType(luaEnv, 1, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, 1);
	lua_pushboolean(luaEnv, IsFileExist(lua_tostring(luaEnv, 1)));
	return 1;
}

/* 判断目录是否存在 */
static int isDirExist(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 1, 1, __FUNCTION__);
	CheckParamType(luaEnv, 1, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, 1);
	lua_pushboolean(luaEnv, IsFileExist(lua_tostring(luaEnv, 1)));
	return 1;
}

/*
 * @desc 从共享表中获取FTP配置信息
 * @param ftpInfo	用于存放FTP信息的结构体
 */
int getFtpInfoFromSharedUpValue(lua_State *luaEnv, FTPINFO *ftpInfo)
{
	// 从共享表中取出FTP配置表, 压入栈顶
	lua_getfield(luaEnv, lua_upvalueindex(1), REGISTRY_FTP_NAME);
	lua_getfield(luaEnv, -1, "host");
	ftpInfo->host = lua_tostring(luaEnv, -1);
	lua_pop(luaEnv, 1);
	lua_getfield(luaEnv, -1, "user");
	ftpInfo->user = lua_tostring(luaEnv, -1);
	lua_pop(luaEnv, 1);
	lua_getfield(luaEnv, -1, "passwd");
	ftpInfo->passwd = lua_tostring(luaEnv, -1);
	lua_pop(luaEnv, 1);
	lua_getfield(luaEnv, -1, "remotePath");
	ftpInfo->remotePath = lua_tostring(luaEnv, -1);
	lua_pop(luaEnv, 1);
	return 0;
}

//void getCommandLine(std::string program, std::string args[], _TCHAR *tcommand)
//{
//	std::string command;
//	command.append(program);
//	command.append(" ");
//	for (int i = 0; i < args->length; i++)
//	{
//		command.append(args[i]);
//		command.append(" ");
//	}
//
//	std::strcpy(tcommand, command.c_str());
//}
//
//void runProcess(std::string program, std::string args[])
//{
//	STARTUPINFO si;
//	PROCESS_INFORMATION pi;
//
//	ZeroMemory(&si, sizeof(si));
//	si.cb = sizeof(si);
//	//隐藏掉可能出现的cmd命令窗口
//	si.dwFlags = STARTF_USESHOWWINDOW;
//	si.wShowWindow = SW_HIDE;
//	ZeroMemory(&pi, sizeof(pi));
//
//	_TCHAR tcommand;
//	getCommandLine(program, args, &tcommand);
//	
//
//	if (!CreateProcess(NULL,
//		&tcommand,
//		NULL,
//		NULL,
//		FALSE,
//		0,
//		NULL,
//		NULL,
//		&si,
//		&pi))
//	{
//		// 创建进程失败
//		program = std::string(program);
//		program = DEFAULT_BIN_DIR + program;
//		getCommandLine(program, args, &tcommand);
//		if (!CreateProcess(NULL,
//			&tcommand,
//			NULL,
//			NULL,
//			FALSE,
//			0,
//			NULL,
//			NULL,
//			&si,
//			&pi))
//		{
//			return;
//		}
//	}
//
//	// Wait until child process exits.
//	WaitForSingleObject(pi.hProcess, INFINITE);
//
//	//关闭进程与线程处理器
//	CloseHandle(pi.hProcess);
//	CloseHandle(pi.hThread);
//}

/*
 * @desc 从共享表中获取FTP配置信息
 * @param ftpInfo	存储FTP信息的结构体
 */
int setFtpInfoToSharedUpValue(lua_State *luaEnv, FTPINFO *ftpInfo)
{
	// 从共享表中取出FTP配置表, 压入栈顶
	lua_getfield(luaEnv, lua_upvalueindex(1), REGISTRY_FTP_NAME);
	lua_pushstring(luaEnv, ftpInfo->host.c_str());
	lua_setfield(luaEnv, -2, "host");
	lua_pushstring(luaEnv, ftpInfo->user.c_str());
	lua_setfield(luaEnv, -2, "user");
	lua_pushstring(luaEnv, ftpInfo->passwd.c_str());
	lua_setfield(luaEnv, -2, "passwd");
	lua_pushstring(luaEnv, ftpInfo->remotePath.c_str());
	lua_setfield(luaEnv, -2, "remotePath");

	// 将栈顶的FTP表信息保存到共享表中
	lua_setfield(luaEnv, lua_upvalueindex(1), REGISTRY_FTP_NAME);

	return 0;
}

/* 
 * @desc 产生新的操作ID
 */
std::string getNewOperationId()
{
	UUID uuid;
	UuidCreate(&uuid);
	char *str;
	UuidToStringA(&uuid, (RPC_CSTR *)&str);
	std::string newUuid(str);
	
	// 删除uuid中所有'-'
	std::remove(newUuid.begin(), newUuid.end(), '-');

	std::string operationId = std::string(newUuid.c_str());

	RpcStringFreeA((RPC_CSTR *)&str);

	return operationId;
}

/* 
 *@desc		上传文件到FTP服务器 
 *@param	localFilePath	  本地文件的绝对路径
 *@param	remotePath		  远程存放位置
 *@param	createIfNotExist  默认值为false,表示当remotePath在服务器上不存在时，将抛出异常
 *							  设置为true后, 如果remotePath在服务器上不存在, 将创建目录
 */
static int uploadToRemote(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 1, 3, __FUNCTION__);
	CheckParamType(luaEnv, 1, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, 1);
	std::string localFilePath = lua_tostring(luaEnv, 1);

	bool createIfNotExist = false;
	if (lua_isnone(luaEnv, 3) == 1) {
	}
	else 
	{
		CheckParamType(luaEnv, 3, LUA_TBOOLEAN, __FUNCTION__);
		createIfNotExist = lua_toboolean(luaEnv, 3);
	}

	//// 从共享表中获取FTP配置信息：registry[key]
	//FTPINFO ftpInfo = FTPINFO();
	//getFtpInfoFromSharedUpValue(luaEnv, &ftpInfo);

	//ExecResult ret;
	//// 设置FTP信息
	//ret = SetFtpInfo(ftpInfo.host, ftpInfo.user, ftpInfo.passwd, ftpInfo.remotePath);

	std::string remotePath("/");
	// 如果没有定义第二个参数, 或 第二个参数值为nil或""
	if (lua_isnone(luaEnv, 2) == 1 || lua_isnil(luaEnv, 2) == 1 || luaL_len(luaEnv, 2) == 0)
	{
		remotePath = "/";
	}
	else
	{
		CheckParamType(luaEnv, 2, LUA_TSTRING, __FUNCTION__);
		ArgsNonEmpty(luaEnv, 2);
		remotePath = lua_tostring(luaEnv, 2);
	}

	// 产生新的操作标识
	std::string newOperationId = getNewOperationId();

	std::string param("put");
	param.append(",");
	param.append(newOperationId);
	param.append(",");
	param.append(localFilePath);
	param.append(",");
	param.append(remotePath);
	param.append(",");
	if (createIfNotExist)
		param.append("True");
	else 
		param.append("False");

	// 发送FTP上传请求给服务器
	ExecResult ret = SendWebResuest(param);

	if (ret.isSuccess) 
	{
		lua_pushboolean(luaEnv, true);
		// 返回操作标识
		lua_pushstring(luaEnv, newOperationId.c_str());
	}
	else
	{
		lua_pushboolean(luaEnv, false);
		lua_pushstring(luaEnv, ret.message.c_str());
	}
	return 2;
}

/* 
 * @desc	从FTP服务器下载文件
 * @param	remoteFilePath	远程文件的绝对路径
 * @param   localFilePath	本地存储位置,可以包含文件名
 */
static int downloadFromRemote(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 2, 2, __FUNCTION__);
	CheckParamType(luaEnv, 1, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, 1);
	CheckParamType(luaEnv, 2, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, 2);

	//// 从共享表中获取FTP配置信息
	//FTPINFO ftpInfo = FTPINFO();
	//getFtpInfoFromSharedUpValue(luaEnv, &ftpInfo);

	//// 设置FTP信息
	//SetFtpInfo(ftpInfo.host, ftpInfo.user, ftpInfo.passwd, ftpInfo.remotePath);

	// 产生新的操作标识
	std::string newOperationId = getNewOperationId();
	
	std::string param("get");
	param.append(",");
	param.append(newOperationId);
	param.append(",");
	param.append(lua_tostring(luaEnv, 1));
	param.append(",");
	param.append(lua_tostring(luaEnv, 2));

	// 发送FTP下载请求给服务器
	ExecResult ret = SendWebResuest(param);

	if (ret.isSuccess)
	{
		lua_pushboolean(luaEnv, true);
		// 返回操作标识
		lua_pushstring(luaEnv, newOperationId.c_str());
	}
	else
	{
		lua_pushboolean(luaEnv, false);
		lua_pushstring(luaEnv, ret.message.c_str());
	}
	return 2;
}

/*
* @desc		获取与指定operationId相关的执行结果
* @param	remoteFilePath	远程文件的绝对路径
* @param	localFilePath	本地存储位置,可以包含文件名
*/
static int getExecuteResult(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 1, 1, __FUNCTION__);
	CheckParamType(luaEnv, 1, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, 1);

	// 获取结果
	ExecResult result = GetExecResult(lua_tostring(luaEnv, 1));

	lua_pushboolean(luaEnv, result.isSuccess);
	lua_pushstring(luaEnv, result.message.c_str());
	return 2;
}

/*
 * @desc 设置FTP配置信息
 * @param table	包含FTP信息的配置表
 */
static int setFtpInfo(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 1, 1, __FUNCTION__);
	CheckParamType(luaEnv, 1, LUA_TTABLE, __FUNCTION__);

	FTPINFO ftpInfo = FTPINFO();

	lua_getfield(luaEnv, 1, "host");
	CheckParamType(luaEnv, -1, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, -1);
	ftpInfo.host = std::string(lua_tostring(luaEnv, -1));

	lua_getfield(luaEnv, 1, "user");
	CheckParamType(luaEnv, -1, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, -1);
	ftpInfo.user = std::string(lua_tostring(luaEnv, -1));

	lua_getfield(luaEnv, 1, "passwd");
	CheckParamType(luaEnv, -1, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, -1);
	ftpInfo.passwd = std::string(lua_tostring(luaEnv, -1));

	lua_getfield(luaEnv, 1, "remotePath");
	if (lua_isnone(luaEnv, -1) == 1 || lua_isnil(luaEnv, -1) == 1 || luaL_len(luaEnv, -1) == 0)
		ftpInfo.remotePath = std::string("");
	else
		ftpInfo.remotePath = std::string(lua_tostring(luaEnv, -1));

	/*lua_pushfstring(luaEnv, "ftp[host=%s, user=%s, passwd=%s, remotePath=%s]\n",
		ftpInfo.host.c_str(), ftpInfo.user.c_str(), ftpInfo.passwd.c_str(), ftpInfo.remotePath.c_str());*/

	setFtpInfoToSharedUpValue(luaEnv, &ftpInfo);
	return 0;
}

static const luaL_Reg libutils_funcs[] = {
	{"showHello", showHello},
	{"isFileExist", isFileExist},
	{"isDirExist", isDirExist},
	//{"setFtpInfo", setFtpInfo},
	{"uploadToRemote", uploadToRemote},
	{"downloadFromRemote", downloadFromRemote},
	{"getExecuteResult", getExecuteResult },
	{"NULL", NULL}
};

 int luaopen_libutils(lua_State *luaEnv)
{
	//luaL_newlib(luaEnv, libutils_funcs);
	//lua_setglobal(luaEnv, LIBUTILS_NAME);	// 将库的名称保存到全局变量中
	luaL_newlibtable(luaEnv, libutils_funcs);

	// 创建FTP数据存储区域, 
	// 当前lua5.2版本不支持将struct数据放进table中
	lua_newtable(luaEnv); // 全局table
	lua_newtable(luaEnv);
	lua_setfield(luaEnv, -2, REGISTRY_FTP_NAME);

	luaL_setfuncs(luaEnv, libutils_funcs, 1);
	lua_setglobal(luaEnv, LIBUTILS_NAME);	// 将库的名称保存到全局变量中
	return 1;
}
