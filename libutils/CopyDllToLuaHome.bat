
if not "%1"=="am_admin" (powershell start -verb runas '%0' am_admin & exit)

%~d0

cd %~dp0

cd "..\x64\Debug"

xcopy ManagedDll.dll "C:\Program Files\Lua\5.2\" /Y /S

pause