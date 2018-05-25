@ECHO OFF

set LOCATION="company"
if %LOCATION% == "home" (
	set DST="D:\Program Files\Aris\FTPWorker\"
) else (
	set DST="D:\casco\ssip\bin\FTPWorker\"
)

REM Set tool path to environment
set ServicePath="%~dp0FTPWorkerService.exe"
set INSTALLER="C:\Windows\Microsoft.NET\Framework\v4.0.30319"
set PATH=%PATH%;%INSTALLER%

echo ----------------------------------------------------------------------
:Main
if "%~1"=="install" (
	REM Starting install FTPWorkerService
	installutil %ServicePath%
	GOTO End
)

if "%~1"=="remove" (
	REM Starting uninstall FTPWorkerService
	installutil /u %ServicePath%
	GOTO End
)

GOTO Usage

echo ----------------------------------------------------------------------


:Usage
	echo "Usage: installutil install | remove"
:End

pause