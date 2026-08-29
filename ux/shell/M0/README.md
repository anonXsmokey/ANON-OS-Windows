# ANON Shell M0

M0 is the first runnable shell prototype. It is intentionally a user-mode application and does not replace Explorer, alter system policy, or require administrator privileges.

## Goals

- Establish the ANON desktop interaction model.
- Prove theme and layout loading.
- Prove visual-performance state switching.
- Provide a safe environment for rapid UI iteration.
- Launch ordinary Windows applications through normal Windows mechanisms.

## Current status

This prototype is a product-development surface, not the final shell and not an ISO component yet.

## Technology baseline

The prototype targets WinUI 3 / Windows App SDK because Microsoft currently recommends that stack for new native Windows desktop applications. Windows App SDK 2.4.0 is the current stable release at the time of this specification.

## Safety boundary

M0 must not:

- modify registry settings
- disable services
- alter Windows security configuration
- replace system shell registration
- install kernel drivers
- require cloud services
- require ANON AI

Those capabilities belong to later, separately tested subsystems.
