# ANON OS Windows — Full ISO Build Architecture

## Decision

ANON OS Windows V1 will produce a real bootable ANON OS installation ISO, built from an appropriately licensed Microsoft Windows source image. It is not a post-install-only playbook and it is not a collection of tweaks that the user must manually execute.

The build system will service the Windows image offline, construct installation media, customize Setup/WinPE/WinRE where appropriate, and validate the resulting artifact in virtual machines before release.

Microsoft documents DISM as the primary mechanism for offline Windows image servicing and supports mounting WIM/VHD/FFU images, adding/removing packages and drivers, modifying files, and committing the resulting image. Microsoft also documents offline image servicing as an automation-friendly deployment path. (Microsoft Learn: Modify a Windows image; Mount and modify a Windows image using DISM.)

## Target pipeline

```text
Microsoft Windows source
        |
        v
Source verification + hash
        |
        v
ANON build workspace
        |
        +-- Base image inventory
        +-- Windows build detection
        +-- Component/package inventory
        +-- License/provenance record
        |
        v
Offline servicing
        |
        +-- DISM image changes
        +-- ANON components
        +-- drivers where justified
        +-- packages/features
        +-- configuration
        |
        v
Windows Setup / Unattend
        |
        v
Customized WinPE
        |
        v
Customized WinRE / Recovery
        |
        v
ISO assembly
        |
        v
Integrity + boot validation
        |
        v
Automated VM installation
        |
        v
ANON acceptance tests
        |
        v
SIGNED RELEASE ARTIFACT
```

## Why this is different from a playbook

A playbook modifies an already-installed system. ANON's release artifact should contain the ANON configuration from the beginning, so installation produces the intended environment in one controlled deployment.

A post-install control layer will still exist for updates, optional features, repairs, and user-selected performance profiles, but it is not the primary distribution mechanism.

## Build layers

### 1. Source layer

The builder consumes a user-supplied or otherwise appropriately licensed Microsoft Windows source. ANON does not embed Microsoft's proprietary source media into this repository.

### 2. Image servicing layer

DISM-based offline servicing is the foundation. Microsoft documents mounting and modifying WIM images, committing changes, adding/removing packages and drivers, and performing these operations from a technician environment or WinPE.

### 3. Setup layer

An `unattend.xml` is used only for supported installation customizations. Microsoft documents answer files and multiple Setup configuration passes for Windows deployment. Answer files should be validated with Windows System Image Manager.

### 4. WinPE layer

ANON's installation environment can eventually include diagnostics, hardware detection, recovery utilities, and a branded installation experience. Microsoft documents customization of WinPE through the Windows ADK and WinPE add-on.

### 5. WinRE layer

The recovery environment will be customized alongside the Windows image where necessary. Microsoft recommends updating Windows RE when relevant Windows image changes affect drivers, packages, updates, or recovery functionality.

### 6. ISO layer

The final builder assembles a bootable ISO containing the customized installation media. The exact media-generation mechanism will be selected during implementation and validated against current Windows ADK tooling.

## Build reproducibility

Every release build must record:

- Windows source identity and hash
- supported Windows build
- ADK version
- ANON build version
- transformation policy versions
- included third-party components
- driver package versions
- configuration manifest
- build timestamp
- artifact hash

## Release channels

- `dev` — experimental, unsigned
- `alpha` — internal VM/hardware validation
- `beta` — controlled external testing
- `stable` — public release

## Required validation

Before a stable ISO is published:

1. ISO integrity validation
2. UEFI VM boot test
3. BIOS/legacy compatibility where supported
4. clean installation test
5. first-boot test
6. driver/device enumeration test
7. Windows Update test
8. application compatibility smoke test
9. gaming runtime smoke test
10. recovery test
11. rollback/update test
12. performance baseline comparison

## Non-negotiable

No release ISO is considered successful merely because it boots. It must demonstrate that ANON changes do not create unacceptable regressions in Windows functionality, gaming, security, recovery, or updateability.
