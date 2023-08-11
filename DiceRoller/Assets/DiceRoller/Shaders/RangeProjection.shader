// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/RangeProjection"
{
    Properties
    {
        [PerRendererData] _FlareColor("Flare Color", Color) = (1, 1, 1, 1)
        [PerRendererData] _LightColor("Light Color", Color) = (1, 1, 1, 1)
        [PerRendererData] _FlareWidth ("Flare Width", Float) = 0.2
        [PerRendererData] _LightWidth("Light Width", Float) = 0.1
        [PerRendererData] _Range("Range", Float) = 1
    }
    SubShader
    {
        Tags 
        {
            "RenderType"="Transparent-1"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "ForcedNoShadowCasting" = "True"
            "LightingMode" = "ForwardBase"
        }

        Pass
        {
            Fog 
			{ 
				Mode Off 
			}

            Cull Off
			ZWrite Off
			//Blend SrcAlpha OneMinusSrcAlpha
            Blend SrcAlpha One

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
                float4 worldPos : TEXCOORD3;
            };

			fixed4 _FlareColor;
            fixed4 _LightColor;
            float _FlareWidth;
            float _LightWidth;
            float _Range;

			sampler2D _CameraDepthTexture;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);

                o.worldPos = v.vertex;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				o.screenUV = ComputeScreenPos(o.pos);
				o.ray = UnityObjectToViewPos(v.vertex).xyz * float3(-1, -1, 1);
                return o;
            }

            float invLerp(float a, float b, float t)
            {
                if (a == b)
                    return a;
                else
                    return clamp((t - a) / (b - a), 0, 1) ;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // find the camera buffer depth and normal for this pixel
				float2 depthUV = i.screenUV.xy / i.screenUV.w;
				float sceneDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, depthUV));

				// locate the pixel in the object space
				i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
				float4 vpos = float4(i.ray * sceneDepth, 1);
				float3 wpos = mul(unity_CameraToWorld, vpos).xyz;
				//float3 opos = mul(unity_WorldToObject, float4(wpos, 1)).xyz;

                // calculate the result color
                float distance = length(wpos.xz - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xz);
                float strength = 1;
                strength *= invLerp(_Range + _LightWidth / 2 + _FlareWidth, _Range + _LightWidth / 2, distance);
                strength *= invLerp(_Range - _LightWidth / 2 - _FlareWidth, _Range - _LightWidth / 2, distance);

                fixed4 color = _LightColor * strength + _FlareColor * (1 - strength);
                color = color * strength + fixed4(0, 0, 0, 0) * (1 - strength);
                color.a = strength;
                return color;

                /*
                float flareStrength = clamp(abs(distance / _FlareHeight), 0, 1);
                float lightStrength = clamp(abs(distance / _LightHeight),0, 1);
                fixed4 color = _FlareColor * lightStrength + _LightColor * (1 - lightStrength);
                color.a *= 1 - clamp(abs(distance / _FlareHeight), 0, 1);
                return color;

				// clip out all pixels that are beyond the 1,1,1 projector cube
				clip(float3(0.5, 0.5, 0.5) - abs(opos.xyz));

				// project the texture from local y axis in object space
				i.uv = (opos.xz + 0.5);

				// sample texture map and calculate texture color
				fixed4 albedo = tex2D(_MainTex, i.uv) * _FlareColor;
				return albedo;
                */
            }
            ENDCG
        }
    }
}
