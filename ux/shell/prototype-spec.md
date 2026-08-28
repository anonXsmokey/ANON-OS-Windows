# ANON Shell Prototype — M0

## Objective

Create a real interactive desktop prototype that proves the ANON interaction model before any default-shell replacement work.

## Prototype surfaces

```text
┌──────────────────────────────────────────────────────────────┐
│ ANON MARK   SEARCH                         STATUS / TIME     │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│                    ANON DESKTOP SPACE                        │
│                                                              │
│   WIDGET CARD                         SYSTEM CARD             │
│                                                              │
│                                                              │
│              RECENT / FAVORITE APPLICATIONS                  │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  ANON   FILES   APPS   GAMES   SYSTEM        AI             │
└──────────────────────────────────────────────────────────────┘
```

This is a functional prototype layout, not the final visual design.

## Required interactions

- launch a normal Windows application
- open the ANON launcher
- open and close a widget
- switch between layout presets
- switch visual-performance preset
- toggle reduced motion
- enter a simulated game-session state
- observe visual policy reduction
- restore desktop state

## Prototype constraints

- no destructive system modifications
- no registry changes
- no replacement of Explorer yet
- no elevated privileges required for the basic prototype
- no AI dependency
- no network dependency for core shell behavior

## Acceptance criteria

The prototype is successful when a user can understand the desktop without instructions, change its layout and visual profile, launch ordinary Windows applications, and see a measurable difference between visual-performance modes.

## Later milestones

The prototype will evolve into:

`Desktop → Full Shell → Default Shell → ISO Integration`

Each transition requires compatibility, performance, and recovery validation.
