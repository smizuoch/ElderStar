import json
from pathlib import Path
import tempfile
import unittest
from PIL import Image
from compare_randomized_frames import compare_case, expected_frames, validate_run, validate_metadata


class RandomizedComparisonTests(unittest.TestCase):
    def test_inventory_includes_all_profiles_and_distortion(self):
        self.assertEqual(416, len(expected_frames()))
        self.assertIn('Particles_Distortion__031', expected_frames())
        self.assertIn('UIParticles_UberLit__095', expected_frames())

    def test_different_color_spaces_and_incomplete_metadata_cannot_pass(self):
        valid = '6000.3.23f1\nDirect3D11\nLinear\nseed=20261013\n416 frames\nanisotropy=ForceEnable\n'
        validate_metadata(valid, valid)
        for other in (valid.replace('Linear', 'Gamma'), valid.replace('416', '64'), '', valid.replace('Direct3D11', 'Null'), valid.replace('ForceEnable', 'Disable')):
            with self.assertRaisesRegex(ValueError, 'metadata'):
                validate_metadata(valid, other)

    def test_same_blank_frames_are_not_visible_control(self):
        frame = Image.new('RGB', (128, 128), (20, 30, 40))
        with self.assertRaisesRegex(ValueError, 'visible'):
            compare_case(frame, frame, control=True)

    def test_real_difference_is_not_accepted(self):
        a = Image.new('RGB', (128, 128), (20, 30, 40))
        b = a.copy()
        b.paste((200, 20, 20), (20, 20, 100, 100))
        self.assertFalse(compare_case(a, b)['passed'])

    def test_empty_inventories_cannot_pass(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            with self.assertRaisesRegex(ValueError, '416'):
                validate_run(root, root)

    def test_mismatched_inputs_cannot_pass(self):
        with tempfile.TemporaryDirectory() as directory:
            a = Path(directory) / 'a'; a.mkdir()
            b = Path(directory) / 'b'; b.mkdir()
            for root in (a, b):
                for name in expected_frames():
                    (root / (name + '.ppm')).touch()
                    (root / (name + '.json')).write_text('{}', encoding='utf-8')
            (b / 'Particles_Distortion__031.json').write_text('{"seed": 7}', encoding='utf-8')
            with self.assertRaisesRegex(ValueError, 'input'):
                validate_run(a, b)


if __name__ == '__main__':
    unittest.main()
