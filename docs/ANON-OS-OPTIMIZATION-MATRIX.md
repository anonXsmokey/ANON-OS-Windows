# ANON OS — Optimization Matrix

This matrix separates safe, reversible optimization from changes that damage Windows capability.

| Area | ANON policy | Default | Gaming session |
|---|---|---:|---:|
| Windows Update | Keep servicing infrastructure intact | ON | ON |
| Defender | Keep protection intact | ON | ON |
| Firewall | Keep network protection intact | ON | ON |
| Core networking | Never disable | ON | ON |
| Audio/input/device services | Never disable | ON | ON |
| GPU/chipset drivers | User/Windows managed | ON | ON |
| ANON animations | Reduce when useful | ON | OFF in Competitive |
| ANON background indexing | Opportunistic | ON | Reduced |
| Capture overhead | User controlled | Stock | Reduced when requested |
| Power policy | Respect Windows policy | Balanced | High performance when selected |
| Game Mode | Supported by Windows | ON | ON |
| Process priority | Per-game/session only | Stock | Above Normal when configured |
| Processor affinity | Per-game/session only | Stock | Explicit profile only |
| Recovery snapshot | Before stateful change | ON | ON |

## Anti-tweak rule

A tweak is rejected when its claimed benefit cannot be measured, when it permanently removes a dependency, or when rollback is unclear.

This prevents ANON from becoming another collection of internet registry tweaks. The goal is lower background contention and better consistency while preserving Windows compatibility.
