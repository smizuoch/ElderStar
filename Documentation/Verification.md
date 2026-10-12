# Verification

Run the graphics-enabled tests from WSL in the repository root:

```bash
./Scripts/run-all-unity-editmode-tests.sh
./Scripts/run-all-unity-playmode-tests.sh
```

The default editors are Unity 2022.3.22f1 and 6000.3.23f1. Both must pass.
Set `UNITY_SECONDARY_VERSION` to select another supported secondary version, or
use `UNITY_VERSION` with an individual runner:

```bash
UNITY_VERSION=6000.3.23f1 ./Scripts/run-editmode-tests.sh
UNITY_VERSION=2022.3.22f1 ./Scripts/run-playmode-tests.sh
```

For another installation path, set `UNITY_EDITOR` to its WSL path.
The runners require Bash, Python 3, rsync, wslpath and Windows interop. They create
separate verification projects under Windows TEMP for each Unity version, retain
their generated ProjectSettings, and exclude the reference checkout, Git and logs.
Do not edit those temporary projects while a test command is running.

Results and editor logs are written to `Logs/Unity/<version>/`. A runner succeeds
only when Unity succeeds and the newly written NUnit XML reports a nonempty passing
suite. TEMP lookup failures, copy failures, empty runs and secondary-editor failures
are errors. `UNITY_STRICT_SECONDARY=0` is an explicit local diagnostic opt-out;
it is not acceptable for release verification.

Run the shell regression checks with:

```bash
bash Tests/Shell/test-unity-env.sh
python3 Tests/Shell/test_unity_runners.py
python3 Tests/Docs/test_release_documentation.py
python3 Tests/Visual/test_compare_frames.py
```

The visual comparator requires Pillow. Follow [the capture instructions](../Tests/Visual/README.md)
to produce fresh original and Built-in frames; all 96 common-feature and 72 PBR
comparisons must pass. Run `python3 Tests/Parity/test_original_contract.py` for
the complete original ShaderLab and Inspector source/resource contracts.
The completed release evidence is in [VerificationReport.md](VerificationReport.md).

Never pass `-nographics` to the rendering tests. Do not use
`Shader.WarmupAllShaders()` as a substitute for shader diagnostics: tests use
synchronous `ShaderUtil.CompilePass` plus compiler messages, followed by actual
camera/particle/UI rendering checks. Golden images are not rewritten by these commands.

## Package and player fixtures

`Tests/PackageImport` is copied into Assets of an isolated Built-in test project.
It checks external/tarball package import, all six original shader names, compilation
of all 48 optimizer outputs, package-relative include paths, and native Package
Manager sample import with resolved materials and five particle systems. All three
tests must pass. Pass
`-novaRequireCache` when testing a tarball to require a Library/PackageCache path.

`Scripts/Unity/NovaPackageBootstrap.cs` can be copied to that project's Assets/Editor.
Use `-executeMethod NovaPackageBootstrap.Install -novaPackageSource <package-folder>`
to add the external package through Package Manager, or
`-executeMethod NovaPackageBootstrap.PackAndInstall -novaPackageSource <package-folder>
-novaPackageTarballs <archive-folder>` to pack and install a tarball. Do not use
`-quit` with these asynchronous operations; the bootstrap exits after the request completes.

`NovaPackagePlayerBuilder.Build` creates a standalone Windows verification player
under the isolated project's Build folder. Run that player with graphics enabled:

```text
NovaPackageSmoke.exe -batchmode -force-d3d11 -novaReport <absolute-report-path> -logFile <absolute-log-path>
```

The builder imports the packaged showcase through Package Manager and includes it
in the standalone build. The player verifies shader retention, four Uber color renders
and distortion behavior, then loads that showcase, simulates its five particle systems
and checks the rendered frame. It writes the frame as `<report-path>.ppm`, writes PASS
or FAIL and exits accordingly. It does not contact external services.

To watch the packaged showcase in the standalone player, run without `-batchmode`:

```text
NovaPackageSmoke.exe -novaShowcase -screen-fullscreen 0 -screen-width 960 -screen-height 640
```

This mode keeps the sample playing until the window is closed. Edit material and
camera controls in the imported Unity scene; the standalone showcase is a visual demo.
The fixtures are test infrastructure and are not included in the NOVA package.
