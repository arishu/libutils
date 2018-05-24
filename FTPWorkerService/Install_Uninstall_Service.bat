
@echo on

set LOCATION="home"

if %LOCATION% == "home" (
	set DST="D:\Program Files\Aris\FTPWorker\"
) else (
	set DST="D:\casco\ssip\bin\"
)

%~d0

cd "%DST%"

set Service=FTPWorkerService.exe
set Operation="%1"

@echo off

if EXIST %Operation% (
	if "%Operation%" == "install" (
		installutil %Service%
	) else if "%Operation%" == "remove" (
		installutil /u %Service%
	) else (
		echo "Usage1: install or remove"
	)
) else (
	echo "Usage2: install or remove"
)

pause