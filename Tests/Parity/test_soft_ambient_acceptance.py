"""Reject unsupported completion claims and stale release verification."""
import gzip
import hashlib
import json
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[2]
FOLDER = ROOT / "Documentation/Verification/SoftAmbient20261015"

class SoftAmbientAcceptanceTests(unittest.TestCase):
    def test_all_remaining_features_keep_individual_limits(self):
        data = json.loads((FOLDER / "remaining-review.json").read_text(encoding="utf8"))
        matrix = json.loads((ROOT / "Documentation/Verification/ApiFeatureCoverage20261013.json").read_text(encoding="utf8"))
        remaining = {r["feature"] for r in matrix["features"] if r["status"] == "Approximate"}
        self.assertEqual({r["feature"] for r in data["rows"]}, remaining)
        self.assertEqual(len(data["rows"]), len(remaining))
        self.assertFalse(data["all_29_resolved"])
        for row in data["rows"]:
            self.assertTrue(row["result"])
            self.assertTrue(row["remaining_requirement"])
            for path in row["evidence"]:
                self.assertTrue((ROOT / path).is_file(), path)

    def test_sdk_aao_and_shader_hashes_are_current(self):
        data = json.loads((FOLDER / "sdk-aao.json").read_text(encoding="utf8"))
        self.assertEqual(data["package"], "0.1.7")
        for row in data["reports"]:
            self.assertTrue(row["success"])
            for key in ["errors", "shaderErrors", "csharpErrors", "magentaPixels"]:
                self.assertEqual(row[key], 0)
            self.assertTrue(row["materialStatesEqual"])
        self.assertEqual({r["aaoEnabled"] for r in data["reports"]}, {False, True})
        self.assertTrue(data["pairs"])
        self.assertTrue(all(r["passed"] for r in data["pairs"]))
        for path, digest in data["runtime_sha256"].items():
            self.assertEqual(hashlib.sha256((ROOT / path).read_bytes()).hexdigest(), digest, path)

    def test_hidden_cost_measurement_recomputes_without_relaxing_gate(self):
        sys.path.insert(0, str(ROOT / "Tests/Performance"))
        from analyze_lighting import analyze
        raw = json.loads(gzip.decompress((FOLDER / "gpu-raw.json.gz").read_bytes()).decode("utf-8-sig"))
        rows = analyze(raw)
        self.assertEqual(rows, json.loads((FOLDER / "gpu-summary.json").read_text(encoding="utf8")))
        ordinary = [r for r in rows if not r["optional"]]
        self.assertEqual(len(ordinary),18)
        self.assertTrue(all(r["accepted"] for r in ordinary))
        self.assertEqual(len(rows)-len(ordinary),2)

if __name__ == "__main__": unittest.main()
