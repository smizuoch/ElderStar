// Copy into Assets/Codex/NovaVRCValidation/Editor in an SDK 3.10.5 avatar project.
// Verification fixture only: the NOVA package has no VRChat assembly dependency.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3A.Editor;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;
using Object = UnityEngine.Object;

public static class NovaVrcAcceptance
{
    const string Root = "Assets/Codex/NovaVRCValidation";
    const string Output = "Exports/NovaVRCValidation";
    const string Scene = Root + "/Nova_PC_Avatar.unity";
    [Serializable] public class Report
    {
        public string unity, operation, bundle, error, aaoVersion, avatarId;
        public bool success, loggedIn, canPublishAvatars, aaoEnabled, sdkStripAllShaders;
        public bool uploadedPrivate;
        public int errors, shaderErrors, particleSystems, novaMaterials, magentaPixels;
        public int sourceSkinnedMeshes, builtSkinnedMeshes;
        public int activeParticleRenderers;
        public long bundleBytes;
        public string[] shaderNames;
        public List<string> errorMessages = new List<string>();
        public string[] sourceMaterialStates, builtMaterialStates, particleStreams;
    }
    static Report report;
    static double deadline;

    // Invoke only after the user authorizes a new private upload. This method
    // rejects unrelated blueprints and keeps an attempt record. Resume requires an
    // explicit matching ID and verifies ownership, name and Private status first.
    public static void UploadPrivate() => UploadPrivate(false);
    public static void ResumePrivate() => UploadPrivate(true);
    static void UploadPrivate(bool resume) => Run(async () =>
    {
        foreach (var label in new[] { "aao-off", "aao-on" })
        {
            var evidence = JsonUtility.FromJson<Report>(File.ReadAllText(Output + "/" + label + ".json"));
            Require(evidence.success && evidence.errors == 0, "AAO comparison must pass before upload: " + label);
        }
        Require(EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64, "PC upload only.");
        Require(File.Exists(Output + "/upload-attempt.json") == resume,
            "Inspect the attempt record before selecting a new upload or an explicit resume.");
        var arguments = Environment.GetCommandLineArgs();
        var resumeIndex = Array.IndexOf(arguments, "-novaResumeAvatarId");
        var resumeId = resumeIndex >= 0 && resumeIndex + 1 < arguments.Length ? arguments[resumeIndex + 1] : "";
        Require(!resume || !string.IsNullOrEmpty(resumeId), "Resume requires the exact reserved avatar ID.");
        EditorSceneManager.OpenScene(Scene);
        var avatar = Object.FindObjectOfType<VRCAvatarDescriptor>().gameObject;
        var pipeline = avatar.GetComponent<PipelineManager>();
        Require(pipeline, "Validation avatar pipeline missing.");
        Require(!resume || pipeline.blueprintId == resumeId, "Reserved ID does not match the owned scene.");
        foreach (var child in avatar.transform.Cast<Transform>().Where(t => t.name.StartsWith("NOVA Skinned ")).ToArray())
            Object.DestroyImmediate(child.gameObject);
        AddSkinnedProbes(avatar);
        var trace = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Anatawa12.AvatarOptimizer.TraceAndOptimize")).First(t => t != null);
        if (!avatar.GetComponent(trace)) avatar.AddComponent(trace);
        Inspect(avatar);
        Capture(avatar, Object.FindObjectOfType<Camera>(), "upload-thumbnail.png");
        EditorWindow.GetWindow<VRCSdkControlPanel>();
        await Task.Delay(3000);
        Require(VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder), "SDK builder unavailable.");
        report.loggedIn = APIUser.CurrentUser != null;
        report.canPublishAvatars = APIUser.CurrentUser?.canPublishAvatars ?? false;
        Require(report.loggedIn && report.canPublishAvatars, "SDK account cannot publish avatars.");
        if (!string.IsNullOrEmpty(pipeline.blueprintId))
        {
            // Earlier local SDK builds can save an assigned ID before any upload.
            // Clear it only when the server confirms no such avatar exists.
            try
            {
                var reserved = await VRCApi.GetAvatar(pipeline.blueprintId, true);
                Require(resume && reserved.ID == resumeId && reserved.Name == "NOVA PC Validation"
                    && reserved.ReleaseStatus == "private" && reserved.AuthorId == APIUser.CurrentUser.id,
                    "Refusing to replace an unrelated remote avatar.");
                report.avatarId = reserved.ID;
                if (!string.IsNullOrEmpty(reserved.GetLatestAssetUrlForPlatform("standalonewindows")))
                { report.uploadedPrivate = true; return; }
            }
            catch (ApiErrorException e) when (!resume && e.StatusCode == System.Net.HttpStatusCode.NotFound) { pipeline.blueprintId = ""; }
        }
        var data = new VRCAvatar { Name = "NOVA PC Validation", ReleaseStatus = "private", Tags = new List<string>(),
            Description = "Private PC validation of NOVA Shader Built-in 0.1.3 with Fire001 and AAO Trace And Optimize." };
        report.avatarId = pipeline.blueprintId;
        File.WriteAllText(Output + "/upload-attempt.json", JsonUtility.ToJson(report, true));
        try
        {
            await builder.BuildAndUpload(avatar, data, Path.GetFullPath(Output + "/upload-thumbnail.png"));
            report.avatarId = pipeline.blueprintId;
            var remote = await VRCApi.GetAvatar(report.avatarId, true);
            Require(remote.ReleaseStatus == "private", "Uploaded avatar is not private.");
            Require(!string.IsNullOrEmpty(remote.GetLatestAssetUrlForPlatform("standalonewindows")), "No PC asset registered remotely.");
            report.uploadedPrivate = true;
        }
        finally
        {
            report.avatarId = pipeline.blueprintId;
            File.WriteAllText(Output + "/upload-attempt.json", JsonUtility.ToJson(report, true));
            if (!string.IsNullOrEmpty(pipeline.blueprintId)) EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Scene);
        }
    }, "sdk-upload");

    // The pair differs only by the public AAO Trace And Optimize default component.
    // Never save these experimental scene changes over the user's source avatar.
    public static void CompareAAO()
    {
        var args = Environment.GetCommandLineArgs();
        var enabled = args.Contains("-novaAAO");
        var label = enabled ? "aao-on" : "aao-off";
        Run(async () =>
        {
            EditorSceneManager.OpenScene(Scene);
            var avatar = Object.FindObjectOfType<VRCAvatarDescriptor>().gameObject;
            avatar.GetComponent<PipelineManager>().blueprintId = ""; // Local comparison only; never saved.
            foreach (var child in avatar.transform.Cast<Transform>().Where(t => t.name.StartsWith("NOVA Skinned ")).ToArray())
                Object.DestroyImmediate(child.gameObject);
            for (var i = 2; i < 4; i++)
            {
                var material = avatar.transform.Find("NOVA Probe " + i).GetComponent<Renderer>().sharedMaterial;
                material.SetColor("_SpecularColor", new Color(.04f, .04f, .04f, 1));
                EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
            }
            AddSkinnedProbes(avatar);
            report.sourceSkinnedMeshes = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
            var tagType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Anatawa12.AvatarOptimizer.AvatarTagComponent")).First(t => t != null);
            var traceType = tagType.Assembly.GetType("Anatawa12.AvatarOptimizer.TraceAndOptimize", true);
            foreach (var component in avatar.GetComponentsInChildren(tagType, true))
                Object.DestroyImmediate(component);
            if (enabled) avatar.AddComponent(traceType);
            report.aaoEnabled = enabled;
            report.aaoVersion = UnityEditor.PackageManager.PackageInfo.FindForAssembly(traceType.Assembly).version;
            report.sdkStripAllShaders = EditorPrefs.GetBool("VRC.SDKBase_StripAllShaders", false);
            Debug.Log("NOVA environment: AAO=" + enabled + "; SDK StripAllShaders=" + report.sdkStripAllShaders);
            if (args.Contains("-novaRefresh"))
                foreach (var shader in avatar.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials)
                    .Where(m => m && m.shader && m.shader.name.StartsWith("Nova/")).Select(m => m.shader).Distinct())
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(shader), ImportAssetOptions.ForceUpdate);
                }
            Inspect(avatar);
            report.sourceMaterialStates = MaterialStates(avatar);
            var sourceStreams = report.particleStreams;
            Capture(avatar, Object.FindObjectOfType<Camera>(), label + "-source.png");
            EditorWindow.GetWindow<VRCSdkControlPanel>();
            await Task.Delay(3000);
            Require(VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder), "SDK builder unavailable.");
            // A local comparison has no remote avatar record. The final publish
            // run validates upload limits through BuildAndUpload separately.
            VRC_SdkBuilder.ActiveBuildType = VRC_SdkBuilder.BuildType.Test;
            var built = await builder.Build(avatar);
            report.bundle = Output + "/" + label + ".vrca";
            File.Copy(built, report.bundle, true);
            report.bundleBytes = new FileInfo(report.bundle).Length;
            var position = avatar.transform.position; var rotation = avatar.transform.rotation;
            avatar.SetActive(false);
            var bundle = AssetBundle.LoadFromFile(report.bundle);
            Require(bundle, "SDK bundle failed to load.");
            try
            {
                var prefab = bundle.LoadAllAssets<GameObject>().Single();
                var instance = Object.Instantiate(prefab);
                instance.transform.SetPositionAndRotation(position, rotation); instance.SetActive(true);
                Inspect(instance, false);
                report.builtSkinnedMeshes = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
                if (enabled) Require(report.builtSkinnedMeshes < report.sourceSkinnedMeshes, "AAO did not exercise mesh merging.");
                Require(sourceStreams.SequenceEqual(report.particleStreams), "SDK/AAO changed particle vertex streams.");
                report.builtMaterialStates = MaterialStates(instance);
                Require(report.sourceMaterialStates.SequenceEqual(report.builtMaterialStates), "SDK/AAO changed NOVA material properties, keywords or textures.");
                Capture(instance, Object.FindObjectOfType<Camera>(), label + "-bundle.png");
                Capture(instance, Object.FindObjectOfType<Camera>(), label + "-bundle-04.png", .4f);
                Capture(instance, Object.FindObjectOfType<Camera>(), label + "-bundle-21.png", 2.1f);
                Object.DestroyImmediate(instance);
            }
            finally { bundle.Unload(true); }
        }, label);
    }

    static void AddSkinnedProbes(GameObject avatar)
    {
        // Two renderers per shader make AAO exercise automatic merging, not just
        // pass through the unchanged ParticleSystemRenderers from Fire001.
        var patternedMap = new Texture2D(4, 4, TextureFormat.RGBA32, false) { name = "AAO custom UV probe", filterMode = FilterMode.Point };
        patternedMap.SetPixels(Enumerable.Range(0, 16).Select(i => (i % 4 + i / 4) % 2 == 0 ? Color.white : new Color(.2f, .6f, .8f, 1)).ToArray());
        patternedMap.Apply();
        var materials = new Material[4];
        for (var shader = 0; shader < 4; shader++)
        for (var copy = 0; copy < 2; copy++)
        {
            var original = avatar.transform.Find("NOVA Probe " + shader);
            var go = new GameObject("NOVA Skinned " + shader + " " + copy, typeof(SkinnedMeshRenderer));
            go.transform.SetParent(avatar.transform, false);
            go.transform.localPosition = new Vector3(.35f + copy * .2f, .4f + shader * .23f, .4f);
            go.transform.localRotation = Quaternion.Euler(0, 180, 0);
            var mesh = Object.Instantiate(original.GetComponent<MeshFilter>().sharedMesh);
            mesh.vertices = mesh.vertices.Select(v => v * .15f).ToArray();
            mesh.colors = new[] { Color.red, Color.green, Color.blue, Color.white };
            mesh.SetUVs(1, Enumerable.Repeat(new Vector4(.2f, .4f, .6f, .8f), mesh.vertexCount).ToList());
            mesh.SetUVs(2, Enumerable.Repeat(new Vector4(.9f, .7f, .5f, .3f), mesh.vertexCount).ToList());
            mesh.boneWeights = Enumerable.Repeat(new BoneWeight { boneIndex0 = 0, weight0 = 1 }, mesh.vertexCount).ToArray();
            mesh.bindposes = new[] { avatar.transform.worldToLocalMatrix * go.transform.localToWorldMatrix };
            mesh.RecalculateBounds();
            var renderer = go.GetComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = mesh; renderer.bones = new[] { avatar.transform }; renderer.rootBone = avatar.transform;
            if (!materials[shader])
            {
                var material = new Material(original.GetComponent<Renderer>().sharedMaterial) { name = "AAO Feature Probe " + shader };
                material.SetTexture("_BaseMap", patternedMap);
                material.SetFloat("_BaseMapOffsetXCoord", 1); material.SetFloat("_BaseMapOffsetYCoord", 5);
                material.SetTexture("_VertexDeformationMap", patternedMap);
                material.SetFloat("_VertexDeformationIntensity", .03f); material.SetFloat("_VertexDeformationIntensityCoord", 8);
                material.EnableKeyword("_VERTEX_DEFORMATION_ENABLED");
                materials[shader] = material;
            }
            renderer.sharedMaterial = materials[shader];
            renderer.updateWhenOffscreen = true;
        }
    }

    public static void Prepare()
    {
        Run(async () =>
        {
            Require(!File.Exists(Scene), "Validation scene already exists; use Validate to reuse it.");
            Directory.CreateDirectory(Output);
            var importer = typeof(Nova.Editor.Core.Scripts.BuiltInEffectPackageImporter);
            var plan = importer.GetMethod("Preview").Invoke(null, new object[] { "C:/Users/massu/Downloads/Fire001.unitypackage" });
            importer.GetMethod("Import").Invoke(null, new[] { plan, (object)false });
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Shinano/Prefab/Shinano.prefab");
            Require(source, "Shinano prefab missing.");
            var avatar = (GameObject)PrefabUtility.InstantiatePrefab(source);
            PrefabUtility.UnpackPrefabInstance(avatar, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            avatar.name = "NOVA PC Validation";
            avatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var pipeline = avatar.GetComponent<PipelineManager>() ?? avatar.AddComponent<PipelineManager>();
            pipeline.blueprintId = "";
            var fire = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/N2Studio/Prefabs/Fire001.prefab"));
            fire.transform.SetParent(avatar.transform, false);
            fire.transform.localPosition = new Vector3(0.65f, 0.9f, -0.15f);
            fire.transform.localScale = Vector3.one * 0.3f;
            var names = new[] { "Nova/Particles/UberUnlit", "Nova/UIParticles/UberUnlit", "Nova/Particles/UberLit", "Nova/UIParticles/UberLit", "Nova/Particles/Distortion" };
            for (var index = 0; index < names.Length; index++)
            {
                var shader = Shader.Find(names[index]);
                Require(shader, "Shader missing: " + names[index]);
                var material = new Material(shader);
                material.SetFloat("_Cull", 0);
                if (index < 4)
                {
                    material.EnableKeyword("_BASE_MAP_MODE_2D");
                    material.SetTexture("_BaseMap", Texture2D.whiteTexture);
                }
                else
                {
                    var map = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                    map.SetPixels(Enumerable.Repeat(new Color(1, .5f, 0, 1), 4).ToArray()); map.Apply();
                    AssetDatabase.CreateAsset(map, Root + "/DistortionOffset.asset");
                    material.SetTexture("_BaseMap", map);
                    material.SetFloat("_DistortionIntensity", .5f);
                    material.renderQueue = 3100;
                }
                AssetDatabase.CreateAsset(material, Root + "/Probe" + index + ".mat");
                var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Object.DestroyImmediate(panel.GetComponent<Collider>());
                panel.name = "NOVA Probe " + index;
                panel.transform.SetParent(avatar.transform, false);
                panel.transform.localPosition = index == 4 ? new Vector3(0, .9f, -.35f) : new Vector3(-.75f, .4f + index * .3f, -.15f);
                panel.transform.localScale = Vector3.one * .25f;
                var mesh = Object.Instantiate(panel.GetComponent<MeshFilter>().sharedMesh);
                mesh.colors = Enumerable.Repeat(index == 4 ? Color.white : Color.HSVToRGB(index * .22f, .7f, 1), mesh.vertexCount).ToArray();
                AssetDatabase.CreateAsset(mesh, Root + "/ProbeMesh" + index + ".asset");
                panel.GetComponent<MeshFilter>().sharedMesh = mesh;
                panel.GetComponent<Renderer>().sharedMaterial = material;
            }
            Require(avatar.GetComponent<VRCAvatarDescriptor>(), "Avatar descriptor missing.");
            PrefabUtility.SaveAsPrefabAsset(avatar, Root + "/Nova_PC_Avatar.prefab");
            var light = new GameObject("Validation Light", typeof(Light)).GetComponent<Light>();
            light.type = LightType.Directional; light.transform.rotation = Quaternion.Euler(30, -30, 0);
            var camera = new GameObject("Validation Camera", typeof(Camera)).GetComponent<Camera>();
            camera.transform.position = new Vector3(0, 1, -3);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.035f, .045f, .06f);
            camera.allowMSAA = false;
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Scene);
            Inspect(avatar);
            Capture(avatar, camera, "editor-before-build.png");
            await Task.CompletedTask;
        }, "prepare");
    }

    public static void Validate() => Run(async () =>
    {
        EditorSceneManager.OpenScene(Scene);
        var avatar = Object.FindObjectOfType<VRCAvatarDescriptor>().gameObject;
        avatar.transform.rotation = Quaternion.Euler(0, 180, 0);
        for (var i = 0; i < 5; i++)
        {
            var probe = avatar.transform.Find("NOVA Probe " + i);
            probe.localRotation = Quaternion.Euler(0, 180, 0);
            var position = probe.localPosition; position.z = i == 4 ? .35f : .2f; probe.localPosition = position;
            if (i == 2 || i == 3)
            {
                var material = probe.GetComponent<Renderer>().sharedMaterial;
                material.SetFloat("_Metallic", 0); material.SetFloat("_Smoothness", .3f);
                Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor.SetupMaterialKeywords(material);
                EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
            }
        }
        var fire = avatar.transform.Cast<Transform>().First(t => t.name.StartsWith("Fire001"));
        fire.localScale = Vector3.one * .1f;
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Scene);
        Inspect(avatar);
        foreach (var texture in EditorUtility.CollectDependencies(new Object[] { avatar }).OfType<Texture2D>())
        {
            var path = AssetDatabase.GetAssetPath(texture);
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer) || !importer.mipmapEnabled || importer.streamingMipmaps) continue;
            Require(path.StartsWith("Assets/"), "Package texture needs streaming mip maps: " + path);
            var backup = Output + "/Backup-20261012/TextureMeta/" + path;
            Directory.CreateDirectory(Path.GetDirectoryName(backup));
            if (!File.Exists(backup + ".meta")) File.Copy(path + ".meta", backup + ".meta");
            importer.streamingMipmaps = true; importer.SaveAndReimport();
            Debug.Log("NOVA VRC: enabled required streaming mip maps: " + path);
        }
        var panel = EditorWindow.GetWindow<VRCSdkControlPanel>();
        await Task.Delay(3000);
        Require(VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder), "SDK builder unavailable.");
        report.loggedIn = APIUser.CurrentUser != null;
        report.canPublishAvatars = APIUser.CurrentUser?.canPublishAvatars ?? false;
        builder.OnSdkBuildProgress += (_, progress) => Debug.Log("NOVA SDK: " + progress);
        builder.OnSdkBuildSuccess += (_, path) => report.bundle = path;
        // Publish validates the actual upload size limits; Build itself does not upload.
        var previousType = VRC_SdkBuilder.ActiveBuildType;
        try
        {
            avatar.GetComponent<PipelineManager>().AssignId(PipelineManager.ContentType.avatar);
            VRC_SdkBuilder.ActiveBuildType = VRC_SdkBuilder.BuildType.Publish;
            report.bundle = await builder.Build(avatar);
            Require(File.Exists(report.bundle), "SDK produced no avatar bundle.");
            report.bundleBytes = new FileInfo(report.bundle).Length;
            File.Copy(report.bundle, Output + "/Nova_PC_Avatar.vrca", true);
            Capture(avatar, Object.FindObjectOfType<Camera>(), "editor-after-build.png");
        }
        finally { avatar.GetComponent<PipelineManager>().blueprintId = ""; VRC_SdkBuilder.ActiveBuildType = previousType; }
    }, "sdk-build");

    public static void BuildAndTest() => Run(async () =>
    {
        EditorSceneManager.OpenScene(Scene);
        var avatar = Object.FindObjectOfType<VRCAvatarDescriptor>().gameObject;
        Inspect(avatar);
        EditorWindow.GetWindow<VRCSdkControlPanel>();
        await Task.Delay(3000);
        Require(VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder), "SDK builder unavailable.");
        report.loggedIn = APIUser.CurrentUser != null;
        report.canPublishAvatars = APIUser.CurrentUser?.canPublishAvatars ?? false;
        builder.OnSdkBuildSuccess += (_, path) => report.bundle = path;
        await builder.BuildAndTest(avatar);
        Require(File.Exists(report.bundle), "Local test bundle missing.");
        report.bundleBytes = new FileInfo(report.bundle).Length;
    }, "sdk-test");

    public static void InspectBundle() => Run(async () =>
    {
        EditorSceneManager.OpenScene(Scene);
        var source = Object.FindObjectOfType<VRCAvatarDescriptor>().gameObject;
        var position = source.transform.position;
        var rotation = source.transform.rotation;
        source.SetActive(false);
        report.bundle = Output + "/Nova_PC_Avatar.vrca";
        var bundle = AssetBundle.LoadFromFile(report.bundle);
        Require(bundle, "Built avatar bundle could not be opened.");
        try
        {
            var assets = bundle.LoadAllAssets<GameObject>();
            Require(assets.Length == 1, "Expected exactly one avatar prefab in the SDK bundle.");
            var avatar = Object.Instantiate(assets[0]);
            avatar.transform.SetPositionAndRotation(position, rotation);
            avatar.SetActive(true);
            Inspect(avatar, false);
            Require(avatar.GetComponentsInChildren<Renderer>(true).All(r => r.sharedMaterials.All(m => !m || m.shader)),
                "Built avatar contains a material with a missing shader.");
            report.bundleBytes = new FileInfo(report.bundle).Length;
            Capture(avatar, Object.FindObjectOfType<Camera>(), "sdk-bundle-render.png");
            Object.DestroyImmediate(avatar);
        }
        finally { bundle.Unload(true); }
        await Task.CompletedTask;
    }, "sdk-bundle");

    static void Inspect(GameObject avatar, bool compileSource = true)
    {
        report.particleSystems = avatar.GetComponentsInChildren<ParticleSystem>(true).Length;
        var activeParticles = avatar.GetComponentsInChildren<ParticleSystemRenderer>(true)
            .Where(r => r.enabled && r.gameObject.activeInHierarchy).ToArray();
        report.activeParticleRenderers = activeParticles.Length;
        report.particleStreams = activeParticles.Select(r =>
        {
            var streams = new List<ParticleSystemVertexStream>(); r.GetActiveVertexStreams(streams);
            return r.name + ":" + string.Join(",", streams);
        }).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var materials = avatar.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials)
            .Where(m => m && m.shader && m.shader.name.StartsWith("Nova/")).Distinct().ToArray();
        report.novaMaterials = materials.Length;
        report.shaderNames = materials.Select(m => m.shader.name).Distinct().ToArray();
        Require(report.shaderNames.Length == 5, "All five NOVA shaders must be present on the avatar.");
        // AAO may remove the sixth, permanently disabled Fire001 renderer.
        Require(report.activeParticleRenderers >= 5, "Visible Fire001 particle renderers missing.");
        Require(!avatar.GetComponentsInChildren<MonoBehaviour>(true).Any(c => c && c.GetType().Namespace?.StartsWith("Nova.Runtime") == true),
            "Avatar must not need NOVA runtime scripts.");
        foreach (var m in materials)
        {
            Require(m.shader.isSupported, "Unsupported shader: " + m.shader.name);
            // Player bundles contain compiled shaders, not Editor shader source.
            if (compileSource)
                for (int pass = 0; pass < m.passCount; pass++) ShaderUtil.CompilePass(m, pass, true);
            report.shaderErrors += ShaderUtil.GetShaderMessages(m.shader).Count(e => e.severity.ToString() == "Error");
        }
        Require(report.shaderErrors == 0, "NOVA shader compile errors.");
    }

    static string[] MaterialStates(GameObject avatar)
    {
        string Number(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        string Vector(Vector4 value) => string.Join(",", new[] { value.x, value.y, value.z, value.w }.Select(Number));
        return avatar.GetComponentsInChildren<Renderer>(true).Where(r => r.enabled && r.gameObject.activeInHierarchy).SelectMany(r => r.sharedMaterials)
            .Where(m => m && m.shader && m.shader.name.StartsWith("Nova/")).Distinct().Select(m =>
            {
                var state = new List<string> { m.shader.name, "queue=" + m.renderQueue,
                    "keywords=" + string.Join(",", m.shaderKeywords.OrderBy(k => k, StringComparer.Ordinal)) };
                for (var i = 0; i < m.shader.GetPropertyCount(); i++)
                {
                    var name = m.shader.GetPropertyName(i);
                    var type = m.shader.GetPropertyType(i);
                    string value;
                    if (type == UnityEngine.Rendering.ShaderPropertyType.Texture)
                    {
                        var texture = m.GetTexture(name);
                        value = texture ? texture.dimension + ":" + texture.width + "x" + texture.height : "null";
                        value += ":" + Vector(m.GetTextureScale(name)) + ":" + Vector(m.GetTextureOffset(name));
                    }
                    else if (type == UnityEngine.Rendering.ShaderPropertyType.Color) value = Vector(m.GetColor(name));
                    else if (type == UnityEngine.Rendering.ShaderPropertyType.Vector) value = Vector(m.GetVector(name));
                    else value = Number(m.GetFloat(name));
                    state.Add(name + "=" + value);
                }
                return string.Join(";", state);
            }).OrderBy(s => s, StringComparer.Ordinal).ToArray();
    }

    static void Capture(GameObject avatar, Camera camera, string file, float time = 1.2f)
    {
        foreach (var ps in avatar.GetComponentsInChildren<ParticleSystem>(true))
        { ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); ps.useAutoRandomSeed = false; ps.randomSeed = 42; ps.Simulate(time, false, true); }
        var rt = new RenderTexture(768, 768, 24);
        var image = new Texture2D(768, 768, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
            image.ReadPixels(new Rect(0, 0, 768, 768), 0, 0); image.Apply();
            File.WriteAllBytes(Output + "/" + file, image.EncodeToPNG());
            report.magentaPixels = image.GetPixels32().Count(c => c.r > 245 && c.g < 10 && c.b > 245);
            Require(report.magentaPixels < 100, "Error-shader magenta detected in rendered frame: " + report.magentaPixels);
        }
        finally { camera.targetTexture = null; RenderTexture.active = previous; Object.DestroyImmediate(rt); Object.DestroyImmediate(image); }
    }

    static async void Run(Func<Task> action, string operation)
    {
        report = new Report { unity = Application.unityVersion, operation = operation };
        // The SDK saves open scenes during a build; restore this owned fixture
        // scene after comparisons so temporary AAO/probe objects cannot accumulate.
        var sceneBackup = operation.StartsWith("aao-") && File.Exists(Scene) ? File.ReadAllBytes(Scene) : null;
        Directory.CreateDirectory(Output);
        Application.logMessageReceived += CountErrors;
        deadline = EditorApplication.timeSinceStartup + 3600;
        EditorApplication.update += Watchdog;
        try { await action(); report.success = true; }
        catch (Exception e) { report.error = e.ToString(); Debug.LogException(e); }
        finally
        {
            Application.logMessageReceived -= CountErrors;
            if (sceneBackup != null) File.WriteAllBytes(Scene, sceneBackup);
            File.WriteAllText(Output + "/" + operation + ".json", JsonUtility.ToJson(report, true));
            EditorApplication.update -= Watchdog;
            EditorApplication.Exit(report.success && report.errors == 0 ? 0 : 1);
        }
    }
    static void CountErrors(string message, string stack, LogType type)
    { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) { report.errors++; report.errorMessages.Add(message); } }
    static void Watchdog()
    { if (EditorApplication.timeSinceStartup > deadline) { report.error = "Timed out"; File.WriteAllText(Output + "/" + report.operation + ".json", JsonUtility.ToJson(report, true)); EditorApplication.Exit(2); } }
    static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
}
