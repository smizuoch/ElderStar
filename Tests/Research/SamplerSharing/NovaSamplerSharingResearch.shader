Shader "Hidden/NovaResearch/SamplerSharing"
{
 Properties { _MainTex("Source",2D)="white"{} _SamplerDonor("Sampler",2D)="white"{} }
 SubShader { Pass { ZTest Always ZWrite Off Cull Off
 CGPROGRAM
 #pragma target 3.5
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 Texture2D _MainTex;
 SamplerState sampler_MainTex;
 Texture2D _SamplerDonor;
 SamplerState sampler_SamplerDonor;
 SamplerState test_linear_clamp_aniso8_sampler;
 float _Mode;
 float4 _Footprint;
 float4 frag(v2f_img i):SV_Target {
   float2 uv=0.5+float2(dot(i.uv-0.5,_Footprint.xy),dot(i.uv-0.5,_Footprint.zw));
   [branch] if(_Mode>=3.0) return _SamplerDonor.Load(int3(0,0,0));
   if(_Mode<0.5) return _MainTex.Sample(sampler_MainTex,uv);
   if(_Mode<1.5) return _MainTex.Sample(sampler_SamplerDonor,uv);
   return _MainTex.Sample(test_linear_clamp_aniso8_sampler,uv);
 }
 ENDCG
 } }
}