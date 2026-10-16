"""Stage only ElderStar package files for Unity's ExportPackage and Client.Pack."""
import argparse
import json
from pathlib import Path
import shutil

ALLOWED = {"Documentation~", "Editor", "Runtime", "Samples~", "CHANGELOG.md",
           "LICENSE.md", "package.json", "README.md", "Editor.meta", "Runtime.meta",
           "CHANGELOG.md.meta", "LICENSE.md.meta", "package.json.meta", "README.md.meta"}


def stage(repo, output, mode):
    if mode not in ("upm", "assets"):
        raise ValueError("Choose upm or assets")
    package = Path(repo) / "Assets/Nova"
    output = Path(output)
    if output.exists():
        raise FileExistsError(output)
    unexpected = {p.name for p in package.iterdir()} - ALLOWED
    if unexpected:
        raise ValueError(f"Unreviewed package files: {sorted(unexpected)}")
    if any(p.is_symlink() for p in package.rglob("*")):
        raise ValueError("Release staging does not follow symlinks")
    manifest = json.loads((package / "package.json").read_text(encoding="utf-8"))
    if manifest["name"] != "me.smizuoch.elderstar":
        raise ValueError("Unexpected package identity")
    output.mkdir(parents=True)
    for source in sorted(package.iterdir()):
        name = source.name
        if mode == "assets":
            name = {"Documentation~": "Documentation", "Samples~": "Samples"}.get(name, name)
        destination = output / name
        if source.is_dir():
            shutil.copytree(source, destination)
        else:
            shutil.copyfile(source, destination)
    if mode == "assets":
        # Unity ignores folders ending in '~' under Assets. Expose only the docs
        # and sample folders; production code, their paths and .meta stay exact.
        for path in output.rglob("*.md"):
            raw = path.read_bytes()
            path.write_bytes(raw.replace(b"Documentation~/", b"Documentation/")
                             .replace(b"Samples~/", b"Samples/"))
        for sample in manifest.get("samples", []):
            sample["path"] = sample["path"].replace("Samples~/", "Samples/")
        (output / "package.json").write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n",
                                            encoding="utf-8", newline="\n")
    return manifest["version"]


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--mode", choices=("assets", "upm"), required=True)
    args = parser.parse_args()
    print("Staged ElderStar", stage(args.repo, args.output, args.mode), args.mode, "at", args.output)
