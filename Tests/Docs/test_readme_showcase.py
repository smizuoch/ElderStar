"""Reader-facing guide links and provenance of the locally captured media."""
from pathlib import Path
import hashlib
import json
import re
import unittest
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[2]
PACKAGE = ROOT / "Assets/Nova"


class ReadmeShowcaseTests(unittest.TestCase):
    def test_guide_covers_authoring_and_the_actual_installation(self):
        text = (ROOT / "README.md").read_text(encoding="utf-8")
        for heading in ["セットアップ", "NOVA用の素材をそのまま使う", "使い方",
                        "Uber Unlit", "Uber Lit", "Distortion", "uGUI",
                        "Custom Vertex Streams", "Mesh GPU Instancing", "最適化", "互換性", "ライセンス"]:
            self.assertRegex(text, r"(?m)^#{2,4} .*" + re.escape(heading))
        self.assertIn("https://github.com/smizuoch/ElderStar.git?path=/Assets/Nova", text)
        self.assertIn("me.smizuoch.elderstar", text)
        self.assertIn("Assets > Import Package > ElderStar Effect...", text)
        self.assertIn("ワールド", text)
        self.assertNotIn("最大50%", text, "Do not inherit upstream performance claims.")
        self.assertNotIn("OptimizedShaderGenerator.Replace()", text, "The replacer is a separate class.")

    def test_repository_and_package_guides_have_the_same_content(self):
        root = (ROOT / "README.md").read_text(encoding="utf-8")
        package = (PACKAGE / "README.md").read_text(encoding="utf-8")
        self.assertEqual(root.replace("Assets/Nova/Documentation~/", "Documentation~/")
                         .replace("Assets/Nova/LICENSE.md", "LICENSE.md"), package)

    def test_local_markdown_and_html_links_resolve(self):
        paths = [ROOT / "README.md", PACKAGE / "README.md", *(PACKAGE / "Documentation~").glob("*.md")]
        for path in paths:
            text = path.read_text(encoding="utf-8")
            targets = re.findall(r"\]\(([^)]+)\)", text) + re.findall(r'(?:src|href)="([^"]+)"', text)
            for target in targets:
                if "://" in target or target.startswith("#"):
                    continue
                target = unquote(target.split("#")[0])
                self.assertTrue((path.parent / target).exists(), f"{path}: {target}")

    def test_guide_media_are_own_captures_with_matching_provenance(self):
        text = (PACKAGE / "README.md").read_text(encoding="utf-8")
        images = re.findall(r'<img[^>]+src="([^"]+)"', text)
        images += re.findall(r"!\[[^\]]*\]\(([^)]+)\)", text)
        images = [p for p in images if "shields.io" not in p]
        self.assertGreaterEqual(len(set(images)), 16)
        manifest = json.loads((PACKAGE / "Documentation~/Images/captures.json").read_text(encoding="utf-8"))
        self.assertEqual(manifest["shader_errors"], 0)
        self.assertEqual(manifest["package_id"], "me.smizuoch.elderstar")
        for image in images:
            self.assertFalse("://" in image, "Use local ElderStar captures, not upstream screenshots.")
            record = manifest["images"][Path(image).name]
            self.assertTrue(record["source"])
            self.assertTrue(record["capture_method"])
            self.assertEqual(hashlib.sha256((PACKAGE / image).read_bytes()).hexdigest(), record["sha256"])


if __name__ == "__main__":
    unittest.main()
