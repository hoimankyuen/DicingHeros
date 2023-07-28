Shader "SimpleMaskCutoff/OntopSpriteCutoff"
{
	Properties
	{
		_Color("Tint", Color) = (1,1,1,1)
		[PerRendererData] _MainTex("Texture", 2D) = "white" {}
		[PerRendererData] _CutoffMask("Cutoff mask", 2D) = "white" {}
		[PerRendererData] _CutoffFrom("Cutoff From", Range(0, 1)) = 0
		[PerRendererData] _CutoffTo("Cutoff To", Range(0, 1)) = 0.5
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

		
		Pass
		{
			Name "MAIN"

			Blend SrcAlpha OneMinusSrcAlpha
			Lighting Off
			Cull Off
			ZTest Off
			ZWrite On

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _CutoffMask;
			float4 _CutoffMask_ST;
			float _CutoffFrom;
			float _CutoffTo;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv1 : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv1 = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv, _CutoffMask);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
				fixed4 col = tex2D(_MainTex, i.uv1);
				fixed4 cutoff = tex2D(_CutoffMask, i.uv2);
				col *= _Color;
				col *= i.color;
				if (_CutoffTo > _CutoffFrom)
				{
					col.a *= step(cutoff.a, _CutoffTo) * step(_CutoffFrom, cutoff.a);
				}
				else if (_CutoffTo < _CutoffFrom)
				{
					col.a *= (1 - (1 - step(cutoff.a, _CutoffTo)) * (1 - step(_CutoffFrom, cutoff.a)));
				}
				else
				{
					col.a = 0;
				}

				return col;
			}
			ENDCG
		}
	}
}
