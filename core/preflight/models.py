from dataclasses import dataclass, asdict
from typing import Optional


@dataclass
class WindowsProfile:
    product_name: str
    build: str
    architecture: str
    edition: str
    is_supported: bool


@dataclass
class HardwareProfile:
    cpu: str
    logical_processors: int
    memory_gb: float
    gpu: Optional[str]
    system_drive_free_gb: float


@dataclass
class PreflightReport:
    windows: WindowsProfile
    hardware: HardwareProfile
    warnings: list[str]
    blockers: list[str]

    def to_dict(self):
        return asdict(self)
