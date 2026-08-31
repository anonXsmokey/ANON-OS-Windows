# ANON OS Custom Installer

ANON OS uses a custom installation experience rather than exposing the stock Windows OOBE as the product UI.

## Product experience

ANON-OS is Windows-based, but its ANON-owned user experience is designed as an independent product. Fedora/GNOME is a **UI inspiration reference only**; ANON-OS is not built on Fedora and does not use Fedora branding or artwork.

The target experience combines a clean GNOME-like information hierarchy with the premium cinematic ANON visual language: dark-first surfaces, restrained violet/indigo accents, rounded cards, optional glass/depth effects, coherent iconography, custom wallpapers, motion profiles and data-driven themes.

The same design system must be shared by the installer, shell, Control Center, Gaming Mode, ANON AI, login/lock screens and recovery environment.

See [`docs/design/ANON-DESIGN-SYSTEM.md`](../docs/design/ANON-DESIGN-SYSTEM.md) for the visual specification and [`installer/config/experience.json`](config/experience.json) for machine-readable experience defaults.

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
- themes, wallpapers and ANON visual components

Microsoft-owned components remain the underlying Windows installation engine, kernel, drivers, and licensed Windows payload. ANON OS does not redistribute or replace those proprietary binaries.

## Installer stages

```text
WELCOME
   ↓
PREFLIGHT
   ↓
LICENSE
   ↓
DISK
   ↓
INSTALL
   ↓
CONFIGURE
   ↓
FIRST BOOT
   ↓
VALIDATION
```

Every destructive operation is explicit and recoverable where technically possible. The core installation path must work without network access.

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
