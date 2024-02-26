#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

#define MAX_SHADOW_CASCADES 4
///////////////Structs/////////////////////

/////////////////Texture2D/////////////////////
TEXTURE2D(_ScreenSpaceShadowmapTexture);
TEXTURE2D_SHADOW(_MainLightShadowmapTexture);
SAMPLER_CMP(sampler_LinearClampCompare);
#define BEYOND_SHADOW_FAR(shadowCoord) shadowCoord.z <= 0.0 || shadowCoord.z >= 1.0
///////////////Parameter/////////////////////
float4x4    _MainLightWorldToShadow[MAX_SHADOW_CASCADES];
float4      _CascadeShadowSplitSpheres0;
float4      _CascadeShadowSplitSpheres1;
float4      _CascadeShadowSplitSpheres2;
float4      _CascadeShadowSplitSpheres3;
float4      _CascadeShadowSplitSphereRadii;

float4      _MainLightShadowOffset0; // xy: offset0, zw: offset1
float4      _MainLightShadowOffset1; // xy: offset2, zw: offset3
float4      _MainLightShadowParams;   // (x: shadowStrength, y: >= 1.0 if soft shadows, 0.0 otherwise, z: main light fade scale, w: main light fade bias)
float4      _MainLightShadowmapSize;  // (xy: 1/width and 1/height, zw: width and height)


half MainLightRealtimeShadow(float4 shadowCoord)
{
    half4 shadowParams = _MainLightShadowParams;
    
    real attenuation;
    real shadowStrength = shadowParams.x;
    attenuation = real(SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture,
        sampler_LinearClampCompare, shadowCoord.xyz));
    //attenuation = LerpWhiteTo(attenuation, shadowStrength);
    
    return BEYOND_SHADOW_FAR(shadowCoord) ? 1.0 : attenuation;//超限的话，返回1.0
}

half ComputeCascadeIndex(float3 positionWS)
{
    float3 fromCenter0 = positionWS - _CascadeShadowSplitSpheres0.xyz;
    float3 fromCenter1 = positionWS - _CascadeShadowSplitSpheres1.xyz;
    float3 fromCenter2 = positionWS - _CascadeShadowSplitSpheres2.xyz;
    float3 fromCenter3 = positionWS - _CascadeShadowSplitSpheres3.xyz;
    float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1), dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));

    half4 weights = half4(distances2 < _CascadeShadowSplitSphereRadii);
    weights.yzw = saturate(weights.yzw - weights.xyz);

    return half(4.0) - dot(weights, half4(4, 3, 2, 1));
}

float4 TransformWorldToShadowCoord(float3 positionWS)
{
    half cascadeIndex = ComputeCascadeIndex(positionWS);
    float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));

    return float4(shadowCoord.xyz, 0);
}

#endif