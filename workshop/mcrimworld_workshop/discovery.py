"""Local environment discovery for the McRimworld Workshop.

Everything here is read-only: it looks for Steam, RimWorld, mods, Workshop
content, config, saves and the .NET SDK on the current Windows machine and
reports what it finds. It never writes, downloads or modifies anything.
"""

from __future__ import annotations

import json
import os
import re
import subprocess
from dataclasses import asdict, dataclass, field
from pathlib import Path

try:
    import winreg  # type: ignore[import-not-found]
except ImportError:  # pragma: no cover - exercised only off Windows
    winreg = None

RIMWORLD_APPID = "294100"
KNOWN_DLC_FOLDERS = {
    "Royalty": "Ludeon.RimWorld.Royalty",
    "Ideology": "Ludeon.RimWorld.Ideology",
    "Biotech": "Ludeon.RimWorld.Biotech",
    "Anomaly": "Ludeon.RimWorld.Anomaly",
    "Odyssey": "Ludeon.RimWorld.Odyssey",
}


@dataclass
class SteamInfo:
    found: bool
    install_path: str | None = None
    library_paths: list[str] = field(default_factory=list)


@dataclass
class RimWorldInfo:
    found: bool
    install_path: str | None = None
    managed_path: str | None = None
    mods_path: str | None = None
    workshop_content_path: str | None = None
    installed_dlc: list[str] = field(default_factory=list)


@dataclass
class UserDataInfo:
    found: bool
    root: str | None = None
    config_path: str | None = None
    saves_path: str | None = None
    scenarios_path: str | None = None
    save_names: list[str] = field(default_factory=list)


@dataclass
class DotnetInfo:
    found: bool
    version: str | None = None
    sdks: list[str] = field(default_factory=list)


@dataclass
class EnvironmentReport:
    steam: SteamInfo
    rimworld: RimWorldInfo
    user_data: UserDataInfo
    dotnet: DotnetInfo

    def to_dict(self) -> dict:
        return {
            "steam": asdict(self.steam),
            "rimworld": asdict(self.rimworld),
            "user_data": asdict(self.user_data),
            "dotnet": asdict(self.dotnet),
        }


def discover_steam() -> SteamInfo:
    install_path = None
    if winreg is not None:
        for hive, subkey in (
            (winreg.HKEY_CURRENT_USER, r"Software\Valve\Steam"),
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\WOW6432Node\Valve\Steam"),
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Valve\Steam"),
        ):
            try:
                with winreg.OpenKey(hive, subkey) as key:
                    value, _ = winreg.QueryValueEx(key, "SteamPath" if hive == winreg.HKEY_CURRENT_USER else "InstallPath")
                    install_path = value.replace("/", "\\")
                    break
            except OSError:
                continue

    if install_path is None:
        for candidate in (r"C:\Program Files (x86)\Steam", r"C:\Program Files\Steam"):
            if Path(candidate).is_dir():
                install_path = candidate
                break

    if install_path is None:
        return SteamInfo(found=False)

    libraries = [install_path]
    vdf_path = Path(install_path) / "steamapps" / "libraryfolders.vdf"
    if vdf_path.is_file():
        try:
            text = vdf_path.read_text(encoding="utf-8", errors="ignore")
            for match in re.finditer(r'"path"\s*"([^"]+)"', text):
                lib = match.group(1).replace("\\\\", "\\")
                if lib not in libraries:
                    libraries.append(lib)
        except OSError:
            pass

    return SteamInfo(found=True, install_path=install_path, library_paths=libraries)


def discover_rimworld(steam: SteamInfo) -> RimWorldInfo:
    candidates: list[Path] = []
    for lib in steam.library_paths:
        candidates.append(Path(lib) / "steamapps" / "common" / "RimWorld")

    install_path = next((c for c in candidates if c.is_dir()), None)
    if install_path is None:
        return RimWorldInfo(found=False)

    managed = install_path / "RimWorldWin64_Data" / "Managed"
    mods = install_path / "Mods"

    workshop_content = None
    for lib in steam.library_paths:
        candidate = Path(lib) / "steamapps" / "workshop" / "content" / RIMWORLD_APPID
        if candidate.is_dir():
            workshop_content = candidate
            break

    data_dir = install_path / "Data"
    installed_dlc = [
        package_id
        for folder, package_id in KNOWN_DLC_FOLDERS.items()
        if (data_dir / folder).is_dir()
    ]

    return RimWorldInfo(
        found=True,
        install_path=str(install_path),
        managed_path=str(managed) if managed.is_dir() else None,
        mods_path=str(mods) if mods.is_dir() else None,
        workshop_content_path=str(workshop_content) if workshop_content else None,
        installed_dlc=installed_dlc,
    )


def discover_user_data() -> UserDataInfo:
    user_profile = os.environ.get("USERPROFILE")
    if not user_profile:
        return UserDataInfo(found=False)

    root = Path(user_profile) / "AppData" / "LocalLow" / "Ludeon Studios" / "RimWorld by Ludeon Studios"
    if not root.is_dir():
        return UserDataInfo(found=False)

    config = root / "Config"
    saves = root / "Saves"
    scenarios = root / "Scenarios"
    save_names = sorted(p.stem for p in saves.glob("*.rws")) if saves.is_dir() else []

    return UserDataInfo(
        found=True,
        root=str(root),
        config_path=str(config) if config.is_dir() else None,
        saves_path=str(saves) if saves.is_dir() else None,
        scenarios_path=str(scenarios) if scenarios.is_dir() else None,
        save_names=save_names,
    )


def discover_dotnet() -> DotnetInfo:
    try:
        version = subprocess.run(
            ["dotnet", "--version"], capture_output=True, text=True, timeout=15, check=False
        )
        sdks = subprocess.run(
            ["dotnet", "--list-sdks"], capture_output=True, text=True, timeout=15, check=False
        )
    except (OSError, subprocess.SubprocessError):
        return DotnetInfo(found=False)

    if version.returncode != 0:
        return DotnetInfo(found=False)

    sdk_lines = [line.strip() for line in sdks.stdout.splitlines() if line.strip()] if sdks.returncode == 0 else []
    return DotnetInfo(found=True, version=version.stdout.strip(), sdks=sdk_lines)


def discover_environment() -> EnvironmentReport:
    steam = discover_steam()
    rimworld = discover_rimworld(steam)
    user_data = discover_user_data()
    dotnet = discover_dotnet()
    return EnvironmentReport(steam=steam, rimworld=rimworld, user_data=user_data, dotnet=dotnet)


def render_report(report: EnvironmentReport) -> str:
    lines: list[str] = ["McRimworld Workshop - Environment Report", "=" * 42]

    lines.append(f"Steam:            {'FOUND' if report.steam.found else 'NOT FOUND'}")
    if report.steam.found:
        lines.append(f"  Install path:   {report.steam.install_path}")
        lines.append(f"  Libraries:      {len(report.steam.library_paths)}")
        for lib in report.steam.library_paths:
            lines.append(f"    - {lib}")

    lines.append(f"RimWorld:         {'FOUND' if report.rimworld.found else 'NOT FOUND'}")
    if report.rimworld.found:
        lines.append(f"  Install path:   {report.rimworld.install_path}")
        lines.append(f"  Managed DLLs:   {report.rimworld.managed_path or 'MISSING'}")
        lines.append(f"  Mods folder:    {report.rimworld.mods_path or 'MISSING'}")
        lines.append(f"  Workshop content: {report.rimworld.workshop_content_path or 'NONE FOUND'}")
        lines.append(f"  Installed DLC:  {', '.join(report.rimworld.installed_dlc) or 'none detected'}")

    lines.append(f"User data:        {'FOUND' if report.user_data.found else 'NOT FOUND'}")
    if report.user_data.found:
        lines.append(f"  Root:           {report.user_data.root}")
        lines.append(f"  Config:         {report.user_data.config_path or 'MISSING'}")
        lines.append(f"  Saves:          {report.user_data.saves_path or 'MISSING'} ({len(report.user_data.save_names)} save(s))")
        lines.append(f"  Scenarios:      {report.user_data.scenarios_path or 'MISSING'}")

    lines.append(f".NET SDK:         {'FOUND ' + report.dotnet.version if report.dotnet.found else 'NOT FOUND'}")
    if report.dotnet.found and report.dotnet.sdks:
        for sdk in report.dotnet.sdks:
            lines.append(f"    - {sdk}")

    return "\n".join(lines)


def report_as_json(report: EnvironmentReport) -> str:
    return json.dumps(report.to_dict(), indent=2)
