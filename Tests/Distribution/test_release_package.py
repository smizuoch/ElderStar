import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "Scripts/Distribution/prepare_release.py"


class ReleasePackageTests(unittest.TestCase):
    def setUp(self):
        spec = importlib.util.spec_from_file_location("prepare_release", SCRIPT)
        self.release = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.release)

    def test_upm_stage_preserves_all_package_bytes_and_excludes_project_assets(self):
        with tempfile.TemporaryDirectory() as folder:
            out = Path(folder) / "upm"
            self.release.stage(ROOT, out, "upm")
            expected = {p.relative_to(ROOT / "Assets/Nova"): p.read_bytes()
                        for p in (ROOT / "Assets/Nova").rglob("*") if p.is_file()}
            actual = {p.relative_to(out): p.read_bytes() for p in out.rglob("*") if p.is_file()}
            self.assertEqual(actual, expected)

    def test_unitypackage_stage_preserves_code_guids_and_makes_samples_importable(self):
        with tempfile.TemporaryDirectory() as folder:
            out = Path(folder) / "assets"
            self.release.stage(ROOT, out, "assets")
            for section in ("Runtime", "Editor"):
                for p in (ROOT / "Assets/Nova" / section).rglob("*"):
                    if p.is_file():
                        self.assertEqual((out / p.relative_to(ROOT / "Assets/Nova")).read_bytes(), p.read_bytes())
            self.assertTrue((out / "Samples/BuiltInShowcase/NovaShowcase.unity").exists())
            self.assertTrue((out / "Documentation/Images/barrier.gif").exists())
            self.assertIn("Documentation/Images/barrier.gif", (out / "README.md").read_text(encoding="utf-8"))
            self.assertEqual(json.loads((out / "package.json").read_text())["samples"][0]["path"], "Samples/BuiltInShowcase")
            self.assertFalse(any("~" in p.name for p in out.rglob("*")))

    def test_staging_refuses_to_overwrite_existing_output(self):
        with tempfile.TemporaryDirectory() as folder:
            with self.assertRaises(FileExistsError):
                self.release.stage(ROOT, Path(folder), "assets")

    def test_unexpected_package_files_are_not_silently_published(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            package = root / "Assets/Nova"
            package.mkdir(parents=True)
            (package / "package.json").write_text('{"name":"me.smizuoch.elderstar","version":"0.1.8"}')
            (package / "personal-notes.txt").write_text("not a release asset")
            with self.assertRaises(ValueError):
                self.release.stage(root, root / "out", "upm")


if __name__ == "__main__":
    unittest.main()
