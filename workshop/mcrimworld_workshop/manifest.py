"""Loading and validating experience manifests against experience.schema.json."""

from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path
from typing import Any

from .schema_validate import validate

SCHEMA_PATH = Path(__file__).resolve().parent.parent / "schema" / "experience.schema.json"


@dataclass
class ManifestResult:
    path: Path
    data: dict | None
    errors: list[str]

    @property
    def valid(self) -> bool:
        return self.data is not None and not self.errors


def load_schema() -> dict:
    with open(SCHEMA_PATH, "r", encoding="utf-8") as f:
        return json.load(f)


def load_manifest(path: str | Path) -> ManifestResult:
    """Parse and validate a single experience manifest file."""
    path = Path(path)
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
    except (OSError, json.JSONDecodeError) as exc:
        return ManifestResult(path=path, data=None, errors=[f"Could not read/parse {path}: {exc}"])

    schema = load_schema()
    result = validate(data, schema)
    errors = list(result.errors)
    errors.extend(_semantic_checks(data))
    return ManifestResult(path=path, data=data if not errors else data, errors=errors)


def _semantic_checks(data: dict[str, Any]) -> list[str]:
    """Checks the schema can't express: cross-references between fields."""
    errors: list[str] = []
    mods = data.get("mods", [])
    known_ids = {m["packageId"] for m in mods if isinstance(m, dict) and "packageId" in m}
    known_ids |= {m["packageId"] for m in data.get("customMods", []) if isinstance(m, dict) and "packageId" in m}

    for mod in mods:
        if not isinstance(mod, dict):
            continue
        pkg = mod.get("packageId", "<unknown>")
        for field_name in ("dependsOn", "loadAfter", "loadBefore", "incompatibleWith"):
            for ref in mod.get(field_name, []):
                if ref not in known_ids:
                    errors.append(
                        f"mods[{pkg}].{field_name} references unknown packageId '{ref}' "
                        "(not declared in mods[] or customMods[])"
                    )
        for dep in mod.get("dependsOn", []):
            if dep in mod.get("incompatibleWith", []):
                errors.append(f"mods[{pkg}] both depends on and is incompatible with '{dep}'")

    sequence = data.get("loadOrder", {}).get("sequence")
    if sequence:
        unknown_in_sequence = [pid for pid in sequence if pid not in known_ids]
        if unknown_in_sequence:
            errors.append(f"loadOrder.sequence references unknown packageId(s): {unknown_in_sequence}")

    dlc = data.get("rimworld", {}).get("dlc", {})
    overlap = set(dlc.get("required", [])) & set(dlc.get("optional", []))
    if overlap:
        errors.append(f"rimworld.dlc lists the same DLC as both required and optional: {sorted(overlap)}")

    return errors
