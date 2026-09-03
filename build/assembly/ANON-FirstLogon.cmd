@echo off
setlocal
set "LOG=%ProgramData%\ANON\Logs\first-logon.log"
if not exist "%ProgramData%\ANON\Logs" mkdir "%ProgramData%\ANON\Logs" >nul 2>&1
>>"%LOG%" echo [%date% %time%] ANON first-logon bootstrap
if exist "C:\ProgramData\ANON\Shell\Bootstrap\ANON.Shell.Bootstrap.exe" (
  start "ANON OS" /min "C:\ProgramData\ANON\Shell\Bootstrap\ANON.Shell.Bootstrap.exe"
  >>"%LOG%" echo [%date% %time%] Bootstrap launched
) else (
  >>"%LOG%" echo [%date% %time%] Bootstrap missing; leaving Windows Explorer available
)
exit /b 0
