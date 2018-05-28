@echo OFF
if not "%1"=="am_admin" (powershell start -verb runas '%0' am_admin & exit)

%~d0

cd %~dp0

set MAIN_HOME="%~dp0"

cd "..\FTPWorker\bin\x64\Debug"

REM Which branch to use
set LOCATION="company"
if %LOCATION% == "home" (
	set DST="D:\Program Files\Aris\FTPWorker\"
	set COPY_OPTIONS=/Y /S /F
) else (
	set DST="D:\casco\ssip\bin\FTPWorker\"
	set COPY_OPTIONS=/Y /S /F
)

REM Try create the directory If directory not exist
if NOT EXIST %DST% (
	MKDIR %DST%
)

:Home
	xcopy "%MAIN_HOME%Install_Uninstall_Service.bat" %DST% %COPY_OPTIONS%

	xcopy FTPWorkerService.exe %DST% %COPY_OPTIONS%
	xcopy FTPWorkerService.exe.config %DST% %COPY_OPTIONS%
	
	xcopy FTPWorker.exe %DST% %COPY_OPTIONS%
	xcopy FTPWorker.exe.config %DST% %COPY_OPTIONS%
	
	xcopy libutilscore.dll %DST% %COPY_OPTIONS%
	xcopy libutilscore.dll.config %DST% %COPY_OPTIONS%
	
	xcopy NLog.dll %DST% %COPY_OPTIONS%
	xcopy NLog.config %DST% %COPY_OPTIONS%

	GOTO End 

:Company
	xcopy "%MAIN_HOME%Install_Uninstall_Service.bat" %DST% %COPY_OPTIONS%

	xcopy FTPWorkerService.exe %DST% %DST% %COPY_OPTIONS%
	xcopy FTPWorkerService.exe.config %DST% %COPY_OPTIONS%

	xcopy FTPWorker.exe %DST% %COPY_OPTIONS%
	xcopy FTPWorker.exe.config %DST% %COPY_OPTIONS%
	
	xcopy libutilscore.dll %DST% %COPY_OPTIONS%
	REM xcopy libutilscore.dll.config %DST% %COPY_OPTIONS%

	xcopy NLog.dll %DST% %COPY_OPTIONS%
	xcopy NLog.config %DST% %COPY_OPTIONS%

:End

pause