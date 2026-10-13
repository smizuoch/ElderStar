"""Summarize complete ABBA GPU runs; retain block variation, not a fake certainty interval."""
import argparse
import json
import math
from pathlib import Path
from statistics import median

SCENARIOS = ['one', 'eight-active', 'eight-plus-32-zero', '32-active', 'mirror-eight-plus-16-zero']


def analyze(report):
    if not report['completed'] or not report['gpuTimingAvailable'] or report['error']:
        raise ValueError('No successful GPU measurement')
    blocks = report['blocks']
    if len(blocks) != 60:
        raise ValueError('Expected 60 ABBA blocks')
    rows = []
    for scenario_index, scenario in enumerate(SCENARIOS):
        group = blocks[scenario_index * 12:(scenario_index + 1) * 12]
        for i, block in enumerate(group):
            if (block['scenario'] != scenario or block['repeat'] != i // 4
                    or block['variant'] != ('baseline' if i % 4 in (0, 3) else 'candidate')
                    or (block['width'], block['height']) != (1920, 1080)
                    or len(block['gpuMs']) != 128
                    or any(not math.isfinite(x) or x <= 0 for x in block['gpuMs'])):
                raise ValueError('Invalid GPU block')
        deltas = []
        for i in (0, 4, 8):
            before = median(group[i]['gpuMs']); after = median(group[i + 3]['gpuMs'])
            candidate = (median(group[i + 1]['gpuMs']) + median(group[i + 2]['gpuMs'])) / 2
            deltas.append(candidate - (before + after) / 2)
        row = dict(scenario=scenario, samples_per_variant=768, paired_deltas_ms=deltas,
                   median_delta_ms=median(deltas), delta_min_ms=min(deltas), delta_max_ms=max(deltas))
        for variant in ('baseline', 'candidate'):
            samples = sorted(x for b in group if b['variant'] == variant for x in b['gpuMs'])
            row[variant + '_median_ms'] = median(samples)
            row[variant + '_p95_ms'] = samples[math.ceil(len(samples) * .95) - 1]
        rows.append(row)
    return rows


def check_zero_culling(rows):
    costs = {row['scenario']: row['candidate_median_ms'] for row in rows}
    if costs['eight-plus-32-zero'] > 1.5 * costs['eight-active']:
        raise ValueError('32 uniform-zero surfaces add more than 50% of eight active surfaces')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('input', type=Path); parser.add_argument('output', type=Path)
    parser.add_argument('--check-zero-culling', action='store_true')
    args = parser.parse_args()
    rows = analyze(json.loads(args.input.read_text(encoding='utf-8')))
    if args.check_zero_culling: check_zero_culling(rows)
    args.output.write_text(json.dumps(rows, indent=2) + '\n', encoding='utf-8')
    for row in rows:
        print(row['scenario'], 'baseline/candidate:', round(row['baseline_median_ms'], 5),
              round(row['candidate_median_ms'], 5), 'paired delta ms:', round(row['median_delta_ms'], 5))
