import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]


class UnityRunnerExitTests(unittest.TestCase):
    def test_result_validation_rejects_empty_failed_missing_and_malformed_results(self):
        cases = [
            ('<test-run result="Passed" total="2" failed="0"/>', True),
            ('<test-run result="Failed" total="2" failed="1"/>', False),
            ('<test-run result="Passed" total="0" failed="0"/>', False),
            ('invalid xml', False),
            (None, False),
        ]
        for xml, success in cases:
            with self.subTest(xml=xml), tempfile.TemporaryDirectory(prefix="nova-result-test-") as folder:
                path = Path(folder) / "result.xml"
                if xml is not None:
                    path.write_text(xml)
                result = subprocess.run(
                    ["bash", "-c", 'source "$1"; nova_validate_test_results "$2"', "_",
                     str(ROOT / "Scripts/unity-env.sh"), str(path)], capture_output=True, text=True)
                self.assertEqual(result.returncode == 0, success, result.stdout + result.stderr)

    def test_secondary_failure_is_failure_by_default_for_both_test_categories(self):
        for category in ("editmode", "playmode"):
            with self.subTest(category=category), tempfile.TemporaryDirectory(prefix="nova-runner-test-") as folder:
                root = Path(folder)
                runner = root / f"run-all-unity-{category}-tests.sh"
                shutil.copyfile(ROOT / "Scripts" / runner.name, runner)
                child = root / f"run-{category}-tests.sh"
                child.write_text('#!/usr/bin/env bash\nprintf "%s\\n" "$UNITY_VERSION" >> "$NOVA_TEST_VERSIONS"\n'
                                 'case "$UNITY_VERSION" in 6000.*) exit 7;; *) exit 0;; esac\n')
                child.chmod(0o755)
                versions = root / "versions.txt"
                env = {k: v for k, v in os.environ.items() if k not in ("UNITY_STRICT_SECONDARY", "UNITY_SECONDARY_VERSION")}
                env["NOVA_TEST_VERSIONS"] = str(versions)
                result = subprocess.run(["bash", str(runner)], env=env, capture_output=True, text=True)
                self.assertEqual(result.returncode, 7, result.stdout + result.stderr)
                self.assertEqual(versions.read_text().splitlines(), ["2022.3.22f1", "6000.3.23f1"])


if __name__ == "__main__":
    unittest.main()
