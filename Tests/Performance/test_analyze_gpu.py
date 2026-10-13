import copy
import unittest
from analyze_gpu import analyze, check_zero_culling


def fixture():
    return dict(completed=True, gpuTimingAvailable=True, error='', blocks=[
        dict(scenario=s, variant=('baseline' if i % 4 in (0, 3) else 'candidate'),
             repeat=i // 4, width=1920, height=1080,
             gpuMs=[1.0 if i % 4 in (0, 3) else .9] * 128)
        for s in ['one', 'eight-active', 'eight-plus-32-zero', '32-active', 'mirror-eight-plus-16-zero']
        for i in range(12)])


class GpuAnalysisTests(unittest.TestCase):
    def test_uniform_zero_budget_rejects_pixel_work_for_invisible_layers(self):
        check_zero_culling(analyze(fixture()))
        r = fixture()
        for block in r['blocks']:
            if block['scenario'] == 'eight-plus-32-zero': block['gpuMs'] = [2.0] * 128
        with self.assertRaises(ValueError): check_zero_culling(analyze(r))

    def test_reports_paired_block_median_differences(self):
        result = analyze(fixture())
        self.assertEqual(len(result), 5)
        self.assertAlmostEqual(result[0]['median_delta_ms'], -.1)
        self.assertEqual(result[0]['samples_per_variant'], 768)

    def test_rejects_incomplete_or_failed_run(self):
        for field, value in [('completed', False), ('gpuTimingAvailable', False), ('error', 'timeout')]:
            r = fixture(); r[field] = value
            with self.assertRaises(ValueError): analyze(r)

    def test_rejects_missing_or_duplicate_block(self):
        r = fixture(); r['blocks'].pop()
        with self.assertRaises(ValueError): analyze(r)
        r = fixture(); r['blocks'][1] = copy.deepcopy(r['blocks'][0])
        with self.assertRaises(ValueError): analyze(r)

    def test_rejects_zero_nan_and_infinite_gpu_time(self):
        for value in [0, -1, float('nan'), float('inf')]:
            r = fixture(); r['blocks'][0]['gpuMs'][3] = value
            with self.assertRaises(ValueError): analyze(r)

    def test_rejects_wrong_size_and_sample_count(self):
        for field, value in [('width', 128), ('gpuMs', [1.0] * 127)]:
            r = fixture(); r['blocks'][0][field] = value
            with self.assertRaises(ValueError): analyze(r)


if __name__ == '__main__': unittest.main()
