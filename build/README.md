# ANON OS Windows Build System

The build system turns a user-supplied, supported Windows installation source into ANON OS Windows installation media. The repository stores build logic and manifests, not Microsoft's proprietary installation media.

## Pipeline

```text
SOURCE -> VERIFY -> MOUNT -> SERVICE -> CONFIGURE -> RECOVERY -> ASSEMBLE -> TEST -> RELEASE
```

## Local build prerequisites

- Supported Windows host
- Windows ADK + WinPE add-on
- DISM
- `oscdimg.exe`
- .NET 8 SDK
- A legally obtained Windows installation ISO matching the supported target
- Administrator PowerShell for servicing/mount operations
- VirtualBox (or another supported VM runner) for release validation

## First command

Run an inventory before modifying anything:

```powershell
powershell -ExecutionPolicy Bypass -File .\build\anon-build.ps1 -Mode Inventory
```

Build/release stages remain gated until the source ISO, servicing policy, recovery policy, assembly configuration, and VM validation are all present. A build must never silently produce an unvalidated ISO.

## Output contract

```text
build/out/
  ANON-OS-Windows-<build>-<arch>.iso
  SHA256SUMS.txt
  BUILD-MANIFEST.json
  RELEASE-NOTES.md
```

The final release gate requires a successful shell build, successful image assembly, successful boot/install validation, and SHA-256 generation.
