# ISO assembly contract

This directory defines the release-media boundary for ANON OS Windows.

The assembler must operate on a user-supplied Windows installation source and produce a bootable installation ISO. It must not download, redistribute, or embed Microsoft installation media in this repository.

Required stages:

1. Validate source ISO and record SHA-256.
2. Extract/install source image and identify the target Windows edition/index.
3. Mount the target image with DISM.
4. Inject only ANON-owned payloads and configuration.
5. Register the ANON shell/app integration without replacing protected Windows components blindly.
6. Add recovery/rollback metadata.
7. Commit and unmount the image with DISM.
8. Rebuild bootable ISO using `oscdimg` while preserving required BIOS/UEFI boot files.
9. Generate `BUILD-MANIFEST.json`, `SHA256SUMS.txt`, and release notes.
10. Boot the resulting ISO in a VM and verify installer startup before release.

The assembler should fail closed on missing source media, missing ADK tools, failed DISM operations, unsigned/unknown payloads, or failed VM validation.
