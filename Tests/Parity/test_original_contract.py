"""Frozen original-source contracts; no dependency on the untracked reference checkout."""
import hashlib
import json
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]
SNAPSHOT = json.loads(Path(__file__).with_name("original-3.6-contract.json").read_text(encoding="utf-8"))
PACKAGE = ROOT / "Assets/Nova"


def tokens(source):
    pattern = r'//[^\n]*|/\*[\s\S]*?\*/|@"(?:[^"]|"")*"|"(?:\\.|[^"\\])*"|\w+|[^\s]'
    return "\n".join(m.group() for m in re.finditer(pattern, source)
                     if not m.group().startswith(("//", "/*")))


class OriginalContractTests(unittest.TestCase):
    def test_every_property_keeps_attributes_label_type_range_default_and_order(self):
        for name, contract in SNAPSHOT["shaders"].items():
            with self.subTest(shader=name):
                source = (PACKAGE / "Runtime/Core/Shaders" / name).read_text(encoding="utf-8-sig")
                declarations = [line.strip() for line in source.split("SubShader")[0].splitlines()
                                if re.match(r'\s*(?:\[[^\]]*\]\s*)*_[A-Za-z]', line)]
                self.assertEqual(declarations, contract["properties"])
                self.assertEqual(re.findall(r'CustomEditor\s+"([^"]+)"', source), contract["custom_editors"])

    def test_inspector_layout_labels_tooltips_controls_and_menu_contracts_match(self):
        for relative, expected in SNAPSHOT["editor_source_contracts"].items():
            with self.subTest(file=relative):
                source = (PACKAGE / relative).read_text(encoding="utf-8-sig")
                self.assertEqual(hashlib.sha256(tokens(source).encode()).hexdigest(), expected)

    def test_icons_and_import_settings_match_the_original_assets(self):
        for name, expected in SNAPSHOT["icons"].items():
            with self.subTest(resource=name):
                self.assertEqual(hashlib.sha256((PACKAGE / "Editor/Core/Resources" / name).read_bytes()).hexdigest(), expected)

    def test_token_normalization_preserves_visible_string_whitespace(self):
        self.assertEqual(tokens('void A() { /* comment */ X(); }'), tokens('void A(){X();}'))
        self.assertNotEqual(tokens('"Render Type"'), tokens('"RenderType"'))


if __name__ == "__main__":
    unittest.main()
