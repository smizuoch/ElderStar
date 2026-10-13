using NUnit.Framework;
using UnityEngine;

public sealed class NovaBuiltInRandomizedMathTests
{
    [TestCase(1.4f, 0f, 0.825f)]
    [TestCase(-0.4f, 1f, 0.425f)]
    [TestCase(0.4f, 0f, 0.325f)]
    [TestCase(3f, 1f, 0.875f)]
    [TestCase(-3f, 0f, 0.125f)]
    public void RandomRowVolumeClampsAfterAddingCustomProgress(float progress,float row,float expected)
    {
        // Original Particles.hlsl clamps normalized frameIndex, after adding progress * framesPerRow.
        var actual=Probe(0,new Vector4(progress,4,2,row));
        Assert.That(actual.r,Is.EqualTo(expected).Within(0.001f));
    }

    [TestCase(0.8f)]
    [TestCase(-0.8f)]
    public void ParallaxOffsetPreservesSignedViewDenominator(float z)
    {
        var view=new Vector3(0.4f,0.2f,z).normalized;
        var expected=new Vector2(view.x,view.y)/(view.z+0.42f)*(-0.8f*0.06f);
        var actual=Probe(1,new Vector4(0.4f,0.2f,z,0));
        Assert.That(actual.r,Is.EqualTo(expected.x).Within(0.001f));
        Assert.That(actual.g,Is.EqualTo(expected.y).Within(0.001f));
    }

    private static Color Probe(float mode,Vector4 input)
    {
        var shader=Shader.Find("Hidden/NovaBuiltInTests/MathProbe");Assert.That(shader,Is.Not.Null);
        Assert.That(shader.isSupported,Is.True);
        var material=new Material(shader);var target=new RenderTexture(4,4,0,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
        var readback=new Texture2D(4,4,TextureFormat.RGBAFloat,false,true);var previous=RenderTexture.active;
        try
        {
            material.SetFloat("_Mode",mode);material.SetVector("_Input",input);
            Graphics.Blit(Texture2D.whiteTexture,target,material);
            RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,4,4),0,0);readback.Apply();
            return readback.GetPixel(2,2);
        }
        finally
        {
            RenderTexture.active=previous;Object.DestroyImmediate(material);Object.DestroyImmediate(readback);Object.DestroyImmediate(target);
        }
    }
}
