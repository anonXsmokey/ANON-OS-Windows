# ANON OS Windows — ISO Product Specification

## Product definition

ANON OS Windows is a bootable ISO product built from an appropriately licensed Windows source. The repository contains the build system, manifests, policies, ANON components, validation logic, and documentation—not Microsoft's proprietary installation media.

## Installation experience

The installer is an ANON experience from boot:

```text
BOOT
 ↓
ANON INTRO
 ↓
HARDWARE / BUILD PREFLIGHT
 ↓
INSTALLATION TARGET
 ↓
PROFILE SELECTION
 ↓
ANON INSTALL
 ↓
FIRST BOOT
 ↓
ANON WELCOME
 ↓
CONTROL CENTER
```

## First-boot goals

A successful installation should arrive at a coherent ANON environment rather than requiring a long manual tweak checklist.

The user chooses preferences such as performance profile, privacy options, optional AI presence, and gaming behavior. Unsafe or incompatible choices are blocked or explained.

## System shell

The final experience should use an ANON shell layer rather than simply applying a theme to the stock Windows desktop. Windows compatibility remains underneath; the visible interaction model is ANON.

## ISO variants

Planned release profiles:

- **ANON Lite** — low-overhead configuration for modest hardware
- **ANON Gaming** — primary desktop/gaming configuration
- **ANON Ultra** — feature-rich configuration for capable systems

These are configuration targets, not separate operating systems. The build pipeline should share as much validated code as possible.

## Update strategy

The project must account for Windows feature updates and cumulative updates. A release cannot assume that a single static set of modifications remains correct forever. Policies therefore require build targeting and validation.

## Recovery

The installation and first-boot experience must retain a clear path into Windows recovery and ANON recovery tooling. Failed transformations must not leave the machine dependent on a manual internet guide to recover.

## Compatibility promise

The product target is maximum practical compatibility with supported modern Windows 10/11 applications, games, drivers, launchers, peripherals, and security software. Unsupported cases must be identified rather than hidden.
