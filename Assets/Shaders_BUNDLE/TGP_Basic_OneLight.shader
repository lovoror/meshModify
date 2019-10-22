// Toony Colors Pro+Mobile Shaders
// (c) 2013,2014 Jean Moreno

Shader "Toony Colors Pro/Normal/OneDirLight/Basic"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
		
		//COLORS
		_Color ("Highlight Color", Color) = (0.8,0.8,0.8,1)
		_SColor ("Shadow Color", Color) = (0.0,0.0,0.0,1)

		_LightDirection ("Light Direction", Vector) = (0.0, 0.0, 0.0, 1.0)
	}
	
	SubShader
	{
		Tags { "Queue"="Geometry" "RenderType"="Opaque" } //Tags { "RenderType"="Opaque" } // 20180213, yysa, changed

		LOD 200
		
		CGPROGRAM
		 
		#include "TGP_Include.cginc"
		 
		//nolightmap nodirlightmap		LIGHTMAP
		//noforwardadd					ONLY 1 DIR LIGHT (OTHER LIGHTS AS VERTEX-LIT)
		#pragma multi_compile UNITY_SHADOW_ON UNITY_SHADOW_OFF	
		#pragma skip_variants FOG_EXP FOG_EXP2 LIGHTPROBE_SH
		#pragma surface surf ToonyColors nolightmap nofog novertexlights nodirlightmap noforwardadd nodynlightmap
		
		
		sampler2D _MainTex;		
		
		struct Input
		{
			half2 uv_MainTex : TEXCOORD0;
		};
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			
			o.Albedo = c.rgb;
			
			o.Alpha = c.a;
		}
		ENDCG
	}
	
	Fallback "VertexLit"
	CustomEditor "G2_TGP_Basic_OneLight_Shadow_Inspector"
}
