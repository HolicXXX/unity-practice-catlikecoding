
#if !defined(MY_LIGHTING_INCLUDED)

#define MY_LIGHTING_INCLUDED

/*#include "UnityStandardBRDF.cginc"
#include "UnityStandardUtils.cginc"*/
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

float4 _Tint;
sampler2D _MainTex, _DetailTex;
float4 _MainTex_ST, _DetailTex_ST;
sampler2D _NormalMap, _DetailNormalMap;
float _BumpScale, _DetailBumpScale;
float _Metallic;
float _Smoothness;

struct VertexData {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
    float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
};

struct Interpolators {
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;
    #if defined(BINORMAL_PER_FRAGMENT)
		float4 tangent : TEXCOORD2;
	#else
		float3 tangent : TEXCOORD2;
		float3 binormal : TEXCOORD3;
	#endif
	float3 worldPos : TEXCOORD4;

    /*#if defined(SHADOWS_SCREEN)
        float4 shadowCoordinates : TEXCOORD5;
    #endif*/
    SHADOW_COORDS(5)

    #if defined(VERTEXLIGHT_ON)
        float3 vertexLightColor : TEXCOORD6;
    #endif
};

UnityLight CreateLight (Interpolators i) {
	UnityLight light;
    #if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
	    light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
    #else
        light.dir = _WorldSpaceLightPos0.xyz;
    #endif
    //float3 lightVec = _WorldSpaceLightPos0.xyz - i.worldPos;
	//float attenuation = 1 / (1 + dot(lightVec, lightVec));

    UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);//diffrent implements for each light

    light.color = _LightColor0.rgb * attenuation;
	light.ndotl = DotClamped(i.normal, light.dir);
	return light;
}

void ComputeVertexLightColor (inout Interpolators i) {
    #if defined(VERTEXLIGHT_ON)
        //calculate one light
        /*float3 lightPos = float3(
            unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x
        );
        float3 lightVec = lightPos - i.worldPos;
        float3 lightDir = normalize(lightVec);
        float ndotl = DotClamped(i.normal, lightDir);
        float attenuation = 1 / (1 + dot(lightVec, lightVec) * unity_4LightAtten0.x);
        i.vertexLightColor = unity_LightColor[0].rgb * ndotl * attenuation;*/

        i.vertexLightColor = Shade4PointLights(
            unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
            unity_LightColor[0].rgb, unity_LightColor[1].rgb,
            unity_LightColor[2].rgb, unity_LightColor[3].rgb,
            unity_4LightAtten0, i.worldPos, i.normal
        );
    #endif
}

UnityIndirect CreateIndirectLight(Interpolators i) {
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

float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign) {
    return cross(normal, tangent.xyz) *
		(binormalSign * unity_WorldTransformParams.w);
}

Interpolators MyVertexProgram(VertexData v) {
	Interpolators i;
	i.pos = UnityObjectToClipPos(v.vertex);
	i.worldPos = mul(unity_ObjectToWorld, v.vertex);
	i.normal = UnityObjectToWorldNormal(v.normal);
    #if defined(BINORMAL_PER_FRAGMENT)
        i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
    #else
        i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
        i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w);
    #endif
	i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
	i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);
    
    /*#if defined(SHADOWS_SCREEN)
        i.shadowCoordinates = ComputeScreenPos(i.pos);
    #endif*/
    TRANSFER_SHADOW(i);
    
    ComputeVertexLightColor(i);
	return i;
}

void InitializeFragmentNormal(inout Interpolators i) {
    /*i.normal.xy = tex2D(_NormalMap, i.uv).wy * 2 - 1;
    i.normal.xy *= _BumpScale;
    i.normal.z = sqrt(1 - saturate(dot(i.normal.xy, i.normal.xy)));*/
    float3 mainNormal = UnpackScaleNormal(tex2D(_NormalMap, i.uv.xy), _BumpScale);
    float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, i.uv.zw), _DetailBumpScale);
    /*i.normal = float3(mainNormal.xy + detailNormal.xy, mainNormal.z * detailNormal.z);*/
    float3 tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);
    //tangentSpaceNormal = tangentSpaceNormal.xzy;
    #if defined(BINORMAL_PER_FRAGMENT)
        float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
    #else
        float3 binormal = i.binormal;
    #endif

    i.normal = normalize(
		tangentSpaceNormal.x * i.tangent +
		tangentSpaceNormal.y * binormal +
		tangentSpaceNormal.z * i.normal
	);
}

float4 MyFragmentProgram(Interpolators i) : SV_TARGET {
	InitializeFragmentNormal(i);

	/*float3 lightDir = _WorldSpaceLightPos0.xyz;//direct to light
	float3 lightColor = _LightColor0.rgb;*/
	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	//float3 reflectionDir = reflect(-lightDir, i.normal);

	float3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Tint.rgb;
    albedo *= tex2D(_DetailTex, i.uv.zw) * unity_ColorSpaceDouble;
	//albedo *= 1 - max(_SpecularTint.r, max(_SpecularTint.g, _SpecularTint.b));
	/*albedo = EnergyConservationBetweenDiffuseAndSpecular(
		albedo, _SpecularTint.rgb, oneMinusReflectivity
	);*/
	float3 specularTint;
	float oneMinusReflectivity;
	albedo = DiffuseAndSpecularFromMetallic(
		albedo, _Metallic, specularTint, oneMinusReflectivity
	);

	/*float3 diffuse = albedo * lightColor * DotClamped(lightDir, i.normal);
	float3 halfVector = normalize(lightDir + viewDir);
	float3 specular = specularTint * lightColor * pow(
		DotClamped(halfVector, i.normal),
		_Smoothness * 100
	);
	return float4(diffuse + specular, 1);*/

	return UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, _Smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i)
	);//take care of everything
}

#endif