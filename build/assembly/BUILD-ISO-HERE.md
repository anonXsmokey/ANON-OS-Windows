# Final build point

The repository-side ISO pipeline is prepared, but a final working ISO cannot be produced from GitHub alone because Windows installation media and Windows ADK tooling are external inputs.

On the Windows build machine, after pulling `main`:

```powershell
powershell -ExecutionPolicy Bypass -File .\build\anon-build.ps1 -Mode Inventory
powershell -ExecutionPolicy Bypass -File .\build\anon-build.ps1 -Mode Build -WindowsIso "C:\path\to\Windows.iso" -Architecture x64
```

Then validate the emitted ISO:

```powershell
powershell -ExecutionPolicy Bypass -File .\build\assembly\validate-iso.ps1 -IsoPath "C:\path\to\ANON-OS-Windows-*.iso" -ExpectedArchitecture x64
powershell -ExecutionPolicy Bypass -File .\build\assembly\vm-smoke-test.ps1 -IsoPath "C:\path\to\ANON-OS-Windows-*.iso"
```

A release is only valid after a clean VM installation and first-logon shell test. Do not use an ISO that has only passed structural checks.
