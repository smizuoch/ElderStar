using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Editor.Core.Scripts
{
    public enum NovaDebugView { Off, Albedo, Normals, Tangents, Depth, Fog, DirectLight, IndirectLight, AdditionalLightCount }

    /// <summary>Editor-only scoped rendering diagnostics; never edits a source material.</summary>
    public static class NovaShaderDiagnostics
    {
        internal const string Keyword = "NOVA_DEBUG_DISPLAY";
        internal readonly struct Scope : IDisposable
        {
            private readonly bool enabled;
            private readonly int mode;
            internal Scope(NovaDebugView view)
            {
                enabled = Shader.IsKeywordEnabled(Keyword); mode = Shader.GetGlobalInt("_NovaDebugMode");
                Shader.SetGlobalInt("_NovaDebugMode", (int)view);
                if (view == NovaDebugView.Off) Shader.DisableKeyword(Keyword); else Shader.EnableKeyword(Keyword);
            }
            public void Dispose()
            {
                Shader.SetGlobalInt("_NovaDebugMode", mode);
                if (enabled) Shader.EnableKeyword(Keyword); else Shader.DisableKeyword(Keyword);
            }
        }
        public static void Render(Camera camera, NovaDebugView view)
        {
            if (camera == null) throw new ArgumentNullException(nameof(camera));
            using (new Scope(view)) camera.Render();
        }
    }

    internal sealed class NovaShaderDiagnosticsWindow : EditorWindow
    {
        private NovaDebugView view;
        private readonly Stack<KeyValuePair<Camera, NovaShaderDiagnostics.Scope>> scopes = new Stack<KeyValuePair<Camera, NovaShaderDiagnostics.Scope>>();
        [MenuItem("Tools/ElderStar/Rendering Diagnostics")]
        private static void Open() { GetWindow<NovaShaderDiagnosticsWindow>("ElderStar Diagnostics"); }
        private void OnEnable() { Camera.onPreRender += BeforeCamera; Camera.onPostRender += AfterCamera; EditorApplication.update += RecoverInterruptedRender; }
        private void OnDisable()
        {
            Camera.onPreRender -= BeforeCamera; Camera.onPostRender -= AfterCamera; EditorApplication.update -= RecoverInterruptedRender;
            RecoverInterruptedRender(); SceneView.RepaintAll();
        }
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck(); view = (NovaDebugView)EditorGUILayout.EnumPopup("Scene view", view);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
            EditorGUILayout.HelpBox("Normals and tangents use world space. Depth is linear eye depth / far clip. Fog shows visibility. Additional lights use grayscale count / 8 (Canvas list count, or contributing ForwardAdd passes). Unlit direct/indirect light views are black. Source materials are unchanged.", MessageType.Info);
        }
        private void BeforeCamera(Camera camera)
        {
            if (camera.cameraType == CameraType.SceneView && view != NovaDebugView.Off)
                scopes.Push(new KeyValuePair<Camera, NovaShaderDiagnostics.Scope>(camera, new NovaShaderDiagnostics.Scope(view)));
        }
        private void AfterCamera(Camera camera)
        {
            if (scopes.Count > 0 && scopes.Peek().Key == camera) scopes.Pop().Value.Dispose();
        }
        private void RecoverInterruptedRender() { while (scopes.Count > 0) scopes.Pop().Value.Dispose(); }
    }

    // Diagnostic variants are useful in the Editor only. Keep the original
    // DEBUG_DISPLAY compatibility keyword independent and unchanged.
    internal sealed class NovaStripDiagnosticVariants : IPreprocessShaders
    {
        public int callbackOrder => 0;
        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            if (!shader.name.StartsWith("Nova/", StringComparison.Ordinal) && !shader.name.StartsWith("Hidden/Nova/", StringComparison.Ordinal)) return;
            var keyword = new ShaderKeyword(NovaShaderDiagnostics.Keyword);
            for (int i = data.Count - 1; i >= 0; i--) if (data[i].shaderKeywordSet.IsEnabled(keyword)) data.RemoveAt(i);
        }
    }
}
