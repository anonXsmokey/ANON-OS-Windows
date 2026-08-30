# ISO Assembly Stage

This directory contains the controlled assembly layer for ANON OS Windows.

The intended product is a Windows installation image with ANON shell integration. The repository does **not** contain Microsoft Windows binaries or installation media.

## Required host inputs

1. A supported Windows installation ISO supplied by the builder.
2. Windows ADK deployment tools, including DISM and `oscdimg.exe`.
3. Administrator PowerShell.
4. A Release build of `ANON.Shell.M0`.
5. A VM capable of UEFI boot for validation.

## Current safety gate

`assemble-anon.ps1` currently produces an **assembly staging tree**, not a falsely-labelled release ISO. Installer integration must be completed against the selected Windows edition/index before `oscdimg` is allowed to create release media.

The release process must verify:

- WIM/ESD source index and architecture.
- Image mount/unmount integrity.
- ANON payload placement and startup integration.
- Recovery/rollback path.
- UEFI bootability.
- Clean installation in a VM.
- First-logon ANON shell startup.
- Basic Files/Apps/Games/System functionality.
- SHA-256 checksum of the final ISO.

No release artifact should be described as a working ISO until those checks pass.
