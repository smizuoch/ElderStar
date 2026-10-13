"""Fail closed on incomplete runs, differing inputs, or unchanged strict thresholds."""
import argparse
import json
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw
from compare_frames import SHADERS, measure, within_thresholds


def expected_frames():
    return {f'{shader}__{i:03}' for shader in SHADERS for i in range(96)} | {
        f'Particles_Distortion__{i:03}' for i in range(32)}


def validate_metadata(reference, actual):
    a, b = reference.splitlines(), actual.splitlines()
    if (a != b or len(a) != 6 or a[1] != 'Direct3D11'
            or a[2] not in ('Gamma', 'Linear') or a[3:5] != ['seed=20261013', '416 frames']
            or a[5] not in ('anisotropy=ForceEnable', 'anisotropy=Disable')):
        raise ValueError('Different or incomplete capture metadata')


def validate_run(reference, actual):
    expected = expected_frames()
    for root in (reference, actual):
        if {p.stem for p in root.glob('*.ppm')} != expected:
            raise ValueError('Expected exactly 416 rendered cases in ' + str(root))
        if {p.stem for p in root.glob('*.json')} != expected:
            raise ValueError('Expected exactly 416 input states in ' + str(root))
    for name in sorted(expected):
        a = json.loads((reference / (name + '.json')).read_text(encoding='utf-8'))
        b = json.loads((actual / (name + '.json')).read_text(encoding='utf-8'))
        if a != b:
            raise ValueError('Different input state: ' + name)
    return expected


def compare_case(a, b, control=False):
    if a.size != (128, 128) or b.size != a.size:
        raise ValueError('Expected matching 128 x 128 images')
    if control:
        for frame in (a, b):
            bg = frame.getpixel((0, 0))
            pixels = frame.get_flattened_data() if hasattr(frame, 'get_flattened_data') else frame.getdata()
            if sum(max(abs(x-y) for x,y in zip(p,bg)) > 8 for p in pixels) < 1638:
                raise ValueError('No visible control geometry')
    metrics = measure(a.tobytes(), b.tobytes())
    return dict(passed=within_thresholds(metrics), **metrics)


def run(reference, actual, report):
    names = validate_run(reference, actual)
    validate_metadata((reference / 'capture.txt').read_text(), (actual / 'capture.txt').read_text())
    report.mkdir(parents=True, exist_ok=True)
    rows = []
    examples = []
    for name in sorted(names):
        a = Image.open(reference / (name + '.ppm')).convert('RGB')
        b = Image.open(actual / (name + '.ppm')).convert('RGB')
        item = dict(case=name, **compare_case(a, b, control=name.endswith('__000')))
        rows.append(item)
        if not item['passed'] or int(name[-3:]) % 8 == 0:
            examples.append((name, a.copy(), b.copy(), item))
    for page in range((len(examples) + 7) // 8):
        sheet = Image.new('RGB', (416, 8 * 156 + 24), '#181b22')
        draw = ImageDraw.Draw(sheet)
        draw.text((8, 4), 'Original URP       Built-in           Difference x4', fill='white')
        for row, (name, a, b, item) in enumerate(examples[page*8:page*8+8]):
            y = 24 + row*156
            draw.text((8,y), name + (' PASS' if item['passed'] else ' FAIL'), fill='white')
            diff = ImageChops.difference(a,b).point(lambda v: min(255,v*4))
            for col, frame in enumerate((a,b,diff)): sheet.paste(frame,(8+132*col,y+18))
        sheet.save(report / f'comparison-{page:02}.png')
    result = dict(cases=len(rows), input_states_equal=len(names),
                  failures=sum(not r['passed'] for r in rows),
                  pixel_identical=sum(r['max_error'] == 0 for r in rows), results=rows)
    (report / 'metrics.json').write_text(json.dumps(result, indent=2)+'\n',encoding='utf-8')
    print(json.dumps({k:v for k,v in result.items() if k != 'results'}))
    return int(result['failures'] != 0)


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    for arg in ('reference', 'actual', 'report'): p.add_argument('--'+arg, type=Path, required=True)
    args = p.parse_args()
    raise SystemExit(run(args.reference, args.actual, args.report))
