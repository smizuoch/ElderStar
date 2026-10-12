using System;
using System.Collections.Generic;
using System.Linq;
using Nova.Editor.Core.Scripts;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInMaterialParityTests
    {
        private static readonly string[] Shaders = {
            "Nova/Particles/UberUnlit", "Nova/Particles/UberLit", "Nova/UIParticles/UberUnlit",
            "Nova/UIParticles/UberLit", "Nova/Particles/Distortion"
        };

        public static IEnumerable<TestCaseData> Cases()
        {
            foreach (var shader in Shaders)
            for (var seed = -1; seed < 48; seed++)
                yield return new TestCaseData(shader, seed);
        }

        [TestCaseSource(nameof(Cases))]
        public void MaterialSetupMatchesOriginalAcrossStateChanges(string shaderName, int seed)
        {
            var actual = new Material(Shader.Find(shaderName));
            var expected = new Material(actual.shader);
            var array = new Texture2DArray(2, 2, 2, TextureFormat.RGBA32, false);
            var volume = new Texture3D(2, 2, 2, TextureFormat.RGBA32, false);
            var texture = new Texture2D(2, 2);
            try
            {
                for (var step = 0; step < 3; step++)
                {
                    if (seed >= 0)
                    {
                        Configure(actual, seed + 101 * step, texture, array, volume);
                        Configure(expected, seed + 101 * step, texture, array, volume);
                    }
                    Setup(expected, true);
                    Setup(actual, false);
                    var context = shaderName + " seed=" + seed + " step=" + step;
                    // Built-in needs this scheduling keyword; URP supplies it externally.
                    string[] Keywords(Material m) => m.shaderKeywords.Where(k => k != "_ADDITIONAL_LIGHTS")
                        .OrderBy(k => k).ToArray();
                    Assert.That(Keywords(actual), Is.EqualTo(Keywords(expected)), context);
                    Assert.That(actual.renderQueue, Is.EqualTo(expected.renderQueue), context);
                    Assert.That(actual.GetTag("RenderType", false), Is.EqualTo(expected.GetTag("RenderType", false)), context);
                    Assert.That(actual.doubleSidedGI, Is.EqualTo(expected.doubleSidedGI), context);
                    Assert.That(actual.GetShaderPassEnabled("ShadowCaster"), Is.EqualTo(expected.GetShaderPassEnabled("ShadowCaster")), context);
                    for (var i = 0; i < actual.shader.GetPropertyCount(); i++)
                    {
                        var type = actual.shader.GetPropertyType(i);
                        if (type != ShaderPropertyType.Float && type != ShaderPropertyType.Range) continue;
                        var name = actual.shader.GetPropertyName(i);
                        Assert.That(actual.GetFloat(name), Is.EqualTo(expected.GetFloat(name)), context + " " + name);
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(actual); Object.DestroyImmediate(expected);
                Object.DestroyImmediate(texture); Object.DestroyImmediate(array); Object.DestroyImmediate(volume);
            }
        }

        private static void Configure(Material m, int seed, Texture2D texture, Texture2DArray array, Texture3D volume)
        {
            var random = new System.Random(seed);
            texture.filterMode = array.filterMode = volume.filterMode = (FilterMode)(seed % 3);
            for (var i = 0; i < m.shader.GetPropertyCount(); i++)
            {
                var name = m.shader.GetPropertyName(i);
                if (m.shader.GetPropertyType(i) == ShaderPropertyType.Texture)
                {
                    var dimension = m.shader.GetPropertyTextureDimension(i);
                    Texture value = dimension == TextureDimension.Tex2DArray ? array :
                        dimension == TextureDimension.Tex3D ? (Texture)volume : texture;
                    m.SetTexture(name, random.Next(2) == 0 ? null : value);
                }
                else if (name.EndsWith("Enabled") || name.EndsWith("MirrorSampling") ||
                         name == "_LitReceiveShadows" || name == "_EnvironmentReflections" || name == "_SpecularHighlights")
                    m.SetFloat(name, random.Next(3) * 0.5f);
            }
            Set(m, "_RenderType", random.Next(3)); Set(m, "_TransparentBlendMode", random.Next(3));
            Set(m, "_ZWriteOverride", random.Next(3) - 1); Set(m, "_Cull", random.Next(3));
            Set(m, "_QueueOffset", random.Next(-50, 51)); Set(m, "_VertexAlphaMode", random.Next(2));
            Set(m, "_BaseMapMode", random.Next(3)); Set(m, "_BaseMapChannel", random.Next(5));
            Set(m, "_BaseMapToneMode", random.Next(3)); Set(m, "_TintAreaMode", random.Next(3));
            Set(m, "_TintColorMode", random.Next(4)); Set(m, "_ColorCorrectionMode", random.Next(3));
            Set(m, "_FlowMapTarget", random.Next(shaderNameIsDistortion(m) ? 4 : 16));
            Set(m, "_ParallaxMapMode", random.Next(3)); Set(m, "_ParallaxMapTarget", random.Next(8));
            Set(m, "_AlphaTransitionMode", random.Next(3)); Set(m, "_AlphaTransitionMapMode", random.Next(3));
            Set(m, "_AlphaTransitionSecondTextureBlendMode", random.Next(3));
            Set(m, "_AlphaTransitionMapSecondTextureMapMode", random.Next(3));
            Set(m, "_EmissionAreaType", random.Next(4)); Set(m, "_EmissionColorType", random.Next(3));
            Set(m, "_EmissionMapMode", random.Next(3)); Set(m, "_LitWorkflowMode", random.Next(2));
        }

        private static bool shaderNameIsDistortion(Material material) => material.shader.name.EndsWith("/Distortion");
        private static void Set(Material m, string name, float value) { if (m.HasProperty(name)) m.SetFloat(name, value); }
        private static void Setup(Material m, bool original)
        {
            if (shaderNameIsDistortion(m))
            {
                if (original) Reference.ParticlesDistortionMaterialPostProcessor.SetupMaterialKeywords(m);
                else ParticlesDistortionMaterialPostProcessor.SetupMaterialKeywords(m);
            }
            else if (m.shader.name.EndsWith("UberLit"))
            {
                if (original)
                {
                    Reference.ParticlesUberLitMaterialPostProcessor.SetupMaterialKeywords(m);
                    Reference.ParticlesUberLitMaterialPostProcessor.SetupMaterialBlendMode(m);
                }
                else
                {
                    ParticlesUberLitMaterialPostProcessor.SetupMaterialKeywords(m);
                    ParticlesUberLitMaterialPostProcessor.SetupMaterialBlendMode(m);
                }
            }
            else if (original)
            {
                Reference.ParticlesUberUnlitMaterialPostProcessor.SetupMaterialKeywords(m);
                Reference.ParticlesUberUnlitMaterialPostProcessor.SetupMaterialBlendMode(m);
            }
            else
            {
                ParticlesUberUnlitMaterialPostProcessor.SetupMaterialKeywords(m);
                ParticlesUberUnlitMaterialPostProcessor.SetupMaterialBlendMode(m);
            }
        }
    }
}
