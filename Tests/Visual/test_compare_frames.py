import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location("compare_frames", Path(__file__).with_name("compare_frames.py"))
compare = importlib.util.module_from_spec(spec)
spec.loader.exec_module(compare)


class ComparisonTests(unittest.TestCase):
    def test_identical_pixels_pass(self):
        result = compare.measure(bytes([0, 128, 255] * 4), bytes([0, 128, 255] * 4))
        self.assertEqual(result["mean_error"], 0)
        self.assertTrue(compare.within_thresholds(result))

    def test_changed_image_fails(self):
        result = compare.measure(bytes([0, 0, 0] * 100), bytes([255, 255, 255] * 100))
        self.assertEqual(result["mean_error"], 1)
        self.assertFalse(compare.within_thresholds(result))

    def test_isolated_large_error_is_not_hidden_by_mean(self):
        result = compare.measure(bytes([0] * 300), bytes([255, 0, 0] + [0] * 297))
        self.assertLess(result["mean_error"], 0.01)
        self.assertFalse(compare.within_thresholds(result))

    def test_rejects_empty_or_mismatched_data(self):
        for a, b in [(b"", b""), (b"abc", b"abcdef"), (b"abcd", b"abcd")]:
            with self.assertRaises(ValueError):
                compare.measure(a, b)

    def test_missing_frames_cannot_pass(self):
        with self.assertRaises(ValueError):
            compare.validate_inventory(set(), set())
        with self.assertRaises(ValueError):
            compare.validate_inventory(compare.EXPECTED, compare.EXPECTED - {next(iter(compare.EXPECTED))})


if __name__ == "__main__":
    unittest.main()
