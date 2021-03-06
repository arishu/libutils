// libutils.cpp: 定义 DLL 应用程序的导出函数。
//

#include "libutils.h"
#include "lua.hpp"
#include "ManagedDll.h"

#define LIBUTILS_NAME "libutils"
#define REGISTRY_FTP_NAME "FtpInfo"

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
void ErrorMsg(lua_State* luaEnv, const char* const pszErrorInfo)
{
	lua_Debug ar;
	lua_getstack(luaEnv, 2, &ar);
	lua_pushfstring(luaEnv, "调用函数[%s]错误: %s\n", ar.name, pszErrorInfo);
	lua_error(luaEnv);
}

// 检测函数调用参数个数是否正常
void CheckParamCount(lua_State* luaEnv, int paramMinCount, int paramMaxCount)
{
	// lua_gettop获取栈中元素个数. 
	int top = lua_gettop(luaEnv);
	if (paramMinCount > top || top > paramMaxCount)
	{
		ErrorMsg(luaEnv, ERROR_ARGUMENT_COUNT);
	}
}

/* 检查参数类型是否正确 */
void CheckParamType(lua_State* luaEnv, int index, int type)
{
	if (lua_type(luaEnv, index) != type)
	{
		ErrorMsg(luaEnv, ERROR_ARGUMENT_TYPE);
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
	CheckParamCount(luaEnv, 1, 1);
	CheckParamType(luaEnv, 1, LUA_TSTRING);
	ArgsNonEmpty(luaEnv, 1);
	lua_pushboolean(luaEnv, IsFileExist(lua_tostring(luaEnv, 1)));
	return 1;
}

/* 判断目录是否存在 */
static int isDirExist(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 1, 1);
	CheckParamType(luaEnv, 1, LUA_TSTRING);
	ArgsNonEmpty(luaEnv, 1);
	lua_pushboolean(luaEnv, IsFileExist(lua_tostring(luaEnv, 1)));
	return 1;
}

/* 设置FTP信息 */
//static int setFtpInfo(lua_State *luaEnv)
//{
//	CheckParamCount(luaEnv, 3, 4);
//	CheckParamType(luaEnv, 1, LUA_TSTRING);
//	ArgsNonEmpty(luaEnv, 1);
//	CheckParamType(luaEnv, 2, LUA_TSTRING);
//	ArgsNonEmpty(luaEnv, 2);
//	CheckParamType(luaEnv, 3, LUA_TSTRING);
//	ArgsNonEmpty(luaEnv, 3);
//	
//	ExecResult ret;
//
//	if (lua_isnone(luaEnv, 4) != 1)
//	{
//		CheckParamType(luaEnv, 4, LUA_TSTRING);
//		ArgsNonEmpty(luaEnv, 4);
//
//		ret = SetFtpInfo(lua_tostring(luaEnv, 1), lua_tostring(luaEnv, 2),
//			lua_tostring(luaEnv, 3), lua_tostring(luaEnv, 4));
//	}
//	else;
//	{
//		ret = SetFtpInfo(lua_tostring(luaEnv, 1), lua_tostring(luaEnv, 2),
//			lua_tostring(luaEnv, 3), "");
//	}
//	
//	lua_pushboolean(luaEnv, ret.isSuccess);
//	lua_pushstring(luaEnv, ret.message.c_str());
//	return 2;
//}

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
 *@desc		上传文件到FTP服务器 
 *@param	localFilePath	  本地文件的绝对路径
 *@param	remotePath		  远程存放位置
 *@param	createIfNotExist  默认值为false,表示当remotePath在服务器上不存在时，将抛出异常
 *							  设置为true后, 如果remotePath在服务器上不存在, 将创建目录
 */
static int uploadToRemote(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 1, 3);
	CheckParamType(luaEnv, 1, LUA_TSTRING);
	ArgsNonEmpty(luaEnv, 1);

	bool createIfNotExist = false;
	if (lua_isnone(luaEnv, 3) == 1) {
	}
	else 
	{
		CheckParamType(luaEnv, 3, LUA_TBOOLEAN);
		createIfNotExist = lua_toboolean(luaEnv, 3);
	}

	// 从共享表中获取FTP配置信息：registry[key]
	FTPINFO ftpInfo = FTPINFO();
	getFtpInfoFromSharedUpValue(luaEnv, &ftpInfo);

	ExecResult ret;
	// 设置FTP信息
	ret = SetFtpInfo(ftpInfo.host, ftpInfo.user, ftpInfo.passwd, ftpInfo.remotePath);

	if (ret.isSuccess) 
	{
		// 如果没有定义第二个参数, 或 第二个参数值为nil或""
		if (lua_isnone(luaEnv, 2) == 1 || lua_isnil(luaEnv, 2) == 1 || luaL_len(luaEnv, 2) == 0)
		{
			ret = UploadToRemote(lua_tostring(luaEnv, 1), "", createIfNotExist);
		}
		else
		{
			CheckParamType(luaEnv, 2, LUA_TSTRING);
			ArgsNonEmpty(luaEnv, 2);

			ret = UploadToRemote(lua_tostring(luaEnv, 1), lua_tostring(luaEnv, 2), createIfNotExist);
		}
	}

	lua_pushboolean(luaEnv, ret.isSuccess);
	lua_pushstring(luaEnv, ret.message.c_str());
	return 2;
}

/* 
 * @desc	从FTP服务器下载文件
 * @param	remoteFilePath	远程文件的绝对路径
 * @param   localFilePath	本地存储位置,可以包含文件名
 */
static int downloadFromRemote(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 2, 2);
	CheckParamType(luaEnv, 1, LUA_TSTRING);
	ArgsNonEmpty(luaEnv, 1);
	CheckParamType(luaEnv, 2, LUA_TSTRING);
	ArgsNonEmpty(luaEnv, 2);

	// 从共享表中获取FTP配置信息
	FTPINFO ftpInfo = FTPINFO();
	getFtpInfoFromSharedUpValue(luaEnv, &ftpInfo);

	ExecResult ret = ExecResult();
	// 设置FTP信息
	ret = SetFtpInfo(ftpInfo.host, ftpInfo.user, ftpInfo.passwd, ftpInfo.remotePath);
	if (ret.isSuccess) { // 设置FTP信息成功
		ret = DownloadFromRemote(lua_tostring(luaEnv, 1), lua_tostring(luaEnv, 2));
	}

	lua_pushboolean(luaEnv, ret.isSuccess);
	lua_pushstring(luaEnv, ret.message.c_str());
	/*lua_pushfstring(luaEnv, "ftp[host=%s, user=%s, passwd=%s, remotePath=%s]\n",
		ftpInfo.host.c_str(), ftpInfo.user.c_str(), ftpInfo.passwd.c_str(), ftpInfo.remotePath.c_str());*/
	return 2;
}

/*
 * @desc 设置FTP配置信息
 * @param table	包含FTP信息的配置表
 */
static int setFtpInfo(lua_State *luaEnv)
{
	CheckParamCount(luaEnv, 1, 1);
	CheckParamType(luaEnv, 1, LUA_TTABLE);

	FTPINFO ftpInfo = FTPINFO();

	lua_getfield(luaEnv, 1, "host");
	CheckParamType(luaEnv, -1, LUA_TSTRING);
	ArgsNonEmpty(luaEnv, -1);
	ftpInfo.host = std::string(lua_tostring(luaEnv, -1));

	lua_getfield(luaEnv, 1, "user");
	CheckParamType(luaEnv, -1, LUA_TSTRING);
	ArgsNonEmpty(luaEnv, -1);
	ftpInfo.user = std::string(lua_tostring(luaEnv, -1));

	lua_getfield(luaEnv, 1, "password");
	CheckParamType(luaEnv, -1, LUA_TSTRING);
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

	//ExecResult ret;
	//if (lua_isnone(luaEnv, -1) != 1) // 定义了remotePath变量
	//{
	//	CheckParamType(luaEnv, -1, LUA_TSTRING);
	//	ArgsNonEmpty(luaEnv, -1);
	//	const char *remotePath = lua_tostring(luaEnv, -1);
	//	ret = SetFtpInfo(host, user, passwd, remotePath);
	//}
	//else
	//{
	//	ret = SetFtpInfo(host, user, passwd, "");
	//}
	//lua_pushboolean(luaEnv, ret.isSuccess);
	//lua_pushstring(luaEnv, ret.message.c_str());
	//return 2;
}

static const luaL_Reg libutils_funcs[] = {
	{"showHello", showHello},
	{"isFileExist", isFileExist},
	{"isDirExist", isDirExist},
	{"setFtpInfo", setFtpInfo},
	{"uploadToRemote", uploadToRemote},
	{"downloadFromRemote", downloadFromRemote},
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
