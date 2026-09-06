# ANON OS Windows — Production Build

This directory contains the controlled Windows image assembly and release-validation layer. Microsoft Windows installation media is supplied by the builder and is not redistributed by this repository.

## Production pipeline

1. Build and publish `ANON.Shell.M0`, `ANON.Shell.Bootstrap`, and `ANON.PerformancePet` for `win-x64`.
2. Supply a properly licensed Windows 11 25H2 installation ISO.
3. `build-iso.ps1` copies the source media, validates the selected WIM/ESD index, injects the three ANON runtime payloads, installs `SetupComplete.cmd`, writes image-specific `Autounattend.xml`, adds the Panther fallback, patches `boot.wim` index 2 with the exact `X:\sources\setup.exe, /legacy` WinPE bridge, and emits BIOS + UEFI ISO media.
4. `validate-iso.ps1` mounts the generated ISO and verifies boot files, WIM index, ANON payloads, SetupComplete, Panther answer file, WinPE bridge, embedded answer file, architecture marker and SHA-256.
5. `vm-smoke-test.ps1` provides a non-falsifying VirtualBox boot harness.
6. `validate-vm.ps1` creates a validation manifest; merely detecting a VM runner never counts as a PASS.
7. `finalize-release.ps1` refuses approval unless the required build/install/first-logon/shell gates are all `PASS`.

## Gaming optimization policy

ANON intentionally avoids destructive “debloat” folklore. It does not permanently kill Windows Update, Defender, Firewall, Search, Bluetooth/WLAN, or core servicing components, and it does not ship third-party driver packs. Gaming controls are session-oriented, visible and reversible. See `docs/SECURITY-GAMING-POLICY.md` and `docs/GAMING-BENCHMARKS.md`.

## AI architecture

AI is provider-neutral and non-critical to the desktop. Local providers can be used without cloud credentials baked into the image. Optional developer integrations such as Free Claude Code stay outside the critical OS path and require user-owned provider configuration. See `docs/AI-ARCHITECTURE.md`.

## Local command

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

## 25H2 installer bridge

The generated `boot.wim` index 2 contains:

```ini
[LaunchApps]
%SYSTEMDRIVE%\sources\setup.exe, /legacy
```

The media-root `Autounattend.xml` is the primary installation answer file; WinPE and installed Panther copies are deterministic fallbacks/diagnostics.

## Release policy

A generated ISO is a **candidate**, not a release. Final approval requires a clean VM to complete Windows installation, OOBE/first logon, automatic ANON Bootstrap + desktop + Performance Pet startup, Gaming Mode/session behavior and recovery validation.
