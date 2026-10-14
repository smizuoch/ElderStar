# Low-cost lighting parity — implementation record

The approved 2026-10-14 worklist is in BuiltInPortingSpec.md. This is an ongoing
implementation record, not a claim of complete URP/API compatibility or release acceptance.

## Interpolation change

- Original `fragUnlit(in out Varyings)` normalizes its interpolated normal before
  `fragLit` uses it. The tangent and binormal remain unnormalized. Normalize only
  the final mapped normal after combining that basis.
- Normal-map binormal uses tangent.w; parallax additionally uses odd negative scale.
  Preserve both contracts. Parallax view direction is calculated per vertex, as in
  the original, retaining the existing signed 1e-4 denominator guard.
- Fog uses the original clip-Z vertex coefficient, including the EXP coefficient,
  then interpolates. The original can produce no fog on the tested orthographic,
  near-clip and oblique projections. ForwardAdd still fades toward black.
- Pack map UV/progress carriers to retain the basis/view direction within the
  SM3.5 stereo/shadowed ForwardAdd budget. UI clip coordinates remain explicit;
  they are not reconstructed with a potentially different particle instance matrix.
- No added color pass, texture sample or screen copy. GPU adoption measurement is
  still pending; operation counts are not a measured performance result.

### Reproduction and results

`Tests/Visual/NovaInterpolationCapture.cs` runs against original NOVA 3.6.0 and the
port. Define `NOVA_VISUAL_REFERENCE` only in the URP project. Run the graphics
PlayMode test `CaptureInterpolation`, passing `-novaVisualOutput <directory>` and
`-novaBlackAmbient`. Keep D3D graphics enabled. Compare with:

```text
python Tests/Visual/compare_interpolation.py <original> <port> --output <report.json>
```

192 images = four Uber shaders × eight feature groups × six projections/scales.
The mesh is coarse, nonplanar, has varying normals/tangents and nonuniform scale.
Features include control, rim, parallax, normal maps, Linear/Exp/Exp2 fog and their
combination. Views include perspective, orthographic, reflected, negative scale,
near-plane crossing and an oblique near plane.

Unity 2022.3.22f1 / D3D11 / Gamma: baseline c38b1f1 fails **56/192** comparisons;
candidate passes **192/192**, using unchanged thresholds and no golden update.
The isolated interpolation commit passes **450 EditMode + 866 PlayMode** tests.
The shader budget test also enables UI clipping, map arrays, parallax, normal map,
stereo, screen shadows and a directional cookie in ForwardAdd.

Raw metrics: `Verification/LightingParity20261014/interpolation2022.json`.
The new fixture originally mismatched Gamma light-intensity settings; both sides
now use URP's `lightsUseLinearIntensity = (activeColorSpace == Linear)` behavior.
The recorded baseline/candidate numbers above were recaptured with that correction.
Flat ambient differences were separately observed and remain under investigation;
black ambient isolates this interpolation change. No independent rim mismatch was
observed in this sample. These captures are not an all-input guarantee.

## Remaining acceptance

Unity 6 and Linear comparisons, optional Canvas lighting, dedicated DepthNormals
camera output, spatial LPPV, Editor debug views, executable distortion APIs,
SDK/AAO bundles and measured performance are still being implemented/verified.
Main merge/push follows the approved final acceptance gates.
