// Runs unchanged in both isolated projects; runtime URP infrastructure is assessed separately.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaPublicApiCapture
{
    [Serializable] private sealed class Report { public string[] rows; }
    private readonly List<string> rows = new List<string>();
    private static Type Find(string n) => AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Nova.Editor."+n)).First(t=>t!=null);

    [Test]
    public void CaptureRuntimeApiDeclarations()
    {
        var assembly=AppDomain.CurrentDomain.GetAssemblies()
            .First(a=>a.GetType("Nova.Runtime.Core.Scripts.ScreenSpaceDistortion")!=null);
        var declarations=new List<string>();
        const BindingFlags flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly;
        foreach(var type in assembly.GetExportedTypes().Where(t=>t.FullName.StartsWith("Nova.")).OrderBy(t=>t.FullName))
        {
            declarations.Add(type.FullName+" type : "+type.BaseType?.FullName);
            foreach(var field in type.GetFields(flags).Where(f=>f.IsPublic||f.IsFamily||f.IsFamilyOrAssembly))
                declarations.Add(type.FullName+" field "+field.FieldType+" "+field.Name);
            foreach(var method in type.GetMethods(flags).Cast<MethodBase>().Concat(type.GetConstructors(flags))
                .Where(m=>m.IsPublic||m.IsFamily||m.IsFamilyOrAssembly))
                declarations.Add(type.FullName+" "+method);
        }
        Assert.That(declarations,Is.Not.Empty);
        var args=Environment.GetCommandLineArgs();var index=Array.IndexOf(args,"-novaRuntimeApiOutput");
        Assert.That(index,Is.GreaterThanOrEqualTo(0));
        File.WriteAllText(args[index+1],JsonUtility.ToJson(new Report{rows=declarations.OrderBy(x=>x,StringComparer.Ordinal).ToArray()},true));
    }

    [Test]
    public void CapturePublicApiBehavior()
    {
        var keywordType=Find("Core.Scripts.ShaderKeywords");
        var methods=keywordType.GetMethods(BindingFlags.Public|BindingFlags.Static).Where(m=>m.Name.StartsWith("Get")).ToArray();
        Assert.That(methods.Length,Is.EqualTo(10));
        foreach(var method in methods)
        {
            var type=method.GetParameters()[0].ParameterType;
            foreach(var value in Enum.GetValues(type).Cast<object>().Concat(new[]{Enum.ToObject(type,-1),Enum.ToObject(type,int.MaxValue)}))
            {
                var key=method.Name+"/"+type.Name+"/"+Convert.ToInt32(value);
                try { rows.Add(key+"="+method.Invoke(null,new[]{value})); }
                catch(TargetInvocationException e) { rows.Add(key+"=throws:"+e.InnerException.GetType().FullName); }
            }
        }
        foreach(var shader in new[]{"Particles/UberUnlit","Particles/UberLit","UIParticles/UberUnlit","UIParticles/UberLit"})
        {
            for(var seed=0;seed<32;seed++) Cleanup(shader,seed);
            Streams(shader,false); Streams(shader,true);
        }
        var args=Environment.GetCommandLineArgs();
        var index=Array.IndexOf(args,"-novaApiOutput"); Assert.That(index,Is.GreaterThanOrEqualTo(0));
        var path=args[index+1]; Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new Report{rows=rows.OrderBy(x=>x,StringComparer.Ordinal).ToArray()},true));
    }

    private void Cleanup(string name,int seed)
    {
        var m=new Material(Shader.Find("Nova/"+name));
        var t=new Texture2D(2,2); var a=new Texture2DArray(2,2,4,TextureFormat.RGBA32,false); var v=new Texture3D(2,2,4,TextureFormat.RGBA32,false);
        try
        {
            var r=new System.Random(20261013+seed);
            foreach(var p in new[]{"_BaseMapMode","_ParallaxMapMode","_AlphaTransitionMapMode","_AlphaTransitionMapSecondTextureMapMode","_EmissionMapMode","_ColorCorrectionMode","_TintAreaMode","_AlphaTransitionMode","_AlphaTransitionSecondTextureBlendMode"}) m.SetFloat(p,r.Next(3));
            m.SetFloat("_TintColorMode",r.Next(4)); m.SetFloat("_EmissionAreaType",r.Next(4)); m.SetFloat("_EmissionColorType",r.Next(3));
            if(m.HasProperty("_LitWorkflowMode"))m.SetFloat("_LitWorkflowMode",r.Next(2));
            var names=new List<string>();
            for(var i=0;i<m.shader.GetPropertyCount();i++)
            {
                if(m.shader.GetPropertyType(i)!=ShaderPropertyType.Texture)continue;
                var p=m.shader.GetPropertyName(i); names.Add(p);
                var d=m.shader.GetPropertyTextureDimension(i);
                m.SetTexture(p,d==TextureDimension.Tex2DArray ? a : d==TextureDimension.Tex3D ? (Texture)v : t);
            }
            var api=Find("Foundation.Scripts.UnusedReferencesRemover").GetMethod("RemoveUnusedReferences",new[]{typeof(Material)});
            api.Invoke(null,new object[]{m});
            string State()=>string.Join(",",names.Select(p=>p+":"+(m.GetTexture(p)==null ? "null" : m.GetTexture(p).dimension.ToString())));
            var state=State(); rows.Add("cleanup/"+name+"/"+seed+"="+state);
            api.Invoke(null,new object[]{m}); Assert.That(State(),Is.EqualTo(state),"Cleanup must be idempotent.");
            var mode=(int)m.GetFloat("_BaseMapMode");
            Assert.That(m.GetTexture(new[]{"_BaseMap","_BaseMap2DArray","_BaseMap3D"}[mode]),Is.Not.Null,"Retain the active base map.");
        }
        finally { UnityEngine.Object.DestroyImmediate(m);UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(v); }
    }

    private void Streams(string name,bool trail)
    {
        var m=new Material(Shader.Find("Nova/"+name)); var obj=new GameObject("Public API streams",typeof(ParticleSystem));
        try
        {
            var system=obj.GetComponent<ParticleSystem>(); system.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var renderer=obj.GetComponent<ParticleSystemRenderer>();
            m.SetFloat("_BaseMapOffsetXCoord",1);m.SetFloat("_VertexDeformationEnabled",1);m.SetFloat("_VertexDeformationIntensityCoord",2);
            var stream=new List<ParticleSystemVertexStream>{ParticleSystemVertexStream.Position};
            if(trail) { renderer.sharedMaterial=null;renderer.trailMaterial=m;renderer.SetActiveTrailVertexStreams(stream); }
            else { renderer.sharedMaterial=m;renderer.SetActiveVertexStreams(stream); }
            var type=Find("Core.Scripts.RendererErrorHandler"); var check=type.GetMethod("CheckErrorWithMaterial");var fix=type.GetMethod("FixErrorWithMaterial");
            Assert.That(check.Invoke(null,new object[]{m}),Is.True);
            fix.Invoke(null,new object[]{m});Assert.That(check.Invoke(null,new object[]{m}),Is.False);
            if(trail)renderer.GetActiveTrailVertexStreams(stream);else renderer.GetActiveVertexStreams(stream);
            rows.Add("streams/"+name+"/trail="+trail+"="+string.Join(",",stream));
            fix.Invoke(null,new object[]{m});Assert.That(check.Invoke(null,new object[]{m}),Is.False);
        }
        finally {UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(m);}
    }
}
#endif
