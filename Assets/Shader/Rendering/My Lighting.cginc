
#if !defined(MY_LIGHTING_INCLUDED)

#define MY_LIGHTING_INCLUDED

/*#include "UnityStandardBRDF.cginc"
#include "UnityStandardUtils.cginc"*/
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"

float4 _Tint;
sampler2D _MainTex;
float4 _MainTex_ST;
float _Metallic;
float _Smoothness;

struct VertexData {
	float4 position : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

struct Interpolators {
	float4 position : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float3 worldPos : TEXCOORD2;
    #if defined(VERTEXLIGHT_ON)
        float3 vertexLightColor : TEXCOORD3;
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
    UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);//diffrent implements for each light
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

Interpolators MyVertexProgram(VertexData v) {
	Interpolators i;
	i.position = UnityObjectToClipPos(v.position);
	i.normal = UnityObjectToWorldNormal(v.normal);
	i.uv = TRANSFORM_TEX(v.uv, _MainTex);
	i.worldPos = mul(unity_ObjectToWorld, v.position);
    ComputeVertexLightColor(i);
	return i;
}

float4 MyFragmentProgram(Interpolators i) : SV_TARGET {
	i.normal = normalize(i.normal);
	/*float3 lightDir = _WorldSpaceLightPos0.xyz;//direct to light
	float3 lightColor = _LightColor0.rgb;*/
	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
	//float3 reflectionDir = reflect(-lightDir, i.normal);

	float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
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