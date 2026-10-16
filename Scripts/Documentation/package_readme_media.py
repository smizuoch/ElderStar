"""Package real Unity documentation captures; requires Pillow and NumPy.

No visual retouching: PNGs are copied verbatim and camera frames are GIF-encoded.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--inspectors", required=True, type=Path)
    parser.add_argument("--effects", required=True, type=Path)
    parser.add_argument("--distortion", required=True, type=Path)
    parser.add_argument("--log", required=True, type=Path)
    args = parser.parse_args()
    log = args.log.read_text(encoding="utf-8", errors="replace")
    assert "ELDERSTAR_README_CAPTURE_COMPLETE=" in log
    assert "error CS" not in log and "Shader error" not in log
    target = ROOT / "Assets/Nova/Documentation~/Images"
    target.mkdir(parents=True, exist_ok=True)
    manifest = {
        "package_id": "me.smizuoch.elderstar", "package_version": "0.1.8",
        "unity": "2022.3.22f1", "graphics_api": "Direct3D11", "color_space": "Gamma",
        "date": "2026-10-16", "shader_errors": 0,
        "upstream_samples_commit": "1c6b88729f91c852c164ef71a32cc2bce83a1892",
        "capture_script_sha256": sha(ROOT / "Scripts/Unity/ElderStarReadmeCapture.cs"),
        "images": {},
    }
    for source in sorted(args.inspectors.glob("*.png")):
        if source.name == "Distortion.png":
            source = args.distortion / source.name
        image = Image.open(source)
        assert image.width in range(630, 660) and 100 < image.height < 900, source
        shutil.copyfile(source, target / source.name)
        manifest["images"][source.name] = {
            "source": "ElderStar Inspector drawing code: " + source.stem,
            "capture_method": "Bounded native Unity EditorWindow screenshot (@oai/sky)",
            "width": image.width, "height": image.height, "sha256": sha(source),
        }
    assert len(manifest["images"]) == 18
    for name, prefab in [("barrier", "pfb_eff_barrier01"), ("ice", "pfb_eff_ice02"),
                         ("slash", "pfb_eff_slash01"), ("heal", "pfb_eff_heal01")]:
        paths = sorted((args.effects / prefab).glob("*.png"))
        assert len(paths) == 48, prefab
        frames = [Image.open(p).convert("RGB") for p in paths]
        assert all(image.size == (960, 600) for image in frames)
        visible = [int(np.any(np.asarray(image) > 100, axis=2).sum()) for image in frames]
        assert max(visible) > 800, f"Invisible effect: {prefab}"
        assert len({sha(p) for p in paths}) > 8, f"Frozen animation: {prefab}"
        destination = target / (name + ".gif")
        # GIF uses centiseconds. 40/40/40/40/50/40 matches 24 FPS over six frames.
        durations = [40, 40, 40, 40, 50, 40] * 8
        frames[0].save(destination, save_all=True, append_images=frames[1:],
                       duration=durations, loop=0, disposal=2, optimize=True)
        manifest["images"][destination.name] = {
            "source": "NOVA MIT Assets/Samples/Prefabs/" + prefab + ".prefab",
            "author": "@Ugokashiya / CyberAgent, Inc.",
            "capture_method": "ElderStar Camera.Render -> RenderTexture -> PNG sequence -> GIF",
            "width": 960, "height": 600, "input_frames": 48, "duration_ms": sum(durations),
            "simulation_seconds": [0.1, 0.1 + 47 / 24], "max_visible_pixels": max(visible),
            "input_sha256": hashlib.sha256("".join(sha(p) for p in paths).encode()).hexdigest(),
            "sha256": sha(destination),
        }
    (target / "captures.json").write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8", newline="\n")
    print("Packaged", len(manifest["images"]), "real captures;", sum((target / n).stat().st_size for n in manifest["images"]), "bytes")


if __name__ == "__main__":
    main()
