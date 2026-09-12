# ANON OS — Customization & Product Audit

Release-oriented checklist for the current ANON desktop customization layer.

## Desktop identity
- [x] ANON-branded home surface
- [x] Reusable ANON visual language
- [x] Search/launcher surface
- [x] Files, Apps, Games, AI and System entry points
- [x] First-run personalization
- [x] Persistent theme/profile/motion preferences
- [x] Responsive launcher search
- [x] Reduced-motion support
- [x] Primary accessibility names
- [x] Honest unavailable telemetry states

## Shell safety
- [x] Explorer remains the recovery desktop
- [x] ANON starts as a per-user launcher
- [x] Bootstrap single-instance protection
- [x] Missing binaries are logged
- [x] No Winlogon Shell replacement
- [x] ANON stays outside the critical Windows boot path

## Product surfaces
- [x] Files local-volume navigation
- [x] Bounded file enumeration and failure states
- [x] Apps/Games discovery off the UI thread
- [x] System performance/hardware/storage/runtime surface
- [x] Control Center for profile/gaming/AI state
- [x] Optional AI with safe provider failure behavior
- [x] AI prompt bound and cancellation on close

## Remaining runtime gates
- [ ] Fresh ISO built from current branch
- [ ] Clean VM boot
- [ ] Clean install
- [ ] First login
- [ ] ANON shell launch
- [ ] Core navigation
- [ ] Gaming enter/exit
- [ ] Gaming reboot persistence
- [ ] Explorer recovery
- [ ] AI local-provider validation
- [ ] Reboot validation

## Release rule
Static checks are not release proof. The exact ISO intended for release must pass the clean-VM installation, first-login, ANON shell, navigation, gaming, recovery and reboot gates before it is called release-ready.
