"""Populate a CLOSED, disposable Built-in Unity project, never the avatar project."""
import argparse
import hashlib
import json
import re
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
p = argparse.ArgumentParser()
p.add_argument('--project', type=Path, required=True)
p.add_argument('--baseline-shader', type=Path, required=True)
p.add_argument('--baseline-include', type=Path, required=True)
args = p.parse_args()
if not (args.project / 'ProjectSettings/ProjectVersion.txt').is_file():
    raise SystemExit('Expected an existing isolated Unity project')
if (args.project / 'Assets/Codex/NovaVRCValidation').exists():
    raise SystemExit('Do not install measurement code in the avatar project')
folder = args.project / 'Assets/NovaBenchmark'
(folder / 'Editor').mkdir(parents=True, exist_ok=True)
shutil.copytree(ROOT / 'Assets/Nova', args.project / 'Assets/Nova', dirs_exist_ok=True)
for relative in ['NovaAvatarGpuBenchmark.cs', 'Editor/NovaAvatarGpuBenchmarkBuilder.cs']:
    shutil.copyfile(Path(__file__).parent / relative, folder / relative)
shader = args.baseline_shader.read_text(encoding='utf-8')
shader = shader.replace('Shader "Nova/Particles/Distortion"', 'Shader "Hidden/NovaBenchmark/BaselineDistortion"')
shader = re.sub(r'#include "BuiltIn/([^"]+)"', r'#include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/\1"', shader)
shader = shader.replace('Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInDistortion.cginc', 'Assets/NovaBenchmark/NovaBuiltInDistortionBaseline.cginc')
(folder / 'BaselineDistortion.shader').write_text(shader, encoding='utf-8', newline='\n')
include = args.baseline_include.read_text(encoding='utf-8')
include = re.sub(r'#include "(Nova[^"]+)"', r'#include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/\1"', include)
(folder / 'NovaBuiltInDistortionBaseline.cginc').write_text(include, encoding='utf-8', newline='\n')
files = [args.baseline_shader, args.baseline_include, *folder.rglob('*.cs'), *folder.glob('*.shader'),
         *folder.glob('*.cginc'), ROOT / 'Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInDistortion.cginc',
         Path(__file__).parent / 'Native/NovaGpuTimer.cpp']
manifest = {str(file): hashlib.sha256(file.read_bytes()).hexdigest() for file in files}
(folder / 'source-hashes.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
print(folder)
