# ANON OS Custom Installer

ANON OS uses a custom installation experience rather than exposing the stock Windows OOBE as the product UI.

## Design boundary

ANON-owned components:

- boot branding and entry flow
- installer UI and navigation
- hardware/preflight screen
- edition/disk selection UX
- installation progress and diagnostics
- ANON configuration selection
- first-boot provisioning
- recovery entry points
- post-install ANON shell handoff
- installer logs and validation

Microsoft-owned components remain the underlying Windows installation engine, kernel, drivers, and licensed Windows payload. ANON OS does not redistribute or replace those proprietary binaries.

## Build flow

```text
Licensed Windows source
        |
        +--> custom WinPE/installer payload
        |       +--> ANON Setup UI
        |       +--> preflight
        |       +--> disk/install orchestration
        |       +--> recovery tools
        |
        +--> Windows image
                +--> ANON system payload
                +--> ANON shell
                +--> ANON first-boot provisioning

                    |
                    v
              ANON OS installer ISO
```

## Release rule

The installer is not considered complete until it has been tested from ISO boot through disk selection, Windows deployment, first boot, ANON provisioning, shell launch, reboot persistence, and recovery entry.
