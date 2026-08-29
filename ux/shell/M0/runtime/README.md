# M0 Runtime

M0 keeps desktop state independent from rendering. The renderer consumes a `CompositionSnapshot` generated from theme, layout, visual, wallpaper, and widget state.

## Runtime flow

```text
User action
    ↓
Runtime service
    ↓
DesktopController
    ↓
CompositionSnapshot
    ↓
Renderer
```

This separation allows the UI to evolve without coupling business rules to XAML controls.

## Current status

**IMPLEMENTED:** state/runtime model and composition snapshot.

**NOT YET TESTED:** compilation and execution on the user's Windows VM.

The next local milestone will be requested only after the prototype is packaged sufficiently for a meaningful VM test.
