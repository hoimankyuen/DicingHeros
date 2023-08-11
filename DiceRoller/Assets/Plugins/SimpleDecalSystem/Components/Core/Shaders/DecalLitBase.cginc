#if !defined(DECAL_LIT_BASE_INCLUDED)
#define DECAL_LIT_BASE_INCLUDED

#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"
#include "Lighting.cginc"

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

fixed4 _Tint;
sampler2D _MainTex;

float _Metallic;
float _Smoothness;

sampler2D _NormalMap;
float _NormalStrength;

sampler2D _CameraDepthTexture;
sampler2D _CameraDepthNormalsTexture;

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;

	float3 normal : NORMAL;
	float4 tangent : TANGENT;
};

struct v2f
{
	float4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
	float4 screenUV : TEXCOORD1;
	float3 ray : TEXCOORD2;

	float3 normal : TEXCOORD3;
	float4 tangent : TEXCOORD4;
	float3 worldPos : TEXCOORD5;
	
	SHADOW_COORDS(6)

	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD7;
	#endif
};

void ComputeVertexLightColor(inout v2f i) {
	#if defined(VERTEXLIGHT_ON)
		i.vertexLightColor = Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb,
			unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, i.worldPos, i.normal
		);
	#endif
}

v2f vert(appdata v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);

	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;
	o.screenUV = ComputeScreenPos(o.pos);
	o.ray = UnityObjectToViewPos(v.vertex).xyz * float3(-1, -1, 1);
	o.normal = UnityObjectToWorldDir(float3(0, 1, 0));
	o.tangent = float4(UnityObjectToWorldDir(float3(0, 0, 1)), 1);
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	return o;
}

UnityLight CreateLight(v2f i) {
	UnityLight light;

	#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
		light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
	#else
		light.dir = _WorldSpaceLightPos0.xyz;
	#endif

	UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
	
	light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

UnityIndirect CreateIndirectLight(v2f i) {
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

	#if defined(VERTEXLIGHT_ON)
		indirectLight.diffuse = i.vertexLightColor;
	#endif

	#if defined(FORWARD_BASE_PASS)
		indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
	#endif

	return indirectLight;
}

fixed4 frag(v2f i) : SV_Target
{
	// find the camera butter depth and normal for this pixel
	float2 depthUV = i.screenUV.xy / i.screenUV.w;
	float sceneDepth;
	float3 sceneNormal;
	DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, depthUV), sceneDepth, sceneNormal);
	sceneNormal = mul(transpose(UNITY_MATRIX_V), float4(sceneNormal.xyz, 0)).xyz;
	sceneDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, depthUV));

	// locate the pixel in the camera (cpos), world (wpos) and object (opos) space
	i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
	float4 cpos = float4(i.ray * sceneDepth, 1);
	float3 wpos = mul(unity_CameraToWorld, cpos).xyz;
	float3 opos = mul(unity_WorldToObject, float4(wpos, 1)).xyz;

	// clip out all pixels that are beyond the 1,1,1 projector cube
	clip(float3(0.5, 0.5, 0.5) - abs(opos.xyz));

	// clip out all pixels that are from the opposite side 
	clip(dot(i.normal, sceneNormal));

	// project the texture from local y axis in object space
	float2 projectionUV = (opos.xz + 0.5);

	// process normal map
	float3 tangentSpaceNormal = UnpackScaleNormal(tex2D(_NormalMap, projectionUV), _NormalStrength);

	// calculate tangent
	float3 axis = cross(i.normal, sceneNormal);
	axis = normalize(axis);
	float theta = acos(dot(i.normal, sceneNormal));
	float cosT = cos(theta);
	float sinT = sin(theta);
	float cosTRev = 1 - cosT;
	float4x4 tangentRotation = float4x4(
		cosTRev * axis.x * axis.x + cosT         , cosTRev * axis.x * axis.y - sinT * axis.z, cosTRev * axis.x * axis.z + sinT * axis.y, 0,
		cosTRev * axis.x * axis.y + sinT * axis.z, cosTRev * axis.y * axis.y + cosT         , cosTRev * axis.y * axis.z - sinT * axis.x, 0,
		cosTRev * axis.x * axis.z - sinT * axis.y, cosTRev * axis.y * axis.z + sinT * axis.x, cosTRev * axis.z * axis.z + cosT         , 0,
		0                                        , 0                                       , 0                                         , 1
	);
	float4 rotatedTangent = mul(tangentRotation, i.tangent);
	rotatedTangent = normalize(rotatedTangent);

	// apply the tangent to the tangented normal
	float3 objectNormal = sceneNormal;
	float4 objectTangent = rotatedTangent;
	float3 objectBinormal = cross(objectNormal, objectTangent.xyz) * (objectTangent.w * unity_WorldTransformParams.w);
	float3x3 TBN = float3x3(normalize(objectTangent).xyz, normalize(objectBinormal), normalize(objectNormal));
	TBN = transpose(TBN);
	float3 finalWorldNormal = mul(TBN, tangentSpaceNormal);

	// collect and update v2f struct
	i.pos = UnityObjectToClipPos(opos);
	i.pos = i.pos;
	i.uv = projectionUV;
	//i.screenUV = i.screenUV;
	//i.ray = i.ray;
	i.normal = finalWorldNormal;
	i.tangent = rotatedTangent;
	i.worldPos = wpos;

	// sample texture map and calculate texture color
	fixed4 albedo = tex2D(_MainTex, i.uv) * _Tint;
	
	// vertex light
	TRANSFER_SHADOW(i);
	ComputeVertexLightColor(i);
	
	// calculate metalic color and apply unity's physic based shading
	float3 specularTint;
	float oneMinusReflectivity;
	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

	albedo.rgb = DiffuseAndSpecularFromMetallic(
		albedo.rgb, _Metallic, specularTint, oneMinusReflectivity
	);

	albedo.rgb = UNITY_BRDF_PBS(
		albedo.rgb, specularTint,
		oneMinusReflectivity, _Smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i)
	);

	return albedo;
}
#endif