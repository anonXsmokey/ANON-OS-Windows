# ANON Shell M0 — Windows-native desktop shell

M0 is the first production-oriented ANON desktop surface. It runs as a normal user-mode Windows application, keeps Explorer as the recovery shell, and does not depend on the Windows App SDK runtime.

## Stack

- Windows 10/11 x64
- .NET 8
- WPF (`Microsoft.NET.Sdk.WindowsDesktop`)
- Self-contained publish
- Trimming, single-file publish, and ReadyToRun disabled for deterministic deployment

## Goals

- Provide the ANON desktop identity at first user logon.
- Keep startup reliable on clean Windows installations.
- Launch ordinary Windows applications using standard Windows mechanisms.
- Expose Files, Applications, Games, System and ANON AI entry points.
- Provide system CPU and memory telemetry without inventing unsupported GPU values.
- Toggle Gaming Mode using the shared ANON gaming-mode flag.
- Fail safely: if M0 cannot start, Windows Explorer remains available.

## Startup contract

```text
FirstLogonCommands
    -> HKCU\Software\Microsoft\Windows\CurrentVersion\Run
    -> ANON.Shell.Bootstrap.exe
    -> ANON.Shell.M0.exe
```

Bootstrap also starts the Performance Pet and records startup failures under `C:\ProgramData\ANON\Logs`.

## Build

From the repository root:

```powershell
.\build\assembly\build-local.ps1 -WindowsIso ".\Win11_iso\Win11_25H2_EnglishInternational_x64_v2.iso"
```

Or from this directory:

```powershell
dotnet restore .\ANON.Shell.M0.csproj --runtime win-x64
dotnet build .\ANON.Shell.M0.csproj -c Release -r win-x64
dotnet publish .\ANON.Shell.M0.csproj -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -p:PublishSingleFile=false -p:PublishReadyToRun=false
```

## Runtime checklist

1. M0 starts after first user logon.
2. Dashboard renders without WinUI/Windows App SDK dependencies.
3. Search launches ANON destinations.
4. Play toggles Gaming Mode.
5. Files opens Explorer.
6. Applications and Games open standard Windows app inventory.
7. System opens Windows Settings.
8. ANON AI opens the local-first Ollama surface.
9. CPU and memory telemetry remain bounded and truthful.
10. Closing or crashing M0 never disables Explorer recovery.
