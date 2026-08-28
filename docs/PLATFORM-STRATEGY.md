# ANON OS Windows — Platform Strategy

## Decision

V1 will be a Windows-native transformation platform. It will operate on an appropriately licensed Windows installation rather than attempting to recreate the Windows application platform.

## Why Windows-native V1

The primary product requirement is maximum compatibility with modern Windows 10/11 applications, games, drivers, launchers, peripherals, anti-cheat systems, and vendor software. Retaining Windows as the underlying platform gives ANON the strongest compatibility foundation while allowing the project to focus engineering effort on gaming performance and user experience.

## Scope

ANON OS Windows will build:

- pre-flight hardware/build detection
- transformation and policy engine
- performance profiles
- gaming lifecycle management
- per-game configuration
- telemetry and benchmark tooling
- backup/recovery/rollback
- ANON Control Center
- optional ANON AI
- compatibility intelligence
- polished gaming-first UX

## Non-goals for V1

- writing a replacement Windows kernel
- redistributing Microsoft's proprietary Windows binaries without appropriate rights
- claiming universal compatibility without evidence
- shipping unsafe collections of unexplained registry tweaks
- making ANON AI mandatory

## Relationship to ANON-OS

`ANON-OS` is the independent operating-system research project and long-term V2 direction. `ANON-OS-Windows` is the practical Windows-native V1 product. The repositories remain separate.

## Release Philosophy

ANON should transform a supported Windows installation predictably, preserve recoverability, and make measurable improvements without compromising the software ecosystem that users depend on.
