using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

public sealed class NovaImportedEffectPlayerSmoke : MonoBehaviour
{
    public GameObject Effect;
    public Camera View;

    private IEnumerator Start()
    {
        var args = Environment.GetCommandLineArgs();
        var path = args[Array.IndexOf(args, "-novaReport") + 1];
        var systems = Effect.GetComponentsInChildren<ParticleSystem>(true);
        for (var i = 0; i < systems.Length; i++)
        {
            systems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            systems[i].useAutoRandomSeed = false;
            systems[i].randomSeed = (uint)(12345 + i);
            systems[i].Simulate(3f, false, true, true);
            systems[i].Pause(false);
        }
        yield return null;
        var target = new RenderTexture(384, 384, 24);
        var image = new Texture2D(384, 384, TextureFormat.RGBA32, false);
        var previous = RenderTexture.active;
        var exit = 1;
        try
        {
            var renderers = Effect.GetComponentsInChildren<Renderer>(true).Where(r => r.enabled).ToArray();
            if (systems.Length != 6 || renderers.Length != 5 || renderers.Any(r => r.sharedMaterials.Any(m =>
                m == null || m.shader == null || m.shader.name != "Nova/Particles/UberUnlit" || !m.shader.isSupported)))
                throw new InvalidOperationException("Fire001 particle/material references failed in player.");
            View.targetTexture = target;
            View.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 384, 384), 0, 0);
            image.Apply();
            var visible = image.GetPixels32().Count(c => c.r > 80 && c.r > c.b * 1.1f);
            if (visible <= 100) throw new InvalidOperationException("Fire001 is not visible in player.");
            using (var stream = File.Create(path + ".ppm"))
            {
                var header = System.Text.Encoding.ASCII.GetBytes("P6\n384 384\n255\n");
                stream.Write(header, 0, header.Length);
                var pixels = image.GetPixels32();
                for (var y = 383; y >= 0; y--)
                for (var x = 0; x < 384; x++)
                {
                    var c = pixels[y * 384 + x]; stream.WriteByte(c.r); stream.WriteByte(c.g); stream.WriteByte(c.b);
                }
            }
            File.WriteAllText(path, "PASS\n" + Application.unityVersion + "\n" + SystemInfo.graphicsDeviceType +
                "\n6 particle systems; 5 enabled renderers; visible fire pixels=" + visible + "\n");
            exit = 0;
        }
        catch (Exception error) { File.WriteAllText(path, "FAIL\n" + error); }
        finally
        {
            View.targetTexture = null;
            RenderTexture.active = previous;
            Destroy(image);
            Destroy(target);
        }
        Application.Quit(exit);
    }
}
