# Ambient lighting

Version 0.1.6 restores the original NOVA glossy ambient contribution when
**Environment Reflections** is disabled. That setting disables reflection maps;
original NOVA still evaluates an ambient reflection through its BRDF. No material
conversion, new property, texture sample, pass or avatar component is required.

Diffuse lighting now uses the supplied SH/proxy irradiance without raising dark
probes to the scene's ambient color. Gamma rendering preserves the sRGB low-light
segment. The glossy fallback follows the original C# Color.gamma conversion,
including HDR values, and respects the scene reflection intensity.

The fallback recovers DC from Built-in's packed renderer SH. It removes the L2
term packed into the constant coefficient and avoids stale skybox RGB retained by
unity_IndirectSpecColor after switching ambient modes. Its alpha supplies the
reflection intensity on the tested Windows D3D path.

This remains a pipeline approximation when a renderer's local/custom probe differs
from the scene-wide ambient probe: URP supplies the latter separately for glossy
fallback, whereas the shader-only Built-in path uses the renderer's DC. Match those
probe inputs or enable reflection maps when the distinction matters. LPPV still
provides spatial diffuse lighting; this change does not implement APV data access.

The corrected nonblack-ambient comparison passes all 192 original cases. Additional
flat, colored, gradient and supplied-sky-probe comparisons cover both Lit shaders,
reflection intensity zero/one and three surface workflows on Unity 2022/6 in Gamma
and Linear. They do not guarantee every world, probe setup or graphics backend.

## Separate global glossy probe (0.1.7)

Add **ElderStar / Global Ambient Lighting** (`NovaAmbientLighting`) to each ordinary
Built-in camera that needs original NOVA's independent global glossy environment
color. It reads `RenderSettings.ambientProbe` DC and reflection intensity before
rendering and applies the original active-color-space conversion. Renderer SH and
LPPV remain responsible for diffuse light. There is no material edit, texture,
scene search, additional pass or screen copy. Nested cameras without this component
use the scriptless fallback; camera exit/disable/destroy restores external state.

This optional component is not allowed on a VRChat avatar. The avatar fallback
still reconstructs glossy DC from renderer SH, so a local/custom probe differing
from the global ambient probe remains a documented difference. A world providing
reflection maps can avoid the disabled-reflection fallback, but an avatar cannot
require that world input. Do not add this component to an avatar to claim parity.

The global `_NovaGlobalGlossyAmbient` value is RGB in active color space, with W=1
only while a bound camera renders (W=0 selects the fallback). External tools may
supply it explicitly, but must own/restore their scope. Shader properties and
material defaults are unchanged.
