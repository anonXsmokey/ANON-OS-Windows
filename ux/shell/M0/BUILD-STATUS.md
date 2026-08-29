# M0 Build Status

## Purpose

This file records the verified local build gate for the ANON Shell M0 prototype.

## Verified so far

- Git is installed on the user's Windows environment.
- The private `ANON-OS-Windows` repository was successfully cloned.
- .NET SDK 8.0.424 is installed.
- The earlier build attempt from `C:\Windows\System32` failed because the project file was not in the current working directory.

## Current gate

The repository must now be opened from:

```text
ANON-OS-Windows\ux\shell\M0
```

Then restore and build the M0 project.

## Required commands

```powershell
dotnet restore .\ANON.Shell.M0.csproj
dotnet build .\ANON.Shell.M0.csproj -c Debug -p:Platform=x64
```

## Status

**CLONED:** verified by user.

**SDK READY:** verified by user (`8.0.424`).

**BUILD:** awaiting execution from the correct project directory.

**RUN:** not yet verified.

**VM TEST:** not yet verified.

Do not mark later stages complete without actual evidence from the user's environment or CI.
