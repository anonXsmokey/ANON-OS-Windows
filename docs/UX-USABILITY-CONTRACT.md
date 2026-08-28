# ANON OS Windows — UX Usability Contract

## Core rule

ANON may look radically different from stock Windows, but it must remain easy to understand and operate.

## Familiar capability, original presentation

ANON preserves the desktop capabilities users expect from Windows while replacing the visual language, layouts, themes, widgets, wallpaper system, shell surfaces, and interaction presentation with an original ANON experience.

## Progressive disclosure

Common tasks stay one interaction away. Advanced controls appear only when requested. The user should not need to understand Windows internals to install an application, manage a file, launch a game, change a theme, or recover the system.

## Input

Primary support:

- keyboard
- mouse / trackpad
- touch where supported by the device
- controller-friendly gaming surfaces where practical

## Personalization

Users can choose:

- desktop layout templates
- themes
- wallpapers
- animated/static visual modes
- widgets
- taskbar/dock behavior
- animation intensity
- transparency/blur intensity
- visual density
- gaming overlays
- AI presence

## Visual performance spectrum

```text
PERFORMANCE  ◀──────────────●──────────────▶  EXPERIENCE
                         ANON BALANCED
```

The setting is not a single switch. It controls a policy set covering animation, composition effects, wallpaper behavior, widget refresh, transparency, blur, and background visual work.

## Game-aware behavior

A user may configure ANON to automatically switch visual policy when a game starts and restore the desktop policy when the game exits.

Example:

```text
Desktop: Immersive
        ↓
Game starts
        ↓
Gaming visual policy
        ↓
Game exits
        ↓
Immersive restored
```

## Accessibility

The shell must support keyboard navigation, scalable text, high contrast, reduced motion, reduced transparency, readable status indicators, and alternatives to audio-only feedback.

## Failure behavior

Critical operations must provide clear explanations, confirmation for destructive actions, visible progress, and recovery options. Never require the user to understand an internal policy name to recover their machine.
