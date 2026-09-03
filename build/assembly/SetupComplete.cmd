@echo off
setlocal
set "BASE=C:\ProgramData\ANON"
set "LOG=%BASE%\Logs\setup-complete.log"
if not exist "%BASE%\Logs" mkdir "%BASE%\Logs" >nul 2>&1
>>"%LOG%" echo [%date% %time%] ANON SetupComplete started
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Shell /t REG_SZ /d "C:\ProgramData\ANON\Shell\Bootstrap\ANON.Shell.Bootstrap.exe" /f >>"%LOG%" 2>&1
reg add "HKLM\SOFTWARE\ANON\OS" /v Installed /t REG_DWORD /d 1 /f >>"%LOG%" 2>&1
reg add "HKLM\SOFTWARE\ANON\OS" /v Version /t REG_SZ /d "Windows-native V1" /f >>"%LOG%" 2>&1
>>"%LOG%" echo [%date% %time%] ANON Winlogon shell configured
exit /b 0
