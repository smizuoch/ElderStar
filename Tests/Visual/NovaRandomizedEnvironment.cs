// Copy into Assets/Editor in an isolated differential-capture project only.
using System;
using UnityEditor;
using UnityEngine;

public static class NovaRandomizedEnvironment
{
    public static void SetColorSpace()
    {
        var args=Environment.GetCommandLineArgs();
        var index=Array.IndexOf(args,"-novaColorSpace");
        if(index < 0 || index+1 >= args.Length)throw new ArgumentException("Missing -novaColorSpace Gamma|Linear");
        PlayerSettings.colorSpace=(ColorSpace)Enum.Parse(typeof(ColorSpace),args[index+1]);
        AssetDatabase.SaveAssets();
        Debug.Log("NOVA capture color space: "+PlayerSettings.colorSpace);
    }
}
