"""Compare fresh original/port captures without reading or changing golden images.

Common features on all four shaders are gated at mean RGB error <= 0.01, max channel error
<= 0.08 and at most 1% of pixels exceeding 0.02 mean RGB error (all normalized).
Common Lit captures use a diffuse surface with specular/reflections disabled.
The separate --pbr run checks original GGX, environment Fresnel and roughness mips.
"""
import argparse
import json
from pathlib import Path

SHADERS = ("Particles_UberUnlit", "UIParticles_UberUnlit", "Particles_UberLit", "UIParticles_UberLit")
CASES = ("base", "vertex-color", "tint", "channel", "mirror", "tritone", "pentone", "greyscale", "gradient", "fade", "dissolve", "second-transition", "flow", "parallax", "emission", "luminance", "rim", "vertex-deformation", "array", "volume", "alpha-blend", "additive", "multiply", "cutout")
EXPECTED = {f"{shader}__{case}.ppm" for shader in SHADERS for case in CASES}
PBR_CASES = tuple(f"{kind}-{workflow}-{smoothness}-{angle}" for kind in ("direct", "reflection")
                  for workflow in (0, 1) for smoothness in (0, 1, 2) for angle in (0, 30, 60))


def measure(reference, actual):
    if not reference or len(reference) != len(actual) or len(reference) % 3:
        raise ValueError("Expected matching, nonempty RGB data")
    delta = [abs(a - b) / 255 for a, b in zip(reference, actual)]
    bad_pixels = sum(sum(delta[i:i + 3]) / 3 > 0.02 for i in range(0, len(delta), 3))
    return {"mean_error": sum(delta) / len(delta), "max_error": max(delta), "bad_pixel_fraction": bad_pixels / (len(delta) // 3)}


def within_thresholds(metrics):
    return metrics["mean_error"] <= 0.01 and metrics["max_error"] <= 0.08 and metrics["bad_pixel_fraction"] <= 0.01


def validate_inventory(reference, actual, expected=EXPECTED):
    if reference != expected or actual != expected:
        raise ValueError(f"Expected exactly {len(expected)} captures per pipeline; missing reference={sorted(expected-reference)}, missing actual={sorted(expected-actual)}, extra={sorted((reference|actual)-expected)}")


def run(reference, actual, report, pbr=False):
    from PIL import Image, ImageChops, ImageDraw
    shaders = SHADERS[2:] if pbr else SHADERS
    cases = PBR_CASES if pbr else CASES
    expected = {f"{shader}__{case}.ppm" for shader in shaders for case in cases}
    validate_inventory({p.name for p in reference.glob("*.ppm")}, {p.name for p in actual.glob("*.ppm")}, expected)
    report.mkdir(parents=True, exist_ok=True)
    results = []
    for shader in shaders:
        for page in range((len(cases) + 5) // 6):
            sheet = Image.new("RGB", (3 * 128 + 32, 6 * 156 + 28), "#181b22")
            draw = ImageDraw.Draw(sheet)
            draw.text((8, 8), "Original URP        Built-in           Difference x4", fill="white")
            for row, case in enumerate(cases[page * 6:page * 6 + 6]):
                filename = f"{shader}__{case}.ppm"
                a = Image.open(reference / filename).convert("RGB")
                b = Image.open(actual / filename).convert("RGB")
                if a.size != (128, 128) or b.size != a.size:
                    raise ValueError(f"Unexpected dimensions: {filename}")
                metrics = measure(a.tobytes(), b.tobytes())
                exact = shader.endswith("UberUnlit")
                passed = within_thresholds(metrics)
                if case == "base" or pbr:
                    for frame in (a, b):
                        bg = frame.getpixel((0, 0))
                        pixels = frame.get_flattened_data() if hasattr(frame, "get_flattened_data") else frame.getdata()
                        coverage = sum(max(abs(c-d) for c,d in zip(pixel,bg)) > 8 for pixel in pixels) / (128*128)
                        if coverage < 0.1:
                            raise ValueError(f"No visible base quad: {filename}")
                results.append({"shader": shader, "case": case, "lit_approximation": not exact, "passed": passed, **metrics})
                y = 28 + row * 156
                label = "PASS" if passed else "FAIL"
                draw.text((8, y), f"{case}  {label}  mean={metrics['mean_error']:.4f}", fill="#9fe2b8" if passed else "#f5b58c")
                diff = ImageChops.difference(a, b).point(lambda value: min(255, value * 4))
                for col, frame in enumerate((a, b, diff)):
                    sheet.paste(frame, (8 + col * 132, y + 18))
            sheet.save(report / f"{shader}-{page + 1}.png")
    (report / "metrics.json").write_bytes((json.dumps(results, indent=2) + "\n").encode("utf-8"))
    rows = ["# Original / Built-in visual comparison", "", "Fresh controlled captures; existing golden images are untouched.", "", "All 96 common-surface captures are gated: mean RGB error ≤ 0.01; maximum channel error ≤ 0.08; fraction of pixels with mean RGB error > 0.02 ≤ 0.01. Values are normalized to 0–1. Lit uses a diffuse dielectric with specular/reflections disabled here; BRDF, shadows and reflection acceptance also require the dedicated PlayMode tests.", "", "| Shader | Feature | Status | Mean | Maximum | Bad pixel fraction |", "|---|---|---|---:|---:|---:|"]
    for item in results:
        status = "PASS" if item["passed"] else "FAIL"
        rows.append(f"| {item['shader']} | {item['case']} | {status} | {item['mean_error']:.5f} | {item['max_error']:.5f} | {item['bad_pixel_fraction']:.5f} |")
    if pbr:
        rows[4] = "All 72 PBR captures use the same unchanged image thresholds as the common-feature suite. Both Lit shaders cover metallic/specular workflows, three smoothness values, three light/view angles, direct GGX and environment Fresnel/mip response. No golden images are modified."
    (report / "report.md").write_bytes(("\n".join(rows) + "\n").encode("utf-8"))
    failures = [item for item in results if not item["passed"]]
    print(json.dumps({"captures": len(results), "failures": len(failures), "failing_cases": [f"{item['shader']}/{item['case']}" for item in failures]}))
    return 1 if failures else 0


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--reference", type=Path, required=True)
    parser.add_argument("--actual", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--pbr", action="store_true")
    args = parser.parse_args()
    raise SystemExit(run(args.reference, args.actual, args.report, args.pbr))
