@echo off
if not "%1"=="am_admin" (powershell start -verb runas '%0' am_admin & exit)

%~d0

cd %~dp0

cd "..\x64\Debug"

set LOCATION="home"

if %LOCATION% == "home" (
	set DST="C:\Program Files\Lua\5.2\"
) else (
	set DST="D:\casco\ssip\bin\"
)

if %LOCATION% == "home" (
	@echo on
	xcopy ManagedDll.dll %DST% /Y /S
	xcopy libutilscore.dll %DST% /Y /S
	xcopy NLog.dll %DST% /Y /S
	xcopy NLog.config %DST% /Y /S
) else (
	@echo on
	xcopy libutils.dll %DST% /Y /S
	xcopy libutils.pdb %DST% /Y /S

	xcopy ManagedDll.dll %DST% /Y /S
	xcopy libutilscore.dll %DST% /Y /S
	xcopy NLog.dll %DST% /Y /S
	xcopy NLog.config %DST% /Y /S
)

pause