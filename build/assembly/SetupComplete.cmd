@echo off
setlocal
set "BASE=C:\ProgramData\ANON"
set "LOG=%BASE%\Logs\setup-complete.log"
if not exist "%BASE%\Logs" mkdir "%BASE%\Logs" >nul 2>&1
>>"%LOG%" echo [%date% %time%] ANON SetupComplete started
rem Keep the stock Windows shell/session plumbing intact. ANON is launched
rem from the Default user profile after Explorer/userinit has initialized.
reg add "HKLM\SOFTWARE\ANON\OS" /v Installed /t REG_DWORD /d 1 /f >>"%LOG%" 2>&1
reg add "HKLM\SOFTWARE\ANON\OS" /v Version /t REG_SZ /d "Windows-native V1" /f >>"%LOG%" 2>&1
reg add "HKLM\SOFTWARE\ANON\OS" /v ShellActivationPending /t REG_DWORD /d 1 /f >>"%LOG%" 2>&1

rem Provision ANON Bootstrap into the Default user profile. This avoids putting
rem a long-running process in FirstLogonCommands/oobeSystem, which runs before
rem Windows presents the desktop. Every newly created user inherits this Run key.
set "DEFAULT_HIVE=C:\Users\Default\NTUSER.DAT"
if exist "%DEFAULT_HIVE%" (
    >>"%LOG%" echo [%date% %time%] Loading Default user hive
    reg load "HKU\ANON_DEFAULT" "%DEFAULT_HIVE%" >>"%LOG%" 2>&1
    if not errorlevel 1 (
        reg add "HKU\ANON_DEFAULT\Software\Microsoft\Windows\CurrentVersion\Run" /v ANONShell /t REG_SZ /d "C:\ProgramData\ANON\Shell\Bootstrap\ANON.Shell.Bootstrap.exe" /f >>"%LOG%" 2>&1
        reg unload "HKU\ANON_DEFAULT" >>"%LOG%" 2>&1
        >>"%LOG%" echo [%date% %time%] Default user ANON launcher provisioned
    ) else (
        >>"%LOG%" echo [%date% %time%] WARNING: could not load Default user hive; Windows shell remains available
    )
) else (
    >>"%LOG%" echo [%date% %time%] WARNING: Default user hive missing; Windows shell remains available
)

>>"%LOG%" echo [%date% %time%] ANON installation marked; launcher deferred to normal user session startup
exit /b 0
