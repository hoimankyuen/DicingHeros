Shader "SimpleMaskCutoff/HalfOntopSprite"
{
	Properties
	{
		_Color("Tint", Color) = (1,1,1,1)
		_OnTopAlpha ("OnTopAlpha", float) = 0.5
		[PerRendererData] _MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{ 
			"Queue" = "AlphaTest" 
			"RenderType" = "TransparentCutout" 
			"IgnoreProjector" = "True" 
			"PreviewType" = "Plane" 
		}

		Lighting Off
		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		CGINCLUDE
		#include "UnityCG.cginc"

		#pragma vertex vert
		#pragma fragment frag

		fixed4 _Color;
		sampler2D _MainTex;
		float4 _MainTex_ST;

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			fixed4 color : COLOR;
		};

		struct v2f
		{
			float4 position : SV_POSITION;
			float2 uv : TEXCOORD0;
			fixed4 color : COLOR;
		};

		v2f vert(appdata v)
		{
			v2f o;
			o.position = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.color = v.color;
			return o;
		}
		ENDCG

		Pass
		{
			ZTest LEqual

			CGPROGRAM
			fixed4 frag(v2f i) : SV_TARGET
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _Color;
				col *= i.color;
				return col;
			}
			ENDCG
		}

		Pass
		{
			ZTest Greater

			CGPROGRAM
			float _OnTopAlpha;
			fixed4 frag(v2f i) : SV_TARGET
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _Color;
				col *= i.color;
				col.a *= _OnTopAlpha;
				return col;
			}
			ENDCG
		}
	}
}
