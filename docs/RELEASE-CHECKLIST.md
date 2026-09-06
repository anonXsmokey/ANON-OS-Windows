# ANON OS Windows — Release Checklist

## Build integrity

- [ ] Source ISO SHA-256 recorded
- [ ] Target image index verified
- [ ] Shell / Bootstrap / Performance Pet published
- [ ] install.wim mounts and commits cleanly
- [ ] boot.wim index 2 mounts and commits cleanly
- [ ] BIOS and UEFI boot files present
- [ ] ISO SHA-256 recorded

## Installer

- [ ] `boot.wim` contains `Windows\\System32\\winpeshl.ini`
- [ ] WinPE launches `X:\\sources\\setup.exe /legacy`
- [ ] Media-root `Autounattend.xml` is present
- [ ] WinPE answer-file copy is present
- [ ] Panther answer-file copy is present
- [ ] `/IMAGE/INDEX` points to the requested image
- [ ] `oobeSystem` and product configuration validate

## Windows runtime

- [ ] Clean VM boots ISO without Setup command-line error
- [ ] Windows installation completes
- [ ] OOBE completes
- [ ] First logon completes
- [ ] ANON Bootstrap starts automatically
- [ ] ANON shell starts automatically
- [ ] Performance Pet starts once
- [ ] Explorer fallback works
- [ ] Reboot preserves expected shell behavior

## Gaming

- [ ] Gaming Mode toggles visibly
- [ ] Performance Pet reflects gaming state
- [ ] Game session policy applies only to the requested process
- [ ] Previous process priority/affinity is restored
- [ ] No system-wide update/security service was permanently disabled
- [ ] At least one repeatable benchmark suite is recorded

## AI

- [ ] AI can be disabled without affecting desktop startup
- [ ] Local provider failure does not block shell
- [ ] Tool-changing actions require confirmation
- [ ] No provider credential is baked into the image

## Release decision

**RELEASE_APPROVED only after every required runtime checkbox is evidenced.** Static build success is not runtime proof.
