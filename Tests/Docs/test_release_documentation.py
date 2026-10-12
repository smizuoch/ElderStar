"""Check that the release matrix resolves the original feature inventory explicitly."""
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]
REQUIRED = set("""Package import;Shader names;Material properties;Custom editors;Material setup;
Render state;Vertex color;Vertex alpha progress;Alpha clipping;Base texture modes;
UV transforms and custom coordinates;Random row selection;Mirror samplers;Channel extraction;
Tritone and pentone;Tint color and maps;Tint rim;Flow targets;Parallax targets;
Greyscale and gradient;Alpha transition;Second alpha transition;Emission;
Rim and luminance transparency;Soft particles;Depth fade;Vertex deformation;
Billboard particles;Mesh particle instancing;Instanced stereo;Scene selection and picking;
Shadow caster;DepthOnly;DepthNormals;Fog;Metallic and specular workflows;
Normal maps;Surface maps;Main light;Additional lights;Receive shadows;
Ambient and light probes;Environment reflections;Lightmaps and vertex lighting;
Forward+;Probe volumes;Foveated rendering;Debug display;UI stencil and ColorMask;
UI rect clipping;UI lighting;Camera distortion;Distortion buffer;Distortion material controls;
Distortion depth and visibility;Distortion cleanup;Distortion camera types;RenderGraph;
URP compatibility mode;Optimizer enum and names;Optimizer pass stripping;
Optimizer includes;Material replacement;Material migration;Samples;Platform scope""".replace("\n", "").split(";"))


class ReleaseDocumentationTests(unittest.TestCase):
    def test_package_relative_markdown_links_resolve(self):
        package = ROOT / "Assets/Nova"
        for path in [package / "README.md", *(package / "Documentation~").glob("*.md")]:
            for target in re.findall(r"\]\(([^)]+)\)", path.read_text(encoding="utf-8")):
                if "://" not in target and not target.startswith("#"):
                    self.assertTrue((path.parent / target.split("#")[0]).exists(), f"{path.name}: {target}")

    def test_every_original_feature_has_one_final_status_and_evidence(self):
        text = (ROOT / "Documentation/BuiltInCompatibilityMatrix.md").read_text(encoding="utf-8")
        self.assertFalse("Pending" in text, "The current matrix must resolve every feature status.")
        rows = {}
        for line in text.splitlines():
            if not line.startswith("| "):
                continue
            cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
            if cells[0] in {"Feature", "---"}:
                continue
            self.assertEqual(len(cells), 4, line)
            feature, status, evidence, fallback = cells
            self.assertNotIn(feature, rows, feature)
            self.assertIn(status, {"Exact", "Approximate", "Unsupported", "Not Applicable"}, feature)
            self.assertTrue(evidence, feature)
            if status in {"Approximate", "Unsupported"}:
                self.assertGreater(len(fallback), 35, feature)
            rows[feature] = status
        self.assertEqual(set(rows), REQUIRED)

    def test_package_documentation_matches_repository(self):
        for source, shipped in [("BuiltInCompatibilityMatrix.md", "Compatibility.md"), ("Migration.md", "Migration.md")]:
            expected = (ROOT / "Documentation" / source).read_bytes()
            if source == "Migration.md":
                expected = expected.replace(b"BuiltInCompatibilityMatrix.md", b"Compatibility.md")
            self.assertEqual(expected,
                             (ROOT / "Assets/Nova/Documentation~" / shipped).read_bytes())

    def test_historical_claims_are_kept_separate(self):
        history = (ROOT / "Documentation/BuiltInVerificationHistory.md").read_text(encoding="utf-8")
        self.assertIn("Phase 4.5B", history)
        self.assertIn("Phase 7.3A", history)
        for path in (ROOT / "Assets/Tests/Editor").glob("*.cs"):
            if "MatrixPath" in path.read_text(encoding="utf-8"):
                self.assertNotIn('"Documentation/BuiltInCompatibilityMatrix.md"', path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
