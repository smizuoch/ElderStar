import json
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[2]


class CoverageInventoryTests(unittest.TestCase):
    def test_every_original_editor_declaration_has_specific_evidence(self):
        report = json.loads((ROOT / 'Documentation/Verification/ApiFeatureCoverage20261013.json').read_text(encoding='utf-8'))
        original = json.loads((ROOT / 'Assets/Tests/Editor/Reference/EditorContract.json').read_text(encoding='utf-8'))['entries']
        self.assertEqual(sorted(original), sorted(row['declaration'] for row in report['editor_apis']))
        for row in report['editor_apis']:
            self.assertTrue(row['scope'])
            for evidence in row['evidence']:
                self.assertTrue((ROOT / evidence).is_file(), evidence)

    def test_material_inventory_count_matches_original_declarations(self):
        report = json.loads((ROOT / 'Documentation/Verification/ApiFeatureCoverage20261013.json').read_text(encoding='utf-8'))['shader_properties']
        original = json.loads((ROOT / report['reference']).read_text(encoding='utf-8'))
        self.assertEqual(sum(len(shader['properties']) for shader in original['shaders'].values()), report['count'])
        for evidence in report['evidence']:
            self.assertTrue((ROOT / evidence).is_file(), evidence)

    def test_original_runtime_apis_have_explicit_pipeline_dispositions(self):
        report = json.loads((ROOT / 'Documentation/Verification/ApiFeatureCoverage20261013.json').read_text(encoding='utf-8'))
        entries = report['runtime_pipeline_apis']['declarations']
        self.assertGreaterEqual(len(entries), 10)
        for entry in entries:
            self.assertTrue(entry['declaration'])
            self.assertTrue(entry['editors'])
            self.assertIn(entry['status'], ['Exact', 'Approximate', 'Not source-compatible'])
            self.assertTrue(entry['reason_or_fallback'])

    def test_every_feature_has_explicit_status_and_evidence(self):
        report = json.loads((ROOT / 'Documentation/Verification/ApiFeatureCoverage20261013.json').read_text(encoding='utf-8'))
        rows = [line.split('|')[1].strip() for line in (ROOT / 'Documentation/BuiltInCompatibilityMatrix.md').read_text(encoding='utf-8').splitlines()
                if line.startswith('|') and any('| ' + s + ' |' in line for s in ['Exact', 'Approximate', 'Unsupported', 'Not Applicable'])]
        self.assertEqual(sorted(rows), sorted(row['feature'] for row in report['features']))
        matrix = {}
        for line in (ROOT / 'Documentation/BuiltInCompatibilityMatrix.md').read_text(encoding='utf-8').splitlines():
            if line.startswith('|'):
                cells = [cell.strip() for cell in line.strip('|').split('|')]
                if len(cells) == 4 and cells[0] in rows:
                    matrix[cells[0]] = cells[1:]
        for row in report['features']:
            self.assertIn(row['status'], ['Exact', 'Approximate', 'Unsupported', 'Not Applicable'])
            self.assertTrue(row['evidence'])
            self.assertTrue(row['reason_or_fallback'])
            self.assertEqual(matrix[row['feature']], [row['status'], row['evidence'], row['reason_or_fallback']])


if __name__ == '__main__': unittest.main()
