import json
from pathlib import Path
import tempfile
import unittest

from compare_effect_frames import run


class EffectComparisonTests(unittest.TestCase):
    def fixture(self, root, visible):
        from PIL import Image, ImageDraw
        paths = [root / "reference", root / "actual"]
        for path in paths:
            path.mkdir()
            for stamp in ("0025", "0075", "0150", "0300", "0500", "0800"):
                (path / (stamp + "-particles.txt")).write_text("Fire:1\nposition/velocity/lifetime/seed\n")
                for angle in (0, 35, 80):
                    image = Image.new("RGB", (384, 384), (10, 20, 30))
                    if visible:
                        ImageDraw.Draw(image).rectangle((160, 200, 220, 300), fill=(255, 120, 10))
                    image.save(path / f"{stamp}-{angle:02}.ppm")
        return paths

    def test_identical_blank_frames_are_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            a, b = self.fixture(root, False)
            with self.assertRaisesRegex(ValueError, "visible"):
                run(a, b, root / "report.json")

    def test_visible_frames_and_matching_state_pass(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            a, b = self.fixture(root, True)
            self.assertEqual(run(a, b, root / "report.json"), 0)
            report = json.loads((root / "report.json").read_text())
            self.assertNotIn(b"\r", (root / "report.json").read_bytes(), "Evidence must use portable LF line endings.")
            self.assertEqual(len(report["frames"]), 18)
            self.assertTrue(report["particle_state_identical"])

    def test_changed_particle_simulation_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            a, b = self.fixture(root, True)
            (b / "0025-particles.txt").write_text("different particle position")
            with self.assertRaisesRegex(ValueError, "simulation differs"):
                run(a, b, root / "report.json")


if __name__ == "__main__":
    unittest.main()
