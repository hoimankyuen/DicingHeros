Shader "SimpleDecal/Overlay"
{
	Properties
	{
		[PerRendererData] _Tint("Tint", Color) = (1, 1, 1, 1)
		[PerRendererData] _MainTex("Texture", 2D) = "white" {}

		[PerRendererData] _Color("Color", Color) = (1, 1, 1, 1)
		[PerRendererData] _Show("Show", Range(0, 1)) = 1
	}

		SubShader
		{
			Tags 
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"ForceNoShadowCasting" = "True"
				"LightMode" = "ForwardBase"
			}

			Pass
			{
				Fog 
				{ 
					Mode Off 
				}

				ZWrite Off
				ZTest Off
				Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM
				#pragma target 3.0

				#pragma vertex vert
				#pragma fragment frag
				#pragma exclude_renderers nomrt

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};


				struct v2f
				{
					float4 pos : SV_POSITION;
					half2 uv : TEXCOORD0;
					float4 screenUV : TEXCOORD1;
					float3 ray : TEXCOORD2;
				};

				sampler2D _MainTex;
				fixed4 _Tint;

				uniform fixed4 _Color;
				uniform fixed _Show;

				sampler2D _CameraDepthTexture;

				v2f vert(appdata v)
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);

					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.screenUV = ComputeScreenPos(o.pos);
					o.ray = UnityObjectToViewPos(v.vertex).xyz * float3(-1, -1, 1);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					
					// find the camera butter depth and normal for this pixel
					float2 depthUV = i.screenUV.xy / i.screenUV.w;
					float sceneDepth;
					sceneDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, depthUV));

					// locate the pixel in the object space
					i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
					float4 vpos = float4(i.ray * sceneDepth, 1);
					float3 wpos = mul(unity_CameraToWorld, vpos).xyz;
					float3 opos = mul(unity_WorldToObject, float4(wpos, 1)).xyz;

					// clip out all pixels that are beyond the 1,1,1 projector cube
					clip(float3(0.5, 0.5, 0.5) - abs(opos.xyz));

					// project the texture from local y axis in object space
					i.uv = (opos.xz + 0.5);

					// sample texture map and calculate texture color
					fixed4 color = _Color;
					color.a *= (tex2D(_MainTex, i.uv) * _Tint).a * _Show;
					return color;
				}

				ENDCG
			}
		}
}