"""Read-only Windows preflight scanner.

This module only gathers information. It never changes Windows configuration.
"""
import platform
import shutil
import subprocess

from .models import HardwareProfile, PreflightReport, WindowsProfile


def _powershell(expression: str) -> str:
    try:
        result = subprocess.run(
            ["powershell.exe", "-NoProfile", "-NonInteractive", "-Command", expression],
            capture_output=True, text=True, timeout=8, check=False,
        )
        return result.stdout.strip()
    except (OSError, subprocess.SubprocessError):
        return ""


def scan_windows() -> WindowsProfile:
    product = _powershell("(Get-CimInstance Win32_OperatingSystem).Caption") or platform.system()
    build = _powershell("(Get-CimInstance Win32_OperatingSystem).BuildNumber") or "unknown"
    edition = _powershell("(Get-CimInstance Win32_OperatingSystem).OperatingSystemSKU") or "unknown"
    arch = _powershell("(Get-CimInstance Win32_OperatingSystem).OSArchitecture") or platform.machine()

    # Initial policy: modern Windows 10/11 era builds are candidates; exact build
    # support is intentionally resolved by the policy database, not guessed here.
    is_supported = "Windows 10" in product or "Windows 11" in product
    return WindowsProfile(product, build, arch, edition, is_supported)


def scan_hardware() -> HardwareProfile:
    cpu = _powershell("(Get-CimInstance Win32_Processor | Select-Object -First 1).Name") or platform.processor() or "unknown"
    logical = int(_powershell("(Get-CimInstance Win32_Processor | Select-Object -First 1).NumberOfLogicalProcessors") or 0)
    memory_bytes = int(_powershell("(Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory") or 0)
    gpu = _powershell("(Get-CimInstance Win32_VideoController | Select-Object -First 1).Name") or None
    drive = shutil.disk_usage("/")
    return HardwareProfile(cpu, logical, round(memory_bytes / (1024**3), 2), gpu, round(drive.free / (1024**3), 2))


def run_preflight() -> PreflightReport:
    windows = scan_windows()
    hardware = scan_hardware()
    warnings = []
    blockers = []

    if not windows.is_supported:
        blockers.append("Unsupported or undetected Windows platform.")
    if hardware.logical_processors and hardware.logical_processors < 2:
        warnings.append("Very low logical CPU count; Gaming profile may be inappropriate.")
    if hardware.memory_gb and hardware.memory_gb < 4:
        warnings.append("Low physical memory; use Lite profile and validate compatibility carefully.")
    if hardware.system_drive_free_gb < 15:
        warnings.append("Low free space on the system drive.")

    return PreflightReport(windows, hardware, warnings, blockers)
