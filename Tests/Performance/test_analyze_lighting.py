import unittest

from analyze_lighting import analyze


def fixture():
    blocks = []
    for i in range(12):
        blocks.append(dict(scenario='ordinary-lit', optional=False, repeat=i // 4,
                           variant='baseline' if i % 4 in (0, 3) else 'candidate',
                           width=1920, height=1080, gpuMs=[.2] * 128, cpuMs=[.1] * 128,
                           draws=1, materialBytes=512, renderTextureBytes=1920 * 1080 * 8))
    return dict(completed=True, gpuTimingAvailable=True, error=None, blocks=blocks)


class LightingAcceptanceTests(unittest.TestCase):
    def test_accepts_equal_cost(self):
        self.assertTrue(analyze(fixture())[0]['accepted'])

    def test_fails_normal_path_over_absolute_or_relative_budget(self):
        report = fixture()
        for block in report['blocks']:
            if block['variant'] == 'candidate':
                block['gpuMs'] = [.23] * 128
        self.assertFalse(analyze(report)[0]['accepted'])

    def test_optional_workload_reports_cost_without_claiming_default_acceptance(self):
        report = fixture()
        for block in report['blocks']:
            block['optional'] = True
            if block['variant'] == 'candidate':
                block['gpuMs'] = [.5] * 128
        self.assertIsNone(analyze(report)[0]['accepted'])

    def test_rejects_missing_or_nonfinite_samples_and_bad_order(self):
        for change in ('missing', 'nan', 'order', 'cpu'):
            report = fixture()
            if change == 'missing': report['blocks'].pop()
            if change == 'nan': report['blocks'][0]['gpuMs'][0] = float('nan')
            if change == 'order': report['blocks'][1]['variant'] = 'baseline'
            if change == 'cpu': report['blocks'][0]['cpuMs'] = []
            with self.assertRaises(ValueError, msg=change): analyze(report)

    def test_normal_path_cannot_add_draws(self):
        report = fixture()
        for block in report['blocks']:
            if block['variant'] == 'candidate': block['draws'] = 2
        self.assertFalse(analyze(report)[0]['accepted'])


if __name__ == '__main__':
    unittest.main()
