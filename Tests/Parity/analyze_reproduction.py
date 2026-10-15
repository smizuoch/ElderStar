"""Recompute bounded evidence metrics; never infer universal equivalence from samples.

No Unity launch, golden updates, image regeneration or runtime modification.
The manifest is explicit: red runs, superseded comparisons and retained duplicates
are not mixed into a new percentage. Cohorts from different revisions stay separate.
"""
import argparse
from collections import Counter
import hashlib
import json
import math
from pathlib import Path
import re
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "Visual"))
from compare_frames import within_thresholds

STATUSES = ("Exact", "Approximate", "Unsupported", "Not Applicable")
VERIFY = "Documentation/Verification/"
RANDOM = VERIFY + "Randomized20261013/"
AMBIENT = VERIFY + "AmbientParity20261014/"
MANIFEST = (
    ("asset_corpus_0.1.2", "0.1.2 historical corpus; not rerun on 0.1.6",
     [(f"Documentation/Validation/AssetCorpus/unity{editor}-{color}.json", count)
      for editor, count in (("2022", 509), ("6", 500)) for color in ("gamma", "linear")]),
    ("randomized_20261013", "2026-10-13 historical randomized run; source hashes retained",
     [(RANDOM + f"{editor}-{color}-{mode}-metrics.json", 416)
      for editor in ("2022", "6000") for color in ("gamma", "linear")
      for mode in ("green", "disabled")]),
    ("ambient_0.1.6", "0.1.6 ambient and interpolation release captures",
     [(AMBIENT + f"{kind}{editor}-{color}.json", count)
      for kind, count in (("ambient", 60), ("interpolation", 192))
      for editor in ("2022", "6") for color in ("gamma", "linear")]),
)


def ratio(numerator, denominator):
    if not 0 <= numerator <= denominator:
        raise ValueError("Invalid numerator/denominator")
    return dict(numerator=numerator, denominator=denominator,
                percent=100 * numerator / denominator if denominator else None)


def feature_summary(rows):
    names = [row["feature"] for row in rows]
    if len(set(names)) != len(names) or any(row["status"] not in STATUSES for row in rows):
        raise ValueError("Duplicate feature or unknown classification")
    counts = {status: sum(row["status"] == status for row in rows) for status in STATUSES}
    return dict(counts=counts,
                exact_classification_share=ratio(counts["Exact"], len(rows) - counts["Not Applicable"]),
                all_inputs_reproduction_percent=None)


def image_summary(document, expected_count):
    rows = document.get("frames", document.get("results", []))
    if not rows or len(rows) != expected_count:
        raise ValueError("Missing or unexpected frame count")
    names = [row.get("name", row.get("frame", row.get("case"))) for row in rows]
    if any(not isinstance(name, str) or not name for name in names) or len(set(names)) != len(names):
        raise ValueError("Missing or duplicate frame identifier")
    passed = identical = 0
    failures = []
    for name, row in zip(names, rows):
        for key in ("mean_error", "max_error", "bad_pixel_fraction"):
            value = row.get(key)
            if type(value) not in (float, int) or not math.isfinite(value) or not 0 <= value <= 1:
                raise ValueError("Missing, nonfinite or out-of-range image metric")
        accepted = within_thresholds(row)
        if type(row.get("passed")) is not bool or row["passed"] != accepted:
            raise ValueError("Recorded pass flag disagrees with unchanged comparator")
        passed += accepted
        identical += row["max_error"] == 0 and row["mean_error"] == 0 and row["bad_pixel_fraction"] == 0
        if not accepted:
            failures.append(name)
    checks = {"total": len(rows), "cases": len(rows), "failed": len(rows) - passed,
              "failures": len(rows) - passed, "pixel_identical": identical}
    for key, value in checks.items():
        if key in document and document[key] != value:
            raise ValueError(f"Recorded {key} disagrees with individual frames")
    return dict(within_threshold=ratio(passed, len(rows)), pixel_identical=ratio(identical, len(rows)),
                failed=len(rows) - passed, failing_frames=failures)


def validate_manifest(entries):
    names = [str(Path(path)) for path, _ in entries]
    if len(set(names)) != len(names):
        raise ValueError("Repeated evidence file")


def verify_api_observations(document, recorded):
    # The committed archive was reformatted to two spaces and a final newline.
    # Reconstruct the original JsonUtility bytes (four spaces, no final newline)
    # and check both original capture hashes; do not merely trust 'identical'.
    original_hash = hashlib.sha256(json.dumps(document, indent=4).encode("utf-8")).hexdigest()
    if (len(document["rows"]) != recorded["rows"] or recorded["identical"] is not True
            or recorded["reference_sha256"] != original_hash or recorded["port_sha256"] != original_hash):
        raise ValueError("API observation provenance mismatch")
    return len(document["rows"])


def build_report(root):
    hashes = {}

    def read(path):
        data = (root / path).read_bytes()
        hashes[path] = hashlib.sha256(data).hexdigest()
        return data.decode("utf-8-sig")

    def load(path):
        return json.loads(read(path))

    inventory = load(VERIFY + "ApiFeatureCoverage20261013.json")
    matrix = read("Documentation/BuiltInCompatibilityMatrix.md")
    matrix_rows = []
    for line in matrix.splitlines():
        cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
        if len(cells) == 4 and cells[1] in STATUSES:
            matrix_rows.append(dict(zip(("feature", "status", "evidence", "reason_or_fallback"), cells)))
    if matrix_rows != inventory["features"]:
        raise ValueError("Feature inventory and current matrix disagree")

    reference = load("Tests/Parity/original-3.6-contract.json")
    property_types = Counter()
    property_count = preserved_count = 0
    shaders = []
    for filename, shader in reference["shaders"].items():
        source = read("Assets/Nova/Runtime/Core/Shaders/" + filename)
        declarations = [line.strip() for line in source.split("SubShader")[0].splitlines()
                        if re.match(r'\s*(?:\[[^\]]*\]\s*)*_[A-Za-z]', line)]
        properties = shader["properties"]
        property_count += len(properties)
        # Include ordering and CustomEditor bindings, not only property names.
        if declarations != properties or re.findall(r'CustomEditor\s+"([^"]+)"', source) != shader["custom_editors"]:
            raise ValueError("Original ShaderLab declaration contract changed: " + filename)
        preserved_count += len(properties)
        for declaration in properties:
            match = re.search(r',\s*(Range\([^)]*\)|Float|Int|Integer|Color|Vector|2D|2DArray|3D|Cube)\s*\)\s*=', declaration)
            if not match:
                raise ValueError("Unrecognized property type: " + declaration)
            property_types["Range" if match[1].startswith("Range(") else match[1]] += 1
        shaders.append(dict(file=filename, property_declarations=len(properties)))

    editor_reference = load("Assets/Tests/Editor/Reference/EditorContract.json")["entries"]
    editor_declarations = [row["declaration"] for row in inventory["editor_apis"]]
    if sorted(editor_reference) != sorted(editor_declarations) or len(set(editor_declarations)) != len(editor_declarations):
        raise ValueError("Original Editor API inventory is incomplete")
    runtime = inventory["runtime_pipeline_apis"]["declarations"]
    for name in ("ApplyDistortionPass", "DistortedUvBufferPass", "DistortionContextItem", "BuiltInDistortionPass", "ScreenSpaceDistortion"):
        read("Assets/Nova/Runtime/Core/Scripts/" + name + ".cs")
    runtime_reference = set()
    for editor in ("2022", "6000"):
        runtime_reference.update(load(RANDOM + editor + "-reference-runtime-declarations.json")["rows"])
    if runtime_reference != {row["declaration"] for row in runtime} or len(runtime_reference) != len(runtime):
        raise ValueError("Original runtime API inventory is incomplete")

    validate_manifest([entry for _, _, entries in MANIFEST for entry in entries])
    cohorts = []
    for name, scope, entries in MANIFEST:
        runs = [dict(source=path, **image_summary(load(path), expected)) for path, expected in entries]
        total = sum(run["within_threshold"]["denominator"] for run in runs)
        passed = sum(run["within_threshold"]["numerator"] for run in runs)
        identical = sum(run["pixel_identical"]["numerator"] for run in runs)
        cohorts.append(dict(id=name, scope=scope, within_threshold=ratio(passed, total),
                            pixel_identical=ratio(identical, total), failed=total - passed, runs=runs))

    observations = []
    summary = load(RANDOM + "summary.json")
    for editor in ("2022", "6000"):
        path = RANDOM + editor + "-api-observations.json"
        document = load(path)
        recorded = next(row for row in summary["api_behavior"] if row["editor"] == editor)
        count = verify_api_observations(document, recorded)
        observations.append(dict(editor=editor, recorded_matching_observations=count,
                                 source=path, all_inputs_coverage_percent=None))

    return dict(schema_version=1, audit_date="2026-10-15",
                assessed_source_commit="3b764095b35121828b1d295ac106b2796455f7fe",
                package_version=load("Assets/Nova/package.json")["version"], reference=inventory["reference"],
                all_inputs_reproduction_percent=None,
                undefined_reason="No closed input domain, weighting measure or exhaustive equivalence proof. Finite sample rates do not determine universal reproduction.",
                features=feature_summary(matrix_rows), feature_inventory=matrix_rows,
                property_declarations=dict(preserved=ratio(preserved_count, property_count), types=dict(property_types), shaders=shaders),
                editor_declaration_inventory_count=len(editor_declarations),
                editor_preservation_evidence="Existing 0.1.6 Unity authoring tests; this audit checks inventory and source contracts, not a fresh reflection run.",
                runtime_declaration_classifications=dict(Counter(row["status"] for row in runtime)),
                runtime_declarations=runtime, api_observations=observations, visual_cohorts=cohorts,
                omitted_evidence="Red runs, superseded lighting captures, retained-original22 duplicate, SDK/AAO port-to-port comparisons and other separately documented suites are not pooled.",
                limitations=[
                    "Exact is the matrix's scoped classification, not a proof for every input or hardware configuration.",
                    "Feature rows and declaration rows are different units and must not be averaged.",
                    "Image pass rates are case counts under fixed tolerances, not a percentage of visual similarity.",
                    "Pixel-identical refers to stored RGB capture metrics, not internal HDR buffers, alpha or unobserved pixels/times.",
                    "Historical cohorts are not reruns of every case on 0.1.6 and are not independent samples of all use.",
                    "No sampling distribution was defined; no universal confidence interval is justified.",
                    "All input values, textures, scene topologies, call sequences, GPU/world/XR conditions remain unenumerated.",
                ], evidence_sha256=dict(sorted(hashes.items())))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    result = json.dumps(build_report(Path(__file__).resolve().parents[2]), ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.write_bytes(result.encode("utf-8"))
    else:
        print(result, end="")
