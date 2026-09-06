# ANON OS Windows — Gaming Benchmark Protocol

ANON does not claim FPS gains without measurements. This protocol makes performance claims reproducible.

## Capture

Record:

- CPU and GPU model
- Driver version
- RAM capacity and speed
- Storage device/model
- Windows build
- Game version
- Resolution, API and graphics preset
- V-Sync / VRR state
- Background applications
- ANON profile

## Metrics

For each title, capture at minimum:

- Average FPS
- 1% low FPS
- 0.1% low FPS
- Frame-time consistency
- CPU utilization
- GPU utilization
- System memory usage
- GPU memory usage

## Test protocol

1. Fresh boot.
2. Wait for startup activity to settle.
3. Run a fixed benchmark or repeatable in-game route.
4. Run at least three passes.
5. Discard only a documented warm-up pass.
6. Compare the same machine/settings with ANON disabled and enabled.
7. Record variance rather than only the best run.

## Acceptance

A gaming change should not be marketed as a performance improvement unless the result is repeatable and the change does not introduce crashes, broken updates, device problems, anti-cheat issues or recovery regressions.

## Recommended test matrix

Use several workload classes instead of one game:

- CPU-bound competitive title
- GPU-bound AAA title
- DX12 title
- DX11 title
- game with kernel-level anti-cheat
- game with heavy shader compilation

ANON's release status remains based on evidence from the target machine, not synthetic claims.
