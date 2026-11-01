import json
import tempfile
import unittest
from pathlib import Path

from mcrimworld_workshop.manifest import load_manifest

EXPERIENCES_DIR = Path(__file__).resolve().parent.parent / "experiences"

MINIMAL_VALID = {
    "schemaVersion": "1.0.0",
    "id": "test-experience",
    "version": "0.1.0",
    "name": "Test Experience",
    "rimworld": {"version": "1.6"},
    "mods": [{"packageId": "some.mod", "name": "Some Mod", "required": True}],
}


class ManifestTests(unittest.TestCase):
    def test_lantern_village_fixture_is_valid(self):
        result = load_manifest(EXPERIENCES_DIR / "lantern-village.json")
        self.assertTrue(result.valid, result.errors)
        self.assertEqual(result.data["id"], "lantern-village")

    def test_minimal_manifest_is_valid(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "minimal.json"
            path.write_text(json.dumps(MINIMAL_VALID), encoding="utf-8")
            result = load_manifest(path)
            self.assertTrue(result.valid, result.errors)

    def test_missing_required_field_is_rejected(self):
        broken = dict(MINIMAL_VALID)
        del broken["rimworld"]
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "broken.json"
            path.write_text(json.dumps(broken), encoding="utf-8")
            result = load_manifest(path)
            self.assertFalse(result.valid)
            self.assertTrue(any("rimworld" in e for e in result.errors))

    def test_unknown_mod_reference_is_rejected(self):
        broken = json.loads(json.dumps(MINIMAL_VALID))
        broken["mods"][0]["dependsOn"] = ["nonexistent.mod"]
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "broken.json"
            path.write_text(json.dumps(broken), encoding="utf-8")
            result = load_manifest(path)
            self.assertFalse(result.valid)
            self.assertTrue(any("unknown packageId" in e for e in result.errors))

    def test_malformed_json_reports_error_without_raising(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "bad.json"
            path.write_text("{not json", encoding="utf-8")
            result = load_manifest(path)
            self.assertFalse(result.valid)


if __name__ == "__main__":
    unittest.main()
