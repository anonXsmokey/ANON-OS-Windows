# M0 Local Build

## Purpose

M0 is the Windows-native ANON desktop shell. It is a user-mode application and is not the operating-system ISO itself.

## Requirements

- Windows 10/11 x64 development environment
- .NET 8 SDK
- Windows desktop development support for WPF

M0 intentionally does not require Windows App SDK / WinUI 3 tooling or a separate Windows App Runtime installation.

## Build

From `ux/shell/M0`:

```powershell
dotnet restore .\ANON.Shell.M0.csproj --runtime win-x64
dotnet build .\ANON.Shell.M0.csproj -c Release -r win-x64
```

## Publish

```powershell
dotnet publish .\ANON.Shell.M0.csproj -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -p:PublishSingleFile=false -p:PublishReadyToRun=false
```

## Test checklist

1. Application starts without crashing on a clean Windows installation.
2. ANON dashboard renders using WPF only.
3. Search opens ANON destinations.
4. Play toggles Gaming Mode.
5. Visual Profile and Reduced Motion controls remain responsive.
6. Files opens Windows Explorer.
7. Applications and Games open the Windows application inventory.
8. System opens Windows Settings.
9. ANON AI opens without making cloud access mandatory.
10. Closing or failing M0 leaves Explorer available as the recovery shell.
11. CPU and memory telemetry are recorded for later performance baselines.

## Status

The commands above are build instructions for the repository. A release claim still requires an actual clean-VM test of boot, OOBE, first logon, M0 startup, reboot persistence and shell recovery.
