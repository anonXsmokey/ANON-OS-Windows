@echo off
setlocal
set "ANON_ROOT=%ProgramData%\ANON\Shell\M0"
if not exist "%ANON_ROOT%\ANON.Shell.M0.exe" exit /b 10
start "ANON OS" /d "%ANON_ROOT%" "%ANON_ROOT%\ANON.Shell.M0.exe"
exit /b 0
