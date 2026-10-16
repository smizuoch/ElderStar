using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nova.Runtime.Core.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Nova.Editor.Core.Scripts
{
    public static class BuiltInShowcase
    {
        [MenuItem("Tools/ElderStar/Built-in/Create Showcase Scene")]
        public static void CreateFromMenu()
        {
            // Unity's additive scene API requires untitled scenes to have a path first.
            if (!EditorSceneManager.EnsureUntitledSceneHasBeenSaved("Save the untitled scene before creating a NOVA sample.")) return;
            var path = EditorUtility.SaveFilePanelInProject("Create ElderStar showcase", "NovaShowcase", "unity", "Choose a new scene path.");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                Create(path);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            catch (Exception error) { Debug.LogException(error); }
        }

        /// <summary>Create a self-contained playable sample without replacing the user's active scene.</summary>
        public static void Create(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath)) throw new ArgumentException("Choose a new .unity path under Assets.");
            scenePath = scenePath.Replace('\\', '/');
            var parent = Path.GetDirectoryName(scenePath)?.Replace('\\', '/');
            var assets = parent + "/" + Path.GetFileNameWithoutExtension(scenePath) + " Assets";
            if (!scenePath.StartsWith("Assets/", StringComparison.Ordinal) || !scenePath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
                || scenePath.Split('/').Any(part => part == ".." || part == ".") || !AssetDatabase.IsValidFolder(parent)
                || File.Exists(scenePath) || Directory.Exists(assets))
                throw new ArgumentException("Choose an unused scene name in an existing folder under Assets.", nameof(scenePath));
            if (GraphicsSettings.currentRenderPipeline != null)
                throw new InvalidOperationException("Select Built-in Rendering before creating this sample.");
            for (var index = 0; index < SceneManager.sceneCount; index++)
                if (string.IsNullOrEmpty(SceneManager.GetSceneAt(index).path))
                    throw new InvalidOperationException("Save untitled scenes before creating a NOVA sample. Existing scene contents are preserved.");
            var previousScene = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assets));
            try
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.45f);
                RenderSettings.fog = false;
                var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
                camera.tag = "MainCamera";
                camera.transform.position = new Vector3(0, 0, -10);
                camera.orthographic = true;
                camera.orthographicSize = 3.4f;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 30;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.025f, 0.035f, 0.06f);
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.depthTextureMode = DepthTextureMode.Depth;
                camera.gameObject.AddComponent<ScreenSpaceDistortion>();
                var light = new GameObject("Key light", typeof(Light)).GetComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.transform.rotation = Quaternion.Euler(20, -20, 0);
                light.renderMode = LightRenderMode.ForcePixel;

                var radial = Texture(assets, "Soft particle", (x, y) =>
                {
                    var distance = new Vector2(x * 2 - 1, y * 2 - 1).magnitude;
                    return new Color(1, 1, 1, Mathf.SmoothStep(1, 0, Mathf.Clamp01(distance)));
                });
                var names = new[] { "Nova/Particles/UberUnlit", "Nova/Particles/UberLit", "Nova/UIParticles/UberUnlit", "Nova/UIParticles/UberLit" };
                var colors = new[] { new Color(0.1f, 0.85f, 1), new Color(1, 0.48f, 0.18f), new Color(0.65f, 0.45f, 1), new Color(0.2f, 1, 0.6f) };
                for (var index = 0; index < names.Length; index++)
                {
                    var position = new Vector3(index % 2 == 0 ? -2.1f : 2.1f, index < 2 ? 1.5f : -0.35f, 0);
                    var material = UberMaterial(assets, "Particles " + index, names[index], colors[index], radial, true);
                    Particles(names[index], position, material, (uint)(420 + index), 0.5f);
                    Label((index + 1).ToString("00") + "  " + names[index].Replace("Nova/", ""),
                        position + new Vector3(-1.4f, -0.8f, -0.1f), 0.105f, colors[index]);
                }
                Label("NOVA / BUILT-IN", new Vector3(-3.55f, 2.85f, 0), 0.22f, Color.white);
                Label("Four particle shaders + camera distortion", new Vector3(-3.55f, 2.47f, 0), 0.09f, new Color(0.6f, 0.67f, 0.8f));
                var grid = Texture(assets, "Distortion grid", (x, y) =>
                {
                    var line = Mathf.Repeat(x * 16, 1) < 0.14f || Mathf.Repeat(y * 8, 1) < 0.14f;
                    return line ? new Color(0.18f, 0.65f, 0.8f) : new Color(0.06f, 0.13f, 0.2f);
                });
                var background = UberMaterial(assets, "Grid", names[0], Color.white, grid, false);
                Quad("Distortion background", new Vector3(0, -2.15f, 1), new Vector3(7.2f, 0.85f, 1), background);
                var distortionTexture = Texture(assets, "Radial offset", (x, y) =>
                {
                    var delta = new Vector2(x * 2 - 1, y * 2 - 1);
                    var amount = Mathf.Clamp01(1 - delta.magnitude) * 0.48f;
                    return new Color(0.5f + delta.x * amount, 0.5f + delta.y * amount, 0, 1);
                }, true);
                var distortion = new Material(RequireShader("Nova/Particles/Distortion"));
                distortion.SetFloat("_Cull", 0);
                distortion.SetTexture("_BaseMap", distortionTexture);
                distortion.SetFloat("_DistortionIntensity", 0.4f);
                ParticlesDistortionMaterialPostProcessor.SetupMaterialKeywords(distortion);
                AssetDatabase.CreateAsset(distortion, assets + "/Distortion.mat");
                var lens = Particles("Moving distortion", new Vector3(0, -2.15f, 0), distortion, 428, 2.5f);
                var lensMain = lens.main;
                lensMain.startSize = 0.65f;
                var lensShape = lens.shape;
                lensShape.scale = new Vector3(1, 0.02f, 1);
                lensMain.startSpeed = 0.05f;
                var lensEmission = lens.emission;
                lensEmission.rateOverTime = 4;
                Label("05  DISTORTION   /   toggle the camera component to compare", new Vector3(-3.55f, -2.75f, 0), 0.095f, new Color(0.6f, 0.67f, 0.8f));
                Label("UI variants shown on particle meshes. Use Canvas Mask / RectMask2D in your UI scene.", new Vector3(-3.55f, -3.07f, 0), 0.07f, new Color(0.45f, 0.52f, 0.65f));
                AssetDatabase.SaveAssets();
                if (!EditorSceneManager.SaveScene(scene, scenePath)) throw new IOException("Could not save sample scene.");
            }
            catch
            {
                AssetDatabase.DeleteAsset(scenePath);
                AssetDatabase.DeleteAsset(assets);
                throw;
            }
            finally
            {
                if (previousScene.IsValid()) SceneManager.SetActiveScene(previousScene);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Shader RequireShader(string name)
        {
            var shader = Shader.Find(name);
            if (shader == null) throw new InvalidOperationException("Missing NOVA shader: " + name);
            return shader;
        }

        private static Material UberMaterial(string folder, string name, string shader, Color color, Texture texture, bool transparent)
        {
            var material = new Material(RequireShader(shader)) { name = name };
            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_RenderType", transparent ? 2 : 0);
            material.SetFloat("_Cull", 0);
            material.SetFloat("_TintAreaMode", 1);
            material.SetColor("_TintColor", color);
            if (shader.EndsWith("/UberLit", StringComparison.Ordinal))
            {
                material.SetFloat("_LitWorkflowMode", 1);
                material.SetFloat("_Metallic", 0.1f);
                material.SetFloat("_Smoothness", 0.6f);
                material.SetFloat("_SpecularHighlights", 1);
                material.SetFloat("_EnvironmentReflections", 0);
                ParticlesUberLitMaterialPostProcessor.SetupMaterialKeywords(material);
            }
            else ParticlesUberUnlitMaterialPostProcessor.SetupMaterialKeywords(material);
            ParticlesUberUnlitMaterialPostProcessor.SetupMaterialBlendMode(material);
            AssetDatabase.CreateAsset(material, folder + "/" + name + ".mat");
            return material;
        }

        private static Texture2D Texture(string folder, string name, Func<float, float, Color> pixel, bool linear = false)
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false, linear) { name = name, wrapMode = TextureWrapMode.Clamp };
            for (var y = 0; y < 64; y++)
            for (var x = 0; x < 64; x++) texture.SetPixel(x, y, pixel(x / 63f, y / 63f));
            texture.Apply();
            AssetDatabase.CreateAsset(texture, folder + "/" + name + ".asset");
            return texture;
        }

        private static ParticleSystem Particles(string name, Vector3 position, Material material, uint seed, float radius)
        {
            var system = new GameObject(name, typeof(ParticleSystem)).GetComponent<ParticleSystem>();
            system.transform.position = position;
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.useAutoRandomSeed = false;
            system.randomSeed = seed;
            var main = system.main;
            main.duration = 4;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = 2;
            main.startSpeed = 0.15f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.playOnAwake = true;
            var emission = system.emission;
            emission.rateOverTime = 24;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            var colorOverLife = system.colorOverLifetime;
            colorOverLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.15f), new GradientAlphaKey(1, 0.6f), new GradientAlphaKey(0, 1) });
            colorOverLife.color = gradient;
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.SetActiveVertexStreams(new List<ParticleSystemVertexStream> { ParticleSystemVertexStream.Position,
                ParticleSystemVertexStream.Normal, ParticleSystemVertexStream.Color, ParticleSystemVertexStream.UV,
                ParticleSystemVertexStream.UV2, ParticleSystemVertexStream.Custom1XYZW, ParticleSystemVertexStream.Custom2XYZW,
                ParticleSystemVertexStream.Tangent });
            return system;
        }

        private static void Quad(string name, Vector3 position, Vector3 scale, Material material)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.position = position;
            quad.transform.localScale = scale;
            UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void Label(string text, Vector3 position, float size, Color color)
        {
            var label = new GameObject(text, typeof(TextMesh)).GetComponent<TextMesh>();
            label.transform.position = position;
            label.text = text;
            label.characterSize = size * 0.36f;
            label.fontSize = 48;
            label.color = color;
            label.anchor = TextAnchor.UpperLeft;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.GetComponent<Renderer>().sharedMaterial = label.font.material;
        }
    }
}
