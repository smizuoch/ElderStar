# Bilingual ElderStar guides — 2026-10-16

`README.md` is now the complete English guide; `README_JA.md` retains the Japanese guide.
Both repository and package copies provide the same Docs / Samples / Demo navigation with six language destinations.
The main guides retain matching section anchors, property rows and images. English descriptions are adapted from the original MIT-licensed NOVA documentation, with ElderStar installation, Built-in depth/lighting, VRChat limitations and optimizer API corrections.

Samples documents the original effect assets shown in ElderStar captures. Demo documents the bundled Built-in Showcase, including both distribution installation paths.
Editor API and image credits are also bilingual. The new Showcase image is an unchanged copy of the existing distribution verification capture; its provenance and hash are recorded in the image manifest.

## Verification

| Check | Result |
| --- | --- |
| `python3 -m unittest discover -s Tests/Docs -v` | 12 passed: language navigation, equivalent main-guide coverage, local links/anchors, media hashes and existing documentation checks. |
| `python3 -m unittest discover -s Tests/Distribution -v` | 5 passed: both package staging layouts preserve all six navigation destinations; UPM preserves package bytes and the Assets layout preserves runtime/editor code and GUIDs. |
| `python3 -m unittest discover -s Tests/Parity -v` | 24 passed: original material/editor contracts and existing evidence checks. |
| Unity 2022.3.22f1 import | New Japanese README imported in the isolated Export2022 fixture; Unity-generated meta retained. Exit 0, no C# compiler or shader error messages in this import log. |
| Rendered documentation review | Local CommonMark/table preview: English and Japanese Docs, Samples and Demo navigation exercised; main introductions, demo capture, expandable contents and English property table visually checked. |
| `git diff --check` | Passed. |

The bilingual checks initially failed on missing guides/navigation; the distribution check initially failed on the missing Japanese README metadata. All targeted tests passed after adding the guides and allowing the two new package-root files in release staging.

## Scope and remaining work

No runtime, Editor implementation, material, shader, sample scene or golden image changed.
This documentation change does not rerun rendering, SDK/AAO, GPU timing or PCVR acceptance; the existing compatibility limits remain explicit in both languages.
The browser preview is a local approximation of GitHub Markdown styling, not a pixel-level GitHub rendering test.

Published v0.1.8 archives and its tag remain unchanged. Git-based installs use the new documentation after the main push; a future package release should bundle this documentation revision.
No implementation TODO remains for the bilingual-guide request.
