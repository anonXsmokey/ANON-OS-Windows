@echo off
setlocal
set "BASE=C:\ProgramData\ANON"
set "LOG=%BASE%\Logs\setup-complete.log"
if not exist "%BASE%\Logs" mkdir "%BASE%\Logs" >nul 2>&1
>>"%LOG%" echo [%date% %time%] ANON SetupComplete started
rem Do not replace Winlogon Shell during SetupComplete. Windows 11 OOBE still
rem needs the stock Explorer shell/session plumbing to finish reliably.
reg add "HKLM\SOFTWARE\ANON\OS" /v Installed /t REG_DWORD /d 1 /f >>"%LOG%" 2>&1
reg add "HKLM\SOFTWARE\ANON\OS" /v Version /t REG_SZ /d "Windows-native V1" /f >>"%LOG%" 2>&1
reg add "HKLM\SOFTWARE\ANON\OS" /v ShellActivationPending /t REG_DWORD /d 1 /f >>"%LOG%" 2>&1
>>"%LOG%" echo [%date% %time%] ANON installation marked; Winlogon shell deferred until first user logon
exit /b 0
