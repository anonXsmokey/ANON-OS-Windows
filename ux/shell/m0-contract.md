# ANON Shell M0 — Implementation Contract

## Goal

Build the first runnable ANON desktop prototype as a normal Windows application. It must demonstrate the interaction model without changing the operating system shell.

## Functional surfaces

- desktop canvas
- ANON launcher
- dock/taskbar surface
- system status cards
- widget host
- theme loader
- layout loader
- visual-performance selector
- reduced-motion control
- application launcher
- simulated game-session state

## Runtime states

```text
DESKTOP
  │
  ├── OPEN APP
  ├── OPEN LAUNCHER
  ├── CHANGE THEME
  ├── CHANGE LAYOUT
  └── CHANGE VISUAL MODE
          │
          ▼
      GAME SESSION
          │
          ▼
      REDUCED VISUAL POLICY
          │
          ▼
       GAME EXIT
          │
          ▼
    RESTORE DESKTOP POLICY
```

## Non-goals

M0 will not replace Explorer, modify the registry, install drivers, change services, require administrator access, require an internet connection, or depend on ANON AI.

## Acceptance gates

1. Launches reliably on supported development Windows.
2. Uses the theme and layout schemas already defined by the repository.
3. Switching visual modes changes actual rendering policy.
4. Reduced-motion mode removes nonessential animation.
5. Ordinary Windows applications can be launched from ANON.
6. Game-session simulation changes visual policy and restores it on exit.
7. Shell crash does not prevent the user from returning to the normal Windows desktop.
8. Startup and idle resource usage are recorded for every prototype build.

## Technology baseline

Initial prototype target: native Windows desktop application using WinUI 3 / Windows App SDK, with C# for rapid UI iteration and the option to move performance-critical components to C++ where profiling justifies it. Microsoft currently recommends WinUI 3 + Windows App SDK for new native Windows desktop applications.

## Definition of done

M0 is not considered complete because a window exists. It is complete when the interaction model, customization model, visual-performance switching, Windows application launching, and recovery behavior can be demonstrated on a real Windows installation and measured.
