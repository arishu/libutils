@echo off
REM Get Administractor Permission
if not "%1"=="am_admin" (powershell start -verb runas '%0' am_admin & exit)


REM Enter Current bat file's Path
%~d0
cd %~dp0

REM Enter Bineray Output Directory
cd "..\x64\Debug"

set LOCATION="home"
if %LOCATION% == "home" (
	set DST="C:\Program Files\Lua\5.2\"
	set COPY_OPTIONS=/Y /S /F
	GOTO Home
) else (
	set DST="D:\casco\ssip\bin\"
	set COPY_OPTIONS=%DST% /Y /S /F
	GOTO Company
)

:Home
	xcopy ManagedDll.dll %DST% %COPY_OPTIONS%
	xcopy libutilscore.dll %DST% %COPY_OPTIONS%
	xcopy NLog.dll %DST% %COPY_OPTIONS%
	xcopy NLog.config %DST% %COPY_OPTIONS%
	REM xcopy CoreFTP.exe %DST% %COPY_OPTIONS%
	GOTO End

:Company
	xcopy libutils.dll %DST% %COPY_OPTIONS%
	xcopy libutils.pdb %DST% %COPY_OPTIONS%

	xcopy ManagedDll.dll %DST% %COPY_OPTIONS%
	xcopy libutilscore.dll %DST% %COPY_OPTIONS%
	xcopy NLog.dll %DST% %COPY_OPTIONS%
	xcopy NLog.config %DST% %COPY_OPTIONS%

:End

pause