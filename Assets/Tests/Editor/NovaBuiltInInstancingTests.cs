using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInInstancingTests
    {
        [TestCase("Nova/Particles/UberUnlit")]
        [TestCase("Nova/Particles/UberLit")]
        [TestCase("Nova/UIParticles/UberUnlit")]
        [TestCase("Nova/UIParticles/UberLit")]
        [TestCase("Nova/Particles/Distortion")]
        public void ProceduralParticleVariantsExistAndEveryPassCompiles(string name)
        {
            var shader = Shader.Find(name);
            Assert.That(ShaderUtil.HasProceduralInstancing(shader), Is.True, name);
            var material = new Material(shader) { enableInstancing = true };
            try
            {
                material.EnableKeyword("PROCEDURAL_INSTANCING_ON");
                material.EnableKeyword("_BASE_MAP_MODE_2D");
                material.EnableKeyword("_VERTEX_DEFORMATION_ENABLED");
                material.EnableKeyword("_ALPHATEST_ENABLED");
                for (var pass = 0; pass < material.passCount; pass++) ShaderUtil.CompilePass(material, pass, true);
                Assert.That(ShaderUtil.GetShaderMessages(shader).Where(m => m.severity.ToString() == "Error")
                    .Select(m => m.message), Is.Empty, name);
            }
            finally { Object.DestroyImmediate(material); }
        }
    }
}
