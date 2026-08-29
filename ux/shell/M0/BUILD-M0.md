# M0 Local Build

## Purpose

M0 is the Windows-native ANON Shell prototype. It is not an operating-system ISO and must not be described as one.

## Requirements

- Windows 10 version 1809 or newer for the declared minimum platform target
- .NET 8 SDK
- Visual Studio 2022 with Windows App SDK / WinUI 3 tooling
- x64 Windows environment for the first test

## Build

From `ux/shell/M0`:

```powershell
dotnet restore .\ANON.Shell.M0.csproj
dotnet build .\ANON.Shell.M0.csproj -c Debug -p:Platform=x64
```

## Publish

```powershell
dotnet publish .\ANON.Shell.M0.csproj -c Release -p:Platform=x64 -p:PublishProfile=win-x64
```

## Test checklist

1. Application starts without crashing.
2. ANON visual resources load.
3. Play enters Gaming Mode.
4. Visual Profile changes update runtime state.
5. Reduced Motion disables transitions.
6. Files opens Windows Explorer.
7. Closing the app returns cleanly to Windows.
8. Record CPU and memory at idle for later baseline comparison.

## Status

The commands above are **not claimed to have been executed by the repository agent**. A real build requires a Windows development environment.
