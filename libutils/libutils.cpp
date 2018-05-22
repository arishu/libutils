// libutils.cpp: 定义 DLL 应arg用程序的导出函数。
//

#pragma comment(lib, "rpcrt4.lib")  // UuidCreate - Minimum supported OS Win 2000
#include "libutils.h"
#include "CoreInterface.h"
#include <Windows.h>
#include <iostream>
#include <algorithm>

#define LIBUTILS_NAME "libutils"
#define REGISTRY_FTP_NAME "FtpInfo"

static const char* const ERROR_ARGUMENT_COUNT = "参数数目错误！";
static const char* const ERROR_ARGUMENT_TYPE = "参数类型错误！";
static const char* const ERROR_ARGUMENT_EMPTY = "参数不能为空! ";

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
void ErrorMsg(lua_State* luaEnv, const char* const pszErrorInfo, const char *funcName)
{
	/*lua_Debug ar;
	int level = 0;

	std::string stackMsg = std::string();
	while (lua_getstack(luaEnv, level, &ar)) {
		int status = lua_getinfo(luaEnv, "Sln", &ar);
		_ASSERT(status);
		stackMsg.append(ar.short_src);
			stackMsg.append(":" + ar.currentline);
			if (std::string(ar.name).empty())
			{
				stackMsg.append(":");
				stackMsg.append(ar.name);
			}
			else
				stackMsg.append(":?");
		level++;
	}
	stackMsg.append("\n");
	stackMsg.append(pszErrorInfo);*/
	lua_pushfstring(luaEnv, "调用函数 %s 出错: %s\n", funcName, pszErrorInfo);
	lua_error(luaEnv);
}

// 检测函数调用参数个数是否正常
void CheckParamCount(lua_State* luaEnv, int paramMinCount, int paramMaxCount, const char *funcName)
{
	// lua_gettop获取栈中元素个数. 
	int top = lua_gettop(luaEnv);
	if (paramMinCount > top || top > paramMaxCount)
	{
		ErrorMsg(luaEnv, ERROR_ARGUMENT_COUNT, funcName);
	}
}

/* 检查参数类型是否正确 */
void CheckParamType(lua_State* luaEnv, int index, int type, const char *funcName)
{
	if (lua_type(luaEnv, index) != type)
	{
		ErrorMsg(luaEnv, ERROR_ARGUMENT_TYPE, funcName);
	}
}

/* 检查参数是否为空 */
void ArgsNonEmpty(lua_State* luaEnv, int index)
{
	if (lua_isnil(luaEnv, index) == 1)
	{
		if (lua_type(luaEnv,index) == LUA_TSTRING || lua_type(luaEnv, index) == LUA_TTABLE)
			if (luaL_len(luaEnv, index) == 0)
				luaL_argerror(luaEnv, index, ERROR_ARGUMENT_EMPTY);
		else
			luaL_argerror(luaEnv, index, ERROR_ARGUMENT_EMPTY);
	}
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
	lua_pushboolean(luaEnv, IsDirectoryExist(lua_tostring(luaEnv, 1)));
	return 1;
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

	std::string operationId("");
	std::string tmpStr = std::string(newUuid.c_str());
	operationId = tmpStr.substr(0, 10);

	RpcStringFreeA((RPC_CSTR *)&str);

	return operationId;
}

/*
*@desc	上传文件到FTP服务器
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
	std::replace(localFilePath.begin(), localFilePath.end(), ' ', '\u0020');

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


	bool createIfNotExist = false;
	if (lua_isnone(luaEnv, 3) == 1) {
	}
	else
	{
		CheckParamType(luaEnv, 3, LUA_TBOOLEAN, __FUNCTION__);
		createIfNotExist = lua_toboolean(luaEnv, 3);
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
	{
		param.append("True");
	}
	else
	{
		param.append("False");
	}

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

/* 下载回调函数 */
//int download_callback(lua_State *luaEnv)
//{
//	if (lua_gettop(luaEnv) == 0 && lua_isfunction(luaEnv, -1))
//	{
//		lua_pcall(luaEnv, 0, 0, 0);
//	}
//	return 0;
//}

/*
 * @desc	从FTP服务器下载文件
 * @param	remoteFilePath	远程文件的绝对路径
 * @param	localFilePath	本地存储位置,可以包含文件名
 */
static int downloadFromRemote(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 2, 2, __FUNCTION__);
	CheckParamType(luaEnv, 1, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, 1);
	CheckParamType(luaEnv, 2, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, 2);

	// 产生新的操作标识
	std::string newOperationId = getNewOperationId();

	std::string param("get");
	param.append(",");
	param.append(newOperationId);
	param.append(",");
	param.append(std::string(lua_tostring(luaEnv, 1)));
	param.append(",");
	param.append(std::string(lua_tostring(luaEnv, 2)));

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
* @param	operationId	操作标识
*/
static int getExecuteResult(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 1, 1, __FUNCTION__);
	CheckParamType(luaEnv, 1, LUA_TSTRING, __FUNCTION__);
	ArgsNonEmpty(luaEnv, 1);

	// 获取结果
	ExecResult result = GetExecResult(std::string(lua_tostring(luaEnv, 1)));

	lua_pushboolean(luaEnv, result.isSuccess);
	lua_pushstring(luaEnv, result.message.c_str());
	return 2;
}

static const luaL_Reg libutils_funcs[] = {
	{"isFileExist", isFileExist},
	{"isDirExist", isDirExist},
	{"uploadToRemote", uploadToRemote},
	{"downloadFromRemote", downloadFromRemote},
	{"getExecuteResult", getExecuteResult},
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
