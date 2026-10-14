using System.Collections.Generic;
using UnityEngine;

namespace Nova.Runtime.UI
{
    /// <summary>Opt-in lighting for a native Canvas. Not a VRChat avatar component.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(Canvas))]
    [AddComponentMenu("NOVA/Canvas Lighting")]
    public sealed class NovaCanvasLighting : MonoBehaviour
    {
        [SerializeField, Range(0, 8)] private int maximumLights = 4;
        [SerializeField] private List<Light> lights = new List<Light>();
        private readonly List<NovaCanvasLightMaterial> modifiers = new List<NovaCanvasLightMaterial>();
        private readonly Light[] selected = new Light[8];
        private readonly float[] scores = new float[8];
        private readonly Vector4[] positions = new Vector4[8], colors = new Vector4[8], directions = new Vector4[8], attenuation = new Vector4[8];
        private Canvas target;
        public int MaximumLights { get => maximumLights; set => maximumLights = Mathf.Clamp(value, 0, 8); }
        /// <summary>Largest number of eligible lights omitted for any Graphic in the last update.</summary>
        public int OmittedLightCount { get; private set; }

        public void RegisterLight(Light light) { if (light != null && !lights.Contains(light)) lights.Add(light); }
        public void UnregisterLight(Light light) { lights.Remove(light); }

        [ContextMenu("Collect Scene Lights")]
        public void CollectSceneLights()
        {
            lights.Clear();
            foreach (var light in FindObjectsOfType<Light>(true))
                if (light.gameObject.scene == gameObject.scene) lights.Add(light);
        }

        private void Reset() { CollectSceneLights(); }
        private void OnEnable()
        {
            target = GetComponent<Canvas>();
            target.additionalShaderChannels |= AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
            RefreshGraphics();
            Canvas.preWillRenderCanvases += RefreshLighting;
        }
        private void OnValidate() { maximumLights = Mathf.Clamp(maximumLights, 0, 8); }
        private void RefreshLighting()
        {
            RefreshGraphics();
            OmittedLightCount = 0;
            foreach (var modifier in modifiers) if (modifier != null) modifier.RefreshLighting();
        }

        private void RefreshGraphics()
        {
            if (target == null) return;
            // uGUI maintains this registry; no scene or hierarchy search per frame.
            var graphics = UnityEngine.UI.GraphicRegistry.GetGraphicsForCanvas(target);
            for (int i = 0; i < graphics.Count; i++)
            {
                var graphic = graphics[i];
                if (graphic == null || graphic.material == null || graphic.material.shader.name != "Nova/UIParticles/UberLit") continue;
                var modifier = graphic.GetComponent<NovaCanvasLightMaterial>();
                if (modifier == null)
                {
                    modifier = graphic.gameObject.AddComponent<NovaCanvasLightMaterial>();
                    modifier.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
                }
                if (modifier.Owner != this)
                {
                    modifier.SetOwner(this);
                    if (!modifiers.Contains(modifier)) modifiers.Add(modifier);
                }
            }
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                var modifier = modifiers[i];
                if (modifier == null) { modifiers.RemoveAt(i); continue; }
                if (modifier.Owner != this) { modifiers.RemoveAt(i); continue; }
                if (modifier.Graphic.canvas != target) { modifier.SetOwner(null); modifiers.RemoveAt(i); Release(modifier); }
            }
        }

        internal void Apply(Material material, UnityEngine.UI.Graphic graphic)
        {
            var rect = graphic.rectTransform;
            var center = rect.TransformPoint(rect.rect.center);
            var radius = rect.TransformVector(new Vector3(rect.rect.width, rect.rect.height, 0) * .5f).magnitude;
            int count = 0, eligible = 0, limit = Mathf.Clamp(maximumLights, 0, 8);
            for (int i = 0; i < lights.Count; i++)
            {
                var light = lights[i];
                if (light == null || !light.isActiveAndEnabled || light.intensity <= 0 || (light.cullingMask & (1 << graphic.gameObject.layer)) == 0) continue;
                if (light.type != LightType.Directional && light.type != LightType.Point && light.type != LightType.Spot) continue;
                float distance = Vector3.Distance(center, light.transform.position);
                bool directional = light.type == LightType.Directional;
                if (!directional && (light.range <= 0 || distance > light.range + radius)) continue;
                float score = light.intensity * light.color.maxColorComponent;
                if (!directional)
                {
                    float nearest = Mathf.Max(0, distance - radius);
                    float distanceSquared = Mathf.Max(.0001f, nearest * nearest);
                    float factor = distanceSquared / (light.range * light.range);
                    float smoothing = Mathf.Clamp01(1 - factor * factor);
                    score *= smoothing * smoothing / distanceSquared;
                    if (light.type == LightType.Spot && distance > radius)
                    {
                        // Bound the whole Graphic, not only its center: a cone may illuminate an edge.
                        float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(light.transform.forward,
                            (center - light.transform.position) / distance), -1, 1));
                        float nearestAngle = Mathf.Max(0, angle - Mathf.Asin(Mathf.Clamp01(radius / distance)));
                        float outer = Mathf.Cos(light.spotAngle * Mathf.Deg2Rad * .5f);
                        float inner = Mathf.Cos(light.innerSpotAngle * Mathf.Deg2Rad * .5f);
                        float spot = Mathf.Clamp01((Mathf.Cos(nearestAngle) - outer) / Mathf.Max(.001f, inner - outer));
                        score *= spot * spot;
                    }
                }
                if (score <= 0) continue;
                eligible++;
                int slot = count;
                while (slot > 0 && score > scores[slot - 1]) slot--;
                if (slot >= limit) continue;
                for (int j = Mathf.Min(count, limit - 1); j > slot; j--) { selected[j] = selected[j - 1]; scores[j] = scores[j - 1]; }
                selected[slot] = light; scores[slot] = score; count = Mathf.Min(count + 1, limit);
            }
            OmittedLightCount = Mathf.Max(OmittedLightCount, eligible - count);
            for (int i = 0; i < count; i++)
            {
                var light = selected[i]; var direction = -light.transform.forward;
                bool directional = light.type == LightType.Directional;
                var position = directional ? direction : light.transform.position;
                positions[i] = new Vector4(position.x, position.y, position.z, directional ? 0 : 1);
                var color = light.color;
                if (QualitySettings.activeColorSpace == ColorSpace.Linear) color = color.linear;
                colors[i] = color * light.intensity;
                directions[i] = direction;
                float inverseRange = directional ? 0 : 1 / Mathf.Max(.0001f, light.range * light.range);
                float scale = 0, bias = 1;
                if (light.type == LightType.Spot)
                {
                    float outer = Mathf.Cos(light.spotAngle * Mathf.Deg2Rad * .5f);
                    float inner = Mathf.Cos(light.innerSpotAngle * Mathf.Deg2Rad * .5f);
                    scale = 1 / Mathf.Max(.001f, inner - outer); bias = -outer * scale;
                }
                attenuation[i] = new Vector4(inverseRange, scale, bias, 0);
            }
            material.EnableKeyword("NOVA_CANVAS_LIGHTING");
            // World-space Canvas can schedule ForwardAdd. This material's base
            // pass already owns the complete selected direct-light list.
            material.SetShaderPassEnabled("ForwardAdd", false);
            material.SetInt("_NovaCanvasLightCount", count);
            material.SetVectorArray("_NovaCanvasLightPositions", positions);
            material.SetVectorArray("_NovaCanvasLightColors", colors);
            material.SetVectorArray("_NovaCanvasLightDirections", directions);
            material.SetVectorArray("_NovaCanvasLightAttenuation", attenuation);
        }

        private void OnDisable()
        {
            Canvas.preWillRenderCanvases -= RefreshLighting;
            foreach (var modifier in modifiers)
                if (modifier != null && modifier.Owner == this) { modifier.SetOwner(null); Release(modifier); }
            modifiers.Clear(); OmittedLightCount = 0;
        }
        internal static void Release(Object value)
        {
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }
    }
}
