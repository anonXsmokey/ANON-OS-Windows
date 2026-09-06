# ANON OS Windows — Gaming System Policy

ANON optimizes for **measurable gaming performance without shipping mystery tweaks**. Policies are evaluated by category, logged, and designed to be reversible.

## Default philosophy

- Keep Windows Update available. Do not ship a permanently broken servicing stack.
- Do not disable antivirus, firewall, BitLocker, SmartScreen, Defender, or core security controls by default.
- Treat VBS / Memory Integrity as a user-selectable performance profile because its cost varies by hardware and workload.
- Do not install a generic "driver pack". GPU, chipset, network and storage drivers remain vendor-controlled and are installed according to hardware compatibility.
- Do not run permanent RAM cleaners, registry cleaners, timer-resolution daemons, disk defragmenters for SSDs, or background booster processes.
- Prefer Windows-native Game Mode, power policy and foreground-game prioritization over undocumented registry folklore.

## Performance profiles

### ANON Balanced

Safe daily-driver profile. Keeps normal Windows security and maintenance behavior.

### ANON Gaming

Session-only game policy. Applies only to the active game process and restores the previous process state when the session ends.

### ANON Competitive

For latency-sensitive workloads. Adds stricter foreground focus and removes optional ANON background activity, while retaining Windows recovery and security mechanisms.

### ANON Maximum

Explicitly advanced profile. May expose documented settings such as Memory Integrity/VBS trade-offs, power-policy changes and startup trimming. Every change must be visible, individually attributable and reversible.

## Never ship as a default

The following are excluded from the base gaming image because they can reduce reliability, break updates/security, or produce no reproducible gaming benefit:

- Windows Update permanently disabled
- Defender permanently disabled
- Firewall disabled
- Security Center disabled
- Search/Indexing globally disabled
- Print Spooler removed
- Bluetooth/WLAN services removed
- Hyper-V components deleted from the image
- random registry "FPS boost" packs
- permanent process-priority hacks
- permanent affinity masks
- third-party driver bundles
- telemetry-removal scripts that delete servicing dependencies

## Evidence standard

A change is a candidate only when one of these is available:

1. Microsoft or vendor documentation supports the behavior.
2. A before/after benchmark demonstrates a repeatable benefit.
3. The change reduces measurable ANON overhead without harming functionality.

No benchmark result is hard-coded into the product without the machine, workload, settings and measurement method being recorded.
