@echo off
if not "%1"=="am_admin" (powershell start -verb runas '%0' am_admin & exit)

%~d0

cd %~dp0

set MAIN_HOME="%~dp0"

cd "..\FTPWorker\bin\x64\Debug"

set LOCATION="company"

if %LOCATION% == "home" (
	set DST="D:\Program Files\Aris\FTPWorker\"
) else (
	set DST="D:\casco\ssip\bin\FTPWorker\"
)

if NOT EXIST %DST% (
	MKDIR %DST%
)

REM Copy need files
if %LOCATION% == "home" (
	@echo on

	xcopy "%MAIN_HOME%Install_Uninstall_Service.bat" %DST% /Y /S

	xcopy FTPWorkerService.exe %DST% /Y /S
	xcopy FTPWorkerService.exe.config %DST% /Y /S
	
	xcopy FTPWorker.exe %DST% /Y /S
	xcopy FTPWorker.exe.config %DST% /Y /S
	
	xcopy libutilscore.dll %DST% /Y /S
	xcopy libutilscore.dll.config %DST% /Y /S
	
	xcopy NLog.dll %DST% /Y /S
	xcopy NLog.config %DST% /Y /S
) else (
	@echo on
	xcopy "%MAIN_HOME%Install_Uninstall_Service.bat" %DST% /Y /S

	xcopy FTPWorkerService.exe %DST% /Y /S
	xcopy FTPWorkerService.exe.config %DST% /Y /S

	xcopy FTPWorker.exe %DST% /Y /S
	xcopy FTPWorker.exe.config %DST% /Y /S
	
	xcopy libutilscore.dll %DST% /Y /S
	REM xcopy libutilscore.dll.config %DST% /Y /S

	xcopy NLog.dll %DST% /Y /S
	xcopy NLog.config %DST% /Y /S
)

pause