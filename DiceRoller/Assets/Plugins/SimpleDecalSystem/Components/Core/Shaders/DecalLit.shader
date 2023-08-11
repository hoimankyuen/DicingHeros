Shader "SimpleDecal/Lit"
{
	Properties
	{
		[PerRendererData] _Tint("Tint", Color) = (1, 1, 1, 1)
		[PerRendererData] _MainTex("Texture", 2D) = "white" {}
		[Gamma][PerRendererData] _Metallic("Metallic", Range(0, 1)) = 0
		[PerRendererData] _Smoothness("Smoothness", Range(0, 1)) = 0.1
		[PerRendererData] _NormalMap("Normal Map", 2D) = "bump" {}
		[PerRendererData] _NormalStrength("Normal Strength",  Range(0, 10)) = 1
	}

	SubShader
	{
		Tags 
		{
			"Queue" = "Geometry+1"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"ForceNoShadowCasting" = "True"
		}

		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
			}

			ZWrite Off
			ZTest Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile _ VERTEXLIGHT_ON

			#pragma vertex vert
			#pragma fragment frag
			
			#define FORWARD_BASE_PASS

			#include "DecalLitBase.cginc"

			ENDCG
		}

		Pass
		{
			Tags
			{
				"LightMode" = "ForwardAdd"
			}

			ZWrite Off
			ZTest Off
			Blend SrcAlpha One

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex vert
			#pragma fragment frag

			#include "DecalLitBase.cginc"

			ENDCG
		}
	}
}