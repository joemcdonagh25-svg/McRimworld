"""A small, dependency-free validator for the subset of JSON Schema (draft
2020-12) used by experience.schema.json: type, properties,
additionalProperties, required, items, enum, const, pattern, minLength,
minItems, uniqueItems, allOf and local ``#/$defs/...`` refs.

This is intentionally not a general-purpose JSON Schema engine. It exists so
the Workshop can validate experience manifests without a third-party
dependency. If the schema grows features this module doesn't understand, it
raises NotImplementedError rather than silently skipping validation.
"""

from __future__ import annotations

import re
from dataclasses import dataclass, field
from typing import Any


@dataclass
class ValidationResult:
    errors: list[str] = field(default_factory=list)

    @property
    def valid(self) -> bool:
        return not self.errors


_SUPPORTED_KEYWORDS = {
    "$ref", "$defs", "$schema", "$id", "title", "description", "default",
    "type", "properties", "additionalProperties", "required", "items",
    "enum", "const", "pattern", "minLength", "minItems", "uniqueItems",
    "allOf", "format",
}

_TYPE_MAP = {
    "object": dict,
    "array": list,
    "string": str,
    "boolean": bool,
    "number": (int, float),
    "integer": int,
    "null": type(None),
}


def validate(instance: Any, schema: dict, root: dict | None = None) -> ValidationResult:
    root = root if root is not None else schema
    errors: list[str] = []
    _check(instance, schema, root, "$", errors)
    return ValidationResult(errors=errors)


def _resolve_ref(ref: str, root: dict) -> dict:
    if not ref.startswith("#/"):
        raise NotImplementedError(f"Only local refs are supported, got: {ref}")
    node: Any = root
    for part in ref[2:].split("/"):
        node = node[part]
    return node


def _check(instance: Any, schema: dict, root: dict, path: str, errors: list[str]) -> None:
    unknown = set(schema.keys()) - _SUPPORTED_KEYWORDS
    if unknown:
        raise NotImplementedError(f"Unsupported schema keyword(s) at {path}: {sorted(unknown)}")

    if "$ref" in schema:
        _check(instance, _resolve_ref(schema["$ref"], root), root, path, errors)
        return

    if "allOf" in schema:
        for sub in schema["allOf"]:
            _check(instance, sub, root, path, errors)

    if "const" in schema and instance != schema["const"]:
        errors.append(f"{path}: expected constant {schema['const']!r}, got {instance!r}")

    if "enum" in schema and instance not in schema["enum"]:
        errors.append(f"{path}: {instance!r} is not one of {schema['enum']}")

    if "type" in schema:
        expected = _TYPE_MAP[schema["type"]]
        if expected is int and isinstance(instance, bool):
            errors.append(f"{path}: expected integer, got boolean")
        elif not isinstance(instance, expected):
            errors.append(f"{path}: expected {schema['type']}, got {type(instance).__name__}")
            return

    if isinstance(instance, str):
        if "pattern" in schema and not re.search(schema["pattern"], instance):
            errors.append(f"{path}: {instance!r} does not match pattern {schema['pattern']!r}")
        if "minLength" in schema and len(instance) < schema["minLength"]:
            errors.append(f"{path}: string shorter than minLength {schema['minLength']}")

    if isinstance(instance, dict):
        properties = schema.get("properties", {})
        for key, sub_schema in properties.items():
            if key in instance:
                _check(instance[key], sub_schema, root, f"{path}.{key}", errors)

        for key in schema.get("required", []):
            if key not in instance:
                errors.append(f"{path}: missing required property '{key}'")

        additional = schema.get("additionalProperties", True)
        if additional is False:
            extra = set(instance.keys()) - set(properties.keys())
            if extra:
                errors.append(f"{path}: unexpected propert{'y' if len(extra) == 1 else 'ies'} {sorted(extra)}")

    if isinstance(instance, list):
        if "minItems" in schema and len(instance) < schema["minItems"]:
            errors.append(f"{path}: array shorter than minItems {schema['minItems']}")
        if schema.get("uniqueItems") and len(instance) != len({_freeze(i) for i in instance}):
            errors.append(f"{path}: array items are not unique")
        if "items" in schema:
            for idx, item in enumerate(instance):
                _check(item, schema["items"], root, f"{path}[{idx}]", errors)


def _freeze(value: Any) -> Any:
    if isinstance(value, dict):
        return tuple(sorted((k, _freeze(v)) for k, v in value.items()))
    if isinstance(value, list):
        return tuple(_freeze(v) for v in value)
    return value
