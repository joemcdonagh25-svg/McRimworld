"""Mod profile manager.

Reads and writes RimWorld's own ModsConfig.xml format (the same shape used
by tools/fix-rimworld-black-screen.ps1), resolves which packageIds are
actually installed (Workshop content + local Mods folder), validates an
experience's mod requirements against that, and can safely back up / apply /
restore a profile.

Nothing here downloads or subscribes to Workshop content — missing items are
reported so the human can subscribe through Steam.
"""

from __future__ import annotations

import json
import shutil
import xml.etree.ElementTree as ET
from dataclasses import dataclass, field
from datetime import datetime, timezone
from pathlib import Path
from xml.dom import minidom

MODS_CONFIG_FILENAME = "ModsConfig.xml"
ACTIVATION_STATE_FILENAME = "activation-state.json"
DEFAULT_RIMWORLD_VERSION = "1.6.0"


@dataclass
class InstalledMod:
    package_id: str
    name: str
    root_dir: str
    source: str  # "workshop" | "local"


@dataclass
class ModsConfigData:
    version: str
    active_mods: list[str]
    known_expansions: list[str]


@dataclass
class ValidationReport:
    missing: list[str] = field(default_factory=list)
    incompatible_pairs: list[tuple[str, str]] = field(default_factory=list)
    unmet_dependencies: list[tuple[str, str]] = field(default_factory=list)
    missing_workshop: list[dict] = field(default_factory=list)

    @property
    def ok(self) -> bool:
        return not (self.missing or self.incompatible_pairs or self.unmet_dependencies)


def read_active_mods(config_dir: Path) -> ModsConfigData | None:
    path = Path(config_dir) / MODS_CONFIG_FILENAME
    if not path.is_file():
        return None
    tree = ET.parse(path)
    root = tree.getroot()
    version = (root.findtext("version") or DEFAULT_RIMWORLD_VERSION).strip()
    active = [li.text.strip() for li in root.findall("./activeMods/li") if li.text]
    known = [li.text.strip() for li in root.findall("./knownExpansions/li") if li.text]
    return ModsConfigData(version=version, active_mods=active, known_expansions=known)


def scan_installed_mods(labeled_dirs: list[tuple[Path, str]]) -> dict[str, InstalledMod]:
    """Scan Mods-folder-shaped directories for About/About.xml files.

    labeled_dirs are (directory, source_label) pairs, e.g. (mods_path, 'local').
    """
    installed: dict[str, InstalledMod] = {}
    for directory, source in labeled_dirs:
        directory = Path(directory)
        if not directory.is_dir():
            continue
        for child in sorted(directory.iterdir()):
            about_path = child / "About" / "About.xml"
            if not about_path.is_file():
                continue
            try:
                about = ET.parse(about_path).getroot()
            except ET.ParseError:
                continue
            package_id = (about.findtext("packageId") or "").strip()
            name = (about.findtext("name") or child.name).strip()
            if not package_id:
                continue
            installed.setdefault(
                package_id.lower(),
                InstalledMod(package_id=package_id, name=name, root_dir=str(child), source=source),
            )
    return installed


def resolve_load_order(manifest: dict) -> list[str]:
    """Return packageIds in an order that satisfies dependsOn/loadAfter/loadBefore,
    or the explicit loadOrder.sequence when the manifest provides one.
    """
    mods = manifest.get("mods", [])
    custom = manifest.get("customMods", [])
    all_ids = [m["packageId"] for m in mods] + [m["packageId"] for m in custom]

    sequence = manifest.get("loadOrder", {}).get("sequence")
    if sequence:
        remainder = [pid for pid in all_ids if pid not in sequence]
        return list(sequence) + remainder

    edges: dict[str, set[str]] = {pid: set() for pid in all_ids}
    for mod in mods:
        pid = mod["packageId"]
        for before in mod.get("dependsOn", []) + mod.get("loadAfter", []):
            if before in edges:
                edges[pid].add(before)
        for after in mod.get("loadBefore", []):
            if after in edges:
                edges[after].add(pid)

    ordered: list[str] = []
    visited: set[str] = set()
    temp_mark: set[str] = set()

    def visit(pid: str) -> None:
        if pid in visited:
            return
        if pid in temp_mark:
            raise ValueError(f"Cyclic load-order constraint involving '{pid}'")
        temp_mark.add(pid)
        for dep in edges.get(pid, ()):
            visit(dep)
        temp_mark.discard(pid)
        visited.add(pid)
        ordered.append(pid)

    for pid in all_ids:
        visit(pid)

    return ordered


def validate_requirements(manifest: dict, installed: dict[str, InstalledMod]) -> ValidationReport:
    report = ValidationReport()
    mods = manifest.get("mods", [])
    required_ids = {m["packageId"] for m in mods if m.get("required", True)}

    for mod in mods:
        pid = mod["packageId"]
        if pid.lower() not in installed and mod.get("source", "workshop") != "builtin":
            if mod.get("required", True):
                report.missing.append(pid)
            if mod.get("source", "workshop") == "workshop":
                report.missing_workshop.append(
                    {"packageId": pid, "name": mod.get("name", pid), "workshopId": mod.get("workshopId")}
                )

    for mod in mods:
        pid = mod["packageId"]
        for dep in mod.get("dependsOn", []):
            if dep not in required_ids and dep.lower() not in installed:
                report.unmet_dependencies.append((pid, dep))
        for incompatible in mod.get("incompatibleWith", []):
            if incompatible in required_ids:
                pair = tuple(sorted((pid, incompatible)))
                if pair not in report.incompatible_pairs:
                    report.incompatible_pairs.append(pair)

    return report


def backup_config(config_dir: Path, backup_root: Path, label: str = "workshop") -> Path | None:
    """Copy ModsConfig.xml (if present) into a timestamped backup folder.
    Returns the backup file path, or None if there was nothing to back up.
    """
    source = Path(config_dir) / MODS_CONFIG_FILENAME
    if not source.is_file():
        return None
    stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    dest_dir = Path(backup_root) / f"{label}-{stamp}"
    dest_dir.mkdir(parents=True, exist_ok=True)
    dest = dest_dir / MODS_CONFIG_FILENAME
    shutil.copy2(source, dest)
    return dest


def write_mods_config(config_dir: Path, mods_config: ModsConfigData) -> Path:
    root = ET.Element("ModsConfigData")
    ET.SubElement(root, "version").text = mods_config.version
    active = ET.SubElement(root, "activeMods")
    for pid in mods_config.active_mods:
        ET.SubElement(active, "li").text = pid
    known = ET.SubElement(root, "knownExpansions")
    for exp in mods_config.known_expansions:
        ET.SubElement(known, "li").text = exp

    rough = ET.tostring(root, encoding="utf-8")
    pretty = minidom.parseString(rough).toprettyxml(indent="  ", encoding="utf-8")

    path = Path(config_dir) / MODS_CONFIG_FILENAME
    path.write_bytes(pretty)
    return path


def _activation_state_path(state_dir: Path) -> Path:
    return Path(state_dir) / ACTIVATION_STATE_FILENAME


def activate_profile(
    manifest: dict,
    config_dir: Path,
    backup_root: Path,
    state_dir: Path,
) -> dict:
    """Back up the current ModsConfig.xml, then write one built from the
    manifest. Returns the activation record that was also persisted to
    state_dir/activation-state.json so restore_previous() can undo it.
    """
    experience_id = manifest["id"]
    existing = read_active_mods(config_dir)
    version = existing.version if existing else DEFAULT_RIMWORLD_VERSION
    known_expansions = existing.known_expansions if existing else sorted(
        {dlc for dlc in manifest.get("rimworld", {}).get("dlc", {}).get("required", [])}
        | set(manifest.get("rimworld", {}).get("dlc", {}).get("optional", []))
    )

    backup_path = backup_config(config_dir, backup_root, label=f"pre-{experience_id}")

    ordered_ids = resolve_load_order(manifest)
    write_mods_config(config_dir, ModsConfigData(version=version, active_mods=ordered_ids, known_expansions=known_expansions))

    record = {
        "experienceId": experience_id,
        "activatedAt": datetime.now(timezone.utc).isoformat(),
        "backupPath": str(backup_path) if backup_path else None,
        "previousActiveMods": existing.active_mods if existing else [],
    }
    state_path = _activation_state_path(state_dir)
    state_path.parent.mkdir(parents=True, exist_ok=True)
    state_path.write_text(json.dumps(record, indent=2), encoding="utf-8")
    return record


def restore_previous(config_dir: Path, state_dir: Path) -> dict:
    """Undo the most recent activate_profile() call by restoring the backup
    it took. Raises FileNotFoundError if there is nothing recorded to undo.
    """
    state_path = _activation_state_path(state_dir)
    if not state_path.is_file():
        raise FileNotFoundError(f"No activation record at {state_path}; nothing to restore.")

    record = json.loads(state_path.read_text(encoding="utf-8"))
    backup_path = record.get("backupPath")
    if not backup_path or not Path(backup_path).is_file():
        raise FileNotFoundError(f"Recorded backup is missing: {backup_path}")

    dest = Path(config_dir) / MODS_CONFIG_FILENAME
    shutil.copy2(backup_path, dest)
    state_path.unlink()
    return record
