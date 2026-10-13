// Editor-only acceptance fixture. No component or runtime script is put on an avatar.
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public static class NovaVrcDistortionProbe
{
    const string Prefix = "NOVA Overlap ";
    static readonly string[] Modes = { "active", "zero", "fade", "dissolve", "depth fade" };

    public static void Prepare(GameObject avatar)
    {
        foreach (var child in avatar.transform.Cast<Transform>().Where(t => t.name.StartsWith(Prefix)).ToArray())
            Object.DestroyImmediate(child.gameObject);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        texture.SetPixels(Enumerable.Repeat(new Color(1, .5f, 0, 1), 4).ToArray()); texture.Apply();
        // Own the serialized texture; never put Unity's shared whiteTexture into
        // a generated bundle that the fixture later unloads with its assets.
        var transition = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        transition.SetPixels(Enumerable.Repeat(Color.white, 4).ToArray()); transition.Apply();
        foreach (var mode in Modes)
        {
            var material = new Material(Shader.Find("Nova/Particles/Distortion")) { name = Prefix + mode };
            material.SetTexture("_BaseMap", texture); material.SetFloat("_Cull", 0);
            material.SetFloat("_DistortionIntensity", mode == "zero" ? 0 : .6f);
            if (mode == "fade" || mode == "dissolve")
            {
                material.SetTexture("_AlphaTransitionMap", transition);
                material.SetFloat("_AlphaTransitionMode", mode == "fade" ? 1 : 2);
                material.SetFloat("_AlphaTransitionProgress", 1); material.SetFloat("_DissolveSharpness", 1);
            }
            if (mode == "depth fade")
            {
                material.SetFloat("_DepthFadeEnabled", 1); material.SetFloat("_DepthFadeNear", 20);
                material.SetFloat("_DepthFadeFar", 30);
            }
            Nova.Editor.Core.Scripts.ParticlesDistortionMaterialPostProcessor.SetupMaterialKeywords(material);
            material.renderQueue = mode == "active" ? 3100 : 3101;
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.name = Prefix + mode; quad.transform.SetParent(avatar.transform, false);
            quad.transform.localPosition = new Vector3(-.55f, 1.45f, .4f);
            quad.transform.localScale = Vector3.one * .15f;
            quad.GetComponent<Renderer>().sharedMaterial = material;
        }
    }

    public static void Verify(GameObject avatar, string output)
    {
        // Use the actual serialized bundle materials, including stripped shader variants.
        var materials = avatar.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials)
            .Where(m => m && m.name.StartsWith(Prefix)).Distinct().ToArray();
        Material Get(string mode) => materials.Single(m => m.name == Prefix + mode || m.name == Prefix + mode + " (Instance)");
        var cameraObject = new GameObject("Bundle overlap camera", typeof(Camera));
        var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var underlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var foreground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var background = new Material(avatar.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials)
            .First(m => m && m.shader.name == "Nova/Particles/UberUnlit"));
        var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        var target = new RenderTexture(128, 128, 24);
        var cpu = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        var previous = RenderTexture.active; var culling = GL.invertCulling;
        var active = Shader.GetGlobalFloat("_NovaCameraDistortionActive");
        var mirror = Shader.GetGlobalFloat("_VRChatMirrorMode");
        try
        {
            for (var y = 0; y < 64; y++) for (var x = 0; x < 64; x++)
                texture.SetPixel(x, y, new Color(x / 63f, y / 63f, ((x / 4 + y / 4) % 2), 1));
            texture.Apply();
            background.SetTexture("_BaseMap", texture); background.SetFloat("_BlendSrc", 1);
            background.SetFloat("_BlendDst", 0); background.SetFloat("_Cull", 0); background.renderQueue = 2000;
            background.SetColor("_BaseColor", Color.white);
            backdrop.GetComponent<Renderer>().sharedMaterial = background;
            underlay.GetComponent<Renderer>().sharedMaterial = Get("active");
            foreach (var item in new[] { backdrop, underlay, foreground }) item.layer = 31;
            backdrop.transform.position = new Vector3(0, 0, 1); backdrop.transform.localScale = Vector3.one * 2;
            underlay.transform.position = new Vector3(0, 0, .2f);
            var camera = cameraObject.GetComponent<Camera>(); camera.cullingMask = 1 << 31;
            camera.transform.position = new Vector3(0, 0, -2); camera.orthographic = true; camera.orthographicSize = 1;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            camera.allowHDR = false; camera.allowMSAA = false; camera.targetTexture = target;
            Shader.SetGlobalFloat("_NovaCameraDistortionActive", 0);
            Color32[] Render(string suffix)
            {
                camera.Render(); RenderTexture.active = target;
                cpu.ReadPixels(new Rect(0, 0, 128, 128), 0, 0); cpu.Apply();
                File.WriteAllBytes(output + suffix + ".png", cpu.EncodeToPNG()); return cpu.GetPixels32();
            }
            int Difference(Color32[] a, Color32[] b) => a.Zip(b, (x, y) =>
                Math.Max(Math.Abs(x.r - y.r), Math.Max(Math.Abs(x.g - y.g), Math.Abs(x.b - y.b)))).Count(x => x > 1);
            var view = camera.worldToCameraMatrix;
            foreach (var reflected in new[] { false, true })
            {
                camera.worldToCameraMatrix = reflected ? Matrix4x4.Scale(new Vector3(-1, 1, 1)) * view : view;
                GL.invertCulling = reflected ? !culling : culling;
                Shader.SetGlobalFloat("_VRChatMirrorMode", reflected ? 2 : 0);
                foreground.SetActive(false); underlay.SetActive(false);
                var empty = Render("-" + reflected + "-empty"); underlay.SetActive(true);
                var expected = Render("-" + reflected + "-underlay");
                if (Difference(empty, expected) <= 500) throw new Exception("SDK overlap probe has no visible distortion.");
                foreground.SetActive(true);
                foreach (var mode in Modes.Skip(1))
                {
                    foreground.GetComponent<Renderer>().sharedMaterial = Get(mode);
                    var actual = Render("-" + reflected + "-" + mode.Replace(" ", "-"));
                    if (Difference(expected, actual) != 0) throw new Exception("SDK overlap changed underlying distortion: " + mode + "; mirror=" + reflected);
                }
            }
        }
        finally
        {
            Shader.SetGlobalFloat("_NovaCameraDistortionActive", active); Shader.SetGlobalFloat("_VRChatMirrorMode", mirror);
            GL.invertCulling = culling; RenderTexture.active = previous;
            foreach (var item in new Object[] { cameraObject, backdrop, underlay, foreground, background, texture, target, cpu })
                Object.DestroyImmediate(item);
        }
    }
}
