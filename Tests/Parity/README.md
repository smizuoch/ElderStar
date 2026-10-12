# Original authoring contract

Reference: NOVA 3.6.0 commit `1c6b88729f91c852c164ef71a32cc2bce83a1892`.

`original-3.6-contract.json` records all 889 ShaderLab property declarations,
CustomEditor declarations, 14 Inspector/utility source token digests, and the two
foldout icons with their original import settings. Whitespace inside UI strings
is preserved. Run `python3 Tests/Parity/test_original_contract.py` to check them.

`Assets/Tests/Editor/Reference/EditorContract.json` was exported by
`NovaEditorContract.Export` in an isolated original-NOVA Unity 6 project. It records
525 public/protected declarations, including signatures, constants and enum values.
The authoring EditMode tests require the port to preserve this entire contract.

The three reference material postprocessors under that directory retain original
code, with only their namespace changed to `Nova.Tests.Reference`, an import of
the preserved editor enums/constants, and whitespace normalized. Their internal
SetKeyword helper is copied separately. They are test-only, MIT-licensed reference
implementations; they do not ship in the Built-in package or reference URP.
The differential suite applies 48 deterministic configurations to each of five
shaders, with three consecutive state changes per configuration, comparing keywords,
render tags/queue, GI flags, pass state and all numeric material values.
Only `_ADDITIONAL_LIGHTS` is excluded: Built-in sets its scheduling variant explicitly
while URP supplies that pipeline keyword externally. Other original state transitions,
including legacy second-map migration and dormant shadow-alpha state, must match.

These are source/API reference contracts, not visual golden images. Never regenerate
them from the port to conceal a discrepancy. New additions to the port's public API
are allowed; removal or alteration of the original contract fails verification.
