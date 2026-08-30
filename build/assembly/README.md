# ANON OS Windows — ISO Assembly & Release

This directory contains the controlled Windows image assembly and release-validation layer. Microsoft Windows installation media is supplied by the builder and is not redistributed by this repository.

## Pipeline

1. Build and publish `ANON.Shell.M0` for `win-x64`.
2. Supply a properly licensed Windows installation ISO.
3. `build-iso.ps1` mounts the source, validates the selected WIM/ESD index, injects the ANON shell, configures offline Winlogon, commits the image, and emits bootable ISO media.
4. `validate-iso.ps1` checks boot files, install image, ANON payload, marker, architecture, and SHA-256.
5. `vm-smoke-test.ps1` provides the VirtualBox boot harness.
6. `validate-vm.ps1` creates a non-falsifying validation manifest; merely detecting a VM runner never counts as a PASS.
7. `finalize-release.ps1` is the final gate. It refuses approval unless ShellBuild, IsoStructure, VmBoot, WindowsInstall, FirstLogon, and ShellSmoke are all `PASS`.

## Local commands

```powershell
dotnet publish ux/shell/M0/ANON.Shell.M0.csproj -c Release -r win-x64 --self-contained true
.
\build\assembly\build-iso.ps1 -WindowsIso <windows.iso> -ShellPublish <publish-dir>
.\build\assembly\validate-iso.ps1 -IsoPath <anon-os.iso> -ExpectedArchitecture x64
.\build\assembly\release-manifest.ps1 -IsoPath <anon-os.iso>
.\build\assembly\vm-smoke-test.ps1 -IsoPath <anon-os.iso>
.\build\assembly\finalize-release.ps1 -ManifestPath <RELEASE-MANIFEST.json> -VmResultPath <VM-RESULT.json>
```

## Release policy

A generated ISO is a **candidate**. It is not a working release until a clean VM has completed Windows installation, reached first logon, launched ANON Shell, and passed the shell smoke test. The finalizer changes the manifest to `RELEASE_APPROVED` only after every gate is explicitly `PASS`.

`assemble-anon.ps1` remains an engineering staging helper and intentionally does not emit release media.
