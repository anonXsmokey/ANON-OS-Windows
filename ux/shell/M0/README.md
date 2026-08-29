# ANON Shell M0 — Developer Build

M0 is the first runnable shell prototype. It is intentionally a user-mode application and does not replace Explorer, alter system policy, or require administrator privileges.

## Developer environment

- Windows 10/11 development machine
- Visual Studio 2026 with .NET desktop development and Windows App SDK tooling, or an equivalent supported setup
- Windows App SDK 2.4.x stable line

Microsoft's current Windows app documentation identifies Windows App SDK as the native Windows desktop application stack and lists 2.4.0 as the current stable release in August 2026.

## Goals

- Establish the ANON desktop interaction model.
- Prove theme and layout loading.
- Prove visual-performance state switching.
- Provide a safe environment for rapid UI iteration.
- Launch ordinary Windows applications through normal Windows mechanisms.
- Demonstrate Gaming Mode state transitions without modifying the host OS.

## Current status

This prototype is a product-development surface, not the final shell and not an ISO component yet.

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
