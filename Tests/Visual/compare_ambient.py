"""Strict original/port ambient comparison; never rewrites reference images."""
import argparse
import json
from pathlib import Path
from PIL import Image
from compare_frames import measure, within_thresholds


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("reference", type=Path)
    parser.add_argument("candidate", type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    expected = {f"Nova-{family}-UberLit_{ambient}_{intensity}_{surface}.ppm"
                for family in ("Particles", "UIParticles")
                for ambient in ("black", "flat", "colored", "gradient", "sky-probe")
                for intensity in (0, 1) for surface in ("diffuse", "dielectric", "metal")}
    for folder in (args.reference, args.candidate):
        if {p.name for p in folder.glob("*.ppm")} != expected:
            raise SystemExit(f"Incomplete or unexpected capture set: {folder}")
    if (args.reference / "metadata.txt").read_text() != (args.candidate / "metadata.txt").read_text():
        raise SystemExit("Editor/color-space/backend mismatch")
    rows = []
    for name in sorted(expected):
        with Image.open(args.reference / name) as reference, Image.open(args.candidate / name) as candidate:
            if reference.size != (128, 128) or candidate.size != reference.size:
                raise SystemExit(f"Unexpected dimensions: {name}")
            row = dict(name=name, **measure(list(reference.convert("RGB").tobytes()), list(candidate.convert("RGB").tobytes())))
        row["passed"] = within_thresholds(row)
        rows.append(row)
    report = {"total": len(rows), "failed": sum(not r["passed"] for r in rows), "frames": rows}
    args.output.write_text(json.dumps(report, indent=2) + "\n", encoding="utf8")
    print(f"{report['total']} comparisons, {report['failed']} failed")
    return int(report["failed"] != 0)


if __name__ == "__main__":
    raise SystemExit(main())
