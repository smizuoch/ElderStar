"""Require explicit behavioral evidence before a matrix promotion; never score alternatives as Exact."""
import gzip
import sys
import json
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[2]
PROMOTED = {"Mirror samplers", "Tint rim", "Rim and luminance transparency", "Depth fade", "Normal maps", "Fog"}

class ExactFeatureAssessmentTests(unittest.TestCase):
    def test_promotions_have_unchanged_scope_and_specific_evidence(self):
        data = json.loads((ROOT / "Documentation/Verification/ExactParity20261015/assessment.json").read_text(encoding="utf8"))
        self.assertEqual(set(row["feature"] for row in data["promoted"]), PROMOTED)
        matrix = (ROOT / "Documentation/BuiltInCompatibilityMatrix.md").read_text(encoding="utf8")
        inventory = json.loads((ROOT / "Documentation/Verification/ApiFeatureCoverage20261013.json").read_text(encoding="utf8"))
        statuses = {row["feature"]: row["status"] for row in inventory["features"]}
        for row in data["promoted"]:
            self.assertEqual(statuses[row["feature"]], "Exact")
            self.assertIn("| " + row["feature"] + " | Exact |", matrix)
            self.assertTrue(row["contract"])
            self.assertTrue(row["scope_limits"])
            for evidence in row["evidence_files"]:
                self.assertTrue((ROOT / evidence).is_file(), evidence)
        self.assertEqual({row["feature"] for row in data["remaining"]}, {name for name,status in statuses.items() if status == "Approximate"})
        for row in data["remaining"]:
            self.assertTrue(row["next_acceptance"])
            self.assertTrue(row["constraint"])
        for row in data["promoted"]:
            for evidence in row.get("retained_image_evidence", []):
                frames = [frame for frame in json.loads((ROOT / evidence["path"]).read_text(encoding="utf8"))["frames"] if evidence["pattern"] in frame["name"]]
                self.assertEqual(len(frames), evidence["frames"])
                self.assertTrue(all(frame["passed"] for frame in frames))
                self.assertEqual(sum(frame["max_error"] == 0 for frame in frames), evidence["pixel_identical"])
        self.assertFalse(data["universal_equivalence_claimed"])

    def test_recorded_performance_recomputes_and_retains_the_existing_gate(self):
        sys.path.insert(0, str(ROOT / "Tests/Performance"))
        from analyze_lighting import analyze
        folder = ROOT / "Documentation/Verification/ExactParity20261015"
        raw = json.loads(gzip.decompress((folder / "gpu-raw.json.gz").read_bytes()).decode("utf-8-sig"))
        actual = analyze(raw)
        self.assertEqual(len(actual), 18)
        self.assertTrue(all(row["accepted"] for row in actual))
        self.assertEqual(actual, json.loads((folder / "gpu-summary.json").read_text(encoding="utf8")))

if __name__ == "__main__": unittest.main()
