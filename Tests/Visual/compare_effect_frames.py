"""Compare Fire001 captures and deterministic particle state without changing baselines."""
import argparse
import json
from pathlib import Path

from compare_frames import measure, validate_inventory, within_thresholds


def run(reference, actual, output):
    from PIL import Image
    stamps = ("0025", "0075", "0150", "0300", "0500", "0800")
    expected = {f"{stamp}-{angle:02}.ppm" for stamp in stamps for angle in (0, 35, 80)}
    validate_inventory({p.name for p in reference.glob("*.ppm")}, {p.name for p in actual.glob("*.ppm")}, expected)
    for stamp in stamps:
        name = stamp + "-particles.txt"
        if (reference / name).read_bytes() != (actual / name).read_bytes():
            raise ValueError(f"Particle simulation differs: {name}")
    rows = []
    for name in sorted(expected):
        a = Image.open(reference / name).convert("RGB")
        b = Image.open(actual / name).convert("RGB")
        if a.size != (384, 384) or b.size != a.size:
            raise ValueError(f"Unexpected capture size: {name}")
        for frame in (a, b):
            background = frame.getpixel((0, 0))
            pixels = frame.get_flattened_data() if hasattr(frame, "get_flattened_data") else frame.getdata()
            if sum(max(abs(c - d) for c, d in zip(pixel, background)) > 8 for pixel in pixels) <= 100:
                raise ValueError(f"No visible effect: {name}")
        metrics = measure(a.tobytes(), b.tobytes())
        rows.append({"frame": name, "passed": within_thresholds(metrics), **metrics})
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_bytes((json.dumps({"particle_state_identical": True, "frames": rows}, indent=2) + "\n").encode("utf-8"))
    failures = sum(not row["passed"] for row in rows)
    print(json.dumps({"frames": len(rows), "failures": failures, "largest_channel_error": max(row["max_error"] for row in rows)}))
    return failures


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--reference", type=Path, required=True)
    parser.add_argument("--actual", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    raise SystemExit(bool(run(args.reference, args.actual, args.output)))
