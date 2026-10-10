using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class NovaBuiltInDistortionCompileTests
{
    [Test]
    public void DistortionApplyShaderIsReferencedByPlayerResource()
    {
        var resource = Resources.Load<Material>("NovaBuiltIn/ApplyDistortion");
        Assert.That(resource, Is.Not.Null, "A serialized Resources reference must retain the hidden shader in player builds.");
        Assert.That(resource.shader.name, Is.EqualTo("Hidden/Nova/Particles/ApplyDistortion"));
    }

    [TestCase("")]
    [TestCase("_FLOW_MAP_TARGET_BASE")]
    [TestCase("_FLOW_MAP_TARGET_ALPHA_TRANSITION _DISSOLVE_TRANSITION_ENABLED")]
    [TestCase("_FADE_TRANSITION_ENABLED _SOFT_PARTICLES_ENABLED")]
    [TestCase("_BASE_SAMPLER_STATE_POINT_MIRROR _DEPTH_FADE_ENABLED")]
    [TestCase("_BASE_SAMPLER_STATE_TRILINEAR_MIRROR _FLOW_MAP_ENABLED _DISSOLVE_TRANSITION_ENABLED _SOFT_PARTICLES_ENABLED _DEPTH_FADE_ENABLED")]
    public void DistortionVariantsCompileSynchronously(string keywords)
    {
        var material = new Material(Shader.Find("Nova/Particles/Distortion"));
        try
        {
            foreach (var keyword in keywords.Split(' ').Where(k => k.Length > 0)) material.EnableKeyword(keyword);
            var pass = material.FindPass("DistortedUvBuffer");
            Assert.That(pass, Is.GreaterThanOrEqualTo(0));
            ShaderUtil.CompilePass(material, pass, true);
            Assert.That(ShaderUtil.IsPassCompiled(material, pass), Is.True);
            var messages = ShaderUtil.GetShaderMessages(material.shader).Where(m => m.severity.ToString() == "Error").Select(m => m.message);
            Assert.That(messages, Is.Empty);
        }
        finally { Object.DestroyImmediate(material); }
    }
}
