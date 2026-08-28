# ANON OS Windows Build System

This directory will contain the reproducible Windows ISO build pipeline.

## Intended workflow

```text
SOURCE -> VERIFY -> MOUNT -> SERVICE -> CONFIGURE -> RECOVERY -> ASSEMBLE -> TEST -> RELEASE
```

The repository stores the build logic and manifests, not Microsoft's proprietary installation media.

## Planned tools

- Windows ADK
- WinPE add-on
- DISM
- Windows System Image Manager where answer-file validation is required
- PowerShell build orchestration
- ISO/media assembly tooling
- VirtualBox automated validation

Microsoft's Windows ADK provides deployment and performance tooling, including the Windows Performance Toolkit. Current ADK versions must be selected and pinned per supported Windows build.

## Design rule

Every transformation must be represented by a versioned manifest/policy. The builder must support a dry-run/inventory phase before making changes.

## Future output

```text
out/
  ANON-OS-Windows-<build>-<arch>.iso
  SHA256SUMS.txt
  BUILD-MANIFEST.json
  RELEASE-NOTES.md
```

Release artifacts are produced only after automated validation succeeds.
