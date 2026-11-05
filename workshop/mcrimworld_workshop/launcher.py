"""High-level operations the CLI exposes: list/inspect/validate/activate/launch/restore.

This module wires discovery.py, manifest.py and profiles.py together. It does
not implement scenario/pawn/world generation (M2/M3) or one-click new-game
start (M4) — launch() starts RimWorld through Steam using whatever mod
profile is currently active, nothing more.
"""

from __future__ import annotations

import os
import subprocess
import sys
from pathlib import Path

from . import discovery, profiles
from .manifest import ManifestResult, load_manifest

REPO_ROOT = Path(__file__).resolve().parents[2]
EXPERIENCES_DIR = REPO_ROOT / "workshop" / "experiences"
STATE_DIR = REPO_ROOT / "workshop" / "state"
BACKUPS_DIR = STATE_DIR / "backups"
RIMWORLD_APPID = "294100"


def list_experience_paths(experiences_dir: Path = EXPERIENCES_DIR) -> list[Path]:
    if not experiences_dir.is_dir():
        return []
    return sorted(experiences_dir.glob("*.json"))


def find_experience(experience_id: str, experiences_dir: Path = EXPERIENCES_DIR) -> Path | None:
    candidate = experiences_dir / f"{experience_id}.json"
    return candidate if candidate.is_file() else None


def load_installed_mods(env: discovery.EnvironmentReport) -> dict[str, profiles.InstalledMod]:
    labeled_dirs: list[tuple[Path, str]] = []
    if env.rimworld.mods_path:
        labeled_dirs.append((Path(env.rimworld.mods_path), "local"))
    if env.rimworld.workshop_content_path:
        labeled_dirs.append((Path(env.rimworld.workshop_content_path), "workshop"))
    return profiles.scan_installed_mods(labeled_dirs)


def validate_experience(result: ManifestResult, env: discovery.EnvironmentReport) -> profiles.ValidationReport | None:
    if not result.valid:
        return None
    installed = load_installed_mods(env)
    return profiles.validate_requirements(result.data, installed)


def activate_experience(result: ManifestResult, env: discovery.EnvironmentReport) -> dict:
    if not result.valid:
        raise ValueError(f"Cannot activate an invalid manifest: {result.errors}")
    if not env.user_data.found or not env.user_data.config_path:
        raise RuntimeError("RimWorld user config folder was not found; run `env` to see what's missing.")

    config_dir = Path(env.user_data.config_path)
    BACKUPS_DIR.mkdir(parents=True, exist_ok=True)
    return profiles.activate_profile(
        manifest=result.data,
        config_dir=config_dir,
        backup_root=BACKUPS_DIR,
        state_dir=STATE_DIR,
    )


def restore_profile(env: discovery.EnvironmentReport) -> dict:
    if not env.user_data.found or not env.user_data.config_path:
        raise RuntimeError("RimWorld user config folder was not found; run `env` to see what's missing.")
    return profiles.restore_previous(Path(env.user_data.config_path), STATE_DIR)


def launch_rimworld(env: discovery.EnvironmentReport) -> str:
    """Start RimWorld through Steam using whatever profile is currently
    active in ModsConfig.xml. This is NOT a one-click new-game/resume flow
    (that is M4) — it is equivalent to pressing Play in Steam.
    """
    if not env.steam.found:
        raise RuntimeError("Steam was not found; cannot launch RimWorld.")

    uri = f"steam://rungameid/{RIMWORLD_APPID}"
    if sys.platform == "win32":
        os.startfile(uri)  # noqa: S606 - launching the local Steam client by design
    else:
        subprocess.run(["xdg-open", uri], check=False)
    return uri


def inspect_text(result: ManifestResult) -> str:
    lines = [f"Manifest: {result.path}"]
    if not result.data:
        lines.append("  FAILED TO PARSE")
        lines.extend(f"  - {e}" for e in result.errors)
        return "\n".join(lines)

    data = result.data
    lines.append(f"  id:      {data.get('id')}")
    lines.append(f"  name:    {data.get('name')}")
    lines.append(f"  version: {data.get('version')}  (schema {data.get('schemaVersion')})")
    lines.append(f"  status:  {data.get('status', 'specified')}")
    lines.append(f"  RimWorld: {data.get('rimworld', {}).get('version')}")
    dlc = data.get("rimworld", {}).get("dlc", {})
    if dlc.get("required"):
        lines.append(f"  DLC required: {', '.join(dlc['required'])}")
    if dlc.get("optional"):
        lines.append(f"  DLC optional: {', '.join(dlc['optional'])}")
    lines.append(f"  Mods ({len(data.get('mods', []))}):")
    for mod in data.get("mods", []):
        flag = "required" if mod.get("required", True) else "optional"
        lines.append(f"    - {mod['packageId']} ({mod.get('name', '')}) [{flag}, source={mod.get('source', 'workshop')}]")
    for custom in data.get("customMods", []):
        lines.append(f"    - {custom['packageId']} [custom, source={custom.get('sourcePath')}]")

    if result.errors:
        lines.append("  VALIDATION ERRORS:")
        lines.extend(f"    - {e}" for e in result.errors)
    else:
        lines.append("  Schema: valid")

    return "\n".join(lines)


def validation_text(report: profiles.ValidationReport | None) -> str:
    if report is None:
        return "  Skipped (manifest is not schema-valid)."
    lines = []
    lines.append(f"  Overall: {'OK' if report.ok else 'ISSUES FOUND'}")
    if report.missing:
        lines.append(f"  Missing required mods: {', '.join(report.missing)}")
    if report.unmet_dependencies:
        lines.append("  Unmet dependencies:")
        lines.extend(f"    - {pid} needs {dep}" for pid, dep in report.unmet_dependencies)
    if report.incompatible_pairs:
        lines.append("  Incompatible mod pairs both required:")
        lines.extend(f"    - {a} <-> {b}" for a, b in report.incompatible_pairs)
    if report.missing_workshop:
        lines.append("  Missing Workshop subscriptions:")
        for item in report.missing_workshop:
            url = f"https://steamcommunity.com/sharedfiles/filedetails/?id={item['workshopId']}" if item.get("workshopId") else "(no workshopId given)"
            lines.append(f"    - {item['name']} ({item['packageId']}): {url}")
    return "\n".join(lines)
