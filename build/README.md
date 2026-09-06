# ANON OS Windows Build System

The build system turns a user-supplied, supported Windows installation source into ANON OS Windows installation media. The repository stores build logic and manifests, not Microsoft's proprietary installation media.

## Production pipeline

```text
SOURCE ISO
   ↓
VERIFY / INVENTORY
   ↓
PUBLISH SHELL + BOOTSTRAP + PERFORMANCE PET
   ↓
SERVICE install.wim
   ↓
SETUPCOMPLETE + AUTOUNATTEND + PANTHER FALLBACK
   ↓
PATCH boot.wim INDEX 2
   ↓
X:\sources\setup.exe /legacy
   ↓
BIOS + UEFI ISO
   ↓
STATIC RELEASE VALIDATION
   ↓
CLEAN VM VALIDATION
   ↓
FINALIZE ONLY AFTER ALL GATES PASS
```

## Local build prerequisites

- Supported Windows host
- Windows ADK / DISM
- `oscdimg.exe`
- .NET 8 SDK
- A legally obtained Windows 11 25H2 installation ISO
- Administrator PowerShell for servicing/mount operations
- VirtualBox (or another supported VM runner) for release validation

## First production command

```powershell
cd E:\ANON-OS\ANON-OS-Windows

git pull origin main

.\build\assembly\build-local.ps1 `
  -WindowsIso "E:\ANON-OS\ANON-OS-Windows\Win11_iso\Win11_25H2_EnglishInternational_x64_v2.iso" `
  -ImageIndex 6
```

The local pipeline publishes all three ANON runtime components, services the target image, patches WinPE, assembles the ISO, validates the artifact and creates the release manifest.

## Output contract

```text
build/out/
  ANON-OS-Windows-<build>-x64.iso
  SHA256SUMS.txt
  BUILD-MANIFEST.json
  RELEASE-MANIFEST.json
```

The generated ISO is a **candidate**, not a release. Static validation must pass first; clean VM boot, Windows installation, OOBE/first logon, ANON shell startup, gaming validation and recovery validation are separate release gates.
