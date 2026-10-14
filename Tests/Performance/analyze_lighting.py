"""Validate offscreen lighting ABBA measurements before applying the approved cost gate."""
import argparse
import json
import math
from pathlib import Path
from statistics import median


def analyze(report):
    if not report['completed'] or not report['gpuTimingAvailable'] or report['error']:
        raise ValueError('No complete GPU measurement')
    blocks = report['blocks']
    if not blocks or len(blocks) % 12:
        raise ValueError('Each scenario needs twelve ABBA blocks')
    rows = []
    seen = set()
    for start in range(0, len(blocks), 12):
        group = blocks[start:start + 12]
        scenario = group[0]['scenario']
        optional = group[0]['optional']
        if scenario in seen: raise ValueError('Duplicate scenario')
        seen.add(scenario)
        for i, block in enumerate(group):
            if (block['scenario'] != scenario or block['optional'] != optional or block['repeat'] != i // 4
                    or block['variant'] != ('baseline' if i % 4 in (0, 3) else 'candidate')
                    or (block['width'], block['height']) != (1920, 1080)):
                raise ValueError('Invalid ABBA block')
            for key in ('gpuMs', 'cpuMs'):
                if len(block[key]) != 128 or any(not math.isfinite(x) or x <= 0 for x in block[key]):
                    raise ValueError('Missing or invalid timings')
            if any(block[key] < 0 for key in ('draws', 'materialBytes', 'renderTextureBytes')):
                raise ValueError('Missing resource measurements')
        row = dict(scenario=scenario, optional=optional)
        for variant in ('baseline', 'candidate'):
            selected = [b for b in group if b['variant'] == variant]
            for key in ('gpuMs', 'cpuMs'):
                row[variant + '_' + key] = median(x for b in selected for x in b[key])
            for key in ('draws', 'materialBytes', 'renderTextureBytes'):
                row[variant + '_' + key] = median(b[key] for b in selected)
        deltas = []
        for i in (0, 4, 8):
            a = (median(group[i]['gpuMs']) + median(group[i + 3]['gpuMs'])) / 2
            b = (median(group[i + 1]['gpuMs']) + median(group[i + 2]['gpuMs'])) / 2
            deltas.append(b - a)
        row['paired_delta_ms'] = deltas
        row['median_delta_ms'] = median(deltas)
        row['budget_ms'] = max(.02, row['baseline_gpuMs'] * .05)
        row['accepted'] = None if optional else (row['median_delta_ms'] <= row['budget_ms']
                                                   and row['candidate_draws'] <= row['baseline_draws'])
        rows.append(row)
    return rows


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('input', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    rows = analyze(json.loads(args.input.read_text(encoding='utf8')))
    args.output.write_text(json.dumps(rows, indent=2) + '\n', encoding='utf8', newline='\n')
    print(json.dumps(rows, indent=2))
    raise SystemExit(1 if any(r['accepted'] is False for r in rows) else 0)
