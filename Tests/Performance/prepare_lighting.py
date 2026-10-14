"""Prepare a CLOSED disposable project with completely independent baseline includes."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser()
parser.add_argument('--project', type=Path, required=True)
parser.add_argument('--baseline-tree', type=Path, required=True)
args = parser.parse_args()
if not (args.project / 'ProjectSettings/ProjectVersion.txt').is_file():
    raise SystemExit('Expected an isolated existing Unity project')
if (args.project / 'Assets/Codex/NovaVRCValidation').exists():
    raise SystemExit('Do not modify the avatar project')
shaders = args.baseline_tree / 'Assets/Nova/Runtime/Core/Shaders'
folder = args.project / 'Assets/NovaBenchmark'
(folder / 'Editor').mkdir(parents=True, exist_ok=True)
shutil.copytree(ROOT / 'Assets/Nova', args.project / 'Assets/Nova', dirs_exist_ok=True)
for relative in ['NovaLightingGpuBenchmark.cs', 'Editor/NovaLightingGpuBenchmarkBuilder.cs']:
    shutil.copyfile(Path(__file__).parent / relative, folder / relative)
baseline = folder / 'LightingBaseline'
shutil.copytree(shaders / 'BuiltIn', baseline / 'BuiltIn', dirs_exist_ok=True,
                ignore=shutil.ignore_patterns('*.meta'))
for kind in ['ParticlesUberUnlit', 'ParticlesUberLit', 'UIParticlesUberUnlit', 'UIParticlesUberLit']:
    source = (shaders / (kind + '.shader')).read_text(encoding='utf8')
    source = source.replace('Shader "Nova/', 'Shader "Hidden/NovaLightingBaseline/', 1)
    (baseline / (kind + '.shader')).write_text(source, encoding='utf8', newline='\n')
files = [p for base in [folder, args.project / 'Assets/Nova'] for p in base.rglob('*')
         if p.is_file() and p.suffix in ('.cs', '.cginc', '.shader', '.asmdef')]
manifest = {str(p.relative_to(args.project)): hashlib.sha256(p.read_bytes()).hexdigest() for p in files}
(folder / 'lighting-source-hashes.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf8', newline='\n')
print(folder)
