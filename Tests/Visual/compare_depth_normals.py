"""Compare the dedicated normal/coverage fixture without replacing images or relaxing thresholds."""
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
    features = ('plain', 'normal', 'array', 'legacy-flow', 'target-flow', 'channel-luminance', 'deform', 'custom-alpha')
    expected = {f'{kind}-{clip}-{negative}-{feature}.ppm' for kind in ('UberUnlit', 'UberLit')
                for clip in (False, True) for negative in (False, True) for feature in features}
    for folder in (args.reference, args.candidate):
        actual = {p.name for p in folder.glob("*.ppm")}
        if actual != expected:
            raise SystemExit(f"{folder}: missing={sorted(expected - actual)}, unexpected={sorted(actual - expected)}")
    rows = []
    for name in sorted(expected):
        with Image.open(args.reference / name) as reference, Image.open(args.candidate / name) as candidate:
            if reference.size != (64, 64) or candidate.size != reference.size:
                raise SystemExit(f"Unexpected dimensions: {name}")
            row = dict(name=name, **measure(list(reference.convert("RGB").tobytes()), list(candidate.convert("RGB").tobytes())))
        row["passed"] = within_thresholds(row)
        rows.append(row)
    report = {"total": len(rows), "failed": sum(not row["passed"] for row in rows), "frames": rows}
    args.output.write_text(json.dumps(report, indent=2) + "\n", encoding="utf8", newline="\n")
    print(f"{report['total']} comparisons, {report['failed']} failed")
    return int(report["failed"] != 0)


if __name__ == "__main__":
    raise SystemExit(main())
