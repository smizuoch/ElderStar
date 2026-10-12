"""Compare pinned MIT NOVA samples, fresh materials and CC0 Kenney textures."""
import argparse
import json
from pathlib import Path

from compare_frames import measure, validate_inventory, within_thresholds


def compare_frame(reference, actual, require_visible):
    if reference.size != (256, 256) or actual.size != reference.size:
        raise ValueError("Unexpected corpus frame dimensions")
    if require_visible:
        for frame in (reference, actual):
            from PIL import Image, ImageChops
            difference = ImageChops.difference(frame, Image.new("RGB", frame.size, frame.getpixel((0, 0))))
            red, green, blue = difference.split()
            maximum = ImageChops.lighter(ImageChops.lighter(red, green), blue)
            if sum(maximum.histogram()[9:]) < 30:
                raise ValueError("No visible geometry in corpus frame")
    a, b = reference.tobytes(), actual.tobytes()
    metrics = ({"mean_error": 0.0, "max_error": 0.0, "bad_pixel_fraction": 0.0}
               if a == b else measure(a, b))
    return {"passed": within_thresholds(metrics), **metrics}


def compare_states(reference, actual):
    expected = {p.name for p in reference.glob("*-state.txt")}
    if not expected or expected != {p.name for p in actual.glob("*-state.txt")}:
        raise ValueError("Mismatched material/particle state inventory")
    for name in sorted(expected):
        if (reference / name).read_bytes() != (actual / name).read_bytes():
            raise ValueError("Material/particle state differs: " + name)
    return len(expected)


def run(reference, actual, output, unity6_fire_crash=False):
    from PIL import Image
    expected = set((reference / "inventory.txt").read_text(encoding="utf-8-sig").splitlines())
    count = 500 if unity6_fire_crash else 509
    if unity6_fire_crash:
        for folder in (reference, actual):
            if not (folder / "engine-crash-skip.txt").is_file():
                raise ValueError("Missing documented engine crash exclusion")
    if len(expected) != count or expected != set((actual / "inventory.txt").read_text(encoding="utf-8-sig").splitlines()):
        raise ValueError("Expected 509 corpus captures (13*9 + 96*4 + 4*2)")
    validate_inventory({p.name for p in reference.glob("*.ppm")}, {p.name for p in actual.glob("*.ppm")}, expected)
    states = compare_states(reference, actual)
    if states != (44 if unity6_fire_crash else 47):
        raise ValueError("Expected 39 particle states and 8 fresh material states")
    rows = []
    visible_effects = set()
    for name in sorted(expected):
        a = Image.open(reference / name).convert("RGB")
        b = Image.open(actual / name).convert("RGB")
        is_effect = name.startswith("pfb_eff_")
        metrics = compare_frame(a, b, require_visible=not is_effect)
        if is_effect:
            try:
                compare_frame(a, b, require_visible=True)
                visible_effects.add(name.rsplit("-", 2)[0])
            except ValueError:
                pass  # A burst may have finished at later times, but each effect must be visible somewhere.
        rows.append({"frame": name, **metrics})
    if len(visible_effects) != (12 if unity6_fire_crash else 13):
        raise ValueError("Missing visible effect(s): " + str(visible_effects))
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_bytes((json.dumps({"identical_states": states, "visible_effects": len(visible_effects), "frames": rows}, indent=2) + "\n").encode())
    failures = [r for r in rows if not r["passed"]]
    print(json.dumps({"frames": len(rows), "failures": len(failures), "failing_frames": [r["frame"] for r in failures]}))
    return bool(failures)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--reference", type=Path, required=True)
    parser.add_argument("--actual", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--unity6-fire-crash", action="store_true")
    args = parser.parse_args()
    raise SystemExit(run(args.reference, args.actual, args.output, args.unity6_fire_crash))
