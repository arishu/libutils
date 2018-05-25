@ECHO OFF

REM Set tool path to environment
set ServicePath="%~dp0FTPWorkerService.exe"
set INSTALLER="C:\Windows\Microsoft.NET\Framework\v4.0.30319"
set PATH=%PATH%;%INSTALLER%

REM Begin to install the service
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
echo ----------------------------------------------------------------------

GOTO Usage



:Usage
	echo "Usage: installutil install | remove"

:End

pause