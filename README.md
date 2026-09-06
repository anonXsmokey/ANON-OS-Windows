<div align="center">

<img src="docs/site/assets/anon-hero.svg" alt="ANON OS" width="100%" />

# ⚡ ANON OS // WINDOWS

### **KEEP WINDOWS. REMOVE THE NOISE. BUILD FOR GAMERS.**

<img src="https://img.shields.io/badge/PLATFORM-WINDOWS%2011%2025H2-0078D4?style=for-the-badge&labelColor=05070d" />
<img src="https://img.shields.io/badge/ARCH-x64-45e7ff?style=for-the-badge&labelColor=05070d" />
<img src="https://img.shields.io/badge/FOCUS-GAMING-9a6cff?style=for-the-badge&labelColor=05070d" />
<img src="https://img.shields.io/badge/AI-LOCAL%20FIRST-45e7ff?style=for-the-badge&labelColor=05070d" />

**A Windows-native desktop environment engineered around focus, measurable performance, gaming control and optional local intelligence.**

[⚡ COMMAND CENTER](docs/site/index.html) · [ARCHITECTURE](docs/PLATFORM-STRATEGY.md) · [PERFORMANCE](docs/PERFORMANCE-FIRST.md) · [ANON AI](docs/ANON-AI.md)

</div>

---

## ◈ WHAT ANON OS IS

ANON OS is a Windows-native gaming environment, not a replacement kernel or compatibility layer. Windows stays underneath for driver and application compatibility; ANON owns the desktop experience, gaming controls, telemetry and optional local AI.

```text
SEARCH → LAUNCH → MEASURE → CONTROL → PLAY → RESTORE
```

The project is built around **measurable state changes, reversible policies and explicit release gates** instead of unexplained optimizer folklore.

## 🎮 GAMING-FIRST CORE

```text
GAME LAUNCH
    ↓
IDENTIFY → COMPATIBILITY → PROFILE
    ↓
SAFE SESSION POLICY
    ↓
TELEMETRY + GAMING MODE
    ↓
RESTORE PREVIOUS STATE
```

Implemented foundations include:

- Gaming Mode lifecycle and state
- Game profile engine
- Compatibility database
- Benchmark harness
- Production game-session policies
- Session restoration path
- Live CPU / memory telemetry
- Animated **ANON Performance Pet** system-tray companion

## 🖥️ ANON DESKTOP

- Cinematic dark-glass visual language
- Search-first launcher for Apps, Games, Files and System
- Persistent bottom dock
- Live performance telemetry
- Visible Gaming Mode state
- Reduced-motion policy
- Seven visual profiles: `CORE` · `CARBON` · `AURORA` · `PULSE` · `CRIMSON` · `MINIMAL` · `IMMERSIVE`
- **ANON Performance Pet** — lightweight animated system-tray telemetry indicator

### Runtime handoff

```text
Windows Logon
     ↓
FirstLogonCommands
     ↓
ANON.Shell.Bootstrap
     ├── ANON.PerformancePet
     └── ANON.Shell.M0
     ↓
ANON Desktop
```

Bootstrap is a safety boundary: if the ANON shell fails, it can fall back to Windows Explorer instead of leaving the machine without a desktop.

## 🧠 ANON AI

```text
ANON AI → PROVIDER ABSTRACTION → OLLAMA → gemma3:4b
                              └───────→ future providers
```

- Local-first by default
- No shared cloud credential baked into the ISO
- Cloud providers remain optional
- AI failure cannot prevent the desktop from operating
- AI is outside the critical boot path

---

# 📦 WINDOWS 11 25H2 INSTALLER

ANON currently targets the supplied **Windows 11 25H2 English International x64** source and **image index 6 (Windows 11 Pro)**.

```text
WINDOWS 11 25H2 ISO
        ↓
SERVICE install.wim
        ↓
M0 + BOOTSTRAP + PERFORMANCE PET
        ↓
SETUPCOMPLETE + AUTOUNATTEND
        ↓
PATCH boot.wim INDEX 2
        ↓
FORCE LEGACY SETUP ENGINE
        ↓
BIOS + UEFI ISO
        ↓
STRUCTURE / PAYLOAD / UNATTEND VALIDATION
        ↓
CLEAN VM INSTALL
        ↓
OOBE → FIRST LOGON → ANON DESKTOP
```

### 25H2 Setup compatibility

Windows 11 24H2/25H2 media can route normal boot-time installation through the newer ConX Setup experience. Current 25H2 deployment testing shows that forcing the legacy Setup engine is the reliable path when an unattended file must carry the full `windowsPE → specialize → oobeSystem` workflow. citeturn4search0turn5search0

ANON patches **boot.wim index 2** with the WinPE launcher mechanism and deliberately calls the Setup executable inside WinPE's `sources` directory:

```ini
[LaunchApps]
%SYSTEMDRIVE%\sources\setup.exe, /legacy
```

The distinction matters: `X:\sources\setup.exe` is the Setup engine used for this handoff; `X:\setup.exe` is not the executable ANON should use for the legacy bridge. Microsoft documents `\sources\setup.exe` as the WinPE Setup entry point, while `Winpeshl.ini` officially supports launching an application with command-line options. citeturn4search9turn0search0

The answer file is carried at the ISO root as `Autounattend.xml`, embedded in the WinPE image for deterministic diagnostics/fallback, and copied into the installed image's `Windows\Panther\unattend.xml`. Microsoft documents root-media discovery and Panther caching/processing for unattended Setup. citeturn5search0turn5search1

### Why ANON does not use the previous direct `/unattend` bridge

The previous engineering candidate launched the WinPE Setup path with a direct `setup.exe /unattend:X:\Autounattend.xml` command. That candidate produced an **invalid command-line argument** failure on the real VirtualBox boot test. It is rejected as a release candidate.

The current implementation returns to the simpler, established 25H2 legacy-Setup mechanism and validates the **exact executable path and `/legacy` argument** inside `boot.wim` before an ISO is accepted as a candidate.

## 🔧 BUILD LOCALLY

Requirements:

- Windows 10/11 technician machine
- Administrator PowerShell
- .NET 8 SDK
- Windows ADK / DISM
- `oscdimg.exe`
- A legally obtained Windows source ISO

```powershell
cd E:\ANON-OS\ANON-OS-Windows

.\build\assembly\build-local.ps1 `
  -WindowsIso "E:\ANON-OS\ANON-OS-Windows\Win11_iso\Win11_25H2_EnglishInternational_x64_v2.iso" `
  -ImageIndex 6
```

The local build publishes **all three** runtime components, services the WIM, patches `boot.wim`, assembles a dual-firmware ISO, validates the image/payload/Setup chain and writes SHA-256/release metadata.

`oscdimg.exe` discovery supports the existing `E:\Windows Kits\...` layout, standard Windows ADK locations and the `OSCDIMG_PATH` environment variable.

### GitHub Actions

`.github/workflows/build-anon-os.yml` is a manual ISO build workflow. It accepts a Windows ISO URL, image index and architecture, publishes Shell + Bootstrap + Performance Pet, builds the ISO, validates it and uploads candidate artifacts.

---

# 🧪 RELEASE GATES

A static validator passing does **not** make an ISO a final release.

| Gate | Required result |
|---|---|
| Shell build | Shell, Bootstrap and Performance Pet publish |
| ISO structure | BIOS + UEFI structure valid |
| WIM | Target image index exists and mounts |
| Payload | All ANON runtime binaries present |
| Setup | `SetupComplete.cmd` present |
| Unattend | Media + WinPE + Panther answer-file paths valid |
| WinPE | `X:\sources\setup.exe, /legacy` bridge present in `boot.wim` index 2 |
| VM boot | ISO reliably reaches Setup |
| Windows install | Clean installation completes |
| OOBE | Intended unattended configuration is consumed |
| First logon | Bootstrap launches automatically |
| Shell smoke | M0 + Performance Pet start |
| Gaming | Gaming/session policies work on clean install |
| Recovery | Restore path verified |

**Release decision: BLOCKED until the VM gates pass.**

## 🖥️ VM VALIDATION

Always test the newest ISO on a clean VM:

1. Create a fresh VM disk.
2. Attach the newly generated ANON ISO.
3. Boot from the ISO.
4. Confirm Windows Setup starts without a command-line error.
5. Confirm the intended unattended Setup path is consumed.
6. Complete installation/OOBE.
7. Reach the first desktop.
8. Reboot once.
9. Confirm Bootstrap, M0 and Performance Pet start automatically.
10. Verify Gaming Mode/profile/session behavior.
11. Verify Explorer fallback by safely terminating the ANON shell process.
12. Record failures before making manual changes.

If Setup fails, inspect:

```text
X:\Windows\Panther
C:\$Windows.~BT\Sources\Panther
C:\Windows\Panther
```

Useful logs: `setupact.log`, `setuperr.log`, `cbs_unattend.log`.

## 🔐 RELEASE INTEGRITY

Every generated candidate records:

- Source ISO SHA-256
- Output ISO SHA-256
- Target architecture
- Target WIM index
- Installer bridge mode
- Payload validation state
- Release-gate state

The release manifest intentionally remains **blocked** until real VM evidence is supplied. The finalization script only changes the manifest to `RELEASE_APPROVED` when the VM result contains PASS for boot, Windows installation, first logon and shell smoke gates.

---

# 🧱 PROJECT ARCHITECTURE

```text
ANON-OS-Windows/
├── core/                    system intelligence + transformation
├── performance/             measurement + optimization
├── gaming/                  profiles + session policies
├── compatibility/           app/game intelligence
├── control-center/          persistent user controls
├── anon-ai/                 provider layer
├── recovery/                recovery + rollback manifests
├── installer/               install/recovery logic
├── tests/                   validation
├── ux/shell/M0/             desktop shell
├── ux/shell/Bootstrap/      safe shell handoff
├── ux/shell/PerformancePet/ telemetry companion
├── build/assembly/          WIM + ISO pipeline
└── docs/                    architecture + design documentation
```

## 🛡️ SAFETY MODEL

```text
PREFLIGHT → BACKUP → POLICY → CHANGE → VERIFY → ROLLBACK
```

No shared cloud credentials are committed to the image. Recovery manifests are checksum-validatable and describe reversible operations where supported.

## 📜 STATUS

**Channel:** private alpha / engineering build  
**Milestone:** installer hardening + VM release gate  
**Current blocker:** real clean-install, OOBE, first-logon and shell validation on the generated Windows 11 25H2 ISO.

The feature foundation spans system, performance, gaming, experience, AI and release layers. The project will not label a build "final" until the real VM gates have been demonstrated.

---

<div align="center">

## ⚡ ANON OS // WINDOWS

**THE WINDOWS GAMING PLATFORM WE WISH EXISTED.**

Built on Windows. Designed around control. Measured before release.

</div>
