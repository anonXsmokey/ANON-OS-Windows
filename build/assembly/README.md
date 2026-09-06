# ANON OS Windows — ISO Assembly & Release

This directory contains the controlled Windows image assembly and release-validation layer. Microsoft Windows installation media is supplied by the builder and is not redistributed by this repository.

## Pipeline

1. Build and publish `ANON.Shell.M0`, `ANON.Shell.Bootstrap`, and `ANON.PerformancePet` for `win-x64`.
2. Supply a properly licensed Windows 11 25H2 installation ISO.
3. `build-iso.ps1` copies the source media, validates the selected WIM/ESD index, injects the three ANON runtime payloads, installs `SetupComplete.cmd`, writes the image-specific `Autounattend.xml`, adds the Panther fallback, patches `boot.wim` index 2 with the exact `X:\sources\setup.exe, /legacy` WinPE bridge, and emits BIOS + UEFI ISO media.
4. `validate-iso.ps1` mounts the generated ISO and verifies boot files, WIM index, ANON payloads, SetupComplete, Panther answer file, WinPE bridge, embedded answer file, architecture marker and SHA-256.
5. `vm-smoke-test.ps1` provides a non-falsifying VirtualBox boot harness.
6. `validate-vm.ps1` creates a validation manifest; merely detecting a VM runner never counts as a PASS.
7. `finalize-release.ps1` is the final gate. It refuses approval unless ShellBuild, IsoStructure, VmBoot, WindowsInstall, FirstLogon, and ShellSmoke are all `PASS`.

## Local commands

The easiest local entrypoint is:

```powershell
cd E:\ANON-OS\ANON-OS-Windows

.\build\assembly\build-local.ps1 `
  -WindowsIso "E:\ANON-OS\ANON-OS-Windows\Win11_iso\Win11_25H2_EnglishInternational_x64_v2.iso" `
  -ImageIndex 6
```

The individual assembly command requires all three published runtime directories:

```powershell
.\build\assembly\build-iso.ps1 `
  -WindowsIso <windows.iso> `
  -ShellPublish <shell-publish-dir> `
  -BootstrapPublish <bootstrap-publish-dir> `
  -PerformancePetPublish <performance-pet-publish-dir>
```

Then validate:

```powershell
.\build\assembly\validate-iso.ps1 -IsoPath <anon-os.iso> -ExpectedArchitecture x64 -ExpectedImageIndex 6
.\build\assembly\release-manifest.ps1 -IsoPath <anon-os.iso>
.\build\assembly\vm-smoke-test.ps1 -IsoPath <anon-os.iso>
.\build\assembly\finalize-release.ps1 -ManifestPath <RELEASE-MANIFEST.json> -VmResultPath <VM-RESULT.json>
```

## 25H2 installer bridge

The generated `boot.wim` index 2 contains:

```ini
[LaunchApps]
%SYSTEMDRIVE%\sources\setup.exe, /legacy
```

This deliberately uses the Setup executable inside WinPE's `sources` directory. The current 25H2 ConX/legacy split is the reason this bridge exists. The media-root `Autounattend.xml` remains the primary installation answer file; the boot image and installed Panther copies are deterministic fallbacks/diagnostic copies.

## Windows media policy

Do **not** commit the Windows ISO, `install.wim`, or `install.esd` to this repository. The root `.gitignore` excludes these large installation artifacts. Keep the licensed source ISO on the build machine and pass its path to `build-local.ps1`.

## Release policy

A generated ISO is a **candidate**. It is not a release until a clean VM has completed Windows installation, reached OOBE/first logon, launched ANON Bootstrap, started the ANON desktop and Performance Pet, and passed the remaining gaming/recovery gates. Static validation alone never changes the release decision.

`assemble-anon.ps1` remains an engineering staging helper and intentionally does not emit release media.
