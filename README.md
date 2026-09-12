<div align="center">

<img src="docs/site/assets/anon-hero.svg" alt="ANON OS" width="100%" />

# ⚡ ANON OS // WINDOWS

### **KEEP WINDOWS. REMOVE THE NOISE. BUILD FOR GAMERS.**

<img src="https://img.shields.io/badge/WINDOWS%2011-25H2-0078D4?style=for-the-badge&labelColor=05070d" />
<img src="https://img.shields.io/badge/ARCH-x64-45e7ff?style=for-the-badge&labelColor=05070d" />
<img src="https://img.shields.io/badge/GAMING-FIRST-9a6cff?style=for-the-badge&labelColor=05070d" />
<img src="https://img.shields.io/badge/AI-LOCAL%20FIRST-45e7ff?style=for-the-badge&labelColor=05070d" />
<img src="https://img.shields.io/badge/RELEASE-VM%20GATED-67e8b0?style=for-the-badge&labelColor=05070d" />

**A Windows-native gaming environment engineered around focus, measurable performance, reversible optimization, premium UX and optional local intelligence.**

[⚡ COMMAND CENTER](docs/site/index.html) · [ARCHITECTURE](docs/PLATFORM-STRATEGY.md) · [GAMING](docs/GAMING-PERFORMANCE-POLICY.md) · [AI](docs/ANON-AI-EXPERIENCE.md)

</div>

---

## ◈ THE IDEA

ANON OS is deliberately **Windows-native**. Windows remains the compatibility foundation for games, launchers, anti-cheat, drivers and peripherals; ANON owns the desktop experience, gaming control layer, telemetry, automation and optional AI.

```text
DISCOVER → LAUNCH → MEASURE → OPTIMIZE → PLAY → RESTORE
```

ANON is not a bag of registry tweaks. A policy is accepted only when it is measurable, reversible and compatible with the rest of Windows.

## 🎮 GAMING-FIRST

```text
GAME DETECT
   ↓
PROFILE + COMPATIBILITY
   ↓
SESSION POLICY
   ├─ power preference
   ├─ Game Mode preference
   ├─ ANON UI reduction
   ├─ capture/overlay policy
   └─ per-game process policy
   ↓
TELEMETRY + BENCHMARK
   ↓
AUTOMATIC RESTORE
```

The gaming foundation includes game profiles, session-scoped process controls, restoration, Gaming Mode lifecycle, compatibility data, benchmark protocol and Performance Pet telemetry.

### Performance philosophy

ANON does **not** ship an aggressive “disable everything” preset. Windows Update, Defender, Firewall, networking, audio, input, device support, servicing and drivers remain available. The Competitive profile concentrates on reversible ANON-side overhead and explicit session controls instead of permanently deleting Windows capabilities.

See the full decision matrix: [ANON Optimization Matrix](docs/ANON-OS-OPTIMIZATION-MATRIX.md).

## 🖥️ THE ANON DESKTOP

- Cinematic dark-glass visual language
- Search-first launcher for apps, games, files and settings
- Persistent keyboard-friendly dock
- Live CPU / memory telemetry
- Gaming Mode state shared with the Performance Pet
- Reduced-motion support
- Visual profiles: `CORE` · `CARBON` · `AURORA` · `PULSE` · `CRIMSON` · `MINIMAL` · `IMMERSIVE`
- Animated **ANON Performance Pet** system-tray companion
- First-run onboarding surface
- Explorer fallback when the ANON shell cannot safely start

```text
WINDOWS LOGON
     ↓
FIRST LOGON CONFIGURATION
     ↓
ANON.Shell.Bootstrap
     ├── Performance Pet
     └── ANON.Shell.M0
             ↓
       ANON DESKTOP
```

Bootstrap is intentionally a safety boundary: if ANON cannot start reliably, it falls back to Explorer instead of leaving the user without a desktop.

## 🧠 ANON AI — FROM CHATBOT TO DESKTOP INTELLIGENCE

```text
ASK → UNDERSTAND → PLAN → CONFIRM → EXECUTE → VERIFY
```

ANON AI is provider-neutral and local-first.

**Ask:** Windows, gaming, ANON settings and project questions.

**Understand:** summarize logs, benchmark results and configuration.

**Optimize:** propose a measurable change and explain the trade-off before applying it.

**Create:** generate scripts, profiles, benchmark definitions and documentation in an isolated workspace.

**Recover:** explain failed changes and guide restoration from recorded state.

The architecture supports local Ollama/OpenAI-compatible endpoints and optional cloud providers. It does not bundle a third-party provider or require provider credentials in the operating image.

AI actions have explicit risk classes and a tool boundary. System changes require confirmation rather than silent execution.

## ✨ EXPERIENCE LAYER

ANON is intended to feel like a product, not an ISO remaster:

- named visual profiles
- custom launcher and desktop surfaces
- first-run onboarding
- integrated gaming surface
- Performance Pet
- live system telemetry
- command-center web presentation
- premium SVG motion graphics
- reduced-motion support
- keyboard-first navigation
- recovery-aware system changes

The project deliberately separates the **critical boot path** from optional intelligence so an AI failure cannot prevent Windows or the ANON desktop from operating.

## 📦 WINDOWS 11 25H2 INSTALLER

```text
SOURCE WINDOWS 11 25H2
        ↓
PUBLISH SHELL / BOOTSTRAP / PET
        ↓
SERVICE install.wim INDEX 6
        ↓
SETUPCOMPLETE + AUTOUNATTEND
        ↓
PATCH boot.wim INDEX 2
        ↓
X:\sources\setup.exe /legacy
        ↓
BIOS + UEFI ISO
        ↓
STATIC VALIDATION
        ↓
CLEAN VM VALIDATION
        ↓
RELEASE ONLY AFTER ALL GATES PASS
```

ANON targets the supplied Windows 11 25H2 English International x64 source with image index 6 (Windows 11 Pro).

The installer bridge deliberately uses the WinPE `sources\setup.exe` legacy client and carries the answer file at media root, inside WinPE and in the installed Panther directory. The build and validator enforce those paths.

The previous direct unattended command-line bridge caused the real VM candidate to fail with an invalid command-line argument. It is not part of the current release path.

## 🛡️ WHAT ANON WILL NOT DO

ANON will not permanently:

- disable Windows Update as a “FPS tweak”
- remove Defender or Firewall for an FPS claim
- delete Windows servicing components
- ship mystery GPU/chipset/network drivers
- disable networking/audio/input/device support needed by games
- apply unexplained registry folklore
- force AI or cloud credentials into the operating image

Those changes create compatibility and recovery debt. ANON instead prefers controlled session policies with measurable rollback.

## 🧪 BENCHMARK / RELEASE MODEL

```text
BUILD
 ↓
STATIC CHECKS
 ↓
BOOT
 ↓
INSTALL
 ↓
OOBE
 ↓
FIRST LOGON
 ↓
SHELL + PET
 ↓
GAMING / RECOVERY
 ↓
REPEATABLE BENCHMARK
 ↓
RELEASE
```

| Gate | Requirement |
|---|---|
| Build | Core + Shell + Bootstrap + Performance Pet publish |
| ISO | BIOS + UEFI media is structurally valid |
| WIM | Correct image index mounts |
| Payload | All ANON runtime binaries exist |
| Installer | SetupComplete + Autounattend + Panther |
| WinPE | Exact `X:\sources\setup.exe /legacy` bridge |
| VM Boot | Setup starts without command-line failure |
| Install | Clean installation completes |
| OOBE | Intended answer-file configuration is consumed |
| First Logon | Bootstrap starts |
| Shell | M0 + Performance Pet start |
| Gaming | Gaming Mode/profile/session restoration works |
| Recovery | State can be restored |
| Benchmark | Results are repeatable and recorded |

**Static validation is not release approval.**

## 🔐 INTEGRITY

Every candidate records source and output SHA-256, architecture, image index, installer bridge and validation state. Generated Windows media is not committed to the repository.

## 🌌 PROJECT ARCHITECTURE

```text
ANON-OS-Windows/
├── core/                 system intelligence + transformation
├── performance/          measurement + optimization
├── gaming/               profiles + session policies
├── compatibility/       app/game intelligence
├── control-center/       user controls
├── ai/                   provider + action boundary
├── recovery/             rollback + recovery manifests
├── installer/            installation support
├── tests/                validation
├── ux/shell/M0/          ANON desktop
├── ux/shell/Bootstrap/   safe runtime handoff
├── ux/shell/PerformancePet/
├── docs/site/            premium command center
└── build/assembly/       WIM + ISO + release gates
```

## 📚 ENGINEERING REFERENCES

ANON uses composable subsystem architecture and provider-abstraction patterns as references, not bundled dependencies.

## 🚦 STATUS

**Channel:** public engineering candidate  
**Primary milestone:** Gaming-first desktop + AI architecture + installer hardening  
**Release status:** **BLOCKED until a clean VM passes every runtime gate.**

The repository intentionally refuses to call an ISO “final” merely because the static validator passes.

---

<div align="center">

## ⚡ ANON OS // WINDOWS

### **THE WINDOWS GAMING PLATFORM WE WISH EXISTED.**

**Built on Windows. Designed around control. Measured before release.**

</div>
