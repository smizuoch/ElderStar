# AGENTS.md

## Project goal

Create NovaShaderBuilt-in, a Built-in Render Pipeline port of CyberAgentGameEntertainment/NovaShader.

The goal is the most complete functional port possible. When exact behavior is impossible because the original implementation depends on URP, ScriptableRendererFeature, ScriptableRenderPass, RenderGraph, or UniversalFragmentPBR, implement the closest practical Built-in approximation and document the difference in `Documentation/BuiltInCompatibilityMatrix.md`.

## Hard rules

- Use TDD in the t-wada style:
  - Maintain a TODO list.
  - Write or update a failing test before each meaningful implementation.
  - Make the smallest change to pass.
  - Refactor only while tests are green.
- Do not remove material properties, shader keywords, or public editor APIs unless the porting spec explicitly says so.
- Preserve original NOVA shader names; do not rename Built-in shaders to a `NovaBuiltIn/...` namespace.
- Preserve original NOVA material property names whenever possible.
- Preserve original NOVA custom editor class names and shader `CustomEditor` declarations.
- Do not silently change golden images.
- Do not update visual baselines unless explicitly instructed.
- Use small commits, and commit only while the targeted tests are green.
- Do not push, merge, rebase, delete branches, or rewrite history without explicit user approval.
- Do not introduce a runtime dependency on `com.unity.render-pipelines.universal`, URP assemblies, or RenderGraph APIs in the Built-in package.
- Do not treat a compile-only shader as complete. Compile success is only T0/T1.

## Required workflow

At the start of every task:

1. Run `git status --short`.
2. Read:
   - `Documentation/BuiltInPortingSpec.md`
   - `Documentation/BuiltInCompatibilityMatrix.md`
   - relevant original NOVA files
3. State the current TODO item.
4. Add or update a test first.
5. Implement the smallest change.
6. Run the relevant test command.
7. Review the diff.
8. Commit only if the targeted tests pass.

At the end of every task, report:

- changed files
- tests run
- pass/fail result
- known remaining risk
- next TODO item

## Git rules

- Work on feature branches named `port/builtin-*`.
- Use small commits.
- Commit messages must be Japanese.
- PR titles should be English.
- Never hide unrelated changes in a commit.

## Testing rules

Use these categories:

- EditMode tests for editor APIs, material properties, shader discovery, migration utilities.
- PlayMode tests for rendering behavior.
- Visual regression tests for final behavior.
- Shader compile diagnostics must be checked before claiming completion.

Do not use `-nographics` for visual PlayMode tests.

## Built-in porting priorities

1. Package skeleton and no-URP compile.
2. Built-in shader compatibility layer.
3. Particles UberUnlit.
4. UI Particles UberUnlit.
5. Particles UberLit.
6. UI Particles UberLit.
7. Distortion.
8. Optimized Shader Generator and Replacer.
9. Samples and migration utilities.
10. Final visual regression.

## Completion definition

NovaShaderBuilt-in is complete only when:

- Built-in package imports without URP.
- All target shaders are found by `Shader.Find`.
- All public material properties required by the original shaders exist.
- EditMode tests pass.
- PlayMode tests pass.
- Visual tests pass within documented thresholds.
- Compatibility matrix marks every original feature as Exact, Approximate, Unsupported, or Not Applicable.
- Every Approximate or Unsupported item has a documented reason and fallback.
