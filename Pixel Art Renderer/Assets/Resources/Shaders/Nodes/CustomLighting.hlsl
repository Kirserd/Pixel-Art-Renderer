#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
//#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#if (SHADERPASS != SHADERPASS_FORWARD)
#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
#endif
#endif

struct CustomLightingData
{
    // Position and orientation
    float3 positionWS;
    float3 normalWS;
    float3 viewDirectionWS;
    float4 shadowCoord;

    // Surface attributes
    float3 albedo;
    float smoothness;
    float ambientOcclusion;

    // Baked lighting
    float3 bakedGI;
    float4 shadowMask;
    float fogFactor;
};

float3 lightColorBuffer = float3(0,0,0);

// Translate a [0, 1] smoothness value to an exponent 
float GetSmoothnessPower(float rawSmoothness)
{
    return exp2(10 * rawSmoothness + 1);
}

#ifndef SHADERGRAPH_PREVIEW
float3 CustomGlobalIllumination(CustomLightingData d)
{
    float3 indirectDiffuse = d.albedo * d.bakedGI * d.ambientOcclusion;

    float3 reflectVector = reflect(-d.viewDirectionWS, d.normalWS);
    // This is a rim light term, making reflections stronger along
    // the edges of view
    float fresnel = Pow4(1 - saturate(dot(d.viewDirectionWS, d.normalWS)));
    // This function samples the baked reflections cubemap
    // It is located in URP/ShaderLibrary/Lighting.hlsl
    float3 indirectSpecular = GlossyEnvironmentReflection(reflectVector,
        RoughnessToPerceptualRoughness(1 - d.smoothness),
        d.ambientOcclusion) * fresnel;

    return indirectDiffuse + indirectSpecular;
}

float3 CustomLightHandling(CustomLightingData d, Light light)
{
    float3 radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation);

    lightColorBuffer = radiance;
    
    float diffuse = saturate(dot(d.normalWS, light.direction));
    float specularDot = saturate(dot(d.normalWS, normalize(light.direction + d.viewDirectionWS)));
    float specular = pow(specularDot, GetSmoothnessPower(d.smoothness)) * diffuse;

    float3 color = d.albedo * radiance * (diffuse + specular);
    return color;
}
#endif

void CalculateCustomLighting_float(float3 Position, float3 Normal, float3 ViewDirection,
    float3 Albedo, float Smoothness, float AmbientOcclusion,
    float2 LightmapUV,
    out float3 AdditionalLights, out float3 MainLight, out float3 LightColor, out float3 GlobalIllumination, out float Fog)
{
    CustomLightingData d;
    d.positionWS = Position;
    d.normalWS = Normal;
    d.viewDirectionWS = ViewDirection;
    d.albedo = Albedo;
    d.smoothness = Smoothness;
    d.ambientOcclusion = AmbientOcclusion;

#ifdef SHADERGRAPH_PREVIEW
    // In preview, there's no shadows or bakedGI
    d.shadowCoord = 0;
    d.bakedGI = 0;
    d.shadowMask = 0;
    d.fogFactor = 0;
#else
    float4 positionCS = TransformWorldToHClip(Position);
#if SHADOWS_SCREEN
    d.shadowCoord = ComputeScreenPos(positionCS);
#else
    d.shadowCoord = TransformWorldToShadowCoord(Position);
#endif
    float2 lightmapUV;
    OUTPUT_LIGHTMAP_UV(LightmapUV, unity_LightmapST, lightmapUV);
    float3 vertexSH;
    OUTPUT_SH(Normal, vertexSH);
    d.bakedGI = SAMPLE_GI(lightmapUV, vertexSH, Normal);
    d.shadowMask = SAMPLE_SHADOWMASK(lightmapUV);
    d.fogFactor = ComputeFogFactor(positionCS.z);
#endif

    AdditionalLights = float3(0, 0, 0);
    MainLight = float3(0, 0, 0); 
    GlobalIllumination = CustomGlobalIllumination(d);
    Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, d.shadowMask);
    MainLight = CustomLightHandling(d, mainLight);
    LightColor = mainLight.color;

#ifdef _ADDITIONAL_LIGHTS
    // Shade additional cone and point lights. Functions in URP/ShaderLibrary/Lighting.hlsl
    uint numAdditionalLights = GetAdditionalLightsCount();
    for (uint lightI = 0; lightI < numAdditionalLights; lightI++) {
        Light light = GetAdditionalLight(lightI, d.positionWS, d.shadowMask);
        AdditionalLights += CustomLightHandling(d, light);
        LightColor += lightColorBuffer;
    }
#endif

    Fog = MixFog(GlobalIllumination + MainLight, d.fogFactor);
}

#endif