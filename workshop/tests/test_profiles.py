import tempfile
import unittest
from pathlib import Path

from mcrimworld_workshop import profiles

ORIGINAL_MODS_CONFIG = """<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData>
  <version>1.6.0</version>
  <activeMods>
    <li>ludeon.rimworld</li>
    <li>ludeon.rimworld.biotech</li>
    <li>some.preexisting.mod</li>
  </activeMods>
  <knownExpansions>
    <li>Ludeon.RimWorld.Biotech</li>
  </knownExpansions>
</ModsConfigData>
"""


class ModsConfigRoundTripTests(unittest.TestCase):
    def test_write_then_read_round_trips(self):
        with tempfile.TemporaryDirectory() as tmp:
            config_dir = Path(tmp)
            data = profiles.ModsConfigData(
                version="1.6.0", active_mods=["a.mod", "b.mod"], known_expansions=["Ludeon.RimWorld.Biotech"]
            )
            profiles.write_mods_config(config_dir, data)
            read_back = profiles.read_active_mods(config_dir)
            self.assertEqual(read_back.version, "1.6.0")
            self.assertEqual(read_back.active_mods, ["a.mod", "b.mod"])
            self.assertEqual(read_back.known_expansions, ["Ludeon.RimWorld.Biotech"])

    def test_read_missing_file_returns_none(self):
        with tempfile.TemporaryDirectory() as tmp:
            self.assertIsNone(profiles.read_active_mods(Path(tmp)))


class BackupAndRestoreTests(unittest.TestCase):
    """Covers the M1 acceptance criterion: automated tests for config backup/restore."""

    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self.tmp.cleanup)
        self.root = Path(self.tmp.name)
        self.config_dir = self.root / "Config"
        self.backup_root = self.root / "backups"
        self.state_dir = self.root / "state"
        self.config_dir.mkdir()
        (self.config_dir / profiles.MODS_CONFIG_FILENAME).write_text(ORIGINAL_MODS_CONFIG, encoding="utf-8")

    def test_backup_config_copies_file(self):
        backup_path = profiles.backup_config(self.config_dir, self.backup_root)
        self.assertIsNotNone(backup_path)
        self.assertTrue(backup_path.is_file())
        self.assertEqual(backup_path.read_text(encoding="utf-8"), ORIGINAL_MODS_CONFIG)

    def test_backup_with_no_existing_config_returns_none(self):
        empty_config_dir = self.root / "EmptyConfig"
        empty_config_dir.mkdir()
        self.assertIsNone(profiles.backup_config(empty_config_dir, self.backup_root))

    def test_activate_then_restore_round_trips_exactly(self):
        manifest = {
            "id": "test-experience",
            "rimworld": {"dlc": {"required": ["Ludeon.RimWorld.Biotech"], "optional": []}},
            "mods": [
                {"packageId": "mod.a", "name": "A", "required": True},
                {"packageId": "mod.b", "name": "B", "required": True, "loadAfter": ["mod.a"]},
            ],
        }

        original_bytes = (self.config_dir / profiles.MODS_CONFIG_FILENAME).read_bytes()

        record = profiles.activate_profile(
            manifest=manifest, config_dir=self.config_dir, backup_root=self.backup_root, state_dir=self.state_dir
        )
        self.assertEqual(record["experienceId"], "test-experience")
        self.assertIsNotNone(record["backupPath"])

        activated = profiles.read_active_mods(self.config_dir)
        self.assertEqual(activated.active_mods, ["mod.a", "mod.b"])

        profiles.restore_previous(self.config_dir, self.state_dir)
        restored_bytes = (self.config_dir / profiles.MODS_CONFIG_FILENAME).read_bytes()
        self.assertEqual(restored_bytes, original_bytes)

        # The activation record is consumed by restore; a second restore must fail loudly, not silently no-op.
        with self.assertRaises(FileNotFoundError):
            profiles.restore_previous(self.config_dir, self.state_dir)

    def test_restore_without_prior_activate_raises(self):
        with self.assertRaises(FileNotFoundError):
            profiles.restore_previous(self.config_dir, self.state_dir)

    def test_no_saves_or_unrelated_files_are_touched(self):
        saves_dir = self.root / "Saves"
        saves_dir.mkdir()
        (saves_dir / "MyColony.rws").write_text("not real save data", encoding="utf-8")

        manifest = {"id": "x", "mods": [{"packageId": "mod.a", "name": "A", "required": True}]}
        profiles.activate_profile(manifest, self.config_dir, self.backup_root, self.state_dir)

        self.assertEqual((saves_dir / "MyColony.rws").read_text(encoding="utf-8"), "not real save data")


class LoadOrderTests(unittest.TestCase):
    def test_depends_on_and_load_after_are_respected(self):
        manifest = {
            "mods": [
                {"packageId": "c", "name": "C", "required": True, "loadAfter": ["a"]},
                {"packageId": "a", "name": "A", "required": True},
                {"packageId": "b", "name": "B", "required": True, "dependsOn": ["a"]},
            ]
        }
        order = profiles.resolve_load_order(manifest)
        self.assertLess(order.index("a"), order.index("b"))
        self.assertLess(order.index("a"), order.index("c"))

    def test_explicit_sequence_wins(self):
        manifest = {
            "mods": [{"packageId": "a", "name": "A", "required": True}, {"packageId": "b", "name": "B", "required": True}],
            "loadOrder": {"sequence": ["b", "a"]},
        }
        self.assertEqual(profiles.resolve_load_order(manifest), ["b", "a"])

    def test_cycle_is_detected(self):
        manifest = {
            "mods": [
                {"packageId": "a", "name": "A", "required": True, "loadAfter": ["b"]},
                {"packageId": "b", "name": "B", "required": True, "loadAfter": ["a"]},
            ]
        }
        with self.assertRaises(ValueError):
            profiles.resolve_load_order(manifest)


class ScanInstalledModsTests(unittest.TestCase):
    def test_reads_package_id_from_about_xml(self):
        with tempfile.TemporaryDirectory() as tmp:
            mod_dir = Path(tmp) / "SomeMod"
            about_dir = mod_dir / "About"
            about_dir.mkdir(parents=True)
            (about_dir / "About.xml").write_text(
                "<ModMetaData><name>Some Mod</name><packageId>author.somemod</packageId></ModMetaData>",
                encoding="utf-8",
            )
            installed = profiles.scan_installed_mods([(Path(tmp), "local")])
            self.assertIn("author.somemod", installed)
            self.assertEqual(installed["author.somemod"].name, "Some Mod")
            self.assertEqual(installed["author.somemod"].source, "local")

    def test_ignores_folders_without_about_xml(self):
        with tempfile.TemporaryDirectory() as tmp:
            (Path(tmp) / "NotAMod").mkdir()
            installed = profiles.scan_installed_mods([(Path(tmp), "local")])
            self.assertEqual(installed, {})


class ValidateRequirementsTests(unittest.TestCase):
    def test_reports_missing_and_incompatible(self):
        manifest = {
            "mods": [
                {"packageId": "needed", "name": "Needed", "required": True, "source": "workshop", "workshopId": "123"},
                {"packageId": "present", "name": "Present", "required": True},
                {"packageId": "incompatible.one", "name": "One", "required": True, "incompatibleWith": ["incompatible.two"]},
                {"packageId": "incompatible.two", "name": "Two", "required": True},
            ]
        }
        installed = {"present": profiles.InstalledMod("present", "Present", "/mods/present", "local")}
        report = profiles.validate_requirements(manifest, installed)

        self.assertFalse(report.ok)
        self.assertIn("needed", report.missing)
        self.assertNotIn("present", report.missing)
        # needed + the two incompatible mods are all missing and default to source=workshop.
        self.assertEqual({item["packageId"] for item in report.missing_workshop}, {"needed", "incompatible.one", "incompatible.two"})
        needed_entry = next(item for item in report.missing_workshop if item["packageId"] == "needed")
        self.assertEqual(needed_entry["workshopId"], "123")
        self.assertIn(("incompatible.one", "incompatible.two"), report.incompatible_pairs)


if __name__ == "__main__":
    unittest.main()
