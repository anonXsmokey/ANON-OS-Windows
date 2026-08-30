# ANON OS Recovery Environment

The recovery layer is deliberately separate from the normal shell. Its job is to restore an ANON transformation state when the desktop shell cannot start.

## Recovery contract

1. Detect ANON installation and its state manifest.
2. Verify the manifest checksum before using it.
3. Restore only recorded reversible operations.
4. Write a recovery log outside the transformed user profile when possible.
5. Never assume a failed optimization is safe to repeat.
6. Return control to Windows recovery if ANON recovery cannot establish a safe state.

The first implementation milestone is the manifest and validation contract. Bootable WinRE integration remains a release-gated task and is not claimed complete until tested in a VM.
