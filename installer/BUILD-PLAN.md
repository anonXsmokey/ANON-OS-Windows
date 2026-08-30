# ANON OS Installer Build Plan

## Installer stages

1. **ANON Boot** — branded boot environment and hardware initialization handoff.
2. **ANON Preflight** — CPU, RAM, storage, firmware, Secure Boot/TPM state and compatibility checks.
3. **ANON License** — clearly identifies the underlying Windows licensing requirement.
4. **ANON Disk** — custom disk discovery, partition layout preview, destructive-action confirmation and logging.
5. **ANON Install** — controlled deployment of the selected Windows image with ANON progress/diagnostics.
6. **ANON Configure** — choose Lite/Gaming/Ultra and optional components.
7. **ANON First Boot** — provision ANON services, shell, policies and recovery metadata.
8. **ANON Validation** — verify files, services, shell registration and reboot persistence.
9. **ANON Recovery** — provide an installer-accessible recovery path.

## ISO contents

```text
/boot                 Microsoft boot foundation + ANON boot configuration
/efi                  UEFI boot foundation
/sources              Windows deployment payload
/ANON/installer       ANON installer assets and configuration
/ANON/recovery        ANON recovery assets
/ANON/branding        logos, fonts, UI assets
/ANON/manifests       build and compatibility metadata
```

## Non-negotiable requirements

- No fake release gates.
- No irreversible system transformation without backup/recovery support.
- No undocumented registry modifications.
- No requirement for ANON AI to operate the base system.
- Installer must work without network access for its core installation path.
- Every release ISO receives a SHA-256 manifest.
- Clean VM testing is mandatory before alpha release.

## Important technical boundary

A Windows-based ANON OS can make the **installer, shell, services, configuration, recovery tooling, branding and integration** custom. The Windows kernel, boot components, drivers and proprietary Windows binaries remain Microsoft components supplied under the user's applicable Windows license. A truly every-bit custom operating system would instead require a new kernel, driver stack, bootloader, userspace and hardware ecosystem and is a different project.
