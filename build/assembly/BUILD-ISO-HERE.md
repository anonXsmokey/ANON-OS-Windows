# Final build point

The repository-side ISO pipeline is prepared, but a Windows ISO still requires the builder's licensed Windows source media and local Windows ADK/DISM tooling.

On the Windows build machine, after pulling `main`:

```powershell
cd E:\ANON-OS\ANON-OS-Windows

git pull origin main

Remove-Item .\build\out -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item .\build\shell-publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item .\build\bootstrap-publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item .\build\performance-pet-publish -Recurse -Force -ErrorAction SilentlyContinue

.\build\assembly\build-local.ps1 `
  -WindowsIso "E:\ANON-OS\ANON-OS-Windows\Win11_iso\Win11_25H2_EnglishInternational_x64_v2.iso" `
  -ImageIndex 6
```

The build publishes Shell + Bootstrap + Performance Pet, services `install.wim` index 6, patches `boot.wim` index 2 with the exact 25H2 bridge:

```ini
[LaunchApps]
%SYSTEMDRIVE%\sources\setup.exe, /legacy
```

It then validates the ISO structure, WIM, payloads, SetupComplete, Panther answer file, embedded answer file and exact legacy Setup bridge before producing release metadata.

A release is only valid after a clean VM installation, OOBE/first-logon, ANON Bootstrap startup, ANON Shell startup, Performance Pet startup, gaming validation and recovery validation. Do not use an ISO that has only passed structural checks.
