"""Guard the meaning of reproduction metrics, not just arithmetic."""
import copy
import json
from pathlib import Path
import unittest

import analyze_reproduction as audit

ROOT = Path(__file__).resolve().parents[2]


def frame(name="one", error=0.0):
    return dict(name=name, mean_error=error, max_error=error,
                bad_pixel_fraction=0.0, passed=error <= 0.01)


class ReproductionAuditTests(unittest.TestCase):
    def test_approximate_features_are_not_given_arbitrary_partial_credit(self):
        result = audit.feature_summary([
            {"feature": "a", "status": "Exact"},
            {"feature": "b", "status": "Approximate"},
            {"feature": "c", "status": "Unsupported"},
            {"feature": "d", "status": "Not Applicable"},
        ])
        self.assertEqual(result["exact_classification_share"],
                         {"numerator": 1, "denominator": 3, "percent": 100 / 3})
        self.assertEqual(result["counts"]["Not Applicable"], 1)
        self.assertIsNone(result["all_inputs_reproduction_percent"])

    def test_zero_denominator_is_undefined_not_perfect(self):
        self.assertIsNone(audit.ratio(0, 0)["percent"])

    def test_duplicate_features_and_unrecognized_status_are_errors(self):
        for rows in ([{"feature": "a", "status": "Exact"}] * 2,
                     [{"feature": "a", "status": "Probably exact"}]):
            with self.assertRaises(ValueError):
                audit.feature_summary(rows)

    def test_threshold_pass_is_not_pixel_identity(self):
        result = audit.image_summary({"frames": [frame(), frame("near", 0.001)]}, 2)
        self.assertEqual(result["within_threshold"]["numerator"], 2)
        self.assertEqual(result["pixel_identical"]["numerator"], 1)

    def test_thresholds_use_the_existing_comparator_including_boundary(self):
        at_limit = dict(frame(), mean_error=0.01, max_error=0.08, bad_pixel_fraction=0.01)
        self.assertEqual(audit.image_summary({"frames": [at_limit]}, 1)["failed"], 0)
        over_limit = dict(at_limit, max_error=0.080001, passed=False)
        self.assertEqual(audit.image_summary({"frames": [over_limit]}, 1)["failed"], 1)

    def test_missing_duplicate_invalid_and_misreported_rows_fail_closed(self):
        good = {"total": 1, "failed": 0, "frames": [frame()]}
        bad = [dict(good, frames=[]), dict(good, total=2), dict(good, failed=1),
               {"frames": [frame(), frame()]}, dict(good, frames=[dict(frame(), passed=False)]),
               dict(good, frames=[dict(frame(), passed=1)])]
        for value in (float("nan"), float("inf"), -0.01, 1.01):
            bad.append(dict(good, frames=[dict(frame(), max_error=value)]))
        for document in bad:
            with self.subTest(document=document), self.assertRaises(ValueError):
                audit.image_summary(document, 1)

    def test_duplicate_evidence_files_cannot_inflate_a_cohort(self):
        with self.assertRaises(ValueError):
            audit.validate_manifest([("same.json", 1), ("same.json", 1)])

    def test_reformatted_api_observations_match_original_hash_but_altered_values_do_not(self):
        directory = ROOT / "Documentation/Verification/Randomized20261013"
        document = json.loads((directory / "2022-api-observations.json").read_text(encoding="utf-8"))
        record = json.loads((directory / "summary.json").read_text(encoding="utf-8"))["api_behavior"][0]
        self.assertEqual(audit.verify_api_observations(document, record), 188)
        altered = copy.deepcopy(document)
        altered["rows"][0] += "tampered"
        with self.assertRaises(ValueError):
            audit.verify_api_observations(altered, record)

    def test_current_evidence_recomputes_without_cross_version_pooling(self):
        result = audit.build_report(ROOT)
        self.assertIsNone(result["all_inputs_reproduction_percent"])
        self.assertNotIn("pooled_visual_percent", result)
        self.assertEqual(result["features"]["counts"],
                         {"Exact": 34, "Approximate": 29, "Unsupported": 0, "Not Applicable": 3})
        expected = {"asset_corpus_0.1.2": (2018, 1994, 1972),
                    "randomized_20261013": (3328, 3328, 2874),
                    "ambient_0.1.6": (1008, 1008, None)}
        for cohort in result["visual_cohorts"]:
            total, passed, exact = expected[cohort["id"]]
            self.assertEqual(cohort["within_threshold"]["denominator"], total)
            self.assertEqual(cohort["within_threshold"]["numerator"], passed)
            if exact is not None:
                self.assertEqual(cohort["pixel_identical"]["numerator"], exact)
        self.assertEqual(sum(result["property_declarations"]["types"].values()), 889)
        self.assertEqual(result["editor_declaration_inventory_count"], 525)
        self.assertEqual(result["runtime_declaration_classifications"],
                         {"Not source-compatible": 13, "Exact": 2, "Approximate": 6})

    def test_report_is_reproducible_and_never_fills_unknown_global_rate(self):
        path = ROOT / "Documentation/Verification/ReproductionAudit20261015.json"
        self.assertEqual(json.loads(path.read_text(encoding="utf-8")), audit.build_report(ROOT))


if __name__ == "__main__":
    unittest.main()
