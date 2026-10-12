import tempfile
import unittest
from pathlib import Path
from PIL import Image

from compare_corpus_frames import compare_frame, compare_states


class CorpusComparisonTests(unittest.TestCase):
    def test_identical_empty_frames_do_not_prove_visibility(self):
        blank = Image.new("RGB", (256, 256), (18, 28, 48))
        with self.assertRaisesRegex(ValueError, "visible"):
            compare_frame(blank, blank, require_visible=True)

    def test_missing_particle_state_cannot_silently_pass(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for name in ("reference", "actual"):
                (root / name).mkdir()
            (root / "reference/effect-state.txt").write_bytes(b"particles:3\n")
            with self.assertRaisesRegex(ValueError, "state inventory"):
                compare_states(root / "reference", root / "actual")

    def test_different_initial_material_defaults_fail(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for name, value in (("reference", b"_ZWrite=1"), ("actual", b"_ZWrite=0")):
                (root / name).mkdir()
                (root / name / "material-state.txt").write_bytes(value)
            with self.assertRaisesRegex(ValueError, "differs"):
                compare_states(root / "reference", root / "actual")


if __name__ == "__main__":
    unittest.main()
