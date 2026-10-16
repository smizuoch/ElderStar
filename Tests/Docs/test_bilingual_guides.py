"""Language routes must stay within ElderStar and retain the same feature coverage."""
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]
PACKAGE = ROOT / "Assets/Nova"


class BilingualGuideTests(unittest.TestCase):
    def test_six_navigation_targets_exist_on_every_guide(self):
        guides = [ROOT / "README.md", ROOT / "README_JA.md", PACKAGE / "README.md", PACKAGE / "README_JA.md"]
        guides += [PACKAGE / "Documentation~" / kind / name for kind in ("Samples", "Demo")
                   for name in ("README.md", "README_JA.md")]
        for path in guides:
            text = path.read_text(encoding="utf-8")
            nav = re.search(r"\*\*Docs\*\*.*?\*\*Demo\*\*[^\n]+", text, re.S)
            self.assertIsNotNone(nav, path)
            targets = re.findall(r"\[(English|日本語)\]\(([^)]+)\)", nav.group())
            self.assertEqual([language for language, _ in targets], ["English", "日本語"] * 3)
            self.assertEqual(len({target for _, target in targets}), 6)
            for _, target in targets:
                self.assertNotIn("://", target, "Language routes must not send readers upstream.")
                self.assertTrue((path.parent / target).is_file(), f"{path}: {target}")

    def test_english_and_japanese_reference_cover_the_same_controls_and_images(self):
        en = (PACKAGE / "README.md").read_text(encoding="utf-8")
        ja = (PACKAGE / "README_JA.md").read_text(encoding="utf-8")
        self.assertIn("## Setup", en)
        self.assertIn("## セットアップ", ja)
        self.assertNotRegex(re.sub(r"\[日本語\]\([^)]*\)", "", en), r"[ぁ-んァ-ヶ一-龯]")
        self.assertEqual(re.findall(r'<a id="([^"]+)"', en), re.findall(r'<a id="([^"]+)"', ja))
        controls = lambda s: re.findall(r"<tr>(?:<td[^>]*>\s*</td>)*<td[^>]*>\s*<b>(.*?)</b>", s)
        # Only the table-header labels are translated. Every property label is shared.
        self.assertEqual([c for c in controls(en) if c not in ("Property Name", "Description")],
                         [c for c in controls(ja) if c not in ("プロパティ名", "説明")])
        images = lambda s: re.findall(r'<img[^>]+src="([^"]+)"', s)
        self.assertEqual(images(en), images(ja))
        for token in ("Assets > Import Package > ElderStar Effect...", "me.smizuoch.elderstar", "Forward+", "APV", "Soft Particles", "URP", "PCVR"):
            self.assertIn(token, en)
            self.assertIn(token, ja)

    def test_both_repository_guides_match_their_packaged_versions(self):
        for name in ("README.md", "README_JA.md"):
            text = (ROOT / name).read_text(encoding="utf-8")
            self.assertEqual(text.replace("Assets/Nova/Documentation~/", "Documentation~/")
                             .replace("Assets/Nova/LICENSE.md", "LICENSE.md"),
                             (PACKAGE / name).read_text(encoding="utf-8"))

    def test_all_local_guide_links_and_explicit_anchors_resolve(self):
        paths = [ROOT / "README.md", ROOT / "README_JA.md", PACKAGE / "README.md", PACKAGE / "README_JA.md"]
        paths += list((PACKAGE / "Documentation~").rglob("*.md"))
        for path in paths:
            text = path.read_text(encoding="utf-8")
            links = re.findall(r"\]\(([^)]+)\)", text) + re.findall(r'(?:src|href)="([^"]+)"', text)
            for link in links:
                if "://" in link:
                    continue
                target = (path.parent / link.split("#")[0]) if not link.startswith("#") else path
                self.assertTrue(target.exists(), f"{path}: {link}")
                if link.startswith("#") and path.name.startswith("README"):
                    self.assertIn('id="' + link[1:] + '"', text)


if __name__ == "__main__":
    unittest.main()
